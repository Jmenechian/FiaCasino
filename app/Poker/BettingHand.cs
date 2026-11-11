using System;
using System.Collections.Generic;
using FiaCasino.Cards;

namespace FiaCasino.Poker;

public enum Seat { Player, Opponent }
public enum PokerAction { Fold, Check, Call, Wager }

public readonly record struct LegalActions(int Call, int MinWagerTo, int MaxWagerTo)
{
    public bool CanWager => MaxWagerTo > 0;
}

public readonly record struct ChipTransfer(Seat Seat, int Amount, bool ToPot);

public sealed class BettingHand
{
    public const int SmallBlind = 1;
    public const int BigBlind = 2;

    private readonly PokerHand _cards;
    private readonly List<ChipTransfer> _transfers = new();
    public IReadOnlyList<ChipTransfer> Transfers => _transfers;
    private readonly int[] _stacks;
    private readonly int[] _street = new int[2];
    private int _actionsLeft = 2;
    private int _lastFullRaise = BigBlind;

    public Seat Dealer { get; }
    public Seat? Actor { get; private set; }
    public int Pot { get; private set; }
    public int AwardedPot { get; private set; }
    public int ReturnedChips { get; private set; }
    public Seat? ReturnedTo { get; private set; }
    public Seat? Folded { get; private set; }
    public ShowdownOutcome? Outcome { get; private set; }
    public HandStage Stage => _cards.Stage;
    public IReadOnlyList<Card> Board => _cards.Board;
    public IReadOnlyList<Card> HoleCards => _cards.HoleCards;
    public IReadOnlyList<Card> RevealedOpponentCards => _cards.RevealedOpponentCards;
    public HandValue? BestHand => _cards.BestHand;
    public HandValue? OpponentBestHand => _cards.OpponentBestHand;
    public int Stack(Seat seat) => _stacks[(int)seat];
    public int Committed(Seat seat) => _street[(int)seat];

    public BettingHand(int playerStack, int opponentStack, Seat dealer, Random? random = null)
    {
        if (playerStack <= 0) throw new ArgumentOutOfRangeException(nameof(playerStack));
        if (opponentStack <= 0 || opponentStack > int.MaxValue - playerStack)
            throw new ArgumentOutOfRangeException(nameof(opponentStack));
        Dealer = dealer;
        Actor = dealer;
        _cards = new PokerHand(random, dealOpponentFirst: dealer == Seat.Player);
        _stacks = new[] { playerStack, opponentStack };
        Pay(dealer, Math.Min(SmallBlind, Stack(dealer)));
        var other = Other(dealer);
        Pay(other, Math.Min(BigBlind, Stack(other)));
        Continue();
    }

    public LegalActions Legal
    {
        get
        {
            if (Actor is not { } seat) return default;
            var other = Other(seat);
            var high = Math.Max(Committed(seat), Committed(other));
            var call = Math.Min(Stack(seat), high - Committed(seat));
            var maximum = Committed(seat) + Stack(seat);
            if (Stack(other) == 0 || maximum <= high) return new(call, 0, 0);
            var minimum = (int)Math.Min(maximum, (long)high + _lastFullRaise);
            return new(call, minimum, maximum);
        }
    }

    // Wagers specify the total committed by this seat on this street.
    public void Act(Seat seat, PokerAction action, int wagerTo = 0)
    {
        if (Actor != seat) throw new InvalidOperationException("It is not this seat's turn.");
        var legal = Legal;
        var other = Other(seat);
        switch (action)
        {
            case PokerAction.Fold when legal.Call > 0:
                Folded = seat;
                ReturnUncalled();
                Award(other == Seat.Player ? ShowdownOutcome.PlayerWins : ShowdownOutcome.OpponentWins);
                return;
            case PokerAction.Check when legal.Call == 0:
                _actionsLeft--;
                break;
            case PokerAction.Call when legal.Call > 0:
                Pay(seat, legal.Call);
                _actionsLeft--;
                break;
            case PokerAction.Wager when legal.CanWager && wagerTo >= legal.MinWagerTo && wagerTo <= legal.MaxWagerTo:
                var raise = wagerTo - Math.Max(Committed(seat), Committed(other));
                _lastFullRaise = Math.Max(_lastFullRaise, raise);
                Pay(seat, wagerTo - Committed(seat));
                _actionsLeft = 1;
                break;
            default:
                throw new InvalidOperationException("That action is not legal.");
        }

        Actor = other;
        Continue();
    }

    internal BotDecision OpponentDecision(Random random)
    {
        if (Actor != Seat.Opponent) throw new InvalidOperationException("It is not the opponent's turn.");
        return FreePlayBot.Choose(_cards.OpponentHoleCards, Board, Legal, Pot,
            Committed(Seat.Opponent), Stack(Seat.Opponent), random);
    }

    private void Continue()
    {
        while (Actor is { } seat)
        {
            var other = Other(seat);
            var allIn = Stack(seat) == 0 || Stack(other) == 0;
            if (_actionsLeft > 0 && !allIn) return;
            if (allIn)
            {
                // The only decision left against an all-in is whether to match the wager.
                if (Stack(seat) > 0 && Committed(seat) < Committed(other)) return;
                if (Stack(other) > 0 && Committed(other) < Committed(seat))
                {
                    Actor = other;
                    return;
                }
            }

            ReturnUncalled();
            Array.Clear(_street);
            if (!_cards.CanAdvance)
            {
                _cards.Showdown();
                Award(_cards.Outcome!.Value);
                return;
            }

            _cards.Advance();
            _lastFullRaise = BigBlind;
            _actionsLeft = 2;
            Actor = Other(Dealer);
        }
    }

    private void Pay(Seat seat, int amount)
    {
        _stacks[(int)seat] -= amount;
        _street[(int)seat] += amount;
        Pot += amount;
        if (amount != 0) _transfers.Add(new(seat, Math.Abs(amount), amount > 0));
    }

    private void ReturnUncalled()
    {
        var difference = Committed(Seat.Player) - Committed(Seat.Opponent);
        if (difference == 0) return;
        var seat = difference > 0 ? Seat.Player : Seat.Opponent;
        var amount = Math.Abs(difference);
        Pay(seat, -amount);
        ReturnedChips = amount;
        ReturnedTo = seat;
    }

    private void Award(ShowdownOutcome outcome)
    {
        AwardedPot = Pot;
        if (outcome == ShowdownOutcome.Tie)
        {
            _transfers.Add(new(Dealer, Pot / 2, false));
            _transfers.Add(new(Other(Dealer), Pot - Pot / 2, false));
            _stacks[(int)Dealer] += Pot / 2;
            _stacks[(int)Other(Dealer)] += Pot - Pot / 2;
        }
        else
        {
            _transfers.Add(new(outcome == ShowdownOutcome.PlayerWins ? Seat.Player : Seat.Opponent, Pot, false));
            _stacks[(int)(outcome == ShowdownOutcome.PlayerWins ? Seat.Player : Seat.Opponent)] += Pot;
        }
        Array.Clear(_street);
        Pot = 0;
        Actor = null;
        Outcome = outcome;
    }

    private static Seat Other(Seat seat) => seat == Seat.Player ? Seat.Opponent : Seat.Player;
}

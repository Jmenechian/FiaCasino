using System;
using System.Collections.Generic;

namespace FiaCasino.Poker;

public sealed class FreePlaySession
{
    public const int BuyIn = 200;

    private readonly Random _deckRandom;
    private readonly Random _botRandom;
    public BettingHand Hand { get; private set; } = null!;
    public int HandNumber { get; private set; }
    public string RefillNotice { get; private set; } = "";
    public string OpponentActivity { get; private set; } = "";

    public FreePlaySession(Random? deckRandom = null, Random? botRandom = null)
    {
        _deckRandom = deckRandom ?? new Random();
        _botRandom = botRandom ?? new Random();
        NextHand();
    }

    public void NextHand()
    {
        if (Hand is { Outcome: null }) throw new InvalidOperationException("Finish this hand first.");
        var player = Hand?.Stack(Seat.Player) ?? BuyIn;
        var opponent = Hand?.Stack(Seat.Opponent) ?? BuyIn;
        RefillNotice = player == 0 ? "You refilled to 200" :
            opponent == 0 ? "Opponent refilled to 200" : "";
        var dealer = HandNumber % 2 == 0 ? Seat.Player : Seat.Opponent;
        Hand = new BettingHand(player == 0 ? BuyIn : player, opponent == 0 ? BuyIn : opponent,
            dealer, _deckRandom);
        HandNumber++;
        PlayOpponent();
    }

    public void Act(PokerAction action, int wagerTo = 0)
    {
        Hand.Act(Seat.Player, action, wagerTo);
        PlayOpponent();
    }

    private void PlayOpponent()
    {
        var actions = new List<string>();
        while (Hand.Actor == Seat.Opponent)
        {
            var decision = Hand.OpponentDecision(_botRandom);
            actions.Add(decision.Action switch
            {
                PokerAction.Fold => "folds",
                PokerAction.Check => "checks",
                PokerAction.Call => $"calls {Hand.Legal.Call}",
                _ => Hand.Committed(Seat.Player) + Hand.Committed(Seat.Opponent) == 0
                    ? $"bets {decision.WagerTo}" : $"raises to {decision.WagerTo}"
            });
            Hand.Act(Seat.Opponent, decision.Action, decision.WagerTo);
        }
        OpponentActivity = actions.Count == 0 ? "" : "Opponent " + string.Join(", then ", actions);
    }
}

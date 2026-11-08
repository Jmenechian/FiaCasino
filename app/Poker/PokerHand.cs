using System;
using System.Collections.Generic;
using System.Linq;
using FiaCasino.Cards;

namespace FiaCasino.Poker;

public enum HandStage { PreFlop, Flop, Turn, River }

public enum ShowdownOutcome { PlayerWins, OpponentWins, Tie }

// A heads-up reveal sequence, independent of Godot. Future cards stay in the deck.
public sealed class PokerHand
{
    private readonly Deck _deck;
    private readonly List<Card> _board = new(5);
    private readonly IReadOnlyList<Card> _opponentCards;
    internal IReadOnlyList<Card> OpponentHoleCards => _opponentCards;
    public IReadOnlyList<Card> Board { get; }
    public IReadOnlyList<Card> HoleCards { get; }
    public HandStage Stage { get; private set; } = HandStage.PreFlop;
    public bool CanAdvance => Stage != HandStage.River;
    public bool CanShowdown => Stage == HandStage.River && Outcome == null;
    public ShowdownOutcome? Outcome { get; private set; }
    public IReadOnlyList<Card> RevealedOpponentCards =>
        Outcome == null ? Array.Empty<Card>() : _opponentCards;
    public HandValue? OpponentBestHand { get; private set; }
    public HandValue? BestHand => _board.Count < 3 ? null :
        HandEvaluator.Evaluate(_board.Concat(HoleCards).ToArray());

    public PokerHand(Random? random = null, bool dealOpponentFirst = false)
    {
        _deck = new Deck(random);
        Board = _board.AsReadOnly();
        var deal = new[] { _deck.Draw(), _deck.Draw(), _deck.Draw(), _deck.Draw() };
        var playerIndex = dealOpponentFirst ? 1 : 0;
        var opponentIndex = 1 - playerIndex;
        HoleCards = Array.AsReadOnly(new[] { deal[playerIndex], deal[playerIndex + 2] });
        _opponentCards = Array.AsReadOnly(new[] { deal[opponentIndex], deal[opponentIndex + 2] });
    }

    public void Advance()
    {
        if (!CanAdvance) throw new InvalidOperationException("The river is already revealed.");
        var count = Stage == HandStage.PreFlop ? 3 : 1;
        for (var i = 0; i < count; i++) _board.Add(_deck.Draw());
        Stage = (HandStage)((int)Stage + 1);
    }

    public void Showdown()
    {
        if (!CanShowdown) throw new InvalidOperationException("Showdown requires an unresolved river.");
        OpponentBestHand = HandEvaluator.Evaluate(_board.Concat(_opponentCards).ToArray());
        Outcome = BestHand!.CompareTo(OpponentBestHand) switch
        {
            > 0 => ShowdownOutcome.PlayerWins,
            < 0 => ShowdownOutcome.OpponentWins,
            _ => ShowdownOutcome.Tie
        };
    }
}

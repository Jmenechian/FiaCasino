using System;
using System.Collections.Generic;
using System.Linq;
using FiaCasino.Cards;

namespace FiaCasino.Poker;

public enum HandStage { PreFlop, Flop, Turn, River }

// A single-seat reveal sequence, independent of Godot. Future cards stay in the deck.
public sealed class PokerHand
{
    private readonly Deck _deck;
    private readonly List<Card> _board = new(5);
    public IReadOnlyList<Card> Board { get; }
    public IReadOnlyList<Card> HoleCards { get; }
    public HandStage Stage { get; private set; } = HandStage.PreFlop;
    public bool CanAdvance => Stage != HandStage.River;
    public HandValue? BestHand => _board.Count < 3 ? null :
        HandEvaluator.Evaluate(_board.Concat(HoleCards).ToArray());

    public PokerHand(Random? random = null)
    {
        _deck = new Deck(random);
        Board = _board.AsReadOnly();
        HoleCards = Array.AsReadOnly(new[] { _deck.Draw(), _deck.Draw() });
    }

    public void Advance()
    {
        if (!CanAdvance) throw new InvalidOperationException("The river is already revealed.");
        var count = Stage == HandStage.PreFlop ? 3 : 1;
        for (var i = 0; i < count; i++) _board.Add(_deck.Draw());
        Stage = (HandStage)((int)Stage + 1);
    }
}

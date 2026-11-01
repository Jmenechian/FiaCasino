using System;
using System.Collections.Generic;
using System.Linq;
using FiaCasino.Cards;

namespace FiaCasino.Poker;

public enum HandCategory
{
    HighCard, OnePair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush
}

// Category and ordered kickers encode poker strength; suits never break a tie.
public sealed class HandValue : IComparable<HandValue>
{
    public HandCategory Category { get; }
    public IReadOnlyList<int> TieBreakers { get; }
    public IReadOnlyList<Card> BestFive { get; }
    public string Label => Category switch
    {
        HandCategory.HighCard => "High card",
        HandCategory.OnePair => "One pair",
        HandCategory.TwoPair => "Two pair",
        HandCategory.ThreeOfAKind => "Three of a kind",
        HandCategory.Straight => "Straight",
        HandCategory.Flush => "Flush",
        HandCategory.FullHouse => "Full house",
        HandCategory.FourOfAKind => "Four of a kind",
        HandCategory.StraightFlush => TieBreakers[0] == 14 ? "Royal flush" : "Straight flush",
        _ => throw new InvalidOperationException()
    };

    internal HandValue(HandCategory category, int[] tieBreakers, Card[] cards)
    {
        Category = category;
        TieBreakers = Array.AsReadOnly(tieBreakers);
        BestFive = Array.AsReadOnly(cards);
    }

    public int CompareTo(HandValue? other)
    {
        if (other == null) return 1;
        var category = Category.CompareTo(other.Category);
        if (category != 0) return category;
        for (var i = 0; i < TieBreakers.Count; i++)
        {
            var rank = TieBreakers[i].CompareTo(other.TieBreakers[i]);
            if (rank != 0) return rank;
        }
        return 0;
    }
}

public static class HandEvaluator
{
    private static int Rank(Card card) => card.Rank switch
    {
        "Two" => 2, "Three" => 3, "Four" => 4, "Five" => 5, "Six" => 6,
        "Seven" => 7, "Eight" => 8, "Nine" => 9, "Ten" => 10,
        "Jack" => 11, "Queen" => 12, "King" => 13, "Ace" => 14,
        _ => throw new ArgumentException("Unknown card rank.")
    };

    // All 21 five-card subsets for seven cards: no requirement to use a hole card.
    public static HandValue Evaluate(IReadOnlyList<Card> cards)
    {
        if (cards == null) throw new ArgumentNullException(nameof(cards));
        if (cards.Count < 5 || cards.Count > 7)
            throw new ArgumentException("Expected five to seven cards.", nameof(cards));
        if (cards.Distinct().Count() != cards.Count)
            throw new ArgumentException("Duplicate card identity.", nameof(cards));
        foreach (var card in cards)
        {
            _ = Rank(card);
            if (card.Suit is not ("Clubs" or "Diamonds" or "Hearts" or "Spades"))
                throw new ArgumentException("Unknown card suit.", nameof(cards));
        }

        HandValue? best = null;
        for (var a = 0; a < cards.Count - 4; a++)
        for (var b = a + 1; b < cards.Count - 3; b++)
        for (var c = b + 1; c < cards.Count - 2; c++)
        for (var d = c + 1; d < cards.Count - 1; d++)
        for (var e = d + 1; e < cards.Count; e++)
        {
            var value = EvaluateFive(new[] { cards[a], cards[b], cards[c], cards[d], cards[e] });
            if (best == null || value.CompareTo(best) > 0) best = value;
        }
        return best!;
    }

    private static HandValue EvaluateFive(Card[] cards)
    {
        var ranks = cards.Select(Rank).OrderByDescending(rank => rank).ToArray();
        var groups = ranks.GroupBy(rank => rank).OrderByDescending(group => group.Count())
            .ThenByDescending(group => group.Key).ToArray();
        var flush = cards.All(card => card.Suit == cards[0].Suit);
        var straight = ranks.Distinct().Count() == 5 && ranks[0] - ranks[4] == 4 ? ranks[0] :
            ranks.SequenceEqual(new[] { 14, 5, 4, 3, 2 }) ? 5 : 0;
        if (flush && straight > 0) return new(HandCategory.StraightFlush, new[] { straight }, cards);
        if (groups[0].Count() == 4)
            return new(HandCategory.FourOfAKind, groups.Select(group => group.Key).ToArray(), cards);
        if (groups[0].Count() == 3 && groups[1].Count() == 2)
            return new(HandCategory.FullHouse, groups.Select(group => group.Key).ToArray(), cards);
        if (flush) return new(HandCategory.Flush, ranks, cards);
        if (straight > 0) return new(HandCategory.Straight, new[] { straight }, cards);
        if (groups[0].Count() == 3)
            return new(HandCategory.ThreeOfAKind, groups.Select(group => group.Key).ToArray(), cards);
        if (groups[0].Count() == 2)
            return new(groups[1].Count() == 2 ? HandCategory.TwoPair : HandCategory.OnePair,
                groups.Select(group => group.Key).ToArray(), cards);
        return new(HandCategory.HighCard, ranks, cards);
    }
}

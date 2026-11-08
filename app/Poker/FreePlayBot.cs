using System;
using System.Collections.Generic;
using System.Linq;
using FiaCasino.Cards;

namespace FiaCasino.Poker;

internal readonly record struct BotDecision(PokerAction Action, int WagerTo = 0);

internal static class FreePlayBot
{
    public static BotDecision Choose(IReadOnlyList<Card> hole, IReadOnlyList<Card> board,
        LegalActions legal, int pot, int committed, int stack, Random random)
    {
        var strength = Strength(hole, board);
        var price = (double)legal.Call / ((long)pot + legal.Call);
        var roll = random.NextDouble();
        if (legal.Call > 0 && strength < price + 0.12 && legal.Call > BettingHand.BigBlind)
            return new(PokerAction.Fold);

        if (legal.CanWager && strength >= 0.52 && roll < (strength - 0.35) * 0.8)
        {
            var target = (long)committed + legal.Call + Math.Max(BettingHand.BigBlind, pot / 2);
            if (target >= legal.MinWagerTo || legal.MinWagerTo - committed < stack)
                return new(PokerAction.Wager, (int)Math.Clamp(target, legal.MinWagerTo, legal.MaxWagerTo));
        }

        return new(legal.Call > 0 ? PokerAction.Call : PokerAction.Check);
    }

    private static double Strength(IReadOnlyList<Card> hole, IReadOnlyList<Card> board)
    {
        if (board.Count == 0)
        {
            if (hole[0].Rank == hole[1].Rank) return 0.68;
            var highCards = hole.Count(card => card.Rank is "Ace" or "King" or "Queen" or "Jack");
            return 0.16 + highCards * 0.16 + (hole[0].Suit == hole[1].Suit ? 0.06 : 0);
        }

        var value = HandEvaluator.Evaluate(board.Concat(hole).ToArray());
        if (board.Count == 5 && value.CompareTo(HandEvaluator.Evaluate(board)) == 0) return 0.2;
        return value.Category switch
        {
            HandCategory.HighCard => 0.2,
            HandCategory.OnePair => 0.44,
            HandCategory.TwoPair => 0.64,
            HandCategory.ThreeOfAKind => 0.76,
            _ => 0.9
        };
    }
}

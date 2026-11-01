using System;
using System.Collections.Generic;

namespace FiaCasino.Cards;

// A shuffled full deck, consumed in order. No dependency on Godot or artwork.
public sealed class Deck
{
    private readonly Queue<Card> _cards;
    public int Remaining => _cards.Count;

    public Deck(Random? random = null)
    {
        random ??= Random.Shared;
        var cards = new List<Card>(52);
        foreach (var suit in new[] { "Clubs", "Diamonds", "Hearts", "Spades" })
        foreach (var rank in new[] { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven",
                     "Eight", "Nine", "Ten", "Jack", "Queen", "King" })
            cards.Add(new Card(rank, suit));

        // Fisher-Yates: each remaining card has an equal chance at each position.
        for (var i = cards.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
        _cards = new Queue<Card>(cards);
    }

    // Queue.Dequeue throws if exhausted, rather than silently reusing a card.
    public Card Draw() => _cards.Dequeue();
}

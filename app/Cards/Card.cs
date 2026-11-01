namespace FiaCasino.Cards;

// Identity has no dependency on artwork, Godot, or a specific game's rules.
public readonly record struct Card(string Rank, string Suit)
{
    public string Id => $"{Rank.ToLowerInvariant()}_of_{Suit.ToLowerInvariant()}";
    public string DisplayName => $"{Rank} of {Suit}";
}

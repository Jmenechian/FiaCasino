using FiaCasino.Cards;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var ranks = new[] { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight",
    "Nine", "Ten", "Jack", "Queen", "King" };
var suits = new[] { "Clubs", "Diamonds", "Hearts", "Spades" };
var expected = (from rank in ranks from suit in suits select new Card(rank, suit)).ToHashSet();
var orders = new HashSet<string>();
var assets = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../app/Assets/Cards"));

foreach (var card in expected)
    Check(File.Exists(Path.Combine(assets, card.Id + ".svg")), $"Missing face: {card.Id}");

for (var seed = 0; seed < 1000; seed++)
{
    var deck = new Deck(new Random(seed));
    Check(deck.Remaining == 52, "Deck must start full");
    var board = Enumerable.Range(0, 5).Select(_ => deck.Draw()).ToArray();
    var hand = Enumerable.Range(0, 2).Select(_ => deck.Draw()).ToArray();
    var dealt = board.Concat(hand).ToArray();
    Check(dealt.Distinct().Count() == 7, $"Duplicate in deal for seed {seed}");
    Check(deck.Remaining == 45, "Seven draws must leave 45");
    var rest = Enumerable.Range(0, 45).Select(_ => deck.Draw()).ToArray();
    var full = dealt.Concat(rest).ToArray();
    Check(full.Length == 52 && full.ToHashSet().SetEquals(expected), "Wrong deck contents");
    Check(full.Distinct().Count() == 52 && deck.Remaining == 0, "Duplicate or incorrect count");
    orders.Add(string.Join(",", full.Select(card => card.Id)));
    try { deck.Draw(); throw new Exception("Exhausted deck allowed a draw"); }
    catch (InvalidOperationException) { }
}
Check(orders.Count > 990, "Shuffle did not vary across seeds");
Console.WriteLine("PASS: 52 faces; 1,000 seeded decks/deals; contents, uniqueness, remaining counts, exhaustion, shuffle variation.");

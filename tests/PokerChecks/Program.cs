using FiaCasino.Cards;
using FiaCasino.Poker;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

static Card[] Cards(string text) => text.Split(' ').Select(token => new Card(
    "23456789TJQKA".IndexOf(token[0]) switch
    {
        0 => "Two", 1 => "Three", 2 => "Four", 3 => "Five", 4 => "Six", 5 => "Seven",
        6 => "Eight", 7 => "Nine", 8 => "Ten", 9 => "Jack", 10 => "Queen", 11 => "King", 12 => "Ace",
        _ => throw new Exception("Bad test rank")
    }, token[1] switch { 'c' => "Clubs", 'd' => "Diamonds", 'h' => "Hearts", 's' => "Spades", _ => throw new Exception() })).ToArray();
static HandValue Eval(string text) => HandEvaluator.Evaluate(Cards(text));

var examples = new[]
{
    ("As Kd 9h 7s 4c", HandCategory.HighCard, "High card"),
    ("As Ad 9h 7s 4c", HandCategory.OnePair, "One pair"),
    ("As Ad 9h 9s 4c", HandCategory.TwoPair, "Two pair"),
    ("As Ad Ah 7s 4c", HandCategory.ThreeOfAKind, "Three of a kind"),
    ("As 2d 3h 4s 5c", HandCategory.Straight, "Straight"),
    ("As Js 9s 7s 4s", HandCategory.Flush, "Flush"),
    ("As Ad Ah 7s 7c", HandCategory.FullHouse, "Full house"),
    ("As Ad Ah Ac 4c", HandCategory.FourOfAKind, "Four of a kind"),
    ("5s 6s 7s 8s 9s", HandCategory.StraightFlush, "Straight flush"),
    ("Ts Js Qs Ks As", HandCategory.StraightFlush, "Royal flush")
};
foreach (var (cards, category, label) in examples)
{
    var value = Eval(cards);
    Check(value.Category == category && value.Label == label, $"Wrong classification: {cards}");
}
for (var i = 1; i < examples.Length; i++)
    Check(Eval(examples[i].Item1).CompareTo(Eval(examples[i - 1].Item1)) > 0, "Category ordering");

// Same-category comparisons exercise every kicker position and primary rank.
foreach (var (strong, weak) in new[]
{
    ("As Kd 9h 7s 5c", "As Kd 9h 7s 4c"),
    ("As Kd 9h 8s 4c", "As Kd 9h 7s 4c"),
    ("As Kd Th 7s 4c", "As Kd 9h 7s 4c"),
    ("As Qd 9h 7s 4c", "As Jd 9h 7s 4c"),
    ("As Qd 9h 7s 4c", "Ks Qd 9h 7s 4c"),
    ("As Ad Kh Qs Jc", "As Ad Kh Qs Tc"),
    ("As Ad Kh Qs Tc", "As Ad Kh Js Tc"),
    ("As Ad Kh Js Tc", "As Ad Qh Js Tc"),
    ("As Ad Qh Js Tc", "Ks Kd Qh Js Tc"),
    ("As Ad Kh Ks Qc", "As Ad Kh Ks Jc"),
    ("As Ad Kh Ks Qc", "As Ad Qh Qs Kc"),
    ("As Ad 2h 2s Qc", "Ks Kd Qh Qs Ac"),
    ("As Ad Ah Ks Qc", "As Ad Ah Ks Jc"),
    ("As Ad Ah Ks Jc", "As Ad Ah Qs Jc"),
    ("As Ad Ah Qs Jc", "Ks Kd Kh Qs Jc"),
    ("2s 3d 4h 5s 6c", "As 2d 3h 4s 5c"),
    ("As Ks 9s 7s 5s", "As Ks 9s 7s 4s"),
    ("As Ad Ah Ks Kc", "As Ad Ah Qs Qc"),
    ("As Ad Ah 2s 2c", "Ks Kd Kh As Ac"),
    ("As Ad Ah Ac Kc", "As Ad Ah Ac Qc"),
    ("As Ad Ah Ac 2c", "Ks Kd Kh Kc Ac"),
    ("2s 3s 4s 5s 6s", "As 2s 3s 4s 5s")
}) Check(Eval(strong).CompareTo(Eval(weak)) > 0, $"Wrong comparison: {strong} / {weak}");

Check(Eval("As Kd Qh Js Tc").CompareTo(Eval("Ah Ks Qd Jc Ts")) == 0, "Suits broke tie");
foreach (var (seven, five) in new[]
{
    ("As Ad Ah Ks Kd Kh 2c", "As Ad Ah Ks Kd"), // Two trips
    ("As Ad Ks Kd Qs Qd Jc", "As Ad Ks Kd Qs"), // Three pairs
    ("As 2d 3h 4s 5c 6d 6h", "2d 3h 4s 5c 6d"),
    ("As Ks 9s 7s 5s 3s 2s", "As Ks 9s 7s 5s"),
    ("Ts Js Qs Ks As 2d 3c", "Ts Js Qs Ks As"), // Board alone
    ("As Ad Ah Ks Qs Js Ts", "As Ks Qs Js Ts"),
    ("As 2s 3s 4s 5s Kd Kh", "As 2s 3s 4s 5s")
})
{
    var value = Eval(seven);
    Check(value.CompareTo(Eval(five)) == 0, $"Wrong best five: {seven}");
    Check(value.BestFive.Count == 5 && value.BestFive.Distinct().Count() == 5 &&
        value.BestFive.All(Cards(seven).Contains), "Invalid best-five identities");
    Check(HandEvaluator.Evaluate(value.BestFive).CompareTo(value) == 0, "BestFive disagrees with value");
    Check(HandEvaluator.Evaluate(Cards(seven).Reverse().ToArray()).CompareTo(value) == 0, "Order dependence");
}
foreach (var invalid in new[] { "As Ad Ah Ac", "As As 2d 3c 4h", "As Ad Ah Ac Ks Kd Kh Kc" })
{
    try { Eval(invalid); throw new Exception("Invalid input accepted"); }
    catch (ArgumentException) { }
}

// Exhaustive standard five-card category census provides an independent oracle.
var deck = new Deck(new Random(0));
var full = Enumerable.Range(0, 52).Select(_ => deck.Draw()).ToArray();
var counts = new int[9];
var royal = 0;
for (var a = 0; a < 48; a++)
for (var b = a + 1; b < 49; b++)
for (var c = b + 1; c < 50; c++)
for (var d = c + 1; d < 51; d++)
for (var e = d + 1; e < 52; e++)
{
    var value = HandEvaluator.Evaluate(new[] { full[a], full[b], full[c], full[d], full[e] });
    counts[(int)value.Category]++;
    if (value.Label == "Royal flush") royal++;
}
Check(counts.SequenceEqual(new[] { 1302540, 1098240, 123552, 54912, 10200, 5108, 3744, 624, 40 }),
    $"Wrong category census: {string.Join(",", counts)}");
Check(royal == 4, "Expected four royal flushes");
Console.WriteLine("PASS: all 2,598,960 five-card hands; categories, royal flushes, kickers, ties, wheel, seven-card best-five selection and input checks.");

// Staged hands retain a single deck and evaluate only the cards revealed so far.
var outcomes = new HashSet<ShowdownOutcome>();
for (var seed = 0; seed < 1000; seed++)
{
    var hand = new PokerHand(new Random(seed));
    var expectedDeck = new Deck(new Random(seed));
    var firstPlayer = expectedDeck.Draw();
    var firstOpponent = expectedDeck.Draw();
    var hole = new[] { firstPlayer, expectedDeck.Draw() };
    var opponent = new[] { firstOpponent, expectedDeck.Draw() };
    Check(hand.Stage == HandStage.PreFlop && hand.Board.Count == 0 && hand.CanAdvance,
        "Wrong initial stage");
    Check(hand.HoleCards.SequenceEqual(hole) && hand.BestHand == null, "Pre-flop leaked evaluation");
    try { hand.Showdown(); throw new Exception("Allowed early showdown"); }
    catch (InvalidOperationException) { }
    var expectedBoard = new List<Card>();
    HandValue? previous = null;
    foreach (var (stage, count) in new[] { (HandStage.Flop, 3), (HandStage.Turn, 4), (HandStage.River, 5) })
    {
        var prefix = hand.Board.ToArray();
        hand.Advance();
        while (expectedBoard.Count < count) expectedBoard.Add(expectedDeck.Draw());
        Check(hand.Stage == stage && hand.Board.Count == count, "Wrong stage/count");
        Check(hand.Board.SequenceEqual(expectedBoard) && hand.Board.Take(prefix.Length).SequenceEqual(prefix),
            "Reveal changed earlier cards or deck order");
        Check(hand.HoleCards.SequenceEqual(hole), "Reveal changed hole cards");
        var visible = hand.Board.Concat(hand.HoleCards).ToArray();
        Check(visible.Distinct().Count() == count + 2, "Duplicate staged card");
        var value = hand.BestHand!;
        Check(value.CompareTo(HandEvaluator.Evaluate(visible)) == 0 && value.BestFive.All(visible.Contains),
            "Evaluation used unrevealed cards");
        if (previous != null) Check(value.CompareTo(previous) >= 0, "Hand weakened on reveal");
        previous = value;
        Check(hand.CanAdvance == (stage != HandStage.River), "Incorrect advance availability");
        Check(hand.CanShowdown == (stage == HandStage.River) && hand.Outcome == null &&
            hand.OpponentBestHand == null && hand.RevealedOpponentCards.Count == 0,
            "Opponent information leaked before showdown");
    }
    var river = hand.Board.ToArray();
    try { hand.Advance(); throw new Exception("Advanced beyond river"); }
    catch (InvalidOperationException) { }
    Check(hand.Stage == HandStage.River && hand.Board.SequenceEqual(river), "Invalid advance changed state");
    hand.Showdown();
    Check(hand.RevealedOpponentCards.SequenceEqual(opponent) &&
        river.Concat(hole).Concat(opponent).Distinct().Count() == 9, "Wrong opponent deal");
    var opponentValue = HandEvaluator.Evaluate(river.Concat(opponent).ToArray());
    var expectedOutcome = HandEvaluator.Evaluate(river.Concat(hole).ToArray()).CompareTo(opponentValue) switch
    {
        > 0 => ShowdownOutcome.PlayerWins,
        < 0 => ShowdownOutcome.OpponentWins,
        _ => ShowdownOutcome.Tie
    };
    Check(hand.OpponentBestHand!.CompareTo(opponentValue) == 0 && hand.Outcome == expectedOutcome &&
        !hand.CanShowdown && hand.Board.SequenceEqual(river), "Wrong showdown result");
    outcomes.Add(expectedOutcome);
    try { hand.Showdown(); throw new Exception("Allowed repeat showdown"); }
    catch (InvalidOperationException) { }
    var fresh = new PokerHand(new Random(seed));
    Check(fresh.Stage == HandStage.PreFlop && fresh.Board.Count == 0 && fresh.BestHand == null &&
        fresh.Outcome == null && fresh.RevealedOpponentCards.Count == 0,
        "Fresh hand retained stage/evaluation");
}
Check(outcomes.Count == 3, "Seeded showdown checks did not exercise all outcomes");
Console.WriteLine("PASS: 1,000 seeded hands; staged reveals, hidden opponent, nine unique cards, all showdown outcomes and fresh-hand reset.");

const Seat player = Seat.Player;
const Seat opponentSeat = Seat.Opponent;
var betting = new BettingHand(200, 200, player, new Random(1));
Check(betting.Pot == 3 && betting.Actor == player && betting.Legal == new LegalActions(1, 4, 200),
    "Incorrect heads-up blinds or first action");
try { betting.Act(opponentSeat, PokerAction.Check); throw new Exception("Allowed out-of-turn action"); }
catch (InvalidOperationException) { }
try { betting.Act(player, PokerAction.Wager, 3); throw new Exception("Allowed undersized raise"); }
catch (InvalidOperationException) { }
Check(betting.Pot == 3 && betting.Actor == player, "Invalid action mutated the hand");
betting.Act(player, PokerAction.Call);
Check(betting.Stage == HandStage.PreFlop && betting.Actor == opponentSeat && betting.Legal.Call == 0,
    "Big blind lost the option after a limp");
betting.Act(opponentSeat, PokerAction.Check);
Check(betting.Stage == HandStage.Flop && betting.Actor == opponentSeat && betting.Legal.MinWagerTo == 2,
    "Wrong flop order or opening bet");
betting.Act(opponentSeat, PokerAction.Wager, 6);
Check(betting.Legal == new LegalActions(6, 12, 198), "Wrong minimum after an opening bet");
betting.Act(player, PokerAction.Wager, 16);
Check(betting.Legal.MinWagerTo == 26, "Minimum raise did not track the last full increment");
betting.Act(opponentSeat, PokerAction.Fold);
Check(betting.Outcome == ShowdownOutcome.PlayerWins && betting.Folded == opponentSeat &&
    betting.AwardedPot == 16 && betting.ReturnedChips == 10 && betting.ReturnedTo == player &&
    betting.Stack(player) == 208 && betting.Stack(opponentSeat) == 192 &&
    betting.RevealedOpponentCards.Count == 0, "Fold payout or uncalled return was wrong");
try { betting.Act(player, PokerAction.Check); throw new Exception("Allowed action after payout"); }
catch (InvalidOperationException) { }

var shortRaise = new BettingHand(3, 200, player, new Random(2));
Check(shortRaise.Legal == new LegalActions(1, 3, 3), "Only the short all-in should be legal");
shortRaise.Act(player, PokerAction.Wager, 3);
Check(shortRaise.Actor == opponentSeat && shortRaise.Legal == new LegalActions(1, 0, 0),
    "Short all-in must offer call or fold without a raise");
shortRaise.Act(opponentSeat, PokerAction.Call);
Check(shortRaise.Board.Count == 5 && shortRaise.Outcome != null && shortRaise.AwardedPot == 6,
    "Called short all-in did not run the full board");

var shortCall = new BettingHand(200, 7, player, new Random(3));
shortCall.Act(player, PokerAction.Wager, 200);
Check(shortCall.Legal == new LegalActions(5, 0, 0), "Short call did not cap at the stack");
shortCall.Act(opponentSeat, PokerAction.Call);
Check(shortCall.AwardedPot == 14 && shortCall.ReturnedChips == 193 && shortCall.ReturnedTo == player &&
    shortCall.Stack(player) + shortCall.Stack(opponentSeat) == 207, "Unequal all-in settlement was wrong");

var shortBlind = new BettingHand(200, 1, player, new Random(4));
Check(shortBlind.Actor == null && shortBlind.Board.Count == 5 && shortBlind.AwardedPot == 2,
    "Matched all-in blind should run out immediately");
var forcedDecision = new BettingHand(1, 200, player, new Random(5));
Check(forcedDecision.Actor == null && forcedDecision.AwardedPot == 2 && forcedDecision.ReturnedChips == 1,
    "Excess big blind should be returned against an all-in small blind");

var tiedCheckdown = false;
for (var seed = 0; seed < 100 && !tiedCheckdown; seed++)
{
    var hand = new BettingHand(100, 150, opponentSeat, new Random(seed));
    while (hand.Actor is { } actor)
        hand.Act(actor, hand.Legal.Call > 0 ? PokerAction.Call : PokerAction.Check);
    if (hand.Outcome != ShowdownOutcome.Tie) continue;
    Check(hand.AwardedPot == 4 && hand.Stack(player) == 100 && hand.Stack(opponentSeat) == 150,
        "A tied checkdown did not award half the pot to each seat");
    tiedCheckdown = true;
}
Check(tiedCheckdown, "Split payout check did not reach a tie");

var sawSplit = false;
var sawRefill = false;
for (var seed = 0; seed < 50; seed++)
{
    var moves = new Random(seed);
    var session = new FreePlaySession(new Random(seed), new Random(seed + 1000));
    var expectedChips = 400;
    for (var number = 1; number <= 20; number++)
    {
        var hand = session.Hand;
        Check(session.HandNumber == number && hand.Dealer == (number % 2 == 1 ? player : opponentSeat),
            "Dealer did not alternate");
        for (var turn = 0; ; turn++)
        {
            Check(hand.Stack(player) >= 0 && hand.Stack(opponentSeat) >= 0 &&
                hand.Stack(player) + hand.Stack(opponentSeat) + hand.Pot == expectedChips,
                "Chip conservation failed");
            if (hand.Outcome != null) break;
            Check(turn < 100 && hand.Actor == player && hand.RevealedOpponentCards.Count == 0,
                "Bot did not yield a valid, private player turn");
            var legal = hand.Legal;
            var roll = moves.Next(10);
            if (legal.CanWager && roll >= 7)
                session.Act(PokerAction.Wager, roll == 9 ? legal.MaxWagerTo : legal.MinWagerTo);
            else if (legal.Call > 0 && roll == 0)
                session.Act(PokerAction.Fold);
            else
                session.Act(legal.Call == 0 ? PokerAction.Check : PokerAction.Call);
        }
        Check(hand.Pot == 0 && hand.AwardedPot > 0 && hand.Actor == null, "Hand did not close");
        if (hand.Folded != null)
            Check(hand.RevealedOpponentCards.Count == 0, "Fold exposed opponent cards");
        else
            Check(hand.Board.Count == 5 && hand.RevealedOpponentCards.Count == 2, "Incomplete showdown");
        if (hand.Outcome == ShowdownOutcome.Tie)
        {
            sawSplit = true;
            Check(hand.AwardedPot % 2 == 0 && hand.BestHand!.CompareTo(hand.OpponentBestHand) == 0,
                "Invalid split pot");
        }
        var refilled = hand.Stack(player) == 0 || hand.Stack(opponentSeat) == 0;
        if (refilled) expectedChips += FreePlaySession.BuyIn;
        session.NextHand();
        sawRefill |= refilled;
        Check((session.RefillNotice.Length > 0) == refilled, "Refill notice did not match busted stacks");
    }
}
Check(sawSplit && sawRefill, "Session checks did not reach splits and refills");
Console.WriteLine("PASS: heads-up blind order, minimum raises, folds, short blinds, unequal all-ins, and 1,000 Free Play hands with splits, refills and chip conservation.");

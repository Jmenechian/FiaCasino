using System;
using System.Collections.Generic;
using System.Linq;
using FiaCasino.Cards;
using FiaCasino.Poker;
using Godot;

namespace FiaCasino.UI;

public partial class AppShell : Control
{
    private Control _page = null!;
    private Label? _readout;
    private bool _showReadout = true;
    private string _screen = "";
    private string _settingsReturn = "lobby";

    private FreePlaySession? _session;

    private void RestartFreePlay()
    {
        _session = new FreePlaySession();
        ShowTable();
    }

    private void Act(PokerAction action, int wagerTo = 0)
    {
        _session!.Act(action, wagerTo);
        ShowTable();
    }

    public override void _Ready()
    {
        GetWindow().MinSize = new Vector2I(960, 600);
        Theme = BuildTheme();
        ShowLobby();
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
        {
            if (_screen == "settings") ReturnFromSettings();
            else if (_screen == "table") ShowLobby();
            GetViewport().SetInputAsHandled();
        }
    }

    private VBoxContainer NewPage(string screen)
    {
        if (IsInstanceValid(_page)) { RemoveChild(_page); _page.QueueFree(); }
        _screen = screen;
        _readout = null;
        var margin = new MarginContainer();
        _page = margin;
        AddChild(margin);
        margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        foreach (var side in new[] { "left", "right", "top", "bottom" })
            margin.AddThemeConstantOverride("margin_" + side, 36);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 20);
        margin.AddChild(column);
        return column;
    }

    private void ShowLobby()
    {
        var page = NewPage("lobby");
        page.AddChild(Text("FIACASINO", 18, "cdb781"));
        var center = new CenterContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        page.AddChild(center);
        var menu = new VBoxContainer { CustomMinimumSize = new Vector2(330, 0) };
        menu.AddThemeConstantOverride("separation", 16);
        center.AddChild(menu);
        menu.AddChild(Text("Take a seat.", 52, "f4ecda", true));
        menu.AddChild(Text("A little casino. Just for the game.", 18, "a6bbb1", true));
        menu.AddChild(new Control { CustomMinimumSize = new Vector2(0, 24) });
        var poker = ActionButton("Poker", ShowTable);
        menu.AddChild(poker);
        menu.AddChild(ActionButton("Settings", () => ShowSettings("lobby")));
        menu.AddChild(ActionButton("Exit", () => GetTree().Quit()));
        page.AddChild(Text("OFFLINE  /  NO REAL MONEY", 14, "80998e", true));
        poker.GrabFocus();
    }

    private void ShowTable()
    {
        if (_session == null) { RestartFreePlay(); return; }
        var hand = _session.Hand;
        var page = NewPage("table");
        page.AddThemeConstantOverride("separation", 10);
        var header = new HBoxContainer();
        header.AddThemeConstantOverride("separation", 16);
        page.AddChild(header);
        header.AddChild(ActionButton("Back", ShowLobby, 100));
        header.AddChild(Text($"FREE PLAY  /  HAND {_session.HandNumber}  /  BLINDS 1–2", 18, "f4ecda"));
        header.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        header.AddChild(ActionButton("Settings", () => ShowSettings("table"), 130));

        var table = new PanelContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        table.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("174b3c"), BorderColor = new Color("547560"),
            BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2,
            CornerRadiusTopLeft = 110, CornerRadiusTopRight = 110,
            CornerRadiusBottomLeft = 110, CornerRadiusBottomRight = 110,
            ContentMarginTop = 20, ContentMarginBottom = 20,
            ContentMarginLeft = 32, ContentMarginRight = 32
        });
        page.AddChild(table);
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", 8);
        table.AddChild(layout);
        layout.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
        var stage = hand.Stage == HandStage.PreFlop ? "PRE-FLOP" : hand.Stage.ToString().ToUpperInvariant();
        var pot = hand.Outcome == null ? $"POT {hand.Pot}" : $"POT PAID {hand.AwardedPot}";
        layout.AddChild(Text($"{stage}  /  {pot}", 17, "f4ecda", true));
        layout.AddChild(CardRow(hand.Board, "Board", 5));
        layout.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
        var seats = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        seats.AddThemeConstantOverride("separation", 56);
        layout.AddChild(seats);
        var player = new VBoxContainer();
        seats.AddChild(player);
        player.AddChild(Text(SeatLabel(hand, Seat.Player, "YOU"), 15, "b0c8b5", true));
        player.AddChild(CardRow(hand.HoleCards, "Your hand"));
        var opponent = new VBoxContainer();
        seats.AddChild(opponent);
        opponent.AddChild(Text(SeatLabel(hand, Seat.Opponent, "OPPONENT"), 15, "b0c8b5", true));
        opponent.AddChild(CardRow(hand.RevealedOpponentCards, "Opponent", 2, faceDown: true));

        _readout = Text("CARD INSPECTOR  /  Click a visible card", 15, "d4c69f", true);
        _readout.Visible = _showReadout;
        page.AddChild(_readout);
        var summary = $"BEST HAND  /  {hand.BestHand?.Label ?? "Available from the flop"}";
        if (hand.OpponentBestHand is { } opponentBest)
            summary += $"     OPPONENT  /  {opponentBest.Label}";
        page.AddChild(Text(summary, 17, "d4c69f", true));

        if (hand.Outcome is { } outcome)
        {
            var result = outcome switch
            {
                ShowdownOutcome.PlayerWins => $"You win {hand.AwardedPot}",
                ShowdownOutcome.OpponentWins => $"Opponent wins {hand.AwardedPot}",
                _ => $"Split pot  /  {hand.AwardedPot / 2} each"
            };
            if (hand.Folded is { } folded) result += folded == Seat.Player ? "  /  You folded" : "  /  Opponent folded";
            if (hand.ReturnedChips > 0)
                result += $"  /  {hand.ReturnedChips} returned to {(hand.ReturnedTo == Seat.Player ? "you" : "opponent")}";
            page.AddChild(Text(result, 19, "f4ecda", true));
            var controls = ActionRow(page);
            var next = ActionButton("Next hand", () => { _session.NextHand(); ShowTable(); });
            controls.AddChild(next);
            controls.AddChild(ActionButton("Restart Free Play", RestartFreePlay));
            next.GrabFocus();
            return;
        }

        var activity = string.Join("  /  ", new[] { _session.RefillNotice, _session.OpponentActivity, "Your turn" }
            .Where(message => message.Length > 0));
        page.AddChild(Text(activity, 17, "f4ecda", true));
        var legal = hand.Legal;
        var actions = ActionRow(page);
        if (legal.Call > 0) actions.AddChild(ActionButton("Fold", () => Act(PokerAction.Fold)));
        var callLabel = legal.Call == hand.Stack(Seat.Player) ? $"Call all-in {legal.Call}" : $"Call {legal.Call}";
        actions.AddChild(legal.Call > 0
            ? ActionButton(callLabel, () => Act(PokerAction.Call))
            : ActionButton("Check", () => Act(PokerAction.Check)));
        if (legal.CanWager)
        {
            var amount = new SpinBox
            {
                MinValue = legal.MinWagerTo, MaxValue = legal.MaxWagerTo, Step = 1, Value = legal.MinWagerTo,
                CustomMinimumSize = new Vector2(124, 0),
                TooltipText = "Total chips to commit on this street"
            };
            actions.AddChild(ActionButton(hand.Committed(Seat.Player) + hand.Committed(Seat.Opponent) == 0
                ? "Bet to" : "Raise to", () => { amount.Apply(); Act(PokerAction.Wager, (int)amount.Value); }));
            actions.AddChild(amount);
            actions.AddChild(ActionButton("All-in", () => Act(PokerAction.Wager, legal.MaxWagerTo)));
        }
    }

    private static HBoxContainer ActionRow(VBoxContainer page)
    {
        var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 12);
        page.AddChild(row);
        return row;
    }

    private static string SeatLabel(BettingHand hand, Seat seat, string name)
    {
        var dealer = seat == hand.Dealer ? " (D)" : "";
        return $"{name}{dealer}  /  STACK {hand.Stack(seat)}  /  IN {hand.Committed(seat)}";
    }

    private HBoxContainer CardRow(IReadOnlyList<Card> cards, string location, int slots = 0, bool faceDown = false)
    {
        var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 16);
        for (var i = 0; i < cards.Count; i++)
        {
            var position = i + 1;
            row.AddChild(new CardView(cards[i], card =>
            {
                var message = $"{location} {position}  /  {card.DisplayName}  /  {card.Id}";
                if (_readout != null) _readout.Text = message;
                GD.Print($"[CardInspector] {message}");
            }));
        }
        for (var i = cards.Count; i < slots; i++)
        {
            var empty = new Panel { CustomMinimumSize = new Vector2(116, 164),
                MouseFilter = MouseFilterEnum.Ignore };
            empty.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(faceDown ? "1c352d" : "174b3c"),
                BorderColor = new Color(faceDown ? "cdb781" : "547560"),
                BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
                CornerRadiusTopLeft = 9, CornerRadiusTopRight = 9,
                CornerRadiusBottomLeft = 9, CornerRadiusBottomRight = 9
            });
            if (faceDown)
            {
                var marker = Text("◆", 32, "cdb781", true);
                marker.VerticalAlignment = VerticalAlignment.Center;
                empty.AddChild(marker);
                marker.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            }
            row.AddChild(empty);
        }
        return row;
    }

    private void ShowSettings(string returnTo)
    {
        _settingsReturn = returnTo;
        var page = NewPage("settings");
        page.AddChild(ActionButton("Back", ReturnFromSettings, 120));
        var center = new CenterContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        page.AddChild(center);
        var settings = new VBoxContainer { CustomMinimumSize = new Vector2(430, 0) };
        settings.AddThemeConstantOverride("separation", 24);
        center.AddChild(settings);
        settings.AddChild(Text("Settings", 42, "f4ecda"));
        var inspector = new CheckButton { Text = "Show card inspector", ButtonPressed = _showReadout };
        inspector.Toggled += enabled => _showReadout = enabled;
        settings.AddChild(inspector);
        settings.AddChild(Text("Card clicks always write to the local log.\nThis preference lasts until you close the app.", 17, "a6bbb1"));
        settings.AddChild(Text("Built with Godot Engine (MIT).\ngodotengine.org/license", 14, "80998e"));
        inspector.GrabFocus();
    }

    private void ReturnFromSettings()
    {
        if (_settingsReturn == "table") ShowTable(); else ShowLobby();
    }

    private static Label Text(string text, int size, string color, bool centered = false)
    {
        var label = new Label { Text = text, HorizontalAlignment = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left,
            MouseFilter = MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", new Color(color));
        return label;
    }

    private static Button ActionButton(string text, Action action, int width = 0)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(width, 54),
            MouseDefaultCursorShape = CursorShape.PointingHand };
        button.Pressed += action;
        return button;
    }

    private static Theme BuildTheme()
    {
        var theme = new Theme { DefaultFontSize = 20 };
        foreach (var (state, color) in new[] { ("normal", "1c352d"), ("hover", "2d5141"), ("pressed", "395f4c") })
            theme.SetStylebox(state, "Button", new StyleBoxFlat
            {
                BgColor = new Color(color), BorderColor = new Color("59725b"),
                BorderWidthBottom = 1, BorderWidthTop = 1, BorderWidthLeft = 1, BorderWidthRight = 1,
                CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
                ContentMarginLeft = 18, ContentMarginRight = 18
            });
        theme.SetColor("font_color", "Button", new Color("f4ecda"));
        theme.SetColor("font_hover_color", "Button", new Color("fff8e8"));
        return theme;
    }
}

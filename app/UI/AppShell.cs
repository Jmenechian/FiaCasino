using System;
using System.Collections.Generic;
using System.Linq;
using FiaCasino.Cards;
using FiaCasino.Poker;
using Godot;

namespace FiaCasino.UI;

public partial class AppShell : Control
{
    private static readonly Vector2 BoardCardSize = new(92, 128);
    private static readonly Vector2 OpponentCardSize = new(74, 104);
    private Control _page = null!;
    private TextureRect _loungeBackground = null!;
    private Theme _loungeTheme = null!;
    private readonly SystemFont _loungeTitle = new() { FontNames = new[] { "Georgia", "Times New Roman" } };
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
        _loungeTheme = BuildTheme(lounge: true);
        _loungeBackground = new TextureRect
        {
            Texture = new GradientTexture2D
            {
                Gradient = new Gradient { Colors = new[] { new Color("382b20"), new Color("14120f") } },
                Fill = GradientTexture2D.FillEnum.Radial,
                FillFrom = new Vector2(0.5f, 0), FillTo = new Vector2(0.5f, 0.85f),
                Width = 1024, Height = 640
            },
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_loungeBackground);
        _loungeBackground.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
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
        _loungeBackground.Visible = screen == "table";
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
        page.Theme = _loungeTheme;
        page.AddThemeConstantOverride("separation", 12);
        var header = new HBoxContainer();
        header.AddThemeConstantOverride("separation", 16);
        page.AddChild(header);
        header.AddChild(ActionButton("Back", ShowLobby, 100));
        var branding = new VBoxContainer();
        header.AddChild(branding);
        var title = Text("FiaCasino", 31, "f7ead6");
        title.AddThemeFontOverride("font", _loungeTitle);
        branding.AddChild(title);
        branding.AddChild(Text("FREE PLAY  /  NO-LIMIT HOLD’EM", 13, "d0b999"));
        header.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        header.AddChild(Text($"HAND {_session.HandNumber}  /  BLINDS 1–2", 15, "d0b999"));
        header.AddChild(ActionButton("Settings", () => ShowSettings("table"), 130));

        var rail = new PanelContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        var railStyle = LoungeFrame("392a20", "b78b52", 2, 140, 9);
        railStyle.ShadowColor = new Color(0, 0, 0, 0.45f);
        railStyle.ShadowSize = 12;
        railStyle.ShadowOffset = new Vector2(0, 6);
        rail.AddThemeStyleboxOverride("panel", railStyle);
        page.AddChild(rail);
        var felt = new PanelContainer();
        felt.AddThemeStyleboxOverride("panel", LoungeFrame("173e32", "876944", 1, 131, 14));
        rail.AddChild(felt);
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", 5);
        felt.AddChild(layout);
        layout.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
        layout.AddChild(Text(SeatLabel(hand, Seat.Opponent, "OPPONENT"), 15, "efe1c7", true));
        layout.AddChild(CardRow(hand.RevealedOpponentCards, "Opponent", OpponentCardSize, 2, faceDown: true));

        var stage = hand.Stage == HandStage.PreFlop ? "PRE-FLOP" : hand.Stage.ToString().ToUpperInvariant();
        var potLabel = hand.Outcome == null ? "POT" : "POT PAID";
        var pot = hand.Outcome == null ? hand.Pot : hand.AwardedPot;
        var potBadge = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        var potStyle = LoungeFrame("102c23", "6e7455", 1, 18, 6);
        potStyle.ContentMarginLeft = 14;
        potStyle.ContentMarginRight = 14;
        potBadge.AddThemeStyleboxOverride("panel", potStyle);
        layout.AddChild(potBadge);
        var potRow = new HBoxContainer();
        potRow.AddThemeConstantOverride("separation", 12);
        potBadge.AddChild(potRow);
        potRow.AddChild(Text($"{stage}  /  {potLabel}", 14, "d0b999"));
        potRow.AddChild(Text($"{pot}", 23, "ffe3a9"));
        layout.AddChild(CardRow(hand.Board, "Board", BoardCardSize, 5));
        layout.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
        layout.AddChild(CardRow(hand.HoleCards, "Your hand", BoardCardSize));
        layout.AddChild(Text(SeatLabel(hand, Seat.Player, "YOU"), 15, "efe1c7", true));
        layout.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

        var dock = new PanelContainer();
        dock.AddThemeStyleboxOverride("panel", LoungeFrame("251e18", "614933", 1, 12, 12));
        page.AddChild(dock);
        var footer = new VBoxContainer();
        dock.AddChild(footer);
        _readout = Text("CARD INSPECTOR  /  Click a visible card", 14, "c4af93");
        _readout.Visible = _showReadout;
        footer.AddChild(_readout);
        var dockRow = new HBoxContainer();
        dockRow.AddThemeConstantOverride("separation", 16);
        footer.AddChild(dockRow);
        var status = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        dockRow.AddChild(status);
        var summary = $"BEST HAND  /  {hand.BestHand?.Label ?? "Available from the flop"}";
        if (hand.OpponentBestHand is { } opponentBest)
            summary += $"     OPPONENT  /  {opponentBest.Label}";
        status.AddChild(Text(summary, 14, "d0b999"));

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
            status.AddChild(Text(result, 19, "f4ecda"));
            var controls = ActionRow(dockRow);
            var next = ActionButton("Next hand", () => { _session.NextHand(); ShowTable(); });
            next.ThemeTypeVariation = "LoungePrimary";
            controls.AddChild(next);
            controls.AddChild(ActionButton("Restart Free Play", RestartFreePlay));
            next.GrabFocus();
            return;
        }

        var activity = string.Join("  /  ", new[] { _session.RefillNotice, _session.OpponentActivity, "Your turn" }
            .Where(message => message.Length > 0));
        var activityLabel = Text(activity, 17, "f4ecda");
        activityLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        status.AddChild(activityLabel);
        var legal = hand.Legal;
        var actions = ActionRow(dockRow);
        if (legal.Call > 0) actions.AddChild(ActionButton("Fold", () => Act(PokerAction.Fold)));
        var callLabel = legal.Call == hand.Stack(Seat.Player) ? $"Call all-in {legal.Call}" : $"Call {legal.Call}";
        var passive = legal.Call > 0
            ? ActionButton(callLabel, () => Act(PokerAction.Call))
            : ActionButton("Check", () => Act(PokerAction.Check));
        passive.ThemeTypeVariation = "LoungePrimary";
        actions.AddChild(passive);
        if (legal.CanWager)
        {
            var amount = new SpinBox
            {
                MinValue = legal.MinWagerTo, MaxValue = legal.MaxWagerTo, Step = 1, Value = legal.MinWagerTo,
                CustomMinimumSize = new Vector2(104, 0),
                TooltipText = "Total chips to commit on this street"
            };
            actions.AddChild(amount);
            actions.AddChild(ActionButton(hand.Committed(Seat.Player) + hand.Committed(Seat.Opponent) == 0
                ? "Bet to" : "Raise to", () => { amount.Apply(); Act(PokerAction.Wager, (int)amount.Value); }));
            actions.AddChild(ActionButton("All-in", () => Act(PokerAction.Wager, legal.MaxWagerTo)));
        }
    }

    private static HBoxContainer ActionRow(HBoxContainer dock)
    {
        var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 12);
        dock.AddChild(row);
        return row;
    }

    private static string SeatLabel(BettingHand hand, Seat seat, string name)
    {
        var dealer = seat == hand.Dealer ? " (D)" : "";
        return $"{name}{dealer}  /  STACK {hand.Stack(seat)}  /  IN {hand.Committed(seat)}";
    }

    private HBoxContainer CardRow(IReadOnlyList<Card> cards, string location, Vector2 size, int slots = 0, bool faceDown = false)
    {
        var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 16);
        for (var i = 0; i < cards.Count; i++)
        {
            var position = i + 1;
            row.AddChild(new CardView(cards[i], size, card =>
            {
                var message = $"{location} {position}  /  {card.DisplayName}  /  {card.Id}";
                if (_readout != null) _readout.Text = message;
                GD.Print($"[CardInspector] {message}");
            }));
        }
        for (var i = cards.Count; i < slots; i++)
        {
            var empty = new Panel { CustomMinimumSize = size,
                MouseFilter = MouseFilterEnum.Ignore };
            empty.AddThemeStyleboxOverride("panel", LoungeFrame(
                faceDown ? "1e2721" : "14362c", faceDown ? "c5a16c" : "55725b", 1, 8, 0));
            if (faceDown)
            {
                var inset = new Panel { MouseFilter = MouseFilterEnum.Ignore };
                inset.AddThemeStyleboxOverride("panel", LoungeFrame("293b30", "6e7358", 1, 5, 0));
                empty.AddChild(inset);
                inset.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
                inset.OffsetLeft = 5;
                inset.OffsetTop = 5;
                inset.OffsetRight = -5;
                inset.OffsetBottom = -5;
                var marker = Text("◆", 25, "d6ad74", true);
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

    private static StyleBoxFlat LoungeFrame(string fill, string border, int width, int radius, int padding) => new()
    {
        BgColor = new Color(fill), BorderColor = new Color(border),
        BorderWidthBottom = width, BorderWidthTop = width, BorderWidthLeft = width, BorderWidthRight = width,
        CornerRadiusBottomLeft = radius, CornerRadiusBottomRight = radius,
        CornerRadiusTopLeft = radius, CornerRadiusTopRight = radius,
        ContentMarginLeft = padding, ContentMarginRight = padding,
        ContentMarginTop = padding, ContentMarginBottom = padding
    };

    private static Theme BuildTheme(bool lounge = false)
    {
        var theme = new Theme { DefaultFontSize = lounge ? 18 : 20 };
        var palette = lounge
            ? new[] { ("normal", "33271e"), ("hover", "49372a"), ("pressed", "5a4432") }
            : new[] { ("normal", "1c352d"), ("hover", "2d5141"), ("pressed", "395f4c") };
        foreach (var (state, color) in palette)
            theme.SetStylebox(state, "Button", new StyleBoxFlat
            {
                BgColor = new Color(color), BorderColor = new Color(lounge ? "99764e" : "59725b"),
                BorderWidthBottom = 1, BorderWidthTop = 1, BorderWidthLeft = 1, BorderWidthRight = 1,
                CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
                ContentMarginLeft = 18, ContentMarginRight = 18
            });
        theme.SetColor("font_color", "Button", new Color("f4ecda"));
        theme.SetColor("font_hover_color", "Button", new Color("fff8e8"));
        if (lounge)
        {
            theme.SetTypeVariation("LoungePrimary", "Button");
            foreach (var (state, color) in new[] { ("normal", "e6c087"), ("hover", "f6d7a4"), ("pressed", "c9a16b") })
            {
                var style = LoungeFrame(color, "f4d5a4", 1, 8, 18);
                style.ContentMarginTop = 5;
                style.ContentMarginBottom = 5;
                theme.SetStylebox(state, "LoungePrimary", style);
            }
            foreach (var state in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color" })
                theme.SetColor(state, "LoungePrimary", new Color("261b11"));
            theme.SetStylebox("normal", "LineEdit", LoungeFrame("1d1712", "99764e", 1, 8, 8));
            theme.SetColor("font_color", "LineEdit", new Color("f4ecda"));
        }
        return theme;
    }
}

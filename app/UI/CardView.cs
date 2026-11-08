using System;
using FiaCasino.Cards;
using Godot;

namespace FiaCasino.UI;

public partial class CardView : Button
{
    public Card Identity { get; }

    public CardView(Card identity, Vector2 size, Action<Card> onSelected)
    {
        Identity = identity;
        Name = identity.Id;
        TooltipText = identity.DisplayName;
        CustomMinimumSize = size;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        AddThemeStyleboxOverride("normal", Frame("e9e3d5", 1));
        AddThemeStyleboxOverride("hover", Frame("d0b77e", 3));
        AddThemeStyleboxOverride("pressed", Frame("d0b77e", 4));
        AddThemeStyleboxOverride("focus", Frame("d0b77e", 3, true));
        var artwork = new TextureRect
        {
            Texture = GD.Load<Texture2D>($"res://Assets/Cards/{identity.Id}.svg"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(artwork);
        artwork.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        artwork.OffsetLeft = 5;
        artwork.OffsetTop = 5;
        artwork.OffsetRight = -5;
        artwork.OffsetBottom = -5;
        Pressed += () => onSelected(Identity);
    }

    private static StyleBoxFlat Frame(string border, int width, bool transparent = false) => new()
    {
        BgColor = transparent ? Colors.Transparent : new Color("f5f0e6"),
        BorderColor = new Color(border),
        BorderWidthLeft = width, BorderWidthRight = width,
        BorderWidthTop = width, BorderWidthBottom = width,
        CornerRadiusTopLeft = 9, CornerRadiusTopRight = 9,
        CornerRadiusBottomLeft = 9, CornerRadiusBottomRight = 9,
        ShadowColor = new Color(0, 0, 0, 0.25f), ShadowSize = transparent ? 0 : 5
    };
}

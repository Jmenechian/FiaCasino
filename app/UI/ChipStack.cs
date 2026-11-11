using Godot;

namespace FiaCasino.UI;

// Original vector chip artwork; amount labels carry the exact value.
public partial class ChipStack : Control
{
    public ChipStack()
    {
        CustomMinimumSize = new Vector2(44, 32);
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Draw()
    {
        for (var layer = 0; layer < 3; layer++)
        {
            var center = new Vector2(22, 22 - layer * 5);
            DrawCircle(center + new Vector2(0, 2), 14, new Color("49251e"));
            DrawCircle(center, 14, new Color("b85b43"));
            DrawArc(center, 12, 0, Mathf.Tau, 40, new Color("f6dfb2"), 2, true);
            for (var stripe = 0; stripe < 6; stripe++)
            {
                var direction = Vector2.FromAngle(stripe * Mathf.Tau / 6);
                DrawLine(center + direction * 9, center + direction * 14, new Color("f6dfb2"), 3, true);
            }
            DrawCircle(center, 7, new Color("843d30"));
        }
    }
}

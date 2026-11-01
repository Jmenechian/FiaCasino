"""Generate the original SVG deck without fonts or external artwork (Python 3)."""
from pathlib import Path

OUTPUT = Path(__file__).resolve().parents[1] / "app/Assets/Cards"
RANKS = {
    "ace": "M2 26 L10 2 L18 26 M5 18 H15",
    "two": "M2 7 C2 -1 19 -1 19 7 C19 13 2 19 2 26 H20",
    "three": "M2 4 C10 -1 20 1 18 8 C17 12 13 14 9 14 M9 14 C24 12 23 30 2 25",
    "four": "M15 26 V2 L2 18 H21",
    "five": "M19 2 H3 V13 C21 6 25 28 3 26",
    "six": "M18 3 C3 -4 -4 29 12 26 C25 24 19 9 4 14",
    "seven": "M2 2 H19 L6 26",
    "eight": "M10 13 C-2 12 0 2 10 2 C20 2 22 12 10 13 C-4 14 -2 26 10 26 C22 26 24 14 10 13",
    "nine": "M3 25 C18 32 25 -1 9 2 C-4 4 2 19 17 14",
    "ten": "M0 7 L5 2 V26 M17 2 C8 2 8 26 17 26 C26 26 26 2 17 2",
    "jack": "M3 2 H19 M15 2 V19 C15 29 2 28 2 19",
    "queen": "M10 2 C-2 2 -2 26 10 26 C22 26 22 2 10 2 M12 20 L21 29",
    "king": "M3 2 V26 M18 2 L3 16 M8 12 L19 26",
}
SUITS = {
    "clubs": ("#23362f", "M -7,0 C -36,-20 -42,22 -19,22 C -9,22 -4,15 0,10 C 4,15 9,22 19,22 C 42,22 36,-20 7,0 C 30,-32 -30,-32 -7,0 Z M -4,8 L -11,33 L 11,33 L 4,8 Z"),
    "diamonds": ("#aa3e47", "M 0,-36 L 27,0 L 0,36 L -27,0 Z"),
    "hearts": ("#aa3e47", "M 0,30 C -9,18 -32,1 -32,-14 C -32,-34 -10,-36 0,-20 C 10,-36 32,-34 32,-14 C 32,1 9,18 0,30 Z"),
    "spades": ("#23362f", "M 0,-32 C -10,-18 -30,-8 -30,8 C -30,28 -8,30 0,12 C 8,30 30,28 30,8 C 30,-8 10,-18 0,-32 Z M -4,10 L -10,34 L 10,34 L 4,10 Z"),
}


def generate():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for rank, rank_path in RANKS.items():
        for suit, (color, suit_path) in SUITS.items():
            corner = (
                f'<path d="{rank_path}" fill="none" stroke="{color}" stroke-width="2.8" '
                'stroke-linecap="round" stroke-linejoin="round"/>'
                f'<path d="{suit_path}" transform="translate(10 44) scale(.22)" fill="{color}"/>'
            )
            svg = (
                '<svg xmlns="http://www.w3.org/2000/svg" width="212" height="308" viewBox="0 0 212 308">'
                '<rect width="212" height="308" rx="10" fill="#f5f0e6"/>'
                f'<g transform="translate(14 14)">{corner}</g>'
                f'<g transform="translate(198 294) rotate(180)">{corner}</g>'
                f'<path d="{suit_path}" transform="translate(106 154) scale(1.1)" fill="{color}"/></svg>'
            )
            (OUTPUT / f"{rank}_of_{suit}.svg").write_text(svg, encoding="utf-8")


if __name__ == "__main__":
    generate()

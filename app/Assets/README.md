# Card assets

The 52 SVG card faces were created directly for this project. No downloaded artwork, fonts, or generated concept images are included in the application. No external card-art attribution or license is required. The complete deck extends the original seven faces with the same colors, paths, dimensions, and layout.

Each file is named after the card identity (`ace_of_spades.svg`, etc.). `CardView` loads the corresponding texture; game data does not derive identity from pixels. Regenerate the faces with `python tools/generate_card_faces.py` from the repository root. There is no artwork selection system.

Godot provides the default interface font. Engine third-party notices are available at https://godotengine.org/license/.

Poker chips are original procedural vector drawings in `UI/ChipStack.cs`; they require no external artwork attribution. The stack icon is decorative; the adjacent number supplies the exact chip amount.

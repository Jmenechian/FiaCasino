# FiaCasino

A personal, free-to-play, offline collection of games in one desktop application. The name is provisional.

Built with Godot 4.7.2 .NET and C#, targeting Windows x64. Linux has not been tested.

## Current status

Milestone 1 is accepted: a lobby with Poker, Settings, and Exit, plus a table preview with five board cards and two player cards. Click a card to inspect its name and position. Settings can hide the inspector for the current session; clicks still write to the local log.

Milestone 2 is accepted: a full 52-card deck and a Deal button. Entering Poker gives an initial five-card board and two player cards. Deal shuffles a fresh deck and draws seven cards without replacement, so there are no duplicates within a deal. Cards can repeat between deals. The current deal survives Settings and lobby navigation until the app closes.

Milestone 3 is implemented and awaiting user review: a BEST HAND label shows the strongest five-card category from the seven displayed cards, including royal flush. It updates on Deal and remains visible when the inspector is hidden. The best hand can use zero, one, or both player cards.

Card identity remains separate from the 52 SVG faces. There is no betting or bot play yet.

## Run the Windows build

If built locally, launch `builds/windows/FiaCasino.exe`. Keep `FiaCasino.pck` and the `data_FiaCasino_windows_x86_64` folder alongside it. Build outputs are not committed to GitHub.

Use Back or Escape to return from the table to the lobby or from Settings to its previous screen. The inspector preference resets when the app closes.

The default Windows log is `%APPDATA%/Godot/app_userdata/FiaCasino/logs/godot.log`. Card entries start with `[CardInspector]`.

## Build from source

Install the **.NET edition of Godot 4.7.2** and a .NET SDK supporting `net8.0` (this project was built using SDK 10.0.401). Open `app/project.godot` in Godot, build the C# project, and press F6/F5 to run the scene/project. Initial package restore may need internet; running the application does not.

Alternatively, from the repository root, with the Godot .NET executable available as `godot`:

```powershell
dotnet build app/FiaCasino.csproj
godot --headless --path app --editor --import --quit
godot --path app
```

For a Windows export, install the matching **.NET export templates** through Godot's export template manager, then run:

```powershell
New-Item -ItemType Directory -Force builds/windows
godot --headless --path app --export-release "Windows Desktop" --quit
```

The local portable editor, if present, is under `local-notes/tools/godot/`; it is not included in a clone. Use the full executable path if Godot is not on PATH.

## Logic checks and artwork

With .NET SDK 10 installed, run `dotnet run --project tests/DeckChecks/DeckChecks.csproj` for standalone deck/deal checks (no Godot required). The application itself still targets `net8.0`. These checks cover deck contents, draws without replacement, exhaustion, shuffle variation, and the presence of all face files.

Run `dotnet run --project tests/PokerChecks/PokerChecks.csproj -c Release` for poker evaluation checks: an exhaustive census of all 2,598,960 five-card hands, royal flush labels, tie-break comparisons, ace-low straights, and seven-card best-five selection. Like DeckChecks, this standalone runner uses .NET 10; the application targets net8.0.

The original SVG faces can be regenerated with Python 3: `python tools/generate_card_faces.py`. See [asset notes](app/Assets/README.md).

## Working on this repository

Read [AGENTS.md](AGENTS.md) for the working agreement and [MILESTONES.md](MILESTONES.md) for progress and review status.

Local working notes live in the ignored `local-notes/` folder, which can be opened as an Obsidian vault. They are not included in clones or backed up by GitHub. Required project constraints remain in the tracked working agreement.

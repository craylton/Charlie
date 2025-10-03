# Charlie

A UCI-compliant chess engine written in C# targeting .NET 6. Charlie uses classic alpha–beta search with iterative deepening, quiescence, and simple heuristics for move ordering and time management. The codebase is small, readable, and intended as a practical reference/starting-point for chess-engine work in C#.

- Protocol: UCI (Universal Chess Interface)
- Language/runtime: C# on .NET 6
- Search: Alpha–beta with aspiration windows, principal-variation search, late move reductions, and tactical/extensions
- Evaluation: Material + piece-square tables, territory/space, hanging-piece penalties, and pawn-structure features
- Board: Bitboards with precomputed sliding attacks and Zobrist hashing

## Getting started

### Prerequisites
- .NET 6 SDK or newer

### Build
```bash
# From the repo root
dotnet build -c Release
```

### Run (UCI mode)
```bash
# Run the engine as a console app that speaks UCI over stdin/stdout
# Windows
dotnet run -c Release --project Charlie
```

In a UCI-compatible GUI (e.g., CuteChess, Arena), configure an engine pointing to the `Charlie` executable.

### Quick UCI session (manual)
```
uci
isready
position startpos moves e2e4 e7e5 g1f3
go depth 10
stop
quit
```

### Supported `go` modes
- `go depth <N>`: fixed-depth search
- `go wtime <ms> btime <ms> [winc <ms> binc <ms>]`: time-managed search using simple budgeting and an increment-aware time manager

### Supported `setoption`
- `setoption name Clear Hash`: clears the transposition table

### Extra console commands (engine-specific)
- `bench [depth <N>]`: run a bench across a suite of positions (see `Charlie.BenchTest.Bench`)
- `perft <depth>`: run a perft from the current position and print per-move counts and totals
- `eval fen <FEN...>`: evaluate a position once and print a centipawn/mate score

## Engine design overview

### Board and move generation
- Bitboards for all piece sets and occupancy
- Precomputed directional attack rays for rooks/bishops and king/knight move masks
- Castling legality includes square occupancy and threat checks
- En passant support for both sides; EP affects hashing (so threefold is position-accurate)

### Search
- Iterative deepening from depth 1 upward; root move ordering by simple promise heuristic
- Aspiration windows centered on the previous iteration’s score
- Principal-variation (PV) search with a narrow re-search on fail-high/low
- Reductions/extensions:
  - Late move reductions on quiet non-first moves
  - Extensions for promotions, in-check, and a small PV extension around shallow depths
- Quiescence search on captures, promotions, and en-passant; forward-promotion pushes included in qsearch from 7th rank
- Simple repetition detection via Zobrist history (`BoardState.IsThreeMoveRepetition`)

### Evaluation
- Material values: P=100, N=320, B=338, R=525, Q=920
- Piece-square tables for all pieces; phase-aware king tables (opening vs endgame)
- Territory/space: counts of attacked squares and uncontested territory
- Hanging-piece penalties and simple pawn-structure features (isolated, doubled, passed)
- Light lazy-evaluation cutoff for clearly won/lost positions

### Transposition table
- Zobrist hashing of full positions including side-to-move, castling, and en passant
- Table stores the best move and search depth for ordering; no bound types or scores recorded (kept intentionally simple)

### Time management
- Heuristic budgeting using available time and increment
- More time is allocated when the score is poor or when the PV best move changes; less time when we are confident in the best move

## Using with CuteChess (example)

`CharlieTest/Program.cs` shows how to run self-play tournaments via CuteChess on Windows. A typical CLI invocation looks like:
```bash
"C:\\Program Files (x86)\\cutechess\\cutechess-cli.exe" \
  -engine cmd="<path-to-Charlie-exe>" name="Charlie" \
  -engine cmd="<path-to-Charlie-exe>" name="Charlie-Alt" \
  -each tc=5+0.05 \
  -games 2 -rounds 50 -recover -concurrency 4
```
Adjust the paths and time controls to your environment.

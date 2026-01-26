# API Guide (`CrawfisSoftware.Collections.Path`)

This project contains the core path/loop abstractions and utilities for working with paths on a 2D grid.

Most of the types live in the namespace `CrawfisSoftware.Collections.Path`.

> Note: Several types depend on `CrawfisSoftware.Collections.Graph` (e.g., `Grid<,>`, `Direction`, and `DirectionExtensions`). Those live in a different package/namespace and are used here to interpret neighbor relationships on a grid.

---

## Key concepts

### What is a “path” in this library?

A path is represented as an ordered list of **positions** plus a few important attributes:

- `IsClosed`: whether the path is a **loop** (last position connects back to the first)
- `PositionCount`: the number of positions *including* the closing edge for loops
- `PathLength`: a path “length” value (type depends on implementation)

### Grid paths use a 1D cell index

`GridPath<,>` stores each cell on the path as an `int` grid index.

Common conversion (with `width` = number of columns):

- `column = index % width`
- `row = index / width`

---

## Files and types

### `IPath.cs` — `IPath<TPosition, TEdgeValue>`

`IPath<TPosition, TEdgeValue>` is the main abstraction.

- Inherits: `IReadOnlyList<TPosition>`
- Properties:
  - `bool IsClosed { get; }`
  - `int PositionCount { get; }`
  - `TEdgeValue PathLength { get; }`

Use this interface when you want to accept "any path-like thing" (not necessarily grid-based).

---

### `GridPath.cs` — `GridPath<TNodeValue, TEdgeValue>`

`GridPath<,>` is a concrete path implementation over a `Grid<TNodeValue, TEdgeValue>`.

- Implements: `IPath<int, float>`
- Stores:
  - `Grid<TNodeValue, TEdgeValue> Grid` (the underlying grid)
  - list of `int` positions (grid cell indices)
  - `float` path length (`PathLength`)

Constructor:

- `GridPath(Grid<TNodeValue,TEdgeValue> grid, IEnumerable<int> positions, float pathLength = -1, bool isClosed = false)`
  - If `pathLength < 0`, `PathLength` defaults to `Count`
  - If `isClosed == true`, the path is treated as a loop (the closing edge is implied)

Important properties:

- `Count`: number of stored positions
- `PositionCount`: `Count + 1` when `IsClosed == true` (accounts for the closing edge)

---

### `PathQuery.cs` — `PathQuery`

`PathQuery` is a static utility class for querying a `GridPath<,>`.

#### `DetermineCellDirectionAt` (turn classification)

- `char DetermineCellDirectionAt<N,E>(GridPath<N,E> path, int positionIndex, bool isLoop = false)`

Given a location along the path, it classifies what happens at that cell:

- Straight: `StringPathQuery.StraightChar` (default `'S'`)
- Left turn: `StringPathQuery.LeftChar` (default `'L'`)
- Right turn: `StringPathQuery.RightChar` (default `'R'`)
- Invalid/disconnected: `StringPathQuery.InvalidChar` (default `'X'`)

Remarks:

- Designed for standard rectangular grids.
- The file notes a TODO for toroidal grids.

#### `DetermineTurtleString`

- `string DetermineTurtleString<N,E>(GridPath<N,E> path)`

Generates a compact movement string (“turtle string”) describing the path.

- For open paths, the string omits the endpoints (dead-ends).
- For closed loops, the string runs across the whole loop.

This string is intended to be searched for patterns (see `StringPathQuery`).

---

### `StringPathQuery.cs` — `StringPathQuery`

`StringPathQuery` provides pattern-search and analysis functions over a turtle string.

#### Turtle alphabet

These are public and configurable static fields:

- `StraightChar` (default `'S'`)
- `LeftChar` (default `'L'`)
- `RightChar` (default `'R'`)
- `InvalidChar` (default `'X'`)

If you change these, do so consistently across producer/consumer code.

#### Search helpers

- `IEnumerable<int> SearchPathString(string pathString, Regex regex, bool isClosed = false)`
  - Returns the start indices of all matches.
  - If `isClosed == true`, the search string is extended by one character to allow wrap-around matches.

Built-in pattern queries:

- `IEnumerable<int> UTurns(string pathString, bool isClosed = false)`
  - Matches `"RR"` or `"LL"`

- `IEnumerable<int> StraightAways(string pathString, int straightLength, bool isClosed = false)`
  - Finds runs of consecutive `StraightChar` with length >= `straightLength`

#### Scalar metrics

- `float SpeedAgilityRatio(string pathString, int pathIndex, int halfWindowSize = 2, bool isClosed = false)`
  - Ratio of straights to turns in a window centered near `pathIndex`.
  - Crops windows at ends for open paths.
  - Returns `-1` when the resulting window is empty.

- `int MaximumConsecutiveStraights(string pathString, bool isClosed = false)`

- `int MaximumConsecutiveTurns(string pathString, bool isClosed = false)`

---

### `GridPathMetrics.cs` — `GridPathMetrics<N, E>`

`GridPathMetrics<,>` computes and stores commonly used metrics derived from a `GridPath<,>`.

Computed fields/properties:

- `GridPath<N,E> Path`
- `StartingCell` / `EndingCell` as `(Column, Row)` tuples
- `string TurtlePath`
- `int MaximumConsecutiveStraights`
- `int MaximumConsecutiveTurns`

Helper methods:

- `(int column, int row) GetGridColumnRowTuple(int pathIndex)`
- `int GetGridIndex(int pathIndex)`
- `int GetGridIndex(float pathDistance)`

Notes:

- `TurtlePath` is produced via `PathQuery.DetermineTurtleString`.
- `StartingCell`/`EndingCell` are derived using the grid width.

---

### `GridLoopMetrics.cs` — `GridLoopMetrics<N, E>`

`GridLoopMetrics<,>` extends `GridPathMetrics<,>` with loop-specific capabilities.

Primary feature: rotate a loop so a chosen cell becomes the effective “start”.

- Constructor:
  - `GridLoopMetrics(GridPath<N,E> gridPath, int startingCellPathIndex = 0)`

- `void SetLoopStartingPathIndex(int index)`
  - Rebuilds the underlying `Path` so `Path[0]` is the specified prior index, then regenerates `TurtlePath`.

This is useful when working with closed loops where the notion of a "start" is arbitrary but string-based analysis needs a consistent anchor.

---

## Typical workflow

1. Build a `GridPath<,>` from grid indices.
2. Compute its turtle-string representation with `PathQuery.DetermineTurtleString`.
3. Search or measure patterns using `StringPathQuery`.
4. Optionally compute common metrics via `GridPathMetrics<,>` or rotate loops via `GridLoopMetrics<,>`.

---

## Example usage

> These examples focus on API usage and omit the setup of `Grid<,>` since it is defined in `CrawfisSoftware.Collections.Graph`.

### Create a path and compute metrics

- Create a `GridPath<,>` with an ordered list of cell indices.
- Create `GridPathMetrics<,>` to compute the turtle path and max straight/turn runs.

### Search a turtle string for U-turns

- Use `StringPathQuery.UTurns(turtle, isClosed: path.IsClosed)` to get indices where `"RR"` or `"LL"` occur.

---

## Notes and limitations

- `PathQuery` currently documents that it does **not** support toroidal grids.
- `StringPathQuery` uses string/regex operations; for very large paths, consider the allocation costs when building or slicing strings.

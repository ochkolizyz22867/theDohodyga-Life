# theDohodyga Life

A small artificial-life simulator built with **C#**, **.NET 8**, and **Windows Forms**.

The application runs a 40 × 120 cellular world using the rules of **Conway's Game of Life**. Each living cell also carries a three-trait genome. When a new cell is born, it inherits the genome of the fittest living neighbor and receives a configurable random mutation.

> **Platform:** Windows  
> **Framework:** .NET 8 (`net8.0-windows`)  
> **UI:** Windows Forms  
> **External dependencies:** None

## Features

- A 40 × 120 simulation grid.
- Toroidal world boundaries: cells on one edge are neighbors of cells on the opposite edge.
- Conway-style survival and birth rules.
- Three inheritable genome traits:
  - **Reproduction Speed**
  - **Resistance**
  - **Immunity**
- Parent selection based on the sum of a neighbor's three gene values.
- Independent random mutation for each gene in a newly born cell.
- A color-coded view of each cell's average gene value.
- Run, pause, single-step, and reset controls.
- Adjustable simulation speed, mutation range, and initial population density.
- A live dashboard showing the generation, living-cell count, and average gene value.

## Getting started

### Requirements

- Windows 10 or Windows 11.
- .NET 8 SDK.
- Visual Studio 2022 or later with the **.NET desktop development** workload, or another .NET 8-compatible development environment.

### Run with Visual Studio

1. Download or clone this repository.
2. Open `theDohodygaLife.sln` in Visual Studio.
3. Make sure the .NET 8 SDK and **.NET desktop development** workload are installed.
4. Select the `Debug` configuration.
5. Press **F5** to build and run the application.

### Run from a terminal

Open PowerShell in the repository directory and run:

```powershell
dotnet restore .\theDohodygaLife.csproj
dotnet run --project .\theDohodygaLife.csproj
```

To create a Release build:

```powershell
dotnet build .\theDohodygaLife.csproj --configuration Release
```

The executable and other build output are created under `bin\`. Build output is intentionally excluded from version control.


## Publish to GitHub

1. Create a new **empty** repository on GitHub. Do not initialize it with a README, license, or `.gitignore`, because those files are already included here.
2. Extract this archive and open PowerShell in the extracted `theDohodyga-Life` folder.
3. Initialize the local repository and push the project (replace the URL with your repository URL):

```powershell
git init
git add .
git commit -m "Initial release of theDohodyga Life"
git branch -M main
git remote add origin https://github.com/YOUR-USERNAME/YOUR-REPOSITORY.git
git push -u origin main
```

The repository already includes an MIT `LICENSE`, a project README, and a `.gitignore`.

## How to use the simulator

### Main controls

| Control | Purpose |
| --- | --- |
| **Run / Pause** | Starts or pauses automatic generation updates. |
| **Step** | Advances the simulation by exactly one generation. |
| **New World** | Recreates the grid using the selected initial population percentage. |
| **Simulation Speed** | Sets the timer interval in milliseconds. A lower value runs generations more frequently. |
| **Genome Mutation** | Sets the maximum mutation magnitude applied independently to each gene of a newly born cell. |
| **Initial Population** | Sets the percentage chance that each grid position starts as a living cell when a new world is created. |

### Cellular rules

The simulation uses the standard Game of Life transition rules:

- A living cell survives when it has **two or three** living neighbors.
- A living cell dies when it has fewer than two or more than three living neighbors.
- An empty position becomes occupied when it has **exactly three** living neighbors.

The eight surrounding positions are counted as neighbors. The grid wraps at every edge, so the world has no hard boundary.

### Genome and inheritance

Every living cell stores three floating-point values:

1. **Reproduction Speed**
2. **Resistance**
3. **Immunity**

New cells inherit from the living neighbor with the highest **fitness**. In this project, fitness is defined as the sum of the three gene values:

```text
fitness = reproductionSpeed + resistance + immunity
```

Each inherited gene is mutated independently. For a mutation setting of `m` percent, the new value is calculated using a random multiplier in the range:

```text
1 - (m / 100)  ...  1 + (m / 100)
```

For example, a mutation setting of 10% allows each inherited gene to change by up to 10% in either direction.

A surviving cell keeps its current genome. Cells that die do not pass their genome on unless they were selected as a parent before the next generation was calculated.

**Important model limitation:** the three traits do not currently change the Game of Life survival or birth conditions. They influence which neighbor is selected as the parent and are displayed as an evolutionary layer on top of the cellular rules. The trait names are part of the simulation model; they are not yet connected to separate biological mechanics.

### Cell colors

A cell's color is based on the average of its three gene values:

| Average gene value | Display category |
| --- | --- |
| Below `0.8` | Low |
| `0.8` to below `1.2` | Standard |
| `1.2` to below `1.6` | Enhanced |
| `1.6` to below `2.0` | High |
| `2.0` or above | Rare |

The dashboard's average gene value is the mean of this per-cell average across all living cells. It displays `0.000` when the world has no living cells.

## Project structure

```text
theDohodyga-Life/
├── BoardView.cs             # Custom control that draws the simulation grid
├── MainForm.cs              # Main window, controls, dashboard, and timer
├── Program.cs               # Application entry point
├── Simulation.cs            # Grid, cells, genomes, inheritance, and evolution
├── theDohodygaLife.csproj    # .NET 8 Windows Forms project definition
├── theDohodygaLife.sln      # Visual Studio solution
├── .editorconfig             # Shared source-formatting defaults
├── .gitignore                # Excludes local and generated files
├── LICENSE                   # MIT License
└── README.md                 # Project documentation
```

### Source files

- **`Program.cs`** initializes Windows Forms and launches `MainForm`.
- **`MainForm.cs`** builds the interface, handles user input, advances generations through a Windows Forms timer, and refreshes the dashboard.
- **`BoardView.cs`** is a custom-drawn control. It reads the current grid and renders living cells using their genome-based colors.
- **`Simulation.cs`** contains the simulation model: genome data, cell state, generation updates, neighbor counting, parent selection, and statistics.

## Implementation notes

### Synchronous generation updates

Each generation is calculated into a separate next-generation array. The current and next arrays are swapped only after every grid position has been evaluated. This ensures that the result of a generation does not depend on the order in which cells are visited.

### Toroidal neighbor lookup

Neighbor coordinates use modulo arithmetic. A row or column that goes past one edge continues from the opposite edge. This is why the world wraps both horizontally and vertically.

### Randomness

A single `Random` instance is kept by the `Simulation` object and reused for world initialization and mutations. This avoids repeatedly creating random generators during mutation.

### Parent selection

For every new cell, the simulator examines its eight neighbors and chooses the living neighbor with the largest total gene value. Under the standard birth rule, a new cell has exactly three living neighbors, so a parent should be available. The initial genome is retained as a defensive fallback.

## Design and scope

The project intentionally uses only the .NET and Windows Forms APIs. There are no NuGet packages, game engines, databases, or external runtime dependencies.

The interface uses a dark green laboratory theme. The simulation model and the user interface are kept in separate classes so that the rules can be extended without having to rewrite the drawing code.

Possible future extensions include trait-specific mechanics, configurable grid dimensions, saving and loading worlds, and automated tests for the simulation rules. These are not currently implemented.

## License

This project is licensed under the **MIT License**. See [`LICENSE`](LICENSE) for the complete license text.

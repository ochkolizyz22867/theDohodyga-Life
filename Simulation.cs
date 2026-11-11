namespace theDohodygaLife;

/// <summary>Three inheritable traits stored in each cell's genome.</summary>
internal readonly struct Genome
{
    public double ReproductionSpeed { get; }
    public double Resistance { get; }
    public double Immunity { get; }

    public double Average => (ReproductionSpeed + Resistance + Immunity) / 3.0;
    public double Fitness => ReproductionSpeed + Resistance + Immunity;

    public Genome(double reproductionSpeed, double resistance, double immunity)
    {
        ReproductionSpeed = reproductionSpeed;
        Resistance = resistance;
        Immunity = immunity;
    }

    public Genome WithMutation(Random random, double mutationRange)
    {
        // Each gene receives an independent change in the range [-range, +range].
        double MutateValue(double value)
        {
            double change = (random.NextDouble() * 2.0 - 1.0) * mutationRange;
            return value * (1.0 + change);
        }

        return new Genome(
            MutateValue(ReproductionSpeed),
            MutateValue(Resistance),
            MutateValue(Immunity));
    }

    public static Genome Initial => new(1.0, 1.0, 1.0);
}

internal struct Cell
{
    public bool IsAlive;
    public Genome Genome;
}

internal readonly record struct SimulationStats(long Generation, int Alive, double AverageGeneValue);

/// <summary>
/// Cell simulation based on Conway's Game of Life rules.
/// A newly created cell inherits its genome from the fittest living neighbor.
/// </summary>
internal sealed class Simulation
{
    public const int Rows = 40;
    public const int Columns = 120;

    private Cell[,] _cells = new Cell[Rows, Columns];
    private Cell[,] _nextCells = new Cell[Rows, Columns];
    private readonly Random _random = new();

    public long Generation { get; private set; }
    public Cell[,] Cells => _cells;

    public void Reset(int populationPercent)
    {
        populationPercent = Math.Clamp(populationPercent, 0, 100);
        Generation = 0;
        _cells = new Cell[Rows, Columns];
        _nextCells = new Cell[Rows, Columns];

        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                bool alive = _random.Next(100) < populationPercent;
                _cells[row, column] = new Cell
                {
                    IsAlive = alive,
                    Genome = alive ? Genome.Initial : default
                };
            }
        }
    }

    public void Step(double mutationRange)
    {
        mutationRange = Math.Clamp(mutationRange, 0.0, 1.0);
        Array.Clear(_nextCells, 0, _nextCells.Length);

        // Important: every decision is based on the previous grid, not on cells already updated.
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Cell current = _cells[row, column];
                int neighbors = CountAliveNeighbors(row, column);

                bool willBeAlive = current.IsAlive
                    ? neighbors == 2 || neighbors == 3
                    : neighbors == 3;

                if (!willBeAlive)
                {
                    _nextCells[row, column] = default;
                    continue;
                }

                Genome genome;
                if (current.IsAlive)
                {
                    // A surviving cell keeps its own genome.
                    genome = current.Genome;
                }
                else
                {
                    // A newly created cell inherits genes from the fittest neighboring cell.
                    Genome parent = FindBestParent(row, column);
                    genome = parent.WithMutation(_random, mutationRange);
                }

                _nextCells[row, column] = new Cell
                {
                    IsAlive = true,
                    Genome = genome
                };
            }
        }

        (_cells, _nextCells) = (_nextCells, _cells);
        Generation++;
    }

    public SimulationStats GetStats()
    {
        int alive = 0;
        double totalAverage = 0.0;

        foreach (Cell cell in _cells)
        {
            if (!cell.IsAlive)
                continue;

            alive++;
            totalAverage += cell.Genome.Average;
        }

        return new SimulationStats(
            Generation,
            alive,
            alive == 0 ? 0.0 : totalAverage / alive);
    }

    private int CountAliveNeighbors(int row, int column)
    {
        int count = 0;

        for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                if (rowOffset == 0 && columnOffset == 0)
                    continue;

                int neighborRow = (row + rowOffset + Rows) % Rows;
                int neighborColumn = (column + columnOffset + Columns) % Columns;

                if (_cells[neighborRow, neighborColumn].IsAlive)
                    count++;
            }
        }

        return count;
    }

    private Genome FindBestParent(int row, int column)
    {
        Genome bestGenome = Genome.Initial;
        double bestFitness = double.NegativeInfinity;

        for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                if (rowOffset == 0 && columnOffset == 0)
                    continue;

                int neighborRow = (row + rowOffset + Rows) % Rows;
                int neighborColumn = (column + columnOffset + Columns) % Columns;
                Cell neighbor = _cells[neighborRow, neighborColumn];

                if (!neighbor.IsAlive)
                    continue;

                double fitness = neighbor.Genome.Fitness;
                if (fitness > bestFitness)
                {
                    bestFitness = fitness;
                    bestGenome = neighbor.Genome;
                }
            }
        }

        // A birth requires exactly three living neighbors, so a parent should always exist.
        // The initial genome is retained as a safe fallback.
        return bestGenome;
    }
}

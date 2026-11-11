using System.Drawing;
using System.Windows.Forms;

namespace theDohodygaLife;

internal sealed class BoardView : Control
{
    private const int CellWidth = 9;
    private const int CellHeight = 13;
    private const int FieldPadding = 14;

    private readonly Simulation _simulation;
    private readonly Font _cellFont = new("Consolas", 9.0f, FontStyle.Bold, GraphicsUnit.Point);
    private readonly Brush _emptyBrush = new SolidBrush(Color.FromArgb(35, 83, 55));

    public BoardView(Simulation simulation)
    {
        _simulation = simulation;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        BackColor = Color.FromArgb(5, 15, 10);
        Size = new Size(
            Simulation.Columns * CellWidth + FieldPadding * 2,
            Simulation.Rows * CellHeight + FieldPadding * 2);
        Margin = Padding.Empty;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);

        Cell[,] cells = _simulation.Cells;
        using var borderPen = new Pen(Color.FromArgb(30, 91, 58), 1.0f);

        for (int row = 0; row < Simulation.Rows; row++)
        {
            for (int column = 0; column < Simulation.Columns; column++)
            {
                int x = FieldPadding + column * CellWidth;
                int y = FieldPadding + row * CellHeight;
                Cell cell = cells[row, column];

                if (!cell.IsAlive)
                {
                    e.Graphics.FillRectangle(_emptyBrush, x + 4, y + 6, 1, 1);
                    continue;
                }

                Color color = GetGenomeColor(cell.Genome.Average);
                var bounds = new Rectangle(x, y, CellWidth, CellHeight);
                TextRenderer.DrawText(
                    e.Graphics,
                    "@",
                    _cellFont,
                    bounds,
                    color,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding);
            }
        }

        e.Graphics.DrawRectangle(
            borderPen,
            FieldPadding - 5,
            FieldPadding - 5,
            Simulation.Columns * CellWidth + 9,
            Simulation.Rows * CellHeight + 9);
    }

    private static Color GetGenomeColor(double average)
    {
        // These thresholds are preserved from the original C++ version.
        if (average < 0.8) return Color.FromArgb(255, 78, 78);
        if (average < 1.2) return Color.FromArgb(61, 238, 125);
        if (average < 1.6) return Color.FromArgb(75, 171, 255);
        if (average < 2.0) return Color.FromArgb(255, 211, 74);
        return Color.FromArgb(196, 125, 255);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cellFont.Dispose();
            _emptyBrush.Dispose();
        }

        base.Dispose(disposing);
    }
}

using System.Drawing;
using System.Windows.Forms;

namespace theDohodygaLife;

internal sealed class MainForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(7, 17, 12);
    private static readonly Color HeaderColor = Color.FromArgb(8, 25, 16);
    private static readonly Color PanelColor = Color.FromArgb(11, 31, 19);
    private static readonly Color PanelLightColor = Color.FromArgb(15, 43, 26);
    private static readonly Color BorderColor = Color.FromArgb(31, 91, 57);
    private static readonly Color GreenColor = Color.FromArgb(60, 235, 130);
    private static readonly Color TextColor = Color.FromArgb(211, 242, 220);
    private static readonly Color MutedColor = Color.FromArgb(112, 151, 124);
    private static readonly Color BrassColor = Color.FromArgb(190, 157, 91);

    private readonly Simulation _simulation = new();
    private readonly System.Windows.Forms.Timer _timer = new();

    private BoardView _boardView = null!;
    private Button _runButton = null!;
    private TrackBar _speedTrackBar = null!;
    private Label _speedValueLabel = null!;
    private NumericUpDown _mutationInput = null!;
    private NumericUpDown _densityInput = null!;
    private Label _generationValue = null!;
    private Label _aliveValue = null!;
    private Label _averageGeneValue = null!;
    private Label _stateLabel = null!;
    private bool _isRunning;

    public MainForm()
    {
        Text = "theDohodyga Life - Cell Evolution Simulator";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1480, 880);
        MinimumSize = new Size(1180, 720);
        BackColor = BackgroundColor;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 9.0f, FontStyle.Regular);
        AutoScaleMode = AutoScaleMode.Dpi;

        _simulation.Reset(20);
        BuildInterface();

        _timer.Interval = 100;
        _timer.Tick += (_, _) => AdvanceGeneration();
        FormClosing += (_, _) => _timer.Stop();

        UpdateDashboard();
    }

    private void BuildInterface()
    {
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColor,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(shell);

        shell.Controls.Add(BuildHeader(), 0, 0);

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColor,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(14, 12, 14, 14),
            Margin = Padding.Empty
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 286));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        shell.Controls.Add(body, 0, 1);

        body.Controls.Add(BuildSidebar(), 0, 0);
        body.Controls.Add(BuildWorkspace(), 1, 0);
    }

    private Control BuildHeader()
    {
        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = HeaderColor,
            Padding = new Padding(20, 8, 20, 8)
        };

        var brandMark = new Label
        {
            Text = "◉",
            Font = new Font("Segoe UI Symbol", 31, FontStyle.Bold),
            ForeColor = BrassColor,
            Location = new Point(21, 11),
            Size = new Size(58, 54),
            TextAlign = ContentAlignment.MiddleCenter
        };
        header.Controls.Add(brandMark);

        var title = new Label
        {
            Text = "THEDOHODYGA LIFE  //  CELL EVOLUTION LAB",
            Font = new Font("Consolas", 18, FontStyle.Bold),
            ForeColor = GreenColor,
            Location = new Point(86, 12),
            Size = new Size(520, 30),
            AutoEllipsis = true
        };
        header.Controls.Add(title);

        var subtitle = new Label
        {
            Text = "ARTIFICIAL LIFE SIMULATOR     •     GENETIC RESEARCH",
            Font = new Font("Consolas", 8.5f, FontStyle.Regular),
            ForeColor = MutedColor,
            Location = new Point(88, 45),
            Size = new Size(610, 20),
            AutoEllipsis = true
        };
        header.Controls.Add(subtitle);

        _stateLabel = new Label
        {
            Text = "●  SYSTEM PAUSED",
            Font = new Font("Consolas", 9, FontStyle.Bold),
            ForeColor = BrassColor,
            BackColor = PanelColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(190, 34),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(ClientSize.Width - 214, 24),
            BorderStyle = BorderStyle.FixedSingle
        };
        header.Controls.Add(_stateLabel);

        header.Resize += (_, _) =>
        {
            _stateLabel.Location = new Point(header.ClientSize.Width - _stateLabel.Width - 20, 24);
        };
        header.Paint += (_, e) =>
        {
            using var pen = new Pen(BorderColor, 1);
            e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
        };

        return header;
    }

    private Control BuildSidebar()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = PanelColor,
            Padding = new Padding(11)
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = PanelColor,
            Padding = new Padding(0, 0, 4, 0)
        };
        sidebar.Controls.Add(flow);

        Panel actions = CreateCard("OPERATIONS", 166, out Panel actionContent);
        var actionGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = PanelColor,
            Padding = new Padding(2)
        };
        actionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        actionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        actionGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        actionGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        _runButton = CreateButton("▶  RUN", true);
        _runButton.Click += (_, _) => ToggleRun();

        Button stepButton = CreateButton("▸  STEP", false);
        stepButton.Click += (_, _) => AdvanceGeneration();

        Button resetButton = CreateButton("✚  NEW WORLD", false);
        resetButton.Click += (_, _) => ResetWorld();

        actionGrid.Controls.Add(_runButton, 0, 0);
        actionGrid.Controls.Add(stepButton, 1, 0);
        actionGrid.Controls.Add(resetButton, 0, 1);
        actionGrid.SetColumnSpan(resetButton, 2);
        actionContent.Controls.Add(actionGrid);
        flow.Controls.Add(actions);

        Panel settings = CreateCard("SIMULATION SETTINGS", 276, out Panel settingsContent);
        BuildSettings(settingsContent);
        flow.Controls.Add(settings);

        Panel legend = CreateCard("GENOME COLOR KEY", 205, out Panel legendContent);
        BuildLegend(legendContent);
        flow.Controls.Add(legend);

        return sidebar;
    }

    private Control BuildWorkspace()
    {
        var workspace = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColor,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12, 0, 0, 0)
        };
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var stats = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = BackgroundColor,
            Padding = new Padding(0, 0, 0, 8)
        };

        stats.Controls.Add(CreateStatCard("GENERATION", out _generationValue, GreenColor));
        stats.Controls.Add(CreateStatCard("LIVE CELLS", out _aliveValue, GreenColor));
        stats.Controls.Add(CreateStatCard("AVG. GENE VALUE", out _averageGeneValue, BrassColor));
        workspace.Controls.Add(stats, 0, 0);

        var boardFrame = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = PanelColor,
            Padding = new Padding(12)
        };
        boardFrame.Paint += (_, e) =>
        {
            using var pen = new Pen(BorderColor, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, boardFrame.Width - 1, boardFrame.Height - 1);
        };

        var boardTitle = new Label
        {
            Text = "LIVE CELL POPULATION     /     120 × 40 GRID",
            Dock = DockStyle.Top,
            Height = 31,
            ForeColor = BrassColor,
            Font = new Font("Consolas", 9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(3, 0, 0, 0)
        };

        var boardSurface = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(5, 15, 10),
            Padding = new Padding(8)
        };
        var scrollHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.FromArgb(5, 15, 10)
        };
        _boardView = new BoardView(_simulation)
        {
            Location = new Point(8, 8)
        };
        scrollHost.Controls.Add(_boardView);
        boardSurface.Controls.Add(scrollHost);
        boardFrame.Controls.Add(boardSurface);
        boardFrame.Controls.Add(boardTitle);
        workspace.Controls.Add(boardFrame, 0, 1);

        var footer = new Label
        {
            Dock = DockStyle.Fill,
            Text = "WORLD WRAP: ON     •     NEIGHBORS: 8     •     INHERITANCE: ACTIVE",
            ForeColor = MutedColor,
            Font = new Font("Consolas", 8.5f, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(3, 0, 0, 0)
        };
        workspace.Controls.Add(footer, 0, 2);

        return workspace;
    }

    private void BuildSettings(Panel content)
    {
        var speedTitle = MakeLabel("SIMULATION SPEED", 9, true, TextColor);
        speedTitle.Location = new Point(5, 4);
        speedTitle.Size = new Size(150, 20);
        content.Controls.Add(speedTitle);

        _speedValueLabel = MakeLabel("100 ms", 9, true, GreenColor);
        _speedValueLabel.Location = new Point(167, 4);
        _speedValueLabel.Size = new Size(65, 20);
        _speedValueLabel.TextAlign = ContentAlignment.MiddleRight;
        content.Controls.Add(_speedValueLabel);

        _speedTrackBar = new TrackBar
        {
            Minimum = 20,
            Maximum = 500,
            Value = 100,
            TickStyle = TickStyle.None,
            SmallChange = 10,
            LargeChange = 25,
            Location = new Point(3, 26),
            Size = new Size(225, 42),
            BackColor = PanelColor
        };
        _speedTrackBar.ValueChanged += (_, _) =>
        {
            _speedValueLabel.Text = $"{_speedTrackBar.Value} ms";
            _timer.Interval = _speedTrackBar.Value;
        };
        content.Controls.Add(_speedTrackBar);

        var mutationTitle = MakeLabel("GENOME MUTATION (± %)", 9, true, TextColor);
        mutationTitle.Location = new Point(5, 83);
        mutationTitle.Size = new Size(170, 24);
        content.Controls.Add(mutationTitle);

        _mutationInput = CreateNumericInput(10, 0, 100);
        _mutationInput.Location = new Point(174, 81);
        content.Controls.Add(_mutationInput);

        var densityTitle = MakeLabel("INITIAL POPULATION (%)", 9, true, TextColor);
        densityTitle.Location = new Point(5, 132);
        densityTitle.Size = new Size(170, 24);
        content.Controls.Add(densityTitle);

        _densityInput = CreateNumericInput(20, 0, 100);
        _densityInput.Location = new Point(174, 130);
        content.Controls.Add(_densityInput);

        var hint = MakeLabel("Mutation is applied only\nto newly created cells.", 8, false, MutedColor);
        hint.Location = new Point(5, 177);
        hint.Size = new Size(220, 38);
        content.Controls.Add(hint);
    }

    private void BuildLegend(Panel content)
    {
        AddLegendRow(content, "@", Color.FromArgb(255, 78, 78), "LOW", "average < 0.8", 5);
        AddLegendRow(content, "@", Color.FromArgb(61, 238, 125), "STANDARD", "0.8 - 1.2", 37);
        AddLegendRow(content, "@", Color.FromArgb(75, 171, 255), "ENHANCED", "1.2 - 1.6", 69);
        AddLegendRow(content, "@", Color.FromArgb(255, 211, 74), "HIGH", "1.6 - 2.0", 101);
        AddLegendRow(content, "@", Color.FromArgb(196, 125, 255), "RARE", "average >= 2.0", 133);
    }

    private void AddLegendRow(Control parent, string symbol, Color color, string title, string description, int top)
    {
        var glyph = new Label
        {
            Text = symbol,
            Font = new Font("Consolas", 14, FontStyle.Bold),
            ForeColor = color,
            Location = new Point(5, top),
            Size = new Size(24, 25),
            TextAlign = ContentAlignment.MiddleCenter
        };
        var name = MakeLabel(title, 8.5f, true, TextColor);
        name.Location = new Point(35, top);
        name.Size = new Size(95, 15);
        var range = MakeLabel(description, 7.5f, false, MutedColor);
        range.Location = new Point(35, top + 14);
        range.Size = new Size(170, 14);
        parent.Controls.Add(glyph);
        parent.Controls.Add(name);
        parent.Controls.Add(range);
    }

    private Panel CreateCard(string title, int height, out Panel content)
    {
        var card = new Panel
        {
            Width = 250,
            Height = height,
            BackColor = PanelColor,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(8)
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(BorderColor, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var heading = MakeLabel(title, 8.5f, true, BrassColor);
        heading.Dock = DockStyle.Top;
        heading.Height = 25;
        heading.TextAlign = ContentAlignment.MiddleLeft;

        content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = PanelColor
        };

        card.Controls.Add(content);
        card.Controls.Add(heading);
        return card;
    }

    private Control CreateStatCard(string title, out Label valueLabel, Color valueColor)
    {
        var card = new Panel
        {
            Width = 178,
            Height = 66,
            BackColor = PanelColor,
            Margin = new Padding(0, 0, 12, 0),
            Padding = new Padding(10, 5, 10, 5)
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(BorderColor, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var titleLabel = MakeLabel(title, 7.5f, true, MutedColor);
        titleLabel.Location = new Point(10, 5);
        titleLabel.Size = new Size(155, 17);

        valueLabel = MakeLabel("0", 17, true, valueColor);
        valueLabel.Location = new Point(10, 24);
        valueLabel.Size = new Size(155, 30);

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);
        return card;
    }

    private static Button CreateButton(string text, bool accent)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(4),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = accent ? Color.FromArgb(17, 73, 42) : PanelLightColor,
            ForeColor = accent ? GreenColor : TextColor,
            Font = new Font("Consolas", 8.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = accent ? GreenColor : BorderColor;
        button.FlatAppearance.MouseOverBackColor = accent
            ? Color.FromArgb(23, 91, 52)
            : Color.FromArgb(20, 57, 35);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(25, 103, 58);
        return button;
    }

    private static NumericUpDown CreateNumericInput(int value, int minimum, int maximum)
    {
        return new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            Width = 58,
            Height = 28,
            TextAlign = HorizontalAlignment.Center,
            BackColor = PanelLightColor,
            ForeColor = GreenColor,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 9, FontStyle.Bold)
        };
    }

    private static Label MakeLabel(string text, float size, bool bold, Color color)
    {
        return new Label
        {
            Text = text,
            AutoEllipsis = true,
            ForeColor = color,
            BackColor = Color.Transparent,
            Font = new Font("Consolas", size, bold ? FontStyle.Bold : FontStyle.Regular)
        };
    }

    private void ToggleRun()
    {
        _isRunning = !_isRunning;
        if (_isRunning)
        {
            _timer.Interval = _speedTrackBar.Value;
            _timer.Start();
        }
        else
        {
            _timer.Stop();
        }

        UpdateDashboard();
    }

    private void AdvanceGeneration()
    {
        double mutationRange = (double)_mutationInput.Value / 100.0;
        _simulation.Step(mutationRange);
        UpdateDashboard();
    }

    private void ResetWorld()
    {
        _simulation.Reset((int)_densityInput.Value);
        UpdateDashboard();
    }

    private void UpdateDashboard()
    {
        SimulationStats stats = _simulation.GetStats();
        _generationValue.Text = stats.Generation.ToString("N0");
        _aliveValue.Text = $"{stats.Alive:N0} / {Simulation.Rows * Simulation.Columns:N0}";
        _averageGeneValue.Text = stats.AverageGeneValue.ToString("0.000");

        _stateLabel.Text = _isRunning ? "●  SYSTEM RUNNING" : "●  SYSTEM PAUSED";
        _stateLabel.ForeColor = _isRunning ? GreenColor : BrassColor;
        _runButton.Text = _isRunning ? "Ⅱ  PAUSE" : "▶  RUN";
        _runButton.ForeColor = _isRunning ? BrassColor : GreenColor;

        _boardView.Invalidate();
    }
}

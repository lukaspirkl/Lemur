using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace VentureUI.Terminal;

public class TerminalView : Control
{
    // --- Avalonia properties ---

    public static readonly StyledProperty<TerminalEmulator?> EmulatorProperty =
        AvaloniaProperty.Register<TerminalView, TerminalEmulator?>(nameof(Emulator));

    public static readonly StyledProperty<int> ScrollOffsetProperty =
        AvaloniaProperty.Register<TerminalView, int>(nameof(ScrollOffset), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly DirectProperty<TerminalView, int> MaxScrollOffsetProperty =
        AvaloniaProperty.RegisterDirect<TerminalView, int>(nameof(MaxScrollOffset), o => o.MaxScrollOffset);

    public TerminalEmulator? Emulator
    {
        get => GetValue(EmulatorProperty);
        set => SetValue(EmulatorProperty, value);
    }

    public int ScrollOffset
    {
        get => GetValue(ScrollOffsetProperty);
        set => SetValue(ScrollOffsetProperty, value);
    }

    private int _maxScrollOffset;
    public int MaxScrollOffset
    {
        get => _maxScrollOffset;
        private set => SetAndRaise(MaxScrollOffsetProperty, ref _maxScrollOffset, value);
    }

    // --- Font / cell metrics ---

    private const double FontSize = 14;
    private static readonly FontFamily MonospaceFamily =
        new("Cascadia Code,Cascadia Mono,Consolas,Menlo,Monospace");

    private double _cellWidth;
    private double _cellHeight;

    // --- Cursor blink ---

    private readonly DispatcherTimer _blinkTimer;
    private bool _cursorBlink = true; // true = cursor visible during blink cycle

    // --- ANSI color palette ---

    private static readonly Color[] AnsiPalette =
    [
        Color.FromRgb(12, 12, 12),    // Black
        Color.FromRgb(197, 15, 31),   // Red
        Color.FromRgb(19, 161, 14),   // Green
        Color.FromRgb(193, 156, 0),   // Yellow
        Color.FromRgb(0, 55, 218),    // Blue
        Color.FromRgb(136, 23, 152),  // Magenta
        Color.FromRgb(58, 150, 221),  // Cyan
        Color.FromRgb(204, 204, 204), // White
        Color.FromRgb(118, 118, 118), // BrightBlack
        Color.FromRgb(231, 72, 86),   // BrightRed
        Color.FromRgb(22, 198, 12),   // BrightGreen
        Color.FromRgb(249, 241, 165), // BrightYellow
        Color.FromRgb(59, 120, 255),  // BrightBlue
        Color.FromRgb(180, 0, 158),   // BrightMagenta
        Color.FromRgb(97, 214, 214),  // BrightCyan
        Color.FromRgb(242, 242, 242), // BrightWhite
    ];

    private static readonly Color DefaultFgColor = Color.FromRgb(242, 242, 242);
    private static readonly Color DefaultBgColor = Color.FromRgb(12, 12, 12);

    private static readonly IBrush DefaultBgBrush = new SolidColorBrush(DefaultBgColor);
    private static readonly IBrush DefaultFgBrush = new SolidColorBrush(DefaultFgColor);
    private static readonly IBrush CursorBrush = new SolidColorBrush(Colors.White);

    // --- Keyboard input ---

    public event Action<byte[]>? Input;

    private static readonly Dictionary<Key, byte[]> KeySequences = new()
    {
        [Key.Up]       = "\x1B[A"u8.ToArray(),
        [Key.Down]     = "\x1B[B"u8.ToArray(),
        [Key.Right]    = "\x1B[C"u8.ToArray(),
        [Key.Left]     = "\x1B[D"u8.ToArray(),
        [Key.Home]     = "\x1B[H"u8.ToArray(),
        [Key.End]      = "\x1B[F"u8.ToArray(),
        [Key.Delete]   = "\x1B[3~"u8.ToArray(),
        [Key.PageUp]   = "\x1B[5~"u8.ToArray(),
        [Key.PageDown] = "\x1B[6~"u8.ToArray(),
        [Key.F1]       = "\x1BOP"u8.ToArray(),
        [Key.F2]       = "\x1BOQ"u8.ToArray(),
        [Key.F3]       = "\x1BOR"u8.ToArray(),
        [Key.F4]       = "\x1BOS"u8.ToArray(),
        [Key.F5]       = "\x1B[15~"u8.ToArray(),
        [Key.F6]       = "\x1B[17~"u8.ToArray(),
        [Key.F7]       = "\x1B[18~"u8.ToArray(),
        [Key.F8]       = "\x1B[19~"u8.ToArray(),
        [Key.F9]       = "\x1B[20~"u8.ToArray(),
        [Key.F10]      = "\x1B[21~"u8.ToArray(),
        [Key.F11]      = "\x1B[23~"u8.ToArray(),
        [Key.F12]      = "\x1B[24~"u8.ToArray(),
        [Key.Tab]      = [(byte)'\t'],
        [Key.Escape]   = [0x1B],
        [Key.Return]   = [(byte)'\r'],
        [Key.Back]     = [0x7F],
    };

    public TerminalView()
    {
        MeasureCellSize();
        ClipToBounds = true;
        Focusable = true;

        _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(530) };
        _blinkTimer.Tick += (_, _) =>
        {
            _cursorBlink = !_cursorBlink;
            InvalidateVisual();
        };
        _blinkTimer.Start();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Ctrl+X → control character (e.g. Ctrl+C = 0x03)
        if (e.KeyModifiers == KeyModifiers.Control &&
            e.Key >= Key.A && e.Key <= Key.Z)
        {
            int code = e.Key - Key.A + 1;
            Input?.Invoke([(byte)code]);
            e.Handled = true;
            return;
        }

        if (KeySequences.TryGetValue(e.Key, out var seq))
        {
            Input?.Invoke(seq);
            e.Handled = true;
        }
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        if (!string.IsNullOrEmpty(e.Text))
        {
            Input?.Invoke(Encoding.UTF8.GetBytes(e.Text));
            e.Handled = true;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EmulatorProperty)
        {
            if (change.OldValue is TerminalEmulator old)
                old.Changed -= OnEmulatorChanged;

            if (change.NewValue is TerminalEmulator emulator)
            {
                emulator.Changed += OnEmulatorChanged;
                ResizeEmulator();
            }

            InvalidateVisual();
        }
        else if (change.Property == ScrollOffsetProperty)
        {
            if (Emulator != null)
                Emulator.ScrollOffset = ScrollOffset;
            InvalidateVisual();
        }
    }

    private void OnEmulatorChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (Emulator != null)
            {
                MaxScrollOffset = Emulator.MaxScrollOffset;
                // Keep ScrollOffset in bounds (scrollback may have grown)
                var clamped = Math.Clamp(ScrollOffset, 0, MaxScrollOffset);
                if (clamped != ScrollOffset) ScrollOffset = clamped;
            }
            InvalidateVisual();
        });
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ResizeEmulator();
    }

    private void ResizeEmulator()
    {
        if (Emulator == null || _cellWidth == 0 || _cellHeight == 0)
            return;

        int cols = Math.Max(1, (int)(Bounds.Width / _cellWidth));
        int rows = Math.Max(1, (int)(Bounds.Height / _cellHeight));
        Emulator.Resize(rows, cols);
    }

    private void MeasureCellSize()
    {
        var ft = new FormattedText(
            "M",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(MonospaceFamily),
            FontSize,
            DefaultFgBrush);

        _cellWidth = ft.Width;
        _cellHeight = ft.Height;
    }

    // --- Scroll input ---

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        int delta = e.Delta.Y > 0 ? 3 : -3;
        ScrollOffset = Math.Clamp(ScrollOffset - delta, 0, MaxScrollOffset);
    }

    // --- Rendering ---

    public override void Render(DrawingContext ctx)
    {
        var emulator = Emulator;

        // Fill background
        ctx.DrawRectangle(DefaultBgBrush, null, new Rect(Bounds.Size));

        if (emulator == null || _cellWidth == 0 || _cellHeight == 0)
            return;

        int rows = emulator.Rows;
        int cols = emulator.Cols;

        var runChars = new StringBuilder(cols);

        for (int row = 0; row < rows; row++)
        {
            double y = row * _cellHeight;
            int runStart = 0;
            var runFg = AnsiColor.Default;
            var runBg = AnsiColor.Default;
            var runAttrs = TextAttributes.None;
            runChars.Clear();

            for (int col = 0; col <= cols; col++)
            {
                // Sentinel empty cell at col==cols flushes the last run
                TerminalCell cell = col < cols ? emulator.GetCell(row, col) : TerminalCell.Empty;
                var (fg, bg) = ResolveColors(cell);

                bool styleBreak = fg != runFg || bg != runBg || cell.Attributes != runAttrs;

                if (col < cols && (runChars.Length == 0 || !styleBreak))
                {
                    if (runChars.Length == 0) { runFg = fg; runBg = bg; runAttrs = cell.Attributes; }
                    runChars.Append(cell.Char == '\0' ? ' ' : cell.Char);
                    continue;
                }

                // Flush accumulated run
                if (runChars.Length > 0)
                {
                    double x = runStart * _cellWidth;
                    double runWidth = runChars.Length * _cellWidth;

                    if (runBg != AnsiColor.Default)
                        ctx.DrawRectangle(BrushFor(runBg, background: true), null,
                            new Rect(x, y, runWidth, _cellHeight));

                    var typeface = (runAttrs & TextAttributes.Bold) != 0
                        ? new Typeface(MonospaceFamily, weight: FontWeight.Bold)
                        : new Typeface(MonospaceFamily);

                    var ft = new FormattedText(
                        runChars.ToString(),
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        FontSize,
                        BrushFor(runFg, background: false));

                    ctx.DrawText(ft, new Point(x, y));

                    if ((runAttrs & TextAttributes.Underline) != 0)
                    {
                        double uy = y + _cellHeight - 2;
                        ctx.DrawLine(new Pen(BrushFor(runFg, background: false), 1),
                            new Point(x, uy), new Point(x + runWidth, uy));
                    }
                }

                // Start new run at current cell
                if (col < cols)
                {
                    runStart = col;
                    runFg = fg;
                    runBg = bg;
                    runAttrs = cell.Attributes;
                    runChars.Clear();
                    runChars.Append(cell.Char == '\0' ? ' ' : cell.Char);
                }
            }
        }

        // Draw cursor
        if (emulator.CursorVisible && _cursorBlink && emulator.ScrollOffset == 0)
        {
            double cx = emulator.CursorCol * _cellWidth;
            double cy = emulator.CursorRow * _cellHeight;
            ctx.DrawRectangle(null, new Pen(CursorBrush, 1.5),
                new Rect(cx, cy, _cellWidth, _cellHeight));
        }
    }

    private static (AnsiColor fg, AnsiColor bg) ResolveColors(TerminalCell cell)
    {
        var fg = cell.Foreground;
        var bg = cell.Background;
        if ((cell.Attributes & TextAttributes.Inverse) != 0)
            (fg, bg) = (bg, fg);
        return (fg, bg);
    }

    private static IBrush BrushFor(AnsiColor color, bool background)
    {
        if (color == AnsiColor.Default)
            return background ? DefaultBgBrush : DefaultFgBrush;

        return new SolidColorBrush(AnsiPalette[(int)color - 1]);
    }
}

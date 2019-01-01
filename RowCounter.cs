using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScriptEditor
{
    public sealed class RowCounter : Canvas
    {
        public static readonly DependencyProperty UpdateCrutchProperty;

        public FontFamily FontFamily { get; set; }
        public Typeface Typeface { get; set; }

        public EditorScrollViewer ScrollViewer { get; set; }

        private int count;

        public int Count
        {
            get => count;
            set
            {
                count = value;
                Refresh();
            }
        }

        public double LetterHeight => GetFormattedText("A").Height;
        public double LetterWidth => GetFormattedText("A").WidthIncludingTrailingWhitespace;

        private const double whiteSpaceOnTheRight = 200;
        private const double whiteSpaceOnTheBottom = 200;
        private double MarginLeft => 0;

        private Editor editor;


        static RowCounter()
        {
            UpdateCrutchProperty = DependencyProperty.Register("UpdateCrutch", typeof(bool), typeof(RowCounter),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.AffectsRender
                )
            );
        }

        public RowCounter()
        {
            Focusable = true;
            FontFamily = new FontFamily("Courier New");
            Typeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            SnapsToDevicePixels = true;


            Margin = new Thickness(MarginLeft, 0, 0, 0);

            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
        }


        public void Initialize(Panel container, Editor editor)
        {
            ScrollViewer = new EditorScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalSingleStepOffset = LetterHeight,
                HorizontalSingleStepOffset = LetterWidth,
            };

            container.Children.Add(ScrollViewer);

            ScrollViewer.Content = this;

            this.editor = editor;
        }

        public void Refresh()
        {
            SetValue(UpdateCrutchProperty, !(bool)GetValue(UpdateCrutchProperty));
        }

        private FormattedText GetFormattedText(string text)
        {
            return new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface,
                14.0,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip
            );
        }


        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            // Prevent focus from fleeing.
            editor.ShouldKeepFocusOnce = true;
            editor.Focus();
        }



        protected override void OnRender(DrawingContext drawingContext)
        {
            // Fill background
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, Width, Height));


            //// Highlight text.
            //foreach (var highlight in Document.TextDecorations.OfType<HighlightBlock>())
            //{
            //    highlight.OnRender(this, drawingContext);
            //}
            string text = string.Join("\r\n", Enumerable.Range(1, Count));

            FormattedText ft2 = GetFormattedText(text);

            // Set new width and height
            Width = ft2.WidthIncludingTrailingWhitespace + whiteSpaceOnTheRight;
            Height = ft2.Height + whiteSpaceOnTheBottom;

            if (Height < MinHeight)
                Height = MinHeight;

            // Draw text
            drawingContext.DrawText(ft2, new Point(0, 0));

            // Draw border
            drawingContext.DrawRectangle(null, new Pen(Brushes.Blue, 1.0), new Rect(0, 0, Width, Height));

        }
    }
}

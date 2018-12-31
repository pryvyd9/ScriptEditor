using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScriptEditor
{


    public sealed class Editor : Canvas
    {
        public static readonly DependencyProperty UpdateCrutchProperty;


        public IDocument Document { get; private set; }

        public FontFamily FontFamily { get; set; }

        public Typeface Typeface { get; set; }


        public Panel CursorCanvas { get; set; }



        private Caret Caret { get; }

        private EditorScrollViewer ScrollViewer { get; set; }


        public double LetterHeight => GetFormattedText("A").Height;
        public double LetterWidth => GetFormattedText("A").WidthIncludingTrailingWhitespace;

        private double WidthBuffer => 200;
        private double HeightBuffer => 200;

        private bool _shouldKeepFocus = false;

        private bool ShouldKeepFocusOnce
        {
            get => _shouldKeepFocus ? !(_shouldKeepFocus = false) : false;
            set => _shouldKeepFocus = value;
        }

        public bool ShouldRememberLongestInRowPosition { get; set; } = true;

        private int desiredInRowPosition;




        static Editor()
        {
            UpdateCrutchProperty = DependencyProperty.Register("UpdateCrutch", typeof(bool), typeof(Editor),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.AffectsRender
                )
            );
        }




        public Editor()
        {
            Caret = new Caret
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };


            Focusable = true;
            FontFamily = new FontFamily("Courier New");
            Typeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            Caret.Width = 1;
            Caret.Height = LetterHeight;


            Margin = new Thickness(0);

            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;

            Children.Add(Caret);
        }


        public void Initialize(Panel container)
        {
            ScrollViewer = new EditorScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                VerticalSingleStepOffset = LetterHeight,
                HorizontalSingleStepOffset = LetterWidth,
                Padding = new Thickness(LetterWidth, LetterHeight, LetterWidth, LetterHeight),
            };

            container.Children.Add(ScrollViewer);

            ScrollViewer.Content = this;
        }

        public void Refresh()
        {
            var viewport = GetVisibleContent();

            SetValue(UpdateCrutchProperty, !(bool)GetValue(UpdateCrutchProperty));

            if (Caret.Position != null)
            {
                Caret.MoveTo(Document.GetPositionInText(Caret.Position, LetterHeight, LetterWidth));

                var newPos = Document.GetPositionInText(Caret.Position);

                var horizontalBuffer = LetterWidth / 2;

                var verticalBuffer = 0;

                if (newPos.inRowPosition <= viewport.firstColumn)
                {
                    ScrollViewer.ScrollToHorizontalOffset(newPos.inRowPosition * LetterWidth - horizontalBuffer);
                }
                else if (newPos.inRowPosition > viewport.lastColumn)
                {
                    ScrollViewer.ScrollToHorizontalOffset((newPos.inRowPosition - (viewport.lastColumn - viewport.firstColumn)) * LetterWidth + horizontalBuffer);
                }

                if (newPos.row <= viewport.firstLine)
                {
                    ScrollViewer.ScrollToVerticalOffset(newPos.row * LetterHeight - verticalBuffer);
                }
                else if (newPos.row >= viewport.lastLine)
                {
                    ScrollViewer.ScrollToVerticalOffset((newPos.row - (viewport.lastLine - viewport.firstLine) + 1) * LetterHeight + verticalBuffer);
                }
            }
        }

        public void SetDocument(Document document)
        {
            Document = DocumentChangeProxy.AsIDocument(document);
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

        private (Point point, int inStringPosition) GetAppropriateCaretPosition(Point point)
        {
            (int inStringPosition, int row, int inRowPosition) = Document.GetPositionInText(point, LetterHeight, LetterWidth);

            double x = inRowPosition * LetterWidth;
            double y = row * LetterHeight;

            return (new Point(x, y), inStringPosition);
        }

        private (int firstLine, int lastLine, int firstColumn, int lastColumn)
            GetVisibleContent()
        {
            var verticalOffset = ScrollViewer.VerticalOffset;
            var horizontalOffset = ScrollViewer.HorizontalOffset;

            var verticalSize = ScrollViewer.ViewportHeight;
            var horizontalSize = ScrollViewer.ViewportWidth;

            var lineCount = verticalSize / LetterHeight;
            var columnCount = horizontalSize / LetterWidth;

            int firstLine = (int)(verticalOffset / LetterHeight);
            int lastLine = firstLine + (int)lineCount;

            int firstColumn = (int)(horizontalOffset / LetterWidth);
            int lastColumn = firstColumn + (int)columnCount;

            return (firstLine, lastLine, firstColumn, lastColumn);
        }



        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Focus();

            // First focus is important to keep.
            ShouldKeepFocusOnce = true;

            var (point, inStringPosition) = GetAppropriateCaretPosition(Mouse.GetPosition(this));

            Caret.Position = Document.Content.NodeAt(inStringPosition);

            desiredInRowPosition = Document.GetPositionInText(inStringPosition).inRowPosition;

            Refresh();

            //var position = Document.GetPositionInText(Mouse.GetPosition(this), LetterHeight, LetterWidth);

            //var word = Document.GetWordOf(Document.Content.NodeAt(position.inStringPosition));

            //HighlightBlock highlightBlock = new HighlightBlock
            //{
            //    Brush = Brushes.Red,
            //    Start = word.start,
            //    End = word.end,
            //};

            //Document.TextDecorations.Add(highlightBlock);
        }


        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Caret.Visibility = Visibility.Visible;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (ShouldKeepFocusOnce)
            {
                Focus();

                return;
            }

            Caret.Visibility = Visibility.Hidden;
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            // Strangely enough.

            // The focus that came with Focus() goes out
            // immediatly so it has to be kept first time.
            // But the second and others are fine to lose.
            if (ShouldKeepFocusOnce)
            {
                Focus();
            }

        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {

            bool upperMode = false;
            bool isShifted = false;
            bool isControled = false;

            if (Keyboard.IsKeyToggled(Key.CapsLock))
                upperMode = true;
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                isShifted = true;
            if (Keyboard.Modifiers == ModifierKeys.Control)
                isControled = true;

            if (!isControled)
            {
                ProcessSimpleKeys(e.Key, upperMode, isShifted);
            }

            ProcessSpecialKeys(e.Key, upperMode, isShifted, isControled);

            return;

        }

        private void PutChar(char ch)
        {
            Document.Insert(Caret.Position, ch);

            Refresh();
        }

        private void ProcessSimpleKeys(Key key, bool upperMode, bool isShifted)
        {
            char ch;

            var shiftedCharacters = new Dictionary<char, char>
            {
                ['1'] = '!',
                ['2'] = '@',
                ['3'] = '#',
                ['4'] = '$',
                ['5'] = '%',
                ['6'] = '^',
                ['7'] = '&',
                ['8'] = '*',
                ['9'] = '(',
                ['0'] = ')',
            };


            if (key >= Key.A && key <= Key.Z)
            {
                ch = char.Parse(key.ToString());

                if (!upperMode && !isShifted)
                {
                    ch = char.ToLower(ch);
                }

                putChar();
                return;
            }

            if (key >= Key.D0 && key <= Key.D9)
            {
                ch = key.ToString().Substring(1)[0];


                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    ch = shiftedCharacters[ch];
                }

                putChar();
                return;
            }

            void putChar() => PutChar(ch);

        }

        private void ProcessSpecialKeys(Key key, bool upperMode, bool isShifted, bool isControled)
        {

            char ch;

            switch (key)
            {
                case Key.Z:
                    if (isControled)
                    {
                        Document.RollbackChanges();

                        Refresh();
                    }
                    return;
                case Key.Space:
                    ch = ' ';
                    break;
                case Key.OemMinus:
                    ch = isShifted ? '_' : '-';
                    break;
                case Key.OemPlus:
                    ch = isShifted ? '+' : '=';
                    break;
                case Key.Oem4:
                    ch = isShifted ? '{' : '[';
                    break;
                case Key.Oem6:
                    ch = isShifted ? '}' : ']';
                    break;
                case Key.Oem1:
                    ch = isShifted ? ':' : ';';
                    break;
                case Key.Oem7:
                    ch = isShifted ? '"' : '\'';
                    break;
                case Key.OemComma:
                    ch = isShifted ? '<' : ',';
                    break;
                case Key.OemPeriod:
                    ch = isShifted ? '>' : '.';
                    break;
                case Key.Oem2:
                    ch = isShifted ? '?' : '/';
                    break;
                case Key.Oem5:
                    ch = isShifted ? '|' : '\\';
                    break;
                case Key.Back:
                    {
                        if (Caret.Position.Previous == null)
                            return;

                        IEnumerable<LinkedListNode<char>> nodesToDelete;

                        if (Document.LineEnding.Contains(Caret.Position.Previous.Value))
                        {
                            nodesToDelete = Caret.Position.Previous.GetRangeNodes(-Document.LineEnding.Length);
                        }
                        else
                        {
                            nodesToDelete = new[] { Caret.Position.Previous };
                        }

                        Document.Delete(nodesToDelete);

                        Refresh();

                        return;
                    }
                case Key.Delete:
                    {
                        if (Caret.Position.Next == null)
                            return;

                        var posToDelete = Document.GetPositionInText(Caret.Position).inStringPosition;

                        IEnumerable<LinkedListNode<char>> nodesToDelete;

                        if (Document.LineEnding.Contains(Caret.Position.Next.Value))
                        {
                            nodesToDelete = Caret.Position.GetRangeNodes(Document.LineEnding.Length);

                        }
                        else
                        {
                            nodesToDelete = new[] { Caret.Position };
                        }

                        Document.Delete(nodesToDelete);

                        Caret.Position = Document.Content.NodeAt(posToDelete);

                        Refresh();

                        return;
                    }
                case Key.Left:
                    {
                        // If start of file then do nothing
                        if (Caret.Position.Previous == null)
                        {
                            Refresh();

                            ShouldKeepFocusOnce = true;

                            return;
                        }

                        if (Document.LineEnding.Contains(Caret.Position.Previous.Value))
                        {
                            Caret.Position = Caret.Position.GetAtOffset(-Document.LineEnding.Length);
                        }
                        else
                        {
                            Caret.Position = Caret.Position.Previous;
                        }

                        var newPos = Document.GetPositionInText(Caret.Position);

                        desiredInRowPosition = newPos.inRowPosition;

                        Refresh();

                        ShouldKeepFocusOnce = true;

                        return;
                    }
                case Key.Right:
                    {
                        // If end of file then do nothing
                        if (Caret.Position.Next.Next == null)
                        {
                            Refresh();

                            ShouldKeepFocusOnce = true;

                            return;
                        }

                        if (Document.LineEnding.Contains(Caret.Position.Value))
                        {
                            Caret.Position = Caret.Position.GetAtOffset(Document.LineEnding.Length);
                        }
                        else
                        {
                            Caret.Position = Caret.Position.Next;
                        }

                        var newPos = Document.GetPositionInText(Caret.Position);

                        desiredInRowPosition = newPos.inRowPosition;

                        Refresh();

                        ShouldKeepFocusOnce = true;

                        return;
                    }
                case Key.Up:
                    {
                        var pos = Document.GetPositionInText(Caret.Position);

                        ShouldKeepFocusOnce = true;

                        if (pos.row == 0)
                            return;

                        if (!ShouldRememberLongestInRowPosition ||
                            desiredInRowPosition < pos.inRowPosition)
                        {
                            desiredInRowPosition = pos.inRowPosition;
                        }

                        var newPos = Document.GetPositionInText(pos.row - 1, desiredInRowPosition);

                        var posToMove = newPos.inStringPosition;

                        if (posToMove < 0)
                            return;

                        Caret.Position = Document.Content.NodeAt(posToMove);

                        Refresh();

                        return;
                    }
                case Key.Down:
                    {
                        var pos = Document.GetPositionInText(Caret.Position);

                        ShouldKeepFocusOnce = true;

                        if (pos.row == Document.Lines.Count - 1)
                            return;

                        if (!ShouldRememberLongestInRowPosition ||
                            desiredInRowPosition < pos.inRowPosition)
                        {
                            desiredInRowPosition = pos.inRowPosition;
                        }

                        var newPos = Document.GetPositionInText(pos.row + 1, desiredInRowPosition);

                        var posToMove = newPos.inStringPosition;

                        if (posToMove >= Document.Content.Count)
                            return;

                        Caret.Position = Document.Content.NodeAt(posToMove);

                        Refresh();

                        return;
                    }
                case Key.Home:
                    {
                        var pos = Document.GetPositionInText(Caret.Position);

                        desiredInRowPosition = 0;

                        var newPos = Document.GetPositionInText(pos.row, desiredInRowPosition);

                        Caret.Position = Document.Content.NodeAt(newPos.inStringPosition);

                        Refresh();

                        return;
                    }
                case Key.End:
                    {
                        var pos = Document.GetPositionInText(Caret.Position);

                        var newPos = Document.GetPositionInText(pos.row, int.MaxValue);

                        desiredInRowPosition = newPos.inRowPosition;

                        Caret.Position = Document.Content.NodeAt(newPos.inStringPosition);

                        Refresh();

                        return;
                    }
                case Key.Enter:
                    {
                        var pos = Document.GetPositionInText(Caret.Position);

                        Document.BreakLine(Document.Lines[pos.row], Caret.Position);



                        Refresh();

                        return;
                    }
                default:
                    return;
            }

            putChar();

            return;

            void putChar() => PutChar(ch);
        }



        protected override void OnRender(DrawingContext drawingContext)
        {
            // Fill background
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, Width, Height));


            // Highlight text.
            foreach (var highlight in Document.TextDecorations.OfType<HighlightBlock>())
            {
                highlight.OnRender(this, drawingContext);
            }

            FormattedText ft2 = GetFormattedText(Document.Text);

            // Set new width and height
            Width = ft2.WidthIncludingTrailingWhitespace + WidthBuffer;
            Height = ft2.Height + HeightBuffer;

           
            // Draw text
            drawingContext.DrawText(ft2, new Point(0, 0));

            // Draw border
            drawingContext.DrawRectangle(null, new Pen(Brushes.Blue, 1.0), new Rect(0, 0, Width, Height));

        }
    }
}

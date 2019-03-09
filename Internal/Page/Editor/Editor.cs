using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

using static ScriptEditor.Tag;

namespace ScriptEditor
{

    public sealed class Editor : Canvas
    {
        public static readonly DependencyProperty UpdateCrutchProperty;

        public IDocument Document { get; private set; }
        public FontFamily FontFamily { get; set; }
        public Typeface Typeface { get; set; }

        private Caret Caret { get; }
        public EditorScrollViewer ScrollViewer { get; set; }


        public double LetterHeight => GetFormattedText("A").Height;
        public double LetterWidth => GetFormattedText("A").WidthIncludingTrailingWhitespace;

        private const double whiteSpaceOnTheRight = 200;
        private const double whiteSpaceOnTheBottom = 200;
        private double MarginLeft => 0;

        private bool _shouldKeepFocus = false;

        public bool ShouldKeepFocusOnce
        {
            get => _shouldKeepFocus ? !(_shouldKeepFocus = false) : false;
            set => _shouldKeepFocus = value;
        }

        public bool ShouldRememberLongestInRowPosition { get; set; } = true;

        private int desiredInRowPosition;



        public Brush SelectionBrush { get; set; }
        public int[] SelectionTags { get; set; } = new[] { Selection };



        private bool isMouseDown;
        private bool isMouseHeld;
        private bool isLineSelected;


        private int oldCaretInStringPosition;
        public (int start, int end) selectionPosition;
        public (int left, int right) SelectionRange =>
            selectionPosition.start < selectionPosition.end ?
                    (selectionPosition.start, selectionPosition.end - 1) :
                    (selectionPosition.end, selectionPosition.start - 1);

        private bool isSelected;







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
                VerticalAlignment = VerticalAlignment.Top,
                Visibility = Visibility.Hidden,
            };

            Focusable = true;
            FontFamily = new FontFamily("Courier New");
            Typeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            Caret.Width = 1;
            Caret.Height = LetterHeight;
            SnapsToDevicePixels = true;


            Margin = new Thickness(MarginLeft, 0, 0, 0);

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
                //Padding = new Thickness(LetterWidth, LetterHeight, LetterWidth, LetterHeight),
            };

            container.Children.Add(ScrollViewer);

            ScrollViewer.Content = this;



            var color = Brushes.SkyBlue.Color;
            color.A = 150;

            SelectionBrush = new SolidColorBrush(color);
        }

        public void Refresh()
        {
            var viewport = GetVisibleContent();

            SetValue(UpdateCrutchProperty, !(bool)GetValue(UpdateCrutchProperty));

            MoveCaret(Caret, viewport);
        }

        public void SetDocument(IDocument document)
        {
            Document = DocumentChangeProxy.AsIDocument(document);

            Document.Content.CollectionChanged += FixCaretPositionOnDocumentChanged;

            Caret.Position = Document.Content.NodeAt(0);

            document.FormatUpdated += d => Refresh();
        }

        private void FixCaretPositionOnDocumentChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Contains(Caret.Position))
                {
                    Caret.Position = Caret.Position.Next;
                }
            }
            //else if(e.Action == NotifyCollectionChangedAction.Add)
            //{
            //    //if((e.NewItems[0] as LinkedListNode<char>).Value == Document.LineEnding[0] &&
            //    //    (e.NewItems[e.NewItems.Count - 1] as LinkedListNode<char>).Value == Document.LineEnding.Last())
            //    //{
            //    //    return;
            //    //}

            //    if (e.NewItems[1] == Caret.Position && Document.IsRevertingChanges)
            //        //Document.InvisibleCharacters.Contains(Caret.Position.Value) && e.NewItems[1] == Caret.Position)
            //    {
            //        Caret.Position = Caret.Position.Previous;

            //        //Refresh();
            //    }
            //}
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

        private (Point point, int inStringPosition) GetAppropriateCaretPosition(Point screenPoint)
        {
            (int inStringPosition, int row, int inRowPosition) = Document.GetPositionInText(screenPoint, LetterHeight, LetterWidth);

            double x = inRowPosition * LetterWidth;
            double y = row * LetterHeight;

            return (new Point(x, y), inStringPosition);
        }

        private int GetCaretInStringPosition()
        {
            return Document.Content.IndexOf(Caret.Position);
        }

        private void UpdateCaretInStringPosition()
        {
            oldCaretInStringPosition = GetCaretInStringPosition();
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


        private void MoveCaret(Caret caret, (int firstLine, int lastLine, int firstColumn, int lastColumn) viewport)
        {
            if (caret.Position != null)
            {
                caret.MoveTo(Document.GetPositionInText(caret.Position, LetterHeight, LetterWidth));

                var newPos = Document.GetPositionInText(caret.Position);

                var horizontalBuffer = LetterWidth / 2;

                var verticalBuffer = 0;

                if (newPos.inRowPosition == 0)
                {
                    ScrollViewer.ScrollToHorizontalOffset(0);
                }
                else if (newPos.inRowPosition <= viewport.firstColumn)
                {
                    ScrollViewer.ScrollToHorizontalOffset(newPos.inRowPosition * LetterWidth - horizontalBuffer + Margin.Left);
                }
                else if (newPos.inRowPosition > viewport.lastColumn - Margin.Left / LetterWidth)
                {
                    ScrollViewer.ScrollToHorizontalOffset((newPos.inRowPosition - (viewport.lastColumn - viewport.firstColumn)) * LetterWidth + horizontalBuffer + Margin.Left);
                }

                if (newPos.row == 0)
                {
                    ScrollViewer.ScrollToVerticalOffset(0);
                }
                else if (newPos.row <= viewport.firstLine)
                {
                    ScrollViewer.ScrollToVerticalOffset(newPos.row * LetterHeight - verticalBuffer);
                }
                else if (newPos.row >= viewport.lastLine)
                {
                    ScrollViewer.ScrollToVerticalOffset((newPos.row - (viewport.lastLine - viewport.firstLine) + 1) * LetterHeight + verticalBuffer);
                }
            }
        }

        private void MoveCaret(int inStringPosition)
        {
            Caret.Position = Document.Content.NodeAt(inStringPosition);

            desiredInRowPosition = Document.GetPositionInText(inStringPosition).inRowPosition;
        }

        private void MoveCaretToMousePosition()
        {
            var (_, inStringPosition) = GetAppropriateCaretPosition(Mouse.GetPosition(this));

            Caret.Position = Document.Content.NodeAt(inStringPosition);

            desiredInRowPosition = Document.GetPositionInText(inStringPosition).inRowPosition;
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



        #region Input

        #region Selection

        private void ClearSelection()
        {
            isSelected = false;
            //isLineSelected = false;
        }

        private void ClearSelectionHighlighting()
        {
            Document.TextLookBlocks.RemoveAll(n => n.Tags.Contains(Selection));
        }

        private void SelectAndHighlightText(int oldPosition, int newPosition)
        {
            if (isSelected)
            {
                selectionPosition.end = newPosition;
            }
            else
            {
                selectionPosition = (oldPosition, newPosition);
            }

            isSelected = true;
            Document.ApplyHighlight(new[] { SelectionRange }, SelectionTags, SelectionBrush);
        }

        private void SelectLine(int rowIndex)
        {
            var line = Document.Lines[rowIndex];

            var start = Document.Content.IndexOf(line.Start);
            var end = Document.Content.IndexOf(line.End) + 1;

            SelectText(start, end);

            isLineSelected = true;
        }

        private void SelectCurrentLine()
        {
            var position = Document.GetPositionInText(Caret.Position);

            SelectLine(position.row);
        }

        private void SelectText(int oldPosition, int newPosition)
        {

            if (isSelected)
            {
                selectionPosition.end = newPosition;
            }
            else
            {
                selectionPosition = (oldPosition, newPosition);
            }

            isSelected = true;
            isLineSelected = false;
        }

        private string GetSelectedText()
        {
            var start = Document.Content.NodeAt(SelectionRange.left);
            var end = start.GetAtOffset(SelectionRange.right - SelectionRange.left);
            var range = start.GetRange(end).ToStr();
            return range;
        }

        private void DeleteSelected()
        {
            Document.Delete(SelectionRange.left, SelectionRange.right + 1);

            ClearSelectionHighlighting();
            ClearSelection();

            oldCaretInStringPosition = GetCaretInStringPosition();
        }

        #endregion


        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isMouseDown)
            {
                isMouseHeld = true;
            }

            if (isMouseHeld)
            {
                ClearSelectionHighlighting();

                MoveCaretToMousePosition();

                SelectAndHighlightText(oldCaretInStringPosition, GetCaretInStringPosition());

                Refresh();
            }

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            isMouseDown = true;

            ClearSelectionHighlighting();

            Focus();

            Caret.SetColor(false);

            // First focus is important to keep.
            ShouldKeepFocusOnce = true;

            var (point, inStringPosition) = GetAppropriateCaretPosition(Mouse.GetPosition(this));

            MoveCaret(inStringPosition);

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                SelectAndHighlightText(oldCaretInStringPosition, GetCaretInStringPosition());
            }
            else
            {
                ClearSelection();
            }

            UpdateCaretInStringPosition();

            Refresh();

        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            isMouseDown = false;
            isMouseHeld = false;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            bool upperMode = false;
            bool isShifted = false;
            bool isControled = false;

            Caret.SetColor(false);

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

            ProcessSpecialKeys(e.Key, upperMode, isShifted, isControled, e);

            return;
        }






        private void PutChar(char ch)
        {
            if (isSelected)
            {
                var range = Document.GetRange(SelectionRange.left, SelectionRange.right + 1);

                Document.Replace(range, ch);
            }
            else
            {
                Document.Insert(Caret.Position, ch);
            }

            Refresh();
        }

        private void PutString(string str)
        {
            if (isSelected)
            {
                var range = Document.GetRange(SelectionRange.left, SelectionRange.right + 1);

                Document.Replace(range, str);

                isSelected = false;
            }
            else if (isLineSelected)
            {
                var startPosition = Document.GetPositionInText(SelectionRange.left);

                var line = Document.Lines[startPosition.row];

                Document.InsertLineAfter(line, str);
            }
            else
            {
                Document.Insert(Caret.Position, str);
            }

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

        private void ProcessSpecialKeys(
            Key key,
            bool upperMode,
            bool isShifted,
            bool isControled,
            KeyEventArgs e)
        {

            char ch;

            switch (key)
            {
                case Key.Z:
                    if (isControled)
                    {
                        Document.RollbackChanges();

                        UpdateCaretInStringPosition();

                        Refresh();

                        
                    }
                    return;
                case Key.C:
                    if (isControled)
                    {
                        if (isSelected)
                        {
                            Clipboard.SetText(GetSelectedText());
                        }
                        else
                        {
                            SelectCurrentLine();
                            Clipboard.SetText(GetSelectedText());
                            ClearSelection();
                        }
                    }
                    return;
                case Key.V:
                    if (isControled)
                    {
                        PutString(Clipboard.GetText());
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

                        if (isSelected)
                        {
                            DeleteSelected();
                        }
                        else
                        {
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
                        }

                        UpdateCaretInStringPosition();

                        Refresh();

                        return;
                    }
                case Key.Delete:
                    {
                        if (Caret.Position.Next == null)
                            return;

                        if (isSelected)
                        {
                            DeleteSelected();
                        }
                        else
                        {
                            var posToDelete = Document.GetPositionInText(Caret.Position).inStringPosition;

                            IEnumerable<LinkedListNode<char>> nodesToDelete;

                            if (Document.LineEnding.Contains(Caret.Position.Value))
                            {
                                nodesToDelete = Caret.Position.GetRangeNodes(Document.LineEnding.Length);
                            }
                            else
                            {
                                nodesToDelete = new[] { Caret.Position };
                            }

                            Document.Delete(nodesToDelete);

                            Caret.Position = Document.Content.NodeAt(posToDelete);
                        }

                        UpdateCaretInStringPosition();


                        Refresh();

                        return;
                    }
                case Key.Left:
                    {
                        ClearSelectionHighlighting();
                        ClearSelection();

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
                        ClearSelectionHighlighting();
                        ClearSelection();

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
                        ClearSelectionHighlighting();
                        ClearSelection();

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
                        ClearSelectionHighlighting();
                        ClearSelection();

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

                        e.Handled = true;

                        return;
                    }
                case Key.End:
                    {
                        var pos = Document.GetPositionInText(Caret.Position);

                        var newPos = Document.GetPositionInText(pos.row, int.MaxValue);

                        desiredInRowPosition = newPos.inRowPosition;

                        Caret.Position = Document.Content.NodeAt(newPos.inStringPosition);

                        Refresh();

                        e.Handled = true;

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

        #endregion



        #region Render


        private void RenderHighlight(HighlightBlock[] blocks, DrawingContext drawingContext)
        {
            foreach (var block in blocks)
            {
                var posStart = Document.GetPositionInText(block.Start);
                var posEnd = Document.GetPositionInText(block.End);

                // For some reason end is located one position behind
                posEnd.inRowPosition++;
                posEnd.inStringPosition++;

                if (posEnd.row == posStart.row)
                {
                    var count = posEnd.inRowPosition - posStart.inRowPosition;
                    block.RenderRange(this, drawingContext, posStart.row, posStart.inRowPosition, count);
                }
                else
                {
                    var rowCount = posEnd.row - posStart.row;

                    int row = posStart.row;

                    // First line
                    {
                        var line = Document.Lines[row];

                        var lineEndPos = Document.GetPositionInText(line.End);

                        var count = lineEndPos.inRowPosition - posStart.inRowPosition;

                        block.RenderRange(this, drawingContext, posStart.row, posStart.inRowPosition, count);

                        row++;
                    }

                    // Middle lines
                    for ( ; row < posEnd.row; )
                    {
                        var line = Document.Lines[row];

                        var lineStart = Document.GetPositionInText(line.Start);

                        var count = line.Text.Length;

                        block.RenderRange(this, drawingContext, lineStart.row, lineStart.inRowPosition, count);

                        row++;
                    }

                    // Last line
                    {
                        var line = Document.Lines[row];

                        var lineStartPos = Document.GetPositionInText(line.Start);

                        var count = posEnd.inRowPosition - lineStartPos.inRowPosition;

                        block.RenderRange(this, drawingContext, lineStartPos.row, lineStartPos.inRowPosition, count);
                    }
                }
            }
        }

        private void RenderTextColor(TextColorBlock[] blocks, DrawingContext drawingContext)
        {
            foreach (var block in blocks)
            {
                var posStart = Document.GetPositionInText(block.Start);
                var posEnd = Document.GetPositionInText(block.End);

                // For some reason end is located one position behind
                //posEnd.inRowPosition++;
                //posEnd.inStringPosition++;

                if (posEnd.row == posStart.row)
                {
                    block.RenderRange(this, drawingContext, block.Text, posStart.row, posStart.inRowPosition);
                }
                else
                {
                    var rowCount = posEnd.row - posStart.row;

                    int row = posStart.row;

                    // First line
                    {
                        var line = Document.Lines[row];

                        var lineEndPos = Document.GetPositionInText(line.End);

                        block.RenderRange(this, drawingContext, block.Start.GetRange(line.End).ToStr(), posStart.row, posStart.inRowPosition);

                        row++;
                    }

                    // Middle lines
                    for (; row < posEnd.row;)
                    {
                        var line = Document.Lines[row];

                        var lineStart = Document.GetPositionInText(line.Start);

                        block.RenderRange(this, drawingContext, line.Text, lineStart.row, lineStart.inRowPosition);

                        row++;
                    }

                    // Last line
                    {
                        var line = Document.Lines[row];

                        var lineStartPos = Document.GetPositionInText(line.Start);

                        var count = posEnd.inRowPosition - lineStartPos.inRowPosition;

                        block.RenderRange(this, drawingContext, line.Start.GetRange(block.End).ToStr(), lineStartPos.row, lineStartPos.inRowPosition);
                    }
                }
            }
        }


        private void RenderText(DrawingContext drawingContext)
        {
            var ft = new FormattedText(
                Document.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface,
                14.0,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip
            );

            // Draw text
            drawingContext.DrawText(ft, new Point(0, 0));
        }

        private void PrintText(DrawingContext drawingContext)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // Get blocks.
            var highlightBlocks =
                Block.GetDecorations<HighlightBlock>(Document);

            RenderHighlight(highlightBlocks, drawingContext);

            RenderText(drawingContext);
            var textColorBlocks =
                Block.GetDecorations<TextColorBlock>(Document);

            RenderTextColor(textColorBlocks, drawingContext);

            Console.WriteLine(watch.ElapsedMilliseconds);

        }


        private void UpdateElementSize()
        {
            FormattedText ft2 = GetFormattedText(Document.Text);

            // Set new width and height
            Width = ft2.WidthIncludingTrailingWhitespace + whiteSpaceOnTheRight;
            Height = ft2.Height + whiteSpaceOnTheBottom;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Console.WriteLine("---------------------start----------------------");

            // Fill background
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, Width, Height));

            PrintText(drawingContext);

            UpdateElementSize();

            // Draw border
            drawingContext.DrawRectangle(null, new Pen(Brushes.Blue, 1.0), new Rect(0, 0, Width, Height));

            Console.WriteLine("---------------------end----------------------");

        }

        #endregion

    }
}

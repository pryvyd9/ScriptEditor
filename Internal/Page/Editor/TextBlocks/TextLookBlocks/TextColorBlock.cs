using System.Windows;
using System.Windows.Media;

namespace ScriptEditor
{
    public class TextColorBlock : TextLookBlock
    {
        public Brush Brush { get; set; }

        //public void OnRender(Editor editor, DrawingContext drawingContext)
        //{
        //    var startInfo = editor.Document.GetPositionInText(Start);
        //    var endInfo = editor.Document.GetPositionInText(End);


        //    double left = startInfo.inRowPosition * editor.LetterWidth;
        //    double top = startInfo.row * editor.LetterHeight;

        //    var ft = new FormattedText(
        //        Start.GetRange(End).ToStr(),
        //        System.Globalization.CultureInfo.CurrentCulture,
        //        FlowDirection.LeftToRight,
        //        editor.Typeface,
        //        14.0,
        //        Brushes.Black,
        //        VisualTreeHelper.GetDpi(editor).PixelsPerDip
        //    );

        //    // Draw text
        //    drawingContext.DrawText(ft, new Point(left, top));

        //    //double right = (endInfo.inRowPosition + 1) * editor.LetterWidth;
        //    //double bottom = (endInfo.row + 1) * editor.LetterHeight;

        //    //double width = right - left;
        //    //double height = bottom - top;

        //    //drawingContext.DrawRectangle(Brush, Pen, new Rect(left, top, width, height));
        //}

        public void OnRender(Editor editor, DrawingContext drawingContext, int inStringPosition)
        {
            var startInfo = editor.Document.GetPositionInText(inStringPosition);
            //var endInfo = editor.Document.GetPositionInText(End);
            var ch = editor.Document.Content.NodeAt(inStringPosition).Value;

            double left = startInfo.inRowPosition * editor.LetterWidth;
            double top = startInfo.row * editor.LetterHeight;

            var ft = new FormattedText(
                ch.ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                editor.Typeface,
                14.0,
                Brush,
                VisualTreeHelper.GetDpi(editor).PixelsPerDip
            );

            // Draw text
            drawingContext.DrawText(ft, new Point(left, top));

            //double right = (endInfo.inRowPosition + 1) * editor.LetterWidth;
            //double bottom = (endInfo.row + 1) * editor.LetterHeight;

            //double width = right - left;
            //double height = bottom - top;

            //drawingContext.DrawRectangle(Brush, Pen, new Rect(left, top, width, height));
        }
    }

}

using System.Windows;
using System.Windows.Media;

namespace ScriptEditor
{
    public sealed class HighlightBlock : TextDecorationBlock
    {
        public Brush Brush { get; set; }
        public Pen Pen { get; set; }

        //public override void OnRender(Editor editor, DrawingContext drawingContext)
        //{
        //    var startInfo = editor.Document.GetPositionInText(Start);
        //    var endInfo = editor.Document.GetPositionInText(End);


        //    double left = startInfo.inRowPosition * editor.LetterWidth;
        //    double top = startInfo.row * editor.LetterHeight;

        //    double right = (endInfo.inRowPosition + 1) * editor.LetterWidth;
        //    double bottom = (endInfo.row + 1) * editor.LetterHeight;

        //    double width = right - left;
        //    double height = bottom - top;

        //    drawingContext.DrawRectangle(Brush, Pen, new Rect(left, top, width, height));
        //}

        public void OnRender(Editor editor, DrawingContext drawingContext, int inStringPosition)
        {
            var startInfo = editor.Document.GetPositionInText(inStringPosition);

            double left = startInfo.inRowPosition * editor.LetterWidth;
            double top = startInfo.row * editor.LetterHeight;

            double right = (startInfo.inRowPosition + 1) * editor.LetterWidth;
            double bottom = (startInfo.row + 1) * editor.LetterHeight;

            double width = right - left;
            double height = bottom - top;

            left = (int)left;
            top = (int)top;
            width = (int)width+1;
            height = (int)height+1;

            drawingContext.DrawRectangle(Brush, Pen, new Rect(left, top, width, height));
        }
    }


}

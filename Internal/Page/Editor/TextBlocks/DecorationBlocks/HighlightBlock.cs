using System.Windows;
using System.Windows.Media;

namespace ScriptEditor
{
    public sealed class HighlightBlock : TextDecorationBlock
    {
        public Brush Brush { get; set; }
        public Pen Pen { get; set; }

        public override void OnRender(Editor editor, DrawingContext drawingContext)
        {
            var startInfo = editor.Document.GetPositionInText(Start);
            var endInfo = editor.Document.GetPositionInText(End);


            double left = startInfo.inRowPosition * editor.LetterWidth;
            double top = startInfo.row * editor.LetterHeight;

            double right = (endInfo.inRowPosition + 1) * editor.LetterWidth;
            double bottom = (endInfo.row + 1) * editor.LetterHeight;

            double width = right - left;
            double height = bottom - top;

            drawingContext.DrawRectangle(Brush, Pen, new Rect(left, top, width, height));
        }
    }


}

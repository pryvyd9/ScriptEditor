using System.Windows;
using System.Windows.Media;
using System.Linq;

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

        public void RenderRange(Editor editor, DrawingContext drawingContext, int row, int inRowPosition, int count)
        {
            double left = inRowPosition * editor.LetterWidth;
            double top = row * editor.LetterHeight;


            double right = (inRowPosition + count) * editor.LetterWidth;
            double bottom = (row + 1) * editor.LetterHeight;


            //var isSelection = Tags.Contains("selection");

            //if (isSelection)
            //{
            //    var poss = editor.Document.GetPositionInText(editor.SelectionRange.left);
            //    left = poss.inRowPosition * editor.LetterWidth;
            //    poss = editor.Document.GetPositionInText(editor.SelectionRange.right + 1);
            //    right = (poss.inRowPosition) * editor.LetterWidth;
            //}


            double width = right - left;
            double height = bottom - top;

            left = (int)left;
            top = (int)top;
            width = (int)width + 1;
            height = (int)height + 1;

            drawingContext.DrawRectangle(Brush, Pen, new Rect(left, top, width, height));
        }


        public void RenderRange(Editor editor, DrawingContext drawingContext, string str, (int inStringPosition, int row, int inRowPosition) pos)
        {
            double left = pos.inRowPosition * editor.LetterWidth;
            double top = pos.row * editor.LetterHeight;


            double right = (pos.inRowPosition + str.Length) * editor.LetterWidth;
            double bottom = (pos.row + 1) * editor.LetterHeight;


            var isSelection = Tags.Contains(Tag.Selection);

            if (isSelection)
            {
                var poss = editor.Document.GetPositionInText(editor.SelectionRange.left);
                left = poss.inRowPosition * editor.LetterWidth;
                poss = editor.Document.GetPositionInText(editor.SelectionRange.right + 1);
                right = (poss.inRowPosition) * editor.LetterWidth;
            }


            double width = right - left;
            double height = bottom - top;

            left = (int)left;
            top = (int)top;
            width = (int)width + 1;
            height = (int)height + 1;

            drawingContext.DrawRectangle(Brush, Pen, new Rect(left, top, width, height));
        }

        public void OnRender(Editor editor, DrawingContext drawingContext, int inStringPosition)
        {
            var startInfo = editor.Document.GetPositionInText(inStringPosition);

            double left = startInfo.inRowPosition * editor.LetterWidth;
            double top = startInfo.row * editor.LetterHeight;

            double right = (startInfo.inRowPosition + 1) * editor.LetterWidth;
            double bottom = (startInfo.row + 1) * editor.LetterHeight;

            var isSelection = Tags.Contains(Tag.Selection);

            if (isSelection)
            {
                var poss = editor.Document.GetPositionInText(editor.SelectionRange.left);
                left = poss.inRowPosition * editor.LetterWidth;
                poss = editor.Document.GetPositionInText(editor.SelectionRange.right + 1);
                right = (poss.inRowPosition) * editor.LetterWidth;
            }

            double width = right - left;
            double height = bottom - top;

            left = (int)left;
            top = (int)top;
            width = (int)width + 1;
            height = (int)height + 1;

            drawingContext.DrawRectangle(Brush, Pen, new Rect(left, top, width, height));
        }
    }


}

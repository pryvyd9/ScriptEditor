using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScriptEditor
{
    public sealed class Line : Block
    {

    }

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

    public sealed class UnderlineBlock : TextDecorationBlock
    {
        public Pen Pen { get; set; }

        public override void OnRender(Editor editor, DrawingContext drawingContext)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class TextDecorationBlock : Block
    {
        public abstract void OnRender(Editor editor, DrawingContext drawingContext);
    }


    public abstract class Block
    {
        public LinkedListNode<char> Start { get; set; }
        public LinkedListNode<char> End { get; set; }

        public string Text => Characters.ToStr();
        public IEnumerable<char> Characters => Start.GetRange(End);


        public override string ToString()
        {
            return Text;
        }
    }


}

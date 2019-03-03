using System;
using System.Windows.Media;

namespace ScriptEditor
{
    public sealed class UnderlineBlock : TextDecorationBlock
    {
        public Pen Pen { get; set; }

        public override void OnRender(Editor editor, DrawingContext drawingContext)
        {
            throw new NotImplementedException();
        }
    }


}

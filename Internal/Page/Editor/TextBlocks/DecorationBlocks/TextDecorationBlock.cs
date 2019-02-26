using System.Windows.Media;

namespace ScriptEditor
{
    public abstract class TextDecorationBlock : Block
    {
        public abstract void OnRender(Editor editor, DrawingContext drawingContext);
    }


}

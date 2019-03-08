using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScriptEditor
{
  
    public class EditorScrollViewer : ScrollViewer
    {
        public bool ShouldAlign { get; set; } = true;

        public double VerticalSingleStepOffset { get; set; }
        public double HorizontalSingleStepOffset { get; set; }

        private const double E = 0.001;



        protected override void OnKeyDown(KeyEventArgs e)
        {
            //base.OnKeyDown(e);
        }

    }
}

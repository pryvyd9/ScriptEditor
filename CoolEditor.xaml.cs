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
    /// <summary>
    /// Interaction logic for ScriptEditor.xaml
    /// </summary>
    public partial class CoolEditor : UserControl
    {
        private readonly List<EditorView> editors = new List<EditorView>();

        public CoolEditor()
        {
            InitializeComponent();
        }

        public void OpenDocument(Document document)
        {
            EditorView editor = new EditorView();

            editors.Add(editor);

            editor.SetDocument(document);

            editorsGrid.Children.Add(editor);
        }

        public void Refresh()
        {
            foreach (var editor in editors)
            {
                editor.Refresh();
            }
        }

        public void Focus(int editorIndex)
        {
            editors[editorIndex].Focus();
        }
    }
}

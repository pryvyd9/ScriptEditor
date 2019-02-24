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
        private readonly HashSet<(Document, EditorView)> editors = new HashSet<(Document, EditorView)>();

        private bool TryFindTabItem(Document document, out TabItem tab)
        {
            var elementTofind = editors.First(n => n.Item1 == document);

            foreach (var item in editorsHolder.Items)
            {
                if (item is TabItem tb && tb.Content == elementTofind.Item2)
                {
                    tab = tb;
                    return true;
                }
            }

            tab = null;
            return false;
        }

        public CoolEditor()
        {
            InitializeComponent();
        }

        public void OpenDocument(Document document)
        {
            EditorView editor = new EditorView();

            editors.Add((document, editor));

            editor.SetDocument(document);

            editorsHolder.Items.Add(new TabItem() { Content = editor, Header = document.Name });
        }

        public void CloseDocument(Document document)
        {
            var elementToRemove = editors.First(n => n.Item1 == document);

            editors.Remove(elementToRemove);

            if (TryFindTabItem(document, out var tab))
            {
                editorsHolder.Items.Remove(tab);
            }
            else
            {
                throw new Exception("Document is not open");
            }
        }

        public void Refresh()
        {
            foreach (var editor in editors)
            {
                editor.Item2.Refresh();
            }
        }

        public void Focus(Document document)
        {
            if (TryFindTabItem(document, out var tab))
            {
                editorsHolder.SelectedIndex = editorsHolder.Items.IndexOf(tab);

                var elementToFocus = editors.First(n => n.Item1 == document);

                elementToFocus.Item2.Focus();
            }
            else
            {
                throw new Exception("Document is not open");
            }

        }
    }
}

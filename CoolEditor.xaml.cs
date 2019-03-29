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
        private readonly HashSet<(IDocument, EditorView)> editors = new HashSet<(IDocument, EditorView)>();

        private bool TryFindTabItem(IDocument document, out TabItem tab)
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

        public void OpenDocument(IDocument document)
        {
            EditorView editor = new EditorView();

            editors.Add((document, editor));

            editor.SetDocument(document);

            editorsHolder.Items.Add(new TabItem() { Content = editor, Header = document.Name });
        }

        public void CloseDocument(IDocument document)
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

        public void Focus(IDocument document)
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

        public void ApplyTextColor(IDocument document, string[] keywords, Color color, int[] tags = null)
        {
            var colorizeTag = -1;

            var allTags = new[] { colorizeTag };

            if (tags != null)
            {
                allTags = allTags.Concat(tags).ToArray();
            }
            
            var brush = new SolidColorBrush(color);

            void Update(IDocument d)
            {
                d.TextLookBlocks.RemoveAll(n => n.Tags.Contains(colorizeTag));

                var search = d.FindAll(keywords, 0, d.Length - 1);
                d.ApplyTextColor(search, allTags, brush);
            }

            document.Updated += Update;

            Update(document);


        }

    }
}

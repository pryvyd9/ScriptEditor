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
        public IEnumerable<IDocument> Documents => editors.Select(n => n.document);

        private IDocument lastFocusedDocument;
        public IDocument ActiveDocument { get; private set; }

        private readonly HashSet<(IDocument document, EditorView editor)> editors = new HashSet<(IDocument, EditorView)>();



        public event DocumentUpdatedEventHandler DocumentUpdated;
        public event DocumentUpdatedEventHandler DocumentOpened;
        public event DocumentUpdatedEventHandler DocumentFocused;



        private bool TryFindTabItem(IDocument document, out TabItemExtended tab)
        {
            var elementTofind = editors.First(n => n.Item1 == document);

            foreach (var item in editorsHolder.Items)
            {
                if (item is TabItemExtended tb && tb.Content == elementTofind.Item2)
                {
                    tab = tb;
                    return true;
                }
            }

            tab = null;
            return false;
        }

        private IDocument GetDocument(TabItemExtended item)
        {
            return editors.First(n => n.Item2 == item.Content).Item1;
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
            editor.EditorFocused += Editor_EditorFocused;

            var tabItem = new TabItemExtended(document)
            {
                Content = editor,
            };

            tabItem.TabClosed   += (o, e) => TabItem_TabClosed(tabItem, e);
            tabItem.TabSelected += (o, e) => Focus(document);

            editorsHolder.Items.Add(tabItem);

            document.Updated += Document_Updated;

            DocumentOpened?.Invoke(document);
        }

        private void SetActiveDocument(IDocument document)
        {
            lastFocusedDocument = ActiveDocument;

            ActiveDocument = document;
        }

        private void Editor_EditorFocused(Editor editor)
        {
            SetActiveDocument(editor.Document);

            DocumentFocused?.Invoke(ActiveDocument);
        }

        private void Document_Updated(IDocument document)
        {
            DocumentUpdated?.Invoke(document);
        }

        private void TabItem_TabClosed(object sender, RoutedEventArgs e)
        {
            var tab = (TabItemExtended)sender;

            var doc = GetDocument(tab);

            //var elementToRemove = editors.First(n => n.Item1 == doc);

            //editors.Remove(elementToRemove);

            //editorsHolder.Items.Remove(tab);

            CloseDocument(doc);

            //if (editors.Any())
            //{
            //    Focus(editors.First().document);
            //}
        }

        public void CloseDocument(IDocument document)
        {
            var elementToRemove = editors.First(n => n.Item1 == document);


            if (!TryFindTabItem(document, out var tab))
                throw new Exception("Document is not open");

            editors.Remove(elementToRemove);
            editorsHolder.Items.Remove(tab);

            if (editors.Any())
            {
                Focus(editors.First().document);
            }
            else
            {
                SetActiveDocument(null);
            }

            //Focus(editors.First().document);
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
            if (!TryFindTabItem(document, out var tab))
                throw new Exception("Document is not open");
           
            editorsHolder.SelectedIndex = editorsHolder.Items.IndexOf(tab);

            var elementToFocus = editors.First(n => n.Item1 == document);

            elementToFocus.Item2.Focus();

            SetActiveDocument(elementToFocus.document);
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

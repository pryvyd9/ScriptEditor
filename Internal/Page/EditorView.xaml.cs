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
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        private Editor editor;

        private RowCounter rowCounter;

        public EditorView()
        {
            InitializeComponent();
        }

        public void Refresh()
        {
            editor.Refresh();
        }

        public void SetDocument(Document document)
        {
            editor = new Editor();
            editor.SetDocument(document);
            editor.Initialize(editorHoldPlace);


            rowCounter = new RowCounter();
            rowCounter.Initialize(leftBarHoldPlace, editor);
            rowCounter.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            rowCounter.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //rowCounter.IsEnabled = false;
            UpdateRowCount();

            editor.Document.Lines.CollectionChanged += Lines_CollectionChanged;

            // Syncronize scrolls
            editor.ScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            rowCounter.ScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

            Bind();
        }

        private void Bind()
        {
            //Binding editorBinding = new Binding
            //{
            //    Source = editor.ScrollViewer,
            //    Path = new PropertyPath("ViewportHeight"),
            //    Mode = BindingMode.OneWay,
            //    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            //};
            //BindingOperations.SetBinding(editor, MinHeightProperty, editorBinding);


            Binding rowConterBinding = new Binding
            {
                Source = (editor.ScrollViewer.Content as FrameworkElement),
                Path = new PropertyPath("Height"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            BindingOperations.SetBinding(rowCounter, MinHeightProperty, rowConterBinding);

        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender.Equals(editor.ScrollViewer))
            {
                rowCounter.ScrollViewer.ScrollToVerticalOffset(editor.ScrollViewer.VerticalOffset);
            }
            else
            {
                editor.ScrollViewer.ScrollToVerticalOffset(rowCounter.ScrollViewer.VerticalOffset);
                rowCounter.ScrollViewer.ScrollToVerticalOffset(editor.ScrollViewer.VerticalOffset);
            }

        }

        private void UpdateRowCount()
        {
            rowCounter.Count = editor.Document.Lines.Count;
        }

        private void Lines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateRowCount();
        }

        public new void Focus()
        {
            editor.Focus();
        }

    }
}

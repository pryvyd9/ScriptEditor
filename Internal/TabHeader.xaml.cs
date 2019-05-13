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

namespace ScriptEditor.Internal
{
    public delegate void TabClosedEventHandler(object sender, RoutedEventArgs e);

    /// <summary>
    /// Interaction logic for TabHeader.xaml
    /// </summary>
    public partial class TabHeader : UserControl
    {
        public event TabClosedEventHandler TabClosed;
        public event TabClosedEventHandler TabSelected;

        //public static readonly DependencyProperty DocumentName;
        //static TabHeader()
        //{
        //    DocumentName = DependencyProperty.Register("UpdateCrutch", typeof(string), typeof(TabHeader),
        //        new FrameworkPropertyMetadata(
        //            false,
        //            FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.AffectsRender
        //        )
        //    );
        //}


        public TabHeader()
        {
            InitializeComponent();
        }
        public void BindName(IDocument doc)
        {
            name.Content = doc.Name;
            doc.NameChanged += (d) => name.Content = d.Name;

            //var binding = new Binding()
            //{
            //    Path = new PropertyPath("Name"),
            //    Source = doc,
            //    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            //};

            //name.SetBinding(ContentProperty, binding);
        }
        //public string Text { get => name.Text; set => name.Text = value; }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            TabClosed?.Invoke(sender, e);
        }

        //private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    TabSelected?.Invoke(sender, e);
        //}

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TabSelected?.Invoke(sender, e);

        }
    }
}

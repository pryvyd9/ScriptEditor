using System.Windows.Controls;

namespace ScriptEditor
{
    class TabItemExtended : TabItem
    {
        public event Internal.TabClosedEventHandler TabClosed
        {
            add => ((Internal.TabHeader)Header).TabClosed += value;
            remove => ((Internal.TabHeader)Header).TabClosed -= value;
        }

        public event Internal.TabClosedEventHandler TabSelected
        {
            add => ((Internal.TabHeader)Header).TabSelected += value;
            remove => ((Internal.TabHeader)Header).TabSelected -= value;
        }

        public TabItemExtended(IDocument doc)
        {
            //Header = new Internal.TabHeader() { Text = name };
            var header = new Internal.TabHeader();
            header.BindName(doc);
            Header = header;
        }

        public new string Name { get => (string)((Internal.TabHeader)Header).name.Content; }
    
    }
}

using System.Collections.Generic;
using System.Collections.Specialized;


namespace ScriptEditor
{
    public class ObservableLinkedList<T> : LinkedList<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public new void AddAfter(LinkedListNode<T> node, T newNode)
        {
            base.AddAfter(node, newNode);

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { node, node.Next });

            CollectionChanged?.Invoke(this, args);
        }

        public new void AddBefore(LinkedListNode<T> node, T newNode)
        {
            base.AddBefore(node, newNode);

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { node.Previous, node });

            CollectionChanged?.Invoke(this, args);
        }

        public new void Remove(LinkedListNode<T> node)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node);

            CollectionChanged?.Invoke(this, args);

            base.Remove(node);
        }
    }


}

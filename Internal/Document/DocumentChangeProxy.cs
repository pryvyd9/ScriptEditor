using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Windows;
using ImpromptuInterface;

using System.Linq;

namespace ScriptEditor
{
    internal sealed class DocumentChangeProxy : DynamicObject
    {
        private static readonly Type type = typeof(Document);
        private readonly Document origin;

        //public ObservableLinkedList<char> Content => origin.Content;
        //public List<TextDecorationBlock> TextDecorations => origin.TextDecorations;
        //public ObservableCollection<Line> Lines => origin.Lines;
        //public char[] InvisibleCharacters => origin.InvisibleCharacters;
        //public string Text => origin.Text;
        //public string LineEnding => origin.LineEnding;
        //public bool IsRevertingChanges => origin.IsRevertingChanges;
        //public string Name => origin.Name;

        public void Insert(LinkedListNode<char> position, IEnumerable<char> collection)
        {
            origin.StartChanges();
            origin.Insert(position, collection);
            origin.CommitChanges();
        }
        public void Insert(LinkedListNode<char> position, char ch)
        {
            origin.StartChanges();
            origin.Insert(position, ch);
            origin.CommitChanges();
        }
        public void Insert(int inStringPosition, char ch)
        {
            origin.StartChanges();
            origin.Insert(inStringPosition, ch);
            origin.CommitChanges();
        }

        public void Delete(int inStringPosition)
        {
            origin.StartChanges();
            origin.Delete(inStringPosition);
            origin.CommitChanges();
        }
        public void Delete(IEnumerable<LinkedListNode<char>> nodes)
        {
            origin.StartChanges();
            origin.Delete(nodes);
            origin.CommitChanges();
        }
        public void Delete(LinkedListNode<char> node)
        {
            origin.StartChanges();
            origin.Delete(node);
            origin.CommitChanges();
        }

        public void BreakLine(Line line, LinkedListNode<char> position)
        {
            origin.StartChanges();
            origin.BreakLine(line, position);
            origin.CommitChanges();
        }


        public (int inStringPosition, int row, int inRowPosition) GetPositionInText(Point point, double letterHeight, double letterWidth) => origin.GetPositionInText(point, letterHeight, letterWidth);
        public (int inStringPosition, int row, int inRowPosition) GetPositionInText(int row, int inRowPosition) => origin.GetPositionInText(row, inRowPosition);
        public (int inStringPosition, int row, int inRowPosition) GetPositionInText(int inStringPosition) => origin.GetPositionInText(inStringPosition);
        public (int inStringPosition, int row, int inRowPosition) GetPositionInText(LinkedListNode<char> node) => origin.GetPositionInText(node);
        public Point GetPositionInText(LinkedListNode<char> node, double letterHeight, double letterWidth) => origin.GetPositionInText(node, letterHeight, letterWidth);


        public void RollbackChanges() => origin.RollbackChanges();

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var proxiedMethod = GetType().GetMethods().SingleOrDefault(n =>
            {
                return
                    n.Name == binder.Name &&
                    args.Select(m => m.GetType()).SequenceEqual(n.GetParameters().Select(m => m.GetType()));
            });

            if (proxiedMethod != null)
            {
                result = proxiedMethod.Invoke(this, args);
                return true;
            }
            else
            {
                var originalMethod = origin.GetType().GetMethods().SingleOrDefault(n =>
                {
                    var nn = n.Name == binder.Name;

                    if (nn)
                    {
                        var aargs = args.Select((m,i) => m?.GetType()).ToArray();
                        //var aargs = args.Select((m,i) => m?.GetType() ?? n.GetParameters().Select(k => k.ParameterType).ElementAt(i)).ToArray();
                        var targs = n.GetParameters().Select(m => m.ParameterType).ToArray();

                        if (aargs.Length != targs.Length)
                        {
                            return false;
                        }

                        for (int i = 0; i < aargs.Length; i++)
                        {
                            var isAssignable = targs[i].IsAssignableFrom(aargs[i]);
                            var isOptional = n.GetParameters().ElementAt(i).IsOptional;

                            if (aargs[i] == null && isOptional)
                            {
                                continue;
                            }
                            else if (!isAssignable)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                   
                    //return
                    //    n.Name == binder.Name &&
                    //    args.Select(m => m.GetType()).SequenceEqual(n.GetParameters().Select(m => m.GetType()));
                });

                if (originalMethod != null)
                {
                    
                    //var newArgs = args.Select(n => n is null ? Type.Missing : n).ToArray();
                    //result = originalMethod.Invoke(this, newArgs);
                    result = originalMethod.Invoke(origin, args);
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            
        }


        private class TypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.AssemblyQualifiedName == y.AssemblyQualifiedName;
            }

            public int GetHashCode(Type obj)
            {
                return obj.GetHashCode();
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                result = type.GetProperty(binder.Name).GetValue(origin);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        internal DocumentChangeProxy(Document document)
        {
            this.origin = document;
        }

        internal static IDocument AsIDocument(Document document)
        {
            return new DocumentChangeProxy(document).ActLike<IDocument>();
        }

    }


}

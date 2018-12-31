using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using ImpromptuInterface;


namespace ScriptEditor
{
    internal sealed class DocumentChangeProxy : DynamicObject
    {
        private static readonly Type type = typeof(Document);
        private readonly Document document;

        internal ObservableLinkedList<char> Content => document.Content;
        internal List<TextDecorationBlock> TextDecorations => document.TextDecorations;
        internal List<Line> Lines => document.Lines;
        internal char[] InvisibleCharacters => document.InvisibleCharacters;
        internal string Text => document.Text;
        internal string LineEnding => document.LineEnding;
        internal bool IsRevertingChanges => document.IsRevertingChanges;

        internal void Insert(LinkedListNode<char> position, IEnumerable<char> collection)
        {
            document.StartChanges();
            document.Insert(position, collection);
            document.CommitChanges();
        }
        internal void Insert(LinkedListNode<char> position, char ch)
        {
            document.StartChanges();
            document.Insert(position, ch);
            document.CommitChanges();
        }
        internal void Insert(int inStringPosition, char ch)
        {
            document.StartChanges();
            document.Insert(inStringPosition, ch);
            document.CommitChanges();
        }

        internal void Delete(int inStringPosition)
        {
            document.StartChanges();
            document.Delete(inStringPosition);
            document.CommitChanges();
        }
        internal void Delete(IEnumerable<LinkedListNode<char>> nodes)
        {
            document.StartChanges();
            document.Delete(nodes);
            document.CommitChanges();
        }
        internal void Delete(LinkedListNode<char> node)
        {
            document.StartChanges();
            document.Delete(node);
            document.CommitChanges();
        }

        internal void BreakLine(Line line, LinkedListNode<char> position)
        {
            document.StartChanges();
            document.BreakLine(line, position);
            document.CommitChanges();
        }


        internal (int inStringPosition, int row, int inRowPosition) GetPositionInText(Point point, double letterHeight, double letterWidth) => document.GetPositionInText(point, letterHeight, letterWidth);
        internal (int inStringPosition, int row, int inRowPosition) GetPositionInText(int row, int inRowPosition) => document.GetPositionInText(row, inRowPosition);
        internal (int inStringPosition, int row, int inRowPosition) GetPositionInText(int inStringPosition) => document.GetPositionInText(inStringPosition);
        internal (int inStringPosition, int row, int inRowPosition) GetPositionInText(LinkedListNode<char> node) => document.GetPositionInText(node);
        internal Point GetPositionInText(LinkedListNode<char> node, double letterHeight, double letterWidth) => document.GetPositionInText(node, letterHeight, letterWidth);


        internal void RollbackChanges() => document.RollbackChanges();





        internal DocumentChangeProxy(Document document)
        {
            this.document = document;
        }

        internal static IDocument AsIDocument(Document document)
        {
            return new DocumentChangeProxy(document).ActLike<IDocument>();
        }
    }


}

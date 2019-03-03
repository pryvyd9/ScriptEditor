using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;


namespace ScriptEditor
{
    public interface IDocument
    {
        ObservableLinkedList<char> Content { get; }
        ObservableCollection<Line> Lines { get; }
        List<TextLookBlock> TextLookBlocks { get; }
        char[] InvisibleCharacters { get; }
        string LineEnding { get; }
        string Text { get; }
        string Name { get; }
        string Path { get; }
        bool IsRevertingChanges { get; }




        void Insert(LinkedListNode<char> position, IEnumerable<char> collection);
        void Insert(LinkedListNode<char> position, char ch);
        void Insert(int inStringPosition, char ch);

        void Delete(int inStringPosition);
        void Delete(IEnumerable<LinkedListNode<char>> nodes);
        void Delete(LinkedListNode<char> node);

        void BreakLine(Line line, LinkedListNode<char> position);

        (int inStringPosition, int row, int inRowPosition) GetPositionInText(Point point, double letterHeight, double letterWidth);
        (int inStringPosition, int row, int inRowPosition) GetPositionInText(int row, int inRowPosition);
        (int inStringPosition, int row, int inRowPosition) GetPositionInText(int inStringPosition);
        (int inStringPosition, int row, int inRowPosition) GetPositionInText(LinkedListNode<char> node);
        Point GetPositionInText(LinkedListNode<char> node, double letterHeight, double letterWidth);

        void RollbackChanges();
    }


}

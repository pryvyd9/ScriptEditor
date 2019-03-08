using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;


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
        void Delete(int left, int right);

        void BreakLine(Line line, LinkedListNode<char> position);

        (int inStringPosition, int row, int inRowPosition) GetPositionInText(Point point, double letterHeight, double letterWidth);
        (int inStringPosition, int row, int inRowPosition) GetPositionInText(int row, int inRowPosition);
        (int inStringPosition, int row, int inRowPosition) GetPositionInText(int inStringPosition);
        (int inStringPosition, int row, int inRowPosition) GetPositionInText(LinkedListNode<char> node);
        Point GetPositionInText(LinkedListNode<char> node, double letterHeight, double letterWidth);

        void RollbackChanges();


        void ApplyHighlight((int start, int end)[] ranges, string[] tags, Brush brush, Pen pen = null);
        void ApplyTextColor((int start, int end)[] ranges, string[] tags, Brush brush);


        (int start, int end)[] FindAll(string substring, int startIndex, int endIndex);
    }


}

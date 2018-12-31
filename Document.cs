using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace ScriptEditor
{
    //public delegate void DocumentUpdatedEventHandler();

   

    public sealed class Document : IDocument
    {
        public LinkedList<char> Content { get; } = new LinkedList<char>();

        public List<Line> Lines { get; } = new List<Line>();

        public List<TextDecorationBlock> TextDecorations { get; } = new List<TextDecorationBlock>();

        public string Text => new string(Content.ToArray());


        


        public string LineEnding { get; } = "\r\n";

        public char[] WhiteDelimiters { get; } = new[] { '\r', '\n', ' ' };

        //public event DocumentUpdatedEventHandler Updated;


        private ChangesBuffer Buffer { get; } = new ChangesBuffer();



        public Document(string text)
        {
            var textLines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);

            foreach (var textLine in textLines)
            {
                if (string.IsNullOrEmpty(textLine))
                {
                    Content.AddLast('\r');

                    Line line = new Line
                    {
                        Start = Content.Last,
                    };

                    Content.AddLast('\n');

                    line.End = Content.Last;

                    Lines.Add(line);
                }
                else
                {
                    Content.AddLast(textLine.First());

                    Line line = new Line
                    {
                        Start = Content.Last,
                    };

                    Content.AddLastRange(textLine.Substring(1) + "\r\n");

                    line.End = Content.Last;

                    Lines.Add(line);
                }
            }
        }

        public (int inStringPosition, int row, int inRowPosition) GetPositionInText(Point point, double letterHeight, double letterWidth)
        {
            var left = point.X;
            var top = point.Y;

            int rowIndex = (int)(top / letterHeight);

            if (rowIndex >= Lines.Count)
            {
                if(Content.Last.Value == LineEnding.Last())
                {
                    return GetPositionInText(Content.Last.GetAtOffset(-LineEnding.Length+1));
                }
                else
                {
                    return GetPositionInText(Content.Last);
                }
            }

            Line line = Lines[rowIndex];

            int inRowIndex = (int)(left / letterWidth);

            LinkedListNode<char> charElement;

            int inStringPosition;

            // If line length is less than inRowIndex then 
            // choose the last visible character.
            if (line.Text.Length - LineEnding.Length < inRowIndex)
            {
                charElement = line.End.Previous.Previous;
                inRowIndex = line.Text.Length - LineEnding.Length;
                inStringPosition = Content.IndexOf(charElement) + 1;
            }
            else
            {
                charElement = line.Start.GetAtOffset(inRowIndex);
                inStringPosition = Content.IndexOf(charElement);

            }

            return (inStringPosition, rowIndex, inRowIndex);
        }

        public (int inStringPosition, int row, int inRowPosition) GetPositionInText(int row, int inRowPosition)
        {
            var line = Lines[row];

            if(line.Start.GetRange(line.End).Count() - 1 <= inRowPosition)
            {
                return GetPositionInText(line.End.GetAtOffset(-1));
            }

            return GetPositionInText(line.Start.GetAtOffset(inRowPosition));

        }

        public (int inStringPosition, int row, int inRowPosition) GetPositionInText(int inStringPosition)
        {
            return GetPositionInText(Content.NodeAt(inStringPosition));
        }

        public (int inStringPosition, int row, int inRowPosition) GetPositionInText(LinkedListNode<char> node)
        {
            int row = 0;
            int inRowPosition = 0;

            LinkedListNode<char> current = Content.First;

            for(int inStringPosition = 0; inStringPosition < Content.Count; inStringPosition++, inRowPosition++)
            {
                if(Lines.Any(n=>n.Start == current))
                {
                    row = Lines.IndexOf(Lines.First(n => n.Start == current));
                    inRowPosition = 0;
                }

                if(current == node)
                {
                    return (inStringPosition, row, inRowPosition);
                }

                current = current.Next;
            }

            return (-1, -1, -1);
        }

        public (LinkedListNode<char> start, LinkedListNode<char> end) GetWordOf(LinkedListNode<char> letter)
        {
            LinkedListNode<char> current = letter;

            LinkedListNode<char> left;

            // Find left end of word
            while (true)
            {
                if (IsWhiteDelimiter(current.Value))
                {
                    left = current.Next;
                    break;
                }
                else if(Content.First == current)
                {
                    left = Content.First;
                    break;
                }

                current = current.Previous;
            }

            LinkedListNode<char> right;

            // Reset current.
            current = letter;

            // Find left end of word
            while (true)
            {
                if (IsWhiteDelimiter(current.Value))
                {
                    right = current.Previous;
                    break;
                }
                else if (Content.Last == current)
                {
                    right = Content.Last;
                    break;
                }

                current = current.Next;
            }

            return (left, right);
        }

        public Point GetPositionInText(LinkedListNode<char> node, double letterHeight, double letterWidth)
        {
            var (inStringPosition, row, inRowPosition) = GetPositionInText(node);


            double x = inRowPosition * letterWidth;
            double y = row * letterHeight;

            return new Point(x, y);
        }

        public void Insert(LinkedListNode<char> position, IEnumerable<char> collection)
        {
            foreach (var item in collection)
            {
                Insert(position, item);
            }
        }

        public void Insert(LinkedListNode<char> position, char ch)
        {
            Content.AddBefore(position, ch);


            if(!LineEnding.Contains(ch) &&
                Lines.Any(n=>n.Start == position))
            {
                Lines.First(n => n.Start == position).Start = position.Previous;
            }

        }

        public void Insert(int inStringPosition, char ch)
        {
            Insert(Content.NodeAt(inStringPosition), ch);
        }

        public void Delete(int inStringPosition)
        {
            Delete(Content.NodeAt(inStringPosition));
        }

        public void Delete(IEnumerable<LinkedListNode<char>> nodes)
        {
            if(nodes.Select(n=>n.Value).ToStr() == LineEnding)
            {
                var firstLine = Lines.First(n => n.End == nodes.Last());

                if(Lines.IndexOf(firstLine) + 1 == Lines.Count)
                {
                    return;
                }

                var secondLine = Lines[Lines.IndexOf(firstLine) + 1];

                MergeLines(firstLine, secondLine);

                return;
            }

            foreach (var node in nodes)
            {
                Delete(node);
            }
        }

        public void Delete(LinkedListNode<char> node)
        {
            if(Lines.Any(n=>n.Start == node))
            {
                Lines.First(n => n.Start == node).Start = node.Next;
            }
            else if (Lines.Any(n => n.End == node))
            {
                Lines.First(n => n.End == node).End = node.Previous;
            }

            Content.Remove(node);
            
        }


        public void BreakLine(Line line, LinkedListNode<char> position)
        {
            Line newLine = new Line
            {
                Start = position,
                End = line.End,
            };

            Lines.Insert(Lines.IndexOf(line) + 1, newLine);

            Insert(position, "\r\n");

            // If line is empty then start is the first in newLine.
            if(line.Start == newLine.Start)
            {
                line.Start = position.GetAtOffset(-LineEnding.Length);
            }
            
            line.End = position.Previous;
        }

        public void MergeLines(Line first, Line second)
        {
            var firstEnd = first.End;
            var firstPreEnd = firstEnd.Previous;

            Delete(firstEnd);
            Delete(firstPreEnd);

            first.End = second.End;
            Lines.Remove(second);
        }




        public void StartChanges() => Buffer.Start();

        public void CommitChanges() => Buffer.Commit();

        public void RoolbackChanges() => Buffer.RollBack();





        private bool IsWhiteDelimiter(char ch)
        {
            return WhiteDelimiters.Contains(ch);
        }

    }


}

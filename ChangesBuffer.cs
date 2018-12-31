using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEditor
{
    public abstract class Change
    {
        public abstract void Revert(Document document);
    }

    public sealed class Insert : Change
    {
        /// <summary>
        /// Node to be inserted
        /// </summary>
        public LinkedListNode<char> Node { get; set; }

        public Insert(LinkedListNode<char> node)
        {
            Node = node;
        }

        public override void Revert(Document document)
        {
            document.Delete(Node);
        }
    }

    public sealed class LineStart : Change
    {
        public Line Line { get; }

        public LinkedListNode<char> From { get; }
        public LinkedListNode<char> To { get; }

        public LineStart(Line line, LinkedListNode<char> from, LinkedListNode<char> to)
        {
            Line = line;
            From = from;
            To = to;
        }

        public override void Revert(Document document)
        {
            Line.Start = From;
        }
    }

    public sealed class LineBreak : Change
    {
        public Line FirstLine { get; }
        public Line SecondLine { get; }

        public LineBreak(Line firstLine, Line secondLine)
        {
            FirstLine = firstLine;
            SecondLine = secondLine;
        }

        public override void Revert(Document document)
        {
            document.Lines.Remove(SecondLine);

            FirstLine.End = SecondLine.End;
        }
    }

    public class ChangeSession
    {
        private Stack<Change> Changes { get; } = new Stack<Change>();

        public void Add(Change change)
        {
            Changes.Push(change);
        }

        public void Rollback(Document document)
        {
            while(Changes.Count > 0)
            {
                var change = Changes.Pop();

                change.Revert(document);
            }
        }
    }


    public sealed class ChangesBuffer
    {
        private readonly Stack<ChangeSession> ChangeSessions = new Stack<ChangeSession>();

        private ChangeSession CurrentSession => ChangeSessions.Peek();

        private readonly Document document;

        private bool isRevertingChanges = false;

        public ChangesBuffer(Document document)
        {
            this.document = document;
        }

        public void Start()
        {
            ChangeSessions.Push(new ChangeSession());
        }

        public void Add(Change change)
        {
            if (isRevertingChanges)
                return;

            CurrentSession.Add(change);
        }

        public void Commit()
        {
            Console.WriteLine("commit");
        }

        public void RollBack()
        {
            if (ChangeSessions.Count == 0)
                return;

            var session = ChangeSessions.Pop();

            session.Rollback(document);
        }
    }


}

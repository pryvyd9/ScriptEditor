using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEditor
{
    internal abstract class Change
    {
        public abstract void Revert(Document document);
    }

    internal sealed class Delete : Change
    {
        /// <summary>
        /// Character to be deteled
        /// </summary>
        public char Character { get; }

        /// <summary>
        /// Position to delete from
        /// </summary>
        public int Position { get; }


        public Delete(char character, int position)
        {
            Character = character;
            Position = position;
        }

        public override void Revert(Document document)
        {
            document.Insert(Position, Character);
        }
    }

    internal sealed class Insert : Change
    {
        /// <summary>
        /// Position of inserted character.
        /// </summary>
        public int Position { get; }

        public Insert(int position)
        {
            Position = position;
        }

        public override void Revert(Document document)
        {
            document.Delete(Position);
        }
    }

    internal sealed class MoveLineStart : Change
    {
        public Line Line { get; }

        public int From { get; }

        public MoveLineStart(Line line, int from)
        {
            Line = line;
            From = from;
        }

        public override void Revert(Document document)
        {
            Line.Start = document.Content.NodeAt(From);
        }
    }

    internal sealed class MoveLineEnd : Change
    {
        public Line Line { get; }

        public int From { get; }

        public MoveLineEnd(Line line, int from)
        {
            Line = line;
            From = from;
        }

        public override void Revert(Document document)
        {
            Line.End = document.Content.NodeAt(From);
        }
    }

    internal sealed class LineBreak : Change
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

    internal sealed class LineMerge : Change
    {
        /// <summary>
        /// LIne to be expanded.
        /// </summary>
        public Line FirstLine { get; }

        /// <summary>
        /// LIne to be thrown away.
        /// </summary>
        public Line SecondLine { get; }

        public LineMerge(Line firstLine, Line secondLine)
        {
            FirstLine = firstLine;
            SecondLine = secondLine;
        }

        public override void Revert(Document document)
        {
            FirstLine.End = SecondLine.Start.Previous;

            document.Lines.Insert(document.Lines.IndexOf(FirstLine) + 1, SecondLine);
        }
    }



    internal class ChangeSession
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


    internal sealed class ChangesBuffer
    {
        private readonly Stack<ChangeSession> ChangeSessions = new Stack<ChangeSession>();

        private ChangeSession CurrentSession => ChangeSessions.Peek();

        private readonly Document document;

        public bool IsRevertingChanges { get; private set; } = false;

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
            if (IsRevertingChanges)
                return;

            CurrentSession.Add(change);
        }

        public void Commit()
        {

        }

        public void RollBack()
        {
            if (ChangeSessions.Count == 0)
                return;

            IsRevertingChanges = true;

            var session = ChangeSessions.Pop();

            session.Rollback(document);

            IsRevertingChanges = false;
        }
    }


}

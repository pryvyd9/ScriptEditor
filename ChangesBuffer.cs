using System;


namespace ScriptEditor
{
    public enum DocumentAction
    {
        InsertAt
    }

    public abstract class Change
    {
        public void Revert()
        {

        }
    }

    public sealed class Insert : Change
    {
        public int Position { get; set; }
    }

    public sealed class ChangesBuffer
    {
        public DocumentAction DocumentAction { get; set; }

        public void Start()
        {
            Console.WriteLine("start");
        }

        public void Add(Change change)
        {

        }

        public void Commit()
        {
            Console.WriteLine("commit");
        }

        public void RollBack()
        {

        }
    }


}

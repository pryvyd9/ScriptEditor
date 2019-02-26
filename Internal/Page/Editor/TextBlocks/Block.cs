using System.Collections.Generic;

namespace ScriptEditor
{
    public abstract class Block
    {
        public LinkedListNode<char> Start { get; set; }
        public LinkedListNode<char> End { get; set; }

        public string Text => Characters.ToStr();
        public IEnumerable<char> Characters => Start.GetRange(End);


        public override string ToString()
        {
            return Text;
        }
    }


}

using System.Collections.Generic;
using System.Linq;

namespace ScriptEditor
{
    public abstract class Block
    {
        //public string Tag { get; set; } = string.Empty;
        public string[] Tags { get; set; }

        public LinkedListNode<char> Start { get; set; }
        public LinkedListNode<char> End { get; set; }

        public string Text => Characters.ToStr();
        public IEnumerable<char> Characters => Start.GetRange(End);

        private static readonly List<string> decorationPriorityTable = new List<string>
        {
            "selection",
            "reserved-keywords",
            "lexical",
            "syntax",
            "semantic",
            string.Empty,
        };

        public static T[] DistinctDecorations<T>(IDocument document) where T : Block
        {
            var allBlocks = document.TextLookBlocks.OfType<T>().ToArray();

            var textLength = document.Text.Length;

            var positions = allBlocks
                .Select((n,i) => 
                    (index: i,
                     start: document.Content.IndexOf(n.Start), 
                     end:   document.Content.IndexOf(n.End  )));

            var markedPositions = new int?[textLength];

            // For each character decide on only textDecoration to apply;
            for (int i = 0; i < textLength; i++)
            {
                if (positions.Any(n => n.start <= i && n.end >= i))
                {
                    var blocksOnPosition = positions.Where(n=> n.start <= i && n.end >= i);

                    // Tag with highest priority
                    var tagOnPosition = decorationPriorityTable
                        .First(tag =>
                            blocksOnPosition.Any(n => allBlocks.ElementAt(n.index).Tags.Contains(tag))
                        );

                    var blockWithTag = blocksOnPosition
                        .Single(n => allBlocks.ElementAt(n.index).Tags.Contains(tagOnPosition));

                    markedPositions[i] = blockWithTag.index;
                }

               
            }

            return markedPositions
                .Select(n => n.HasValue ? allBlocks.ElementAt(n.Value) : null)
                .ToArray();

            //throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return Text;
        }


    }
}

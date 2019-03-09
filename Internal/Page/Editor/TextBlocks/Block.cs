using System.Collections.Generic;
using System.Linq;

namespace ScriptEditor
{
    public static class Tag
    {
        // selection
        public const int Selection = 0;

        // reserved-keywords
        public const int ReservedKeywords = 1;

        // lexical
        public const int Lexical = 2;

        // syntax
        public const int Syntax = 3;

        // semantic
        public const int Semantic = 4;

    }

    public abstract class Block
    {
        //public string Tag { get; set; } = string.Empty;
        public int[] Tags { get; set; }

        public LinkedListNode<char> Start { get; set; }
        public LinkedListNode<char> End { get; set; }

        public string Text => Characters.ToStr();
        public IEnumerable<char> Characters => Start.GetRange(End);
        public int Length => Start.GetRangeLength(End);




        private static readonly List<int> decorationPriorityTable = new List<int>
        {
            Tag.Selection,
            Tag.ReservedKeywords,
            Tag.Lexical,
            Tag.Syntax,
            Tag.Semantic,
        };

        public static T[] GetDecorations<T>(IDocument document) where T : Block
        {
            var allBlocks = document.TextLookBlocks.OfType<T>().ToArray();
            return allBlocks;
        }

        public static T[] DistinctDecorations<T>(IDocument document) where T : Block
        {
            var allBlocks = document.TextLookBlocks.OfType<T>().ToArray();

            var textLength = document.Length;

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

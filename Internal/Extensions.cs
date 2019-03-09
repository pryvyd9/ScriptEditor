using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptEditor
{
    public static class StringExtensions
    {
        public static string ToStr(this IEnumerable<char> collection)
        {
            return new string(collection.ToArray());
        }

        public static int[] IndexOfAll(this string str, string subString)
        {
            int length = subString.Length;

            var indices = new List<int>();

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == subString[0])
                {
                    int k = i;

                    i++;

                    for (int j = 1, matchCounter = 1; j < length; j++, i++)
                    {
                        if (str[i] == subString[j])
                        {
                            matchCounter++;
                        }
                        else
                        {
                            matchCounter = 0;
                        }

                        if (matchCounter == length)
                        {
                            indices.Add(k);
                            break;
                        }

                    }
                }
                
            }

            return indices.ToArray();
        }

        private static bool BeginsWith(string origin, int oindex, string substring, int sindex)
        {
            for (int i = 0; i < substring.Length - sindex; i++)
            {
                if (origin[oindex + i] != substring[sindex + i])
                {
                    return false;
                }
            }

            return true;
        }

        public static int[][] IndexOfAll(this string str, string[] subStrings)
        {
            var indices = subStrings.Select(n => new List<int>()).ToArray();

            for (int i = 0; i < str.Length; i++)
            {
                var substringsForMatching = subStrings
                    .Select((n, index) => (index, n))
                    .Where(n => n.n[0] == str[i]);


                if (substringsForMatching.Count() > 0)
                {
                    foreach (var (substringIndex, substring) in substringsForMatching)
                    {
                        if (BeginsWith(str, i + 1, substring, 1))
                        {
                            indices[substringIndex].Add(i);
                        }
                    }
                }
            }

            return indices.Select(n => n.ToArray()).ToArray();
        }
    }

    public static class LinkedListExtensions
    {
        public static void AddLastRange<T>(this LinkedList<T> list, IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                list.AddLast(item);
            }
        }

        public static LinkedListNode<T> GetAtOffset<T>(this LinkedListNode<T> start, int offset)
        {
            if (offset == 0)
            {
                return start;
            }

            LinkedListNode<T> temp = start;

            int step = offset < 0 ? -1 : 1;

            for (int i = 0; offset < 0 ? i > offset : i < offset; i += step)
            {
                temp = getNext();
            }

            return temp;

            LinkedListNode<T> getNext() => offset < 0 ? temp.Previous : temp.Next;
        }

        public static LinkedListNode<T> NodeAt<T>(this LinkedList<T> collection, int index)
        {
            if(index >= collection.Count || index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (index == 0)
                return collection.First;

            LinkedListNode<T> current = collection.First;

            for(int i = 0; i < index; i++)
            {
                current = current.Next;
            }

            return current;
        }

        public static IEnumerable<T> GetRange<T>(this LinkedListNode<T> start, LinkedListNode<T> end)
        {
            if (start == end)
            {
                return new List<T> { start.Value };
            }

            List<T> buffer = new List<T>();

            buffer.Add(start.Value);

            var current = start;

            while (current.Next != end)
            {
                if (current.Next == null)
                {
                    throw new Exception("end point does not precede start point.");
                }

                current = current.Next;

                buffer.Add(current.Value);
            }

            buffer.Add(end.Value);

            return buffer;
        }

        public static int GetRangeLength<T>(this LinkedListNode<T> start, LinkedListNode<T> end)
        {
            if (start == end)
            {
                return 1;
            }

            var current = start;

            int i = 1;

            while (current.Next != end)
            {
                if (current.Next == null)
                {
                    throw new Exception("end point does not precede start point.");
                }

                current = current.Next;
                i++;
            }

            return i;
        }

        public static IEnumerable<LinkedListNode<T>> GetRangeNodes<T>(this LinkedListNode<T> start, int offset)
        {
            if (offset == 0)
            {
                return new[] { start };
            }

            List<LinkedListNode<T>> buffer = new List<LinkedListNode<T>>();

            int step = offset < 0 ? -1 : 1;

            for (int i = 0; offset < 0 ? i > offset : i < offset; i += step)
            {
                buffer.Add(start.GetAtOffset(i));
            }

            return offset < 0 ? buffer.AsEnumerable().Reverse() : buffer;
        }

        public static int IndexOf<T>(this LinkedList<T> list, LinkedListNode<T> node)
        {
            LinkedListNode<T> current = list.First;

            for(int i = 0; current != null; i++, current = current.Next)
            {
                if(current == node)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int OffsetTo<T>(this LinkedListNode<T> node, LinkedListNode<T> elementToTheRight)
        {
            if(node == elementToTheRight)
            {
                return 0;
            }

            LinkedListNode<T> current = node;

            int i = 0;

            for (;  current.Next != elementToTheRight; i++)
            {
                if (current.Next == null)
                {
                    return -1;
                }

                current = current.Next;
            }

            return i;
        }
    }

}

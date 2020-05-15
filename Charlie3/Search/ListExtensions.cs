using System.Collections.Generic;

namespace Charlie.Search
{
    public static class ListExtensions
    {
        public static void MoveToFront<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) return;

            list.Remove(item);
            list.Insert(0, item);
        }
    }
}

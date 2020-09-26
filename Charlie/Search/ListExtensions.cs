using System.Collections.Generic;

namespace Charlie.Search
{
    public static class ListExtensions
    {
        public static List<T> MoveToFront<T>(this List<T> list, T item)
        {
            if (list.Contains(item))
            {
                list.Remove(item);
                list.Insert(0, item);
            }

            return list;
        }
    }
}

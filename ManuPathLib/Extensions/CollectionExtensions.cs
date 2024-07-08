using System;
using System.Collections.Generic;
using System.Text;

namespace ManuPath.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Each<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
            return collection;
        }
    }
}

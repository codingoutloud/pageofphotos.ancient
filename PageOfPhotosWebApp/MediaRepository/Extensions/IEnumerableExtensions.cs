using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRepository.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IList<T>> GroupAndSlice<T, TKey>(this IEnumerable<T> source,
            int groupSize, Func<T, TKey> sortComparer, Func<T, T, bool> groupComparer)
        {
            if (source == null || groupSize == 0)
            {
                yield break;
            }

            source = source.OrderBy(sortComparer);

            var currentGroup = new List<T>();
            var backlog = new Queue<IList<T>>();

            T firstInGroup = source.First();
            int count = 0;

            foreach (T item in source)
            {
                if (!groupComparer(firstInGroup, item))
                {
                    yield return currentGroup;

                    currentGroup = new List<T>();

                    firstInGroup = item;
                }

                currentGroup.Add(item);

                if (currentGroup.Count == groupSize)
                {
                    yield return currentGroup;

                    currentGroup = new List<T>();
                }

                count++;
            }

            if (count == 0)
            {
                yield break;
            }
            else
            {
                if (currentGroup.Any())
                {
                    yield return currentGroup;
                }
            }
        }
    }
}

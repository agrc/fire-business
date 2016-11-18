using System.Collections.Generic;
using System.Linq;

namespace fire_business_soe.Comparers
{
    public class MultiSetComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        public bool Equals(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                return second == null;

            if (second == null)
                return false;

            if (ReferenceEquals(first, second))
                return true;

            var firstCollection = first as ICollection<T>;
            var secondCollection = second as ICollection<T>;
            if (firstCollection == null || secondCollection == null)
            {
                return !HaveMismatchedElement(first, second);
            }

            if (firstCollection.Count != secondCollection.Count)
                return false;

            if (firstCollection.Count == 0)
                return true;

            return !HaveMismatchedElement(first, second);
        }

        public int GetHashCode(IEnumerable<T> enumerable)
        {
            return enumerable.OrderBy(x => x).Aggregate(17, (current, val) => current*23 + ((val == null) ? 42 : val.GetHashCode()));
        }

        private static bool HaveMismatchedElement(IEnumerable<T> first, IEnumerable<T> second)
        {
            int firstNullCount;
            int secondNullCount;

            var firstElementCounts = GetElementCounts(first, out firstNullCount);
            var secondElementCounts = GetElementCounts(second, out secondNullCount);

            if (firstNullCount != secondNullCount || firstElementCounts.Count != secondElementCounts.Count)
                return true;

            foreach (var kvp in firstElementCounts)
            {
                var firstElementCount = kvp.Value;
                int secondElementCount;
                secondElementCounts.TryGetValue(kvp.Key, out secondElementCount);

                if (firstElementCount != secondElementCount)
                    return true;
            }

            return false;
        }

        private static Dictionary<T, int> GetElementCounts(IEnumerable<T> enumerable, out int nullCount)
        {
            var dictionary = new Dictionary<T, int>();
            nullCount = 0;

            foreach (var element in enumerable)
            {
                if (element == null)
                {
                    nullCount++;
                }
                else
                {
                    int num;
                    dictionary.TryGetValue(element, out num);
                    num++;
                    dictionary[element] = num;
                }
            }

            return dictionary;
        }
    }
}
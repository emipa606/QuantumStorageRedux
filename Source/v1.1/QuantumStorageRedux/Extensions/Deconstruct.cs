using System.Collections.Generic;
using System.Linq;

namespace QuantumStorageRedux {
    internal static class DeconstructExtenstions {
        public static void Deconstruct<T>(this IEnumerable<IGrouping<bool, T>> group, out IEnumerable<T> pos, out IEnumerable<T> neg) {
            var query = group.ToLookup(x => x.Key);
            pos = query[true].SelectMany(x => x);
            neg = query[false].SelectMany(x => x);
        }
    }
}

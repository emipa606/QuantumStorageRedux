using System.Collections.Generic;
using System.Linq;

namespace QuantumStorageRedux;

internal static class DeconstructExtenstions
{
    public static void Deconstruct<T>(this IEnumerable<IGrouping<bool, T>> group, out IEnumerable<T> pos,
        out IEnumerable<T> neg)
    {
        var lookup = group.ToLookup(x => x.Key);
        pos = lookup[true].SelectMany(x => x);
        neg = lookup[false].SelectMany(x => x);
    }
}
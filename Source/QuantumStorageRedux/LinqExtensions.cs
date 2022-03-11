using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumStorageRedux;

internal static class LinqExtensions
{
    internal static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> seq)
    {
        return seq.Select((item, index) => (item, index));
    }

    internal static IEnumerable<(A, B)> Zip<A, B>(this IEnumerable<A> seqA, IEnumerable<B> seqB)
    {
        if (seqA == null)
        {
            throw new ArgumentNullException(nameof(seqA));
        }

        if (seqB == null)
        {
            throw new ArgumentNullException(nameof(seqB));
        }

        using var iteratorA = seqA.GetEnumerator();
        using var iteratorB = seqB.GetEnumerator();
        while (iteratorA.MoveNext() && iteratorB.MoveNext())
        {
            yield return (iteratorA.Current, iteratorB.Current);
        }
    }
}
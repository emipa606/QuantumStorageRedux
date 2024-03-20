using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QuantumStorageRedux;

public static class LogUtils
{
    public static string Display(Thing thing)
    {
        return $"{thing.def.defName} ({thing.stackCount})";
    }

    internal static string Display(QThing thing)
    {
        return $"{thing.def.defName} ({thing.stackCount})";
    }

    internal static string Display(IEnumerable<QThing> list)
    {
        if (!list.Any())
        {
            return "Empty";
        }

        return list.Aggregate(new StringBuilder(), delegate(StringBuilder acc, QThing thing)
        {
            acc.Append($"{Display(thing)} | ");
            return acc;
        }).ToString();
    }
}
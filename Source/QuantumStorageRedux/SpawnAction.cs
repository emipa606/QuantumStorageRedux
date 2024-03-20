using Verse;

namespace QuantumStorageRedux;

internal class SpawnAction(Map map, IntVec3 cell, QThing qthing) : IPerformable
{
    public void Perform()
    {
        Utils.DropSound(map, cell, qthing.def);
        Utils.Spawn(qthing.Make(), cell, map, Rot4.North);
    }

    public string Display()
    {
        var text = LogUtils.Display(qthing);
        return $"Spawn {text} on {cell}";
    }
}
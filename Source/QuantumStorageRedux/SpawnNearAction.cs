using Verse;

namespace QuantumStorageRedux;

internal class SpawnNearAction(Map map, IntVec3 cell, QThing qthing) : IPerformable
{
    public void Perform()
    {
        GenPlace.TryPlaceThing(qthing.Make(), cell, map, ThingPlaceMode.Near);
    }

    public string Display()
    {
        var text = LogUtils.Display(qthing);
        return $"SpawnNear {text} on {cell}";
    }
}
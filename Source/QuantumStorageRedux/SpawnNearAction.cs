using Verse;

namespace QuantumStorageRedux;

internal class SpawnNearAction : IPerformable
{
    private readonly IntVec3 cell;
    private readonly Map map;

    private readonly QThing qthing;

    public SpawnNearAction(Map map, IntVec3 cell, QThing qthing)
    {
        this.map = map;
        this.cell = cell;
        this.qthing = qthing;
    }

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
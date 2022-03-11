using Verse;

namespace QuantumStorageRedux;

internal class SpawnAction : IPerformable
{
    private readonly IntVec3 cell;
    private readonly Map map;

    private readonly QThing qthing;

    public SpawnAction(Map map, IntVec3 cell, QThing qthing)
    {
        this.map = map;
        this.cell = cell;
        this.qthing = qthing;
    }

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
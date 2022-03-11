using Verse;

namespace QuantumStorageRedux;

internal class DeSpawnAction : IPerformable
{
    private readonly Thing thing;

    public DeSpawnAction(Thing thing)
    {
        this.thing = thing;
    }

    public void Perform()
    {
        if (!thing.Spawned)
        {
            return;
        }

        Utils.ThrowDustPuff(thing.Map, thing.Position);
        thing.DeSpawn();
    }

    public string Display()
    {
        return $"DeSpawn {LogUtils.Display(thing)} on {thing.Position}";
    }
}
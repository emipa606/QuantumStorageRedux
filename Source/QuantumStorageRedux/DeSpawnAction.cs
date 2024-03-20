using Verse;

namespace QuantumStorageRedux;

internal class DeSpawnAction(Thing thing) : IPerformable
{
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
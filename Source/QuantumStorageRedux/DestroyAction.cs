using Verse;

namespace QuantumStorageRedux;

internal class DestroyAction(Thing thing) : IPerformable
{
    public void Perform()
    {
        if (thing.Destroyed)
        {
            return;
        }

        Utils.ThrowDustPuff(thing.Map, thing.Position);
        thing.Destroy();
    }

    public string Display()
    {
        return $"Destroy {LogUtils.Display(thing)} on {thing.Position}";
    }
}
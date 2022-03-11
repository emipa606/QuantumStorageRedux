using Verse;

namespace QuantumStorageRedux;

internal class DestroyAction : IPerformable
{
    private readonly Thing thing;

    public DestroyAction(Thing thing)
    {
        this.thing = thing;
    }

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
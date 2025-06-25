using System.Text;
using RimWorld;
using Verse;

namespace QuantumStorageRedux;

internal class CompQSRRelay : ThingComp
{
    private QNetwork network;

    private CompPowerTrader powerTrader;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        powerTrader = parent.TryGetComp<CompPowerTrader>();
        network = QNetworkManager.Get(parent.Map);
        network.RegisterInput(powerTrader, GenAdj.CellsAdjacent8Way(parent));
        network.RegisterRelay(powerTrader, GenAdj.CellsOccupiedBy(parent));
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        network.UnregisterInput(powerTrader);
        network.UnregisterRelay(powerTrader);
    }

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("QSRComponents_GlobalNetwork".Translate());
        stringBuilder.Append("\n");
        stringBuilder.Append("QSRComponents_MaxStacks".Translate());
        stringBuilder.Append(" ");
        stringBuilder.Append(network.CellCapacity().ToString("F0"));
        return stringBuilder.ToString();
    }

    public override void CompTick()
    {
        relayTick(100);
    }

    private void relayTick(int tickAmount)
    {
        if (Find.TickManager.TicksGame % tickAmount == 0 && Utils.PoweredOn(powerTrader))
        {
            network.Mantain(parent.Map, Find.TickManager.TicksGame);
        }
    }
}
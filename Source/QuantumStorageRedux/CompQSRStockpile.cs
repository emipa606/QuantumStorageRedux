using System.Text;
using RimWorld;
using Verse;

namespace QuantumStorageRedux;

internal class CompQSRStockpile : ThingComp
{
    private QNetwork network;

    private CompPowerTrader powerTrader;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        powerTrader = parent.TryGetComp<CompPowerTrader>();
        network = new QNetwork();
        network.RegisterInput(powerTrader, GenAdj.CellsAdjacent8Way(parent));
        network.RegisterStorage(powerTrader, GenAdj.CellsOccupiedBy(parent));
        QNetworkManager.RegisterLocalNetwork(network);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        QNetworkManager.UnregisterLocalNetwork(network);
    }

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("QSRComponents_LocalNetwork".Translate());
        stringBuilder.Append("\n");
        stringBuilder.Append("QSRComponents_MaxStacks".Translate());
        stringBuilder.Append(" ");
        stringBuilder.Append(network.CellCapacity().ToString("F0"));
        return stringBuilder.ToString();
    }

    public override void CompTick()
    {
        stockpileTick(100);
    }

    private void stockpileTick(int tickAmount)
    {
        if (Find.TickManager.TicksGame % tickAmount == 0 && Utils.PoweredOn(powerTrader))
        {
            network.Mantain(parent.Map, Find.TickManager.TicksGame);
        }
    }
}
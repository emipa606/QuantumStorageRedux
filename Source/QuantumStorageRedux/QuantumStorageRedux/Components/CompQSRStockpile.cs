using System.Text;
using RimWorld;
using Verse;

namespace QuantumStorageRedux {
    internal class CompQSRStockpile : ThingComp {
        private QNetwork network;
        private CompPowerTrader powerTrader;

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);

            this.powerTrader = this.parent.TryGetComp<CompPowerTrader>();

            this.network = new QNetwork();
            this.network.RegisterInput(this.powerTrader, GenAdj.CellsAdjacent8Way(this.parent));
            this.network.RegisterStorage(this.powerTrader, GenAdj.CellsOccupiedBy(this.parent));

            QNetworkManager.RegisterLocalNetwork(this.network);
        }

        public override void PostDeSpawn(Map map) {
            base.PostDeSpawn(map);

            QNetworkManager.UnregisterLocalNetwork(this.network);
        }

        public override string CompInspectStringExtra() {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(("QSRComponents_LocalNetwork").Translate());
            stringBuilder.Append("\n");

            stringBuilder.Append("QSRComponents_MaxStacks".Translate());
            stringBuilder.Append(" ");
            stringBuilder.Append(this.network.CellCapacity().ToString("F0"));

            return stringBuilder.ToString();
        }

        public override void CompTick() {
            this.StockpileTick(100);
        }

        private void StockpileTick(int tickAmount) {
            if (Find.TickManager.TicksGame % tickAmount != 0) {
                return;
            }

            if (!Utils.PoweredOn(this.powerTrader)) {
                return;
            }

            this.network.Mantain(this.parent.Map, Find.TickManager.TicksGame);
        }
    }
}

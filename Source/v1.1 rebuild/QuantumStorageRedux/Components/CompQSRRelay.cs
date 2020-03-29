using System.Text;
using RimWorld;
using Verse;

namespace QuantumStorageRedux {
    internal class CompQSRRelay : ThingComp {
        private QNetwork network;
        private CompPowerTrader powerTrader;

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);

            this.powerTrader = this.parent.TryGetComp<CompPowerTrader>();

            this.network = QNetworkManager.Get(this.parent.Map);
            this.network.RegisterInput(this.powerTrader, GenAdj.CellsAdjacent8Way(this.parent));
            this.network.RegisterRelay(this.powerTrader, GenAdj.CellsOccupiedBy(this.parent));
        }

        public override void PostDeSpawn(Map map) {
            base.PostDeSpawn(map);

            this.network.UnregisterInput(this.powerTrader);
            this.network.UnregisterRelay(this.powerTrader);
        }

        public override string CompInspectStringExtra() {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(("QSRComponents_GlobalNetwork").Translate());
            stringBuilder.Append("\n");

            stringBuilder.Append("QSRComponents_MaxStacks".Translate());
            stringBuilder.Append(" ");
            stringBuilder.Append(this.network.CellCapacity().ToString("F0"));

            return stringBuilder.ToString();
        }

        public override void CompTick() {
            this.RelayTick(100);
        }

        private void RelayTick(int tickAmount) {
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

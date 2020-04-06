using Verse;
using RimWorld;


namespace QuantumStorageRedux {
    internal class DestroyAction : IPerformable {
        private readonly Thing thing;

        public DestroyAction(Thing thing) {
            this.thing = thing;
        }

        public void Perform() {
            if (this.thing.Destroyed) {
                return;
            }

            Utils.ThrowDustPuff(this.thing.Map, this.thing.Position);
            this.thing.Destroy();
        }

        public string Display() {
            return "Destroy " + LogUtils.Display(this.thing) + " on " + this.thing.Position;
        }
    }

    internal class Quality : IPerformable {
        private readonly Thing thing;
        private readonly Thing originalThing;
        private QualityCategory quality;
        private bool qualitySet;
        public static QualityCategory fetchQuality(Thing thing) {
            QualityCategory workingValue = default(QualityCategory);

            QualityUtility.TryGetQuality(thing, out workingValue);

            thing.TryGetComp<CompQuality>()?.SetQuality(workingValue, ArtGenerationContext.Colony);

            QLog.Message(QLog.Ctx.Thing, "Quality.fetchQuality: Attempting to set Quality: ");
            QLog.Message(QLog.Ctx.Thing, workingValue.GetLabel());



            return workingValue;
        }

        public static bool checkQuality(Thing thing) {
            QualityCategory workingValue = default(QualityCategory);
            if (!QualityUtility.TryGetQuality(thing, out workingValue)) {
                /*QLog.
                 * ssage(QLog.Ctx.Thing, "No Quality Assigned");*/
                return false;
            }

            QLog.Message(QLog.Ctx.Thing, "Quality.checkQuality: Quality located: ");
            QLog.Message(QLog.Ctx.Thing, workingValue.GetLabel());
            return true;
        }

        private bool hasQuality() {
            QualityCategory workingValue = default(QualityCategory);

            if (!QualityUtility.TryGetQuality(this.thing, out workingValue)) {
                QLog.Message(QLog.Ctx.Thing, "Quality.hasQuality: No Quality Assigned");
                return false;
            }

            QLog.Message(QLog.Ctx.Thing, "Quality.hasQuality: Quality located: ");
            QLog.Message(QLog.Ctx.Thing, workingValue.GetLabel());
            return true;
        }

        public void setQuality() {
            QualityCategory workingValue = default(QualityCategory);

            QualityUtility.TryGetQuality(this.thing, out workingValue);

            this.thing.TryGetComp<CompQuality>()?.SetQuality(workingValue, ArtGenerationContext.Colony);

            QLog.Message(QLog.Ctx.Thing, "Quality.setQuality: Attempting to set Quality: ");
            QLog.Message(QLog.Ctx.Thing, workingValue.GetLabel());

            QLog.Message(QLog.Ctx.Thing, "Quality.setQuality: Quality set to: ");
            QLog.Message(QLog.Ctx.Thing, this.thing.TryGetComp<CompQuality>()?.Quality.GetLabel());

            this.quality = workingValue;
        }
        
        public Quality(Thing thing) {
            this.thing = thing;
            this.qualitySet = false;

            QLog.Message(QLog.Ctx.Thing, "Quality Constructor: Attempting to get Quality of thing: ");
            QLog.Message(QLog.Ctx.Thing, this.thing.Label);          
        }

        public Thing getThing () {
            QLog.Message(QLog.Ctx.Thing, "Quality.getThing: Attempting to get Quality of thing: ");
            QLog.Message(QLog.Ctx.Thing, this.thing.Label);
            QLog.Message(QLog.Ctx.Thing, this.thing.TryGetComp<CompQuality>()?.Quality.GetLabel());

            if (qualitySet) {
                return this.thing;
            }

            this.Perform();

            return this.thing;
        }

        public QualityCategory getQuality() {
            if (qualitySet) {
                return this.quality;
            }

            this.Perform();
            return this.quality;
        }

        public string Display() {
            return "Quality.Display: Attempting to get Quality of thing: " + this.thing.Label;
        }

        public void Perform() {
            if (!this.hasQuality()) {
                this.qualitySet = true;
                return;
            }
            this.setQuality();
            this.qualitySet = true;
        }
    }

    internal class DeSpawnAction : IPerformable {
        private readonly Thing thing;

        public DeSpawnAction(Thing thing) {
            this.thing = thing;
        }

        public void Perform() {
            if (!this.thing.Spawned) {
                return;
            }

            Utils.ThrowDustPuff(this.thing.Map, this.thing.Position);
            this.thing.DeSpawn();
        }

        public string Display() {
            return "DeSpawn " + LogUtils.Display(this.thing) + " on " + this.thing.Position;
        }
    }



    internal class SpawnAction : IPerformable {
        private readonly Map map;
        private readonly IntVec3 cell;
        private readonly QThing qthing;

        public SpawnAction(Map map, IntVec3 cell, QThing qthing) {
            this.map = map;
            this.cell = cell;
            this.qthing = qthing;
        }

        public void Perform() {
            /*Silent Stockpiling*/
            /*Utils.DropSound(this.map, this.cell, this.qthing.def);*/
            Utils.Spawn(this.qthing.Make(), this.cell, this.map, Rot4.North);
        }

        public string Display() {
            return "Spawn " + LogUtils.Display(this.qthing) + " on " + this.cell;
        }
    }

    internal class SpawnNearAction : IPerformable {
        private readonly Map map;
        private readonly IntVec3 cell;
        private readonly QThing qthing;

        public SpawnNearAction(Map map, IntVec3 cell, QThing qthing) {
            this.map = map;
            this.cell = cell;
            this.qthing = qthing;
        }

        public void Perform() {
            /*Silence Drop Sound*/
            /*Utils.DropSound(this.map, this.cell, this.qthing.def);*/

            /*GenSpawn, instead of TryPlace*/
            /*GenPlace.TryPlaceThing(this.qthing.Make(), this.cell, this.map, ThingPlaceMode.Near);*/
            GenSpawn.Spawn(this.qthing.Make().def, this.cell, this.map, 0);
        }
        public string Display() {
            return "SpawnNear " + LogUtils.Display(this.qthing) + " on " + this.cell;
        }

    }
}

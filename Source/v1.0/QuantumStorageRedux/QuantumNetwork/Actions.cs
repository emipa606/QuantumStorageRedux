using Verse;

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
            Utils.DropSound(this.map, this.cell, this.qthing.def);
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
            Utils.DropSound(this.map, this.cell, this.qthing.def);
            GenPlace.TryPlaceThing(this.qthing.Make(), this.cell, this.map, ThingPlaceMode.Near);
        }
        public string Display() {
            return "SpawnNear " + LogUtils.Display(this.qthing) + " on " + this.cell;
        }

    }
}

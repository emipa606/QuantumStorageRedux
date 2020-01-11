using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace QuantumStorageRedux {
    internal class QThing {
        public Thing thing;
        public ThingDef def;
        public ThingDef stuff;
        public int stackCount;
        public int hitPoints;
        public List<IPerformable> actions;
        public Source source;

        private readonly List<QThing> absorbed;

        public QThing(Thing thing, Source source) {
            this.source = source;
            this.thing = thing;
            this.def = thing.def;
            this.stuff = thing.Stuff;
            this.actions = new List<IPerformable>();

            if (this.source != Source.Merge) {
                this.stackCount = thing.stackCount;
                this.hitPoints = thing.HitPoints;
            }

            if (this.source == Source.Merge) {
                this.absorbed = new List<QThing>();
            }
        }

        public QThing Absorb(QThing qthing, int amount) {
            if (this.thing.def.useHitPoints) {
                this.hitPoints = Mathf.CeilToInt(
                    (this.hitPoints * this.stackCount + qthing.thing.HitPoints * amount) /
                    (float)(this.stackCount + amount)
                );
            }

            this.stackCount += amount;
            this.absorbed.Add(qthing);
            this.source = this.absorbed.Count() > 1 ? Source.Composite : Source.Merge;

            return this;
        }

        public bool AllowedByStorage(StorageSettings storageSettings) {
            if (storageSettings == null) {
                return false;
            }

            switch (this.source) {
                case Source.Composite:
                    if (!Utils.Allows(storageSettings, this)) {
                        return false;
                    }

                    break;
                default:
                    if (!storageSettings.AllowedToAccept(this.thing)) {
                        return false;
                    }

                    if (storageSettings.Priority < Utils.Priority(this.thing.Map, this.thing.Position)) {
                        return false;
                    }

                    break;
            }

            return true;
        }

        public Thing Make() {
            if (this.thing is Corpse corpseThing) {
                var corpse = ThingMaker.MakeThing(corpseThing.InnerPawn.RaceProps.corpseDef) as Corpse;
                corpseThing.GetDirectlyHeldThings().TryTransferToContainer(corpseThing.InnerPawn, corpse.GetDirectlyHeldThings());

                return corpse;
            }

            var thing = ThingMaker.MakeThing(this.def, this.stuff);
            thing.HitPoints = this.hitPoints;
            thing.stackCount = this.stackCount;

            return thing;
        }

        public QThing Insert(Map map, IntVec3 cell) {
            switch (this.source) {
                case Source.Composite:
                    this.actions.Add(new SpawnAction(map, cell, this));
                    this.actions = this.actions.
                        Concat(this.absorbed.
                            Select(qthing => new DestroyAction(qthing.thing) as IPerformable)
                        ).
                        ToList();

                    break;
                default:
                    this.actions.Add(new SpawnAction(map, cell, this));
                    this.actions.Add(new DeSpawnAction(this.thing));

                    break;
            }

            return this;
        }

        public QThing MoveOut(Map map, IntVec3 cell) {
            if (this.source == Source.Input) {
                return this;
            }

            if (this.source == Source.Merge || this.source == Source.Composite) {
                if (this.absorbed.All(qthing => qthing.source == Source.Input)) {
                    return this;
                }
            }

            switch (this.source) {
                case Source.Composite:
                    this.actions.Add(new SpawnNearAction(map, cell, this));
                    this.actions = this.actions.
                        Concat(this.absorbed.
                            Select(qthing => new DestroyAction(qthing.thing) as IPerformable)
                        ).
                        ToList();

                    break;
                default:
                    this.actions.Add(new SpawnNearAction(map, cell, this));
                    this.actions.Add(new DeSpawnAction(this.thing));

                    break;
            }

            return this;
        }

        public enum Source {
            Input,
            Merge,
            Composite,
            Storage,
        }
    }
}

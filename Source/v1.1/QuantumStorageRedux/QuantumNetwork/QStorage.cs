using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace QuantumStorageRedux {
    internal class QStorage {
        private Map map;
        private List<IntVec3> cells;
        private List<QCell> storage;

        private readonly Kind kind;
        private readonly int cellCapacity;
        private readonly List<QThing> excluded;
        private readonly HashSet<ThingDef> existingDefs;
        private readonly Dictionary<ThingDef, List<(int cellIndex, int thingIndex)>> defsIndex;

        public QStorage(Kind kind, int cellCapacity) {
            this.kind = kind;
            this.cellCapacity = cellCapacity;
            this.excluded = new List<QThing>();

            switch (this.kind) {
                case Kind.Relay:
                    this.existingDefs = new HashSet<ThingDef>();
                    break;
                case Kind.Storage:
                    this.defsIndex = new Dictionary<ThingDef, List<(int, int)>>();
                    break;
            }
        }

        public QStorage FromCells(Map map, List<IntVec3> cells, Func<StorageSettings, Thing, bool> isAllowed) {
            this.map = map;
            this.cells = cells;

            this.storage = this.cells.
                Select((cell, cellIndex) => {
                    var storageSettings = Utils.GetStorageSettings(this.map, cell);
                    var qthings = new QThing[this.cellCapacity];

                    foreach (var (thing, thingIndex) in Utils.GetItemList(this.map, cell).WithIndex()) {
                        if (thingIndex >= this.cellCapacity) {
                            break;
                        }

                        if (!isAllowed(storageSettings, thing)) {
                            this.excluded.Add(new QThing(thing, QThing.Source.Storage));
                            continue;
                        }

                        switch (this.kind) {
                            case Kind.Relay:
                                this.existingDefs.Add(thing.def);
                                break;
                            case Kind.Storage:
                                this.IndexDef(thing.def, cellIndex, thingIndex);
                                break;
                        }

                        qthings[thingIndex] = new QThing(thing, QThing.Source.Storage);
                    }

                    return new QCell(cell, storageSettings, qthings);
                }).
                ToList();

            return this;
        }

        public void InsertToRelay(List<QThing> insertionQueue, QStorage reqStorage) {
            foreach (var qcell in this.storage) {
                if (qcell.qthings[0] != null) {
                    QLog.Warning(QLog.Ctx.Relay, "Skipping " + qcell.cell + "[0]: Already occupied");
                    continue;
                }

                foreach (var def in qcell.storageSettings.filter.AllowedThingDefs) {
                    QLog.Message(QLog.Ctx.Relay, "Trying to request " + def.defName + " from storage");

                    if (this.existingDefs.Contains(def)) {
                        QLog.Warning(QLog.Ctx.Relay, "Skipping " + qcell.cell + "[0]: " + def.defName + " Already in relay");
                        continue;
                    }

                    var requested = reqStorage.RequestWithDef(def);
                    if (requested == null) {
                        QLog.Warning(QLog.Ctx.Relay, "Skipping " + qcell.cell + "[0]: " + def.defName + " Not found");
                        continue;
                    }

                    if (!requested.AllowedByStorage(qcell.storageSettings)) {
                        QLog.Warning(QLog.Ctx.Relay, "Skipping " + qcell.cell + "[0]: Not allowed by storage settings");
                        continue;
                    }

                    QLog.Message(QLog.Ctx.Relay, "Inserted " + LogUtils.Display(requested) + " at " + qcell.cell + "[0]");
                    this.existingDefs.Add(def);
                    qcell.qthings[0] = requested.Insert(this.map, qcell.cell);
                    break;
                }

                if (qcell.qthings[0] != null) {
                    continue;
                }

                var insertedIndex = insertionQueue.FindIndex(x => {
                    QLog.Message(QLog.Ctx.Relay, "Trying to insert " + LogUtils.Display(x));

                    if (this.existingDefs.Contains(x.def)) {
                        QLog.Warning(QLog.Ctx.Relay, "Skipping " + qcell.cell + "[0]: " + x.def.defName + " Already in relay");
                        return false;
                    }

                    if (!x.AllowedByStorage(qcell.storageSettings)) {
                        QLog.Warning(QLog.Ctx.Relay, "Skipping " + qcell.cell + "[0]: Not allowed by storage settings");
                        return false;
                    }

                    QLog.Message(QLog.Ctx.Relay, "Inserted " + LogUtils.Display(x) + " at " + qcell.cell + "[0]");
                    this.existingDefs.Add(x.def);
                    qcell.qthings[0] = x.Insert(this.map, qcell.cell);
                    return true;
                });

                if (insertedIndex > -1) {
                    insertionQueue.RemoveAt(insertedIndex);
                    continue;
                }
            }
        }

        public void Insert(List<QThing> insertionQueue) {
            if (insertionQueue.Count() == 0) {
                return;
            }

            var orderedStorage = this.storage.
                OrderByDescending(qcell => qcell.storageSettings != null ? qcell.storageSettings.Priority : StoragePriority.Unstored);

            foreach (var qcell in orderedStorage) {
                if (insertionQueue.Count() == 0) {
                    return;
                }

                if (qcell.storageSettings == null) {
                    QLog.Warning(QLog.Ctx.Insertion, "Skipping " + qcell.cell + ": No storage settings");
                    continue;
                }

                foreach (var (qthing, qindex) in qcell.qthings.WithIndex()) {
                    if (insertionQueue.Count() == 0) {
                        return;
                    }

                    if (qcell.qthings.Count(x => x != null) >= this.cellCapacity) {
                        QLog.Warning(QLog.Ctx.Insertion, "Skipping " + qcell.cell + "[" + qindex + "]: Cell is full");
                        continue;
                    }

                    if (qthing != null) {
                        QLog.Warning(QLog.Ctx.Insertion, "Skipping " + qcell.cell + "[" + qindex + "]: Already occupied");
                        continue;
                    }

                    var insertedIndex = insertionQueue.FindIndex(x => {
                        QLog.Message(QLog.Ctx.Insertion, "Trying to insert " + LogUtils.Display(x));

                        if (!x.AllowedByStorage(qcell.storageSettings)) {
                            QLog.Warning(QLog.Ctx.Insertion, "Skipping " + qcell.cell + "[" + qindex + "]: Not allowed by storage settings");
                            return false;
                        }

                        QLog.Message(QLog.Ctx.Insertion, "Inserted " + LogUtils.Display(x) + " at " + qcell.cell + "[" + qindex + "]");
                        qcell.qthings[qindex] = x.Insert(this.map, qcell.cell);
                        return true;
                    });

                    if (insertedIndex > -1) {
                        insertionQueue.RemoveAt(insertedIndex);
                    }
                }
            }
        }

        public List<IPerformable> Diff() {
            return this.cells.
                Zip(this.storage).
                Aggregate(new List<IPerformable>(), (actions, x) => {
                    var (cell, qcell) = x;
                    var things = Utils.GetItemList(this.map, cell);

                    for (var i = 0; i < this.cellCapacity; i++) {
                        var thing = things.ElementAtOrDefault(i);
                        var qthing = qcell.qthings[i];

                        if (qthing == null) {
                            continue;
                        }

                        if (
                            thing == null ||
                            qthing.source == QThing.Source.Composite ||
                            thing.def != qthing.def ||
                            thing.stackCount != qthing.stackCount ||
                            thing.HitPoints != qthing.hitPoints
                        ) {
                            actions = actions.Concat(qthing.actions).ToList();
                        }
                    }

                    return actions;
                });
        }

        public List<QThing> ExcludedThings() {
            return this.excluded;
        }

        public QThing RequestWithDef(ThingDef def) {
            if (!this.defsIndex.ContainsKey(def)) {
                return null;
            }

            var (cellIndex, thingIndex) = this.defsIndex[def].Last();
            var qthing = this.storage[cellIndex].qthings[thingIndex];

            this.defsIndex[def].RemoveLast();
            this.storage[cellIndex].qthings[thingIndex] = null;

            return qthing;
        }

        public string Display() {
            return this.storage.
                Aggregate(new StringBuilder(this + "#" + this.GetHashCode() + "\n"), (acc, qcell) => {
                    acc.Append(qcell.cell + ": | ");

                    qcell.qthings.
                        Select(qthing => qthing == null ? "null | " : LogUtils.Display(qthing) + " | ").
                        ToList().
                        ForEach(row => acc.Append(row));


                    acc.Append("\n");

                    return acc;
                }).
                ToString();
        }

        private void IndexDef(ThingDef def, int cellIndex, int thingIndex) {
            if (!this.defsIndex.ContainsKey(def)) {
                this.defsIndex.Add(def, new List<(int, int)>());
            }

            this.defsIndex[def].Add((cellIndex, thingIndex));
        }

        public enum Kind {
            Relay,
            Storage,
        }
    }
}

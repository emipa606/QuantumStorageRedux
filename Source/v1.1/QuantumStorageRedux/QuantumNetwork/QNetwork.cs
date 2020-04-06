using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using Verse;


namespace QuantumStorageRedux {
    internal class QNetwork {
        private readonly Dictionary<CompPowerTrader, IEnumerable<IntVec3>> input;
        private readonly Dictionary<CompPowerTrader, IEnumerable<IntVec3>> storage;
        private readonly Dictionary<CompPowerTrader, IEnumerable<IntVec3>> relays;

        private int currentTick;
        private int cellCapacity;
        private bool isFull;

        private const int BASE_CAPACITY = 2;
        private const int RESEARCH_STEP_INCREMENT = 2;

        public QNetwork() {
            this.input = new Dictionary<CompPowerTrader, IEnumerable<IntVec3>>();
            this.storage = new Dictionary<CompPowerTrader, IEnumerable<IntVec3>>();
            this.relays = new Dictionary<CompPowerTrader, IEnumerable<IntVec3>>();

            this.currentTick = 0;
            this.cellCapacity = this.CalculateCellCapacity();
            this.isFull = false;
        }

        public int CellCapacity() {
            return this.cellCapacity;
        }

        public bool IsFull() {
            return this.isFull;
        }

        public void RegisterInput(CompPowerTrader powerTrader, IEnumerable<IntVec3> cells) {
            this.input.Add(powerTrader, cells);
        }

        public void UnregisterInput(CompPowerTrader powerTrader) {
            this.input.Remove(powerTrader);
        }

        public void RegisterStorage(CompPowerTrader powerTrader, IEnumerable<IntVec3> cells) {
            this.storage.Add(powerTrader, cells);
        }

        public void UnregisterStorage(CompPowerTrader powerTrader) {
            this.storage.Remove(powerTrader);
        }

        public void RegisterRelay(CompPowerTrader powerTrader, IEnumerable<IntVec3> cells) {
            this.relays.Add(powerTrader, cells);
        }

        public void UnregisterRelay(CompPowerTrader powerTrader) {
            this.relays.Remove(powerTrader);
        }

        public void Mantain(Map map, int tick) {
            if (this.currentTick == tick) {
                return;
            }

#if DEBUG
            var stopwatch = new Stopwatch();
            stopwatch.Start();
#endif

            this.currentTick = tick;
            this.cellCapacity = this.CalculateCellCapacity();

            var input = new HashSet<IntVec3>(this.input.
                Where(x => Utils.PoweredOn(x.Key)).
                SelectMany(x => x.Value));

            var storage = this.storage.
                Where(x => Utils.PoweredOn(x.Key)).
                SelectMany(x => x.Value).
                ToList();

            var relays = this.relays.
                Where(x => Utils.PoweredOn(x.Key)).
                Select(x => x.Value.ToList()).
                ToList();

            QLog.Message("Mantain " + this + "#" + this.GetHashCode() + " on " + this.currentTick + " tick");
            QLog.Message(input.Count() + " input cells");
            QLog.Message(storage.Count() + " storage cells");
            QLog.Message(relays.Count() + " relays");
            QLog.Message(this.cellCapacity + " stacks per cell");

            var qRelayStorages = relays.
                Select(cells => {
                    return new QStorage(QStorage.Kind.Relay, 1).FromCells(map, cells, (storageSettings, thing) => {
                        if (!storageSettings.AllowedToAccept(thing)) {
                            return false;
                        }

                        if (thing.stackCount < thing.def.stackLimit) {
                            return false;
                        }

                        return true;
                    });
                }).
                ToList();

            var qstorage = new QStorage(QStorage.Kind.Storage, this.cellCapacity).FromCells(map, storage, (_, thing) => {
                if (thing.stackCount < thing.def.stackLimit) {
                    return false;
                }

                return true;
            });

            QLog.Message("Quantum Storage");
            QLog.Message("Excluded things: " + LogUtils.Display(qstorage.ExcludedThings()));
            QLog.Message(qstorage.Display());

            var inputThings = input.
                Where(cell => Utils.Priority(map, cell) != StoragePriority.Unstored).
                SelectMany(cell => Utils.GetItemList(map, cell)).
                Select(thing => new QThing(thing, QThing.Source.Input));

            var (partialStacks, fullStacks) = qRelayStorages.
                SelectMany(qrelay => qrelay.ExcludedThings()).
                Concat(qstorage.ExcludedThings()).
                Concat(inputThings).
                GroupBy(thing => thing.stackCount < thing.def.stackLimit);

            QLog.Message("Input");
            QLog.Message("Full stacks: " + LogUtils.Display(fullStacks));
            QLog.Message("Partial stacks: " + LogUtils.Display(partialStacks));

            var mergedStacks = this.MergePartialStacks(partialStacks);
            QLog.Message("Merged stacks: " + LogUtils.Display(mergedStacks));

            var insertionQueue = fullStacks.
                Concat(mergedStacks).
                ToList();

            QLog.Message("Insertion queue: " + LogUtils.Display(insertionQueue));

            var relaysActions = qRelayStorages.
                SelectMany(relay => {
                    relay.InsertToRelay(insertionQueue, qstorage);
                    return relay.Diff();
                })
                .ToList();

            qstorage.Insert(insertionQueue);
            var storageActions = qstorage.Diff();

            QLog.Message("Leftovers: " + LogUtils.Display(insertionQueue));

            this.isFull = insertionQueue.Count() > 0;

            var allActions = insertionQueue.
                SelectMany(qthing => qthing.MoveOut(map, input.RandomElement()).actions).
                Concat(relaysActions).
                Concat(storageActions);

            QLog.Message("Actions");
            foreach (var action in allActions) {
                QLog.Message(action.Display());
                action.Perform();
            }

#if DEBUG
            stopwatch.Stop();
            Log.Message("Elapsed time " + stopwatch.ElapsedMilliseconds + " ms");
# endif

        }

        private int CalculateCellCapacity() {
            var capacity = BASE_CAPACITY;

            if (DefDatabase<ResearchProjectDef>.GetNamed("ResearchProject_QSRCapacityUpgrade_1").IsFinished) {
                capacity += RESEARCH_STEP_INCREMENT;
            }

            if (DefDatabase<ResearchProjectDef>.GetNamed("ResearchProject_QSRCapacityUpgrade_2").IsFinished) {
                capacity += RESEARCH_STEP_INCREMENT;
            }

            if (DefDatabase<ResearchProjectDef>.GetNamed("ResearchProject_QSRCapacityUpgrade_3").IsFinished) {
                capacity += RESEARCH_STEP_INCREMENT;
            }

            if (DefDatabase<ResearchProjectDef>.GetNamed("ResearchProject_QSRCapacityUpgrade_4").IsFinished) {
                capacity += RESEARCH_STEP_INCREMENT;
            }

            return capacity;
        }

        private List<QThing> MergePartialStacks(IEnumerable<QThing> partialStacks) {
            return partialStacks
                .GroupBy(x => x.def.shortHash)
                .Aggregate(new List<QThing>(), (qthings, defStacks) => {
                    var lastStack = defStacks.Aggregate(new QThing(defStacks.First().thing, QThing.Source.Merge), (composite, qthing) => {
                        var amount = qthing.stackCount;
                        var remainder = composite.stackCount + amount - qthing.def.stackLimit;
                        if (remainder > 0) {
                            amount -= remainder;
                        }

                        composite.Absorb(qthing, amount);

                        if (composite.stackCount == composite.def.stackLimit) {
                            qthings.Add(composite);
                            return new QThing(qthing.thing, QThing.Source.Merge).Absorb(qthing, remainder);
                        }

                        return composite;
                    });

                    if (lastStack.stackCount > 0) {
                        qthings.Add(lastStack);
                    }

                    return qthings;
                });
        }
    }
}

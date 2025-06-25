using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace QuantumStorageRedux;

internal class QNetwork
{
    private const int BASE_CAPACITY = 2;

    private const int RESEARCH_STEP_INCREMENT = 2;
    private readonly Dictionary<CompPowerTrader, IEnumerable<IntVec3>> input;

    private readonly Dictionary<CompPowerTrader, IEnumerable<IntVec3>> relays;

    private readonly Dictionary<CompPowerTrader, IEnumerable<IntVec3>> storage;

    private int cellCapacity;

    private int currentTick;

    private bool isFull;

    public QNetwork()
    {
        input = new Dictionary<CompPowerTrader, IEnumerable<IntVec3>>();
        storage = new Dictionary<CompPowerTrader, IEnumerable<IntVec3>>();
        relays = new Dictionary<CompPowerTrader, IEnumerable<IntVec3>>();
        currentTick = 0;
        cellCapacity = calculateCellCapacity();
        isFull = false;
    }

    public int CellCapacity()
    {
        return cellCapacity;
    }

    public bool IsFull()
    {
        return isFull;
    }

    public void RegisterInput(CompPowerTrader powerTrader, IEnumerable<IntVec3> cells)
    {
        input.Add(powerTrader, cells);
    }

    public void UnregisterInput(CompPowerTrader powerTrader)
    {
        input.Remove(powerTrader);
    }

    public void RegisterStorage(CompPowerTrader powerTrader, IEnumerable<IntVec3> cells)
    {
        storage.Add(powerTrader, cells);
    }

    public void UnregisterStorage(CompPowerTrader powerTrader)
    {
        storage.Remove(powerTrader);
    }

    public void RegisterRelay(CompPowerTrader powerTrader, IEnumerable<IntVec3> cells)
    {
        relays.Add(powerTrader, cells);
    }

    public void UnregisterRelay(CompPowerTrader powerTrader)
    {
        relays.Remove(powerTrader);
    }

    public void Mantain(Map map, int tick)
    {
        if (currentTick == tick)
        {
            return;
        }

        currentTick = tick;
        cellCapacity = calculateCellCapacity();
        var intVec3S =
            new HashSet<IntVec3>(input.Where(powerTrader => Utils.PoweredOn(powerTrader.Key)).SelectMany(x => x.Value));
        var cells2 = storage.Where(storeSpace => Utils.PoweredOn(storeSpace.Key)).SelectMany(x => x.Value).ToList();
        var source = (from cells in (from x in relays where Utils.PoweredOn(x.Key) select x.Value.ToList()).ToList()
            select new QStorage(QStorage.Kind.Relay, 1).FromCells(map, cells,
                delegate(StorageSettings storageSettings, Thing thing)
                {
                    if (!storageSettings.AllowedToAccept(thing))
                    {
                        return false;
                    }

                    return thing.stackCount >= thing.def.stackLimit;
                })).ToList();
        var quantumStorage = new QStorage(QStorage.Kind.Storage, cellCapacity).FromCells(map, cells2,
            (_, thing) => thing.stackCount >= thing.def.stackLimit);
        (from thing in Enumerable.Concat(
                second: from thing in intVec3S.Where(cell => Utils.Priority(map, cell) != StoragePriority.Unstored)
                    .SelectMany(cell => Utils.GetItemList(map, cell))
                select new QThing(thing, QThing.Source.Input),
                first: source.SelectMany(qrelay => qrelay.ExcludedThings()).Concat(quantumStorage.ExcludedThings()))
            group thing by thing.stackCount < thing.def.stackLimit).Deconstruct(out var pos, out var neg);
        var second2 = mergePartialStacks(pos);
        var insertionQueue = neg.Concat(second2).ToList();
        var second3 = source.SelectMany(delegate(QStorage relay)
        {
            relay.InsertToRelay(insertionQueue, quantumStorage);
            return relay.Diff();
        }).ToList();
        quantumStorage.Insert(insertionQueue);
        var second4 = quantumStorage.Diff();
        isFull = insertionQueue.Any();
        foreach (var item in insertionQueue.SelectMany(qthing => qthing.MoveOut(map, intVec3S.RandomElement()).actions)
                     .Concat(second3).Concat(second4))
        {
            item.Perform();
        }
    }

    private static int calculateCellCapacity()
    {
        var num = 2;
        if (DefDatabase<ResearchProjectDef>.GetNamed("ResearchProject_QSRCapacityUpgrade_1").IsFinished)
        {
            num += 2;
        }

        if (DefDatabase<ResearchProjectDef>.GetNamed("ResearchProject_QSRCapacityUpgrade_2").IsFinished)
        {
            num += 2;
        }

        if (DefDatabase<ResearchProjectDef>.GetNamed("ResearchProject_QSRCapacityUpgrade_3").IsFinished)
        {
            num += 2;
        }

        if (DefDatabase<ResearchProjectDef>.GetNamed("ResearchProject_QSRCapacityUpgrade_4").IsFinished)
        {
            num += 2;
        }

        return num;
    }

    private static List<QThing> mergePartialStacks(IEnumerable<QThing> partialStacks)
    {
        return partialStacks.GroupBy(x => x.def.shortHash).Aggregate([],
            delegate(List<QThing> quantumThings, IGrouping<ushort, QThing> defStacks)
            {
                var qThing = defStacks.Aggregate(new QThing(defStacks.First().thing, QThing.Source.Merge),
                    delegate(QThing composite, QThing quantumThing)
                    {
                        var num = quantumThing.stackCount;
                        var num2 = composite.stackCount + num - quantumThing.def.stackLimit;
                        if (num2 > 0)
                        {
                            num -= num2;
                        }

                        composite.Absorb(quantumThing, num);
                        if (composite.stackCount != composite.def.stackLimit)
                        {
                            return composite;
                        }

                        quantumThings.Add(composite);
                        return new QThing(quantumThing.thing, QThing.Source.Merge).Absorb(quantumThing, num2);
                    });
                if (qThing.stackCount > 0)
                {
                    quantumThings.Add(qThing);
                }

                return quantumThings;
            });
    }
}
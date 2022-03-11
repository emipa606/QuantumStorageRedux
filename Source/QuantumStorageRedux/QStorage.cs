using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace QuantumStorageRedux;

internal class QStorage
{
    public enum Kind
    {
        Relay,
        Storage
    }

    private readonly int cellCapacity;

    private readonly Dictionary<ThingDef, List<(int cellIndex, int thingIndex)>> defsIndex;

    private readonly List<QThing> excluded;

    private readonly HashSet<ThingDef> existingDefs;

    private readonly Kind kind;

    private List<IntVec3> cells;

    private Map map;

    private List<QCell> storage;

    public QStorage(Kind kind, int cellCapacity)
    {
        this.kind = kind;
        this.cellCapacity = cellCapacity;
        excluded = new List<QThing>();
        switch (this.kind)
        {
            case Kind.Relay:
                existingDefs = new HashSet<ThingDef>();
                break;
            case Kind.Storage:
                defsIndex = new Dictionary<ThingDef, List<(int, int)>>();
                break;
        }
    }

    public QStorage FromCells(Map incomingMap, List<IntVec3> incomingCells,
        Func<StorageSettings, Thing, bool> isAllowed)
    {
        map = incomingMap;
        cells = incomingCells;
        storage = cells.Select(delegate(IntVec3 cell, int cellIndex)
        {
            var storageSettings = Utils.GetStorageSettings(map, cell);
            var array = new QThing[cellCapacity];
            foreach (var (thing, num) in Utils.GetItemList(map, cell).WithIndex())
            {
                if (num >= cellCapacity)
                {
                    break;
                }

                if (!isAllowed(storageSettings, thing))
                {
                    excluded.Add(new QThing(thing, QThing.Source.Storage));
                }
                else
                {
                    switch (kind)
                    {
                        case Kind.Relay:
                            existingDefs.Add(thing.def);
                            break;
                        case Kind.Storage:
                            IndexDef(thing.def, cellIndex, num);
                            break;
                    }

                    array[num] = new QThing(thing, QThing.Source.Storage);
                }
            }

            return new QCell(cell, storageSettings, array);
        }).ToList();
        return this;
    }

    public void InsertToRelay(List<QThing> insertionQueue, QStorage reqStorage)
    {
        foreach (var qcell in storage)
        {
            if (qcell.qthings[0] != null)
            {
                continue;
            }

            foreach (var allowedThingDef in qcell.storageSettings.filter.AllowedThingDefs)
            {
                if (existingDefs.Contains(allowedThingDef))
                {
                    continue;
                }

                var qThing = reqStorage.RequestWithDef(allowedThingDef);
                if (qThing == null || !qThing.AllowedByStorage(qcell.storageSettings))
                {
                    continue;
                }

                existingDefs.Add(allowedThingDef);
                qcell.qthings[0] = qThing.Insert(map, qcell.cell);
                break;
            }

            if (qcell.qthings[0] != null)
            {
                continue;
            }

            var num = insertionQueue.FindIndex(delegate(QThing x)
            {
                if (existingDefs.Contains(x.def))
                {
                    return false;
                }

                if (!x.AllowedByStorage(qcell.storageSettings))
                {
                    return false;
                }

                existingDefs.Add(x.def);
                qcell.qthings[0] = x.Insert(map, qcell.cell);
                return true;
            });
            if (num > -1)
            {
                insertionQueue.RemoveAt(num);
            }
        }
    }

    public void Insert(List<QThing> insertionQueue)
    {
        if (!insertionQueue.Any())
        {
            return;
        }

        foreach (var qcell2 in storage.OrderByDescending(qcell =>
                     qcell.storageSettings?.Priority ?? StoragePriority.Unstored))
        {
            if (!insertionQueue.Any())
            {
                break;
            }

            if (qcell2.storageSettings == null)
            {
                continue;
            }

            foreach (var item in qcell2.qthings.WithIndex())
            {
                var (qThing, qindex) = item;
                if (!insertionQueue.Any())
                {
                    return;
                }

                if (qcell2.qthings.Count(x => x != null) >= cellCapacity || qThing != null)
                {
                    continue;
                }

                var num = insertionQueue.FindIndex(delegate(QThing x)
                {
                    if (!x.AllowedByStorage(qcell2.storageSettings))
                    {
                        return false;
                    }

                    qcell2.qthings[qindex] = x.Insert(map, qcell2.cell);
                    return true;
                });
                if (num > -1)
                {
                    insertionQueue.RemoveAt(num);
                }
            }
        }
    }

    public List<IPerformable> Diff()
    {
        return cells.Zip(storage).Aggregate(new List<IPerformable>(),
            delegate(List<IPerformable> actions, (IntVec3, QCell) x)
            {
                var item = x.Item1;
                var item2 = x.Item2;
                var itemList = Utils.GetItemList(map, item);
                for (var i = 0; i < cellCapacity; i++)
                {
                    var thing = itemList.ElementAtOrDefault(i);
                    var qThing = item2.qthings[i];
                    if (qThing != null && (thing == null || qThing.source == QThing.Source.Composite ||
                                           thing.def != qThing.def || thing.stackCount != qThing.stackCount ||
                                           thing.HitPoints != qThing.hitPoints))
                    {
                        actions = actions.Concat(qThing.actions).ToList();
                    }
                }

                return actions;
            });
    }

    public List<QThing> ExcludedThings()
    {
        return excluded;
    }

    public QThing RequestWithDef(ThingDef def)
    {
        if (defsIndex == null || !defsIndex.ContainsKey(def) || !defsIndex[def].Any())
        {
            return null;
        }

        var tuple = defsIndex[def].Last();
        var item = tuple.cellIndex;
        var item2 = tuple.thingIndex;
        var result = storage[item].qthings[item2];
        defsIndex[def].RemoveLast();
        storage[item].qthings[item2] = null;
        return result;
    }

    public string Display()
    {
        return storage.Aggregate(new StringBuilder($"{this}#{GetHashCode()}\n"),
            delegate(StringBuilder acc, QCell qcell)
            {
                var cell = qcell.cell;
                acc.Append($"{cell}: | ");
                qcell.qthings.Select(qthing => qthing != null ? LogUtils.Display(qthing) + " | " : "null | ").ToList()
                    .ForEach(delegate(string row) { acc.Append(row); });
                acc.Append("\n");
                return acc;
            }).ToString();
    }

    private void IndexDef(ThingDef def, int cellIndex, int thingIndex)
    {
        if (!defsIndex.ContainsKey(def))
        {
            defsIndex.Add(def, new List<(int, int)>());
        }

        defsIndex[def].Add((cellIndex, thingIndex));
    }
}
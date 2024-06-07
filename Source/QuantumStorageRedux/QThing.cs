using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace QuantumStorageRedux;

internal class QThing
{
    public enum Source
    {
        Input,
        Merge,
        Composite,
        Storage
    }

    private readonly List<QThing> absorbed;

    public readonly ThingDef def;

    public readonly ThingDef stuff;

    public readonly Thing thing;

    public List<IPerformable> actions;

    public GeneSet geneSet;

    public int hitPoints;

    public Source source;

    public int stackCount;

    public QThing(Thing thing, Source source)
    {
        this.source = source;
        this.thing = thing;
        if (thing is Genepack genepack)
        {
            geneSet = genepack.GeneSet;
        }

        def = thing.def;
        stuff = thing.Stuff;
        actions = [];
        if (this.source != Source.Merge)
        {
            stackCount = thing.stackCount;
            hitPoints = thing.HitPoints;
        }

        if (this.source == Source.Merge)
        {
            absorbed = [];
        }
    }

    public QThing Absorb(QThing qthing, int amount)
    {
        if (thing.def.useHitPoints)
        {
            hitPoints = Mathf.CeilToInt(((hitPoints * stackCount) + (qthing.thing.HitPoints * amount)) /
                                        (float)(stackCount + amount));
        }

        stackCount += amount;
        absorbed.Add(qthing);
        source = absorbed.Count <= 1 ? Source.Merge : Source.Composite;
        return this;
    }

    public bool AllowedByStorage(StorageSettings storageSettings)
    {
        if (storageSettings == null)
        {
            return false;
        }

        if (source == Source.Composite)
        {
            if (!Utils.Allows(storageSettings, this))
            {
                return false;
            }
        }
        else
        {
            if (!storageSettings.AllowedToAccept(thing))
            {
                return false;
            }

            if ((int)storageSettings.Priority < (int)Utils.Priority(thing.Map, thing.Position))
            {
                return false;
            }
        }

        return true;
    }

    public Thing Make()
    {
        if (thing is Corpse corpse)
        {
            var corpse2 = ThingMaker.MakeThing(corpse.InnerPawn.RaceProps.corpseDef) as Corpse;
            corpse.GetDirectlyHeldThings().TryTransferToContainer(corpse.InnerPawn, corpse2?.GetDirectlyHeldThings());
            return corpse2;
        }

        if (thing is UnfinishedThing unfinishedThing)
        {
            if (ThingMaker.MakeThing(unfinishedThing.def, stuff) is UnfinishedThing obj)
            {
                obj.workLeft = unfinishedThing.workLeft;
                obj.Creator = unfinishedThing.Creator;
                return obj;
            }
        }

        if (thing is Genepack)
        {
            var pack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
            pack.Initialize(geneSet.GenesListForReading);
            return pack;
        }

        var makeThing = ThingMaker.MakeThing(def, stuff);
        var compQuality = makeThing.TryGetComp<CompQuality>();
        var compQuality2 = thing.TryGetComp<CompQuality>();
        if (compQuality2 != null)
        {
            _ = compQuality2.Quality;
            if (compQuality != null)
            {
                _ = compQuality.Quality;
                if (compQuality.Quality != compQuality2.Quality)
                {
                    makeThing.TryGetComp<CompQuality>().SetQuality(compQuality2.Quality, ArtGenerationContext.Colony);
                }
            }
        }

        makeThing.HitPoints = hitPoints;
        makeThing.stackCount = stackCount;
        return makeThing;
    }

    public QThing Insert(Map map, IntVec3 cell)
    {
        if (source == Source.Composite)
        {
            actions.Add(new SpawnAction(map, cell, this));
            actions = actions
                .Concat(absorbed.Select((Func<QThing, IPerformable>)(qthing => new DestroyAction(qthing.thing))))
                .ToList();
        }
        else
        {
            actions.Add(new SpawnAction(map, cell, this));
            actions.Add(new DeSpawnAction(thing));
        }

        return this;
    }

    public QThing MoveOut(Map map, IntVec3 cell)
    {
        if (source == Source.Input)
        {
            return this;
        }

        if ((source == Source.Merge || source == Source.Composite) &&
            absorbed.All(qthing => qthing.source == Source.Input))
        {
            return this;
        }

        if (source == Source.Composite)
        {
            actions.Add(new SpawnNearAction(map, cell, this));
            actions = actions
                .Concat(absorbed.Select((Func<QThing, IPerformable>)(qthing => new DestroyAction(qthing.thing))))
                .ToList();
        }
        else
        {
            actions.Add(new SpawnNearAction(map, cell, this));
            actions.Add(new DeSpawnAction(thing));
        }

        return this;
    }
}
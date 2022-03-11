using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace QuantumStorageRedux;

public static class Utils
{
    public static Type[] QuantumCompClasses =
    {
        typeof(CompQSRStockpile),
        typeof(CompQSRWarehouse),
        typeof(CompQSRRelay)
    };

    public static StorageSettings GetStorageSettings(Map map, IntVec3 cell)
    {
        if (cell.GetZone(map) != null && cell.GetZone(map).GetType().ToString()
                .Equals("RimWorld.Zone_Stockpile"))
        {
            return (cell.GetZone(map) as Zone_Stockpile)?.GetStoreSettings();
        }

        var thing = cell.GetThingList(map).Find(x => x.GetType().ToString().Equals("RimWorld.Building_Storage"));
        if (thing != null)
        {
            return (thing as Building_Storage)?.GetStoreSettings();
        }

        return null;
    }

    public static StoragePriority Priority(Map map, IntVec3 cell)
    {
        return GetStorageSettings(map, cell)?.Priority ?? StoragePriority.Unstored;
    }

    internal static bool Allows(StorageSettings storageSettings, QThing qthing)
    {
        if (!storageSettings.filter.Allows(qthing.def))
        {
            return false;
        }

        if (!qthing.def.useHitPoints)
        {
            return true;
        }

        var value = GenMath.RoundedHundredth(qthing.hitPoints / (float)qthing.thing.MaxHitPoints);
        if (!storageSettings.filter.AllowedHitPointsPercents.IncludesEpsilon(Mathf.Clamp01(value)))
        {
            return false;
        }

        return true;
    }

    public static List<Thing> GetItemList(Map map, IntVec3 cell)
    {
        return (from x in cell.GetThingList(map)
            where x.def.category == ThingCategory.Item
            select x).ToList();
    }

    public static bool PoweredOn(CompPowerTrader powerTrader)
    {
        return powerTrader is { PowerOn: true };
    }

    public static void ThrowDustPuff(Map map, IntVec3 cell)
    {
        FleckMaker.ThrowDustPuff(cell.ToVector3() + new Vector3(0.5f, 0f, 0.5f), map, 0.5f);
    }

    public static void ThrowSparkle(Map map, IntVec3 cell)
    {
        FleckMaker.ThrowLightningGlow(cell.ToVector3() + new Vector3(0.5f, 0f, 0.5f), map, 0.05f);
    }

    public static void DropSound(Map map, IntVec3 cell, ThingDef thingDef)
    {
        if (thingDef.soundDrop != null)
        {
            thingDef.soundDrop.PlayOneShot(SoundInfo.InMap(new TargetInfo(cell, map)));
        }
    }

    public static Thing Spawn(Thing thing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish,
        bool respawningAfterLoad = false)
    {
        if (thing.Spawned)
        {
            Log.Error($"Tried to spawn {thing.ToStringSafe()} but it's already spawned.");
            return thing;
        }

        if (map == null)
        {
            Log.Error($"Tried to spawn {thing.ToStringSafe()} in a null map.");
            return null;
        }

        if (!loc.InBounds(map))
        {
            var obj = new[]
            {
                "Tried to spawn ",
                thing.ToStringSafe(),
                " out of bounds at ",
                null,
                null
            };
            var intVec = loc;
            obj[3] = intVec.ToString();
            obj[4] = ".";
            Log.Error(string.Concat(obj));
            return null;
        }

        var cellRect = GenAdj.OccupiedRect(loc, rot, thing.def.Size);
        if (!cellRect.InBounds(map))
        {
            var obj2 = new[]
            {
                "Tried to spawn ",
                thing.ToStringSafe(),
                " out of bounds at ",
                null,
                null,
                null,
                null
            };
            var intVec = loc;
            obj2[3] = intVec.ToString();
            obj2[4] = " (out of bounds because size is ";
            obj2[5] = thing.def.Size.ToString();
            obj2[6] = ").";
            Log.Error(string.Concat(obj2));
            return null;
        }

        if (thing.def.randomizeRotationOnSpawn)
        {
            rot = Rot4.Random;
        }

        switch (wipeMode)
        {
            case WipeMode.Vanish:
                GenSpawn.WipeExistingThings(loc, rot, thing.def, map, DestroyMode.Vanish);
                break;
            case WipeMode.FullRefund:
                GenSpawn.WipeAndRefundExistingThings(loc, rot, thing.def, map);
                break;
        }

        if (thing.holdingOwner != null)
        {
            thing.holdingOwner.Remove(thing);
        }

        thing.Position = loc;
        thing.Rotation = rot;
        thing.SpawnSetup(map, respawningAfterLoad);
        if (thing.Spawned && thing.stackCount == 0)
        {
            Log.Error($"Spawned thing with 0 stackCount: {thing}");
            thing.Destroy();
            return null;
        }

        if (thing.def.passability != Traversability.Impassable)
        {
            return thing;
        }

        foreach (var item in cellRect)
        {
            foreach (var item2 in item.GetThingList(map).ToList())
            {
                if (item2 != thing)
                {
                    (item2 as Pawn)?.pather.TryRecoverFromUnwalkablePosition(false);
                }
            }
        }

        return thing;
    }
}
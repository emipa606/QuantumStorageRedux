using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace QuantumStorageRedux {
    public static class Utils {
        public static System.Type[] QuantumCompClasses = { typeof(CompQSRStockpile), typeof(CompQSRWarehouse), typeof(CompQSRRelay) };

        public static StorageSettings GetStorageSettings(Map map, IntVec3 cell) {
            if (
                cell.GetZone(map) != null &&
                cell.GetZone(map).GetType().ToString().Equals("RimWorld.Zone_Stockpile")
            ) {
                return (cell.GetZone(map) as Zone_Stockpile).GetStoreSettings();
            }

            var buildingStorage = cell.GetThingList(map)
                .Find(x => x.GetType().ToString().Equals("RimWorld.Building_Storage"));

            if (buildingStorage != null) {
                return (buildingStorage as Building_Storage).GetStoreSettings();
            }

            return null;
        }

        public static StoragePriority Priority(Map map, IntVec3 cell) {
            var storeSettings = Utils.GetStorageSettings(map, cell);
            if (storeSettings != null) {
                return storeSettings.Priority;
            }

            return StoragePriority.Unstored;
        }

        internal static bool Allows(StorageSettings storageSettings, QThing qthing) {
            if (!storageSettings.filter.Allows(qthing.def)) {
                return false;
            }

            if (qthing.def.useHitPoints) {
                var hitPointsPercent = GenMath.RoundedHundredth(qthing.hitPoints / (float)qthing.thing.MaxHitPoints);
                if (!storageSettings.filter.AllowedHitPointsPercents.IncludesEpsilon(Mathf.Clamp01(hitPointsPercent))) {
                    return false;
                }
            }

            return true;
        }

        public static List<Thing> GetItemList(Map map, IntVec3 cell) {
            return cell.GetThingList(map).
                Where(x => x.def.category == ThingCategory.Item).
                ToList();
        }

        public static bool PoweredOn(CompPowerTrader powerTrader) {
            if (
                powerTrader != null &&
                powerTrader.PowerOn
            ) {
                return true;
            }

            return false;
        }

        public static void ThrowDustPuff(Map map, IntVec3 cell) {
            MoteMaker.ThrowDustPuff(cell.ToVector3() + new Vector3(0.5f, 0.0f, 0.5f), map, 0.5f);
        }

        public static void ThrowSparkle(Map map, IntVec3 cell) {
            MoteMaker.ThrowLightningGlow(cell.ToVector3() + new Vector3(0.5f, 0.0f, 0.5f), map, 0.05f);
        }

        public static void DropSound(Map map, IntVec3 cell, ThingDef thingDef) {
            if (thingDef.soundDrop != null) {
                thingDef.soundDrop.PlayOneShot(SoundInfo.InMap(new TargetInfo(cell, map)));
            }
        }

        public static Thing Spawn(Thing thing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false) {
            
            if (thing.Spawned) {
                Log.Error("Tried to spawn " + thing.ToStringSafe<Thing>() + " but it's already spawned.");
                return thing;
            }

            if (map == null) {
                Log.Error("Tried to spawn " + thing.ToStringSafe<Thing>() + " in a null map.", false);
                return null;
            }

            if (!loc.InBounds(map)) {
                Log.Error("Tried to spawn " + thing.ToStringSafe<Thing>() + " out of bounds at " + loc + ".", false);
                return null;
            }

            var occupiedRect = GenAdj.OccupiedRect(loc, rot, thing.def.Size);
            if (!occupiedRect.InBounds(map)) {
                Log.Error("Tried to spawn " + thing.ToStringSafe<Thing>() + " out of bounds at " + loc + " (out of bounds because size is " + thing.def.Size + ").", false);
                return null;
            }

            if (thing.def.randomizeRotationOnSpawn) {
                rot = Rot4.Random;
            }

            switch (wipeMode) {
                case WipeMode.Vanish:
                    QLog.Message(QLog.Ctx.Thing, "Spawning with Wipemode Vanish");
                    GenSpawn.WipeExistingThings(loc, rot, thing.def, map, DestroyMode.Vanish);
                    /*GenSpawn.Spawn(thing.def, loc, map, WipeMode.Vanish);*/

                    /*                    Thing newThing = GenSpawn.Spawn(thing.def, loc, map, WipeMode.Vanish);
                                        QualityCategory quality = new Quality(thing).getQuality();

                                        newThing.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);

                                        QLog.Message(QLog.Ctx.Thing, "Utils.Spawn: Attempting to set Quality: ");
                                        QLog.Message(QLog.Ctx.Thing, quality.GetLabel());

                                        QLog.Message(QLog.Ctx.Thing, "Quality.setQuality: Quality set to: ");
                                        QLog.Message(QLog.Ctx.Thing, newThing.TryGetComp<CompQuality>()?.Quality.GetLabel());*/



                    /*if (Quality.checkQuality(thing)) {
                                            QLog.Warning(QLog.Ctx.Thing, "v---------- Utils.Spawn ----------v");
                                            QLog.Message(QLog.Ctx.Thing, "Thing had quality, attempting to set");
                                            Quality.fetchQuality(thing);
                                            QLog.Message(QLog.Ctx.Thing, Quality.fetchQuality(thing).ToString());
                                            QLog.Warning(QLog.Ctx.Thing, "^---------- Utils.Spawn ----------^");
                                        }*/

                    break;
                case WipeMode.FullRefund:
                    GenSpawn.WipeAndRefundExistingThings(loc, rot, thing.def, map);
                    /*GenSpawn.Spawn(thing.def, loc, map, WipeMode.FullRefund);*/
                    break;
            }

            if (thing.holdingOwner != null) {
                thing.holdingOwner.Remove(thing);
            }

            

            thing.Position = loc;
            thing.Rotation = rot;
            thing.SpawnSetup(map, respawningAfterLoad);

            if (
                thing.Spawned &&
                thing.stackCount == 0
            ) {
                Log.Error("Spawned thing with 0 stackCount: " + thing, false);
                thing.Destroy(DestroyMode.Vanish);
                return null;
            }

            if (thing.def.passability == Traversability.Impassable) {
                foreach (var cell in occupiedRect) {
                    foreach (var existingThing in cell.GetThingList(map).ToList<Thing>()) {
                        if (existingThing != thing) {
                            (existingThing as Pawn)?.pather.TryRecoverFromUnwalkablePosition(false);
                        }
                    }
                }
            }

            
            return thing;
        }
    }
}

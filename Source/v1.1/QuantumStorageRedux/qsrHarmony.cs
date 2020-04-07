using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace QuantumStorageRedux {
    internal static class qsrHarmony {
        private static Harmony harmony;

        public static void Patch() {
            harmony = new Harmony("pw.cheetah.rimworld.quantumstorageredux");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            QLog.Warning(QLog.Ctx.Default, " v--------------- QUANTUM LOG ---------------v");
            QLog.Message(QLog.Ctx.Default, "Executing Quantum Storage | Build 40");
            QLog.Warning(QLog.Ctx.Default, " v--------------- QUANTUM LOG ---------------v");
        }
    }

    [HarmonyPatch(typeof(GenSpawn), "Spawn", new Type[] {
        typeof(Thing),  
        typeof(IntVec3),
        typeof(Map),
        typeof(Rot4),
        typeof(WipeMode),
        typeof(bool),
    })]
    public static class PatchedSpawn {
        public static bool Prefix(out Thing __result, Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false) {
            __result = null;
            if (!respawningAfterLoad) {
                __result = newThing;
                return true;
            }

            __result = Utils.Spawn(newThing, loc, map, rot, wipeMode, respawningAfterLoad);
            return false;
        }
    }
}

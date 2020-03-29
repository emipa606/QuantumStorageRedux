using System;
using System.Reflection;
/* Update Harmony to HarmonyLib*/
/*using Harmony;*/
using HarmonyLib;
using Verse;

namespace QuantumStorageRedux {
    internal static class qsrHarmony {
        /*private static MethodInfo originalSpawn;
        private static MethodInfo quantumSpawn;*/
        /* Update HarmonyInstance to Harmony */
        /*private static HarmonyInstance harmony;*/
        private static Harmony harmony;

        public static void Patch() {
            /* Update HarmonyInstance.Create to newHarmony() */
            /*harmony = HarmonyInstance.Create("pw.cheetah.rimworld.quantumstorageredux");*/
            harmony = new Harmony("pw.cheetah.rimworld.quantumstorageredux");
            /*quantumSpawn = typeof(Harmony).GetMethod("PatchedSpawn", BindingFlags.Static | BindingFlags.Public);
            originalSpawn = typeof(GenSpawn).GetMethod("Spawn", new Type[] {
                typeof(Thing),
                typeof(IntVec3),
                typeof(Map),
                typeof(Rot4),
                typeof(WipeMode),
                typeof(bool),
            });*/

            harmony.PatchAll(Assembly.GetExecutingAssembly());

            /*harmony.Patch(originalSpawn, new HarmonyMethod(quantumSpawn), null);*/
        }

        /*public static bool PatchedSpawn(out Thing __result, Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false) {
            __result = null;
            if (!respawningAfterLoad) {
                __result = newThing;
                return true;
            }

            __result = Utils.Spawn(newThing, loc, map, rot, wipeMode, respawningAfterLoad);
            return false;
        }*/
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

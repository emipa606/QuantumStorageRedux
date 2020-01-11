using System;
using System.Reflection;
using Harmony;
using Verse;

namespace QuantumStorageRedux {
    internal static class Harmony {
        private static MethodInfo originalSpawn;
        private static MethodInfo quantumSpawn;
        private static HarmonyInstance harmony;

        public static void Patch() {
            harmony = HarmonyInstance.Create("pw.cheetah.rimworld.quantumstorageredux");
            quantumSpawn = typeof(Harmony).GetMethod("PatchedSpawn", BindingFlags.Static | BindingFlags.Public);
            originalSpawn = typeof(GenSpawn).GetMethod("Spawn", new Type[] {
                typeof(Thing),
                typeof(IntVec3),
                typeof(Map),
                typeof(Rot4),
                typeof(WipeMode),
                typeof(bool),
            });

            harmony.Patch(originalSpawn, new HarmonyMethod(quantumSpawn), null);
        }

        public static bool PatchedSpawn(out Thing __result, Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false) {
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

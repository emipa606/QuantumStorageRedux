using HarmonyLib;
using Verse;

namespace QuantumStorageRedux;

[HarmonyPatch(typeof(GenSpawn), "Spawn", typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode),
    typeof(bool))]
public static class PatchedSpawn
{
    public static bool Prefix(out Thing __result, Thing newThing, IntVec3 loc, Map map, Rot4 rot,
        WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false)
    {
        __result = null;
        if (!respawningAfterLoad)
        {
            __result = newThing;
            return true;
        }

        __result = Utils.Spawn(newThing, loc, map, rot, wipeMode, true);
        return false;
    }
}
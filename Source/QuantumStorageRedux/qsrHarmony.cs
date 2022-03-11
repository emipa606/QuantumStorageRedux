using System.Reflection;
using HarmonyLib;

namespace QuantumStorageRedux;

internal static class qsrHarmony
{
    private static Harmony harmony;

    public static void Patch()
    {
        harmony = new Harmony("pw.cheetah.rimworld.quantumstorageredux");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}
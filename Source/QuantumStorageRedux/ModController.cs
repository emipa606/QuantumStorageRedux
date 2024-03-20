using System.Reflection;
using HarmonyLib;
using Verse;

namespace QuantumStorageRedux;

internal class ModController : Mod
{
    public ModController(ModContentPack content)
        : base(content)
    {
        new Harmony("pw.cheetah.rimworld.quantumstorageredux").PatchAll(Assembly.GetExecutingAssembly());
    }
}
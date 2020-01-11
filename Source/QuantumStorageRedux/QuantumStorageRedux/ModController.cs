using Verse;

namespace QuantumStorageRedux {
    internal class ModController : Mod {
        public ModController(ModContentPack content) : base(content) {
            Harmony.Patch();
        }
    }
}

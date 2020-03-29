using RimWorld;
using Verse;

namespace QuantumStorageRedux {
    internal class QCell {
        public IntVec3 cell;
        public StorageSettings storageSettings;
        public QThing[] qthings;

        public QCell(IntVec3 cell, StorageSettings storageSettings, QThing[] qthings) {
            this.cell = cell;
            this.storageSettings = storageSettings;
            this.qthings = qthings;
        }
    }
}

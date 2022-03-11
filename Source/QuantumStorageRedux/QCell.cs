using RimWorld;
using Verse;

namespace QuantumStorageRedux;

internal class QCell
{
    public IntVec3 cell;

    public QThing[] qthings;

    public StorageSettings storageSettings;

    public QCell(IntVec3 cell, StorageSettings storageSettings, QThing[] qthings)
    {
        this.cell = cell;
        this.storageSettings = storageSettings;
        this.qthings = qthings;
    }
}
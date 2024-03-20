using RimWorld;
using Verse;

namespace QuantumStorageRedux;

internal class QCell(IntVec3 cell, StorageSettings storageSettings, QThing[] qthings)
{
    public readonly QThing[] qthings = qthings;

    public readonly StorageSettings storageSettings = storageSettings;
    public IntVec3 cell = cell;
}
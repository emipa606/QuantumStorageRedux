using System.Linq;
using Verse;

namespace QuantumStorageRedux;

internal class PlaceWorker_QSRRequireWalkable : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        if (GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size).Any(cell => !cell.Walkable(map)))
        {
            return "PlaceWorker_QSRRequireWalkable".Translate();
        }

        return true;
    }
}
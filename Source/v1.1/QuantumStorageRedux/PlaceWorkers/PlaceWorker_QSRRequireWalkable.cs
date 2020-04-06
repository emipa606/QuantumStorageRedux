using System.Linq;
using Verse;

namespace QuantumStorageRedux {
    internal class PlaceWorker_QSRRequireWalkable : PlaceWorker {
        public AcceptanceReport AllowsPlacing(
            BuildableDef checkingDef,
            IntVec3 loc,
            Rot4 rot,
            Map map,
            Thing thingToIgnore = null
        ) {
            var containsNotWalkable = GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size).
                Any(cell => !cell.Walkable(map));

            if (containsNotWalkable) {
                return "PlaceWorker_QSRRequireWalkable".Translate();
            }

            return true;
        }
    }
}

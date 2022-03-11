using System.Linq;
using Verse;

namespace QuantumStorageRedux;

internal class PlaceWorker_QSRNoQSOverlap : PlaceWorker
{
    public AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null)
    {
        if (GenAdj.CellsAdjacent8Way(loc, rot, checkingDef.Size)
            .Union(GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
            .SelectMany(cell => map.thingGrid.ThingsListAt(cell))
            .Any(delegate(Thing thing)
            {
                if (thing.TryGetComp<CompQSRStockpile>() != null || thing.TryGetComp<CompQSRWarehouse>() != null ||
                    thing.TryGetComp<CompQSRRelay>() != null)
                {
                    return true;
                }

                if (thing.def.entityDefToBuild == null)
                {
                    return false;
                }

                if (thing.def.entityDefToBuild is ThingDef { comps: { } } thingDef &&
                    thingDef.comps.Any(comp => Utils.QuantumCompClasses.Contains(comp.compClass)))
                {
                    return true;
                }

                return false;
            }))
        {
            return "PlaceWorker_QSRNoQSOverlap".Translate();
        }

        return true;
    }
}
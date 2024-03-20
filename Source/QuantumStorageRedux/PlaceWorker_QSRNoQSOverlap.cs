using System.Linq;
using Verse;

namespace QuantumStorageRedux;

internal class PlaceWorker_QSRNoQSOverlap : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        if (GenAdj.CellsAdjacent8Way(loc, rot, checkingDef.Size)
            .Union(GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
            .SelectMany(cell => map.thingGrid.ThingsListAt(cell))
            .Any(delegate(Thing thingToCheck)
            {
                if (thingToCheck.TryGetComp<CompQSRStockpile>() != null ||
                    thingToCheck.TryGetComp<CompQSRWarehouse>() != null ||
                    thingToCheck.TryGetComp<CompQSRRelay>() != null)
                {
                    return true;
                }

                return thingToCheck.def.entityDefToBuild is ThingDef { comps: not null } thingDef &&
                       thingDef.comps.Any(comp => Utils.QuantumCompClasses.Contains(comp.compClass));
            }))
        {
            return "PlaceWorker_QSRNoQSOverlap".Translate();
        }

        return true;
    }
}
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace QuantumStorageRedux;

internal class Alert_NoQuantumStorageSpace : Alert
{
    public Alert_NoQuantumStorageSpace()
    {
        defaultLabel = "NoQuantumStorageSpace".Translate();
        defaultExplanation = "NoQuantumStorageSpaceDesc".Translate();
        defaultPriority = AlertPriority.High;
    }

    protected override Color BGColor { get; } = new Color(1f, 0.9215686f, 0.01568628f, 0.35f);

    public override AlertReport GetReport()
    {
        if (!QNetworkManager.AnyNetworkIsFull())
        {
            return false;
        }

        var homeMap = Find.Maps.FirstOrDefault(map => map.IsPlayerHome);
        if (homeMap == null)
        {
            return true;
        }

        var storages = homeMap.listerBuildings.allBuildingsColonist.Where(building =>
            building.TryGetComp<CompQSRStockpile>() != null || building.TryGetComp<CompQSRWarehouse>() != null ||
            building.TryGetComp<CompQSRRelay>() != null);
        if (!storages.Any())
        {
            return true;
        }

        var report = new AlertReport { culpritsThings = [], active = true };
        foreach (var building in storages)
        {
            report.culpritsThings.Add(building);
        }

        return report;
    }
}
using RimWorld;
using UnityEngine;
using Verse;

namespace QuantumStorageRedux {
    internal class Alert_NoQuantumStorageSpace : Alert {
        private Color bgColor = new Color(1f, 0.9215686f, 0.01568628f, .35f);

        protected override Color BGColor {
            get {
                return this.bgColor;
            }
        }

        public Alert_NoQuantumStorageSpace() {
            this.defaultLabel = "NoQuantumStorageSpace".Translate();
            this.defaultExplanation = "NoQuantumStorageSpaceDesc".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override AlertReport GetReport() {
            return QNetworkManager.AnyNetworkIsFull();
        }



    }
}

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace QuantumStorageRedux {
    [StaticConstructorOnStartup]
    internal static class QNetworkManager {
        private static readonly HashSet<QNetwork> localNetworks = new HashSet<QNetwork>();
        private static readonly Dictionary<Map, QNetwork> globalNetworks = new Dictionary<Map, QNetwork>();

        public static QNetwork Get(Map map) {
            if (globalNetworks.ContainsKey(map)) {
                return globalNetworks[map];
            }

            var network = new QNetwork();
            globalNetworks.Add(map, network);
            return network;
        }

        public static void RegisterLocalNetwork(QNetwork network) {
            localNetworks.Add(network);
        }

        public static void UnregisterLocalNetwork(QNetwork network) {
            localNetworks.Remove(network);
        }

        public static bool AnyNetworkIsFull() {
            return globalNetworks.Any(pair => pair.Value.IsFull()) ||
                localNetworks.Any(network => network.IsFull());
        }
    }
}

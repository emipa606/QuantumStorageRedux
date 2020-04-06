#define DEBUG

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;

namespace QuantumStorageRedux {
    [StaticConstructorOnStartup]
    internal class QLog {
        public enum Ctx {
            Default,
            Insertion,
            Relay,
            Storage,
            Thing,
        }

        private static readonly Dictionary<Ctx, bool> displaySettings = new Dictionary<Ctx, bool> {
            { Ctx.Default, true },
            { Ctx.Insertion, true },
            { Ctx.Relay, true },
            { Ctx.Storage, true },
            { Ctx.Thing, true },
        };

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Message(Ctx ctx, string text) {
            if (displaySettings[ctx]) {
                Log.Message("QSR." + ctx.ToString() + ": " + text);
                FileLog.Log("QSR." + ctx.ToString() + ": " + text);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Warning(Ctx ctx, string text) {
            if (displaySettings[ctx]) {
                Log.Warning("QSR." + ctx.ToString() + ": " + text);
                FileLog.Log("QSR." + ctx.ToString() + ": " + text);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Error(Ctx ctx, string text) {
            if (displaySettings[ctx]) {
                Log.Error("QSR." + ctx.ToString() + ": " + text);
                FileLog.Log("QSR." + ctx.ToString() + ": " + text);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Message(string text) {
            Message(Ctx.Default, text);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Warning(string text) {
            Warning(Ctx.Default, text);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Error(string text) {
            Error(Ctx.Default, text);
        }
    }

    public static class LogUtils {
        public static string Display(Thing thing) {
            return thing.def.defName + " (" + thing.stackCount + ")";
        }

        internal static string Display(QThing thing) {
            return thing.def.defName + " (" + thing.stackCount + ")";
        }

        internal static string Display(IEnumerable<QThing> list) {
            if (list.Count() == 0) {
                return "Empty";
            }

            return list.Aggregate(new StringBuilder(), (acc, thing) => {
                acc.Append(LogUtils.Display(thing) + " | ");
                return acc;
            }).ToString();
        }
    }
}

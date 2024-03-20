using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using Verse;

namespace QuantumStorageRedux;

[StaticConstructorOnStartup]
internal class QLog
{
    public enum Ctx
    {
        Default,
        Insertion,
        Relay,
        Storage,
        Thing
    }

    private static readonly Dictionary<Ctx, bool> displaySettings = new Dictionary<Ctx, bool>
    {
        {
            Ctx.Default,
            true
        },
        {
            Ctx.Insertion,
            true
        },
        {
            Ctx.Relay,
            true
        },
        {
            Ctx.Storage,
            true
        },
        {
            Ctx.Thing,
            true
        }
    };

    [Conditional("DEBUG")]
    public static void Message(Ctx ctx, string text)
    {
        if (!displaySettings[ctx])
        {
            return;
        }

        Log.Message($"QSR.{ctx}: {text}");
        FileLog.Log($"QSR.{ctx}: {text}");
    }

    [Conditional("DEBUG")]
    public static void Warning(Ctx ctx, string text)
    {
        if (!displaySettings[ctx])
        {
            return;
        }

        Log.Warning($"QSR.{ctx}: {text}");
        FileLog.Log($"QSR.{ctx}: {text}");
    }

    [Conditional("DEBUG")]
    public static void Error(Ctx ctx, string text)
    {
        if (!displaySettings[ctx])
        {
            return;
        }

        Log.Error($"QSR.{ctx}: {text}");
        FileLog.Log($"QSR.{ctx}: {text}");
    }

    [Conditional("DEBUG")]
    public static void Message(string text)
    {
    }

    [Conditional("DEBUG")]
    public static void Warning(string text)
    {
    }

    [Conditional("DEBUG")]
    public static void Error(string text)
    {
    }
}
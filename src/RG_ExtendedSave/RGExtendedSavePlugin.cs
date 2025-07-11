﻿using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace RGExtendedSave
{
    [BepInProcess("RoomGirl")]
    [BepInPlugin(GUID, PluginName, Version)]
    public class RGExtendedSavePlugin : BasePlugin
    {
        public const string PluginName = "RG ExtendedSave";
        public const string GUID = "com.bogus.RGExtendedSave";
        public const string Version = "0.0.4";

        internal static new ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;

            Harmony.CreateAndPatchAll(typeof(Character.Hooks), $"{GUID}-character");
            Harmony.CreateAndPatchAll(typeof(Coordinate.Hooks), $"{GUID}-coordinate");
            Harmony.CreateAndPatchAll(typeof(Ext.Hooks), $"{GUID}-extensions");
            Harmony.CreateAndPatchAll(typeof(ExtendedSaveHooks), $"{GUID}-extended-save");

            ClassInjector.RegisterTypeInIl2Cpp<PluginData>();
        }
    }
}

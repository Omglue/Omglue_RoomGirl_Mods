using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.IL2CPP.Utils;
using BepInEx.Logging;
using Chara;
using CharaCustom;
using HarmonyLib;
using RGExtendedSave;
using System.Collections;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace RG_HeightMod
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class HeightMod : BasePlugin
    {
        public const string PluginName = "RoomGirl Height Mod";
        public const string GUID = "RoomGirl.Glueman.HeightMod";
        public const string Version = "2.5.0";
        internal static new ManualLogSource Log;

        internal static ConfigEntry<int> NumberOfTry;
        internal static ConfigEntry<bool> ApplyHeight;

        public static GameObject ControllerObject;
        public static MonoBehaviour ControllerInstance;

        public override void Load()
        {
            Log = base.Log;

            NumberOfTry = Config.Bind("Debug", "When character studio is opened, this mod will try in loop to find the height slider until it it gets created and the mod finds it. This is the max ammount of try allowed. The default 300 should be 100s at 60fps. You can try to increase this number if the height slidder doesnt appear and you have long loadings", 300);
            ApplyHeight = Config.Bind("Debug", "Apply height > change the value (from on to off or off to on) of this settings to reforce apply the height number to charachters present", true);

            ClassInjector.RegisterTypeInIl2Cpp<HeightModController>();
            ClassInjector.RegisterTypeInIl2Cpp<HeightModCharaController>();

            ControllerObject = new GameObject("HeightModGameObject");
            GameObject.DontDestroyOnLoad(ControllerObject);
            ControllerObject.hideFlags = HideFlags.HideAndDontSave;
            ControllerObject.AddComponent<HeightModController>();

            Harmony.CreateAndPatchAll(typeof(Hooks));
        }
    }

    public class HeightModController : MonoBehaviour
    {
        public void Awake() => HeightMod.ControllerInstance = this;
    }

    public class HeightModCharaController : MonoBehaviour
    {
        public ChaControl ChaControlInstance { get; private set; }

        protected void Awake() => MonoBehaviourExtensions.StartCoroutine(HeightMod.ControllerInstance, DelayApplyMaleHeight());

        public IEnumerator DelayApplyMaleHeight()
        {
            if (ChaControlInstance.Sex == 0)
            {
                ChaControlInstance = GetComponent<ChaControl>();

                for (int i = 0; i < 60; i++) yield return null;

                PluginData data = ExtendedSave.GetExtendedDataById(ChaControlInstance.ChaFile, HeightMod.GUID);
                if (data != null && data.data.TryGetValue("HeightSave", out var val) && val is float fVal) ChaControlInstance.ChaFile.Custom.body.shapeValueBody[0] = fVal;

                yield return null;
                ChaControlInstance.UpdateShapeBodyValueFromCustomInfo();

                Events.CardBeingSaved += OnCardBeingSaved;
                HeightMod.ApplyHeight.SettingChanged += (sender, e) => ChaControlInstance.UpdateShapeBodyValueFromCustomInfo();
            }
            else
            {
                // Remove this component
            }
        }

        public void OnCardBeingSaved(ChaFile file)
        {
            if (file == ChaControlInstance.ChaFile)
            {
                PluginData data = new PluginData();
                data.data.Add("HeightSave", ChaControlInstance.ChaFile.Custom.body.shapeValueBody[0]);
                data.version = 1;
                ExtendedSave.SetExtendedDataById(ChaControlInstance.ChaFile, HeightMod.GUID, data);
            }
        }
    }

    internal static class Hooks 
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomBase), nameof(CustomBase.Awake))]
        public static void Show_MaleHeightSlider_Patch()
        {
            MonoBehaviourExtensions.StartCoroutine(HeightMod.ControllerInstance, IGuessThisHasToBeDelayed());

            IEnumerator IGuessThisHasToBeDelayed()
            {
                for (int i = HeightMod.NumberOfTry.Value; i > 0; i--)
                {
                    for (int j = 20;  j > 0; j--) yield return null;

                    Transform element = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_ShapeWhole/Scroll View/Viewport/Content")?.transform.GetChild(0);
                    element?.gameObject.SetActive(true);
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.Initialize))]
        public static void Remove_InitializeMaleHeightOverriding_Patch(ChaControl __instance, ChaFileControl _chaFile = null)
        {
            if (__instance.Sex == 0 && _chaFile != null)
                __instance.ChaFile.Custom.body.shapeValueBody[0] = _chaFile.Custom.body.shapeValueBody[0];

            __instance.gameObject.AddComponent<HeightModCharaController>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetShapeBodyValue))]
        public static void Remove_SetShapeBodyValueMaleHeightOverriding_Patch(ChaControl __instance, int index, float value)
        {
            if (__instance.Sex == 0 && index == 0)
            {
                __instance.ChaFile.Custom.body.shapeValueBody[0] = value;
                __instance.SIBBody.ChangeValue(0, value);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.UpdateShapeBodyValueFromCustomInfo))]
        public static void Remove_UpdateShapeBodyValueFromCustomInfoHeightOverridind_Patch(ChaControl __instance)
        {
            if (__instance.Sex == 0)
                __instance.SIBBody.ChangeValue(0, __instance.ChaFile.Custom.body.shapeValueBody[0]);
        }
    }
}
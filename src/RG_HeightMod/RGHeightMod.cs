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
    public class RGHeightMod : BasePlugin
    {
        public const string PluginName = "RoomGirl Height Mod";
        public const string GUID = "com.roomgirl.rgheightmod.82375";
        public const string Version = "2.4.24";
        internal static new ManualLogSource Log;

        internal static ConfigEntry<int> TryNumber;
        internal static ConfigEntry<bool> ApplyHeight;

        public static GameObject ControllerObject;
        public static MonoBehaviour ControllerInstance;

        public override void Load()
        {
            Log = base.Log;

            TryNumber = Config.Bind("Debug", "When male character studio is opened, this mod will try in loop to find the height slider until it is created in the tree. This is the max ammount of try allowed. The default 2000 should be 100s at 60fps. You can try to increase this number if the height slidder doesnt appear", 2000);
            ApplyHeight = Config.Bind("Debug", "Apply height > change the value (from on to off or off to on) of this settings to reforce apply the height number to charachters present", true);

            ClassInjector.RegisterTypeInIl2Cpp<RGHeightModController>();
            ClassInjector.RegisterTypeInIl2Cpp<RGHeightModCharaController>();

            ControllerObject = new GameObject("RGHeightModGameObject");
            GameObject.DontDestroyOnLoad(ControllerObject);
            ControllerObject.hideFlags = HideFlags.HideAndDontSave;
            ControllerObject.AddComponent<RGHeightModController>();

            Harmony.CreateAndPatchAll(typeof(Hooks));
        }
    }

    public class RGHeightModController : MonoBehaviour
    {
        public void Awake()
        {
            RGHeightMod.ControllerInstance = this;
        }
    }

    public class RGHeightModCharaController : MonoBehaviour
    {
        public ChaControl ChaControlInternal { get; private set; }

        protected void Awake()
        {
            RGHeightMod.Log.LogMessage("RoomGirl Height Mod CharaController Added To New Chara");

            ChaControlInternal = GetComponent<ChaControl>();

            MonoBehaviourExtensions.StartCoroutine(this, DelayApplyMaleHeight());
        }

        public IEnumerator DelayApplyMaleHeight()
        {
            for (int i = 0; i < 240; i++)
            {
                yield return null;
            }

            if (ChaControlInternal.Sex == 0)
            {
                var data = ExtendedSave.GetExtendedDataById(ChaControlInternal.ChaFile, RGHeightMod.GUID);
                if (data != null && data.data.TryGetValue("HeightSave", out var val) && val is float fVal)
                {
                    ChaControlInternal.ChaFile.Custom.body.shapeValueBody[0] = fVal;
                    yield return null;
                }

                ChaControlInternal.UpdateShapeBodyValueFromCustomInfo();

                Events.CardBeingSaved += OnCardBeingSaved;
                RGHeightMod.ApplyHeight.SettingChanged += (object sender, System.EventArgs e) => ChaControlInternal.UpdateShapeBodyValueFromCustomInfo();
            }
        }

        public void OnCardBeingSaved(ChaFile file)
        {
            var data = new PluginData();
            data.data.Add("HeightSave", ChaControlInternal.ChaFile.Custom.body.shapeValueBody[0]);
            data.version = 1;
            ExtendedSave.SetExtendedDataById(ChaControlInternal.ChaFile, RGHeightMod.GUID, data);
        }
    }

    internal static class Hooks 
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomBase), nameof(CustomBase.Awake))]
        public static void ShowMaleHeightSliderPatch()
        {
            MonoBehaviourExtensions.StartCoroutine(RGHeightMod.ControllerInstance, IGuessThisHasToBeDelayed());

            IEnumerator IGuessThisHasToBeDelayed()
            {
                for (int i = RGHeightMod.TryNumber.Value; i >= 0; i--)
                {
                    yield return null;
                    yield return null;
                    yield return null;
                    var element = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_ShapeWhole/Scroll View/Viewport/Content")?.transform.GetChild(0);
                    element?.gameObject.SetActive(true);
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.Initialize))]
        public static void ChaControl_Initialize_RemoveHeightLock(ChaControl __instance, ChaFileControl _chaFile = null)
        {
            if (__instance.Sex == 0 && _chaFile != null)
            {
                RGHeightMod.Log.LogMessage("Init patch : " + _chaFile.Custom.body.shapeValueBody[0]);
                __instance.ChaFile.Custom.body.shapeValueBody[0] = _chaFile.Custom.body.shapeValueBody[0];

                //MonoBehaviourExtensions.StartCoroutine(RGHeightMod.ControllerInstance, Delay());
                //IEnumerator Delay()
                //{
                    //yield return null;
                    //yield return null;
                    //yield return null;
                    //yield return null;
                    //yield return null;
                    //__instance.UpdateShapeBodyValueFromCustomInfo();
                //}
            }
            
            __instance.gameObject.AddComponent<RGHeightModCharaController>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetShapeBodyValue))]
        public static void ChaControl_SetShapeBodyValue_RemoveHeightLock2(ChaControl __instance, int index, float value)
        {
            if (__instance.Sex == 0 && index == 0)
            {
                RGHeightMod.Log.LogMessage("SetSBV patch : " + value);
                __instance.ChaFile.Custom.body.shapeValueBody[0] = value;
                __instance.SIBBody.ChangeValue(0, value);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.UpdateShapeBodyValueFromCustomInfo))]
        public static void ChaControl_SetShapeBodyValue_RemoveHeightLock(ChaControl __instance)
        {
            if (__instance.Sex == 0)
            {
                RGHeightMod.Log.LogMessage("UpdatedSBV patch : " + __instance.ChaFile.Custom.body.shapeValueBody[0]);
                __instance.SIBBody.ChangeValue(0, __instance.ChaFile.Custom.body.shapeValueBody[0]);
            }
        }
    }
}
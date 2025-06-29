using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.IL2CPP.Utils;
using BepInEx.Logging;
using Chara;
using CharaCustom;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System.Collections;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RG_HeightMod
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class HeightMod : BasePlugin
    {
        public const string PluginName = "RoomGirl Height Mod";
        public const string GUID = "RoomGirl.Glueman.HeightMod";
        public const string Version = "3.0.0";

        internal static ConfigEntry<int> NumberOfTry;

        internal static new ManualLogSource Log;
        public static GameObject ControllerObject;
        public static MonoBehaviour ControllerInstance;

        public override void Load()
        {
            Log = base.Log;

            NumberOfTry = Config.Bind("Debug", "When character studio is opened, this mod will try in loop to find the height slider until it it gets created and the mod finds it. This is the max ammount of try allowed. The default 300 should be 100s at 60fps. You can try to increase this number if the height slidder doesnt appear and you have long loadings", 300);

            ClassInjector.RegisterTypeInIl2Cpp<HeightModController>();

            ControllerObject = new GameObject("HeightModGameObject");
            GameObject.DontDestroyOnLoad(ControllerObject);
            ControllerObject.hideFlags = HideFlags.HideAndDontSave;
            ControllerObject.AddComponent<HeightModController>();

            Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
        }
    }

    public class HeightModController : MonoBehaviour
    {
        public void Awake() => HeightMod.ControllerInstance = this;
    }

    internal static class Hooks 
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomBase), nameof(CustomBase.Awake))]
        public static void Ennable_MaleHeightSlider()
        {
            MonoBehaviourExtensions.StartCoroutine(HeightMod.ControllerInstance, DelayEnnableMaleHeightSlider());

            IEnumerator DelayEnnableMaleHeightSlider()
            {
                for (int i = HeightMod.NumberOfTry.Value; i > 0; i--)
                {
                    for (int j = 20;  j > 0; j--) yield return null;

                    Transform heightSlider = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_ShapeWhole/Scroll View/Viewport/Content")?.transform.GetChild(0);
                    if (heightSlider != null)
                    {
                        heightSlider.gameObject.SetActive(true);
                        yield break;
                    }
                }
            }
        }
        
        public static Dictionary<int, float> CapturedHeight = new Dictionary<int, float>();

        [HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.Initialize))]
        public static void Capture_InitialMaleHeight(byte _sex, int _id, ChaFileControl _chaFile = null)
        {
            if (_sex == 0 && _chaFile != null)
                CapturedHeight.Add(_id, _chaFile.Custom.body.shapeValueBody[0]);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.Initialize))]
        public static void Cancel_InitializeMaleHeightOverride(ChaControl __instance, byte _sex, int _id, ChaFileControl _chaFile = null)
        {
            if (_sex == 0 && _chaFile != null && CapturedHeight.ContainsKey(_id))
            {
                __instance.ChaFile.Custom.body.shapeValueBody[0] = CapturedHeight[_id];
                CapturedHeight.Remove(_id);
            }
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(ChaControl), nameof(ChaControl.InitShapeBody)),
            HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetShapeBodyValue)),
            HarmonyPatch(typeof(ChaControl), nameof(ChaControl.UpdateShapeBodyValueFromCustomInfo))]
        public static void Cancel_MaleHeightOverride(ChaControl __instance)
        {
            if (__instance.Sex == 0)
            {
                __instance.SIBBody.ChangeValue(0, __instance.ChaFile.Custom.body.shapeValueBody[0]);
                ChaInfo charaInfo = (ChaInfo)__instance;
                charaInfo.UpdateShapeBody = true;
            }
        }
    }
}

/* Keeping this code for futur ffeature for a size slider on both male and female

ClassInjector.RegisterTypeInIl2Cpp<HeightModCharaController>();

__instance.gameObject.AddComponent<HeightModCharaController>();

public class HeightModCharaController : MonoBehaviour
{
    public ChaControl Chara { get; private set; }

    protected void Awake() => MonoBehaviourExtensions.StartCoroutine(HeightMod.ControllerInstance, DelayApplyMaleHeight());

    public IEnumerator DelayApplyMaleHeight()
    {
        Chara = GetComponent<ChaControl>();

        for (int i = 0; i < 60; i++) yield return null;

        PluginData data = ExtendedSave.GetExtendedDataById(Chara.ChaFile, HeightMod.GUID);
        if (data != null && data.data.TryGetValue("SizeSaved", out var val) && val is float fVal)
            Chara.ChaFile.Custom.body.shapeValueBody[0] = fVal;

        ChaInfo charaInfo = (ChaInfo)Chara;
        charaInfo.UpdateShapeBody = true;

        Events.CardBeingSaved += OnCardBeingSaved;
        HeightMod.ApplyHeight.SettingChanged += (sender, e) => charaInfo.UpdateShapeBody = true;
    }

    public void OnCardBeingSaved(ChaFile charaFile)
    {
        if (charaFile == Chara.ChaFile)
        {
            PluginData data = new PluginData();
            data.data.Add("SizeSaved", Chara.ChaFile.Custom.body.shapeValueBody[0]);
            data.version = 1;
            ExtendedSave.SetExtendedDataById(Chara.ChaFile, HeightMod.GUID, data);
        }
    }
}*/
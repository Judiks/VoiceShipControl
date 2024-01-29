using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using VoiceShipControll.Helpers;
using VoiceShipControll.Patches;

namespace VoiceShipControll
{
    [BepInPlugin(_modGUID, _modName, _modVersion)]
    public class VoiceShipControll : BaseUnityPlugin
    {
        private const string _modGUID = "Judik.VoiceShipControll";
        private const string _modName = "Voice Ship Controll";
        private const string _modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(_modGUID);

        public static VoiceShipControll Instance;
        public static event Action<bool> OnAssambliesLoaded;
        internal ManualLogSource _logger;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                return;
            }
            _logger = BepInEx.Logging.Logger.CreateLogSource(_modGUID);
            _logger.LogInfo("Judik.VoiceShipControll has started");
            harmony.PatchAll(typeof(StartOfRoundPatch));
        }

        // parsing not dinamic values
        public void ParseVoiceShipControllSettings()
        {
            // parsing not dinamic values
            PluginConstants.LanguageCode = JsonReader.GetValue(PluginConstants.LanguageCodeKey);
            _logger.LogInfo("Parsed LanguageCode: " + PluginConstants.LanguageCode);
        }
    }
}

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
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
            } else
            {
                return;
            }
            _logger = BepInEx.Logging.Logger.CreateLogSource(_modGUID);
            _logger.LogInfo("Judik.VoiceShipControll has started");
            // Make sure instead of failing we load the System.Speech Library from the embedded resources.

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string[] resources = executingAssembly.GetManifestResourceNames();
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            foreach (string resource in resources)
            {
                if (resource.EndsWith("Speech.dll"))
                {
                    using (Stream stream = executingAssembly.GetManifestResourceStream(resource))
                    {
                        if (stream == null)
                            continue;

                        byte[] assemblyRawBytes = new byte[stream.Length];
                        stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                        try
                        {
                            Assembly.Load(assemblyRawBytes);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.Print("Failed to load: " + resource + " Exception: " + ex.Message);
                        }
                    }
                }
            }
            
            harmony.PatchAll(typeof(StartOfRoundPatch));
        }

        private  Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {

            _logger.LogInfo("Importing " + args.Name);
            String resourceName = _modName + "." + new AssemblyName(args.Name).Name + ".dll";
            _logger.LogInfo("Located at: " + resourceName);

            if (Assembly.GetExecutingAssembly().GetManifestResourceInfo(resourceName) == null) return null;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                _logger.LogInfo("Found file! Length: " + stream.Length);

                byte[] assemblyData = new byte[stream.Length];

                stream.Read(assemblyData, 0, assemblyData.Length);

                try
                {
                    Assembly loaded = Assembly.Load(assemblyData);
                    _logger.LogInfo($"Loaded {loaded.FullName}");


                    return loaded;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to load assembly: " + ex.Message + "\n" + ex.ToString());
                    return null;
                }
            }
        }
    }
}

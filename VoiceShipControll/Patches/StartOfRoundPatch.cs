using Dissonance;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Speech.Recognition;
using System.Threading;
using UnityEngine;
using Microsoft.Win32;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using VoiceShipControll.Helpers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Object = UnityEngine.Object;
using UnityEngine.Windows;
using GameNetcodeStuff;
using System.Threading.Tasks;
using System.Text;

namespace VoiceShipControll.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch : MonoBehaviour
    {
        public static object recognition;
        public static Recognizer recognizer;
        [HarmonyPatch(nameof(StartOfRound.StartTrackingAllPlayerVoices))]
        [HarmonyPostfix]
        static void AddSoundRecognition(StartOfRound __instance)
        {
            Debug.Log("Starting voice ship controll tracking for player: " + __instance.voiceChatModule.LocalPlayerName);
            ParseVoiceShipControllSettings();


            var recognizer = new Recognizer();
            recognizer.Start();
            //InitializeSpeachToText(true);

            //var thread = new Thread(() =>
            //{
            //    Task.Run(async () =>


            //});
            //thread.Start();
            //__instance.localPlayerController.voicePlayerState = __instance.voiceChatModule.FindPlayer(__instance.voiceChatModule.LocalPlayerName);
            //__instance.localPlayerController.voicePlayerState.OnStartedSpeaking += MicrophoneLisrenerStart;
            //__instance.localPlayerController.voicePlayerState.OnStoppedSpeaking += MicrophoneLisrenerStop;
        }

        public static void ParseVoiceShipControllSettings()
        {
            Debug.Log("Parse VoiceShipControll Settings");
            JObject json = JObject.Parse(File.ReadAllText($"{PlaginConstants.PathToFolder}\\VoiceShipControllSettings.json"));
            PlaginConstants.JarviceVoiceCommands = JObject.FromObject(json["jarvice-voice-commands"]).ToObject<Dictionary<string, string>>();
            Debug.Log("Parsed JarviceVoiceCommands Count: " + PlaginConstants.JarviceVoiceCommands.Count);
            PlaginConstants.TerminalVoiceCommands = JObject.FromObject(json["terminal-voice-commands"]).ToObject<Dictionary<string, string>>();
            Debug.Log("Parsed TerminalVoiceCommands Count: " + PlaginConstants.TerminalVoiceCommands.Count);
            PlaginConstants.JarvisVoiceAssets = JObject.FromObject(json["jarvice-voice-assets"]).ToObject<Dictionary<string, string>>();
            Debug.Log("Parsed JarvisVoiceAssets Count: " + PlaginConstants.JarvisVoiceAssets.Count);
            PlaginConstants.LanguageCode = json.Value<string>("language-code");
            Debug.Log("Parsed LanguageCode: " + PlaginConstants.LanguageCode);
        }

        public static void InitializeSpeachToText(bool isLoaded)
        {
            Debug.Log("Initialize Speech Recognizer");
            recognition = new SpeechRecognitionEngine();
            var recognitionInstance = (SpeechRecognitionEngine)recognition;
            Debug.Log("Start tracking voice");
            recognitionInstance.SetInputToDefaultAudioDevice();
            try
            {
                var values = new List<string>();
                values.AddRange(PlaginConstants.TerminalVoiceCommands.Values);
                values.AddRange(PlaginConstants.JarviceVoiceCommands.Values);
                // Create a GrammarBuilder with predefined choices
                if (values.Count == 0)
                {
                    Debug.Log("No voice commands");
                    return;
                }
                GrammarBuilder grammarBuilder = new GrammarBuilder(new Choices(values.ToArray()));
                grammarBuilder.Culture = recognitionInstance.RecognizerInfo.Culture;
                // Create a Grammar with the GrammarBuilder
                Grammar grammar = new Grammar(grammarBuilder);

                // Load the Grammar into the SpeechRecognitionEngine
                recognitionInstance.LoadGrammar(grammar);

                // Subscribe to the SpeechRecognized event
                recognitionInstance.SpeechRecognized += SpeechRecognized;
                // Start speech recognition
                recognitionInstance.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }

        public static void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence < 0.6)
            {
                return;
            }
            string spokenText = e.Result.Text;
            Debug.Log("Speech Recognized: " + spokenText);
            if (PlaginConstants.JarvisVoiceAssets.ContainsKey(spokenText))
            {
                var assetName = string.Empty;
                PlaginConstants.JarvisVoiceAssets.TryGetValue(spokenText, out assetName);
                if (string.IsNullOrEmpty(assetName)) { return; }
                ShipCommands.PlayJarvisVoice(assetName);
            }
            if (PlaginConstants.TerminalVoiceCommands.Values.Any(value => value == spokenText))
            {
                ShipCommands.ApplyTerminalCommand(spokenText);
            }
        }
    }
}

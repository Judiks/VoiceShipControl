using HarmonyLib;
using System;
using System.IO;
using System.Speech.Recognition;
using UnityEngine;
using Debug = UnityEngine.Debug;
using VoiceShipControll.Helpers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace VoiceShipControll.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch : MonoBehaviour
    {
        public static object recognition;

        [HarmonyPatch(nameof(StartOfRound.StartTrackingAllPlayerVoices))]
        [HarmonyPostfix]
        static async void AddSoundRecognition(StartOfRound __instance)
        {
            Debug.Log("Starting voice ship controll tracking for player: " + __instance.voiceChatModule.LocalPlayerName);

            SocketListener.InitSocketListener();
            SocketListener.OnErrorReceivedEvent += Recognizer.PythonError;
            SocketListener.OnMessageReceivedEvent += Recognizer.PythonSpeechRecognized;
            Recognizer.InitRecognizer();
            //InitializeSpeachToText(true);
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

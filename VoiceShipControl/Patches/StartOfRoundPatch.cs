using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Security.AccessControl;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoiceShipControl.Helpers;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace VoiceShipControl.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch : NetworkBehaviour
    {
        public static StartOfRound Instance;
        public static bool IsRecognitionEnabled;

        [HarmonyPatch(nameof(StartOfRound.LateUpdate))]
        [HarmonyPostfix]
        static void AddSoundRecognition(StartOfRound __instance)
        {
            Instance = __instance;
            if (Instance == null)
            {
                return;
            }
            if (__instance.gameObject.activeSelf && !IsRecognitionEnabled)
            {
                IsRecognitionEnabled = true;
                EnambleRecognition();
                var audioClip = AssetLoader.Load<AudioClip>(PluginConstants.ShipIntroAudioAssetName.Value);
                if (audioClip.Bundle != null && audioClip.Result != null)
                {
                    __instance.shipIntroSpeechSFX = audioClip.Result;
                }
            }
        }

        private static void EnambleRecognition()
        {
            if (SocketListener.Instance == null)
            {
                Debug.Log("SocketListener not founded initializing");
                SocketListener.InitSocketListener();
                SocketListener.OnErrorReceivedEvent += Recognizer.PythonError;
                SocketListener.OnMessageReceivedEvent += Recognizer.PythonSpeechRecognized;
            }
            else
            {
                SocketListener.StartServer();
            }
            if (Recognizer.Instance == null)
            {
                Debug.Log("Recognizer not founded initializing");
                Recognizer.InitRecognizer();

            }
            else
            {
                Recognizer.IsStarted = Recognizer.IsProcessStarted = false;
            }
        }

        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPostfix]
        static void PlayStartGameAudio()
        {
            var terminal = Object.FindObjectOfType<Terminal>();
            //terminal.groupCredits = 100000;
            AudioClipHelper.PlayAudioSourceByValue(PluginConstants.StartOfRoundAudioAssetName.Value, StartOfRound.Instance.speakerAudioSource);
        }

        [HarmonyPatch(nameof(StartOfRound.EndGameServerRpc))]
        [HarmonyPostfix]
        static void PlayEndGameAudio()
        {
            AudioClipHelper.PlayAudioSourceByValue(PluginConstants.EndOfRoundAudioAssetName.Value, StartOfRound.Instance.speakerAudioSource);
        }

        [HarmonyPatch(nameof(StartOfRound.OnDestroy))]
        [HarmonyPrefix]
        static void OnDestroy()
        {
            SocketListener.StopSocketListener();
            Recognizer.StopRecognizer();
            IsRecognitionEnabled = false;
            Debug.Log("recognizer stopped");
        }
    }
}
using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoiceShipControll.Helpers;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace VoiceShipControll.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch : NetworkBehaviour
    {
        public static StartOfRound Instance;
        public static bool IsRecognitionEnabled;

        [HarmonyPatch(nameof(StartOfRound.LateUpdate))]
        [HarmonyPostfix]
        static async void AddSoundRecognition(StartOfRound __instance)
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
                var audioClip = AssetLoader.Load<AudioClip>(PluginConstants.WelcomeJarvisKey);
                if (audioClip.Item2 != null && audioClip.Item1 != null) { 
                    __instance.shipIntroSpeechSFX = audioClip.Item1;   
                }
            }
        }

        private static void EnambleRecognition()
        {
            Debug.Log("Starting voice ship controll tracking for player: " + Instance.voiceChatModule.LocalPlayerName);
            if (SocketListener.Instance == null)
            {
                Debug.Log("SocketListener not founded initializing");
                SocketListener.InitSocketListener();
                SocketListener.OnErrorReceivedEvent += Recognizer.PythonError;
                SocketListener.OnMessageReceivedEvent += Recognizer.PythonSpeechRecognized;
            } else
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
        static async void PlayStartGameAudio()
        {
            AudioClipHelper.PlayAudioSourceByKey(PluginConstants.StartGameAudioKey, StartOfRound.Instance.speakerAudioSource);
        }

        [HarmonyPatch(nameof(StartOfRound.EndGameServerRpc))]
        [HarmonyPostfix]
        static async void PlayEndGameAudio()
        {
            AudioClipHelper.PlayAudioSourceByKey(PluginConstants.EndGameAudioKey, StartOfRound.Instance.speakerAudioSource);
        }

        [HarmonyPatch(nameof(StartOfRound.OnDestroy))]
        [HarmonyPrefix]
        static async void OnDestroy()
        {
            SocketListener.StopSocketListener();
            Recognizer.StopRecognizer();
            IsRecognitionEnabled = false;
            Debug.Log("recognizer stopped");
        }
    }
}

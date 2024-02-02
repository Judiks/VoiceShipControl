using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Security.AccessControl;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoiceShipControll.Helpers;
using static System.Runtime.CompilerServices.RuntimeHelpers;
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
        static void PlayStartGameAudio()
        {
            AudioClipHelper.PlayAudioSourceByValue(PluginConstants.StartOfRoundAudioAssetName.Value, StartOfRound.Instance.speakerAudioSource);
        }
        public static GameObject FindAllInChildren(GameObject parent)
        {
            // Check children recursively
            foreach (Transform child in parent.transform)
            {
                Debug.Log("Clicked objectName.name: " + child.gameObject.name);
                Debug.Log("Clicked objectName.transform.name: " + child.gameObject.transform.name);
                Debug.Log("Clicked objectName.transform.name: " + Vector3.Distance(UnityInput.Current.mousePosition, child.transform.position));
                var gameObject = FindAllInChildren(child.gameObject);
                if (gameObject != null)
                {
                    // ButtonInfo script found in one of the children
                    return gameObject;
                }
            }

            // ButtonInfo script not found in the current object or its children
            return null;
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
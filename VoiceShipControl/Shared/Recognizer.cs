using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace VoiceShipControl.Helpers
{
    internal class Recognizer : MonoBehaviour
    {
        public static Process recognitionProcess;
        public static Recognizer Instance;
        public static bool IsProcessStarted;
        public static bool IsStarted;
        public static void InitRecognizer(bool visible = false, string name = "Recognizer")
        {
            if (Instance != null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                // add an invisible game object to the scene
                GameObject obj = new GameObject();
                if (!visible)
                {
                    obj.hideFlags = HideFlags.HideAndDontSave;
                }
                DontDestroyOnLoad(obj);
                Instance = obj.AddComponent<Recognizer>();

                Debug.Log("Recognizer object created");
                Instantiate(Instance.gameObject, new Vector3(1, 1, 0), Quaternion.identity);
            }
        }

        public void Update()
        {
            if (Instance == null)
            {
                return;
            }
            if (!IsStarted)
            {
                IsStarted = true;
                Instance.StartCoroutine(Execute());
            }
        }

        // running py recognizer process
        public static IEnumerator Execute()
        {
            Debug.Log("Recognizer started");
            yield return new WaitForSeconds(0f);
            try
            {
                recognitionProcess = new Process();
                recognitionProcess.StartInfo.FileName = $"{PluginConstants.PathToFolder}\\dist\\recognizer\\recognizer.exe";
                recognitionProcess.StartInfo.Arguments = $"\"{PluginConstants.LanguageCode}\"";
                recognitionProcess.StartInfo.CreateNoWindow = true;
                recognitionProcess.StartInfo.UseShellExecute = false;
                recognitionProcess.Exited += (object sender, EventArgs e) =>
                {
                    Debug.LogWarning("Process exited");
                };
                recognitionProcess.Start();
                IsProcessStarted = true;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

        }

        // if you want add timeot for microphone listener in py script it's will be helpfull
        public static void PythonError(string error, EventArgs e)
        {
            Console.WriteLine(error);
            if (error.Contains("speech_recognition.exceptions.WaitTimeoutError"))
            {
                IsStarted = false;
            }
        }

        // position of 'if' important for stable app work
        public static void PythonSpeechRecognized(string spokenText, EventArgs e)
        {
            if (!StartOfRound.Instance.localPlayerController.isPlayerDead || PluginConstants.IsUserCanUseCommandsWhenDead.Value)
            {
                Console.WriteLine($"Spocken text: {spokenText}");
                if (StartOfRound.Instance.localPlayerController.isInHangarShipRoom || PluginConstants.IsUserCanUseCommandsOutsideTheShip.Value)
                {
                    if (DetectAndRunStartGameVoiceCommand(spokenText))
                    {
                        return;
                    }
                    if (DetectAndRunShipDoorVoiceCommand(spokenText))
                    {
                        return;
                    }
                    if (DetectAndRunSwitchVoiceCommand(spokenText))
                    {
                        return;
                    }
                }

                DetectAndRunVoiceCommand(spokenText);

                if (StartOfRound.Instance.localPlayerController.isInHangarShipRoom || PluginConstants.IsUserCanUseCommandsOutsideTheShip.Value)
                {
                    if (DetectAndRunBuyCommand(spokenText))
                    {
                        return;
                    }
                    if (DetectAndRunTransmitCommand(spokenText))
                    {
                        return;
                    }
                    if (DetectAndRunRerouteCommand(spokenText))
                    {
                        return;
                    }
                    DetectAndRunTerminalCommand(spokenText);
                }
            }
        }

        public static bool DetectAndRunStartGameVoiceCommand(string spokenText)
        {
            if (PluginConstants.StartGame.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                StartOfRound.Instance.StartGameServerRpc();
                return true;
            }
            return false;
        }

        public static bool DetectAndRunShipDoorVoiceCommand(string spokenText)
        {
   
            if (PluginConstants.CloseDoor.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                HangarShipDoor door = FindFirstObjectByType<HangarShipDoor>();

                if (door != null)
                {
                    InteractTrigger trigger = door.transform.Find("HangarDoorButtonPanel/StopButton/Cube (3)").GetComponent<InteractTrigger>();
                    trigger.Interact(GameNetworkManager.Instance.localPlayerController.transform);
                }
                return true;
            }
            if (PluginConstants.OpenDoor.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                HangarShipDoor door = FindFirstObjectByType<HangarShipDoor>();

                if (door != null)
                {
                    InteractTrigger trigger = door.transform.Find("HangarDoorButtonPanel/StartButton/Cube (2)").GetComponent<InteractTrigger>();
                    trigger.Interact(GameNetworkManager.Instance.localPlayerController.transform);
                }
                return true;
            }
            return false;
        }

        public static bool DetectAndRunSwitchVoiceCommand(string spokenText)
        {

            if (PluginConstants.SwitchOn.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                var shipLight = FindObjectOfType<ShipLights>();
                shipLight.SetShipLightsServerRpc(true);
                return true;
            }
            if (PluginConstants.SwitchOff.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                var shipLight = FindObjectOfType<ShipLights>();
                shipLight.SetShipLightsServerRpc(false);
                return true;
            }
            return false;
        }

        public static bool DetectAndRunVoiceCommand(string spokenText)
        {
            var voiceCommand = PluginConstants.VoiceCommands.FirstOrDefault(x => x.Value.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())));
            if (voiceCommand.Value != null && !string.IsNullOrEmpty(voiceCommand.Value.Value) && !string.IsNullOrEmpty(voiceCommand.Key))
            {
                var audioSource = StartOfRound.Instance.localPlayerController.gameObject.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = StartOfRound.Instance.localPlayerController.gameObject.AddComponent<AudioSource>();
                }
                AudioClipHelper.PlayValuePairAudioSourceByKey(voiceCommand.Key, audioSource);
                return true;
            }
            return false;
        }

        public static bool DetectAndRunBuyCommand(string spokenText)
        {
            if (PluginConstants.BuyKeyPhrase.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                var buyVoiceCommandNameValuePair = PluginConstants.BuyVoiceCommands.FirstOrDefault(x => x.Value.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())));
                if (buyVoiceCommandNameValuePair.Value != null && !string.IsNullOrEmpty(buyVoiceCommandNameValuePair.Value.Value) && !string.IsNullOrEmpty(buyVoiceCommandNameValuePair.Key))
                {
                    var count = string.Empty;
                    var buyVoiceCountCommandNameValuePair = PluginConstants.BuyVoiceCountCommands.FirstOrDefault(x => x.Value.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())));
                    if (buyVoiceCountCommandNameValuePair.Value != null && !string.IsNullOrEmpty(buyVoiceCountCommandNameValuePair.Value.Value) && !string.IsNullOrEmpty(buyVoiceCountCommandNameValuePair.Key))
                    {
                        count = buyVoiceCountCommandNameValuePair.Key;
                        Console.WriteLine(count + " buy count command");
                    }
                    ShipCommands.BuyCommand($"{buyVoiceCommandNameValuePair.Key} {count}");
                    return true;
                }
            }
            return false;
        }

        public static bool DetectAndRunTerminalCommand(string spokenText)
        {
            var terminalVoiceCommandNameValuePair = PluginConstants.TerminalVoiceCommands.FirstOrDefault(x => x.Value.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())));
            if (terminalVoiceCommandNameValuePair.Value != null && !string.IsNullOrEmpty(terminalVoiceCommandNameValuePair.Value.Value) && !string.IsNullOrEmpty(terminalVoiceCommandNameValuePair.Key))
            {
                ShipCommands.TerminalCommand(terminalVoiceCommandNameValuePair.Key);
                return true;
            }
            return false;
        }

        public static bool DetectAndRunTransmitCommand(string spokenText)
        {
            var keyPhrase = PluginConstants.TransmitKeyPhrase.Value.Split('|').FirstOrDefault(y => spokenText.ToLower().Contains(y.ToLower()));
            if (!string.IsNullOrEmpty(keyPhrase))
            {
                var endIndexOfSpaceKeyPhrase = spokenText.LastIndexOf(keyPhrase);
                var newText = spokenText.Substring(endIndexOfSpaceKeyPhrase);
                var startsWithWhiteSpace = char.IsWhiteSpace(newText, 0); // 0 = first character
                if (startsWithWhiteSpace)
                {
                    newText.Remove(0);
                } else
                {
                    var startIndexOfSpace = newText.IndexOf(" ");
                    newText = spokenText.Substring(startIndexOfSpace);
                }
                ShipCommands.TerminalCommand($"transmit {newText}");
                return true;
            }
            return false;
        }

        public static bool DetectAndRunRerouteCommand(string spokenText)
        {
            if (PluginConstants.RerouteKeyPhrase.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                var rerouteVoiceCommandNameValuePair = PluginConstants.ReroutePlanetComands.FirstOrDefault(x => x.Value.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())));
                if (rerouteVoiceCommandNameValuePair.Value != null && !string.IsNullOrEmpty(rerouteVoiceCommandNameValuePair.Value.Value) && !string.IsNullOrEmpty(rerouteVoiceCommandNameValuePair.Key))
                {
                    ShipCommands.RerouteCommand(rerouteVoiceCommandNameValuePair.Key);
                    return true;
                }
            }
            return false;
        }

        public static void StopRecognizer()
        {
            recognitionProcess.Kill();
            IsProcessStarted = IsStarted = false;

        }

        void OnApplicationQuit()
        {
            StopRecognizer();
        }
    }
}

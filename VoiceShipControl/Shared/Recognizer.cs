using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;
using VoiceShipControl.Shared;
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
                Instance = obj.AddComponent<Recognizer>();
                Debug.Log("Recognizer object created");
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
            var languageCode = PluginConstants.LanguageDictionary.GetValueOrDefault(PluginConstants.LanguageCode.Value);
            if (!string.IsNullOrEmpty(languageCode))
            {
                IsProcessStarted = true;
            }
            else
            {
                Debug.LogWarning("Recognizer not started Language incorrect!");
                yield return null;
            }
            yield return new WaitForSeconds(0f);
            try
            {
                Debug.Log("Current lang: " + languageCode);
                recognitionProcess = new Process();
                recognitionProcess.StartInfo.FileName = FileHelper.GetFilePath("recognizer.exe");
                recognitionProcess.StartInfo.Arguments = $"\"{languageCode}\"";
                recognitionProcess.StartInfo.CreateNoWindow = true;
                recognitionProcess.StartInfo.UseShellExecute = false;
                recognitionProcess.Exited += (object sender, EventArgs e) =>
                {
                    Debug.LogWarning("Process exited");
                };
                recognitionProcess.Start();
                Debug.Log("Recognizer started");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

        }

        // if you want add timeot for microphone listener in py script it's will be helpfull
        public static void PythonError(string error, EventArgs e)
        {
            Debug.LogError(error);
        }

        // position of 'if' important for stable app work
        public static void PythonSpeechRecognized(string spokenText, EventArgs e)
        {
            if (!StartOfRound.Instance.localPlayerController.isPlayerDead || PluginConstants.IsUserCanUseCommandsWhenDead.Value)
            {
                Console.WriteLine($"Spocken text: {spokenText}");
                if (StartOfRound.Instance.localPlayerController.isInHangarShipRoom || PluginConstants.IsUserCanUseCommandsOutsideTheShip.Value)
                {
                    if (DetectAndRunStartEndGameVoiceCommand(spokenText))
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
                    if (DetectAndRunTeleporterVoiceCommand(spokenText))
                    {
                        return;
                    }                    
                    if (DetectAndRunMonitorVoiceCommand(spokenText))
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

        public static bool DetectAndRunStartEndGameVoiceCommand(string spokenText)
        {
            if (PluginConstants.StartGame.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                StartOfRound.Instance.StartGameServerRpc();
                return true;
            }
            if (PluginConstants.EndGame.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                StartOfRound.Instance.EndGameServerRpc((int)StartOfRound.Instance.localPlayerController.playerClientId);
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

            var shipLight = FindObjectOfType<ShipLights>();
            if (PluginConstants.SwitchOn.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                shipLight.SetShipLightsServerRpc(true);
                return true;
            }
            if (PluginConstants.SwitchOff.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                shipLight.SetShipLightsServerRpc(false);
                return true;
            }
            return false;
        }

        public static bool DetectAndRunMonitorVoiceCommand(string spokenText)
        {
            if (PluginConstants.SwitchMonitor.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                InteractTrigger trigger = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001/CameraMonitorSwitchButton/Cube (2)").GetComponent<InteractTrigger>();
                if (trigger != null)
                {
                    trigger.Interact(GameNetworkManager.Instance.localPlayerController.transform);
                }
                return true;
            }
            if (PluginConstants.ToggleMonitor.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                InteractTrigger trigger = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001/CameraMonitorOnButton/Cube (2)").GetComponent<InteractTrigger>();
                if (trigger != null)
                {
                    trigger.Interact(GameNetworkManager.Instance.localPlayerController.transform);
                }
                return true;
            }
            return false;
        }

        public static bool DetectAndRunTeleporterVoiceCommand(string spokenText)
        {
            var teleporters = FindObjectsOfType<ShipTeleporter>();
            var teleporter = teleporters.FirstOrDefault(x => !x.isInverseTeleporter);
            var inverseTeleporter = teleporters.FirstOrDefault(x => x.isInverseTeleporter);
            if (PluginConstants.Teleporter.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                if (teleporter.isActiveAndEnabled && (inverseTeleporter.buttonTrigger.interactable || PluginConstants.IsUserCanUseTeleportAlways.Value))
                {
                    teleporter.PressTeleportButtonOnLocalClient();
                }
                return true;
            }
            if (PluginConstants.InverseTeleporter.Value.Split('|').Any(y => spokenText.ToLower().Contains(y.ToLower())))
            {
                if (inverseTeleporter.isActiveAndEnabled && (inverseTeleporter.buttonTrigger.interactable || PluginConstants.IsUserCanUseTeleportAlways.Value))
                {
                    inverseTeleporter.PressTeleportButtonOnLocalClient();
                    return true;
                }
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
            Debug.Log("Stop recognizer");
            IsProcessStarted = IsStarted = false;
            recognitionProcess.Kill();

        }

        void OnDestroy()
        {
            Debug.Log("Destriy recognizer");
            StopRecognizer();
        }

        void OnApplicationQuit()
        {
            StopRecognizer();
            Destroy(this);
        }
    }
}

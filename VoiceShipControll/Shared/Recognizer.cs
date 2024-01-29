using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace VoiceShipControll.Helpers
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
            Console.WriteLine(spokenText);

            var voiceCommands = JsonReader.GetKeyValuePairs(PluginConstants.VoiceCommandsKey);
            var voiceCommand = voiceCommands.FirstOrDefault(x => spokenText.ToLower().Contains(x.Value.ToLower()));
            if (!string.IsNullOrEmpty(voiceCommand.Value) && !string.IsNullOrEmpty(voiceCommand.Key))
            {
                var audioSource = StartOfRound.Instance.localPlayerController.gameObject.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = StartOfRound.Instance.localPlayerController.gameObject.AddComponent<AudioSource>();
                }
                AudioClipHelper.PlayValuePairAudioSourceByKey(voiceCommand.Key, audioSource);
            }
            var buyKeyword = JsonReader.GetValue(PluginConstants.BuyKeywordKey);
            var buyCommands = JsonReader.GetKeyValuePairs(PluginConstants.BuyVoiceCommandsKey);
            var buyCommand = buyCommands.FirstOrDefault(x => spokenText.ToLower().Contains(x.Value.ToLower()));
            if (!string.IsNullOrEmpty(buyCommand.Key) && spokenText.Contains(buyKeyword))
            {
                Console.WriteLine(spokenText + " buy command");
                ShipCommands.BuyCommand(buyCommand.Key);
                return;
            }
            var terminalVoiceCommands = JsonReader.GetKeyValuePairs(PluginConstants.TerminalVoiceCommandsKey);
            var terminalVoiceCommand = terminalVoiceCommands.FirstOrDefault(x => spokenText.ToLower().Contains(x.Value.ToLower()));
            if (!string.IsNullOrEmpty(terminalVoiceCommand.Value) && !string.IsNullOrEmpty(terminalVoiceCommand.Key))
            {
                Console.WriteLine(spokenText + " terminal command");
                ShipCommands.ApplyTerminalCommand(terminalVoiceCommand.Key);
            }

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


using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

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
            if (!IsStarted)
            {
                IsStarted = true;
                Instance.StartCoroutine(Execute());
            }
        }


        public static IEnumerator Execute()
        {
            yield return new WaitForSeconds(0f);
            try
            {
                recognitionProcess = Process.Start($"{PlaginConstants.PathToFolder}\\dist\\recognizer\\recognizer.exe", $"\"en-US\"");
                recognitionProcess.Exited += (object sender, EventArgs e) =>
                {
                    Console.WriteLine("Process exited");
                };
                Console.WriteLine("Execute Recognizer started");
                recognitionProcess.Start();
                IsProcessStarted = true;
                Console.WriteLine("Recognizer started");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

        }




        public static void PythonError(string error, EventArgs e)
        {
            Console.WriteLine(error);
            if (error.Contains("speech_recognition.exceptions.WaitTimeoutError"))
            {
                IsStarted = false;
            }
        }

        public static void PythonSpeechRecognized(string message, EventArgs e)
        {
                Console.WriteLine(message);
                string spokenText = message;
                var command = PlaginConstants.JarviceVoiceCommands.FirstOrDefault(x => spokenText.ToLower().Contains(x.Value.ToLower()));
                if (!string.IsNullOrEmpty(command.Value) && !string.IsNullOrEmpty(command.Key))
                {
                    Console.WriteLine(command.Value + " founded command");
                    var assetName = string.Empty;
                    PlaginConstants.JarvisVoiceAssets.TryGetValue(command.Value, out assetName);
                    Console.WriteLine(command.Value + " founded asset");
                    if (string.IsNullOrEmpty(assetName)) { return; }
                    ShipCommands.PlayJarvisVoice(assetName);
                }
                var bayCommand = PlaginConstants.TerminalBuyCommands.FirstOrDefault(x => spokenText.ToLower().Contains(x.ToLower()));
                if (!string.IsNullOrEmpty(bayCommand))
                {
                    Console.WriteLine(message + " buy command");
                    ShipCommands.BuyCommand(bayCommand);
                    return;
                }
                command = PlaginConstants.TerminalVoiceCommands.FirstOrDefault(x => spokenText.ToLower().Contains(x.Value.ToLower()));
                if (!string.IsNullOrEmpty(command.Value) && !string.IsNullOrEmpty(command.Key))
                {
                    Console.WriteLine(message + " terminal command");
                    ShipCommands.ApplyTerminalCommand(command.Key);
                }
                
        }
        static void OnApplicationQuit()
        {
            recognitionProcess.Kill();
        }
    }
}

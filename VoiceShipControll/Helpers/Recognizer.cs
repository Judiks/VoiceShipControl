using Microsoft.Build.Tasks;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace VoiceShipControll.Helpers
{
    internal class Recognizer : MonoBehaviour
    {
        private SynchronizationContext _synchronizationContext;
        Thread _socketServerThread;
        Thread _recognizerThread;
        SocketListener _listener;
        Process recognitionProcess;
        public void Start()
        {
            _synchronizationContext = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
            _socketServerThread = new Thread(() =>
            {
                _listener = new SocketListener();
                _listener.OnErrorReceivedEvent += PythonError;
                _listener.OnMessageReceivedEvent += PythonSpeechRecognized;
                _listener.StartServer();
                _listener.StartBroadcasting();

            });
            _recognizerThread = new Thread(() =>
            {
                Execute();
            });

            _recognizerThread.Start();
            _socketServerThread.Start();
            Console.WriteLine("Recognizer started");
        }

        public void Execute()
        {
            recognitionProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $"\"{PlaginConstants.PathToFolder}\\Python312\\python.exe\"",
                    Arguments = $"\"{PlaginConstants.PathToFolder}\\recognizer.py\" \"en-US\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
            try
            {
                recognitionProcess.Exited += (object sender, EventArgs e) =>
                {
                    Console.WriteLine("Process exited");
                };
                Console.WriteLine("Execute Recognizer started");
                recognitionProcess.Start();
                recognitionProcess.WaitForExit(10000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }




        public void PythonError(string error, EventArgs e)
        {
            _synchronizationContext.Post((state) =>
            {
                Console.WriteLine(error);
                if (error.Contains("speech_recognition.exceptions.WaitTimeoutError"))
                {
                    Execute();
                }
            }, null);
        }

        public void PythonSpeechRecognized(string message, EventArgs e)
        {
            _synchronizationContext.Post((state) =>
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
                
            }, null);
        }
        void OnApplicationQuit()
        {
            recognitionProcess.Kill();
        }
    }
}

using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace VoiceShipControll.Helpers
{
    internal class ShipCommands
    {

        public static void ApplyTerminalCommand(string inputText)
        {
            var terminal = Object.FindObjectOfType<Terminal>();
            if (HUDManager.Instance == null || GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null || terminal == null)
            {
                Console.WriteLine("terminal is null");
                return;
            }
            terminal.terminalInUse = true;
            GameNetworkManager.Instance.localPlayerController.inTerminalMenu = true;
            terminal.LoadNewNode(terminal.terminalNodes.specialNodes[13]);
            terminal.TextChanged(string.Empty);
            terminal.SetTerminalInUseLocalClient(inUse: true);
            terminal.screenText.text = string.Empty;
            terminal.currentText = string.Empty;
            terminal.textAdded = 0;
            terminal.TextChanged(inputText);
            terminal.screenText.text = inputText;
            terminal.OnSubmit();
        }

        public static void BuyCommand(string inputText)
        {
            var terminal = Object.FindObjectOfType<Terminal>();
            if (HUDManager.Instance == null || GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null || terminal == null)
            {
                Console.WriteLine("terminal is null");
                return;
            }

            try
            {
                terminal.terminalInUse = true;
                GameNetworkManager.Instance.localPlayerController.inTerminalMenu = true;
                terminal.LoadNewNode(terminal.terminalNodes.specialNodes[13]);
                terminal.TextChanged(string.Empty);
                terminal.SetTerminalInUseLocalClient(inUse: true);
                terminal.screenText.text = string.Empty;
                terminal.currentText = string.Empty;
                terminal.textAdded = 0;
                terminal.TextChanged(inputText);
                terminal.screenText.text = inputText;
                terminal.OnSubmit();
                if (terminal.currentNode.name == "CannotAfford")
                {
                    terminal.QuitTerminal();
                    AudioClipHelper.PlayAudioSourceByKey(PluginConstants.BuyDeclinedAudioKey, StartOfRound.Instance.speakerAudioSource);
                    return;
                }
                terminal.TextChanged(string.Empty);
                terminal.screenText.text = string.Empty;
                terminal.currentText = string.Empty;
                terminal.textAdded = 0;
                terminal.TextChanged("confirm");
                terminal.screenText.text = "confirm";
                terminal.OnSubmit();
                if (terminal.currentNode.name != "ParserError1")
                {
                    AudioClipHelper.PlayAudioSourceByKey(PluginConstants.BuySuccessAudioKey, StartOfRound.Instance.speakerAudioSource);
                }
                terminal.QuitTerminal();
                //ShowAllTerminalData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} {ex.StackTrace}");
            }
        }

        // i made it just for dev purpose
        public static void ShowAllTerminalData()
        {
            var terminal = Object.FindObjectOfType<Terminal>();
            for (int j = 0; j < terminal.terminalNodes.allKeywords.Length; j++)
            {
                try
                {
                    Console.WriteLine($"allKeywords {j}: " + terminal.terminalNodes.allKeywords[j].name);
                    Console.WriteLine($"allKeywords {j}: " + terminal.terminalNodes.allKeywords[j].word);
                    Console.WriteLine($"allKeywords {j}: " + terminal.terminalNodes.allKeywords[j].defaultVerb);
                    Console.WriteLine($"allKeywords {j}: " + string.Join(",", terminal.terminalNodes.allKeywords[j].compatibleNouns.Select(x => x.result.name)));
                }
                catch (Exception)
                {
                    Console.WriteLine("option result not found");
                }

            }

            for (int i = 0; i < terminal.terminalNodes.specialNodes.Count; i++)
            {
                Console.WriteLine($"specialNodes {i}: " + terminal.terminalNodes.specialNodes[i].name);
                for (int j = 0; j < terminal.terminalNodes.specialNodes[i].terminalOptions.Length; i++)
                {
                    try
                    {
                        Console.WriteLine($"terminalOptions {i}: " + terminal.terminalNodes.specialNodes[i].terminalOptions[j].result.name);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("option result not found");
                    }
                }
            }

            for (int j = 0; j < terminal.currentNode.terminalOptions.Length; j++)
            {
                Console.WriteLine($"terminalOptions {j}: " + terminal.currentNode.terminalOptions[j].result.name);
            }

            for (int j = 0; j < terminal.currentNode.terminalOptions.Length; j++)
            {
                Console.WriteLine($"terminalOptions.noun {j}: " + terminal.currentNode.terminalOptions[j].noun.name);
                Console.WriteLine($"terminalOptions.noun {j}: " + terminal.currentNode.terminalOptions[j].noun.word);
            }
        }
    }
}

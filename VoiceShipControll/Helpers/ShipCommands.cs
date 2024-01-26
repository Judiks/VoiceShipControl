using System;
using UnityEngine;
using Object = UnityEngine.Object;
using HarmonyLib;
using Steamworks;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

namespace VoiceShipControll.Helpers
{
    internal class ShipCommands
    {
        private static bool usedTerminalThisSession = false;
        private static bool syncedTerminalValues = false;
        public static Dictionary<string, int> items = new Dictionary<string, int>
                    {
                        { "Walkie-Talkie", 0 },
                        { "Flashlight", 1 },
                        { "Shovel", 2 },
                        { "Lockpicker", 3 },
                        { "Pro", 4 },
                        { "Stun Grenade", 5 },
                        { "Boom Box", 6 },
                        { "Inhaler", 7 },
                        { "Stun Gun", 8 },
                        { "Jet Pack", 9 },
                        { "Extension Ladder", 10 },
                        { "Radar Booster", 11 }
                    };
        public static void PlayJarvisVoice(string assetName)
        {
            var result = AssetLoader.Load<AudioClip>(assetName);
            var audioSource = StartOfRound.Instance.localPlayerController.gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = StartOfRound.Instance.localPlayerController.gameObject.AddComponent<AudioSource>();
            }
            audioSource.clip = result.Item1;
            audioSource.Play();
            result.Item2.Unload(false);
        }

        public static void ApplyTerminalCommand(string inputText) {
            var terminal = Object.FindObjectOfType<Terminal>();
            if (inputText == "open terminal")
            {
                StartOfRound.Instance.localPlayerUsingController = true;
                terminal.BeginUsingTerminal();
                return;
            }
            if (inputText == "close terminal")
            {
                StartOfRound.Instance.localPlayerUsingController = false;
                terminal.QuitTerminal();
                return;
            }
            terminal.terminalInUse = true;

            terminal.textAdded = inputText.Length;
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
                Console.WriteLine("starting buing");
                var itemsToBuy = items.Where(x => x.Key.ToLower().Contains(inputText.ToLower())).Select(x => x.Value);
                Console.WriteLine(String.Join(" ", itemsToBuy) + " founded items");
                terminal.BuyItemsServerRpc(itemsToBuy.ToArray(), terminal.groupCredits, 0);
                Console.WriteLine(inputText + " buyed");
            } catch(Exception ex) {
                Console.WriteLine($"{ex.Message} {ex.StackTrace}");
            }
        }

    }
}

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using System;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using VoiceShipControl.Helpers;
using VoiceShipControl.Patches;
using static VoiceShipControl.PluginConstants;

namespace VoiceShipControl
{
    [BepInPlugin(_modGUID, _modName, _modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    public class VoiceShipControl : BaseUnityPlugin
    {
        private const string _modGUID = "Judik.VoiceShipControl";
        private const string _modName = "Voice Ship Control";
        private const string _modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(_modGUID);

        public static VoiceShipControl Instance;
        internal ManualLogSource _logger;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                return;
            }

            ConfigureVoiceShipControlSettings();
            _logger = BepInEx.Logging.Logger.CreateLogSource(_modGUID);
            _logger.LogInfo("Judik.VoiceShipControl has started");
            harmony.PatchAll(typeof(StartOfRoundPatch));
            AddSettingsMenu();
        }

        public void AddSettingsMenu()
        {
            var terminalKeyInput = new InputComponent()
            {
                Placeholder = "Terminal command (try use exist commands)",
            };
            var terminalValueInput = new InputComponent()
            {
                Placeholder = "Command key phrase",
            };
            var voicePhraseValueInput = new InputComponent()
            {
                Placeholder = "Command key phrase",
            };
            var voicePhraseAssetValueInput = new InputComponent()
            {
                Placeholder = "Asset name",
            };
            ModMenu.RegisterMod(new ModMenu.ModSettingsConfig
            {
                Name = "Voice Ship Controll",
                Id = _modGUID,
                Version = _modVersion,
                Description = "Here you can add your own key phrases with assets what should be played like an 'answer' to your in game 'assistant'. " + Environment.NewLine +
                "1. After any of 'Add' buttons click key phrase will be added to Lethal Config Settings, but not displayed untill game restart." + Environment.NewLine +
                "2. All key phrases can have multiple meaning. That's mean you can add more than 1 audio asset or phrase connected to same command. To separate it type '|' beetwen phrases/assets." + Environment.NewLine +
                "3. All key phrases stored in VoiceShipControl.json file. That's mean you can modify them directly there without this menu." + Environment.NewLine +
                "4. All assets what you can connect stored in VoiceShipControl/Assets folder." + Environment.NewLine +
                "5. For now there is no logic to remove commands, so if  you want to do it go to VoiceShipControl.json and just remove raw with that command what you want.",
                MenuComponents = new MenuComponent[]
                        {
                            new LabelComponent
                            {
                                Text = "Add terminal key phrases.",
                                FontSize = 14f
                            },
                            new HorizontalComponent
                            {
                                Children = new MenuComponent[]
                                {
                                    terminalKeyInput,
                                    terminalValueInput,
                                }
                            },
                            new ButtonComponent
                            {
                                Text = "Add",
                                OnClick = (self) =>
                                {
                                    JsonHelper.SetKeyValuePair(terminalKeyInput.Value, TerminalVoiceCommandsKey, terminalValueInput.Value);
                                    terminalValueInput.Value = string.Empty;
                                    terminalKeyInput.Value = string.Empty;
                                    TerminalVoiceCommands.Add(terminalKeyInput.Value, Config.Bind("Terminal voice commands",
                                        terminalKeyInput.Value,
                                        terminalValueInput.Value,
                                        "Key phrase what you need to say to make some action via terminal. You can separate text with '|' to add multiple options"));
                                }
                            },
                            new LabelComponent
                            {
                                Text = "Add custom voice commands.",
                                FontSize = 14f
                            },
                            new HorizontalComponent
                            {
                                Children = new MenuComponent[]
                                {
                                    new LabelComponent()
                                    {
                                        Text = "Custom voice Key phrase: " + (VoiceCommands.Count + 1),
                                        FontSize = 12f
                                    },
                                    terminalValueInput,
                                }
                            },
                            new ButtonComponent
                            {
                                Text = "Add",
                                OnClick = (self) =>
                                {
                                    JsonHelper.SetKeyValuePair("Custom voice Key phrase: " + VoiceCommands.Count, VoiceCommandsKey, voicePhraseValueInput.Value);
                                    voicePhraseValueInput.Value = string.Empty;
                                    VoiceCommands.Add("Custom Asset Key phrase: " + VoiceCommands.Count, Config.Bind("Any voice commands",
                                        "Custom voice Key phrase: " + (VoiceCommands.Count + 1),
                                        voicePhraseValueInput.Value,
                                        "Any Key phrase by which one will be played audio asset. You can separate text with '|' to add multiple options"));
                                }
                            },
                            new LabelComponent
                            {
                                Text = "Add custom voice asset.",
                                FontSize = 14f
                            },
                            new HorizontalComponent
                            {
                                Children = new MenuComponent[]
                                {
                                    new LabelComponent()
                                    {
                                        Text = "Custom voice asset for Key phrase: " + (VoiceCommands.Count + 1),
                                        FontSize = 12f
                                    },
                                    terminalValueInput,
                                }
                            },
                            new ButtonComponent
                            {
                                Text = "Add",
                                OnClick = (self) =>
                                {
                                    JsonHelper.SetKeyValuePair("Custom voice asset for Key phrase: " + VoicePlayAudioAssetNames.Count, VoiceCommandsKey, voicePhraseAssetValueInput.Value);
                                    voicePhraseValueInput.Value = string.Empty;
                                    VoicePlayAudioAssetNames.Add("Custom Asset Key phrase: " + VoicePlayAudioAssetNames.Count, Config.Bind("Played by voice audio assets",
                                        "Custom voice asset for Key phrase: " + (VoicePlayAudioAssetNames.Count + 1),
                                        voicePhraseAssetValueInput.Value,
                                        "Any audio asset what will play by 'Any voice command'. You can separate text with '|' to add multiple options"));
                                }
                            },
                        }
            });
        }

        // parsing not dinamic values
        public void ConfigureVoiceShipControlSettings()
        {
            LethalConfigManager.SetModDescription("Enjoy my friends <3");
            // parsing not dinamic values
            LanguageCode = Config.Bind("General",
                 "Language Code",
                 SupportedLanguages.English,
                 "Voice recognizer language code");
            VoiceActivationButton = Config.Bind("General",
                 "Voice Command Activation Button",
                 KeyCode.Mouse4,
                 "Voice command activation button"); 
            IsVoiceActivationButtonNeeded = Config.Bind("General",
                "Is voice activation button needed?",
                true,
                "If voice activation button needed then x if not empty");
            IsUserCanUseCommandsOutsideTheShip = Config.Bind("General",
                "Is user can use commands outside the ship?",
                false,
                "If user can use commands outside the ship then x if not empty");
            IsUserCanUseCommandsWhenDead = Config.Bind("General",
                "Is user can use commands when dead?",
                false,
                "If user can use commands outside the ship then x if not empty");
            BuyKeyPhrase = Config.Bind("Main key phrases",
                "Buy Key Phrase",
                "buy|need|waiting",
                "Key phrase what you need to say to make related purchase. You can separate text with '|' to add multiple options");

            RerouteKeyPhrase = Config.Bind("Main key phrases",
                "Reroute Key Phrase",
                "route|go to|next planet",
                "Key phrase what you need to say to make rerouting to other planet. You can separate text with '|' to add multiple options");

            TransmitKeyPhrase = Config.Bind("Main key phrases",
                "Transmit Key Phrase",
                "transmit|send|all",
                "Key phrase what you need to say to make message transmiting with 'Message transmitter'. You can separate text with '|' to add multiple options");

            var startGameCommand = JsonHelper.GetValue(PluginConstants.StartGameKey);
            StartGame = Config.Bind("Static commands",
                "Start game (host only)",
                startGameCommand,
                "Start game command. You can separate text with '|' to add multiple options");

            var openDoorCommand = JsonHelper.GetValue(PluginConstants.OpenDoorKey);
            OpenDoor = Config.Bind("Static commands",
                "Open ship door",
                openDoorCommand,
                "Open ship door command. You can separate text with '|' to add multiple options");

            var closeDoorCommand = JsonHelper.GetValue(PluginConstants.CloseDoorKey);
            CloseDoor = Config.Bind("Static commands",
                "Close ship door",
                closeDoorCommand,
                "Close ship door command. You can separate text with '|' to add multiple options");

            var switchOnCommand = JsonHelper.GetValue(PluginConstants.SwitchOnKey);
            SwitchOn = Config.Bind("Static commands",
                "Turn ON ship light",
                switchOnCommand,
                "Turn ON ship light command. You can separate text with '|' to add multiple options");

            var switchOffCommand = JsonHelper.GetValue(PluginConstants.SwitchOffKey);
            SwitchOff = Config.Bind("Static commands",
                "Turn OFF ship light",
                switchOffCommand,
                "Turn OFF ship light command. You can separate text with '|' to add multiple options");

            var shipIntroAudioAssetName = JsonHelper.GetValue(PluginConstants.ShipIntroAudioAssetNameKey);
            ShipIntroAudioAssetName = Config.Bind("Static assets",
                "Ship intro audio asset name",
                shipIntroAudioAssetName,
                "Any audio asset what will play instead default audio on ship from speacker. You can separate text with '|' to add multiple options");


            var buySuccessAudioAssetName = JsonHelper.GetValue(PluginConstants.BuySuccessAudioAssetNameKey);
            BuySuccessAudioAssetName = Config.Bind("Static assets",
                "Buy SUCCESS audio asset name",
                buySuccessAudioAssetName,
                "Any audio asset what will play if purchase was SUCCESS. You can separate text with '|' to add multiple options");


            var buyDeclinedAudioAssetName = JsonHelper.GetValue(PluginConstants.BuyDeclinedAudioAssetNameKey);
            BuyDeclinedAudioAssetName = Config.Bind("Static assets",
                "Buy DICLINED audio asset name",
                buyDeclinedAudioAssetName,
                "Any audio asset what will play if purchase was DICLINED. You can separate text with '|' to add multiple options");


            var startOfRoundAudioAssetName = JsonHelper.GetValue(PluginConstants.StartOfRoundAudioAssetNameKey);
            StartOfRoundAudioAssetName = Config.Bind("Static assets",
                "Start of round audio asset name",
                startOfRoundAudioAssetName,
                "Any audio asset what will play when round is started. You can separate text with '|' to add multiple options");


            var endOfRoundAudioAssetName = JsonHelper.GetValue(PluginConstants.EndOfRoundAudioAssetNameKey);
            EndOfRoundAudioAssetName = Config.Bind("Static assets",
                "End of round audio asset name",
                endOfRoundAudioAssetName,
                "Any audio asset what will play when round is ended. You can separate text with '|' to add multiple options");

            var rerouteVoiceCommands = JsonHelper.GetKeyValuePairs(PluginConstants.ReroutePlanetComandsKey);
            foreach (var rerouteVoiceCommand in rerouteVoiceCommands)
            {
                ReroutePlanetComands.Add(rerouteVoiceCommand.Key, Config.Bind("Reroute key Phrases",
                    rerouteVoiceCommand.Key,
                    rerouteVoiceCommand.Value,
                    "Key phrase what you need to say with 'Reroute Key Phrase' to land on other planet. You can separate text with '|' to add multiple options"));
            }

            var buyVoiceCommands = JsonHelper.GetKeyValuePairs(PluginConstants.BuyVoiceCommandsKey);
            foreach (var buyVoiceCommand in buyVoiceCommands)
            {
                BuyVoiceCommands.Add(buyVoiceCommand.Key, Config.Bind("Buy key Phrases",
                    buyVoiceCommand.Key,
                    buyVoiceCommand.Value,
                    "Key phrase what you need to say with 'Buy Key Phrase' to buy this item. You can separate text with '|' to add multiple options"));
            }

            var buyCountVoiceCommands = JsonHelper.GetKeyValuePairs(PluginConstants.BuyVoiceCountCommandsKey);
            foreach (var buyCountVoiceCommand in buyCountVoiceCommands)
            {
                BuyVoiceCountCommands.Add(buyCountVoiceCommand.Key, Config.Bind("Buy count key Phrases",
                    buyCountVoiceCommand.Key,
                    buyCountVoiceCommand.Value,
                    "Key phrase what you need to say with 'Buy Key Phrase' to buy related count of items. You can separate text with '|' to add multiple options"));
            }

            var terminalVoiceCommands = JsonHelper.GetKeyValuePairs(PluginConstants.TerminalVoiceCommandsKey);
            foreach (var terminalVoiceCommand in terminalVoiceCommands)
            {
                TerminalVoiceCommands.Add(terminalVoiceCommand.Key, Config.Bind("Terminal voice commands",
                    terminalVoiceCommand.Key,
                    terminalVoiceCommand.Value,
                    "Key phrase what you need to say to make some action via terminal. You can separate text with '|' to add multiple options"));
            }

            var voiceCommands = JsonHelper.GetKeyValuePairs(PluginConstants.VoiceCommandsKey);
            for (int i = 0; i < voiceCommands.Count; i++)
            {
                VoiceCommands.Add("Custom Asset Key phrase: " + i, Config.Bind("Any voice commands",
                    "Custom voice Key phrase: " + (i + 1),
                    voiceCommands.Values.ElementAt(i),
                    "Any Key phrase by which one will be played audio asset. You can separate text with '|' to add multiple options"));
            }

            var voicePlayAudioAssets = JsonHelper.GetKeyValuePairs(PluginConstants.VoicePlayAudioAssetsKey);
            for (int i = 0; i < voicePlayAudioAssets.Count; i++)
            {
                VoicePlayAudioAssetNames.Add("Custom Asset Key phrase: " + i, Config.Bind("Audio assets played by voice command",
                    "Custom voice asset for Key phrase: " + (i + 1),
                    voicePlayAudioAssets.Values.ElementAt(i),
                    "Any audio asset what will play by 'Any voice command'. You can separate text with '|' to add multiple options"));
            }

        }

    }
}

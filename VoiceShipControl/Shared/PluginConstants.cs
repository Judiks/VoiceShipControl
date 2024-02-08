using BepInEx.Configuration;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VoiceShipControl
{
    public static class PluginConstants
    {   // DEFOULT
        //public static ConfigEntry<bool> IsVoiceActivationButtonNeeded;
        public static ConfigEntry<bool> IsUserCanUseCommandsOutsideTheShip;
        public static ConfigEntry<bool> IsUserCanUseCommandsWhenDead;
        public static ConfigEntry<bool> IsUserCanUseTeleportAlways;
        public static ConfigEntry<KeyCode> VoiceActivationButton;
        public static ConfigEntry<SupportedLanguages> LanguageCode;
        public static string PathToFolder = VoiceShipControl.Instance.Info.Location.TrimEnd("VoiceShipControl.dll".ToCharArray());

        // COMMANDS

        public static ConfigEntry<string> BuyKeyPhrase;
        public static ConfigEntry<string> TransmitKeyPhrase;
        public static ConfigEntry<string> RerouteKeyPhrase;
        public const string BuyVoiceCommandsKey = "buy-voice-commands";
        public static Dictionary<string, ConfigEntry<string>> BuyVoiceCommands = new Dictionary<string, ConfigEntry<string>>();
        public const string BuyVoiceCountCommandsKey = "buy-voice-count-commands";
        public static Dictionary<string, ConfigEntry<string>> BuyVoiceCountCommands = new Dictionary<string, ConfigEntry<string>>();
        public const string TerminalVoiceCommandsKey = "terminal-voice-commands";
        public static Dictionary<string, ConfigEntry<string>> TerminalVoiceCommands = new Dictionary<string, ConfigEntry<string>>();
        public const string VoiceCommandsKey = "voice-commands";
        public static Dictionary<string, ConfigEntry<string>> VoiceCommands = new Dictionary<string, ConfigEntry<string>>();
        public const string ReroutePlanetComandsKey = "reroute-planet-commands";
        public static Dictionary<string, ConfigEntry<string>> ReroutePlanetComands = new Dictionary<string, ConfigEntry<string>>();
        // ASSETS
        public const string VoicePlayAudioAssetsKey = "voice-assets";
        public static Dictionary<string, ConfigEntry<string>> VoicePlayAudioAssetNames = new Dictionary<string, ConfigEntry<string>>();
        public const string StartOfRoundAudioAssetNameKey = "start-game-voice-assets";
        public static ConfigEntry<string> StartOfRoundAudioAssetName;
        public const string EndOfRoundAudioAssetNameKey = "end-game-voice-assets";
        public static ConfigEntry<string> EndOfRoundAudioAssetName;
        public const string BuySuccessAudioAssetNameKey = "buy-success-voice-assets";
        public static ConfigEntry<string> BuySuccessAudioAssetName;
        public const string BuyDeclinedAudioAssetNameKey = "buy-declined-voice-assets";
        public static ConfigEntry<string> BuyDeclinedAudioAssetName;
        public const string ShipIntroAudioAssetNameKey = "ship-intro-assets";
        public static ConfigEntry<string> ShipIntroAudioAssetName;
        public const string StartGameKey = "start-game";
        public static ConfigEntry<string> StartGame;
        public const string EndGameKey = "end-game";
        public static ConfigEntry<string> EndGame;
        public const string CloseDoorKey = "close-door";
        public static ConfigEntry<string> CloseDoor;
        public const string OpenDoorKey = "open-door";
        public static ConfigEntry<string> OpenDoor;
        public const string SwitchOnKey = "switch-on";
        public static ConfigEntry<string> SwitchOn;
        public const string SwitchOffKey = "switch-off";
        public static ConfigEntry<string> SwitchOff;
        public const string TeleporterKey = "teleporter";
        public static ConfigEntry<string> Teleporter;
        public const string InverseTeleporterKey = "inverse-teleporter";
        public static ConfigEntry<string> InverseTeleporter;
        public const string SwitchMonitorKey = "switch-monitor";
        public static ConfigEntry<string> SwitchMonitor;
        public const string ToggleMonitorKey = "toggle-monitor";
        public static ConfigEntry<string> ToggleMonitor;
        // CONFIGS ENTRY



        public static readonly Dictionary<SupportedLanguages, string> LanguageDictionary = new Dictionary<SupportedLanguages, string>
        {
            { SupportedLanguages.Afrikaans, "af-ZA" },
            { SupportedLanguages.Albanian, "sq-AL" },
            { SupportedLanguages.Amharic, "am-ET" },
            { SupportedLanguages.Arabic, "ar-SA" },
            { SupportedLanguages.Armenian, "hy-AM" },
            { SupportedLanguages.Azerbaijani, "az-AZ" },
            { SupportedLanguages.Basque, "eu-ES" },
            { SupportedLanguages.Belarusian, "be-BY" },
            { SupportedLanguages.Bengali, "bn-IN" },
            { SupportedLanguages.Bosnian, "bs-BA" },
            { SupportedLanguages.Bulgarian, "bg-BG" },
            { SupportedLanguages.Catalan, "ca-ES" },
            { SupportedLanguages.ChineseCantonese, "yue-Hant-HK" },
            { SupportedLanguages.ChineseMandarinSimplified, "zh-CN" },
            { SupportedLanguages.ChineseMandarinTraditional, "zh-TW" },
            { SupportedLanguages.Croatian, "hr-HR" },
            { SupportedLanguages.Czech, "cs-CZ" },
            { SupportedLanguages.Danish, "da-DK" },
            { SupportedLanguages.Dutch, "nl-NL" },
            { SupportedLanguages.English, "en-US" },
            { SupportedLanguages.Estonian, "et-EE" },
            { SupportedLanguages.Filipino, "fil-PH" },
            { SupportedLanguages.Finnish, "fi-FI" },
            { SupportedLanguages.French, "fr-FR" },
            { SupportedLanguages.Galician, "gl-ES" },
            { SupportedLanguages.Georgian, "ka-GE" },
            { SupportedLanguages.German, "de-DE" },
            { SupportedLanguages.Greek, "el-GR" },
            { SupportedLanguages.Gujarati, "gu-IN" },
            { SupportedLanguages.Hebrew, "he-IL" },
            { SupportedLanguages.Hindi, "hi-IN" },
            { SupportedLanguages.Hungarian, "hu-HU" },
            { SupportedLanguages.Icelandic, "is-IS" },
            { SupportedLanguages.Indonesian, "id-ID" },
            { SupportedLanguages.Italian, "it-IT" },
            { SupportedLanguages.Japanese, "ja-JP" },
            { SupportedLanguages.Javanese, "jv-ID" },
            { SupportedLanguages.Kannada, "kn-IN" },
            { SupportedLanguages.Kazakh, "kk-KZ" },
            { SupportedLanguages.Khmer, "km-KH" },
            { SupportedLanguages.Korean, "ko-KR" },
            { SupportedLanguages.Kurdish, "ku-IQ" },
            { SupportedLanguages.Kyrgyz, "ky-KG" },
            { SupportedLanguages.Lao, "lo-LA" },
            { SupportedLanguages.Latvian, "lv-LV" },
            { SupportedLanguages.Lithuanian, "lt-LT" },
            { SupportedLanguages.Luxembourgish, "lb-LU" },
            { SupportedLanguages.Macedonian, "mk-MK" },
            { SupportedLanguages.Malay, "ms-MY" },
            { SupportedLanguages.Malayalam, "ml-IN" },
            { SupportedLanguages.Marathi, "mr-IN" },
            { SupportedLanguages.Mongolian, "mn-MN" },
            { SupportedLanguages.Nepali, "ne-NP" },
            { SupportedLanguages.Norwegian, "nb-NO" },
            { SupportedLanguages.Oriya, "or-IN" },
            { SupportedLanguages.Pashto, "ps-AF" },
            { SupportedLanguages.Persian, "fa-IR" },
            { SupportedLanguages.Polish, "pl-PL" },
            { SupportedLanguages.PortugueseBrazil, "pt-BR" },
            { SupportedLanguages.PortuguesePortugal, "pt-PT" },
            { SupportedLanguages.Punjabi, "pa-IN" },
            { SupportedLanguages.Romanian, "ro-RO" },
            { SupportedLanguages.Russian, "ru-RU" },
            { SupportedLanguages.Serbian, "sr-RS" },
            { SupportedLanguages.Sinhala, "si-LK" },
            { SupportedLanguages.Slovak, "sk-SK" },
            { SupportedLanguages.Slovenian, "sl-SI" },
            { SupportedLanguages.Somali, "so-SO" },
            { SupportedLanguages.Spanish, "es-ES" },
            { SupportedLanguages.Sundanese, "su-ID" },
            { SupportedLanguages.Swahili, "sw-TZ" },
            { SupportedLanguages.Swedish, "sv-SE" },
            { SupportedLanguages.Tamil, "ta-IN" },
            { SupportedLanguages.Telugu, "te-IN" },
            { SupportedLanguages.Thai, "th-TH" },
            { SupportedLanguages.Turkish, "tr-TR" },
            { SupportedLanguages.Ukrainian, "uk-UA" },
            { SupportedLanguages.Urdu, "ur-PK" },
            { SupportedLanguages.Uzbek, "uz-UZ" },
            { SupportedLanguages.Vietnamese, "vi-VN" },
            { SupportedLanguages.Welsh, "cy-GB" },
            { SupportedLanguages.Xhosa, "xh-ZA" },
            { SupportedLanguages.Yiddish, "yi-DE" },
            { SupportedLanguages.Yoruba, "yo-NG" },
            { SupportedLanguages.Zulu, "zu-ZA" }
        };

    }
}

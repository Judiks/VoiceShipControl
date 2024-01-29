namespace VoiceShipControll
{
    public class PluginConstants
    {
        public static string LanguageCode = string.Empty;
        public const string LanguageCodeKey = "language-code";
        public const string BuyKeywordKey = "buy-keyword";
        public const string WelcomeJarvisKey = "jarvis-welcome";
        public static string PathToFolder = VoiceShipControll.Instance.Info.Location.TrimEnd("VoiceShipControll.dll".ToCharArray());

        // COMMANDS
        public const string BuyVoiceCommandsKey = "buy-voice-commands";
        public const string TerminalVoiceCommandsKey = "terminal-voice-commands";
        public const string VoiceCommandsKey = "voice-commands";
        // ASSETS
        public const string VoiceAssetsKey = "voice-assets";
        public const string StartGameAudioKey = "start-game-voice-assets";
        public const string EndGameAudioKey = "end-game-voice-assets";
        public const string BuySuccessAudioKey = "buy-success-voice-assets";
        public const string BuyDeclinedAudioKey = "buy-declined-voice-assets";
    }
}

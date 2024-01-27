using System.Collections.Generic;

namespace VoiceShipControll
{
    internal class PlaginConstants
    {
        public static string LanguageCode = string.Empty;
        public static string PathToFolder = VoiceShipControll.Instance.Info.Location.TrimEnd("VoiceShipControll.dll".ToCharArray());

        // COMMANDS
        public static Dictionary<string, string> JarviceVoiceCommands = new Dictionary<string, string>();
        public static Dictionary<string, string> TerminalVoiceCommands = new Dictionary<string, string>();
        public static List<string> TerminalBuyCommands = new List<string>() { 
            "pro", "walkie", "teleport", "flash"
        };
        // ASSETS
        public static Dictionary<string, string> JarvisVoiceAssets = new Dictionary<string, string>();
    }
}

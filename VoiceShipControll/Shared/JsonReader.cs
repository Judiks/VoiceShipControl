using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace VoiceShipControll.Helpers
{
    internal class JsonReader
    {
        public static string GetValue(string key)
        {
            JObject json = JObject.Parse(File.ReadAllText($"{PluginConstants.PathToFolder}\\VoiceShipControllSettings.json"));
            var resul = json.Value<string>(key); ;
            return resul;
        }
        public static Dictionary<string, string> GetKeyValuePairs(string key)
        {
            JObject json = JObject.Parse(File.ReadAllText($"{PluginConstants.PathToFolder}\\VoiceShipControllSettings.json"));
            var resul = JObject.FromObject(json[key]).ToObject<Dictionary<string, string>>();
            return resul;
        }
    }
}

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace VoiceShipControll.Helpers
{
    internal class JsonHelper
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
        public static void SetKeyValuePair(string key, string subKey, string value)
        {
            // Specify the path to your JSON file
            string filePath = $"{PluginConstants.PathToFolder}\\VoiceShipControllSettings.json";

            // Read the JSON file into a string
            string jsonString = File.ReadAllText(filePath);

            // Parse JSON string to JObject
            JObject jsonObject = JObject.Parse(jsonString);

            if (string.IsNullOrEmpty(subKey))
            {
                // Modify the JSON object (add a new property)
                jsonObject[key] = value;
            } else
            {
                // Modify the JSON object (add a new property)
                jsonObject[subKey][key] = value;
            }

            // Convert JObject back to JSON string
            string updatedJsonString = jsonObject.ToString();

            // Write the updated JSON string back to the file
            File.WriteAllText(filePath, updatedJsonString);
        }
    }
}

using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace VoiceShipControll.Helpers
{
    internal class AudioClipHelper
    {

        private static void PlayAudioSourceVoice(string assetName, AudioSource audioSource)
        {
            try
            {


                var result = AssetLoader.Load<AudioClip>(assetName);
                if (result.Item1 == null)
                {
                    if (result.Item2 != null)
                    {
                        result.Item2.Unload(false);
                    }
                    return;
                }
                if (audioSource == null)
                {
                    result.Item2.Unload(false);
                    return;
                }
                Console.WriteLine("PlayAudioSourceVoice Play");
                audioSource.PlayOneShot(result.Item1);
                result.Item2.Unload(false);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void PlayValuePairAudioSourceByKey(string assetKey, AudioSource audioSource)
        {
            if (audioSource == null)
            {
                Debug.Log("audioSource empty");
                return;
            }
            try
            {
                var assetNamesString = string.Empty;
                var voiceAssets = JsonReader.GetKeyValuePairs(PluginConstants.VoiceAssetsKey);
                voiceAssets.TryGetValue(assetKey, out assetNamesString);
                Console.WriteLine(assetNamesString + " founded asset");
                if (string.IsNullOrEmpty(assetNamesString)) { return; }
                if (assetNamesString.Contains("|"))
                {
                    Console.WriteLine(assetNamesString + " Contains |");
                    var assetNames = assetNamesString.Split('|');
                    var random = new Random();
                    int assetNameIndex = random.Next(0, assetNames.Length);
                    PlayAudioSourceVoice(assetNames[assetNameIndex], audioSource);
                    return;
                }
                PlayAudioSourceVoice(assetNamesString, audioSource);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void PlayAudioSourceByKey(string assetKey, AudioSource audioSource)
        {
            if (audioSource == null)
            {
                Debug.Log("audioSource empty");
                return;
            }
            try
            {
                var assetNamesString = string.Empty;
                assetNamesString = JsonReader.GetValue(assetKey);
                Console.WriteLine(assetNamesString + " founded asset");
                if (string.IsNullOrEmpty(assetNamesString)) { return; }
                if (assetNamesString.Contains("|"))
                {
                    Console.WriteLine(assetNamesString + " Contains |");
                    var assetNames = assetNamesString.Split('|');
                    var random = new Random();
                    int assetNameIndex = random.Next(0, assetNames.Length);
                    PlayAudioSourceVoice(assetNames[assetNameIndex], audioSource);
                    return;
                }
                PlayAudioSourceVoice(assetNamesString, audioSource);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

    }
}

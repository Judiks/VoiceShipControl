using System;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace VoiceShipControl.Helpers
{
    internal class AudioClipHelper
    {

        private static void PlayAudioSourceVoice(string assetName, AudioSource audioSource)
        {
            try
            {
                Console.WriteLine("Start Play audioSource " + assetName);
                var result = AssetLoader.Load<AudioClip>(assetName);
                if (result.Result == null)
                {
                    if (result.Bundle != null)
                    {
                        result.Bundle.Unload(false);
                    }
                    return;
                }
                if (audioSource == null)
                {
                    result.Bundle.Unload(false);
                    return;
                }
                Console.WriteLine("PlayAudioSourceVoice Play");
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.PlayOneShot(result.Result);
                }
                else
                {
                    audioSource.PlayOneShot(result.Result);
                }
                result.Bundle.Unload(false);
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
                Debug.Log(assetKey);
                var assetNameValuePair = PluginConstants.VoicePlayAudioAssetNames.FirstOrDefault(x => x.Key == assetKey);
                if (assetNameValuePair.Value != null && !string.IsNullOrEmpty(assetNameValuePair.Value.Value) && !string.IsNullOrEmpty(assetNameValuePair.Key))
                {
                    Console.WriteLine(assetNameValuePair.Value.Value + " founded asset");
                    if (string.IsNullOrEmpty(assetNameValuePair.Value.Value)) { return; }
                    if (assetNameValuePair.Value.Value.Contains("|"))
                    {
                        Console.WriteLine(assetNameValuePair.Value.Value + " Contains |");
                        var assetNames = assetNameValuePair.Value.Value.Split('|');
                        var random = new Random();
                        int assetNameIndex = random.Next(0, assetNames.Length);
                        PlayAudioSourceVoice(assetNames[assetNameIndex], audioSource);
                        return;
                    }
                    PlayAudioSourceVoice(assetNameValuePair.Value.Value, audioSource);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void PlayAudioSourceByValue(string assetValue, AudioSource audioSource)
        {
            if (audioSource == null)
            {
                Debug.Log("audioSource empty");
                return;
            }
            try
            {
                Console.WriteLine(assetValue + " founded asset");
                if (string.IsNullOrEmpty(assetValue)) { return; }
                if (assetValue.Contains("|"))
                {
                    Console.WriteLine(assetValue + " Contains |");
                    var assetNames = assetValue.Split('|');
                    var random = new Random();
                    int assetNameIndex = random.Next(0, assetNames.Length);
                    PlayAudioSourceVoice(assetNames[assetNameIndex], audioSource);
                    return;
                }
                PlayAudioSourceVoice(assetValue, audioSource);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

    }
}

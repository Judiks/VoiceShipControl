using BepInEx.Configuration;
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
                Console.WriteLine("Start Play audioSource " + assetName);
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
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.PlayOneShot(result.Item1);
                } else
                {
                    audioSource.PlayOneShot(result.Item1);
                }
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

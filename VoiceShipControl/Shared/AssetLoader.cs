using System.IO;
using System;
using System.Linq;
using UnityEngine;
using VoiceShipControl.Shared;

namespace VoiceShipControl.Helpers
{
    class AssetData<T>
    {
        public T Result { get; set; }
        public AssetBundle Bundle { get; set; }

        public AssetData(T result, AssetBundle bundle)
        {
            Result = result;
            Bundle = bundle;
        }
    }
    internal class AssetLoader
    {
        public static AssetData<T> Load<T>(string assetName) where T : UnityEngine.Object
        {
            T result;
            var bundle = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(x => x.name == assetName);
            if (bundle == null)
            {
                bundle = AssetBundle.LoadFromFile(FileHelper.GetFilePath(assetName));
            }
            if (bundle != null)
            {
                result = bundle.LoadAsset<T>(assetName);
                if (result != null)
                {
                    Debug.Log($"{assetName} loaded succesfuly");
                }
                else
                {
                    Debug.Log($"{assetName} asset not exist");
                }
            }
            else
            {
                result = null;
                Debug.Log($"Error while loading {assetName}");
            }
            return new AssetData<T>(result, bundle);
        }
    }


}

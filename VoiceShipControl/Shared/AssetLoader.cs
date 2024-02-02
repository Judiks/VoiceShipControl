using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace VoiceShipControl.Helpers
{
    internal class AssetLoader
    {
        public static (T, AssetBundle) Load<T>(string assetName) where T : UnityEngine.Object
        {
            T result;
            var bundle = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(x => x.name == assetName);
            if (bundle == null)
            {
                bundle = AssetBundle.LoadFromFile($"{PluginConstants.PathToFolder}\\Assets\\{assetName}");
            }
            if (bundle != null)
            {
                result = bundle.LoadAsset<T>(assetName);
                if (result != null)
                {
                    Debug.Log($"{assetName} loaded succesfuly");
                } else
                {
                    Debug.Log($"{assetName} asset not exist");
                }
            }
            else
            {
                result = null;
                Debug.Log($"Error while loading {assetName}");
            }
            return (result, bundle);
        }
    }

}

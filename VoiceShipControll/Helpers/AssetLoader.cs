using UnityEngine;
namespace VoiceShipControll.Helpers
{
    internal class AssetLoader
    {
        public static (T, AssetBundle) Load<T>(string assetName) where T : UnityEngine.Object
        {
            T result;
            var bundle = AssetBundle.LoadFromFile($"{PlaginConstants.PathToFolder}\\Assets\\{assetName}");
            if (bundle != null )
            {
                result = bundle.LoadAsset<T>(assetName);
                Debug.Log($"{assetName} loaded succesfuly");
            } else
            {
                result = null;
                Debug.Log($"Error while loading {assetName}");
            }
            return (result, bundle);
        }
    }

}

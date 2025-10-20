using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.AssetLoaderModule
{
    /// <summary>Wraps Resources - the only asset source Sorcery Strife has today (no Addressables package).</summary>
    public class ResourceAssetLoaderService : IAssetLoaderService
    {
        public T LoadAsset<T>(string key) where T : Object
        {
            return Resources.Load<T>(key);
        }

        public IReadOnlyList<T> LoadAllAssets<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(path);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.AssetLoaderModule
{
    public interface IAssetLoaderService
    {
        T LoadAsset<T>(string key) where T : Object;

        IReadOnlyList<T> LoadAllAssets<T>(string path) where T : Object;
    }
}

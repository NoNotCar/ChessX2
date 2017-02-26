using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Importing : AssetPostprocessor {
    static readonly string DEFAULTS_KEY = "DEFAULTS_DONE";
    static readonly uint DEFAULTS_VERSION = 2;
    public void OnPreprocessTexture()
    {
        if (!isAssetProcessed)
        {
            var textureImporter = assetImporter as TextureImporter;
            textureImporter.spritePixelsPerUnit = 16;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }

    public bool isAssetProcessed
    {
        get
        {
            string key = string.Format("{0}_{1}", DEFAULTS_KEY, DEFAULTS_VERSION);
            return assetImporter.userData.Contains(key);
        }
        set
        {
            string key = string.Format("{0}_{1}", DEFAULTS_KEY, DEFAULTS_VERSION);
            assetImporter.userData = value ? key : string.Empty;
        }
    }
}

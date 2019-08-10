using UnityEngine;
using UnityEditor;
using System.IO;

class AssetsBuilder
{
    static void BuildSprites() {
        var assetBundleBuild = new AssetBundleBuild();
        assetBundleBuild.assetBundleName = "sprites";
        assetBundleBuild.assetNames = Directory.GetFiles("Assets/sprites");

        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", new AssetBundleBuild[] { assetBundleBuild }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
}
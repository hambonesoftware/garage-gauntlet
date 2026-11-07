using System;
using UnityEditor;
using UnityEngine;

namespace GarageGauntlet.Editor
{
    internal sealed class RealismMaterials : AssetPostprocessor
    {
        private const string Folder = "Assets/Resources/realism/";

        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith(Folder, StringComparison.Ordinal))
                Configure((TextureImporter)assetImporter, assetPath);
        }

        internal static void Prepare()
        {
            foreach (string surface in new[] { "asphalt", "ballast", "timber" })
            {
                string materialPath = Folder + surface + "-surface.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(Shader.Find("Standard"));
                    AssetDatabase.CreateAsset(material, materialPath);
                }

                material.mainTexture = LoadTexture(Folder + surface + "-diff.jpg");
                material.SetTexture("_BumpMap", LoadTexture(Folder + surface + "-nor_gl.jpg"));
                material.SetFloat("_BumpScale", surface == "asphalt" ? .32f : .46f);
                material.SetFloat("_Glossiness", .15f);
                material.EnableKeyword("_NORMALMAP");
                EditorUtility.SetDirty(material);
            }
            AssetDatabase.SaveAssets();
        }

        private static Texture2D LoadTexture(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null)
                throw new InvalidOperationException("Missing scale model texture " + path);
            TextureImporterType before = importer.textureType;
            Configure(importer, path);
            if (importer.textureType != before)
                importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void Configure(TextureImporter importer, string path)
        {
            bool normal = path.EndsWith("-nor_gl.jpg", StringComparison.Ordinal);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normal;
            importer.maxTextureSize = path.EndsWith("asphalt-diff.jpg", StringComparison.Ordinal)
                || path.EndsWith("locomotive-color.png", StringComparison.Ordinal) ? 1024 : 512;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 4;
        }
    }
}

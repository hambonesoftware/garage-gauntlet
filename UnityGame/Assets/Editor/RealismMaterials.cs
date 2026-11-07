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
            foreach (string surface in new[] { "asphalt", "ballast", "timber", "brick", "roof" })
            {
                Material material = GetMaterial(surface + "-surface");
                material.mainTexture = LoadTexture(Folder + surface + "-diff.jpg");
                material.SetTexture("_BumpMap", LoadTexture(Folder + surface + "-nor_gl.jpg"));
                material.SetFloat("_BumpScale", surface == "asphalt" ? .32f : .46f);
                material.SetFloat("_Glossiness", .15f);
                material.EnableKeyword("_NORMALMAP");
                EditorUtility.SetDirty(material);
            }
            Material diecast = GetMaterial("diecast-finish");
            diecast.mainTexture = LoadTexture(Folder + "diecast-color.png");
            diecast.SetTexture("_BumpMap", LoadTexture(Folder + "diecast-nor_gl.png"));
            Texture2D finish = LoadTexture(Folder + "diecast-metal.png");
            diecast.SetTexture("_MetallicGlossMap", finish);
            diecast.SetTexture("_OcclusionMap", finish);
            diecast.SetFloat("_BumpScale", .65f);
            diecast.SetFloat("_GlossMapScale", .91f);
            diecast.SetFloat("_OcclusionStrength", .7f);
            diecast.EnableKeyword("_NORMALMAP");
            diecast.EnableKeyword("_METALLICGLOSSMAP");
            EditorUtility.SetDirty(diecast);
            MakeReflection();
            AssetDatabase.SaveAssets();
        }

        private static Material GetMaterial(string name)
        {
            string path = Folder + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }

        private static void MakeReflection()
        {
            string path = Folder + "workbench-reflection.asset";
            if (AssetDatabase.LoadAssetAtPath<Cubemap>(path) != null)
                return;
            const int size = 32;
            var map = new Cubemap(size, TextureFormat.RGBA32, true) { name = "Workbench window light" };
            var pixels = new Color[size * size];
            Vector3 window = new Vector3(-.4f, .87f, .3f).normalized;
            Vector3 bounce = new Vector3(.88f, .30f, -.39f).normalized;
            for (int face = 0; face < 6; face++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float u = (x + .5f) * 2 / size - 1;
                        float v = (y + .5f) * 2 / size - 1;
                        Vector3 direction = CubeDirection(face, u, v).normalized;
                        float sky = Mathf.SmoothStep(0, 1, direction.y * .5f + .5f);
                        Color room = Color.Lerp(new Color(.10f, .12f, .15f), new Color(.36f, .43f, .52f), sky);
                        float highlight = Mathf.Pow(Mathf.Clamp01(Vector3.Dot(direction, window)), 14);
                        float fill = Mathf.Pow(Mathf.Clamp01(Vector3.Dot(direction, bounce)), 23);
                        pixels[y * size + x] = room + new Color(.63f, .56f, .46f) * highlight
                            + new Color(.29f, .35f, .42f) * fill;
                    }
                }
                map.SetPixels(pixels, (CubemapFace)face);
            }
            map.Apply(true);
            AssetDatabase.CreateAsset(map, path);
        }

        private static Vector3 CubeDirection(int face, float u, float v)
        {
            switch ((CubemapFace)face)
            {
                case CubemapFace.PositiveX: return new Vector3(1, v, -u);
                case CubemapFace.NegativeX: return new Vector3(-1, v, u);
                case CubemapFace.PositiveY: return new Vector3(u, 1, -v);
                case CubemapFace.NegativeY: return new Vector3(u, -1, v);
                case CubemapFace.PositiveZ: return new Vector3(u, v, 1);
                default: return new Vector3(-u, v, -1);
            }
        }

        private static Texture2D LoadTexture(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null)
                throw new InvalidOperationException("Missing scale model texture " + path);
            TextureImporterType before = importer.textureType;
            bool wasSrgb = importer.sRGBTexture;
            Configure(importer, path);
            if (importer.textureType != before || importer.sRGBTexture != wasSrgb)
                importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void Configure(TextureImporter importer, string path)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            bool normal = name.EndsWith("-nor_gl", StringComparison.Ordinal);
            bool palette = name == "craftsman-palette";
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normal && name != "diecast-metal";
            importer.maxTextureSize = name == "asphalt-diff" || name == "locomotive-color" || name == "diecast-color" ? 1024 : 512;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = palette ? FilterMode.Point : FilterMode.Trilinear;
            importer.mipmapEnabled = !palette;
            importer.anisoLevel = palette ? 0 : 4;
        }
    }
}

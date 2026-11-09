using UnityEngine;

namespace GarageGauntlet
{
    internal sealed partial class RailwayTown
    {
        private Texture2D modelGrain;
        private Mesh foliageClump;
        private Material trunkFinish;
        private Material[] flockFinishes;

        private Texture2D ModelGrain()
        {
            if (modelGrain != null)
                return modelGrain;

            const int size = 128;
            var pixels = new Color32[size * size];
            var random = new System.Random(421);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = x * Mathf.PI * 2 / size;
                float v = y * Mathf.PI * 2 / size;
                float cloud = Mathf.Sin(u * 3 + v * 5) * Mathf.Cos(u * 7 - v * 4);
                float mottling = Mathf.Cos(u * 13 + v * 11 + Mathf.Sin(v * 4));
                byte shade = (byte)Mathf.Clamp(211 + cloud * 19 + mottling * 8 + random.Next(-41, 36), 105, 255);
                pixels[y * size + x] = new Color32(shade, shade, shade, 255);
            }

            modelGrain = Own(new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "Fine scenery grain",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 4
            });
            modelGrain.SetPixels32(pixels);
            modelGrain.Apply(true, true);
            return modelGrain;
        }

        private Material ModelFlock(Color color, Vector2 repeat)
        {
            Material flock = Paint(color.r, color.g, color.b);
            flock.mainTexture = ModelGrain();
            flock.mainTextureScale = repeat;
            flock.SetFloat("_Glossiness", .025f);
            return flock;
        }

        private void DrawFlockedTree(Vector3 origin, int index)
        {
            origin.y += MapleJunctionPlan.GroundHeight(origin.x, origin.z);
            if (foliageClump == null)
            {
                foliageClump = CreateFoliageClump();
                trunkFinish = Paint(.33f, .23f, .16f);
                flockFinishes = new[]
                {
                    ModelFlock(new Color(.28f, .36f, .19f), Vector2.one),
                    ModelFlock(new Color(.40f, .48f, .25f), Vector2.one),
                    ModelFlock(new Color(.50f, .51f, .28f), Vector2.one)
                };
            }

            float height = index % 3 == 0 ? 2.40f : 1.62f + index % 4 * .10f;
            Box("Model tree trunk", origin + Vector3.up * (.29f * height),
                new Vector3(.064f, .60f, .062f) * height, trunkFinish, Quaternion.Euler(-5, 0, 6));
            var random = new System.Random(907 + index * 41);
            int palette = index % flockFinishes.Length;
            AddScenery(foliageClump, flockFinishes[palette], origin + Vector3.up * (.78f * height),
                Quaternion.Euler(8, index * 67, -11), new Vector3(.50f, .43f, .45f) * height);
            for (int branch = 0; branch < 7; branch++)
            {
                float angle = (branch * 137 + index * 51) * Mathf.Deg2Rad;
                float reach = .19f + (float)random.NextDouble() * .065f;
                float level = .54f + (branch % 4) * .086f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * reach, level, Mathf.Sin(angle) * reach) * height;
                Vector3 joint = origin + Vector3.up * (.36f * height);
                Vector3 branchVector = origin + offset - joint;
                Box("Model tree branch", joint + branchVector / 2,
                    new Vector3(.064f, branchVector.magnitude, .067f), trunkFinish,
                    Quaternion.FromToRotation(Vector3.up, branchVector));
                float tuft = .36f + (float)random.NextDouble() * .10f;
                AddScenery(foliageClump, flockFinishes[(palette + branch % 2) % flockFinishes.Length], origin + offset,
                    Quaternion.Euler(branch * 16, index * 61 + branch * 31, branch * 11),
                    new Vector3(tuft, tuft * .83f, tuft) * height);
            }
        }

        private Mesh CreateFoliageClump()
        {
            const int rings = 8, slices = 12;
            var vertices = new Vector3[(rings + 1) * (slices + 1)];
            var uv = new Vector2[vertices.Length];
            var indices = new int[rings * slices * 6];
            int triangle = 0;
            for (int row = 0; row <= rings; row++)
            for (int col = 0; col <= slices; col++)
            {
                float u = col / (float)slices;
                float v = row / (float)rings;
                float azimuth = u * Mathf.PI * 2;
                float polar = v * Mathf.PI;
                float latitude = Mathf.Sin(polar);
                var direction = new Vector3(Mathf.Sin(azimuth) * latitude, Mathf.Cos(polar), Mathf.Cos(azimuth) * latitude);
                float radius = .5f + .045f * Mathf.Sin(azimuth * 3 + polar * 6) * latitude
                    + .042f * Mathf.Cos(azimuth * 5 - polar * 7) * latitude;
                int a = row * (slices + 1) + col;
                vertices[a] = direction * radius;
                uv[a] = new Vector2(u, v);
                if (row == rings || col == slices)
                    continue;
                int b = a + slices + 1;
                indices[triangle++] = a;
                indices[triangle++] = b;
                indices[triangle++] = a + 1;
                indices[triangle++] = a + 1;
                indices[triangle++] = b;
                indices[triangle++] = b + 1;
            }

            var mesh = Own(new Mesh { name = "Flocked foam canopy", vertices = vertices, uv = uv, triangles = indices });
            mesh.RecalculateNormals();
            return mesh;
        }

        private void DrawWorkbench(float width, float depth, Material timber, Material trim)
        {
            Vector3 center = RaceCourse.BoardCenter;
            for (int layer = 0; layer < 3; layer++)
            {
                float y = -.44f - layer * .19f;
                Box("Visible plywood lamination", center + new Vector3(0, y, 0), new Vector3(width + .025f, .035f, depth + .025f), trim);
            }

            Material steel = Paint(.51f, .53f, .53f);
            steel.SetFloat("_Metallic", .59f);
            steel.SetFloat("_Glossiness", .51f);
            Material orange = Paint(.70f, .33f, .10f);
            Material black = Paint(.095f, .105f, .10f);
            Material paper = Paint(.70f, .66f, .54f);
            for (int side = -1; side <= 1; side += 2)
            for (int i = -2; i <= 2; i++)
            {
                float x = i * width / 5.3f;
                float z = side * (depth / 2 + .054f);
                Box("Display board screw", center + new Vector3(x, -.52f, z), new Vector3(.16f, .15f, .045f), steel);
                Box("Screw slot", center + new Vector3(x, -.52f, z + side * .027f), new Vector3(.105f, .027f, .017f), black,
                    Quaternion.Euler(0, 0, (i + 2) * 34));
            }

            const float workY = -1.14f;
            Vector3 knife = center + new Vector3(15.9f, workY + .16f, -depth / 2 - 3.3f);
            Quaternion angle = Quaternion.Euler(0, 14, 0);
            Box("Hobby knife handle", knife, new Vector3(4.15f, .28f, .52f), orange, angle);
            Box("Hobby knife collet", knife + angle * new Vector3(2.08f, 0, 0), new Vector3(.62f, .27f, .41f), black, angle);
            Box("Hobby knife blade", knife + angle * new Vector3(2.64f, 0, 0), new Vector3(.71f, .055f, .30f), steel, angle);
            for (int ring = 0; ring < 5; ring++)
            {
                Vector3 notch = knife + angle * new Vector3(1.25f + ring * .16f, .143f, 0);
                Box("Knife grip rib", notch, new Vector3(.068f, .035f, .52f), black, angle);
            }

            Vector3 sandingBlock = center + new Vector3(-17.3f, workY + .20f, -depth / 2 - 3.5f);
            Quaternion turn = Quaternion.Euler(0, -14, 0);
            SurfaceBox("Workbench sanding block", sandingBlock, new Vector3(3.6f, .40f, 1.6f), new Vector2(1.8f, 1), timber, turn);
            Box("Sandpaper wrap", sandingBlock + Vector3.up * .213f, new Vector3(3.54f, .045f, 1.55f), paper, turn);
        }
    }
}

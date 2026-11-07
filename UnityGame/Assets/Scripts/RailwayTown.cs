using System.Collections.Generic;
using UnityEngine;

namespace GarageGauntlet
{
    internal sealed class RailwayTown
    {
        private readonly Transform root;
        private readonly Shader shader;
        private readonly Dictionary<string, Material> palettes = new Dictionary<string, Material>();
        private readonly List<Vector3> housePositions = new List<Vector3>();
        public Transform Car { get; }
        public Transform Train { get; }
        public Camera Camera { get; }

        public RailwayTown(Transform root, RaceCourse course, Shader shader)
        {
            this.root = root;
            this.shader = shader != null ? shader : Shader.Find("Standard");
            SetupLighting();
            DrawBoard(course);
            DrawTown();
            DrawRailway();
            Car = Group("RC racer", root);
            Model("toy/vehicle-racer", Car, Vector3.zero, new Vector3(0, 180, 0), new Vector3(.96f, .62f, 1.6f));
            Train = Group("Model train", root);
            Model("train/train-locomotive-a", Train, new Vector3(1.13f, 0, 0),
                new Vector3(0, 90, 0), new Vector3(1.05f, 1.32f, 2.1f));
            Model("train/train-carriage-wood", Train, new Vector3(-1.10f, 0, 0),
                new Vector3(0, 90, 0), new Vector3(1.05f, 1.18f, 2.1f));

            Camera = Group("Race camera", root).gameObject.AddComponent<Camera>();
            Camera.orthographic = true;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(.64f, .77f, .78f);
            Camera.farClipPlane = 100;
        }

        public bool Blocks(Vector3 position)
        {
            foreach (Vector3 house in housePositions)
            {
                if (Mathf.Abs(position.x - house.x) < 1.12f && Mathf.Abs(position.z - house.z) < 1.08f)
                    return true;
            }
            return false;
        }

        private void SetupLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.68f, .72f, .76f);
            Light sun = Group("Workshop sunlight", root).gameObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1, .93f, .80f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(54, -34, 0);
        }

        private void DrawBoard(RaceCourse course)
        {
            Material wood = Paint(.39f, .20f, .12f);
            Material grass = Paint(.37f, .52f, .28f);
            Material asphalt = Paint(.22f, .27f, .30f);
            Material shoulder = Paint(.83f, .67f, .44f);
            Material cream = Paint(.93f, .89f, .70f);
            Box("Wooden display board", new Vector3(0, -.56f, 0), new Vector3(38, .9f, 30), wood);
            Box("Felt landscape", new Vector3(0, -.105f, 0), new Vector3(36.5f, .17f, 28.5f), grass);
            Ribbon(course, RaceCourse.HalfWidth + .21f, .024f, shoulder, "Road shoulder");
            Ribbon(course, RaceCourse.HalfWidth, .035f, asphalt, "RC course");

            for (float distance = 1.7f; distance < course.Length - 1.2f; distance += 2.6f)
            {
                Vector3 at = course.Sample(distance, out Vector3 direction);
                if (Mathf.Abs(at.z) > 1)
                    Box("Road marking", at + Vector3.up * .044f, new Vector3(.075f, .014f, 1.12f),
                        cream, Quaternion.LookRotation(direction));
            }

            Vector3 start = course.Sample(0, out Vector3 forward);
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Quaternion turn = Quaternion.LookRotation(forward);
            for (int i = 0; i < 8; i++)
            {
                Vector3 square = start + right * ((i - 3.5f) * .51f) + Vector3.up * .052f;
                Box("Start check", square, new Vector3(.51f, .012f, .28f), i % 2 == 0 ? cream : asphalt, turn);
                Box("Finish check", square + forward * .28f, new Vector3(.51f, .012f, .28f), i % 2 == 0 ? asphalt : cream, turn);
            }

            Model("toy/gate-finish", root, start + Vector3.up * .04f, new Vector3(0, turn.eulerAngles.y, 0),
                new Vector3(5.1f, 2.15f, .55f));
            for (int quarter = 1; quarter <= 3; quarter++)
            {
                Vector3 at = course.Sample(course.Length * quarter / 4, out Vector3 direction);
                Vector3 side = Vector3.Cross(Vector3.up, direction) * 2.23f;
                Vector3 size = new Vector3(.29f, .43f, .29f);
                Model("toy/item-cone", root, at + side, Vector3.zero, size);
                Model("toy/item-cone", root, at - side, Vector3.zero, size);
            }
        }

        private void DrawTown()
        {
            Material water = Paint(.21f, .54f, .62f);
            Material pondEdge = Paint(.59f, .69f, .42f);
            Box("Pond bank", new Vector3(5, .015f, 3.7f), new Vector3(4.4f, .1f, 3.0f), pondEdge);
            Box("Model pond", new Vector3(5, .067f, 3.7f), new Vector3(3.95f, .025f, 2.55f), water);
            House("building-type-a", -7.1f, 3.1f, 125);
            House("building-type-d", -3.9f, 5.4f, 176);
            House("building-type-g", -.1f, 3.4f, 180);
            House("building-type-m", -7.4f, -3.6f, 22);
            House("building-type-d", -3.8f, -3.5f, 5);
            House("building-type-a", -.4f, -3.8f, 0);
            House("building-type-g", 3.2f, -3.9f, 0);
            House("building-type-m", 7.3f, -3.2f, 340);

            Vector2[] trees =
            {
                new Vector2(-15.8f, 8.4f), new Vector2(-15.5f, -7.5f), new Vector2(-8.7f, 11.6f),
                new Vector2(-3.9f, 13), new Vector2(1, 10.7f), new Vector2(8.9f, 11.1f),
                new Vector2(15.5f, 6.6f), new Vector2(16.1f, -5.2f), new Vector2(10.2f, -11.6f),
                new Vector2(5.9f, -11.4f), new Vector2(-5.8f, -11.8f), new Vector2(-10.3f, -10.8f),
                new Vector2(8.4f, 4.2f), new Vector2(-8.5f, 5.0f), new Vector2(1.9f, 4.8f)
            };
            for (int i = 0; i < trees.Length; i++)
            {
                Vector3 scale = i % 2 == 0 ? new Vector3(1.6f, 2.65f, 1.6f) : new Vector3(1.15f, 1.7f, 1.15f);
                Model(i % 2 == 0 ? "town/tree-large" : "town/tree-small", root,
                    new Vector3(trees[i].x, -.02f, trees[i].y), new Vector3(0, i * 73 % 360, 0), scale);
            }
        }

        private void House(string model, float x, float z, float angle)
        {
            Vector3 position = new Vector3(x, -.025f, z);
            housePositions.Add(position);
            Model("town/" + model, root, position, new Vector3(0, angle, 0), new Vector3(2.1f, 1.65f, 1.95f));
        }

        private void DrawRailway()
        {
            Material ballast = Paint(.46f, .39f, .33f);
            Material sleepers = Paint(.31f, .23f, .17f);
            Material metal = Paint(.74f, .76f, .73f);
            Box("Ballast", new Vector3(0, .021f, 0), new Vector3(36.4f, .05f, 1.55f), ballast);
            for (int i = -34; i <= 34; i++)
                Box("Rail tie", new Vector3(i * .52f, .061f, 0), new Vector3(.17f, .053f, 1.30f), sleepers);
            Box("North rail", new Vector3(0, .105f, .43f), new Vector3(36.5f, .065f, .066f), metal);
            Box("South rail", new Vector3(0, .105f, -.43f), new Vector3(36.5f, .065f, .066f), metal);
            Material warning = Paint(.98f, .65f, .19f);
            foreach (float x in new[] { -15.6f, 15.2f })
            {
                Box("Crossing sign post", new Vector3(x, .50f, 1.15f), new Vector3(.09f, 1.04f, .09f), metal);
                Box("Crossing sign", new Vector3(x, 1.02f, 1.15f), new Vector3(.43f, .43f, .09f), warning,
                    Quaternion.Euler(0, 0, 45));
            }
        }

        private void Ribbon(RaceCourse course, float halfWidth, float y, Material material, string name)
        {
            var vertices = new Vector3[course.Count * 2];
            var triangles = new int[course.Count * 6];
            for (int i = 0; i < course.Count; i++)
            {
                Vector3 center = course.At(i) + Vector3.up * y;
                Vector3 side = Vector3.Cross(Vector3.up, course.Tangent(i)) * halfWidth;
                int current = i * 2;
                int next = (current + 2) % vertices.Length;
                vertices[current] = center - side;
                vertices[current + 1] = center + side;
                int triangle = i * 6;
                triangles[triangle] = current;
                triangles[triangle + 1] = next;
                triangles[triangle + 2] = current + 1;
                triangles[triangle + 3] = current + 1;
                triangles[triangle + 4] = next;
                triangles[triangle + 5] = next + 1;
            }
            var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            Transform surface = Group(name, root);
            surface.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            surface.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private void Model(string path, Transform parent, Vector3 position, Vector3 euler, Vector3 dimensions)
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
                throw new System.InvalidOperationException("Missing model " + path);

            Transform pivot = Group(prefab.name, parent);
            Transform model = Object.Instantiate(prefab, pivot).transform;
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            string kit = path.Substring(0, path.IndexOf('/'));
            Material material = Palette(kit);
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
                var slots = renderer.sharedMaterials;
                for (int slot = 0; slot < slots.Length; slot++)
                    slots[slot] = material;
                renderer.sharedMaterials = slots;
            }

            Vector3 center = pivot.InverseTransformPoint(bounds.center);
            model.localPosition -= new Vector3(center.x, center.y - bounds.extents.y, center.z);
            pivot.localScale = new Vector3(dimensions.x / bounds.size.x, dimensions.y / bounds.size.y, dimensions.z / bounds.size.z);
            pivot.localPosition = position;
            pivot.localRotation = Quaternion.Euler(euler);
        }

        private Material Palette(string kit)
        {
            if (palettes.TryGetValue(kit, out Material material))
                return material;
            material = Paint(1, 1, 1);
            material.mainTexture = Resources.Load<Texture2D>(kit + "/colormap");
            palettes.Add(kit, material);
            return material;
        }

        private Material Paint(float red, float green, float blue)
        {
            var material = new Material(shader) { color = new Color(red, green, blue) };
            material.SetFloat("_Glossiness", .14f);
            return material;
        }

        private void Box(string name, Vector3 position, Vector3 scale, Material material, Quaternion? rotation = null)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            Object.Destroy(box.GetComponent<BoxCollider>());
            box.transform.SetParent(root, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            box.transform.localRotation = rotation ?? Quaternion.identity;
            box.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Transform Group(string name, Transform parent)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }
    }
}

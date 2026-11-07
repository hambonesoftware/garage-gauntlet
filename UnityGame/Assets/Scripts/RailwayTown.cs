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
        private readonly List<CrossingSignal> signals = new List<CrossingSignal>();
        private readonly Dictionary<Material, List<CombineInstance>> scenery = new Dictionary<Material, List<CombineInstance>>();
        private Mesh cube;
        private Material signalOff, signalRed;
        private sealed class CrossingSignal
        {
            public float CrossingX;
            public Transform Arm;
            public Renderer Lamp;
        }

        public Transform Car { get; }
        public Transform Train { get; }
        public Camera Camera { get; }
        public TrailRenderer BoostTrail { get; }

        public RailwayTown(Transform root, RaceCourse course, Shader shader)
        {
            this.root = root;
            this.shader = shader != null ? shader : Shader.Find("Standard");
            SetupLighting();
            DrawBoard(course);
            DrawTown();
            DrawRailway(course);
            CombineScenery();
            Car = CreateRacer("Player RC", new Color(.45f, .98f, .86f), 0);
            Transform trailAnchor = Group("Boost trail", Car);
            trailAnchor.localPosition = new Vector3(0, .12f, -.65f);
            BoostTrail = trailAnchor.gameObject.AddComponent<TrailRenderer>();
            BoostTrail.time = .22f;
            BoostTrail.startWidth = .53f;
            BoostTrail.endWidth = .04f;
            BoostTrail.minVertexDistance = .12f;
            BoostTrail.sharedMaterial = Glow(.15f, .90f, 1);
            BoostTrail.emitting = false;
            Train = Group("Model train", root);
            Model("train/train-locomotive-a", Train, new Vector3(1.13f, 0, 0),
                new Vector3(0, 90, 0), new Vector3(1.05f, 1.32f, 2.1f));
            Model("train/train-carriage-wood", Train, new Vector3(-1.10f, 0, 0),
                new Vector3(0, 90, 0), new Vector3(1.05f, 1.18f, 2.1f));
            Model("train/train-carriage-container-red", Train, new Vector3(-3.35f, 0, 0),
                new Vector3(0, 90, 0), new Vector3(1.05f, 1.23f, 2.1f));

            Camera = Group("Race camera", root).gameObject.AddComponent<Camera>();
            Camera.gameObject.AddComponent<AudioListener>();
            Camera.orthographic = true;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(.14f, .21f, .29f);
            Camera.farClipPlane = 100;
        }

        public Transform CreateRacer(string name, Color color, int variant)
        {
            Transform racer = Group(name, root);
            Model("toy/vehicle-racer", racer, Vector3.zero, new Vector3(0, 180, 0),
                new Vector3(.96f, .62f, 1.6f), variant % 2 == 0 ? "colormap" : "variation-b");
            if (variant != 0)
                Box("Team stripe", new Vector3(0, .64f, -.06f), new Vector3(.34f, .03f, .32f),
                    Paint(color.r, color.g, color.b), parent: racer);
            return racer;
        }

        public Transform CreateBoostPad(Vector3 position, Vector3 forward)
        {
            Transform pad = Group("Battery pickup", root);
            pad.SetPositionAndRotation(position + Vector3.up * .061f, Quaternion.LookRotation(forward));
            Material outer = Glow(.15f, .75f, .70f);
            Material inner = Paint(.14f, .35f, .41f);
            Box("Charge plate", Vector3.zero, new Vector3(1.36f, .035f, 1.5f), outer, parent: pad);
            Box("Grip inlay", Vector3.up * .023f, new Vector3(1.14f, .025f, 1.27f), inner, parent: pad);
            for (int row = 0; row < 2; row++)
            {
                float z = -.35f + row * .52f;
                Box("Chevron", new Vector3(-.20f, .05f, z), new Vector3(.10f, .02f, .55f), outer,
                    Quaternion.Euler(0, 48, 0), pad);
                Box("Chevron", new Vector3(.20f, .05f, z), new Vector3(.10f, .02f, .55f), outer,
                    Quaternion.Euler(0, -48, 0), pad);
            }
            return pad;
        }

        public static bool SignalClosed(float trainX, float crossingX) =>
            trainX >= crossingX - 7.6f && trainX <= crossingX + 5.15f;

        public static bool TrainHits(float trainX, Vector3 position) =>
            Mathf.Abs(position.z) < .92f && position.x >= trainX - 4.65f && position.x <= trainX + 2.65f;

        public void UpdateTrain(float x, float elapsed, float dt)
        {
            Train.position = new Vector3(x, .13f, 0);
            bool visible = x > -21 && x < 23;
            if (Train.gameObject.activeSelf != visible)
                Train.gameObject.SetActive(visible);
            bool flash = (int)(elapsed * 7) % 2 == 0;
            foreach (CrossingSignal signal in signals)
            {
                bool closed = SignalClosed(x, signal.CrossingX);
                Quaternion target = Quaternion.Euler(0, 0, closed ? 0 : 77);
                signal.Arm.localRotation = Quaternion.RotateTowards(signal.Arm.localRotation, target, dt * 150);
                Material lamp = closed && flash ? signalRed : signalOff;
                if (signal.Lamp.sharedMaterial != lamp)
                    signal.Lamp.sharedMaterial = lamp;
            }
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
            RenderSettings.ambientLight = new Color(.51f, .57f, .73f);
            Light sun = Group("Workshop sunlight", root).gameObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1, .84f, .63f);
            sun.intensity = 1.16f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(43, -38, 0);
        }

        private void DrawBoard(RaceCourse course)
        {
            Material wood = Paint(.30f, .14f, .09f);
            Material trim = Paint(.69f, .40f, .23f);
            Material grass = Paint(.29f, .44f, .31f);
            Material asphalt = Paint(.20f, .24f, .31f);
            Material shoulder = Paint(.68f, .51f, .35f);
            Material cream = Paint(.92f, .85f, .68f);
            Box("Workshop table", new Vector3(0, -1.44f, 0), new Vector3(76, .6f, 61), Paint(.19f, .27f, .34f));
            Box("Wooden display board", new Vector3(0, -.56f, 0), new Vector3(38, .9f, 30), wood);
            Box("Tabletop edge band", new Vector3(0, -.25f, 0), new Vector3(38.1f, .12f, 30.1f), trim);
            Box("Felt landscape", new Vector3(0, -.105f, 0), new Vector3(36.5f, .17f, 28.5f), grass);
            Ribbon(course, RaceCourse.HalfWidth + .21f, .024f, shoulder, "Road shoulder");
            Ribbon(course, RaceCourse.HalfWidth, .035f, asphalt, "RC course");
            Material curbRed = Paint(.74f, .29f, .20f);
            for (int i = 0; i < course.Count; i += 2)
            {
                Vector3 at = course.At(i);
                Vector3 tangent = course.Tangent(i);
                if (Mathf.Abs(at.z) < 1.55f ||
                    Mathf.Abs(Vector3.SignedAngle(course.Tangent(i - 2), course.Tangent(i + 2), Vector3.up)) < 10)
                    continue;
                Vector3 side = Vector3.Cross(Vector3.up, tangent) * (RaceCourse.HalfWidth + .03f);
                Box("Corner curb", at + side + Vector3.up * .065f, new Vector3(.27f, .09f, .61f),
                    i % 4 == 0 ? curbRed : cream, Quaternion.LookRotation(tangent));
                Box("Corner curb", at - side + Vector3.up * .065f, new Vector3(.27f, .09f, .61f),
                    i % 4 == 0 ? curbRed : cream, Quaternion.LookRotation(tangent));
            }

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
            House("building-type-a", -7.1f, 3.1f, 125, "variation-b");
            House("building-type-d", -3.9f, 5.4f, 176, "variation-c");
            House("building-type-g", -.1f, 3.4f, 180, "colormap");
            House("building-type-m", -7.4f, -3.6f, 22, "variation-c");
            House("building-type-d", -3.8f, -3.5f, 5, "variation-b");
            House("building-type-a", -.4f, -3.8f, 0, "colormap");
            House("building-type-g", 3.2f, -3.9f, 0, "variation-c");
            House("building-type-m", 7.3f, -3.2f, 340, "variation-b");

            Material platform = Paint(.58f, .47f, .36f);
            Material canopy = Paint(.26f, .35f, .43f);
            Material iron = Paint(.13f, .19f, .26f);
            Material amber = Glow(1, .62f, .27f);
            Box("Station platform", new Vector3(-3.5f, .06f, 1.52f), new Vector3(7.1f, .16f, 1.2f), platform);
            Box("Station canopy", new Vector3(-3.5f, 1.34f, 1.55f), new Vector3(6.8f, .19f, 1.3f), canopy);
            for (int i = 0; i < 4; i++)
            {
                float x = -6.55f + 2.02f * i;
                Box("Canopy column", new Vector3(x, .68f, 1.92f), new Vector3(.11f, 1.1f, .11f), iron);
                Box("Platform light", new Vector3(x, 1.19f, 1.5f), new Vector3(.48f, .085f, .16f), amber);
            }
            for (int i = 0; i < 5; i++)
            {
                float x = -8.2f + i * 4.05f;
                Box("Street light mast", new Vector3(x, .74f, -5.35f), new Vector3(.07f, 1.5f, .07f), iron);
                Box("Street light cap", new Vector3(x, 1.49f, -5.35f), new Vector3(.27f, .13f, .23f), amber);
            }

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

        private void House(string model, float x, float z, float angle, string texture)
        {
            Vector3 position = new Vector3(x, -.025f, z);
            housePositions.Add(position);
            Model("town/" + model, root, position, new Vector3(0, angle, 0), new Vector3(2.1f, 1.65f, 1.95f), texture);
        }

        private void DrawRailway(RaceCourse course)
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
            Material stripe = Paint(.90f, .87f, .74f);
            signalOff = Paint(.30f, .14f, .12f);
            signalRed = Glow(1, .16f, .08f);
            foreach (Vector3 crossing in course.Crossings)
            {
                float x = crossing.x - 2.18f;
                float z = crossing.x > 0 ? -1.25f : 1.25f;
                Box("Crossing signal post", new Vector3(x, .65f, z), new Vector3(.13f, 1.25f, .13f), metal);
                Box("Signal housing", new Vector3(x, 1.19f, z), new Vector3(.35f, .33f, .29f), warning);
                Renderer lamp = Box("Crossing lamp", new Vector3(x, 1.39f, z), new Vector3(.29f, .14f, .27f), signalOff, parent: root);
                Transform arm = Group("Crossing arm", root);
                arm.localPosition = new Vector3(x, .72f, z);
                arm.localRotation = Quaternion.Euler(0, 0, 77);
                for (int section = 0; section < 7; section++)
                    Box("Arm stripe", new Vector3(.17f + .31f * section, 0, 0), new Vector3(.31f, .10f, .09f),
                        section % 2 == 0 ? signalRed : stripe, parent: arm);
                signals.Add(new CrossingSignal { CrossingX = crossing.x, Arm = arm, Lamp = lamp });
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

        private void Model(string path, Transform parent, Vector3 position, Vector3 euler, Vector3 dimensions, string texture = "colormap")
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
                throw new System.InvalidOperationException("Missing model " + path);

            Transform pivot = Group(prefab.name, parent);
            Transform model = Object.Instantiate(prefab, pivot).transform;
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            string kit = path.Substring(0, path.IndexOf('/'));
            Material material = Palette(kit + "/" + texture);
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

        private Material Palette(string texture)
        {
            if (palettes.TryGetValue(texture, out Material material))
                return material;
            material = Paint(1, 1, 1);
            material.mainTexture = Resources.Load<Texture2D>(texture);
            palettes.Add(texture, material);
            return material;
        }

        private Material Paint(float red, float green, float blue)
        {
            var material = new Material(shader) { color = new Color(red, green, blue) };
            material.SetFloat("_Glossiness", .14f);
            return material;
        }

        private Material Glow(float red, float green, float blue)
        {
            Material material = Paint(red, green, blue);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(red, green, blue) * .45f);
            return material;
        }

        private Renderer Box(string name, Vector3 position, Vector3 scale, Material material,
            Quaternion? rotation = null, Transform parent = null)
        {
            Quaternion orientation = rotation ?? Quaternion.identity;
            if (parent == null)
            {
                if (cube == null)
                    cube = SceneryCube();
                if (!scenery.TryGetValue(material, out List<CombineInstance> meshes))
                {
                    meshes = new List<CombineInstance>();
                    scenery.Add(material, meshes);
                }
                meshes.Add(new CombineInstance { mesh = cube, transform = Matrix4x4.TRS(position, orientation, scale) });
                return null;
            }

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            Object.Destroy(box.GetComponent<BoxCollider>());
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            box.transform.localRotation = orientation;
            Renderer renderer = box.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            return renderer;
        }

        private static Mesh SceneryCube()
        {
            Vector3[] faces = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            var vertices = new Vector3[24];
            var normals = new Vector3[24];
            var triangles = new int[36];
            for (int face = 0; face < faces.Length; face++)
            {
                Vector3 normal = faces[face];
                Vector3 axis = Mathf.Abs(normal.y) > .5f ? Vector3.forward : Vector3.up;
                Vector3 u = Vector3.Cross(axis, normal) * .5f;
                Vector3 v = Vector3.Cross(normal, u);
                Vector3 center = normal * .5f;
                int vertex = face * 4;
                vertices[vertex] = center - u - v;
                vertices[vertex + 1] = center + u - v;
                vertices[vertex + 2] = center + u + v;
                vertices[vertex + 3] = center - u + v;
                for (int i = 0; i < 4; i++)
                    normals[vertex + i] = normal;
                int triangle = face * 6;
                triangles[triangle] = triangles[triangle + 3] = vertex;
                triangles[triangle + 1] = vertex + 1;
                triangles[triangle + 2] = triangles[triangle + 4] = vertex + 2;
                triangles[triangle + 5] = vertex + 3;
            }
            return new Mesh { name = "Scenery cube", vertices = vertices, normals = normals, triangles = triangles };
        }

        private void CombineScenery()
        {
            foreach (KeyValuePair<Material, List<CombineInstance>> batch in scenery)
            {
                var mesh = new Mesh { name = "Tabletop scenery" };
                mesh.CombineMeshes(batch.Value.ToArray());
                Transform group = Group("Scenery batch", root);
                group.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                group.gameObject.AddComponent<MeshRenderer>().sharedMaterial = batch.Key;
            }
            scenery.Clear();
        }

        private static Transform Group(string name, Transform parent)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace GarageGauntlet
{
    internal sealed class RailwayTown
    {
        private readonly Transform root;
        private readonly Shader shader;
        private readonly Dictionary<string, Material> palettes = new Dictionary<string, Material>();
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

        public ScaleRcCar PlayerModel { get; }
        public Transform Car => PlayerModel.Root;
        public Transform Train { get; }
        public Camera Camera { get; }

        public RailwayTown(Transform root, RaceCourse course, Shader shader)
        {
            this.root = root;
            this.shader = shader != null ? shader : Shader.Find("Standard");
            SetupLighting();
            DrawBoard(course);
            DrawTown();
            DrawRailway(course);
            CombineScenery();
            PlayerModel = new ScaleRcCar(root, this.shader);
            Train = Group("Model train", root);
            Model("realism/locomotive", Train, new Vector3(.45f, 0, 0),
                new Vector3(0, 90, 0), new Vector3(.98f, .96f, 3.48f), "locomotive-color");
            Model("train/train-carriage-container-red", Train, new Vector3(-2.91f, 0, 0),
                new Vector3(0, 90, 0), new Vector3(1.05f, 1.23f, 3.10f));

            Camera = Group("Race camera", root).gameObject.AddComponent<Camera>();
            Camera.gameObject.AddComponent<AudioListener>();
            Camera.orthographic = true;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(.14f, .21f, .29f);
            Camera.farClipPlane = 100;
        }

        private static readonly Color[] SlotColors =
        {
            new Color(.94f, .34f, .25f), new Color(.34f, .91f, .62f),
            new Color(.98f, .72f, .25f), new Color(.35f, .58f, .98f)
        };

        public static Color SlotColor(int lane) => SlotColors[lane];

        public ScaleRcCar CreateRacer(string name) => new ScaleRcCar(root, shader, name);

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

        private void SetupLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.51f, .57f, .73f);
            Cubemap reflection = Resources.Load<Cubemap>("realism/workbench-reflection");
            if (reflection != null)
            {
                RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
                RenderSettings.customReflectionTexture = reflection;
                RenderSettings.reflectionIntensity = .73f;
            }
            Light sun = Group("Workshop sunlight", root).gameObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1, .84f, .63f);
            sun.intensity = 1.16f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(43, -38, 0);
        }

        private void DrawBoard(RaceCourse course)
        {
            Material wood = Surface("timber", new Color(.79f, .58f, .45f), .28f);
            Material trim = Paint(.69f, .40f, .23f);
            Material grass = Paint(.29f, .44f, .31f);
            Material plastic = Paint(.16f, .18f, .21f);
            Material slot = Paint(.047f, .054f, .065f);
            Material contact = Paint(.69f, .66f, .57f);
            contact.SetFloat("_Glossiness", .48f);
            Material shoulder = Paint(.27f, .29f, .32f);
            Material cream = Paint(.92f, .85f, .68f);
            Box("Workshop table", new Vector3(0, -1.44f, 0), new Vector3(76, .6f, 61), Paint(.19f, .27f, .34f));
            SurfaceBox("Wooden display board", new Vector3(0, -.56f, 0), new Vector3(38, .9f, 30), new Vector2(4.1f, 1.4f), wood);
            Box("Tabletop edge band", new Vector3(0, -.25f, 0), new Vector3(38.1f, .12f, 30.1f), trim);
            Box("Felt landscape", new Vector3(0, -.105f, 0), new Vector3(36.5f, .17f, 28.5f), grass);
            Ribbon(course, RaceCourse.HalfWidth + .16f, .024f, shoulder, "Slot track bevel");
            Ribbon(course, RaceCourse.HalfWidth, .035f, plastic, "Molded slot track");
            for (int lane = 0; lane < RaceCourse.LaneCount; lane++)
            {
                float offset = RaceCourse.SlotOffset(lane);
                Color color = SlotColor(lane);
                Ribbon(course, .039f, .055f, slot, "Guide slot", offset, true);
                Ribbon(course, .018f, .056f, contact, "Left power contact", offset - .103f, true);
                Ribbon(course, .018f, .056f, contact, "Right power contact", offset + .103f, true);
                Ribbon(course, .025f, .054f, Paint(color.r, color.g, color.b), "Lane stripe", offset + .34f, true);
            }
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

            for (float distance = 1.7f; distance < course.Length - 1.2f; distance += 3.7f)
            {
                Vector3 at = course.Sample(distance, out Vector3 direction);
                if (Mathf.Abs(at.z) > 1)
                    Box("Snap track section joint", at + Vector3.up * .039f, new Vector3(4.23f, .003f, .038f),
                        slot, Quaternion.LookRotation(direction));
            }

            Vector3 start = course.Sample(0, out Vector3 forward);
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Quaternion turn = Quaternion.LookRotation(forward);
            for (int i = 0; i < 8; i++)
            {
                Vector3 square = start + right * ((i - 3.5f) * .51f) + Vector3.up * .052f;
                Box("Start check", square, new Vector3(.51f, .012f, .28f), i % 2 == 0 ? cream : plastic, turn);
                Box("Finish check", square + forward * .28f, new Vector3(.51f, .012f, .28f), i % 2 == 0 ? plastic : cream, turn);
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
            Craftsman(-3.8f, -3.5f, 5, new Color(.91f, .78f, .69f));
            Craftsman(-.4f, -3.8f, 0, Color.white);
            Craftsman(3.2f, -3.9f, 348, new Color(.78f, .88f, .89f));
            House("building-type-m", 7.3f, -3.2f, 340, "variation-b");

            DrawBrickStation();
            Material platform = Surface("timber", new Color(.83f, .77f, .65f), .13f);
            Material canopy = Paint(.26f, .35f, .43f);
            Material iron = Paint(.13f, .19f, .26f);
            Material amber = Glow(1, .62f, .27f);
            SurfaceBox("Station platform boards", new Vector3(-3.5f, .06f, 1.52f),
                new Vector3(7.1f, .16f, 1.2f), new Vector2(1.2f, .48f), platform);
            Box("Station canopy", new Vector3(-3.5f, 1.34f, 1.55f), new Vector3(6.8f, .19f, 1.3f), canopy);
            Box("Canopy flashing", new Vector3(-3.5f, 1.34f, .903f), new Vector3(6.9f, .18f, .07f), iron);
            Box("Platform edge stripe", new Vector3(-3.5f, .145f, .984f), new Vector3(6.94f, .012f, .11f),
                Paint(.72f, .60f, .29f));
            for (int i = 0; i < 16; i++)
            {
                float x = -6.69f + i * .42f;
                Box("Roof seam", new Vector3(x, 1.445f, 1.55f), new Vector3(.022f, .026f, 1.30f), iron);
            }
            for (int i = 0; i < 4; i++)
            {
                float x = -6.55f + 2.02f * i;
                Box("Canopy column", new Vector3(x, .68f, 1.92f), new Vector3(.11f, 1.1f, .11f), iron);
                Box("Platform light", new Vector3(x, 1.19f, 1.5f), new Vector3(.48f, .085f, .16f), amber);
            }
            for (int i = 0; i < 2; i++)
            {
                float x = -5.4f + i * 3.2f;
                Box("Platform bench seat", new Vector3(x, .40f, 1.73f), new Vector3(.96f, .055f, .24f), canopy);
                Box("Platform bench back", new Vector3(x, .52f, 1.83f), new Vector3(.96f, .19f, .036f), canopy);
                Box("Bench leg", new Vector3(x - .32f, .25f, 1.73f), new Vector3(.056f, .28f, .19f), iron);
                Box("Bench leg", new Vector3(x + .32f, .25f, 1.73f), new Vector3(.056f, .28f, .19f), iron);
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
            Model("town/" + model, root, position, new Vector3(0, angle, 0), new Vector3(2.1f, 1.65f, 1.95f), texture);
        }

        private void Craftsman(float x, float z, float angle, Color tint)
        {
            Vector3 position = new Vector3(x, -.025f, z);
            var material = new Material(Palette("realism/craftsman-palette")) { color = tint };
            Model("realism/craftsman", root, position, new Vector3(0, angle + 180, 0),
                new Vector3(2.4f, 1.8f, 2.15f), uniform: true, finish: material);
        }

        private void DrawBrickStation()
        {
            const float x = -3.55f, z = 3.04f, front = 2.19f, back = 3.89f, width = 4.35f;
            const float eave = 1.59f, ridge = 2.20f;
            Material brick = Surface("brick", new Color(.83f, .76f, .70f), .12f);
            Material tile = Surface("roof", new Color(.73f, .73f, .76f), .17f);
            Material stone = Paint(.66f, .60f, .49f);
            Material fascia = Paint(.30f, .28f, .25f);
            Material frames = Paint(.80f, .75f, .62f);
            Material glazing = Paint(.17f, .25f, .28f);
            Material ticketWindow = Glow(.79f, .51f, .23f);

            Box("Station foundation", new Vector3(x, .07f, z), new Vector3(width + .13f, .20f, 1.78f), stone);
            SurfaceBox("Weathered station brick", new Vector3(x, .87f, z), new Vector3(width, 1.44f, 1.70f),
                new Vector2(.67f, .67f), brick);
            Box("Station lintel band", new Vector3(x, 1.39f, front - .009f), new Vector3(width, .058f, .047f), stone);
            Box("Front gutter", new Vector3(x, eave, front - .105f), new Vector3(width + .30f, .064f, .082f), fascia);
            Box("Rear gutter", new Vector3(x, eave, back + .105f), new Vector3(width + .30f, .064f, .082f), fascia);

            float rise = ridge - eave;
            float run = (back - front) / 2 + .13f;
            float roofAngle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            float roofDepth = Mathf.Sqrt(rise * rise + run * run);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 position = new Vector3(x, (ridge + eave) / 2, z + side * run / 2);
                SurfaceBox("Clay tile roof", position, new Vector3(width + .29f, .075f, roofDepth),
                    new Vector2(.84f, .74f), tile, Quaternion.Euler(side * roofAngle, 0, 0));
            }
            Box("Terracotta ridge cap", new Vector3(x, ridge + .028f, z), new Vector3(width + .31f, .082f, .15f),
                Paint(.46f, .27f, .20f));
            DrawGables(x, z, width, front, back, eave, ridge, brick);
            SurfaceBox("Station chimney", new Vector3(x + 1.22f, 2.13f, z + .31f), new Vector3(.38f, .72f, .36f),
                new Vector2(.67f, .67f), brick);
            Box("Chimney coping", new Vector3(x + 1.22f, 2.50f, z + .31f), new Vector3(.47f, .076f, .45f), stone);
            Box("Chimney cap", new Vector3(x + 1.22f, 2.55f, z + .31f), new Vector3(.26f, .07f, .23f), fascia);

            for (int i = 0; i < 4; i++)
            {
                float wx = x + (i - 1.5f) * .96f;
                Box("Recessed sash surround", new Vector3(wx, .87f, front - .016f), new Vector3(.60f, .69f, .07f), frames);
                Box("Sash window glass", new Vector3(wx, .87f, front - .056f), new Vector3(.47f, .56f, .02f),
                    i == 1 ? ticketWindow : glazing);
                Box("Sash centre bar", new Vector3(wx, .87f, front - .075f), new Vector3(.027f, .56f, .018f), frames);
                Box("Sash cross bar", new Vector3(wx, .87f, front - .075f), new Vector3(.47f, .029f, .018f), frames);
                Box("Stone sill", new Vector3(wx, .505f, front - .053f), new Vector3(.67f, .066f, .14f), stone);
            }
            Box("Enamel station sign", new Vector3(x, 1.27f, front - .051f), new Vector3(1.42f, .23f, .042f), frames);
            Transform name = Group("Switchyard station sign", root);
            name.localPosition = new Vector3(x, 1.269f, front - .081f);
            TextMesh text = name.gameObject.AddComponent<TextMesh>();
            text.text = "SWITCHYARD";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 48;
            text.characterSize = .032f;
            text.color = new Color(.18f, .19f, .20f);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                text.font = font;
                name.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }
        }

        private void DrawGables(float x, float z, float width, float front, float back, float eave, float ridge, Material brick)
        {
            float left = x - width / 2, right = x + width / 2;
            Vector3[] vertices =
            {
                new Vector3(left, eave, front), new Vector3(left, eave, back), new Vector3(left, ridge, z),
                new Vector3(right, eave, back), new Vector3(right, eave, front), new Vector3(right, ridge, z)
            };
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                uv[i] = new Vector2(vertices[i].z, vertices[i].y) / .67f;
            var mesh = new Mesh { name = "Brick gable ends", vertices = vertices, uv = uv,
                triangles = new[] { 0, 1, 2, 3, 4, 5 } };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            Transform ends = Group("Station gables", root);
            ends.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            ends.gameObject.AddComponent<MeshRenderer>().sharedMaterial = brick;
        }

        private void DrawRailway(RaceCourse course)
        {
            Material ballast = Surface("ballast", new Color(.74f, .70f, .64f), .08f);
            Material sleepers = Paint(.22f, .16f, .12f);
            Material metal = Paint(.74f, .76f, .73f);
            Material rust = Paint(.34f, .19f, .12f);
            Material steel = Paint(.62f, .64f, .64f);
            steel.SetFloat("_Metallic", .53f);
            steel.SetFloat("_Glossiness", .54f);
            SurfaceBox("Gravel rail bed", new Vector3(0, .021f, 0), new Vector3(36.4f, .05f, 1.55f),
                new Vector2(1.42f, 1.20f), ballast);
            for (int i = -34; i <= 34; i++)
            {
                float x = i * .52f;
                Box("Rail tie", new Vector3(x, .061f, 0), new Vector3(.17f, .053f, 1.30f), sleepers);
                if (x > 5.5f)
                {
                    Box("North tie plate", new Vector3(x, .093f, .43f), new Vector3(.135f, .018f, .17f), rust);
                    Box("South tie plate", new Vector3(x, .093f, -.43f), new Vector3(.135f, .018f, .17f), rust);
                }
            }
            foreach (float rail in new[] { -.43f, .43f })
            {
                Box("Oxidized rail web", new Vector3(0, .107f, rail), new Vector3(36.5f, .064f, .043f), rust);
                Box("Polished rail head", new Vector3(0, .14f, rail), new Vector3(36.5f, .018f, .062f), steel);
            }
            Material warning = Paint(.98f, .65f, .19f);
            Material stripe = Paint(.90f, .87f, .74f);
            signalOff = Paint(.30f, .14f, .12f);
            signalRed = Glow(1, .16f, .08f);
            foreach (Vector3 crossing in course.Crossings)
            {
                if (crossing.x > 0)
                    DetailCrossing(course, crossing);
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

        private void DetailCrossing(RaceCourse course, Vector3 crossing)
        {
            Material timber = Surface("timber", new Color(.82f, .76f, .69f), .16f);
            for (int i = 0; i < 3; i++)
            {
                float z = (i - 1) * .266f;
                SurfaceBox("Timber crossing deck", new Vector3(crossing.x, .103f, z),
                    new Vector3(4.54f, .048f, .243f), new Vector2(1.25f, .38f), timber);
            }
            Material chalk = Paint(.81f, .78f, .66f);
            Material pole = Paint(.40f, .44f, .45f);
            float gate = course.Locate(crossing, out _);
            Vector3 marking = course.Sample(gate - 2.15f, out Vector3 forward);
            Vector3 side = Vector3.Cross(Vector3.up, forward);
            Box("Stop line", marking + Vector3.up * .05f, new Vector3(3.75f, .011f, .13f), chalk,
                Quaternion.LookRotation(forward));
            Vector3 sign = marking - side * 2.52f;
            Box("Crossbuck mast", sign + Vector3.up * .72f, new Vector3(.065f, 1.48f, .065f), pole);
            Quaternion front = Quaternion.LookRotation(forward);
            Box("Crossbuck left board", sign + Vector3.up * 1.44f, new Vector3(.57f, .093f, .055f), chalk,
                front * Quaternion.Euler(0, 0, 42));
            Box("Crossbuck right board", sign + Vector3.up * 1.44f, new Vector3(.57f, .093f, .055f), chalk,
                front * Quaternion.Euler(0, 0, -42));
        }

        private void Ribbon(RaceCourse course, float halfWidth, float y, Material material, string name,
            float offset = 0, bool breakAtCrossing = false)
        {
            var vertices = new Vector3[course.Count * 2];
            var uv = new Vector2[vertices.Length];
            var triangles = new List<int>(course.Count * 6);
            for (int i = 0; i < course.Count; i++)
            {
                Vector3 center = course.At(i) + Vector3.up * y;
                Vector3 tangent = Vector3.Cross(Vector3.up, course.Tangent(i));
                center += tangent * offset;
                Vector3 side = tangent * halfWidth;
                int current = i * 2;
                int next = (current + 2) % vertices.Length;
                vertices[current] = center - side;
                vertices[current + 1] = center + side;
                uv[current] = new Vector2(vertices[current].x, vertices[current].z) / 2.3f;
                uv[current + 1] = new Vector2(vertices[current + 1].x, vertices[current + 1].z) / 2.3f;
                if (!breakAtCrossing || (Mathf.Abs(course.At(i).z) > .8f && Mathf.Abs(course.At(i + 1).z) > .8f))
                {
                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(current + 1);
                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next + 1);
                }
            }
            var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles.ToArray(), uv = uv };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            Transform surface = Group(name, root);
            surface.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            surface.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private void Model(string path, Transform parent, Vector3 position, Vector3 euler, Vector3 dimensions,
            string texture = "colormap", bool uniform = false, Material finish = null)
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
                throw new System.InvalidOperationException("Missing model " + path);

            Transform pivot = Group(prefab.name, parent);
            Transform model = Object.Instantiate(prefab, pivot).transform;
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            string kit = path.Substring(0, path.IndexOf('/'));
            Material material = finish != null ? finish : Palette(kit + "/" + texture);
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
            Vector3 scale = new Vector3(dimensions.x / bounds.size.x, dimensions.y / bounds.size.y, dimensions.z / bounds.size.z);
            pivot.localScale = uniform ? Vector3.one * Mathf.Min(scale.x, scale.y, scale.z) : scale;
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

        private Material Surface(string texture, Color tint, float gloss)
        {
            Material template = Resources.Load<Material>("realism/" + texture + "-surface");
            Material material = template != null ? new Material(template) : new Material(shader);
            material.color = tint;
            material.SetFloat("_Glossiness", gloss);
            return material;
        }

        private void SurfaceBox(string name, Vector3 position, Vector3 size, Vector2 tile, Material material,
            Quaternion? rotation = null)
        {
            Transform surface = Group(name, root);
            surface.localPosition = position;
            surface.localRotation = rotation ?? Quaternion.identity;
            surface.gameObject.AddComponent<MeshFilter>().sharedMesh = SceneryCube(size, tile);
            surface.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
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
                    cube = SceneryCube(Vector3.one, Vector2.one);
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

        private static Mesh SceneryCube(Vector3 size, Vector2 tile)
        {
            Vector3[] faces = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            var vertices = new Vector3[24];
            var normals = new Vector3[24];
            var uv = new Vector2[24];
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
                float tilesU = Vector3.Scale(u * 2, size).magnitude / tile.x;
                float tilesV = Vector3.Scale(v * 2, size).magnitude / tile.y;
                uv[vertex] = Vector2.zero;
                uv[vertex + 1] = new Vector2(tilesU, 0);
                uv[vertex + 2] = new Vector2(tilesU, tilesV);
                uv[vertex + 3] = new Vector2(0, tilesV);
                for (int i = 0; i < 4; i++)
                {
                    normals[vertex + i] = normal;
                    vertices[vertex + i] = Vector3.Scale(vertices[vertex + i], size);
                }
                int triangle = face * 6;
                triangles[triangle] = triangles[triangle + 3] = vertex;
                triangles[triangle + 1] = vertex + 1;
                triangles[triangle + 2] = triangles[triangle + 4] = vertex + 2;
                triangles[triangle + 5] = vertex + 3;
            }
            var mesh = new Mesh { name = "Scenery cube", vertices = vertices, normals = normals, triangles = triangles, uv = uv };
            mesh.RecalculateTangents();
            return mesh;
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

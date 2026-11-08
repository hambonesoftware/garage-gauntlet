using System.Collections.Generic;
using UnityEngine;

namespace GarageGauntlet
{
    internal sealed partial class RailwayTown
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
            public RailwayLoop.Crossing Crossing;
            public Transform Arm;
            public Renderer Lamp;
        }

        public ScaleRcCar PlayerModel { get; }
        public Transform Car => PlayerModel.Root;
        public Transform Train { get; }
        private readonly Transform wagon;
        public Camera Camera { get; }

        public RailwayTown(Transform root, RaceCourse course, Shader shader)
        {
            this.root = root;
            this.shader = shader != null ? shader : Shader.Find("Standard");
            SetupLighting();
            DrawBoard(course);
            DrawTown(course);
            DrawRailway(course);
            CombineScenery();
            PlayerModel = new ScaleRcCar(root, this.shader);
            Train = Group("Model train", root);
            wagon = Group("Model freight wagon", root);
            Model("realism/locomotive", Train, Vector3.zero, Vector3.zero,
                new Vector3(.98f, .96f, 3.48f), "locomotive-color");
            Model("train/train-carriage-container-red", wagon, Vector3.zero, Vector3.zero,
                new Vector3(1.05f, 1.23f, 3.10f));

            Camera = Group("Race camera", root).gameObject.AddComponent<Camera>();
            Camera.gameObject.AddComponent<AudioListener>();
            Camera.orthographic = true;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(.14f, .21f, .29f);
            Camera.farClipPlane = 270;
        }

        private static readonly Color[] SlotColors =
        {
            new Color(.94f, .34f, .25f), new Color(.34f, .91f, .62f),
            new Color(.98f, .72f, .25f), new Color(.35f, .58f, .98f)
        };

        public static Color SlotColor(int lane) => SlotColors[lane];

        public ScaleRcCar CreateRacer(string name) => new ScaleRcCar(root, shader, name);

        public void UpdateTrain(RailwayLoop railway, float elapsed, float dt)
        {
            Train.SetPositionAndRotation(railway.EnginePosition + Vector3.up * .13f, Quaternion.LookRotation(railway.EngineDirection));
            wagon.SetPositionAndRotation(railway.WagonPosition + Vector3.up * .13f, Quaternion.LookRotation(railway.WagonDirection));
            bool flash = (int)(elapsed * 7) % 2 == 0;
            foreach (CrossingSignal signal in signals)
            {
                bool closed = railway.SignalClosed(signal.Crossing);
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
            Material grass = ModelFlock(new Color(.32f, .38f, .23f), new Vector2(1f, 1f));
            Material plastic = Paint(.14f, .145f, .15f);
            plastic.mainTexture = ModelGrain();
            plastic.mainTextureScale = Vector2.one * 2.9f;
            plastic.SetFloat("_Glossiness", .31f);
            Material slot = Paint(.047f, .054f, .065f);
            Material contact = Paint(.69f, .66f, .57f);
            contact.SetFloat("_Glossiness", .48f);
            Material shoulder = Paint(.27f, .29f, .32f);
            Material cream = Paint(.92f, .85f, .68f);
            float width = 2 * RaceCourse.BoardHalfWidth;
            float depth = 2 * RaceCourse.BoardHalfDepth;
            Material worktop = Surface("timber", new Color(.48f, .37f, .30f), .23f);
            Vector3 center = RaceCourse.BoardCenter;
            SurfaceBox("Workshop table", center + Vector3.down * 1.44f, new Vector3(width + 24, .6f, depth + 20), new Vector2(13, 3), worktop);
            SurfaceBox("Wooden display board", center + Vector3.down * .56f, new Vector3(width, .9f, depth), new Vector2(4.1f, 1.4f), wood);
            Box("Tabletop edge band", center + Vector3.down * .25f, new Vector3(width + .1f, .12f, depth + .1f), trim);
            SurfaceBox("Flocked tabletop", center + Vector3.down * .105f, new Vector3(width - .3f, .17f, depth - .3f), new Vector2(3.4f, 3.4f), grass);
            DrawWorkbench(width, depth, wood, trim);
            Ribbon(course, RaceCourse.HalfWidth + .16f, .024f, shoulder, "Slot track bevel");
            Ribbon(course, RaceCourse.HalfWidth, .035f, plastic, "Molded slot track");
            DrawFlyover(course);
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
                Vector3 at = course.Sample(course.DistanceAt(i), out Vector3 tangent);
                if (NearCrossing(course, course.DistanceAt(i), 2.4f) ||
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
                if (!NearCrossing(course, distance, 1.8f))
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

        private void DrawFlyover(RaceCourse course)
        {
            Material girder = Paint(.24f, .29f, .33f);
            Material footing = Paint(.51f, .49f, .43f);
            Material rail = Paint(.77f, .65f, .40f);
            float start = course.Railway.BridgeStart;
            float end = course.Railway.BridgeEnd;
            float approach = start - RaceCourse.RampLength;
            float exit = end + RaceCourse.RampLength;
            int segments = Mathf.CeilToInt(exit - approach);
            float panel = (exit - approach) / segments;
            for (int i = 0; i < segments; i++)
            {
                Vector3 center = course.Sample(approach + (i + .5f) * panel, out Vector3 forward);
                Quaternion turn = Quaternion.LookRotation(forward);
                Vector3 side = turn * Vector3.right;
                Vector3 up = turn * Vector3.up;
                Box("Elevated deck slab", center - up * .095f, new Vector3(4.43f, .21f, panel + .12f), girder, turn);
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Vector3 edge = center + side * (sign * 2.24f);
                    Box("Bridge edge beam", edge + up * .105f, new Vector3(.16f, .33f, panel + .12f), girder, turn);
                    Box("Flyover safety rail", edge + up * .26f, new Vector3(.08f, .085f, panel + .12f), rail, turn);
                }
            }

            foreach (float distance in new[] { start - 4.3f, start - .75f, (start + end) / 2, end + .75f, end + 4.3f })
            {
                Vector3 deck = course.Sample(distance, out Vector3 forward);
                forward.y = 0;
                Quaternion turn = Quaternion.LookRotation(forward);
                Vector3 side = turn * Vector3.right;
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Vector3 basePoint = deck + side * (sign * 1.83f);
                    Box("Bridge footing", new Vector3(basePoint.x, .08f, basePoint.z), new Vector3(.70f, .18f, .70f), footing, turn);
                    Box("Bridge support", new Vector3(basePoint.x, deck.y / 2, basePoint.z), new Vector3(.42f, deck.y, .47f), footing, turn);
                }
            }
        }

        private void DrawTown(RaceCourse course)
        {
            DrawMainStreet();
            DrawTownBlocks();
            DrawTownSquare();
            DrawMillPond();
            DrawDepotYard();
            DrawCottageHill(course);
            DrawMapleJunctionDetails();
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
            RaceCourse rail = course.Railway.Path;
            Material ballast = Surface("ballast", new Color(.74f, .70f, .64f), .08f);
            Material sleepers = Paint(.22f, .16f, .12f);
            Material metal = Paint(.74f, .76f, .73f);
            Material rust = Paint(.34f, .19f, .12f);
            Material steel = Paint(.62f, .64f, .64f);
            steel.SetFloat("_Metallic", .53f);
            steel.SetFloat("_Glossiness", .54f);
            Ribbon(rail, .79f, .047f, ballast, "Loop ballast");
            int ties = Mathf.RoundToInt(rail.Length / .52f);
            for (int i = 0; i < ties; i++)
            {
                Vector3 point = rail.Sample(rail.Length * i / ties, out Vector3 forward);
                Quaternion turn = Quaternion.LookRotation(forward);
                Box("Rail tie", point + Vector3.up * .064f, new Vector3(1.30f, .053f, .17f), sleepers, turn);
            }
            foreach (float offset in new[] { -.43f, .43f })
            {
                Ribbon(rail, .030f, .107f, rust, "Oxidized rail web", offset);
                Ribbon(rail, .034f, .14f, steel, "Polished rail head", offset);
            }

            Material warning = Paint(.98f, .65f, .19f);
            Material stripe = Paint(.90f, .87f, .74f);
            Material timber = Surface("timber", new Color(.82f, .76f, .69f), .16f);
            signalOff = Paint(.30f, .14f, .12f);
            signalRed = Glow(1, .16f, .08f);
            foreach (RailwayLoop.Crossing crossing in course.Railway.Crossings)
            {
                Vector3 acrossRail = Vector3.Cross(Vector3.up, crossing.RailDirection);
                float deckWidth = 4.56f / Mathf.Max(.75f, Mathf.Abs(Vector3.Dot(acrossRail, crossing.RoadDirection)));
                Quaternion railTurn = Quaternion.LookRotation(acrossRail);
                for (int plank = -1; plank <= 1; plank++)
                    SurfaceBox("Timber crossing deck", crossing.Position + acrossRail * (plank * .265f) + Vector3.up * .104f,
                        new Vector3(deckWidth, .046f, .247f), new Vector2(1.25f, .38f), timber, railTurn);

                Vector3 approach = course.Sample(crossing.RoadDistance - 2.65f, out Vector3 forward);
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                Quaternion roadTurn = Quaternion.LookRotation(forward);
                Vector3 post = approach - right * 2.53f;
                Box("Crossing stop line", approach + Vector3.up * .050f, new Vector3(3.98f, .012f, .14f), stripe, roadTurn);
                Box("Crossing signal post", post + Vector3.up * .65f, new Vector3(.13f, 1.25f, .13f), metal);
                Box("Signal housing", post + Vector3.up * 1.19f, new Vector3(.35f, .33f, .29f), warning, roadTurn);
                Renderer lamp = Box("Crossing lamp", post + Vector3.up * 1.39f, new Vector3(.29f, .14f, .27f), signalOff, roadTurn, root);
                Transform hinge = Group("Crossing gate", root);
                hinge.SetPositionAndRotation(post + Vector3.up * .72f, roadTurn);
                Transform arm = Group("Crossing arm", hinge);
                arm.localRotation = Quaternion.Euler(0, 0, 77);
                for (int section = 0; section < 8; section++)
                    Box("Arm stripe", new Vector3(.16f + .31f * section, 0, 0), new Vector3(.31f, .10f, .09f),
                        section % 2 == 0 ? signalRed : stripe, parent: arm);
                signals.Add(new CrossingSignal { Crossing = crossing, Arm = arm, Lamp = lamp });
            }
        }

        private static bool NearCrossing(RaceCourse course, float distance, float range)
        {
            foreach (RailwayLoop.Crossing crossing in course.Railway.Crossings)
                if (Mathf.Abs(course.Delta(distance, crossing.RoadDistance)) < range)
                    return true;
            return false;
        }

        private void Ribbon(RaceCourse course, float halfWidth, float y, Material material, string name,
            float offset = 0, bool breakAtCrossing = false)
        {
            var vertices = new Vector3[course.Count * 2];
            var uv = new Vector2[vertices.Length];
            var triangles = new List<int>(course.Count * 6);
            for (int i = 0; i < course.Count; i++)
            {
                Vector3 center = course.At(i) + Vector3.up * (y + course.ElevationAt(course.DistanceAt(i)));
                Vector3 tangent = Vector3.Cross(Vector3.up, course.Tangent(i));
                center += tangent * offset;
                Vector3 side = tangent * halfWidth;
                int current = i * 2;
                int next = (current + 2) % vertices.Length;
                vertices[current] = center - side;
                vertices[current + 1] = center + side;
                uv[current] = new Vector2(vertices[current].x, vertices[current].z) / 2.3f;
                uv[current + 1] = new Vector2(vertices[current + 1].x, vertices[current + 1].z) / 2.3f;
                if (!breakAtCrossing || !NearCrossing(course, (course.DistanceAt(i) + course.DistanceAt(i + 1)) / 2, .95f))
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
                AddScenery(cube, material, position, orientation, scale);
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

        private void AddScenery(Mesh mesh, Material material, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!scenery.TryGetValue(material, out List<CombineInstance> meshes))
            {
                meshes = new List<CombineInstance>();
                scenery.Add(material, meshes);
            }
            meshes.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(position, rotation, scale) });
        }

        private void CombineScenery()
        {
            foreach (KeyValuePair<Material, List<CombineInstance>> batch in scenery)
            {
                var mesh = new Mesh { name = "Tabletop scenery", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
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

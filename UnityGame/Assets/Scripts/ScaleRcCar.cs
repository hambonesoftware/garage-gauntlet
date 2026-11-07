using System.Collections.Generic;
using UnityEngine;

namespace GarageGauntlet
{
    internal sealed class ScaleRcCar
    {
        private const float TireRadius = .207f;
        private readonly Transform body;
        private readonly Transform[] hubs = new Transform[4];
        private readonly Transform[] tires = new Transform[4];
        private float roll, previousSpeed, steerAngle, lean, pitch;
        private static Mesh cube;
        public Transform Root { get; }

        public ScaleRcCar(Transform parent, Shader shader)
        {
            Root = Group("Scale RC rally car", parent);
            body = Group("Painted RC shell", Root);

            Material paint = Finish(shader, new Color(.90f, .22f, .087f), .53f, .13f);
            Material cream = Finish(shader, new Color(.91f, .87f, .73f), .38f);
            Material rubber = Finish(shader, new Color(.043f, .047f, .049f), .07f);
            Material tread = Finish(shader, new Color(.073f, .078f, .083f), .12f);
            Material plastic = Finish(shader, new Color(.075f, .083f, .095f), .24f);
            Material glass = Finish(shader, new Color(.12f, .20f, .26f), .86f, .35f);
            Material metal = Finish(shader, new Color(.61f, .65f, .67f), .48f, .57f);
            Material lamps = Finish(shader, new Color(.97f, .87f, .62f), .64f, .18f);
            Material tail = Finish(shader, new Color(.65f, .043f, .025f), .52f);

            MakeBody(paint, cream, plastic, glass, metal, lamps, tail);
            MakeWheels(rubber, tread, plastic, metal, paint);
        }

        public void Animate(float speed, float steer, float dt)
        {
            roll = Mathf.Repeat(roll + speed * dt / TireRadius * Mathf.Rad2Deg, 360);
            float blend = 1 - Mathf.Exp(-12 * dt);
            steerAngle = Mathf.Lerp(steerAngle, 26 * steer, blend);
            lean = Mathf.Lerp(lean, -5 * steer * Mathf.Clamp01(Mathf.Abs(speed) / 7), blend);
            float weightShift = Mathf.Clamp((speed - previousSpeed) / Mathf.Max(dt, .01f) * -.12f, -3, 3);
            pitch = Mathf.Lerp(pitch, weightShift, blend);
            previousSpeed = speed;
            for (int i = 0; i < tires.Length; i++)
            {
                if (i < 2)
                    hubs[i].localRotation = Quaternion.Euler(0, steerAngle, 0);
                tires[i].localRotation = Quaternion.Euler(roll, 0, 0);
            }
            body.localRotation = Quaternion.Euler(pitch, 0, lean);
        }

        public void ResetMotion()
        {
            previousSpeed = steerAngle = lean = pitch = 0;
            hubs[0].localRotation = hubs[1].localRotation = Quaternion.identity;
            body.localRotation = Quaternion.identity;
        }

        private void MakeBody(Material paint, Material cream, Material plastic, Material glass,
            Material metal, Material lamps, Material tail)
        {
            Vector4[] sections =
            {
                new Vector4(-.83f, .33f, .20f, .34f), new Vector4(-.77f, .42f, .15f, .42f),
                new Vector4(-.58f, .456f, .16f, .46f), new Vector4(-.33f, .432f, .15f, .465f),
                new Vector4(.29f, .434f, .15f, .448f), new Vector4(.56f, .448f, .16f, .40f),
                new Vector4(.76f, .399f, .17f, .353f), new Vector4(.84f, .33f, .20f, .31f)
            };
            Part("Beveled painted shell", body, Hull(sections), paint);

            Vector3 fl = new Vector3(-.35f, .44f, .32f), fr = new Vector3(.35f, .44f, .32f);
            Vector3 rl = new Vector3(-.35f, .455f, -.53f), rr = new Vector3(.35f, .455f, -.53f);
            Vector3 tf = new Vector3(-.274f, .66f, .055f), tr = new Vector3(.274f, .66f, .055f);
            Vector3 tl = new Vector3(-.265f, .665f, -.31f), tb = new Vector3(.265f, .665f, -.31f);
            Part("Windscreen and smoked side glazing", body, Faces(fl, fr, tr, tf, fr, rr, tb, tr,
                rr, rl, tl, tb, rl, fl, tf, tl), glass);
            Part("Painted roof", body, Faces(tf, tr, tb, tl), paint);
            Part("Roof stripe", body, Faces(new Vector3(-.069f, .663f, .052f), new Vector3(.069f, .663f, .052f),
                new Vector3(.069f, .668f, -.307f), new Vector3(-.069f, .668f, -.307f)), cream);
            Part("Bonnet stripe", body, Faces(new Vector3(-.069f, .404f, .56f), new Vector3(.069f, .404f, .56f),
                new Vector3(.069f, .450f, .30f), new Vector3(-.069f, .450f, .30f),
                new Vector3(-.069f, .357f, .758f), new Vector3(.069f, .357f, .758f),
                new Vector3(.069f, .404f, .56f), new Vector3(-.069f, .404f, .56f)), cream);

            Block("Front splitter", body, new Vector3(0, .194f, .803f), new Vector3(.72f, .044f, .12f), plastic);
            Block("Radiator grille", body, new Vector3(0, .262f, .836f), new Vector3(.29f, .074f, .023f), plastic);
            Block("Rear bumper", body, new Vector3(0, .203f, -.801f), new Vector3(.72f, .055f, .075f), plastic);
            Block("Rear wing", body, new Vector3(0, .53f, -.737f), new Vector3(.66f, .035f, .17f), plastic, Quaternion.Euler(-9, 0, 0));
            for (int side = -1; side <= 1; side += 2)
            {
                Block("Wing bracket", body, new Vector3(side * .21f, .468f, -.735f), new Vector3(.044f, .113f, .053f), plastic);
                Block("Clear headlamp", body, new Vector3(side * .26f, .305f, .796f), new Vector3(.14f, .042f, .066f), lamps);
                Block("Red tail lamp", body, new Vector3(side * .26f, .334f, -.813f), new Vector3(.14f, .045f, .044f), tail);
                Block("Side sill", body, new Vector3(side * .416f, .188f, -.005f), new Vector3(.045f, .046f, .63f), plastic);
                Block("Side mirror", body, new Vector3(side * .385f, .479f, .266f), new Vector3(.10f, .044f, .067f), paint);
                Block("Shell clip", body, new Vector3(side * .16f, .393f, .604f), new Vector3(.04f, .019f, .063f), metal);
                Block("Door handle", body, new Vector3(side * .437f, .386f, -.23f), new Vector3(.021f, .017f, .088f), plastic);
            }
            Block("Windscreen wiper", body, new Vector3(.062f, .467f, .289f), new Vector3(.28f, .012f, .018f),
                plastic, Quaternion.Euler(0, 10, 0));
        }

        private void MakeWheels(Material rubber, Material tread, Material plastic, Material metal, Material paint)
        {
            Mesh tire = Tire();
            Mesh treads = Tread();
            Mesh rim = AxleCylinder(.122f, .026f, 20);
            Mesh hub = AxleCylinder(.029f, .031f, 10);
            Mesh shock = AxleCylinder(.023f, .19f, 10);
            for (int i = 0; i < hubs.Length; i++)
            {
                float side = i % 2 == 0 ? -1 : 1;
                float z = i < 2 ? .536f : -.553f;
                hubs[i] = Group("Steering hub", Root);
                hubs[i].localPosition = new Vector3(side * .414f, TireRadius - .065f, z);
                tires[i] = Group("Rolling wheel", hubs[i]);
                Part("Rubber tire", tires[i], tire, rubber);
                Part("Molded tire tread", tires[i], treads, tread);
                Part("Alloy wheel face", tires[i], rim, metal, new Vector3(side * .071f, 0, 0));
                Part("Axle cap", tires[i], hub, metal, new Vector3(side * .096f, 0, 0));
                for (int spoke = 0; spoke < 5; spoke++)
                {
                    float angle = spoke * Mathf.PI * 2 / 5;
                    Block("Inset wheel spoke", tires[i], new Vector3(side * .088f, .068f * Mathf.Cos(angle), .068f * Mathf.Sin(angle)),
                        new Vector3(.012f, .089f, .023f), plastic, Quaternion.Euler(spoke * 72, 0, 0));
                }
                Part("Shock absorber", Root, shock, paint, new Vector3(side * .335f, .258f, z),
                    Quaternion.Euler(0, 0, 76 * side));
            }
        }

        private static Mesh Hull(Vector4[] sections)
        {
            var vertices = new List<Vector3>();
            var indices = new List<int>();
            foreach (Vector4 section in sections)
            {
                float z = section.x, w = section.y, low = section.z, high = section.w;
                vertices.AddRange(new[]
                {
                    new Vector3(-w + .04f, low, z), new Vector3(-w, low + .043f, z),
                    new Vector3(-w, high - .07f, z), new Vector3(-w + .052f, high, z),
                    new Vector3(w - .052f, high, z), new Vector3(w, high - .07f, z),
                    new Vector3(w, low + .043f, z), new Vector3(w - .04f, low, z)
                });
            }
            for (int ring = 0; ring < sections.Length - 1; ring++)
            {
                for (int side = 0; side < 8; side++)
                {
                    int a = ring * 8 + side;
                    int b = ring * 8 + (side + 1) % 8;
                    indices.AddRange(new[] { a, a + 8, b, b, a + 8, b + 8 });
                }
            }
            int last = (sections.Length - 1) * 8;
            for (int i = 1; i < 7; i++)
                indices.AddRange(new[] { 0, i, i + 1, last, last + i + 1, last + i });
            return MeshFrom("Beveled shell", vertices.ToArray(), indices.ToArray());
        }

        private static Mesh Tire() => RadialProfile("Rubber tire", 28,
            new[] { -.086f, -.086f, -.074f, -.054f, .054f, .074f, .086f, .086f },
            new[] { .004f, .149f, .184f, TireRadius, TireRadius, .184f, .149f, .004f });

        private static Mesh AxleCylinder(float radius, float width, int sides) => RadialProfile("Axle cylinder", sides,
            new[] { -width / 2, -width / 2, width / 2, width / 2 }, new[] { .001f, radius, radius, .001f });

        private static Mesh RadialProfile(string name, int sides, float[] axial, float[] radii)
        {
            var vertices = new Vector3[sides * axial.Length];
            var triangles = new List<int>();
            for (int ring = 0; ring < axial.Length; ring++)
            {
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2 / sides;
                    int vertex = ring * sides + i;
                    vertices[vertex] = new Vector3(axial[ring], Mathf.Cos(a) * radii[ring], Mathf.Sin(a) * radii[ring]);
                    if (ring == axial.Length - 1)
                        continue;
                    int next = ring * sides + (i + 1) % sides;
                    triangles.AddRange(new[] { vertex, next, vertex + sides, next, next + sides, vertex + sides });
                }
            }
            return MeshFrom(name, vertices, triangles.ToArray());
        }

        private static Mesh Tread()
        {
            var blocks = new CombineInstance[28];
            for (int i = 0; i < blocks.Length; i++)
            {
                Quaternion aroundAxle = Quaternion.Euler(i * 360f / blocks.Length, 0, 0);
                Vector3 position = aroundAxle * new Vector3(i % 2 == 0 ? -.025f : .025f, TireRadius - .001f, 0);
                Quaternion tilt = aroundAxle * Quaternion.Euler(0, i % 2 == 0 ? -16 : 16, 0);
                blocks[i] = new CombineInstance
                {
                    mesh = Cube(),
                    transform = Matrix4x4.TRS(position, tilt, new Vector3(.082f, .011f, .016f))
                };
            }
            var mesh = new Mesh { name = "Staggered tread blocks" };
            mesh.CombineMeshes(blocks);
            return mesh;
        }

        private static Mesh Cube()
        {
            if (cube != null)
                return cube;
            cube = RailwayTown.SceneryCube(Vector3.one, Vector2.one);
            return cube;
        }

        private static Mesh Faces(params Vector3[] vertices)
        {
            var triangles = new List<int>();
            for (int i = 0; i < vertices.Length; i += 4)
                triangles.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
            return MeshFrom("Scale model detail", vertices, triangles.ToArray());
        }

        private static Mesh MeshFrom(string name, Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Material Finish(Shader shader, Color color, float smoothness, float metallic = 0)
        {
            var material = new Material(shader) { color = color };
            material.SetFloat("_Glossiness", smoothness);
            material.SetFloat("_Metallic", metallic);
            return material;
        }

        private static void Block(string name, Transform parent, Vector3 position, Vector3 size, Material material,
            Quaternion? rotation = null)
        {
            Transform part = Part(name, parent, Cube(), material, position);
            part.localScale = size;
            part.localRotation = rotation ?? Quaternion.identity;
        }

        private static Transform Part(string name, Transform parent, Mesh mesh, Material material,
            Vector3 position = default, Quaternion? rotation = null)
        {
            Transform part = Group(name, parent);
            part.localPosition = position;
            part.localRotation = rotation ?? Quaternion.identity;
            part.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            return part;
        }

        private static Transform Group(string name, Transform parent)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }
    }
}

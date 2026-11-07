using UnityEngine;

namespace GarageGauntlet
{
    internal sealed class ScaleRcCar
    {
        private readonly Transform body;
        private readonly Transform[] hubs = new Transform[4];
        private readonly Transform[] tires = new Transform[4];
        private readonly float tireRadius;
        private float roll, previousSpeed, steerAngle, lean, pitch;
        public Transform Root { get; }

        public ScaleRcCar(Transform parent, Shader shader)
        {
            Root = Group("Die-cast RC car", parent);
            Transform rig = Group("Die-cast assembly", Root);
            body = Group("Painted shell and glass", rig);
            Material finish = Resources.Load<Material>("realism/diecast-finish");
            if (finish == null)
                throw new System.InvalidOperationException("Missing die-cast car material.");
            var glass = new Material(shader) { color = new Color(.13f, .19f, .22f) };
            glass.SetFloat("_Glossiness", .91f);
            glass.SetFloat("_Metallic", .18f);

            Transform shell = Part("shell", body, finish);
            Part("glass", body, glass);
            Bounds combined = BoundsOf(shell);
            string[] wheelNames = { "front-left", "front-right", "rear-left", "rear-right" };
            float modelRadius = 0;
            for (int i = 0; i < wheelNames.Length; i++)
            {
                Transform wheel = Part(wheelNames[i], rig, finish);
                Bounds bounds = BoundsOf(wheel);
                combined.Encapsulate(bounds);
                hubs[i] = Group(wheelNames[i] + " steering", rig);
                hubs[i].localPosition = rig.InverseTransformPoint(bounds.center);
                tires[i] = Group("Rolling wheel", hubs[i]);
                wheel.SetParent(tires[i], true);
                modelRadius += bounds.extents.y / wheelNames.Length;
            }

            float scale = 1.67f / combined.size.z;
            Vector3 origin = rig.InverseTransformPoint(combined.center);
            float bottom = rig.InverseTransformPoint(combined.min).y;
            rig.localScale = Vector3.one * scale;
            rig.localPosition = new Vector3(-origin.x * scale, -.075f - bottom * scale, -origin.z * scale);
            tireRadius = modelRadius * scale;
        }

        public void Animate(float speed, float steer, float dt)
        {
            roll = Mathf.Repeat(roll + speed * dt / tireRadius * Mathf.Rad2Deg, 360);
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

        private static Transform Part(string name, Transform parent, Material material)
        {
            GameObject prefab = Resources.Load<GameObject>("realism/diecast-" + name);
            if (prefab == null)
                throw new System.InvalidOperationException("Missing die-cast car part " + name);
            Transform model = Object.Instantiate(prefab, parent, false).transform;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = material;
            return model;
        }

        private static Bounds BoundsOf(Transform model)
        {
            Renderer[] parts = model.GetComponentsInChildren<Renderer>();
            Bounds bounds = parts[0].bounds;
            for (int i = 1; i < parts.Length; i++)
                bounds.Encapsulate(parts[i].bounds);
            return bounds;
        }

        private static Transform Group(string name, Transform parent)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }
    }
}

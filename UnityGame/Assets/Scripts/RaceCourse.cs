using System.Collections.Generic;
using UnityEngine;

namespace GarageGauntlet
{
    internal sealed class RaceCourse
    {
        private readonly List<Vector3> points = new List<Vector3>();
        private readonly List<float> distances = new List<float> { 0 };

        public const float HalfWidth = 2.15f;
        public const int LaneCount = 4;
        public const float BoardHalfWidth = 28.5f;
        public const float BoardHalfDepth = 21.5f;
        public static float SlotOffset(int lane) => (lane - 1.5f) * 1.01f;
        public float Length => distances[points.Count];
        public int Count => points.Count;
        public IReadOnlyList<Vector3> Crossings { get; }

        public RaceCourse()
        {
            Vector3[] corners =
            {
                Point(0, -11), Point(6, -11), Point(12, -11), Point(17, -8),
                Point(20, -3), Point(20, 3), Point(23, 9), Point(21, 15),
                Point(15, 16), Point(9, 13), Point(4, 8), Point(-1, 8),
                Point(-6, 12), Point(-13, 16), Point(-20, 14), Point(-23, 9),
                Point(-22, 3), Point(-22, -3), Point(-22, -9), Point(-18, -15),
                Point(-11, -16), Point(-6, -12)
            };
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 a = corners[(i + corners.Length - 1) % corners.Length];
                Vector3 b = corners[i];
                Vector3 c = corners[(i + 1) % corners.Length];
                Vector3 d = corners[(i + 2) % corners.Length];
                for (int step = 0; step < 12; step++)
                {
                    float t = step / 12f;
                    points.Add(.5f * (2 * b + (c - a) * t +
                        (2 * a - 5 * b + 4 * c - d) * t * t +
                        (3 * b - a - 3 * c + d) * t * t * t));
                }
            }

            for (int i = 0; i < Count; i++)
                distances.Add(distances[i] + Vector3.Distance(At(i), At(i + 1)));

            var crossings = new List<Vector3>();
            for (int i = 0; i < Count; i++)
            {
                Vector3 start = At(i);
                Vector3 end = At(i + 1);
                if ((start.z < 0) == (end.z < 0))
                    continue;
                float fraction = -start.z / (end.z - start.z);
                crossings.Add(Vector3.Lerp(start, end, fraction));
            }
            Crossings = crossings;
        }

        public Vector3 At(int index) => points[(index + Count) % Count];
        public Vector3 Tangent(int index) => (At(index + 1) - At(index - 1)).normalized;
        public static Vector3 Point(float x, float z) => new Vector3(x, 0, z);
        public float Delta(float from, float to) => Mathf.Repeat(to - from + Length / 2, Length) - Length / 2;

        public Vector3 Lane(float distance, float offset, out Vector3 forward)
        {
            Vector3 center = Sample(distance, out forward);
            return center + Vector3.Cross(Vector3.up, forward) * offset;
        }

        public float Curvature(float distance)
        {
            const float span = 1.35f;
            Sample(distance - span, out Vector3 before);
            Sample(distance + span, out Vector3 after);
            return Vector3.SignedAngle(before, after, Vector3.up) * Mathf.Deg2Rad / (2 * span);
        }

        public Vector3 Sample(float distance, out Vector3 direction)
        {
            distance = Mathf.Repeat(distance, Length);
            int next = distances.BinarySearch(distance);
            int index = Mathf.Min(Count - 1, next >= 0 ? next : ~next - 1);
            float t = Mathf.InverseLerp(distances[index], distances[index + 1], distance);
            direction = Vector3.Lerp(Tangent(index), Tangent(index + 1), t).normalized;
            return Vector3.Lerp(At(index), At(index + 1), t);
        }

        public float Locate(Vector3 position, out float lateral)
        {
            position.y = 0;
            float nearest = float.PositiveInfinity;
            float distance = 0;
            for (int i = 0; i < Count; i++)
            {
                Vector3 a = At(i);
                Vector3 segment = At(i + 1) - a;
                float t = Mathf.Clamp01(Vector3.Dot(position - a, segment) / segment.sqrMagnitude);
                float gap = (position - (a + segment * t)).sqrMagnitude;
                if (gap >= nearest)
                    continue;
                nearest = gap;
                distance = Mathf.Lerp(distances[i], distances[i + 1], t);
            }

            lateral = Mathf.Sqrt(nearest);
            return distance;
        }
    }
}

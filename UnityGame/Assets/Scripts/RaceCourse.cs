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
        public const float BoardHalfWidth = 49;
        public const float BoardHalfDepth = 43;
        public const float BridgeHeight = 2.68f;
        public const float RampLength = 8.5f;
        public static readonly Vector3 BoardCenter = new Vector3(7, 0, 10);
        public static float SlotOffset(int lane) => (lane - 1.5f) * 1.01f;
        public float Length => distances[points.Count];
        public int Count => points.Count;
        public RailwayLoop Railway { get; }

        public enum Layout
        {
            MapleJunction, OrchardSprint, TimberRidge, DepotRun,
            SpringfieldIllinois, SpringfieldMissouri, SpringfieldMassachusetts, SpringfieldOregon
        }
        public Layout Route { get; }
        public bool IsSpringfield => Route >= Layout.SpringfieldIllinois;
        public const int LayoutCount = 8;

        public RaceCourse(Layout layout = Layout.MapleJunction) : this(RoadCorners(layout))
        {
            Route = layout;
            Railway = new RailwayLoop(this);
        }

        private static Vector3[] RoadCorners(Layout layout)
        {
            if (layout >= Layout.SpringfieldIllinois)
                return SpringfieldPlan.Road(layout);

            // Share Main Street and the southern flyover. Each layout connects them via a different route.
            var corners = new List<Vector3>
            {
                Point(0, -22), Point(14, -22), Point(25, -23), Point(33, -19), Point(36, -12), Point(35, 2)
            };
            switch (layout)
            {
                case Layout.OrchardSprint:
                    corners.AddRange(new[]
                    {
                        Point(40, 8), Point(45, 16), Point(44, 23), Point(39, 28),
                        Point(30, 28), Point(20, 26.5f), Point(9, 26), Point(-4, 25.5f),
                        Point(-17, 25), Point(-26, 25), Point(-34, 20), Point(-37, 12), Point(-36, 2)
                    });
                    break;
                case Layout.TimberRidge:
                    corners.AddRange(new[]
                    {
                        Point(36, 8), Point(32.5f, 13), Point(31, 18), Point(32, 23), Point(27, 27),
                        Point(18, 27.5f), Point(11, 23.5f), Point(2, 21), Point(-9, 22),
                        Point(-19, 26), Point(-26, 27), Point(-35, 23), Point(-38, 16), Point(-38, 9),
                        Point(-34, 4), Point(-35, 0)
                    });
                    break;
                case Layout.DepotRun:
                    corners.AddRange(new[]
                    {
                        Point(33.5f, 8), Point(34, 14), Point(39, 20), Point(38, 25), Point(31, 27),
                        Point(21, 26.5f), Point(12, 26), Point(3, 25), Point(-6, 24), Point(-14, 24),
                        Point(-22, 21), Point(-28, 19), Point(-35, 20), Point(-38, 15),
                        Point(-37, 8), Point(-33.5f, 2)
                    });
                    break;
                default:
                    corners.AddRange(new[]
                    {
                        Point(35.5f, 9), Point(34.5f, 16), Point(31, 22.5f), Point(24.5f, 26.5f),
                        Point(17, 27.5f), Point(10, 27), Point(4, 24), Point(-3, 21.5f),
                        Point(-12, 21.7f), Point(-19, 23.4f), Point(-26, 25), Point(-34, 20),
                        Point(-37, 12), Point(-36, 2)
                    });
                    break;
            }
            corners.AddRange(new[]
            {
                Point(-38.2f, -5), Point(-36.5f, -13), Point(-32, -20),
                Point(-22, -25), Point(-13, -23), Point(-7, -22)
            });
            return corners.ToArray();
        }

        internal RaceCourse(Vector3[] corners)
        {
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

        }

        public Vector3 At(int index) => points[(index + Count) % Count];
        public float DistanceAt(int index) => distances[index];
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
            before.y = after.y = 0;
            return Vector3.SignedAngle(before, after, Vector3.up) * Mathf.Deg2Rad / (2 * span);
        }

        public Vector3 Sample(float distance, out Vector3 direction)
        {
            distance = Mathf.Repeat(distance, Length);
            int next = distances.BinarySearch(distance);
            int index = Mathf.Min(Count - 1, next >= 0 ? next : ~next - 1);
            float t = Mathf.InverseLerp(distances[index], distances[index + 1], distance);
            direction = Vector3.Lerp(Tangent(index), Tangent(index + 1), t).normalized;
            Vector3 point = Vector3.Lerp(At(index), At(index + 1), t);
            point.y = ElevationAt(distance, point);
            Vector3 slopeProbe = direction * .15f;
            direction.y = (ElevationAt(distance + .15f, point + slopeProbe) - ElevationAt(distance - .15f, point - slopeProbe)) / .30f;
            direction.Normalize();
            return point;
        }

        public float ElevationAt(float distance, Vector3 position)
        {
            if (Railway == null)
                return 0;
            float rampDistance = Mathf.Max(Railway.BridgeStart - distance, distance - Railway.BridgeEnd);
            return Mathf.Max(GroundHeight(position.x, position.z),
                Mathf.SmoothStep(BridgeHeight, 0, Mathf.Clamp01(rampDistance / RampLength)));
        }

        public float GroundHeight(float x, float z) => IsSpringfield
            ? SpringfieldPlan.GroundHeight(Route, x, z) : MapleJunctionPlan.GroundHeight(x, z);

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

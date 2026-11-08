using System.Collections.Generic;
using UnityEngine;

namespace GarageGauntlet
{
    internal sealed class RailwayLoop
    {
        internal readonly struct Crossing
        {
            public readonly Vector3 Position, RoadDirection, RailDirection;
            public readonly float RoadDistance, RailDistance;

            public Crossing(Vector3 position, Vector3 road, Vector3 rail, float roadDistance, float railDistance)
            {
                Position = position;
                RoadDirection = road.normalized;
                RailDirection = rail.normalized;
                RoadDistance = roadDistance;
                RailDistance = railDistance;
            }
        }

        public RaceCourse Path { get; } = new RaceCourse(new[]
        {
            RaceCourse.Point(-11, 0), RaceCourse.Point(5, 0), RaceCourse.Point(26, 0), RaceCourse.Point(43, 0),
            RaceCourse.Point(50, 5), RaceCourse.Point(52, 14), RaceCourse.Point(52, 31), RaceCourse.Point(47, 39),
            RaceCourse.Point(37, 43), RaceCourse.Point(16, 43), RaceCourse.Point(-6, 43), RaceCourse.Point(-19, 41),
            RaceCourse.Point(-25, 35), RaceCourse.Point(-26, 24), RaceCourse.Point(-26, 7), RaceCourse.Point(-26, -11),
            RaceCourse.Point(-26, -23), RaceCourse.Point(-24, -28.7f), RaceCourse.Point(-20.5f, -30.5f),
            RaceCourse.Point(-17, -28.7f), RaceCourse.Point(-15, -24), RaceCourse.Point(-15, -16),
            RaceCourse.Point(-15, -6), RaceCourse.Point(-13, -1)
        });

        public IReadOnlyList<Crossing> Crossings { get; }
        public IReadOnlyList<Crossing> OverpassTracks { get; }
        public float BridgeStart { get; }
        public float BridgeEnd { get; }
        public Vector3 EnginePosition { get; private set; }
        public Vector3 EngineDirection { get; private set; }
        public Vector3 WagonPosition { get; private set; }
        public Vector3 WagonDirection { get; private set; }
        private float distance;

        public RailwayLoop(RaceCourse road)
        {
            var crossings = new List<Crossing>();
            var overpass = new List<Crossing>();
            for (int i = 0; i < road.Count; i++)
            for (int j = 0; j < Path.Count; j++)
            {
                Vector3 a = road.At(i), b = Path.At(j);
                Vector3 slot = road.At(i + 1) - a, rail = Path.At(j + 1) - b;
                float denominator = Cross(slot, rail);
                if (Mathf.Abs(denominator) < .00001f)
                    continue;
                float t = Cross(b - a, rail) / denominator;
                float u = Cross(b - a, slot) / denominator;
                if (t >= 0 && t < 1 && u >= 0 && u < 1)
                {
                    Vector3 point = a + slot * t;
                    var crossing = new Crossing(point, slot, rail,
                        Mathf.Lerp(road.DistanceAt(i), road.DistanceAt(i + 1), t),
                        Mathf.Lerp(Path.DistanceAt(j), Path.DistanceAt(j + 1), u));
                    if (point.z < -20)
                        overpass.Add(crossing);
                    else
                        crossings.Add(crossing);
                }
            }
            if (crossings.Count != 2 || overpass.Count != 2)
                throw new System.InvalidOperationException("Switchyard requires two grade crossings and two rails under one bridge.");
            Crossings = crossings;
            OverpassTracks = overpass;
            BridgeStart = overpass[0].RoadDistance - 1.8f;
            BridgeEnd = overpass[overpass.Count - 1].RoadDistance + 1.8f;
            SetTrainDistance(0);
        }

        private static float Cross(Vector3 a, Vector3 b) => a.x * b.z - a.z * b.x;

        public void SetTrainDistance(float along)
        {
            distance = Mathf.Repeat(along, Path.Length);
            EnginePosition = Path.Sample(distance + .45f, out Vector3 engine);
            EngineDirection = engine;
            WagonPosition = Path.Sample(distance - 2.91f, out Vector3 wagon);
            WagonDirection = wagon;
        }

        public bool SignalClosed(Crossing crossing)
        {
            float gap = Path.Delta(distance, crossing.RailDistance);
            return gap >= -5.4f && gap <= 9.2f;
        }

        public bool Hits(Vector3 position) =>
            HitsCar(position, EnginePosition, EngineDirection, 1.93f, .70f) ||
            HitsCar(position, WagonPosition, WagonDirection, 1.74f, .74f);

        private static bool HitsCar(Vector3 position, Vector3 center, Vector3 direction, float halfLength, float halfWidth)
        {
            Vector3 offset = position - center;
            return Mathf.Abs(offset.y) < 1.5f && Mathf.Abs(Vector3.Dot(offset, direction)) < halfLength
                && Mathf.Abs(Cross(offset, direction)) < halfWidth;
        }
    }
}

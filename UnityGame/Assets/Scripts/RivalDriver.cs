using UnityEngine;

namespace GarageGauntlet
{
    internal sealed class RivalDriver
    {
        private readonly RaceCourse course;
        private readonly float basePace, homeLane, gridDistance, phase;
        private float speed, lane;
        public readonly string Name;
        public readonly Color Color;
        public readonly Transform Car;
        public float Distance { get; private set; }
        public bool Finished => Distance >= course.Length * RaceProgress.TotalLaps;

        public RivalDriver(RaceCourse course, RailwayTown town, string name, Color color,
            float basePace, float homeLane, float gridDistance, int index)
        {
            this.course = course;
            this.basePace = basePace;
            this.homeLane = homeLane;
            this.gridDistance = gridDistance;
            phase = index * 2.4f;
            Name = name;
            Color = color;
            Car = town.CreateRacer(name, color, index + 1);
            Reset();
        }

        public void Reset()
        {
            speed = 0;
            lane = homeLane;
            Distance = gridDistance;
            Place();
        }

        public void Step(float dt, float elapsed, float trainX, Transform player)
        {
            if (Finished)
                return;
            Vector3 center = course.Sample(Distance, out Vector3 forward);
            Vector3 lookAhead = course.Sample(Distance + 3.6f, out Vector3 nextForward);
            float bend = Mathf.Abs(Vector3.SignedAngle(forward, nextForward, Vector3.up));
            float pace = basePace + .75f * Mathf.Sin(elapsed * .92f + phase) - Mathf.Min(bend * .046f, 2.7f);
            if (center.z * lookAhead.z < 0 && RailwayTown.SignalClosed(trainX, lookAhead.x))
                pace = Mathf.Clamp(Mathf.Abs(center.z) * 2.1f - 1.5f, 0, pace);

            float targetLane = homeLane + .30f * Mathf.Sin(Distance * .14f + phase);
            Vector3 difference = player.position - Car.position;
            float ahead = Vector3.Dot(forward, difference);
            if (ahead > 0 && ahead < 3 && difference.sqrMagnitude < 9)
            {
                float side = Vector3.Dot(Vector3.Cross(Vector3.up, forward), difference);
                targetLane = side >= 0 ? -1.02f : 1.02f;
                if (Mathf.Abs(side) < .75f)
                    pace = Mathf.Min(pace, 5.8f);
            }

            lane = Mathf.MoveTowards(lane, targetLane, dt * 1.8f);
            speed = Mathf.MoveTowards(speed, pace, dt * (pace < speed ? 26 : 9));
            Distance = Mathf.Min(course.Length * RaceProgress.TotalLaps, Distance + speed * dt);
            Place();
        }

        private void Place()
        {
            Vector3 position = course.Lane(Distance, lane, out Vector3 forward);
            Car.SetPositionAndRotation(position + Vector3.up * .11f, Quaternion.LookRotation(forward));
        }
    }
}

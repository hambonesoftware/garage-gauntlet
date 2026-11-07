using UnityEngine;

namespace GarageGauntlet
{
    internal enum SlotIncident { None, Deslot, Train }

    internal sealed class SlotMotor
    {
        public const float TopSpeed = 14.6f;
        private readonly RaceCourse course;
        private readonly float lane;
        private float strain, recovery, skidDirection;
        private SlotIncident incident;
        public float Distance { get; private set; }
        public float Speed { get; private set; }
        public float Trigger { get; private set; }
        public bool Recovering => recovery > 0 || incident != SlotIncident.None;
        public float SafeSpeed => LimitAt(Distance);

        public SlotMotor(RaceCourse course, int lane)
        {
            this.course = course;
            this.lane = RaceCourse.SlotOffset(lane);
        }

        public void Reset()
        {
            Distance = Speed = Trigger = strain = recovery = skidDirection = 0;
            incident = SlotIncident.None;
        }

        public float LimitAt(float distance)
        {
            float bend = course.Curvature(distance);
            float laneRadius = Mathf.Clamp(1 - bend * lane, .45f, 1.65f);
            return Mathf.Clamp(Mathf.Sqrt(27f * laneRadius / Mathf.Max(.01f, Mathf.Abs(bend))), 5.1f, TopSpeed);
        }

        public float BrakeTarget()
        {
            float target = LimitAt(Distance);
            for (int sample = 1; sample <= 5; sample++)
            {
                float ahead = sample * .9f;
                float corner = LimitAt(Distance + ahead);
                target = Mathf.Min(target, Mathf.Sqrt(corner * corner + 2 * 13 * ahead));
            }
            return target;
        }

        public SlotIncident Step(float throttle, float dt, float trainX)
        {
            Trigger = Mathf.Clamp01(throttle);
            if (incident != SlotIncident.None)
            {
                recovery = Mathf.Max(0, recovery - dt);
                if (recovery == 0 && !RailwayTown.TrainHits(trainX, course.Lane(Distance, lane, out _)))
                    incident = SlotIncident.None;
                return SlotIncident.None;
            }

            float target = Trigger * TopSpeed;
            Speed = Mathf.MoveTowards(Speed, target, (target < Speed ? 19 : 15) * dt);
            float bend = course.Curvature(Distance);
            float laneRadius = Mathf.Clamp(1 - bend * lane, .45f, 1.65f);
            float next = Mathf.Min(course.Length * RaceProgress.TotalLaps, Distance + Speed * dt / laneRadius);
            if (RailwayTown.TrainHits(trainX, course.Lane(next, lane, out _)))
                return Stop(SlotIncident.Train, 1.15f, Mathf.Sign(bend));

            Distance = next;
            float overspeed = Speed / LimitAt(Distance) - 1.07f;
            strain = overspeed > 0 ? strain + overspeed * dt : Mathf.Max(0, strain - dt * .8f);
            return strain > .065f ? Stop(SlotIncident.Deslot, 1.0f, -Mathf.Sign(bend)) : SlotIncident.None;
        }

        private SlotIncident Stop(SlotIncident reason, float delay, float direction)
        {
            incident = reason;
            recovery = delay;
            skidDirection = direction;
            Speed = strain = 0;
            return reason;
        }

        public void Draw(ScaleRcCar model, float dt)
        {
            Vector3 position = course.Lane(Distance, lane, out Vector3 forward);
            float skid = incident == SlotIncident.Deslot ? Mathf.Sin(Mathf.PI * Mathf.Clamp01(1 - recovery)) : 0;
            position += Vector3.Cross(Vector3.up, forward) * (skidDirection * skid * .47f);
            Quaternion yaw = Quaternion.AngleAxis(skidDirection * skid * 26, Vector3.up);
            model.Root.SetPositionAndRotation(position + Vector3.up * .11f, yaw * Quaternion.LookRotation(forward));
            model.Animate(Speed, 0, dt);
        }
    }
}

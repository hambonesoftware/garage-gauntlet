using UnityEngine;

namespace GarageGauntlet
{
    internal enum SlotIncident { None, Deslot, Train }

    internal sealed class SlotMotor
    {
        public const float TopSpeed = 14.6f;
        internal const float GuideOffset = .56f;
        private const float TailOffset = .68f;
        private readonly RaceCourse course;
        private readonly float lane;
        private float strain, recovery, skidDirection, slideVelocity;
        private SlotIncident incident;
        public float Distance { get; private set; }
        public float Speed { get; private set; }
        public float Trigger { get; private set; }
        public float SlideAngle { get; private set; }
        public bool Recovering => recovery > 0 || incident != SlotIncident.None;
        public float SafeSpeed => LimitAt(Distance);

        public SlotMotor(RaceCourse course, int lane)
        {
            this.course = course;
            this.lane = RaceCourse.SlotOffset(lane);
        }

        public void Reset()
        {
            Distance = Speed = Trigger = strain = recovery = skidDirection = SlideAngle = slideVelocity = 0;
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
                if (recovery == 0 && !TouchesTrain(Distance, 0, trainX))
                {
                    incident = SlotIncident.None;
                    SlideAngle = slideVelocity = 0;
                }
                return SlotIncident.None;
            }

            float target = Trigger * TopSpeed;
            Speed = Mathf.MoveTowards(Speed, target, (target < Speed ? 19 : 15) * dt);
            float bend = course.Curvature(Distance);
            float laneRadius = Mathf.Clamp(1 - bend * lane, .45f, 1.65f);
            float next = Mathf.Min(course.Length * RaceProgress.TotalLaps, Distance + Speed * dt / laneRadius);
            if (TouchesTrain(next, SlideAngle, trainX))
                return Stop(SlotIncident.Train, 1.15f, Mathf.Sign(bend));

            Distance = next;
            float speedRatio = Speed / SafeSpeed;
            float nextBend = course.Curvature(next);
            float load = speedRatio * speedRatio + .12f * Trigger;
            float curveWeight = Mathf.Clamp01(Mathf.Abs(nextBend) / .09f);
            float targetAngle = Mathf.Sign(nextBend) * curveWeight * 39 * Mathf.Clamp01((load - .66f) / .89f);
            slideVelocity += (targetAngle - SlideAngle) * 96 * dt;
            slideVelocity *= Mathf.Exp(-13 * dt);
            SlideAngle += slideVelocity * dt;

            float overspeed = speedRatio - 1.07f;
            strain = overspeed > 0 ? strain + overspeed * dt : Mathf.Max(0, strain - dt * .8f);
            float slide = Mathf.Abs(SlideAngle);
            bool lostGuide = slide > 34 || (slide > 19 && strain > .065f);
            return lostGuide ? Stop(SlotIncident.Deslot, 1f, -Mathf.Sign(SlideAngle)) : SlotIncident.None;
        }

        private SlotIncident Stop(SlotIncident reason, float delay, float direction)
        {
            incident = reason;
            recovery = delay;
            skidDirection = direction;
            Speed = strain = 0;
            slideVelocity = 0;
            return reason;
        }

        private bool TouchesTrain(float distance, float angle, float trainX)
        {
            if (trainX < -RaceCourse.BoardHalfWidth - 2.65f || trainX > RaceCourse.BoardHalfWidth + 4.65f)
                return false;
            Pose(distance, angle, out Vector3 guide, out Vector3 center, out Vector3 heading);
            return RailwayTown.TrainHits(trainX, guide) || RailwayTown.TrainHits(trainX, center)
                || RailwayTown.TrainHits(trainX, center - heading * TailOffset);
        }

        internal void Pose(float distance, float angle, out Vector3 guide, out Vector3 center, out Vector3 heading)
        {
            guide = course.Lane(distance, lane, out Vector3 tangent);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 right = Vector3.Cross(Vector3.up, tangent);
            heading = tangent * Mathf.Cos(radians) + right * Mathf.Sin(radians);
            center = guide - heading * GuideOffset;
        }

        public void Draw(ScaleRcCar model, float dt)
        {
            float settle = incident == SlotIncident.None ? 0 : Mathf.Clamp01(1 - recovery);
            float skid = incident == SlotIncident.Deslot ? Mathf.Sin(Mathf.PI * settle) : 0;
            float angle = SlideAngle * (1 - Mathf.SmoothStep(0, 1, settle)) - skidDirection * skid * 19;
            Pose(Distance, angle, out _, out Vector3 position, out Vector3 heading);
            position += Vector3.Cross(Vector3.up, heading) * (skidDirection * skid * .42f);
            model.Root.SetPositionAndRotation(position + Vector3.up * .11f, Quaternion.LookRotation(heading));
            model.Animate(Speed, 0, dt);
        }
    }
}

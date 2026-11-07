using UnityEngine;

namespace GarageGauntlet
{
    internal sealed class RivalDriver
    {
        private readonly RaceCourse course;
        private readonly ScaleRcCar model;
        private readonly SlotMotor motor;
        private readonly int lane;
        private readonly float pace, phase;
        public Color Color => RailwayTown.SlotColor(lane);
        public Transform Car => model.Root;
        public float Distance => motor.Distance;
        public bool Finished => Distance >= course.Length * RaceProgress.TotalLaps;

        public RivalDriver(RaceCourse course, RailwayTown town, string name, int lane, float pace)
        {
            this.course = course;
            this.lane = lane;
            this.pace = pace;
            phase = lane * 2.4f;
            motor = new SlotMotor(course, lane);
            model = town.CreateRacer(name);
            Reset();
        }

        public void Reset()
        {
            motor.Reset();
            model.ResetMotion();
            motor.Draw(model, 0);
        }

        public void Step(float dt, float elapsed, float trainX)
        {
            if (Finished)
                return;
            float variation = Mathf.Sin(elapsed * 1.35f + phase) * .08f;
            float speed = motor.BrakeTarget() * (pace + variation);
            float offset = RaceCourse.SlotOffset(lane);
            Vector3 current = course.Lane(Distance, offset, out _);
            Vector3 ahead = course.Lane(Distance + 4.8f, offset, out _);
            if (current.z * ahead.z <= 0 && RailwayTown.SignalClosed(trainX, ahead.x))
            {
                float room = Mathf.Max(0, Mathf.Abs(current.z) - 1.26f);
                speed = Mathf.Min(speed, Mathf.Sqrt(17f * room));
            }

            motor.Step(speed / SlotMotor.TopSpeed, dt, trainX);
        }

        public void Draw(float dt) => motor.Draw(model, dt);
    }
}

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

        public void Step(float dt, float elapsed)
        {
            if (Finished)
                return;
            float variation = Mathf.Sin(elapsed * 1.35f + phase) * .08f;
            float speed = motor.BrakeTarget() * (pace + variation);
            foreach (RailwayLoop.Crossing crossing in course.Railway.Crossings)
            {
                float room = course.Delta(Distance, crossing.RoadDistance);
                if (room >= 0 && room < 13 && course.Railway.SignalClosed(crossing))
                    speed = Mathf.Min(speed, Mathf.Sqrt(17f * Mathf.Max(0, room - 1.65f)));
            }

            motor.Step(speed / SlotMotor.TopSpeed, dt);
        }

        public void Draw(float dt) => motor.Draw(model, dt);
    }
}

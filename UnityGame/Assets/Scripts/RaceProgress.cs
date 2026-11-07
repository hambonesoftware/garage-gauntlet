using UnityEngine;

namespace GarageGauntlet
{
    internal sealed class RaceProgress
    {
        public const int TotalLaps = 3;
        private const int Sectors = 4;
        private readonly RaceCourse course;
        private int nextSector = 1;
        public int CompletedLaps { get; private set; }
        public int Lap => Mathf.Min(TotalLaps, CompletedLaps + 1);
        public bool Finished => CompletedLaps == TotalLaps;
        public float LastGate => course.Length * (nextSector - 1) / Sectors;

        public RaceProgress(RaceCourse course) => this.course = course;

        public void Reset()
        {
            nextSector = 1;
            CompletedLaps = 0;
        }

        public float RankDistance(float projected)
        {
            if (Finished)
                return course.Length * TotalLaps;
            float sectorProgress = Mathf.Clamp(course.Delta(LastGate, projected), 0, course.Length / Sectors);
            return CompletedLaps * course.Length + LastGate + sectorProgress;
        }

        public bool Advance(float previous, float projected, float lateral)
        {
            float travel = course.Delta(previous, projected);
            if (Finished || travel <= 0 || travel > 2 || lateral > RaceCourse.HalfWidth + .2f)
                return false;

            float gate = course.Length * nextSector / Sectors;
            float remaining = Mathf.Repeat(gate - previous, course.Length);
            if (remaining > travel + .001f)
                return false;

            if (nextSector < Sectors)
            {
                nextSector++;
                return false;
            }

            CompletedLaps++;
            nextSector = 1;
            return true;
        }
    }
}

namespace GarageGauntlet
{
    internal sealed class RaceProgress
    {
        public const int TotalLaps = 3;
        private readonly float lapLength;
        public int CompletedLaps { get; private set; }
        public int Lap => Finished ? TotalLaps : CompletedLaps + 1;
        public bool Finished => CompletedLaps == TotalLaps;

        public RaceProgress(RaceCourse course) => lapLength = course.Length;

        public void Reset() => CompletedLaps = 0;

        public bool Advance(float distance)
        {
            if (Finished || distance < lapLength * (CompletedLaps + 1))
                return false;
            CompletedLaps++;
            return true;
        }
    }
}

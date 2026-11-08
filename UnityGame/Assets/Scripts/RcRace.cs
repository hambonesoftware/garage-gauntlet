using UnityEngine;

namespace GarageGauntlet
{
    public sealed partial class RcRace : MonoBehaviour
    {
        private const string TrialBestKey = "garage-gauntlet-slot-trial-switchyard";
        private const string PrixBestKey = "garage-gauntlet-slot-prix-switchyard";
        private const float MaximumStep = .025f;
        private enum RaceState { Ready, Countdown, Racing, Paused, Finished }

        [SerializeField] private Shader paletteShader = null;
        private readonly RaceCourse course = new RaceCourse();
        private RaceState state;
        private RailwayTown town;
        private RaceProgress progress;
        private RivalDriver[] rivals;
        private SlotMotor motor;
        private RaceAudio sound;
        private Transform car;
        private Camera raceCamera;
        private bool grandPrix = true, record;
        private float countdown, elapsed, best, lapStarted, bestLap;
        private float trainX;
        private int finalPlace;
        private string notice;
        private float noticeUntil;

        private float RaceTime => elapsed;
        private string RecordKey => grandPrix ? PrixBestKey : TrialBestKey;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            progress = new RaceProgress(course);
            motor = new SlotMotor(course, 1);
            town = new RailwayTown(transform, course, paletteShader);
            car = town.Car;
            raceCamera = town.Camera;
            sound = new RaceAudio(gameObject);
            rivals = new[]
            {
                new RivalDriver(course, town, "Ember", 0, .89f),
                new RivalDriver(course, town, "Switch", 2, .84f),
                new RivalDriver(course, town, "Volt", 3, .94f)
            };
            ResetRace();
            town.UpdateTrain(-3.5f, 0, 1);
            UpdateCamera(true);
        }

        private void ResetRace()
        {
            ReleaseTrigger();
            progress.Reset();
            motor.Reset();
            town.PlayerModel.ResetMotion();
            motor.Draw(town.PlayerModel, 0);
            elapsed = lapStarted = bestLap = 0;
            best = PlayerPrefs.GetFloat(RecordKey);
            finalPlace = 1;
            record = false;
            notice = string.Empty;
            noticeUntil = 0;
            trainX = -RailwayTown.TrainEnd;
            town.UpdateTrain(trainX, 0, 1);
            foreach (RivalDriver rival in rivals)
            {
                rival.Reset();
                rival.Car.gameObject.SetActive(grandPrix);
            }
        }

        private void StartRace(bool competition)
        {
            grandPrix = competition;
            ResetRace();
            countdown = 3;
            state = RaceState.Countdown;
            sound.Begin();
            sound.Signal(RaceSound.Tick);
        }

        private void TogglePause()
        {
            if (state == RaceState.Racing || state == RaceState.Countdown)
            {
                pausedState = state;
                state = RaceState.Paused;
                ReleaseTrigger();
            }
            else if (state == RaceState.Paused)
                state = pausedState;
        }

        private RaceState pausedState;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                TogglePause();
            if (Input.GetKeyDown(KeyCode.M))
                sound.Toggle();
            if (Input.GetKeyDown(KeyCode.Return) && (state == RaceState.Ready || state == RaceState.Finished))
                StartRace(grandPrix);

            float dt = Time.deltaTime;
            float throttle = state == RaceState.Countdown || state == RaceState.Racing ? ReadTrigger() : 0;
            if (state == RaceState.Countdown)
            {
                int before = Mathf.CeilToInt(countdown);
                countdown -= dt;
                if (countdown <= 0)
                {
                    state = RaceState.Racing;
                    sound.Signal(RaceSound.Start);
                    Notify("GO!", .95f);
                }
                else if (Mathf.CeilToInt(countdown) != before)
                    sound.Signal(RaceSound.Tick);
            }

            if (state == RaceState.Racing)
            {
                while (dt > 0 && state == RaceState.Racing)
                {
                    float step = Mathf.Min(dt, MaximumStep);
                    Step(throttle, step);
                    dt -= step;
                }

                motor.Draw(town.PlayerModel, Time.deltaTime);
                if (grandPrix)
                    foreach (RivalDriver rival in rivals)
                        rival.Draw(Time.deltaTime);
            }
            sound.Update(motor.Speed, state == RaceState.Racing || state == RaceState.Countdown);
        }

        private void Step(float throttle, float dt)
        {
            elapsed += dt;
            trainX = -RailwayTown.TrainEnd + Mathf.Repeat(elapsed * 5.65f, RailwayTown.TrainEnd * 2);
            town.UpdateTrain(trainX, elapsed, dt);
            if (grandPrix)
                foreach (RivalDriver rival in rivals)
                    rival.Step(dt, elapsed, trainX);

            SlotIncident incident = motor.Step(throttle, dt, trainX);
            if (incident != SlotIncident.None)
            {
                sound.Signal(RaceSound.Hit);
                Notify(incident == SlotIncident.Train ? "TRAIN HIT · RE-SLOTTING" : "DESLOTTED · EASE OFF IN TURNS");
            }
            if (progress.Advance(motor.Distance))
                CompleteLap();
        }

        private void CompleteLap()
        {
            float lapTime = RaceTime - lapStarted;
            lapStarted = RaceTime;
            if (bestLap <= 0 || lapTime < bestLap)
                bestLap = lapTime;
            if (!progress.Finished)
            {
                Notify(progress.Lap == RaceProgress.TotalLaps ? "FINAL LAP" : "LAP " + progress.Lap);
                sound.Signal(RaceSound.Lap);
                return;
            }

            finalPlace = PlaceInRace();
            state = RaceState.Finished;
            ReleaseTrigger();
            sound.Signal(RaceSound.Finish);
            record = best <= 0 || RaceTime < best;
            if (record)
            {
                best = RaceTime;
                PlayerPrefs.SetFloat(RecordKey, best);
                PlayerPrefs.Save();
            }
        }

        private int PlaceInRace()
        {
            int place = 1;
            if (!grandPrix)
                return place;
            float playerDistance = motor.Distance;
            foreach (RivalDriver rival in rivals)
            {
                if (rival.Finished || rival.Distance > playerDistance)
                    place++;
            }
            return place;
        }

        private bool CrossingWarning()
        {
            foreach (Vector3 crossing in course.Crossings)
            {
                if ((car.position - crossing).sqrMagnitude < 73 && RailwayTown.SignalClosed(trainX, crossing.x))
                    return true;
            }
            return false;
        }

        private void Notify(string message, float seconds = 1.8f)
        {
            notice = message;
            noticeUntil = Time.unscaledTime + seconds;
        }

        private void LateUpdate() => UpdateCamera(false);

        private void UpdateCamera(bool snap)
        {
            float aspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            bool overview = state == RaceState.Ready;
            Vector3 target = overview ? Vector3.zero : car.position + car.forward * motor.Speed * .125f;
            float size = overview ? Mathf.Max(25.5f, 34f / aspect) : Mathf.Max(7.45f, 5.8f / aspect);
            Vector3 offset = new Vector3(5, 15, -12);
            float blend = snap ? 1 : 1 - Mathf.Exp(-6 * Time.deltaTime);
            Vector3 cameraTarget = target + offset * (overview ? 2.25f : 1);
            raceCamera.transform.position = Vector3.Lerp(raceCamera.transform.position, cameraTarget, blend);
            raceCamera.transform.rotation = Quaternion.LookRotation(-offset);
            raceCamera.orthographicSize = Mathf.Lerp(raceCamera.orthographicSize, size, blend);
        }
    }
}

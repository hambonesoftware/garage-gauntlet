using UnityEngine;

namespace GarageGauntlet
{
    public sealed partial class RcRace : MonoBehaviour
    {
        private const string TrialBestKey = "garage-gauntlet-slot-trial-maple-junction-sweeper";
        private const string PrixBestKey = "garage-gauntlet-slot-prix-maple-junction-sweeper";
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
            course.Railway.SetTrainDistance(0);
            town.UpdateTrain(course.Railway, 0, 1);
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
            if (state == RaceState.Ready)
            {
                course.Railway.SetTrainDistance(Time.unscaledTime * 5.2f);
                town.UpdateTrain(course.Railway, Time.unscaledTime, dt);
            }
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
            course.Railway.SetTrainDistance(elapsed * 5.8f);
            town.UpdateTrain(course.Railway, elapsed, dt);
            if (grandPrix)
                foreach (RivalDriver rival in rivals)
                    rival.Step(dt, elapsed);

            SlotIncident incident = motor.Step(throttle, dt);
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
            foreach (RailwayLoop.Crossing crossing in course.Railway.Crossings)
            {
                if ((car.position - crossing.Position).sqrMagnitude < 100 && course.Railway.SignalClosed(crossing))
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
            Vector3 target = car.position + car.forward * motor.Speed * .125f;
            float size = Mathf.Max(7.45f, 5.8f / aspect);
            Vector3 overhead = new Vector3(4, 21, -10) * 5;
            Vector3 offset = overview ? overhead : new Vector3(5, 9, -12);
            Quaternion rotation = Quaternion.LookRotation(-offset);
            if (overview)
                FrameBoard(rotation, aspect, out target, out size);
            if (state == RaceState.Countdown || (state == RaceState.Paused && pausedState == RaceState.Countdown))
            {
                Quaternion overheadRotation = Quaternion.LookRotation(-overhead);
                FrameBoard(overheadRotation, aspect, out Vector3 boardTarget, out float boardSize);
                float descent = Mathf.SmoothStep(0, 1, Mathf.Clamp01((3 - countdown) / 2.45f));
                raceCamera.transform.SetPositionAndRotation(Vector3.Lerp(boardTarget + overhead, target + offset, descent),
                    Quaternion.Slerp(overheadRotation, rotation, descent));
                raceCamera.orthographicSize = Mathf.Lerp(boardSize, size, descent);
                return;
            }
            float blend = snap ? 1 : 1 - Mathf.Exp(-6 * Time.deltaTime);
            raceCamera.transform.position = Vector3.Lerp(raceCamera.transform.position, target + offset, blend);
            raceCamera.transform.rotation = Quaternion.Slerp(raceCamera.transform.rotation, rotation, blend);
            raceCamera.orthographicSize = Mathf.Lerp(raceCamera.orthographicSize, size, blend);
        }

        private void FrameBoard(Quaternion rotation, float aspect, out Vector3 target, out float size)
        {
            Rect menu = GridMenuCard;
            float left = GridMenuOnSide ? menu.xMax / Width + .024f : .045f;
            float top = GridMenuOnSide ? .06f : menu.yMax / Height + .03f;
            const float right = .955f, bottom = .95f;
            Vector3 horizontal = rotation * Vector3.right;
            Vector3 vertical = rotation * Vector3.up;
            size = Mathf.Max(ProjectedExtent(horizontal) / (aspect * Mathf.Max(.15f, right - left)),
                ProjectedExtent(vertical) / Mathf.Max(.15f, bottom - top));
            target = RaceCourse.BoardCenter + Vector3.up * 1.4f
                - horizontal * ((left + right - 1) * size * aspect)
                + vertical * ((top + bottom - 1) * size);
        }

        private static float ProjectedExtent(Vector3 axis) =>
            (RaceCourse.BoardHalfWidth + 3) * Mathf.Abs(axis.x) + 3.2f * Mathf.Abs(axis.y)
            + (RaceCourse.BoardHalfDepth + 3) * Mathf.Abs(axis.z);
    }
}

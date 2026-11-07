using UnityEngine;

namespace GarageGauntlet
{
    public sealed partial class RcRace : MonoBehaviour
    {
        private const string TrialBestKey = "garage-gauntlet-railway-race-v1";
        private const string PrixBestKey = "garage-gauntlet-grand-prix-v1";
        private const float MaximumStep = .025f;
        private enum RaceState { Ready, Countdown, Racing, Paused, Finished }

        private sealed class ChargePad
        {
            public Transform Surface;
            public float ReadyAt;
        }

        [SerializeField] private Shader paletteShader = null;
        private readonly RaceCourse course = new RaceCourse();
        private RaceState state;
        private RailwayTown town;
        private RaceProgress progress;
        private RivalDriver[] rivals;
        private ChargePad[] pads;
        private RaceAudio sound;
        private Transform car;
        private Camera raceCamera;
        private bool grandPrix = true, boosting, record, touchSeen;
        private float countdown, elapsed, penalties, best, lapStarted, bestLap;
        private float speed, heading, distance, trainX, boost, recovery, steering;
        private int finalPlace;
        private Vector3 velocity;
        private string notice;
        private float noticeUntil;

        private float RaceTime => elapsed + penalties;
        private string RecordKey => grandPrix ? PrixBestKey : TrialBestKey;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            progress = new RaceProgress(course);
            town = new RailwayTown(transform, course, paletteShader);
            car = town.Car;
            raceCamera = town.Camera;
            sound = new RaceAudio(gameObject);
            rivals = new[]
            {
                new RivalDriver(course, town, "EMBER", new Color(1f, .47f, .32f), 9.6f, -.90f, -1.4f, 0),
                new RivalDriver(course, town, "SWITCH", new Color(1f, .78f, .35f), 8.9f, .92f, -2.3f, 1),
                new RivalDriver(course, town, "VOLT", new Color(.57f, .66f, 1f), 8.25f, -.66f, -3.65f, 2)
            };
            pads = new[] { MakePad(.09f, .67f), MakePad(.39f, -.52f), MakePad(.65f, .66f) };
            ResetRace();
            town.UpdateTrain(-3.5f, 0, 1);
            UpdateCamera(true);
        }

        private ChargePad MakePad(float fraction, float lane)
        {
            Vector3 point = course.Lane(course.Length * fraction, lane, out Vector3 forward);
            return new ChargePad { Surface = town.CreateBoostPad(point, forward) };
        }

        private void ResetRace()
        {
            progress.Reset();
            elapsed = penalties = lapStarted = bestLap = recovery = 0;
            best = PlayerPrefs.GetFloat(RecordKey);
            boost = .36f;
            finalPlace = 1;
            boosting = record = false;
            notice = string.Empty;
            noticeUntil = 0;
            trainX = -23;
            town.UpdateTrain(trainX, 0, 1);
            foreach (RivalDriver rival in rivals)
            {
                rival.Reset();
                rival.Car.gameObject.SetActive(grandPrix);
            }
            foreach (ChargePad pad in pads)
            {
                pad.ReadyAt = 0;
                pad.Surface.gameObject.SetActive(true);
            }
            Recover(0);
        }

        private void Recover(float at)
        {
            distance = Mathf.Repeat(at, course.Length);
            car.position = course.Sample(at, out Vector3 forward) + Vector3.up * .11f;
            car.rotation = Quaternion.LookRotation(forward);
            heading = car.eulerAngles.y;
            speed = steering = 0;
            velocity = Vector3.zero;
            town.PlayerModel.ResetMotion();
            SetBoosting(false);
            town.BoostTrail.Clear();
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
                SetBoosting(false);
            }
            else if (state == RaceState.Paused)
                state = pausedState;
        }

        private RaceState pausedState;

        private void Update()
        {
            touchSeen |= Input.touchCount > 0;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                TogglePause();
            if (Input.GetKeyDown(KeyCode.M))
                sound.Toggle();
            if (Input.GetKeyDown(KeyCode.Return) && (state == RaceState.Ready || state == RaceState.Finished))
                StartRace(grandPrix);

            float dt = Time.deltaTime;
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
                if (Input.GetKeyDown(KeyCode.R) && recovery <= 0)
                    ResetCar(false);
                ReadControls(out float turn, out float throttle, out bool useBoost);
                while (dt > 0 && state == RaceState.Racing)
                {
                    float step = Mathf.Min(dt, MaximumStep);
                    Step(turn, throttle, useBoost, step);
                    dt -= step;
                }
                town.PlayerModel.Animate(speed, steering, Time.deltaTime);
            }
            sound.Update(speed, boosting, state == RaceState.Racing || state == RaceState.Countdown);
        }

        private void Step(float turn, float throttle, bool useBoost, float dt)
        {
            elapsed += dt;
            trainX = -23 + Mathf.Repeat(elapsed * 5.65f, 49);
            town.UpdateTrain(trainX, elapsed, dt);
            if (grandPrix)
            {
                foreach (RivalDriver rival in rivals)
                    rival.Step(dt, elapsed, trainX, car);
            }
            foreach (ChargePad pad in pads)
            {
                if (pad.ReadyAt > 0 && elapsed >= pad.ReadyAt)
                {
                    pad.ReadyAt = 0;
                    pad.Surface.gameObject.SetActive(true);
                }
            }
            if (recovery > 0)
                recovery = Mathf.Max(0, recovery - dt);
            else
                Drive(turn, throttle, useBoost, dt);
        }

        private void ResetCar(bool struckByTrain)
        {
            Recover(progress.LastGate + .35f);
            recovery = struckByTrain ? .84f : .35f;
            penalties += struckByTrain ? 3 : 2;
            sound.Signal(RaceSound.Hit);
            Notify(struckByTrain ? "TRAIN COLLISION  +3s" : "BACK ON TRACK  +2s");
        }

        private void SetBoosting(bool active)
        {
            if (boosting == active)
                return;
            boosting = active;
            town.BoostTrail.emitting = active;
            if (active)
                sound.Signal(RaceSound.Boost);
        }

        private void Drive(float turn, float throttle, bool useBoost, float dt)
        {
            float previous = distance;
            course.Locate(car.position, out float lateral);
            bool offRoad = lateral > RaceCourse.HalfWidth;
            SetBoosting(useBoost && throttle > 0 && boost > 0 && !offRoad);
            if (boosting)
                boost = Mathf.Max(0, boost - .49f * dt);
            float limit = offRoad ? 4.8f : boosting ? 18.0f : 12.8f;
            float target = throttle >= 0 ? limit * throttle : 4.4f * throttle;
            float rate = throttle == 0 ? 11 : speed * throttle < 0 ? 25 : boosting ? 22 : 14;
            speed = Mathf.MoveTowards(speed, target, rate * dt);
            steering = Mathf.MoveTowards(steering, turn, dt * 10);
            float traction = Mathf.Min(Mathf.Abs(speed) / 1.6f, 1) * Mathf.Sign(speed);
            heading += steering * traction * (boosting ? 116 : 148) * dt;
            if (Mathf.Abs(steering) < .05f && !offRoad && speed > 1.5f)
            {
                Vector3 aim = course.Sample(distance + 2.8f, out _) - car.position;
                aim.y = 0;
                float correction = Vector3.SignedAngle(car.forward, aim, Vector3.up);
                if (Mathf.Abs(correction) < 58)
                    heading += Mathf.Clamp(correction * 1.9f, -42, 42) * dt;
            }

            car.rotation = Quaternion.Euler(0, heading, 0);
            velocity = Vector3.Lerp(velocity, car.forward * speed, 1 - Mathf.Exp(-15 * dt));
            Vector3 next = car.position + velocity * dt;
            if (town.Blocks(next))
            {
                speed = 0;
                velocity = Vector3.zero;
                return;
            }
            car.position = next;
            if (Mathf.Abs(next.x) > 17.8f || Mathf.Abs(next.z) > 13.8f)
            {
                ResetCar(false);
                return;
            }
            if (RailwayTown.TrainHits(trainX, next))
            {
                ResetCar(true);
                return;
            }

            foreach (ChargePad pad in pads)
            {
                if (pad.ReadyAt != 0 || (next - pad.Surface.position).sqrMagnitude > .95f * .95f)
                    continue;
                pad.ReadyAt = elapsed + 5;
                pad.Surface.gameObject.SetActive(false);
                boost = Mathf.Min(1, boost + .65f);
                sound.Signal(RaceSound.Charge);
                Notify("BATTERY CHARGED", 1.25f);
            }

            distance = course.Locate(next, out lateral);
            if (progress.Advance(previous, distance, lateral))
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
            SetBoosting(false);
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
            float playerDistance = progress.RankDistance(distance);
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
            Vector3 target = overview ? Vector3.zero : car.position + car.forward * Mathf.Max(speed, 0) * .125f;
            float size = overview ? Mathf.Max(16.2f, 19f / aspect) : Mathf.Max(boosting ? 8.1f : 7.45f, 5.8f / aspect);
            Vector3 offset = new Vector3(5, 15, -12);
            float blend = snap ? 1 : 1 - Mathf.Exp(-6 * Time.deltaTime);
            raceCamera.transform.position = Vector3.Lerp(raceCamera.transform.position, target + offset, blend);
            raceCamera.transform.rotation = Quaternion.LookRotation(-offset);
            raceCamera.orthographicSize = Mathf.Lerp(raceCamera.orthographicSize, size, blend);
        }
    }
}

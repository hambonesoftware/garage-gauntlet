using UnityEngine;

namespace GarageGauntlet
{
    public sealed class RcRace : MonoBehaviour
    {
        private const int Laps = 3;
        private const string BestTimeKey = "garage-gauntlet-railway-race-v1";
        private enum RaceState { Ready, Countdown, Racing, Finished }

        [SerializeField] private Shader paletteShader = null;
        private readonly RaceCourse course = new RaceCourse();
        private RaceState state;
        private RailwayTown town;
        private Transform car, train;
        private Camera raceCamera;
        private float countdown, time, best, speed, heading, distance, trainX;
        private int lap, gate;
        private Vector3 velocity;
        private float hitCooldown;
        private string notice;
        private float noticeUntil;
        private GUIStyle title, body, small, button, panel, accent;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            best = PlayerPrefs.GetFloat(BestTimeKey);
            town = new RailwayTown(transform, course, paletteShader);
            car = town.Car;
            train = town.Train;
            raceCamera = town.Camera;
            ResetRace();
            trainX = -3.5f;
            UpdateTrainPosition();
            UpdateCamera(true);
        }

        private void ResetRace()
        {
            lap = 1;
            gate = 1;
            time = 0;
            hitCooldown = 0;
            notice = string.Empty;
            trainX = -23;
            UpdateTrainPosition();
            Recover(0);
        }

        private void Recover(float at)
        {
            distance = at;
            car.position = course.Sample(at, out Vector3 forward) + Vector3.up * .11f;
            car.rotation = Quaternion.LookRotation(forward);
            heading = car.eulerAngles.y;
            speed = 0;
            velocity = Vector3.zero;
        }

        private void StartRace()
        {
            ResetRace();
            countdown = 3;
            state = RaceState.Countdown;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Return) && (state == RaceState.Ready || state == RaceState.Finished))
                StartRace();

            if (state == RaceState.Countdown)
            {
                countdown -= dt;
                if (countdown <= 0)
                {
                    state = RaceState.Racing;
                    Notify("GO!", .8f);
                }
            }

            if (state != RaceState.Racing)
                return;

            time += dt;
            hitCooldown = Mathf.Max(0, hitCooldown - dt);
            trainX += 5.3f * dt;
            if (trainX > 23)
                trainX = -23;
            UpdateTrainPosition();

            if (Input.GetKeyDown(KeyCode.R))
                ResetCar();

            ReadControls(out float steer, out float throttle);
            Drive(steer, throttle, dt);
        }

        private void UpdateTrainPosition()
        {
            train.position = new Vector3(trainX, .13f, 0);
            bool visible = Mathf.Abs(trainX) < 20;
            if (train.gameObject.activeSelf != visible)
                train.gameObject.SetActive(visible);
        }

        private void ResetCar()
        {
            Recover(course.Length * ((gate - 1) / 4f) + .35f);
            time += 2;
            Notify("Back on track  +2s");
        }

        private void Drive(float steer, float throttle, float dt)
        {
            float oldDistance = distance;
            course.Locate(car.position, out float lateral);
            bool offRoad = lateral > RaceCourse.HalfWidth;
            float limit = offRoad ? 4.5f : 13.5f;
            float target = throttle >= 0 ? limit * throttle : 4.2f * throttle;
            float rate = throttle == 0 ? 12 : speed * throttle < 0 ? 22 : 13;
            speed = Mathf.MoveTowards(speed, target, rate * dt);
            float steering = Mathf.Min(Mathf.Abs(speed) / 2, 1) * Mathf.Sign(speed);
            heading += steer * steering * 125 * dt;
            car.rotation = Quaternion.Euler(0, heading, 0);
            velocity = Vector3.Lerp(velocity, car.forward * speed, 1 - Mathf.Exp(-11 * dt));
            Vector3 nextPosition = car.position + velocity * dt;
            if (town.Blocks(nextPosition))
            {
                speed = 0;
                velocity = Vector3.zero;
                return;
            }
            car.position = nextPosition;

            Vector3 position = car.position;
            if (Mathf.Abs(position.x) > 17.8f || Mathf.Abs(position.z) > 13.8f)
            {
                ResetCar();
                return;
            }

            if (hitCooldown == 0 && Mathf.Abs(position.z) < .9f && Mathf.Abs(position.x - trainX) < 2.65f)
            {
                Recover(oldDistance);
                hitCooldown = 1.4f;
                time += 3;
                Notify("Train crossing  +3s");
                return;
            }

            distance = course.Locate(position, out lateral);
            float advance = Mathf.Repeat(distance - oldDistance + course.Length / 2, course.Length) - course.Length / 2;
            if (lateral > RaceCourse.HalfWidth + .2f || advance <= 0 || advance > 2)
                return;

            float toGate = Mathf.Repeat(course.Length * (gate / 4f) - oldDistance, course.Length);
            if (toGate > advance + .001f)
                return;

            if (gate < 4)
            {
                gate++;
                return;
            }

            if (lap < Laps)
            {
                lap++;
                gate = 1;
                Notify("Lap " + lap);
                return;
            }

            state = RaceState.Finished;
            if (best <= 0 || time < best)
            {
                best = time;
                PlayerPrefs.SetFloat(BestTimeKey, best);
                PlayerPrefs.Save();
                Notify("New personal best!");
            }
        }

        private void Notify(string message, float seconds = 2)
        {
            notice = message;
            noticeUntil = Time.unscaledTime + seconds;
        }

        private void LateUpdate() => UpdateCamera(false);

        private void UpdateCamera(bool snap)
        {
            float aspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            Vector3 target = car.position;
            bool overview = state == RaceState.Ready;
            if (overview)
                target = Vector3.zero;
            float size = overview ? Mathf.Max(16, 20 / aspect) : Mathf.Max(7.5f, 5.8f / aspect);
            Vector3 offset = new Vector3(5, 15, -12);
            float blend = snap ? 1 : 1 - Mathf.Exp(-5 * Time.deltaTime);
            raceCamera.transform.position = Vector3.Lerp(raceCamera.transform.position, target + offset, blend);
            raceCamera.transform.rotation = Quaternion.LookRotation(-offset);
            raceCamera.orthographicSize = Mathf.Lerp(raceCamera.orthographicSize, size, blend);
        }

        private static float Scale => Mathf.Clamp(Mathf.Min(Screen.width / 640f, Screen.height / 480f), .45f, 1.7f);
        private static float Width => Screen.width / Scale;
        private static float Height => Screen.height / Scale;
        private static bool ShowTouch => Application.isMobilePlatform || Input.touchSupported;
        private static Rect Left => new Rect(18, Height - 83, 65, 65);
        private static Rect Right => new Rect(92, Height - 83, 65, 65);
        private static Rect Reverse => new Rect(Width - 171, Height - 83, 68, 65);
        private static Rect Gas => new Rect(Width - 94, Height - 83, 76, 65);

        private void ReadControls(out float steer, out float throttle)
        {
            steer = Axis(KeyCode.A, KeyCode.LeftArrow, KeyCode.D, KeyCode.RightArrow);
            throttle = Axis(KeyCode.S, KeyCode.DownArrow, KeyCode.W, KeyCode.UpArrow);
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    continue;
                Vector2 point = new Vector2(touch.position.x / Scale, (Screen.height - touch.position.y) / Scale);
                if (Left.Contains(point)) steer = -1;
                if (Right.Contains(point)) steer = 1;
                if (Reverse.Contains(point)) throttle = -1;
                if (Gas.Contains(point)) throttle = 1;
            }
        }

        private static int Axis(KeyCode negative, KeyCode alternateNegative, KeyCode positive, KeyCode alternatePositive)
        {
            return (Input.GetKey(positive) || Input.GetKey(alternatePositive) ? 1 : 0)
                - (Input.GetKey(negative) || Input.GetKey(alternateNegative) ? 1 : 0);
        }

        private void OnGUI()
        {
            if (title == null)
                CreateStyles();
            GUI.matrix = Matrix4x4.Scale(new Vector3(Scale, Scale, 1));
            GUI.Box(new Rect(14, 14, 250, 72), GUIContent.none, panel);
            GUI.Label(new Rect(27, 22, 215, 23), "GARAGE GAUNTLET", accent);
            GUI.Label(new Rect(27, 48, 224, 26), "Lap " + lap + " / " + Laps + "      " + FormatTime(time), body);

            if (state == RaceState.Ready || state == RaceState.Finished)
                DrawMenu();
            else if (state == RaceState.Countdown)
                GUI.Label(new Rect(Width / 2 - 32, Height / 2 - 90, 100, 60), Mathf.CeilToInt(countdown).ToString(), title);
            else
                DrawRaceHud();
        }

        private void DrawMenu()
        {
            float w = Mathf.Min(460, Width - 36);
            float x = (Width - w) / 2;
            float y = Height / 2 - 92;
            GUI.Box(new Rect(x, y, w, 184), GUIContent.none, panel);
            GUI.Label(new Rect(x + 18, y + 13, w - 36, 22), "MODEL RAILWAY  /  TIME TRIAL", accent);
            GUI.Label(new Rect(x + 18, y + 35, w - 36, 48),
                state == RaceState.Ready ? "Railway Rush" : FormatTime(time), title);
            string hint = state == RaceState.Ready
                ? (ShowTouch ? "Three laps. Steer and accelerate with the on-screen controls."
                    : "Three laps. Drive with WASD or the arrow keys.")
                : "Personal best  " + FormatTime(best);
            GUI.Label(new Rect(x + 18, y + 87, w - 36, 38), hint, small);
            if (GUI.Button(new Rect(x + 18, y + 127, w - 36, 41),
                state == RaceState.Ready ? "Start race" : "Race again", button))
                StartRace();
        }

        private void DrawRaceHud()
        {
            if (GUI.Button(new Rect(Width - 110, 14, 96, 34), ShowTouch ? "Reset" : "Reset [R]", button))
                ResetCar();
            if (Time.unscaledTime < noticeUntil)
            {
                GUI.Box(new Rect(Width / 2 - 109, 94, 218, 36), GUIContent.none, panel);
                GUI.Label(new Rect(Width / 2 - 99, 101, 205, 28), notice, accent);
            }
            if (!ShowTouch)
                return;
            GUI.Box(Left, "LEFT", button);
            GUI.Box(Right, "RIGHT", button);
            GUI.Box(Reverse, "BRAKE", button);
            GUI.Box(Gas, "GO", button);
        }

        private void CreateStyles()
        {
            var background = new Texture2D(1, 1);
            background.SetPixel(0, 0, new Color(.06f, .10f, .12f, .94f));
            background.Apply();
            panel = new GUIStyle(GUI.skin.box);
            panel.normal.background = background;
            title = new GUIStyle(GUI.skin.label) { fontSize = 33, fontStyle = FontStyle.Bold };
            body = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
            small = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
            accent = new GUIStyle(body) { fontSize = 14 };
            title.normal.textColor = body.normal.textColor = small.normal.textColor = Color.white;
            accent.normal.textColor = new Color(1, .77f, .35f);
            button = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold };
        }

        private static string FormatTime(float seconds)
        {
            int hundredths = Mathf.FloorToInt(seconds * 100);
            return (hundredths / 6000).ToString("00") + ":" +
                (hundredths / 100 % 60).ToString("00") + "." + (hundredths % 100).ToString("00");
        }
    }
}

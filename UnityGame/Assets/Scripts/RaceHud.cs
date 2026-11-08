using UnityEngine;

namespace GarageGauntlet
{
    public sealed partial class RcRace
    {
        private static readonly Color Mint = new Color(.35f, .98f, .80f);
        private static readonly Color Amber = new Color(1f, .71f, .35f);
        private static readonly Color Ivory = new Color(.96f, .96f, .89f);
        private static readonly Color Muted = new Color(.63f, .72f, .76f);
        private static readonly Color Ink = new Color(.038f, .075f, .105f, .96f);
        private static readonly Color Quiet = new Color(.11f, .19f, .24f, .94f);
        private static readonly string[] Places = { "1ST", "2ND", "3RD", "4TH" };
        private GUIStyle display, metric, body, small, label, centered, primary, secondary;
        private Texture2D map;
        private float Scale => Mathf.Clamp(Mathf.Min(Screen.width / 680f, Screen.height / 480f), .45f, 1.85f);
        private float Width => Screen.width / Scale;
        private float Height => Screen.height / Scale;
        private int triggerFinger = -1;
        private bool mouseTrigger;
        private static readonly float[] TriggerTicks = { 1, .75f, .5f, .25f, 0 };
        private Rect TriggerPad
        {
            get
            {
                float height = Mathf.Min(Mathf.Max(138, Height - 184), Mathf.Clamp(Height * .57f, 248, 370));
                return new Rect(Width - 192, Height - height - 14, 178, height);
            }
        }

        private static Rect TriggerTrack(Rect pad) => new Rect(pad.x + 22, pad.y + 58, 62, pad.height - 94);

        private void ReleaseTrigger()
        {
            triggerFinger = -1;
            mouseTrigger = false;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                ReleaseTrigger();
        }

        private float ReadTrigger()
        {
            float keyboard = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space)
                ? 1 : Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? .36f : 0;
            Rect pad = TriggerPad;
            bool found = false;
            float touchPower = 0;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    continue;
                Vector2 point = ToHud(touch.position);
                if (triggerFinger < 0 && touch.phase == TouchPhase.Began && pad.Contains(point))
                    triggerFinger = touch.fingerId;
                if (triggerFinger == touch.fingerId)
                {
                    found = true;
                    touchPower = PowerAt(pad, point.y);
                }
            }
            if (!found)
                triggerFinger = -1;
            if (Input.touchCount != 0)
            {
                mouseTrigger = false;
                return Mathf.Max(keyboard, touchPower);
            }

            Vector2 mouse = ToHud(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
                mouseTrigger = pad.Contains(mouse);
            if (!Input.GetMouseButton(0))
                mouseTrigger = false;
            return Mathf.Max(keyboard, mouseTrigger ? PowerAt(pad, mouse.y) : 0);
        }

        private Vector2 ToHud(Vector2 position) => new Vector2(position.x / Scale, (Screen.height - position.y) / Scale);
        private static float PowerAt(Rect pad, float y)
        {
            Rect track = TriggerTrack(pad);
            return TriggerPower(Mathf.InverseLerp(track.yMax, track.yMin, y));
        }

        internal static float TriggerPower(float travel) => MapTrigger(travel, .18f, .82f, .30f, .76f);
        internal static float TriggerTravel(float power) => MapTrigger(power, .30f, .76f, .18f, .82f);

        private static float MapTrigger(float value, float first, float last, float low, float high)
        {
            if (value <= first)
                return Mathf.Lerp(0, low, Mathf.InverseLerp(0, first, value));
            if (value <= last)
                return Mathf.Lerp(low, high, Mathf.InverseLerp(first, last, value));
            return Mathf.Lerp(high, 1, Mathf.InverseLerp(last, 1, value));
        }

        private void OnGUI()
        {
            if (display == null)
                CreateHud();
            GUI.matrix = Matrix4x4.Scale(new Vector3(Scale, Scale, 1));

            if (state == RaceState.Ready)
                DrawGridMenu();
            else
            {
                DrawTelemetry();
                if (state == RaceState.Finished)
                    DrawResults();
                else if (state == RaceState.Paused)
                    DrawPause();
                else if (state == RaceState.Countdown)
                {
                    DrawCountdown();
                    DrawTrigger(SlotMotor.TopSpeed, false);
                }
                else
                    DrawRaceHud();
            }
        }

        private bool GridMenuOnSide => Screen.width >= Screen.height * 1.45f;

        private Rect GridMenuCard
        {
            get
            {
                float width = Mathf.Min(380, GridMenuOnSide ? Width * .43f : Width - 32);
                return new Rect(GridMenuOnSide ? 26 : (Width - width) / 2,
                    GridMenuOnSide ? Height / 2 - 144 : 22, width, 288);
            }
        }

        private void DrawGridMenu()
        {
            Rect card = GridMenuCard;
            float x = card.x, y = card.y, width = card.width;
            Card(card, Mint);
            Caption(x + 19, y + 19, width - 38, "GARAGE GAUNTLET   /   01");
            GUI.Label(new Rect(x + 18, y + 41, width - 36, 62), "Railway Rush", display);
            GUI.Label(new Rect(x + 19, y + 106, width - 38, 42),
                "Race around Switchyard. Watch for the model train at both crossings.", body);

            if (GUI.Button(new Rect(x + 18, y + 163, width - 36, 45), "START GRAND PRIX", primary))
                StartRace(true);
            float half = (width - 44) / 2;
            if (GUI.Button(new Rect(x + 18, y + 215, half, 36), "TIME TRIAL", secondary))
                StartRace(false);
            if (GUI.Button(new Rect(x + 26 + half, y + 215, half, 36), sound.Enabled ? "SOUND ON" : "SOUND OFF", secondary))
                sound.Toggle();
            GUI.Label(new Rect(x + 19, y + 258, width - 38, 20), "Drag the trigger  •  W for full power  •  Shift to ease", small);
        }

        private void DrawTelemetry()
        {
            const float x = 14, y = 14;
            Card(new Rect(x, y, 253, 88), Mint);
            Caption(x + 12, y + 8, 223, grandPrix ? "GRAND PRIX" : "TIME TRIAL");
            string rank = grandPrix ? Places[(state == RaceState.Finished ? finalPlace : PlaceInRace()) - 1] + "   " : "";
            GUI.Label(new Rect(x + 11, y + 22, 229, 31), rank + "LAP " + progress.Lap + " / " + RaceProgress.TotalLaps, metric);
            GUI.Label(new Rect(x + 12, y + 56, 113, 24), FormatTime(RaceTime), body);
            string lapTime = bestLap > 0 ? "BEST LAP " + FormatTime(bestLap) : "3-LAP CIRCUIT";
            GUI.Label(new Rect(x + 109, y + 57, 139, 20), lapTime, small);

            if (state == RaceState.Racing || state == RaceState.Countdown)
            {
                if (GUI.Button(new Rect(Width - 91, 14, 77, 34), "PAUSE", secondary))
                    TogglePause();
                DrawMap(new Rect(Width - 178, 54, 164, 109));
            }
        }

        private void DrawCountdown()
        {
            Rect card = new Rect(Width / 2 - 58, Height / 2 - 120, 116, 82);
            Card(card, Amber);
            GUI.Label(new Rect(card.x, card.y + 11, card.width, 59), Mathf.CeilToInt(countdown).ToString(), centered);
        }

        private void DrawRaceHud()
        {
            bool train = CrossingWarning();
            float suggestedSpeed = motor.BrakeTarget();
            bool corner = !motor.Recovering && suggestedSpeed < SlotMotor.TopSpeed - .5f
                && suggestedSpeed < motor.Speed + .7f;
            bool tailSliding = !motor.Recovering && Mathf.Abs(motor.SlideAngle) > 16;
            string banner = Time.unscaledTime < noticeUntil ? notice
                : train ? "TRAIN CROSSING · LIFT OFF"
                : tailSliding ? "TAIL SLIDING · EASE OFF" : corner ? "TIGHT BEND · LIFT OFF" : "";
            if (banner.Length != 0)
            {
                Rect alert = new Rect(Width / 2 - 142, 116, 284, 35);
                Card(alert, train || corner || tailSliding ? Amber : Mint);
                Caption(alert.x + 11, alert.y + 9, alert.width - 22, banner);
            }

            DrawTrigger(suggestedSpeed, corner || train || tailSliding);
        }

        private void DrawTrigger(float suggestedSpeed, bool warning)
        {
            Rect pad = TriggerPad;
            Color accent = warning ? Amber : Mint;
            Card(pad, accent);
            Caption(pad.x + 13, pad.y + 9, pad.width - 26, "THROTTLE TRIGGER");
            GUI.Label(new Rect(pad.x + 12, pad.y + 24, pad.width - 24, 29),
                Mathf.RoundToInt(motor.Trigger * 100) + "%", metric);

            Rect track = TriggerTrack(pad);
            Fill(new Rect(track.x - 4, track.y - 4, track.width + 8, track.height + 8), Quiet);
            Fill(track, Ink);
            float leverY = track.yMax - track.height * TriggerTravel(motor.Trigger);
            Fill(new Rect(track.x, leverY, track.width, track.yMax - leverY), accent * .56f);

            foreach (float tick in TriggerTicks)
            {
                float y = track.yMax - track.height * TriggerTravel(tick);
                Fill(new Rect(track.xMax + 8, y, 8, 1), Muted);
                GUI.Label(new Rect(track.xMax + 20, y - 10, 58, 20), Mathf.RoundToInt(tick * 100) + "%", small);
            }

            if (suggestedSpeed < SlotMotor.TopSpeed - .5f)
            {
                float safeY = track.yMax - track.height * TriggerTravel(suggestedSpeed / SlotMotor.TopSpeed);
                Fill(new Rect(track.x - 6, safeY, track.width + 12, 2), Amber);
            }

            Fill(new Rect(track.x - 4, leverY - 6, track.width + 8, 12), accent);
            Fill(new Rect(track.x + 10, leverY - 1, track.width - 20, 2), Ink);
            GUI.Label(new Rect(pad.x + 13, pad.yMax - 24, pad.width - 26, 20), "UP · LIFT TO BRAKE", small);
        }

        private void DrawPause()
        {
            Rect card = Center(312, 200);
            Card(card, Amber);
            Caption(card.x + 17, card.y + 14, card.width - 34, "RACE PAUSED");
            GUI.Label(new Rect(card.x + 16, card.y + 33, card.width - 32, 57), "Pit stop", display);
            if (GUI.Button(new Rect(card.x + 17, card.y + 97, card.width - 34, 40), "RESUME", primary))
                TogglePause();
            if (GUI.Button(new Rect(card.x + 17, card.y + 145, card.width - 34, 34), "MAIN MENU", secondary))
                ReturnToGrid();
        }

        private void DrawResults()
        {
            Rect card = Center(370, 239);
            Card(card, record ? Amber : Mint);
            Caption(card.x + 17, card.y + 14, card.width - 34, record ? "NEW PERSONAL BEST" : "CHECKERED FLAG");
            string headingText = grandPrix ? Places[finalPlace - 1] + " PLACE" : "TIME TRIAL";
            GUI.Label(new Rect(card.x + 17, card.y + 33, card.width - 34, 60), headingText, display);
            GUI.Label(new Rect(card.x + 18, card.y + 104, card.width - 36, 29),
                FormatTime(RaceTime) + "   •   Best lap " + FormatTime(bestLap), body);
            GUI.Label(new Rect(card.x + 18, card.y + 130, card.width - 36, 24),
                "Personal best " + FormatTime(best), small);
            float half = (card.width - 42) / 2;
            if (GUI.Button(new Rect(card.x + 17, card.y + 177, half, 40), "RACE AGAIN", primary))
                StartRace(grandPrix);
            if (GUI.Button(new Rect(card.x + 25 + half, card.y + 177, half, 40), "MAIN MENU", secondary))
                ReturnToGrid();
        }

        private void ReturnToGrid()
        {
            grandPrix = true;
            ResetRace();
            state = RaceState.Ready;
        }

        private Rect Center(float width, float height) => new Rect((Width - width) / 2, (Height - height) / 2, width, height);

        private void DrawMap(Rect card)
        {
            Card(card, Quiet);
            Rect route = new Rect(card.x + 9, card.y + 9, card.width - 18, card.height - 17);
            GUI.DrawTexture(route, map);
            if (grandPrix)
            {
                foreach (RivalDriver rival in rivals)
                    Dot(route, rival.Car.position, rival.Color, 5);
            }
            Dot(route, town.Train.position, Amber, 4);
            Dot(route, car.position, Mint, 7);
        }

        private static Vector2 MapPoint(Vector3 position) => new Vector2(
            Mathf.Clamp01(.5f + (position.x - RaceCourse.BoardCenter.x) / (2 * RaceCourse.BoardHalfWidth)),
            Mathf.Clamp01(.5f - (position.z - RaceCourse.BoardCenter.z) / (2 * RaceCourse.BoardHalfDepth)));

        private static void Dot(Rect route, Vector3 position, Color color, float size)
        {
            Vector2 dot = MapPoint(position);
            Fill(new Rect(route.x + route.width * dot.x - size / 2,
                route.y + route.height * dot.y - size / 2, size, size), color);
        }

        private Texture2D DrawMapTexture()
        {
            const int width = 146, height = 92;
            var pixels = new Color[width * height];
            Stroke(course.Railway.Path, new Color(.79f, .51f, .28f, .92f), 0);
            Stroke(course, new Color(.61f, .68f, .68f, .76f), 1);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;

            void Stroke(RaceCourse path, Color color, int weight)
            {
                for (int segment = 0; segment < path.Count; segment++)
                {
                    Vector2 a = MapPoint(path.At(segment));
                    Vector2 b = MapPoint(path.At(segment + 1));
                    for (int step = 0; step < 10; step++)
                    {
                        Vector2 p = Vector2.Lerp(a, b, step / 10f);
                        int x = Mathf.RoundToInt(p.x * (width - 1));
                        int y = Mathf.RoundToInt((1 - p.y) * (height - 1));
                        for (int row = Mathf.Max(0, y - weight); row <= Mathf.Min(height - 1, y + weight); row++)
                            for (int col = Mathf.Max(0, x - weight); col <= Mathf.Min(width - 1, x + weight); col++)
                                pixels[row * width + col] = color;
                    }
                }
            }
        }

        private void Caption(float x, float y, float width, string text) =>
            GUI.Label(new Rect(x, y, width, 21), text, label);

        private static void Card(Rect rect, Color edge)
        {
            Fill(rect, Ink);
            Fill(new Rect(rect.x, rect.y, rect.width, 2), edge);
        }

        private static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void CreateHud()
        {
            display = Text(39, Ivory, true);
            metric = Text(24, Ivory, true);
            body = Text(15, Ivory);
            body.wordWrap = true;
            small = Text(13, Muted);
            label = Text(13, Mint, true);
            centered = Text(48, Ivory, true);
            centered.alignment = TextAnchor.MiddleCenter;
            primary = MakeButton(Mint, Ink);
            secondary = MakeButton(Quiet, Ivory);
            map = DrawMapTexture();
        }

        private static GUIStyle Text(int size, Color color, bool bold = false)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal };
            style.normal.textColor = color;
            return style;
        }

        private static GUIStyle MakeButton(Color background, Color foreground)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(), padding = new RectOffset()
            };
            var normal = new Texture2D(1, 1);
            normal.SetPixel(0, 0, background);
            normal.Apply();
            var hover = new Texture2D(1, 1);
            hover.SetPixel(0, 0, Color.Lerp(background, Color.white, .18f));
            hover.Apply();
            style.normal.background = normal;
            style.hover.background = style.active.background = hover;
            style.normal.textColor = style.hover.textColor = style.active.textColor = foreground;
            return style;
        }

        private static string FormatTime(float seconds)
        {
            int hundredths = Mathf.FloorToInt(seconds * 100);
            return (hundredths / 6000).ToString("00") + ":" +
                (hundredths / 100 % 60).ToString("00") + "." + (hundredths % 100).ToString("00");
        }
    }
}

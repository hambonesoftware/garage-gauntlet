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
        private Rect TriggerPad => new Rect(Width - 174, Height - 141, 156, 122);

        private void ReleaseTrigger()
        {
            triggerFinger = -1;
            mouseTrigger = false;
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
        private static float PowerAt(Rect pad, float y) =>
            Mathf.Lerp(.12f, 1, 1 - Mathf.InverseLerp(pad.y + 28, pad.yMax - 10, y));

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
                    DrawCountdown();
                else
                    DrawRaceHud();
            }
        }

        private void DrawGridMenu()
        {
            float width = Mathf.Min(380, Width - 32);
            float x = Width >= 790 ? 26 : (Width - width) / 2;
            float y = Height / 2 - 144;
            Card(new Rect(x, y, width, 288), Mint);
            Caption(x + 19, y + 19, width - 38, "GARAGE GAUNTLET   /   01");
            GUI.Label(new Rect(x + 18, y + 41, width - 36, 62), "Railway Rush", display);
            GUI.Label(new Rect(x + 19, y + 106, width - 38, 42),
                "Four powered lanes. Squeeze the trigger on straights and lift before tight bends.", body);

            if (GUI.Button(new Rect(x + 18, y + 163, width - 36, 45), "START GRAND PRIX", primary))
                StartRace(true);
            float half = (width - 44) / 2;
            if (GUI.Button(new Rect(x + 18, y + 215, half, 36), "TIME TRIAL", secondary))
                StartRace(false);
            if (GUI.Button(new Rect(x + 26 + half, y + 215, half, 36), sound.Enabled ? "SOUND ON" : "SOUND OFF", secondary))
                sound.Toggle();
            GUI.Label(new Rect(x + 19, y + 258, width - 38, 20), "W / Space for full power  •  Shift for corners", small);
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

            Rect pad = TriggerPad;
            Card(pad, corner || train || tailSliding ? Amber : Mint);
            Caption(pad.x + 10, pad.y + 8, pad.width - 20, "TRIGGER   " + Mathf.RoundToInt(motor.Trigger * 100) + "%");
            Rect guide = new Rect(pad.x + 12, pad.y + 31, 33, 75);
            Fill(guide, Quiet);
            float fill = guide.height * motor.Trigger;
            Fill(new Rect(guide.x, guide.yMax - fill, guide.width, fill), corner || tailSliding ? Amber : Mint);
            float safe = Mathf.Clamp01(suggestedSpeed / SlotMotor.TopSpeed);
            Fill(new Rect(guide.x, guide.yMax - guide.height * safe, guide.width, 2), Ivory);
            GUI.Label(new Rect(pad.x + 51, pad.y + 34, 100, 20), "SLIDE UP", small);
            GUI.Label(new Rect(pad.x + 51, pad.y + 55, 100, 20), "TO SQUEEZE", small);
            GUI.Label(new Rect(pad.x + 51, pad.y + 87, 101, 20), "LIFT TO BRAKE", small);
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
            town.UpdateTrain(-3.5f, 0, 1);
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
            Mathf.Clamp01((position.x + 17) / 34), Mathf.Clamp01((12.5f - position.z) / 25));

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
            Color rail = new Color(.55f, .36f, .23f, .8f);
            for (int x = 1; x < width - 1; x++)
                pixels[(height / 2) * width + x] = rail;
            Color road = new Color(.61f, .68f, .68f, .76f);
            for (int segment = 0; segment < course.Count; segment++)
            {
                Vector2 a = MapPoint(course.At(segment));
                Vector2 b = MapPoint(course.At(segment + 1));
                for (int step = 0; step < 10; step++)
                {
                    Vector2 p = Vector2.Lerp(a, b, step / 10f);
                    int x = Mathf.RoundToInt(p.x * (width - 1));
                    int y = Mathf.RoundToInt((1 - p.y) * (height - 1));
                    for (int row = Mathf.Max(0, y - 1); row <= Mathf.Min(height - 1, y + 1); row++)
                        for (int col = Mathf.Max(0, x - 1); col <= Mathf.Min(width - 1, x + 1); col++)
                            pixels[row * width + col] = road;
                }
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
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

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
        private GUIStyle display, metric, body, small, label, centered, primary, secondary, padText;
        private Texture2D map;
        private float Scale => Mathf.Clamp(Mathf.Min(Screen.width / 680f, Screen.height / 480f), .45f, 1.85f);
        private float Width => Screen.width / Scale;
        private float Height => Screen.height / Scale;
        private bool ShowTouch => Application.isMobilePlatform || touchSeen;
        private Rect Left => new Rect(16, Height - 85, 70, 65);
        private Rect Right => new Rect(94, Height - 85, 70, 65);
        private Rect Reverse => new Rect(Width - 233, Height - 85, 63, 65);
        private Rect Boost => new Rect(Width - 162, Height - 85, 65, 65);
        private Rect Gas => new Rect(Width - 89, Height - 85, 73, 65);

        private void ReadControls(out float turn, out float throttle, out bool useBoost)
        {
            turn = Axis(KeyCode.A, KeyCode.LeftArrow, KeyCode.D, KeyCode.RightArrow);
            throttle = Axis(KeyCode.S, KeyCode.DownArrow, KeyCode.W, KeyCode.UpArrow);
            useBoost = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift);
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    continue;
                Vector2 point = new Vector2(touch.position.x / Scale, (Screen.height - touch.position.y) / Scale);
                if (Left.Contains(point)) turn = -1;
                if (Right.Contains(point)) turn = 1;
                if (Reverse.Contains(point)) throttle = -1;
                if (Gas.Contains(point)) throttle = 1;
                if (Boost.Contains(point)) useBoost = true;
            }
        }

        private static int Axis(KeyCode negative, KeyCode alternateNegative, KeyCode positive, KeyCode alternatePositive)
        {
            return (Input.GetKey(positive) || Input.GetKey(alternatePositive) ? 1 : 0)
                - (Input.GetKey(negative) || Input.GetKey(alternateNegative) ? 1 : 0);
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
                "Race three laps through a model train town. Grab teal batteries and beat the field.", body);

            if (GUI.Button(new Rect(x + 18, y + 163, width - 36, 45), "START GRAND PRIX", primary))
                StartRace(true);
            float half = (width - 44) / 2;
            if (GUI.Button(new Rect(x + 18, y + 215, half, 36), "TIME TRIAL", secondary))
                StartRace(false);
            if (GUI.Button(new Rect(x + 26 + half, y + 215, half, 36), sound.Enabled ? "SOUND ON" : "SOUND OFF", secondary))
                sound.Toggle();
            GUI.Label(new Rect(x + 19, y + 258, width - 38, 20), ShowTouch
                ? "Hold GO to drive. Hold BOOST with GO."
                : "WASD / arrows to drive  •  Space to boost", small);
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
            float width = ShowTouch ? 126 : 210;
            float height = ShowTouch ? 41 : 59;
            float bottom = Height - (ShowTouch ? 94 : 20);
            Rect meter = new Rect(Width / 2 - width / 2, bottom - height, width, height);
            Card(meter, boosting ? Amber : Mint);
            Caption(meter.x + 9, meter.y + 7, meter.width - 18, boosting ? "BOOSTING" : "BOOST");
            Fill(new Rect(meter.x + 9, meter.y + 24, meter.width - 18, 7), Quiet);
            Fill(new Rect(meter.x + 9, meter.y + 24, (meter.width - 18) * boost, 7), boosting ? Amber : Mint);
            if (!ShowTouch)
                GUI.Label(new Rect(meter.x + 9, meter.y + 34, meter.width - 18, 19), "SPACE  boost    R  recover", small);

            bool warning = CrossingWarning();
            string banner = Time.unscaledTime < noticeUntil ? notice : warning ? "TRAIN APPROACHING" : "";
            if (banner.Length != 0)
            {
                Rect alert = new Rect(Width / 2 - 114, 115, 228, 35);
                Card(alert, warning ? Amber : Mint);
                Caption(alert.x + 11, alert.y + 9, alert.width - 22, banner);
            }

            if (!ShowTouch)
                return;
            PadButton(Left, "LEFT", Quiet);
            PadButton(Right, "RIGHT", Quiet);
            PadButton(Reverse, "BRAKE", Quiet);
            PadButton(Boost, "BOOST", boosting ? new Color(.36f, .27f, .12f) : Quiet);
            PadButton(Gas, "GO", new Color(.09f, .31f, .26f, .96f));
        }

        private void DrawPause()
        {
            Rect card = Center(312, 200);
            Card(card, Amber);
            Caption(card.x + 17, card.y + 14, card.width - 34, "RACE PAUSED");
            GUI.Label(new Rect(card.x + 16, card.y + 33, card.width - 32, 57), "Pit stop", display);
            if (GUI.Button(new Rect(card.x + 17, card.y + 97, card.width - 34, 40), "RESUME", primary))
                TogglePause();
            float half = (card.width - 42) / 2;
            if (GUI.Button(new Rect(card.x + 17, card.y + 145, half, 34), "RECOVER", secondary))
            {
                if (pausedState == RaceState.Racing && recovery <= 0)
                    ResetCar(false);
                TogglePause();
            }
            if (GUI.Button(new Rect(card.x + 25 + half, card.y + 145, half, 34), "MAIN MENU", secondary))
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

        private void PadButton(Rect rect, string text, Color color)
        {
            Fill(rect, color);
            Fill(new Rect(rect.x, rect.y, rect.width, 2), Mint);
            GUI.Label(rect, text, padText);
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
            padText = Text(14, Ivory, true);
            padText.alignment = TextAnchor.MiddleCenter;
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

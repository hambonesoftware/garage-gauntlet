using UnityEngine;

namespace GarageGauntlet
{
    internal sealed partial class RailwayTown
    {
        private void DrawSpringfield(RaceCourse course)
        {
            Material stone = Surface("brick", new Color(.89f, .80f, .66f), .12f);
            Material brick = Surface("brick", new Color(.79f, .49f, .37f), .10f);
            Material asphalt = ModelFlock(new Color(.30f, .30f, .28f), Vector2.one);
            Material roof = Surface("timber", new Color(.41f, .46f, .47f), .15f);
            Material glass = Paint(.20f, .31f, .36f);
            Material lamp = Glow(1, .80f, .48f);
            Material park = ModelFlock(SpringfieldPlan.Flock(course.Route) * 1.22f, Vector2.one);

            DrawSpringfieldFrontage(course, stone, asphalt, roof, lamp);
            switch (course.Route)
            {
                case RaceCourse.Layout.SpringfieldIllinois:
                    DrawCapitolSquare(stone, roof, glass, park);
                    SpringfieldRoadLink(course, new Vector2(-9.9f, -21), new Vector2(-9.9f, -2.5f), asphalt, stone);
                    SpringfieldRoadLink(course, new Vector2(11, -21), new Vector2(11, -2.5f), asphalt, stone);
                    SpringfieldStreet(-11, -2.5f, 33, asphalt, stone);
                    SpringfieldStreet(14, 11.3f, 22, asphalt, stone);
                    SpringfieldHouses(course, 16, 14, 5, 0);
                    SpringfieldHouses(course, 14, 9, 3, 180);
                    DrawPrairieFields(16, 39, park);
                    SpringfieldRow(course, -21, -15, new[] { "BAKERY", "BOOKS", "PHARMACY", "MARKET" }, brick, roof, stone, glass);
                    SpringfieldRow(course, 13, -15, new[] { "HARDWARE", "CAFE", "PRINT SHOP", "BARBER" }, brick, roof, stone, glass);
                    SpringfieldStreet(-20, 5, 24, asphalt, stone);
                    SpringfieldHouses(course, -20, 8, 5, 0);
                    SpringfieldHouses(course, -20, 2.3f, 4, 180);
                    TownSign("LINCOLN HOME", new Vector3(-13, 1.15f, 6.4f), 2.8f, .29f, -1, stone);
                    DrawSpringfieldStation("UNION STATION", 24, 5, brick, stone, roof, glass);
                    SpringfieldGroves(course, false, new Vector3(25, 0, 20), new Vector3(-20, 0, 33), new Vector3(5, 0, 35));
                    break;
                case RaceCourse.Layout.SpringfieldMissouri:
                    DrawRouteSixtySix(brick, stone, roof, glass, lamp);
                    SpringfieldRoadLink(course, new Vector2(13.9f, -20), new Vector2(13.9f, -4.4f), asphalt, stone);
                    SpringfieldStreet(13.9f, -4.4f, 21, asphalt, stone);
                    SpringfieldRow(course, 19, -7.2f, new[] { "MOTOR LODGE", "MOTOR LODGE", "SERVICE" }, brick, roof, stone, glass);
                    SpringfieldStreet(-20, 15.0f, 29, asphalt, stone);
                    SpringfieldHouses(course, -18, 17.9f, 6, 0);
                    SpringfieldHouses(course, -22, -8.2f, 4, 180);
                    SpringfieldRow(course, -21, -15, new[] { "MOTOR INN", "AUTO PARTS", "GARAGE", "BARBECUE" }, brick, roof, stone, glass);
                    SpringfieldStreet(-22, 9, 35, asphalt, stone);
                    SpringfieldRow(course, -20, 12, new[] { "FEED", "COFFEE", "FRISCO HOTEL", "DEPOT CAFE", "SUPPLY", "LIVERY" }, brick, roof, stone, glass);
                    DrawSpringfieldStation("FRISCO FREIGHT", -11, 4.6f, brick, stone, roof, glass);
                    SpringfieldHouses(course, 15, 12.5f, 5, 0);
                    SpringfieldGroves(course, false, new Vector3(5, 0, 31), new Vector3(-20, 0, 34), new Vector3(37, 0, 30));
                    break;
                case RaceCourse.Layout.SpringfieldMassachusetts:
                    DrawArmory(course, brick, stone, roof, glass, park);
                    SpringfieldRoadLink(course, new Vector2(-2.7f, -21), new Vector2(-2.7f, -2.5f), asphalt, stone);
                    SpringfieldStreet(-2.7f, -2.5f, 24, asphalt, stone);
                    SpringfieldStreet(-11, 11.3f, 21, asphalt, stone);
                    SpringfieldRow(course, -12, 15.0f, new[] { "MILL OFFICE", "MACHINE SHOP", "PATTERN SHOP" }, brick, roof, stone, glass);
                    SpringfieldHouses(course, -22, -7.5f, 4, 180);
                    SpringfieldHouses(course, 1, 8.9f, 3, 180);
                    SpringfieldRow(course, -21, -15, new[] { "IRON WORKS", "TOOL SHOP", "CLOTHIER", "DRY GOODS" }, brick, roof, stone, glass);
                    SpringfieldRow(course, 2, -15, new[] { "BOOKS", "COFFEE", "BAKERY", "MARKET" }, brick, roof, stone, glass);
                    SpringfieldStreet(-22, 7, 26, asphalt, stone);
                    DrawSpringfieldStation("WATER SHOPS", -12, 4, brick, stone, roof, glass);
                    SpringfieldHouses(course, -22, 10, 5, 0);
                    SpringfieldWater("Connecticut River", 3.0f, park,
                        new Vector2(-31, 41), new Vector2(-18, 38), new Vector2(-8, 35), new Vector2(2, 37),
                        new Vector2(10, 43), new Vector2(16, 49.65f));
                    SpringfieldGroves(course, false, new Vector3(-20, 0, 25), new Vector3(29, 0, 30));
                    break;
                default:
                    Material cedar = Surface("timber", new Color(.75f, .53f, .35f), .20f);
                    DrawBoothKelly(cedar, stone, roof, glass);
                    SpringfieldRoadLink(course, new Vector2(-15, -20), new Vector2(-15, -5.5f), asphalt, stone);
                    SpringfieldStreet(-15, -5.5f, 13.4f, asphalt, stone);
                    SpringfieldHouses(course, -22, -8.7f, 3, 0);
                    SpringfieldStreet(-24, 13.2f, 28, asphalt, stone);
                    SpringfieldHouses(course, -21, 16, 6, 0);
                    SpringfieldHouses(course, 16, -8.2f, 4, 0);
                    SpringfieldStreet(-23, 7, 28, asphalt, stone);
                    SpringfieldHouses(course, -22, 10, 5, 0);
                    SpringfieldRow(course, 16, -14, new[] { "SAW SERVICE", "DINER", "HARDWARE" }, cedar, roof, stone, glass);
                    DrawSpringfieldStation("MILL DEPOT", -15, 4, cedar, stone, roof, glass);
                    SpringfieldWater("Millrace", 2.0f, park,
                        new Vector2(1, -6), new Vector2(4, -1), new Vector2(7, 5), new Vector2(12, 9),
                        new Vector2(17, 12), new Vector2(22, 17), new Vector2(27, 17));
                    SpringfieldGroves(course, true, new Vector3(-18, 0, 23), new Vector3(-6, 0, 36),
                        new Vector3(35, 0, 36), new Vector3(44, 0, 29), new Vector3(30, 0, 27), new Vector3(7, 0, 19));
                    break;
            }
        }

        private void DrawSpringfieldFrontage(RaceCourse course, Material stone, Material asphalt, Material posts, Material lamps)
        {
            for (int i = -5; i <= 25; i++)
            {
                Vector3 curb = course.Lane(i, -2.69f, out Vector3 forward);
                Quaternion turn = Quaternion.LookRotation(forward);
                Box("Highway sidewalk", curb + Vector3.up * .055f, new Vector3(.92f, .11f, 1.05f), stone, turn);
                if (i % 6 == 0)
                    StreetLamp(curb.x, curb.z, posts, lamps);
                if (i % 4 != 2)
                    continue;
                Vector3 stall = course.Lane(i, 2.64f, out forward);
                Box("Parallel parking bay", stall + Vector3.up * .024f, new Vector3(.89f, .048f, 3.4f), asphalt, turn);
                Box("Parking divider", stall + Vector3.up * .054f, new Vector3(.82f, .015f, .055f), stone, turn);
            }
        }

        private void DrawCapitolSquare(Material stone, Material roof, Material glass, Material park)
        {
            const float z = -10;
            Box("Capitol Square green", new Vector3(1, .04f, z), new Vector3(18, .09f, 13), park);
            Box("Limestone apron", new Vector3(1, .12f, z - 1), new Vector3(12, .24f, 8.5f), stone);
            SurfaceBox("Old State Capitol sandstone wings", new Vector3(1, 1.82f, z), new Vector3(9.6f, 3.25f, 5.8f),
                new Vector2(.72f, .66f), stone);
            Box("Capitol roof", new Vector3(1, 3.53f, z), new Vector3(9.8f, .26f, 6), roof);
            for (int side = -1; side <= 1; side += 2)
            for (int window = 1; window <= 3; window++)
            {
                float x = 1 + side * (1.6f + window * .83f);
                ShopWindow(x, 1.25f, z - 2.9f, -1, .48f, .66f, stone, glass);
                ShopWindow(x, 2.56f, z - 2.9f, -1, .48f, .66f, stone, glass);
            }
            Box("Capitol portico entablature", new Vector3(1, 3.39f, z - 3.15f), new Vector3(4.2f, .34f, 1.1f), stone);
            for (int i = 0; i < 6; i++)
                Box("Portico column", new Vector3(-.65f + i * .66f, 1.86f, z - 3.56f), new Vector3(.19f, 2.92f, .19f), stone);
            for (int step = 0; step < 4; step++)
                Box("Portico stair", new Vector3(1, .15f + step * .065f, z - 4.27f + step * .18f),
                    new Vector3(4.4f, .16f + step * .13f, .28f), stone);
            Box("Capitol cupola drum", new Vector3(1, 4.01f, z), new Vector3(2.9f, .94f, 2.9f), stone);
            Material copper = Paint(.44f, .31f, .23f);
            RoundStack("Old State Capitol dome", 1, z, 4.46f, new[] { 1.43f, 1.41f, 1.24f, .99f, .69f, .35f }, .20f, copper);
            Box("Capitol lantern", new Vector3(1, 5.94f, z), new Vector3(.42f, .57f, .42f), stone);
            TownSign("OLD STATE CAPITOL", new Vector3(1, 3.4f, z - 3.75f), 3.8f, .33f, -1, stone);
            for (int i = 0; i < 4; i++)
            {
                DrawFlockedTree(new Vector3(-6.3f, 0, -14 + i * 2.65f), 317 + i);
                DrawFlockedTree(new Vector3(8.6f, 0, -14 + i * 2.65f), 337 + i);
            }
        }

        private void DrawRouteSixtySix(Material brick, Material stone, Material roof, Material glass, Material lamp)
        {
            Material red = Paint(.73f, .19f, .14f);
            Material cream = Paint(.91f, .78f, .55f);
            Material aqua = Paint(.33f, .66f, .59f);
            SpringfieldBuilding(7, -15, 5.6f, 4.2f, 4.4f, brick, roof, stone, glass, 5);
            Box("Gillioz marquee", new Vector3(7, 1.58f, -17.76f), new Vector3(5.8f, .33f, 1.55f), red);
            TownSign("GILLIOZ THEATRE", new Vector3(7, 1.71f, -18.56f), 4.9f, .37f, -1, cream);
            Box("Gillioz blade sign", new Vector3(9.22f, 3.86f, -17.48f), new Vector3(.68f, 3.6f, .51f), red);
            for (int i = 0; i < 7; i++)
                Box("Blade sign lamp", new Vector3(9.22f, 2.42f + i * .48f, -17.76f), new Vector3(.32f, .25f, .055f), lamp);
            for (int i = 0; i < 13; i++)
                Box("Marquee lamps", new Vector3(4.44f + .43f * i, 1.43f, -18.51f), new Vector3(.11f, .06f, .11f), lamp);
            SpringfieldBuilding(22, -13, 8, 2.5f, 3.5f, cream, aqua, stone, glass, 4);
            Box("Roadside diner awning", new Vector3(22, 1.7f, -15.24f), new Vector3(8.2f, .18f, 1.1f), aqua);
            TownSign("ROUTE SIXTY SIX DINER", new Vector3(22, 2.37f, -14.83f), 5.4f, .47f, -1, cream);
            Box("Motel sign pole", new Vector3(29, 1.34f, -17), new Vector3(.15f, 2.68f, .15f), roof);
            TownSign("MOTOR COURT", new Vector3(29, 2.8f, -17), 3.5f, .72f, -1, aqua);
        }

        private void DrawArmory(RaceCourse course, Material brick, Material stone, Material roof, Material glass, Material park)
        {
            const float x = 19, z = 19;
            float y = course.GroundHeight(x, z);
            Box("Armory retaining plinth", new Vector3(x, y / 2, z), new Vector3(17, y, 10.8f), stone);
            Box("Armory parade ground", new Vector3(x, y + .07f, z), new Vector3(17.1f, .15f, 10.9f), park);
            SpringfieldBuilding(x, z + 1.2f, 14.2f, 3.25f, 5.3f, brick, roof, stone, glass, 9, y + .15f);
            SpringfieldBuilding(x, z + .65f, 3.2f, 5.1f, 4.6f, brick, roof, stone, glass, 3, y + .15f);
            Box("Arsenal tower cupola", new Vector3(x, y + 5.65f, z + .65f), new Vector3(1.52f, .79f, 1.52f), stone);
            Box("Arsenal cupola roof", new Vector3(x, y + 6.12f, z + .65f), new Vector3(1.74f, .22f, 1.74f), roof);
            Box("Flagstaff", new Vector3(x + 5.6f, y + 1.88f, z - 4.0f), new Vector3(.055f, 3.6f, .055f), stone);
            Box("Flag", new Vector3(x + 6.07f, y + 3.44f, z - 4.0f), new Vector3(.96f, .47f, .055f), Paint(.70f, .22f, .20f));
            TownSign("SPRINGFIELD ARMORY", new Vector3(x, y + 1.09f, z - 4.12f), 4.2f, .43f, -1, stone);
        }

        private void DrawBoothKelly(Material cedar, Material stone, Material roof, Material glass)
        {
            SpringfieldBuilding(3, -12, 13.8f, 3.35f, 5.2f, cedar, roof, stone, glass, 9);
            SpringfieldBuilding(3, -8.1f, 8.5f, 1.85f, 3.0f, cedar, roof, stone, glass, 5);
            TownSign("BOOTH KELLY MILL", new Vector3(3, 2.74f, -14.69f), 6.0f, .51f, -1, stone);
            Box("Mill boiler stack", new Vector3(-5.0f, 2.52f, -10.1f), new Vector3(.64f, 5.04f, .64f), roof);
            Box("Mill chimney cap", new Vector3(-5.0f, 5.03f, -10.1f), new Vector3(.82f, .21f, .82f), roof);
            Material bark = Paint(.37f, .25f, .17f);
            Material cut = Paint(.79f, .60f, .39f);
            for (int stack = 0; stack < 3; stack++)
            for (int layer = 0; layer < 3; layer++)
            for (int log = 0; log < 4 - layer; log++)
            {
                float x = -12.0f + stack * 3.15f + (log + layer * .5f) * .34f;
                float y = .18f + layer * .29f;
                Box("Stacked lumber", new Vector3(x, y, -14.3f), new Vector3(.31f, .28f, 2.05f), bark);
                Box("Fresh-cut lumber ends", new Vector3(x, y, -15.34f), new Vector3(.25f, .21f, .043f), cut);
            }
        }

        private void DrawSpringfieldStation(string name, float x, float z, Material wall, Material stone, Material roof, Material glass)
        {
            SpringfieldBuilding(x, z, 6.8f, 2.55f, 2.95f, wall, roof, stone, glass, 5);
            Box("Station platform", new Vector3(x, .09f, z - 2.44f), new Vector3(8.5f, .18f, 1.7f), stone);
            Box("Station canopy", new Vector3(x, 1.47f, z - 2.23f), new Vector3(7.1f, .18f, 1.7f), roof);
            for (int i = 0; i < 4; i++)
                Box("Station canopy post", new Vector3(x - 2.87f + i * 1.91f, .75f, z - 2.85f),
                    new Vector3(.12f, 1.46f, .12f), roof);
            TownSign(name, new Vector3(x, 2.23f, z - 1.54f), 4.8f, .38f, -1, stone);
        }

        private void SpringfieldBuilding(float x, float z, float width, float height, float depth,
            Material wall, Material roof, Material stone, Material glass, int bays, float baseY = 0)
        {
            float front = z - depth / 2;
            SurfaceBox("Molded miniature facade", new Vector3(x, baseY + height / 2, z),
                new Vector3(width, height, depth), new Vector2(.62f, .64f), wall);
            Box("Roof cap", new Vector3(x, baseY + height + .065f, z), new Vector3(width + .2f, .15f, depth + .22f), roof);
            Box("Facade cornice", new Vector3(x, baseY + height - .15f, front), new Vector3(width + .1f, .13f, .19f), stone);
            for (int i = 0; i < bays; i++)
            {
                float bay = x + width * ((i + .5f) / bays - .5f);
                ShopWindow(bay, baseY + height * .32f, front, -1, width / bays * .52f, height * .33f, stone, glass);
                if (height > 2.7f)
                    ShopWindow(bay, baseY + height * .71f, front, -1, width / bays * .42f, height * .22f, stone, glass);
            }
        }

        private void SpringfieldRow(RaceCourse course, float firstX, float z, string[] signs, Material wall, Material roof,
            Material stone, Material glass)
        {
            for (int i = 0; i < signs.Length; i++)
            {
                float x = firstX + i * 4.15f;
                if (!SpringfieldClear(course, x, z, 4.9f, 2.4f))
                    continue;
                float height = 2.25f + i % 3 * .42f;
                Box("Storefront sidewalk", new Vector3(x, .065f, z - 2.25f), new Vector3(4.18f, .13f, 1.25f), stone);
                SpringfieldBuilding(x, z, 3.85f, height, 3.1f, wall, roof, stone, glass, 3);
                TownSign(signs[i], new Vector3(x, 1.45f, z - 1.62f), 3.45f, .34f, -1, stone);
            }
        }

        private void SpringfieldHouses(RaceCourse course, float firstX, float z, int houses, int facing)
        {
            Color[] walls = { new Color(.88f, .72f, .46f), new Color(.71f, .78f, .73f), new Color(.83f, .60f, .48f) };
            for (int i = 0; i < houses; i++)
            {
                float x = firstX + i * 4.25f;
                if (SpringfieldClear(course, x, z, 4.1f, 2.2f))
                    Craftsman(x, z, facing, walls[i % walls.Length]);
            }
        }

        private void DrawPrairieFields(float x, float z, Material verge)
        {
            Material soil = Surface("ballast", new Color(.65f, .52f, .36f), .06f);
            Material crop = Paint(.43f, .45f, .22f);
            for (int field = 0; field < 2; field++)
            {
                float fieldX = x + field * 15;
                Box("Prairie field boundary", new Vector3(fieldX, .022f, z), new Vector3(13.2f, .06f, 5.2f), verge);
                Box("Plowed prairie field", new Vector3(fieldX, .043f, z), new Vector3(12.2f, .037f, 4.6f), soil);
                for (int row = 0; row < 9; row++)
                    Box("Raised crop row", new Vector3(fieldX, .070f, z + (row - 4) * .48f),
                        new Vector3(11.8f, .036f, .16f), crop);
            }
        }

        private void SpringfieldRoadLink(RaceCourse course, Vector2 start, Vector2 end, Material road, Material stone)
        {
            float length = Vector2.Distance(start, end);
            int count = Mathf.CeilToInt(length);
            Vector3 direction = RaceCourse.Point(end.x - start.x, end.y - start.y).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);
            Vector3 across = rotation * Vector3.right;
            for (int i = 0; i < count; i++)
            {
                Vector2 point = Vector2.Lerp(start, end, (i + .5f) / count);
                if (!SpringfieldClear(course, point.x, point.y, 2.35f, .85f))
                    continue;
                Vector3 center = RaceCourse.Point(point.x, point.y);
                Box("Town street connection", center + Vector3.up * .023f, new Vector3(1.75f, .047f, length / count + .02f), road, rotation);
                for (int side = -1; side <= 1; side += 2)
                    Box("Town sidewalk connection", center + across * side * 1.08f + Vector3.up * .040f,
                        new Vector3(.42f, .071f, length / count + .02f), stone, rotation);
            }
        }

        private void SpringfieldStreet(float x, float z, float length, Material asphalt, Material stone)
        {
            Box("Local town street", new Vector3(x + length / 2, .022f, z), new Vector3(length, .047f, 2.0f), asphalt);
            for (int side = -1; side <= 1; side += 2)
                Box("Local sidewalk", new Vector3(x + length / 2, .041f, z + side * 1.27f),
                    new Vector3(length, .071f, .53f), stone);
        }

        private void SpringfieldGroves(RaceCourse course, bool firs, params Vector3[] groves)
        {
            Material needles = Paint(.19f, .32f, .20f);
            Material tips = Paint(.24f, .38f, .22f);
            Material wood = Paint(.32f, .22f, .16f);
            int index = 0;
            foreach (Vector3 grove in groves)
            for (int i = 0; i < 17; i++)
            {
                float angle = i * 2.39996f;
                float radius = Mathf.Sqrt(i) * 1.55f;
                float x = grove.x + Mathf.Cos(angle) * radius;
                float z = grove.z + Mathf.Sin(angle) * radius;
                if (!SpringfieldClear(course, x, z, 3.7f, 1.7f) || Mathf.Abs(x - 7) > 47 || Mathf.Abs(z - 10) > 40)
                    continue;
                Vector3 point = new Vector3(x, 0, z);
                if (!firs)
                {
                    DrawFlockedTree(point, 230 + index++, course);
                    continue;
                }
                float y = course.GroundHeight(x, z);
                float size = 1.9f + i % 5 * .23f;
                Box("Fir trunk", new Vector3(x, y + size * .35f, z), new Vector3(.13f, size * .75f, .13f), wood);
                for (int branch = 0; branch < 4; branch++)
                    RoundStack("Flocked fir crown", x, z, y + size * (.37f + branch * .155f),
                        new[] { size * (.24f - branch * .037f), .03f }, size * .31f, branch % 2 == 0 ? needles : tips, 9);
            }
        }

        private static bool SpringfieldClear(RaceCourse course, float x, float z, float road, float rail)
        {
            Vector3 point = RaceCourse.Point(x, z);
            course.Locate(point, out float roadGap);
            course.Railway.Path.Locate(point, out float railGap);
            return roadGap > road && railGap > rail;
        }

        private void SpringfieldWater(string name, float halfWidth, Material bank, params Vector2[] path)
        {
            Material water = Paint(.24f, .48f, .52f);
            water.SetFloat("_Glossiness", .86f);
            Material ripple = Paint(.40f, .61f, .62f);
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 at = new Vector3(path[i].x, .029f, path[i].y);
                FlatOval(name + " bank", at - Vector3.up * .009f, new Vector2(halfWidth + .38f, halfWidth + .38f), bank);
                FlatOval(name + " water", at, new Vector2(halfWidth, halfWidth), water);
                if (i == 0)
                    continue;
                Vector3 from = new Vector3(path[i - 1].x, at.y, path[i - 1].y);
                Vector3 center = (from + at) / 2;
                Quaternion turn = Quaternion.LookRotation(at - from);
                float length = Vector3.Distance(from, at);
                Box(name + " banks", center - Vector3.up * .011f, new Vector3(2 * halfWidth + .72f, .012f, length), bank, turn);
                Box(name + " water", center, new Vector3(2 * halfWidth, .012f, length), water, turn);
                Box(name + " reflection", center + Vector3.up * .009f,
                    new Vector3(.055f, .008f, length * .43f), ripple, turn);
            }
        }

        private void RoundStack(string name, float x, float z, float baseY, float[] radii, float rise, Material finish, int sides = 20)
        {
            var vertices = new Vector3[sides * radii.Length];
            var indices = new int[sides * (radii.Length - 1) * 6];
            int index = 0;
            for (int level = 0; level < radii.Length; level++)
            for (int side = 0; side < sides; side++)
            {
                float angle = side * 2 * Mathf.PI / sides;
                int a = level * sides + side, next = level * sides + (side + 1) % sides;
                vertices[a] = new Vector3(radii[level] * Mathf.Cos(angle), level * rise, radii[level] * Mathf.Sin(angle));
                if (level == radii.Length - 1)
                    continue;
                indices[index++] = a;
                indices[index++] = a + sides;
                indices[index++] = next;
                indices[index++] = next;
                indices[index++] = a + sides;
                indices[index++] = next + sides;
            }
            var mesh = Own(new Mesh { name = name, vertices = vertices, triangles = indices });
            mesh.RecalculateNormals();
            AddScenery(mesh, finish, new Vector3(x, baseY, z), Quaternion.identity, Vector3.one);
        }
    }
}

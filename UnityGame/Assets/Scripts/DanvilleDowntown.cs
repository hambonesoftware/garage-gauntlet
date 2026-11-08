using UnityEngine;

namespace GarageGauntlet
{
    internal sealed partial class RailwayTown
    {
        private void DrawDanvilleLandmarks()
        {
            Material brick = Surface("brick", new Color(.86f, .72f, .63f), .18f);
            Material stone = Paint(.79f, .74f, .62f);
            Material roof = Surface("roof", new Color(.49f, .51f, .51f), .17f);
            Material glass = Paint(.20f, .29f, .31f);
            glass.SetFloat("_Glossiness", .69f);
            Material enamel = Paint(.35f, .47f, .42f);
            Material signRed = Paint(.65f, .23f, .18f);
            Material brass = Paint(.75f, .56f, .29f);
            Material lights = Glow(.88f, .66f, .35f);

            DrawCourthouse(brick, stone, roof, glass);
            DrawMapleTheater(brick, stone, roof, glass, signRed, brass, lights);
            DrawMainStreetGreen(stone, enamel, glass, lights);
            DrawFillingStation(stone, roof, glass, enamel, signRed, lights);
        }

        private void DrawCourthouse(Material brick, Material stone, Material roof, Material glass)
        {
            const float x = -8.65f, z = -17.78f, front = -18.83f;
            Box("Courthouse public walk", new Vector3(x, .035f, -19.09f), new Vector3(4.55f, .10f, 1.23f), stone);
            Box("Courthouse stone foundation", new Vector3(x, .15f, z), new Vector3(4.38f, .29f, 2.22f), stone);
            SurfaceBox("Courthouse brick wings", new Vector3(x, 1.44f, z), new Vector3(4.22f, 2.30f, 2.10f),
                new Vector2(.62f, .62f), brick);
            Box("Courthouse roof coping", new Vector3(x, 2.66f, z), new Vector3(4.43f, .14f, 2.30f), stone);
            SurfaceBox("Courthouse slate roof", new Vector3(x, 2.76f, z), new Vector3(4.29f, .13f, 2.21f),
                new Vector2(.55f, .60f), roof);

            for (int side = -1; side <= 1; side += 2)
            {
                ShopWindow(x + side * 1.52f, .90f, front, -1, .55f, .76f, stone, glass);
                ShopWindow(x + side * 1.52f, 1.85f, front, -1, .55f, .66f, stone, glass);
                Box("Stone wing pilaster", new Vector3(x + side * 1.96f, 1.48f, front - .025f),
                    new Vector3(.15f, 2.26f, .13f), stone);
            }
            for (int step = 0; step < 3; step++)
                Box("Courthouse entry step", new Vector3(x, .055f + step * .06f, front - .57f + step * .11f),
                    new Vector3(2.25f - step * .12f, .11f, .31f), stone);
            Box("Courthouse paired doors", new Vector3(x, .93f, front - .042f), new Vector3(.78f, 1.29f, .10f), glass);
            Box("Courthouse door divider", new Vector3(x, .93f, front - .102f), new Vector3(.036f, 1.20f, .029f), stone);

            for (int column = 0; column < 4; column++)
            {
                float cx = x + (column - 1.5f) * .55f;
                Box("Portico column base", new Vector3(cx, .29f, front - .44f), new Vector3(.23f, .13f, .24f), stone);
                Box("Portico column", new Vector3(cx, 1.27f, front - .44f), new Vector3(.155f, 1.92f, .155f), stone);
                Box("Portico capital", new Vector3(cx, 2.23f, front - .44f), new Vector3(.24f, .14f, .24f), stone);
            }
            Box("Portico lintel", new Vector3(x, 2.40f, front - .33f), new Vector3(2.34f, .24f, .73f), stone);
            TownSign("COURTHOUSE", new Vector3(x, 2.40f, front - .71f), 1.48f, .22f, -1, stone);
            for (int side = -1; side <= 1; side += 2)
                Box("Pediment sloped molding", new Vector3(x + side * .57f, 2.70f, front - .33f),
                    new Vector3(1.27f, .14f, .74f), stone, Quaternion.Euler(0, 0, -side * 21));
        }

        private void DrawMapleTheater(Material brick, Material stone, Material roof, Material glass,
            Material red, Material brass, Material lights)
        {
            const float x = 15, z = -17.15f, front = -18.35f;
            Box("Theater sidewalk", new Vector3(x, .038f, -18.90f), new Vector3(4.27f, .087f, 1.22f), stone);
            SurfaceBox("Theater pressed brick facade", new Vector3(x, 1.51f, z), new Vector3(3.55f, 2.94f, 2.40f),
                new Vector2(.61f, .61f), brick);
            SurfaceBox("Theater rolled roof", new Vector3(x, 3.00f, z), new Vector3(3.63f, .12f, 2.48f),
                new Vector2(.61f, .61f), roof);
            Box("Theater cornice", new Vector3(x, 3.10f, front), new Vector3(3.73f, .18f, .19f), stone);
            Box("Theater entry glazing", new Vector3(x, .67f, front - .04f), new Vector3(1.54f, 1.20f, .09f), glass);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Theater entrance pier", new Vector3(x + side * 1.53f, 1.52f, front - .07f), new Vector3(.17f, 2.94f, .18f), stone);
                Box("Theater poster case", new Vector3(x + side * 1.12f, .82f, front - .06f), new Vector3(.43f, .62f, .11f), brass);
                Box("Theater poster", new Vector3(x + side * 1.12f, .82f, front - .124f), new Vector3(.30f, .46f, .025f), red);
                ShopWindow(x + side * .69f, 2.51f, front, -1, .56f, .53f, stone, glass);
            }
            Box("Theater marquee canopy", new Vector3(x, 1.43f, front - .39f), new Vector3(2.74f, .18f, .79f), red);
            Box("Theater marquee gold rim", new Vector3(x, 1.49f, front - .79f), new Vector3(2.77f, .075f, .075f), brass);
            TownSign("MAPLE THEATRE", new Vector3(x, 1.70f, front - .73f), 2.24f, .29f, -1, stone);
            for (int bulb = 0; bulb < 9; bulb++)
                Box("Marquee lamp", new Vector3(x + (bulb - 4) * .28f, 1.40f, front - .796f),
                    new Vector3(.065f, .050f, .036f), lights);
        }

        private void DrawMainStreetGreen(Material stone, Material enamel, Material dark, Material lamps)
        {
            const float x = 19.54f, z = -17.80f;
            Material grass = ModelFlock(new Color(.40f, .48f, .26f), Vector2.one);
            Material water = Paint(.29f, .49f, .49f);
            water.SetFloat("_Glossiness", .81f);
            SurfaceBox("Main Street civic green", new Vector3(x, .028f, z), new Vector3(4.98f, .086f, 2.52f),
                new Vector2(2.0f, 2.0f), grass);
            Box("Square crosswalk landing", new Vector3(x, .052f, -19.57f), new Vector3(4.95f, .106f, 1.04f), stone);
            Box("Square fountain walk", new Vector3(x, .065f, z - .62f), new Vector3(4.10f, .057f, .76f), stone);
            FlatOval("Square fountain stone base", new Vector3(x, .15f, z), new Vector2(.80f, .60f), stone);
            FlatOval("Square fountain water", new Vector3(x, .156f, z), new Vector2(.64f, .45f), water);
            Box("Square fountain pedestal", new Vector3(x, .29f, z), new Vector3(.28f, .29f, .28f), stone);
            FlatOval("Square fountain upper bowl", new Vector3(x, .46f, z), new Vector2(.30f, .30f), stone);
            for (int side = -1; side <= 1; side += 2)
            {
                float bench = x + side * 1.55f;
                Box("Square painted bench seat", new Vector3(bench, .27f, z), new Vector3(1.02f, .078f, .30f), enamel);
                Box("Square painted bench back", new Vector3(bench, .41f, z + .13f), new Vector3(1.02f, .22f, .061f), enamel);
                Box("Square bench support", new Vector3(bench, .14f, z), new Vector3(.79f, .19f, .10f), dark);
                StreetLamp(x + side * 2.13f, -19.56f, dark, lamps);
            }
            TownSign("MAPLE SQUARE", new Vector3(x, .59f, z + .94f), 1.58f, .23f, -1, stone);
            Box("Square sign post", new Vector3(x, .30f, z + .96f), new Vector3(.07f, .61f, .07f), dark);
        }

        private void DrawFillingStation(Material stone, Material roof, Material glass, Material enamel, Material red, Material lamps)
        {
            const float x = 25, z = -17.25f;
            Box("Filling station foundation", new Vector3(x, .08f, z), new Vector3(3.34f, .15f, 2.19f), stone);
            Box("Filling station painted block walls", new Vector3(x, .81f, z), new Vector3(3.26f, 1.42f, 2.08f), enamel);
            SurfaceBox("Filling station flat roof", new Vector3(x, 1.56f, z), new Vector3(3.44f, .13f, 2.24f), new Vector2(.71f, .71f), roof);
            ShopWindow(x - .77f, .72f, z - 1.04f, -1, 1.00f, .65f, stone, glass);
            Box("Filling station service bay door", new Vector3(x + .70f, .59f, z - 1.09f), new Vector3(1.16f, .98f, .10f), stone);
            for (int panel = 0; panel < 5; panel++)
                Box("Service bay door seam", new Vector3(x + .70f, .25f + panel * .17f, z - 1.145f),
                    new Vector3(1.10f, .018f, .019f), enamel);
            Box("Filling station canopy", new Vector3(x, 1.24f, -18.75f), new Vector3(3.12f, .16f, 1.36f), stone);
            TownSign("MAPLE OIL", new Vector3(x, 1.27f, -19.44f), 1.59f, .22f, -1, stone);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Filling station canopy post", new Vector3(x + side * 1.41f, .62f, -19.16f), new Vector3(.10f, 1.19f, .10f), enamel);
                Box("Painted vintage fuel pump", new Vector3(x + side * .42f, .36f, -18.84f), new Vector3(.23f, .58f, .23f), red);
                Box("Fuel pump meter", new Vector3(x + side * .42f, .54f, -18.97f), new Vector3(.135f, .14f, .035f), stone);
                Box("Fuel pump globe", new Vector3(x + side * .42f, .69f, -18.84f), new Vector3(.16f, .14f, .16f), lamps);
            }
        }
    }
}

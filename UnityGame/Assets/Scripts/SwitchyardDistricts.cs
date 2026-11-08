using UnityEngine;

namespace GarageGauntlet
{
    internal sealed partial class RailwayTown
    {
        private Material townLettering;

        private void DrawMainStreet()
        {
            Material brick = Surface("brick", new Color(.93f, .79f, .68f), .12f);
            Material cream = Paint(.87f, .81f, .67f);
            Material pavement = Paint(.50f, .47f, .40f);
            Material roof = Paint(.24f, .26f, .27f);
            Material glass = Paint(.18f, .32f, .36f);
            glass.SetFloat("_Glossiness", .82f);
            Material lit = Glow(.82f, .58f, .32f);
            Material green = Paint(.22f, .43f, .35f);
            Material red = Paint(.67f, .27f, .21f);
            Material blue = Paint(.26f, .41f, .57f);
            Material plaster = Paint(.77f, .66f, .47f);
            Material tile = Surface("roof", new Color(.70f, .65f, .61f), .25f);
            Material stucco = Surface("brick", new Color(.65f, .72f, .72f), .22f);
            brick.SetFloat("_Glossiness", .24f);
            string[] names = { "BAKERY", "MODEL SHOP", "POST OFFICE", "DINER", "BOOKS", "HARDWARE", "MARKET", "CAFE" };
            Material[] awnings = { red, green, blue, red, blue, green, red, blue };
            float[] eaves = { 2.05f, 1.83f, 2.48f, 1.63f, 2.29f, 1.87f, 2.37f, 1.65f };

            for (int row = 0; row < 2; row++)
            {
                int face = -1;
                float z = row == 0 ? -17.75f : -12.15f;
                Box("Main Street sidewalk", new Vector3(4.25f, .013f, z + face * 1.14f),
                    new Vector3(15.25f, .07f, 1.32f), pavement);
                for (int i = 0; i < 4; i++)
                {
                    int shop = row * 4 + i;
                    float x = -1.2f + i * 3.65f;
                    float height = eaves[shop];
                    float front = z + face * 1.02f;
                    float back = z - face * 1.02f;
                    Vector3 center = new Vector3(x, height / 2 + .035f, z);
                    Vector3 walls = new Vector3(3.25f, height, 2.04f);
                    Material shell = shop == 2 || shop == 6 ? stucco : brick;
                    bool pitched = shop == 1 || shop == 4 || shop == 6;
                    if (shop == 3 || shop == 7)
                        Box(names[shop] + " plaster walls", center, walls, plaster);
                    else
                        SurfaceBox(names[shop] + " molded masonry", center, walls, new Vector2(.62f, .62f), shell);
                    Box("Molded foundation lip", new Vector3(x, .09f, z), new Vector3(3.34f, .17f, 2.12f), pavement);
                    DrawShopRoof(x, z, height + .035f, face, pitched, shop == 2 || shop == 0, shell, tile, roof, cream);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        float wx = x + side * .92f;
                        ShopWindow(wx, .56f, front, face, .99f, .72f, cream, shop % 2 == 0 ? lit : glass);
                        if (height > 1.75f)
                            ShopWindow(wx, height - .36f, front, face, .58f, .43f, cream, glass);
                        ShopWindow(wx, height - .47f, back, -face, .56f, .49f, cream, glass);
                    }
                    Box("Recessed shop door", new Vector3(x, .53f, front + face * .035f), new Vector3(.53f, .95f, .065f), awnings[shop]);
                    Box("Shop door glass", new Vector3(x, .66f, front + face * .077f), new Vector3(.34f, .46f, .018f), glass);
                    Box("Shop doorstep", new Vector3(x, .073f, front + face * .20f), new Vector3(.75f, .10f, .40f), cream);
                    Box("Rear molded service door", new Vector3(x, .53f, back - face * .044f), new Vector3(.65f, .98f, .095f), awnings[shop]);
                    Box("Rear threshold", new Vector3(x, .08f, back - face * .17f), new Vector3(.81f, .14f, .37f), cream);
                    Box("Gutter downpipe", new Vector3(x - 1.45f, height * .51f, back - face * .067f), new Vector3(.084f, height * .95f, .084f), roof);
                    TownSign(names[shop], new Vector3(x, 1.24f, front + face * .05f), 1.85f, .24f, face, cream);
                    for (int stripe = 0; stripe < 9; stripe++)
                    {
                        Box("Canvas shop awning", new Vector3(x + (stripe - 4) * .272f, 1.02f, front + face * .27f),
                            new Vector3(.273f, .055f, .56f), shop == 2 || stripe % 2 == 0 ? awnings[shop] : cream,
                            Quaternion.Euler(face * 13, 0, 0));
                    }
                }
                for (int i = 0; i < 4; i++)
                    StreetLamp(.63f + i * 3.65f, z + face * 1.62f, roof, lit);
            }

            TownSign("SWITCHYARD  •  MAIN ST", new Vector3(-4.12f, 1.06f, -18.15f), 2.32f, .32f, -1, cream);
            Box("Welcome sign post", new Vector3(-4.12f, .57f, -18.11f), new Vector3(.09f, 1.06f, .09f), roof);
        }

        private void DrawTownBlocks()
        {
            Material pavement = Surface("asphalt", new Color(.68f, .66f, .60f), .12f);
            Material sidewalk = Paint(.56f, .52f, .43f);
            Material poles = Paint(.18f, .21f, .21f);
            Material lamps = Glow(.85f, .65f, .37f);
            Color[] walls = { new Color(.89f, .78f, .65f), new Color(.78f, .88f, .89f), new Color(.88f, .83f, .73f) };

            SurfaceBox("Market Lane", new Vector3(4.26f, .009f, -15.1f), new Vector3(17.6f, .049f, 1.74f),
                new Vector2(2.6f, 1.7f), pavement);
            SurfaceBox("Station approach", new Vector3(5.5f, .009f, -7.6f), new Vector3(1.68f, .049f, 13.4f),
                new Vector2(1.5f, 2.2f), pavement);
            SurfaceBox("Town Hall walk", new Vector3(.7f, .016f, 8.78f), new Vector3(1.45f, .060f, 7.15f),
                new Vector2(1.25f, 1.3f), sidewalk);

            for (int block = 0; block < 2; block++)
            {
                float center = block == 0 ? -15.3f : 20.7f;
                float lane = block == 0 ? -14.1f : -10.55f;
                float spacing = block == 0 ? 2.92f : 4.15f;
                SurfaceBox("Residential lane", new Vector3(center, .009f, lane), new Vector3(13.6f, .049f, 1.80f),
                    new Vector2(2.55f, 1.5f), pavement);
                for (int side = -1; side <= 1; side += 2)
                {
                    Box("Residential sidewalk", new Vector3(center, .026f, lane + side * 1.1f), new Vector3(13.6f, .06f, .49f), sidewalk);
                    for (int lot = 0; lot < 3; lot++)
                    {
                        float x = center + (lot - 1) * 4.18f;
                        float z = lane + side * spacing;
                        Craftsman(x, z, side == 1 ? 0 : 180, walls[(block + lot + (side + 1) / 2) % walls.Length]);
                        Box("Front garden path", new Vector3(x, .018f, (z + lane + side * .84f) / 2),
                            new Vector3(.32f, .040f, spacing - .86f), sidewalk);
                    }
                    StreetLamp(center - 5.8f, lane + side * 1.22f, poles, lamps);
                    StreetLamp(center + 5.8f, lane + side * 1.22f, poles, lamps);
                }
            }

            int tree = 40;
            foreach (Vector3 position in new[]
            {
                RaceCourse.Point(-32, -13), RaceCourse.Point(-30, -8), RaceCourse.Point(-31, 5), RaceCourse.Point(-31, 12),
                RaceCourse.Point(-29, 17), RaceCourse.Point(-18, 20), RaceCourse.Point(-12, 19), RaceCourse.Point(5, 20),
                RaceCourse.Point(14, 19), RaceCourse.Point(23, 18), RaceCourse.Point(29, 13), RaceCourse.Point(30, 6),
                RaceCourse.Point(40, 26), RaceCourse.Point(36, 33), RaceCourse.Point(29, 36), RaceCourse.Point(19, 34),
                RaceCourse.Point(7, 34), RaceCourse.Point(-4, 36), RaceCourse.Point(-15, 34), RaceCourse.Point(-18, 31)
            })
                DrawFlockedTree(position, tree++);
        }

        private void DrawShopRoof(float x, float z, float eave, int face, bool pitched, bool stepped,
            Material walls, Material tile, Material roof, Material trim)
        {
            if (pitched)
            {
                const float run = 1.13f, rise = .53f;
                float pitch = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
                for (int side = -1; side <= 1; side += 2)
                {
                    SurfaceBox("Molded tile roof half", new Vector3(x, eave + rise / 2, z + side * run / 2),
                        new Vector3(3.47f, .095f, Mathf.Sqrt(run * run + rise * rise)), new Vector2(.52f, .55f), tile,
                        Quaternion.Euler(side * pitch, 0, 0));
                    Box("Roof eave gutter", new Vector3(x, eave + .012f, z + side * run), new Vector3(3.53f, .085f, .088f), roof);
                }
                DrawGables(x, z, 3.25f, z - 1.02f, z + 1.02f, eave, eave + rise, walls);
                Box("Clip-on ridge molding", new Vector3(x, eave + rise + .042f, z), new Vector3(3.49f, .072f, .135f), roof);
                Box("Chimney cap", new Vector3(x + .90f, eave + .66f, z - face * .44f), new Vector3(.39f, .11f, .39f), trim);
                SurfaceBox("Molded chimney", new Vector3(x + .90f, eave + .37f, z - face * .44f), new Vector3(.30f, .54f, .30f),
                    new Vector2(.55f, .55f), walls);
                return;
            }

            Box("Inset roof panel", new Vector3(x, eave + .042f, z), new Vector3(3.26f, .11f, 2.04f), roof);
            Box("Front parapet", new Vector3(x, eave + .11f, z + face * 1.02f), new Vector3(3.36f, .28f, .13f), trim);
            Box("Rear coping", new Vector3(x, eave + .09f, z - face * 1.02f), new Vector3(3.36f, .12f, .13f), trim);
            for (int side = -1; side <= 1; side += 2)
                Box("Side coping", new Vector3(x + side * 1.60f, eave + .09f, z), new Vector3(.13f, .12f, 2.03f), trim);
            if (stepped)
            {
                Box("Raised parapet course", new Vector3(x, eave + .27f, z + face * 1.02f), new Vector3(2.23f, .22f, .15f), trim);
                Box("Parapet crest", new Vector3(x, eave + .40f, z + face * 1.02f), new Vector3(1.31f, .11f, .18f), trim);
            }
            Box("Vent housing", new Vector3(x + .8f, eave + .16f, z - face * .42f), new Vector3(.38f, .27f, .34f), roof);
        }

        private void DrawTownSquare()
        {
            Material stone = Paint(.68f, .63f, .52f);
            Material paving = Surface("ballast", new Color(.77f, .67f, .56f), .06f);
            Material brick = Surface("brick", new Color(.95f, .84f, .71f), .12f);
            Material slate = Paint(.23f, .32f, .36f);
            Material pool = Paint(.24f, .61f, .65f);
            Material dark = Paint(.19f, .26f, .26f);
            Material flowers = Paint(.85f, .45f, .26f);
            Material leaves = Paint(.39f, .53f, .27f);
            Material lamp = Glow(.91f, .67f, .37f);
            SurfaceBox("Town square paving", new Vector3(.70f, .016f, 3.72f), new Vector3(8.1f, .069f, 3.25f),
                new Vector2(.82f, .82f), paving);
            FlatOval("Fountain surround", new Vector3(.8f, .14f, 4.35f), new Vector2(1.35f, 1.02f), stone);
            FlatOval("Fountain basin", new Vector3(.8f, .154f, 4.35f), new Vector2(1.13f, .80f), pool);
            Box("Fountain pedestal", new Vector3(.8f, .31f, 4.35f), new Vector3(.33f, .34f, .33f), stone);
            FlatOval("Fountain tier", new Vector3(.8f, .51f, 4.35f), new Vector2(.43f, .43f), stone);
            Box("Fountain finial", new Vector3(.8f, .65f, 4.35f), new Vector3(.13f, .29f, .13f), stone);

            for (int side = -1; side <= 1; side += 2)
            {
                float x = .8f + side * 2.92f;
                Box("Planter", new Vector3(x, .14f, 5.04f), new Vector3(1.33f, .21f, .51f), stone);
                for (int i = 0; i < 5; i++)
                {
                    float plantX = x + (i - 2) * .22f;
                    Box("Planter foliage", new Vector3(plantX, .28f, 5.04f), new Vector3(.22f, .17f, .27f), leaves);
                    Box("Miniature flower", new Vector3(plantX, .38f, 5.03f), new Vector3(.095f, .066f, .095f), flowers);
                }
                Box("Park bench seat", new Vector3(x, .26f, 3.95f), new Vector3(1.05f, .08f, .34f), slate);
                Box("Park bench back", new Vector3(x, .44f, 4.08f), new Vector3(1.05f, .24f, .07f), slate);
                for (int i = -1; i <= 1; i += 2)
                    Box("Park bench leg", new Vector3(x + i * .36f, .14f, 3.95f), new Vector3(.09f, .23f, .23f), dark);
                StreetLamp(x, 2.73f, dark, lamp);
            }

            const float hallX = .7f, hallZ = 14.1f;
            const float facade = hallZ - 1.24f;
            const float tower = facade + .72f;
            Box("Town hall steps", new Vector3(hallX, .09f, facade - .31f), new Vector3(3.95f, .20f, 1.14f), stone);
            SurfaceBox("Town hall masonry", new Vector3(hallX, 1.18f, hallZ), new Vector3(4.2f, 2.19f, 2.48f), new Vector2(.68f, .68f), brick);
            Box("Town hall cornice", new Vector3(hallX, 2.29f, hallZ), new Vector3(4.35f, .15f, 2.64f), stone);
            Box("Town hall slate roof", new Vector3(hallX, 2.39f, hallZ), new Vector3(4.27f, .14f, 2.56f), slate);
            SurfaceBox("Town clock tower", new Vector3(hallX, 2.75f, tower), new Vector3(1.32f, 2.50f, 1.23f), new Vector2(.68f, .68f), brick);
            for (int side = -1; side <= 1; side += 2)
            {
                ShopWindow(hallX + side * 1.30f, .87f, facade, -1, .76f, .91f, stone, dark);
                ShopWindow(hallX + side * 1.30f, 1.77f, facade, -1, .67f, .59f, stone, pool);
                Box("Clock tower pitched cap", new Vector3(hallX, 4.18f, tower + side * .37f),
                    new Vector3(1.62f, .11f, .91f), slate, Quaternion.Euler(side * 33, 0, 0));
            }
            Box("Town hall doors", new Vector3(hallX, .80f, facade - .04f), new Vector3(.80f, 1.28f, .10f), dark);
            TownSign("TOWN HALL", new Vector3(hallX, 1.68f, facade - .07f), 1.43f, .27f, -1, stone);
            FlatOval("Clock face", new Vector3(hallX, 3.50f, tower - .635f), new Vector2(.45f, .45f), stone, Quaternion.Euler(-90, 0, 0));
            Box("Clock minute hand", new Vector3(hallX + .11f, 3.55f, tower - .653f), new Vector3(.25f, .04f, .029f), dark, Quaternion.Euler(0, 0, 26));
            Box("Clock hour hand", new Vector3(hallX - .06f, 3.56f, tower - .654f), new Vector3(.041f, .18f, .029f), dark, Quaternion.Euler(0, 0, 46));
        }

        private void DrawMillPond()
        {
            const float x = 12.9f, z = 5.56f;
            Material water = Paint(.14f, .43f, .50f);
            water.SetFloat("_Glossiness", .88f);
            Material shore = Paint(.52f, .59f, .31f);
            Material ripple = Paint(.25f, .49f, .51f);
            Material plank = Surface("timber", new Color(.85f, .70f, .50f), .14f);
            Material posts = Paint(.43f, .31f, .20f);
            Material reed = Paint(.44f, .57f, .27f);
            Material trim = Paint(.81f, .73f, .55f);
            FlatOval("Mill Pond grassy bank", new Vector3(x, .021f, z), new Vector2(4.47f, 2.87f), shore);
            FlatOval("Mill Pond water", new Vector3(x, .025f, z), new Vector2(4.03f, 2.49f), water);
            for (int i = 0; i < 9; i++)
            {
                float wx = x + (i % 3 - 1) * 1.61f + (i / 3) * .21f;
                Box("Painted pond ripple", new Vector3(wx, .028f, z + (i / 3 - 1) * .81f), new Vector3(.63f, .005f, .034f), ripple);
            }
            SurfaceBox("Mill Pond boardwalk", new Vector3(x + 1.1f, .13f, z + 1.90f), new Vector3(4.40f, .17f, .79f),
                new Vector2(.7f, .48f), plank);
            SurfaceBox("Fishing pier", new Vector3(x + 1.86f, .13f, z + .93f), new Vector3(.72f, .17f, 1.20f),
                new Vector2(.45f, .48f), plank);
            for (int i = 0; i < 7; i++)
            {
                float wx = x - .94f + i * .66f;
                Box("Boardwalk post", new Vector3(wx, .23f, z + 2.19f), new Vector3(.09f, .46f, .09f), posts);
                for (int j = 0; j < 3; j++)
                    Box("Pond reed", new Vector3(wx + j * .095f, .17f, z - 2.14f + .055f * (i % 3)),
                        new Vector3(.035f, .36f + .11f * j, .032f), reed, Quaternion.Euler(8, 0, -12 + 13 * j));
            }
            Box("Boardwalk railing", new Vector3(x + 1.04f, .40f, z + 2.19f), new Vector3(4.02f, .063f, .075f), posts);
            House("building-type-m", 16.7f, 9.13f, 12, "variation-b");
            TownSign("MILL POND", new Vector3(8.9f, .83f, 4.18f), 1.32f, .25f, -1, trim);
            Box("Pond sign post", new Vector3(8.9f, .43f, 4.21f), new Vector3(.08f, .86f, .08f), posts);
        }

        private void DrawDepotYard()
        {
            Material gravel = Surface("ballast", new Color(.70f, .63f, .55f), .08f);
            Material brick = Surface("brick", new Color(.79f, .70f, .65f), .10f);
            Material planks = Surface("timber", new Color(.71f, .56f, .43f), .15f);
            Material timber = Paint(.36f, .27f, .21f);
            Material iron = Paint(.25f, .32f, .35f);
            Material rail = Paint(.61f, .60f, .55f);
            Material cream = Paint(.84f, .74f, .55f);
            const float x = -13.5f, z = -2.83f;
            SurfaceBox("Depot yard gravel", new Vector3(x, .01f, z - 1.10f), new Vector3(10.45f, .06f, 4.37f), new Vector2(1.1f, 1.1f), gravel);
            for (int i = 0; i < 20; i++)
                Box("Siding sleeper", new Vector3(x + (i - 9.5f) * .51f, .059f, z), new Vector3(.16f, .045f, 1.20f), timber);
            for (int side = -1; side <= 1; side += 2)
                Box("Siding rail", new Vector3(x, .108f, z + side * .43f), new Vector3(10.24f, .061f, .056f), rail);
            for (int i = 0; i < 2; i++)
                Model("train/train-carriage-wood", root, new Vector3(x + 2.01f - i * 3.25f, .13f, z),
                    new Vector3(0, 90, 0), new Vector3(.99f, 1.21f, 2.96f));
            Box("Siding buffer", new Vector3(x - 5.06f, .27f, z), new Vector3(.21f, .43f, 1.22f), iron);
            SurfaceBox("Goods shed brick walls", new Vector3(-13.65f, .96f, 2.35f), new Vector3(5.65f, 1.89f, 2.12f),
                new Vector2(.74f, .74f), brick);
            Box("Goods shed roof", new Vector3(-13.65f, 1.95f, 2.34f), new Vector3(5.88f, .17f, 2.47f), iron,
                Quaternion.Euler(-8, 0, 0));
            SurfaceBox("Goods loading platform", new Vector3(-13.65f, .16f, 1.09f), new Vector3(5.78f, .24f, .66f),
                new Vector2(.67f, .46f), planks);
            for (int i = 0; i < 3; i++)
            {
                float bay = -15.43f + i * 1.78f;
                SurfaceBox("Sliding goods door", new Vector3(bay, .88f, 1.267f), new Vector3(1.02f, 1.30f, .055f),
                    new Vector2(.53f, .53f), planks);
                Box("Goods door runner", new Vector3(bay, 1.48f, 1.23f), new Vector3(1.19f, .063f, .057f), iron);
                SurfaceBox("Freight crate", new Vector3(bay + .64f, .44f, 1.09f), new Vector3(.35f, .35f, .35f),
                    new Vector2(.36f, .36f), planks);
            }
            TownSign("FREIGHT DEPOT", new Vector3(-13.65f, 1.70f, 1.24f), 1.66f, .23f, -1, cream);

            const float towerX = -18.1f, towerZ = 2.85f;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int front = -1; front <= 1; front += 2)
                    Box("Water tower leg", new Vector3(towerX + .50f * side, .87f, towerZ + .50f * front), new Vector3(.11f, 1.73f, .11f), iron);
                Box("Water tower brace", new Vector3(towerX, .90f, towerZ + .50f * side), new Vector3(.073f, 1.72f, .067f), iron,
                    Quaternion.Euler(0, 0, side * 36));
            }
            for (int i = 0; i < 18; i++)
            {
                float angle = i * 20;
                Quaternion turn = Quaternion.Euler(0, angle, 0);
                Vector3 stave = new Vector3(towerX, 2.14f, towerZ) + turn * Vector3.forward * .71f;
                Box("Water tank stave", stave, new Vector3(.254f, .94f, .092f), timber, turn);
                for (int band = -1; band <= 1; band += 2)
                    Box("Water tank iron strap", stave + Vector3.up * (.31f * band) + turn * Vector3.forward * .049f,
                        new Vector3(.272f, .060f, .025f), iron, turn);
            }
            FlatOval("Water tank cap", new Vector3(towerX, 2.64f, towerZ), new Vector2(.79f, .79f), iron);
            FlatOval("Water tank bottom", new Vector3(towerX, 1.68f, towerZ), new Vector2(.77f, .77f), iron);
        }

        private void DrawCottageHill(RaceCourse course)
        {
            Material path = Surface("ballast", new Color(.73f, .65f, .55f), .08f);
            Material fence = Paint(.84f, .79f, .64f);
            Material soil = Paint(.39f, .28f, .19f);
            Material leaf = Paint(.39f, .53f, .26f);
            Color[] houseColors = { new Color(.91f, .76f, .64f), Color.white, new Color(.78f, .88f, .89f) };
            SurfaceBox("Cottage Hill lane", new Vector3(-12.35f, .013f, 8.10f), new Vector3(10.9f, .06f, 1.01f), new Vector2(.95f, .95f), path);
            for (int i = 0; i < 3; i++)
            {
                float x = -15.85f + i * 3.23f;
                Craftsman(x, 10.25f, i == 1 ? 8 : -5, houseColors[i]);
                House(i == 1 ? "building-type-g" : "building-type-a", -4.5f + 4.1f * i, -6.3f, 180,
                    i == 1 ? "variation-c" : "variation-b");
                Box("Kitchen garden soil", new Vector3(x - .86f, .013f, 9.00f), new Vector3(.95f, .064f, .60f), soil);
                for (int plant = 0; plant < 4; plant++)
                    Box("Kitchen garden greens", new Vector3(x - 1.19f + plant * .22f, .076f, 9.00f), new Vector3(.16f, .09f, .36f), leaf);
                for (int picket = 0; picket < 9; picket++)
                {
                    if (picket == 4 || picket == 5)
                        continue;
                    Box("Cottage fence picket", new Vector3(x - 1.42f + picket * .33f, .23f, 8.72f), new Vector3(.09f, .48f, .066f), fence);
                }
                for (int section = 0; section < 2; section++)
                    Box("Cottage fence rail", new Vector3(x - 1.02f + section * 2.17f, .24f, 8.72f),
                        new Vector3(section == 0 ? 1.12f : .76f, .075f, .082f), fence);
            }
            TownSign("COTTAGE HILL", new Vector3(-7.2f, .75f, 8.1f), 1.62f, .24f, -1, fence);
            Box("Cottage Hill sign post", new Vector3(-7.2f, .40f, 8.12f), new Vector3(.078f, .8f, .078f), soil);
            Vector2[] trees =
            {
                new Vector2(-26, 12), new Vector2(-25, -9), new Vector2(-18, 18.6f), new Vector2(-12, 19.2f),
                new Vector2(-7.7f, 16.8f), new Vector2(-4.9f, 18.5f), new Vector2(4.5f, 18.1f), new Vector2(7.8f, 18.8f),
                new Vector2(13, 19.4f), new Vector2(19, 19), new Vector2(24.2f, 17.7f), new Vector2(26.3f, 10.1f),
                new Vector2(26.4f, 5), new Vector2(28.4f, -4.7f), new Vector2(28.7f, -14.8f), new Vector2(17.3f, -17.9f),
                new Vector2(12.3f, -18.4f), new Vector2(-9.2f, -19.3f), new Vector2(-17.4f, -19), new Vector2(-24.7f, -15.3f),
                new Vector2(-18.2f, -7.1f), new Vector2(-13.8f, -9.7f), new Vector2(-10.2f, -7.6f), new Vector2(-7.2f, -7.3f),
                new Vector2(-19.4f, 6.7f), new Vector2(-8.5f, 5.6f), new Vector2(-5.1f, 6.1f), new Vector2(5.7f, 3.7f),
                new Vector2(6.7f, -3.15f), new Vector2(10.3f, -3.65f), new Vector2(14.1f, -4.6f), new Vector2(15.8f, -1.95f),
                new Vector2(17.7f, 4.2f), new Vector2(15.8f, 11.3f), new Vector2(12.7f, 9.5f), new Vector2(5.4f, 13.5f)
            };
            for (int i = 0; i < trees.Length; i++)
            {
                Vector3 position = new Vector3(trees[i].x, -.02f, trees[i].y);
                course.Locate(position, out float gap);
                if (gap < RaceCourse.HalfWidth + .83f)
                    continue;
                DrawFlockedTree(position, i);
            }
        }

        private void ShopWindow(float x, float y, float wall, int face, float width, float height, Material frame, Material glass)
        {
            Box("Window frame", new Vector3(x, y, wall + face * .019f), new Vector3(width, height, .057f), frame);
            Box("Recessed glazing", new Vector3(x, y, wall + face * .049f), new Vector3(width - .105f, height - .10f, .018f), glass);
            Box("Window mullion", new Vector3(x, y, wall + face * .061f), new Vector3(.039f, height - .06f, .019f), frame);
            Box("Window sill", new Vector3(x, y - height / 2, wall + face * .044f), new Vector3(width + .055f, .045f, .13f), frame);
        }

        private void StreetLamp(float x, float z, Material pole, Material glass)
        {
            Box("Street lamp base", new Vector3(x, .10f, z), new Vector3(.18f, .20f, .18f), pole);
            Box("Street lamp mast", new Vector3(x, .68f, z), new Vector3(.071f, 1.35f, .071f), pole);
            Box("Street lamp lantern", new Vector3(x, 1.37f, z), new Vector3(.19f, .21f, .19f), glass);
            Box("Street lamp cap", new Vector3(x, 1.50f, z), new Vector3(.24f, .062f, .24f), pole);
        }

        private void TownSign(string title, Vector3 center, float width, float height, int face, Material board)
        {
            Box(title + " sign board", center, new Vector3(width, height, .05f), board);
            Transform sign = Group(title + " lettering", root);
            sign.localPosition = center + Vector3.forward * (.029f * face);
            sign.localRotation = Quaternion.Euler(0, face > 0 ? 180 : 0, 0);
            TextMesh text = sign.gameObject.AddComponent<TextMesh>();
            text.text = title;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 48;
            text.fontStyle = FontStyle.Bold;
            text.characterSize = Mathf.Min(height * .20f, width / (title.Length * 3.1f));
            text.color = new Color(.15f, .18f, .19f);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.font = font;
            if (townLettering == null)
            {
                font.RequestCharactersInTexture("ABCDEFGHIJKLMNOPQRSTUVWXYZ •", 48, FontStyle.Bold);
                townLettering = new Material(Resources.Load<Shader>("TownLettering"));
            }
            townLettering.mainTexture = font.material.mainTexture;
            sign.GetComponent<MeshRenderer>().sharedMaterial = townLettering;
        }

        private void FlatOval(string name, Vector3 center, Vector2 radius, Material material, Quaternion? rotation = null)
        {
            const int segments = 40;
            var vertices = new Vector3[segments + 1];
            var triangles = new int[segments * 3];
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                vertices[i + 1] = new Vector3(radius.x * Mathf.Sin(angle), 0, radius.y * Mathf.Cos(angle));
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }
            var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            Transform surface = Group(name, root);
            surface.localPosition = center;
            surface.localRotation = rotation ?? Quaternion.identity;
            surface.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            surface.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }
    }
}

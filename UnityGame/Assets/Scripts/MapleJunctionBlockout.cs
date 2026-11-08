using UnityEngine;

namespace GarageGauntlet
{
    internal sealed partial class RailwayTown
    {
        private void DrawMapleTerrain(RaceCourse course, Material grass)
        {
            const int columns = 98, rows = 86;
            var vertices = new Vector3[(columns + 1) * (rows + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[columns * rows * 6];
            int triangle = 0;
            for (int row = 0; row <= rows; row++)
            for (int col = 0; col <= columns; col++)
            {
                float x = RaceCourse.BoardCenter.x + (2f * col / columns - 1) * (RaceCourse.BoardHalfWidth - .15f);
                float z = RaceCourse.BoardCenter.z + (2f * row / rows - 1) * (RaceCourse.BoardHalfDepth - .15f);
                int a = row * (columns + 1) + col;
                var point = new Vector3(x, MapleJunctionPlan.GroundHeight(x, z), z);
                if (point.y > .025f)
                {
                    float distance = course.Locate(point, out float gap);
                    if (gap < 3.5f)
                    {
                        float deck = course.Sample(distance, out _).y;
                        point.y = Mathf.Lerp(Mathf.Min(point.y, deck), point.y, Mathf.InverseLerp(2.5f, 3.5f, gap));
                    }
                }
                vertices[a] = point - Vector3.up * .017f;
                uv[a] = new Vector2(x, z) / 3.4f;
                if (row == rows || col == columns)
                    continue;
                int b = a + columns + 1;
                triangles[triangle++] = a;
                triangles[triangle++] = b;
                triangles[triangle++] = a + 1;
                triangles[triangle++] = a + 1;
                triangles[triangle++] = b;
                triangles[triangle++] = b + 1;
            }

            var mesh = new Mesh { name = "Maple Junction terrain grades", vertices = vertices, uv = uv, triangles = triangles };
            mesh.RecalculateNormals();
            Transform surface = Group(mesh.name, root);
            surface.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            surface.gameObject.AddComponent<MeshRenderer>().sharedMaterial = grass;
        }

        private void DrawMapleDistricts(RaceCourse course)
        {
            Material asphalt = Paint(.25f, .24f, .22f);
            Material gravel = Paint(.52f, .45f, .33f);
            Material walks = Paint(.67f, .60f, .48f);
            Material siding = Paint(.66f, .64f, .54f);
            Material roof = Paint(.29f, .34f, .35f);
            Material brick = Paint(.63f, .37f, .29f);
            Material wood = Paint(.44f, .30f, .22f);
            Material crops = Paint(.46f, .46f, .24f);
            Material soil = Paint(.41f, .34f, .23f);

            LayMapleStreet("Main Street", MapleJunctionPlan.MainStreet, 1.15f, asphalt);
            LayMapleStreet("Cottage Lane", MapleJunctionPlan.CottageLane, 1.38f, asphalt);
            LayMapleStreet("Freight access", MapleJunctionPlan.FreightLane, 1.48f, gravel);
            LayMapleStreet("Mill Pond trail", MapleJunctionPlan.MillWalk, .83f, walks);
            LayMapleStreet("Farm service road", MapleJunctionPlan.FarmLane, 1.48f, gravel);

            ReserveLot("Maple Theater", 15, -17.15f, new Vector3(3.5f, 2.25f, 2.4f), brick, roof, walks);
            ReserveLot("Service station", 25, -17.25f, new Vector3(3.3f, 1.48f, 2.15f), siding, roof, walks);
            Box("Service forecourt", new Vector3(24.9f, .020f, -18.55f), new Vector3(4.3f, .047f, 1.1f), walks);
            for (int stall = 0; stall < 7; stall++)
                Box("Parallel parking marking", new Vector3(12.4f + stall * 1.55f, .044f, -19.52f),
                    new Vector3(.055f, .009f, .32f), walks);

            ReserveLot("Hill cottage west", 18.4f, 14.0f, new Vector3(2.25f, 1.43f, 1.85f), siding, roof, walks);
            ReserveLot("Hill cottage bend", 24.3f, 13.5f, new Vector3(2.25f, 1.55f, 1.8f), brick, roof, walks);
            ReserveLot("Hill cottage crest", 25.0f, 17.4f, new Vector3(2.15f, 1.45f, 1.9f), siding, roof, walks);
            ReserveLot("Hill cottage north", 21.5f, 21.5f, new Vector3(2.3f, 1.5f, 1.8f), siding, roof, walks);
            LayMapleStreet("Hill driveway", new[] { new Vector2(22, 13), new Vector2(24.3f, 12.6f) }, .42f, gravel);
            LayMapleStreet("Crest driveway", new[] { new Vector2(21.3f, 18.2f), new Vector2(24.9f, 17.4f) }, .42f, gravel);

            ReserveLot("Switchyard warehouse", -21, 2, new Vector3(4.4f, 1.7f, 2.35f), brick, roof, gravel);
            ReserveLot("Lumber storage", -23.2f, -5.6f, new Vector3(2.2f, 1.08f, 2.6f), wood, roof, gravel);
            Box("Freight loading apron", new Vector3(-20.7f, .035f, .45f), new Vector3(4.7f, .10f, .8f), gravel);

            ReserveLot("Mill Pond picnic shelter", 8.1f, 10.8f, new Vector3(2.55f, .92f, 1.7f), wood, roof, walks);
            Box("Park drainage channel", new Vector3(9.53f, .015f, 9.4f), new Vector3(.59f, .045f, 2.5f), soil,
                Quaternion.Euler(0, -18, 0));
            Box("Park footbridge deck", new Vector3(9.53f, .14f, 9.4f), new Vector3(1.12f, .11f, 1.0f), wood);

            ReserveLot("Farmhouse", -7, 33.35f, new Vector3(2.4f, 1.6f, 2.0f), siding, roof, gravel);
            ReserveLot("Old barn", 5.8f, 34.65f, new Vector3(3.7f, 2.48f, 2.75f), brick, roof, gravel);
            ReserveLot("Grain elevator and silo pad", 28.5f, 35.5f, new Vector3(2.2f, 3.2f, 1.55f), siding, roof, gravel);
            Box("Plowed field", new Vector3(16.1f, .008f, 33.8f), new Vector3(12, .050f, 4.95f), soil);
            for (int row = 0; row < 8; row++)
                Box("Crop row", new Vector3(16.1f, .045f, 31.8f + row * .55f), new Vector3(11.4f, .068f, .18f), crops);

            int tree = 100;
            foreach (Vector3 point in new[]
            {
                RaceCourse.Point(-39.5f, 8), RaceCourse.Point(-40, 12), RaceCourse.Point(-40, 16), RaceCourse.Point(-38, 19),
                RaceCourse.Point(-33, 9), RaceCourse.Point(-32.8f, 4), RaceCourse.Point(-33, 15), RaceCourse.Point(-30, 11),
                RaceCourse.Point(7.8f, 11.9f), RaceCourse.Point(12, 12.5f), RaceCourse.Point(16, 13.5f), RaceCourse.Point(19, 6.7f),
                RaceCourse.Point(-4, 39), RaceCourse.Point(3.8f, 39.7f), RaceCourse.Point(11, 39.4f), RaceCourse.Point(19, 39.3f),
                RaceCourse.Point(-1, 47), RaceCourse.Point(8, 47.6f), RaceCourse.Point(15, 48), RaceCourse.Point(24, 47.2f)
            })
            {
                course.Locate(point, out float slotClearance);
                course.Railway.Path.Locate(point, out float railClearance);
                if (slotClearance > RaceCourse.HalfWidth + 1.1f && railClearance > 1.55f)
                    DrawFlockedTree(point, tree++);
            }
        }

        private void LayMapleStreet(string name, Vector2[] points, float width, Material material)
        {
            for (int i = 1; i < points.Length; i++)
            {
                Vector2 from = points[i - 1], to = points[i];
                int sections = Mathf.CeilToInt(Vector2.Distance(from, to) / 1.2f);
                Vector3 start = StreetPoint(from);
                for (int section = 1; section <= sections; section++)
                {
                    Vector3 end = StreetPoint(Vector2.Lerp(from, to, section / (float)sections));
                    Vector3 direction = end - start;
                    Box(name, (start + end) / 2, new Vector3(width, .055f, direction.magnitude + .20f),
                        material, Quaternion.LookRotation(direction));
                    start = end;
                }
            }
        }

        private static Vector3 StreetPoint(Vector2 point) =>
            new Vector3(point.x, MapleJunctionPlan.GroundHeight(point.x, point.y) + .025f, point.y);

        private void ReserveLot(string name, float x, float z, Vector3 size, Material shell, Material roof, Material baseMaterial)
        {
            float y = MapleJunctionPlan.GroundHeight(x, z);
            Box(name + " graded lot", new Vector3(x, y - .17f, z), new Vector3(size.x + .34f, .43f, size.z + .34f), baseMaterial);
            Box(name + " study volume", new Vector3(x, y + size.y / 2 + .035f, z), size, shell);
            Box(name + " roof line", new Vector3(x, y + size.y + .06f, z), new Vector3(size.x + .16f, .12f, size.z + .16f), roof);
        }
    }
}

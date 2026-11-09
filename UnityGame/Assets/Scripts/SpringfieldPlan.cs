using UnityEngine;
using static GarageGauntlet.RaceCourse;
using District = GarageGauntlet.MapleJunctionPlan.District;

namespace GarageGauntlet
{
    internal static class SpringfieldPlan
    {
        public static Vector3[] Road(Layout route)
        {
            switch (route)
            {
                case Layout.SpringfieldIllinois:
                    return new[]
                    {
                        Point(-2, -23), Point(12, -23), Point(27, -23), Point(36, -20),
                        Point(39, -13), Point(39, 3), Point(39, 16), Point(36, 24), Point(28, 28),
                        Point(13, 28), Point(10, 24), Point(9, 18), Point(3, 15), Point(-8, 15),
                        Point(-16, 19), Point(-26, 21), Point(-34, 20), Point(-38, 14), Point(-38, 1),
                        Point(-37, -14), Point(-32, -22), Point(-23, -25), Point(-12, -23)
                    };
                case Layout.SpringfieldMissouri:
                    return new[]
                    {
                        Point(1, -22), Point(18, -22), Point(31, -23), Point(40, -20),
                        Point(44, -11), Point(44, 4), Point(44, 15), Point(40, 21), Point(33, 22),
                        Point(22, 21), Point(15, 24), Point(12, 30), Point(5, 34), Point(-5, 33),
                        Point(-15, 28), Point(-26, 29), Point(-34, 26), Point(-37, 18), Point(-35, 8),
                        Point(-31, 2), Point(-32, -5), Point(-38, -13), Point(-34, -21),
                        Point(-22, -25), Point(-12, -23)
                    };
                case Layout.SpringfieldMassachusetts:
                    return new[]
                    {
                        Point(3, -23), Point(15, -23), Point(26, -21), Point(32, -15), Point(32, -7),
                        Point(31, 4), Point(31, 11), Point(35, 17), Point(40, 23), Point(37, 30),
                        Point(30, 35), Point(20, 35), Point(13, 30), Point(8, 25), Point(-2, 24),
                        Point(-11, 21), Point(-18, 15), Point(-26, 14), Point(-34, 15), Point(-38, 10),
                        Point(-38, -2), Point(-39, -10), Point(-36, -19), Point(-29, -23),
                        Point(-22, -25), Point(-12, -23)
                    };
                default:
                    return new[]
                    {
                        Point(2, -22), Point(13, -22), Point(23, -23), Point(32, -20), Point(37, -14),
                        Point(37, -7), Point(34, 2), Point(35, 9), Point(40, 15), Point(43, 24),
                        Point(40, 32), Point(32, 35), Point(24, 31), Point(18, 26), Point(8, 27),
                        Point(0, 33), Point(-9, 33), Point(-19, 29), Point(-26, 26), Point(-35, 23),
                        Point(-38, 15), Point(-35, 8), Point(-37, 0), Point(-35, -10),
                        Point(-36, -17), Point(-31, -22), Point(-22, -25), Point(-12, -23)
                    };
            }
        }

        public static District[] Districts(Layout route)
        {
            switch (route)
            {
                case Layout.SpringfieldIllinois:
                    return new[] { new District("OLD STATE CAPITOL", 1, -10), new District("LINCOLN HOME", -13, 8),
                        new District("UNION STATION", 24, 8) };
                case Layout.SpringfieldMissouri:
                    return new[] { new District("GILLIOZ THEATRE", 7, -15), new District("FRISCO YARD", -12, 5),
                        new District("ROUTE 66", 30, -3) };
                case Layout.SpringfieldMassachusetts:
                    return new[] { new District("SPRINGFIELD ARMORY", 22, 24), new District("CONNECTICUT RIVER", -6, 33),
                        new District("WATER SHOPS", -11, 7) };
                default:
                    return new[] { new District("BOOTH-KELLY MILL", 4, -11), new District("MILLRACE", 13, 13),
                        new District("FIR RIDGE", 39, 36) };
            }
        }

        public static Color Flock(Layout route)
        {
            switch (route)
            {
                case Layout.SpringfieldIllinois: return new Color(.44f, .49f, .28f);
                case Layout.SpringfieldMissouri: return new Color(.43f, .39f, .24f);
                case Layout.SpringfieldMassachusetts: return new Color(.35f, .41f, .29f);
                default: return new Color(.24f, .36f, .24f);
            }
        }

        public static float GroundHeight(Layout route, float x, float z)
        {
            switch (route)
            {
                case Layout.SpringfieldIllinois: return 0;
                case Layout.SpringfieldMissouri: return Rise(x, z, 1, 32, 19, 8, 1.25f);
                case Layout.SpringfieldMassachusetts: return Rise(x, z, 21, 23, 23, 15, 2.55f);
                default: return Rise(x, z, 39, 30, 13, 12, 2.3f) + Rise(x, z, -10, 35, 14, 7, 1.5f);
            }
        }

        private static float Rise(float x, float z, float centerX, float centerZ, float width, float depth, float height)
        {
            float u = (x - centerX) / width, v = (z - centerZ) / depth;
            float profile = Mathf.Max(0, 1 - u * u - v * v);
            return height * profile * profile;
        }
    }
}

using UnityEngine;

namespace GarageGauntlet
{
    internal static class MapleJunctionPlan
    {
        internal readonly struct District
        {
            public readonly string Name;
            public readonly Vector3 Marker;

            public District(string name, float x, float z)
            {
                Name = name;
                Marker = new Vector3(x, 2.1f, z);
            }
        }

        public static readonly District[] Districts =
        {
            new District("MAIN STREET", 4, -19),
            new District("COTTAGE HILL", 24, 16),
            new District("SWITCHYARD", -17, -2),
            new District("MILL POND", 13, 5),
            new District("OUTSKIRTS", 8, 35)
        };

        public static readonly Vector2[] MainStreet =
        {
            new Vector2(-8.5f, -14.1f), new Vector2(-7, -16.6f), new Vector2(-7, -19.15f),
            new Vector2(27, -19.15f), new Vector2(28.3f, -16.3f), new Vector2(28, -10.55f)
        };
        public static readonly Vector2[] CottageLane =
        {
            new Vector2(28, -10.55f), new Vector2(28, -4), new Vector2(27, 1), new Vector2(26, 5),
            new Vector2(24.8f, 8), new Vector2(23, 11), new Vector2(20.8f, 15), new Vector2(21.2f, 18.2f), new Vector2(24, 20)
        };
        public static readonly Vector2[] FreightLane =
        {
            new Vector2(-22, -14.1f), new Vector2(-22, -9), new Vector2(-21, -5.5f), new Vector2(-19.5f, -2), new Vector2(-18, 0)
        };
        public static readonly Vector2[] MillWalk =
        {
            new Vector2(5.2f, 4.4f), new Vector2(7.4f, 7), new Vector2(9.4f, 9.3f),
            new Vector2(12.5f, 10.6f), new Vector2(16, 11.25f), new Vector2(19.4f, 9.8f), new Vector2(24.8f, 8)
        };
        public static readonly Vector2[] FarmLane =
        {
            new Vector2(-22, 29), new Vector2(-13, 30), new Vector2(-3, 30), new Vector2(6, 30.8f),
            new Vector2(15, 30.3f), new Vector2(25, 31), new Vector2(32, 33.5f), new Vector2(37, 36)
        };

        public static float GroundHeight(float x, float z) =>
            Rise(x, z, -36, 13, 7, 15, 1.85f) + Rise(x, z, 24, 16, 12, 10, 1.6f)
            + Rise(x, z, 12, 38.7f, 22, 3.2f, 1.05f) + Rise(x, z, 12, 47.5f, 20, 3.2f, 1.25f);

        private static float Rise(float x, float z, float centerX, float centerZ, float radiusX, float radiusZ, float height)
        {
            float across = (x - centerX) / radiusX;
            float along = (z - centerZ) / radiusZ;
            float profile = Mathf.Max(0, 1 - across * across - along * along);
            return height * profile * profile;
        }
    }
}

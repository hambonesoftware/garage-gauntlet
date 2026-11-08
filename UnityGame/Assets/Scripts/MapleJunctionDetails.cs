using UnityEngine;

namespace GarageGauntlet
{
    // Small-scale town storytelling. All static geometry is added to the existing
    // scenery batches; no per-frame objects or expensive physics are introduced.
    internal sealed partial class RailwayTown
    {
        private void DrawMapleJunctionDetails()
        {
            DrawDowntownDetails();
            DrawResidentialDetails();
            DrawPondDetails();
            DrawFreightDetails();
        }

        private void DrawDowntownDetails()
        {
            Material steel = Paint(.24f, .28f, .30f);
            Material red = Paint(.67f, .24f, .17f);
            Material cream = Paint(.87f, .80f, .64f);
            Material asphalt = Paint(.29f, .31f, .31f);
            Material planter = Paint(.52f, .49f, .40f);
            Material leaves = Paint(.31f, .47f, .26f);
            // A lived-in commercial street: bins, bicycle racks and curb planters.
            for (int i = 0; i < 4; i++)
            {
                float x = -.3f + i * 3.65f;
                Box("Main Street planter", new Vector3(x, .17f, -19.18f),
                    new Vector3(.66f, .30f, .44f), planter);
                for (int p = 0; p < 3; p++)
                    Box("Main Street planted hedge", new Vector3(x + (p - 1) * .18f, .37f, -19.18f),
                        new Vector3(.22f, .22f, .31f), leaves);
            }
            for (int i = 0; i < 3; i++)
            {
                float x = 1.5f + i * 4.1f;
                Box("Downtown rubbish bin", new Vector3(x, .27f, -10.88f),
                    new Vector3(.33f, .52f, .34f), steel);
                Box("Bin lid", new Vector3(x, .55f, -10.88f),
                    new Vector3(.38f, .055f, .39f), asphalt);
            }
            Box("Downtown fire hydrant stem", new Vector3(-2.8f, .22f, -19.05f),
                new Vector3(.17f, .38f, .17f), red);
            Box("Downtown fire hydrant cap", new Vector3(-2.8f, .43f, -19.05f),
                new Vector3(.25f, .10f, .25f), red);
            Box("Downtown fire hydrant arms", new Vector3(-2.8f, .29f, -19.05f),
                new Vector3(.35f, .10f, .13f), red);
            // A removable miniature-town wayfinding board at the square.
            Box("Town directory board", new Vector3(4.18f, .85f, 5.55f),
                new Vector3(.90f, .66f, .09f), cream);
            Box("Town directory post", new Vector3(4.18f, .37f, 5.55f),
                new Vector3(.09f, .71f, .09f), steel);
        }

        private void DrawResidentialDetails()
        {
            Material gravel = Paint(.58f, .55f, .47f);
            Material fence = Paint(.81f, .78f, .68f);
            Material wood = Paint(.49f, .32f, .22f);
            Material green = Paint(.33f, .49f, .28f);
            // Each garden is deliberately different rather than cloned.
            for (int i = 0; i < 3; i++)
            {
                float x = -15.85f + i * 3.23f;
                float z = 11.95f;
                Box("Cottage garden stepping stone", new Vector3(x + .54f, .045f, z),
                    new Vector3(.48f, .06f, .39f), gravel);
                Box("Cottage garden shed", new Vector3(x - .58f, .49f, z + .40f),
                    new Vector3(.93f, .91f, .72f), wood);
                Box("Cottage garden shed roof", new Vector3(x - .58f, .99f, z + .40f),
                    new Vector3(1.08f, .13f, .84f), fence);
                for (int p = 0; p < 3 + i; p++)
                    Box("Cottage garden shrub", new Vector3(x + .45f + p * .16f, .17f, z + .55f),
                        new Vector3(.19f, .30f, .22f), green);
            }
        }

        private void DrawPondDetails()
        {
            Material timber = Paint(.44f, .31f, .21f);
            Material metal = Paint(.26f, .29f, .30f);
            Material cream = Paint(.84f, .78f, .61f);
            // Shore-side picnic table and park notice board.
            Box("Pond picnic table", new Vector3(18.6f, .38f, 3.45f),
                new Vector3(1.15f, .10f, .63f), timber);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Pond picnic bench", new Vector3(18.6f, .23f, 3.45f + side * .53f),
                    new Vector3(1.15f, .08f, .23f), timber);
                Box("Pond picnic support", new Vector3(18.6f + side * .38f, .20f, 3.45f),
                    new Vector3(.09f, .38f, .09f), metal);
            }
            Box("Pond park notice", new Vector3(9.0f, .86f, 8.9f),
                new Vector3(.84f, .57f, .08f), cream);
            Box("Pond notice post", new Vector3(9.0f, .43f, 8.9f),
                new Vector3(.09f, .83f, .09f), timber);
        }

        private void DrawFreightDetails()
        {
            Material wood = Paint(.58f, .43f, .30f);
            Material metal = Paint(.27f, .30f, .30f);
            Material tarp = Paint(.39f, .43f, .35f);
            // Stacked cargo and a covered pallet at the goods shed.
            for (int i = 0; i < 3; i++)
            {
                float x = -16.3f + i * .85f;
                Box("Freight pallet", new Vector3(x, .12f, 3.92f),
                    new Vector3(.72f, .14f, .62f), wood);
                Box("Freight crate", new Vector3(x, .48f, 3.92f),
                    new Vector3(.58f, .58f, .54f), i == 1 ? tarp : wood);
                Box("Freight crate band", new Vector3(x, .48f, 3.92f),
                    new Vector3(.055f, .60f, .56f), metal);
            }
        }
    }
}

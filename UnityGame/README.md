# Garage Gauntlet · Railway Rush

The [Maple Junction master plan](MapleJunctionPlan.md) shows how the five districts, local streets, terrain, railway, and slot route fit together. The map shows the original Maple Junction course.

Race a die-cast slot car on eight courses across five model railroad tables. Use PREV and NEXT on the start screen to pick a course. In Maple Junction the highway becomes Main Street, with storefronts, curb parking, and a crosswalk along the racing straight. The railway meets the slot track at two grade crossings and runs below a ramped flyover at its southern turnaround. The three-lap Grand Prix pits you against Ember, Switch, and Volt. Time Trial lets you race solo. Each course keeps separate personal bests for both modes. The game runs locally in Unity or in the browser.

- **Maple Junction** follows Main Street into the broad Cottage Hill sweeper.
- **Orchard Sprint** takes a longer route around the east side of town.
- **Timber Ridge** winds through Cottage Hill and down the wooded slope.
- **Depot Run** doubles back through the switchyard approach before descending to the crossing.

## The Springfield courses

Each Springfield is a separate tabletop town with its own route and scenery. The layouts compress landmarks and neighborhoods into a raceable model.

- **Springfield, Illinois** has a flat civic grid, the Old State Capitol and Lincoln’s neighborhood. [Downtown visitor map](https://visitspringfieldillinois.com/assets/documents/2024VisitorMap.pdf) · [Lincoln Home](https://www.nps.gov/places/abraham-lincoln-home.htm)
- **Springfield, Missouri** pairs a long Route 66 straight with the Gillioz Theatre and the Frisco freight district. [Visit Springfield history](https://www.springfieldmo.org/about-springfield/history/) · [Route 66 landmarks](https://www.springfieldmo.org/plan/vacation-ideas/route-66/)
- **Springfield, Massachusetts** climbs around a brick armory above the Connecticut River. [Springfield Armory landscape inventory](https://www.nps.gov/parkhistory/online_books/spar/cli.pdf)
- **Springfield, Oregon** has the Booth-Kelly Mill, a millrace and fir-lined foothills. [Springfield Downtown Refinement Plan](https://www.springfield-or.gov/wp-content/uploads/2016/12/1986DowntownRefinementPlan.pdf)

## Play in Unity

1. Sign in to Unity Hub and activate your Editor license. Add the `UnityGame` folder as a project and open it in Unity **6000.3.28f1 LTS**.
2. Open `Assets/Scenes/RailwayRush.unity` and press **Play**.
3. Hold **W**, **Up Arrow**, or **Space** for full throttle. Use **Shift** for corner pace. On touch screens or with a mouse, press the tall on-screen **Trigger** and drag up for more power or down to ease off. The middle range gives you more room to fine-tune your speed. Release to brake. **Esc** or **P** pauses, **M** toggles sound, and **Enter** starts or repeats a race.

Each car follows its own slot by its front guide. The rear swings outward in fast turns. Ease off the trigger to settle the tail before the guide pops out. Lift when the crossing lights flash. A car that jumps its slot or hits the train is automatically re-slotted after a delay.

## Publish to GitHub Pages

1. Add **Web Build Support** to Unity 6000.3.28f1 in Unity Hub.
2. In Unity, choose **Garage Gauntlet → Build GitHub Pages site**. The export is written to `UnityGame/Builds/Web`. Its loader can unpack the compressed game files on static hosting.
3. In a GitHub repository you can publish from, put the **contents** of `Builds/Web` at the root of a `gh-pages` branch. Include `index.html`, the `Build` directory, and `.nojekyll`.
4. In the repository's **Settings → Pages**, select **Deploy from a branch**, choose **gh-pages** and **/(root)**, and save. Use **Visit site** after deployment. Serve the export over HTTP when testing it locally.

## Art

The included Kenney models and texture atlases are released under **CC0**. The original license notices are in [ThirdParty](ThirdParty/).

The die-cast RC car uses a Khronos model fitted to the game's steering and wheel animation. The Craftsman houses, textured station, and locomotive use additional [CC0 assets and credits](ThirdParty/ScaleModelAssets.md).

- [Toy Car Kit 1.2](https://kenney.nl/assets/toy-car-kit)
- [Train Kit 1.1](https://kenney.nl/assets/train-kit)
- [City Kit (Suburban) 2.0](https://kenney.nl/assets/city-kit-suburban)

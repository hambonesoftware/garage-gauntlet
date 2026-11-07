# Garage Gauntlet · Railway Rush

Race a die-cast slot car around a four-lane circuit in a tabletop railway town. The three-lap Grand Prix pits you against Ember, Switch, and Volt. Time Trial gives you the same circuit on your own with a separate personal best. The game runs locally in Unity or in the browser.

## Play in Unity

1. Sign in to Unity Hub and activate your Editor license. Add the `UnityGame` folder as a project and open it in Unity **6000.3.28f1 LTS**.
2. Open `Assets/Scenes/RailwayRush.unity` and press **Play**.
3. Hold **W**, **Up Arrow**, or **Space** for full throttle. Use **Shift** for corner pace. On touch screens or with a mouse, press the on-screen **Trigger** and slide upward for more power. Release to brake. **Esc** or **P** pauses, **M** toggles sound, and **Enter** starts or repeats a race.

Each car follows its own slot. Lift before tight turns and whenever the crossing lights flash. A car that jumps its slot or hits the train is automatically re-slotted after a delay. Slot racing has its own personal-best records.

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

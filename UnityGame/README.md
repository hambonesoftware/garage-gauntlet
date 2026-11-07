# Garage Gauntlet · Railway Rush

Race a miniature RC car around a tabletop railway town. The three-lap Grand Prix pits you against Ember, Switch, and Volt. Time Trial gives you the same circuit on your own with a separate personal best. The game runs locally in Unity or in the browser.

## Play in Unity

1. Sign in to Unity Hub and activate your Editor license. Add the `UnityGame` folder as a project and open it in Unity **6000.3.28f1 LTS**.
2. Open `Assets/Scenes/RailwayRush.unity` and press **Play**.
3. Use **WASD** or the **arrow keys** to drive, **Space** or **Left Shift** to boost, and **R** to recover at the last checkpoint. **Esc** or **P** pauses, **M** toggles sound, and **Enter** starts or repeats a race. On touch devices, hold **GO** and steer with the opposite thumb. Hold **BOOST** with **GO** to spend battery charge. Open **Pause** to recover.

Drive across the teal pads to recharge. Flashing signals and lowered gates mean a train is approaching. Grass slows the car, resetting adds two seconds to your time, and a train collision adds three.

## Publish to GitHub Pages

1. Add **Web Build Support** to Unity 6000.3.28f1 in Unity Hub.
2. In Unity, choose **Garage Gauntlet → Build GitHub Pages site**. The export is written to `UnityGame/Builds/Web`. Its loader can unpack the compressed game files on static hosting.
3. In a GitHub repository you can publish from, put the **contents** of `Builds/Web` at the root of a `gh-pages` branch. Include `index.html`, the `Build` directory, and `.nojekyll`.
4. In the repository's **Settings → Pages**, select **Deploy from a branch**, choose **gh-pages** and **/(root)**, and save. Use **Visit site** after deployment. Serve the export over HTTP when testing it locally.

## Art

The included Kenney models and texture atlases are released under **CC0**. The original license notices are in [ThirdParty](ThirdParty/).

The RC body and animated wheels are built for Railway Rush. The scale-model surfaces and locomotive use additional [CC0 assets and credits](ThirdParty/ScaleModelAssets.md).

- [Toy Car Kit 1.2](https://kenney.nl/assets/toy-car-kit)
- [Train Kit 1.1](https://kenney.nl/assets/train-kit)
- [City Kit (Suburban) 2.0](https://kenney.nl/assets/city-kit-suburban)

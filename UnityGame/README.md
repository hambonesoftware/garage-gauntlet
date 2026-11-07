# Garage Gauntlet · Railway Rush

Race a miniature RC car around a tabletop railway town. Complete three laps, watch for the train at both crossings, and try to beat your personal best. The time trial runs locally in Unity or in the browser.

## Play in Unity

1. Sign in to Unity Hub and activate your Editor license. Add the `UnityGame` folder as a project and open it in Unity **6000.3.28f1 LTS**.
2. Open `Assets/Scenes/RailwayRush.unity` and press **Play**.
3. Use **WASD** or the **arrow keys** to drive, **R** to recover at the last checkpoint, and **Enter** to start or race again. Touch devices have on-screen steering, throttle, brake/reverse, and reset controls.

An off-road detour slows the car. Resetting adds two seconds and a train collision adds three seconds.

## Publish to GitHub Pages

1. Add **Web Build Support** to Unity 6000.3.28f1 in Unity Hub.
2. In Unity, choose **Garage Gauntlet → Build GitHub Pages site**. The export is written to `UnityGame/Builds/Web`. Its loader can unpack the compressed game files on static hosting.
3. In a GitHub repository you can publish from, put the **contents** of `Builds/Web` at the root of a `gh-pages` branch. Include `index.html`, the `Build` directory, and `.nojekyll`.
4. In the repository's **Settings → Pages**, select **Deploy from a branch**, choose **gh-pages** and **/(root)**, and save. Use **Visit site** after deployment. Serve the export over HTTP when testing it locally.

## Art

The included Kenney models and texture atlases are released under **CC0**. The original license notices are in [ThirdParty](ThirdParty/).

- [Toy Car Kit 1.2](https://kenney.nl/assets/toy-car-kit)
- [Train Kit 1.1](https://kenney.nl/assets/train-kit)
- [City Kit (Suburban) 2.0](https://kenney.nl/assets/city-kit-suburban)

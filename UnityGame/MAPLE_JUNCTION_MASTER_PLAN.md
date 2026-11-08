# Maple Junction — Town Master Plan

For the current layout, playtest scope, and measured overhead drawing, see the [Maple Junction town master plan](MapleJunctionPlan.md). The notes below record the earlier scenery proposal.

## Creative direction
Garage Gauntlet: Railway Rush takes place on an elaborate, physically plausible model railway table. Maple Junction is a fictional Midwestern town with a compact historic core, older residential streets, railway freight operations, and a small park. The player is racing a guided die-cast slot car through a miniature built by an enthusiast, not driving a full-size vehicle on an open-world road.

## Existing town anchors (preserve)
- Main Street: eight individually dressed storefronts, awnings, lamp posts, commercial lane.
- Town Square: clock tower, fountain, benches, gardens.
- Switchyard: freight depot, sidings, station, working locomotive and crossing gates.
- Cottage Hill: Craftsman homes, gardens, picket fences and neighborhood lanes.
- Mill Pond: water, shoreline, pier, boardwalk, park scenery.
- Four-lane slot circuit with two live grade crossings and southern railway flyover.

## Spatial design rules
1. Preserve the tabletop perimeter and the four-lane race route. The train and slot track must remain independent systems.
2. Keep a clear buffer from slot track and railway before placing props. Verify at the closest sampled point, not just against the control polygon.
3. Buildings face streets; rear service areas face alleys or yards. Sidewalks connect entrances to roads.
4. Each block needs a focal building, at least two smaller support elements, distinct planting, and a visible reason for being there.
5. Every district must be recognizable from the orthographic race camera and from a lower, more immersive camera.
6. Maintain consistent toy scale across trains, cars, houses, people, trees and fixtures.
7. Use repeated modular assets with controlled variation in wear, roofline, materials, and lot setbacks.
8. Keep geometry batched by material and avoid unnecessary runtime colliders, Update methods and expensive transparent effects.

## District identities
### Main Street
Older brick commercial core with bakery, model shop, post office, diner, books, hardware, market and cafe. Add curbside parking, bicycle racks, bins, shop-window displays, delivery crates, planter boxes, fire hydrants, utility poles, storefront advertisements and service alleys. Keep signage readable without becoming visually noisy.

### Town Square
Civic focal point: clock tower, fountain, benches, paths, planters, small monument and a posted community events board. Maintain the fountain and hall as the central camera landmarks.

### Cottage Hill
Small detached homes with individual garages, hedges, garden sheds, laundry lines, vegetable plots, play equipment, fences and winding footpaths. Avoid identical lots. Subtle elevation should support a visual overlook without breaking the racing route.

### Switchyard
Working freight district with depot, loading platforms, timber stacks, freight wagons, signal posts, maintenance shed, utility transformers and covered pallets. Freight operations should read as a functioning system, not disconnected props.

### Mill Pond
Quiet natural district with fishing pier, picnic tables, reeds, park signage, winding footpath, benches and shaded trees. Use restrained water highlights and convincing flocked vegetation.

### Outskirts (future expansion)
Agricultural edge: grain elevator, small barn, rural crossing, hedgerows and a wooded cutting. This district requires an intentional course/board expansion, not simply placing buildings beyond the current playable perimeter.

## Race storytelling
- Establish the whole physical tabletop in the menu overview.
- As the countdown begins, ease down toward the die-cast car and make the track visually dominant.
- The two live train crossings remain true hazards; the flyover remains a safe grade-separated crossing.
- The railway, storefronts, and park should remain visible while racing without distracting from the braking points.
- Give each sector a distinct visual rhythm: Main Street straight, residential bends, industrial crossing, park edge and elevated flyover.

## Implementation sequence
### Phase 1 — Low-risk dressing
Add reusable, batched props to the existing neighborhoods. Preserve geometry, train timing, car handling and UI. (Initial source pass committed.)

### Phase 2 — Spatial validation
Produce a measured overhead layout from Unity world coordinates. Check footprints against road, train, track, camera occlusion and board edge. Relocate any props that violate clearance.

### Phase 3 — Production asset block
Create one finished downtown block with model-quality textured storefronts, roofs, doors, signs, curb details, ground transitions and soft lighting. Validate in mobile WebGL before propagating the standard.

### Phase 4 — Living town
Animate crossing signals (already present), station clock, one or two small ambient vehicles, selected storefront lights, and a garage door. Use a shared animation controller and visibility/distance limits.

### Phase 5 — Race camera and polish
Implement a smooth overview-to-race camera transition with stable framing in landscape and portrait. Test actual iPhone touch behavior, frame time, shadows and texture clarity.

## Acceptance tests
- Unity 6000.3.28f1 LTS scene loads without missing models, shaders or null references.
- Two live railway crossings and two bridge-underpass intersections are retained.
- The four slots and all racers complete three laps without clipping into newly added scenery.
- Static prop count remains batched, with no new per-frame scripting cost.
- All five existing districts are identifiable in a single overview image.
- On iPhone portrait and landscape, HUD controls remain usable and the race camera keeps the car visible.
- Compare screenshots before and after under identical camera, resolution and lighting.
- Rebuild WebGL, smoke-test, then publish the generated export to gh-pages.

## Current status
The initial source pass adds MapleJunctionDetails.cs and invokes DrawMapleJunctionDetails from RailwayTown.DrawTown. This is a source change, not yet a verified Unity/WebGL build. The live GitHub Pages branch must be rebuilt from Unity before the new town dressing appears there.

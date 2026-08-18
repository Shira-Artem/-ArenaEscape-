Pass A6 — Kill Bad Mountains

Goal:
Remove the visual mountain/cliff garbage that was entering the orc arena, gate, palisade, props and portal view.
This is intentionally a cleanup/reset pass, not an art-polish pass.

Replace in Assets/Scripts/Level0WorldLayout/:
- L0OrcArenaBackdropModule.cs
- L0Exit.cs
- L0Layout.cs
- L0Props.cs

What changed:
1. L0OrcArenaBackdropModule.cs
   - Removed all tall cliff/mountain/ridge generation.
   - Removed close/side/rear cliff shelves.
   - Builds only:
     - BACKDROP_A6_SAFE_LowHorizonOnly: very far, low, flat horizon blocks.
     - BACKDROP_A6_SAFE_FogMistOnly: fog/mist bands.
     - BACKDROP_A6_SAFE_FarFenceOnly: small far fence silhouettes.
   - No cone/pyramid/spire/mountain objects are created by this module.

2. L0Exit.cs
   - Removed extra background mountains completely.
   - L0Exit now owns only connector ground and a small lantern.
   - Backdrop ownership stays in L0OrcArenaBackdropModule.

3. L0Props.cs
   - CreateRockSpire no longer creates a tall spire.
   - It returns null near the orc arena / gate / road forbidden vista.
   - If used very far away, it creates a low boulder cluster instead of a tall mountain.

4. L0Layout.cs
   - BuildLayout() now always calls ClearLayout() first.
   - This avoids Unity inspector serialization leaving Clear Before Build = false and stacking stale generated worlds.

Manual Unity check:
1. Exit Play Mode.
2. Replace the files.
3. Let Unity recompile.
4. Select LEVEL0_WORLD_LAYOUT_BUILDER and use Clear Level0 World Layout once.
5. Enter Play Mode.
6. In Hierarchy search for:
   - CleanBackgroundMountain: should find nothing.
   - RockSpire: should find nothing near the orc arena.
   - BACKDROP_A6_SAFE: should find only low horizon/fog/far fence objects.
7. Check road -> gate -> center -> portal. No large mountains should be inside the arena or on props.

Not run:
Unity, Play Mode, build, tests, batchmode, dotnet, msbuild, mono, csc, compilers.

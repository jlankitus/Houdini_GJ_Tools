
Clayxel
Created and owned by Andrea Interguglielmi, https://twitter.com/andreintg
For support please use our discord server:
discord: https://discord.gg/Uh3Yc43

For general enquiries you can also get in touch via twitter:
twitter: https://twitter.com/clayxels

For all other kind of enquires that are not development related, like refunds, please get in touch directly via email:
ainterguglielmi@gmail.com

Usage:
1) Add an empty gameObject in scene, then add the ClayContainer component to it
2) Use the custom inspector to perform basic operations like adding a ClayObject
3) After you add a ClayObject, use the inspector to change primitive type, blending and color.
4) Mess around, it's easy : )

Tips:
- hit "p" on your keyboard to pick-select clay objects from the viewport 
- Drag solids up and down the hierarchy to isolate negative blends
- Split clayxels into many containers to make large and complex models

Change log:
v0.91b patch:
- resolves the unsafe code error upon import of the package
- removes duplicated dll, oops
- packaged geometry3sharp dll

v0.91
- sculpts can now be scaled up or down and the final shape will be retained
- added instancing and a new example scene to show how it works
- added normal smoothing on frozen meshes
- added prefs to change bounds color and picking shortcut
- improved auto-retopology, no more holes
- improved scene size when freezing to mesh by forcing assets to be stored on disk
- fixed distance-fields deteriorations in the sphere, torus and curve solids
- fixed a performance bug when generating collision meshes on each frame

v0.9
- added slicing attributes on sphere and torus primitives
- setClayxelDetail now allows for dynamic change of resolution without re-initializing the container, good for in-game LOD
- changed resolution workflow to be less confusing, no more chunks, clayxelDetail from 0 to 100, buttons to expand bounds
- improved mesh generation for collisions, use ClayContainer.generateMesh(int levelOfDetail)
- improved meshing and retopology (retopo will still leave holes, non-retopo meshes are guaranteed watertight)
- optimized performance on clay evaluation
- added cast/receive shadows option on containers
- added public API to access the internal point cloud: ClayContainer.getPointCloudBuffers()
- added support for UI Scaling in Unity
- added example scene to show normal maps and texturing of clayxels
- added example scene to show dynamic mesh generation for collisions
- improved clayObject inspector attributes with a bigger numeric range to make sculpting easier (only affects UI)
- bug fix: flattened sphere primitive causing holes
- bug fix: undo issues on ClayObject inspector
- bug fix: freeze mesh then hit play, container will not show the right state of frozen

v0.822
- added built-in render pipeline examples
- added options to fine tune performance, see ClayxelsPrefs.cs
- added cache system to sculpt without any limit on number of solids (see scene file exampleCache)
- prefab compatibility improvements
- added public interface documentation
- added warning when editor has UI scaling
- bug fix: cube left over when switching to offsetter or spline clayObject mode
- bug fix: disappearing clayxels on 45 degrees cubes
- bug fix: mesh freeze not working on certain graphc cards
- bug fix: load a scene with clayxels from script, then unload, then load back, error
- bug fix: builds fail because of PrefabUtils
- misc bug fixes and improvements

v0.7
- added ClayObject Spline mode
- added ClayObject Offset mode
- HDRP and URP shaders are now customizable using Amplify Shader 
- added new solids: hexagon and prism
- auto retopology (asset store only, windows only)
- materials will now automatrically display their attributes in the inspector
- optimized: C# code and GPU-read bottlenecks for chunks
- optimized: built-in renderer shader
- clayxels can now work without GameObjects

v0.6
- bug fix: clayxels disappearing on certain editor events
- bug fix: mirrored objects disappearing
- bug fix: fail to parse attributes in claySDF.compute on certain localized windows systems
- bug fix: frozen mesh sometimes has missing triangles

v0.51
- improved picking to be more responsive
- added new emissiveIntensity parameter
- new user-defined file for custom primitives: userClay.compute
- urp and hdrp frozen mesh shader
- all negative blended shapes are now visualized with wires
- performance optmizations to on grids with large sizes
- custom materials override
- added menu entry under GameObject/3D Objects/Clayxel Container
- new solids added are now centered within the grid
- added Clayxels namespace
- Clayxel component is now Clayxels.ClayContainer
- ClayObjects are auto-renamed unless the name is changed by the user
- misc bug fixes and optimizations

v0.5
- initial HDRP and URP compatibility
- use #define CLAYXELS_INDIRECTDRAW in Clayxels.cs to unlock better performance on modern hardware 
- restructured mouse picking to be more robust and work on all render pipelines
- core optimizations to allow for thousands of solids
- misc bug fixes
v0.462
- fixed? occasional tiny holes in frozen mesh
v0.461
- fixed disappearing cells when next to grid boundaries
v0.46
- added shift select functionality when picking clay-objects
- improved, clayxels texture now use full alpha values (not a cutoff)
- improved ugly sharp corners on negative shapes
- improved, grids now grow/shrink from the center to facilitate resolution changes
- fixed solids having bad parameters upon solid-type change in the inspector (it now reverts to good default values)
- fixed disappearing clayxels when unity looses and regain focus (alt-tabbing to other apps)
- fixed: glitchy points on seams when solids go beyond bounds
- fixed: scaling grids caused clayxels to change size erroneously
- fixed: inspector undo should work as expected now
- fixed: building executables containing clayxels caused errors
- fixed freeze to mesh on bigger grids caused some solids to disappear
v0.45
- mac support
- clayxels can now be textured and oriented to make foliage and such
v0.43
- new surface shader, integrates with scene lights with shadows and Unity's PostProcess Stack
- selection highlight when hitting "p" shortcut
- inspector multi-edit for all selected clayObjects
v0.42
picking bug fixed
v0.41
new shader for mobile and Mac OSX
v0.4
first beta released

Start of Instant-Meshes copyright notice
https://github.com/wjakob/instant-meshes
Copyright (c) 2015 Wenzel Jakob, Daniele Panozzo, Marco Tarini,
and Olga Sorkine-Hornung. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors
   may be used to endorse or promote products derived from this software
   without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
End of Instant-Meshes copyright notice
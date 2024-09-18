A node shader editor with primary focus on the built-in pipeline and VRChat

Install with [VCC](https://z3y.github.io/vpm-package-listing/) or add the git url

## [Documentation](https://z3y.github.io/Graphlit)

## Templates

- Unlit, Lit and Toon

## Features

- Easily customizable custom function nodes
- Full custom lighting possible with the Unlit graph
- Live Preview
- Access to the Light Color, Direction, Shadow Attenuation and Distance Attenuation
- Highest quality node previews - Previews rendered directly into the rect of the shader node. 3D previews are spheres rendered on a quad with all mesh data created in the fragment shader for highest quality.
- Great performance - You can disable previews by default to achieve the maximum performance for large graphs.
- Maximized workspace area with a simple design - The graph view takes up the entire editor window, while only selecting nodes enables an element for additional settings.
- Register/Fetch variable nodes - Wirelessly connect nodes without cluttering the graph view.
- Custom material inspector - Handles some basic things like setting up transparency modes, drawing vector fields correctly, foldouts etc.
- Node hotkeys
- Varyings packing - All varyings are packed when possible. For example using uv1 and uv0 nodes will only create one float4 varying and pack them in xy and zw components.
- Grabpass - Created when using the screen color node.
- Outline - Duplicates the first pass of the target and inverts cull, you can use the outline pass branch node to adjust the outline.
- Geometric Specular Anti-Aliasing
- Bakery Mono SH
- Lightmapped Specular
- Bicubic Lightmap

## Preview

![image](https://github.com/z3y/graphlit/assets/33181641/e3c10af5-9c49-4794-875f-ea1692a10b78)

https://github.com/z3y/MyShaderGraph/assets/33181641/ae523917-56ee-420d-90ac-a3f3afdecf82

## Currently not implemented

- Lacks validation (Same nodes shouldn't be allowed to connect to both shader stages, etc.)
- Undo not fully implemented
- Default textures sometimes dont apply in the preview
- Not all inputs available in all transform spaces
- Custom varyings
- Custom function parser is not very advanced
- Changing inputs or outputs on custom function nodes can leave some edges disconnected
- Sharing sampler states between textures will result in failed to compile previews
- Previews can lead to high CPU usage if the interaction mode is set to default. Set `Preferences -> General -> Interaction Mode` to monitor refresh rate or custom

## [Discord](https://discord.gg/bw46tKgRFT)

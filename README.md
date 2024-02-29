A node shader editor with primary focus on the built-in pipeline and VRChat. Early in development, but fully functional. 

Install with [VCC](https://z3y.github.io/vpm-package-listing/) or add the git url.

## Templates
- Unlit
- Lit (Flat and PBR)

## Features
- Easily customizable custom function nodes

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

## Lit Template
- Bakery Mono SH
- Lightmapped Specular
- Bicubic Lightmap


## Preview

![Screenshot 2024-02-09 231826](https://github.com/z3y/MyShaderGraph/assets/33181641/f94a7774-2273-48e5-835c-8ddad39d4983)

https://github.com/z3y/MyShaderGraph/assets/33181641/ae523917-56ee-420d-90ac-a3f3afdecf82

# How to use

Generally most of the tutorials for other shader editors can apply.

## Creating new shaders
`Right Click > Create > Shader Graph Z > Lit Graph`

## Master Node
- This is the main node where all settings, properties and target features can be adjusted. When the property is added it will appear in the node search list under Properties.

- All vertex ports (Position, Normal, Tangent) are in absolute World Space, to avoid unnecessary conversion from world, to object, back to world again. Use a transform node to convert from object to world when needed.


## Custom function node
- Custom function nodes are entirely defined with code. They can either be inlined directly in the node, or imported from a file.
- Inputs and outputs are automatically created by parsing the text. The last declared function is used.
- You can add custom function nodes to the graph search tree menu by creating an .hlsl file with a ZSGFunction tag.
- You can bind inputs (example "float2 UV" would automatically pass in the uv0 texture coordinate to the function when nothing is connected)
- Any variable type can be used, even if it's not supported by the graph, to pass values between custom functions

## Hotkeys
Left click while holding down one of the keys:
```
Alpha1: Float
Alpha2: Float2
Alpha3: Float3
Alpha4: Float4
Alpha5: Color
Alpha6: Texture2DProperty
Period: Dot
M: Multiply
A: Add
Z: Swizzle
N: Normalize
O: OneMinus
S: Subtract
T: SampleTexture2D
U: UV
P: Preview
C: CustomFunction
B: Branch
V: Append
L: Lerp
```


## Currently not implemented
- Might lack some basic nodes
- Lacks validation (Same nodes shouldn't be allowed to connect to both shader stages, etc.)
- Undo not fully implemented
- Default textures sometimes dont apply in the preview
- Not all inputs available in all transform spaces
- It doesn't really have a good name
- Custom varyings
- Custom function parser is not very advanced
- Changing inputs or outputs on custom function nodes can leave some edges disconnected
- Sharing sampler states between textures will result in failed to compile previews


[Patreon](https://www.patreon.com/z3y) |
[Bug Reports](https://github.com/z3y/ShaderGraphZ/issues) |
[Discord Support](https://discord.gg/bw46tKgRFT)
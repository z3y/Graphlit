A node shader editor with primary focus on the built-in pipeline and VRChat. Early in development, but fully functional.

## Templates
- Unlit
- Lit (Flat and PBR)

## Features
- Easily customizable custom function nodes
- High quality node previews
- Custom material inspector
- Node hotkeys
- Varyings packing
- Grabpass
- Outline

Install with [VCC](https://z3y.github.io/vpm-package-listing/) or add the git url

## Preview

![Screenshot 2024-02-09 231826](https://github.com/z3y/MyShaderGraph/assets/33181641/f94a7774-2273-48e5-835c-8ddad39d4983)

https://github.com/z3y/MyShaderGraph/assets/33181641/ae523917-56ee-420d-90ac-a3f3afdecf82
## Custom function node
- Custom function nodes are entirely defined with code. They can either be inlined directly in the node, or imported from a file.
- Inputs and outputs are automatically created by parsing the text. The last declared function is used.
- You can add custom function nodes to the graph search tree menu by creating an .hlsl file with a ZSGFunction tag.
- You can bind inputs (example "float2 UV" would automatically pass in the uv0 texture coordinate to the function when nothing is connected)
- Any variable type can be used, even if it's not supported by the graph, to pass values between custom functions

## Currently not implemented
- Groups are not saved
- Might lack some basic nodes
- Lacks validation (Same nodes shouldn't be allowed to connect to both shader stages, etc.)
- Undo not fully implemented
- Might have small breaking changes
- Default textures sometimes dont apply in the preview
- Not all inputs available in all transform spaces
- It doesn't really have a good name
- Custom varyings
- Custom function parser is not very advanced
- Changing inputs or outputs on custom function nodes can leave some edges disconnected
- Copied nodes might have missing edges
- Sharing sampler states between textures will result in failed to compile previews
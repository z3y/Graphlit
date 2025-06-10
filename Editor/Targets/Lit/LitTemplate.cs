using UnityEngine.UIElements;
using Graphlit.Nodes.PortType;
using Graphlit.Nodes;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Graphlit
{
    [NodeInfo("Targets/Lit Target"), Serializable]
    public class LitTemplate : TemplateOutput
    {
        [MenuItem("Assets/Create/Graphlit/Lit Graph (Full)")]
        public static void CreateLitVariantFull() => GraphlitImporter.CreateFromSample("Packages/com.z3y.graphlit/Shaders/Lit.graphlit");

        [MenuItem("Assets/Create/Graphlit/Lit Graph (Simple)")]
        public static void CreateLitVariantSimple() => GraphlitImporter.CreateFromSample("Packages/com.z3y.graphlit/Shaders/Samples/Lit Simple.graphlit");

        [MenuItem("Assets/Create/Graphlit/Toon Graph")]
        public static void CreateToonVariant() => GraphlitImporter.CreateFromSample("Packages/com.z3y.graphlit/Shaders/Toon.graphlit");

        [MenuItem("Assets/Create/Graphlit/Custom Graph")]
        public static void CreateCustomVariant() => GraphlitImporter.CreateFromSample("Packages/com.z3y.graphlit/Shaders/Custom.graphlit");


        public override string Name { get; } = "Lit";
        public override int[] VertexPorts => new int[] { VERTEX_POSITION, VERTEX_NORMAL, VERTEX_TANGENT };
        public override int[] FragmentPorts => new int[] { ALBEDO, ALPHA, METALLIC, OCCLUSION, EMISSION, SPECULAR_ROUGHNESS, REFLECTANCE, NORMAL_TS, CUTOFF,
            BASE_WEIGHT, SPECULAR_WEIGHT, SPECULAR_COLOR, DIFFUSE_ROUGHNESS, SPECULAR_ROUGHNESS_ANISOTROPY, SPECULAR_IOR, TANGENT,
            COAT_WEIGHT, COAT_COLOR, COAT_ROUGHNESS, COAT_ROUGHNESS_ANISOTROPY, COAT_IOR, COAT_DARKENING,
            THIN_FILM_WEIGHT, THIN_FILM_IOR, THIN_FILM_THICKNESS
        };

        public override string TemplateGUID => "131fe11a59ae68b498c21549d0ebdd85";

        const int VERTEX_POSITION = 0;
        const int VERTEX_NORMAL = 1;
        const int VERTEX_TANGENT = 2;
        const int ALBEDO = 3;
        const int ALPHA = 4;
        const int CUTOFF = 5;
        const int SPECULAR_ROUGHNESS = 6;
        const int METALLIC = 7;
        const int OCCLUSION = 8;
        const int EMISSION = 9;
        const int NORMAL_TS = 10;
        const int REFLECTANCE = 11;

        const int BASE_WEIGHT = 12;
        const int SPECULAR_WEIGHT = 13;
        const int SPECULAR_COLOR = 14;
        const int DIFFUSE_ROUGHNESS = 15;
        const int SPECULAR_IOR = 17;

        const int SPECULAR_ROUGHNESS_ANISOTROPY = 16;
        const int TANGENT = 18;

        const int COAT_WEIGHT = 19;
        const int COAT_COLOR = 20;
        const int COAT_ROUGHNESS = 21;
        const int COAT_ROUGHNESS_ANISOTROPY = 22;
        const int COAT_IOR = 23;
        const int COAT_DARKENING = 24;

        const int THIN_FILM_WEIGHT = 25;
        const int THIN_FILM_THICKNESS = 26;
        const int THIN_FILM_IOR = 27;



        public override IEnumerable<Port> Inputs => base.Inputs;
        public override void Initialize()
        {
            inputContainer.Clear();

            var vertexStage = new Label("Vertex") { style = { fontSize = 13, marginLeft = 23 } };
            inputContainer.Add(vertexStage);
            AddPort(new(PortDirection.Input, new Float(3), VERTEX_POSITION, "Position"));
            AddPort(new(PortDirection.Input, new Float(3), VERTEX_NORMAL, "Normal"));
            AddPort(new(PortDirection.Input, new Float(3), VERTEX_TANGENT, "Tangent"));

            AddSeparator();

            var fragmentStage = new Label("Fragment") { style = { fontSize = 13, marginLeft = 23 } };
            inputContainer.Add(fragmentStage);

            AddSeparator();
            inputContainer.Add(new Label("Base") { style = { marginLeft = 23 } });
            // AddPort(new(PortDirection.Input, new Float(1), BASE_WEIGHT, "BaseWeight"), true, "Weight");
            AddPort(new(PortDirection.Input, new Float(3), ALBEDO, "Albedo"), true, "Color");
            AddPort(new(PortDirection.Input, new Float(1), METALLIC, "Metallic"), true, "Metallic");
            AddPort(new(PortDirection.Input, new Float(1), OCCLUSION, "Occlusion"));
            //AddPort(new(PortDirection.Input, new Float(1), DIFFUSE_ROUGHNESS, "DiffuseRoughness"), true, "Diffuse Roughness");

            AddSeparator();
            inputContainer.Add(new Label("Specular") { style = { marginLeft = 23 } });
            //AddPort(new(PortDirection.Input, new Float(1), SPECULAR_WEIGHT, "SpecularWeight"), true, "Weight");
            AddPort(new(PortDirection.Input, new Float(3), SPECULAR_COLOR, "SpecularColor"), true, "Color");

            AddPort(new(PortDirection.Input, new Float(1), SPECULAR_ROUGHNESS, "Roughness"), true, "Roughness");
            AddPort(new(PortDirection.Input, new Float(1), SPECULAR_ROUGHNESS_ANISOTROPY, "Anisotropy"), true, "Anisotropy");
            AddPort(new(PortDirection.Input, new Float(1), SPECULAR_IOR, "IOR"), true, "IOR");
            //AddPort(new(PortDirection.Input, new Float(1), REFLECTANCE, "Reflectance"));

            AddSeparator();
            inputContainer.Add(new Label("Coat") { style = { marginLeft = 23 } });
            AddPort(new(PortDirection.Input, new Float(1), COAT_WEIGHT, "CoatWeight"), true, "Weight");
            AddPort(new(PortDirection.Input, new Float(3), COAT_COLOR, "CoatColor"), true, "Color");
            AddPort(new(PortDirection.Input, new Float(1), COAT_ROUGHNESS, "CoatRoughness"), true, "Roughness");
            //AddPort(new(PortDirection.Input, new Float(1), COAT_ROUGHNESS_ANISOTROPY, "CoatAnisotropy"), true, "Anisotropy");
            AddPort(new(PortDirection.Input, new Float(1), COAT_IOR, "CoatIOR"), true, "IOR");
            //AddPort(new(PortDirection.Input, new Float(1), COAT_DARKENING, "CoatDarkening"), true, "Darkening");

            AddSeparator();
            inputContainer.Add(new Label("Thin Film") { style = { marginLeft = 23 } });
            AddPort(new(PortDirection.Input, new Float(1), THIN_FILM_WEIGHT, "ThinFilmWeight"), true, "Weight");
            AddPort(new(PortDirection.Input, new Float(1), THIN_FILM_THICKNESS, "ThinFilmThickness"), true, "Thickness");
            //AddPort(new(PortDirection.Input, new Float(1), THIN_FILM_IOR, "ThinFilmIOR"), true, "IOR");

            AddSeparator();
            inputContainer.Add(new Label("Geometry") { style = { marginLeft = 23 } });
            AddPort(new(PortDirection.Input, new Float(3), NORMAL_TS, "Normal"));
            AddPort(new(PortDirection.Input, new Float(3), TANGENT, "Tangent"));

            AddPort(new(PortDirection.Input, new Float(1), ALPHA, "Alpha"), true, "Opacity");
            AddPort(new(PortDirection.Input, new Float(1), CUTOFF, "Cutoff"));

            AddSeparator();
            inputContainer.Add(new Label("Emission") { style = { marginLeft = 23 } });
            AddPort(new(PortDirection.Input, new Float(3), EMISSION, "Emission"));


            inputContainer.style.paddingBottom = 8;

            Bind(VERTEX_POSITION, PortBinding.PositionWS);
            Bind(VERTEX_NORMAL, PortBinding.NormalWS);
            Bind(VERTEX_TANGENT, PortBinding.TangentWS);
            DefaultValues[ALBEDO] = "float3(1.0, 1.0, 1.0)";
            DefaultValues[ALPHA] = "1.0";
            DefaultValues[CUTOFF] = "0.5";
            DefaultValues[SPECULAR_ROUGHNESS] = "0.5";
            DefaultValues[REFLECTANCE] = "0.5";
            DefaultValues[METALLIC] = "0.0";
            DefaultValues[OCCLUSION] = "1.0";
            DefaultValues[EMISSION] = "float3(0.0, 0.0, 0.0)";
            DefaultValues[NORMAL_TS] = "float3(0.0, 0.0, 1.0)";

            DefaultValues[BASE_WEIGHT] = "1.0";
            DefaultValues[DIFFUSE_ROUGHNESS] = "output.Roughness";
            DefaultValues[SPECULAR_WEIGHT] = "1.0";
            DefaultValues[SPECULAR_COLOR] = "float3(1.0, 1.0, 1.0)";
            DefaultValues[SPECULAR_ROUGHNESS_ANISOTROPY] = "0";
            DefaultValues[SPECULAR_IOR] = "1.5";

            DefaultValues[TANGENT] = "float3(1,0,0)";

            DefaultValues[COAT_WEIGHT] = "0";
            DefaultValues[COAT_COLOR] = "1";
            DefaultValues[COAT_ROUGHNESS] = "0";
            DefaultValues[COAT_ROUGHNESS_ANISOTROPY] = "0";
            DefaultValues[COAT_IOR] = "1.6";
            DefaultValues[COAT_DARKENING] = "1";

            DefaultValues[THIN_FILM_WEIGHT] = "0";
            DefaultValues[THIN_FILM_THICKNESS] = "0.5";
            DefaultValues[THIN_FILM_IOR] = "1.4";
        }

        [SerializeField] bool _cbirp = false;
        [SerializeField] bool _cbirpReflections = false;
        [SerializeField] bool _specular = true;
        [SerializeField] bool _fwdAddBlendOpMax = false;

        enum Normal
        {
            Tangent = 0,
            World = 1,
            Object = 2,
        }
        [SerializeField] Normal _normal = Normal.Tangent;
        string NormalDropoffDefine()
        {
            return _normal switch
            {
                Normal.World => "#define _NORMAL_DROPOFF_WS",
                Normal.Object => "#define _NORMAL_DROPOFF_OS",
                Normal.Tangent or _ => "#define _NORMAL_DROPOFF_TS",
            };
        }

        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);

            var normal = new EnumField("Normal", _normal);
            normal.RegisterValueChangedCallback(x => _normal = (Normal)x.newValue);
            root.Add(normal);

            if (_cbirpExists)
            {
                var cbirp = new Toggle("CBIRP") { value = _cbirp };
                cbirp.RegisterValueChangedCallback(x => _cbirp = x.newValue);
                root.Add(cbirp);

                var cbirpReflections = new Toggle("CBIRP Reflections") { value = _cbirpReflections };
                cbirpReflections.RegisterValueChangedCallback(x => _cbirpReflections = x.newValue);
                root.Add(cbirpReflections);
            }
            var spec = new Toggle("Specular") { value = _specular };
            spec.RegisterValueChangedCallback(x => _specular = x.newValue);
            root.Add(spec);
        }

        static readonly PropertyDescriptor _monosh = new(PropertyType.Float, "Mono SH", "_MonoSH") { customAttributes = "[Toggle(_BAKERY_MONOSH)][Header(Baked GI)] [Folder(Advanced Options)]" };
        static readonly PropertyDescriptor _bicubicLightmap = new(PropertyType.Float, "Bicubic Lightmap", "_BicubicLightmap") { customAttributes = "[Toggle(_BICUBIC_LIGHTMAP)] [Folder(Advanced Options)]" };
        static readonly PropertyDescriptor _nonLinearLightprobeSh = new(PropertyType.Float, "Non Linear Light Probe SH", "_NonLinearLightProbeSH") { customAttributes = "[Toggle(_NONLINEAR_LIGHTPROBESH)] [Folder(Advanced Options)]" };
        static readonly PropertyDescriptor _specularHighlights = new(PropertyType.Float, "Specular Highlights", "_SpecularHighlights") { customAttributes = "[ToggleOff] [Folder(Advanced Options)] [Header(Specular)]", FloatValue = 1 };
        static readonly PropertyDescriptor _glossyReflections = new(PropertyType.Float, "Environment Reflections", "_GlossyReflections") { customAttributes = "[ToggleOff] [Folder(Advanced Options)]", FloatValue = 1 };
        static readonly PropertyDescriptor _specularOcclusion = new(PropertyType.Float, "Specular Occlusion", "_SpecularOcclusion") { FloatValue = 1, Range = new Vector2(0, 5), customAttributes = "[Folder(Advanced Options)]" };

        static readonly PropertyDescriptor _mirror = new(PropertyType.Float, "Mirror", "_Mirror") { customAttributes = "[Toggle(_MIRROR)] [Folder(Advanced Options)]" };
        static readonly PropertyDescriptor _lmSpec = new(PropertyType.Float, "Lightmapped Specular", "_LightmappedSpecular") { customAttributes = "[Toggle(_LIGHTMAPPED_SPECULAR)] [Folder(Advanced Options)]" };
        static readonly PropertyDescriptor _ltcgi = new(PropertyType.Float, "LTCGI", "_LTCGI") { customAttributes = "[Toggle(_LTCGI)] [Folder(Advanced Options)]" };
        static readonly PropertyDescriptor _lightVolumes = new(PropertyType.Float, "VRC Light Volumes", "_VRC_LightVolumes") { customAttributes = "[Toggle(_VRC_LIGHTVOLUMES)] [Folder(Advanced Options)]" };

        static readonly PropertyDescriptor _decalery = new(PropertyType.Float, "Decalery", "_Decalery") { customAttributes = "[Toggle(_DECALERY)] [Folder(Advanced Options)]" };

        static readonly PropertyDescriptor _cbirpProp = new(PropertyType.Float, "CBIRP", "_CBIRP") { customAttributes = "[Toggle(_CBIRP)] [Header(Clustered BIRP)] [Folder(Advanced Options)]" };
        static readonly PropertyDescriptor _cbirpReflectionsProp = new(PropertyType.Float, "CBIRP Reflections", "_CBIRP_Reflections") { customAttributes = "[Toggle(_CBIRP_REFLECTIONS)] [Folder(Advanced Options)]" };

        void AddAreaLitProperties(ShaderBuilder builder)
        {
            var p = builder.properties;
            p.Add(new(PropertyType.Float, "Area Lit", "_AreaLitToggle") { customAttributes = "[Toggle(_AREALIT)] [Header(Area Lit)] [Folder(Advanced Options)]" });
            p.Add(new(PropertyType.Texture2D, "Light Mesh", "_LightMesh") { customAttributes = "[ShowIf(_AreaLitToggle, 1)] [Folder(Advanced Options)] [NoScaleOffset]", DefaultTextureEnum = DefaultTextureName.black });
            p.Add(new(PropertyType.Texture2D, "Light Texture 0", "_LightTex0") { customAttributes = "[ShowIf(_AreaLitToggle, 1)] [Folder(Advanced Options)] [NoScaleOffset]", DefaultTextureEnum = DefaultTextureName.white });
            p.Add(new(PropertyType.Texture2D, "Light Texture 1", "_LightTex1") { customAttributes = "[ShowIf(_AreaLitToggle, 1)] [Folder(Advanced Options)] [NoScaleOffset]", DefaultTextureEnum = DefaultTextureName.black });
            p.Add(new(PropertyType.Texture2D, "Light Texture 2", "_LightTex2") { customAttributes = "[ShowIf(_AreaLitToggle, 1)] [Folder(Advanced Options)] [NoScaleOffset]", DefaultTextureEnum = DefaultTextureName.black });
            p.Add(new(PropertyType.Texture2DArray, "Light Texture 3", "_LightTex3") { customAttributes = "[ShowIf(_AreaLitToggle, 1)] [Folder(Advanced Options)] [NoScaleOffset]", DefaultTextureEnum = DefaultTextureName.black });
            p.Add(new(PropertyType.Float, "Opaque Lights", "_OpaqueLights") { customAttributes = "[ShowIf(_AreaLitToggle, 1)] [ToggleOff] [Folder(Advanced Options)]", FloatValue = 1 });
            p.Add(new(PropertyType.Texture2D, "Shadow Mask (RGBA)", "_AreaLitOcclusion") { customAttributes = "[ShowIf(_AreaLitToggle, 1)] [Folder(Advanced Options)] [NoScaleOffset]", DefaultTextureEnum = DefaultTextureName.white });

        }

        const string _ltcgiPath = "Packages/at.pimaker.ltcgi/Shaders/LTCGI.cginc";
        const string _cbirpPath = "Packages/z3y.clusteredbirp/Shaders/cbirp.hlsl";
        const string _vrcLightVolumesPath = "Packages/red.sim.lightvolumes/Shaders/LightVolumes.cginc";
        const string _areaLitPath = "Assets/AreaLit/Shader/Lighting.hlsl";


        bool _ltcgiExists = System.IO.File.Exists(_ltcgiPath);
        bool _cbirpExists = System.IO.File.Exists(_cbirpPath);
        bool _lightVolumesExists = System.IO.File.Exists(_vrcLightVolumesPath);
        bool _areaLitExists = System.IO.File.Exists(_areaLitPath);

        const string Vertex = "Packages/com.z3y.graphlit/ShaderLibrary/Vertex.hlsl";
        const string FragmentForward = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentForwardPBR.hlsl";
        const string FragmentShadow = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentShadow.hlsl";
        const string FragmentMeta = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentMeta.hlsl";
        const string FragmentDepth = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentDepth.hlsl";
        const string FragmentDepthNormals = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentDepthNormals.hlsl";

        public override void OnBeforeBuild(ShaderBuilder builder)
        {
            AddTerrainTag(builder);
            builder.dependencies.Add(_graphlitConfigPath);

            builder.dependencies.Add(_ltcgiPath);
            builder.dependencies.Add(_cbirpPath);
            builder.dependencies.Add(_vrcLightVolumesPath);
            builder.dependencies.Add(_areaLitPath);


            builder.properties.Add(_surfaceOptions);
            builder.properties.Add(_surfaceBlend);
            builder.properties.Add(_blendModePreserveSpecular);
            builder.properties.Add(_transClipping);
            builder.properties.Add(_alphaToMask);
            builder.properties.Add(_alphaClip);
            builder.properties.Add(_mode);
            builder.properties.Add(_srcBlend);
            builder.properties.Add(_dstBlend);
            builder.properties.Add(_zwrite);
            builder.properties.Add(_ztest);

            builder.properties.Add(_cull);
            //if (GraphView.graphData.outlinePass != GraphData.OutlinePassMode.Disabled) builder.properties.Add(_outlineToggle);
            builder.properties.Add(_dfgProperty);

            builder.properties.Add(_monosh);
            builder.properties.Add(_bicubicLightmap);
            builder.properties.Add(_lmSpec);
            builder.properties.Add(_nonLinearLightprobeSh);
            builder.properties.Add(_queueOffset);
            if (_lightVolumesExists)
            {
                builder.properties.Add(_lightVolumes);
            }


            if (_specular)
            {
                builder.properties.Add(_specularHighlights);
                builder.properties.Add(_glossyReflections);
                builder.properties.Add(_mirror);
                builder.properties.Add(_specularOcclusion);
            }


            if (_ltcgiExists)
            {
                builder.properties.Add(_ltcgi);
                builder.subshaderTags["LTCGI"] = "_LTCGI";
            }

            if (forceNoShadowCasting)
            {
                builder.subshaderTags["ForceNoShadowCasting"] = "True";
            }

            if (_areaLitExists)
            {
                AddAreaLitProperties(builder);
            }
            if (_cbirpExists && !_cbirp)
            {
                builder.properties.Add(_cbirpProp);
                builder.properties.Add(_cbirpReflectionsProp);
            }

            builder.properties.Add(_decalery);

            builder._defaultTextures["_DFG"] = _dfg;

            builder.subshaderTags["RenderType"] = "Opaque";
            builder.subshaderTags["Queue"] = "Geometry";

            bool urp = GetRenderPipeline() == RenderPipeline.URP;

            if (urp)
            {
                builder.subshaderTags["UniversalMaterialType"] = "Lit";
            }


            {
                var portFlags = new List<int>() { VERTEX_POSITION, VERTEX_NORMAL, VERTEX_TANGENT, ALBEDO, ALPHA, CUTOFF, SPECULAR_ROUGHNESS, METALLIC, OCCLUSION, REFLECTANCE, EMISSION, NORMAL_TS,
                BASE_WEIGHT, SPECULAR_WEIGHT, SPECULAR_COLOR, DIFFUSE_ROUGHNESS, SPECULAR_ROUGHNESS_ANISOTROPY, TANGENT, SPECULAR_IOR,
                COAT_WEIGHT, COAT_COLOR, COAT_ROUGHNESS, COAT_ROUGHNESS_ANISOTROPY, COAT_IOR, COAT_DARKENING,
                THIN_FILM_WEIGHT, THIN_FILM_IOR, THIN_FILM_THICKNESS
                };
                var pass = new PassBuilder("Forward", Vertex, FragmentForward, portFlags.ToArray());
                pass.tags["LightMode"] = urp ? "UniversalForward" : "ForwardBase";
                TerrainPass(pass);
                pass.renderStates["Cull"] = "[_Cull]";
                pass.renderStates["ZWrite"] = "[_ZWrite]";
                pass.renderStates["ZTest"] = "[_ZTest]";
                pass.renderStates["AlphaToMask"] = "[_AlphaToMask]";
                pass.renderStates["Blend"] = "[_SrcBlend] [_DstBlend]";


                pass.pragmas.Add("#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ALPHATEST_ON");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");


                if (urp)
                {
                    AddURPLightingPragmas(pass);
                    pass.pragmas.Add("#pragma instancing_options renderinglayer");
                }
                else
                {
                    pass.pragmas.Add("#pragma multi_compile_fwdbase");
                    pass.pragmas.Add("#pragma shader_feature_fragment VERTEXLIGHT_ON");
                }

                //pass.pragmas.Add("#pragma skip_variants LIGHTPROBE_SH");
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                pass.pragmas.Add("#pragma shader_feature_local_fragment _BAKERY_MONOSH");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _BICUBIC_LIGHTMAP");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _LIGHTMAPPED_SPECULAR");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _NONLINEAR_LIGHTPROBESH");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _MIRROR");

                pass.pragmas.Add("#pragma shader_feature_local_fragment _ANISOTROPY");

                pass.pragmas.Add("#pragma shader_feature_local_fragment _COAT");
                //pass.pragmas.Add("#pragma shader_feature_local_fragment _COATANISOTROPY");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _THINFILM");

                pass.pragmas.Add("#pragma shader_feature_local_vertex _DECALERY");


                if (!_specular)
                {
                    pass.pragmas.Add("#define _SPECULARHIGHLIGHTS_OFF");
                    pass.pragmas.Add("#define _GLOSSYREFLECTIONS_OFF");
                }
                else
                {
                    pass.pragmas.Add("#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF");
                    pass.pragmas.Add("#pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF");
                }
                if (_cbirpExists && _cbirp)
                {
                    pass.pragmas.Add("#define _CBIRP");
                    if (_cbirpReflections)
                    {
                        pass.pragmas.Add("#define _CBIRP_REFLECTIONS");
                    }
                }
                if (_cbirpExists && !_cbirp)
                {
                    pass.pragmas.Add("#pragma shader_feature_local_fragment _CBIRP");
                    pass.pragmas.Add("#pragma shader_feature_local_fragment _CBIRP_REFLECTIONS");
                }
                if (_areaLitExists)
                {
                    pass.pragmas.Add("#pragma shader_feature_local_fragment _AREALIT");
                }

                pass.pragmas.Add(NormalDropoffDefine());

                if (_ltcgiExists) pass.pragmas.Add("#pragma shader_feature_local_fragment _LTCGI");
                if (_lightVolumesExists) pass.pragmas.Add("#pragma shader_feature_local_fragment _VRC_LIGHTVOLUMES");


                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.FrontFace);
                pass.varyings.RequirePositionCS();
                pass.attributes.RequireUV(1, 2);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.BitangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.TangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);

                PortBindings.Require(pass, ShaderStage.Vertex, PortBinding.UV2);

                pass.varyings.RequireCustomString("#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)\ncentroid LIGHTMAP_COORD lightmapUV : LIGHTMAP;\n#endif");
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                IncludeConfig(pass);
                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");

                pass.properties.Add(_specularOcclusion);
                builder.AddPass(pass);

            }

            if (!urp)
            {
                var portFlags = new List<int>() { VERTEX_POSITION, VERTEX_NORMAL, VERTEX_TANGENT, ALBEDO, ALPHA, CUTOFF, SPECULAR_ROUGHNESS, METALLIC, OCCLUSION, REFLECTANCE, NORMAL_TS,
                BASE_WEIGHT, SPECULAR_WEIGHT, SPECULAR_COLOR, DIFFUSE_ROUGHNESS, SPECULAR_ROUGHNESS_ANISOTROPY, TANGENT, SPECULAR_IOR,
                COAT_WEIGHT, COAT_COLOR, COAT_ROUGHNESS, COAT_ROUGHNESS_ANISOTROPY, COAT_IOR, COAT_DARKENING,
                THIN_FILM_WEIGHT, THIN_FILM_IOR, THIN_FILM_THICKNESS
                };
                var pass = new PassBuilder("ForwardAdd", Vertex, FragmentForward, portFlags.ToArray());
                pass.tags["LightMode"] = "ForwardAdd";
                TerrainPass(pass);

                pass.renderStates["Fog"] = "{ Color (0,0,0,0) }";
                pass.renderStates["Cull"] = "[_Cull]";
                pass.renderStates["Blend"] = "[_SrcBlend] One";
                pass.renderStates["ZWrite"] = "Off";
                pass.renderStates["ZTest"] = "[_ZTest]";
                pass.renderStates["AlphaToMask"] = "[_AlphaToMask]";

                if (_fwdAddBlendOpMax)
                {
                    //pass.renderStates["BlendOp"] = "Max, Add";
                }

                pass.pragmas.Add("#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ALPHATEST_ON");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");

                pass.pragmas.Add("#pragma multi_compile_fwdadd_fullshadows");
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                if (!_specular)
                {
                    pass.pragmas.Add("#define _SPECULARHIGHLIGHTS_OFF");
                }
                else
                {
                    pass.pragmas.Add("#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF");
                }

                pass.pragmas.Add("#pragma shader_feature_local_fragment _ANISOTROPY");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _COAT");
                //pass.pragmas.Add("#pragma shader_feature_local_fragment _COATANISOTROPY");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _THINFILM");

                pass.pragmas.Add("#pragma shader_feature_local_vertex _DECALERY");

                pass.pragmas.Add(NormalDropoffDefine());

                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.FrontFace);
                pass.varyings.RequirePositionCS();
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.BitangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.TangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);

                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.properties.Add(_specularOcclusion);

                IncludeConfig(pass);
                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");
                builder.AddPass(pass);
            }
            if (urp)
            {
                {
                    var pass = new PassBuilder("DepthOnly", Vertex, FragmentDepth, VERTEX_NORMAL, VERTEX_TANGENT, VERTEX_POSITION, ALPHA, CUTOFF);
                    CreateUniversalDepthPass(pass);
                    builder.AddPass(pass);
                }
                {
                    var pass = new PassBuilder("DepthNormals", Vertex, FragmentDepthNormals, VERTEX_POSITION, VERTEX_NORMAL, VERTEX_TANGENT, ALPHA, CUTOFF, NORMAL_TS);
                    CreateUniversalDepthNormalsPass(pass);
                    builder.AddPass(pass);
                }
            }
            {
                var pass = new PassBuilder("ShadowCaster", Vertex, FragmentShadow, VERTEX_POSITION, VERTEX_NORMAL, VERTEX_TANGENT, ALPHA, CUTOFF);
                CreateShadowCaster(pass, urp);
                builder.AddPass(pass);
            }
            {
                var pass = new PassBuilder("Meta", Vertex, FragmentMeta, ALPHA, CUTOFF, ALBEDO, METALLIC, SPECULAR_ROUGHNESS, EMISSION,
                    COAT_WEIGHT, COAT_COLOR, COAT_ROUGHNESS, COAT_IOR, SPECULAR_IOR
                    );
                pass.tags["LightMode"] = "Meta";
                pass.renderStates["Cull"] = "Off";
                TerrainPass(pass);
                pass.pragmas.Add("#pragma shader_feature EDITOR_VISUALIZATION");

                pass.pragmas.Add("#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ALPHATEST_ON");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _COAT");

                pass.attributes.RequireUV(0, 2);
                pass.attributes.RequireUV(1, 2);
                pass.attributes.RequireUV(2, 2);

                pass.attributes.RequirePositionOS();
                pass.varyings.RequirePositionCS();

                pass.varyings.RequireCustomString("#ifdef EDITOR_VISUALIZATION\nfloat2 VizUV : TEXCOORD*;\n#endif");
                pass.varyings.RequireCustomString("#ifdef EDITOR_VISUALIZATION\nfloat4 LightCoord : TEXCOORD*;\n#endif");

                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.properties.Add(_specularOcclusion);

                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");
                pass.pragmas.Add("#include \"Packages/com.unity.render-pipelines.core/ShaderLibrary/MetaPass.hlsl\"");

                builder.AddPass(pass);
            }

        }
    }
}


using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using UnityEngine;
using ZSG.Nodes.PortType;
using ZSG.Nodes;
using System.Linq;
using UnityEditor.UIElements;
using UnityEditor;
using Texture2D = UnityEngine.Texture2D;
using UnityEditor.Build.Content;
using static UnityEditor.MaterialProperty;
using UnityEditor.Experimental.GraphView;
using NUnit.Framework.Internal;

namespace ZSG
{
    [NodeInfo("Multiply", "a * b")]
    public class MultiplyNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), A, "A"));
            AddPort(new(PortDirection.Input, new Float(1, true), B, "B"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(ImplicitTruncation(A, B), "Multiply" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = {PortData[A].Name} * {PortData[B].Name};");
        }
    }
    [NodeInfo("Dot", "dot(a, b)")]
    public class DotNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), A, "A"));
            AddPort(new(PortDirection.Input, new Float(1, true), B, "B"));
            AddPort(new(PortDirection.Output, new Float(1, false), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ImplicitTruncation(A, B);
            PortData[OUT] = new GeneratedPortData(new Float(1), "Dot" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = dot({PortData[A].Name}, {PortData[B].Name});");
        }
    }
    [NodeInfo("Normalize", "normalize(a)")]
    public class NormalizeNode : ShaderNode
    {
        const int IN = 0;
        const int OUT = 1;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(3, true), IN));
            AddPort(new(PortDirection.Output, new Float(3, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(PortData[IN].Type, "Normalize" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = normalize({PortData[IN].Name});");
        }
    }
    [NodeInfo("Add", "a + b")]
    public class AddNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), A, "A"));
            AddPort(new(PortDirection.Input, new Float(1, true), B, "B"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(ImplicitTruncation(A, B), "Add" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = {PortData[A].Name} + {PortData[B].Name};");
        }
    }

    [NodeInfo("Float3"), Serializable]
    public class Float3Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector3 _value;
        public override bool EnablePreview => false;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(3, true), OUT));
            string propertyName = GetVariableNameForPreview(OUT);

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(propertyName, _value);
            };

            var f = new Vector3Field { value = _value };
            f.RegisterValueChangedCallback((evt) =>
            {
                _value = evt.newValue;
                UpdatePreviewMaterial();
            });
            inputContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (visitor.GenerationMode == GenerationMode.Preview)
            {
                string propertyName = GetVariableNameForPreview(OUT);
                var prop = new PropertyDescriptor(PropertyType.Float3, "", propertyName)
                {
                    vectorValue = _value
                };
                visitor.AddProperty(prop);
                PortData[OUT] = new GeneratedPortData(new Float(3), propertyName);
            }
            else
            {
                PortData[OUT] = new GeneratedPortData(new Float(3), "float3" + _value.ToString());
            }
        }
    }

    public abstract class PropertyNode : ShaderNode
    {
        public void SetReference(string guid)
        {
            _ref = guid;
        }

        protected abstract PropertyType propertyType { get; }

        protected const int OUT = 0;
        [SerializeField] protected string _ref;
        public override Color Accent => new Color(0.3f,0.7f,0.3f);
        protected PropertyDescriptor propertyDescriptor;
        public override bool EnablePreview => false;

        public override void AddElements()
        {
            var graphData = GraphView.graphData;
            propertyDescriptor = graphData.properties.Find(x => x.guid == _ref);
            if (string.IsNullOrEmpty(_ref) || propertyDescriptor is null)
            {
                propertyDescriptor = new PropertyDescriptor(propertyType, "Display Name");
                graphData.properties.Add(propertyDescriptor);
                _ref = propertyDescriptor.guid;
            }
            else
            {
                _ref = propertyDescriptor.guid;
            }

            var imguiContainer = new IMGUIContainer(OnGUI);
            {
                var s = imguiContainer.style;
                s.width = 75;
                s.marginLeft = 6;
            }
            inputContainer.Add(imguiContainer);
        }

        // imagine dealing with binding
        void OnGUI()
        {
            EditorGUILayout.LabelField(propertyDescriptor.GetReferenceName());
        }

        public override void AdditionalElements(VisualElement root)
        {
            var guid = new Label(_ref);
            root.Add(guid);
            var displayName = new TextField("Display Name") { value = propertyDescriptor.displayName };
            displayName.RegisterValueChangedCallback(evt => {
                propertyDescriptor.displayName = evt.newValue;
            });
            root.Add(displayName);

            var referenceName = new TextField("Reference Name") { value = propertyDescriptor.referenceName };
            referenceName.RegisterValueChangedCallback(evt => {
                propertyDescriptor.referenceName = evt.newValue;
            });
            root.Add(referenceName);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            //string propertyName = GetVariableNameForPreview(OUT);
            visitor.AddProperty(propertyDescriptor);
            PortData[OUT] = new GeneratedPortData(portDescriptors[OUT].Type, propertyDescriptor.GetReferenceName());
        }
    }

    [NodeInfo("Float Property"), Serializable]
    public class FloatPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float;
        public override void AddElements()
        {
            base.AddElements();

            AddPort(new(PortDirection.Output, new Float(1), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetFloat(propertyDescriptor.GetReferenceName(), propertyDescriptor.floatValue);
            };

        }

        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);

            var value = new FloatField("X") { value = propertyDescriptor.floatValue };
            value.RegisterValueChangedCallback(evt =>
            {
                propertyDescriptor.floatValue = evt.newValue;
                UpdatePreviewMaterial();
            });
            root.Add(value);
        }
    }
    [NodeInfo("Float2 Property"), Serializable]
    public class Float2PropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float2;
        public override void AddElements()
        {
            base.AddElements();

            AddPort(new(PortDirection.Output, new Float(2), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(propertyDescriptor.GetReferenceName(), propertyDescriptor.vectorValue);
            };

        }

        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);

            var value = new Vector2Field() { value = propertyDescriptor.vectorValue };
            value.RegisterValueChangedCallback(evt =>
            {
                propertyDescriptor.vectorValue = evt.newValue;
                UpdatePreviewMaterial();
            });
            root.Add(value);
        }
    }
    [NodeInfo("Float3 Property"), Serializable]
    public class Float3PropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float3;
        public override void AddElements()
        {
            base.AddElements();

            AddPort(new(PortDirection.Output, new Float(3), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(propertyDescriptor.GetReferenceName(), propertyDescriptor.vectorValue);
            };

        }

        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);

            var value = new Vector3Field() { value = propertyDescriptor.vectorValue };
            value.RegisterValueChangedCallback(evt =>
            {
                propertyDescriptor.vectorValue = evt.newValue;
                UpdatePreviewMaterial();
            });
            root.Add(value);
        }
    }
    [NodeInfo("Float4 Property"), Serializable]
    public class Float4PropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float4;
        public override void AddElements()
        {
            base.AddElements();

            AddPort(new(PortDirection.Output, new Float(4), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(propertyDescriptor.GetReferenceName(), propertyDescriptor.vectorValue);
            };

        }

        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);

            var value = new Vector4Field() { value = propertyDescriptor.vectorValue };
            value.RegisterValueChangedCallback(evt =>
            {
                propertyDescriptor.vectorValue = evt.newValue;
                UpdatePreviewMaterial();
            });
            root.Add(value);
        }
    }


    [NodeInfo("Float4"), Serializable]
    public class Float4Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector4 _value;
        public override bool EnablePreview => false;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(4, true), OUT));
            string propertyName = GetVariableNameForPreview(OUT);

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(propertyName, _value);
            };

            var f = new Vector4Field { value = _value };
            f.RegisterValueChangedCallback((evt) =>
            {
                _value = evt.newValue;
                UpdatePreviewMaterial();
            });
            inputContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (visitor.GenerationMode == GenerationMode.Preview)
            {
                string propertyName = GetVariableNameForPreview(OUT);
                var prop = new PropertyDescriptor(PropertyType.Float4, "", propertyName);
                prop.vectorValue = _value;
                visitor.AddProperty(prop);
                PortData[OUT] = new GeneratedPortData(new Float(4), propertyName);
            }
            else
            {
                PortData[OUT] = new GeneratedPortData(new Float(4), "float4" + _value.ToString());
            }
        }
    }

    [NodeInfo("Float2"), Serializable]
    public class Float2Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector2 _value;
        public override bool EnablePreview => false;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(2, true), OUT));
            string propertyName = GetVariableNameForPreview(OUT);

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(propertyName, _value);
            };

            var f = new Vector2Field { value = _value };
            f.RegisterValueChangedCallback((evt) =>
            {
                _value = evt.newValue;
                UpdatePreviewMaterial();
            });
            inputContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (visitor.GenerationMode == GenerationMode.Preview)
            {
                string propertyName = GetVariableNameForPreview(OUT);
                var prop = new PropertyDescriptor(PropertyType.Float2, "", propertyName)
                {
                    vectorValue = _value
                };
                visitor.AddProperty(prop);
                PortData[OUT] = new GeneratedPortData(new Float(2), propertyName);
            }
            else
            {
                PortData[OUT] = new GeneratedPortData(new Float(2), "float2" + _value.ToString());
            }
        }
    }

    [NodeInfo("Float"), Serializable]
    public class FloatNode : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private float _value;

        public override bool EnablePreview => false;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(1), OUT));
            string propertyName = GetVariableNameForPreview(OUT);

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetFloat(propertyName, _value);
            };

            var f = new FloatField ("X") { value = _value };
            f.Children().First().style.minWidth = 0;
            f.RegisterValueChangedCallback((evt) =>
            {
                _value = evt.newValue;
                UpdatePreviewMaterial();
            });
            inputContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (visitor.GenerationMode == GenerationMode.Preview)
            {
                string propertyName = GetVariableNameForPreview(OUT);
                var prop = new PropertyDescriptor(PropertyType.Float, "", propertyName)
                {
                    floatValue = _value
                };
                visitor.AddProperty(prop);
                PortData[OUT] = new GeneratedPortData(new Float(1), propertyName);
            }
            else
            {
                PortData[OUT] = new GeneratedPortData(new Float(1), "float(" + _value.ToString() + ")");
            }
        }
    }

    [NodeInfo("UV"), Serializable]
    public class UVNode : ShaderNode
    {
        const int OUT = 0;

        [SerializeField] Channel _uv = Channel.UV0;

        enum Channel
        {
            UV0, UV1, UV2, UV3
        }

        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(2, true), OUT));
            Bind(OUT, ChannelToBinding());

            var dropdown = new EnumField(_uv);

            dropdown.RegisterValueChangedCallback((evt) =>
            {
                _uv = (Channel)evt.newValue;
                Bind(OUT, ChannelToBinding());
                GeneratePreviewForAffectedNodes();
            });
            inputContainer.Add(dropdown);

        }

        private PortBinding ChannelToBinding()
        {
            return _uv switch
            {
                Channel.UV0 => PortBinding.UV0,
                Channel.UV1 => PortBinding.UV1,
                Channel.UV2 => PortBinding.UV2,
                Channel.UV3 => PortBinding.UV3,
                _ => throw new NotImplementedException(),
            };
        }

        protected override void Generate(NodeVisitor visitor)
        {
        }
    }

    [@NodeInfo("Swizzle"), Serializable]
    public sealed class SwizzleNode : ShaderNode
    {
        const int IN = 0;
        const int OUT = 1;
        [SerializeField] string swizzle = "x";

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));

            var f = new TextField { value = swizzle };
            f.RegisterValueChangedCallback((evt) =>
            {
                string newValue = Swizzle.Validate(evt, f);
                if (!swizzle.Equals(newValue))
                {
                    swizzle = newValue;
                    GeneratePreviewForAffectedNodes();
                }
            });
            extensionContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            int components = swizzle.Length;
            var data = new GeneratedPortData(new Float(components, false), PortData[IN].Name + "." + swizzle);
            PortData[OUT] = data;
        }
    }

    [NodeInfo("Time")]
    public sealed class TimeNode : ParameterNode
    {
        public override (string, Float) Parameter => ("_Time", new(4));
    }

    public abstract class ParameterNode : ShaderNode
    {
        const int OUT = 0;
        public abstract (string, Float) Parameter { get; }

        public override bool LowProfile => true;
        public override bool EnablePreview => false;

        public sealed override void AddElements()
        {
            AddPort(new(PortDirection.Output, Parameter.Item2, OUT, Parameter.Item1));
        }

        protected sealed override void Generate(NodeVisitor visitor)
        {
            var data = PortData[OUT];
            data.Name = Parameter.Item1;
            PortData[OUT] = data;
        }
    }

    [NodeInfo("Sin", "sin(A)")]
    public class SinNode : ShaderNode
    {
        const int IN = 0;
        const int OUT = 1;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(new Float(1, true), "Sin" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = sin({PortData[IN].Name});");
        }
    }

    [NodeInfo("DDX")]
    public class DDXNode : ShaderNode
    {
        const int IN = 0;
        const int OUT = 1;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(new Float(1, true), "DDX" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = ddx({PortData[IN].Name});");
        }
    }
    [NodeInfo("DDY")]
    public class DDYNode : ShaderNode
    {
        const int IN = 0;
        const int OUT = 1;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(new Float(1, true), "DDY" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = ddy({PortData[IN].Name});");
        }
    }

    [NodeInfo("BindingTest"), Serializable]
    public class BindingTestNode : ShaderNode
    {
        [SerializeField] PortBinding _binding = PortBinding.UV0;
        [SerializeField] int _components = 3;


        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(_components), 0));

            var dropdown = new EnumField(_binding);

            dropdown.RegisterValueChangedCallback((evt) =>
            {
                _binding = (PortBinding)evt.newValue;
                Bind(0, _binding);
                GeneratePreviewForAffectedNodes();
            });
            inputContainer.Add(dropdown);

            var intField = new IntegerField("Components")
            {
                value = _components
            };
            intField.RegisterValueChangedCallback((evt) =>
            {
                _components = evt.newValue;
                portDescriptors[0].Type = new Float(_components);
                GeneratePreviewForAffectedNodes();
            });
            inputContainer.Add(intField);

            Bind(0, _binding);
            preview3D = true;
        }

        protected override void Generate(NodeVisitor visitor)
        {
            var data = PortData[0];
            var @float = (Float)data.Type;
            @float.components = _components;
            data.Type = @float;
            PortData[0] = data;
        }
    }

    [NodeInfo("Texture 2D Property"), Serializable]
    public class Texture2DPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Texture2D;
        Texture2D GetTexture() => Helpers.SerializableReferenceToObject<Texture2D>(propertyDescriptor.defaultTexture);

        public override void AddElements()
        {
            base.AddElements();

            AddPort(new(PortDirection.Output, new Nodes.PortType.Texture2D(), OUT));


            var texture = GetTexture();
            onUpdatePreviewMaterial = (mat) =>
            {
                mat.SetTexture(propertyDescriptor.GetReferenceName(), texture);
            };

            EditorApplication.delayCall += () =>
            {
                UpdatePreviewMaterial();
            };
        }

        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);



            var texField = new ObjectField
            {
                value = GetTexture(),
                objectType = typeof(Texture2D)
            };
            texField.RegisterValueChangedCallback((evt) =>
            {
                propertyDescriptor.defaultTexture = Helpers.AssetSerializableReference(evt.newValue);
                var texture = GetTexture();
                onUpdatePreviewMaterial = (mat) =>
                {
                    mat.SetTexture(propertyDescriptor.GetReferenceName(), texture);
                };
                UpdatePreviewMaterial();
            });
            root.Add(texField);
        }
    }

    [NodeInfo("Sample Texture 2D")]
    public class SampleTexture2DNode : ShaderNode
    {
        const int UV = 0;
        const int TEX = 1;
        const int OUT_RGBA = 3;

        const int OUT_R = 4;
        const int OUT_G = 5;
        const int OUT_B = 6;
        const int OUT_A = 7;

        Port _texturePort;
        public override void AddElements()
        {
            _texturePort = AddPort(new(PortDirection.Input, new Nodes.PortType.Texture2D(), TEX, "Texture 2D"));
            AddPort(new(PortDirection.Input, new Float(2), UV, "UV"));
            AddPort(new(PortDirection.Output, new Float(4), OUT_RGBA, "RGBA"));

            AddPort(new(PortDirection.Output, new Float(1), OUT_R, "R"));
            AddPort(new(PortDirection.Output, new Float(1), OUT_G, "G"));
            AddPort(new(PortDirection.Output, new Float(1), OUT_B, "B"));
            AddPort(new(PortDirection.Output, new Float(1), OUT_A, "A"));


            Bind(UV, PortBinding.UV0);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT_RGBA] = new GeneratedPortData(new Float(4), "TextureSample" + UniqueVariableID++);
            PortData[OUT_R] = new GeneratedPortData(new Float(1), "TextureSample_R" + UniqueVariableID++);
            PortData[OUT_G] = new GeneratedPortData(new Float(1), "TextureSample_G" + UniqueVariableID++);
            PortData[OUT_B] = new GeneratedPortData(new Float(1), "TextureSample_B" + UniqueVariableID++);
            PortData[OUT_A] = new GeneratedPortData(new Float(1), "TextureSample_A" + UniqueVariableID++);



            if (_texturePort.connected)
            {
                var propertyName = PortData[TEX].Name;
                visitor.AppendLine($"{PortData[OUT_RGBA].Type} {PortData[OUT_RGBA].Name} = {propertyName}.Sample(sampler{propertyName}, {PortData[UV].Name});");
            }
            else
            {
                visitor.AppendLine($"{PortData[OUT_RGBA].Type} {PortData[OUT_RGBA].Name} = float4(1,1,1,1);");
            }

            visitor.AppendLine($"{PortData[OUT_R].Type} {PortData[OUT_R].Name} = {PortData[OUT_RGBA].Name}.r;");
            visitor.AppendLine($"{PortData[OUT_G].Type} {PortData[OUT_G].Name} = {PortData[OUT_RGBA].Name}.g;");
            visitor.AppendLine($"{PortData[OUT_B].Type} {PortData[OUT_B].Name} = {PortData[OUT_RGBA].Name}.b;");
            visitor.AppendLine($"{PortData[OUT_A].Type} {PortData[OUT_A].Name} = {PortData[OUT_RGBA].Name}.a;");
        }
    }

    [NodeInfo("Function"), Serializable]
    public class CustomFunctionNode : ShaderNode
    {
        [SerializeField] string _code;
        [SerializeField] string _name;
        [SerializeField] List<PortDescriptor> _descriptors = new List<PortDescriptor>();
        const int OUT = 0;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(4), OUT));
            //string propertyName = GetVariableNameForPreview(OUT);
        }

        public override void AdditionalElements(VisualElement root)
        {
            var name = new TextField("Name")
            {
                value = _name
            };
            name.RegisterValueChangedCallback((evt) =>
            {
                _name = evt.newValue;
            });
            root.Add(name);

            var columns = new Columns();

            var portDir = new Column()
            {
                name = "dd",
                makeCell = () => new EnumField(PortDirection.Input),
                bindCell = (e, i) =>
                {
                    var field = e as EnumField;
                    field.value = _descriptors[i].Direction;

                    field.RegisterValueChangedCallback(evt =>
                    {
                        _descriptors[i].Direction = (PortDirection)evt.newValue;
                    });
                },
                width = 50
            };
            columns.Add(portDir);


            var ports = new MultiColumnListView(columns)
            {
                headerTitle = "Ports",
                showAddRemoveFooter = true,
                reorderMode = ListViewReorderMode.Animated,
                showFoldoutHeader = true,
                reorderable = true,
                itemsSource = _descriptors
            };
            root.Add(ports);

            var code = new TextField()
            {
                value = _code,
                multiline = true
            };
            code.RegisterValueChangedCallback((evt) =>
            {
                _code = evt.newValue;
            });
            root.Add(code);


        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            GeneratePreviewForAffectedNodes();
        }

        protected override void Generate(NodeVisitor visitor)
        {
            visitor.AddFunction(_code);

            PortData[OUT] = new GeneratedPortData(new Float(4, true), "Function" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = {_name}();");
        }
    }
}
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZSG.Nodes.PortType;
using static ZSG.ShaderBuilder;

namespace ZSG
{
    public enum GenerationMode
    {
        Preview,
        Final
    }

    public enum ShaderStage
    {
        Vertex,
        Fragment
    }

    public class NodeVisitor
    {
        public NodeVisitor(ShaderBuilder shaderBuilder, ShaderStage stage, int passIndex, string outputStruct)
        {
            _shaderBuilder = shaderBuilder;
            _function = shaderBuilder.passBuilders[passIndex].functions;
            _props = shaderBuilder.passBuilders[passIndex].properties;

            if (stage == ShaderStage.Vertex)
            {
                _expression = _shaderBuilder.passBuilders[passIndex].vertexDescription;
            }
            else if (stage == ShaderStage.Fragment)
            {
                _expression = _shaderBuilder.passBuilders[passIndex].surfaceDescription;
            }

            _expression.Add($"{outputStruct} output = ({outputStruct})0;");

            Pass = passIndex;
            Stage = stage;
        }

        internal ShaderBuilder _shaderBuilder;

        public GenerationMode GenerationMode => _shaderBuilder.GenerationMode;
        public ShaderStage Stage { get; }
        public int Pass {  get; }

        private List<string> _expression;
        private HashSet<string> _function;
        private HashSet<PropertyDescriptor> _props;

        private int _uniqueID = 0;
        /*public string GetInputVariable(int ID)
        {
            if (visitInfo.Name.TryGetValue(ID, out var name))
            {
                return name;
            }
            else
            {
                var newName = "Node" + _uniqueID++;
                visitInfo.Name[ID] = newName;
                return newName;
            }
        }

        public IPortType GetType(int ID) => visitInfo.PortType[ID];
        public Float GetTypeFloat(int ID) => (Float)visitInfo.PortType[ID];

        public void SetVariable(int ID, string name) => visitInfo.Name[ID] = name;
        public void SetOutputType(int ID, IPortType type)
        {
            if (type is Float @float)
            {
                //TODO: inherit
                @float.fullPrecision = true;
            }
            visitInfo.PortType[ID] = type;
        }

        public void OutputExpression(int outID, int lhs, string @operator, int rhs, string prefix = null)
        {
            var name = (prefix ?? "Node") + _uniqueID++;
            SetVariable(outID, name);
            AppendLine($"{GetTypeFloat(outID)} {name} = {GetVariable(lhs)} {@operator} {GetVariable(rhs)};");
        }

        public Float ImplicitTruncation(params int[] IDs)
        {
            int trunc = 4;
            int max = 1;
            for (int i = 0; i < IDs.Length; i++)
            {
                var ID = IDs[i];
                var type = GetTypeFloat(ID);
                var components = type.components;
                if (components == 1)
                {
                    continue;
                }
                max = Mathf.Max(max, components);
                trunc = Mathf.Min(trunc, components);
            }
            trunc = Mathf.Min(trunc, max);

            for (int i = 0; i < IDs.Length; i++)
            {
                var ID = IDs[i];
                Cast(ID, trunc);
            }

            return new Float(trunc);
        }

        public void Cast(int portID, int targetComponent)
        {
            var name = GetVariable(portID);
            var type = GetTypeFloat(portID);
            var components = type.components;
            string typeName = type.fullPrecision ? "float" : "half";

            if (components == targetComponent)
            {
                return;
            }

            // downcast
            if (components > targetComponent)
            {
                name = name + ".xyz"[..(targetComponent + 1)];
            }
            else
            {
                // upcast
                if (components == 1)
                {
                    // no need to upcast
                    // name = "(" + name + ").xxxx"[..(targetComponent + 2)];
                    return;
                }
                else if (components == 2)
                {
                    if (targetComponent == 3)
                    {
                        name = typeName + "3(" + name + ", 0)";
                    }
                    if (targetComponent == 4)
                    {
                        name = typeName + "4(" + name + ", 0, 0)";
                    }
                }
                else if (components == 3)
                {
                    if (targetComponent == 4)
                    {
                        name = typeName + "4(" + name + ", 0)";
                    }
                }
            }

            type.components = targetComponent;
            SetVariable(portID, name);
        }*/

        public void AppendLine(string value)
         {
             _expression.Add(value);
         }
        public void AddFunction(string function)
        {
            _function.Add(function);
        }
        public void AddProperty(PropertyDescriptor property)
        {
            _props.Add(property);
        }
    }
}

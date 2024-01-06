using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph
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

    public abstract class NodeVisitor
    {
        public NodeVisitor(ShaderBuilder shaderBuilder)
        {
            _shaderBuilder = shaderBuilder;
        }
        internal ShaderBuilder _shaderBuilder;
        public GenerationMode GenerationMode => _shaderBuilder.GenerationMode;

        public abstract void Visit(ShaderNode shaderNode);
    }

    interface IRequirePropertyVisitor
    {
        void VisitProperty(PropertyVisitor visitor);
    }

    interface IRequireDescriptionVisitor
    {
        void VisitDescription(DescriptionVisitor visitor);
    }
    interface IMayRequirePropertyVisitor : IRequirePropertyVisitor
    {
        bool IsProperty { get; set; }
    }
    interface IRequireFunctionVisitor
    {
        void VisitFunction(FunctionVisitor visitor);
    }

    public static class NodeVisitorExtensions
    {
        public static void Visit(this ShaderNode shaderNode, IEnumerable<NodeVisitor> visitors)
        {
            foreach (var visitor in visitors)
            {
                visitor.Visit(shaderNode);
            }
        }
    }

    public class DescriptionVisitor : NodeVisitor
    {
        public DescriptionVisitor(ShaderBuilder shaderBuilder, ShaderStage stage, int passIndex) : base(shaderBuilder)
        {
            Stage = stage;
            // PassIndex = passIndex;

            if (stage == ShaderStage.Vertex)
            {
                _target = _shaderBuilder.passBuilders[passIndex].vertexDescription;
            }
            else if (stage == ShaderStage.Fragment)
            {
                _target = _shaderBuilder.passBuilders[passIndex].surfaceDescription;
            }
        }

        private List<string> _target;

        public void AppendLine(string value)
        {
            _target.Add(value);
        }

        public override void Visit(ShaderNode shaderNode)
        {
            if (shaderNode is IRequireDescriptionVisitor node)
            {
                node.VisitDescription(this);
            }
        }

        public ShaderStage Stage { get; private set; }
        // private int PassIndex { get; set; }
    }

    public class PropertyVisitor : NodeVisitor
    {
        public PropertyVisitor(ShaderBuilder shaderBuilder) : base(shaderBuilder)
        {
        }

        public void AddProperty(string property)
        {
            _shaderBuilder.properties.Add(property);
        }

        public void AddProperty(PropertyDescriptor property)
        {
            _shaderBuilder.properties.Add(property.ToString());
        }

        public override void Visit(ShaderNode shaderNode)
        {
            if (shaderNode is IMayRequirePropertyVisitor mayRequiereProperty1 && !mayRequiereProperty1.IsProperty)
            {
                return;
            }
            if (shaderNode is IRequirePropertyVisitor node)
            {
                node.VisitProperty(this);
            }
        }
    }
    public class FunctionVisitor : NodeVisitor
    {
        public FunctionVisitor(ShaderBuilder shaderBuilder, int passIndex) : base(shaderBuilder)
        {
            _target = shaderBuilder.passBuilders[passIndex].functions;
        }

        private Dictionary<string, string> _target;

        public void AddFunction(string key, string function)
        {
            _target[key] = function;
        }

        public override void Visit(ShaderNode shaderNode)
        {
            if (shaderNode is IRequireFunctionVisitor node)
            {
                node.VisitFunction(this);
            }
        }
    }
}

using System;
using UnityEditor.Experimental.GraphView;
using z3y.ShaderGraph.Nodes;

public class ShaderGraphPort : Port
{
    public ShaderGraphPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
    {
    }
    public Action<Edge> onConnect = (edge) => { };
    public Action<Edge> onDisconnect = (edge) => { };

    public override void Connect(Edge edge)
    {
        onConnect(edge);
    }

    public override void Disconnect(Edge edge)
    {
        onDisconnect(edge);
    }

    internal static ShaderGraphPort Create<T>(Orientation horizontal, ShaderNode.Direction direction, Capacity capacity, Type type)
    {
        throw new NotImplementedException();
    }
}

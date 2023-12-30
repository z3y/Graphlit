using System;
using UnityEditor.Experimental.GraphView;

public class ShaderGraphPort : Port
{
    protected ShaderGraphPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
    {
    }
}

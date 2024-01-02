
using UnityEditor.Experimental.GraphView;

namespace z3y.ShaderGraph
{
    public static class Helpers
    {

    }

    public static class PortExtenstions
    {
        public static int GetPortID(this Port port)
        {
            return (int)port.userData;
        }
        public static void SetPortID(this Port port, int id)
        {
            port.userData = id;
        }
    }
}
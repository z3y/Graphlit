
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace z3y.ShaderGraph
{
    public static class Helpers
    {

        // https://stackoverflow.com/questions/6219454/efficient-way-to-remove-all-whitespace-from-string
        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }
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
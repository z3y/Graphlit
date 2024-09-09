using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [System.Serializable]
    public class PortDescriptor
    {
        public PortDescriptor(PortDirection direction, IPortType type, int id, string name = "")
        {
            Direction = direction;
            Type = type;
            ID = id;
            Name = name;
        }

        public PortDirection Direction { get; set; }
        public IPortType Type { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }

    }

    public enum PortDirection
    {
        Input,
        Output
    }
}
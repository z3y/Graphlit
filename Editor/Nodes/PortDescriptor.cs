using ZSG.Nodes.PortType;

namespace ZSG
{
    public class PortDescriptor
    {
        public PortDescriptor(PortDirection direction, IPortType type, int id, string name = "")
        {
            Direction = direction;
            Type = type;
            ID = id;
            Name = name;
        }

        public PortDirection Direction { get; }
        public IPortType Type { get; set; }
        public int ID { get; }
        public string Name { get; }

    }

    public enum PortDirection
    {
        Input,
        Output
    }
}
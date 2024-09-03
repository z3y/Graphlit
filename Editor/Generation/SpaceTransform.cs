namespace Graphlit
{
    public static class SpaceTransform
    {
        public static string ObjectToWorldPosition(string positionOS) => $"TransformObjectToWorld({positionOS})";
        public static string WorldToObjectPosition(string positionWS) => $"TransformWorldToObject({positionWS})";
        public static string ObjectToWorldNormal(string normalOS) => $"TransformObjectToWorldNormal({normalOS})";
        public static string WorldToObjectNormal(string normalWS) => $"TransformWorldToObjectNormal({normalWS})";
        public static string ObjectToWorldDirection(string directionOS) => $"TransformObjectToWorldDir({directionOS})";
        public static string WorldToObjectDirection(string directionOS) => $"TransformWorldToObjectDir({directionOS})";

        public enum Type
        {
            Position = 0,
            Normal = 1,
            Direction = 2
        }
        public static string TypeTotring(Type type)
        {
            return type switch
            {
                Type.Position => "",
                Type.Normal => "Normal",
                Type.Direction => "Dir",
                _ => throw new System.NotImplementedException(),
            };
        }
        public enum Space
        {
            Object = 0,
            World = 1,
        }

    }
}
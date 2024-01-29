namespace ZSG
{
    public static class SpaceTransform
    {
        public static string ObjectToWorld(string positionOS) => $"TransformObjectToWorld({positionOS})";
        public static string ObjectToWorldNormal(string normalOS) => $"TransformObjectToWorldNormal({normalOS})";
        public static string ObjectToWorldDirection(string directionOS) => $"TransformObjectToWorldDir({directionOS})";
    }
}
namespace Application.Cache
{
    public static class CacheKeys
    {
        public static string Orden(int id) => $"orden:{id}";
        public static string Producto(int id) => $"producto:{id}";
    }
}

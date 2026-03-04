namespace Domain.Services
{
    public static class DescuentoService
    {
        private const decimal DescTotal = 0.10m; // 10% por total
        private const decimal DescCant = 0.05m; // 5% por cantidad
        private const decimal UmbralTotal = 500m; // sobre $500
        
        public static decimal CalcDescuento(decimal total, int cantidad)
        {
            decimal descuento = 0;

            if (total > UmbralTotal)
                descuento += DescTotal;

            if (cantidad > 5)
                descuento += DescCant;

            return descuento;
        }

        public static decimal AplicarDescuento(decimal total, int cantidad)
        {
            var descuento = CalcDescuento(total, cantidad);
            return total * (1 - descuento);
        }
    }
}

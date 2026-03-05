using Domain.Services;

namespace Tests.Domain
{
    public class DescuentoServiceTests
    {
        // CalcDescuento

        [Fact]
        public void CalcDescuento_WhenTotalAndQuantityAreLow_ReturnsZero()
        {
            decimal total = 100m;
            int cantidad = 3;

            decimal descuento = DescuentoService.CalcDescuento(total, cantidad);

            Assert.Equal(0m, descuento);
        }

        [Fact]
        public void CalcDescuento_WhenTotalExceedsThreshold_Returns10Percent()
        {
            decimal total = 600m;
            int cantidad = 2;

            decimal descuento = DescuentoService.CalcDescuento(total, cantidad);

            Assert.Equal(0.10m, descuento);
        }

        [Fact]
        public void CalcDescuento_WhenQuantityExceedsThreshold_Returns5Percent()
        {
            decimal total = 200m;
            int cantidad = 6;

            decimal descuento = DescuentoService.CalcDescuento(total, cantidad);

            Assert.Equal(0.05m, descuento);
        }

        [Fact]
        public void CalcDescuento_WhenBothThresholdsExceeded_Returns15Percent()
        {
            decimal total = 800m;
            int cantidad = 7;

            decimal descuento = DescuentoService.CalcDescuento(total, cantidad);

            Assert.Equal(0.15m, descuento);
        }

        [Fact]
        public void CalcDescuento_WhenTotalIsExactly500_ReturnsNoDiscount()
        {
            decimal descuento = DescuentoService.CalcDescuento(500m, 3);

            Assert.Equal(0m, descuento);
        }

        [Fact]
        public void CalcDescuento_WhenQuantityIsExactly5_ReturnsNoDiscount()
        {
            decimal descuento = DescuentoService.CalcDescuento(100m, 5);

            Assert.Equal(0m, descuento);
        }

        // AplicarDescuento()

        [Fact]
        public void AplicarDescuento_WithNoDiscount_ReturnsTotalUnchanged()
        {
            decimal resultado = DescuentoService.AplicarDescuento(100m, 2);

            Assert.Equal(100m, resultado);
        }

        [Fact]
        public void AplicarDescuento_With10Percent_DiscountCorrectly()
        {
            decimal resultado = DescuentoService.AplicarDescuento(600m, 2);

            Assert.Equal(540m, resultado); // 600 * 0.90 = 540
        }

        [Fact]
        public void AplicarDescuento_With5Percent_DiscountCorrectly()
        {
            decimal resultado = DescuentoService.AplicarDescuento(200m, 6);

            Assert.Equal(190m, resultado); // 200 * 0.95 = 190
        }

        [Fact]
        public void AplicarDescuento_With15Percent_DiscountCorrectly()
        {            decimal resultado = DescuentoService.AplicarDescuento(1000m, 7);

            Assert.Equal(850m, resultado); // 1000 * 0.85 = 850
        }

        [Fact]
        public void AplicarDescuento_WithZeroTotal_ReturnsZero()
        {
            decimal resultado = DescuentoService.AplicarDescuento(0m, 10);

            Assert.Equal(0m, resultado);
        }

        [Theory]
        [InlineData(500, 5, 0.0)]   // exactamente en el umbral → sin descuento
        [InlineData(500.01, 5, 0.10)]  // apenas supera → 10%
        [InlineData(500, 6, 0.05)]  // total en umbral, cantidad supera → 5%
        [InlineData(600, 7, 0.15)]  // ambos superan → 15%
        [InlineData(0, 0, 0.0)]   // todo en cero
        public void CalcDescuento_VariousCases_ReturnsCorrectDiscount(
            decimal total, int cantidad, double descuentoEsperado)
        {
            decimal resultado = DescuentoService.CalcDescuento(total, cantidad);

            Assert.Equal((decimal)descuentoEsperado, resultado);
        }
    }
}

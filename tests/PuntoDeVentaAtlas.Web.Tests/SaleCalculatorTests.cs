using PuntoDeVentaAtlas.Web.Domain;
using Xunit;
namespace PuntoDeVentaAtlas.Web.Tests;
public sealed class SaleCalculatorTests
{
    [Fact] public void CalculatesWeightedProductsTaxAndDiscount(){var result=SaleCalculator.Calculate([(46.90m,.250m,0m),(149m,1m,.16m)],10m);Assert.Equal(160.73m,result.Subtotal);Assert.Equal(16.07m,result.Discount);Assert.Equal(23.84m,result.Tax);Assert.Equal(168.50m,result.Total);}
    [Fact] public void UsesCommercialMidpointRounding(){var result=SaleCalculator.Calculate([(46.90m,.250m,0m)],0);Assert.Equal(11.73m,result.Subtotal);}
    [Fact] public void CapsDiscountAtOneHundredPercent(){var result=SaleCalculator.Calculate([(100m,1m,0m)],150m);Assert.Equal(0m,result.Total);}
    [Fact] public void RejectsZeroQuantity()=>Assert.Throws<ArgumentOutOfRangeException>(()=>SaleCalculator.Calculate([(10m,0m,0m)],0));
}

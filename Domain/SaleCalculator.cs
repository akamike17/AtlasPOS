namespace PuntoDeVentaAtlas.Web.Domain;
public sealed record SaleTotals(decimal Subtotal,decimal Discount,decimal Tax,decimal Total);
public static class SaleCalculator
{
    public static SaleTotals Calculate(IEnumerable<(decimal Price,decimal Quantity,decimal TaxRate)> lines,decimal discountPercent)
    {
        decimal subtotal=0,tax=0;foreach(var line in lines){if(line.Price<0||line.Quantity<=0||line.TaxRate<0)throw new ArgumentOutOfRangeException(nameof(lines));var amount=Round(line.Price*line.Quantity);subtotal+=amount;tax+=Round(amount*line.TaxRate);}subtotal=Round(subtotal);tax=Round(tax);var discount=Round(subtotal*Math.Clamp(discountPercent,0,100)/100);return new(subtotal,discount,tax,subtotal-discount+tax);
    }
    public static decimal Round(decimal value)=>decimal.Round(value,2,MidpointRounding.AwayFromZero);
}

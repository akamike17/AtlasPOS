using PuntoDeVentaAtlas.Web.Models;

namespace PuntoDeVentaAtlas.Web.Services;

public interface IPointOfSaleService
{
    DashboardViewModel Dashboard();
    SaleResult Checkout(SaleRequest request);
    IReadOnlyList<DeviceStatus> Devices();
}

public sealed class PointOfSaleService : IPointOfSaleService
{
    private readonly object _gate = new();
    private readonly List<Product> _products =
    [
        new(1,"CAF-001","7501001000011","Café artesanal 500 g","Abarrotes",149.00m,24,"pza",false,0.16m,"#c96f3b"),
        new(2,"LEC-001","7501001000028","Leche entera 1 L","Lácteos",29.50m,48,"pza",false,0m,"#5a8dee"),
        new(3,"MAN-KG","2000000001012","Manzana Gala","Frutas y verduras",46.90m,18.75m,"kg",true,0m,"#ef6262"),
        new(4,"PAN-001","7501001000042","Pan integral","Panadería",54.00m,12,"pza",false,0m,"#d49a62"),
        new(5,"REF-600","7501001000059","Refresco 600 ml","Bebidas",22.00m,6,"pza",false,0.16m,"#8769d4"),
        new(6,"JAB-001","7501001000066","Jabón líquido 500 ml","Limpieza",67.50m,31,"pza",false,0.16m,"#33a6a6"),
        new(7,"QUE-KG","2000000001074","Queso manchego","Lácteos",189.00m,8.4m,"kg",true,0m,"#e0ad38"),
        new(8,"GAL-001","7501001000080","Galletas de avena","Abarrotes",38.00m,19,"pza",false,0.16m,"#bc7a54")
    ];
    private readonly List<Customer> _customers =
    [
        new(1,"Público general","XAXX010101000","",""),
        new(2,"Mariana López","LOPM850312AB2","mariana@correo.mx","55 1234 5678"),
        new(3,"Comercial del Centro SA de CV","CCE201015KQ4","compras@comercial.mx","55 8765 4321")
    ];
    private readonly List<SaleResult> _sales = [];
    private int _sequence = 1047;

    public DashboardViewModel Dashboard()
    {
        lock (_gate)
        {
            var today = _sales.Where(x => x.CreatedAt.Date == DateTime.Today).ToList();
            var total=_customers.Count; var fiscal=_customers.Count(x=>!string.IsNullOrWhiteSpace(x.Rfc));
            var customerDashboard=new CustomerDashboard(total,fiscal,_customers.Count(x=>!string.IsNullOrWhiteSpace(x.Email)),_customers.Count(x=>!string.IsNullOrWhiteSpace(x.Phone)),fiscal,[new("Con RFC",fiscal,total==0?0:fiscal*100m/total)]);
            return new(_products.ToList(), _customers.ToList(), _sales.TakeLast(8).Reverse().ToList(),
                today.Sum(x => x.Total), today.Count, _products.Count(x => x.Stock <= 10),customerDashboard);
        }
    }

    public SaleResult Checkout(SaleRequest request)
    {
        lock (_gate)
        {
            if (request.Lines.Count == 0) throw new InvalidOperationException("Agrega al menos un producto.");
            var lines = new List<SaleTicketLine>();
            decimal subtotal = 0, tax = 0;
            foreach (var item in request.Lines)
            {
                var product = _products.FirstOrDefault(x => x.Id == item.ProductId)
                    ?? throw new InvalidOperationException("Uno de los productos ya no existe.");
                if (item.Quantity <= 0 || item.Quantity > product.Stock)
                    throw new InvalidOperationException($"Existencia insuficiente de {product.Name}.");
                var baseAmount = decimal.Round(product.Price * item.Quantity, 2, MidpointRounding.AwayFromZero);
                subtotal += baseAmount;
                tax += decimal.Round(baseAmount * product.TaxRate, 2, MidpointRounding.AwayFromZero);
                lines.Add(new(product.Name, item.Quantity, product.Unit, product.Price, baseAmount + decimal.Round(baseAmount * product.TaxRate, 2, MidpointRounding.AwayFromZero)));
            }
            var discount = decimal.Round(subtotal * Math.Clamp(request.DiscountPercent, 0, 100) / 100, 2, MidpointRounding.AwayFromZero);
            var total = subtotal - discount + tax;
            var paid = request.Payments.Sum(x => x.Amount);
            if (paid < total) throw new InvalidOperationException("El pago no cubre el total de la venta.");
            foreach (var item in request.Lines)
            {
                var index = _products.FindIndex(x => x.Id == item.ProductId);
                _products[index] = _products[index] with { Stock = _products[index].Stock - item.Quantity };
            }
            var result = new SaleResult($"V-{++_sequence:000000}", DateTime.Now, subtotal, discount, tax,
                total, paid, paid - total, lines, request.Payments.ToList(), "Administrador");
            _sales.Add(result);
            return result;
        }
    }

    public IReadOnlyList<DeviceStatus> Devices() =>
    [
        new("scanner","Escáner de códigos","HID / teclado",true,"Captura directa en búsqueda","Entrada HID estándar","Peripherals:Scanner",false),
        new("printer","Impresora de tickets","ESC/POS",false,"Puerto listo para USB, red o Bluetooth","ITicketPrinter","Peripherals:Printer",true),
        new("scale","Báscula electrónica","Serial / USB",false,"Adaptador aislado para protocolo del fabricante","IScaleConnector","Peripherals:Scale",true),
        new("pinpad","Terminal bancaria","API del adquirente",false,"Integrada, semiintegrada o liga de pago","ICardPaymentConnector","Peripherals:Card",true),
        new("signature","Firma del cliente","Canvas / biométrico",true,"Canvas disponible; SDK biométrico intercambiable","ISignaturePad","Peripherals:Signature",true),
        new("invoice","Facturación CFDI 4.0","PAC",false,"Contrato listo para credenciales del PAC","IInvoiceConnector","Peripherals:Invoice",true)
    ];
}

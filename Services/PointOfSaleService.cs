using PuntoDeVentaAtlas.Web.Models;

namespace PuntoDeVentaAtlas.Web.Services;

public interface IDeviceCatalogService
{
    IReadOnlyList<DeviceStatus> Devices();
}

// Este adapter sólo describe dispositivos. El comercio/persistencia vive en IMySqlPointOfSaleService.
public sealed class DeviceCatalogService : IDeviceCatalogService
{
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

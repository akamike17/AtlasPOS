namespace PuntoDeVentaAtlas.Web.Integrations;

// Puertos estables: cada proveedor implementa uno sin contaminar el flujo de venta.
public enum CardConnectionMode { IntegratedPinPad, SemiIntegratedTerminal, PaymentLink }
public sealed record CardCharge(decimal Amount, string Currency, string SaleFolio, CardConnectionMode Mode);
public sealed record CardChargeResult(bool Approved, string Authorization, string Reference, string Message);
public interface ICardPaymentConnector { Task<CardChargeResult> ChargeAsync(CardCharge charge, CancellationToken ct); }

public sealed record FiscalCustomer(string Rfc, string LegalName, string FiscalRegime, string ZipCode, string CfdiUse, string Email);
public sealed record InvoiceResult(bool Stamped, string? Uuid, byte[]? Pdf, byte[]? Xml, string Message);
public interface IInvoiceConnector { Task<InvoiceResult> StampAsync(string saleFolio, FiscalCustomer customer, CancellationToken ct); }

public interface IScaleConnector { Task<decimal> ReadKilogramsAsync(CancellationToken ct); }
public interface ITicketPrinter { Task PrintAsync(string escPosDocument, CancellationToken ct); }
public interface ISignaturePad { Task<SignatureCapture> CaptureAsync(CancellationToken ct); }
public sealed record SignatureCapture(byte[] Image, byte[]? BiometricData, string DeviceId, DateTime CapturedAt);

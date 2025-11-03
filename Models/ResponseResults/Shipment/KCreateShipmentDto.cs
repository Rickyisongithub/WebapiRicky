// ReSharper disable InconsistentNaming
namespace KairosWebAPI.Models.ResponseResults.Shipment
{
    public class KCreateShipmentDto
    {
        public string? Company { get; set; }
        public int PackNum { get; set; }
        public DateTime? ShipDate { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShipStatus { get; set; }
        public decimal? ChrgAmount { get; set; }
        public decimal? OTSOrderNum { get; set; }
        public decimal? BTCustNum { get; set; }
        public decimal? CustNum { get; set; }
        public decimal? ShipToCustNum { get; set; }
        public string? BTCustID { get; set; }
        public string? CurrencyCode { get; set; }
        public string? ServRef1 { get; set; }
       public List<ShipDtlCreateDtp>? ShipDtls { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ShipDtlCreateDtp
    {
        public string? Company { get; set; }
        public int PackNum { get; set; }
        public decimal PackLine { get; set; }
        public decimal? OrderNum { get; set; }
        public int OrderLine { get; set; }
        public int OrderRelNum { get; set; }
        public string? LineType { get; set; }
        public decimal OurInventoryShipQty { get; set; }
        public int Packages { get; set; }
        public string? PartNum { get; set; }
        public string? LineDesc { get; set; }
        public string? IUM { get; set; }
        public bool ShipCmpl { get; set; }
        public decimal? CustNum { get; set; }
        public string? InventoryShipUOM { get; set; }
        public string? WarehouseCode { get; set; }
        public string? BinNum { get; set; }
        public string? PackNumShipStatus { get; set; }
        public bool ReadyToInvoice { get; set; }
        public decimal? SellingInventoryShipQty { get; set; }
        public decimal? SellingFactor { get; set; }
        public string? SalesUM { get; set; }
        public string? SellingFactorDirection { get; set; }
        public decimal? ShipToCustNum { get; set; }
        public decimal? MFCustNum { get; set; }
        public string? MFShipToNum { get; set; }
        public decimal? DisplayInvQty { get; set; }
        public string? MarkForAddrList { get; set; }
        public string? MFCustID { get; set; }
        public bool OrderHold { get; set; }
        public string? OurReqUM { get; set; }
        public DateTime? RequestDate { get; set; }
        public string? CustNumCustID { get; set; }
        public string? OrderNumPSCurrCode { get; set; }
        public string? OrderNumCurrencyCode { get; set; }
        public string? CurrencyCode { get; set; }
        public string? OrderLineProdCode { get; set; }
        public string? PartNumPricePerCode { get; set; }
        public decimal? PartNumSellingFactor { get; set; }
        public string? PartNumSalesUM { get; set; }
        public string? PartNumIUM { get; set; }
        public string? LotNum { get; set; }
        public bool Invoiced { get; set; }
    }


}

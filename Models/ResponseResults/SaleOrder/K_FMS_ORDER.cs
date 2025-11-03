// ReSharper disable InconsistentNaming
namespace KairosWebAPI.Models.ResponseResults.SaleOrder
{
    public class KFmsOrder
    {
        public int OrderHed_OrderNum { get; set; }
        public string? OrderHed_Company { get; set; }
        public double OrderHed_OrderAmt { get; set; }
        public DateTime? OrderHed_OrderDate { get; set; }
        public DateTime? OrderHed_NeedByDate { get; set; }
        public bool OrderHed_UseOTS { get; set; }
        public string? OrderHed_OTSName { get; set; }
        public string? OrderHed_OTSAddress1 { get; set; }
        public string? OrderHed_OTSAddress2 { get; set; }
        public string? OrderHed_OTSCity { get; set; }
        public string? OrderHed_PONum { get; set; }
        public DateTime? OrderHed_RequestDate { get; set; }
        public string? OrderHed_OrderStatus { get; set; }
        public string? OrderHed_EntryPerson { get; set; }
        public double OrderDtl_UnitPrice { get; set; }
        public string? OrderDtl_PartNum { get; set; }
        public string? OrderDtl_POLine { get; set; }
        public string? OrderDtl_LineDesc { get; set; }
        public string? OrderDtl_LineStatus { get; set; }
        public string? OrderDtl_LineType { get; set; }
        public double OrderDtl_OrderQty { get; set; }
        public string? Customer_Address1 { get; set; }
        public string? Customer_CustID { get; set; }
        public int Customer_CustNum { get; set; }
        public string? Customer_Name { get; set; }
        public string? OrderHed_SysRowID { get; set; }
        public decimal? OrderRel_OurReqQty { get; set; }
        public string? OrderRel_OTSName { get; set; }
        public string? OrderRel_OTSAddress1 { get; set; }
        public string? OrderRel_OTSAddress2 { get; set; }
        public string? OrderRel_OTSAddress3 { get; set; }
        public string? OrderRel_OTSCity { get; set; }
        public string? OrderRel_OTSState { get; set; }
        public string? OrderRel_OTSZIP { get; set; }
        public int? OrderRel_OrderRelNum { get; set; }
        public int? OrderDtl_OrderLine { get; set; }
        public string? OrderRel_KFMS_EndLocation_c { get; set; }
        public bool OrderRel_KFMS_SubJourney_c { get; set; }
    }
}

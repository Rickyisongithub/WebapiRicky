using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;
using KairosWebAPI.Models.ResponseResults.SaleOrder;
using KairosWebAPI.Models.ResponseResults.Shipment;

namespace KairosWebAPI.Services.Order
{
    public interface IOrderService
    {
        Task<ServiceResponse<KListResponse<SalesOrderResponse>>> GetOrderList(FilterParams filters);
        Task<ServiceResponse<SalesOrderResponse>> GetOrderByNumber(string company, decimal orderNumber);
        Task<ServiceResponse<KListResponse<KFmsOrder>>> GetFmsOrders(FMSOrderParams filters);
        Task<ServiceResponse<AttachmentsResponse<AttachmentsValue>>> GetAttachments(int orderNum);
        Task<ServiceResponse<string>> SaveAttachment(SaveAttachmentDto model, string fileUrl);
        Task<ServiceResponse<DocTypeResponse<DocTypeValue>>> GetDocType(string company);
        Task<ServiceResponse<string>> UpdateOrder(SalesOrderResponse model, string company, int orderNumber);
        Task<ServiceResponse<SalesOrderResponse>> UpdateOrderStatus(string company, int orderNumber, string status, string partNum, int hoursWorked);
        Task<ServiceResponse<KShipmentResponse>> CreateShipment(KCreateShipmentDto model);
        Task<ServiceResponse<KShipmentReportResponse>> GetShipmentReportBytesString(int packNum);
    }
}

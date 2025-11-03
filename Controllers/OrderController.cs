using KairosWebAPI.AuthorizationFilter;
using KairosWebAPI.DatabaseContext;
using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;
using KairosWebAPI.Models.ResponseResults.SaleOrder;
using KairosWebAPI.Services.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KairosWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(KAuthorizationFilter))]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;
        public OrderController(IOrderService orderService,AppDbContext context)
        {
            this._orderService = orderService;
            _context = context;
            
        }

        [HttpGet("get-orders")]
        public async Task<IActionResult> GetFmsOrders([FromQuery] FMSOrderParams filters)
        {
            var orders = await _orderService.GetFmsOrders(filters);
            if (orders.Data is not { value: not null })
                return BadRequest(ServiceResponse<List<KFmsOrder>>.ReturnFailed(orders.StatusCode, orders.Errors));
            var order = orders.Data.value;
            var result = await _context.Journeys.Where(x => x.StartDate!.Value.Date == filters.OrderDate.Date).Select(x => x.OrderNum).ToListAsync();

            var ordersToReturn = order.Where(x => !result.Contains(x.OrderHed_OrderNum)).ToList();
            var groupedOrderToReturn = ordersToReturn.GroupBy(c => c.OrderHed_OrderNum).Select(c => new
            {
                OrderNum = c.Key,
                Details = c
            }).ToList();
            return Ok(ServiceResponse<dynamic>.ReturnResultWith200(groupedOrderToReturn));
        }

        [HttpGet("GetAttachments")]
        public async Task<IActionResult> GetAttachments(int orderNum)
        {
            var attachments = await _orderService.GetAttachments(orderNum);
            if (attachments.Data != null && attachments.Data != null)
            {
                return Ok(ServiceResponse<AttachmentsResponse<AttachmentsValue>>.ReturnResultWith200(attachments.Data));
            }

            return BadRequest(ServiceResponse<AttachmentsResponse<AttachmentsValue>>.ReturnFailed(attachments.StatusCode, attachments.Errors));
        }

        [HttpPost("SaveAttachment")]
        public async Task<IActionResult> SaveAttachment(SaveAttachmentDto saveAttachmentDto)
        {
            
            var fileUrl = this.Request.Scheme + "://" + this.Request.Host + $"/kairoswebapi/Files/";
            var saveAttachmentSvc = await _orderService.SaveAttachment(saveAttachmentDto, fileUrl);
            if(saveAttachmentSvc.StatusCode == StatusCodes.Status201Created)
            {
                return Ok(ServiceResponse<string>.ReturnResultWith200(saveAttachmentSvc.Data ?? "Done"));
            }
            return BadRequest(ServiceResponse<string>.ReturnFailed(saveAttachmentSvc.StatusCode, saveAttachmentSvc.Errors));
        }

        [HttpGet("GetDocType")]
        public async Task<IActionResult> GetDocType(string company)
        {
            var docTypes = await _orderService.GetDocType(company);
            if (docTypes.Data != null && docTypes.Data.value != null)
            {
                return Ok(ServiceResponse<DocTypeResponse<DocTypeValue>>.ReturnResultWith200(docTypes.Data));
            }

            return BadRequest(ServiceResponse<DocTypeResponse<DocTypeValue>>.ReturnFailed(docTypes.StatusCode, docTypes.Errors));
        }
    }
}

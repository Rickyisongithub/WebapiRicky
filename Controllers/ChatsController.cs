using KairosWebAPI.Models.Dto;
using KairosWebAPI.Services;
using KairosWebAPI.Services.ChatService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KairosWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _service;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatsController(IChatService service, IHubContext<ChatHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }
        // GET: api/<ChatsController>
        [HttpGet("{truckId}")]
        public async Task<IActionResult> Get(string truckId)
        {
            return Ok(await _service.GetAllChats(truckId));
        }

        [HttpPost("ChatWithFile")]
        public async Task<IActionResult> Post([FromForm] CreateChatDto model)
            {
            var result = await _service.AddChat1(model);
            if (result.Success && result.Data != null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveOne", result.Data.TruckId, result.Data.Message);
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ChatWithFileType")]
        public async Task<IActionResult> Post2([FromForm] CreateChatDto model)
            {
            var result = await _service.AddChat2(model);
            if (result.Success && result.Data != null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveOne", result.Data.TruckId, result.Data.Message);
                return Ok(result);
            }
            return BadRequest(result);
        }

        // PUT api/<ChatsController>/5
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UpdateChatDto value)
        {
            return Ok(await _service.UpdateChat(value));
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateChatStatusDto value)
        {
            return Ok(await _service.UpdateChatStatus(value));
        }

        // DELETE api/<ChatsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            return Ok(await _service.DeleteChat(id));
        }

        //[Authorize]
        //[HttpGet("/Download/File")]
        //public IActionResult DownloadFile(string filename)
        //{
        //    try
        //    {
        //        if (filename.Contains(".."))
        //        {
        //            return NotFound();
        //        }

        //        var filePath = Path.Combine(uploadDirectoryPath, filename);

        //        if (!System.IO.File.Exists(filePath))
        //        {
        //            return NotFound();
        //        }

        //        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        //        return File(fileBytes, "application/octet-stream", filename);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Internal server error");
        //    }
        //}


    }
}

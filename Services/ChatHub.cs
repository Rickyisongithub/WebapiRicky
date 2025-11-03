using KairosWebAPI.Models.Dto;
using KairosWebAPI.Services.ChatService;
using Microsoft.AspNetCore.SignalR;

namespace KairosWebAPI.Services
{
    public class ChatHub :Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }
        public async Task SendMessage(CreateChatDto message)
        {
            var response = await _chatService.AddChat(message);
            if(response.Success && response.Data != null)
            {
                await Clients.All.SendAsync("ReceiveMessage", message);

            }
        }
        public Task SendMessage1(string user, string message)               // Two parameters accepted
        {
            return Clients.All.SendAsync("ReceiveOne", user, message);    // Note this 'ReceiveOne' 
        }
    }
}

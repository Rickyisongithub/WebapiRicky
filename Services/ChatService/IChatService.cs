using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;

namespace KairosWebAPI.Services.ChatService
{
    public interface IChatService
    {
        Task<ServiceResponse<Chat>> AddChat(CreateChatDto dto);
        Task<ServiceResponse<Chat>> AddChat1(CreateChatDto dto);
        Task<ServiceResponse<Chat>> AddChat2(CreateChatDto dto);
        Task<ServiceResponse<Chat>> UpdateChat(UpdateChatDto dto);
        Task<ServiceResponse<Chat>> UpdateChatStatus(UpdateChatStatusDto dto);
        Task<ServiceResponse<string>> DeleteChat(long id);
        Task<ServiceResponse<List<Chat>>> GetAllChats(string vehicleId);
        Task<string?> WriteFile(IFormFile file);

    }
}

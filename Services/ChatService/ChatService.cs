using KairosWebAPI.DatabaseContext;
using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KairosWebAPI.Services.ChatService
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<Chat>> AddChat(CreateChatDto dto)
        {
            Chat chat = new()
            {
                TruckId = dto.TruckId,
                Status = dto.Status,
                Type = dto.Type, //Admin,User
                Message = dto.Message,
                TimeStamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                CreatedBy = dto.Type,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
            };
            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            return ServiceResponse<Chat>.ReturnResultWith201(chat);
        }
        // Add file with chat
        //returns file URL
        public async Task<ServiceResponse<Chat>> AddChat1([FromBody][Bind(nameof(CreateChatDto.TruckId), nameof(CreateChatDto.Type), nameof(CreateChatDto.Status), nameof(CreateChatDto.Message), nameof(CreateChatDto.File))] CreateChatDto dto)
        {
            string? fileUrl = null;
            if (dto.File is { Length: > 0 })
            {
                fileUrl = await WriteFile(dto.File);
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return ServiceResponse<Chat>.Return404("File upload failed.");
                }
            }

            Chat chat = new()
            {
                TruckId = dto.TruckId,
                Status = dto.Status,
                Type = dto.Type, // Admin, User
                Message = dto.Message,
                TimeStamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                CreatedBy = dto.Type,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                FileUrl = fileUrl
            };

            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            return ServiceResponse<Chat>.ReturnResultWith201(chat);
        }

        //Add file and file type with chat
        //returns 
        public async Task<ServiceResponse<Chat>> AddChat2(CreateChatDto dto)
        {
            string? fileUrl = null;
            if (dto.File is { Length: > 0 })
            {
                fileUrl = await WriteFileWithFileType(dto.File);
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return ServiceResponse<Chat>.Return404("File upload failed.");
                }
            }

            Chat chat = new()
            {
                TruckId = dto.TruckId,
                Status = dto.Status,
                Type = dto.Type, // Admin, User
                Message = dto.Message,
                TimeStamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                CreatedBy = dto.Type,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                FileUrl = fileUrl,
                FileType = dto.FileType
            };

            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            return ServiceResponse<Chat>.ReturnResultWith201(chat);
        }

        public async Task<ServiceResponse<Chat>> UpdateChat(UpdateChatDto dto)
        {
            var dbMessage  = await _context.Chats.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (dbMessage == null) return ServiceResponse<Chat>.Return422("Record Not Found");
            dbMessage.TruckId = dto.TruckId;
            dbMessage.Status = dto.Status;
            dbMessage.Type = dto.Type;
            dbMessage.Message = dto.Message;
            dbMessage.UpdatedDate = DateTime.Now;

            _context.Chats.Update(dbMessage);
            await _context.SaveChangesAsync();

            return ServiceResponse<Chat>.ReturnResultWith200(dbMessage);
        }

        public async Task<ServiceResponse<Chat>> UpdateChatStatus(UpdateChatStatusDto dto)
        {
            var dbMessage = await _context.Chats.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (dbMessage == null) return ServiceResponse<Chat>.Return422("Record Not Found");
            dbMessage.Status = dto.Status;
            dbMessage.UpdatedDate = DateTime.Now;
            _context.Chats.Update(dbMessage);
            await _context.SaveChangesAsync();

            return ServiceResponse<Chat>.ReturnResultWith200(dbMessage);

        }

        public async Task<ServiceResponse<string>> DeleteChat(long id)
        {
            var dbMessage = await _context.Chats.FirstOrDefaultAsync(x => x.Id == id);
            if (dbMessage == null) return ServiceResponse<string>.Return422("Record Not Found");
            _context.Chats.Remove(dbMessage);
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.ReturnResultWith200("Record Deleted Successfully");

        }

        public async Task<ServiceResponse<List<Chat>>> GetAllChats(string vehicleId)
        {
           var list = 
                await _context.Chats.Where(x => x.TruckId == vehicleId)
                .AsNoTracking()
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();

            return ServiceResponse<List<Chat>>.ReturnResultWith200(list);
        }

        public async Task<string?> WriteFile(IFormFile file)
        {
            var imageUrl = "";
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                var filename = DateTime.Now.Ticks.ToString() + extension;

                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload\\Files");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                var exactPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload\\Files", filename);
                await using (var stream = new FileStream(exactPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                imageUrl = "/Upload/Files/" + filename;
            }
            catch (Exception)
            {
                // ignored
            }

            return imageUrl;
        }

        private async Task<string?> WriteFileWithFileType(IFormFile file)
        {
            var imageUrl = "";
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                var filename = DateTime.Now.Ticks.ToString() + extension;

                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                var exactPath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files", filename);
                await using (var stream = new FileStream(exactPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // ImageURL = imgPath + "/Upload/Files/" + filename;
                imageUrl = "/Upload/Files/" + filename;
            }
            catch (Exception)
            {
                // ignored
            }

            //return filename;
            return imageUrl;
        }

    }
}

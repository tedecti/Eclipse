using System.Collections;
using Eclipse.Exceptions;
using Eclipse.Middlewares;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio.Exceptions;
using Message = NuGet.Protocol.Plugins.Message;

namespace Eclipse.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatRoomController : ControllerBase
{
    private readonly IChatRepository _chatRepository;

    public ChatRoomController(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    [HttpPost("create")]
    public async Task<ApiResponse<ChatRoom>> CreateChatRoom(Guid targetUserId)
    {
        try
        {
            var currentUserId = Guid.Parse(User.FindFirst("UserId").Value);
            var chatRoom = await _chatRepository.CreateOrGetChatRoomAsync(currentUserId, targetUserId);
            return new ApiResponse<ChatRoom>{ Message = "Success", Data = chatRoom };
        }
        catch (NotFoundException)
        {
            throw new NotFoundException("User");
        }
    }

    [HttpGet("list")]
    public async Task<ApiResponse<IEnumerable<ChatRoomDto>>> GetUserChatRooms()
    {
        var currentUserId = Guid.Parse(User.FindFirst("UserId").Value);
        var chatRooms = await _chatRepository.GetUserChatRoomsAsync(currentUserId);

        var chatRoomDtos = chatRooms.Select(cr => new ChatRoomDto
        {
            Id = cr.Id,
            OtherUser = cr.UserId1 == currentUserId
                ? new UserDtoForChats(cr.User2)
                : new UserDtoForChats(cr.User1),
            LastMessage = cr.Messages
                .OrderByDescending(m => m.Timestamp)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    MessageText = m.MessageText,
                    Timestamp = m.Timestamp.ToLongDateString(),
                    SenderId = m.SenderId
                })
                .FirstOrDefault(),
            UnreadCount = cr.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId)
        });

        return new ApiResponse<IEnumerable<ChatRoomDto>>
        {
            Message = "Success",
            Data = chatRoomDtos
        };
    }


    [HttpGet("{chatRoomId}")]
    public async Task<ApiResponse<ChatRoom>> GetChatRoom(Guid chatRoomId)
    {
        var currentUserId = Guid.Parse(User.FindFirst("UserId").Value);

        if (!await _chatRepository.ValidateChatRoomAccessAsync(chatRoomId, currentUserId))
        {
            throw new ForbiddenException();
        }

        var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId);
        if (chatRoom == null)
        {
            throw new NotFoundException("Room");
        }

        return new ApiResponse<ChatRoom> { Message = "Success", Data = chatRoom };
    }
}
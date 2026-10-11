using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Models;
using CheckCarsAPI.Data;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CheckCarsAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly NotificationService _notificationService;

    public ChatsController(
        ApplicationDbContext context,
        IHubContext<ChatHub> hubContext,
        NotificationService notificationService)
    {
        _context = context;
        _hubContext = hubContext;
        _notificationService = notificationService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Returns the chats the current user participates in, with a preview of the last message.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyChats()
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var chats = await _context.Chats
            .Where(c => c.Users.Any(u => u.Id == userId))
            .Select(c => new
            {
                c.ChatId,
                c.Name,
                Participants = c.Users.Select(u => new { u.Id, u.UserName, u.Email, u.AvatarUrl }),
                LastMessage = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => new { m.MessageId, m.SenderId, m.Content, m.SentAt })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(chats);
    }

    /// <summary>
    /// Returns a single chat (with its participants) if the current user belongs to it.
    /// </summary>
    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetChat(int chatId)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var chat = await _context.Chats
            .Where(c => c.ChatId == chatId && c.Users.Any(u => u.Id == userId))
            .Select(c => new
            {
                c.ChatId,
                c.Name,
                Participants = c.Users.Select(u => new { u.Id, u.UserName, u.Email, u.AvatarUrl })
            })
            .FirstOrDefaultAsync();

        if (chat == null)
        {
            return NotFound();
        }

        return Ok(chat);
    }

    /// <summary>
    /// Returns the message history of a chat (newest first, paginated) for a member.
    /// </summary>
    [HttpGet("{chatId}/Messages")]
    public async Task<IActionResult> GetMessages(int chatId, [FromQuery] int take = 50, [FromQuery] int skip = 0)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var isMember = await _context.Chats
            .AnyAsync(c => c.ChatId == chatId && c.Users.Any(u => u.Id == userId));
        if (!isMember)
        {
            return Forbid();
        }

        take = Math.Clamp(take, 1, 200);
        skip = Math.Max(skip, 0);

        var messages = await _context.Messages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .Select(m => new { m.MessageId, m.ChatId, m.SenderId, m.Content, m.SentAt })
            .ToListAsync();

        return Ok(messages);
    }

    /// <summary>
    /// Creates a chat. The current user is always added as a participant.
    /// </summary>
    [HttpPost("CreateChat")]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatModel model)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var participantIds = (model?.UserIds ?? new List<string>())
            .Append(userId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(u => participantIds.Contains(u.Id))
            .ToListAsync();

        if (users.Count == 0)
        {
            return BadRequest("No valid participants.");
        }

        var chat = new Chat
        {
            Name = model?.Name,
            Users = users
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            chat.ChatId,
            chat.Name,
            Participants = users.Select(u => new { u.Id, u.UserName, u.Email })
        });
    }

    /// <summary>
    /// Sends a message to a chat. The sender is taken from the token, the message is
    /// persisted, pushed to the chat group in real time, and the other participants
    /// receive a notification.
    /// </summary>
    [HttpPost("{chatId}/SendMessage")]
    public async Task<IActionResult> SendMessage(int chatId, [FromBody] SendMessageModel model)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(model?.Content))
        {
            return BadRequest("Content is required.");
        }

        var chat = await _context.Chats
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.ChatId == chatId);

        if (chat == null)
        {
            return NotFound();
        }

        if (!chat.Users.Any(u => u.Id == userId))
        {
            return Forbid();
        }

        var message = new Message
        {
            ChatId = chatId,
            SenderId = userId,
            Content = model.Content,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Entrega en vivo al grupo del chat
        await _hubContext.Clients.Group(chatId.ToString())
            .SendAsync("ReceiveMessage", chatId, message.SenderId, message.Content, message.SentAt);

        // Notificar a los demás participantes
        var senderName = chat.Users.FirstOrDefault(u => u.Id == userId)?.UserName ?? "Alguien";
        var recipients = chat.Users.Where(u => u.Id != userId).Select(u => u.Id).ToList();
        if (recipients.Count > 0)
        {
            var preview = model.Content.Length > 80 ? model.Content[..80] + "…" : model.Content;
            await _notificationService.NotifyUsersAsync(
                recipients,
                title: chat.Name ?? $"Mensaje de {senderName}",
                message: $"{senderName}: {preview}",
                type: "Chat",
                url: $"/chats/{chatId}");
        }

        return Ok(new { message.MessageId, message.ChatId, message.SenderId, message.Content, message.SentAt });
    }

    public class CreateChatModel
    {
        public string? Name { get; set; }
        public List<string>? UserIds { get; set; }
    }

    public class SendMessageModel
    {
        public string? Content { get; set; }
    }
}

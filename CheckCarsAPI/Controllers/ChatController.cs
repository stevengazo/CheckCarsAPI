using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Models;
using CheckCarsAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;

namespace CheckCarsAPI.Controllers;
[Route("api/[controller]")]
[ApiController]

public class ChatsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatsController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost("CreateChat")]
    public async Task<IActionResult> CreateChat([FromBody] Chat chat)
    {
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        return Ok(chat);
    }

    [HttpPost("{chatId}/SendMessage")]
    public async Task<IActionResult> SendMessage(int chatId, [FromBody] Message message)
    {
        message.ChatId = chatId;
        message.SentAt = DateTime.UtcNow;

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Enviar mensaje mediante SignalR
        await _hubContext.Clients.Group(chatId.ToString())
            .SendAsync("ReceiveMessage", chatId, message.SenderId, message.Content, message.SentAt);
        return Ok(message);
    }
}

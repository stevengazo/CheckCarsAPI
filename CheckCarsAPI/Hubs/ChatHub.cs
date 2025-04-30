// ChatHub.cs
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessageToGroup(int chatId, string senderId, string message)
    {
        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", chatId, senderId, message, DateTime.UtcNow);
    }

    public override async Task OnConnectedAsync()
    {
        string userId = Context.UserIdentifier;
        // Aquí podrías añadir lógica para unir al usuario a grupos automáticamente
        await base.OnConnectedAsync();
    }

    public async Task JoinChat(int chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task LeaveChat(int chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }
}

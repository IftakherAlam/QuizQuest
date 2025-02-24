using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class CommentHub : Hub
{
    public async Task SendComment(int templateId, string userName, string text)
    {
        await Clients.All.SendAsync("ReceiveComment", templateId, userName, text);
    }

    public async Task UpdateLikes(int templateId, int likeCount)
    {
        await Clients.All.SendAsync("ReceiveLikeUpdate", templateId, likeCount);
    }
}

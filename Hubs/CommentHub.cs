using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace QuizFormsApp.Hubs
{
    public class CommentHub : Hub
    {
        public async Task SendComment(int templateId, string userName, string text)
        {
            await Clients.All.SendAsync("ReceiveComment", templateId, userName, text);
        }
    }
}

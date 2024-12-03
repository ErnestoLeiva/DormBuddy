using Microsoft.AspNetCore.SignalR;
using DormBuddy.Hubs;

namespace DormBuddy.Models
{
    public class GlobalMessageService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        private string? _message;
        private string? _type;

        public GlobalMessageService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void SetMessage(string message, string type = "info") // for now will be a global message
        {
            _message = message;
            _type = type;


            // notifiy connected clients
            _hubContext.Clients.All.SendAsync("ReceiveMessage", _message, _type);
        }
        

        public (string? Message, string? Type) GetMessage()
        {
            var tempMessage = _message;
            var tempType = _type;

            // Clear message after retrieval
            _message = null;
            _type = null;

            return (tempMessage, tempType);
        }

        public bool HasMessage() => !string.IsNullOrEmpty(_message);
    }

}
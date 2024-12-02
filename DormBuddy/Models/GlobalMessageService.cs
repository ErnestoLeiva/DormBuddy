namespace DormBuddy.Models
{
    public class GlobalMessageService
    {
        private string _message;
        private string _type;

        public void SetMessage(string message, string type = "info")
        {
            _message = message;
            _type = type;
        }

        public (string Message, string Type) GetMessage()
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
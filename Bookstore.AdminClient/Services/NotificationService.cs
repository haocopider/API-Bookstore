namespace Bookstore.AdminClient.Services
{
    public class NotificationService
    {
        public event Action<string, string>? OnNotify;

        public void Success(string message)
        {
            OnNotify?.Invoke("success", message);
        }

        public void Error(string message)
        {
            OnNotify?.Invoke("danger", message);
        }

        public void Warning(string message)
        {
            OnNotify?.Invoke("warning", message);
        }

        public void Info(string message)
        {
            OnNotify?.Invoke("info", message);
        }
    }
}

namespace CelestialEngine.Extensions.WebUI
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    internal class WebUIMessageBusSink
    {
        private ConcurrentQueue<WebUIMessage> pendingMessages;

        public WebUIMessageBusSink()
        {
            this.pendingMessages = new ConcurrentQueue<WebUIMessage>();
        }

        public void PushMessageToGame(string name, string data)
        {
            this.pendingMessages.Enqueue(new WebUIMessage() { Name = name, Data = data });
        }

        internal IEnumerable<WebUIMessage> GetAllAndFlush()
        {
            while (this.pendingMessages.TryDequeue(out WebUIMessage message))
            {
                yield return message;
            }
        }
    }
}

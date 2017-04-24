using SlackAPI;
using SlackAPI.WebSocketMessages;
using System;

namespace EhiiiBot
{
    public class Bot
    {
        public string Token { get; set; }
        public string ReplyTemplate { get; set; }

        public SlackSocketClient SlackClient  { get; private set; }

        //Eventi
        public event Action Connected;
        public event Action<string> ConnectionFailed;
        public event Action<NewMessage> NewMessage;
        public event Action<MessageReceived> ReplyReceived;
        public event Action Disconnected;

        public Bot(string token, string replyTemplate)
        {
            Token = token;
            ReplyTemplate = replyTemplate;
        }

        /// <summary>
        /// Si connette alla rete Slack e si mette in ascolto
        /// </summary>
        public void Connect()
        {
            SlackClient = new SlackSocketClient(Token);
            SlackClient.Connect((response) => {
                // Il client si è connesso
                if (response.ok)
                {
                    Connected?.Invoke();
                }
                else
                {
                    ConnectionFailed?.Invoke(response.error);
                }
            });

            SlackClient.OnMessageReceived += (message) =>
            {
                //Non rispondo a me stesso
                if (!message.user.Equals(SlackClient.MySelf.id))
                {
                    //Notifico che il bot ha ricevuto un messaggio
                    NewMessage?.Invoke(message);

                    //Rispondo
                    SlackClient.SendMessage(
                        (received) => {
                            //Il messaggio di risposta è stato ricevuto
                            ReplyReceived?.Invoke(received);
                        },
                        message.channel,
                        ReplyTemplate);
                }
            };
        }

        /// <summary>
        /// Si disconnette dalla rete slack e termina l'esecuzione del bot
        /// </summary>
        public void Disconnect()
        {
            try
            {
                SlackClient.CloseSocket();
            }
            catch { }
            SlackClient = null;
            Disconnected?.Invoke();
        }
    }
}

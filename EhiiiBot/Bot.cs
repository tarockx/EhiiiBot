using SlackAPI;
using SlackAPI.WebSocketMessages;
using System;

namespace EhiiiBot
{
    /// <summary>
    /// Un basilare wrapper intorno alla libreria SlackAPI che implementa un semplice Bot slack.
    /// Il Bot risponde ad ogni messaggio ricevuto con il testo indicato dalla property ReplyTemplate
    /// </summary>
    public class Bot
    {
        private string Token { get; set; } //Token di autenticazione del Bot
        private string ReplyTemplate { get; set; } //Messaggio di risposta del Bot

        public SlackSocketClient SlackClient  { get; private set; }

        //Eventi per comunicazione con la UI
        public event Action Connected;
        public event Action<string> ConnectionFailed;
        public event Action<NewMessage> NewMessage;
        public event Action<MessageReceived> ReplyReceived;
        public event Action Disconnected;

        /// <summary>
        /// Costruisce una nuova istanza del Bot
        /// </summary>
        /// <param name="token">Il token di autenticazione del Bot. Questo token è reperibile nella pagina di configurazione del Bot sul sito Slack</param>
        /// <param name="replyTemplate">La risposta da inviare ad ogni messaggio ricevuto</param>
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
                        (received) =>
                        {
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

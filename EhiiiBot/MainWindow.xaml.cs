using System;
using System.Reflection;
using System.Windows;

namespace EhiiiBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bot Bot;

        public string Token { get; set; }
        public string ReplyTemplate { get; set; }

        public static readonly DependencyProperty BotRunningProperty = DependencyProperty.Register("BotRunning", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        public bool BotRunning
        {
            get { return (bool)GetValue(BotRunningProperty); }
            set { SetValue(BotRunningProperty, value); }
        }

        public static readonly DependencyProperty LogMessageProperty = DependencyProperty.Register("LogMessage", typeof(string), typeof(MainWindow), new PropertyMetadata(""));
        public string LogMessage
        {
            get { return (string)GetValue(LogMessageProperty); }
            set { SetValue(LogMessageProperty, value); }
        }

        
        public MainWindow()
        {
            ReplyTemplate = "Ehiii!";
            InitializeComponent();

            PostToLog("Ehiii! Bot " + Assembly.GetExecutingAssembly().GetName().Version + " initialization complete!");
        }

        private void btnStartBot_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Token))
            {
                MessageBox.Show("Error: you must specify the bot's Token to connect.", "Missing data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(ReplyTemplate))
            {
                MessageBox.Show("Error: you must specify the bot's Reply Message to start.", "Missing data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                PostToLog("Connecting to the Slack network using bot token: " + Token);
                SetBotRunning(true);

                Bot = new Bot(Token, ReplyTemplate);

                Bot.Connected += Bot_Connected;
                Bot.ConnectionFailed += Bot_ConnectionFailed;
                Bot.NewMessage += Bot_NewMessage;
                Bot.ReplyReceived += Bot_ReplyReceived;
                Bot.Disconnected += Bot_Disconnected;

                Bot.Connect();
            }
            catch (Exception ex)
            {
                PostToLog("Error starting bot, exception raised: " + ex.Message);
                MessageBox.Show("Error starting bot: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetBotRunning(false);
            }
        }

        private void btnStopBot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Bot.Disconnect();
            }
            catch(Exception ex) {
                PostToLog("Error stopping bot, exception raised: " + ex.Message);
                MessageBox.Show("Error stopping bot: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Bot_ConnectionFailed(string obj)
        {
            SetBotRunning(false);
            MessageBox.Show("Error connecting to the Slack network: " + obj, "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Bot_Connected()
        {
            SetBotRunning(true);
            PostToLog("Bot started and running!");
        }

        private void Bot_ReplyReceived(SlackAPI.WebSocketMessages.MessageReceived obj)
        {
            PostToLog("Message " + obj.reply_to + " has been received by the user");
        }

        private void Bot_NewMessage(SlackAPI.WebSocketMessages.NewMessage obj)
        {
            string user = obj.user;
            try
            {
                user = Bot.SlackClient.UserLookup[obj.user].name;
            }
            catch { }

            PostToLog("Bot received message from user \"" + user + "\". Sending reply...");
        }

        private void Bot_Disconnected()
        {
            PostToLog("Bot stopped correctly!");
            SetBotRunning(false);
        }

        private void PostToLog(string msg)
        {
            string dtm = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            Dispatcher.Invoke(new Action(() =>
            {
                LogMessage += (dtm + " - " + msg + "\n");
            }));
        }

        private void SetBotRunning(bool running)
        {
            Dispatcher.Invoke(new Action(() => {
                BotRunning = running;
            }));
        }
    }
}

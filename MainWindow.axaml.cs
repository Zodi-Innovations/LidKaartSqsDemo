using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading; 
using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace AWSSqsDemoAvalonia1
{
    public partial class MainWindow : Window 
    {
        private const string QueueUrl = "https://sqs.eu-north-1.amazonaws.com/888577025424/Ismail";
        private readonly AmazonSQSClient _client;
        private CancellationTokenSource _cancellationTokenSource;

        public MessageData MemberData { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; 
            _client = new AmazonSQSClient(RegionEndpoint.EUNorth1);
            StartReceivingMessages();
        }
        //Ontvangen
        private async void StartReceivingMessages()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var response = await _client.ReceiveMessageAsync(new ReceiveMessageRequest
                    {
                        QueueUrl = QueueUrl,
                        MaxNumberOfMessages = 5,
                        WaitTimeSeconds = 15
                    }, _cancellationTokenSource.Token);
                    //bericht verwerken
                    foreach (var message in response.Messages)
                    {
                        UpdateMemberData(message.Body);

                       
                        await _client.DeleteMessageAsync(new DeleteMessageRequest
                        {
                            QueueUrl = QueueUrl,
                            ReceiptHandle = message.ReceiptHandle
                        });
                    }
                }
            }
            catch (TaskCanceledException)
            {
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving messages: {ex.Message}");
            }
        } 
        //Gevens bijwerken
        private void UpdateMemberData(string messageBody)
        {
            try
            {
                var data = JsonSerializer.Deserialize<MessageData>(messageBody);
                if (data != null)
                {
                    
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        MemberData = data; 
                        DataContext = MemberData; 
                    });
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Failed to parse message: {e.Message}");
            }
        }

        //Stopt met verwerken van Messages
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _cancellationTokenSource?.Cancel(); 
        }
    }
    

    public class MessageData
    {
        public string lastname { get; set; }
        public string firstname { get; set; }
        public string uuid { get; set; }
        public string clubName { get; set; }
        public string clubLogo { get; set; }
        
    }
}

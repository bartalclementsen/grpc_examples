using Grpc.Core;
using GrpcExamples.Server;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace GrpcExamples.DesktopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GrpcClientCore.GrpcServiceProvider provider;
        private Greeter.GreeterClient client;

        public MainWindow()
        {
            InitializeComponent();
        }
 
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                Log($"Calling server with name {NameTextbox.Text}");

                //var result = await client.SayHelloAsync(new HelloRequest { Name = NameTextbox.Text, Age = int.Parse(AgeTextbox.Text) });
                //Log(System.Text.Json.JsonSerializer.Serialize(result));

                await stream.RequestStream.WriteAsync(new HelloRequest
                {
                    Name = NameTextbox.Text,
                    Age = int.Parse(AgeTextbox.Text),
                    Advanced = new HelloRequestAdvanced
                    {
                        From = NameTextbox.Text,
                        To = "Someone",
                        Message = $"I am age {AgeTextbox.Text}"
                    }
                });
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.ToString());
            }
            finally
            {
                Log($"Duration {sw.ElapsedMilliseconds}ms");
            }
        }

        private void Log(object o)
        {
            LogTextBox.Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"{DateTime.Now:HH:mm:ss} | {o ?? "NULL"} {Environment.NewLine}");
                LogTextBox.ScrollToEnd();
            });
        }

        private AsyncDuplexStreamingCall<GrpcExamples.Server.HelloRequest, GrpcExamples.Server.HelloReply> stream;
        private Task listeningTask;
        
        
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Log($"Connecting to {ServerAddressTextbox.Text}");
            if(provider != null)
            {
                provider.Dispose();
                provider = null;
            }
            
            provider = new GrpcClientCore.GrpcServiceProvider(ServerAddressTextbox.Text);
            client = provider.GetGreeterClient();

            stream = client.SayHelloStream();

            listeningTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    var result = stream.ResponseStream.Current;
                    Log(System.Text.Json.JsonSerializer.Serialize(result));
                }
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 49002);

        public MainWindow()
        {
            tcpListener.Start();
            Trace.WriteLine("TCP listener started");

            InitializeComponent();
        }

        async private void ReceiveInformation()
        {
            Trace.WriteLine("Start Receiving"); 
            using TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
            NetworkStream stream = tcpClient.GetStream();
            var responseBytes = new byte[512];
            var builder = new StringBuilder();
            int bytes;
            DriveInfo[] drives;
            DirectoryInfo directoryInfo;
            StreamReader reader;
            do
            {
                bytes = await stream.ReadAsync(responseBytes);
                string responsePart = Encoding.UTF8.GetString(responseBytes, 0, bytes);
                if (responsePart == "")
                    break;
                else if (responsePart == "ConnectionStabilization")
                {
                    drives = DriveInfo.GetDrives();
                    string names = "";
                    foreach (DriveInfo drive in drives)
                    {
                        names += drive.Name + " ";
                    }
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(names));
                }
                else if (responsePart.Contains(".txt"))
                {
                    reader = new StreamReader(responsePart);
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(reader.ReadToEnd()));
                }
                else
                {
                    directoryInfo = new DirectoryInfo(responsePart);

                    string namesOfDirectories = "";
                    string namesOfFiles = "";
                    foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                        namesOfDirectories += directory.Name + ": ";

                    foreach (FileInfo file in directoryInfo.GetFiles())
                        namesOfFiles += file.Name + ": ";
                    await stream.WriteAsync(Encoding.UTF8.GetBytes($"{namesOfDirectories + namesOfFiles}"));
                }
                Log.Text += "\nServer Recieved " + responsePart;
            }
            while (bytes > 0);
            ReceiveInformation();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tcpListener.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReceiveInformation();
            Trace.WriteLine("Ready to recieve");
        }
    }

}

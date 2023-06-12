using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int port = 49002;
        StringBuilder currentPath = new StringBuilder();
        bool txtFile = false;
        TcpClient tcpClient;
        NetworkStream stream;
        bool fromStart = true;
        public MainWindow()
        {
            InitializeComponent();
        }

        async private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Drive.Items.Clear();
            tcpClient = new TcpClient(System.Net.Sockets.AddressFamily.InterNetwork);
            await tcpClient.ConnectAsync(IPAddress.Parse(ip_address.Text), port);
            if (tcpClient.Connected)
                Recieved.Text = ($"Подключение с {tcpClient.Client.RemoteEndPoint} установлено");
            else
                Recieved.Text = ("Не удалось подключиться");
            stream = tcpClient.GetStream();
            await stream.WriteAsync(Encoding.UTF8.GetBytes("SomeSecretWord"));
            Connect.IsEnabled = false;
            Disconnect.IsEnabled = true;
            Send.IsEnabled = true;
            byte[] data = new byte[512];
            int bytes = await stream.ReadAsync(data);
            string response = Encoding.UTF8.GetString(data, 0, bytes);
            string drive = "";
            for (int i = 0; i < response.Length; i++)
            {
                if (response[i] == ' ')
                {
                    Drive.Items.Add(drive);
                    drive = "";
                }
                else
                {
                    drive += response[i];
                }
            }
            ConnectionState.Text = "Подключен к серверу";

        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            tcpClient.Dispose();
            Connect.IsEnabled=true;
            Disconnect.IsEnabled=false;
            ConnectionState.Text = "Нет подключения к серверу";

        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (fromStart == true)
            {
                currentPath.Clear();
                currentPath.Append(Drive.SelectedItem.ToString());
                Send_toServer(currentPath.ToString());
                fromStart = false;
            }
            else if (FolderView.SelectedItem.ToString().Contains(".txt"))
            {
                if (currentPath.ToString().EndsWith('\\'))
                {
                    Send_toServer(currentPath.ToString() + FolderView.SelectedItem.ToString());
                }
                else
                {
                    Send_toServer(currentPath.ToString() + "\\" + FolderView.SelectedItem.ToString());
                }

                txtFile = true;
            }
            else
            {
                currentPath.Append("\\" + FolderView.SelectedItem.ToString());
                Send_toServer(currentPath.ToString());
            }
            Send_toClient();
        }

        private async void Send_toServer(string currentPath)
        {
            byte[] data = new byte[10240];
            int bytes = await stream.ReadAsync(data);
            string response = Encoding.UTF8.GetString(data, 0, bytes);
            string item = "";
            bool flag = false;
            if (txtFile == true)
            {
                txtFile = false;
                Recieved.Text = response;
            }
            else
            {
                FolderView.Items.Clear();
                for (int i = 0; i < response.Length; i++)
                {
                    if (response[i] == ':')
                    {
                        flag = true;
                    }
                    else if (response[i] == ' ' && flag)
                    {
                        FolderView.Items.Add(item);
                        item = "";
                    }
                    else
                    {
                        item += response[i];
                        flag = false;
                    }
                }
            }
        }

        private async void Send_toClient()
        {
            byte[] data = new byte[10240];
            int bytes = await stream.ReadAsync(data);
            string response = Encoding.UTF8.GetString(data, 0, bytes);
            string item = "";
            bool flag = false;
            if (txtFile == true)
            {
                txtFile = false;
                Recieved.Text = response;
            }
            else
            {
                FolderView.Items.Clear();
                for (int i = 0; i < response.Length; i++)
                {
                    if (response[i] == ':')
                    {
                        flag = true;
                    }
                    else if (response[i] == ' ' && flag)
                    {
                        FolderView.Items.Add(item);
                        item = "";
                    }
                    else
                    {
                        item += response[i];
                        flag = false;
                    }
                }
            }
        }

        private void Backwards_Click(object sender, RoutedEventArgs e)
        {
            bool find = false;
            for (int i = currentPath.Length - 1; i >= 0; i--)
            {
                if (currentPath[i] == '\\')
                {
                    find = true;
                    currentPath.Remove(i, currentPath.Length - i);
                    if (currentPath[i - 1] == ':')
                    {
                        fromStart = true;
                    }
                    break;
                }

            }
            if (find == false)
            {
                return;
            }
            FolderView.SelectedItem = null;
            Send_toServer(currentPath.ToString());
            Send_toClient();
        }

        private void Drive_Selected(object sender, RoutedEventArgs e)
        {

            fromStart = true;
        }

        private void FolderView_Selected(object sender, RoutedEventArgs e)
        {
            fromStart = false;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Super_chat
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UdpClient Client;
        const int localeport = 5500; // порт для приема сообщений
        const int remoteport = 5500; // порт для отправки сообщений
       // const int TTL = 20;
        const string IP = "127.0.0.1"; 
        IPAddress groupAddress; // адрес для групповой рассылки
        string nikName;


        public MainWindow()
        {
            InitializeComponent();

            btnLogin.IsEnabled = true;
            lbLog.IsEnabled = false;
            tbMessage.IsEnabled = false;
            btnSend.IsEnabled = false;
            btnLogout.IsEnabled = false;

            groupAddress = IPAddress.Parse(IP);
        }

        // метод приема сообщений
        private void RecMessage()
        {
             
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                
                byte[] buffer = Client.Receive(ref remoteIp);
                string message = Encoding.UTF8.GetString(buffer);
                lbLog.Dispatcher.BeginInvoke(new Action(() => addMessage("Собеседник: " + message)));
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void addMessage(string message)
        {
            lbLog.Items.Add(DateTime.Now.ToString() + ": " + message);
            var border = (Border)VisualTreeHelper.GetChild(lbLog, 0);
            var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            scrollViewer.ScrollToBottom();
        }



        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            nikName = tbNik.Text;

            try
            {
                Client = new UdpClient(localeport); // UdpClient для получения данных
                Client.JoinMulticastGroup(groupAddress);

                btnLogin.IsEnabled = false;
                lbLog.IsEnabled = true;
                tbMessage.IsEnabled = true;
                btnSend.IsEnabled = true;
                btnLogout.IsEnabled = true;

                 Thread th = new Thread(RecMessage);
                 th.Start();


                string message = nikName + " вошел в чат";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                Client.Send(buffer, buffer.Length, IP, remoteport);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            
        }

        

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            string message = tbNik + " покидает чат";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            Client.Send(buffer, buffer.Length, IP, remoteport);
            Client.DropMulticastGroup(groupAddress);

            Client.Close();

            btnLogin.IsEnabled = true;
            lbLog.IsEnabled = false;
            tbMessage.IsEnabled = false;
            btnSend.IsEnabled = false;
            btnLogout.IsEnabled = false;
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

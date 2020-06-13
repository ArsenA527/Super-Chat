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
        bool alive = false; // будет ли работать поток для приема
        UdpClient client;
        const int LOCALPORT = 8001; // порт для приема сообщений
        const int REMOTEPORT = 8001; // порт для отправки сообщений
        const int TTL = 20;
        const string HOST = "235.5.5.1"; // хост для групповой рассылки
        IPAddress groupAddress; // адрес для групповой рассылки

        string userName; // имя пользователя в чате


        public MainWindow()
        {
            InitializeComponent();

            btnLogin.IsEnabled = true;
            
            btnSend.IsEnabled = false;
            btnLogout.IsEnabled = false;

            groupAddress = IPAddress.Parse(HOST);
        }

        // метод приема сообщений
        private void RecMessage()
        {
            alive = true;
            try
            {
                while (alive)
                {
                    IPEndPoint remoteIp = null;
                    byte[] data = client.Receive(ref remoteIp);
                    string message = Encoding.UTF8.GetString(data);

                    // добавляем полученное сообщение в текстовое поле
                    lbLog.Dispatcher.BeginInvoke(new Action(() => addMessage("Собеседник: " + message)));
                }
            }
            catch (ObjectDisposedException)
            {
                if (!alive)
                    return;
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            userName = tbNik.Text;
           

            try
            {
                client = new UdpClient(LOCALPORT);
                // присоединяемся к групповой рассылке
                client.JoinMulticastGroup(groupAddress, TTL);

                // запускаем задачу на прием сообщений
                Task receiveTask = new Task(RecMessage);
                receiveTask.Start();

                // отправляем первое сообщение о входе нового пользователя
                string message = userName + " вошел в чат";
                byte[] data = Encoding.UTF8.GetBytes(message);
                client.Send(data, data.Length, HOST, REMOTEPORT);

                btnLogin.IsEnabled = false;
                btnLogout.IsEnabled = true;
                btnSend.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void ExitChat()
        {
            string message = userName + " покидает чат";
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, HOST, REMOTEPORT);
            client.DropMulticastGroup(groupAddress);

            alive = false;
            client.Close();

            btnLogin.IsEnabled = true;
            btnLogout.IsEnabled = false;
            btnSend.IsEnabled = false;
        }


        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            ExitChat();
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            UdpClient otherClient = new UdpClient(); // создаем UdpClient для отправки сообщений
            try
            {
                string message = $"{tbNik.Text}: {tbMessage.Text}";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                otherClient.Send(buffer, buffer.Length, HOST, REMOTEPORT);
                tbMessage.Clear();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message); ;
            }
            finally
            {
                otherClient.Close();
            }
        }
    }
}

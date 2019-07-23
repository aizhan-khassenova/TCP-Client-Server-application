using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace WinFormTcpServer
{
    public partial class Form1 : Form
    {
        IPAddress MyIp = null;
        Thread Potok = null;
        ServerThreadClass TSC = null;
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
            string MyHost = Dns.GetHostName();
            byte i = 0;
            MyIp = Dns.GetHostEntry(MyHost).AddressList[i];
            while (MyIp.AddressFamily != AddressFamily.InterNetwork)
            {
                i++;
                MyIp = Dns.GetHostEntry(MyHost).AddressList[i];
            }
            IpAddress.Text = MyIp.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "0" | textBox1.Text == "")
            {
                MessageBox.Show("Введите номер порта", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    button1.Enabled = false;
                    button2.Enabled = true;
                    textBox2.AppendText(Environment.NewLine + "Старт сервера " + MyIp + ":" + textBox1.Text);
                    

                    TSC = new ServerThreadClass();
                    TSC.TB = textBox2;
                    TSC.MyIp = MyIp;
                    TSC.port = textBox1.Text;
                    Potok = new Thread(TSC.Start);
                    Potok.IsBackground = true;
                    Potok.Start();

                    textBox2.AppendText(Environment.NewLine + "Cервер запущен ");
                }
                catch(Exception ex)
                {
                    Potok.Abort();
                    textBox2.AppendText(Environment.NewLine + "Cервер остановлен ");
                    button1.Enabled = true;
                    button2.Enabled = false;
                }
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.AppendText(Environment.NewLine + "Cервер остановлен ");
            button1.Enabled = true;
            button2.Enabled = false;
            TSC.flag = false;
            TSC = null;
            Potok = null;
        }
        
    }
    class ServerThreadClass
    {
        public NetworkStream stream;
        int CountBytes = 0;
        public bool flag = true;
        public TextBox TB { get; set; }
        StringBuilder builder2 = new StringBuilder();
        TcpListener listener = null;
        TcpClient mytcpclient;
        public IPAddress MyIp { get; set; }
        public string port;
        
        public void Start()
        {
            try
            {
                listener = new TcpListener(MyIp, Convert.ToInt32(port));
                listener.Start();
                
                byte[] data = new byte[1024];
                while (flag)
                {
                    mytcpclient = listener.AcceptTcpClient();
                    stream = mytcpclient.GetStream();
                    do
                    {
                        CountBytes = stream.Read(data, 0, data.Length);
                        builder2.Append(Encoding.Unicode.GetString(data, 0, CountBytes));
                    }
                    while (stream.DataAvailable);
                    TB.Invoke((MethodInvoker)(delegate () { TB.AppendText(Environment.NewLine + " Подключился " + builder2.ToString()); }));
                    data = null;

                    DateTime DTime = DateTime.Now;

                    data = Encoding.Unicode.GetBytes(DTime.ToString());
                    TB.Invoke((MethodInvoker)(delegate () { TB.AppendText(Environment.NewLine + " Отправляем - " + DTime.ToString()); }));
                    
                    stream.Write(data, 0, data.Length);
                    builder2.Clear();
                }
                stream.Close();
                listener.Stop();
                Thread.ResetAbort();
            }
            catch(Exception ex)
            {
                //TB.AppendText(Environment.NewLine + "Отключаемся");
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }
    }
}

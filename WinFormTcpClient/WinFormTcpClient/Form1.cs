using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;

namespace WinFormTcpClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread Thr = null;
            if (textBox1.Text != "" & textBox2.Text != "")
            {
                try
                {
                    ConnClass CC = new ConnClass();
                    CC.TB = textBox3;
                    CC.address = textBox1.Text;
                    CC.port = Convert.ToInt32(textBox2.Text);
                    CC.Btn = button1;
                    Thr = new Thread(CC.CONNZAP);
                    Thr.Start();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(""+ex);
                    if (Thr != null)
                    {
                        Thr.Abort();
                        button1.Enabled = true;
                    }
                }
            }
            else
            {
                MessageBox.Show("Введите параметры подключения", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
    public class ConnClass
    {
        public TcpClient client = null;
        public NetworkStream stream = null;
        public byte[] data = new byte[64];
        public string address { get; set; }
        public int port { get; set; }
        public TextBox TB { get; set; }
        private int CountBytes = 0;
        StringBuilder builder = new StringBuilder();
        public Button Btn { get; set; }

        public void CONNZAP()
        {
            try
            {
                Btn.Invoke((MethodInvoker)(delegate () { Btn.Enabled = false; }));
                client = new TcpClient(address, port);//подключаемся
                stream = client.GetStream();//открываем поток
                
                {
                    data = Encoding.Unicode.GetBytes(Environment.UserName);//имя залогиневшего пользователя конвертим в массив байт
                    stream.Write(data, 0, data.Length);//отправляем массив байт по сети
                    
                    do
                    {
                        CountBytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, CountBytes));
                    }
                    while (stream.DataAvailable);
                    TB.Invoke((MethodInvoker)(delegate () { TB.AppendText(Environment.NewLine + " Получили от сервера - " + builder.ToString()); }));
                    builder.Clear();
                    Btn.Invoke((MethodInvoker)(delegate () { Btn.Enabled = true; }));
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("Не удалось установить соединение, проверьте параметры подключения и брандмауэр на предмет блокировки порта" + ex, "Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (client != null)
                {
                    client.Close();
                }
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
                Btn.Invoke((MethodInvoker)(delegate () { Btn.Enabled = true; }));
            }
        }
    }
}

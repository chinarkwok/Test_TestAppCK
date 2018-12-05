using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using System.Xml;
using Oracle.ManagedDataAccess.Client;

namespace TestAppCK
{
    public partial class Form1 : Form
    {
        private IConnectionFactory factory;
        private bool MQReady = false;
        private bool OracleReady = false;

        private bool isTesting = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InitProducer();

        }

        /// <summary>
        /// 初始化MQ
        /// </summary>
        public void InitProducer()
        {
            try
            {
                //初始化工厂
                factory = new ConnectionFactory("tcp://" + textBox1.Text.Trim() + ":" + textBox2.Text.Trim());
                sendMSG("第一条测试", 0);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 发送MQ信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool sendMSG(string msg, int type = 1)//type=0为初始化测试
        {
            //建立工厂连接
            using (IConnection connection = factory.CreateConnection())
            {
                //通过工厂连接创建Session会话
                using (ISession session = connection.CreateSession())
                {


                    //创建一个发送消息的对象
                    if (type == 0)
                    {
                        try
                        {//通过会话创建生产者，方法里new出来MQ的Queue
                            IMessageProducer prod = session.CreateProducer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic("TestTopic"));
                            string message1 = "{'value':'" + msg + "'}";
                            prod.Send(message1, MsgDeliveryMode.NonPersistent, MsgPriority.Normal, TimeSpan.MinValue);

                            textBox4.AppendText("MQ初始化成功\n");
                            MQReady = true;
                        }
                        catch
                        {
                            textBox4.AppendText("MQ初始化失败\n");
                            MQReady = false;
                            return false;
                        }
                    }
                    else
                    {
                        //通过会话创建生产者，方法里new出来MQ的Queue
                        IMessageProducer prod = session.CreateProducer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(textBox3.Text.Trim()));
                        ITextMessage msgtmp = prod.CreateTextMessage();
                        msgtmp.Text = msg;
                        prod.Send(msgtmp, Apache.NMS.MsgDeliveryMode.NonPersistent, Apache.NMS.MsgPriority.Normal, TimeSpan.MinValue);

                        //prod.Send(msg);
                    }

                }
            }
            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MQReady && OracleReady)
            {
                if (isTesting == false)
                {
                    textBox4.AppendText("开始测试" + DateTime.Now + "\n");
                    timer1.Interval = int.Parse(textBox6.Text.Trim());
                    timer1.Start();
                    button3.Text = "End Test";
                    isTesting = true;
                }
                else
                {
                    timer1.Stop();
                    button3.Text = "Start Test";
                    isTesting = false;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string strConnection = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=101.201.252.80)(PORT=1521))(CONNECT_DATA = (SERVICE_NAME = apts))); Persist Security Info = True; User ID = aptshd; Password = hd; ";

            string strConnection = textBox5.Text.Trim();
            try
            {
                string str_sql = textBox7.Text.Trim();
                OracleConnection conn = new OracleConnection(strConnection);
                OracleCommand cmd1 = new OracleCommand(str_sql, conn);
                conn.Open();
                textBox4.AppendText("Ora初始化成功\n");
                //conn.Close();
                OracleReady = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                textBox4.AppendText("Ora初始化失败\n");
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string strConnection = textBox5.Text.Trim();
            try
            {
                string str_sql = textBox7.Text.Trim();
                OracleConnection conn = new OracleConnection(strConnection);
                OracleCommand cmd1 = new OracleCommand(str_sql, conn);
                conn.Open();
                sendMSG(textBox8.Text.Trim());
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}

using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using INIFILE;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;

namespace RFIDmjkz
{

    public partial class Form1 : Form
    {
        SerialPort sp1 = new SerialPort();
         
        public Form1()
        {
            InitializeComponent();
        }

        //加载
        private void Form1_Load(object sender, EventArgs e)
        {
            INIFILE.Profile.LoadProfile();//加载所有
            // 预置波特率
            switch (Profile.G_BAUDRATE)
            {
                case "9600":
                    cbBaudRate.SelectedIndex = 0;
                    break;
                case "19200":
                    cbBaudRate.SelectedIndex = 1;
                    break; 
                default:
                    {
                        MessageBox.Show("波特率预置参数错误。");
                        return;
                    }                  
            }

            //预置数据位
            switch (Profile.G_DATABITS)
            {
                case  "8":
                    cbDataBits.SelectedIndex = 0;
                    break;
                default:
                    {
                        MessageBox.Show("数据位预置参数错误。");
                        return;
                    }
            }
            //预置停止位
            switch (Profile.G_STOP)
            {
                case "1":
                    cbStop.SelectedIndex = 0;
                        break;
                default:
                    {
                        MessageBox.Show("停止位预置参数错误。");
                        return;
                    }
            }

            //预置校验位
            switch(Profile.G_PARITY)
            {
                case "NONE":
                    cbParity.SelectedIndex = 0;
                    break;
                default:
                    {
                        MessageBox.Show("校验位预置参数错误。");
                        return;
                    }
            }

            //添加串口项目
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {
                cbSerial.Items.Add(s);
            }

            //串口设置默认选择项
           // cbSerial.SelectedIndex = 0; 
            sp1.BaudRate = 9600;
            Control.CheckForIllegalCrossThreadCalls = false; 
            sp1.DataReceived += new SerialDataReceivedEventHandler(sp1_DataReceived);
            //准备就绪              
            sp1.DtrEnable = true;
            sp1.RtsEnable = true;
            sp1.ReadTimeout = 1000;
            sp1.Close();
        }

        void sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sp1.IsOpen) 
            {
                txtReceive.SelectAll();
                txtReceive.SelectionColor = Color.Black;         //改变字体的颜色
                byte[] byteRead = new byte[sp1.BytesToRead];    //BytesToRead:sp1接收的字符个数
                    try
                    {
                        Byte[] receivedData = new Byte[sp1.BytesToRead];        //创建接收字节数组
                        sp1.Read(receivedData, 0, receivedData.Length);         //读取数据
                        sp1.DiscardInBuffer();                                  //清空SerialPort控件的Buffer
                        String strRcv = null;
                        for (int i = 0; i < receivedData.Length; i++) 
                        {
                            strRcv += receivedData[i].ToString("X2");
                        }
                        txtReceive.Text += strRcv;
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message, "出错提示");
                        txtSend.Text = "";
                    }
            }
            else
            {
                MessageBox.Show("请打开某个串口", "错误提示");
            }
        }

        //发送按钮
        private void btnSend_Click(object sender, EventArgs e)
        {
            tmSend.Enabled = false;
            if (!sp1.IsOpen)
            {
                MessageBox.Show("请先打开串口！", "Error");
                return;
            }

            String strSend = txtSend.Text;
            string[] strArray = strSend.Split(' ');
            int byteBufferLength = strArray.Length;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i] == "")
                {
                    byteBufferLength--;
                }
            }
            byte[] byteBuffer = new byte[byteBufferLength];
            int m = 0;
            for (int i = 0; i < strArray.Length; i++)
            {
                Byte[] bytesOfStr = Encoding.Default.GetBytes(strArray[i]);
                int decNum = 0;
                if (strArray[i] == "")
                {
                    continue;
                }
                else
                {
                    decNum = Convert.ToInt32(strArray[i], 16);
                }
                try
                {
                    byteBuffer[m] = Convert.ToByte(decNum);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("输入错误，请重新输入！", "Error");
                    tmSend.Enabled = false;
                    return;
                }
                m++;
            }
            sp1.Write(byteBuffer, 0, byteBuffer.Length);
            //敖天鹏 
            //读取UID
            String kahao = "";
            int n = 0;
            String strCon = "Server=LAPTOP-HAZY;DataBase=RFID_sql;Trusted_Connection=SSPI";
            SqlConnection conn = new SqlConnection(strCon);
            SqlCommand com = new SqlCommand("select * from UID ", conn);
            
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            try
            {
                SqlDataReader rd = com.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        kahao = rd["KAHAO"].ToString();
                        label2.Text = kahao;
                        label3.Text = txtReceive.Text;
                        if (kahao.Trim() == txtReceive.Text.Trim())
                        {
                            label_tg.Text = kahao + " 通过";
                            n = 1;

                        }
                        else
                        {
                            label_tg.Text = kahao + "未通过";
                            return;
                        }
                    }
                }
            }
            catch (Exception)
            { }
            finally
            {
                //rd.Close();
                conn.Close();
            }
            //打开连接12号节点的串口
            if (n ==1) {
                sp1.Close();    //关闭第一个串口
                cbSerial.SelectedIndex = 1;
                //打开第二个串口
                if (!sp1.IsOpen)
                {
                    try
                    {
                        //设置串口号
                        string serialName = cbSerial.SelectedItem.ToString();
                        sp1.PortName = serialName;
                        //设置各“串口设置”
                        string strBaudRate = cbBaudRate.Text;
                        string strDateBits = cbDataBits.Text;
                        string strStopBits = cbStop.Text;
                        Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                        Int32 iDateBits = Convert.ToInt32(strDateBits);

                        sp1.Parity = Parity.None;       //校验位
                        sp1.BaudRate = iBaudRate;       //波特率
                        sp1.DataBits = iDateBits;       //数据位
                        switch (cbStop.Text)            //停止位
                        {
                            case "1":
                                sp1.StopBits = StopBits.One;
                                break;
                            default:
                                MessageBox.Show("Error：参数不正确!", "Error");
                                break;
                        }

                        if (sp1.IsOpen == true)//如果打开状态，则先关闭一下
                        {
                            sp1.Close();
                        }

                        //设置必要控件不可用
                        cbSerial.Enabled = false;
                        cbBaudRate.Enabled = false;
                        cbDataBits.Enabled = false;
                        cbStop.Enabled = false;
                        cbParity.Enabled = false;

                        sp1.Open();     //打开串口
                        btnSwitch.Text = "关闭串口";
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Error:" + ex.Message, "Error");
                        tmSend.Enabled = false;
                        return;
                    }
                }
                else
                {
                    //恢复控件功能
                    //设置必要控件不可用
                    cbSerial.Enabled = true;
                    cbBaudRate.Enabled = true;
                    cbDataBits.Enabled = true;
                    cbStop.Enabled = true;
                    cbParity.Enabled = true;
                    sp1.Close();                    //关闭串口
                    btnSwitch.Text = "打开串口";
                }
            }
        }

        //开关按钮
        private void btnSwitch_Click(object sender, EventArgs e)
        {
            if (!sp1.IsOpen)
            {
                try
                {
                    //设置串口号
                    string serialName = cbSerial.SelectedItem.ToString();
                    sp1.PortName = serialName;
                    //设置各“串口设置”
                    string strBaudRate = cbBaudRate.Text;
                    string strDateBits = cbDataBits.Text;
                    string strStopBits = cbStop.Text;
                    Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                    Int32 iDateBits = Convert.ToInt32(strDateBits);

                    sp1.Parity = Parity.None;       //校验位
                    sp1.BaudRate = iBaudRate;       //波特率
                    sp1.DataBits = iDateBits;       //数据位
                    switch (cbStop.Text)            //停止位
                    {
                        case "1":
                            sp1.StopBits = StopBits.One;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }

                    if (sp1.IsOpen == true)//如果打开状态，则先关闭一下
                    {
                        sp1.Close();
                    }

                    //设置必要控件不可用
                    cbSerial.Enabled = false;
                    cbBaudRate.Enabled = false;
                    cbDataBits.Enabled = false;
                    cbStop.Enabled = false;
                    cbParity.Enabled = false;

                    sp1.Open();     //打开串口
                    btnSwitch.Text = "关闭串口";
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message, "Error");
                    tmSend.Enabled = false;
                    return;
                }
            }
            else
            {
                //恢复控件功能
                //设置必要控件不可用
                cbSerial.Enabled = true;
                cbBaudRate.Enabled = true;
                cbDataBits.Enabled = true;
                cbStop.Enabled = true;
                cbParity.Enabled = true;
                sp1.Close();                    //关闭串口
                btnSwitch.Text = "打开串口";
            }
        }

        //退出按钮
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //关闭时事件
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            INIFILE.Profile.SaveProfile();
            sp1.Close();
        }

        private void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
                //正则匹配
                string patten = "[0-9a-fA-F]|\b|0x|0X| "; //“\b”：退格键
                Regex r = new Regex(patten);
                Match m = r.Match(e.KeyChar.ToString());

                if (m.Success )
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
        }

        private void txtSend_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSecond_KeyPress(object sender, KeyPressEventArgs e)
        {
            string patten = "[0-9]|\b"; //“\b”：退格键
            Regex r = new Regex(patten);
            Match m = r.Match(e.KeyChar.ToString());

            if (m.Success)
            {
                e.Handled = false;   //没操作“过”，系统会处理事件    
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}

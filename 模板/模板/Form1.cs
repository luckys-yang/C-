using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// 添加指令集
using System.IO.Ports;
using System.Runtime.InteropServices;


namespace TEST1
{
    public partial class My_EXE : Form
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string  key, string val, string filePath); // 系统DLL导入ini写函数
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string  key, string def, StringBuilder retVal,int size,  string filePath); // 系统DLL导入ini读函数
        string FileName = System.AppDomain.CurrentDomain.BaseDirectory + "Backup.ini";  // ini文件名
        StringBuilder BackupBuf = new StringBuilder(50);    // 存储读出的ini内容变量

        bool Timer3_Flag = false;   // 用于串口通讯断帧

        public My_EXE()
        {
            InitializeComponent();
            // 禁止这种异常的抛出(不加的话运行会报错)，避免出现跨线程访问控件
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            // 支持中文
            serialPort1.Encoding = Encoding.GetEncoding("GB2312");
        }
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        //打开软件加载
        private void My_EXE_Load(object sender, EventArgs e)
        {
            SearchAndAddSerialToComboBox(serialPort1, comboBox1);
            // 恢复发送栏
            GetPrivateProfileString("串口1", "发送栏", "", BackupBuf, 50, FileName);
            textBox3.Text = BackupBuf.ToString();
        }
        // 手动扫描并添加可用串口
        private void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)
        {
            MyBox.Text = " ";
            // 获取本机串口列表
            string[] ComputerPortName = SerialPort.GetPortNames();
            string BackupPort;

            // 恢复端口号
            GetPrivateProfileString("串口1", "端口号", "", BackupBuf, 50, FileName);
            BackupPort = BackupBuf.ToString();
            //清空下拉框
            MyBox.Items.Clear();
            for (byte i = 0; i < ComputerPortName.Length; i++)
            {
                try
                {
                    MyPort.PortName = ComputerPortName[i];
                    MyPort.Open();
                    MyBox.Items.Add(MyPort.PortName);
                    MyPort.Close();
                    // 检查端口号是否有效,端口号初始化为备份端口号
                    if (BackupPort == MyPort.PortName)
                    {
                        comboBox1.Text = BackupPort;
                    }
                    // 备份端口无效则默认第一个
                    if (MyBox.Text == "")
                    {
                        MyBox.Text = MyPort.PortName;
                    }
                }
                catch
                {

                }
            }
            if (MyBox.Text == "")
            {
                textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                textBox1.AppendText("没有检测到串口工具!\r\n");
            }
            // 备份端口号
            WritePrivateProfileString("串口1", "端口号", MyBox.Text, FileName);
        }
        //【手动扫描】
        private void button1_Click(object sender, EventArgs e)
        {
            // 先判断串口是否打开，打开了则要关闭再扫描
            if (serialPort1.IsOpen == true)
            {
                serialPort1.Close();
                button2.BackgroundImage = Properties.Resources.b1;
                button2.Tag = "OFF";
                textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                textBox1.AppendText("扫描并添加串口时关闭串口!\r\n");
            }
            SearchAndAddSerialToComboBox(serialPort1, comboBox1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Tag.ToString() == "OFF")
            {
                // 打开串口
                try
                {
                    serialPort1.PortName = comboBox1.Text;
                    // 把字符串波特率转换为32位波特率
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.Open();
                    button2.BackgroundImage = Properties.Resources.b2;
                    button2.Tag = "ON";
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    textBox1.AppendText("手动打开串口!\r\n");
                    // 开启定时器
                    timer1.Start();
                }
                catch
                {
                    serialPort1.Close();
                    button2.BackgroundImage = Properties.Resources.b1;
                    button2.Tag = "OFF";
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    textBox1.AppendText("串口打开失败!\r\n");
                    timer1.Stop();
                }
            }
            else
            {
                // 关闭串口
                serialPort1.Close();
                button2.BackgroundImage = Properties.Resources.b1;
                button2.Tag = "OFF";
                textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                textBox1.AppendText("手动关闭串口!\r\n");
                timer1.Stop();
            }
        }
        //【下拉框发生变化】
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen == true)
            {
                serialPort1.Close();
                try
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.Open();
                    button2.BackgroundImage = Properties.Resources.b2;
                    button2.Tag = "ON";
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    textBox1.AppendText("串口更换成功!\r\n");
                }
                catch
                {
                    serialPort1.Close();
                    button2.BackgroundImage = Properties.Resources.b1;
                    button2.Tag = "OFF";
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    textBox1.AppendText("串口更换失败!\r\n");
                }
            }
            // 备份端口号
            WritePrivateProfileString("串口1", "端口号", comboBox1.Text, FileName);
        }
        //【定时器扫描】
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                serialPort1.Close();
                button2.BackgroundImage = Properties.Resources.b1;
                button2.Tag = "OFF";
                // 重新扫描并添加串口
                SearchAndAddSerialToComboBox(serialPort1, comboBox1);
            }
        }
        // 串口接收
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 接格式为ASCII
            if (!checkBox1.Checked)
            {
                try
                {
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    string str = serialPort1.ReadExisting();    // 以字符串格式读
                    textBox1.AppendText(str);

                    // 统计接收字节数
                    UInt32 RBytes = Convert.ToUInt32(textBox5.Text, 10);    // 定义接收字节数变量并初始化为已接收字节数
                    RBytes += (UInt32)str.Length;   // 加
                    textBox5.Text = Convert.ToString(RBytes, 10);   // 转换为字符串
                }
                catch
                {
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    textBox1.AppendText("ASCII格式接收错误!\r\n");
                }
            }
            // 接收格式为HEX
            else
            {
                try
                {
                    // 断帧
                    if (Timer3_Flag)
                    {
                        Timer3_Flag = false;
                        textBox1.AppendText("\r\n");
                        textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    }

                    byte[] data = new byte[serialPort1.BytesToRead];   // 定义缓冲区
                    serialPort1.Read(data, 0, data.Length);
                    // 遍历
                    foreach (byte Number in data)
                    {
                        string str = Convert.ToString(Number, 16).ToUpper();
                        textBox1.AppendText((str.Length == 1 ? "0" + str : str) + " ");
                    }
                    // 
                    // 统计接收字节数
                    UInt32 RBytes = Convert.ToUInt32(textBox5.Text, 10);    // 定义接收字节数变量并初始化为已接收字节数
                    RBytes += (UInt32)data.Length;   // 加
                    textBox5.Text = Convert.ToString(RBytes, 10);   // 转换为字符串
                }
                catch
                {
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    textBox1.AppendText("HEX格式接收错误!\r\n");
                }
            }
        }
        // 【清屏】
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox4.Text = "0";
            textBox5.Text = "0";
        }
        // CRC计算
        private UInt16 Crc_Check(byte[] Data, byte DataLEN)
        {
            UInt16 CRC = 0xFFFF;

            for(byte i = 0; i < DataLEN; i++)
            {
                CRC = Data[i];
                for(byte j = 0;j < 8; j++)
                {
                    if((CRC & 0x0001) == 0x0001)
                    {
                        CRC = (UInt16)((CRC >> 1) ^ 0xA001);
                    }
                    else
                    {
                        CRC = (UInt16)(CRC >> 1);
                    }
                }
            }
            CRC = (UInt16)((CRC >> 8) + (CRC << 8));
            return CRC;
        }
        // 【发送】
        private void button4_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[1];

            // 发送格式为ASCII
            if (!checkBox3.Checked)
            {
                try
                {
                    // 支持中文
                    Encoding Chinese = System.Text.Encoding.GetEncoding("GB2312");
                    byte[] SendByte = Chinese.GetBytes(textBox3.Text);

                    foreach (byte Number in SendByte)
                    {
                        data[0] = Number;
                        serialPort1.Write(data, 0, 1);
                    }
                    //发送新行
                    if (checkBox5.Checked)
                    {
                        data[0] = 0x0D;
                        serialPort1.Write(data, 0, 1);  // 发送回车
                        data[0] = 0x0A;
                        serialPort1.Write(data, 0, 1);  // 发送换行
                    }
                    //统计发送字节数(先把当前已发送的字节转换为32位)
                    UInt32 SBytes = Convert.ToUInt32(textBox4.Text, 10);
                    SBytes += (UInt32)SendByte.Length;
                    // 加回车换行2个 字节
                    if (checkBox5.Checked)
                    {
                        SBytes += 2;
                    }
                    textBox4.Text = Convert.ToString(SBytes, 10);
                }
                catch
                {
                    textBox1.AppendText("\r\n串口数据发送失败!!!\r\n");
                    serialPort1.Close();
                    button2.BackgroundImage = Properties.Resources.b1;
                    button2.Tag = "OFF";
                }
            }
            // 发送格式为HEX
            else
            {
                // 处理字符串
                string Buf = textBox3.Text;
                // 去掉0x 0X
                Buf = Buf.Replace("0x", string.Empty);
                Buf = Buf.Replace("0X", string.Empty);
                Buf = Buf.Replace(" ", string.Empty);
                byte[] Calculate_CRC = new byte[(Buf.Length - Buf.Length % 2) / 2];
                textBox3.Text = "";
                //循环发送，保证都是2位，1位就丢弃
                for (int i = 0; i < (Buf.Length - Buf.Length % 2) / 2; i++)
                {
                    textBox3.AppendText(Buf.Substring(i * 2, 2) + " ");
                    try
                    {
                        // 转换为16进制发送
                        data[0] = Convert.ToByte(Buf.Substring(i * 2, 2), 16);
                        serialPort1.Write(data, 0, 1);
                        Calculate_CRC[i] = data[0];
                    }
                    catch
                    {
                        textBox1.AppendText("\r\n串口数据发送失败!!!\r\n");
                        serialPort1.Close();
                        button2.BackgroundImage = Properties.Resources.b1;
                        button2.Tag = "OFF";
                    }
                }
                // 发送CRC
                if (checkBox4.Checked == true)
                {
                    UInt32 CRC = Crc_Check(Calculate_CRC, (byte)Calculate_CRC.Length);
                    byte CRC_H = (byte)(CRC >> 8);
                    byte CRC_L = (byte)CRC;

                    try
                    {
                        data[0] = CRC_L;
                        serialPort1.Write(data, 0, 1);  // 发送低位
                        data[0] = CRC_H;
                        serialPort1.Write(data, 0, 1);  // 发送高位
                    }
                    catch
                    {
                        textBox1.AppendText("\r\n串口数据发送失败!!!\r\n");
                        serialPort1.Close();
                        button2.BackgroundImage = Properties.Resources.b1;
                        button2.Tag = "OFF";
                    }
                }
                //统计发送字节数
                UInt32 SBytes = Convert.ToUInt32(textBox4.Text,10);
                SBytes += (UInt32)Calculate_CRC.Length;
                // 如果有CRC则加多2个 字节
                if (checkBox4.Checked)
                {
                    SBytes += 2;
                }
                textBox4.Text = Convert.ToString(SBytes, 10);
            }
            if (checkBox6.Checked)
            {
                textBox3.Text = " ";
            }
        }
        // 清除发送
        private void button5_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            //HEX发送
            if (checkBox3.Checked == true)
            {
                //CRC有效
                checkBox4.Enabled = true;
                // 回车换行无效
                checkBox5.Enabled = false;
            }
                //ASCII发送
            else
            {
                //CRC无效
                checkBox4.Enabled = false;
                // 回车换行有效
                checkBox5.Enabled = true;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // 备份发送栏
            WritePrivateProfileString("串口1", "发送栏", textBox3.Text, FileName);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            // 启动定时发送
            if (checkBox2.Checked)
            {
                textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                textBox1.AppendText("启动定时发送!!!\r\n");

                // 设置时间
                try
                {
                    timer2.Interval = Convert.ToUInt16(textBox2.Text, 10);
                }
                catch
                {
                    MessageBox.Show("输入时间有误，设定为默认值", "提示");
                    textBox2.Text = "1000";
                    timer2.Interval = 1000;
                }
                // 启动定时器2
                timer2.Start();
            }
            // 关闭定时发送
            else
            {
                textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                textBox1.AppendText("关闭定时发送!!!\r\n");
                // 关闭定时器
                timer2.Stop();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // 触发按钮4单击事件
            button4.PerformClick();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // 设置时间
            try
            {
                timer2.Interval = Convert.ToUInt16(textBox2.Text, 10);
            }
            catch
            {
                MessageBox.Show("输入时间有误，设定为默认值", "提示");
                textBox2.Text = "1000";
                timer2.Interval = 1000;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // 启动与禁用断帧功能
            if (checkBox1.Checked)
            {
                textBox6.Enabled = true;
                // 启动定时器3
                try
                {
                    timer3.Interval = Convert.ToUInt16(textBox6.Text, 10);
                    timer3.Start();
                }
                catch
                {
                    MessageBox.Show("输入时间有误，设定为默认值", "提示");
                    textBox3.Text = "500";
                    timer3.Interval = 500;
                }
            }
            else
            {
                textBox6.Enabled = false;
                timer3.Stop();
            }
        }
        // 定时器3中断
        private void timer3_Tick(object sender, EventArgs e)
        {
            Timer3_Flag = true;
        }
        // 设置定时器3时间
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            try
            {
                timer3.Interval = Convert.ToUInt16(textBox6.Text, 10);
            }
            catch
            {
                MessageBox.Show("输入时间有误，设定为默认值", "提示");
                textBox6.Text = "500";
                timer3.Interval = 500;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
        }
    }
}

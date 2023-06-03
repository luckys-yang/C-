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


namespace STC实战上位机
{
    public partial class My_EXE : Form
    {
        byte Send_Num = 0;  //发送编号

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string  key, string val, string filePath); // 系统DLL导入ini写函数
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string  key, string def, StringBuilder retVal,int size,  string filePath); // 系统DLL导入ini读函数
        string FileName = System.AppDomain.CurrentDomain.BaseDirectory + "Backup.ini";  // ini文件名
        StringBuilder BackupBuf = new StringBuilder(50);    // 存储读出的ini内容变量

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
            // 显示时间
            textBox4.Text = DateTime.Now.ToString("HH:mm:ss");
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
            byte[] Data = new byte[serialPort1.BytesToRead];    // 定义缓冲区

            // 检验数据是否接收完
            bool Rec_Flag = true;
            if (Send_Num == 1)  // 采集命令
            {
                if (Data.Length < 9)
                {
                    Rec_Flag = false;
                }
            }
            else if (Send_Num == 2)  // 控制命令
            {
                if (Data.Length < 8)
                {
                    Rec_Flag = false;
                }
            }
            else
            {
                serialPort1.DiscardOutBuffer(); //清空缓存
                Rec_Flag = false;
            }
            // 数据处理
            if (Rec_Flag)
            {
                // 提取数据
                try
                {
                    serialPort1.Read(Data, 0, Data.Length);
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "收 <-");
                    foreach (byte Number in Data)
                    {
                        string str = Convert.ToString(Number, 16).ToUpper();
                        textBox1.AppendText((str.Length == 1 ? "0" + str : str) + " ");
                    }
                    textBox1.AppendText("\r\n");
                    textBox1.AppendText("\r\n");
                }
                catch
                {
 
                }
                // 数据解析
                if (Data.Length == 9)
                {
                    UInt16 CRC = Crc_Check(Data, 7);
                    byte CRC_H = (byte)( CRC >> 8);
                    byte CRC_L = (byte)CRC;

                    if (((Data[7] == CRC_L) && (Data[8] == CRC_H)) || ((Data[7] == CRC_H) && (Data[8] == CRC_L)))
                    {
                        // 校验地址与功能码
                        if ((Data[0] == 0x01) && (Data[1] == 0x03))
                        {
                            // 校验数据长度
                            if (Data[2] == 0x04)
                            {
                                // PCB板温度
                                float Temp_float = (float)(Data[4] / 2.0 - 30);
                                textBox2.Text = Temp_float.ToString() + "℃";

                                // PWM灯亮度值
                                switch (Data[6])
                                {
                                    case 0:
                                        {
                                            textBox3.Text = "0";
                                            break;
                                        }
                                    case 20:
                                        {
                                            textBox3.Text = "20";
                                            break;
                                        }
                                    case 40:
                                        {
                                            textBox3.Text = "40";
                                            break;
                                        }
                                    case 60:
                                        {
                                            textBox3.Text = "60";
                                            break;
                                        }
                                    case 80:
                                        {
                                            textBox3.Text = "80";
                                            break;
                                        }
                                    case 100:
                                        {
                                            textBox3.Text = "100";
                                            break;
                                        }
                                    default: break;
                                }
                            }
                        }
                    }
                    else
                    {
                        textBox1.AppendText("\r\nCRC校验码错误，请检查\r\n");
                    }
                }
                else if (Data.Length == 8)
                {
                     UInt16 CRC = Crc_Check(Data, 6);
                    byte CRC_H = (byte)( CRC >> 8);
                    byte CRC_L = (byte)CRC;

                    if (((Data[7] == CRC_L) && (Data[8] == CRC_H)) || ((Data[7] == CRC_H) && (Data[8] == CRC_L)))
                    {
                        // 校验地址与功能码
                        if ((Data[0] == 0x01) && (Data[1] == 0x06))
                        {
                            if ((Data[2] == 0x94) && (Data[3] == 0xC2))
                            {
                                // PWM灯亮度值
                                switch (Data[5])
                                {
                                    case 0:
                                        {
                                            textBox3.Text = "0";
                                            break;
                                        }
                                    case 20:
                                        {
                                            textBox3.Text = "20";
                                            break;
                                        }
                                    case 40:
                                        {
                                            textBox3.Text = "40";
                                            break;
                                        }
                                    case 60:
                                        {
                                            textBox3.Text = "60";
                                            break;
                                        }
                                    case 80:
                                        {
                                            textBox3.Text = "80";
                                            break;
                                        }
                                    case 100:
                                        {
                                            textBox3.Text = "100";
                                            break;
                                        }
                                    default: break;
                                }
                            }
                        }
                    }
                }
            }
        }
        // 【清屏】
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
        // CRC计算
        private UInt16 Crc_Check(byte[] Data, byte DataLEN)
        {
            UInt16 CRC = 0xFFFF;

            for(byte i = 0; i < DataLEN; i++)
            {
                CRC ^= Data[i];
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

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        // 采集数据
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    byte[] Data = new byte[8] {0x01, 0x03, 0x9C, 0x41, 0x00, 0x02, 0x00, 0x00};
                    //插入CRC
                    UInt16 CRC = Crc_Check(Data, 6);
                    byte CRC_H = (byte)(CRC >> 8);
                    byte CRC_L = (byte)CRC;
                    Data[6] = CRC_L;
                    Data[7] = CRC_H;
                    // 发送
                    serialPort1.Write(Data, 0, 8);
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "发 ->");
                    foreach (byte Number in Data)
                    {
                        string str = Convert.ToString(Number, 16).ToUpper();
                        textBox1.AppendText((str.Length == 1 ? "0" + str : str) + " ");
                    }
                    textBox1.AppendText("\r\n");
                    Send_Num = 1;
                }
                catch
                {
                    serialPort1.Close();
                    button2.BackgroundImage = Properties.Resources.b1;
                    button2.Tag = "OFF";
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    textBox1.AppendText("采集命令发送失败\r\n");
                    timer1.Stop();  //关闭串口工具异常检测
                }
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    byte[] Data = new byte[8] { 0x01, 0x06, 0x9C, 0x42, 0x00, 0x00, 0x00, 0x00 };
                    // 获取PWM灯亮度值
                    switch (comboBox3.Text.Length)
                    {
                        case 2:
                            {
                                // 0%则把%去掉
                                Data[5] = Convert.ToByte(comboBox3.Text.Substring(0, 1), 10);
                                break;
                            }
                        case 3:
                            {
                                // 10%则把%去掉
                                Data[5] = Convert.ToByte(comboBox3.Text.Substring(0, 2), 10);
                                break;
                            }
                        case 4:
                            {
                                // 100%则把%去掉
                                Data[5] = Convert.ToByte(comboBox3.Text.Substring(0, 3), 10);
                                break;
                            }
                        default: break;
                    }
                    //插入CRC
                    UInt16 CRC = Crc_Check(Data, 6);
                    byte CRC_H = (byte)(CRC >> 8);
                    byte CRC_L = (byte)CRC;
                    Data[6] = CRC_L;
                    Data[7] = CRC_H;
                    // 发送
                    serialPort1.Write(Data, 0, 8);
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "发 ->");
                    foreach (byte Number in Data)
                    {
                        string str = Convert.ToString(Number, 16).ToUpper();
                        textBox1.AppendText((str.Length == 1 ? "0" + str : str) + " ");
                    }
                    textBox1.AppendText("\r\n");
                    // 重新启动timer2,错开采集与控制指令
                    timer2.Stop();
                    timer2.Start();
                    Send_Num = 2;
                }
                catch
                {
                    serialPort1.Close();
                    button2.BackgroundImage = Properties.Resources.b1;
                    button2.Tag = "OFF";
                    textBox1.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + "->");
                    textBox1.AppendText("控制命令发送失败\r\n");
                    timer1.Stop();  //关闭串口工具异常检测
                }
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox5.Text == "")
            {
                MessageBox.Show("不能为空", "提示");
                return;
            }
            if (button5.Tag.ToString() == "1")
            {
                int i = 0;
                // 处理字符串
                string Buf = textBox5.Text;
                // 去掉0x 0X
                Buf = Buf.Replace("0x", string.Empty);
                Buf = Buf.Replace("0X", string.Empty);
                Buf = Buf.Replace(" ", string.Empty);
                byte[] data = new byte[((Buf.Length - Buf.Length % 2) / 2) + 2];
                //循环发送，保证都是2位，1位就丢弃
                for (i = 0; i < (Buf.Length - Buf.Length % 2) / 2; i++)
                {
                    try
                    {
                        // 转换为16进制
                        data[i] = Convert.ToByte(Buf.Substring(i * 2, 2), 16);
                    }
                    catch
                    {
                        MessageBox.Show("非十六进制！！", "警告");
                        textBox5.Text = "";
                        return;
                    }
                }
                UInt16 CRC = Crc_Check(data, (byte)i);
                byte CRC_H = (byte)(CRC >> 8);
                byte CRC_L = (byte)CRC;
                // 默认低位在前高位在后
                data[i] = CRC_L;
                data[i + 1] = CRC_H;
                textBox5.Text = "";
                textBox5.ForeColor = Color.Red;
                foreach (byte Number in data)
                {
                    string str = Convert.ToString(Number, 16).ToUpper();
                    textBox5.AppendText((str.Length == 1 ? "0" + str : str) + " ");
                }
                button5.Tag = "2";
                button5.Text = "清除";
            }
            else
            {
                textBox5.Text = "";
                button5.Text = "计算";
                textBox5.ForeColor = Color.Black;
                button5.Tag = "1";
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            textBox4.Text = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}

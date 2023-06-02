using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TEST1
{
    public partial class Form2 : Form
    {
        private const int StartPrint = 40;  // 点坐标偏移量
        private const int Unit_length = 32; // 单位格大小

        private int DrawStep = 8;   // 默认绘制单位
        private const int MaxStep = 32; // 绘制单位最大值
        private const int MinStep = 1;  // 绘制单位最小值

        private List<byte> DataList = new List<byte>(); //线性链表

        private Pen TablePen = new Pen(Color.FromArgb(0x00, 0x00, 0x00));   //轴线颜色
        private Pen LinesPen = new Pen(Color.FromArgb(0xFF, 0x00, 0x00));   //波形颜色

        public Form2()
        {
            // 波形稳定刷新---开启双缓冲
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // 重新设定波形显示窗体尺寸
            int width = Screen.GetWorkingArea(this).Width - My_EXE.MainForm.Width;
            int Heigth = this.Height - this.ClientRectangle.Height;
            Heigth += Unit_length * 16;
            Heigth += StartPrint * 2;
            this.Size = new Size(width, Heigth);
        }

        // 链表尾部添加数据
        public void AddDataToList(byte[] Data)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                DataList.Add(Data[i]);
            }
            // 刷新显示
            this.Invalidate();
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            String Str = "";
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();

            // 绘画横轴线
            for (int i = 0; i < (this.ClientRectangle.Height - StartPrint) / Unit_length; i++)
            {
                // 画直线---画笔，起始坐标，终点坐标
                e.Graphics.DrawLine(TablePen, StartPrint, StartPrint + i * Unit_length, this.ClientRectangle.Width, StartPrint + i * Unit_length);
                // "X"表示转换为16进制
                Str = ((16 - i) * 16).ToString("X");
                Str = "0x" + (Str.Length == 1 ? Str + "0" : Str);
                if (0 == i)
                {
                    Str = "0xFF";
                }
                gp.AddString(Str, this.Font.FontFamily, (int)FontStyle.Regular, 13, new RectangleF(0, StartPrint + i * Unit_length - 8, 400, 50), null);
            }
            // 绘画纵轴线
            for (int i = 0; i <= (this.ClientRectangle.Width - StartPrint) / Unit_length; i++)
            {
                e.Graphics.DrawLine(TablePen, StartPrint + i * Unit_length, StartPrint, StartPrint + i * Unit_length, StartPrint + Unit_length * 16);
                gp.AddString((i * (Unit_length / DrawStep)).ToString(), this.Font.FontFamily, (int)FontStyle.Regular, 13, new RectangleF(StartPrint + i * Unit_length - 7, this.ClientRectangle.Height - StartPrint + 4, 400, 50), null);
            }
            // 绘制文字
            e.Graphics.DrawPath(Pens.Black, gp);
            // 如果数据量大于可容纳的数据量，即删除最左数据
            if (DataList.Count >= (this.ClientRectangle.Width - StartPrint) / DrawStep)
            {
                DataList.RemoveRange(0, DataList.Count - (this.ClientRectangle.Width - StartPrint) / DrawStep);
            }
            for (int i = 0; i < DataList.Count - 1; i++)
            {
                e.Graphics.DrawLine(LinesPen, StartPrint + i * DrawStep, StartPrint + Unit_length * 16 - DataList[i] * (Unit_length / 16), StartPrint + (i + 1) * DrawStep, StartPrint + Unit_length * 16 - DataList[i + 1] * (Unit_length / 16));
            }
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                    // 退出波形
                case Keys.Escape:
                    {
                        this.Close();
                        break;
                    }
                case Keys.PageUp:
                    {
                        // 放大波形
                        if (DrawStep < MaxStep)
                        {
                            DrawStep++;
                            this.Invalidate();
                        }
                        break;
                    }
                case Keys.PageDown:
                    {
                        // 缩小波形
                        if (DrawStep > MinStep)
                        {
                            DrawStep--;
                            this.Invalidate();
                        }
                        break;
                    }
                default: break;
            }
        }
    }
}

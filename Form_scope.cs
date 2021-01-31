using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace GameVirtualScope
{
    public partial class Form_scope : Form
    {

        //放大倍数、刷新频率、是否反转颜色
        public double times;
        public int freq;
        public bool color_reverse = false;
        public bool sharpen = false;

        private int aim_w;
        private int aim_h;
        private int aim_x, aim_y;

        //画东西的笔
        Pen pen_cross = new Pen(Color.Gold, 12f);

        #region 穿透
        #region 在窗口结构中为指定的窗口设置信息
        /// <summary>
        /// 在窗口结构中为指定的窗口设置信息
        /// </summary>
        /// <param name="hwnd">欲为其取得信息的窗口的句柄</param>
        /// <param name="nIndex">欲取回的信息</param>
        /// <param name="dwNewLong">由nIndex指定的窗口信息的新值</param>
        /// <returns></returns>
        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);
        #endregion

        #region 从指定窗口的结构中取得信息
        /// <summary>
        /// 从指定窗口的结构中取得信息
        /// </summary>
        /// <param name="hwnd">欲为其获取信息的窗口的句柄</param>
        /// <param name="nIndex">欲取回的信息</param>
        /// <returns></returns>
        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);
        #endregion

        private const uint WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = (-20);
        #endregion 
        /// <summary>
        /// timer定时程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //如果窗口是可见的，则抓图
            if (this.Visible)
            {
                Bitmap image = new Bitmap(aim_w, aim_h);
                Graphics imgGraphics = Graphics.FromImage(image);
                if (color_reverse)
                {
                    imgGraphics.CopyFromScreen(aim_x, aim_y, 0, 0, new Size(aim_w, aim_h), CopyPixelOperation.MergePaint);
                }
                else
                    imgGraphics.CopyFromScreen(aim_x, aim_y, 0, 0, new Size(aim_w, aim_h));
                //实际上是被拉伸显示的

                if (sharpen)
                    this.BackgroundImage = SharpenImage(image);
                else
                    this.BackgroundImage = image;

                GetWindowLong(this.Handle, GWL_EXSTYLE);
                SetWindowLong(this.Handle, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED);

            }
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0014) // 禁掉清除背景消息
                return;
            base.WndProc(ref m);
        }
        private void Form_scope_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;//设置本窗体
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲

            //GetWindowLong(this.Handle, GWL_EXSTYLE);
            //SetWindowLong(this.Handle, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED);
            //return;
            //设置timer
            timer1.Interval = 1000 / freq;
            timer1.Start();

            //计算aim区域大小
            aim_w = (int)(this.Width / times);
            aim_h = (int)(this.Height / times);
            aim_x = Screen.PrimaryScreen.Bounds.Width / 2 - aim_w / 2;
            aim_y = Screen.PrimaryScreen.Bounds.Height / 2 - aim_h / 2;

            //准星
            // panel1.Location = new Point(this.Width / 2 - panel1.Width / 2, this.Height / 2 - panel1.Height / 2);

        }

        private void Form_scope_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }

        public Form_scope()
        {
            InitializeComponent();
        }
        //锐化
        public Bitmap SharpenImage(Bitmap bmp)
        {
            int height = bmp.Height;
            int width = bmp.Width;
            Bitmap newbmp = new Bitmap(width, height);

            LockBitmap lbmp = new LockBitmap(bmp);
            LockBitmap newlbmp = new LockBitmap(newbmp);
            lbmp.LockBits();
            newlbmp.LockBits();

            Color pixel;
            //拉普拉斯模板
            int[] Laplacian = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int r = 0, g = 0, b = 0;
                    int Index = 0;
                    for (int col = -1; col <= 1; col++)
                    {
                        for (int row = -1; row <= 1; row++)
                        {
                            pixel = lbmp.GetPixel(x + row, y + col); r += pixel.R * Laplacian[Index];
                            g += pixel.G * Laplacian[Index];
                            b += pixel.B * Laplacian[Index];
                            Index++;
                        }
                    }
                    //处理颜色值溢出
                    r = r > 255 ? 255 : r;
                    r = r < 0 ? 0 : r;
                    g = g > 255 ? 255 : g;
                    g = g < 0 ? 0 : g;
                    b = b > 255 ? 255 : b;
                    b = b < 0 ? 0 : b;
                    newlbmp.SetPixel(x - 1, y - 1, Color.FromArgb(r, g, b));
                }
            }
            lbmp.UnlockBits();
            newlbmp.UnlockBits();
            return newbmp;
        }
    }
}

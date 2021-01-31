using System;
using System.Collections;
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
    public partial class Form1 : Form
    {
        //作者QQ470138890 佳好友备注来意
        Form_scope form_scope;
        public Form1()
        {
            InitializeComponent();
        }
        private int Hotkey1;
        public delegate void HotkeyEventHandler(int HotKeyID);

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
        private void Form1_Load(object sender, EventArgs e)
        {
           

            Hotkey hotkey;
            hotkey = new Hotkey(this.Handle);
            Hotkey1 = hotkey.RegisterHotkey(System.Windows.Forms.Keys.F4, 0);   //定义快键(Ctrl + F2)
            hotkey.OnHotkey += Hotkey_OnHotkey;

        }

        private void Hotkey_OnHotkey(int HotKeyID)
        {
            if (HotKeyID == Hotkey1)
            {
                checkBox_switch.Checked = !checkBox_switch.Checked;
            }
        }

        private void Chanage() {
            if (form_scope != null)
            {
                form_scope.Dispose();
                form_scope = null;
            }

            int scope_weight = (int)(numericUpDown_scope_weight.Value);
            int scope_height = (int)(numericUpDown_scope_height.Value);

            double times = (double)numericUpDown_scope_times.Value;
            int freq = (int)numericUpDown_freq.Value;

            int screen_w = Screen.PrimaryScreen.Bounds.Width;
            int screen_h = Screen.PrimaryScreen.Bounds.Height;

            int scope_x = screen_w / 2 + (int)numericUpDown_scope_pos_X.Value - scope_weight / 2;
            int scope_y = screen_h / 2 + (int)numericUpDown_scope_pos_Y.Value - scope_height / 2;

            int scope_opa = (int)numericUpDown_scope_opa.Value;
            bool color_reverse = checkBox_color_rev.Checked;
            bool sharpen = checkBox_sharpen.Checked;

            form_scope = new Form_scope();
            form_scope.times = times;
            form_scope.freq = freq;
            form_scope.color_reverse = color_reverse;
            form_scope.sharpen = sharpen;
            form_scope.Height = scope_height;
            form_scope.Width = scope_weight;
            form_scope.Location = new Point(scope_x, scope_y);
            form_scope.Show();
            //form_scope.Visible = false;
             form_scope.Opacity = scope_opa / 100.0;


        }

        private void checkBox_switch_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_switch.Checked)
            {
                Chanage();
               // this.WindowState = FormWindowState.Minimized;
            }
            else
            {
                try
                {
                    form_scope.Visible = false;

                }
                catch (Exception)
                {

                   
                }
              //  this.WindowState = FormWindowState.Normal;
            }
        }
    }
  

    public class Hotkey : System.Windows.Forms.IMessageFilter
    {
        Hashtable keyIDs = new Hashtable();
        IntPtr hWnd;

        public event Form1.HotkeyEventHandler OnHotkey;

        public enum KeyFlags
        {
            MOD_ALT = 0x1,
            MOD_CONTROL = 0x2,
            MOD_SHIFT = 0x4,
            MOD_WIN = 0x8
        }
        [DllImport("user32.dll")]
        public static extern UInt32 RegisterHotKey(IntPtr hWnd, UInt32 id, UInt32 fsModifiers, UInt32 vk);

        [DllImport("user32.dll")]
        public static extern UInt32 UnregisterHotKey(IntPtr hWnd, UInt32 id);

        [DllImport("kernel32.dll")]
        public static extern UInt32 GlobalAddAtom(String lpString);

        [DllImport("kernel32.dll")]
        public static extern UInt32 GlobalDeleteAtom(UInt32 nAtom);

        public Hotkey(IntPtr hWnd)
        {
            this.hWnd = hWnd;
            Application.AddMessageFilter(this);
        }

        public int RegisterHotkey(Keys Key, KeyFlags keyflags)
        {
            UInt32 hotkeyid = GlobalAddAtom(System.Guid.NewGuid().ToString());
            RegisterHotKey((IntPtr)hWnd, hotkeyid, (UInt32)keyflags, (UInt32)Key);
            keyIDs.Add(hotkeyid, hotkeyid);
            return (int)hotkeyid;
        }

        public void UnregisterHotkeys()
        {
            Application.RemoveMessageFilter(this);
            foreach (UInt32 key in keyIDs.Values)
            {
                UnregisterHotKey(hWnd, key);
                GlobalDeleteAtom(key);
            }
        }

        public bool PreFilterMessage(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == 0x312)
            {
                if (OnHotkey != null)
                {
                    foreach (UInt32 key in keyIDs.Values)
                    {
                        if ((UInt32)m.WParam == key)
                        {
                            OnHotkey((int)m.WParam);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

}

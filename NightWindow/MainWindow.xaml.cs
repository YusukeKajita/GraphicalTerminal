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
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;

namespace NightWindow
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Process m_cmd;
        StreamWriter m_standardInputCmd;
        private bool m_isCheckEcho = false;
        private List<string> m_history;
        private int m_historyIndex;
        private float moves;
        private DispatcherTimer m_timer;

        public MainWindow()
        {
            InitializeComponent();
            this.Top = 0;
            this.Left = 0;
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 5;

            this.MouseWheel += MainWindow_MouseWheel;
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            this.TextBox_Command.KeyDown += TextBox_Command_KeyDown;

            this.TextBox_Command.Focus();



            this.m_cmd = new System.Diagnostics.Process();


            //ComSpec(cmd.exe)のパスを取得して、FileNameプロパティに指定 
            this.m_cmd.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
            //出力を読み取れるようにする 
            this.m_cmd.StartInfo.UseShellExecute = false;
            this.m_cmd.StartInfo.RedirectStandardOutput = true;
            this.m_cmd.StartInfo.RedirectStandardError = true;
            this.m_cmd.StartInfo.RedirectStandardInput = true;
            this.m_cmd.StartInfo.CreateNoWindow = true;
            //ウィンドウを表示しないようにする 
            //コマンドラインを指定（"/c"は実行後閉じるために必要） 
            this.m_cmd.StartInfo.Arguments = @"dir c:\ /w";
            this.m_cmd.OutputDataReceived += M_Cmd_OutputDataReceived;
            this.m_cmd.ErrorDataReceived += M_Cmd_OutputDataReceived;
            //起動 
            this.m_cmd.Start();
            //this.m_StandardOutputCmd = this.m_Cmd.StandardOutput; 
            this.m_cmd.BeginOutputReadLine();
            this.m_cmd.BeginErrorReadLine();


            this.m_standardInputCmd = this.m_cmd.StandardInput;
            this.m_history = new List<string>();
            this.m_history.Add("");
            this.Closed += MainWindow_Closed;
            m_timer = new DispatcherTimer();
            m_timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            m_timer.Tick += M_timer_Tick;m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            this.Height += this.moves;
            this.moves *= 0.9f;
            if (Height <= 100)
            {
                Height = 100;
                this.moves = 0;
            }
        }

        private void M_Cmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.TextBlock_Log.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!this.m_isCheckEcho)
                {
                    this.TextBlock_Log.Text += this.TextBlock_CurrentDirectory.Text + "\n";
                }
                else
                {
                    this.m_isCheckEcho = false;
                }
                if (this.TextBlock_Log.Text.Length > 1000)
                {
                    this.TextBlock_Log.Text.Remove(0, this.TextBlock_Log.Text.Length - 1000);
                }
                this.TextBlock_CurrentDirectory.Text = e.Data;
            }));

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            m_cmd.WaitForExit(1000);
            this.m_timer.Stop();
        }

        private void InitContent()
        {
        }

        void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.TextBox_Command.Focus();
        }



        void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                this.moves += e.Delta/60;
                

                
            }
            catch
            {

            }
        }

        private void TextBox_Command_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.m_isCheckEcho = true;
                this.m_standardInputCmd.WriteLine(this.TextBox_Command.Text);
                this.m_standardInputCmd.WriteLine("");
                string str = (string)this.TextBox_Command.Text.Clone();

                if(this.m_history.Count > 10)
                {
                    this.m_history.RemoveAt(0);
                }
                this.m_history.Add(str);

                this.m_historyIndex = this.m_history.Count;
                this.TextBox_Command.Text = "";
            }
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                this.m_historyIndex--;
                if(this.m_historyIndex < 0)
                {
                    this.m_historyIndex = 0;
                }
                this.TextBox_Command.Text = this.m_history[m_historyIndex];
                this.TextBox_Command.Select(this.TextBox_Command.Text.Length, 0);

            }
            if (e.Key == Key.Down)
            {
                this.m_historyIndex++;
                if (this.m_historyIndex > this.m_history.Count-1)
                {
                    this.m_historyIndex = this.m_history.Count - 1;
                }
                this.TextBox_Command.Text = this.m_history[m_historyIndex];
                this.TextBox_Command.Select(this.TextBox_Command.Text.Length, 0);

            }
        }
    }
}

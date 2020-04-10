using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MahAppBase.View
{
    /// <summary>
    /// Test.xaml 的互動邏輯
    /// </summary>
    public partial class Test : UserControl
    {
        #region Declarations
        private string _ExecPath = string.Empty;
        #endregion

        #region Property
        public string ExecPath
        {
            get
            {
                return _ExecPath;
            }
            set
            {
                _ExecPath = value;
            }
        }
        #endregion

        #region Memberfunction
        public Test(string title, string content, string execPath)
        {
            InitializeComponent();
            tbContent.Text = content;
            tbTitle.Text = title;
            this.ExecPath = execPath;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true; 
            process.StartInfo.FileName = "cmd.exe";
            process.Start();
            process.StandardInput.WriteLine($"start {ExecPath}\\START_IMS.bat");
            process.StandardInput.AutoFlush = true;
            process.StandardInput.WriteLine("exit");
        }
        #endregion
    }
}

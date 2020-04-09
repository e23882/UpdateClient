using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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

namespace MahAppBase.View
{
    /// <summary>
    /// Test.xaml 的互動邏輯
    /// </summary>
    public partial class Test : UserControl
    {

        #region Declarations
        #endregion

        #region Property

        private string _ExecPath;
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
            process.StandardInput.WriteLine($"start {ExecPath}/START_IMS.bat");
            process.StandardInput.AutoFlush = true;
            process.StandardInput.WriteLine("exit");
        }
        #endregion


    }
}

using System.Collections.Generic;
using System.Windows.Documents;
using MahAppBase.ViewModel;
using MahApps.Metro.Controls;

namespace MahAppBase
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Declarations
        private MainComponent MainViewModel = null;
        #endregion

        #region MemberFunction
        public MainWindow()
        {
            InitializeComponent();
            MainViewModel = new MainComponent();
            mwMain.DataContext = MainViewModel;
        }
        #endregion
    }
}
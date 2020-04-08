﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MahAppBase.ViewModel
{
    /// <summary>
    /// 基礎ViewModel，繼承、實作INotifyPropertyChanged Interface
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Declarations
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region MemberFunction

        public void OnPropertyChanged([CallerMemberName] string propertyName ="")
        {
            OnPropertyChangedExplicit(propertyName);
        }

        public void OnPropertyChangedExplicit(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
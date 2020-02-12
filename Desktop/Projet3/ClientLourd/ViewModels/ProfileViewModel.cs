﻿using ClientLourd.Models.Bindable;
using ClientLourd.Utilities.Commands;
using ClientLourd.Utilities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClientLourd.Services.RestService;
using ClientLourd.Views.Dialogs;
using MaterialDesignThemes.Wpf;

namespace ClientLourd.ViewModels
{
    class ProfileViewModel: ViewModelBase
    {
        private SessionInformations _sessionInformations;
        private PrivateProfileInfo _profileInfo;

        public override void AfterLogin()
        {
            _sessionInformations = (((MainWindow)Application.Current.MainWindow)?.DataContext as MainViewModel)?.SessionInformations as SessionInformations;
            Task task = GetUserInfo(_sessionInformations.User.ID);
        }

        private async Task GetUserInfo(string userID)
        {
            ProfileInfo = await RestClient.GetUserInfo(userID);
        }

        public override void AfterLogOut()
        {
        
        }

        public SessionInformations SessionInformations
        {
            get { return _sessionInformations; }
        }

        private RelayCommand<object> _closeProfileCommand;

        public ICommand CloseProfileCommand
        {
            get { return _closeProfileCommand ?? (_closeProfileCommand = new RelayCommand<object>(obj => CloseProfile(obj))); }
        }

        private async Task CloseProfile(object obj)
        {
            //await DialogHost.Show(new EditProfileDialog());
            (((MainWindow)Application.Current.MainWindow)?.DataContext as MainViewModel).ContainedView = Enums.Views.Editor.ToString();
        }

        private RelayCommand<object> _editProfileCommand;

        public ICommand EditProfileCommand
        {
            get { return _editProfileCommand ?? (_editProfileCommand = new RelayCommand<object>(obj => EditProfile(obj))); }
        }

        private async Task EditProfile(object obj)
        {
            await DialogHost.Show(new EditProfileDialog(ProfileInfo));
            //(((MainWindow)Application.Current.MainWindow)?.DataContext as MainViewModel).ContainedView = Enums.Views.Editor.ToString();
        }

        public RestClient RestClient
        {
            get { return (((MainWindow)Application.Current.MainWindow)?.DataContext as MainViewModel)?.RestClient; }
        }

        public PrivateProfileInfo ProfileInfo
        {
            get { return _profileInfo; }
            set
            {
                if (value != _profileInfo)
                {
                    _profileInfo = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}

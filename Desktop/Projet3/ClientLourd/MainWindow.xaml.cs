﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClientLourd.Utilities.Window;
using ClientLourd.Views;
using MaterialDesignThemes.Wpf;
using ClientLourd.ModelViews;
using ClientLourd.Utilities.Commands;

namespace ClientLourd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((MainViewModel) DataContext).UserLogout += OnUserLogout;
        }

        private void OnUserLogout(object source, EventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                Init();
                ChatBox.Init();
                LoginScreen.Init();
            });
        }

        private void Init()
        {
            ((ViewModelBase) DataContext).Init();
            MenuToggleButton.IsChecked = false;
        }

        /// <summary>
        /// Clear the chat notification when the chat is open or close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            ClearChatNotification();
        }

        public void ClearChatNotification()
        {
            //Clear the notification when chatToggleButton is checked or unchecked
            ((ChatViewModel) ChatBox.DataContext).ClearNotificationCommand.Execute(null);
        }

        RelayCommand<object> _exportChatCommand;

        /// <summary>
        /// Command use to export the chat as an external window
        /// </summary>
        public ICommand ExportChatCommand
        {
            get
            {
                return _exportChatCommand ??
                       (_exportChatCommand = new RelayCommand<object>(param => ExportChat(this, null),
                           o => ChatToggleButton.IsEnabled));
            }
        }

        private void ExportChat(object sender, RoutedEventArgs e)
        {
            Drawer.IsRightDrawerOpen = false;
            RightDrawerContent.Children.Clear();
            ChatWindow chatWindow = new ChatWindow(ChatBox)
            {
                Title = "Chat",
                DataContext = DataContext,
                Owner = this,
            };
            ChatToggleButton.IsEnabled = false;
            chatWindow.Show();
        }
    }
}
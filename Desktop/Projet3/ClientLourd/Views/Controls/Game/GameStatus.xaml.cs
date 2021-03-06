using ClientLourd.Models.Bindable;
using ClientLourd.Services.SocketService;
using ClientLourd.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ClientLourd.Views.Controls.Game
{
    public partial class GameStatus : UserControl
    {
        public GameStatus()
        {
            InitializeComponent();
        }

        public void AfterLogin()
        {
            InitEventHandler();
        }

        public void AfterLogout()
        {
        }

        private void InitEventHandler()
        {
            SocketClient.RoundEnded += SocketClientOnRoundEnded;
            SocketClient.MatchSync += SocketClientOnMatchSync;
            SocketClient.MatchEnded += SocketClientOnMatchEnded;
        }

        private void SocketClientOnMatchEnded(object source, EventArgs args)
        {
            var e = (MatchEventArgs) args;
            UpdatePlayersScore(e.Players);
        }

        private void SocketClientOnMatchSync(object source, EventArgs args)
        {
            var e = (MatchEventArgs) args;
            UpdatePlayersScore(e.Players);
        }

        private void SocketClientOnRoundEnded(object source, EventArgs args)
        {
            var e = (MatchEventArgs) args;
            UpdatePlayersScore(e.Players);
        }

        public SocketClient SocketClient
        {
            get
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    return (((MainWindow) Application.Current.MainWindow)?.DataContext as MainViewModel)
                        ?.SocketClient;
                });
            }
        }


        public GameViewModel GameViewModel
        {
            get => Application.Current.Dispatcher.Invoke(() => { return (GameViewModel) DataContext; });
        }

        private void UpdatePlayersScore(dynamic playersInfo)
        {
            foreach (dynamic info in playersInfo)
            {
                var dic = (Dictionary<object, object>) info;
                if (!dic.ContainsKey("Points") || !dic.ContainsKey("UserID"))
                    break;
                var tmpPlayer = GameViewModel.Players.FirstOrDefault(p => p.User.ID == info["UserID"]);
                var newPoints = info["Points"] - tmpPlayer.Score;
                if (tmpPlayer != null && newPoints != 0)
                {
                    tmpPlayer.Score = info["Points"];
                    tmpPlayer.PointsRecentlyGained = newPoints;
                    GameViewModel.OrderPlayers();
                    Task.Delay(100).ContinueWith((t) =>
                    {
                        AnimatePoints(tmpPlayer.User.ID, (tmpPlayer.PointsRecentlyGained > 0));
                    });
                }
            }
        }


        private void AnimatePoints(string playerID, bool pointsGained)
        {
            for (int i = 0; i < List.Items.Count; i++)
            {
                if ((List.Items[i] as Player).User.ID == playerID)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ContentPresenter c = (ContentPresenter) List.ItemContainerGenerator.ContainerFromIndex(i);

                        TextBlock tb = (pointsGained)
                            ? (c.ContentTemplate.FindName("PointsGainedTextBlock", c) as TextBlock)
                            : (c.ContentTemplate.FindName("PointsLostTextBlock", c) as TextBlock);

                        Storyboard sb = (Storyboard) FindResource("PointsAnimations");
                        for (int j = 0; j < sb.Children.Count; j++)
                        {
                            Storyboard.SetTarget(sb.Children[j], tb);
                        }

                        sb.Begin();
                    });
                }
            }
        }
    }
}
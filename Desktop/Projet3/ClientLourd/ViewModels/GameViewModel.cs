﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClientLourd.Models.Bindable;
using ClientLourd.Models.NonBindable;
using ClientLourd.Services.InkCanvas;
using ClientLourd.Services.ProfileViewerService;
using ClientLourd.Services.RestService;
using ClientLourd.Services.ServerStrokeDrawerService;
using ClientLourd.Services.SocketService;
using ClientLourd.Services.SoundService;
using ClientLourd.Utilities.Commands;
using ClientLourd.Utilities.Enums;
using ClientLourd.Utilities.Extensions;
using ClientLourd.Views.Controls;
using ClientLourd.Views.Dialogs;
using MaterialDesignThemes.Wpf;
using Lobby = ClientLourd.Models.Bindable.Lobby;
using Timer = System.Timers.Timer;

namespace ClientLourd.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private string _drawingID;
        private string _word;
        private char[] _guess;
        private DateTime _time;
        private int _healthPoint;
        private ObservableCollection<Player> _players;
        private long _round;
        private long _totalRound;
        private StrokesReader _stokesReader;
        private string _canvasMessage;
        private GameModes _mode;
        private bool _roundStarted;
        private int _nHeartsTotal;

        public ServerStrokeDrawerService StrokeDrawerService { get; set; }

        public GameViewModel()
        {
            Players = new ObservableCollection<Player>();
        }

        public ResourceDictionary CurrentDictionary
        {
            get => (((MainWindow)Application.Current.MainWindow)?.DataContext as MainViewModel)?.CurrentDictionary;
        }

        private void InitEventHandler()
        {
            SocketClient.MatchStarted += SocketClientOnMatchStarted;
            SocketClient.MatchEnded += SocketClientOnMatchEnded;
            SocketClient.MatchTimesUp += SocketClientOnMatchTimesUp;
            SocketClient.AreYouReady += SocketClientOnAreYouReady;
            SocketClient.GuessResponse += SocketClientOnGuessResponse;
            SocketClient.PlayerGuessedTheWord += SocketClientOnPlayerGuessedTheWord;
            SocketClient.MatchSync += SocketClientOnMatchSync;
            SocketClient.YourTurnToDraw += SocketClientOnYourTurnToDraw;
            SocketClient.NewPlayerIsDrawing += SocketClientOnNewPlayerIsDrawing;
            SocketClient.PlayerLeftMatch += SocketClientOnPlayerLeftMatch;
            SocketClient.ServerStrokeSent += SocketClientOnServerStrokeSent;
            SocketClient.UserDeleteStroke += SocketClientOnUserDeleteStroke;
            SocketClient.CoopWordGuessed += SocketClientCoopWordGuessed;
        }

        


        public SoundService SoundService
        {
            get { return (((MainWindow)Application.Current.MainWindow)?.DataContext as MainViewModel)?.SoundService; }
        }

        private DateTime _timeGained;
        public DateTime TimeGained
        {
            get { return _timeGained; }
            set
            {
                if (_timeGained != value)
                {
                    _timeGained = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void SocketClientOnUserDeleteStroke(object source, EventArgs args)
        {
            Application.Current.Dispatcher.Invoke(delegate 
            {
                var e = (DrawingEventArgs) args;
                

                byte[] idTodelete = e.Data.Clone();
                

                if(BitConverter.IsLittleEndian)
                    Array.Reverse(idTodelete);

                Editor.Canvas.RemoveStroke(new Guid(idTodelete));
            });
        }

        private void SocketClientOnServerStrokeSent(object source, EventArgs args)
        {
            if (_roundStarted)
            {
                StrokeDrawerService?.Enqueue((args as StrokeSentEventArgs).StrokeInfo);
            }
        }

        private void SocketClientOnPlayerLeftMatch(object source, EventArgs args)
        {
            var e = (MatchEventArgs) args;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Players.Remove(Players.FirstOrDefault(p => p.User.ID == e.UserID));
                if (e.UserID == SessionInformations.User.ID)
                {
                    ResetView();
                }
            });
        }

        private void SocketClientOnNewPlayerIsDrawing(object source, EventArgs args)
        {
            var e = (MatchEventArgs) args;
            

            

            if (Mode == GameModes.FFA)
            {
                Time = e.Time;
            }
            
            _roundStarted = true;
            CanStillGuess = true;
            //TODO: if solo or coop
            

            Players.ToList().ForEach(p => p.IsDrawing = false);
            Players.ToList().ForEach(p => p.GuessedTheWord = false);
            Player player = Players.FirstOrDefault(p => p.User.ID == e.UserID);
            player.IsDrawing = true;

           StrokeDrawerService.ChangeMode(player.User.IsCPU);

            NotifyPropertyChanged(nameof(DrawerIsCPU));
            if (SessionInformations.User.ID != e.UserID && Mode == GameModes.FFA)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnNewCanavasMessage($"{e.Username} {CurrentDictionary["IsDrawingNext"]}");
                });
            }
            Guess = new char[e.WordLength];
        }

        private void SocketClientOnYourTurnToDraw(object source, EventArgs args)
        {
            
            var e = (MatchEventArgs) args;
            //Enable the canvas
            Word = e.Word; 
            _drawingID = e.DrawingID;
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnNewCanavasMessage($"{CurrentDictionary["YourTurnDraw"]} {e.Word}");
            });
            ChangeCanvasStatus(true);
        }

        private void SocketClientOnMatchSync(object source, EventArgs args)
        {
            var e = (MatchEventArgs) args;
            Round = e.Laps;
            TotalRound = e.LapTotal;
            Time = e.Time;
        }

        private void SocketClientOnPlayerGuessedTheWord(object source, EventArgs args)
        {
            var e = (MatchEventArgs) args;
            var player = Players.FirstOrDefault(p => p.User.ID == e.UserID);
            player.GuessedTheWord = true;
        }

        private void SocketClientOnGuessResponse(object source, EventArgs args)
        {
            var e = (MatchEventArgs) args;
            if (e.Valid)
            {
                Player player = Players.First(p => p.User.ID == SessionInformations.User.ID);
                Application.Current.Dispatcher.Invoke(() => 
                {
                    CanStillGuess = false;
                    SoundService.PlayWordGuessedRight();
                });
            }
            else
            {
                if (Mode == GameModes.FFA || HealthPoint > 1)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CanStillGuess = false;
                        OnNewCanavasMessage($"{CurrentDictionary["TryAgain"]}");
                        SoundService.PlayWordGuessedWrong();
                    });
                    Task.Delay(2000).ContinueWith((t) =>
                    {
                        if (_roundStarted)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                CanStillGuess = true;
                                //Refresh command
                                CommandManager.InvalidateRequerySuggested();
                            });
                        }

                    });
                }
            }
        }

        public int HeartsTotal
        {
            get => _nHeartsTotal;
            set
            {
                _nHeartsTotal = value;
                NotifyPropertyChanged();
            }
        }

        private void SocketClientOnAreYouReady(object source, EventArgs args)
        {
            var e = (MatchEventArgs)args;
            HealthPoint = e.Lives;
            HeartsTotal = e.Lives;
            PrepareMatch();
        }


        private void SocketClientOnMatchTimesUp(object source, EventArgs args)
        {
            PrepareNextRound();
            var e = (MatchEventArgs) args;
            if (Mode == GameModes.FFA)
            {
                Time = DateTime.MinValue;

            }
            CanStillGuess = false;
        }

        private void SocketClientOnMatchEnded(object source, EventArgs args)
        {
            var e = (MatchEventArgs) args;
            //TODO show leaderboard
            StrokeDrawerService.Stop();
        }

        private void SocketClientOnMatchStarted(object source, EventArgs args)
        {
            StrokeDrawerService.Start();
        }
        
        public Lobby Lobby
        {
            get
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    return (((MainWindow) Application.Current.MainWindow)?.DataContext as MainViewModel)
                        ?.CurrentLobby;
                });
            }
        }
        public SessionInformations SessionInformations
        {
            get
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    return (((MainWindow) Application.Current.MainWindow)?.DataContext as MainViewModel)
                        ?.SessionInformations;
                });
            }
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

        public void OrderPlayers()
        {
            Players = new ObservableCollection<Player>(_players.OrderByDescending(p => p.Score));
        }

        public ObservableCollection<Player> Players
        {
            get => _players;
            
            set
            {
                _players = value;
                NotifyPropertyChanged();
            }
        }
        
        public char[] Guess
        {
            get => _guess;
            set
            {
                _guess = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsYourTurn
        {
            get => !string.IsNullOrEmpty(Word);
        }

        private bool _canStillGuess;
        public bool CanStillGuess
        {
            get => _canStillGuess;
            set
            {
                _canStillGuess = value;
                NotifyPropertyChanged();
            }
        }

        public string Word
        {
            get => _word;
            set
            {
                _word = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsYourTurn));
            }
        }
        
        public long TotalRound
        {
            get => _totalRound;
            set
            {
                _totalRound = value;
                NotifyPropertyChanged();
            }
        }
        public long Round
        {
            get => _round;
            set
            {
                _round = value;
                NotifyPropertyChanged();
            }
        }
        public DateTime Time
        {
            get => _time;
            set
            {
                _time = value;
                NotifyPropertyChanged();
            }
        }

        public int HealthPoint
        {
            get => _healthPoint;
            set
            {
                _healthPoint = value;
                NotifyPropertyChanged();
            }
        }

        public GameModes Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                NotifyPropertyChanged();
            }
        }



        public bool DrawerIsCPU
        {
            get
            {
                var player = Players.FirstOrDefault(p => p.IsDrawing);
                if(player != null)
                {
                    return player.User.IsCPU;
                }
                return false;
            } 
            
        }

        public Editor Editor { get; set; }
        
        RelayCommand<object> _askHintCommand;

        public ICommand AskHintCommand
        {
            get
            {
                return _askHintCommand ??
                       (_askHintCommand = new RelayCommand<object>(p => SocketClient.SendMessage(new Tlv(SocketMessageTypes.AskForHint)), (p) => CanStillGuess));
            }
        }
        
        
        
        RelayCommand<object> _sendGuessCommand;

        public ICommand SendGuessCommand
        {
            get
            {
                return _sendGuessCommand ??
                       (_sendGuessCommand = new RelayCommand<object>(channel => SendGuess()));
            }
        }
        
        
        
        private void PrepareNextRound()
        {
            ChangeCanvasStatus(false);
            _roundStarted = false;
            Word = "";
        }
        private void SendGuess()
        {
            SocketClient.SendMessage(new Tlv(SocketMessageTypes.GuessTheWord, new string(Guess)));
        }

        private void ClearCanvas()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Editor.SelectPen();
                Editor.Canvas.Strokes.Clear();
            });
        }

        private void ChangeCanvasStatus(bool isEnable)
        {
            Application.Current.Dispatcher.Invoke(() => { Editor.IsEnabled = isEnable; });
            if (isEnable)
            {
                _stokesReader.Start(_drawingID);
            }
            else
            {
                _stokesReader.Stop();
                
            }

            Task.Run(() =>
            {
                Thread.Sleep(100);
                ClearCanvas();
            });
        }

        private void PrepareMatch()
        {
            Word = "";
            Mode = Lobby.Mode;

            Application.Current.Dispatcher.Invoke(() =>
            {
                ClearCanvas();
                Players = Lobby.Players;
                _stokesReader = new StrokesReader(Editor, SocketClient, ((EditorViewModel)Editor.DataContext).EditorInformation);
                ChangeCanvasStatus(false);
                SocketClient.SendMessage(new Tlv(SocketMessageTypes.ReadyToStart));
            });
        }


        public delegate void CanvasMessageHandler(string message);

        public event CanvasMessageHandler NewCanavasMessage;
        
        


        private bool _guessButtonIsDefault;
        public bool GuessButtonIsDefault
        {
            get => _guessButtonIsDefault;
            set
            {
                _guessButtonIsDefault = value;
                NotifyPropertyChanged();
            }
        }


        public ICommand ViewPublicProfile
        {
            get { return ProfileViewer.ViewPublicProfileCommand; }
        }

        protected virtual void OnNewCanavasMessage(string message)
        {
            NewCanavasMessage?.Invoke(message);
        }

        int _teamPoints;
        public int TeamPoints
        {
            get => _teamPoints;
            set
            {
                if (_teamPoints != value)
                {
                    _teamPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        int _teamNewPoints;
        public int TeamNewPoints
        {
            get => _teamNewPoints;
            set
            {
                if (_teamNewPoints != value)
                {
                    _teamNewPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void SocketClientCoopWordGuessed(object source, EventArgs args)
        {

            MatchEventArgs e = (MatchEventArgs)args;
            Application.Current.Dispatcher.Invoke(() => 
            {
                if (e.UserID == SessionInformations.User.ID)
                {
                    OnNewCanavasMessage((string)CurrentDictionary["RightGuessSolo"]);
                }
                else
                {
                    OnNewCanavasMessage($"{e.Username} {CurrentDictionary["RightGuessCoop"]} {e.Word}.");
                }

            });
        }

        public override void AfterLogOut()
        {
        }

        public override void AfterLogin()
        {
            _players = new ObservableCollection<Player>();
            ClearCanvas();
            InitEventHandler();
        }

        private void ResetView()
        {
            StrokeDrawerService.Close();
            ClearCanvas();
            Players.Clear();
            Time = DateTime.MinValue;
        }
    }
}
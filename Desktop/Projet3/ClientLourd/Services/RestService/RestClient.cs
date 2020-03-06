﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Documents;
using ClientLourd.Models.Bindable;
using ClientLourd.Models.NonBindable;
using ClientLourd.Services.EnumService;
using ClientLourd.Services.RestService.Exceptions;
using ClientLourd.Utilities.Constants;
using ClientLourd.Utilities.Enums;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization.Json;
using DataFormat = RestSharp.DataFormat;

namespace ClientLourd.Services.RestService
{
    public class RestClient
    {
        private RestSharp.RestClient _client;
        private string _sessionToken;
        private NetworkInformations _networkInformations;

        public RestClient(NetworkInformations informations)
        {
            _networkInformations = informations;
            _client = new RestSharp.RestClient()
            {
                Timeout = 10000,
            };
        }

        /// <summary>
        /// Try to login using the username and password specified
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="RestConflictException"></exception>
        /// <exception cref="RestBadRequestException"></exception>
        /// <exception cref="RestException"></exception>
        public async Task<Dictionary<string, object>> Login(string username, string password)
        {
            _client.BaseUrl = new Uri($"http://{_networkInformations.IP}:{_networkInformations.RestPort}");
            RestRequest request = new RestRequest("auth", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new {username = username});
            var response = await Execute(request);
            var deseralizer = new JsonDeserializer();
            dynamic data = deseralizer.Deserialize<dynamic>(response);
            _sessionToken = data["SessionToken"];
            return data;
        }
        
        public async Task<Dictionary<string, object>> Bearer(string username, string bearer)
        {
            _client.BaseUrl = new Uri($"http://{_networkInformations.IP}:{_networkInformations.RestPort}");
            RestRequest request = new RestRequest("auth/bearer", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new {username = username, Bearer=bearer});
            var response = await Execute(request);
            var deseralizer = new JsonDeserializer();
            dynamic data = deseralizer.Deserialize<dynamic>(response);
            _sessionToken = data["SessionToken"];
            return data;
        }

        public async Task<Dictionary<string, object>> Register(User user, string password)
        {
            _client.BaseUrl = new Uri($"http://{_networkInformations.IP}:{_networkInformations.RestPort}");
            RestRequest request = new RestRequest("auth/register", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new {Username = user.Username, FirstName=user.FirstName, LastName= user.LastName, Email=user.Email, Password = password, PictureID=user.PictureID});
            var response = await Execute(request);
            var deseralizer = new JsonDeserializer();
            dynamic data = deseralizer.Deserialize<dynamic>(response);
            _sessionToken = data["SessionToken"];
            return data;
        }

        public async Task<string> PutProfile(object obj)
        {
            RestRequest request = new RestRequest("users", Method.PUT);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            request.AddJsonBody(obj);
            var response = await Execute(request);
            var deseralizer = new JsonDeserializer();
            dynamic data = deseralizer.Deserialize<dynamic>(response);
            return data;
        }

        
        public async Task<List<Channel>> GetChannels()
        {
            RestRequest request = new RestRequest("chat/channels", Method.GET);
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            var response = await Execute(request);
            var channels = JsonConvert.DeserializeObject<List<Channel>>(response.Content);
            channels.ForEach(c => c.Messages = new ObservableCollection<Message>());
            return channels;
        }

        public async Task<Channel> GetChannel(string channelId)
        {
            RestRequest request = new RestRequest("chat/channels", Method.GET);
            //TODO mettre dans le bon format
            request.AddParameter("channelID", channelId);
            var response = await Execute(request);
            return JsonConvert.DeserializeObject<Channel>(response.Content);
        }

        public async Task<List<Message>> GetChannelMessages(string channelID, int start, int end)
        {
            RestRequest request = new RestRequest($"chat/messages/{channelID}");
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            request.AddParameter("start", start, ParameterType.QueryString);
            request.AddParameter("end", end, ParameterType.QueryString);
            var response = await Execute(request);
            var deseralizer = new JsonDeserializer();
            List<Message> messages = new List<Message>();
            var objects = deseralizer.Deserialize<List<dynamic>>(response);
            var messagesInformations = ((Dictionary<string, object>)objects[0])["Messages"];
            foreach (var message in (List<dynamic>)messagesInformations)
            {
                User user = new User(message["Username"], message["UserID"]);
                messages.Add(new Message((int)message["Timestamp"], user, message["Message"]));
            }
            return messages;
        }

        public async Task<Stats> GetStats()
        {
            RestRequest request = new RestRequest("stats");
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            var response = await Execute(request);
            return JsonConvert.DeserializeObject<Stats>(response.Content); 
        }


        public async Task<StatsHistory> GetStats(int start, int end)
        {
            RestRequest request = new RestRequest("stats//history");
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            request.AddParameter("start", start, ParameterType.QueryString);
            request.AddParameter("end", end, ParameterType.QueryString);
            var response = await Execute(request);
            return JsonConvert.DeserializeObject<StatsHistory>(response.Content);
        }



        public async Task<User> GetUserInfo(string userID)
        {
            RestRequest request = new RestRequest($"users/{userID}", Method.GET);
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            var response = await Execute(request);
            return JsonConvert.DeserializeObject<User>(response.Content);
        }

        public async Task<string> PostGameInformations(string word, string[] hints, DifficultyLevel difficultyLevel)
        {
            
            RestRequest request = new RestRequest($"games", Method.POST);
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            request.AddJsonBody(new {Hints =hints, Word=word, Difficulty=(int)difficultyLevel});
            var response = await Execute(request);
            var deseralizer = new JsonDeserializer();
            return deseralizer.Deserialize<dynamic>(response)["GameID"];
        }

        public async Task PutGameInformations(string gameID, PotraceMode mode,double blackLevel, int brushSize)
        {
            RestRequest request = new RestRequest( $"games/{gameID}/image", Method.PUT);
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            request.AddJsonBody(new
            {
                Mode=(int)mode, 
                BlackLevel=blackLevel, 
                BrushSize=brushSize,
            });
            var response = await Execute(request);

        }

        public async Task PostGameImage(string gameID, string image, PotraceMode mode, double blackLevel, int brushSize)
        {
            RestRequest request = new RestRequest($"games/{gameID}/image", Method.POST);
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            request.AddFile("file", image, "");
            request.AddParameter("blacklevel", blackLevel, ParameterType.GetOrPost);
            request.AddParameter("brushsize", brushSize, ParameterType.GetOrPost);
            request.AddParameter("mode", (int) mode, ParameterType.GetOrPost);
            var response = await Execute(request);
        }

        public async Task<string> PostGroup(string groupName, int playersMax, int virtualPlayers, GameModes gameType, DifficultyLevel difficulty)
        {
            RestRequest request = new RestRequest($"/groups", Method.POST);
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            request.AddJsonBody(new
            {
                GroupName = groupName,
                PlayersMax = playersMax,
                VirtualPlayers = virtualPlayers,
                GameType = (int)gameType,
                Difficulty = (int)difficulty

            });
            var response = await Execute(request);
            var deseralizer = new JsonDeserializer();

            return deseralizer.Deserialize<dynamic>(response)["GroupID"];
        }

        public async Task<ObservableCollection<Lobby>> GetGroup()
        {
            RestRequest request = new RestRequest($"/groups", Method.GET);
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            var response = await Execute(request);
            var deseralizer = new JsonDeserializer();
            List<dynamic> tmpResponse = deseralizer.Deserialize<List<dynamic>>(response);
            ObservableCollection<Lobby> groups = JsonConvert.DeserializeObject<ObservableCollection<Lobby>>(response.Content);
            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].Mode = (GameModes)tmpResponse[i]["GameType"];
                groups[i].GameName= tmpResponse[i]["GroupName"];
                groups[i].Host = tmpResponse[i]["OwnerName"];
                groups[i].HostID = tmpResponse[i]["OwnerID"];
                groups[i].PlayersCount = groups[i].Players.Count;
            }
            return groups;
        }

        /*public async Task<object> GetGroup(string groupID)
        {
            RestRequest request = new RestRequest($"/groups/{groupID}", Method.GET);
            request.AddParameter("SessionToken", _sessionToken, ParameterType.HttpHeader);
            var response = await Execute(request);
            
            var deseralizer = new JsonDeserializer();
            return deseralizer.Deserialize<dynamic>(response);
        }*/


        private Task<IRestResponse> Execute(RestRequest request)
        {
            Task<IRestResponse> task = new Task<IRestResponse>(() =>
            {
                OnStartWaiting(this);
                var response = _client.Execute(request);
                OnStopWaiting(this);
                var deseralizer = new JsonDeserializer();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return response;
                    case HttpStatusCode.BadRequest:
                        throw new RestBadRequestException(deseralizer.Deserialize<dynamic>(response)["Error"]);
                    case HttpStatusCode.Conflict:
                        
                        throw new RestConflictException(deseralizer.Deserialize<dynamic>(response)["Error"]);
                    case HttpStatusCode.Unauthorized:
                        throw new RestUnauthorizedException(deseralizer.Deserialize<dynamic>(response)["Error"]);
                    case HttpStatusCode.NotFound:
                        throw new RestNotFoundException(deseralizer.Deserialize<dynamic>(response)["Error"]);
                    default:
                        throw new RestException(response.ErrorMessage);
                }
            });
            task.Start();
            return task;
        }

        public delegate void RestEventHandler(object source, EventArgs args);

        public event RestEventHandler StartWaiting;
        public event RestEventHandler StopWaiting;

        protected virtual void OnStartWaiting(object source)
        {
            StartWaiting?.Invoke(source, EventArgs.Empty);
        }

        protected virtual void OnStopWaiting(object source)
        {
            StopWaiting?.Invoke(source, EventArgs.Empty);
        }
    }
}
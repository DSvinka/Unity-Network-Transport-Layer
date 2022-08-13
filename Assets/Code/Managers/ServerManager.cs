using System;
using System.Collections.Generic;
using System.Text;
using Code.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace Code.Managers
{
    public class ServerManager : MonoBehaviour
    {
        private const int MAX_CONNECTIONS_COUNT = 10;

        private int _port = 5805;
        private int _hostID;
        private int _reliableChannel;

        private bool _isStarted = false;
        private byte _error;

        private List<int> _connectionIDs;
        private Dictionary<int, UserModel> _connectionUsers;


        #region Unity Callbacks

        private void Start()
        {
            _connectionIDs = new List<int>(MAX_CONNECTIONS_COUNT);
            _connectionUsers = new Dictionary<int, UserModel>(MAX_CONNECTIONS_COUNT);
        }

        private void OnDestroy()
        {
            _connectionIDs.Clear();
            _connectionUsers.Clear();
            
            if (_isStarted)
            {
                StopServer();
            }
        }

        private void Update()
        {
            if (!_isStarted) return;

            int recHostId;
            int connectionId;
            int channelId;
            
            int dataSize;
            var bufferSize = 1024;
            var recBuffer = new byte[bufferSize];

            var recData = NetworkTransport.Receive(
                out recHostId, out connectionId, out channelId,
                recBuffer, bufferSize, out dataSize,
                out _error
            );

            while (recData != NetworkEventType.Nothing)
            {
                switch (recData)
                {
                    case NetworkEventType.Nothing:
                        break;
                    case NetworkEventType.ConnectEvent:
                        _connectionIDs.Add(connectionId);

                        SendMessageToAll($"[{connectionId}] Player has connected.");
                        Debug.Log($"[{connectionId}] Player has connected.");
                        break;
                    case NetworkEventType.DataEvent:
                        var message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                        if (!_connectionUsers.ContainsKey(connectionId))
                        {
                            _connectionUsers.Add(connectionId, new UserModel()
                            {
                                Id = connectionId, Nickname = message
                            });
                            
                            SendMessageToAll($"[{connectionId}] Player change nickname to {message}.");
                            Debug.Log($"[{connectionId}] Player change nickname to {message}.");
                        }
                        else
                        {
                            var userModel = _connectionUsers[connectionId];
                            SendMessageToAll($"{userModel.Nickname}: {message}");
                            Debug.Log($"{userModel.Nickname}: {message}");    
                        }
                        
                        break;
                    case NetworkEventType.DisconnectEvent:
                        _connectionIDs.Remove(connectionId);
                        
                        var leaveUserModel = _connectionUsers[connectionId];
                        SendMessageToAll($"[{leaveUserModel.Id}] Player {leaveUserModel.Nickname} has disconnected");
                        Debug.Log($"[{leaveUserModel.Id}] Player {leaveUserModel.Nickname} has disconnected");
                        
                        _connectionUsers.Remove(connectionId);
                        break;
                    case NetworkEventType.BroadcastEvent:
                        break;
                }

                recData = NetworkTransport.Receive(
                    out recHostId, out connectionId, out channelId, 
                    recBuffer, bufferSize, out dataSize, 
                    out _error
                );
            }
        }

        #endregion

        
        public void StartServer()
        {
            if (_isStarted) return;
            
            NetworkTransport.Init();
            var connectionConfig = new ConnectionConfig();
            
            _reliableChannel = connectionConfig.AddChannel(QosType.Reliable);

            var hostTopology = new HostTopology(connectionConfig, MAX_CONNECTIONS_COUNT);
            _hostID = NetworkTransport.AddHost(hostTopology, _port);
            _isStarted = true;
        }

        public void StopServer()
        {
            if (!_isStarted) return;

            NetworkTransport.RemoveHost(_hostID);
            NetworkTransport.Shutdown();

            _isStarted = false;
        }

        public void SendMessageToAll(string message)
        {
            for (var i = 0; i < _connectionIDs.Count; i++)
            {
                SendMessage(message, _connectionIDs[i]);
            }
        }

        public void SendMessage(string message, int connectionID)
        {
            var buffer = Encoding.Unicode.GetBytes(message);
            NetworkTransport.Send(
                _hostID, connectionID, _reliableChannel, 
                buffer, message.Length * sizeof(char),
                out _error
            );
            
            var networkError = (NetworkError) _error;
            if (networkError != NetworkError.Ok)
                Debug.LogError(networkError);
        }
    }
}

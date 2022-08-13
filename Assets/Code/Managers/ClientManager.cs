using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Code.Managers
{
    public class ClientManager: MonoBehaviour
    {
        public event Action<object> OnMessageReceive;
        
        private const int MAX_CONNECTIONS_COUNT = 10;

        private int _port = 0;
        private int _serverPort = 5805;
        private int _hostID;
        private int _reliableChannel;

        private string _nickname;
        private int _connectionID;
        private bool _isConnected = false;
        private byte _error;

        #region Unity Callbacks

        private void Update()
        {
            if (!_isConnected) return;
            
            int recHostId;
            int connectionId;
            int channelId;
            
            int dataSize;
            var bufferSize = 1024;
            var recBuffer = new byte[bufferSize];

            var recData = NetworkTransport.ReceiveFromHost(
                _hostID, out connectionId, out channelId,
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
                        OnMessageReceive?.Invoke("You have been connected to server.");
                        SendMessageToHost(_nickname);
                        Debug.Log("You have been connected to server.");
                        break;
                    case NetworkEventType.DataEvent:
                        var message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                        OnMessageReceive?.Invoke(message);
                        Debug.Log(message);
                        break;
                    case NetworkEventType.DisconnectEvent:
                        _isConnected = false;
                        OnMessageReceive?.Invoke("You have been disconnected from server.");
                        Debug.Log($"You have been disconnected from server.");
                        break;
                    case NetworkEventType.BroadcastEvent:
                        break;
                }

                recData = NetworkTransport.ReceiveFromHost(
                    _hostID, out connectionId, out channelId, 
                    recBuffer, bufferSize, out dataSize, 
                    out _error
                );
            }
        }

        #endregion

        public void Connect(string nickname)
        {
            NetworkTransport.Init();
            var connectionConfig = new ConnectionConfig();
            _reliableChannel = connectionConfig.AddChannel(QosType.Reliable);
            
            var hostTopology = new HostTopology(connectionConfig, MAX_CONNECTIONS_COUNT);
            
            _hostID = NetworkTransport.AddHost(hostTopology, _port);
            _connectionID = NetworkTransport.Connect(_hostID, "127.0.0.1", _serverPort, 0, out _error);

            var networkError = (NetworkError) _error;
            if (networkError == NetworkError.Ok)
            {
                _nickname = nickname;
                _isConnected = true;
            }
            else
            {
                Debug.LogError(networkError);
            }
        }

        public void Disconnect()
        {
            if (!_isConnected) return;

            NetworkTransport.Disconnect(_hostID, _connectionID, out _error);
            _isConnected = false;
        }
        
        public void SendMessageToHost(string message)
        {
            var buffer = Encoding.Unicode.GetBytes(message);
            NetworkTransport.Send(
                _hostID, _connectionID, _reliableChannel, 
                buffer, message.Length * sizeof(char),
                out _error
            );
            
            var networkError = (NetworkError) _error;
            if (networkError != NetworkError.Ok)
                Debug.LogError(networkError);
        }
    }
}
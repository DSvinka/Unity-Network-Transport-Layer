using System;
using System.Collections;
using System.Collections.Generic;
using Code.Managers;
using Code.Views;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Code.Controllers
{
    public class ChatController : MonoBehaviour
    {
        [Header("Server Buttons")]
        [SerializeField] private Button _startServerButton;
        [SerializeField] private Button _shutDownServerButton;
        
        [Header("Client Buttons")]
        [SerializeField] private Button _connectClientButton;
        [SerializeField] private Button _disconnectClientButton;
        [SerializeField] private Button _sendMessageButton;
        
        [Header("Inputs")]
        [SerializeField] private TMP_InputField _nicknameInput;
        [SerializeField] private TMP_InputField _messageInput;
        
        [Header("Views")]
        [SerializeField] private ChatView _chatView;
        
        [Header("Managers")]
        [SerializeField] private ServerManager _serverManager;
        [SerializeField] private ClientManager _clientManager;

        #region Unity Callbacks

        private void Start()
        {
            _startServerButton.onClick.AddListener(() => StartServer());
            _shutDownServerButton.onClick.AddListener(() => ShutDownServer());
            
            _connectClientButton.onClick.AddListener(() => Connect());
            _disconnectClientButton.onClick.AddListener(() => Disconnect());
            _sendMessageButton.onClick.AddListener(() => SendMessage());
            
            _clientManager.OnMessageReceive += ReceiveMessage;
        }

        private void OnDestroy()
        {
            _clientManager.OnMessageReceive -= ReceiveMessage;
        }

        #endregion

        #region Server

        private void StartServer()
        {
            _serverManager.StartServer();
        }
        
        private void ShutDownServer()
        {
            _serverManager.StopServer();
        }

        #endregion

        #region Client

        private void Connect()
        {
            if (_nicknameInput.text.Length <= 0)
            {
                Debug.LogError("Nickname is required!");
                return;
            }
            
            _clientManager.Connect(_nicknameInput.text);
            _nicknameInput.interactable = false;
        }
        
        private void Disconnect()
        {
            _clientManager.Disconnect();
            _nicknameInput.interactable = true;
        }
        
        private void SendMessage()
        {
            _clientManager.SendMessageToHost(_messageInput.text);
            _messageInput.text = "";
        }
        
        private void ReceiveMessage(object message)
        {
            _chatView.ReceiveMessage(message);
        }

        #endregion
    }
}
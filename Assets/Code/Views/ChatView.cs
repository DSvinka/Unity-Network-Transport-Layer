using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Views
{
    public class ChatView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textObject;
        [SerializeField] private Scrollbar _scrollbar;

        private List<string> _messages = new List<string>();

        private void Start()
        {
            _scrollbar.onValueChanged.AddListener((float value) => UpdateText());
        }

        public void ReceiveMessage(object message)
        {
            _messages.Add(message.ToString());
            
            var value = (_messages.Count - 1) * _scrollbar.value;
            _scrollbar.value = Mathf.Clamp(value, 0, 1);

            UpdateText();
        }

        private void UpdateText()
        {
            var text = "";
            var index = (int)(_messages.Count * _scrollbar.value);
            
            foreach (var message in _messages)
            {
                text += message + "\n";
            }

            _textObject.text = text;
        }
    }
}

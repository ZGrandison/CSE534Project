using UnityEngine;
using UnityEngine.UI;

namespace Riptide.Demos.DedicatedClient
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _singleton;
        public static UIManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(UIManager)} instance already exists, destroying object!");
                    Destroy(value);
                }
            }
        }

        [SerializeField] private InputField usernameField;
        [SerializeField] private GameObject connectScreen;
        [SerializeField] private GameObject inGameScreen;

        private void Awake()
        {
            Singleton = this;
        }

        public void ConnectClicked()
        {
            usernameField.interactable = false;
            connectScreen.SetActive(false);
            inGameScreen.SetActive(true);

            NetworkManager.Singleton.Connect();
        }

        public void BackToMain()
        {
            usernameField.interactable = true;
            connectScreen.SetActive(true);
            inGameScreen.SetActive(false);
        }

        public void updateHealthText(int health)
        {
            Text healthDisplay = inGameScreen.GetComponentInChildren<Text>();
            healthDisplay.text = "Current Health: " + health;
        }

        #region Messages
        public void SendName()
        {
            Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.PlayerName);
            message.AddString(usernameField.text);
            NetworkManager.Singleton.Client.Send(message);
        }
        #endregion
    }
}

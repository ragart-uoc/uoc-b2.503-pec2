using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Mirror;
using TMPro;
using PEC2.Entities;

namespace PEC2.Managers
{
    /// <summary>
    /// Class <c>UIManager</c> controls the UI of the game.
    /// </summary>
    public class UIManager : NetworkBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static UIManager _instance;

        /// <value>Property <c>menu</c> is a reference to the in-game menu.</value>
        public GameObject menu;
        
        /// <value>Property <c>serverMenu</c> is a reference to the server menu.</value>
        public GameObject serverMenu;

        /// <value>Property <c>messageText</c> is a reference to the overlay Text to display winning text, etc.</value>
        public TextMeshProUGUI messageText;

        /// <value>Property <c>eventContainer</c> is a reference to the event container.</value>
        public GameObject eventContainer;
        
        /// <value>Property <c>eventPrefab</c> is a reference to the event prefab.</value>
        public GameObject eventPrefab;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        /// <summary>
        /// Method <c>Start</c> is called before the first frame update.
        /// </summary>
        private void Start()
        {
            messageText.text = String.Empty;
            if (isServerOnly)
                serverMenu.SetActive(true);
        }
        
        /// <summary>
        /// Method <c>OnMenu</c> is called when the menu button is pressed.
        /// </summary>
        private void OnMenu(InputValue inputValue)
        {
            if (!isClient)
                return;
            ToggleMenu();
        }

        /// <summary>
        /// Method <c>ToggleMenu</c> is used to show or hide the menu.
        /// </summary>
        public void ToggleMenu()
        {
            menu.SetActive(!menu.activeSelf);
        }

        /// <summary>
        /// Method <c>ChangePlayerName</c> is used to change the player name.
        /// </summary>
        /// <param name="newName">The new player name.</param>
        public void ChangePlayerName(string newName)
        {
            if (!NetworkClient.isConnected)
                return;
            NetworkClient.localPlayer.GetComponent<Tank>().SetName(newName);
        }
        
        /// <summary>
        /// Method <c>ChangePlayerColor</c> is used to change the player color.
        /// </summary>
        /// <param name="dropdown">The dropdown menu.</param>
        public void ChangePlayerColor(TMP_Dropdown dropdown)
        {
            if (!NetworkClient.isConnected)
                return;
            var color = dropdown.options[dropdown.value].text;
            if (ColorUtility.TryParseHtmlString(color, out var newColor))
                NetworkClient.localPlayer.GetComponent<Tank>().SetColor(newColor);
        }

        /// <summary>
        /// Method <c>GoToMainMenu</c> loads the main menu scene.
        /// </summary>
        public void GoToMainMenu()
        {
            if (isClient && NetworkClient.isConnected)
                NetworkManager.singleton.StopClient();
            if (isServer && NetworkServer.active)
                NetworkManager.singleton.StopServer();
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Method <c>QuitGame</c> quits the game.
        /// </summary>
        public void QuitGame()
        {
            Application.Quit();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        /// <summary>
        /// Method <c>WriteMessage</c> is used to write a message to the screen.
        /// </summary>
        /// <param name="message">The message to write to the screen</param>
        public void WriteMessage(string message)
        {
            if (!isServer)
                return;
            messageText.text = message;
            RpcWriteMessage(message);
        }
        
        /// <summary>
        /// Method <c>RpcWriteMessage</c> is used to write a message to the screen on the client.
        /// </summary>
        /// <param name="message">The message to write to the screen</param>
        [ClientRpc]
        private void RpcWriteMessage(string message)
        {
            messageText.text = message;
        }
        
        /// <summary>
        /// Method <b>BlinkingMessage</b> is used to display a message on the screen for a specified duration.
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="duration">The duration to display the message for</param>
        public IEnumerator BlinkingMessage(string message, float duration = 0)
        {
            var time = 0f;
            // If duration equals 0, then the message will blink forever
            while (duration == 0 || time < duration)
            {
                WriteMessage(message);
                yield return new WaitForSeconds(.5f);
                WriteMessage(String.Empty);
                yield return new WaitForSeconds(.5f);
                time += 1f;
            }
        }
        
        /// <summary>
        /// Method <c>SendEvent</c> is used to send an event to the event container.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="duration">The duration to display the message for</param>
        public void SendEvent(string message, float duration)
        {
            if (!isServer)
                return;
            if (isServerOnly)
                StartCoroutine(OnSendEvent(message, duration));
            RpcSendEvent(message, duration);
        }
        
        /// <summary>
        /// Method <c>RpcSendEvent</c> is used to send an event to the event container on the client.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="duration">The duration to display the message for</param>
        [ClientRpc]
        private void RpcSendEvent(string message, float duration)
        {
            StartCoroutine(OnSendEvent(message, duration));
        }

        /// <summary>
        /// Method <c>OnSendEvent</c> is used to send an event to the event container.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="duration">The duration to display the message for</param>
        private IEnumerator OnSendEvent(string message, float duration)
        {
            var newEvent = Instantiate(eventPrefab, eventContainer.transform);
            newEvent.GetComponent<TextMeshProUGUI>().text = message;
            yield return new WaitForSeconds(duration);
            Destroy(newEvent);
        }
    }
}

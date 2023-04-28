using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;
using PEC2.Entities;

namespace PEC2.Managers
{
    /// <summary>
    /// Class <c>MenuManager</c> is used to control the in-game menu.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static MenuManager _instance;

        /// <value>Property <c>menu</c> is a reference to the in-game menu.</value>
        public GameObject menu;

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
        /// Method <c>OnMenu</c> is called when the menu button is pressed.
        /// </summary>
        private void OnMenu(InputValue inputValue)
        {
            if (!NetworkClient.isConnected)
                return;
            ToggleMenu();
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
        /// Method <c>ToggleMenu</c> is used to show or hide the menu.
        /// </summary>
        public void ToggleMenu()
        {
            menu.SetActive(!menu.activeSelf);
        }

        /// <summary>
        /// Method <c>GoToMainMenu</c> loads the main menu scene.
        /// </summary>
        public void GoToMainMenu()
        {
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
    }
}

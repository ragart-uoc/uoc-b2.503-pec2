using System;
using UnityEngine;
using Mirror;
using TMPro;
using PEC2.Managers;

namespace PEC2.Entities
{
    /// <summary>
    /// Class <c>TankManager</c> is used to manage various settings on a tank. It works with the GameManager class to control how the tanks behave and whether or not players have control of their tank in the different phases of the game.
    /// </summary>
    [Serializable]
    public class Tank : NetworkBehaviour
    {
        /// <value>Property <c>playerName</c> represents the name of the player.</value>
        [SyncVar(hook = "OnChangeName")]
        public string playerName;
        
        /// <value>Property <c>playerNameText</c> represents the text component of the player name.</value>
        public TextMeshProUGUI playerNameText;
        
        /// <value>Property <c>playerColor</c> represents the color of the player tank.</value>
        [SyncVar(hook = "OnChangeColor")]
        public Color playerColor;

        /// <value>Property <c>coloredPlayerText</c> represents the player with their number colored to match their tank.</value>
        [HideInInspector]
        public string coloredPlayerText;

        /// <value>Property <c>wins</c> represents the number of wins this player has so far.</value>
        public int wins;

        /// <value>Property <c>m_Movement</c> represents the tank's movement script, used to disable and enable control.</value>
        private TankMovement m_Movement;

        /// <value>Property <c>m_Shooting</c> represents the tank's shooting script, used to disable and enable control.</value>
        private TankShooting m_Shooting;

        /// <value>Property <c>m_Health</c> represents the tank's health script, used to disable and enable control.</value>
        private TankHealth m_Health;

        /// <value>Property <c>m_CanvasGameObject</c> is used to disable the world space UI during the Starting and Ending phases of each round.</value>
        private GameObject m_CanvasGameObject;

        /// <value>Property <c>m_CameraManager</c> is used to add the tank to the group camera.</value>
        private CameraManager m_CameraManager;

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Get references to the components
            m_Movement = GetComponent<TankMovement>();
            m_Shooting = GetComponent<TankShooting>();
            m_Health = GetComponent<TankHealth>();
            m_CanvasGameObject = GetComponentInChildren<Canvas>().gameObject;
            m_CameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
            
            // Get player name from PlayerPrefs
            var newPlayerName = PlayerPrefs.GetString("PlayerName", "Player " + netId);
            SetName(newPlayerName);
                
            // Get player color from PlayerPrefs
            var playerColorString = PlayerPrefs.GetString("PlayerColor", "");

            // Convert string to color
            var newPlayerColor = playerColorString.Split(',').Length == 3
                ? ColorFromString(playerColorString)
                : Color.blue;
            SetColor(newPlayerColor);

            // Add to cinemachine target group
            m_CameraManager.AddPlayer(gameObject);
        }
        
        /// <summary>
        /// Method <c>DisableControl</c> is used to disable the tank during the phases of the game where the player shouldn't be able to control it.
        /// </summary>
        public void DisableControl()
        {
            m_Movement.enabled = false;
            m_Shooting.enabled = false;
            m_Health.enabled = false;

            m_CanvasGameObject.SetActive(false);
        }

        /// <summary>
        /// Method <c>EnableControl</c> is used to enable the tank during the phases of the game where the player should be able to control it.
        /// </summary>
        public void EnableControl()
        {
            m_Movement.enabled = true;
            m_Shooting.enabled = true;
            m_Health.enabled = true;

            m_CanvasGameObject.SetActive(true);
        }

        /// <summary>
        /// Method <c>SetName</c> is used to set the name of the tank.
        /// </summary>
        public void SetName(string newName)
        {
            if (!isLocalPlayer)
                return;
            CmdSetName(newName);
        }
        
        [Command]
        public void CmdSetName(string newName)
        {
            playerName = newName;
            RpcUpdateName(newName);
        }
        
        [ClientRpc]
        public void RpcUpdateName(string newName)
        {
            OnChangeName(playerName, newName);
        }
        
        public void OnChangeName(string oldName, string newName)
        {
            // Change the name of the player
            playerNameText.text = newName;
        }
        
        /// <summary>
        /// Method <c>SetColor</c> is used to set the color of the tank.
        /// </summary>
        public void SetColor(Color newColor)
        {
            if (!isLocalPlayer)
                return;
            CmdSetColor(newColor);
        }
        
        [Command]
        public void CmdSetColor(Color newColor)
        {
            playerColor = newColor;
            RpcUpdateColor(newColor);
        }
        
        [ClientRpc]
        public void RpcUpdateColor(Color newColor)
        {
            OnChangeColor(playerColor, newColor);
        }
        
        public void OnChangeColor(Color oldColor, Color newColor)
        {
            // Get all of the renderers of the tank
            var renderers = GetComponentsInChildren<MeshRenderer>();

            // Go through all the renderers...
            foreach (var r in renderers)
            {
                // ... set their material color to the color specific to this tank.
                r.material.color = newColor;
            }
            
            // Change the color of the player name
            playerNameText.color = newColor;
        }

        /// <summary>
        /// Method <c>ColorToString</c> is used to convert a color to a string.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The color as a string.</returns>
        private string ColorToString(Color color)
        {
            return $"{color.r * 255},{color.g * 255},{color.b * 255}";
        }
        
        /// <summary>
        /// Method <c>ColorFromString</c> is used to convert a string to a color.
        /// </summary>
        /// <param name="color">The string to convert.</param>
        /// <returns>The string as a color.</returns>
        private Color ColorFromString(string color)
        {
            var colorRGBArray = color.Split(',');
            return new Color(
                float.Parse(colorRGBArray[0]) / 255f, 
                float.Parse(colorRGBArray[1]) / 255f, 
                float.Parse(colorRGBArray[2]) / 255f);
        }
    }
}
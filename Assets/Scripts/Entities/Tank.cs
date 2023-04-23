using System;
using UnityEngine;
using Mirror;
using TMPro;
using PEC2.Managers;
using PEC2.Utilities;
using UnityEngine.InputSystem;

namespace PEC2.Entities
{
    /// <summary>
    /// Class <c>Tank</c> is used to manage various settings on a tank. It works with the GameManager class to control how the tanks behave and whether or not players have control of their tank in the different phases of the game.
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
        [SyncVar]
        public string coloredPlayerText;

        /// <value>Property <c>wins</c> represents the number of wins this player has so far.</value>
        [SyncVar]
        public int wins;
        
        /// <value>Property <c>controlsEnabled</c> represents whether or not the tank is currently controllable.</value>
        [SyncVar(hook = "OnChangeControlsEnabled")]
        public bool controlsEnabled = true;
        
        /// <value>Property <c>m_Rigidbody</c> represents the rigidbody of the tank.</value>
        private Rigidbody m_Rigidbody;
        
        /// <value>Property <c>m_SpawnDirection</c> represents the direction the tank will face when it spawns.</value>
        private Vector3 m_SpawnDirection;

        /// <value>Property <c>m_SpawnRotation</c> represents the rotation the tank will have when it spawns.</value>
        private Quaternion m_SpawnRotation;
        
        /// <value>Property <c>m_TankHealth</c> represents the tank health.</value>
        private TankHealth m_TankHealth;
        
        /// <value>Property <c>m_GameManager</c> represents the game manager.</value>
        private GameManager m_GameManager;

        /// <value>Property <c>m_CameraManager</c> is used to add the tank to the group camera.</value>
        private CameraManager m_CameraManager;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Get the references to the spawn point
            var goTransform = transform;
            m_SpawnDirection = goTransform.position;
            m_SpawnRotation = goTransform.rotation;
            
            // Get the rigidbody
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Get references to the components
            m_GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            m_CameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
            m_TankHealth = GetComponent<TankHealth>();
            
            // Get player name from PlayerPrefs
            var newPlayerName = PlayerPrefs.GetString("PlayerName", "Player " + netId);
            SetName(newPlayerName);
                
            // Get player color from PlayerPrefs
            var playerColorString = PlayerPrefs.GetString("PlayerColor", "");

            // Convert string to color
            var newPlayerColor = playerColorString.Split(',').Length == 3
                ? ColorStrings.ColorFromString(playerColorString)
                : Color.blue;
            SetColor(newPlayerColor);
                
            // Refresh the group camera targets
            m_CameraManager.UpdateTargetGroup();
            
            // Disable controls depeding on the game state
            EnableControls(m_GameManager.controlsEnabled);
        }

        /// <summary>
        /// Method <c>Respawn</c> is used to respawn the tank.
        /// </summary>
        public void Respawn()
        {
            if (!isServer)
                return;
            OnRespawn();
            RpcRespawn();
        }

        /// <summary>
        /// Method <c>OnRespawn</c> is used to respawn the tank.
        /// </summary>
        [ClientRpc]
        public void RpcRespawn()
        {
            OnRespawn(); 
        }
        
        /// <summary>
        /// Method <c>OnRespawn</c> is used to respawn the tank.
        /// </summary>
        public void OnRespawn()
        {
            // Ensure the game object is disabled
            gameObject.SetActive(false);
            
            // Reset the position and rotation
            var goTransform = transform;
            goTransform.position = m_SpawnDirection;
            goTransform.rotation = m_SpawnRotation;

            // Enable the gameobject
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Method <c>SetName</c> is used to set the name of the tank.
        /// </summary>
        /// <param name="newName">The new name of the tank.</param>
        public void SetName(string newName)
        {
            if (!isLocalPlayer)
                return;
            CmdSetName(newName);
        }
        
        /// <summary>
        /// Command <c>CmdSetName</c> is used to set the name of the tank.
        /// </summary>
        /// <param name="newName">The new name of the tank.</param>
        [Command]
        public void CmdSetName(string newName)
        {
            playerName = newName;
            OnChangeName(playerName, newName);
        }
        
        /// <summary>
        /// Method <c>OnChangeName</c> is used to update the name of the tank.
        /// </summary>
        /// <param name="oldName">The old name of the tank.</param>
        /// <param name="newName">The new name of the tank.</param>
        public void OnChangeName(string oldName, string newName)
        {
            // Change the name of the player
            playerNameText.text = newName;
        }
        
        /// <summary>
        /// Method <c>SetColor</c> is used to set the color of the tank.
        /// </summary>
        /// <param name="newColor">The new color of the tank.</param>
        public void SetColor(Color newColor)
        {
            if (!isLocalPlayer)
                return;
            CmdSetColor(newColor);
        }
        
        /// <summary>
        /// Command <c>CmdSetColor</c> is used to set the color of the tank.
        /// </summary>
        /// <param name="newColor">The new color of the tank.</param>
        [Command]
        public void CmdSetColor(Color newColor)
        {
            playerColor = newColor;
            coloredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(playerColor) + ">" + playerName + "</color>";
            OnChangeColor(playerColor, newColor);
        }
        
        /// <summary>
        /// Method <c>OnChangeColor</c> is used to update the color of the tank.
        /// </summary>
        /// <param name="oldColor">The old color of the tank.</param>
        /// <param name="newColor">The new color of the tank.</param>
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
        /// Method <c>EnableControls</c> is used to enable or disable the controls of the tank.
        /// </summary>
        /// <param name="enable">The new state of the controls.</param>
        public void EnableControls(bool enable)
        {
            if (!isLocalPlayer)
                return;
            CmdEnableControls(enable);
        }

        /// <summary>
        /// Command <c>CmdEnableControls</c> is used to enable or disable the controls of the tank.
        /// </summary>
        /// <param name="enable">The new state of the controls.</param>
        [Command]
        public void CmdEnableControls(bool enable)
        {
            controlsEnabled = enable;
            OnChangeControlsEnabled(controlsEnabled, enable);
        }

        /// <summary>
        /// Method <c>OncChangeControlsEnabled</c> is used to enable or disable the controls of the tank.
        /// </summary>
        /// <param name="oldControlsEnabled">The old state of the controls.</param>
        /// <param name="newControlsEnabled">The new state of the controls.</param>
        public void OnChangeControlsEnabled(bool oldControlsEnabled, bool newControlsEnabled)
        {
            gameObject.GetComponent<PlayerInput>().enabled = newControlsEnabled;
        }
    }
}
﻿using System;
using UnityEngine;
using Mirror;
using TMPro;
using PEC2.Managers;
using PEC2.Utilities;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

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
        public string playerName = "Player";
        
        /// <value>Property <c>playerNameText</c> represents the text component of the player name.</value>
        public TextMeshProUGUI playerNameText;
        
        /// <value>Property <c>playerColor</c> represents the color of the player tank.</value>
        [SyncVar(hook = "OnChangeColor")]
        public Color playerColor = Color.blue;

        /// <value>Property <c>coloredPlayerName</c> represents the name of the player with the color tag.</value>
        [HideInInspector]
        [SyncVar (hook = "OnChangeColoredPlayerName")]
        public string coloredPlayerName;

        /// <value>Property <c>wins</c> represents the number of wins this player has so far.</value>
        [SyncVar(hook = "OnChangeWins")]
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
        
        /// <value>Property <c>m_UIManager</c> represents the UI manager.</value>
        private UIManager m_UIManager;

        /// <value>Property <c>m_CameraManager</c> is used to add the tank to the group camera.</value>
        private CameraManager m_CameraManager;
        
        /// <value>Property <c>playerInfoPrefab</c> represents the player info prefab.</value>
        public GameObject playerInfoPrefab;
        
        /// <value>Property <c>m_PlayersInfoPanel</c> represents the players info panel.</value>
        private GameObject m_PlayersInfoPanel;
        
        /// <value>Property <c>m_PlayerInfo</c> represents the player info.</value>
        private GameObject m_PlayerInfo;
        
        /// <value>Property <c>m_PlayerInfoNameText</c> represents the player info name text.</value>
        private TextMeshProUGUI m_PlayerInfoNameText;
        
        /// <value>Property <c>m_PlayerInfoWinsText</c> represents the player info wins text.</value>
        private TextMeshProUGUI m_PlayerInfoWinsText;

        /// <value>Property <c>m_EventsRegistrable</c> represents whether or not the events are registrable.</value>
        private bool m_EventsRegistrable;
        
        /// <value>Property <c>m_PlayerHasJoined</c> represents whether or not the player has joined.</value>
        private bool m_PlayerHasJoined;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            m_EventsRegistrable = false;

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
            m_UIManager = GameObject.Find("UIManager").GetComponent<UIManager>();
            m_CameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
            m_TankHealth = GetComponent<TankHealth>();
            
            // Create the player info
            m_PlayersInfoPanel = GameObject.Find("PlayersInfo");
            m_PlayerInfo = Instantiate(playerInfoPrefab, m_PlayersInfoPanel.transform);
            m_PlayerInfo.transform.SetParent(m_PlayersInfoPanel.transform);
            m_PlayerInfoNameText = m_PlayerInfo.transform.Find("PlayerInfoName").GetComponent<TextMeshProUGUI>();
            m_PlayerInfoWinsText = m_PlayerInfo.transform.Find("PlayerInfoWins").GetComponent<TextMeshProUGUI>();
            
            // Get player name from PlayerPrefs
            var newPlayerName = PlayerPrefs.GetString("PlayerName", "Player " + netId);
            SetName(newPlayerName);
                
            // Get player color from PlayerPrefs
            var playerColorString = PlayerPrefs.GetString("PlayerColor", "");
            var newPlayerColor = playerColorString.Split(',').Length == 3
                ? ColorStrings.ColorFromString(playerColorString)
                : Color.blue;
            SetColor(newPlayerColor);
            
            // Dispatch the joined event
            if (isLocalPlayer)
            {
                var newColoredPlayerName = GetColoredPlayerName(newPlayerName, newPlayerColor);
                CmdSendLocalEvent(newColoredPlayerName + " joined the game", 2f);
            }

            // Update the player info for existing non-local players
            if (!isLocalPlayer)
                UpdatePlayerInfo(coloredPlayerName, "WINS: " + wins);

            // Refresh the group camera targets
            m_CameraManager.UpdateTargetGroup();
            
            // Disable controls depeding on the game state
            EnableControls(m_GameManager.controlsEnabled);
            
            // Enable event dispatching
            m_EventsRegistrable = true;
        }

        /// <summary>
        /// Method <c>OnStopClient</c> is called when the client stops.
        /// </summary>
        public override void OnStopClient()
        {
            // Remove the player info
            Destroy(m_PlayerInfo.gameObject);
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
            UpdateColoredPlayerName();
            if (isServerOnly)
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
            UpdateColoredPlayerName();
            if (isServerOnly)
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
        /// Command <c>UpdateColoredPlayerName</c> is used to update the colored player name.
        /// </summary>
        [Server]
        public void UpdateColoredPlayerName()
        {
            coloredPlayerName = GetColoredPlayerName(playerName, playerColor);
            if (isServerOnly)
                OnChangeColoredPlayerName(coloredPlayerName, coloredPlayerName);
        }
        
        /// <summary>
        /// Method <c>GetColoredPlayerName</c> is used to get the colored player name.
        /// </summary>
        /// <returns></returns>
        public string GetColoredPlayerName(string thisName, Color thisColor)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(thisColor) + ">" + thisName + "</color>";
        }
        
        /// <summary>
        /// Method <c>OnChangeColoredPlayerName</c> is used to update the colored player name.
        /// </summary>
        /// <param name="oldColoredPlayerName">The old colored player name.</param>
        /// <param name="newColoredPlayerName">The new colored player name.</param>
        public void OnChangeColoredPlayerName(string oldColoredPlayerName, string newColoredPlayerName)
        {
            // Dispatch the event
            if (oldColoredPlayerName != "")
                SendLocalEvent(oldColoredPlayerName + " is now " + coloredPlayerName, 2f);
            // Change the name of the player info
            UpdatePlayerInfo(coloredPlayerName);
        }
        
        /// <summary>
        /// Method <c>OnChangeWins</c> is used to update the UI.
        /// </summary>
        public void OnChangeWins(int oldWins, int newWins)
        {
            UpdatePlayerInfo(coloredPlayerName, "WINS: " + newWins);
        }

        /// <summary>
        /// Method <c>UpdatePlayerInfo</c> is used to update the UI.
        /// </summary>
        /// <param name="nameText">The new name text.</param>
        /// <param name="winsText">The new wins text.</param>
        private void UpdatePlayerInfo(string nameText, string winsText = null)
        {
            if (m_PlayerInfo == null)
                return;
            // Update the UI
            m_PlayerInfoNameText.text = nameText;
            if (winsText != null)
                m_PlayerInfoWinsText.text = winsText;
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
            if (isServerOnly)
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
        
        /// <summary>
        /// Method <c>SendLocalEvent</c> is used to send a local event to the UI.
        /// </summary>
        /// <param name="message">The message of the event.</param>
        /// <param name="duration">The duration of the event.</param>
        public void SendLocalEvent(string message, float duration)
        {
            if (m_UIManager != null && m_EventsRegistrable && isLocalPlayer)
                CmdSendLocalEvent(message, duration);
        }
        
        /// <summary>
        /// Command <c>CmdSendLocalEvent</c> is used to send a local event to the UI.
        /// </summary>
        /// <param name="message">The message of the event.</param>
        /// <param name="duration">The duration of the event.</param>
        [Command]
        public void CmdSendLocalEvent(string message, float duration)
        {
            m_UIManager.SendEvent(message, duration);
        }
    }
}
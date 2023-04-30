using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Discovery;
using TMPro;
using PEC2.Entities;
using PEC2.Utilities;

namespace PEC2.UI
{
    /// <summary>
    /// Class <c>MenuInterfaceController</c> controls the UI of the game.
    /// </summary>
    public class MenuInterfaceController : MonoBehaviour
    {
        /// <summary>
        /// Property <c>m_DiscoveredServers</c> represents the list of discovered servers.
        /// </summary>
        private readonly Dictionary<long, ServerResponse> m_DiscoveredServers = new Dictionary<long, ServerResponse>();
        
        /// <summary>
        /// Property <c>networkDiscovery</c> represents the network discovery.
        /// </summary>
        private NetworkDiscovery m_NetworkDiscovery;

        /// <summary>
        /// Property <c>m_DiscoveredGamesList</c> represents the list of discovered games.
        /// </summary>
        private List<DiscoveredGame> m_DiscoveredGamesList;

        /// <summary>
        /// Property <c>serverIP</c> represents the IP of the server.
        /// </summary>
        public string serverIP = "localhost";

        #region InspectorObjects

            /// <summary>
            /// Property <c>menuButtons</c> represents the menu buttons.
            /// </summary>
            public GameObject menuButtons;

            /// <summary>
            /// Property <c>serverPanel</c> represents the server panel.
            /// </summary>
            public GameObject serverPanel;

            /// <summary>
            /// Property <c>playerPanel</c> represents the player panel.
            /// </summary>
            public GameObject playerPanel;

            /// <summary>
            /// Property <c>discoveredGamesPanel</c> represents the discovered games panel.
            /// </summary>
            public GameObject discoveredGamesPanel;

            /// <summary>
            /// Property <c>discoveredGamePrefab</c> represents the discovered game prefab.
            /// </summary>
            public GameObject discoveredGamePrefab;

            /// <summary>
            /// Property <c>playerNameInput</c> represents the player name input.
            /// </summary>
            public GameObject playerNameInput;

            /// <summary>
            /// Property <c>playerColorInput</c> represents the player color input.
            /// </summary>
            public GameObject playerColorInput;
        
        #endregion

        /// <summary>
        /// Method <c>Start</c> is called before the first frame update.
        /// </summary>
        private void Start()
        {
            // Get the network discovery
            m_NetworkDiscovery = GameObject.Find("NetworkManager").GetComponent<NetworkDiscovery>();
        }

        /// <summary>
        /// Method <c>OnValidate</c> is called when the script is loaded or a value is changed in the inspector.
        /// </summary>
        #if UNITY_EDITOR
        private void OnValidate()
        {
            /*
            if (m_NetworkDiscovery != null)
                return;
            m_NetworkDiscovery = GetComponent<NetworkDiscovery>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(m_NetworkDiscovery.OnServerFound, OnDiscoveredServer);
            UnityEditor.Undo.RecordObjects(new Object[] { this, m_NetworkDiscovery }, "Set NetworkDiscovery");
            */
        }
        #endif

        /// <summary>
        /// Method <c>ToggleServerMode</c> is called when entering or leaving server mode.
        /// </summary>
        public void ToggleServerMode()
        {
            serverPanel.SetActive(!serverPanel.activeSelf);
            menuButtons.SetActive(!menuButtons.activeSelf);
        }

        /// <summary>
        /// Method <c>TooglePlayerMode</c> is called when entering or leaving player mode.
        /// </summary>
        public void TooglePlayerMode()
        {
            playerPanel.SetActive(!playerPanel.activeSelf);
            menuButtons.SetActive(!menuButtons.activeSelf);
        }

        /// <summary>
        /// Method <c>CreateDedicatedServerAction</c> is called when the create dedicated server button is pressed.
        /// </summary>
        public void CreateDedicatedServerAction()
        {
            Debug.Log("NetworkServer.active: " + NetworkServer.active);
            Debug.Log("NetworkClient.isConnected: " + NetworkClient.isConnected);
            Debug.Log("NetworkClient.active: " + NetworkClient.active);
            if (NetworkServer.active || NetworkClient.active || NetworkClient.isConnected)
                return;
            m_DiscoveredServers.Clear();
            NetworkManager.singleton.StartServer();
            m_NetworkDiscovery.AdvertiseServer();
        }

        /// <summary>
        /// Method <c>WaitForServersAndUpdate</c> is called when the wait for servers and update button is pressed.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WeWaitForServersAndUpdate(){
            
            // Network Discovery starts
            // Debug.Log("Updating servers...");
            m_DiscoveredServers.Clear();
            m_NetworkDiscovery.StartDiscovery();

            // Delete the old list
            if (m_DiscoveredGamesList != null)
            {
                if (m_DiscoveredGamesList.Count > 0)
                {
                    foreach (var discoveredGame in m_DiscoveredGamesList)
                    {
                        Destroy(discoveredGame.banner);
                    }
                    m_DiscoveredGamesList.Clear();
                }
            }
            else
            {
                m_DiscoveredGamesList = new List<DiscoveredGame>();
            }
            
            // Give some time to discover the servers
            yield return new WaitForSeconds(0.25f);
            
            // Generate the new list
            var i = 0;
            foreach (var info in m_DiscoveredServers.Values)
            {
                var discoveredGame = new DiscoveredGame
                {
                    banner = Instantiate(discoveredGamePrefab)
                };
                discoveredGame.banner.transform.SetParent(discoveredGamesPanel.transform, false);
                
                var rectTransform = discoveredGame.banner.GetComponent<RectTransform>();
                rectTransform.Translate(0, -60 * i, 0);
                discoveredGame.address = info;

                var button = discoveredGame.banner.GetComponentInChildren<Button>();
                button.onClick.AddListener(delegate {JoinServer(discoveredGame.address);});

                var text = discoveredGame.banner.GetComponentInChildren<TextMeshProUGUI>();
                text.SetText(discoveredGame.address.EndPoint.Address.ToString());

                m_DiscoveredGamesList.Add(discoveredGame);
                Debug.Log(" ---------> Added");
                i++;
            }
            //Debug.Log(" -> " + i + " servers found!");
            m_NetworkDiscovery.StopDiscovery();
        }

        /// <summary>
        /// Method <c>UpdateServersAction</c> is called when the update servers button is pressed.
        /// </summary>
        public void UpdateServersAction()
        {
            StartCoroutine(WeWaitForServersAndUpdate());
        }

        /// <summary>
        /// Method <c>CloseGameAction</c> is called when the close game button is pressed.
        /// </summary>
        public void CloseGameAction()
        {
            Application.Quit();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        /// <summary>
        /// Method <c>SaveSettings</c> is used to save the settings of the player.
        /// </summary>
        private void SaveSettings()
        {
            // Save Name
            var nameText = playerNameInput.GetComponent<TMP_InputField>().text;
            if (nameText != null && nameText.Trim() != "")
            {
                // Playerprefs Name
                PlayerPrefs.SetString("PlayerName", nameText);
                //Debug.Log("Playerprefs name: " + NameText);
            }
            
            // Save Color
            var dropdown = playerColorInput.GetComponent<TMP_Dropdown>();
            var colorText = dropdown.options[dropdown.value].text;
            if (colorText != null && colorText.Trim() != "")
            {
                var actualColorString = ConvertStringToColorString(colorText);
                if (!actualColorString.Equals(""))
                {
                    // Playerprefs Color
                    PlayerPrefs.SetString("PlayerColor", actualColorString);
                    //Debug.Log("Playerprefs color: " + ActualColorString);
                }
            }
        }

        /// <summary>
        /// Method <c>ConvertStringToColorString</c> is used to convert a string to a color string.
        /// </summary>
        /// <param name="colorName">The color name.</param>
        /// <returns>The color string.</returns>
        private string ConvertStringToColorString(string colorName)
        {
            return colorName switch
            {
                "Blue" => ColorStrings.ColorToString(Color.blue),
                "Green" => ColorStrings.ColorToString(Color.green),
                "Yellow" => ColorStrings.ColorToString(Color.yellow),
                "Red" => ColorStrings.ColorToString(Color.red),
                _ => ""
            };
        }

        /// <summary>
        /// Method <c>JoinServer</c> is used to join a server.
        /// </summary>
        /// <param name="info">The server info.</param>
        private void JoinServer(ServerResponse info)
        {
            SaveSettings();
            m_NetworkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(info.uri);
        }

        /// <summary>
        /// Method <c>CreateGame</c> is used to create a game.
        /// </summary>
        public void CreateGame()
        {
            Debug.Log("NetworkServer.active: " + NetworkServer.active);
            Debug.Log("NetworkClient.isConnected: " + NetworkClient.isConnected);
            Debug.Log("NetworkClient.active: " + NetworkClient.active);
            if (NetworkServer.active || NetworkClient.active || NetworkClient.isConnected)
                return;
            SaveSettings();
            m_DiscoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            m_NetworkDiscovery.AdvertiseServer();
        }

        /// <summary>
        /// Method <c>JoinGame</c> is used to join a game.
        /// </summary>
        public void JoinGame()
        {
            Debug.Log("NetworkServer.active: " + NetworkServer.active);
            Debug.Log("NetworkClient.isConnected: " + NetworkClient.isConnected);
            Debug.Log("NetworkClient.active: " + NetworkClient.active);
            if (NetworkServer.active || NetworkClient.active || NetworkClient.isConnected)
                return;
            SaveSettings();
            NetworkManager.singleton.networkAddress = serverIP;
            NetworkManager.singleton.StartClient();
        }

        /// <summary>
        /// Method <c>OnDiscoveredServer</c> is called when a server is discovered.
        /// </summary>
        /// <param name="info">The server info.</param>
        public void OnDiscoveredServer(ServerResponse info)
        {
            // Note that you can check the versioning to decide if you can connect to the server or not using this method
            m_DiscoveredServers[info.serverId] = info;
            //Debug.Log("Server Discovered!");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using UnityEngine.UI;

namespace PEC2
{
    public class MenuInterfaceController : MonoBehaviour
    {
        readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        Vector2 scrollViewPos = Vector2.zero;
        public NetworkDiscovery networkDiscovery;

        // Listado de partidas encontradas
        private List<DiscoveredGame> DiscoveredGamesList;

        // Para jugar en LAN, especificar IP del servidor
        public string serverIP = "localhost";

        // Inspector objects

        public GameObject MenuButtons;

        public GameObject ServerPanel;

        public GameObject PlayerPanel;

        public GameObject DiscoveredGamesPanel;

        public GameObject DiscoveredGamePrefab;

        public GameObject RefreshServersButton;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (networkDiscovery == null)
            {
                networkDiscovery = GetComponent<NetworkDiscovery>();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
                UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
            }
        }
#endif

        public void ServerModeAction(){
            MenuButtons.SetActive(false);
            ServerPanel.SetActive(true);
        }

        public void PlayerModeAction(){
            MenuButtons.SetActive(false);
            PlayerPanel.SetActive(true);
            
            // Comienza Network Discovery
            Debug.Log("Actualizando servidores...");
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
            DiscoveredGamesList = new List<DiscoveredGame>();
        }

        public void CreateDedicatedServerAction(){
            if (!NetworkClient.isConnected && !NetworkServer.active) {
                if (!NetworkClient.active) {
                    discoveredServers.Clear();
                    NetworkManager.singleton.StartServer();
                    networkDiscovery.AdvertiseServer();
                }
            }
        }

        public void UpdateServersAction(){
            // Borramos la lista antigua
            if(DiscoveredGamesList.Count > 0){
                foreach(DiscoveredGame discoveredGame in DiscoveredGamesList){
                    Destroy(discoveredGame.banner);
                }
                DiscoveredGamesList.Clear();
            }

            // Generamos la lista nueva
            int i = 0;
            foreach (ServerResponse info in discoveredServers.Values)
            {
                DiscoveredGame discoveredGame = new DiscoveredGame();
                discoveredGame.banner = Instantiate(DiscoveredGamePrefab);
                discoveredGame.banner.transform.SetParent(DiscoveredGamesPanel.transform, false);
                RectTransform rectTransform = discoveredGame.banner.GetComponent<RectTransform>();
                rectTransform.Translate(0, -60*i, 0);
                discoveredGame.address = info;

                Button button = discoveredGame.banner.GetComponentInChildren<Button>();
                button.onClick.AddListener(delegate {JoinServer(discoveredGame.address);});

                TMPro.TextMeshProUGUI texto = discoveredGame.banner.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                texto.SetText(discoveredGame.address.EndPoint.Address.ToString());

                DiscoveredGamesList.Add(discoveredGame);
                i++;
            }
            Debug.Log(" -> " + i + " servidores encontrados!");
        }

        private void JoinServer(ServerResponse info){
            networkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(info.uri);
        }

        public class DiscoveredGame{
            public ServerResponse address{get; set;}
            public GameObject banner{get; set;}
        }

        public void CreateGame(){
            Debug.Log(" -> Crear partida");
            if(!NetworkClient.isConnected && !NetworkServer.active){
                if(!NetworkClient.active){
                    discoveredServers.Clear();
                    NetworkManager.singleton.StartHost();
                    networkDiscovery.AdvertiseServer();
                }
            }
        }

        public void JoinGame(){
            Debug.Log(" -> Unirse a partida");
            if(!NetworkClient.isConnected && !NetworkServer.active){
                if(!NetworkClient.active){
                    Debug.Log(" Cliente desconectado + servidor inactivo + cliente inactivo...");
                    NetworkManager.singleton.networkAddress = serverIP;
                    NetworkManager.singleton.StartClient();
                }
            }
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            // Note that you can check the versioning to decide if you can connect to the server or not using this method
            discoveredServers[info.serverId] = info;
            Debug.Log("Server Discovered!");
        }
    }
}

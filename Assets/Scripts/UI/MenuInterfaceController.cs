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
        private List<PartidaEncontrada> PartidasEncontradas;

        // Para jugar en LAN, especificar IP del servidor
        public string serverIP = "localhost";

        // Inspector objects

        public GameObject BotonesInicio;

        public GameObject PanelServidor;

        public GameObject PanelJugador;

        public GameObject PanelPartidas;

        public GameObject PartidaEncontradaPrefab;

        public GameObject ActualizaServidoresButton;

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

        public void ModoServidorAction(){
            BotonesInicio.SetActive(false);
            PanelServidor.SetActive(true);
        }

        public void ModoJugadorAction(){
            BotonesInicio.SetActive(false);
            PanelJugador.SetActive(true);
            
            // Comienza Network Discovery
            Debug.Log("Actualizando servidores...");
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
            PartidasEncontradas = new List<PartidaEncontrada>();
        }

        public void CrearServidorDedicadoAction(){
            if (!NetworkClient.isConnected && !NetworkServer.active) {
                if (!NetworkClient.active) {
                    discoveredServers.Clear();
                    NetworkManager.singleton.StartServer();
                    networkDiscovery.AdvertiseServer();
                }
            }
        }

        public void ActualizaServidoresAction(){
            // Borramos la lista antigua
            if(PartidasEncontradas.Count > 0){
                foreach(PartidaEncontrada partidaEncontrada in PartidasEncontradas){
                    Destroy(partidaEncontrada.banner);
                }
                PartidasEncontradas.Clear();
            }

            // Generamos la lista nueva
            int i = 0;
            foreach (ServerResponse info in discoveredServers.Values)
            {
                PartidaEncontrada partidaEncontrada = new PartidaEncontrada();
                partidaEncontrada.banner = Instantiate(PartidaEncontradaPrefab);
                partidaEncontrada.banner.transform.SetParent(PanelPartidas.transform, false);
                RectTransform rectTransform = partidaEncontrada.banner.GetComponent<RectTransform>();
                rectTransform.Translate(0, -60*i, 0);
                partidaEncontrada.address = info;

                Button button = partidaEncontrada.banner.GetComponentInChildren<Button>();
                button.onClick.AddListener(delegate {JoinServer(partidaEncontrada.address);});

                TMPro.TextMeshProUGUI texto = partidaEncontrada.banner.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                texto.SetText(partidaEncontrada.address.EndPoint.Address.ToString());

                PartidasEncontradas.Add(partidaEncontrada);
                i++;
            }
            Debug.Log(" -> " + i + " servidores encontrados!");
        }

        private void JoinServer(ServerResponse info){
            networkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(info.uri);
        }

        public class PartidaEncontrada{
            public ServerResponse address{get; set;}
            public GameObject banner{get; set;}
        }

        public void CrearPartida(){
            Debug.Log(" -> Crear partida");
            if(!NetworkClient.isConnected && !NetworkServer.active){
                if(!NetworkClient.active){
                    discoveredServers.Clear();
                    NetworkManager.singleton.StartHost();
                    networkDiscovery.AdvertiseServer();
                }
            }
        }

        public void UnirsePartida(){
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

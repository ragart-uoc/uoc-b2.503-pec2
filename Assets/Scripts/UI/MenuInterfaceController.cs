using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;

namespace PEC2
{
    public class MenuInterfaceController : MonoBehaviour
    {
        readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        Vector2 scrollViewPos = Vector2.zero;
        public NetworkDiscovery networkDiscovery;

        // Para jugar en LAN, especificar IP del servidor
        public string serverIP = "localhost";

        // Inspector objects

        public GameObject BotonesInicio;

        public GameObject PanelServidor;

        public GameObject PanelJugador;

        public GameObject PanelPartidas;

        public GameObject PartidaEncontradaPrefab;

        private void Update() {
            if(NetworkManager.singleton != null && !NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active){
                int i = 0;
                foreach (KeyValuePair<long, Mirror.Discovery.ServerResponse> entry in discoveredServers)
                {
                    GameObject a = Instantiate(PartidaEncontradaPrefab, new Vector3(0, i++, 0), new Quaternion(), PanelPartidas.transform);
                }
            }
        }

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
        }

        public void CrearServidorDedicadoAction(){
            if (!NetworkClient.isConnected && !NetworkServer.active) {
                if (!NetworkClient.active) {
                    NetworkManager.singleton.StartServer();
                }
            }
        }

        public void CrearPartida(){
            Debug.Log(" -> Crear partida");
            if(!NetworkClient.isConnected && !NetworkServer.active){
                if(!NetworkClient.active){
                    Debug.Log(" Cliente desconectado + servidor inactivo + cliente inactivo...");
                    NetworkManager.singleton.StartHost();
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

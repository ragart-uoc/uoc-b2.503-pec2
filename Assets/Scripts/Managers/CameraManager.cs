using System;
using System.Linq;
using UnityEngine;
using Cinemachine;
using Mirror;

namespace PEC2.Managers
{
    /// <summary>
    /// Class <c>GameManager</c> controls the flow of the game.
    /// </summary>
    public class CameraManager : NetworkBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static CameraManager _instance;

        /// <value>Property <c>m_GroupTargetCamera</c> is a reference to the group camera CinemachineTargetGroup component.</value>
        public CinemachineTargetGroup groupTargetCamera;

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
        /// Method <c>OnStartServer</c> is invoked for NetworkBehaviour objects when they become active on the server.
        /// </summary>
        public override void OnStartServer()
        {
            UpdateTargetGroup();
        }
        
        /// <summary>
        /// Method <c>OnStartClient</c> is invoked for NetworkBehaviour objects when they become active on the client.
        /// </summary>
        public override void OnStartClient()
        {
            UpdateTargetGroup();
        }

        /// <summary>
        /// Method <c>UpdateTargetGroup</c> updates the group camera targets.
        /// </summary>
        public void UpdateTargetGroup()
        {
            if (isServer)
            {
                OnChangeTargetGroup();
                RpcUpdateTargetGroup();
            }
            else if (isLocalPlayer)
            {
                CmdUpdateTargetGroup();
            }
        }
        
        /// <summary>
        /// Command <c>CmdUpdateTargetGroup</c> updates the group camera targets.
        /// </summary>
        [Command]
        private void CmdUpdateTargetGroup()
        {
            OnChangeTargetGroup();
            RpcUpdateTargetGroup();
        }

        /// <summary>
        /// Method <c>RpcUpdateTargetGroup</c> updates the group camera targets.
        /// </summary>
        [ClientRpc]
        private void RpcUpdateTargetGroup()
        {
            OnChangeTargetGroup();
        }
        
        /// <summary>
        /// Method <c>OnChangeTargetGroup</c> is called when the group camera targets change.
        /// </summary>
        private void OnChangeTargetGroup()
        {
            // Clear all targets from the group camera
            groupTargetCamera.m_Targets = Array.Empty<CinemachineTargetGroup.Target>();
            
            // Get entities from either the network server or the network client
            var entities = isServer ? NetworkServer.spawned.Values : NetworkClient.spawned.Values;
            
            // Filter only entities with the tag "Player" or "Enemy" and that are active
            var filteredEntities = entities
                .Where(e=> e.gameObject.CompareTag("Player") || e.gameObject.CompareTag("Enemy"))
                .Where(e=> e.gameObject.activeSelf);
            
            // Loop through all spawned entities
            foreach (var entity in filteredEntities)
            {
                // Add the entity to the group camera
                AddTargetToGroupCamera(entity.gameObject);
            }
        }
        
        /// <summary>
        /// Method <c>AddTargetToGroupCamera</c> adds a target to the group camera.
        /// </summary>
        /// <param name="target">The target to add.</param>
        private void AddTargetToGroupCamera(GameObject target)
        {
            if (groupTargetCamera.FindMember(target.transform) > -1)
                return;
            groupTargetCamera.AddMember(target.transform, 1, 5);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace PEC2.Managers
{
    /// <summary>
    /// Class <c>GameManager</c> controls the flow of the game.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static CameraManager _instance;

        /// <value>Property <c>m_GroupTargetCamera</c> is a reference to the group camera CinemachineTargetGroup component.</value>
        public CinemachineTargetGroup groupTargetCamera;
        
        /// <value>Property <c>m_Players</c> is a list of all players in the scene.</value>
        private List<GameObject> m_Players = new List<GameObject>();

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
        }
        
        public void AddPlayer(GameObject player)
        {
            m_Players.Add(player);
            RefreshGroupCamera();
        }
        
        public void RemovePlayer(GameObject player)
        {
            m_Players.Remove(player);
            RefreshGroupCamera();
        }

        private void RefreshGroupCamera()
        {
            // Add all players to the group camera
            foreach (var player in m_Players)
            {
                AddTargetToGroupCamera(player);
            }

            // Remove all targets that are not in the list
            var members = groupTargetCamera.m_Targets;
            foreach (var member in members)
            {
                if (!m_Players.Contains(member.target.gameObject))
                    RemoveTargetFromGroupCamera(member.target.gameObject);
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
        
        /// <summary>
        /// Method <c>RemoveTargetFromGroupCamera</c> removes a target from the group camera.
        /// </summary>
        /// <param name="target">The target to remove.</param>
        private void RemoveTargetFromGroupCamera(GameObject target)
        {
            if (groupTargetCamera.FindMember(target.transform) == -1)
                return;
            groupTargetCamera.RemoveMember(target.transform);
        }
    }
}

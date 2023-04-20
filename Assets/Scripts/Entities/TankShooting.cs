using UnityEngine;
using UnityEngine.Serialization;
using Mirror;

namespace PEC2.Entities
{
    /// <summary>
    /// Class <c>TankShooting</c> is used to control the shooting of the tank.
    /// </summary>
    public class TankShooting : NetworkBehaviour
    {
        /// <value>Property <c>shell</c> represents the prefab of the shell.</value>
        [FormerlySerializedAs("m_Shell")]
        public Rigidbody shell;
        
        /// <value>Property <c>altShell</c> represents the prefab of the alt shell.</value>
        public Rigidbody altShell;

        /// <value>Property <c>fireTransform</c> represents the child of the tank where the shells are spawned.</value>
        [FormerlySerializedAs("m_FireTransform")]
        public Transform fireTransform;

        /// <summary>
        /// Method <c>OnFire</c> is called when the fire button is released.
        /// </summary>
        private void OnFire()
        {
            if (!isLocalPlayer)
                return;
            CmdFire();
        }
        
        /// <summary>
        /// Method <c>OnAltFire</c> is called when the alt fire button is released.
        /// </summary>
        private void OnAltFire()
        {
            if (!isLocalPlayer)
                return;
            CmdAltFire();
        }

        /// <summary>
        /// Command <c>CmdFire</c> fires the shell.
        /// </summary>
        [Command]
        private void CmdFire()
        {
            // Create an instance of the shell and store a reference to it's rigidbody
            var shellInstance = Instantiate(shell, fireTransform.position, fireTransform.rotation);
            
            // Spawn the shell on the clients
            NetworkServer.Spawn(shellInstance.gameObject);
        }

        /// <summary>
        /// Command <c>CmdAltFire</c> fires the shell.
        /// </summary>
        [Command]
        private void CmdAltFire()
        {
            // Create an instance of the shell and store a reference to it's rigidbody
            var shellInstance = Instantiate(altShell, fireTransform.position, fireTransform.rotation);
            
            // Spawn the shell on the clients
            NetworkServer.Spawn(shellInstance.gameObject);
        }
    }
}
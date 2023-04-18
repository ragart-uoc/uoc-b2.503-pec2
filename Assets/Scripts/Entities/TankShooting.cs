using UnityEngine;
using UnityEngine.UI;
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

        /// <value>Property <c>fireTransform</c> represents the child of the tank where the shells are spawned.</value>
        [FormerlySerializedAs("m_FireTransform")]
        public Transform fireTransform;

        /// <value>Property <c>shootingAudio</c> represents the audio source to play when the shell is fired.</value>
        [FormerlySerializedAs("m_ShootingAudio")]
        public AudioSource shootingAudio;

        /// <value>Property <c>fireClip</c> represents the audio that plays when each shot is fired.</value>
        [FormerlySerializedAs("m_FireClip")]
        public AudioClip fireClip;

        /// <value>Property <c>minLaunchForce</c> represents the force given to the shell if the fire button is not held.</value>
        [FormerlySerializedAs("m_MinLaunchForce")]
        public float minLaunchForce = 15f;

        /// <value>Property <c>m_CurrentLaunchForce</c> represents the force that will be given to the shell when the fire button is released.</value>
        private float m_CurrentLaunchForce;

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
        /// Command <c>CmdFire</c> fires the shell.
        /// </summary>
        [Command]
        private void CmdFire()
        {
            // Create an instance of the shell and store a reference to it's rigidbody
            var shellInstance = Instantiate(shell, fireTransform.position, fireTransform.rotation);
            
            // Spawn the shell on the clients
            NetworkServer.Spawn(shellInstance.gameObject);

            // Set the shell's velocity to the launch force in the fire position's forward direction
            shellInstance.velocity = m_CurrentLaunchForce * fireTransform.forward;

            // Change the clip to the firing clip and play it
            shootingAudio.clip = fireClip;
            shootingAudio.Play();

            // Reset the launch force.  This is a precaution in case of missing button events
            m_CurrentLaunchForce = minLaunchForce;
        }
    }
}
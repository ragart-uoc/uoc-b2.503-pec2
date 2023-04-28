using UnityEngine;
using UnityEngine.UI;
using Mirror;
using PEC2.Managers;

namespace PEC2.Entities
{
    /// <summary>
    /// Class <c>TankHealth</c> is used to manage the health of the tank.
    /// </summary>
    public class TankHealth : NetworkBehaviour
    {
        /// <value>Property <c>StartingHealth</c> represents the amount of health each tank starts with.</value>
        private const float StartingHealth = 100f;

        /// <value>Property <c>currentHealth</c> represents how much health the tank currently has.</value>
        [SyncVar(hook = "OnChangeHealth")]
        public float currentHealth;

        /// <value>Property <c>slider</c> represents the slider to represent how much health the tank currently has.</value>
        public Slider slider;

        /// <value>Property <c>fillImage</c> represents the image component of the slider.</value>
        public Image fillImage;

        /// <value>Property <c>fullHealthColor</c> represents the color the health bar will be when on full health.</value>
        public Color fullHealthColor = Color.green;

        /// <value>Property <c>zeroHealthColor</c> represents the color the health bar will be when on no health.</value>
        public Color zeroHealthColor = Color.red;

        /// <value>Property <c>explosionPrefab</c> represents the prefab that will be used whenever the tank dies.</value>
        public GameObject explosionPrefab;

        /// <value>Property <c>m_Dead</c> represents whether or not the tank is currently dead.</value>
        private bool m_Dead;
        
        /// <value>Property <c>destroyOnDeath</c> represents whether or not the tank should be destroyed when it dies.</value>
        public bool destroyOnDeath;

        /// <value>Property <c>m_CameraManager</c> is used to add the tank to the group camera.</value>
        private CameraManager m_CameraManager;

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Set the initial health of the tank.
            currentHealth = StartingHealth;
            if (isServer)
                OnChangeHealth(currentHealth, currentHealth);

            // Get a reference to the camera manager
            m_CameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        }

        /// <summary>
        /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            if (!isServer)
                return;

            // When the tank is enabled, reset the tank's health and whether or not it's dead.
            currentHealth = StartingHealth;
            m_Dead = false;

            // Update the health slider's value and color.
            OnChangeHealth(currentHealth, currentHealth);
        }

        /// <summary>
        /// Method <c>TakeDamage</c> is used to inflict damage upon the tank.
        /// </summary>
        /// <param name="amount">The amount of damage to inflict.</param>
        public void TakeDamage(float amount)
        {
            if (!isServer)
                return;
            
            // Reduce current health by the amount of damage done
            currentHealth -= amount;

            // If the current health is at or below zero and it has not yet been registered, call OnDeath
            if (!(currentHealth <= 0f) || m_Dead)
                return;
            
            Die();
        }

        /// <summary>
        /// Method <c>OnChangeHealth</c> is called when the health value is changed.
        /// </summary>
        /// <param name="oldHealth">The previous health value.</param>
        /// <param name="newHealth">The new health value.</param>
        public void OnChangeHealth(float oldHealth, float newHealth)
        {
            // Set the slider's value appropriately.
            slider.value = newHealth;

            // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
            fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, newHealth / StartingHealth);
        }

        /// <summary>
        /// Method <c>Die</c> is called when the tank reaches zero health.
        /// </summary>
        private void Die()
        {
            // Set the flag so that this function is only called once
            m_Dead = true;
            
            // Explode
            OnExplode();
            RpcExplode();

            // Destroy or disable the tank
            if (destroyOnDeath)
            {
                Destroy(gameObject);
                NetworkServer.Destroy(gameObject);
            }
            else if (isServer)
            {
                gameObject.SetActive(false);
                RpcDisable();
            }
                
            // Refresh the group camera targets
            m_CameraManager.UpdateTargetGroup();
        }
        
        /// <summary>
        /// Method <c>RpcExplode</c> is used to play the explosion on the client. 
        /// </summary>
        [ClientRpc]
        private void RpcExplode()
        {
            OnExplode();
        }

        /// <summary>
        /// Method <c>OnExplode</c> is used to play the explosion.
        /// </summary>
        private void OnExplode()
        {
            // Instantiate the explosion prefab and get a reference to the particle system on it.
            var goTransform = transform;
            var explosionParticles = Instantiate(explosionPrefab, goTransform.position, goTransform.rotation)
                .GetComponent<ParticleSystem>();

            // Get a reference to the audio source on the instantiated prefab.
            var explosionAudio = explosionParticles.GetComponent<AudioSource>();

            // Play the particle system of the tank exploding
            explosionParticles.Play();

            // Play the tank explosion sound effect
            explosionAudio.Play();
        }
        
        /// <summary>
        /// Method <c>RpcDisable</c> is used to disable the tank on the client.
        /// </summary>
        [ClientRpc]
        public void RpcDisable()
        {
            gameObject.SetActive(false);
        }
    }
}
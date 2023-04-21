using UnityEngine;
using UnityEngine.UI;
using Mirror;

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

        /// <value>Property <c>m_ExplosionAudio</c> represents the audio source to play when the tank explodes.</value>
        private AudioSource m_ExplosionAudio;

        /// <value>Property <c>m_ExplosionParticles</c> represents the particle system the will play when the tank is destroyed.</value>
        private ParticleSystem m_ExplosionParticles;

        /// <value>Property <c>m_Dead</c> represents whether or not the tank is currently dead.</value>
        private bool m_Dead;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Instantiate the explosion prefab and get a reference to the particle system on it.
            m_ExplosionParticles = Instantiate(explosionPrefab).GetComponent<ParticleSystem>();

            // Get a reference to the audio source on the instantiated prefab.
            m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

            // Disable the prefab so it can be activated when it's required.
            m_ExplosionParticles.gameObject.SetActive(false);
        }

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            currentHealth = StartingHealth;
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
            if (currentHealth <= 0f && !m_Dead)
            {
                OnDeath();
            }
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
        /// Method <c>OnDeath</c> is called when the tank reaches zero health.
        /// </summary>
        private void OnDeath()
        {
            // Set the flag so that this function is only called once
            m_Dead = true;
            
            // Move the instantiated explosion prefab to the tank's position and turn it on
            m_ExplosionParticles.transform.position = transform.position;
            m_ExplosionParticles.gameObject.SetActive(true);

            // Play the particle system of the tank exploding
            m_ExplosionParticles.Play();

            // Play the tank explosion sound effect
            m_ExplosionAudio.Play();

            // Turn the tank off
            //gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
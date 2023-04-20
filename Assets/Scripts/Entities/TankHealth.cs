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
        
        /// <value>Property <c>startingHealth</c> represents the amount of health each tank starts with.</value>
        public float startingHealth = 100f;

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

        /// <value>Property <c>m_CurrentHealth</c> represents how much health the tank currently has.</value>
        [SyncVar(hook = "SetHealthUI")]
        private float m_CurrentHealth;

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
        /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            // When the tank is enabled, reset the tank's health and whether or not it's dead.
            m_CurrentHealth = startingHealth;
            m_Dead = false;
        }

        /// <summary>
        /// Method <c>TakeDamage</c> is used to inflict damage upon the tank.
        /// </summary>
        /// <param name="amount">The amount of damage to inflict.</param>
        public void TakeDamage(float amount)
        {
            // Reduce current health by the amount of damage done
            m_CurrentHealth -= amount;

            // If the current health is at or below zero and it has not yet been registered, call OnDeath
            if (m_CurrentHealth <= 0f && !m_Dead)
            {
                OnDeath();
            }
        }

        /// <summary>
        /// Method <c>SetHealthUI</c> is used to update the health slider's value and color.
        /// </summary>
        /// <param name="oldHealth">The previous health value.</param>
        /// <param name="newHealth">The new health value.</param>
        private void SetHealthUI(float oldHealth, float newHealth)
        {
            // Set the slider's value appropriately.
            slider.value = newHealth;

            // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
            fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, newHealth / startingHealth);
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
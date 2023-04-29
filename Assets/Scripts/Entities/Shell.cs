using UnityEngine;
using Mirror;

namespace PEC2.Entities
{
    /// <summary>
    /// Class <c>ShellExplosion</c> is used to make the shell explode.
    /// </summary>
    public class Shell : NetworkBehaviour
    {
        /// <value>Property <c>tankMask</c> represents the layer mask of the tanks. It's used to filter what the explosion affects It should be set to "Players".</value>
        public LayerMask tankMask;

        /// <value>Property <c>explosionParticles</c> represents the particles that will play on explosion.</value>
        public ParticleSystem explosionParticles;

        /// <value>Property <c>audioSOurce</c> represents the audio source.</value>
        public AudioSource audioSource;

        /// <value>Property <c>fireClip</c> represents the audio that plays when each shot is fired.</value>
        public AudioClip fireClip;
        
        /// <value>Property <c>explosionAudio</c> represents the audio that will play on explosion.</value>
        public AudioClip explosionClip;

        /// <value>Property <c>launchForce</c> represents the force given to the shell.</value>
        public float launchForce = 15f;

        /// <value>Property <c>maxDamage</c> represents the amount of damage done if the explosion is centred on a tank.</value>
        public float maxDamage = 100f;

        /// <value>Property <c>explosionForce</c> represents the amount of force added to a tank at the centre of the explosion.</value>
        public float explosionForce = 1000f;

        /// <value>Property <c>maxLifeTime</c> represents the time in seconds before the shell is removed.</value>
        public float maxLifeTime = 2f;

        /// <value>Property <c>explosionRadius</c> represents the maximum distance away from the explosion tanks can be and are still affected.</value>
        public float explosionRadius = 5f;

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Set the shell's velocity to the launch force in the fire position's forward direction
            GetComponent<Rigidbody>().velocity = launchForce * transform.forward;

            // Change the clip to the firing clip and play it
            if (audioSource.enabled)
            {
                audioSource.clip = fireClip;
                audioSource.Play();
            }

            // If it isn't destroyed by then, destroy the shell after it's lifetime
            Destroy(gameObject, maxLifeTime);
        }

        /// <summary>
        /// Method <c>OnTriggerEnter</c> is called when a GameObject collides with another GameObject.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
			// Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius
            var colliders = Physics.OverlapSphere(transform.position, explosionRadius, tankMask);

            // Go through all the colliders...
            foreach (var c in colliders)
            {
                // ... and find their rigidbody.
                var targetRigidbody = c.GetComponent<Rigidbody>();

                // If they don't have a rigidbody, go on to the next collider
                if (!targetRigidbody)
                    continue;

                // Add an explosion force.
                targetRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius);

                // Find the TankHealth script associated with the rigidbody.
                var targetHealth = targetRigidbody.GetComponent<TankHealth>();

                // If there is no TankHealth script attached to the gameobject, go on to the next collider.
                if (!targetHealth)
                    continue;

                // Calculate the amount of damage the target should take based on it's distance from the shell.
                var damage = CalculateDamage(targetRigidbody.position);

                // Deal this damage to the tank.
                targetHealth.TakeDamage(damage);
            }

            // Unparent the particles from the shell.
            explosionParticles.transform.parent = null;

            // Play the particle system.
            explosionParticles.Play();

            // Play the explosion sound effect.
            if (audioSource.enabled)
            {
                audioSource.clip = explosionClip;
                audioSource.Play();
            }

            // Once the particles have finished, destroy the gameobject they are on.
            var mainModule = explosionParticles.main;
            Destroy(explosionParticles.gameObject, mainModule.duration);

            // Destroy the shell.
            Destroy(gameObject);
        }

        /// <summary>
        /// Method <c>CalculateDamage</c> calculates the amount of damage a target should take based on its distance to the explosion.
        /// </summary>
        private float CalculateDamage(Vector3 targetPosition)
        {
            // Create a vector from the shell to the target.
            var explosionToTarget = targetPosition - transform.position;

            // Calculate the distance from the shell to the target.
            var explosionDistance = explosionToTarget.magnitude;

            // Calculate the proportion of the maximum distance (the explosionRadius) the target is away.
            var relativeDistance = (explosionRadius - explosionDistance) / explosionRadius;

            // Calculate damage as this proportion of the maximum possible damage.
            var damage = relativeDistance * maxDamage;

            // Make sure that the minimum damage is always 0.
            damage = Mathf.Max(0f, damage);

            return damage;
        }
    }
}
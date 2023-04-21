using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Mirror;

namespace PEC2.Entities
{
    /// <summary>
    /// Class <c>TankMovement</c> is used to move the tank.
    /// </summary>
    public class TankMovement : NetworkBehaviour
    {

        /// <value>Property <c>speed</c> represents how fast the tank moves forward and back.</value>
        [FormerlySerializedAs("m_Speed")]
        public float speed = 12f;

        /// <value>Property <c>turnSpeed</c> represents how fast the tank turns in degrees per second.</value>
        [FormerlySerializedAs("m_TurnSpeed")]
        public float turnSpeed = 180f;

        /// <value>Property <c>movementAudio</c> is a reference to the audio source used to play engine sounds.</value>
        [FormerlySerializedAs("m_MovementAudio")]
        public AudioSource movementAudio;

        /// <value>Property <c>engineIdling</c> represents the audio to play when the tank isn't moving.</value>
        [FormerlySerializedAs("m_EngineIdling")]
        public AudioClip engineIdling;

        /// <value>Property <c>engineDriving</c> represents the audio to play when the tank is moving.</value>
        [FormerlySerializedAs("m_EngineDriving")]
        public AudioClip engineDriving;

        /// <value>Property <c>pitchRange</c> represents the amount by which the pitch of the engine noises can vary.</value>
        [FormerlySerializedAs("m_PitchRange")]
        public float pitchRange = 0.2f;

        /// <value>Property <c>m_Rigidbody</c> represents the rigidbody component of the tank.</value>
        private Rigidbody m_Rigidbody;

        /// <value>Property <c>m_MovementInputValue</c> represents the current value of the movement input.</value>
        private float m_MovementInputValue;

        /// <value>Property <c>m_TurnInputValue</c> represents the current value of the turn input.</value>
        private float m_TurnInputValue;

        /// <value>Property <c>m_OriginalPitch</c> represents the pitch of the audio source at the start of the scene.</value>
        private float m_OriginalPitch;

        /// <value>Property <c>m_particleSystems</c> represents the references to all the particles systems used by the Tanks.</value>
        private ParticleSystem[] m_ParticleSystems;

        /// <value>Property <c>m_MoveInput</c> represents the current value of the move input.</value>
        private Vector2 m_MoveInput;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            // When the tank is turned on, make sure it's not kinematic
            m_Rigidbody.isKinematic = false;

            // Also reset the input values
            m_MovementInputValue = 0f;
            m_TurnInputValue = 0f;

            // We grab all the Particle systems child of that Tank to be able to Stop/Play them on Deactivate/Activate
            // It is needed because we move the Tank when spawning it, and if the Particle System is playing while we do that
            // it "think" it move from (0,0,0) to the spawn point, creating a huge trail of smoke
            m_ParticleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (var p in m_ParticleSystems)
            {
                p.Play();
            }
        }

        /// <summary>
        /// Method <c>OnDisable</c> is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            // When the tank is turned off, set it to kinematic so it stops moving
            m_Rigidbody.isKinematic = true;

            // Stop all particle system so it "reset" it's position to the actual one instead of thinking we moved when spawning
            foreach (var p in m_ParticleSystems)
            {
                p.Stop();
            }
        }

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Store the original pitch of the audio source
            m_OriginalPitch = movementAudio.pitch;
        }

        /// <summary>
        /// Method <c>Update</c> is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void Update()
        {
            EngineAudio();
        }

        /// <summary>
        /// Method <c>FixedUpdate</c> is called every fixed frame-rate frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void FixedUpdate()
        {
            if (!isLocalPlayer)
                return;
            Move();
            Turn();
        }

        /// <summary>
        /// Method <c>OnMove</c> is called when the move input is changed.
        /// </summary>
        private void OnMove(InputValue value)
        {
            m_MoveInput = value.Get<Vector2>();
        }

        /// <summary>
        /// Method <c>Move</c> is used to move the tank.
        /// </summary>
        private void Move()
        {
            
            // Assign the input value to the appropriate direction
            m_MovementInputValue = m_MoveInput.y;
            
            // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames
            var movement = transform.forward * (m_MovementInputValue * speed * Time.deltaTime);

            // Apply this movement to the rigidbody's position
            m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
        }

        /// <summary>
        /// Method <c>Turn</c> is used to turn the tank.
        /// </summary>
        private void Turn()
        {
            // Assign the input value to the appropriate direction
            m_TurnInputValue = m_MoveInput.x;

            // Determine the number of degrees to be turned based on the input, speed and time between frames
            var turn = m_TurnInputValue * turnSpeed * Time.deltaTime;

            // Make this into a rotation in the y axis.
            var turnRotation = Quaternion.Euler(0f, turn, 0f);

            // Apply this rotation to the rigidbody's rotation
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
        }

        /// <summary>
        /// Method <c>EngineAudio</c> is used to play different audio clips based on the tank's movement and whether or not the tank is stationary.
        /// </summary>
        private void EngineAudio()
        {
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
            {
                // ... and if the audio source is currently playing the driving clip...
                if (movementAudio.clip != engineDriving) return;
                // ... change the clip to idling and play it.
                movementAudio.clip = engineIdling;
                movementAudio.pitch = Random.Range(m_OriginalPitch - pitchRange, m_OriginalPitch + pitchRange);
                movementAudio.Play();
            }
            else
            {
                // Otherwise if the tank is moving and if the idling clip is currently playing...
                if (movementAudio.clip != engineIdling) return;
                // ... change the clip to driving and play.
                movementAudio.clip = engineDriving;
                movementAudio.pitch = Random.Range(m_OriginalPitch - pitchRange, m_OriginalPitch + pitchRange);
                movementAudio.Play();
            }
        }
    }
}
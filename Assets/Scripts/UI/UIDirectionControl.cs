using UnityEngine;

namespace PEC2.UI
{
    /// <summary>
    /// Class <c>UIDirectionControl</c> is used to make sure world space UI elements such as the health bar face the correct direction.
    /// </summary>
    public class UIDirectionControl : MonoBehaviour
    {
        /// <value>Property <c>useRelativeRotation</c> represents whether the relative rotation should be used for this gameobject.</value>
        public bool useRelativeRotation = true;

        /// <value>Property <c>m_RelativeRotation</c> represents the local rotation at the start of the scene.</value>
        private Quaternion m_RelativeRotation;

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            m_RelativeRotation = transform.parent.localRotation;
        }

        /// <summary>
        /// Method <c>Update</c> is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void Update()
        {
            if (useRelativeRotation)
                transform.rotation = m_RelativeRotation;
        }
    }
}
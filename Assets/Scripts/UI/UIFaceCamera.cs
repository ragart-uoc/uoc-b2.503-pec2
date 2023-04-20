using UnityEngine;

namespace PEC2.UI
{
    /// <summary>
    /// Class <c>UIFaceCamera</c> is used to make a UI element face the camera.
    /// </summary>
    public class UIFaceCamera : MonoBehaviour
    {
        /// <value>Property <c>m_MainCameraTransform</c> represents the transform of the main camera.</value>
        private Transform m_MainCameraTransform;

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            m_MainCameraTransform = Camera.main.transform;
        }
        
        /// <summary>
        /// Method <c>LateUpdate</c> is called every frame, if the Behaviour is enabled.
        /// </summary>
        private void LateUpdate()
        {
            var cameraRotation = m_MainCameraTransform.rotation;
            transform.LookAt(transform.position + cameraRotation * Vector3.forward, 
                cameraRotation * Vector3.up);
        }
    }
}

using System;
using System.Collections;
using UnityEngine;
using Mirror;
using TMPro;

namespace PEC2.Managers
{
    /// <summary>
    /// Class <c>UIManager</c> controls the UI of the game.
    /// </summary>
    public class UIManager : NetworkBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static UIManager _instance;

        /// <value>Property <c>messageText</c> is a reference to the overlay Text to display winning text, etc.</value>
        public TextMeshProUGUI messageText;

        /// <value>Property <c>eventContainer</c> is a reference to the event container.</value>
        public GameObject eventContainer;
        
        /// <value>Property <c>eventPrefab</c> is a reference to the event prefab.</value>
        public GameObject eventPrefab;
        
        /// <summary>
        /// Method <c>WriteMessage</c> is used to write a message to the screen.
        /// </summary>
        /// <param name="message">The message to write to the screen</param>
        public void WriteMessage(string message)
        {
            if (!isServer)
                return;
            messageText.text = message;
            RpcWriteMessage(message);
        }
        
        /// <summary>
        /// Method <c>RpcWriteMessage</c> is used to write a message to the screen on the client.
        /// </summary>
        /// <param name="message">The message to write to the screen</param>
        [ClientRpc]
        private void RpcWriteMessage(string message)
        {
            messageText.text = message;
        }
        
        /// <summary>
        /// Method <b>BlinkingMessage</b> is used to display a message on the screen for a specified duration.
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="duration">The duration to display the message for</param>
        public IEnumerator BlinkingMessage(string message, float duration = 0)
        {
            var time = 0f;
            // If duration equals 0, then the message will blink forever
            while (duration == 0 || time < duration)
            {
                WriteMessage(message);
                yield return new WaitForSeconds(.5f);
                WriteMessage(String.Empty);
                yield return new WaitForSeconds(.5f);
                time += 1f;
            }
        }
        
        /// <summary>
        /// Method <c>SendEvent</c> is used to send an event to the event container.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="duration">The duration to display the message for</param>
        public void SendEvent(string message, float duration)
        {
            if (!isServer)
                return;
            if (isServerOnly)
                StartCoroutine(OnSendEvent(message, duration));
            RpcSendEvent(message, duration);
        }
        
        /// <summary>
        /// Method <c>RpcSendEvent</c> is used to send an event to the event container on the client.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="duration">The duration to display the message for</param>
        [ClientRpc]
        private void RpcSendEvent(string message, float duration)
        {
            StartCoroutine(OnSendEvent(message, duration));
        }

        /// <summary>
        /// Method <c>OnSendEvent</c> is used to send an event to the event container.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="duration">The duration to display the message for</param>
        private IEnumerator OnSendEvent(string message, float duration)
        {
            var newEvent = Instantiate(eventPrefab, eventContainer.transform);
            newEvent.GetComponent<TextMeshProUGUI>().text = message;
            yield return new WaitForSeconds(duration);
            Destroy(newEvent);
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace PEC2.Managers
{
    /// <summary>
    /// Class <c>StartupManager</c> controls the startup of the game.
    /// </summary>
    public class StartupManager : MonoBehaviour
    {
        /// <value>Property <c>screenText</c> represents the UI element containing the opening text.</value>
        public TextMeshProUGUI screenText;
        
        /// <summary>
        /// Method <c>Start</c> is called before the first frame update.
        /// </summary>
        private IEnumerator Start()
        {
            screenText.canvasRenderer.SetAlpha(0.0f);
            screenText.CrossFadeAlpha(1.0f, 1.5f, false);
            yield return new WaitForSeconds(2.5f);
            screenText.CrossFadeAlpha(0.0f, 1.5f, false);
            yield return new WaitForSeconds(1.5f);
            // Load the menu scene
            SceneManager.LoadScene("MainMenu");
        }
    }
}

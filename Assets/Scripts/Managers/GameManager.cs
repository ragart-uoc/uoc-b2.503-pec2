using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using PEC2.Entities;
using UnityEngine.SceneManagement;

namespace PEC2.Managers
{
    /// <summary>
    /// Class <c>GameManager</c> controls the flow of the game.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static GameManager _instance;

        /// <value>Property <c>roundsToWin</c> represents the number of rounds a single player has to win to win the game.</value>
        public int roundsToWin = 5;

        /// <value>Property <c>startDelay</c> represents the delay between the start of RoundStarting and RoundPlaying phases.</value>
        public float startDelay = 3f;

        /// <value>Property <c>endDelay</c> represents the delay between the end of RoundPlaying and RoundEnding phases.</value>
        public float endDelay = 3f;
        
        /// <value>Property <c>controlsEnabled</c> is used to check if the controls are enabled.</value>
        [SyncVar(hook = "OnChangeControlsEnabled")]
        public bool controlsEnabled;
        
        /// <value>Property <c>uiManager</c> is a reference to the UI manager.</value>
        public UIManager uiManager;
        
        /// <value>Property <c>npcManager</c> is a reference to the NPC manager.</value>
        public NpcManager npcManager;
        
        /// <value>Property <c>m_MessageCoroutine</c> is used to start and stop the message coroutine.</value>
        private Coroutine m_MessageCoroutine;

        /// <value>Property <c>m_GameLoopCoroutine</c> is used to start and stop the game loop.</value>
        private Coroutine m_GameLoopCoroutine;

        /// <value>Property <c>m_RoundNumber</c> represents which round the game is currently on.</value>
        private int m_RoundNumber;

        /// <value>Property <c>m_StartWait</c> is used to have a delay whilst the round starts.</value>
        private WaitForSeconds m_StartWait;

        /// <value>Property <c>m_EndWait</c> is used to have a delay whilst the round or game ends.</value>
        private WaitForSeconds m_EndWait;
        
        /// <value>Property <c>m_GameStarted</c> is used to check if the game has started.</value>
        private bool m_GameStarted;

        /// <value>Property <c>m_RoundWinner</c> is a reference to the winner of the current round. Used to make an announcement of who won.</value>
        private Tank m_RoundWinner;

        /// <value>Property <c>m_GameWinner</c> is a reference to the winner of the game. Used to make an announcement of who won.</value>
        private Tank m_GameWinner;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
        }

        /// <summary>
        /// Method <c>FixedUpdate</c> is called every fixed framerate frame.
        /// </summary>
        private void FixedUpdate()
        {
            if (!isServer)
                return;
            
            // if the game has started and there are no active connections, reset the game
            if (m_GameStarted && CheckActiveConnections() == 0)
                ResetGame();
        }
        
        /// <summary>
        /// Method <c>OnStartServer</c> is called when the server starts.
        /// </summary>
        public override void OnStartServer()
        {
            if (!isServer)
                return;

            // Create the delays so they only have to be made once
            m_StartWait = new WaitForSeconds(startDelay);
            m_EndWait = new WaitForSeconds(endDelay);

            // Reset the game
            ResetGame();
            
            // Start the game loop
            m_GameLoopCoroutine = StartCoroutine(GameLoop());
        }

        /// <summary>
        /// Method <c>OnStopServer</c> is called when the server stops.
        /// </summary>
        public override void OnStopServer()
        {
            if (!isServer)
                return;
            
            // Reset the game
            ResetGame();
        }

        /// <summary>
        /// Method <c>ResetGame</c> is used to reset all the game data and start the game from the beginning.
        /// </summary>
        private void ResetGame()
        {
            if (!isServer)
                return;
            
            // Reset the game started flag
            m_GameStarted = false;
            
            // Reset rounds and winners
            m_RoundNumber = 0;
            m_RoundWinner = null;
            m_GameWinner = null;
            
            // Destroy all enemy tanks
            NpcManager.DeSpawn();
            
            // Stop all active coroutines
            if (m_GameLoopCoroutine != null)
                StopCoroutine(m_GameLoopCoroutine);
            if (m_MessageCoroutine != null)
                StopCoroutine(m_MessageCoroutine);
            
            // Reset the message text
            uiManager.WriteMessage(String.Empty);
        }

        /// <summary>
        /// Method <c>GameLoop</c> is the coroutine that controls the flow of the game. It's called from start and will run each phase of the game one after another
        /// </summary>
        private IEnumerator GameLoop()
        {
            if (!isServer)
                yield break;

            // Stop tanks from moving
            controlsEnabled = false;
            
            // Start off by running the 'GameStarting' coroutine but don't return until it's finished
            yield return StartCoroutine(GameStarting());
            
            // Proceed to run the 'RoundStarting' coroutine but don't return until it's finished
            yield return StartCoroutine(RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found
            if (m_GameWinner != null)
            {
                m_GameStarted = false;
                yield return new WaitForSeconds(3f);
                NetworkServer.DisconnectAll();
                SceneManager.LoadScene("MainMenu");
            }
            else
                m_GameLoopCoroutine = StartCoroutine(GameLoop());
        }

        /// <summary>
        /// Method <c>GameStarting</c> is called before the first round to set up the game.
        /// </summary>
        /// <returns></returns>
        private IEnumerator GameStarting()
        {
            if (!isServer)
                yield break;

            // Do not start the game until at least two players are connected
            if (m_GameStarted)
                yield break;

            // Stop tanks from moving
            controlsEnabled = false;
            
            // Print waiting for players message
            m_MessageCoroutine = StartCoroutine(uiManager.BlinkingMessage("Waiting for players..."));
            
            // Do not start the game until at least two players are connected
            while (CheckActiveConnections() < 2)
            {
                yield return null;
            }
            StopCoroutine(m_MessageCoroutine);
            m_GameStarted = true;
        }

        /// <summary>
        /// Method <c>RoundStarting</c> is called before each round to set up the round.
        /// </summary>
        private IEnumerator RoundStarting()
        {
            if (!isServer)
                yield break;

            // As soon as the round starts reset the tanks and make sure they can't move
            npcManager.ReSpawn();
            if (m_RoundNumber > 0) RespawnTanks();

            // Increment the round number and display text showing the players what round it is
            m_RoundNumber++;
            uiManager.WriteMessage("ROUND " + m_RoundNumber);

            // Wait for the specified length of time until yielding control back to the game loop
            yield return m_StartWait;
        }

        /// <summary>
        /// Method <c>RoundPlaying</c> is called from GameLoop each time a round is playing.
        /// </summary>
        private IEnumerator RoundPlaying()
        {
            if (!isServer)
                yield break;

            // As soon as the round begins playing let the players control the tanks
            controlsEnabled = true;

            // Clear the text from the screen
            uiManager.WriteMessage(string.Empty);

            // While there is not one tank left...
            while (!RoundIsOver())
            {
                // ... return on the next frame
                yield return null;
            }
        }

        /// <summary>
        /// Method <c>RoundEnding</c> is called after each round to clean up things and determine if there is a winner of the game.
        /// </summary>
        private IEnumerator RoundEnding()
        {
            if (!isServer)
                yield break;

            // Stop tanks from moving
            controlsEnabled = false;

            // Clear the winner from the previous round
            m_RoundWinner = null;

            // See if there is a winner now the round is over
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score
            if (m_RoundWinner != null)
                m_RoundWinner.wins++;

            // Now the winner's score has been incremented, see if someone has one the game
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it
            var message = EndMessage();
            uiManager.WriteMessage(message);

            // Wait for the specified length of time until yielding control back to the game loop
            yield return m_EndWait;
        }

        /// <summary>
        /// Method <c>RoundIsOver</c> is called to determine if the round is over.
        /// </summary>
        private bool RoundIsOver()
        {
            var activePlayers = GetAlivePlayers();
            var activeEnemies = GetAliveEnemies();
            return CheckActiveConnections() == 1
                   || activePlayers.Count == 0
                   || (activePlayers.Count == 1 && activeEnemies.Count == 0);
        }

        /// <summary>
        /// Method <c>GetRoundWinner</c> is called at the end of a round to determine which tank (if any) won the round.
        /// </summary>
        private Tank GetRoundWinner()
        {
            var activePlayers = GetAlivePlayers();
            return activePlayers.FirstOrDefault()?.GetComponent<Tank>();
        }

        /// <summary>
        /// Method <c>GetGameWinner</c> is called at the end of a round to determine which tank (if any) won the game.
        /// </summary>
        private Tank GetGameWinner()
        {
            var allPlayers = GetAllPlayers();
            var activePlayers = GetAlivePlayers();
            return CheckActiveConnections() == 1
                ? allPlayers.FirstOrDefault()?.GetComponent<Tank>()
                : activePlayers.FirstOrDefault(p => p.GetComponent<Tank>().wins == roundsToWin)?
                    .GetComponent<Tank>();
        }

        /// <summary>
        /// Method <c>EndMessage</c> is used to construct a string message to display at the end of each round.
        /// </summary>
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw
            var message = "DRAW!";

            // If there is a winner then change the message to reflect that
            if (m_RoundWinner != null)
                message = m_RoundWinner.coloredPlayerName + " WINS THE ROUND!";

            // Add some line breaks after the initial message
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message
            var players = GetAllPlayers();
            foreach (var player in players)
            {
                var playerTank = player.GetComponent<Tank>();
                message += playerTank.coloredPlayerName + ": " + playerTank.wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that
            if (m_GameWinner != null)
                message = m_GameWinner.coloredPlayerName + " WINS THE GAME!";

            return message;
        }
        
        /// <summary>
        /// Method <b>ResetAllTanks</b> is used to reset all tanks in the game.
        /// </summary>
        private void RespawnTanks()
        {
            // Get all spawned players with tag Player
            var players = GetAllPlayers();
            foreach (var player in players)
            {
                player.GetComponent<Tank>().Respawn();
            }
        }
        
        /// <summary>
        /// Method <c>OnChangeControlsEnabled</c> is called when the controlsEnabled property is changed.
        /// </summary>
        /// <param name="oldControlsEnabled">The old value of the controlsEnabled property</param>
        /// <param name="newControlsEnabled">The new value of the controlsEnabled property</param>
        public void OnChangeControlsEnabled(bool oldControlsEnabled, bool newControlsEnabled)
        {
            var players = GetAllPlayers();
            foreach (var player in players)
            {
                player.GetComponent<Tank>().controlsEnabled = newControlsEnabled;
            }
        }

        /// <summary>
        /// Method <c>CheckActiveConnections</c> is used to check how many active connections there are.
        /// </summary>
        /// <returns>The number of active connections</returns>
        private int CheckActiveConnections()
        {
            return NetworkServer.connections.Count;
        }

        /// <summary>
        /// Method <c>GetAllPlayers</c> is used to get all the players in the game.
        /// </summary>
        /// <returns>A list of all the players</returns>
        private List<GameObject> GetAllPlayers()
        {
            // Get all connections with tag Player
            var players = NetworkServer.connections.Values
                .Where(c => c != null)
                .Select(c => c.identity.gameObject)
                .Where(p => p.CompareTag("Player"))
                .ToList();
            return players;
        }

        /// <summary>
        /// Method <c>GetAlivePlayers</c> is used to get all the players that are still alive.
        /// </summary>
        /// <returns>A list of all the players that are still alive</returns>
        private List<GameObject> GetAlivePlayers()
        {
            var players = NetworkServer.connections.Values
                .Where(c => c != null)
                .Select(c => c.identity.gameObject)
                .Where(p => p.CompareTag("Player") && p.activeSelf)
                .ToList();
            return players;
        }
        
        /// <summary>
        /// Method <c>GetAliveEnemies</c> is used to get all the enemies that are still alive.
        /// </summary>
        /// <returns>A list of all the enemies that are still alive</returns>
        private List<GameObject> GetAliveEnemies()
        {
            var players = NetworkServer.spawned.Values
                .Where(c => c != null)
                .Select(c => c.gameObject)
                .Where(p => p.CompareTag("Enemy"))
                .ToList();
            return players;
        }
    }
}
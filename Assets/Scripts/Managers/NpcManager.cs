using UnityEngine;
using Mirror;

using Random = UnityEngine.Random;

namespace PEC2.Managers
{
    /// <summary>
    /// Method <c>NpcManager</c> is used to spawn enemies in the scene.
    /// </summary>
    public class NpcManager : NetworkBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static NpcManager _instance;

        /// <value>Property <c>enemyPrefab</c> represents the prefab of the enemy.</value>
        public GameObject enemyPrefab;

        /// <value>Property <c>numberOfEnemies</c> represents the number of enemies to spawn.</value>
        public int numberOfEnemies = 4;

        /// <value>Property <c>cameraManager</c> is used to add the tank to the group camera.</value>
        public CameraManager cameraManager;
        
        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        /// <summary>
        /// Method <c>Spawn</c> is used to spawn enemies in the scene.
        /// </summary>
        private void Spawn() {
            for (var i = 0; i < numberOfEnemies; i++) {
                var spawnPosition = new Vector3(
                    Random.Range(-40.0f, 40.0f),
                    0.0f,
                    Random.Range(-40.0f, 40.0f));
 
                var spawnRotation = Quaternion.Euler(
                    0.0f,
                    Random.Range(0, 180),
                    0.0f);
 
                var enemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);
                NetworkServer.Spawn(enemy);
                
                // Refresh the group camera targets
                cameraManager.UpdateTargetGroup();
            }
        }
        
        /// <summary>
        /// Method <c>DeSpawn</c> is used to despawn enemies in the scene.
        /// </summary>
        public static void DeSpawn()
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies) {
                NetworkServer.Destroy(enemy);
            }
        }

        /// <summary>
        /// Method <c>ReSpawn</c> is used to respawn enemies in the scene.
        /// </summary>
        public void ReSpawn()
        {
            DeSpawn();
            Spawn();
        }
    }
}

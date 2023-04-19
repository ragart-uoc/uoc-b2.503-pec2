using UnityEngine;
using Mirror;

namespace PEC2.Managers
{
    /// <summary>
    /// Method <c>EnemySpawnManager</c> is used to spawn enemies in the scene.
    /// </summary>
    public class EnemySpawnManager : NetworkBehaviour
    {
        /// <value>Property <c>enemyPrefab</c> represents the prefab of the enemy.</value>
        public GameObject enemyPrefab;
        
        /// <value>Property <c>numberOfEnemies</c> represents the number of enemies to spawn.</value>
        public int numberOfEnemies = 4;
 
        /// <summary>
        /// Method <c>OnStartServer</c> is invoked for NetworkBehaviour objects when they become active on the server.
        /// </summary>
        public override void OnStartServer() {
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
            }
        }
    }
}

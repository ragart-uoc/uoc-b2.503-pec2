using UnityEngine;
using Mirror.Discovery;

namespace PEC2.Entities
{
    /// <summary>
    /// Class <c>DiscoveredGame</c> represents a discovered game.
    /// </summary>
    public class DiscoveredGame
    {
        /// <summary>
        /// Property <c>address</c> represents the address of the discovered game.
        /// </summary>
        public ServerResponse address {get; set;}
        
        /// <summary>
        /// Property <c>banner</c> represents the banner of the discovered game.
        /// </summary>
        public GameObject banner {get; set;}
    }
}

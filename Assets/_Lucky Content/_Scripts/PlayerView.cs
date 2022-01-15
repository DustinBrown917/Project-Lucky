using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// Base view class used to reflect the data of a player object.
    /// </summary>
    public abstract class PlayerView : MonoBehaviour
    {
        /// <summary>
        /// Hidden reference to the player reflected in this class.
        /// </summary>
        protected Player m_targetPlayer;

        /// <summary>
        /// The player reflected by this class.
        /// </summary>
        public Player TargetPlayer => m_targetPlayer;

        protected virtual void OnDestroy()
        {
            if(m_targetPlayer != null) {
                DisconnectEvents();
            }
        }

        /// <summary>
        /// Set the target player of this view.
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer(Player player)
        {
            if (m_targetPlayer != null) {
                DisconnectEvents();
            }

            m_targetPlayer = player;

            if (m_targetPlayer != null) {
                ConnectEvents();
                HandleNewTarget();
            } else {
                HandleNullTarget();
            }
        }

        /// <summary>
        /// Called when a new not-null player is assigned as target player.
        /// Used to hook up any events necessary with the new player.
        /// </summary>
        protected virtual void ConnectEvents() { }

        /// <summary>
        /// Called just before a new player is set and the current player is not null.
        /// Used to disconnect any events that were connected in ConnectEvents()
        /// </summary>
        protected virtual void DisconnectEvents() { }


        /// <summary>
        /// Called when m_targetPlayer is set to null.
        /// Any initialization for a null target player should be done here.
        /// </summary>
        protected virtual void HandleNullTarget() { }

        /// <summary>
        /// Called when m_targetPlayer is set to a not-null value.
        /// Any initialization for a new target player should be done here.
        /// </summary>
        protected virtual void HandleNewTarget() { }
    } 
}

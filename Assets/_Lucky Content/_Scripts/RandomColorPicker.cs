using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace Lucky
{
    /// <summary>
    /// Picks a random color and shares that color with clients.
    /// </summary>
    public class RandomColorPicker : MonoBehaviourPun
    {

        /*vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv*/
        #region ========================================== Subclasses ==================================================
        /*vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv*/

        /// <summary>
        /// Event wrapper class for selecting a random color.
        /// </summary>
        [Serializable]
        public class ColorPickedEvent : UnityEvent<BettableColors> { }

        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/
        #endregion ===================================== End Subclasses ================================================
        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/

        /// <summary>
        /// Colors that can possibly be selected.
        /// </summary>
        private BettableColors[] m_bettableColors;

        /// <summary>
        /// Invoked when a random color is picked.
        /// </summary>
        public ColorPickedEvent onColorPicked;

        private void Awake()
        {
            //Get all the bettable colors from the BettableColors enum.
            m_bettableColors = (BettableColors[])Enum.GetValues(typeof(BettableColors));
        }

        /// <summary>
        /// Picks a random color from the color options.
        /// </summary>
        public void PickRandomColor()
        {
            //We only want this to execute on one machine. Would be ideal if it could be isolated to serverside. Alas, I do not have Photon Cloud.
            if (photonView.Owner.IsMasterClient) {
                //Pick a color
                //Good ol' Xorshift128. Plenty fast enough to meet the needs of this project.
                //Xorshift1024 could be an alternative if more uniform/less repetative generation space is needed.
                //But in that case, you'd probably want to pregen a set of numbers at startup that is expected to meet the needs of the project.
                BettableColors color = m_bettableColors[UnityEngine.Random.Range(0, m_bettableColors.Length)];

                //Notify clients of the selected color.
                photonView.RPC(nameof(RPCNotifyColorPicked), RpcTarget.All, color);
            }

        }

        /// <summary>
        /// Notify locally of a color being selected.
        /// </summary>
        /// <param name="color"></param>
        [PunRPC]
        private void RPCNotifyColorPicked(BettableColors color)
        {
            onColorPicked?.Invoke(color);
        }
    } 
}

using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// The object representing all data that is relevant to a single player of Project-Lucky.
    /// Not designed to replace the Photon Player class, but rather work along side it.
    /// </summary>
    public class Player : MonoBehaviourPunCallbacks
    {

        /*vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv*/
        #region ========================================== Statics ==================================================
        /*vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv*/

        private static Player m_local;
        public static Player Local
        {
            get => m_local;
            private set {
                if (m_local != value) {
                    Player oldPlayer = m_local;
                    m_local = value;
                    OnLocalPlayerChanged?.Invoke(oldPlayer, m_local);
                }
            }
        }

        private static Player m_other;
        public static Player Other
        {
            get => m_other;
            private set {
                if (m_other != value) {
                    Player oldPlayer = m_other;
                    m_other = value;
                    OnOtherPlayerChanged?.Invoke(oldPlayer, m_other);
                }
            }
        }

        /// <summary>
        /// Raised when the local player changes.
        /// [Old Value, New Value]
        /// </summary>
        public static event Action<Player, Player> OnLocalPlayerChanged;
        /// <summary>
        /// Raised when the other player changes.
        /// [Old Value, New Value]
        /// </summary>
        public static event Action<Player, Player> OnOtherPlayerChanged;

        /// <summary>
        /// Clears static references to players.
        /// </summary>
        public static void ClearPlayerReferences()
        {
            Local = null;
            Other = null;
        }

        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/
        #endregion ===================================== End Statics ================================================
        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/

        [Tooltip("How many chips should the player start with? When player chips reach 0, they will be reset to this number.")]
        [SerializeField] private int m_startingChipCount = 100;

        /// <summary>
        /// How many chips the player currently has.
        /// </summary>
        public int CurrentChipCount { get; private set; }

        /// <summary>
        /// What color the player is current betting on.
        /// </summary>
        public BettableColors CurrentBetColor { get; private set; }

        /// <summary>
        /// The current value of the bet the player has placed.
        /// Note: This is not subtracted from CurrentChipCount until a bet is lost.
        /// </summary>
        public int CurrentBetValue { get; private set; }

        /// <summary>
        /// Has the player locked their bet in yet?
        /// </summary>
        public bool BetLockedIn { get; private set; }

        /// <summary>
        /// Raised when the player's CurrentChipCount changes.
        /// </summary>
        public event Action<int> OnCurrentChipCountChanged;

        /// <summary>
        /// Raised when the player's CurrentBetValue changes.
        /// </summary>
        public event Action<int> OnCurrentBetValueChanged;

        /// <summary>
        /// Raised when the player's CurrentBetColor changes.
        /// </summary>
        public event Action<BettableColors> OnCurrentBetColourChanged;

        /// <summary>
        /// Raised when the player's BetLockedIn changes.
        /// </summary>
        public event Action<bool> OnBetLockedInChanged;

        #region MonoBehaviour Callbacks
        private void Awake()
        {
            CurrentChipCount = m_startingChipCount;
        }

        private void Start()
        {
            //Register as either the local player or the other player.
            if (photonView.IsMine) {
                Local = this;
            } else {
                Other = this;
            }

        }

        private void OnDestroy()
        {
            //Deregister
            if (Other == this) {
                Other = null;
            } else if (Local == this) {
                Local = null;
            }
        }
        #endregion /MonoBehaviour Callbacks

        #region PUN Callbacks
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            //Update new player with this player's values.
            photonView.RPC(nameof(RPCSetCurrentBetValue), newPlayer, CurrentBetValue);
            photonView.RPC(nameof(RPCSetCurrentBetColor), newPlayer, CurrentBetColor);
            photonView.RPC(nameof(RPCSetCurrentChipCount), newPlayer, CurrentChipCount);
            photonView.RPC(nameof(RPCSetBetLockedIn), newPlayer, BetLockedIn);
        }
        #endregion /PUN Callbacks



        #region CurrentBetValue setting
        /// <summary>
        /// Set the value of CurrentBetValue, notifying all clients of the value changed.
        /// </summary>
        /// <param name="value"></param>
        public void SetCurrentBetValue(int value)
        {
            //If this is not the local player, return.
            if (!photonView.IsMine) return;

            //clamp to between 0 and the player's current chip count.
            value = Mathf.Clamp(value, 0, CurrentChipCount);

            //Only send RPC if CurrentBatValue is actually going to change.
            if (value != CurrentBetValue) {

                //Send RPC with the given value 
                photonView.RPC(nameof(RPCSetCurrentBetValue), RpcTarget.All, value);
            }

        }



        /// <summary>
        /// Sets the value of CurrentBetValue and triggers local notification of value change.
        /// </summary>
        /// <param name="value"></param>
        [PunRPC]
        private void RPCSetCurrentBetValue(int value)
        {
            CurrentBetValue = value;

            //Notify that CurrentBetValue has changed.
            OnCurrentBetValueChanged?.Invoke(CurrentBetValue);
        }
        #endregion /CurrentBetValue setting



        #region CurrentBetColor setting
        /// <summary>
        /// Set the value of CurrentBetColor, notifying all clients if the value changed.
        /// </summary>
        /// <param name="color"></param>
        public void SetCurrentBetColor(BettableColors color)
        {
            //If this is not the local player, return.
            if (!photonView.IsMine) return;

            //If color hasn't actually changed, don't send the RPC.
            if (color != CurrentBetColor) {

                //Send RPC to all clients to notify of change.
                photonView.RPC(nameof(RPCSetCurrentBetColor), RpcTarget.All, color);
            }
        }



        /// <summary>
        /// Sets the value of of CurrentBetColor and triggers local notification of value change.
        /// </summary>
        /// <param name="color"></param>
        [PunRPC]
        private void RPCSetCurrentBetColor(BettableColors color)
        {
            CurrentBetColor = color;

            //Notify of value change.
            OnCurrentBetColourChanged?.Invoke(CurrentBetColor);
        }
        #endregion /CurrentBetColor setting



        #region CurrentChipCount setting
        /// <summary>
        /// Set the value of CurrentChipCount, notifying all clients if the value changed.
        /// </summary>
        /// <param name="value"></param>
        public void SetCurrentChipCount(int value)
        {
            //If this is not the local player, return.
            if (!photonView.IsMine) return;

            //If color hasn't actually changed, don't send the RPC.
            if (value != CurrentChipCount) {

                //Send RPC to all clients to notify of change.
                photonView.RPC(nameof(RPCSetCurrentChipCount), RpcTarget.All, value);
            }
        }



        /// <summary>
        /// Sets the value of of CurrentBetColor and triggers local notification of value change.
        /// </summary>
        /// <param name="value"></param>
        [PunRPC]
        private void RPCSetCurrentChipCount(int value)
        {
            //Automatically roll over to 100 if value is less than 100.
            if (value <= 0) value = m_startingChipCount;

            CurrentChipCount = value;

            //Notify of value change.
            OnCurrentChipCountChanged?.Invoke(CurrentChipCount);
        }
        #endregion /CurrentChipCount setting



        #region BetLockedIn setting
        /// <summary>
        /// Set the value of BetLockedIn, notifying all clients if the value changed.
        /// </summary>
        /// <param name="value"></param>
        public void SetBetLockedIn(bool value)
        {
            //If this is not the local player, return.
            if (!photonView.IsMine) return;

            //If value hasn't actually changed, don't send the RPC.
            if (value != BetLockedIn) {

                //Send RPC to all clients to notify of change.
                photonView.RPC(nameof(RPCSetBetLockedIn), RpcTarget.All, value);
            }
        }



        /// <summary>
        /// Sets the value of of BetLockedIn and triggers local notification of value change.
        /// </summary>
        /// <param name="color"></param>
        [PunRPC]
        private void RPCSetBetLockedIn(bool value)
        {
            BetLockedIn = value;

            //Notify of value change.
            OnBetLockedInChanged?.Invoke(BetLockedIn);
        }
        #endregion /BetLockedIn setting
    }
}

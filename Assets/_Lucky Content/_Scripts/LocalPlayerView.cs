using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// View class to reflect the data of the local player.
    /// </summary>
    public class LocalPlayerView : PlayerView
    {
        [Tooltip("The text representing the player's current chip count.")]
        [SerializeField] private TextMeshProUGUI m_currentChipsText;

        [Tooltip("The text representing the player's current bet.")]
        [SerializeField] private TextMeshProUGUI m_currentBetText;

        private void Awake()
        {
            //Listen for any changes to the local player.
            Player.OnLocalPlayerChanged += HandleLocalPlayerChanged;
        }

        private void OnEnable()
        {
            //When enabled, get the local player and assign it as this view's target.
            SetPlayer(Player.Local);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //Cleaning up static event listener.
            Player.OnLocalPlayerChanged -= HandleLocalPlayerChanged;
        }

        /// <summary>
        /// Handle a change to the local player.
        /// </summary>
        /// <param name="oldPlayer"></param>
        /// <param name="newPlayer"></param>
        private void HandleLocalPlayerChanged(Player oldPlayer, Player newPlayer)
        {
            SetPlayer(newPlayer);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void ConnectEvents()
        {
            m_targetPlayer.OnCurrentBetColourChanged += HandleCurrentBetColourChanged;
            m_targetPlayer.OnCurrentBetValueChanged += HandleCurrentBetValueChanged;
            m_targetPlayer.OnCurrentChipCountChanged += HandleCurrentChipCountChanged;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void DisconnectEvents()
        {
            m_targetPlayer.OnCurrentBetColourChanged -= HandleCurrentBetColourChanged;
            m_targetPlayer.OnCurrentBetValueChanged -= HandleCurrentBetValueChanged;
            m_targetPlayer.OnCurrentChipCountChanged -= HandleCurrentChipCountChanged;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void HandleNullTarget()
        {
            m_currentBetText.text = "";
            m_currentChipsText.text = "";
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void HandleNewTarget()
        {
            UpdateCurrentBetText();
            UpdateCurrentChipCountText();
        }

        //Think these are fairly self documenting.
        private void HandleCurrentChipCountChanged(int value) => UpdateCurrentChipCountText();
        private void HandleCurrentBetColourChanged(BettableColors color) => UpdateCurrentBetText();
        private void HandleCurrentBetValueChanged(int value) => UpdateCurrentBetText();

        /// <summary>
        /// Updates the current bet text to read the current value and color of the bet.
        /// </summary>
        public void UpdateCurrentBetText()
        {
            //If the player's current bet is 0, we consider that as "no bet placed"
            if(m_targetPlayer.CurrentBetValue <= 0) {
                m_currentBetText.text = "No Bet Placed";
            //Otherwise, we show the amount and color they have bet.
            } else {
                switch (m_targetPlayer.CurrentBetColor) {
                    case BettableColors.Red:
                        m_currentBetText.text = $"{m_targetPlayer.CurrentBetValue} <color=red>RED</color>";
                        break;
                    case BettableColors.Green:
                        m_currentBetText.text = $"{m_targetPlayer.CurrentBetValue} <color=green>GREEN</color>";
                        break;
                }
            }
        }

        /// <summary>
        /// Updates the current chip count text to read the player's current chip count.
        /// </summary>
        public void UpdateCurrentChipCountText()
        {
            m_currentChipsText.text = m_targetPlayer.CurrentChipCount.ToString();
        }
    } 
}

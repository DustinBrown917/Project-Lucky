using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// View class to reflect the data of the other player.
    /// </summary>
    public class OtherPlayerView : PlayerView
    {
        [Tooltip("The text component representing the other player's name.")]
        [SerializeField] private TextMeshProUGUI m_playerNameText;

        [Tooltip("The text component representing the other player's current chip total.")]
        [SerializeField] private TextMeshProUGUI m_currentChipsText;

        [Tooltip("The text component representing the other player's current bet.")]
        [SerializeField] private TextMeshProUGUI m_currentBetText;

        /// <summary>
        /// String builder used to create text updates with minimal garbage.
        /// </summary>
        private StringBuilder m_stringBulder = new StringBuilder();

        #region Monobehaviour Callbacks
        private void Awake()
        {
            //Listen for changed to who the other player is.
            Player.OnOtherPlayerChanged += HandleOtherPlayerChanged;
        }

        private void OnEnable()
        {
            //Get the other player on enable and set as target.
            SetPlayer(Player.Other);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //Clean up static event listener.
            Player.OnOtherPlayerChanged -= HandleOtherPlayerChanged;
        }
        #endregion /Monobehaviour Callbacks

        #region Instance Methods

        /// <summary>
        /// Handle a change in who the other player is.
        /// </summary>
        /// <param name="oldPlayer"></param>
        /// <param name="newPlayer"></param>
        private void HandleOtherPlayerChanged(Player oldPlayer, Player newPlayer)
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
            m_playerNameText.text = "";
            m_currentBetText.text = "";
            m_currentChipsText.text = "";
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void HandleNewTarget()
        {
            m_playerNameText.text = "Your opponent is " + m_targetPlayer.photonView.Owner.NickName;
            UpdateCurrentBetText();
            UpdateCurrentChipCountText();
        }

        private void HandleCurrentChipCountChanged(int value) => UpdateCurrentChipCountText();
        private void HandleCurrentBetColourChanged(BettableColors color) => UpdateCurrentBetText();
        private void HandleCurrentBetValueChanged(int value) => UpdateCurrentBetText();

        /// <summary>
        /// Updates the current bet text to read the current value and color of the bet.
        /// </summary>
        public void UpdateCurrentBetText()
        {
            m_stringBulder.Clear();

            //value of 0 means no bet has been placed yet.
            if (m_targetPlayer.CurrentBetValue <= 0) {
                m_currentBetText.text = "Has not placed a bet yet.";
            } else {
                //Build the string to represent the current bet.
                //Will look like "They are betting [40] on RED/GREEN."
                m_stringBulder.Append("They are betting ");
                m_stringBulder.Append(m_targetPlayer.CurrentBetValue);

                switch (m_targetPlayer.CurrentBetColor) {
                    case BettableColors.Red:
                        m_stringBulder.Append(" on <color=red>RED</color>.");
                        break;
                    case BettableColors.Green:
                        m_stringBulder.Append(" on <color=green>GREEN</color>.");
                        break;
                }

                m_currentBetText.text = m_stringBulder.ToString();
            }
        }

        /// <summary>
        /// Updates the current chip count text to read the player's current chip count.
        /// </summary>
        public void UpdateCurrentChipCountText()
        {
            m_stringBulder.Clear();

            //Build the string to represent the player's current chips.
            //WIll look like "Currently has [40] chips."
            m_stringBulder.Append("Currently has ");
            m_stringBulder.Append(m_targetPlayer.CurrentChipCount);
            m_stringBulder.Append(" chips.");
            m_currentChipsText.text = m_stringBulder.ToString();
        }
        #endregion /Instance Methods
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lucky
{
    /// <summary>
    /// The controller for the UI that allows the players to palce a bet.
    /// </summary>
    public class PlaceBetPanel : MonoBehaviour
    {
        [Tooltip("The text that will represent the current amount being bet.")]
        [SerializeField] private TextMeshProUGUI m_betAmountText;

        [Tooltip("The button that will notify that the player is locked in.")]
        [SerializeField] private Button m_lockInButton;

        [Tooltip("The button to press to increase the bet amount.")]
        [SerializeField] private Button m_incrementBetAmountButton;

        [Tooltip("The button to press to decrease the bet amount.")] 
        [SerializeField] private Button m_decrementBetAmountButton;

        [Tooltip("The toggle to press to switch the bet to red.")]
        [SerializeField] private Toggle m_redToggle;

        [Tooltip("The toggle to press to switch the bet to green.")]
        [SerializeField] private Toggle m_greenToggle;

        #region Monobehaviour Callbacks
        private void OnEnable()
        {
            //Ensure there is a local  player when the panel is opened.
            if(Player.Local == null) {
                Debug.LogError("Local player is null. I'm not even sure how this happened, tbh.");
                gameObject.SetActive(false);
                return;
            }

            //Ensure the selected color aligns with the player's current bet color
            switch (Player.Local.CurrentBetColor) {
                case BettableColors.Red:
                    m_redToggle.SetIsOnWithoutNotify(true);
                    break;
                case BettableColors.Green:
                    m_greenToggle.SetIsOnWithoutNotify(true);
                    break;
            }

            //Ensure the bet amount text aligns with the player's current bet
            HandlePlayerBetValueChanged(Player.Local.CurrentBetValue);

            Player.Local.OnCurrentBetValueChanged += HandlePlayerBetValueChanged;
        }



        private void OnDisable()
        {
            //If there is a local player currently, clean their listener.
            if(Player.Local != null) {
                Player.Local.OnCurrentBetValueChanged -= HandlePlayerBetValueChanged;
            }
        }
        #endregion /Monobehaviour Callbacks

        /// <summary>
        /// Handle a change in which color is selected, updating the local player's CurrentBetColor
        /// (Referenced by UI)
        /// </summary>
        public void HandleColorToggled()
        {
            if (m_redToggle.isOn) {
                Player.Local.SetCurrentBetColor(BettableColors.Red);
            } else {
                Player.Local.SetCurrentBetColor(BettableColors.Green);
            }
        }

        /// <summary>
        /// Modify the local player's CurrentBetValue by <paramref name="amount"/>
        /// (Referenced by UI)
        /// </summary>
        /// <param name="amount"></param>
        public void ModifyBetAmount(int amount)
        {
            Player.Local.SetCurrentBetValue(Player.Local.CurrentBetValue + amount);
        }

        /// <summary>
        /// Raised when the local player's CurrentBetValue changes.
        /// Updates UI components to accurately reflect the local player's CurrentBetValue.
        /// </summary>
        /// <param name="newAmount"></param>
        private void HandlePlayerBetValueChanged(int newAmount)
        {
            //If the new amount is less than or equal to zero, do not allow them to decrement anymore
            //And disable the lock in button (no bet value = no bet at all).
            if(newAmount <= 0) {
                m_decrementBetAmountButton.interactable = false;
                m_lockInButton.interactable = false;
            //If the new amount is greater than zero, make sure the player can decrement
            //And allow the player to lock in their bet.
            } else {
                m_decrementBetAmountButton.interactable = true;
                m_lockInButton.interactable = true;
            }

            //Enale or disable the increment button based on whether or not the current bet
            //meets or exceeds the player's current chip count.
            if(newAmount >= Player.Local.CurrentChipCount) {
                m_incrementBetAmountButton.interactable = false;
            } else {
                m_incrementBetAmountButton.interactable = true;
            }

            //Update the current bet text.
            m_betAmountText.text = newAmount.ToString();
        }

        /// <summary>
        /// Set the lockin state of the player to true and disable the panel.
        /// (referenced by UI).
        /// </summary>
        public void LockIn()
        {
            Player.Local.SetBetLockedIn(true);
            gameObject.SetActive(false);
        }
    } 
}

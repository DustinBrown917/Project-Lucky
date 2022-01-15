using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// Handles setting the player's name, saving, and retrieving it from PlayerPrefs.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {
        /// <summary>
        /// The path to save/retrieve the player's name in PlayerPrefs.
        /// </summary>
        private const string m_PLAYER_PREFS_PATH = "PlayerName";

        private void Start()
        {
            GetComponent<TMP_InputField>().text = PlayerPrefs.GetString(m_PLAYER_PREFS_PATH, "Player");
        }

        /// <summary>
        /// Set the player's name.
        /// </summary>
        /// <param name="newName"></param>
        public void UpdatePlayerNickname(string newName)
        {
            PhotonNetwork.NickName = newName;

            PlayerPrefs.SetString(m_PLAYER_PREFS_PATH, newName);
        }
    } 
}

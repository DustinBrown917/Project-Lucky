using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lucky
{
    /// <summary>
    /// A controller class used to handle the generalized flow of the MainMenu scene.
    /// </summary>
    public class MainMenuController : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// The current version of the game - not needed for this project currently but here for posterity.
        /// </summary>
        private const string m_GAME_VERSION = "1.0";

        [Tooltip("The panel containing all elements needed to name the player and attempt a connection.")]
        [SerializeField] private GameObject m_mainControlPanel;

        [Tooltip("The text that will be used to broadcast to the user the current connection status.")]
        [SerializeField] private TextMeshProUGUI m_statusText;

        [Tooltip("The button the player will need to press to join a game.")]
        [SerializeField] private Button m_playButton;

        [Tooltip("The maximum number of players allowed in the room.")]
        [SerializeField] private byte m_maxPlayersPerRoom = 2;




        #region MonoBehaviour Callbacks
        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            AttemptConnection();
        }
        #endregion /MonoBehaviour Callbacks




        #region Instance Methods
        /// <summary>
        /// Attempts to connect to the photon master server.
        /// </summary>
        private void AttemptConnection()
        {
            if (!PhotonNetwork.IsConnected) {
                m_statusText.text = "Connecting to master server...";
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = m_GAME_VERSION;
            }
        }


        /// <summary>
        /// Sets the play button interactable to true if Photon is ready and the player name is not empty,
        /// otherwise sets the play button interactable to false.
        /// </summary>
        public void UpdatePlayButtonInteraction()
        {
            m_playButton.interactable = PhotonNetwork.IsConnectedAndReady && !string.IsNullOrWhiteSpace(PhotonNetwork.NickName);
        }

        /// <summary>
        /// Begins the process of joining a room. If not currently connected to photon
        /// (Referenced by UI)
        /// </summary>
        public void JoinRoom()
        {
            m_mainControlPanel.SetActive(false);

            m_statusText.text = "Attempting to join a random room...";
            PhotonNetwork.JoinRandomRoom();
        }

        /// <summary>
        /// If this isn't self documenting, I don't know what is.
        /// (Referenced by UI)
        /// </summary>
        public void Quit()
        {
            Application.Quit();
        }
        #endregion




        #region Photon PUN Callbacks
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnConnectedToMaster()
        {
            m_statusText.text = "Successfully connected to master server.";
            UpdatePlayButtonInteraction();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            m_statusText.text = "No room found to join. Creating a room...";

            //Create a room, ensuring that at least one player is allowed in the room.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = (byte)Mathf.Max(m_maxPlayersPerRoom, 1) });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            //If we didn't intend to disconnect...
            if(cause != DisconnectCause.DisconnectByClientLogic) {
                m_statusText.text = $"Unexpected disconnection. Bummer. Cause: {cause}";

                //Revert state of the ui.
                m_mainControlPanel?.SetActive(true);
                UpdatePlayButtonInteraction();

                //Attempt to connect again.
                AttemptConnection();
            }

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnJoinedRoom()
        {
            //If we're the first in the room, then we are master client and it is up to us to load the scene.
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
                PhotonNetwork.LoadLevel("GameScene");
            }
        }
        #endregion /Photon PUN Callbacks
    }
}

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lucky
{
    /// <summary>
    /// Controller class that will handle the generalized flow of the GameScene.
    /// </summary>
    public class GameSceneController : MonoBehaviourPunCallbacks
    {
        [Tooltip("The object that will be spawned in to represent players.")]
        [SerializeField] private Player m_playerGameObjectPrefab;
        [Tooltip("The root UI object that contains the controls for placing bets.")]
        [SerializeField] private PlaceBetPanel m_placeBetPanel;
        [Tooltip("The RandomColorPicker that will be invoked to change the color of the object of the bet.")]
        [SerializeField] private RandomColorPicker m_randomColorPicker;
        [Tooltip("The color swapper of the object being bet upon.")]
        [SerializeField] private ColorSwapper m_bettingObjectColorSwapper;

        [Tooltip("The color the object being bet upon will turn while it's 'deciding' what color to be.")]
        [SerializeField] private Color m_neutralColor;

        [Tooltip("The ChipStackPool that will handle the local player's chip graphics.")]
        [SerializeField] private PlayerChipStackPool m_localPlayerChipStackPool;
        [Tooltip("The ChipStackPool that will handle the other player's chip graphics.")]
        [SerializeField] private PlayerChipStackPool m_otherPlayerChipStackPool;

        /// <summary>
        /// Has the random color picker chosen a new color, but it has not yet been applied to the object of the bet?
        /// </summary>
        private bool m_randomColorReady = false;

        /// <summary>
        /// The previous color that was selected for the object of the bet.
        /// </summary>
        private BettableColors m_lastColorPicked;

        private void Awake()
        {
            //Connect necessary events.
            Player.OnOtherPlayerChanged += HandlePlayerChanged;
            Player.OnLocalPlayerChanged += HandlePlayerChanged;
        }

        private void Start()
        {
            //Instantiate the player game object for this client.
            PhotonNetwork.Instantiate(m_playerGameObjectPrefab.name, Vector3.zero, Quaternion.identity, 0);
        }

        private void OnDestroy()
        {
            //Disconnect necessary events.
            Player.OnOtherPlayerChanged -= HandlePlayerChanged;
            Player.OnLocalPlayerChanged -= HandlePlayerChanged;
        }

        /// <summary>
        /// Notifies the necessary components of a player change.
        /// </summary>
        /// <param name="oldPlayer"></param>
        /// <param name="newPlayer"></param>
        private void HandlePlayerChanged(Player oldPlayer, Player newPlayer)
        {
            //If there was a player previously...
            if(oldPlayer != null) {
                //Remove event handler from old player
                oldPlayer.OnBetLockedInChanged -= HandlePlayerBetLockedInChanged;

                //Notify the appropriate ChipStackPool of the old player's departure.
                if(oldPlayer == m_localPlayerChipStackPool.TargetPlayer) {
                    m_localPlayerChipStackPool.SetPlayer(null);
                } else if(oldPlayer == m_otherPlayerChipStackPool.TargetPlayer) {
                    m_otherPlayerChipStackPool.SetPlayer(null);
                }
            }
            
            //If the new player is not null...
            if(newPlayer != null) {
                //Subscribe event
                newPlayer.OnBetLockedInChanged += HandlePlayerBetLockedInChanged;

                //Notify the appropriate ChipStackPool of the new player.
                if (newPlayer == Player.Local) {
                    m_localPlayerChipStackPool.SetPlayer(newPlayer);

                    //If the new player does not have their bet locked in upon entering the room,
                    //Enable the ui controls to allow them to place a bet.
                    if (!Player.Local.BetLockedIn) {
                        m_placeBetPanel.gameObject.SetActive(true);
                    }
                } else if(newPlayer == Player.Other) {
                    m_otherPlayerChipStackPool.SetPlayer(newPlayer);
                }
            }
        }

        /// <summary>
        /// Handle's a change in one of the players' locked in state.
        /// </summary>
        /// <param name="value"></param>
        private void HandlePlayerBetLockedInChanged(bool value)
        {
            //If all players are locked in, pick a bet and let the chips fall where they may.
            if (AllPlayersAreLockedIn()) {
                StartCoroutine(ColorPickSequence());
                Player.Local.SetBetLockedIn(false);
            }
        }

        /// <summary>
        /// Have the local player leave the room.
        /// </summary>
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        /// <summary>
        /// When the local player leaves the room, return them to the main menu.
        /// </summary>
        public override void OnLeftRoom()
        {
            Player.ClearPlayerReferences();
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Have all players present locked a bet in?
        /// </summary>
        /// <returns></returns>
        public bool AllPlayersAreLockedIn()
        {
            return (Player.Local == null || Player.Local.BetLockedIn) && (Player.Other == null || Player.Other.BetLockedIn);
        }

        /// <summary>
        /// Recieve the random color chosen by the random color picker.
        /// (Referenced in editor)
        /// </summary>
        /// <param name="color"></param>
        public void HandleRandomColorPicked(BettableColors color)
        {
            m_lastColorPicked = color;
            m_randomColorReady = true;
        }

        /// <summary>
        /// Coroutine handling the sequence of:
        /// Players Lock in > Pause > Random color is picked > Pause > Handle the results of the players' bets.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ColorPickSequence()
        {
            //Set the bet object to its neutral color, then pause for ***dramatic effect*** (⌐■_■)
            m_bettingObjectColorSwapper.SetColor(m_neutralColor);
            yield return new WaitForSeconds(1.0f);

            //Would much rather have this run through serverside code. Alas, I am but a lowly peasant and do not have Photon Cloud.
            //If this view belongs to the local player and the local player is the master client, invoke the random color picker to do its thing.
            if(photonView.IsMine && photonView.Owner.IsMasterClient) {
                m_randomColorPicker.PickRandomColor();
            }
            
            //Wait until we receive notification that a random color is ready.
            //Should be instantaneous for the master client, but gotta wait for network on the other player.
            yield return new WaitUntil(() => m_randomColorReady);

            //Reset color ready state
            m_randomColorReady = false;

            //Reveal the chosen color to the player.
            m_bettingObjectColorSwapper.SetColor(m_lastColorPicked.ToColor());

            //Pause for ***dramatic effect*** ☜(ﾟヮﾟ☜)
            yield return new WaitForSeconds(1.0f);

            //Handle the consequences of the bet.
            HandleBetResult();

            //Reenable the betting panel so the player can challenge the Gods of fate again.
            m_placeBetPanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// Distribute the chips according to the players' bets.
        /// </summary>
        private void HandleBetResult()
        {
            if(Player.Local.CurrentBetColor == m_lastColorPicked) {
                Player.Local.SetCurrentChipCount(Player.Local.CurrentChipCount + Player.Local.CurrentBetValue);
            } else {
                Player.Local.SetCurrentChipCount(Player.Local.CurrentChipCount - Player.Local.CurrentBetValue);
            }

            Player.Local.SetCurrentBetValue(0);  
        }
    } 
}

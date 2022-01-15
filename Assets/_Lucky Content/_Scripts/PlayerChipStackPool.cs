using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// The manager for a player's  graphical chips stacks, distributing chip stacks between the different chip piles.
    /// Although this isn't strictly "UI" as views normally are, it functions similarly as it's primary purpose is to 
    /// broadcast information to the player visually with little other function.
    /// 
    /// With the amount of chips a player can win being limited only by the capicity of an int, some liberties had to be taken
    /// with the representation of chips. In a hypothetical case where player's can win 2.4 billion chips, that would be 240 million chipstacks
    /// on one side of the table since chip stacks are required to represent 10 chips. My solution is a bit mundane:
    /// Somewhere between 1 and 240 million, there is a number where chipstacks cease having individual meaning. Ie. It's easy to look at 3 stacks and say
    /// "That's 3 stacks", but it's not so easy to look at 100+ stacks and easily identify how many are there.
    /// 
    /// As is, an individual pile of chip stacks has space for 20 stacks therein. After that, new chip stacks begin to lose meaning and so they are not added.
    /// More chip stack spaces can be added to piles easily via the editor.
    /// 
    /// As such, this class's job is to instantiate all necessary chipstacks on awake and distribute them accordingly throughout the game, avoiding the expensive
    /// instantiate and destroy calls throughout the game.
    /// </summary>
    public class PlayerChipStackPool : PlayerView
    {
        private const int m_CHIP_STACK_VALUE = 10;

        /*vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv*/
        #region ========================================== Subclasses ==================================================
        /*vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv*/

        /// <summary>
        /// Color class that stores values in HSV to make procedural generation of distinct colors easier.
        /// </summary>
        private struct ColorHSV
        {
            public static implicit operator Color(ColorHSV col) => Color.HSVToRGB(col.h, col.s, col.v);

            public float h, s, v;

            public ColorHSV(float h, float s, float v)
            {
                this.h = h;
                this.s = s;
                this.v = v;
            }
        }

        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/
        #endregion ===================================== End Subclasses ================================================
        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/

        [Tooltip("The prefab for an individual chipstack.")]
        [SerializeField] private GameObject m_chipStackPrefab;

        [Tooltip("The ChipPile representing the player's chips.")]
        [SerializeField] private ChipPile m_playerChipPile;
        [Tooltip("The ChipPile representing the player's red bet.")]
        [SerializeField] private ChipPile m_redBetChipPile;
        [Tooltip("The ChipPile representing the player's green bet.")]
        [SerializeField] private ChipPile m_greenBetChipPile;

        /// <summary>
        /// How big should the pool be.
        /// </summary>
        private int m_startingPoolSize;

        /// <summary>
        /// Currently pooled chips. 
        /// Using a stack to make it easier to maintain color consistency. 
        /// ie. If you put a red chip stack in, the next chip stack you pull out should be red.
        /// </summary>
        private Stack<GameObject> m_pooledChipStacks = new Stack<GameObject>();

        #region MonoBehaviour Callbacks
        private void Awake()
        {
            //Determine pool. Since red and green bet piles should never have chips at the same time, we only need to
            //Consider the greater of the two (though they really shouldn't be different sizes, but that's up to the designer!)
            m_startingPoolSize = m_playerChipPile.MaxChipStackCount + Mathf.Max(m_redBetChipPile.MaxChipStackCount, m_greenBetChipPile.MaxChipStackCount);

            CreateChipStacks();
        }
        #endregion /MonoBehaviour Callbacks

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void HandleNewTarget()
        {
            ReturnAllChipsToPool();
            BalanceChipStackPiles();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void HandleNullTarget()
        {
            ReturnAllChipsToPool();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void ConnectEvents()
        {
            m_targetPlayer.OnCurrentBetColourChanged += HandlePlayerCurrentBetColourChanged;
            m_targetPlayer.OnCurrentBetValueChanged += HandlePlayerCurrentBetValueChanged;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void DisconnectEvents()
        {
            m_targetPlayer.OnCurrentBetColourChanged -= HandlePlayerCurrentBetColourChanged;
            m_targetPlayer.OnCurrentBetValueChanged -= HandlePlayerCurrentBetValueChanged;
        }

        /// <summary>
        /// Handle the player changing their bet colour.
        /// </summary>
        /// <param name="color"></param>
        private void HandlePlayerCurrentBetColourChanged(BettableColors color)
        {
            //Determine where chips currently are and where they should be moved ti.
            ChipPile from, to;
            if(color == BettableColors.Green) {
                from = m_redBetChipPile;
                to = m_greenBetChipPile;
            } else {
                from = m_greenBetChipPile;
                to = m_redBetChipPile;
            }

            //Remember how many chipstacks were in the from ChipPile - that's how many we'll need in the to pile.
            int targetStackCount = from.CurrentChipStackCount;

            //Return the from chipstacks to the pool.
            while(from.TryPopChipStack(out GameObject chipStack)) {
                PoolChipStack(chipStack);
            }

            //Move the same number of chipstacks to the to pile, allowing for inconsistencies in pile size because I just cannot leave "well enough" alone.
            while(!to.PileIsFull && to.CurrentChipStackCount < targetStackCount && TryPopPooledChipStack(out GameObject chipStack) ) {
                to.AddChipStack(chipStack);
            }
        }

        /// <summary>
        /// Handle a change in the player's current bet value.
        /// </summary>
        /// <param name="betValue"></param>
        private void HandlePlayerCurrentBetValueChanged(int betValue)
        {
            BalanceChipStackPiles();
        }

        /// <summary>
        /// Ensure that all ChipPiles are representing the correct number if chip stacks.
        /// </summary>
        private void BalanceChipStackPiles()
        {
            //Figure out which bet pile we're dealing with.
            ChipPile betPile;
            if (m_targetPlayer.CurrentBetColor == BettableColors.Green) {
                betPile = m_greenBetChipPile;
            } else {
                betPile = m_redBetChipPile;
            }

            //How many stacks should be in the bet pile.
            int desiredBetStackCount = Mathf.Min(m_targetPlayer.CurrentBetValue / m_CHIP_STACK_VALUE, betPile.MaxChipStackCount);
            //How many stacks should be in the player's pile?
            int desiredPlayerStackCount = Mathf.Min((m_targetPlayer.CurrentChipCount - m_targetPlayer.CurrentBetValue) / m_CHIP_STACK_VALUE, m_playerChipPile.MaxChipStackCount);

            //How many stacks do we need added to the bet pile?
            int betPileModification = desiredBetStackCount - betPile.CurrentChipStackCount;
            //How many stacks do we need removed from the player pile?
            int playerPileModification = desiredPlayerStackCount - m_playerChipPile.CurrentChipStackCount;

            //Only two of the below loops should ever execute.

            //Return any bet pile stacks to the pool that do not belong.
            for (int i = betPileModification; i < 0; i++) {
                if (betPile.TryPopChipStack(out GameObject chipStack)) {
                    PoolChipStack(chipStack);
                }
            }
            

            //Return any player pile stacks to the pool that do not belong.
            for (int i = playerPileModification; i < 0; i++) {
                if (m_playerChipPile.TryPopChipStack(out GameObject chipStack)) {
                    PoolChipStack(chipStack);
                }
            }
            

            //Pull any chip stacks from the pool that are missing from the bet pile.
            for (int i = betPileModification; i > 0; i--) {
                if (TryPopPooledChipStack(out GameObject chipStack)) {
                    betPile.AddChipStack(chipStack);
                }
            }
            

            //Pull any chip stacks from the pool that are missing from the player pile.
            for (int i = playerPileModification; i > 0; i--) {
                if (TryPopPooledChipStack(out GameObject chipStack)) {
                    m_playerChipPile.AddChipStack(chipStack);
                }
            }
            
        }

        /// <summary>
        /// Returns all chips from their respective piles to the pool.
        /// </summary>
        private void ReturnAllChipsToPool()
        {
            while(m_greenBetChipPile.TryPopChipStack(out GameObject chipStack)) {
                PoolChipStack(chipStack);
            }

            while (m_redBetChipPile.TryPopChipStack(out GameObject chipStack)) {
                PoolChipStack(chipStack);
            }

            while (m_playerChipPile.TryPopChipStack(out GameObject chipStack)) {
                PoolChipStack(chipStack);
            }
        }

        /// <summary>
        /// Pre-populte the chipstack pool.
        /// </summary>
        public void CreateChipStacks()
        {
            //Generate a random color to start with.
            ColorHSV col = new ColorHSV(UnityEngine.Random.value, UnityEngine.Random.Range(0.75f, 1.0f), UnityEngine.Random.Range(0.75f, 1.0f));

            //Determine the greatest huestep possible to generate unique colours.
            float hueStep = 1.0f / m_startingPoolSize;

            for(int i = 0; i < m_startingPoolSize; i++) {
                //Get next colour
                col.h += hueStep;
                if (col.h > 1.0f) col.h -= 1.0f;

                //Instantiate chip stack
                GameObject chipStack = Instantiate(m_chipStackPrefab);
                //Give it a colour
                chipStack.GetComponentInChildren<ColorSwapper>().SetColor(col);
                //Pool it
                PoolChipStack(chipStack);
            }
        }

        /// <summary>
        /// Moves a given chip stack to the pool.
        /// </summary>
        /// <param name="chipStack"></param>
        public void PoolChipStack(GameObject chipStack)
        {
            chipStack.gameObject.SetActive(false);
            m_pooledChipStacks.Push(chipStack);
            chipStack.transform.SetParent(transform);
        }

        /// <summary>
        /// Tries to pop a chipstack from the pool.
        /// </summary>
        /// <param name="chipStack"></param>
        /// <returns>True if a chipstack pops, false if the pool is empty.</returns>
        public bool TryPopPooledChipStack(out GameObject chipStack)
        {
            if(m_pooledChipStacks.Count == 0) {
                chipStack = null;
                return false;
            }

            chipStack = m_pooledChipStacks.Pop();
            chipStack.gameObject.SetActive(true);
            chipStack.transform.SetParent(null);
            return true;
        }
    } 
}

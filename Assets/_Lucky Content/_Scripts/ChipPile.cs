using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// Physical representation of a player's chips. 
    /// </summary>
    public class ChipPile : MonoBehaviour
    {
        [Tooltip("The positions that chip stacks will spawn in, in the order that they should appear.")]
        [SerializeField] private Transform[] m_stackPositions;

        /// <summary>
        /// Maximum number of chip stacks that can represented by this stack graphically.
        /// </summary>
        public int MaxChipStackCount => m_stackPositions.Length;

        /// <summary>
        /// The chip stack game objects currently held by this ChipPile
        /// </summary>
        private Stack<GameObject> m_chipStacks = new Stack<GameObject>();

        /// <summary>
        /// How many chip stacks are currently represented in this pile.
        /// </summary>
        public int CurrentChipStackCount => m_chipStacks.Count;

        /// <summary>
        /// Is this chip pile currently representing the maximum possible chip stacks.
        /// </summary>
        public bool PileIsFull => MaxChipStackCount == CurrentChipStackCount;

        /// <summary>
        /// Add a chip stack to this ChipPile.
        /// </summary>
        /// <param name="go"></param>
        public void AddChipStack(GameObject go)
        {
            m_chipStacks.Push(go);
            if(m_chipStacks.Count > m_stackPositions.Length) {
                go.SetActive(false);
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
            } else {
                go.transform.SetParent(m_stackPositions[m_chipStacks.Count - 1]);
                go.transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Try got get a chip stack out of this pile.
        /// </summary>
        /// <param name="chipStack"></param>
        /// <returns>True if a chipstack was gotten, false if the ChipPile is empty.</returns>
        public bool TryPopChipStack(out GameObject chipStack)
        {
            if(m_chipStacks.Count == 0) {
                chipStack = null;
                return false;
            }
 
            chipStack = m_chipStacks.Pop();

            if(chipStack == null) {
                return false;
            }

            chipStack.transform.SetParent(null);

            return true;
        }
    } 
}

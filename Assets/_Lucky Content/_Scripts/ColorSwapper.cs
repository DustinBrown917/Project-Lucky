using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// Helper class to make changing of material colours at runtime via property blocks easier.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class ColorSwapper : MonoBehaviour
    {
        private MeshRenderer m_meshRenderer;

        private MaterialPropertyBlock m_propBlock;

        private void Awake()
        {
            m_meshRenderer = GetComponent<MeshRenderer>();
            m_propBlock = new MaterialPropertyBlock();
        }
        
        /// <summary>
        /// Set the colour of the property block of the attached MeshRenderer.
        /// </summary>
        /// <param name="col"></param>
        public void SetColor(Color col)
        {
            m_meshRenderer.GetPropertyBlock(m_propBlock);
            m_propBlock.SetColor("_Color", col);
            m_meshRenderer.SetPropertyBlock(m_propBlock);
        }
    } 
}

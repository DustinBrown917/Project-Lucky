using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// This is an utility class that can be used to set the initial colour of a material via property blocks
    /// both at design time and run time. 
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class DefaultColor : MonoBehaviour
    {
        [SerializeField] private Color m_color = Color.white;
        private void Start()
        {
            ApplyDefaultColor();
            Destroy(this);
        }

        private void OnValidate()
        {
            ApplyDefaultColor();
        }

        private void ApplyDefaultColor()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mr.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", m_color);
            mr.SetPropertyBlock(mpb);
        }
    } 
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasySlime
{
    public class MaterialSwitcher : MonoBehaviour
    {
        [SerializeField] Renderer m_Renderer = null;

        [SerializeField] Material[] m_Materials = null;

        int m_MaterialIndex = 0;

		private void Start()
        {
            if (m_MaterialIndex < m_Materials.Length)
            {
                m_Renderer.sharedMaterial = m_Materials[m_MaterialIndex];
            }
        }

		public void NextMaterial()
        {
            m_MaterialIndex += 1;

            if (m_MaterialIndex >= m_Materials.Length)
            {
                m_MaterialIndex = 0;
            }

            m_Renderer.sharedMaterial = m_Materials[m_MaterialIndex];
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasySlime
{
    public class ChildTransformToggler : MonoBehaviour
    {
        int m_Index = 0;

        private void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            if (transform.childCount > 0)
                transform.GetChild(0).gameObject.SetActive(true);
        }

        public void Next()
        {
            transform.GetChild(m_Index).gameObject.SetActive(false);

            m_Index += 1;

            if (m_Index >= transform.childCount)
            {
                m_Index = 0;
            }

            transform.GetChild(m_Index).gameObject.SetActive(true);
        }
    }
}
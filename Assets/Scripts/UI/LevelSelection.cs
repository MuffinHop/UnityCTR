using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class LevelSelection : MonoBehaviour
    {
        [SerializeField] private GameObject _selected;

        public void Select()
        {
            _selected.SetActive(true);
        }

        public void Deselect()
        {
            _selected.SetActive(false);
        }
    }
}
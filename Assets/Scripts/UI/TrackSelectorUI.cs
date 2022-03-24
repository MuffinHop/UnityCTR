using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class TrackSelectorUI : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Transform _baseLevelSelection;
        [SerializeField] private string[] _trackList;
        [SerializeField] private Vector3 _basePosition;
        [SerializeField] private Transform[] _trackButtons;
        private int _advanceMenu = 0;
        private float _advanceMenuTimer = 0f;

        private void Start()
        {
            _basePosition = _baseLevelSelection.position;
            _trackButtons = new Transform[_trackList.Length];
            for (int i = 0; i < _trackList.Length; i++)
            {
                var levelSelection = Instantiate(_baseLevelSelection, _baseLevelSelection.position,
                    _baseLevelSelection.rotation, _canvas.transform);
                levelSelection.gameObject.SetActive(true);
                var textMeshPro = levelSelection.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
                textMeshPro.text = _trackList[i];
                _trackButtons[i] = levelSelection;
            }
        }

        private int index = 0;

        private void Update()
        {
            if (Time.frameCount % 240 == 0)
            {
                _advanceMenu = 1;
            }

            if (_advanceMenu != 0)
            {
                _advanceMenuTimer += Time.deltaTime * 5f;
                if (_advanceMenuTimer >= 1f)
                {
                    _advanceMenuTimer = 0f;
                    _advanceMenu = 0;
                    index++;
                    if (index >= _trackList.Length)
                    {
                        index = 0;
                    }
                }
            }

            for (int i = 0; i < _trackList.Length; i++)
            {
                float y = 1.5f + ((i + index + _advanceMenuTimer) * 140f) % (_trackList.Length * 140.0f);
                _trackButtons[i].position = _basePosition + new Vector3(-Mathf.Cos(y / (140f * Mathf.PI)) * 140f,
                    y - (_trackList.Length / 2) * 140f, 0f);
                if (i == ((-index + _trackList.Length / 2) % _trackList.Length))
                {
                    _trackButtons[i].GetComponent<LevelSelection>().Select();
                }
                else
                {
                    _trackButtons[i].GetComponent<LevelSelection>().Deselect();
                }
            }
        }
    }
}

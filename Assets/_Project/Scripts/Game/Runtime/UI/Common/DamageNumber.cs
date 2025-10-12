using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game
{
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private float _offset;
        [SerializeField] private float _duration;

        private TextMeshProUGUI _textMesh;

        public string Text
        {
            get => _textMesh.text;
            set => _textMesh.text = value;
        }

        public void SetColor(Color color)
        {
            _textMesh.color = color;
        }

        private void Awake()
        {
            _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
                transform.forward = mainCamera.transform.forward;

            if (TryGetComponent(out TempObject tempObject))
                tempObject.TimeOfLife = _duration;

            _textMesh.transform.DOMoveY(transform.position.y + _offset, _duration).SetLink(gameObject);
        }
    }
}

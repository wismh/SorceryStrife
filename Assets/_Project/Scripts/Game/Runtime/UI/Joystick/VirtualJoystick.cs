using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    public class VirtualJoystick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [InputControl(layout = "Vector2")]
        [SerializeField] private string _controlPath = "<Gamepad>/leftStick";

        [SerializeField] private RectTransform _container;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _movementRange = 100f;
        [SerializeField] private float _deadZone = 0.05f;
        [SerializeField] private bool _isFloating = true;
        [SerializeField] private float _idleAlpha = 0.35f;
        [SerializeField] private float _activeAlpha = 0.9f;

        public Vector2 Value { get; private set; }

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        private LevelUpInterstitialState _levelUpState;
        private Image _touchZoneImage;
        private RectTransform _parentRectTransform;
        private Vector2 _defaultContainerPosition;
        private int _activePointerId = -1;

        [Inject]
        public void Construct(LevelUpInterstitialState levelUpState)
        {
            _levelUpState = levelUpState;
        }

        private void Awake()
        {
            var isMobileOrEditor = Application.isMobilePlatform || Application.isEditor;
            if (!isMobileOrEditor)
            {
                gameObject.SetActive(false);
                return;
            }

            _touchZoneImage = GetComponent<Image>();
            _parentRectTransform = _container.parent as RectTransform;
            _defaultContainerPosition = _container.anchoredPosition;
            _canvasGroup.alpha = _idleAlpha;
        }

        private void Start()
        {
            _levelUpState.OnEntered += HandleLevelUpEntered;
            _levelUpState.OnExited += HandleLevelUpExited;
        }

        private void OnDestroy()
        {
            _levelUpState.OnEntered -= HandleLevelUpEntered;
            _levelUpState.OnExited -= HandleLevelUpExited;
        }

        private void HandleLevelUpEntered()
        {
            ResetInput();
            _container.gameObject.SetActive(false);
            if (_touchZoneImage != null)
                _touchZoneImage.raycastTarget = false;
        }

        private void HandleLevelUpExited()
        {
            _container.gameObject.SetActive(true);
            _canvasGroup.alpha = _idleAlpha;
            if (_touchZoneImage != null)
                _touchZoneImage.raycastTarget = true;
        }

        private void ResetInput()
        {
            _activePointerId = -1;
            _handle.anchoredPosition = Vector2.zero;

            if (_isFloating)
                _container.anchoredPosition = _defaultContainerPosition;

            _canvasGroup.alpha = _idleAlpha;
            Value = Vector2.zero;
            SendValueToControl(Vector2.zero);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_activePointerId != -1)
                return;

            _activePointerId = eventData.pointerId;
            _canvasGroup.alpha = _activeAlpha;

            if (_isFloating)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint);

                _container.anchoredPosition = localPoint;
                _handle.anchoredPosition = Vector2.zero;
            }
            else
            {
                ProcessDrag(eventData.position, eventData.pressEventCamera);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
                return;

            ProcessDrag(eventData.position, eventData.pressEventCamera);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
                return;

            ResetInput();
        }

        private void ProcessDrag(Vector2 screenPosition, Camera eventCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _container,
                screenPosition,
                eventCamera,
                out var localPoint);

            var clampedDelta = Vector2.ClampMagnitude(localPoint, _movementRange);
            _handle.anchoredPosition = clampedDelta;

            var rawInput = clampedDelta / _movementRange;
            var magnitude = rawInput.magnitude;

            Vector2 processedInput;
            if (magnitude < _deadZone)
            {
                processedInput = Vector2.zero;
            }
            else
            {
                var normalizedMagnitude = Mathf.InverseLerp(_deadZone, 1f, magnitude);
                processedInput = rawInput.normalized * normalizedMagnitude;
            }

            Value = processedInput;
            SendValueToControl(processedInput);
        }
    }
}

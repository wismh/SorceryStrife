using Lumenwake;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Drop-in replacement for Crystal Framework's SafeArea. Same notch / sim-device behaviour
    /// plus an additional tablet pass: on screens whose long/short axis ratio falls below
    /// <see cref="_tabletAspectThreshold"/>, the safe-area rect is intersected with a centred
    /// rect of <see cref="_targetAspectRatio"/>, letterboxing the UI into a phone-shaped
    /// region. Reverts to plain Screen.safeArea on phone-aspect devices.
    /// </summary>
    public class ImprovedSafeArea : MonoBehaviour
    {
        #region Simulations

        /// <summary>Editor-only simulation device with a known safe-area cutout.</summary>
        public enum SimDevice
        {
            None,
            iPhoneX,
            iPhoneXsMax,
            Pixel3XL_LSL,
            Pixel3XL_LSR,

            /// <summary>iPad 10.2" portrait (4:3) — exercise the tablet path in editor.</summary>
            iPadPortrait,

            /// <summary>iPad 10.2" landscape (4:3).</summary>
            iPadLandscape
        }

        /// <summary>Editor-only override. Static so it can be flipped at runtime without scene reload.</summary>
        public static SimDevice Sim = SimDevice.None;

        // iPhone X / Xs / 11 Pro — Aspect ~19.5:9.
        private readonly Rect[] _nsaIPhoneX =
        {
            new(0f, 102f / 2436f, 1f, 2202f / 2436f),
            new(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f)
        };

        // iPhone Xs Max / XR / 11 / 11 Pro Max — Aspect ~19.5:9.
        private readonly Rect[] _nsaIPhoneXsMax =
        {
            new(0f, 102f / 2688f, 1f, 2454f / 2688f),
            new(132f / 2688f, 63f / 1242f, 2424f / 2688f, 1179f / 1242f)
        };

        // Pixel 3 XL landscape-left — Aspect 18.5:9.
        private readonly Rect[] _nsaPixel3XLLSL =
        {
            new(0f, 0f, 1f, 2789f / 2960f),
            new(0f, 0f, 2789f / 2960f, 1f)
        };

        // Pixel 3 XL landscape-right.
        private readonly Rect[] _nsaPixel3XLLSR =
        {
            new(0f, 0f, 1f, 2789f / 2960f),
            new(171f / 2960f, 0f, 2789f / 2960f, 1f)
        };

        // iPad (4:3) — no notch, full screen.
        private readonly Rect[] _nsaIPad =
        {
            new(0f, 0f, 1f, 1f),
            new(0f, 0f, 1f, 1f)
        };

        #endregion Simulations

        [Header("Safe area")]
        [SerializeField]
        private bool _conformX = true;

        [SerializeField]
        private bool _conformY = true;

        [Header("Tablet letterbox")]
        [Tooltip("Long-axis / short-axis ratio below which the screen is treated as a tablet. " +
                 "16:9 ≈ 1.78, iPhone Pro ≈ 2.16, iPad 4:3 ≈ 1.33. Default 1.6 catches tablets only.")]
        [SerializeField]
        private float _tabletAspectThreshold = 1.6f;

        [Tooltip("Target long/short ratio of the centred UI rect on tablets. " +
                 "Match the canvas reference (16:9 = 1.78).")]
        [SerializeField]
        private float _targetAspectRatio = 16f / 9f;

        [SerializeField]
        private bool _enableTabletLetterbox = true;

        [Header("Debug")]
        [SerializeField]
        private bool _logging = false;

        private RectTransform _panel;
        private Rect _lastSafeArea = new(0f, 0f, 0f, 0f);
        private Vector2Int _lastScreenSize = new(0, 0);
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        private void Awake()
        {
            _panel = GetComponent<RectTransform>();

            if (_panel == null)
            {
                LoggingSystem.LogError($"[ImprovedSafeArea] No RectTransform on {name} — destroying.");
                Destroy(gameObject);
                return;
            }

            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            Rect safeArea = GetEffectiveSafeArea();

            if (safeArea == _lastSafeArea
                && Screen.width == _lastScreenSize.x
                && Screen.height == _lastScreenSize.y
                && Screen.orientation == _lastOrientation)
            {
                return;
            }

            _lastScreenSize.x = Screen.width;
            _lastScreenSize.y = Screen.height;
            _lastOrientation = Screen.orientation;

            ApplySafeArea(safeArea);
        }

        /// <summary>
        /// OS safe area (or simulated equivalent in editor), then intersected with the tablet
        /// aspect-ratio rect when the current screen qualifies as a tablet.
        /// </summary>
        private Rect GetEffectiveSafeArea()
        {
            Rect osSafeArea = GetOsOrSimSafeArea();

            if (!_enableTabletLetterbox)
            {
                return osSafeArea;
            }

            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return osSafeArea;
            }

            float longAxis = Mathf.Max(Screen.width, Screen.height);
            float shortAxis = Mathf.Min(Screen.width, Screen.height);
            float aspect = longAxis / shortAxis;

            // Phone-aspect or wider — OS safe area is enough.
            if (aspect >= _tabletAspectThreshold)
            {
                return osSafeArea;
            }

            Rect aspectRect = BuildCenteredAspectRect(_targetAspectRatio);

            return Rect.MinMaxRect(
                Mathf.Max(osSafeArea.xMin, aspectRect.xMin),
                Mathf.Max(osSafeArea.yMin, aspectRect.yMin),
                Mathf.Min(osSafeArea.xMax, aspectRect.xMax),
                Mathf.Min(osSafeArea.yMax, aspectRect.yMax));
        }

        private Rect GetOsOrSimSafeArea()
        {
            Rect safeArea = Screen.safeArea;

            if (!Application.isEditor || Sim == SimDevice.None)
            {
                return safeArea;
            }

            bool portrait = Screen.height > Screen.width;
            Rect nsa = new(0f, 0f, 1f, 1f);

            switch (Sim)
            {
                case SimDevice.iPhoneX:
                    nsa = portrait ? _nsaIPhoneX[0] : _nsaIPhoneX[1];
                    break;
                case SimDevice.iPhoneXsMax:
                    nsa = portrait ? _nsaIPhoneXsMax[0] : _nsaIPhoneXsMax[1];
                    break;
                case SimDevice.Pixel3XL_LSL:
                    nsa = portrait ? _nsaPixel3XLLSL[0] : _nsaPixel3XLLSL[1];
                    break;
                case SimDevice.Pixel3XL_LSR:
                    nsa = portrait ? _nsaPixel3XLLSR[0] : _nsaPixel3XLLSR[1];
                    break;
                case SimDevice.iPadPortrait:
                case SimDevice.iPadLandscape:
                    nsa = portrait ? _nsaIPad[0] : _nsaIPad[1];
                    break;
            }

            return new Rect(Screen.width * nsa.x, Screen.height * nsa.y,
                Screen.width * nsa.width, Screen.height * nsa.height);
        }

        /// <summary>
        /// Builds a centred rect matching the given long/short ratio. Whichever axis would
        /// otherwise overflow gets letterboxed: pillars on the long axis when the screen is
        /// more elongated than the target, bars on the short axis when it's squarer (e.g. 4:3
        /// tablet with a 16:9 target — the short axis is reduced to keep the target shape).
        /// </summary>
        private Rect BuildCenteredAspectRect(float longShortRatio)
        {
            if (longShortRatio <= 0f)
            {
                return new Rect(0f, 0f, Screen.width, Screen.height);
            }

            bool landscape = Screen.width >= Screen.height;
            float screenLong = Mathf.Max(Screen.width, Screen.height);
            float screenShort = Mathf.Min(Screen.width, Screen.height);

            float rectLong, rectShort;
            if (screenLong / screenShort >= longShortRatio)
            {
                // Screen is more elongated than target — full short axis, reduce long axis.
                rectLong = screenShort * longShortRatio;
                rectShort = screenShort;
            }
            else
            {
                // Screen is squarer than target — full long axis, reduce short axis.
                rectLong = screenLong;
                rectShort = screenLong / longShortRatio;
            }

            float rectWidth = landscape ? rectLong : rectShort;
            float rectHeight = landscape ? rectShort : rectLong;
            float xMargin = (Screen.width - rectWidth) * 0.5f;
            float yMargin = (Screen.height - rectHeight) * 0.5f;

            return new Rect(xMargin, yMargin, rectWidth, rectHeight);
        }

        private void ApplySafeArea(Rect r)
        {
            _lastSafeArea = r;

            if (!_conformX)
            {
                r.x = 0f;
                r.width = Screen.width;
            }

            if (!_conformY)
            {
                r.y = 0f;
                r.height = Screen.height;
            }

            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // Guards against the Samsung Note 10+/A71/S20 startup quirk where the first
            // Screen poll returns NaN extents.
            if (anchorMin.x >= 0f && anchorMin.y >= 0f && anchorMax.x >= 0f && anchorMax.y >= 0f)
            {
                _panel.anchorMin = anchorMin;
                _panel.anchorMax = anchorMax;
            }

            if (_logging)
            {
                LoggingSystem.Log(
                    $"[ImprovedSafeArea] {name}: x={r.x} y={r.y} w={r.width} h={r.height} on {Screen.width}x{Screen.height}");
            }
        }
    }
}

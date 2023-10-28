using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameSoft.Tools;
using GameSoft.Tools.Extensions;
using GameSoft.Tools.ZenjectExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace GameSoft.Windows
{
    public class WindowsManager : MonoBehaviour
    {
        private const int MinOrder = 10;
        private const float ShadowFadeInTime = 0.2f;
        [SerializeField] private Graphic shadow;
        [SerializeField] private GraphicRaycaster shadowRaycaster;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GraphicRaycaster mainRaycaster;

        [SerializeField] private float shadowAlphaValue = 1;
        private readonly Dictionary<string, BaseWindow> _cacheWindows = new();
        private readonly List<BaseWindow> _windows = new();

        private Sequence _hideTween;
        private int _order;
        private float _shadowAlpha = 0.7f;
        private Canvas _shadowCanvas;
        private Sequence _showTween;
        private WindowsConfig _windowsConfig;
        private BaseWindow TopWindow => _windows.LastOrDefault(w => w != null);

        private void Awake()
        {
            mainCanvas.enabled = false;
            mainRaycaster.enabled = false;
            _shadowCanvas = shadow.GetComponent<Canvas>();
            _shadowCanvas.overrideSorting = true;
            _shadowCanvas.enabled = false;
            shadowRaycaster.enabled = false;
            if (Math.Abs(shadowAlphaValue - 1) > 0.001f) _shadowAlpha = shadowAlphaValue;
            shadow.SetAlpha(0);
            _order = MinOrder;
        }

        public event Action<bool> OnOpenWindow;
        public event Action<bool> OnCloseWindow;
        public event Action<bool> OnChangeShadowFade;

        /// <summary>
        ///     Set windows config
        /// </summary>
        /// <param name="windowsConfig"></param>
        public void SetWindowsConfig(WindowsConfig windowsConfig)
        {
            _windowsConfig = windowsConfig;
        }

        /// <summary>
        ///     Check if window is opened
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsOpened<T>() where T : BaseWindow
        {
            return _windows.Any(window => window is T);
        }

        /// <summary>
        ///     Get last opened window
        /// </summary>
        /// <returns></returns>
        public BaseWindow GetLastWindow()
        {
            return _windows.LastOrDefault();
        }

        /// <summary>
        ///     Is opened any window
        /// </summary>
        /// <returns></returns>
        public bool IsOpenedAny()
        {
            return _windows.Count > 0;
        }

        /// <summary>
        ///     Get window by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWindow<T>() where T : BaseWindow
        {
            var windowName = typeof(T).Name;
            T window;
            if (_cacheWindows.TryGetValue(windowName, out var cacheWindow))
            {
                window = cacheWindow as T;
                if (window != null)
                {
                    window.ResetDefaultValuesOnWindow();
                    return window;
                }
            }

            var windowPrefab = LoadWindowPrefab<T>(windowName);
            window = InstantiateManager.InstantiateAndInject(windowPrefab, mainCanvas.transform, false);
            window.InitializeWindow(this);
            if (!window.IsDestroyAfterClose) _cacheWindows[windowName] = window;
            window.HideFast();
            if (window.IsDestroyAfterClose) window.OnHidedCallback += () => Destroy(window.gameObject, 0.5f);
            return window;
        }

        /// <summary>
        ///     Close window by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Sequence Close<T>() where T : BaseWindow
        {
            var window = _windows.FirstOrDefault(w => w is T);
            return window == null ? DOTween.Sequence() : Close(window);
        }

        /// <summary>
        ///     Close window by type
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public Sequence Close(BaseWindow window)
        {
            if (window != null) _windows.Remove(window);
            _windows.RemoveAll(w => w == null);
            OnCloseWindow?.Invoke(window.UseActivity);
            return Hide(window);
        }

        /// <summary>
        ///     Close all windows
        /// </summary>
        /// <param name="competeActiveShowCloseAnimation"></param>
        /// <returns></returns>
        public Sequence CloseAll(bool competeActiveShowCloseAnimation = true)
        {
            if (competeActiveShowCloseAnimation)
            {
                _hideTween.Complete(true);
                _showTween.Complete(true);
            }

            var sequence = DOTween.Sequence();
            for (var i = _windows.Count - 1; i >= 0; i--) sequence.Append(Close(_windows[i]));
            return sequence;
        }

        /// <summary>
        ///     Close window fast
        /// </summary>
        /// <param name="window"></param>
        public void CloseFast(BaseWindow window)
        {
            if (window != null) _windows.Remove(window);
            _windows.RemoveAll(w => w == null);
            HideFast(window);
        }

        /// <summary>
        ///     Close all windows fast
        /// </summary>
        public void CloseAllFast()
        {
            for (var i = _windows.Count - 1; i >= 0; i--) CloseFast(_windows[i]);
        }

        /// <summary>
        ///     Show or hide shadow while open window
        /// </summary>
        /// <param name="window"></param>
        /// <param name="isShadowShow"></param>
        public void ShowOrHideShadowWhileOpenWindow(BaseWindow window, bool isShadowShow)
        {
            var needShadow = window.IsNeedShadow;
            switch (needShadow)
            {
                case true when !isShadowShow:
                    shadow.SetAlpha(0);
                    break;
                case true:
                    shadow.SetAlpha(_shadowAlpha);
                    break;
            }

            OnChangeShadowFade?.Invoke(isShadowShow);
        }

        /// <summary>
        ///     Show or hide shadow
        /// </summary>
        /// <param name="endValue"></param>
        /// <returns></returns>
        public Sequence ShowOrHideShadow(float endValue)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(shadow.DOFade(endValue, ShadowFadeInTime));
            return sequence;
        }

        /// <summary>
        ///     Open window
        /// </summary>
        /// <param name="callbackBeforeShow"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T OpenWindow<T>(Action<T> callbackBeforeShow = null) where T : BaseWindow
        {
            var window = GetWindow<T>();
            callbackBeforeShow?.Invoke(window);
            ShowWindow(window);
            return window;
        }

        /// <summary>
        ///     Show window
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public Sequence ShowWindow(BaseWindow window)
        {
            var last = TopWindow;
            if (_windows.Count > 0 && window == last) return DOTween.Sequence();
            var tween = Show(window);

            if (!_windows.Contains(window))
            {
                _windows.Add(window);
            }
            else
            {
                _windows.Remove(window);
                _windows.Add(window);
            }

            return tween;
        }

        private T LoadWindowPrefab<T>(string windowName) where T : BaseWindow
        {
            var windowPrefab = _windowsConfig != null
                ? _windowsConfig.GetWindow<T>(windowName)
                : Resources.Load<T>("Windows/" + windowName);
            if (_windowsConfig == null && windowPrefab != null)
                DDebug.LogWarning("WindowsConfig is null, please set it in WindowsManager");
            if (_windowsConfig == null && windowPrefab == null)
                throw new Exception(
                    "WindowsConfig is null and windowPrefab from path 'Resources/Windows' " +
                    "is null, please set right config in WindowsManager");

            return windowPrefab;
        }

        private Sequence Hide(BaseWindow window)
        {
            _hideTween = DOTween.Sequence();
            if (window == null) return _hideTween;
            _hideTween.AppendCallback(window.OnStartHide);
            _hideTween.Append(window.Hide());
            _hideTween.AppendCallback(window.OnHided);
            _hideTween.AppendCallback(window.CallHidedCallback);
            _order -= 2;
            if (_windows.Count > 0)
            {
                var isShadow = window.IsNeedShadow;
                if (!TopWindow.IsNeedShadow && isShadow)
                    if (!window.IsShadowWaitNewWindow)
                        _hideTween.Append(shadow.DOFade(0, ShadowFadeInTime));
            }
            else
            {
                if (window.IsNeedShadow)
                    if (!window.IsShadowWaitNewWindow)
                        _hideTween.Append(shadow.DOFade(0, ShadowFadeInTime));
            }

            _hideTween.AppendCallback(() =>
            {
                if (!window.IsShadowWaitNewWindow) _shadowCanvas.sortingOrder = _order - 2;
            });

            if (_windows.Count != 0) return _hideTween;
            if (!window.IsShadowWaitNewWindow)
                _hideTween.AppendCallback(OnLastClosed);
            return _hideTween;
        }

        private void HideFast(BaseWindow window)
        {
            if (window == null) return;

            window.OnStartHide();
            window.HideFast();
            window.OnHided();
            window.CallHidedCallback();

            _order -= 2;
            if (!window.IsShadowWaitNewWindow) _shadowCanvas.sortingOrder = _order - 2;

            if (_windows.Count > 0)
            {
                var isShadow = window.IsNeedShadow;
                if (!TopWindow.IsNeedShadow && isShadow)
                    if (!window.IsShadowWaitNewWindow)
                        shadow.SetAlpha(0);
            }
            else
            {
                if (window.IsNeedShadow)
                    if (!window.IsShadowWaitNewWindow)
                        shadow.SetAlpha(0);

                if (!window.IsShadowWaitNewWindow) OnLastClosed();
            }
        }

        private Sequence Show(BaseWindow window)
        {
            _showTween = DOTween.Sequence();
            if (window == null) return _showTween;
            if (_windows.Count == 0) _showTween.AppendCallback(OnFirstOpened);

            _showTween.AppendCallback(window.OnStartShow);
            _shadowCanvas.sortingOrder = _order;
            window.SortingOrder = _order + 1;
            _order += 2;

            if (window.IsNeedShadow)
                if (!window.IsShadowWaitNewWindow)
                    _showTween.Append(shadow.DOFade(_shadowAlpha, ShadowFadeInTime));

            _showTween.Append(window.Show());
            _showTween.AppendCallback(() =>
            {
                OnOpenWindow?.Invoke(window.UseActivity);
                window.OnShowed();
            });
            _showTween.AppendCallback(window.CallShowedCallback);
            return _showTween;
        }

        private void ShowFast(BaseWindow window)
        {
            if (window == null) return;
            if (_windows.Count == 0) OnFirstOpened();
            window.OnStartShow();

            _shadowCanvas.sortingOrder = _order;
            window.SortingOrder = _order + 1;
            _order += 2;

            if (window.IsNeedShadow)
                if (!window.IsShadowWaitNewWindow)
                    _showTween.Append(shadow.DOFade(_shadowAlpha, ShadowFadeInTime));

            window.Show();
            window.OnShowed();
            window.CallShowedCallback();
        }

        private void OnLastClosed()
        {
            mainCanvas.enabled = false;
            _shadowCanvas.enabled = false;
            mainRaycaster.enabled = false;
            shadowRaycaster.enabled = false;
        }

        private void OnFirstOpened()
        {
            mainCanvas.enabled = true;
            _shadowCanvas.enabled = true;
            mainRaycaster.enabled = true;
            shadowRaycaster.enabled = true;
        }
    }
}
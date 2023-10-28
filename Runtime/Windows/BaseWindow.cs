using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GameSoft.Windows
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public abstract class BaseWindow : MonoBehaviour
    {
        [SerializeField] protected Button _closeButton;
        [SerializeField] protected Canvas _canvas;
        [SerializeField] protected GraphicRaycaster _canvasRaycaster;

        public bool BackButtonEnabled = true;
        public bool IsNeedShadow = true;
        public bool IsShadowWaitNewWindow = false;
        public bool IsShowBanner = true;
        public bool IsDestroyAfterClose = true;
        public bool UseActivity = false;
        public bool IsShowing => _canvas.enabled;

        public event Action OnShowedCallback;
        public event Action OnHidedCallback;

        protected WindowsManager _windowsManager;
        protected bool _isShadowShow = false;

        public int SortingOrder
        {
            get => _canvas.sortingOrder;
            set
            {
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = value;
            }
        }

        public virtual void ResetDefaultValuesOnWindow()
        {
            _isShadowShow = IsNeedShadow;
            IsShadowWaitNewWindow = false;
        }

        public virtual void InitializeWindow(WindowsManager manager)
        {
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            if (_canvasRaycaster == null) _canvasRaycaster = GetComponent<GraphicRaycaster>();
            _windowsManager = manager;
            if (_closeButton) _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        protected virtual void OnCloseButtonClicked() => Close();

        public virtual Sequence Close()
        {
            return CloseNative();
        }

        public virtual Sequence Close(Action closeCallback)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(CloseNative()).OnComplete(() => { closeCallback?.Invoke(); });
            return sequence;
        }

        protected virtual Sequence CloseNative()
        {
            return _windowsManager.Close(this);
        }

        public virtual void HideFast()
        {
            _canvas.enabled = false;
            _canvasRaycaster.enabled = false;
        }

        public virtual void ShowFast()
        {
            _canvas.enabled = true;
            _canvasRaycaster.enabled = true;
        }

        public virtual Sequence Hide()
        {
            return DOTween.Sequence();
        }

        public virtual Sequence Show()
        {
            return DOTween.Sequence();
        }

        public virtual void OnHided()
        {
            _canvas.enabled = false;
        }

        public virtual void OnShowed()
        {
            _canvasRaycaster.enabled = true;
        }

        public virtual void OnStartHide()
        {
            _canvasRaycaster.enabled = false;
        }

        public virtual void OnStartShow()
        {
            _canvas.enabled = true;
        }

        public void CallShowedCallback() => OnShowedCallback?.Invoke();
        public void CallHidedCallback() => OnHidedCallback?.Invoke();

        public void ShowOrHideShadow(bool? overrideShadowShow = null)
        {
            _isShadowShow = overrideShadowShow ?? !_isShadowShow;
            _windowsManager.ShowOrHideShadowWhileOpenWindow(this, _isShadowShow);
        }
    }
}
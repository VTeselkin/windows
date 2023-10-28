using System;
using DG.Tweening;
using GameSoft.Windows.Extensions;
using UnityEngine;

namespace GameSoft.Windows
{
    public class BaseWindowAnimatedMultiple : BaseWindow
    {
        [Header("Animations")] [SerializeField]
        protected CanvasGroup canvasGroup;

        [SerializeField] protected RectTransform windowTransform;

        [SerializeField] protected Side showSide = Side.Top;
        [SerializeField] protected Side hideSide = Side.Bottom;
        [SerializeField] protected float multiplierPos = 1f;
        [SerializeField] protected float alfaTime = 0.5f;
        [SerializeField] protected float posTime = 0.3f;
        [SerializeField] protected AnimationWindowType defaultAnimationType;

        protected AnimationWindowType? AnimationType;
        protected bool Inited;
        protected Rect ParentSize;

        private Rect GetParentSize
        {
            get
            {
                if (Inited) return ParentSize;
                ParentSize = ((RectTransform)transform.parent).rect;
                return ParentSize;
            }
        }


#if UNITY_EDITOR
        private void Reset()
        {
            if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
#endif
        /// <summary>
        ///     Initialize window
        /// </summary>
        /// <param name="manager"></param>
        public override void InitializeWindow(WindowsManager manager)
        {
            base.InitializeWindow(manager);
            windowTransform ??= GetComponent<RectTransform>();
        }

        /// <summary>
        ///     Pre initialize animation type
        /// </summary>
        /// <param name="type"></param>
        public void PreInitializeAnimation(AnimationWindowType type)
        {
            AnimationType = type;
        }

        /// <summary>
        ///     Reset animation type to default
        /// </summary>
        public void ResetAnimationType()
        {
            AnimationType = null;
        }

        /// <summary>
        ///     Hide window without animation
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override void HideFast()
        {
            var type = AnimationType ?? defaultAnimationType;
            switch (type)
            {
                case AnimationWindowType.Alpha:
                    HideFastByAlpha();
                    break;
                case AnimationWindowType.Pos:
                    HideFastByPos();
                    break;
                case AnimationWindowType.All:
                    HideFastByAlpha();
                    HideFastByPos();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Show window without animation
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override void ShowFast()
        {
            var type = AnimationType ?? defaultAnimationType;
            switch (type)
            {
                case AnimationWindowType.Alpha:
                    ShowFastByAlpha();
                    break;
                case AnimationWindowType.Pos:
                    ShowFastByPos();
                    break;
                case AnimationWindowType.All:
                    ShowFastByAlpha();
                    ShowFastByPos();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Hide window with animation
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override Sequence Hide()
        {
            var sequence = DOTween.Sequence();

            var type = AnimationType ?? defaultAnimationType;
            switch (type)
            {
                case AnimationWindowType.Alpha:
                    sequence.Append(HideByAlpha());
                    break;
                case AnimationWindowType.Pos:
                    sequence.Append(HideByPos());
                    break;
                case AnimationWindowType.All:
                    sequence.Append(HideByAlphaAndPos());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return sequence;
        }

        /// <summary>
        ///     Show window with animation
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override Sequence Show()
        {
            var sequence = DOTween.Sequence();

            var type = AnimationType ?? defaultAnimationType;
            switch (type)
            {
                case AnimationWindowType.Alpha:
                    sequence.Append(ShowByAlpha());
                    break;
                case AnimationWindowType.Pos:
                    sequence.Append(ShowByPos());
                    break;
                case AnimationWindowType.All:
                    sequence.Append(ShowByAlphaAndPos());

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return sequence;
        }

        #region AlphaAndPos

        private Sequence ShowByAlphaAndPos()
        {
            windowTransform.anchoredPosition = Vector2.zero;
            windowTransform.anchoredPosition = GetPos(showSide);
            var sequence = DOTween.Sequence();
            sequence.Append(base.Show());
            sequence.Join(canvasGroup.DOFade(1, alfaTime).SetEase(Ease.OutQuad));
            sequence.Join(windowTransform.DOAnchorPos(Vector2.zero, posTime).SetEase(Ease.OutQuad));
            return sequence;
        }


        private Sequence HideByAlphaAndPos()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(base.Show());
            sequence.Join(canvasGroup.DOFade(0, alfaTime - 0.2f).SetEase(Ease.OutQuad));
            sequence.Join(windowTransform.DOAnchorPos(GetPos(hideSide), posTime + 0.2f).SetEase(Ease.OutQuad));
            return sequence;
        }

        #endregion

        #region Alpha

        private Sequence ShowByAlpha()
        {
            windowTransform.anchoredPosition = Vector2.zero;
            var sequence = DOTween.Sequence();
            sequence.Append(base.Show());
            sequence.Append(canvasGroup.DOFade(1, 0.3f).SetEase(Ease.OutQuad));
            return sequence;
        }

        private Sequence HideByAlpha()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(0, 0.3f).SetEase(Ease.InQuad));
            sequence.Append(base.Hide());
            return sequence;
        }

        private void ShowFastByAlpha()
        {
            base.ShowFast();
            windowTransform.anchoredPosition = Vector2.zero;
            canvasGroup.alpha = 1;
        }

        private void HideFastByAlpha()
        {
            base.HideFast();
            canvasGroup.alpha = 0;
        }

        #endregion

        #region Pos

        private Vector2 GetPos(Side side)
        {
            return side switch
            {
                Side.Left => new Vector2(-GetParentSize.width * multiplierPos, 0),
                Side.Right => new Vector2(GetParentSize.width * multiplierPos, 0),
                Side.Top => new Vector2(0, GetParentSize.height * multiplierPos),
                Side.Bottom => new Vector2(0, -GetParentSize.height * multiplierPos),
                _ => Vector2.zero
            };
        }

        private void HideFastByPos()
        {
            base.HideFast();

            windowTransform.anchoredPosition = GetPos(hideSide);
        }

        private void ShowFastByPos()
        {
            canvasGroup.alpha = 1;
            windowTransform.anchoredPosition = Vector2.zero;
            base.ShowFast();
        }

        private Sequence HideByPos()
        {
            windowTransform.anchoredPosition = Vector2.zero;
            var sequence = DOTween.Sequence();

            var tween = windowTransform.DOAnchorPos(GetPos(hideSide), 0.3f);
            tween.SetEase(Ease.OutQuad);

            sequence.Append(tween);
            sequence.Append(base.Hide());
            return sequence;
        }

        private Sequence ShowByPos()
        {
            canvasGroup.alpha = 1;
            windowTransform.anchoredPosition = GetPos(showSide);
            var sequence = DOTween.Sequence();
            sequence.Append(base.Show());
            var tween = windowTransform.DOAnchorPos(Vector2.zero, 0.3f);
            tween.SetEase(Ease.OutQuad);
            sequence.Append(tween);
            return sequence;
        }

        #endregion
    }


    public enum AnimationWindowType
    {
        Alpha = 0,
        Pos = 1,
        All = 2
    }
}
using DG.Tweening;
using UnityEngine;

namespace GameSoft.Windows
{
    /// <summary>
    ///     Base window with animation
    /// </summary>
    public abstract class BaseWindowAnimatedAlpha : BaseWindowAnimated
    {
        [Header("Animations")] [SerializeField]
        protected CanvasGroup canvasGroup;

#if UNITY_EDITOR
        private void Reset()
        {
            if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
#endif
        /// <summary>
        ///     Hide window without animation
        /// </summary>
        public override void HideFast()
        {
            base.HideFast();
            canvasGroup.alpha = 0;
        }

        /// <summary>
        ///     Show window without animation
        /// </summary>
        public override void ShowFast()
        {
            base.ShowFast();
            canvasGroup.alpha = 1;
        }

        /// <summary>
        ///     Hide window with animation
        /// </summary>
        /// <returns></returns>
        public override Sequence Hide()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(0, 0.3f).SetEase(Ease.InQuad));
            sequence.Append(base.Hide());
            return sequence;
        }

        /// <summary>
        ///     Show window with animation
        /// </summary>
        /// <returns></returns>
        public override Sequence Show()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(base.Show());
            sequence.Append(canvasGroup.DOFade(1, 0.3f).SetEase(Ease.OutQuad));
            return sequence;
        }
    }
}
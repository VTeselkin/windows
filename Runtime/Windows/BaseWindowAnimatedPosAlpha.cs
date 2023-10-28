using DG.Tweening;
using GameSoft.Windows.Extensions;
using UnityEngine;

namespace GameSoft.Windows
{
    /// <summary>
    ///     Base window with animation
    /// </summary>
    public class BaseWindowAnimatedPosAlpha : BaseWindowAnimated
    {
        [Header("Animations")] [SerializeField]
        protected CanvasGroup canvasGroup;

        [SerializeField] private Side showSide = Side.Top;
        [SerializeField] private Side hideSide = Side.Bottom;
        [SerializeField] private float multiplierPos = 1f;

        /// <summary>
        ///     Initialize window
        /// </summary>
        /// <param name="manager"></param>
        public override void InitializeWindow(WindowsManager manager)
        {
            base.InitializeWindow(manager);
            windowTransform ??= GetComponent<RectTransform>();
        }

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

        /// <summary>
        ///     Hide window fast
        /// </summary>
        public override void HideFast()
        {
            base.HideFast();
            canvasGroup.alpha = 0;
        }

        /// <summary>
        ///     Show window fast
        /// </summary>
        public override void ShowFast()
        {
            canvasGroup.alpha = 1;
            windowTransform.anchoredPosition = Vector2.zero;
            base.ShowFast();
        }

        /// <summary>
        ///     Hide window with animation
        /// </summary>
        /// <returns></returns>
        public override Sequence Hide()
        {
            var sequence = DOTween.Sequence();

            var tween = windowTransform.DOAnchorPos(GetPos(hideSide), 0.3f);
            tween.SetEase(Ease.InQuad);
            tween.OnStart(() => windowTransform.anchoredPosition = Vector2.zero);

            sequence.Append(tween);
            sequence.Join(canvasGroup.DOFade(0, 0.3f).SetEase(Ease.InQuart));
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

            var tween = windowTransform.DOAnchorPos(Vector2.zero, 0.3f);
            tween.SetEase(Ease.OutQuad);
            tween.OnStart(() => windowTransform.anchoredPosition = GetPos(showSide));

            sequence.Append(tween);
            sequence.Join(canvasGroup.DOFade(1, 0.3f).SetEase(Ease.OutQuart));
            return sequence;
        }
    }
}
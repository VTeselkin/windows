using DG.Tweening;
using GameSoft.Windows.Extensions;
using UnityEngine;

namespace GameSoft.Windows
{
    /// <summary>
    ///     Base window with animation
    /// </summary>
    public class BaseWindowAnimatedPosWithDrag : BaseWindowAnimated
    {
        [SerializeField] protected Side showSide = Side.Top;
        [SerializeField] protected Side hideSide = Side.Bottom;
        [SerializeField] protected float multiplierPos = 1f;
        [SerializeField] private bool drag;
        [SerializeField] private Vector2 dragPos;


        /// <summary>
        ///     Show window fast
        /// </summary>
        public override void ShowFast()
        {
            windowTransform.anchoredPosition = Vector2.zero;
            base.ShowFast();
        }

        /// <summary>
        ///     Hide window fast
        /// </summary>
        public override void HideFast()
        {
            windowTransform.anchoredPosition = GetPos(showSide);
            base.HideFast();
        }

        /// <summary>
        ///     Show window with animation
        /// </summary>
        /// <returns></returns>
        public override Sequence Show()
        {
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() =>
            {
                windowTransform.anchoredPosition = GetPos(showSide);
                _canvas.enabled = true;
            });

            var pos = drag ? Vector2.zero + dragPos : Vector2.zero;
            var tween = windowTransform.DOAnchorPos(pos, 0.5f);
            tween.SetEase(Ease.OutQuad);
            sequence.Append(tween);
            if (drag) sequence.Append(windowTransform.DOAnchorPos(Vector2.zero, 0.1f));

            return sequence;
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
            sequence.AppendCallback(() => _canvas.enabled = false);

            return sequence;
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
    }
}
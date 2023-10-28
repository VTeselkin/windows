using DG.Tweening;
using GameSoft.Windows.Extensions;
using UnityEngine;

namespace GameSoft.Windows
{
    /// <summary>
    ///     Base window with animation
    /// </summary>
    public class BaseWindowAnimatedPos : BaseWindowAnimated
    {
        [SerializeField] protected Side showSide = Side.Top;
        [SerializeField] protected Side hideSide = Side.Bottom;
        [SerializeField] protected float multiplierPos = 1f;


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
        ///     Show window without animation
        /// </summary>
        public override void ShowFast()
        {
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
            windowTransform.anchoredPosition = Vector2.zero;
            var tween = windowTransform.DOAnchorPos(GetPos(hideSide), 0.3f);
            tween.SetEase(Ease.OutQuad);
            sequence.Append(tween);
            sequence.Append(base.Hide());
            return sequence;
        }

        /// <summary>
        ///     Show window with animation
        /// </summary>
        /// <returns></returns>
        public override Sequence Show()
        {
            windowTransform.anchoredPosition = GetPos(showSide);
            var sequence = DOTween.Sequence();
            sequence.Append(base.Show());
            var tween = windowTransform.DOAnchorPos(Vector2.zero, 0.3f);
            tween.SetEase(Ease.OutQuad);
            sequence.Append(tween);
            return sequence;
        }
    }
}
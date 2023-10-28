using UnityEngine;

namespace GameSoft.Windows
{
    public class BaseWindowAnimated : BaseWindow
    {
        [Header("Animations")] [SerializeField]
        protected RectTransform windowTransform;

        private Rect _parentSize;

        protected bool Inited;

        protected Rect GetParentSize
        {
            get
            {
                if (!Inited) _parentSize = ((RectTransform)transform.parent).rect;

                return _parentSize;
            }
        }

        /// <summary>
        ///     Initialize window
        /// </summary>
        /// <param name="manager"></param>
        public override void InitializeWindow(WindowsManager manager)
        {
            base.InitializeWindow(manager);
            windowTransform ??= GetComponent<RectTransform>();
        }
    }
}
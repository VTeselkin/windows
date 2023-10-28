using System.Collections.Generic;
using UnityEngine;

namespace GameSoft.Windows
{
    /// <summary>
    ///     WindowsConfig: ScriptableObject for caching windows
    /// </summary>
    [CreateAssetMenu(fileName = "WindowsConfig", menuName = "GameSoft/WindowsConfig", order = 0)]
    public class WindowsConfig : ScriptableObject
    {
        [SerializeField] private string pathToWindows;
        [SerializeField] private BaseWindow[] windows;
        private readonly Dictionary<string, BaseWindow> _windowsById = new();

        public string PathToWindows => pathToWindows;

        /// <summary>
        ///     Get cached window by name or create new instance
        /// </summary>
        /// <param name="windowName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWindow<T>(string windowName) where T : BaseWindow
        {
            if (_windowsById.TryGetValue(windowName, out var baseWindowCache)) return baseWindowCache as T;
            foreach (var t in windows)
            {
                if (t is not T) continue;
                _windowsById.Add(windowName, t);
                break;
            }

            return _windowsById.TryGetValue(windowName, out var baseWindow) ? baseWindow as T : null;
        }

        public string PathToWindow(BaseWindow window)
        {
            return $"{pathToWindows}/{window.name}";
        }
    }
}
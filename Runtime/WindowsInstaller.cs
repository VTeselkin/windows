using GameSoft.Windows;
using UnityEngine;
using Zenject;

namespace GameSoft
{
    public class WindowsInstaller : MonoInstaller<WindowsInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<WindowsManager>().FromComponentOn(GameObject.Find("WindowsManager"))
                .AsSingle();
        }
    }
}
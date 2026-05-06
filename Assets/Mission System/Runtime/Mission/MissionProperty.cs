using System;
using UnityEngine;

namespace Tomoe.MissionSystem.Runtime
{
    [Serializable]
    public class MissionProperty
    {
        [SerializeField] private bool isVisible;
        
        public bool IsVisible => isVisible;
    }
}
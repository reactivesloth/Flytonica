using System;
using UnityEngine;

namespace Code.Internal.UserInterface
{
    public class StyleConstants: MonoBehaviour
    {
        public static StyleConstants Instance { get; private set; }

        [field: SerializeField] public Color Green { get; private set; }
        [field: SerializeField] public Color Red { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements
{
    public class InteractiveObjectView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private List<SpriteChangeElement> spriteChangeElements;
        [SerializeField] private List<ColorChangeElement> colorChangeElements;

        private State _state = State.Non;

        public State State => _state;

        public event Action OnPressAction;

        public void OnPress()
        {
            OnPressAction?.Invoke();

            if (_state == State.Selected)
                ToNormal();
            else
                Select();
        }

        public void Hover()
        {
            if (_state == State.Selected)
                return;

            _state = State.Hover;
            spriteChangeElements.ForEach(s => s.SetHover());
            colorChangeElements.ForEach(s => s.SetHover());
        }

        public void Unhover()
        {
            if (_state == State.Hover)
            {
                _state = State.Non;
                ToNormal();
            }
        }

        public void Select()
        {
            _state = State.Selected;
            spriteChangeElements.ForEach(s => s.SetSelected());
            colorChangeElements.ForEach(s => s.SetSelected());
        }

        public void ToNormal()
        {
            _state = State.Non;
            spriteChangeElements.ForEach(s => s.SetNormal());
            colorChangeElements.ForEach(s => s.SetNormal());
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Hover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Unhover();
        }
    }

    public enum State
    {
        Non,
        Hover,
        Selected
    }

    [Serializable]
    public class ColorChangeElement
    {
        [SerializeField] private Graphic element;
        [SerializeField] private Color normal = Color.white, hover = Color.white, selected = Color.black;

        public void SetNormal() => SetColor(normal);
        
        public void SetHover()=> SetColor(hover);
        
        public void SetSelected()=> SetColor(selected);
        
        private void SetColor(Color color)
        {
            if(!element) return;
            element.color = color;
        }
    }
    
    [Serializable]
    public class SpriteChangeElement
    {
        [SerializeField] private Image element;
        [SerializeField] private Sprite normal, hover, selected;
        
        public void SetNormal() =>
            SetSprite(normal);


        public void SetHover() =>
            SetSprite(hover);

        public void SetSelected() =>
            SetSprite(selected);

        private void SetSprite(Sprite sprite)
        {
            if(!sprite) return;
            element.sprite = sprite;
        }
    }
}
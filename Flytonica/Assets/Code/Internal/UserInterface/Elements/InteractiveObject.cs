using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements
{
    public class InteractiveObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private List<SpriteChangeElement> spriteChangeElements;
        [SerializeField] private List<ColorChangeElement> colorChangeElements;

        private State _state;

        public bool Interactable { get; set; } = true;

        public State State
        {
            get => _state;
            private set
            {
                if (!Enum.IsDefined(typeof(State), value))
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(State));
                _state = value;
                StateChanged?.Invoke(State);
            }
        }

        public event Action SelectAction;
        public event Action UnselectAction;

        public event Action<State> StateChanged;

        public void OnPress()
        {
            if(!Interactable)
                return;
            
            if (State == State.Selected)
                ToNormal();
            else
                Select();
        }

        public void Hover()
        {
            if (State == State.Selected)
                return;

            State = State.Hover;
            spriteChangeElements.ForEach(s => s.SetHover());
            colorChangeElements.ForEach(s => s.SetHover());
        }

        public void Unhover()
        {
            if (State == State.Hover)
            {
                State = State.Non;
                ToNormal();
            }
        }

        public void SelectWithoutNotify()
        {
            State = State.Selected;
            spriteChangeElements.ForEach(s => s.SetSelected());
            colorChangeElements.ForEach(s => s.SetSelected());
        }
        
        public void Select()
        {
            SelectAction?.Invoke();
            State = State.Selected;
            spriteChangeElements.ForEach(s => s.SetSelected());
            colorChangeElements.ForEach(s => s.SetSelected());
        }

        public void ToNormal()
        {
            if(State == State.Selected)
                UnselectAction?.Invoke();
            State = State.Non;
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

        public void OnPointerClick(PointerEventData eventData)
        {
            OnPress();
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
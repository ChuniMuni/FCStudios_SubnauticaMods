﻿using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Enums;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCSAlterraIndustrialSolutions.Models.Buttons
{
    /// <summary>
    /// This class is a component for all interface buttons except the color picker and the paginator.
    /// For the color picker see the <see cref="ColorItemButton"/>
    /// For the paginator see the <see cref="PaginatorButton"/> 
    /// </summary>
    public class InterfaceButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        #region Public Properties

        /// <summary>
        /// The pages to change to.
        /// </summary>
        public GameObject ChangePage { get; set; }
        public string BtnName { get; set; }
        public Color HOVER_COLOR { get; set; } = new Color(0.07f, 0.38f, 0.7f, 1f);
        public Color STARTING_COLOR { get; set; } = Color.white;
        public InterfaceButtonMode ButtonMode { get; set; } = InterfaceButtonMode.Background;
        public Text TextComponent { get; set; }
        public int SmallFont { get; set; } = 140;
        public int LargeFont { get; set; } = 180;
        public object Tag { get; set; }
        public float IncreaseButtonBy { get; set; }

        #endregion

        #region Public Methods
        
        public void Start()
        {
            if (GetComponent<Image>() != null)
            {
                if (ButtonMode != InterfaceButtonMode.None)
                {
                    GetComponent<Image>().color = STARTING_COLOR;
                }
                else
                {
                    GetComponent<Image>().color = new Color(1,1,1,0);

                }
            }
        }

        public void OnEnable()
        {
            switch (ButtonMode)
            {
                case InterfaceButtonMode.TextScale:
                    TextComponent.fontSize = TextComponent.fontSize;
                    break;
                case InterfaceButtonMode.TextColor:
                    TextComponent.color = STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    if (GetComponent<Image>() != null)
                    {
                        GetComponent<Image>().color = STARTING_COLOR;
                    }
                    break;
                case InterfaceButtonMode.BackgroundScale:
                    if (gameObject != null)
                    {
                        gameObject.transform.localScale = gameObject.transform.localScale;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        #region Public Overrides
        public override void OnDisable()
        {
            base.OnDisable();

            switch (ButtonMode)
            {
                case InterfaceButtonMode.TextScale:
                    TextComponent.fontSize = TextComponent.fontSize;
                    break;
                case InterfaceButtonMode.TextColor:
                    TextComponent.color = STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    if (GetComponent<Image>() != null)
                    {
                        GetComponent<Image>().color = STARTING_COLOR;
                    }
                    break;
                case InterfaceButtonMode.BackgroundScale:
                    if (gameObject != null)
                    {
                        gameObject.transform.localScale = gameObject.transform.localScale;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsHovered)
            {
                Log.Info($"Button Mode: {ButtonMode}");

                switch (ButtonMode)
                {
                    case InterfaceButtonMode.TextScale:
                        TextComponent.fontSize = LargeFont;
                        break;
                    case InterfaceButtonMode.TextColor:
                        TextComponent.color = HOVER_COLOR;
                        break;
                    case InterfaceButtonMode.Background:
                        if (GetComponent<Image>() != null)
                        {
                            GetComponent<Image>().color = HOVER_COLOR;
                        }
                        break;
                    case InterfaceButtonMode.BackgroundScale:
                        if (gameObject != null)
                        {
                            gameObject.transform.localScale +=
                                new Vector3(IncreaseButtonBy, IncreaseButtonBy, IncreaseButtonBy);
                        }
                        break;
                }
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            switch (ButtonMode)
            {
                case InterfaceButtonMode.TextScale:
                    TextComponent.fontSize = SmallFont;
                    break;
                case InterfaceButtonMode.TextColor:
                    TextComponent.color = STARTING_COLOR;
                    break;
                case InterfaceButtonMode.Background:
                    if (GetComponent<Image>() != null)
                    {
                        GetComponent<Image>().color = STARTING_COLOR;
                    }
                    break;
                case InterfaceButtonMode.BackgroundScale:
                    if (gameObject != null)
                    {
                        gameObject.transform.localScale -=
                            new Vector3(IncreaseButtonBy, IncreaseButtonBy, IncreaseButtonBy);
                    }
                    break;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (IsHovered)
            {
                Log.Info($"Clicked Button: {BtnName}");
                Display.OnButtonClick(BtnName, Tag);
            }
        }
        #endregion

    }
}

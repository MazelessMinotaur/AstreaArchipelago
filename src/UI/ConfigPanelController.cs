﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace AstreaArchipelago.src.UI
{
    internal class ConfigPanelController : MonoBehaviour
    {

        private Label _label;
        //private Button _button;
        //private Toggle _toggle;

        private int _clickCount;

        public void Init()
        {
            //VisualTreeAsset v = Resources.Load<VisualTreeAsset>("ConfigPanelUI");
        }

        //Add logic that interacts with the UI controls in the `OnEnable` methods
        private void OnEnable()
        {
            //// The UXML is already instantiated by the UIDocument component
            //var uiDocument = GetComponent<>();

            //_button = uiDocument.rootVisualElement.Q("button") as Button;
            //_toggle = uiDocument.rootVisualElement.Q("toggle") as Toggle;

            //_button.RegisterCallback<ClickEvent>(PrintClickMessage);

            //var _inputFields = uiDocument.rootVisualElement.Q("input-message");
            //_inputFields.RegisterCallback<ChangeEvent<string>>(InputMessage);
        }

        //private void OnDisable()
        //{
        //    _button.UnregisterCallback<ClickEvent>(PrintClickMessage);
        //}

        //private void PrintClickMessage(ClickEvent evt)
        //{
        //    ++_clickCount;

        //    Debug.Log($"{"button"} was clicked!" +
        //            (_toggle.value ? " Count: " + _clickCount : ""));
        //}

        //public static void InputMessage(ChangeEvent<string> evt)
        //{
        //    Debug.Log($"{evt.newValue} -> {evt.target}");
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.Events;
using Button = UnityEngine.UI.Button;
using BepInEx.Logging;
using Image = UnityEngine.UI.Image;

namespace AstreaArchipelago.src.UI
{
    internal class GeneratedUI : MonoBehaviour
    {
        private GameObject uiCanvas;
        private InputField usernameInput;
        private InputField passwordInput;
        private Button submitButton;
        internal static new ManualLogSource Logger;


        static bool created = false;

        public GeneratedUI(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void Start()
        {
            if (!created)
            {
                CreateUI();
            }
            created = true;
        }

        private void CreateUI()
        {
            return;
            // Create Canvas
            Logger.LogInfo("ui 1");
            uiCanvas = new GameObject("CustomUICanvas");
            Canvas canvas = uiCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.AddComponent<CanvasScaler>();
            uiCanvas.AddComponent<GraphicRaycaster>();

            // Create Username Input Field
            usernameInput = CreateInputField("Username", new Vector2(0, 100));

            // Create Password Input Field
            passwordInput = CreateInputField("Password", new Vector2(0, 100));
            passwordInput.contentType = InputField.ContentType.Password; // Hide input
            Logger.LogInfo("ui 2");
            // Create Submit Button
            submitButton = CreateButton("Submit", new Vector2(0, 0));
            Logger.LogInfo("ui 3");
            submitButton.interactable = true;

            Logger.LogInfo("adding listeneter");
            submitButton.onClick.AddListener(OnSubmit);
            Logger.LogInfo("listeneter added");
            OnSubmit();
        }

        private InputField CreateInputField(string placeholderText, Vector2 position)
        {
            GameObject inputFieldObject = new GameObject(placeholderText + "InputField");
            inputFieldObject.transform.SetParent(uiCanvas.transform);
            RectTransform rectTransform = inputFieldObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;

            InputField inputField = inputFieldObject.AddComponent<InputField>();
            inputField.placeholder = CreatePlaceholder(placeholderText);

            // Add Text Component
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(inputFieldObject.transform);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = "place holder text";

            return inputField;
        }

        private Text CreatePlaceholder(string text)
        {
            GameObject placeholderObject = new GameObject("Placeholder");
            placeholderObject.transform.SetParent(uiCanvas.transform);
            Text placeholderText = placeholderObject.AddComponent<Text>();
            placeholderText.text = text;
            placeholderText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            placeholderText.color = Color.gray;

            return placeholderText;
        }

        private Button CreateButton(string buttonText, Vector2 position)
        {
            GameObject buttonObject = new GameObject(buttonText + "Button");
            buttonObject.transform.SetParent(uiCanvas.transform);
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(160, 30); // Set size as needed
            buttonObject.SetActive(true);

            // Create and assign an Image component
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = Color.white; // Set color as needed
            buttonImage.raycastTarget = true; // Enable raycast target


            Button button = buttonObject.AddComponent<Button>();
            button.interactable = true;
            //Text btxt = buttonObject.AddComponent<Text>();
            //btxt.text = buttonText;
            //btxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return button;
        }

        private void OnSubmit()
        {
            Logger.LogInfo($"testing");

            string username = usernameInput.text;
            string password = passwordInput.text;

            // Handle submission logic here
            Debug.Log($"Username: {username}, Password: {password}");
            Logger.LogInfo($"Username: {username}, Password: {password}");

        }
    }
}

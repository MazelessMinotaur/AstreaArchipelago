using Astrea;
using BepInEx.Logging;
using LittleLeo;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace AstreaArchipelago.src
{
    internal class ArchipelagoConfigMenu : ModSettingsButton
    {
        internal static ManualLogSource Logger;

        public ArchipelagoConfigMenu(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void Load()
        {
            Logger.LogInfo("trying to make a new menu button");
            ModSettingsButton[] m = Resources.FindObjectsOfTypeAll<ModSettingsButton>();
            if (m.Length != 1)
            {
                Logger.LogInfo("more than 1 mod button?");
                return;
            }

            ModSettingsButton button = m[0];

            Logger.LogInfo("dupping");

            ModSettingsButton dup = Instantiate(button);
            Logger.LogInfo($"positions:normal {button.gameObject.transform.localPosition.x}, mine {dup.gameObject.transform.localPosition.x}");

            Logger.LogInfo("moving");

            
            dup.gameObject.transform.localPosition = button.gameObject.transform.localPosition;
            dup.gameObject.transform.position = button.gameObject.transform.position;
            dup.gameObject.transform.localEulerAngles = button.gameObject.transform.localEulerAngles;
            dup.gameObject.transform.localScale = button.gameObject.transform.localScale;

            //Logger.LogInfo($"parents, og : {button.transform.GetParent().name}, dup: {dup.transform.GetParent().name}");
            dup.transform.SetParent( button.gameObject.transform.GetParent(), false );
            Logger.LogInfo($"parents, og : {button.transform.GetParent().name}, dup: {dup.transform.GetParent().name}");



            dup.gameObject.transform.SetLocalX(button.gameObject.transform.localPosition.x - 1);

            dup.gameObject.SetActive(true);

            Logger.LogInfo($"positions:normal {button.gameObject.transform.localPosition.x}, mine {dup.gameObject.transform.localPosition.x}");

        }

        
        public void AddMenuButton()
        {
            Astrea.BattleActions.BattleAction a;
            Logger.LogInfo("trying to make a new menu button");
            CanvasGameplay[] m = Resources.FindObjectsOfTypeAll<CanvasGameplay>();
            if (m.Length != 1)
            {
                Logger.LogInfo("more than 1 mod button?");
                return;
            }
            SettingsTabItem[] s = Resources.FindObjectsOfTypeAll<SettingsTabItem>();

            if (s.Length >= 20)
            {
                Logger.LogInfo("lots of settings, skip");
                return;

            }

            for (int i = 0; i < s.Length; i++)
            {
                SettingsTabItem original = s[i];
                SettingsTabItem dup =  Instantiate(original);
                dup.gameObject.SetActive(true);
                dup.transform.SetParent(original.gameObject.transform.GetParent(), false);

                dup.gameObject.transform.position = original.gameObject.transform.position;
                dup.gameObject.transform.SetLocalX(original.gameObject.transform.localPosition.y - 1);

            }

            GameObject myGO;

            myGO = new GameObject();
            myGO.name = "TestCanvas";
            myGO.AddComponent<Canvas>();
            Canvas myCanvas;

            myCanvas = myGO.GetComponent<Canvas>();
            myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            myGO.AddComponent<CanvasScaler>();
            myGO.AddComponent<GraphicRaycaster>();

            GameObject buttonObject = new GameObject("Button");
            buttonObject.transform.SetParent(myCanvas.transform);
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = myCanvas.transform.position;
            rectTransform.sizeDelta = new Vector2(160, 30); // Set size as needed
            buttonObject.SetActive(true);

            // Create and assign an Image component
            Image buttonImage = buttonObject.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = Color.white; // Set color as needed
            buttonImage.raycastTarget = true; // Enable raycast target


            Button button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(logButtonPress);

            button.interactable = true;

            //TextMesh text;
            //RectTransform rectTransform;
            //GameObject myText;

            //myText = new GameObject();
            //myText.transform.parent = myGO.transform;
            //myText.name = "wibble";
            //text = myText.AddComponent<TextMesh>();
            //text.text = "wobble";
            //text.fontSize = 85;

            //// Text position
            //rectTransform = text.GetComponent<RectTransform>();
            //rectTransform.localPosition = new Vector3(0, 0, 0);
            //rectTransform.sizeDelta = new Vector2(400, 200);

            //SettingsTabItem si = s[0];
            //myGO.transform.SetParent(si.transform);
        }

        void logButtonPress()
        {
            Logger.LogInfo("button?");
        }
    }
}

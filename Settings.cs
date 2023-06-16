using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace D86_CE
{
    public class Settings : MonoBehaviour
    {
        #region Variables
        public Main main;

        public bool discordPresence;
        public string displayName;
        public KeyCode freeCamkey;

        public float freeCamSpeed;
        public float freeCamSens;
        public float freeCamSmooth;

        private GameObject discordPresenceToggle;
        private GameObject displayNameParent;
        private GameObject displayNameLabel;
        private GameObject displayNameInput;
        #endregion

        #region Methods
        private void Start()
        {
            CreateUI();
        }

        private void CreateUI()
        {
            GameObject toggleTemplate = GameObject.Find("tglBeautify");
            Font font = toggleTemplate.GetComponentInChildren<Text>(true).font;
            Sprite checkmark = toggleTemplate.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite;

            DefaultControls.Resources resources = new DefaultControls.Resources();

            DefaultControls.Resources toggleResources = new DefaultControls.Resources();
            toggleResources.checkmark = checkmark;
            if (!discordPresenceToggle)
            {
                discordPresenceToggle = DefaultControls.CreateToggle(toggleResources);
                discordPresenceToggle.name = "DiscordPresenceToggle";

                discordPresenceToggle.GetComponent<Toggle>().isOn = discordPresence;
                discordPresenceToggle.GetComponent<Toggle>().onValueChanged.AddListener(delegate { DiscordPresenceChange(discordPresenceToggle.GetComponent<Toggle>()); });

                discordPresenceToggle.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
                discordPresenceToggle.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);

                discordPresenceToggle.GetComponentInChildren<Text>(true).GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                discordPresenceToggle.GetComponentInChildren<Text>(true).GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                discordPresenceToggle.GetComponentInChildren<Text>(true).GetComponent<RectTransform>().localPosition = new Vector2(135, -12);
                discordPresenceToggle.GetComponentInChildren<Text>(true).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
                discordPresenceToggle.GetComponentInChildren<Text>(true).gameObject.AddComponent<Shadow>();
                discordPresenceToggle.GetComponentInChildren<Text>(true).gameObject.AddComponent<Outline>();
                discordPresenceToggle.GetComponentInChildren<Text>(true).font = font;
                discordPresenceToggle.GetComponentInChildren<Text>(true).fontSize = 42;
                discordPresenceToggle.GetComponentInChildren<Text>(true).fontStyle = FontStyle.Italic;
                discordPresenceToggle.GetComponentInChildren<Text>(true).color = Color.white;
                discordPresenceToggle.GetComponentInChildren<Text>(true).text = "DiscordPresence";

                discordPresenceToggle.transform.SetParent(GameObject.Find("Settings_magictouch").transform.GetChild(1));

                discordPresenceToggle.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                discordPresenceToggle.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                discordPresenceToggle.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                discordPresenceToggle.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                discordPresenceToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, -70);
                discordPresenceToggle.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 55);
            }
            if (!displayNameParent)
            {
                displayNameParent = DefaultControls.CreatePanel(resources);
                displayNameParent.name = "DisplayName";

                displayNameParent.GetComponent<Image>().color = Color.clear;

                displayNameParent.AddComponent<HorizontalLayoutGroup>();
                displayNameParent.GetComponent<HorizontalLayoutGroup>().spacing = 5;
                displayNameParent.GetComponent<HorizontalLayoutGroup>().childControlHeight = true;
                displayNameParent.GetComponent<HorizontalLayoutGroup>().childScaleHeight = true;
                displayNameParent.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = true;
                displayNameParent.GetComponent<HorizontalLayoutGroup>().childControlWidth = false;
                displayNameParent.GetComponent<HorizontalLayoutGroup>().childScaleWidth = false;
                displayNameParent.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;

                displayNameParent.transform.SetParent(GameObject.Find("Settings_magictouch").transform.GetChild(1));

                displayNameParent.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                displayNameParent.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                displayNameParent.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                displayNameParent.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                displayNameParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(310, -110);
                displayNameParent.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 55);
            }
            if (!displayNameLabel)
            {
                displayNameLabel = DefaultControls.CreateText(resources);

                displayNameLabel.GetComponent<Text>().text = "DisplayName";
                displayNameLabel.GetComponent<Text>().gameObject.AddComponent<Shadow>();
                displayNameLabel.GetComponent<Text>().gameObject.AddComponent<Outline>();
                displayNameLabel.GetComponent<Text>().font = font;
                displayNameLabel.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
                displayNameLabel.GetComponent<Text>().fontSize = 42;
                displayNameLabel.GetComponent<Text>().fontStyle = FontStyle.Italic;
                displayNameLabel.GetComponent<Text>().color = Color.white;
                displayNameLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(270, 55);
                displayNameLabel.GetComponent<RectTransform>().localPosition = new Vector3(-200, 0, 0);

                displayNameLabel.transform.SetParent(displayNameParent.transform);
            }
            if (!displayNameInput)
            {
                displayNameInput = DefaultControls.CreateInputField(resources);
                displayNameInput.name = "DisplayNameInput";

                displayNameInput.GetComponent<InputField>().text = displayName;
                displayNameInput.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 40);
                foreach (var text in displayNameInput.GetComponentsInChildren<Text>(true))
                {
                    text.font = font;
                    text.fontSize = 38;
                    text.alignment = TextAnchor.MiddleLeft;
                    text.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                    if (text.name == "Placeholder")
                    {
                        text.text = "Enter Displayname...";
                    }
                }
                displayNameInput.GetComponent<InputField>().onValueChanged.AddListener(delegate { DisplayNameChange(displayNameInput.GetComponent<InputField>()); });

                displayNameInput.transform.SetParent(displayNameParent.transform);
            }
        }
        private void DisplayNameChange(InputField change)
        {
            displayName = change.text;
            main.displayName.Value = displayName;
            PhotonNetwork.Disconnect();
            if (displayName == "")
            {
                string userName = FindObjectOfType<SI_GetUserData>().playername;
                PlayerPrefs.SetString("DisplayName", userName);
                PlayerPrefs.SetString("NamePickUserName", userName);
                PlayerPrefs.SetString("MyName", userName);
                PlayerProfile.NickName = userName;
            }
            else
            {
                PlayerPrefs.SetString("NamePickUserName", displayName);
                PlayerPrefs.SetString("DisplayName", displayName);
                PlayerPrefs.SetString("MyName", displayName);
                PlayerProfile.NickName = displayName;
            }
        }

        private void DiscordPresenceChange(Toggle change)
        {
            discordPresence = change.isOn;
            main.discordPresence.Value = discordPresence;
        }
        #endregion
    }
}

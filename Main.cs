using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace D86_CE
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        #region Declarations

        public const string
            MODNAME = "D86_CE",
            AUTHOR = "Va1lidUser, MageSneaky",
            GUID = "_" + MODNAME,
            VERSION = "1.0.0.0";

        internal readonly ManualLogSource log;
        internal readonly Harmony harmony;
        internal readonly Assembly assembly;
        public readonly string modFolder;

        #endregion

        #region Variables
        //Game Variables
        public static GameController gameController;
        public static SelectCarMenuUI selectCarMenuUI;
        public static CarController p_Car;

        //General
        public static bool cursorLocked = false;
        public static Settings settings;

        private bool do1Time = false;

        public ConfigEntry<bool> discordPresence;
        public ConfigEntry<string> displayName;
        public ConfigEntry<bool> customSkinLoader;

        //Freecam
        public static bool freeCamEnabled = false;
        public static GameObject freeCamHolder;
        public static Camera freeCam;

        public static ConfigEntry<KeyCode> freeCamkey;
        public static ConfigEntry<KeyCode> savedLocationskey;
        public static ConfigEntry<KeyCode> saveLocationkey;
        public static ConfigEntry<KeyCode> showPlayerList;

        public static ConfigEntry<float> freeCamSpeed;
        public static ConfigEntry<float> freeCamSens;
        public static ConfigEntry<float> freeCamSmooth;    

        //Discord Presence
        internal static DiscordRPC.RichPresence prsnc;
        public static long Timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        public float CheckInterval;
        private float CoolDown;

        #endregion

        #region Constructor
        public Main()
        {
            log = Logger;
            harmony = new Harmony(GUID);
            assembly = Assembly.GetExecutingAssembly();
            modFolder = Path.GetDirectoryName(assembly.Location);
        }
        #endregion

        #region Methods
        private void Start()
        {
            harmony.PatchAll(assembly);
            InitConfig();
            SceneManager.activeSceneChanged += OnSceneChanged;

            if (discordPresence.Value)
            {
                CheckInterval = 5;
                CoolDown = CheckInterval;

                DiscordRPC.EventHandlers eventHandlers = default;
                eventHandlers.readyCallback = (DiscordRPC.ReadyCallback)Delegate.Combine(eventHandlers.readyCallback, new DiscordRPC.ReadyCallback(ReadyCallback));
                eventHandlers.disconnectedCallback = (DiscordRPC.DisconnectedCallback)Delegate.Combine(eventHandlers.disconnectedCallback, new DiscordRPC.DisconnectedCallback(DisconnectedCallback));
                eventHandlers.errorCallback = (DiscordRPC.ErrorCallback)Delegate.Combine(eventHandlers.errorCallback, new DiscordRPC.ErrorCallback(ErrorCallback));

                DiscordRPC.Initialize("985589313649668137", ref eventHandlers, true, "0612");
                prsnc = default;
                SetStatus();
                ReadyCallback();
            }
        }

        private void Update()
        {
            foreach (var scrollRect in FindObjectsOfType<ScrollRect>())
            {
                scrollRect.scrollSensitivity = 50;
            }
            if (CoolDown > 0)
                CoolDown -= Time.deltaTime;
            else
            {
                CoolDown = CheckInterval;
                CheckDiscordStatus();
            }
            if (SceneManager.GetActiveScene().name == "MainMenuScene")
            {
                if (!settings)
                {
                    if (GameObject.Find("Settings_magictouch"))
                    {
                        settings = GameObject.Find("Settings_magictouch").AddComponent<Settings>();
                        settings.main = this;
                        settings.discordPresence = discordPresence.Value;
                        settings.displayName = displayName.Value;
                        settings.freeCamkey = freeCamkey.Value;
                        settings.freeCamSpeed = freeCamSpeed.Value;
                        settings.freeCamSens = freeCamSens.Value;
                        settings.freeCamSmooth = freeCamSmooth.Value;
                    }
                }
                if (!do1Time)
                {
                    if (displayName.Value == "")
                    {
                        string userName = FindObjectOfType<SI_GetUserData>().playername;
                        PlayerPrefs.SetString("DisplayName", userName);
                        PlayerPrefs.SetString("NamePickUserName", userName);
                        PlayerPrefs.SetString("MyName", userName);
                        PlayerProfile.NickName = userName;
                    }
                    else
                    {
                        PlayerPrefs.SetString("NamePickUserName", displayName.Value);
                        PlayerPrefs.SetString("DisplayName", displayName.Value);
                        PlayerPrefs.SetString("MyName", displayName.Value);
                        PlayerProfile.NickName = displayName.Value;
                    }
                    do1Time = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                cursorLocked = false;
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                switch (cursorLocked)
                {
                    case true:
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        cursorLocked = false;
                        break;
                    case false:
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        cursorLocked = true;
                        break;
                }
            }
            if (Helper.InGame())
            {
                if (Input.GetKeyDown(freeCamkey.Value))
                {
                    if (!gameController)
                    {
                        if (FindObjectOfType<GameController>())
                        {
                            gameController = FindObjectOfType<GameController>();
                        }
                    }
                    if (!p_Car)
                    {
                        if (gameController)
                        {
                            p_Car = gameController.m_PlayerCar;
                        }
                    }
                    if (p_Car)
                    {
                        if (!p_Car.gameObject)
                        {
                            GameObject gameObject = p_Car.gameObject;
                        }
                    }
                    if (!freeCam)
                    {
                        freeCamHolder = new GameObject();
                        freeCamHolder.GetComponent<Transform>().position = p_Car.transform.position;
                        freeCamHolder.name = "FreeCam";
                        freeCamHolder.gameObject.AddComponent<Freecam>();
                        freeCamHolder.gameObject.AddComponent<Followcam>();
                        freeCamHolder.gameObject.AddComponent<Camera>();
                        freeCamHolder.AddComponent<AudioListener>();
                        freeCamHolder.gameObject.GetComponent<Freecam>().mapName = SceneManager.GetActiveScene().name.ToLower().Replace(" ", "_");
                        freeCam = freeCamHolder.gameObject.GetComponent<Camera>();
                        freeCam.farClipPlane = 10000f;
                        freeCam.nearClipPlane = 0.01f;
                        freeCam.fieldOfView = 70;
                    }
                    freeCamEnabled = !freeCamEnabled;
                    if (freeCamEnabled)
                    {
                        p_Car.enabled = false;
                        p_Car.GetComponent<Rigidbody>().isKinematic = true;
                        freeCam.enabled = true;
                        freeCamHolder.GetComponent<AudioListener>().enabled = true;
                        p_Car.GetComponent<AudioListener>().enabled = false;
                    }
                    else
                    {
                        p_Car.enabled = true;
                        p_Car.GetComponent<Rigidbody>().isKinematic = false;
                        freeCam.enabled = false;
                        p_Car.RB.velocity = new Vector3(0f, 0f, 0f);
                        Freecam freecam = freeCamHolder.GetComponent<Freecam>();
                        freeCamHolder.GetComponent<AudioListener>().enabled = false;
                        freeCamHolder.GetComponent<Followcam>().SetTargetCar(null);
                        freecam.followCameraActive = false;
                        freecam.savesListOpen = false;
                        freecam.playersListOpen = false;
                        if(freecam.savesList != null) Destroy(freecam.savesList.gameObject);
                        if (freecam.playersList != null) Destroy(freecam.playersList.gameObject);
                        freecam.savesList = null;
                        freecam.playersList = null;
                        p_Car.GetComponent<AudioListener>().enabled = true;
                    }
                }
            }
        }
        private void OnSceneChanged(Scene current, Scene next)
        {
            settings = null;
            gameController = null;
            p_Car = null;
            do1Time = false;
            freeCamHolder = null;
            freeCam = null;
        }

        private void InitConfig()
        {
            displayName = Config.Bind<string>("General", "DisplayName", "", "Displayname");
            discordPresence = Config.Bind<bool>("General", "DiscordPresence", true, "Enable Discord presence");
            freeCamkey = Config.Bind<KeyCode>("Freecam", "FreeCamKey", KeyCode.F, "Keybind for free cam");
            savedLocationskey = Config.Bind<KeyCode>("Freecam", "SavedLocationskey", KeyCode.L, "Keybind to show the saved locations list");
            saveLocationkey = Config.Bind<KeyCode>("Freecam", "SaveLocationKey", KeyCode.H, "Keybind to save the current freecam location");
            freeCamSpeed = Config.Bind<float>("Freecam", "FreeCamSpeed", 20f, "Speed of camera (Can be changed by using mouse scroll)");
            freeCamSens = Config.Bind<float>("Freecam", "FreeCamSens", 100f, "Sensetivity for freecam");
            freeCamSmooth = Config.Bind<float>("Freecam", "FreeCamSmoothness", 3f, "Smoothness of freecam");
            showPlayerList = Config.Bind<KeyCode>("Freecam", "ShowPlayerList", KeyCode.P, "Keybind to show players in current session (multiplayer)");
        }

        private string FormatString(string text)
        {
            if (text.Contains("o - Persistant"))
            {
                text = text.Replace("o - Persistant", "");
            }
            else
            {
                text = text.Replace(" - Persistant", "");
            }
            return text;
        }
        #endregion

        #region DiscordPresence
        private static void ErrorCallback(int errorCode, string message)
        {
            Debug.Log($"ErrorCallback: {errorCode}: {message}");
        }

        private static void DisconnectedCallback(int errorCode, string message)
        {
            Debug.Log($"DisconnectedCallback: {errorCode}: {message}");
        }

        private static void ReadyCallback()
        {
            Debug.Log("Discord Presence Ready");
        }

        private void CheckDiscordStatus()
        {
            if (FindObjectOfType<DriftPanelUI>())
            {
                SetStatus(SceneManager.GetActiveScene());
            }
            else
            {
                SetStatus();
            }
        }

        private void SetStatus(Scene map)
        {
            if (Helper.IsMultiplayer())
            {
                int playerCount = FindObjectOfType<SI_PersistantUserList>().PlayerNamePhoton.Count();
                prsnc.partyMax = 16;
                prsnc.partySize = playerCount;
            }
            else
            {
                prsnc.partyMax = 0;
                prsnc.partySize = 0;
            }

            prsnc.state = $"Drifting on {FormatString(map.name)} Using {Helper.GetCar()}";
            prsnc.details = $"Score: {Helper.GetScore()} Race Time: {Helper.GetRaceTime()}";
            prsnc.largeImageKey = map.name.ToLower().Replace(" ", "_");
            DiscordRPC.UpdatePresence(ref prsnc);
        }

        static void SetStatus()
        {
            prsnc.state = "In Main Menu";
            prsnc.startTimestamp = Timestamp;
            prsnc.details = null;
            prsnc.largeImageKey = null;
            prsnc.partyMax = 0;
            prsnc.partySize = 0;
            prsnc.smallImageKey = "icon";
            prsnc.smallImageText = "Drift86 by RewindApp";
            DiscordRPC.UpdatePresence(ref prsnc);
        }

        #endregion
    }
}
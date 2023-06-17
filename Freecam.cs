using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

namespace D86_CE
{
    public class Freecam : MonoBehaviour
    {
        #region[Variables]
        private float xRotation;
        private float yRotation;

        public bool followCameraActive = false;
        public int cameraSaveSlots = 0;
        string path = "";
        string fullPath = "";
        public static string directory = "/saves/";
        public string mapName = "";

        public GameObject savesList;
        public bool savesListOpen = false;

        public GameObject playersList;
        public bool playersListOpen = false;

        public MapObject map;
        #endregion

        #region Methods
        private void Start()
        {
            path = Application.dataPath;
            path += "/../";
            fullPath = path + directory + mapName + ".dat";
            map = Load();
        }

        public void Update()
        {
            float x = Input.GetAxisRaw("Mouse X") * Main.freeCamSens.Value * Time.deltaTime;
            float y = Input.GetAxisRaw("Mouse Y") * Main.freeCamSens.Value * Time.deltaTime;
            float cSpeed = Main.freeCamSpeed.Value;

            if (Main.freeCamEnabled && !savesListOpen && !playersListOpen)
            {
                yRotation += x;
                xRotation -= y;
            }
            if(Main.freeCamEnabled)
            {
                if (Input.GetKeyDown(Main.showPlayerList.Value) && Helper.IsMultiplayer())
                {
                    if (!playersList)
                    {
                        CreatePlayersList();
                    }
                    if (savesListOpen) CloseSavesList();
                    if (!playersListOpen)
                    {
                        OpenPlayerList();
                    }
                    else
                    {
                        ClosePlayerList();
                    }
                }
            }
            if (Main.freeCamEnabled && !followCameraActive)
            {
                Vector3 position = transform.position + new Vector3(0f, 0f, 0f);
                if (Input.GetKeyDown(KeyCode.PageUp))
                {
                    map = Load();
                    if (map.saves != null)
                    {
                        if (cameraSaveSlots >= map.saves.Length - 1) cameraSaveSlots = 0; else cameraSaveSlots++;
                        transform.position = map.saves[cameraSaveSlots].position;
                        transform.eulerAngles = new Vector3(map.saves[cameraSaveSlots].rX, map.saves[cameraSaveSlots].rY, 0);
                        xRotation = map.saves[cameraSaveSlots].rX;
                        yRotation = map.saves[cameraSaveSlots].rY;
                        GetComponentInChildren<Camera>().GetComponent<Transform>().position = map.saves[cameraSaveSlots].position;
                        GetComponentInChildren<Camera>().GetComponent<Transform>().eulerAngles = new Vector3(map.saves[cameraSaveSlots].rX, map.saves[cameraSaveSlots].rY, 0);
                    }
                }
                if (Input.GetKeyDown(KeyCode.PageDown))
                {
                    map = Load();
                    if (map.saves != null)
                    {
                        if (cameraSaveSlots <= 0) cameraSaveSlots = map.saves.Length - 1; else cameraSaveSlots--;
                        transform.position = map.saves[cameraSaveSlots].position;
                        transform.eulerAngles = new Vector3(map.saves[cameraSaveSlots].rX, map.saves[cameraSaveSlots].rY, 0);
                        xRotation = map.saves[cameraSaveSlots].rX;
                        yRotation = map.saves[cameraSaveSlots].rY;
                        GetComponentInChildren<Camera>().GetComponent<Transform>().position = map.saves[cameraSaveSlots].position;
                        GetComponentInChildren<Camera>().GetComponent<Transform>().eulerAngles = new Vector3(map.saves[cameraSaveSlots].rX, map.saves[cameraSaveSlots].rY, 0);
                    }
                }
                if (Input.GetKeyDown(Main.saveLocationkey.Value))
                {
                    map = Load();
                    List<PosAndRot> p = new List<PosAndRot>();
                    if (map.saves != null)
                    {
                        p = map.saves.ToList();
                    }
                    PosAndRot s = new PosAndRot { position = transform.position, rX = transform.eulerAngles.x, rY = transform.eulerAngles.y };
                    Debug.Log($"{s.position.x} | {s.position.y} | {s.position.z} | {s.rX} | {s.rY}");
                    p.Add(s);
                    MapObject newMap = new MapObject();
                    newMap.mapName = mapName;
                    newMap.saves = p.ToArray();
                    Save(newMap);
                }
                if (Input.GetKeyDown(Main.savedLocationskey.Value))
                {
                    if (!savesList)
                    {
                        CreateSavesList();
                    }
                    if (playersListOpen) ClosePlayerList();
                    if (!savesListOpen)
                    {
                        OpenSavesList();
                    }
                    else
                    {
                        CloseSavesList();
                    }
                }
                Main.freeCamSpeed.Value += Input.mouseScrollDelta.y;
                Main.freeCamSpeed.Value = Mathf.Clamp(Main.freeCamSpeed.Value, 1, 1000);
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    cSpeed = Main.freeCamSpeed.Value * 2f;
                }
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    cSpeed = Main.freeCamSpeed.Value / 2f;
                }
                if (Input.GetKey(KeyCode.W))
                {
                    position = (transform.position += transform.forward * Time.deltaTime * cSpeed);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    position = (transform.position -= transform.forward * Time.deltaTime * cSpeed);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    position = (transform.position -= transform.right * Time.deltaTime * cSpeed);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    position = (transform.position += transform.right * Time.deltaTime * cSpeed);
                }
                if (Input.GetKey(KeyCode.E))
                {
                    position = (transform.position += transform.up * Time.deltaTime * cSpeed);
                }
                if (Input.GetKey(KeyCode.Q))
                {
                    position = (transform.position -= transform.up * Time.deltaTime * cSpeed);
                }

                xRotation = Mathf.Clamp(xRotation, -90, 90);
                Quaternion targetRotation = Quaternion.Euler(xRotation, yRotation, 0f);

                transform.rotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * Main.freeCamSmooth.Value);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
            }

            if (Main.freeCamEnabled && Input.GetKeyDown(KeyCode.Mouse0) && !followCameraActive && !savesListOpen && !playersListOpen)
            {
                RaycastHit hit;

                if (Physics.SphereCast(transform.position, 1, transform.forward, out hit, 1000))
                {
                    if (hit.collider.tag == "Car")
                    {
                        GetComponent<Followcam>().SetTargetCar(hit.transform);
                        GetComponent<Followcam>().enabled = true;
                        followCameraActive = true;
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0) && followCameraActive && !savesListOpen && !playersListOpen)
            {
                DisableFollowCamera();
            }
        }

        private void OpenSavesList()
        {
            savesListOpen = true;
            Main.cursorLocked = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GetLocations();
        }

        private void OpenPlayerList()
        {
            playersListOpen = true;
            Main.cursorLocked = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GetPlayers();
        }

        private void CloseSavesList()
        {
            savesListOpen = false;
            savesList.SetActive(false);
            Main.cursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void ClosePlayerList()
        {
            playersListOpen = false;
            playersList.SetActive(false);
            Main.cursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void GetLocations()
        {
            map = Load();
            int i = 1;
            foreach (Transform child in savesList.transform.GetChild(0).GetChild(0))
            {
                Destroy(child.gameObject);
            }
            if (map.saves != null)
            {
                foreach (var save in map.saves)
                {
                    DefaultControls.Resources Resources = new DefaultControls.Resources();

                    GameObject parent = DefaultControls.CreatePanel(Resources);
                    Destroy(parent.GetComponent<Image>());
                    parent.AddComponent<HorizontalLayoutGroup>();
                    parent.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
                    parent.GetComponent<HorizontalLayoutGroup>().childControlWidth = false;
                    parent.GetComponent<HorizontalLayoutGroup>().childControlHeight = true;
                    parent.GetComponent<HorizontalLayoutGroup>().childScaleWidth = true;
                    parent.GetComponent<HorizontalLayoutGroup>().childScaleHeight = true;
                    parent.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = true;
                    parent.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = true;

                    parent.transform.SetParent(savesList.transform.GetChild(0).GetChild(0));

                    parent.GetComponent<RectTransform>().sizeDelta = new Vector2(430, 60);
                    parent.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    GameObject newButton = DefaultControls.CreateButton(Resources);
                    newButton.name = "LoadSaveButton " + i;

                    newButton.GetComponent<Button>().onClick.AddListener(delegate { LoadSelectedLocation(save); });

                    newButton.GetComponentInChildren<Text>().text = $"{i}. Pos: X: {save.position.x} Y: {save.position.y} z: {save.position.z} Rot: X: {save.rX} Y: {save.rY}";
                    newButton.GetComponentInChildren<Text>().fontSize = 20;
                    newButton.GetComponentInChildren<Text>().color = Color.white;

                    newButton.transform.SetParent(parent.transform);

                    newButton.GetComponent<RectTransform>().sizeDelta = new Vector2(348.75f, 60);
                    newButton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    ColorBlock colors = newButton.GetComponent<Button>().colors;
                    colors.normalColor = new Color32(63, 63, 63, 255);
                    colors.highlightedColor = new Color32(75, 75, 75, 255);
                    colors.pressedColor = new Color32(90, 90, 90, 255);
                    colors.selectedColor = new Color32(75, 75, 75, 255);
                    newButton.GetComponent<Button>().colors = colors;

                    GameObject deleteButton = DefaultControls.CreateButton(Resources);
                    deleteButton.name = "DeleteButton";

                    deleteButton.GetComponent<Button>().onClick.AddListener(delegate { DeleteSelectedLocation(save); });

                    deleteButton.GetComponentInChildren<Text>().text = "X";
                    deleteButton.GetComponentInChildren<Text>().fontSize = 25;
                    deleteButton.GetComponentInChildren<Text>().color = Color.white;

                    deleteButton.transform.SetParent(parent.transform);

                    deleteButton.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
                    deleteButton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    colors = deleteButton.GetComponent<Button>().colors;
                    colors.normalColor = new Color32(63, 63, 63, 255);
                    colors.highlightedColor = new Color32(75, 75, 75, 255);
                    colors.pressedColor = new Color32(90, 90, 90, 255);
                    colors.selectedColor = new Color32(75, 75, 75, 255);
                    deleteButton.GetComponent<Button>().colors = colors;

                    i++;
                }
            }
            savesList.SetActive(true);
        }

        private void GetPlayers()
        {
            foreach (Transform child in playersList.transform.GetChild(0).GetChild(0))
            {
                Destroy(child.gameObject);
            }
            MultiplayerCarController[] players = FindObjectsOfType<MultiplayerCarController>();
            foreach (MultiplayerCarController player in players)
            {
                DefaultControls.Resources Resources = new DefaultControls.Resources();

                GameObject newButton = DefaultControls.CreateButton(Resources);
                string playerName = player.GetComponent<PhotonView>().Owner.NickName;
                newButton.name = "LoadSaveButton " + playerName;

                newButton.GetComponent<Button>().onClick.AddListener(delegate { SelectCar(player.transform); });

                newButton.GetComponentInChildren<Text>().text = $"{playerName}";
                newButton.GetComponentInChildren<Text>().fontSize = 20;
                newButton.GetComponentInChildren<Text>().color = Color.white;

                newButton.transform.SetParent(playersList.transform.GetChild(0).GetChild(0));

                newButton.GetComponent<RectTransform>().sizeDelta = new Vector2(430, 60);
                newButton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                ColorBlock colors = newButton.GetComponent<Button>().colors;
                colors.normalColor = new Color32(63, 63, 63, 255);
                colors.highlightedColor = new Color32(75, 75, 75, 255);
                colors.pressedColor = new Color32(90, 90, 90, 255);
                colors.selectedColor = new Color32(75, 75, 75, 255);
                newButton.GetComponent<Button>().colors = colors;
            }
            playersList.SetActive(true);
        }

        private void DisableFollowCamera()
        {
            GetComponent<Followcam>().SetTargetCar(null);
            GetComponent<Followcam>().enabled = false;
            followCameraActive = false;
        }

        private void LoadSelectedLocation(PosAndRot p)
        {
            transform.position = p.position;
            transform.eulerAngles = new Vector3(p.rX, p.rY, 0);
            xRotation = p.rX;
            yRotation = p.rY;
            GetComponentInChildren<Camera>().GetComponent<Transform>().position = p.position;
            GetComponentInChildren<Camera>().GetComponent<Transform>().eulerAngles = new Vector3(p.rX, p.rY, 0);
        }

        private void SelectCar(Transform player)
        {
            if(player != null)
            {
                GetComponent<Followcam>().SetTargetCar(player.transform);
                GetComponent<Followcam>().enabled = true;
                followCameraActive = true;
            }
        }

        private void DeleteSelectedLocation(PosAndRot p)
        {
            map = Load();
            MapObject newMap = new MapObject();
            newMap.mapName = mapName;
            List<PosAndRot> newP = new List<PosAndRot>();
            foreach (PosAndRot save in map.saves)
            {
                if(save.position.x != p.position.x && save.position.y != p.position.y && save.position.z != p.position.z && save.rX != p.rX && save.rY != p.rY)
                {
                    newP.Add(save);
                }
            }
            newMap.saves = newP.ToArray();
            Save(newMap);
            GetLocations();
        }

        private void CreateSavesList()
        {
            DefaultControls.Resources Resources = new DefaultControls.Resources();

            savesList = DefaultControls.CreateScrollView(Resources);
            savesList.name = "SavesList";

            savesList.GetComponent<ScrollRect>().horizontal = false;
            savesList.GetComponent<ScrollRect>().scrollSensitivity = 50;
            savesList.GetComponent<Image>().color = new Color32(0, 0, 0, 180);

            savesList.transform.SetParent(GameObject.Find("CarStateCanvas").transform);

            savesList.transform.GetChild(0).GetChild(0).gameObject.AddComponent<VerticalLayoutGroup>();
            savesList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().spacing = 5;
            savesList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().padding = new RectOffset(10, 10, 10, 10);
            savesList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().childControlWidth = true;
            savesList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;
            savesList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().childControlHeight = false;
            savesList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;

            savesList.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            savesList.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            savesList.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260, -300);
            savesList.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            savesList.GetComponent<RectTransform>().sizeDelta = new Vector2(430, 500);
        }

        private void CreatePlayersList()
        {
            DefaultControls.Resources Resources = new DefaultControls.Resources();

            playersList = DefaultControls.CreateScrollView(Resources);
            playersList.name = "PlayersList";

            playersList.GetComponent<ScrollRect>().horizontal = false;
            playersList.GetComponent<ScrollRect>().scrollSensitivity = 50;
            playersList.GetComponent<Image>().color = new Color32(0, 0, 0, 180);

            playersList.transform.SetParent(GameObject.Find("CarStateCanvas").transform);

            playersList.transform.GetChild(0).GetChild(0).gameObject.AddComponent<VerticalLayoutGroup>();
            playersList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().spacing = 5;
            playersList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().padding = new RectOffset(10, 10, 10, 10);
            playersList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().childControlWidth = true;
            playersList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;
            playersList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().childControlHeight = false;
            playersList.transform.GetChild(0).GetChild(0).GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;

            playersList.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            playersList.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            playersList.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260, -300);
            playersList.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            playersList.GetComponent<RectTransform>().sizeDelta = new Vector2(430, 500);
        }

        private void Save(MapObject saveMap)
        {
            CreateFile();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(fullPath);
            bf.Serialize(file, saveMap);
            file.Close();
        }
        private MapObject Load()
        {
            CreateFile();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fullPath, FileMode.Open);
            MapObject newmap = new MapObject();
            if (file.Length > 0)
            {
                MapObject data = (MapObject)bf.Deserialize(file);
                newmap.saves = data.saves;
            }
            file.Close();
            newmap.mapName = mapName;
            return newmap;
        }

        private void CreateFile()
        {
            string dir = path + directory;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(fullPath))
            {
                Debug.Log($"Save File Does Not Exist Create A New One at {fullPath}");
                FileStream file = File.Create(fullPath);
                file.Close();
            }
        }
        #endregion

        #region Classes
        [System.Serializable]
        public class MapObject
        {
            public string mapName;
            public PosAndRot[] saves;
        }

        [System.Serializable]
        public class PosAndRot
        {
            public SerializableVector3 position;
            public float rX;
            public float rY;
        }
        #endregion

        #region Structs
        [System.Serializable]
        public struct SerializableVector3
        {
            public float x;
            public float y;
            public float z;
            public SerializableVector3(float rX, float rY, float rZ)
            {
                x = rX;
                y = rY;
                z = rZ;
            }

            public override string ToString()
            {
                return String.Format("[{0}, {1}, {2}]", x, y, z);
            }

            public static implicit operator Vector3(SerializableVector3 rValue)
            {
                return new Vector3(rValue.x, rValue.y, rValue.z);
            }

            public static implicit operator SerializableVector3(Vector3 rValue)
            {
                return new SerializableVector3(rValue.x, rValue.y, rValue.z);
            }
        }
        #endregion
    }
}

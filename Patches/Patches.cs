using HarmonyLib;
using UnityEngine;

namespace D86_CE.Patches
{
    internal class Patches
    {
        [HarmonyPatch(typeof(NamePickGui), nameof(NamePickGui.StartChat))]
        class NamePickGui_Patch
        {
            [HarmonyPrefix]
            static bool StartChat()
            {
                ChatGui chatGui = Object.FindObjectOfType<ChatGui>();
                if (PlayerPrefs.HasKey("DisplayName"))
                {
                    if (PlayerPrefs.GetString("DisplayName") != "")
                    {
                        chatGui.UserName = PlayerPrefs.GetString("DisplayName");
                    }
                }
                else
                {
                    chatGui.UserName = PlayerPrefs.GetString("MyName");
                }
                chatGui.Connect();
                PlayerPrefs.SetString("NamePickUserName", chatGui.UserName);

                return false;
            }
        }
        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.NickName), MethodType.Getter)]
        class PlayerProfile_Patch
        {
            static string NickName
            {
                get
                {
                    if (PlayerPrefs.HasKey("DisplayName"))
                    {
                        if (PlayerPrefs.GetString("DisplayName") != "")
                        {
                            return PlayerPrefs.GetString("DisplayName");
                        }
                    }
                    if (!PlayerPrefs.HasKey("nn"))
                    {
                        Debug.Log("Created nn");
                        PlayerPrefs.SetString("nn", string.Format("Player {0}", Random.Range(0, 99999)));
                    }
                    Debug.Log("Used nn");
                    return PlayerPrefs.GetString("nn");
                }
            }
        }
    }
}

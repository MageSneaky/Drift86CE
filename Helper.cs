using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace D86_CE
{
    public class Helper : MonoBehaviour
    {
        public static bool IsMultiplayer()
        {
            if (FindObjectOfType<SI_PersistantUserList>()) return true;
            return false;
        }

        public static bool IsOfficial()
        {
            if (SceneManager.GetActiveScene().name.Contains("Persistant")) return true;
            return false;
        }

        public static bool InGame()
        {
            if (!SceneManager.GetActiveScene().name.ToLowerInvariant().Contains("mainmenu")
                && !SceneManager.GetActiveScene().name.ToLowerInvariant().Contains("empty")
                && !SceneManager.GetActiveScene().name.ToLowerInvariant().Contains("load"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetCar()
        {
            if (FindObjectOfType<UserControl>())
            {
                string carName = FindObjectOfType<UserControl>().GetComponent<SI_ApplySkin>().PrefabCarName;
                carName = carName.Replace("_", " ");
                return carName;
            }
            else
            {
                return "unknown";
            }
        }

        public static int GetScore()
        {
            if (FindObjectOfType<DriftPanelUI>())
            {
                float score = FindObjectOfType<DriftPanelUI>().DriftRaceEntity.PlayerDriftStatistics.TotalScore;
                int intscore = (int)Math.Round(score);
                return intscore;
            }
            else
            {
                return 0;
            }
        }

        public static String GetRaceTime()
        {
            if (FindObjectOfType<DriftPanelUI>())
            {
                float raceTimeF = FindObjectOfType<DriftPanelUI>().DriftRaceEntity.PlayerDriftStatistics.TotalRaceTime;
                TimeSpan ts = TimeSpan.FromSeconds(raceTimeF);

                String raceTime = ts.ToString("m\\:ss\\:fff");
                return raceTime;
            }
            else
            {
                return "00:00:00";
            }
        }
    }
}

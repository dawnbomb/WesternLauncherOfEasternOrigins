using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Western_Launcher_Of_Eastern_Origins
{
    public static class LibraryTouhou
    {
        public static string VersionDate { get; set; } = "July 6 2026";
        public static Version VersionNumber { get; set; } = new Version(1, 0, 5); //Version Numbers (in order) are Major.Minor.Build.Revision

        public static string TouhouLauncherPath { get; set; } = ""; //where this exe is. If in debug mode (coding / developer mode), its set to use the release folder on my local PC. 
        public static string ModsXMLLocation { get; set; } = "";
        public static string GamesXMLLocation { get; set; } = "";
        public static List<GameData> MasterGameList { get; set; } = new();
        public static List<TouhouMod> MasterModsList { get; set; } = new();
        public static List<Achievement> MasterAchievementsList { get; set; } = new();
        public static string PlayerName { get; set; } = "";



        public static string GenerateKey() 
        {
            

            long currentTimeTicks = DateTime.UtcNow.Ticks;

            Random random = new();            
            int randomNumber = random.Next(0, 1000000);  
                        
            return $"{currentTimeTicks}-{randomNumber}";
            
        }

        private static readonly HashSet<string> ValidTexts =
        [
            "1CC",
            "1cc",
            "NB",
            "Clear",
            "✓",
            "✔",
            "ND",
            "NM",
        ];

        private static readonly HashSet<string> InvalidTexts =
        [
            "@",
            "#",
            "$",
        ];

        public static bool IsValidAchievement(Achievement achievement)
        {
            return achievement.Level != 0 
                && (ValidTexts.Contains(achievement.PlayerText) || achievement.PlayerText.Contains("!")) 
                && !InvalidTexts.Any(x => achievement.PlayerText.Contains(x));
        }

        public static readonly string[] GameOrder =
        [
            //This is a list of every mainline game. 
            //If chronological mode is enabled, the player needs a 1CC in an earlier game to get quests for the next game. 
            "Touhou 6",
            "Touhou 7",
            "Touhou 8",
            "Touhou 9",
            "Touhou 10",
            "Touhou 11",
            "Touhou 12",
            "Touhou 13",
            "Touhou 14",
            "Touhou 15",
            "Touhou 16",
            "Touhou 17",
            "Touhou 18",
            "Touhou 19",
            "Touhou 20"
        ];

        public static List<Achievement> GetTopTen(GameLauncher GameLauncher) 
        {
            return LibraryTouhou.MasterAchievementsList.Where(a =>
                    a.Level != 0 &&
                    (ValidTexts.Contains(a.PlayerText) || a.PlayerText.Contains("!")) &&
                    !a.PlayerText.Contains("@") &&
                    !a.PlayerText.Contains("#") &&
                    !a.PlayerText.Contains("$"))
                .OrderByDescending(a => a.Level).Take(10).ToList();

        }

        public static int GetPlayerLevel(GameLauncher GameLauncher) //This function both updates the player level label in the GameLauncher and returns the player level as an int.
        {
            //Player level is whatever is the highest number, of the following.
            //1st best -5
            //2nd best -4
            //3rd best -3
            //4th best -2
            //5th best -1
            //6th best -0
            //Min Lv 1

            int PlayerLevel = 1;
            List<int> Levels = new();

            //All 1CCs above Lv0 goto the Levels List. 
            foreach (Achievement achievement in LibraryTouhou.MasterAchievementsList)
            {
                if (achievement.Level != 0)
                {
                    if (achievement.PlayerText == "1CC" || achievement.PlayerText == "1cc" || achievement.PlayerText == "NB" || achievement.PlayerText == "Clear" || achievement.PlayerText == "✓" || achievement.PlayerText == "✔" || achievement.PlayerText == "ND" || achievement.PlayerText == "NM" || achievement.PlayerText.Contains("!"))
                    {
                        if (!achievement.PlayerText.Contains("@") && !achievement.PlayerText.Contains("#") && !achievement.PlayerText.Contains("$"))
                        {
                            Levels.Add(achievement.Level);
                        }

                    }

                }
            }
            Levels.Sort((a, b) => b.CompareTo(a));  //Sort the levels list.

            List<int> PlayersBests = new();
            if (Levels.Count >= 1) { PlayersBests.Add(Levels[0] - 5); }
            if (Levels.Count >= 2) { PlayersBests.Add(Levels[1] - 4); }
            if (Levels.Count >= 3) { PlayersBests.Add(Levels[2] - 3); }
            if (Levels.Count >= 4) { PlayersBests.Add(Levels[3] - 2); }
            if (Levels.Count >= 5) { PlayersBests.Add(Levels[4] - 1); }
            if (Levels.Count >= 6) { PlayersBests.Add(Levels[5] - 0); }
            if (Levels.Count >= 7) { PlayersBests.Add(Levels[6] - 0); }
            if (Levels.Count >= 8) { PlayersBests.Add(Levels[7] - 0); }
            if (Levels.Count >= 9) { PlayersBests.Add(Levels[8] - 0); }
            if (Levels.Count >= 10) { PlayersBests.Add(Levels[9] + 1); }
            foreach (int LevelValue in PlayersBests) //Set the player level to the biggest number.
            {
                if (LevelValue > PlayerLevel)
                {
                    PlayerLevel = LevelValue;
                }
            }

            if (PlayerLevel == 1 && Levels.Count >= 3) //escape being lv1
            {
                PlayerLevel = 2;
            }


            GameLauncher.LabelPlayerLevel.Content = PlayerLevel.ToString();
            return PlayerLevel;
        }




    }
}

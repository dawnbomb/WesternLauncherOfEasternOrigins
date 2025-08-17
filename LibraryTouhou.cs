using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WesternLauncherOfEasternOrigins
{
    public static class LibraryTouhou
    {
        public static string VersionDate { get; set; } = "Augest 17 2025";
        public static Version VersionNumber { get; set; } = new Version(1, 0, 2); //Version Numbers (in order) are Major.Minor.Build.Revision

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

        

        public static List<Achievement> Sortthem() 
        {
            return null;
        }
    }
}

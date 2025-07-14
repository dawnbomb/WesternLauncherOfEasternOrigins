using OfficeOpenXml;
using static OfficeOpenXml.LicenseContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Drawing;


namespace WesternLauncherOfEasternOrigins
{
    
    public class TouhouGame
    {
        public string? SeriesName { get; set; } = "Touhou X"; //For example: Touhou 6
        public string? SubtitleName { get; set; } = "NewGame"; //For Example: Embodiment of Scarlet Devil
        
        public string? Date { get; set; } = "???";
        public GameType? Type { get; set; } = GameType.None;
        public string? CardArt { get; set; } = "";
        public string? CodeName { get; set; } = "";
        public string? Description { get; set; } = "???";
        public bool Practise { get; set; } = false;

        public string ColorBorder { get; set; } = "#606060";
        public string ColorBack { get; set; } = "#202020";
        public string ColorText { get; set; } = "#FFFFFF";

        //new stuff
        public List<Achievement> AchievementList { get; set; } = new();
        public List<TouhouMod> ModList { get; set; } = new();
    }

    public class Achievement 
    {
        public TouhouGame TouhouGame { get; set; }
        public AchievementTypes Type { get; set; } = AchievementTypes.Basic;
        public int Level { get; set; } = 0;
        public string Key { get; set; } = "";
        public string Note { get; set; } = "";

        public string PlayerText { get; set; } = "";


        //For Basic & Unique Achievements & non-bullet_hell games. 
        public string Name { get; set; } = "";

        //For Chart Achievements.
        public string ShotType { get; set; } = ""; 
        public string Difficulty { get; set; } = ""; //For Chart Achievements.

        

        public enum AchievementTypes { Basic, Chart,  }

    }

    public enum GameType
    {
        None,
        PC98,
        Touhou,
        
    }

    public class TouhouMod
    {
        public string? Name { get; set; } = "Name";
        public string? Key { get; set; } = "Key"; // The name that appears to end users        
        public string? Description { get; set; } = "";
        public string? THCrapText { get; set; } = "";
        public bool Recommend { get; set; } = false;
    }








}

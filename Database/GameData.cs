using OfficeOpenXml;
using static OfficeOpenXml.LicenseContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Drawing;


namespace WesternLauncherOfEasternOrigins
{
    
    public class GameData
    {
        public string? SeriesName { get; set; } = "Touhou X"; //For example: Touhou 6
        public string? SubtitleName { get; set; } = "NewGame"; //For Example: Embodiment of Scarlet Devil
        
        public string? Date { get; set; } = "???";
        public GameType? Type { get; set; } = GameType.None;
        public GameSeries? Series { get; set; } = GameSeries.None;
        public string? CardArt { get; set; } = "";
        public string? CodeName { get; set; } = "";
        public string? Description { get; set; } = "???";
        public string? PrologueText { get; set; } = "";
        public string? WikiLink { get; set; } = "";
        public bool Practise { get; set; } = false;

        public string ColorBorder { get; set; } = "#606060";
        public string ColorBack { get; set; } = "#202020";
        public string ColorText { get; set; } = "#FFFFFF";

        //new stuff
        public List<Achievement> AchievementList { get; set; } = new();
        public List<TouhouMod> ModList { get; set; } = new();
        public List<GameLink> LinkList { get; set; } = new();
        
    }

    public class Achievement 
    {
        public GameData TheGame { get; set; }
        public AchievementTypes Type { get; set; } = AchievementTypes.Basic;
        public int Level { get; set; } = 0;
        public string Key { get; set; } = LibraryTouhou.GenerateKey();
        public string Note { get; set; } = ""; //UNUSED

        public string PlayerText { get; set; } = "";


        //For Basic & Unique Achievements & non-bullet_hell games. 
        public string Name { get; set; } = "";

        //For Chart Achievements.
        public string ShotType { get; set; } = "New"; 
        public string Difficulty { get; set; } = "New"; //For Chart Achievements.

        

        public enum AchievementTypes { Basic, Chart,  }

    }

    public enum GameSeries
    {
        None,
        Touhou,
        TouhouFangame,
        Megaman,
        Indie

    }

    public enum GameType
    {
        None,
        PC98,
        Touhou, 
        MajorTouhouFanGames,
        OtherTouhouFanGames,
        Lenen,
        Kaisendou,
        Hidden,
        
    }

    public class TouhouMod
    {
        public string? Name { get; set; } = "Name"; // The name that appears to end users   
        public string? Key { get; set; } = PixelWPF.LibraryPixel.GenerateKey(); //Used to track what mods users have enabled. 
        public string? Description { get; set; } = "";
        public string? THCrapText { get; set; } = "repos/Example/Example/";
        public string? Note { get; set; } = ""; //Notes about a mod for ME. End users dont see these.
        public bool Recommend { get; set; } = false;

    }

    public class GameLink 
    {
        public string Name { get; set; } = "";
        public string URL { get; set; } = "";
        public string Tooltip { get; set; } = "";
    }






}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using WesternLauncherOfEasternOrigins.Properties;

namespace WesternLauncherOfEasternOrigins
{
    class Loading
    {
       
        

        

        public void LoadTheMods() 
        {            
            XElement xml = XElement.Load(LibraryTouhou.ModsXMLLocation);

            
            foreach (XElement modElement in xml.Element("ModsList").Elements("Mod"))
            {
                TouhouMod mod = new TouhouMod
                {
                    Name = modElement.Element("Name")?.Value,
                    Key = modElement.Element("Key")?.Value,
                    Description = modElement.Element("Description")?.Value,
                    THCrapText = modElement.Element("THCrapText")?.Value,
                    Note = modElement.Element("Note")?.Value,
                    Recommend = Convert.ToBoolean(modElement.Element("Recommend")?.Value)
                };

                // Append each new Mod to the existing NewModsList
                LibraryTouhou.MasterModsList.Add(mod);
            }


        }


        public void LoadTheGames() 
        {
            //string SaveLocation = "";
            //if (SaveType == "Games") { SaveLocation = LibraryMan.GamesXMLLocation; }
            //if (SaveType == "Player") { SaveLocation = LibraryMan.TouhouLauncherPath + "\\Player Profiles\\" + LibraryMan.PlayerName + ".xml"; }


            XElement xml = XElement.Load(LibraryTouhou.GamesXMLLocation);

            foreach (XElement gameElement in xml.Element("GamesList").Elements("Game"))
            {
                string GameName = gameElement.Element("SeriesName")?.Value;
                int i = 1;

                GameData game = new GameData
                {
                    SeriesName = gameElement.Element("SeriesName")?.Value,
                    SubtitleName = gameElement.Element("Subtitle")?.Value,                    
                    CodeName = gameElement.Element("CodeName")?.Value,
                    CardArt = gameElement.Element("CardArt")?.Value,
                    Date = gameElement.Element("Date")?.Value,
                    Type = Enum.TryParse<GameType>(gameElement.Element("Type")?.Value, out GameType type) ? type : GameType.None,
                    Description = gameElement.Element("Description")?.Value,
                    ColorBack = gameElement.Element("ColorBack")?.Value, 
                    ColorBorder = gameElement.Element("ColorBorder")?.Value,
                    ColorText = gameElement.Element("ColorText")?.Value,
                };

                foreach (XElement modElement in gameElement.Descendants("Link"))
                {
                    //if (modElement.Element("Name")?.Value == "") { continue; }

                    GameLink GameLink = new();
                    game.LinkList.Add(GameLink);
                    GameLink.Name = modElement.Element("Name")?.Value;
                    GameLink.URL = modElement.Element("URL")?.Value;
                    GameLink.Tooltip = modElement.Element("Tooltip")?.Value;


                }

                foreach (XElement modElement in gameElement.Descendants("Mod"))
                {
                    string TheKey = modElement.Element("Key")?.Value;

                    foreach (TouhouMod TMod in LibraryTouhou.MasterModsList) 
                    {
                        if (TMod.Key == TheKey) 
                        {
                            game.ModList.Add(TMod);
                            break;
                        }
                    }
                    
                }

                foreach (XElement achievementElement in gameElement.Descendants("Achievement"))
                {
                    GameName = gameElement.Element("SeriesName")?.Value;
                    i = 1;

                    Achievement achievement = new();
                    if (achievementElement.Element("Type")?.Value == "Basic") 
                    { 
                        achievement.Type = Achievement.AchievementTypes.Basic;
                        achievement.Name = achievementElement.Element("Name")?.Value;
                        achievement.ShotType = "";
                        achievement.Difficulty = "";
                    }
                    if (achievementElement.Element("Type")?.Value == "Chart") 
                    {
                        achievement.Type = Achievement.AchievementTypes.Chart;
                        achievement.ShotType = achievementElement.Element("Row")?.Value;
                        achievement.Difficulty = achievementElement.Element("Column")?.Value;
                    }
                    achievement.Level = Int32.Parse(achievementElement.Element("Tooltip")?.Value);
                    //achievement.PlayerText = achievementElement.Element("PlayerText")?.Value;
                    achievement.Key = achievementElement.Element("Key")?.Value;    
                    achievement.Note = achievementElement.Element("Note")?.Value;
                    achievement.TheGame = game;

                    game.AchievementList.Add(achievement);
                    LibraryTouhou.MasterAchievementsList.Add(achievement);

                }


                // Append each new Mod to the existing NewModsList
                LibraryTouhou.MasterGameList.Add(game);
            }
        }


        

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;

namespace WesternLauncherOfEasternOrigins.Editors
{
    class SaveGames
    {
        public void SaveEverything(GameEditor Editor, ModEditor ModEditor) 
        {
            SaveTheMods(ModEditor);
            SaveTheGames(Editor, "Games");
            
        }

        public void SaveTheMods(ModEditor ModEditor)
        {

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(LibraryMan.ModsXMLLocation, settings))
            {
                writer.WriteStartElement("Root");
                writer.WriteStartElement("ModsList");
                foreach (TreeViewItem Item in ModEditor.TheTreeView.Items)
                {
                    TouhouMod Mod = Item.Tag as TouhouMod;

                    writer.WriteStartElement("Mod");
                    writer.WriteElementString("Name", Mod.Name);
                    writer.WriteElementString("Key", Mod.Key); 
                    writer.WriteElementString("THCrapText", Mod.THCrapText); 
                    writer.WriteElementString("Recommend", Mod.Recommend.ToString());
                    writer.WriteElementString("Description", Mod.Description);


                    writer.WriteEndElement(); //End Mod   
                }
                writer.WriteEndElement(); //End ModsList  
                writer.WriteEndElement(); //End Root  
                writer.Flush(); //Ends the XML
            }
        }

        public void SaveTheGames(GameEditor Editor, string SaveType) 
        {            
            string SaveLocation = "";
            if (SaveType == "Games")  { SaveLocation = LibraryMan.GamesXMLLocation; }
            if (SaveType == "Player") { SaveLocation = LibraryMan.TouhouLauncherPath + "\\Player Profiles\\" + LibraryMan.PlayerName + ".xml"; }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(SaveLocation, settings))
            {
                writer.WriteStartElement("Root");
                writer.WriteStartElement("GamesList"); 

                foreach (TreeViewItem Item in Editor.TheTreeView.Items)
                {
                    TouhouGame Game = Item.Tag as TouhouGame;

                    writer.WriteStartElement("Game"); //This is the root of the XML   
                    writer.WriteElementString("SeriesName", Game.SeriesName); //This is all misc editor data.
                    writer.WriteElementString("Subtitle", Game.SubtitleName);
                    writer.WriteElementString("Date", Game.Date); //This is the name of the file that this editor uses.
                    writer.WriteElementString("CardArt", Game.CardArt);
                    writer.WriteElementString("Type", Game.Type.ToString());
                    writer.WriteElementString("CodeName", Game.CodeName);
                    writer.WriteElementString("Description", Game.Description);
                    writer.WriteElementString("ColorBorder", Game.ColorBorder);
                    writer.WriteElementString("ColorBack", Game.ColorBack);
                    writer.WriteElementString("ColorText", Game.ColorText);

                    writer.WriteStartElement("ModsList");
                    foreach (TouhouMod touhouMod in Game.ModList)
                    {
                        writer.WriteStartElement("Mod");
                        writer.WriteElementString("Name", touhouMod.Name);
                        writer.WriteElementString("Key", touhouMod.Key); 
                        writer.WriteEndElement(); //End Mod 
                    }
                    writer.WriteEndElement(); //End AchievementList

                    writer.WriteStartElement("AchievementList");
                    foreach (Achievement achievement in Game.AchievementList)
                    {
                        writer.WriteStartElement("Achievement");

                        writer.WriteElementString("Level", achievement.Level.ToString());
                        if (achievement.Type == Achievement.AchievementTypes.Basic) 
                        {
                            writer.WriteElementString("Type", "Basic");
                            writer.WriteElementString("Name", achievement.Name);
                        }
                        if (achievement.Type == Achievement.AchievementTypes.Chart) 
                        {
                            writer.WriteElementString("Type", "Chart");
                            writer.WriteElementString("Difficulty", achievement.Difficulty);
                            writer.WriteElementString("ShotType", achievement.ShotType);
                        }

                        writer.WriteElementString("PlayerText", achievement.PlayerText);
                        writer.WriteElementString("Note", achievement.Note);
                        writer.WriteElementString("Key", achievement.Key);
                        writer.WriteEndElement(); //End Achievement 
                    }
                    writer.WriteEndElement(); //End AchievementList

                    writer.WriteEndElement(); //End Game
                }
                

                writer.WriteEndElement(); //End GamesList  
                writer.WriteEndElement(); //End Root  
                writer.Flush(); //Ends the XML 
            }

            
        }

        
    }
}

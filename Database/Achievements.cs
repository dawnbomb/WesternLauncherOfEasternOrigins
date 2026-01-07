using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Xml;
using System.Diagnostics.Metrics;
using WesternLauncherOfEasternOrigins.Editors;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;

namespace WesternLauncherOfEasternOrigins
{
    class Achievements
    {
        public void LoadAllPlayers(GameLauncher GameLauncher)  //AFTER game loading!!! //Loads all player names into combobox. Also selects a player.
        {
            List<string> PlayerList = new();

            {
                string directoryPath = LibraryTouhou.TouhouLauncherPath + "\\Player Profiles\\";
                if (Directory.Exists(directoryPath))
                {
                    // Get all XML files in the directory
                    string[] fileEntries = Directory.GetFiles(directoryPath, "*.xml");
                    foreach (string fileName in fileEntries)
                    {
                        // Extract the file name without extension and add to the list
                        string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                        PlayerList.Add(nameWithoutExtension);
                    }
                }
                else //if player profile folder is somehow missing, we create it. 
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }



            foreach (string Player in PlayerList)
            {
                ComboBoxItem comboBoxItem = new();
                comboBoxItem.Content = Player;
                GameLauncher.PlayerComboBox.Items.Add(comboBoxItem);

                if (Player == Properties.Settings.Default.LastPlayer)
                {
                    comboBoxItem.IsSelected = true;
                    LibraryTouhou.PlayerName = Player;
                }
            }

            if (GameLauncher.PlayerComboBox.SelectedItem == null)
            {
                while (GameLauncher.PlayerComboBox.Items.Count == 0)
                {
                    GameLauncher.CreateNewPlayer(); //create new player
                }

                ComboBoxItem comboBoxItem = GameLauncher.PlayerComboBox.Items[0] as ComboBoxItem;
                comboBoxItem.IsSelected = true;
                LibraryTouhou.PlayerName = comboBoxItem.Content as string;
                Properties.Settings.Default.LastPlayer = LibraryTouhou.PlayerName;
                Properties.Settings.Default.Save();
            }
        }

        public void LoadPlayerAchievements() //Loads the currently selected player's achievement data.
        {
            //Step 1: Load all player names (aka xml names) into a ComboBox for player profiles.
            //Set previous player as selected player. If their xml is gone, select random player.
            //If no player xmls exist, create a new one.

            //Step 2: Load the selected players XML achievement data.

            XElement xml = XElement.Load(LibraryTouhou.TouhouLauncherPath + "\\Player Profiles\\" + LibraryTouhou.PlayerName + ".xml");


            foreach (Achievement achievement in LibraryTouhou.MasterAchievementsList)
            {
                bool isMatchFound = false;  // Flag to check if a match is found

                foreach (XElement QuestElement in xml.Element("AchievementList").Elements("Achievement"))
                {
                    if (achievement.Key == QuestElement.Element("Key")?.Value)
                    {
                        achievement.PlayerText = QuestElement.Element("PlayerText")?.Value;
                        isMatchFound = true;  // Set flag to true when a match is found
                        break;  // Break the loop as we found the match and updated the PlayerText
                    }
                }

                if (!isMatchFound)  // Check if no match was found after checking all elements
                {
                    achievement.PlayerText = "";  // Set to empty string if no matching element is found
                }
            }



        }


        public int PlayerLevel(GameLauncher GameLauncher) 
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
                    if (achievement.PlayerText == "1CC" || achievement.PlayerText == "1cc" || achievement.PlayerText == "NB" || achievement.PlayerText == "Clear" || achievement.PlayerText == "✓" || achievement.PlayerText == "✔" || achievement.PlayerText == "ND" || achievement.PlayerText == "NM" || achievement.PlayerText.Contains("!") ) 
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

        public void CreateQuestBoard(GameLauncher GameLauncher)  //Decides what quests to suggest to the user today.
        {
            GameLauncher.QuestPanel.Children.Clear();

            int ThePlayerLevel = PlayerLevel(GameLauncher); //Player Level is the 6th highest lv of achievement.
            
            
            List<Achievement> ToDoAchievements = new();
            foreach (Achievement achievement in LibraryTouhou.MasterAchievementsList) 
            {   
                if (achievement.PlayerText != "1CC" && achievement.Level != 0)
                {
                    if (achievement.Difficulty == "Extra") //If its an extra stage, check if it's even fucking unlocked first xd
                    {
                        bool cleared = false;
                        foreach (Achievement AB in LibraryTouhou.MasterAchievementsList) 
                        {
                            if (AB.TheGame == achievement.TheGame && AB.ShotType == achievement.ShotType && AB.Difficulty != "Easy") 
                            {
                                if ((AB.PlayerText.Contains("1CC") || AB.PlayerText.Contains("1cc") || AB.PlayerText.Contains("NB") || AB.PlayerText.Contains("Clear") || AB.PlayerText.Contains("✓") || AB.PlayerText.Contains("✔") || AB.PlayerText.Contains("ND") || AB.PlayerText.Contains("NM") || AB.PlayerText.Contains("!"))) 
                                {                                    
                                    if (!AB.PlayerText.Contains("@") && !AB.PlayerText.Contains("#") && !AB.PlayerText.Contains("$")) 
                                    {
                                        cleared = true;
                                    }
                                    
                                }
                                
                            }
                        }

                        if (cleared == true) 
                        {
                            ToDoAchievements.Add(achievement);
                        }
                    }
                    else 
                    {
                        ToDoAchievements.Add(achievement);
                    }
                    
                }               
            }


            bool CheckDifficulty(Achievement achievement) //checks to ignore 
            {
                //if (Properties.Settings.Default.ShowEasyMode == false && achievement.Difficulty == "Easy")
                //{
                //    return true;
                //}
                //if (Properties.Settings.Default.ShowNormalMode == false && achievement.Difficulty == "Normal")
                //{
                //    return true;
                //}
                return false;
                //if (Properties.Settings.Default.ShowEasyMode == false && Difficulty == "Easy")
                //{
                //    continue;
                //}
                //if (Properties.Settings.Default.ShowNormalMode == false && Difficulty == "Normal")
                //{
                //    continue;
                //}
            }

            bool CheckExAvailable(Achievement achievement) 
            {
                return false;
            }

            bool QuestDebugMode = true;
            for (int i = 0; i < 5;)
            {


                if (i < 1)  //Last Launched Game
                {
                    List<Achievement> TheListA = new();

                    foreach (GameData Game in LibraryTouhou.MasterGameList)
                    {
                        if (Game.CodeName == Properties.Settings.Default.LastTouhouGame)
                        {
                            foreach (Achievement achievement in Game.AchievementList)
                            {
                                if (CheckDifficulty(achievement)) { continue; }

                                if (ToDoAchievements.Contains(achievement))
                                {
                                    TheListA.Add(achievement);
                                }
                            }
                        }
                    }

                    List<Achievement> TheList = new();

                    int LvX = 0;
                    while (TheList.Count == 0)
                    {
                        LvX--;
                        UpX();
                        if (ThePlayerLevel - LvX == 0 || LvX == -7)
                        {
                            break;
                        }

                    }

                    if (TheList.Count != 0)
                    {
                        MakeAQuest(TheList, "Last");
                        i++;
                        continue;
                    }

                    void UpX()
                    {
                        foreach (Achievement achievement in TheListA)
                        {
                            if (CheckDifficulty(achievement)) { continue; }

                            if (achievement.Level == ThePlayerLevel + LvX)
                            {
                                TheList.Add(achievement);
                            }
                        }
                    }

                    {   //Extra stage check
                        foreach (Achievement achievement in TheListA)
                        {
                            if (CheckDifficulty(achievement)) { continue; }
                            if (achievement.PlayerText != "1CC" && achievement.Level <= ThePlayerLevel  && (achievement.Difficulty == "Extra" || achievement.Difficulty == "Phantasm") )
                            {
                                TheList.Add(achievement);
                            }
                        }
                        if (TheList.Count != 0)
                        {
                            MakeAQuest(TheList, "Last");
                            i++;
                            continue;
                        }
                    }
                    
                }

                Random RNG = new Random();
                int asdf = RNG.Next(1, 3); //the max roll is confusingly not possible. >:(
                if (i == 1 && asdf == 1) //NewGame or NewExtra. 50% chance to recommend. Recommend Lv-1 or lower.
                {

                    List<GameData> LowMainClears = new();  //NewGame: Player has 2 or less achievements
                    List<GameData> NoExtraClears = new(); //NewExtra: 0 EX-Clears, atleast 1 main clear.
                    List<GameData> SomeExtraClears = new();

                    foreach (GameData Game in LibraryTouhou.MasterGameList)
                    {
                        int MainClears = 0;
                        int EXclears = 0;
                        foreach (Achievement achievement in Game.AchievementList)
                        {
                            if (CheckDifficulty(achievement)) { continue; }
                            if (achievement.PlayerText == "1CC" && achievement.Difficulty != "Extra")
                            {
                                MainClears++;
                            }
                            if (achievement.PlayerText == "1CC" && achievement.Difficulty == "Extra")
                            {
                                EXclears++;
                            }
                        }
                        if (MainClears <= 2)
                        {
                            LowMainClears.Add(Game);
                        }
                        if (EXclears == 0 && MainClears >= 2)
                        {
                            NoExtraClears.Add(Game);
                        }
                        if (EXclears > 0 && MainClears >= 2)
                        {
                            SomeExtraClears.Add(Game);
                        }
                    }

                    Random random = new Random();
                    int randomIndex = random.Next(1, 5); //the max value is confusingly not a possible roll. >:(  

                    if (LowMainClears.Count != 0 && randomIndex == 1) //Low Main
                    {
                        List<Achievement> TheList = new();
                        foreach (GameData Game in LowMainClears)
                        {
                            foreach (Achievement achievement in Game.AchievementList)
                            {
                                if (CheckDifficulty(achievement)) { continue; }
                                if (ToDoAchievements.Contains(achievement))
                                {
                                    if (achievement.Difficulty != "Extra" && achievement.Level <= ThePlayerLevel - 1)
                                    {
                                        TheList.Add(achievement);
                                    }

                                }
                            }
                        }
                        TheList.Sort((a, b) => b.Level.CompareTo(a.Level));
                        while (TheList.Count >= 10)
                        {
                            TheList.RemoveAt(TheList.Count - 1);
                        }
                        if (TheList.Count != 0)
                        {
                            MakeAQuest(TheList, "New");
                            i++;
                            continue;
                        }

                    }

                    if (NoExtraClears.Count != 0 && randomIndex == 2) //No Extra
                    {
                        List<Achievement> TheList = new();
                        foreach (GameData Game in NoExtraClears)
                        {
                            foreach (Achievement achievement in Game.AchievementList)
                            {
                                if (CheckDifficulty(achievement)) { continue; }
                                if (ToDoAchievements.Contains(achievement))
                                {
                                    if (achievement.Difficulty == "Extra" && achievement.Level <= ThePlayerLevel - 1)
                                    {
                                        TheList.Add(achievement);
                                    }

                                }
                            }
                        }
                        TheList.Sort((a, b) => b.Level.CompareTo(a.Level));
                        while (TheList.Count >= 10)
                        {
                            TheList.RemoveAt(TheList.Count - 1);
                        }
                        if (TheList.Count != 0)
                        {
                            MakeAQuest(TheList, "EX");
                            i++;
                            continue;
                        }



                    }

                    if (SomeExtraClears.Count != 0 && randomIndex == 3) //Extra
                    {
                        List<Achievement> TheList = new();
                        foreach (GameData Game in SomeExtraClears)
                        {
                            foreach (Achievement achievement in Game.AchievementList)
                            {
                                if (CheckDifficulty(achievement)) { continue; }
                                if (ToDoAchievements.Contains(achievement))
                                {
                                    if (achievement.Difficulty == "Extra" && achievement.Level <= ThePlayerLevel - 1)
                                    {
                                        TheList.Add(achievement);
                                    }

                                }
                            }
                        }
                        TheList.Sort((a, b) => b.Level.CompareTo(a.Level));
                        while (TheList.Count >= 10)
                        {
                            TheList.RemoveAt(TheList.Count - 1);
                        }
                        if (TheList.Count != 0)
                        {
                            MakeAQuest(TheList, "Old");
                            i++;
                            continue;
                        }



                    }


                }




                if (i == 1 && asdf == 2) //new extra
                {
                    List<Achievement> TheList = new();

                    int LvX = 0;
                    while (TheList.Count == 0)
                    {
                        LvX--;
                        UpX();
                        if (ThePlayerLevel + LvX == 0)
                        {
                            break;
                        }

                    }

                    if (TheList.Count != 0)
                    {
                        MakeAQuest(TheList, "EX");
                        i++;
                        continue;
                    }

                    void UpX()
                    {
                        foreach (Achievement achievement in ToDoAchievements)
                        {
                            if (CheckDifficulty(achievement)) { continue; }
                            if (achievement.Level == ThePlayerLevel + LvX)
                            {
                                if (achievement.Difficulty == "Extra" || achievement.Difficulty == "Phantasm")
                                {
                                    TheList.Add(achievement);
                                }

                            }
                        }
                    }
                }


                

                if (i < 3) //RECOMMEND LOWER LV. Lv-X. X starts at -1, and keeps going.
                {
                    List<Achievement> TheList = new();

                    int LvX = 0;
                    while (TheList.Count == 0)
                    {
                        LvX--;
                        UpX();
                        if (ThePlayerLevel + LvX == 0)
                        {
                            break;
                        }

                    }

                    if (TheList.Count != 0)
                    {
                        MakeAQuest(TheList, "Lv" + LvX.ToString());
                        i++;
                        continue;
                    }

                    void UpX()
                    {
                        foreach (Achievement achievement in ToDoAchievements)
                        {
                            if (CheckDifficulty(achievement)) { continue; }
                            if (achievement.Level == ThePlayerLevel + LvX)
                            {
                                TheList.Add(achievement);
                            }
                        }
                    }
                }

                if (i < 4) //RECOMMEND SAME LEVEL.
                {
                    List<Achievement> TheList = new();
                    foreach (Achievement achievement in ToDoAchievements)
                    {
                        if (CheckDifficulty(achievement)) { continue; }
                        if (achievement.Level == ThePlayerLevel)
                        {
                            TheList.Add(achievement);
                        }
                    }
                    if (TheList.Count != 0)
                    {
                        MakeAQuest(TheList, "Lv+0");
                        i++;
                        continue;
                    }
                }
                
                if (i < 5) //RECOMMEND HIGH LEVEL. If none, another +1 untill finding something. Stops at Lv+100.
                {
                    List<Achievement> TheList = new();

                    int LvX = 0;
                    while (TheList.Count == 0) 
                    {
                        LvX++;
                        UpX();                        
                        if (LvX == 100) 
                        {
                            break;
                        }
                        
                    }

                    if (TheList.Count != 0)
                    {
                        MakeAQuest(TheList, "Lv+" + LvX.ToString());
                        i++;
                        continue;
                    }

                    void UpX() 
                    {
                        foreach (Achievement achievement in ToDoAchievements)
                        {
                            if (CheckDifficulty(achievement)) { continue; }
                            if (achievement.Level == ThePlayerLevel + LvX)
                            {
                                TheList.Add(achievement);
                            }
                        }
                    }                    
                    
                }

                
                //Secret: Unranked (for Lv 30+ players)

                if (i < 6) //Totally Random. Canot normally get this. A safety measure.
                {                    
                    MakeAQuest(ToDoAchievements, "RNG");
                    i++;
                    continue;
                }


                i++; //Safety measure against players doing literally everything.

            }
            

            void MakeAQuest(List<Achievement> TheQuests, string ExtraText) //Creates a quest chosen at random from a given list.
            {
                Random random = new Random();
                int randomIndex = random.Next(TheQuests.Count);
                Achievement achievement = TheQuests[randomIndex];
                ToDoAchievements.Remove(achievement);


                Brush TheBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(achievement.TheGame.ColorBorder));
                Brush TheBackColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(achievement.TheGame.ColorBack));
                Brush TheTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(achievement.TheGame.ColorText));
                Brush TheGrayGrid = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#50191919"));

                Border AchievementBorder = new();
                DockPanel.SetDock(AchievementBorder, Dock.Top);
                //AchievementBorder.BorderThickness = new Thickness(15,0,15,0);
                AchievementBorder.BorderBrush = TheBorderColor;
                GameLauncher.QuestPanel.Children.Add(AchievementBorder);

                Border BlackBorder = new();
                DockPanel.SetDock(BlackBorder, Dock.Top);
                BlackBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                BlackBorder.BorderBrush = Brushes.Black;
                //GameLauncher.QuestPanel.Children.Add(BlackBorder);
                AchievementBorder.Child = BlackBorder;

                DockPanel QuestPanel = new();
                DockPanel.SetDock(QuestPanel, Dock.Top);
                QuestPanel.Background = TheBackColor;
                BlackBorder.Child = QuestPanel;

                Label QuestLevelLabel = new Label();
                QuestLevelLabel.Foreground = TheTextColor;
                QuestLevelLabel.Width = 59;
                DockPanel.SetDock(QuestLevelLabel, Dock.Left);
                QuestPanel.Children.Add(QuestLevelLabel);
                QuestLevelLabel.Content = "Lv " + achievement.Level;


                //TextBox goalBox = new();
                //goalBox.Foreground = TheTextColor;
                //goalBox.Background = TheBackColor;
                //goalBox.Width = 90;
                //goalBox.Margin = new Thickness(5,3,0,3);
                //DockPanel.SetDock(goalBox, Dock.Left);
                //AchievementPanel.Children.Add(goalBox);

                Border verticalLineA = new Border();
                verticalLineA.Background = TheBorderColor;
                verticalLineA.Width = 2;
                verticalLineA.Margin = new Thickness(0, 0, 1, 0); // 3 units right margin
                DockPanel.SetDock(verticalLineA, Dock.Left);
                QuestPanel.Children.Add(verticalLineA);

                Label gameLabel = new Label();
                gameLabel.Foreground = TheTextColor;
                gameLabel.Width = 330;
                DockPanel.SetDock(gameLabel, Dock.Left);
                QuestPanel.Children.Add(gameLabel);
                gameLabel.Content = achievement.TheGame.SubtitleName;


                Border verticalLine = new Border();
                verticalLine.Background = TheBorderColor;
                verticalLine.Width = 2;
                verticalLine.Margin = new Thickness(0, 0, 3, 0); // 3 units right margin
                DockPanel.SetDock(verticalLine, Dock.Left);
                QuestPanel.Children.Add(verticalLine);

                if (achievement.Type == Achievement.AchievementTypes.Basic)
                {
                    Label QuestNameLabel = new Label();
                    QuestNameLabel.Foreground = TheTextColor;
                    //QuestNameLabel.MinWidth = 60;
                    DockPanel.SetDock(QuestNameLabel, Dock.Left);
                    QuestPanel.Children.Add(QuestNameLabel);
                    QuestNameLabel.Content = achievement.Name + achievement.Difficulty;
                }
                if (achievement.Type == Achievement.AchievementTypes.Chart)
                {
                    Label QuestDifficultyLabel = new Label();
                    QuestDifficultyLabel.Foreground = TheTextColor;
                    QuestDifficultyLabel.Width = 78;
                    DockPanel.SetDock(QuestDifficultyLabel, Dock.Left);
                    QuestPanel.Children.Add(QuestDifficultyLabel);
                    QuestDifficultyLabel.Content = achievement.Difficulty;


                    Border verticalLineB = new Border();
                    verticalLineB.Background = TheBorderColor;
                    verticalLineB.Width = 2;
                    verticalLineB.Margin = new Thickness(0, 0, 3, 0); // 3 units right margin
                    DockPanel.SetDock(verticalLineB, Dock.Left);
                    QuestPanel.Children.Add(verticalLineB);

                    Label QuestShotLabel = new Label();
                    QuestShotLabel.Foreground = TheTextColor;
                    DockPanel.SetDock(QuestShotLabel, Dock.Left);
                    QuestPanel.Children.Add(QuestShotLabel);
                    QuestShotLabel.Content = achievement.ShotType;
                }


                if( QuestDebugMode == true)
                {   //Debug Text
                    Label QuestTypeLabel = new Label();
                    QuestTypeLabel.Foreground = TheTextColor;
                    QuestTypeLabel.Width = 56;
                    DockPanel.SetDock(QuestTypeLabel, Dock.Right);
                    QuestPanel.Children.Add(QuestTypeLabel);
                    QuestTypeLabel.Content = ExtraText;

                    Border verticalLineB = new Border();
                    verticalLineB.Background = TheBorderColor;
                    verticalLineB.Width = 2;
                    verticalLineB.Margin = new Thickness(0, 0, 3, 0); // 3 units right margin
                    DockPanel.SetDock(verticalLineB, Dock.Right);
                    QuestPanel.Children.Add(verticalLineB);

                    QuestPanel.LastChildFill = false;


                }
            }
            
            


        }


        public void CreateAchievementsBoard(GameLauncher GameLauncher)
        {
            GameLauncher.MasterAchievementPanel.Children.Clear();

            foreach (GameData Game in LibraryTouhou.MasterGameList)
            {
                if (Properties.Settings.Default.ShowTouhouPC98 == false && Game.Type == GameType.PC98) 
                {
                    continue;
                }
                if (Properties.Settings.Default.ShowTouhou075 == false && Game.CodeName == "th075")
                {
                    continue;
                }
                if (Properties.Settings.Default.ShowLenenGames == false && Game.Type == GameType.Lenen)
                {
                    continue;
                }
                if (Properties.Settings.Default.ShowKaisendouGames == false && Game.Type == GameType.Kaisendou)
                {
                    continue;
                }
                if (Properties.Settings.Default.ShowFanBulletHell == false && Game.Type == GameType.MajorTouhouFanGames)
                {
                    continue;
                }
                if (Properties.Settings.Default.ShowOtherFanTouhouBulletHell == false && Game.Type == GameType.OtherTouhouFanGames)
                {
                    continue;
                }
                if (Game.Type == GameType.GARBAGE)
                {
                    continue;
                }
                
                CreateMasterAchievementPanel(GameLauncher, Game);
            }
        }

        private void CreateMasterAchievementPanel(GameLauncher GameLauncher, GameData Game)
        {
            if (Game.Type == GameType.Hidden || Game.Type == GameType.GARBAGE) { return; }

            List<string> DifficultyList = new();
            List<string> ShotTypeList = new();

            {
                //Sort or identify all shot types & difficulties.
                if (Game.AchievementList.Count == 0)
                {
                    //return;
                }
                foreach (Achievement achievement in Game.AchievementList)
                {
                    if (achievement.Type == Achievement.AchievementTypes.Basic)
                    {
                        continue;
                    }
                    if (achievement.Difficulty == "")
                    {
                        continue;
                    }
                    if (!DifficultyList.Contains(achievement.Difficulty))
                    {
                        DifficultyList.Add(achievement.Difficulty);
                    }
                    if (!ShotTypeList.Contains(achievement.ShotType))
                    {
                        ShotTypeList.Add(achievement.ShotType);
                    }
                }

                foreach (Achievement achievement in Game.AchievementList)
                {
                    //HERE
                }
            }

            Brush TheBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Game.ColorBorder));
            Brush TheBackColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Game.ColorBack));
            Brush TheTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Game.ColorText));
            Brush TheGrayGrid = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#50191919"));

            if (Properties.Settings.Default.ColorAchievements == false) 
            {
                TheBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444"));
                TheBackColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"));
                TheTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB"));
                TheGrayGrid = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#50191919"));
            }

            Border border = new Border();
            border.BorderThickness = new System.Windows.Thickness(15, 0, 15, 15);
            border.BorderBrush = TheBorderColor;
            border.Tag = Game;
            DockPanel.SetDock(border, Dock.Top);
            GameLauncher.MasterAchievementPanel.Children.Add(border);

            DockPanel GamePanel = new DockPanel();
            DockPanel.SetDock(GamePanel, Dock.Top);
            GamePanel.LastChildFill = false;
            GamePanel.MinHeight = 50;
            border.Child = GamePanel;

            ///////////////////////////////////////////

            DockPanel NamePanel = new(); //This is the game name and it's label
            NamePanel.Height = 26;
            DockPanel.SetDock(NamePanel, Dock.Top);
            NamePanel.Background = TheBorderColor;
            GamePanel.Children.Add(NamePanel);

            Label gameNameLabel = new Label(); //The game name
            gameNameLabel.Margin = new Thickness(0,-5,0,-4);
            gameNameLabel.Content = Game.SeriesName + ": " + Game.SubtitleName + " (" + Game.Date + ")";
            gameNameLabel.FontWeight = FontWeights.Bold;
            gameNameLabel.Foreground = TheTextColor;
            NamePanel.Children.Add(gameNameLabel);


            int NameWidth = 164;
            int GoalWidth = 105;

            //////////////////////////////////////////////////////////////////////
            ////////////////// TOP ROW OF DIFFICULTY NAMES ///////////////////////
            //////////////////////////////////////////////////////////////////////
            if (DifficultyList.Count != 0 && ShotTypeList.Count != 0)
            {
                bool Thin = true;

                DockPanel DifficultysRow = new();
                DockPanel.SetDock(DifficultysRow, Dock.Top);
                DifficultysRow.LastChildFill = false;
                DifficultysRow.Background = TheBorderColor;
                if (Thin == true) { DifficultysRow.Height = 28; }
                GamePanel.Children.Add(DifficultysRow);

                Label goalsLabel = new Label();
                DockPanel.SetDock(goalsLabel, Dock.Left);
                //goalsLabel.Content = "?? %"; //////////////////////////Date thing
                goalsLabel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));
                goalsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909090"));
                goalsLabel.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                goalsLabel.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                if (Thin == true) { goalsLabel.Margin = new Thickness(0, 0, 0, 0); }
                goalsLabel.Width = NameWidth;
                DifficultysRow.Children.Add(goalsLabel);

                foreach (string Difficulty in DifficultyList)
                {
                    if (Properties.Settings.Default.ShowEx2 == true && DifficultyList.Count >= 6 && Difficulty == "Easy")
                    {
                        continue;
                    }
                    if (Properties.Settings.Default.ShowEx3 == true && DifficultyList.Count >= 7 && Difficulty == "Normal")
                    {
                        continue;
                    }

                    Border DifficultyLabelBorder = new();
                    DifficultyLabelBorder.Height = 28;
                    DifficultyLabelBorder.BorderThickness = new Thickness(0);
                    DifficultysRow.Children.Add(DifficultyLabelBorder);

                    Label DifficultyLabel = new Label();
                    DockPanel.SetDock(DifficultyLabel, Dock.Left);
                    DifficultyLabel.Content = Difficulty;
                    DifficultyLabel.Width = GoalWidth;
                    //GoalLabel.Height = 26;
                    if (Thin == true) { DifficultyLabel.Margin = new Thickness(0, -4, 0, -15); }
                    DifficultyLabel.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                    //GoalLabel.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    //DifficultysRow.Children.Add(DifficultyLabel);
                    DifficultyLabelBorder.Child = DifficultyLabel;

                    if (Difficulty == "Easy")
                    {
                        DifficultyLabelBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F3F4A"));
                        DifficultyLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#839BC3"));
                    }
                    if (Difficulty == "Normal")
                    {
                        DifficultyLabelBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F3E0F"));
                        DifficultyLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#87AC87"));
                    }
                    if (Difficulty == "Hard")
                    {
                        DifficultyLabelBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#603203"));
                        DifficultyLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C78D31"));
                    }
                    if (Difficulty == "Lunatic" || Difficulty == "Unreal" || Difficulty == "Overdrive" || Difficulty == "URA")
                    {
                        DifficultyLabelBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#520d00")); //520000
                        DifficultyLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B47B6B")); 
                    }
                    if (Difficulty == "Extra" || Difficulty == "Labyrinth")
                    {
                        DifficultyLabelBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242563")); //1A0E3E  
                        DifficultyLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7973d4")); //725681
                    }
                    if (Difficulty == "Phantasm" || Difficulty == "A-Extra" || Difficulty == "Carrefour" || Difficulty == "Sequal" || Difficulty == "E-Extra" || Difficulty == "EX-Ult")
                    {
                        DifficultyLabelBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#471759")); //2c0e3e
                        DifficultyLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b66eba")); //81567d
                    }
                    if (Difficulty == "Luna NB")
                    {
                        DifficultyLabelBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A0E3E"));
                        DifficultyLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#725681"));
                    }
                    if (Difficulty == "Extra NB")
                    {
                        DifficultyLabelBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A0E3E"));
                        DifficultyLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#725681"));
                    }
                    if (Difficulty == "Phan NB")
                    {
                        DifficultyLabelBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2c0e3e"));
                        DifficultyLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#81567d"));
                    }
                }

            }           
            //////////////////////////////////////////////////////////////////////
            ////////////////// TOP ROW OF DIFFICULTY NAMES ///////////////////////
            //////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////////
            ////////////////// LEFT COLUMN OF SHOT TYPE NAMES //////////////////// 
            //////////////////////////////////////////////////////////////////////
            if (DifficultyList.Count != 0 && ShotTypeList.Count != 0)  
            {
                foreach (string ShotType in ShotTypeList)
                {
                    bool Thin = true;

                    DockPanel AchievementRowPanel = new();
                    DockPanel.SetDock(AchievementRowPanel, Dock.Top);
                    AchievementRowPanel.LastChildFill = false;
                    AchievementRowPanel.Background = TheBackColor;
                    if (Thin == true) { AchievementRowPanel.Height = 26; }
                    AchievementRowPanel.Height = 26;
                    GamePanel.Children.Add(AchievementRowPanel);

                    Border NameContainer = new();
                    NameContainer.Height = 26;
                    NameContainer.BorderBrush = TheGrayGrid;
                    NameContainer.BorderThickness = new Thickness(0, 1, 0, 0);
                    AchievementRowPanel.Children.Add(NameContainer);

                    Label ShotLabel = new Label();
                    ShotLabel.Width = NameWidth;
                    ShotLabel.Content = ShotType;
                    ShotLabel.Foreground = TheTextColor; 
                    if (Thin == true) { ShotLabel.Margin = new Thickness(0, -5, 0, -15); }
                    //NameContainer.Children.Add(ShotLabel);
                    NameContainer.Child = ShotLabel;

                    foreach (string Difficulty in DifficultyList)
                    {
                        if (Properties.Settings.Default.ShowEx2 == true && DifficultyList.Count >= 6 && Difficulty == "Easy")
                        {
                            continue;
                        }
                        if (Properties.Settings.Default.ShowEx3 == true && DifficultyList.Count >= 7 && Difficulty == "Normal")
                        {
                            continue;
                        }

                        foreach (Achievement achievement in Game.AchievementList) //Assign achievement as tag
                        {
                            if (achievement.Difficulty == Difficulty && achievement.ShotType == ShotType)
                            {
                                
                                //if (Game.SeriesName == "Touhou 7" && Properties.Settings.Default.ShowPhantasmMode == false && Difficulty == "Phantasm")
                                //{
                                //    continue;
                                //}

                                MakeAGoalBox(achievement, AchievementRowPanel, GoalWidth);

                            }
                        }


                    }

                }
            }
            //////////////////////////////////////////////////////////////////////
            ////////////////// LEFT COLUMN OF SHOT TYPE NAMES //////////////////// 
            //////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////////
            ///////// UNIQUE ACHIEVEMENT ROWS UNDER THE ACHIEVEMENT GRID ///////// 
            //////////////////////////////////////////////////////////////////////
            {
                //Unique Achievement Lines

                


                if (Game.AchievementList.Any(achievement => achievement.Type == Achievement.AchievementTypes.Basic)) 
                {
                    if (Game.AchievementList.Any(achievement => achievement.Type == Achievement.AchievementTypes.Chart)) 
                    {
                        if (Properties.Settings.Default.ShowTouhouSpecialAchievements == false)
                        {
                            return;
                        }

                        //Make some kind of seperator line
                        DockPanel SeperatorLinePanel = new();
                        DockPanel.SetDock(SeperatorLinePanel, Dock.Top);
                        SeperatorLinePanel.Height = 10;
                        SeperatorLinePanel.Background = TheBorderColor;
                        GamePanel.Children.Add(SeperatorLinePanel);
                    }
                    
                }
               

                foreach (Achievement achievement in Game.AchievementList)
                {
                    if (achievement.Type == Achievement.AchievementTypes.Basic)
                    {
                        
                        DockPanel AchievementPanel = new();
                        DockPanel.SetDock(AchievementPanel, Dock.Top);
                        AchievementPanel.Background = TheBackColor;
                        GamePanel.Children.Add(AchievementPanel);

                        {
                            MakeAGoalBox(achievement, AchievementPanel, NameWidth);


                        }

                        Label nameLabel = new Label();
                        nameLabel.Content = achievement.Name;
                        nameLabel.FontSize = 20;
                        nameLabel.Foreground = TheTextColor;
                        AchievementPanel.Children.Add(nameLabel);
                    }

                }

            }
            //////////////////////////////////////////////////////////////////////
            ///////// UNIQUE ACHIEVEMENT ROWS UNDER THE ACHIEVEMENT GRID ///////// 
            //////////////////////////////////////////////////////////////////////
            

            //////////////////////////////////////////////////////////////////////
            ///////////// THE ACTUAL ACHIEVEMENT BOXES IN THIS GRID ////////////// 
            //////////////////////////////////////////////////////////////////////
            void MakeAGoalBox(Achievement achievement, DockPanel AchievementPanel, int TheGoalWidth)
            {
                Brush TheGrayGrid = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0191919")); //From 50 to A0 transparency

                TextBox GoalBox = new();
                GoalBox.Tag = achievement;

                Achievement MyAchievement = GoalBox.Tag as Achievement;

                GoalBox.Width = TheGoalWidth;
                {
                    //GoalBox.Style = (Style)Application.Current.FindResource("LabelBox"); 
                    //GoalBox.Style = (Style)Application.Current.FindResource("NoMouseOverTextBoxStyle");
                    GoalBox.BorderThickness = new Thickness(1,1,1,1);
                    GoalBox.BorderBrush = TheGrayGrid;
                    
                }
                
                GoalBox.Text = MyAchievement.PlayerText;
                //GoalBox.FontSize = 20; //Font size is overwritten in method SetTextboxColor();
                GoalBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                GoalBox.VerticalContentAlignment = VerticalAlignment.Center;
                GoalBox.Background = TheBackColor;
                GoalBox.Foreground = TheTextColor;
                if (achievement.Difficulty == "") 
                {
                    //GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D4E4F"));
                    //GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    GoalBox.BorderBrush = TheGrayGrid; //TheBorderColor
                    GoalBox.BorderThickness = new Thickness(2);
                }
                AchievementPanel.Children.Add(GoalBox);
                SetTextboxColor(GoalBox);




                GoalBox.ContextMenu = null;

                GoalBox.PreviewMouseRightButtonDown += ChangeCC;
                void ChangeCC(object sender, RoutedEventArgs e)
                {

                    if (GoalBox.Text == "1CC")
                    {
                        GoalBox.Text = "";
                    }
                    else if (GoalBox.Text != "1CC")
                    {
                        GoalBox.Text = "1CC";
                    }
                    e.Handled = true;
                }

                GoalBox.TextChanged += TheTextChanged;
                void TheTextChanged(object sender, RoutedEventArgs e)
                {
                    MyAchievement.PlayerText = GoalBox.Text;
                    SetTextboxColor(GoalBox);
                    SavePlayer();
                    PlayerLevel(GameLauncher);


                }


                GoalBox.PreviewMouseLeftButtonDown += (sender, e) =>
                {
                    TextBox textBox = sender as TextBox;
                    if (textBox != null && !textBox.IsKeyboardFocusWithin)
                    {
                        textBox.Focus();
                        textBox.CaretIndex = textBox.Text.Length;
                        e.Handled = true;  // Prevent the default mouse down behavior
                    }
                };
                GoalBox.GotFocus += (sender, e) =>
                {
                    TextBox textBox = sender as TextBox;
                    if (textBox != null && !textBox.IsKeyboardFocusWithin)
                    {
                        textBox.CaretIndex = textBox.Text.Length;  // Ensure the caret is at the end when focus is gained
                    }
                };

                GoalBox.MouseEnter += (sender, e) =>
                {
                    GameLauncher.QuestLevelLabel.Content = MyAchievement.ShotType + " " + MyAchievement.Difficulty + " Lv: ";

                    if (MyAchievement.Level != 0)
                    {
                        GameLauncher.QuestLevelLabel.Content = GameLauncher.QuestLevelLabel.Content + MyAchievement.Level.ToString();
                    }
                    else 
                    {
                        GameLauncher.QuestLevelLabel.Content = GameLauncher.QuestLevelLabel.Content + "--";
                    }
                    
                };
            }
            //////////////////////////////////////////////////////////////////////
            ///////////// THE ACTUAL ACHIEVEMENT BOXES IN THIS GRID ////////////// 
            //////////////////////////////////////////////////////////////////////

        }




        private void SetTextboxColor(TextBox GoalBox) 
        {
            Achievement achievement = GoalBox.Tag as Achievement;
            string Difficulty = achievement.Difficulty;

            if (GoalBox.Text.Contains("1CC") || GoalBox.Text.Contains("1cc") || GoalBox.Text.Contains("NB") || GoalBox.Text.Contains("Clear") || GoalBox.Text.Contains("✓") || GoalBox.Text.Contains("✔") || GoalBox.Text.Contains("ND") || GoalBox.Text.Contains("NM") || GoalBox.Text.Contains("!") || GoalBox.Text.Contains("Hidden") || GoalBox.Text.Contains("HIDDEN"))
            {
                if (Difficulty == "")
                {
                    GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D4E4F"));
                    GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    //GoalBox.FontWeight = FontWeights.Bold;
                }
                if (Difficulty == "Easy")
                {
                    GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F3F4A"));
                    GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#839BC3"));
                    //GoalBox.FontWeight = FontWeights.Bold;
                }
                if (Difficulty == "Normal")
                {
                    GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F3E0F"));
                    GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#87AC87"));
                    //GoalBox.FontWeight = FontWeights.Bold;
                }
                if (Difficulty == "Hard")
                {
                    GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#603203"));
                    GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C78D31"));
                    //GoalBox.FontWeight = FontWeights.Bold;
                }
                if (Difficulty == "Lunatic" || Difficulty == "Unreal" || Difficulty == "Overdrive" || Difficulty == "URA")
                {
                    GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#520000"));
                    GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B47B6B"));
                    //GoalBox.FontWeight = FontWeights.Bold;
                }
                if (Difficulty == "Extra" || Difficulty == "Labyrinth")
                {
                    GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242563"));
                    GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7973d4"));
                    //GoalBox.FontWeight = FontWeights.Bold;
                }
                if (Difficulty == "Phantasm" || Difficulty == "A-Extra" || Difficulty == "Carrefour" || Difficulty == "Sequal" || Difficulty == "E-Extra" || Difficulty == "EX-Ult")
                {
                    GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#471759"));
                    GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b66eba"));
                    //GoalBox.FontWeight = FontWeights.Bold;
                }
                if (Difficulty == "Luna NB")
                {
                    GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A0E3E"));
                    GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#725681"));
                    //GoalBox.FontWeight = FontWeights.Bold;
                }
                if (Difficulty == "Extra NB")
                {
                    GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A0E3E"));
                    GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#725681"));
                    //GoalBox.FontWeight = FontWeights.Bold;
                }
            }
            else
            {
                GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(achievement.TheGame.ColorBack));
                GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(achievement.TheGame.ColorText));
            }
            if (GoalBox.Text.Length >= 19)
            {
                GoalBox.FontSize = 10;
                GoalBox.FontFamily = new FontFamily("Segoe UI");
            }
            if (GoalBox.Text.Length >= 18)
            {
                GoalBox.FontSize = 11;
                GoalBox.FontFamily = new FontFamily("Segoe UI");
            }
            if (GoalBox.Text.Length >= 17)
            {
                GoalBox.FontSize = 12;
                GoalBox.FontFamily = new FontFamily("Segoe UI");
            }
            else if (GoalBox.Text.Length >= 16)
            {
                GoalBox.FontSize = 13;
                GoalBox.FontFamily = new FontFamily("Segoe UI");
            }
            else if (GoalBox.Text.Length >= 15)
            {
                GoalBox.FontSize = 14;
                GoalBox.FontFamily = new FontFamily("Segoe UI");
            }
            else if (GoalBox.Text.Length >= 14)
            {
                GoalBox.FontSize = 15;
                GoalBox.FontFamily = new FontFamily("Segoe UI");
            }
            else if (GoalBox.Text.Length >= 13)
            {
                GoalBox.FontSize = 16;
                GoalBox.FontFamily = new FontFamily("Segoe UI");
            }
            else if (GoalBox.Text.Length >= 12)
            {
                GoalBox.FontSize = 18;
                GoalBox.FontFamily = new FontFamily("Segoe UI");
            }            
            else if (GoalBox.Text.Length >= 10)
            {
                GoalBox.FontSize = 20;
                GoalBox.FontFamily = new FontFamily("Segoe UI");
            }
            else
            {
                GoalBox.FontSize = 20;
                GoalBox.FontFamily = (FontFamily)GoalBox.FindResource("AppFont"); //new FontFamily("Dawns 10px ArkPixel");
            }

            if (GoalBox.Text.Contains("@") || GoalBox.Text.Contains("#") || GoalBox.Text.Contains("$")) //If text contains any "Negation" characters, make the achievement box gray (and don't award EXP).
            {
                GoalBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B3B3B"));
                GoalBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3B3B3"));
            }
            

        }


        private void SavePlayer() 
        {

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(LibraryTouhou.TouhouLauncherPath + "\\Player Profiles\\" + LibraryTouhou.PlayerName + ".xml", settings))
            {
                writer.WriteStartElement("Root");
                writer.WriteStartElement("AchievementList");

                foreach (Achievement achievement in LibraryTouhou.MasterAchievementsList)
                {
                    writer.WriteStartElement("Achievement");
                    writer.WriteElementString("Type", "Basic");
                    writer.WriteElementString("Name", achievement.Name);
                    writer.WriteElementString("Difficulty", achievement.Difficulty);
                    writer.WriteElementString("ShotType", achievement.ShotType);
                    writer.WriteElementString("Key", achievement.Key);
                    writer.WriteElementString("Level", achievement.Level.ToString());
                    writer.WriteElementString("PlayerText", achievement.PlayerText);
                    writer.WriteElementString("Note", achievement.Note);
                    writer.WriteEndElement(); //End Achievement 
                }

                writer.WriteEndElement(); //End AchievementList  
                writer.WriteEndElement(); //End Root  
                writer.Flush(); //Ends the XML 
            }
        }






    }
}

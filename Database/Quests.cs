using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Western_Launcher_Of_Eastern_Origins
{
    internal class Quests
    {
        bool QuestDebugMode = true;
        GameLauncher GameLauncher { get; set; }
        List<Achievement> ToDoAchievements { get; set; } = new();

        public void CreateQuestBoard(GameLauncher TheGameLauncher)  //Decides what quests to suggest to the user today.
        {
            this.GameLauncher = TheGameLauncher;
            GameLauncher.QuestPanel.Children.Clear();

            int ThePlayerLevel = LibraryTouhou.GetPlayerLevel(GameLauncher); //Player Level is the 6th highest lv of achievement.


            foreach (Achievement achievement in LibraryTouhou.MasterAchievementsList) //Add all achievements to a "To do" list.
            {
                ToDoAchievements.Add(achievement);
            }

            foreach (Achievement achievement in LibraryTouhou.MasterAchievementsList) //Remove from todo list if they meet any of these conditions.
            {                
                if (achievement.PlayerText == "1CC"){ ToDoAchievements.Remove(achievement); continue; }
                if (achievement.Level == 0) { ToDoAchievements.Remove(achievement); continue; }
                if (Properties.Settings.Default.QuestsPC98 == false && achievement.TheGame.Type == GameType.PC98) { ToDoAchievements.Remove(achievement); continue; }
                if (achievement.Difficulty == "Extra") 
                {
                    bool cleared = false; //Lets first check if the extra stage is even unlocked.
                    foreach (Achievement AB in LibraryTouhou.MasterAchievementsList)
                    {
                        if (AB.TheGame == achievement.TheGame && AB.ShotType == achievement.ShotType && AB.Difficulty != "Easy")
                        {
                            if ((AB.PlayerText.Contains("1CC") || AB.PlayerText.Contains("1cc") || AB.PlayerText.Contains("NB") || AB.PlayerText.Contains("Clear") || AB.PlayerText.Contains("✓") || AB.PlayerText.Contains("✔") || AB.PlayerText.Contains("ND") || AB.PlayerText.Contains("NM") || AB.PlayerText.Contains("!")))
                            {
                                if (!AB.PlayerText.Contains("@") && !AB.PlayerText.Contains("#") && !AB.PlayerText.Contains("$"))
                                {
                                    cleared = true;
                                    break;
                                }

                            }

                        }
                    }

                    if (cleared == false) { ToDoAchievements.Remove(achievement); continue; }
                }
            }
            if (Properties.Settings.Default.QuestsChronological == true) { QuestsChronological(ToDoAchievements); } 

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

            
            void QuestsChronological(List<Achievement> PossibleQuests) //Require games to be 1CCd in chronological order. (the player must 1CC the previous game, before the next game can be recommended.)
            {

                int unlockedIndex = 0; // Touhou 6 is always available

                for (int i = 0; i < LibraryTouhou.GameOrder.Length - 1; i++)
                {
                    bool hasClear = LibraryTouhou.MasterAchievementsList.Any(a =>a.TheGame.SeriesName == LibraryTouhou.GameOrder[i] && LibraryTouhou.IsValidAchievement(a));

                    if (!hasClear) 
                    {
                        break;
                    }
                       

                    unlockedIndex = i + 1;
                }

                PossibleQuests.RemoveAll(a =>Array.IndexOf(LibraryTouhou.GameOrder, a.TheGame.SeriesName) > unlockedIndex);
            }


            for (int QuestNumber = 0; QuestNumber < 5;) //Roll 5 quests. (only 0~4 aka a total of 5 will be rolled).
            {


                if (QuestNumber < 1 && Properties.Settings.Default.Quest1LastGame == true)  //Last Launched Game
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
                        QuestNumber++;
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
                            if (achievement.PlayerText != "1CC" && achievement.Level <= ThePlayerLevel && (achievement.Difficulty == "Extra" || achievement.Difficulty == "Phantasm"))
                            {
                                TheList.Add(achievement);
                            }
                        }
                        if (TheList.Count != 0)
                        {
                            MakeAQuest(TheList, "Last");
                            QuestNumber++;
                            continue;
                        }
                    }

                }

                Random RNG = new Random();
                int Quest1RNG = RNG.Next(1, 3); //the max roll is confusingly not possible. >:(
                if (QuestNumber == 1 && Quest1RNG == 1) //NewGame or NewExtra. 50% chance to recommend. Recommend Lv-1 or lower.
                {
                    //NOTE: Rewrite my quest 2 (this one) code so that it's a 33% chance to be an extra, and is otherwise a game. 
                    //And it only passes and does a quest 3 roll if no possible quests are valid. 

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
                            QuestNumber++;
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
                            QuestNumber++;
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
                            QuestNumber++;
                            continue;
                        }



                    }


                }




                if (QuestNumber == 1 && Quest1RNG == 2) //new extra
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
                        QuestNumber++;
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




                if (QuestNumber < 3) //RECOMMEND LOWER LV. Lv-X. X starts at -1, and keeps going.
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
                        QuestNumber++;
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

                if (QuestNumber < 4) //RECOMMEND SAME LEVEL.
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
                        QuestNumber++;
                        continue;
                    }
                }

                if (QuestNumber < 5) //RECOMMEND HIGH LEVEL. If none, another +1 untill finding something. Stops at Lv+100.
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
                        QuestNumber++;
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

                if (QuestNumber < 6) //Totally Random. Canot normally get this. A safety measure.
                {
                    MakeAQuest(ToDoAchievements, "RNG");
                    QuestNumber++;
                    continue;
                }


                QuestNumber++; //Safety measure against players doing literally everything.

            }


            




        }


        private void MakeAQuest(List<Achievement> TheQuests, string ExtraText) //Creates a quest chosen at random from a given list.
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


            if (QuestDebugMode == true) //Show relative difficulty to player level.
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
}

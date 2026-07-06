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
using Western_Launcher_Of_Eastern_Origins.Editors;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;

namespace Western_Launcher_Of_Eastern_Origins
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
                    LibraryTouhou.GetPlayerLevel(GameLauncher);


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

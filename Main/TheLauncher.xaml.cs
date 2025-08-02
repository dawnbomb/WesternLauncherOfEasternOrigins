using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices; //For borderless gaming check
using System.Runtime.Intrinsics.X86;
using System.Security.AccessControl; //for making folders not be read only in my documents.
using System.Security.Policy;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using Ookii.Dialogs.Wpf;
using PixelWPF;
using WesternLauncherOfEasternOrigins.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

//USES THE FOLLOWING NUGET PACKAGES:
//OOkii.Dialogs.WPF
//Newtonsoft.Json
//EPPlus - used to load excel sheet (the games and mods list). Loading from a sheet lets users modify the sheet and customize it.
//Microsoft.Web.WebView2 - used for the browser, for the achievement tracker.

//M:
//cd "M:\Touhou Launcher\New Touhou Launcher"
//dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
//
//build is found in bin/release/net7/win-x64/publish





//NOTE TO SELF!!!!
//Maybe once a year(?) i should delete all mods in repo, then run every single game (twice) with every mod enabled to update them all, then copy everything back to the mods folder, and re-release / update the program.
//Note 2: Mods update after CLOSING the game / thcrap, they do NOT fully download from running thcrap, nor do they update before launching a game (this seems stupid)
//This means to add a new mod, select it in thcrap, hit next, stop and set it up for my program. Then, run the game, close it, then copy it to the mods folder. 

namespace WesternLauncherOfEasternOrigins
{

    //Ship with *most* mods.
    //Some mods require game to restart.
    //these mods will have a notification symbol.

        //in DEBUG mode if the mods are already downloaded, have a removal symbol?

    
    

    public partial class GameLauncher : Window, INotifyPropertyChanged
    {   
        string GameInfoPath { get; set; } = ""; //Stores where did the user set the game location, mod list, etc. Its a variable incase i later allow users to change this folder location.
        string Game { get; set; } = "";
        GameData TheGame {  get; set; }
        List<TouhouMod> EnabledMods { get; set; } = new();

        public Button CurrentGame { get; set; }

        public GameLauncher()
        {          

            InitializeComponent();
            this.Title = "Western Launcher of Eastern Origins  v" + LibraryTouhou.VersionNumber + "  (" + LibraryTouhou.VersionDate + ")";
            Dispatcher.InvokeAsync(async () => await PixelWPF.GithubUpdater.CheckForUpdatesAsync("WesternLauncherOfEasternOrigins", "dawnbomb/WesternLauncherOfEasternOrigins/releases/latest", LibraryTouhou.VersionNumber));

            DataContext = this;
            

            string basepath = AppDomain.CurrentDomain.BaseDirectory;
            #if DEBUG
            LibraryTouhou.TouhouLauncherPath = "D:\\Western Launcher of Eastern Origins"; //Path.GetFullPath(Path.Combine(basepath, @"..\..\..\..\Western Launcher of Eastern Origins"));
            #else
            LibraryTouhou.TouhouLauncherPath = basepath;
            DevMenu.Visibility = Visibility.Collapsed;
            //DevMenu.IsEnabled = false;
            #endif

            LibraryTouhou.ModsXMLLocation = LibraryTouhou.TouhouLauncherPath + "\\Other\\Touhou Mods.xml";
            LibraryTouhou.GamesXMLLocation = LibraryTouhou.TouhouLauncherPath + "\\Other\\Touhou Games.xml";
                        
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            GameInfoPath = Path.Combine(documentsPath, "Western Launcher of Eastern Origins", "Game Info");            
            Directory.CreateDirectory(GameInfoPath);// Ensure the directory exists


            {   //SETTINGS 
                Tab1.Visibility = Visibility.Collapsed;
                Tab2.Visibility = Visibility.Collapsed;
                if (Properties.Settings.Default.VolumeMixerAtLauncher == false) { VolumeMixerButton.Visibility = Visibility.Collapsed; }
                if (Properties.Settings.Default.ShowQuestBoard == false) { QuestBoardBorder.Visibility = Visibility.Collapsed; }
                if (Properties.Settings.Default.ShowNotepad == false) { NotepadBorder.Visibility = Visibility.Collapsed; }
                if (Properties.Settings.Default.ShowNewPlayerButton == false) { NewPlayerButton.Visibility = Visibility.Collapsed; }
                if (Properties.Settings.Default.ShowPlayerLv == false) { PlayerLvPanel.Visibility = Visibility.Collapsed; }

                Notepad.Text = Properties.Settings.Default.NotepadText;
                ResolutionComboBox.Text = Properties.Settings.Default.Resolution;
            }

            

            Loading Loading = new();
            Loading.LoadTheMods();    //Loads a master mod list. Thats every mod for every game. 
            Loading.LoadTheGames();   //Loads all game data, their achievements, and mods for them from mod list.               

            Achievements Achievements = new();
            Achievements.LoadAllPlayers(this); //Loads all player names into combobox. Also selects a player.
            Achievements.LoadPlayerAchievements(); //Loads the currently selected player's achievement data.
            Achievements.CreateQuestBoard(this); //Decides a list of recommended quests based on the current players skill level and achievements. 
            Achievements.CreateAchievementsBoard(this);  //Shows every achievement in a master list, and the current players progression for all of them.

            //TouhouGames TouhouGames = new(); //THE OLD REFERENCE
            //TouhouGames.SetupTouhouGames(LibraryMan.TouhouLauncherPath);
            //TouhouMods TouhouMods = new();
            //TouhouMods.SetupTouhouMods(ModsList);


            GenerateLauncher();

            
            this.Loaded += new RoutedEventHandler(SetupDebugTabMods);
            this.Loaded += new RoutedEventHandler(FocusLastLaunchedGame);
            
        }
                

        public void SetupDebugTabMods(object sender, RoutedEventArgs e) 
        {
            {   //STEP: Show modslist mods
                HashSet<string> uniqueEntries = new HashSet<string>();
                // Collect entries from ModsList
                foreach (TouhouMod mod in LibraryTouhou.MasterModsList)
                {
                    // Add the THCrapText to the HashSet, which automatically avoids duplicates
                    uniqueEntries.Add(mod.THCrapText);
                }

                // Convert the HashSet to a List and sort it alphabetically
                List<string> sortedEntries = uniqueEntries.ToList();
                sortedEntries.Sort();

                // Clear existing text and append sorted, unique entries
                TextThingy2.Text = "";
                foreach (string entry in sortedEntries)
                {
                    TextThingy2.Text += "\n" + entry;
                }
            }

            {   //STEP: Show all touhou crap repo folder mods
                string reposPath = Path.Combine(LibraryTouhou.TouhouLauncherPath, "Other", "Extra Programs", "Touhou Crap", "repos");

                // Check if the repos directory exists
                if (Directory.Exists(reposPath))
                {
                    // Get all directories within the 'repos' directory (one level deep)
                    var firstLevelDirectories = Directory.GetDirectories(reposPath, "*", SearchOption.TopDirectoryOnly);

                    foreach (var firstLevelDirectory in firstLevelDirectories)
                    {
                        // Get second level directories within each first-level directory
                        var secondLevelDirectories = Directory.GetDirectories(firstLevelDirectory, "*", SearchOption.TopDirectoryOnly);
                        foreach (var secondLevelDirectory in secondLevelDirectories)
                        {
                            // Create a DirectoryInfo object to easily get detailed information
                            var dirInfo = new DirectoryInfo(secondLevelDirectory);
                            var parentDirInfo = dirInfo.Parent; // This is the first-level directory
                            var grandparentDirInfo = parentDirInfo?.Parent; // This is the repos directory

                            // Format to include "repos/" at the beginning
                            var pathToShow = grandparentDirInfo != null ? grandparentDirInfo.Name + "/" + parentDirInfo.Name + "/" + dirInfo.Name : parentDirInfo.Name + "/" + dirInfo.Name;

                            // Append the full path including "repos/"
                            TextThingy3.Text += $"\n{pathToShow}";
                        }
                    }
                }
            }


            {   //STEP: Compared every mod folder inside the touhou crap repos, with every mod path in ModsList, and displays the diffrences except nmlgc and thpatch folders (eng patch).
                // Collect entries from ModsList
                HashSet<string> modsListEntries = new HashSet<string>();
                foreach (var mod in LibraryTouhou.MasterModsList)
                {
                    modsListEntries.Add(mod.THCrapText); // These entries already include the trailing slash
                }

                // Collect entries from the Touhou Crap repo folders, excluding certain directories
                HashSet<string> repoEntries = new HashSet<string>();
                string reposPath = Path.Combine(LibraryTouhou.TouhouLauncherPath, "Other", "Extra Programs", "Touhou Crap", "repos");
                if (Directory.Exists(reposPath))
                {
                    var firstLevelDirectories = Directory.GetDirectories(reposPath, "*", SearchOption.TopDirectoryOnly);
                    foreach (var firstLevelDirectory in firstLevelDirectories)
                    {
                        // Exclude directories from the nmlgc and thpatch folders
                        if (firstLevelDirectory.EndsWith("nmlgc") || firstLevelDirectory.EndsWith("thpatch"))
                        {
                            continue; // Skip this directory and its subdirectories
                        }

                        var secondLevelDirectories = Directory.GetDirectories(firstLevelDirectory, "*", SearchOption.TopDirectoryOnly);
                        foreach (var secondLevelDirectory in secondLevelDirectories)
                        {
                            var dirInfo = new DirectoryInfo(secondLevelDirectory);
                            var pathToShow = dirInfo.Parent?.Parent?.Name + "/" + dirInfo.Parent?.Name + "/" + dirInfo.Name + "/"; // Ensure a trailing slash
                            if (!string.IsNullOrWhiteSpace(pathToShow))
                            {
                                repoEntries.Add(pathToShow);
                            }
                        }
                    }
                }

                // Find entries that are in repoEntries but not in modsListEntries
                var uniqueToRepo = new HashSet<string>(repoEntries.Except(modsListEntries));

                // Sort and display these unique entries in TextThingy4
                List<string> sortedUniqueToRepo = uniqueToRepo.ToList();
                sortedUniqueToRepo.Sort();
                TextThingy4.Text = string.Join("\n", sortedUniqueToRepo);
            }
            
            
        }

        public void FocusLastLaunchedGame(object sender, RoutedEventArgs e) 
        {
            if (Properties.Settings.Default.ScrollToLastGame == false) 
            {
                return;
            }

            string LastGame = Properties.Settings.Default.LastTouhouGame;


            foreach (Button TheButton in GamesPanel.Children)
            {
                LastGame = Properties.Settings.Default.LastTouhouGame;

                GameData TheGame = TheButton.Tag as GameData;
                if (Properties.Settings.Default.LastTouhouGame == TheGame.CodeName)
                {
                    // This Scrolls the ScrollViewer so X is scrolled to the top.
                    GeneralTransform transform = TheButton.TransformToAncestor(GameScrollViewer);
                    Point relativeLocation = transform.Transform(new Point(0, 0));
                    GameScrollViewer.ScrollToVerticalOffset(relativeLocation.Y);
                    TheButton.BringIntoView();
                    break;
                }
            }

            foreach (Border theborder in MasterAchievementPanel.Children)
            {
                GameData TheGame = theborder.Tag as GameData;
                if (Properties.Settings.Default.LastTouhouGame == TheGame.CodeName)
                {
                    // This Scrolls the ScrollViewer so X is scrolled to the top.
                    GeneralTransform transform = theborder.TransformToAncestor(AchievementScrollViewer);
                    Point relativeLocation = transform.Transform(new Point(0, 0));
                    AchievementScrollViewer.ScrollToVerticalOffset(relativeLocation.Y);
                    //theborder.BringIntoView();
                    break;
                }
            }
        }


        public string CardColor { get; set; } = "#FF1D1D1D";
        //New launceher
        private void GenerateLauncher()
        {


            foreach (GameData GameData in LibraryTouhou.MasterGameList)
            {
                if (GameData.Type == GameType.Hidden) { continue; }

                var Game = GameData;


                Button TheButton = new();
                TheButton.Style = null;
                TheButton.Tag = GameData;
                TheButton.BorderBrush = Brushes.Black;
                TheButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CardColor));
                TheButton.Height = 80;
                DockPanel.SetDock(TheButton, Dock.Top);
                TheButton.Click += (sender, e) => GameLoad(Game, TheButton);

                ContextMenu contextMenu = new();
                TheButton.ContextMenu = contextMenu;
                

                foreach (GameLink gamelink in GameData.LinkList) 
                {
                    MenuItem NewLinkItem = new MenuItem { Header = gamelink.Name, ToolTip = gamelink.Tooltip };
                    contextMenu.Items.Add(NewLinkItem);
                    NewLinkItem.Click += OpenLink;//ButtonCreateWorkshop2; 
                    void OpenLink(object sender, RoutedEventArgs e)
                    {
                        PixelWPF.LibraryMan.OpenWebsiteURL(gamelink.URL);
                    }
                }

                // Define the GameLoad event handler method


                DockPanel Card = new();
                Card.Background = Brushes.Transparent;
                Card.Width = 500; //THIS FIXES A WEIRD ISSUE WHERE I CAN'T GET THE CARD TO ACTUALLY ATTACH TO THE LEFT. (so if text isn't wide enough the image isn't touching left)
                Card.HorizontalAlignment = HorizontalAlignment.Left;
                DockPanel.SetDock(Card, Dock.Left);
                TheButton.Content = Card;

                Image Image = new();
                Image.HorizontalAlignment = HorizontalAlignment.Left;                
                DockPanel.SetDock(Image, Dock.Left);
                Image.Width = 190;
                Card.Children.Add(Image);
                Image.Stretch = Stretch.Fill;
                if (Game.CardArt != null && Game.CardArt != "") 
                {
                    if (File.Exists(System.IO.Path.Combine(LibraryTouhou.TouhouLauncherPath, "Card Art", Game.CardArt)))
                    { 
                        Image.Source = new BitmapImage(new Uri(System.IO.Path.Combine(LibraryTouhou.TouhouLauncherPath, "Card Art", Game.CardArt), UriKind.Absolute)); 
                    }
                    
                }
                else
                {
                    Image.Source = new BitmapImage(new Uri("pack://application:,,,/Graphics/Title Buttons/Unknown.png"));
                }

                DockPanel RightPanel = new();
                RightPanel.Background = Brushes.Transparent;
                Card.Children.Add(RightPanel);
                DockPanel.SetDock(RightPanel, Dock.Left);
                RightPanel.VerticalAlignment = VerticalAlignment.Center;                

                Label LabelTitleDate = new();
                RightPanel.Children.Add(LabelTitleDate);
                DockPanel.SetDock(LabelTitleDate, Dock.Top);
                LabelTitleDate.FontSize = 10;
                LabelTitleDate.Content = Game.SeriesName + " - " + Game.Date;

                Label LabelName = new();
                RightPanel.Children.Add(LabelName);
                DockPanel.SetDock(LabelName, Dock.Top);
                LabelName.FontSize = 10;
                LabelName.Content = Game.SubtitleName;
                //

                GamesPanel.Children.Add(TheButton);

                //this should really not be here, and is (somehow???) causing a bug where the bottom game is highlighted, even though the correct one is loaded.
                if (Properties.Settings.Default.LastTouhouGame == GameData.CodeName) { GameLoad(GameData, TheButton); } 
                


            }

            RefreshGameCards();
        }


        



        private void GameLoad(GameData Game, Button TheButton) //Trigger: When a Game card (button) is clicked.
        {
            //TheButton.Background = Brushes.MidnightBlue;
            //if (CurrentGame != null) 
            //{
            //    if (TheButton != CurrentGame) 
            //    {                    
            //        CurrentGame.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CardColor));
            //    }
                
            //}

            RefreshGameCards();
            TheButton.Background = Brushes.MidnightBlue;
            CurrentGame = TheButton;

            TheGame = Game;
            LabelGameName2.Content = /*Game.SeriesName + " " +*/ Game.SubtitleName;
            BasicsText.Text = Game.Description;
            
           

            ModsPanel.Children.Clear();
            EnabledMods.Clear();
            List<string> loadedMods = LoadGameInfoFromXml(Game.CodeName);

            try 
            {
                foreach (Border theborder in MasterAchievementPanel.Children)
                {
                    GameData TheGame = theborder.Tag as GameData;
                    if (Game == TheGame)
                    {
                        // This Scrolls the ScrollViewer so X is scrolled to the top.
                        GeneralTransform transform = theborder.TransformToAncestor(AchievementScrollViewer);
                        Point relativeLocation = transform.Transform(new Point(0, 0));
                        AchievementScrollViewer.ScrollToVerticalOffset(relativeLocation.Y);
                        theborder.BringIntoView();
                        break;
                    }
                }
            } catch { }
            


            foreach (TouhouMod GameMod in Game.ModList) //creates a checkbox for each mod.
            {
                DockPanel ModPanel = new DockPanel();
                ModsPanel.Children.Add(ModPanel);
                DockPanel.SetDock(ModPanel, Dock.Top);
                ModPanel.Background = Brushes.Transparent;
                //DockPanel.Height = 26;

                ContextMenu ModMenu = new();
                ModPanel.ContextMenu = ModMenu;

                MenuItem OpenModFolder = new MenuItem { Header = "Open Mod Folder" };                
                ModMenu.Items.Add(OpenModFolder);


                CheckBox ModCheckBox = new();
                ModPanel.Children.Add(ModCheckBox);
                ModCheckBox.LayoutTransform = new ScaleTransform(1.5, 1.5);
                ModCheckBox.Margin = new Thickness(10,0,0,0);

                foreach (TouhouMod TMod in LibraryTouhou.MasterModsList) 
                { 
                    if (TMod.Key == GameMod.Key) 
                    {
                        ModCheckBox.Tag = TMod;

                        if (TMod.Recommend == true) 
                        {
                            Label LabelRecommend = new();
                            ModPanel.Children.Add(LabelRecommend);
                            LabelRecommend.Content = "👍";
                            LabelRecommend.FontSize = 20;
                            LabelRecommend.Margin = new Thickness(0, -5, 0, 0);
                            LabelRecommend.ToolTip = "This mod is recommended, as it generally just makes the game better.";
                            ToolTipService.SetInitialShowDelay(LabelRecommend, 0);
                        }



                        string THCrapFolderPath = System.IO.Path.Combine(LibraryTouhou.TouhouLauncherPath, "Other","Extra Programs", "Touhou Crap");
                        string THCrapFolderPath2 = LibraryTouhou.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\" + TMod.THCrapText;
                        if (!System.IO.Directory.Exists(THCrapFolderPath2)) //Checks to make sure the mod exists in touhou crap. If not, make the mod Orange. This is because the program must move the mod there. 
                        {
                            Label LabelWarn = new();
                            ModPanel.Children.Add(LabelWarn); //👍
                            LabelWarn.Content = "⚠";
                            LabelWarn.FontSize = 20;
                            LabelWarn.Margin = new Thickness(-3, -3, 0, 0);
                            LabelWarn.Foreground = Brushes.Orange;
                            LabelWarn.ToolTip = "Missing from Touhou Crap Repos";
                            ToolTipService.SetInitialShowDelay(LabelWarn, 0);

                            OpenModFolder.Foreground = Brushes.Gray;
                        }
                        else 
                        {
                            OpenModFolder.Click += OpenWFolder;//ButtonCreateWorkshop2;
                        }

                        void OpenWFolder(object sender, RoutedEventArgs e)
                        {
                            
                            PixelWPF.LibraryMan.OpenFolder(THCrapFolderPath2);

                        }

                        //string modsFolderPath = System.IO.Path.Combine(TouhouLauncherPath, "Extra Programs", "Mods");
                        //string modsFolderPath2 = TouhouLauncherPath + "\\Extra Programs\\Mods\\" + TMod.Value.THCrapText;
                        //string modifiedPath = modsFolderPath2.Replace("repos/", "");
                        ////"repos/Revenant/ShowRank/"
                        //if (!System.IO.Directory.Exists(modifiedPath)) //Checks to make sure the mod exists in the modsfolder.Ifnot,make the mod red. This means the mod can't get to end users when published, big problem!
                        //{
                        //    Label LabelWarn = new();
                        //    DockPanel.Children.Add(LabelWarn); //👍
                        //    LabelWarn.Content = "⚠";
                        //    LabelWarn.FontSize = 14;
                        //    LabelWarn.Margin = new Thickness(-3, -3, 0, 0);
                        //    LabelWarn.Foreground = Brushes.Red;
                        //    LabelWarn.ToolTip = "Missing from Mods Folder";
                        //    ToolTipService.SetInitialShowDelay(LabelWarn, 0);
                        //}

                        Label Label = new();
                        ModPanel.Children.Add(Label); //👍
                        Label.Content = TMod.Name;
                        Label.FontSize = 20;
                        Label.Margin = new Thickness(-3, -3, 0, 0);

                        

                        ModPanel.MouseEnter += (s, e) =>
                        {
                            ModPanel.Background = new SolidColorBrush(Colors.DarkSlateGray);
                            TextBoxModName.Text = TMod.Name;
                            TextBoxModDescription.Text = TMod.Description;
                                                        
                            string DisplayPath = TMod.THCrapText.Replace("repos/", "");
                            TextBoxModPath.Text = DisplayPath;
                        };
                        ModPanel.MouseLeave += (s, e) =>
                        {
                            ModPanel.Background = Brushes.Transparent; // Or any default color you wish
                        };
                        ModPanel.MouseLeftButtonDown += (s, e) =>
                        {
                            ModCheckBox.IsChecked = !ModCheckBox.IsChecked;
                            // If you need to explicitly call the CheckBox's Click event handler
                            CheckboxClick(ModCheckBox, GameMod); // Assuming CheckboxClick can accept these parameters
                        };    


                        ModCheckBox.IsChecked = loadedMods.Contains(TMod.Key.ToString());
                        if (ModCheckBox.IsChecked == true) { EnabledMods.Add(TMod); }
                                                
                    }  
                }                
                ModCheckBox.Click += (sender, e) => CheckboxClick(sender, GameMod);                               
                
            }


            

        }





        



        //OTHER SHIT



        private List<string> LoadGameInfoFromXml(string gameCodeName)
        {
            GameLocationTextbox.Text = "";

            var modsList = new List<string>();            
            string filePath = System.IO.Path.Combine(GameInfoPath, gameCodeName + ".xml");

            if (File.Exists(filePath))
            {
                XDocument xmlDoc = XDocument.Load(filePath);
                var gameExePathElement = xmlDoc.Root.Element("GameExePath");
                if (gameExePathElement != null)
                {
                    GameLocationTextbox.Text = gameExePathElement.Value;
                }

                var mods = xmlDoc.Descendants("Mod");
                foreach (var mod in mods)
                {
                    var keyElement = mod.Element("Key");
                    if (keyElement != null)
                    {
                        modsList.Add(keyElement.Value);
                    }
                }
            }

            return modsList;

        }

        

        private void SaveUserGameInfo() 
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(GameInfoPath + "\\" + TheGame.CodeName + ".xml", settings))
            {
                writer.WriteStartElement("Root");

                writer.WriteElementString("GameExePath", GameLocationTextbox.Text);
                writer.WriteStartElement("ModsList");
                foreach (TouhouMod GameMod in EnabledMods)
                {
                    writer.WriteStartElement("Mod");
                    writer.WriteElementString("Name", GameMod.Name); //The mod Key.
                    writer.WriteElementString("Key", GameMod.Key); //The mod Key.
                    writer.WriteEndElement(); //End Mod
                }
                writer.WriteEndElement(); //End Mods List
                writer.WriteEndElement(); //End Root

                writer.Flush(); //Ends the XML GameFile
            }
        }


        private void SetGameLocation(object sender, RoutedEventArgs e) //Trigger: Click Browse...
        {
            VistaOpenFileDialog FileSelect = new VistaOpenFileDialog();
            FileSelect.Title = "Please select the game exe";
            if ((bool)FileSelect.ShowDialog(this))
            {
                GameLocationTextbox.Text = FileSelect.FileName;

                SaveUserGameInfo();



            }
        }
        private void CheckboxClick(object sender, TouhouMod TMod)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsChecked == true) // If the checkbox is being checked and the mod is not already in the list, add it
                {
                    if (!EnabledMods.Contains(TMod))
                    {
                        EnabledMods.Add(TMod);
                    }
                }
                else // If the checkbox is being unchecked and the mod is in the list, remove it
                {
                    if (EnabledMods.Contains(TMod))
                    {
                        EnabledMods.Remove(TMod);
                    }
                }
            }
            SaveUserGameInfo();
        }


        





        


        public void PressEnterKey()
        {
            var inputs = new NativeMethods.INPUT[]
            {
                new NativeMethods.INPUT
                {
                    Type = NativeMethods.INPUT_KEYBOARD,
                    Data = new NativeMethods.INPUT.InputUnion
                    {
                        Keyboard = new NativeMethods.KEYBDINPUT
                        {
                            Vk = 0x0D, // Virtual-Key code for the Enter key
                            Scan = 0,
                            Flags = 0, // 0 for key press
                            Time = 0,
                            ExtraInfo = IntPtr.Zero
                        }
                    }
                },
                new NativeMethods.INPUT
                {
                    Type = NativeMethods.INPUT_KEYBOARD,
                    Data = new NativeMethods.INPUT.InputUnion
                    {
                        Keyboard = new NativeMethods.KEYBDINPUT
                        {
                            Vk = 0x0D,
                            Scan = 0,
                            Flags = NativeMethods.KEYEVENTF_KEYUP, // Key up
                            Time = 0,
                            ExtraInfo = IntPtr.Zero
                        }
                    }
                }
            };

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }




        




        
        private void ResolutionBoxClosed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Resolution = ResolutionComboBox.Text;
            Properties.Settings.Default.Save();
        }



        //All this is for getting NoMoreBorder to open properly D:
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const int SW_RESTORE = 9;
        const int SW_SHOW = 5;

        const uint WM_SYSCOMMAND = 0x0112;
        const int SC_RESTORE = 0xF120;
        const int SC_SHOW = 0xF130; // Not defined, but maybe app is using custom handling









        private void OpenGameFolder(object sender, RoutedEventArgs e) //Trigger: Click Open
        {
            if (System.IO.File.Exists(GameLocationTextbox.Text))
            {
                string directoryPath = System.IO.Path.GetDirectoryName(GameLocationTextbox.Text);

                // Use Process.Start to open the folder in File Explorer
                System.Diagnostics.Process.Start("explorer.exe", directoryPath);
            }
            
        }

        

        

        private void OpenVolumeMixer(object sender, RoutedEventArgs e)
        {
            try
            {
                // Path to the Windows Volume Mixer (sndvol.exe)
                string volumeMixerPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\sndvol.exe";

                // Start the Volume Mixer
                System.Diagnostics.Process.Start(volumeMixerPath);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                MessageBox.Show("Failed to open the Volume Mixer: " + ex.Message);
            }
        }

        

        

        private void NotepadTextChanged(object sender, TextChangedEventArgs e)
        {
            

            Properties.Settings.Default.NotepadText = Notepad.Text;
            Properties.Settings.Default.Save();          



        }
                


        private void OpenEditorWindow(object sender, RoutedEventArgs e)
        {
            Window Editor = new Editor();
            Editor.Show();
        }

        private void ButtonNewPlayerClick(object sender, RoutedEventArgs e)
        {
            CreateNewPlayer();
        }

        public void CreateNewPlayer() 
        {
            NewPlayerProfile dialog = new NewPlayerProfile(this);
            if (dialog.ShowDialog() == true)
            {
                
            }
            
        }

        private void PlayerComboboxDropdownClosed(object sender, EventArgs e)
        {
            //select player and close / kill.
            ComboBoxItem comboBoxItem = PlayerComboBox.SelectedItem as ComboBoxItem;
            if (comboBoxItem == null) 
            {
                return;
            }
            //if (comboBoxItem.Content as string == Properties.Settings.Default.LastPlayer) 
            //{
            //    return;
            //}


            Properties.Settings.Default.LastPlayer = comboBoxItem.Content as string;
            LibraryTouhou.PlayerName = comboBoxItem.Content as string;
            Properties.Settings.Default.Save();

            Achievements Achievements = new();
            Achievements.LoadPlayerAchievements(); //Loads the currently selected player's achievement data.
            Achievements.CreateQuestBoard(this); //Decides a list of recommended quests based on the current players skill level and achievements. 
            Achievements.CreateAchievementsBoard(this);  //Shows every achievement in a master list, and the current players progression for all of them.
        }

        private void RefreshAchievements(object sender, RoutedEventArgs e)
        {
            RefreshGameCards();

            Achievements Achievements = new();
            Achievements.LoadPlayerAchievements(); //Loads the currently selected player's achievement data.

            if (Properties.Settings.Default.ShowQuestBoard == false)
            {
                QuestBoardBorder.Visibility = Visibility.Collapsed;
                return;
            }
            if (Properties.Settings.Default.ShowQuestBoard == true)
            {
                QuestBoardBorder.Visibility = Visibility.Visible;
            }


            Achievements.CreateQuestBoard(this); //Decides a list of recommended quests based on the current players skill level and achievements. 
            Achievements.CreateAchievementsBoard(this);  //Shows every achievement in a master list, and the current players progression for all of them.                       

        }

        private void RefreshGameCards()
        {

            foreach (Button button in GamesPanel.Children.OfType<Button>())
            {
                button.Visibility = Visibility.Visible;

                GameData touhouGame = button.Tag as GameData;

                if (touhouGame.Type == GameType.PC98 && Properties.Settings.Default.ShowTouhouPC98 == false)
                {
                    button.Visibility = Visibility.Collapsed;
                }
                if (touhouGame.Type == GameType.Lenen && Properties.Settings.Default.ShowLenenGames == false)
                {
                    button.Visibility = Visibility.Collapsed;
                }
                if (touhouGame.Type == GameType.Kaisendou && Properties.Settings.Default.ShowKaisendouGames == false)
                {
                    button.Visibility = Visibility.Collapsed;
                }
                if (touhouGame.Type == GameType.MajorTouhouFanGames && Properties.Settings.Default.ShowFanBulletHell == false)
                {
                    button.Visibility = Visibility.Collapsed;
                }
                if (touhouGame.Type == GameType.OtherTouhouFanGames && Properties.Settings.Default.ShowOtherFanTouhouBulletHell == false)
                {
                    button.Visibility = Visibility.Collapsed;
                }
                if (touhouGame.CodeName == "th075" && Properties.Settings.Default.ShowTouhou075 == false)
                {
                    button.Visibility = Visibility.Collapsed;
                }


            }


            

            foreach (Button button in GamesPanel.Children.OfType<Button>()) //Colors! 
            {                
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CardColor));

                if (Properties.Settings.Default.ColorGameCards == false)
                {
                    continue;
                }

                GameData touhouGame = button.Tag as GameData;

                
                if (touhouGame.Type == GameType.Lenen)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E2E32"));
                }
                if (touhouGame.Type == GameType.Kaisendou)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#17301e"));
                }
                if (touhouGame.Type == GameType.MajorTouhouFanGames )
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3014"));
                }
                if (touhouGame.Type == GameType.OtherTouhouFanGames)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30141C"));
                }


            }

        }

        private void FastScroll(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;

            if (scrollViewer != null)
            {
                // Calculate the new vertical offset
                // You might need to adjust the factor '40' based on your desired scroll speed
                double offsetChange = (e.Delta > 0 ? -90 : 90);  // Positive delta means scroll up, negative means scroll down

                // Apply the new offset, ensuring we don't scroll beyond the scrollable content
                double newOffset = scrollViewer.VerticalOffset + offsetChange;
                scrollViewer.ScrollToVerticalOffset(newOffset);

                // Mark the event as handled to prevent the default scroll behavior
                e.Handled = true;
            }
        }


        private void GotoTab1(object sender, RoutedEventArgs e)
        {
            Tab1.IsSelected = true;
        }

        private void GotoDebugTab(object sender, RoutedEventArgs e)
        {
            Tab2.IsSelected = true;
        }
        private void DebugCallNotification(object sender, RoutedEventArgs e)
        {
            PixelWPF.LibraryMan.Notification("Test Title","Test Test");
        }

        private void OpenPatchnotes(object sender, RoutedEventArgs e)
        {
            PixelWPF.Patchnotes patchnotes = new();
            patchnotes.LoadPatchnotes(LibraryTouhou.TouhouLauncherPath + "\\Other\\Patchnotes.txt");
            patchnotes.Show();
        }
    }
}

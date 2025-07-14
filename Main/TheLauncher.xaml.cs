using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Globalization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;
using WesternLauncherOfEasternOrigins.Properties;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices; //For borderless gaming check
using System.Windows.Automation;
using Microsoft.Web.WebView2.Core;
using System.Security.Policy;
using System.Security.AccessControl; //for making folders not be read only in my documents.
using OfficeOpenXml;
using System.Data;
using System.Text.Json.Nodes;

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
        string GameInfoPath = ""; //Stores where did the user set the game location, mod list, etc. Its a variable incase i later allow users to change this folder location.
        string Game = "";
        TouhouGame TheGame {  get; set; }
        List<string> EnabledMods { get; set; } = new();

        public Button CurrentGame { get; set; }

        public GameLauncher()
        {
            InitializeComponent();
            DataContext = this;            

            ResolutionComboBox.Text = Properties.Settings.Default.Resolution;

            string basepath = AppDomain.CurrentDomain.BaseDirectory;
            

            #if DEBUG
            LibraryMan.TouhouLauncherPath = Path.GetFullPath(Path.Combine(basepath, @"..\..\..\..\..\Release"));            
            #else
            LibraryMan.TouhouLauncherPath = basepath;
            //DevMenu.Visibility = Visibility.Collapsed;
            //DevMenu.IsEnabled = false;
            #endif

            LibraryMan.ModsXMLLocation = LibraryMan.TouhouLauncherPath + "\\Other\\Touhou Mods.xml";
            LibraryMan.GamesXMLLocation = LibraryMan.TouhouLauncherPath + "\\Other\\Touhou Games.xml";
                        
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            GameInfoPath = Path.Combine(documentsPath, "Touhou Western Launcher of Eastern Origins", "Game Info");            
            Directory.CreateDirectory(GameInfoPath);// Ensure the directory exists




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

            

            Tab1.Visibility = Visibility.Collapsed;
            Tab2.Visibility = Visibility.Collapsed;

            if (Properties.Settings.Default.VolumeMixerAtLauncher == false) { VolumeMixerButton.Visibility = Visibility.Collapsed; }
            Notepad.Text = Properties.Settings.Default.NotepadText;

            TextThingy2.Text = "";

            { //show modslist mods
                HashSet<string> uniqueEntries = new HashSet<string>();
                // Collect entries from ModsList
                foreach (TouhouMod mod in LibraryMan.MasterModsList)
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

            { //show touhou crap repo folder mods
                string reposPath = Path.Combine(LibraryMan.TouhouLauncherPath, "Other","Extra Programs", "Touhou Crap", "repos");

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


            { //This compared every mod folder inside the touhou crap repos, with every mod path in ModsList, and displays the diffrences except nmlgc and thpatch folders (eng patch).
                // Collect entries from ModsList
                HashSet<string> modsListEntries = new HashSet<string>();
                foreach (var mod in LibraryMan.MasterModsList)
                {
                    modsListEntries.Add(mod.THCrapText); // These entries already include the trailing slash
                }

                // Collect entries from the Touhou Crap repo folders, excluding certain directories
                HashSet<string> repoEntries = new HashSet<string>();
                string reposPath = Path.Combine(LibraryMan.TouhouLauncherPath, "Other","Extra Programs", "Touhou Crap", "repos");
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


            this.Loaded += new RoutedEventHandler(AfterThisIsLoaded);
        }

        public void AfterThisIsLoaded(object sender, RoutedEventArgs e) 
        {
            if (Properties.Settings.Default.ScrollToLastGame == false) 
            {
                return;
            }

            
            foreach (Button TheButton in GamesPanel.Children)
            {
                TouhouGame TheGame = TheButton.Tag as TouhouGame;
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
                TouhouGame TheGame = theborder.Tag as TouhouGame;
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

        

        //New launceher
        private void GenerateLauncher()
        {


            foreach (TouhouGame GameData in LibraryMan.MasterGameList)
            {
                

                var Game = GameData;


                Button TheButton = new();
                TheButton.Tag = GameData;
                TheButton.Height = 80;
                DockPanel.SetDock(TheButton, Dock.Top);
                TheButton.Click += (sender, e) => GameLoad(Game, TheButton);


                
                // Define the GameLoad event handler method


                DockPanel Card = new();
                TheButton.Content = Card;

                Image Image = new();
                Card.Children.Add(Image);
                DockPanel.SetDock(Image, Dock.Left);
                Image.Width = 190;
                Image.Stretch = Stretch.Fill;
                if (Game.CardArt != null && Game.CardArt != "") 
                {
                    if (File.Exists(System.IO.Path.Combine(LibraryMan.TouhouLauncherPath, "Card Art", Game.CardArt)))
                    { 
                        Image.Source = new BitmapImage(new Uri(System.IO.Path.Combine(LibraryMan.TouhouLauncherPath, "Card Art", Game.CardArt), UriKind.Absolute)); 
                    }
                }

                DockPanel RightPanel = new();
                Card.Children.Add(RightPanel);
                DockPanel.SetDock(RightPanel, Dock.Left);
                RightPanel.VerticalAlignment = VerticalAlignment.Center;                

                Label LabelTitleDate = new();
                RightPanel.Children.Add(LabelTitleDate);
                DockPanel.SetDock(LabelTitleDate, Dock.Top);
                LabelTitleDate.FontSize = 12;
                LabelTitleDate.Content = Game.SeriesName + " - " + Game.Date;

                Label LabelName = new();
                RightPanel.Children.Add(LabelName);
                DockPanel.SetDock(LabelName, Dock.Top);
                LabelName.FontSize = 12;
                LabelName.Content = Game.SubtitleName;
                //

                GamesPanel.Children.Add(TheButton);

                //this should really not be here, and is (somehow???) causing a bug where the bottom game is highlighted, even though the correct one is loaded.
                if (Properties.Settings.Default.LastTouhouGame == GameData.CodeName) { GameLoad(GameData, TheButton); } 
                


            }

            RefreshGameCards();
        }

        

        

        private void GameLoad(TouhouGame Game, Button TheButton) //Trigger: When a Game card (button) is clicked.
        {
            TheButton.Background = Brushes.MidnightBlue;
            if (CurrentGame != null) 
            {
                if (TheButton != CurrentGame) 
                {
                    
                    CurrentGame.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1D1D1D"));
                }
                
            }
            CurrentGame = TheButton;

            TheGame = Game;
            LabelGameName2.Content = Game.SeriesName + " " + Game.SubtitleName;
            BasicsText.Text = Game.Description;
            
            string GameLastKnownLocation = System.IO.Path.Combine(GameInfoPath, TheGame.CodeName + ".txt");
            if (System.IO.File.Exists(GameLastKnownLocation))
            {
                GameLocationTextbox.Text = System.IO.File.ReadAllText(GameLastKnownLocation);
            }
            else { GameLocationTextbox.Text = ""; }

            ModsPanel.Children.Clear();
            EnabledMods.Clear();
            List<string> loadedMods = LoadModsFromXml(Game.CodeName);

            
            foreach (TouhouMod GameMod in Game.ModList) //creates a checkbox for each mod.
            {
                DockPanel DockPanel = new DockPanel();
                ModsPanel.Children.Add(DockPanel);
                DockPanel.SetDock(DockPanel, Dock.Top);
                DockPanel.Height = 26;

                CheckBox ModCheckBox = new();
                DockPanel.Children.Add(ModCheckBox);
                ModCheckBox.LayoutTransform = new ScaleTransform(1.5, 1.5);

                foreach (TouhouMod TMod in LibraryMan.MasterModsList) 
                { 
                    if (TMod.Key == GameMod.Key) 
                    {
                        ModCheckBox.Tag = TMod;

                        if (TMod.Recommend == true) 
                        {
                            Label LabelRecommend = new();
                            DockPanel.Children.Add(LabelRecommend);
                            LabelRecommend.Content = "👍";
                            LabelRecommend.FontSize = 14;
                            LabelRecommend.Margin = new Thickness(0, -5, 0, 0);
                            LabelRecommend.ToolTip = "This mod is recommended, as it generally just makes the game better.";
                            ToolTipService.SetInitialShowDelay(LabelRecommend, 0);
                        }



                        string THCrapFolderPath = System.IO.Path.Combine(LibraryMan.TouhouLauncherPath, "Other","Extra Programs", "Touhou Crap");
                        string THCrapFolderPath2 = LibraryMan.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\" + TMod.THCrapText;
                        if (!System.IO.Directory.Exists(THCrapFolderPath2)) //Checks to make sure the mod exists in touhou crap. If not, make the mod Orange. This is because the program must move the mod there. 
                        {
                            Label LabelWarn = new();
                            DockPanel.Children.Add(LabelWarn); //👍
                            LabelWarn.Content = "⚠";
                            LabelWarn.FontSize = 14;
                            LabelWarn.Margin = new Thickness(-3, -3, 0, 0);
                            LabelWarn.Foreground = Brushes.Orange;
                            LabelWarn.ToolTip = "Missing from Touhou Crap Repos";
                            ToolTipService.SetInitialShowDelay(LabelWarn, 0);                            
                        }

                        //string modsFolderPath = System.IO.Path.Combine(TouhouLauncherPath, "Extra Programs", "Mods");
                        //string modsFolderPath2 = TouhouLauncherPath + "\\Extra Programs\\Mods\\" + TMod.Value.THCrapText;
                        //string modifiedPath = modsFolderPath2.Replace("repos/", "");
                        ////"repos/Revenant/ShowRank/"
                        //if (!System.IO.Directory.Exists(modifiedPath)) //Checks to make sure the mod exists in the mods folder. If not, make the mod red. This means the mod can't get to end users when published, big problem!
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
                        DockPanel.Children.Add(Label); //👍
                        Label.Content = TMod.Key;
                        Label.FontSize = 14;
                        Label.Margin = new Thickness(-3, -3, 0, 0);

                        

                        DockPanel.MouseEnter += (s, e) =>
                        {
                            DockPanel.Background = new SolidColorBrush(Colors.DarkSlateGray);
                            TextBoxModName.Text = TMod.Key;
                            TextBoxModDescription.Text = TMod.Description;
                                                        
                            string DisplayPath = TMod.THCrapText.Replace("repos/", "");
                            TextBoxModPath.Text = DisplayPath;
                        };
                        DockPanel.MouseLeave += (s, e) =>
                        {
                            DockPanel.Background = Brushes.Transparent; // Or any default color you wish
                        };
                        DockPanel.MouseLeftButtonDown += (s, e) =>
                        {
                            ModCheckBox.IsChecked = !ModCheckBox.IsChecked;
                            // If you need to explicitly call the CheckBox's Click event handler
                            CheckboxClick(ModCheckBox, GameMod.Key); // Assuming CheckboxClick can accept these parameters
                        };    


                        ModCheckBox.IsChecked = loadedMods.Contains(TMod.Key.ToString());
                        if (ModCheckBox.IsChecked == true) { EnabledMods.Add(TMod.Key); }
                                                
                    }  
                }                
                ModCheckBox.Click += (sender, e) => CheckboxClick(sender, GameMod.Key);                               
                
            }


            

        }





        



        //OTHER SHIT



        private List<string> LoadModsFromXml(string gameCodeName)
        {
            var modsList = new List<string>();
            
            string filePath = System.IO.Path.Combine(GameInfoPath, gameCodeName + ".xml");

            if (File.Exists(filePath))
            {
                XDocument xmlDoc = XDocument.Load(filePath);
                var mods = xmlDoc.Descendants("Mod");
                foreach (var mod in mods)
                {
                    modsList.Add(mod.Value);
                }
            }

            return modsList;
        }

        private void CheckboxClick(object sender, string Mod) 
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsChecked == true) // If the checkbox is being checked and the mod is not already in the list, add it
                {                    
                    if (!EnabledMods.Contains(Mod))
                    {
                        EnabledMods.Add(Mod);
                    }
                }
                else // If the checkbox is being unchecked and the mod is in the list, remove it
                {                    
                    if (EnabledMods.Contains(Mod))
                    {
                        EnabledMods.Remove(Mod);
                    }
                }
            }


            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true; 
            using (XmlWriter writer = XmlWriter.Create(GameInfoPath + "\\" + TheGame.CodeName + ".xml", settings))
            {
                writer.WriteStartElement("Root");

                writer.WriteElementString("FilePath", "????");
                writer.WriteStartElement("ModsList");
                foreach (string ModName in EnabledMods)
                {                    
                    writer.WriteElementString("Mod", ModName.ToString()); //The mod Name.
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

                string filePath = System.IO.Path.Combine(GameInfoPath, TheGame.CodeName + ".txt");
                System.IO.Directory.CreateDirectory(GameInfoPath);
                System.IO.File.WriteAllText(filePath, GameLocationTextbox.Text);


            }
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
        

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        






        

        

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
            LibraryMan.PlayerName = comboBoxItem.Content as string;
            Properties.Settings.Default.Save();

            Achievements Achievements = new();
            Achievements.LoadPlayerAchievements(); //Loads the currently selected player's achievement data.
            Achievements.CreateQuestBoard(this); //Decides a list of recommended quests based on the current players skill level and achievements. 
            Achievements.CreateAchievementsBoard(this);  //Shows every achievement in a master list, and the current players progression for all of them.
        }

        private void RefreshAchievements(object sender, RoutedEventArgs e)
        {
            Achievements Achievements = new();
            Achievements.LoadPlayerAchievements(); //Loads the currently selected player's achievement data.
            Achievements.CreateQuestBoard(this); //Decides a list of recommended quests based on the current players skill level and achievements. 
            Achievements.CreateAchievementsBoard(this);  //Shows every achievement in a master list, and the current players progression for all of them.
            RefreshGameCards();
        }

        private void RefreshGameCards()
        {

            foreach (Button button in GamesPanel.Children.OfType<Button>())
            {
                button.Visibility = Visibility.Visible;

                TouhouGame touhouGame = button.Tag as TouhouGame;

                if (touhouGame.Type == GameType.PC98 && Properties.Settings.Default.ShowTouhouPC98 == false)
                {
                    button.Visibility = Visibility.Collapsed;
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

        private void BuiltLatest(object sender, RoutedEventArgs e)
        {
            string drive = "M:";
            string commandToExecute = @"cd ""M:\Touhou Launcher\New Touhou Launcher"" & dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true";

            // Start a new process for the command prompt
            Process cmdProcess = new Process();
            Task.Delay(200).Wait();
            cmdProcess.StartInfo.FileName = "cmd.exe";
            cmdProcess.StartInfo.RedirectStandardInput = true;
            cmdProcess.StartInfo.RedirectStandardOutput = true;
            cmdProcess.StartInfo.CreateNoWindow = true;
            cmdProcess.StartInfo.UseShellExecute = false;
            cmdProcess.Start();

            Task.Delay(200).Wait();
            // Execute the commands
            using (StreamWriter sw = cmdProcess.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine(drive);  // Change the drive
                    sw.WriteLine(commandToExecute);  // Change directory and run dotnet publish
                }
            }
            Task.Delay(200).Wait();
            // Wait for the command to complete
            //cmdProcess.WaitForExit();

            // Open the folder in File Explorer
            string folderPath = @"M:\Touhou Launcher\New Touhou Launcher\bin\Release\net7.0-windows\win-x64\publish";
            Process.Start("explorer.exe", folderPath);
        }
    }
}

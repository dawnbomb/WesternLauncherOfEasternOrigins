using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;

namespace WesternLauncherOfEasternOrigins
{
    public partial class GameLauncher
    {

        public void LaunchGame(object sender, RoutedEventArgs e)
        {   
            if (!File.Exists(GameLocationTextbox.Text))
            {
                MessageBox.Show("The game doesn't seem to exist at that location.\n\nLocation: " + GameLocationTextbox.Text, "Game Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //FirstGameLaunch
            if (Properties.Settings.Default.FirstGameLaunch == false) 
            {
                MessageBox.Show("I see your launching a game for the very first time. Here are some things to keep in mind." +
                    "\n\n- Use volume mixer before windows blows your ears out :(" +    
                    "\n- Touhou games sometimes update before launching." +
                    "\n- Old bullet hells sometimes hide their fullscreen setting behind a config file or exe." +
                    "\n- You can change various launcher settings on the top bar's settings menu." +
                    "\n" +
                    "\nPS: You can type in the achievement chart! A game is 'cleared' if the text '1CC' appears (but it remembers ANY text you put it!). Recommended quests are randomly selected for a players skill level, estimated from their achievements (1CCs). " + GameLocationTextbox.Text, "Message to first time players :)", MessageBoxButton.OK, MessageBoxImage.Warning);

                Properties.Settings.Default.FirstGameLaunch = true;
                Properties.Settings.Default.Save();
            }


            if (SettingController()) { return; }
            SettingAutoMinimize();

            if (TheGame.Type == GameType.None) { LaunchNone(); }
            if (TheGame.Type == GameType.PC98) { LaunchPC98(); }
            if (TheGame.Type == GameType.Touhou) { LaunchTouhou(); }

            Properties.Settings.Default.LastTouhouGame = TheGame.CodeName;
            Properties.Settings.Default.Save();
        }

        ////////LAUNCHES///////////

        public void LaunchNone() 
        {            
            string gamePath = GameLocationTextbox.Text;    

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = gamePath,
                UseShellExecute = false, //If true, similar to double-clicking the executable.               
                WorkingDirectory = System.IO.Path.GetDirectoryName(gamePath), //I personally confirmed some games (exes) require this, or they launch "strangely" or not at all.
                //Verb = "runas" // Runs game as administrator. Some games may need this.
            };            
            System.Diagnostics.Process.Start(startInfo);

            
        }

        public void LaunchPC98()
        {
            SettingJoy2Key();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "\"" + LibraryMan.TouhouLauncherPath + "\\Other\\Extra Programs\\Neko Project II\\np21x64w.exe" + "\"";
            Process.Start(startInfo);
        }

        public void LaunchTouhou()
        {
            
            SetupMods(TheGame);
            SetupResolution(); //for vpatch only    

            var gameDir = System.IO.Path.GetDirectoryName(LibraryMan.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\bin\\thcrap_loader.exe");
            var startInfo = new System.Diagnostics.ProcessStartInfo(LibraryMan.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\bin\\thcrap_loader.exe")
            {
                UseShellExecute = false, //If true, similar to double-clicking the executable. 
                WorkingDirectory = gameDir, //I personally confirmed some games (exes) require this, or they launch "strangely" or not at all.                    
                //Verb = "runas" // Runs game as administrator. Some games may need this.
                Arguments = "\"WesternLauncherOfEasternOrigins.js\" \"" + GameLocationTextbox.Text + "\"",//"th07",//TheGame.CodeName, //th06",
                CreateNoWindow = true,
            };
            System.Diagnostics.Process.Start(startInfo);


            Properties.Settings.Default.LastTouhouGame = TheGame.CodeName;
            Properties.Settings.Default.Save();

            


            SettingCloseTCRAP();
            SettingPractiseTool();
        }


       
        ////////SETTINGS///////////
      
        


        public void SettingAutoMinimize()
        {
            if (Properties.Settings.Default.AutoMinimize == true)
            {
                this.WindowState = WindowState.Minimized;
            }
        }


        public bool SettingController()
        {
            if (Properties.Settings.Default.ControllerCheck == false) { return false; }

            bool isControllerConnected = false;
            for (int i = 0; i < 4; i++)  // XInput supports up to 4 controllers
            {
                if (GamePad.IsControllerConnected(i))
                {
                    isControllerConnected = true;
                    break;
                }
            }

            if (isControllerConnected)
            {
                // At least one controller is connected
                return false;
            }
            else
            {
                MessageBox.Show("You forgot to plug in a controller! Most touhou games do NOT support hotplugging (controllers plugged in only after a game is launched). \n\nIf you inteded to play without a controller, you can toggle [requires a controller] from the settings menu at the top left.", "Controller Check", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
        }

        public void SettingJoy2Key()
        {
            if (Properties.Settings.Default.AutoJoy2Key == true)
            {
                ProcessStartInfo Joy2Key = new ProcessStartInfo();
                Joy2Key.FileName = "\"" + LibraryMan.TouhouLauncherPath + "\\Other\\Extra Programs\\JoyToKey\\JoyToKey.exe" + "\"";
                Process.Start(Joy2Key);
            }
        }

        public void SettingCloseTCRAP() 
        {
            var startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMinutes < 2)  // Continue looping for 2 minutes
            {

                var patcherWindows = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Touhou Community Reliant Automatic Patcher"));
                foreach (AutomationElement window in patcherWindows)
                {
                    var runGameButton = window.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Run the game"));

                    // Check if the 'Run the game' button exists and is enabled
                    if (runGameButton != null && (bool)runGameButton.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty))
                    {
                        // Close the window since the 'Run the game' button is enabled
                        IntPtr hWnd = new IntPtr(window.Current.NativeWindowHandle);
                        WindowHandler.CloseWindow("Touhou Community Reliant Automatic Patcher");  // Assuming CloseWindow method is adjusted to send WM_CLOSE

                        return;  // Exit the Task after handling the window
                    }
                }
                Thread.Sleep(10); // Sleep for 10 milliseconds
            }



            //Task.Run(() =>
            //{
            //    var startTime = DateTime.Now;
            //    while ((DateTime.Now - startTime).TotalMinutes < 2)  // Continue looping for 2 minutes
            //    {

            //        var patcherWindows = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Touhou Community Reliant Automatic Patcher"));
            //        foreach (AutomationElement window in patcherWindows)
            //        {
            //            var runGameButton = window.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Run the game"));

            //            // Check if the 'Run the game' button exists and is enabled
            //            if (runGameButton != null && (bool)runGameButton.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty))
            //            {
            //                // Close the window since the 'Run the game' button is enabled
            //                IntPtr hWnd = new IntPtr(window.Current.NativeWindowHandle);
            //                WindowHandler.CloseWindow("Touhou Community Reliant Automatic Patcher");  // Assuming CloseWindow method is adjusted to send WM_CLOSE

            //                return;  // Exit the Task after handling the window
            //            }
            //        }
            //        Thread.Sleep(10); // Sleep for 10 milliseconds
            //    }
            //});
        }

        public void SettingPractiseTool()
        {
            if (Properties.Settings.Default.AutoPractise == true)
            {
                Task.Delay(2000).Wait(); //really long incase of updates, lag, etc?

                //Becuase updates change the name of the exe, i search for every program with thprac prefix, and run the first one i find.
                string exeDirectory = Path.Combine(LibraryMan.TouhouLauncherPath, "Other\\Extra Programs");
                string[] files = Directory.GetFiles(exeDirectory, "thprac*.exe");

                if (files.Length > 0)
                {
                    string TouhouPracticeToolexe = files[0];
                    ProcessStartInfo startInfo2 = new ProcessStartInfo 
                    {
                        FileName = TouhouPracticeToolexe 
                    };
                    Process.Start(startInfo2);
                }


                if (Properties.Settings.Default.SkipPractisePopups = false) { return; }
                

                Task.Run(() =>
                {
                    var startTime = DateTime.Now;
                    while ((DateTime.Now - startTime).TotalMinutes < 2)  // Continue looping for 5 minutes
                    {
                        var applyWindows = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Apply thprac?"));
                        foreach (AutomationElement window in applyWindows)
                        {
                            var btnCondition = new PropertyCondition(AutomationElement.NameProperty, "Yes");
                            var button = window.FindFirst(TreeScope.Descendants, btnCondition);

                            if (button != null)
                            {
                                var invokePattern = button.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                invokePattern?.Invoke();
                                return;  // Exit the Task after pressing "Yes"
                            }
                        }
                        Thread.Sleep(10); // Sleep for 0.01 second
                    }
                });


                Task.Run(() =>
                {
                    var startTime = DateTime.Now;
                    while ((DateTime.Now - startTime).TotalMinutes < 2)  // Continue looping for 5 minutes
                    {
                        var completeWindows = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Complete"));
                        foreach (AutomationElement completeWindow in completeWindows)
                        {
                            var okButtonCondition = new PropertyCondition(AutomationElement.NameProperty, "OK");
                            var okButton = completeWindow.FindFirst(TreeScope.Descendants, okButtonCondition);

                            if (okButton != null)
                            {
                                var invokePatternOk = okButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                invokePatternOk?.Invoke();
                                return;  // Exit the Task after pressing "OK"
                            }
                        }
                        Thread.Sleep(10); // Sleep for 0.01 second
                    }
                });











            }
        }


        ////////Misc Functions for launcers///////////


        public void SetupMods(TouhouGame Game)
        {
            string json = @"{
                ""dat_dump"": false,
                ""patched_files_dump"": false,
                ""patches"": [
                    {
                        ""archive"": ""repos/nmlgc/base_tsa/""
                    },
                    {
                        ""archive"": ""repos/nmlgc/base_tasofro/""
                    }
                    , //starting here is end patch
                    {
                        ""archive"": ""repos/nmlgc/script_latin/""
                    },
                    {
                        ""archive"": ""repos/nmlgc/western_name_order/""
                    }
                    ,
                    {
                        ""archive"": ""repos/thpatch/lang_en/""
                    }                            
                ]
            }";
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(json);

            foreach (DockPanel dockPanel in ModsPanel.Children.OfType<DockPanel>())
            {
                // Find the CheckBox inside the current DockPanel
                CheckBox checkBox = dockPanel.Children.OfType<CheckBox>().FirstOrDefault();
                if (checkBox != null && checkBox.IsChecked == true)
                {
                    // Proceed if CheckBox is checked
                    TouhouMod Tmod = checkBox.Tag as TouhouMod;
                    if (Tmod != null)
                    {
                        JObject newPatch = new JObject
                        {
                            ["archive"] = Tmod.THCrapText
                        };
                        jsonObject.patches.Add(newPatch);
                    }
                }
            }
            //foreach (string ModName in EnabledMods)
            //{
            //    string TheModName = ModName.ToString();

            //    foreach (TouhouMod Modd in LibraryMan.MasterModsList)
            //    {
            //        string key = Modd.Key.ToString();
            //        if (key == TheModName)
            //        {
            //            JObject newPatch = new JObject
            //            {
            //                ["archive"] = Modd.THCrapText
            //            };
            //            jsonObject.patches.Add(newPatch);
            //        }
            //    }
            //}
            string updatedJson = JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);



            File.WriteAllText(LibraryMan.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\config\\WesternLauncherOfEasternOrigins.js", updatedJson);  // Write the modified JSON string to a file



        }



        

        public void SetupResolution()
        {
            if (Properties.Settings.Default.Resolution == "720p" || Properties.Settings.Default.Resolution == "960p" || Properties.Settings.Default.Resolution == "1080p")
            {
                string exeDirectory = System.IO.Path.GetDirectoryName(GameLocationTextbox.Text);
                string iniFilePath = System.IO.Path.Combine(exeDirectory, "vpatch.ini");

                if (File.Exists(iniFilePath)) // Check if vpatch.ini exists in the directory
                {
                    string[] lines = File.ReadAllLines(iniFilePath);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith("Width")) // Check if the line starts with "Width" or "Height" and modify them
                        {
                            if (Properties.Settings.Default.Resolution == "720p") { lines[i] = "Width = 960"; } //1280?  (x0.75?)
                            if (Properties.Settings.Default.Resolution == "960p") { lines[i] = "Width = 1280"; } //1600?  (x0.75?)
                            if (Properties.Settings.Default.Resolution == "1080p") { lines[i] = "Width = 1440"; } //1920?  (x0.75?)

                        }
                        else if (lines[i].StartsWith("Height"))
                        {
                            if (Properties.Settings.Default.Resolution == "720p") { lines[i] = "Height = 720"; }
                            if (Properties.Settings.Default.Resolution == "960p") { lines[i] = "Height = 960"; }
                            if (Properties.Settings.Default.Resolution == "1080p") { lines[i] = "Height = 1080"; }

                        }
                    }
                    File.WriteAllLines(iniFilePath, lines); // Write the modified lines back to the file
                }
            }
        }

        


    }
}

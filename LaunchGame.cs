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
using PixelWPF;
using NAudio.CoreAudioApi;

namespace WesternLauncherOfEasternOrigins
{
    public partial class GameLauncher
    {

        public void LaunchGame(object sender, RoutedEventArgs e)
        {   
            if (!File.Exists(GameLocationTextbox.Text))
            {
                string ErrorMessage = "The game exe was not found at the expected location.\n\nLocation: " + GameLocationTextbox.Text;
                PixelWPF.LibraryPixel.NotificationNegative("Game Not Found", ErrorMessage);
                
                return;
            }
            //FirstGameLaunch
            if (Properties.Settings.Default.FirstGameLaunch == false) 
            {
                PixelWPF.LibraryPixel.Notification("First time game launch tips!",
                "- Use volume mixer before windows blows your ears out :(" +
                "\n- Games sometimes update before launching." +
                "\n- Some old games hide the fullscreen toggle inside a config file or the exe." +
                "\n- Launcher settings exist. Look at them." +
                "\n" +
                "\nAchievement System:" +
                "\n- You can type in the achievement chart! " +
                "\n- A game is 'cleared' if the text \"1CC\" is in a achievement box (but it remembers ANY text you put it!)." +
                "\n- Right clicking toggles 1CC text." +
                "\n- The player gets a Skill Lv based on 1CCs." +
                "\n- Mousing over an achievement shows it's difficulty lv in the top right." +
                "\n- Quests appear above the achievement list." +
                "\n- Quests are semi-random, influenced by skill lv, recent game launches, and variety.");

                Properties.Settings.Default.FirstGameLaunch = true;
                Properties.Settings.Default.Save();
            }

            


            if (SettingController()) { return; }
            SettingAutoMinimize();

            Properties.Settings.Default.LastTouhouGame = TheGame.CodeName;
            Properties.Settings.Default.Save();

            
            if (TheGame.Type == GameType.PC98) { LaunchPC98(); return; }
            if (TheGame.Type == GameType.Touhou) { LaunchTouhou(); return; }

            LaunchNone();

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
            startInfo.FileName = "\"" + LibraryTouhou.TouhouLauncherPath + "\\Other\\Extra Programs\\Neko Project II\\np21x64w.exe" + "\"";
            Process.Start(startInfo);
        }



        public async Task LaunchTouhou()
        {
            
            SetupMods(TheGame);
            SetupResolution(); //for vpatch only    
            //VolumeMonitor.StartVolumeMonitoring();
            //SettingOpenVolumeMixerIfGameIsAt100Volume();
            //await Task.Delay(300);

            var gameDir = System.IO.Path.GetDirectoryName(LibraryTouhou.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\bin\\thcrap_loader.exe");
            var startInfo = new System.Diagnostics.ProcessStartInfo(LibraryTouhou.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\bin\\thcrap_loader.exe")
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

            


            //SettingCloseTCRAP();
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

        public async Task SettingOpenVolumeMixerIfGameIsAt100Volume()
        {
            if (!Properties.Settings.Default.OpenVolumeMixerAt100Volume)
                return;

            //VolumeMonitor vm = new VolumeMonitor();
            //await vm.MonitorNewAppsFor100PercentVolume();
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
                PixelWPF.LibraryPixel.NotificationNegative("Controller Not Connected!",
                    "Most touhou games do NOT support Hotplugging (Controllers plugged after a game is launched). " +
                    "\n" +
                    "\nIf you want to play without a controller, toggle the [requires a controller] setting from the settings menu." +
                    "\n" +
                    "\nThis error message is *not* incorrect. Check your battery / wireless settings / etc." +
                    "");
                
                return true;
            }
        }

        public void SettingJoy2Key()
        {
            if (Properties.Settings.Default.AutoJoy2Key == true)
            {
                ProcessStartInfo Joy2Key = new ProcessStartInfo();
                Joy2Key.FileName = "\"" + LibraryTouhou.TouhouLauncherPath + "\\Other\\Extra Programs\\JoyToKey\\JoyToKey.exe" + "\"";
                Process.Start(Joy2Key);
            }
        }

        public async Task SettingCloseTCRAP() 
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
                Task.Delay(6000).Wait(); //really long incase of updates, lag, etc?

                //Becuase updates change the name of the exe, i search for every program with thprac prefix, and run the first one i find.
                string exeDirectory = Path.Combine(LibraryTouhou.TouhouLauncherPath, "Other\\Extra Programs");
                string[] files = Directory.GetFiles(exeDirectory, "thprac*.exe");

                if (files.Length > 0)
                {
                    string TouhouPracticeToolexe = files[0];
                    ProcessStartInfo startInfo2 = new ProcessStartInfo 
                    {
                        FileName = TouhouPracticeToolexe,
                        Arguments = "--attach"
                    };
                    Process.Start(startInfo2);
                }

                //I'm keeping this incase i later need it. Like if attaching to a incompatable game causes a popup.
                //Also maybe i should just move this code into PixelWPF as a AcceptPopup command?

                //if (Properties.Settings.Default.SkipPractisePopups == false) { return; }
                

                //Task.Run(() =>
                //{
                //    var startTime = DateTime.Now;
                //    while ((DateTime.Now - startTime).TotalMinutes < 2)  // Continue looping for 5 minutes
                //    {
                //        var applyWindows = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Apply thprac?"));
                //        foreach (AutomationElement window in applyWindows)
                //        {
                //            var btnCondition = new PropertyCondition(AutomationElement.NameProperty, "Yes");
                //            var button = window.FindFirst(TreeScope.Descendants, btnCondition);

                //            if (button != null)
                //            {
                //                var invokePattern = button.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                //                invokePattern?.Invoke();
                //                return;  // Exit the Task after pressing "Yes"
                //            }
                //        }
                //        Thread.Sleep(10); // Sleep for 0.01 second
                //    }
                //});


                //Task.Run(() =>
                //{
                //    var startTime = DateTime.Now;
                //    while ((DateTime.Now - startTime).TotalMinutes < 2)  // Continue looping for 5 minutes
                //    {
                //        var completeWindows = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Complete"));
                //        foreach (AutomationElement completeWindow in completeWindows)
                //        {
                //            var okButtonCondition = new PropertyCondition(AutomationElement.NameProperty, "OK");
                //            var okButton = completeWindow.FindFirst(TreeScope.Descendants, okButtonCondition);

                //            if (okButton != null)
                //            {
                //                var invokePatternOk = okButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                //                invokePatternOk?.Invoke();
                //                return;  // Exit the Task after pressing "OK"
                //            }
                //        }
                //        Thread.Sleep(10); // Sleep for 0.01 second
                //    }
                //});











            }
        }


        ////////Misc Functions for launcers///////////


        public void SetupMods(GameData Game)
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



            File.WriteAllText(LibraryTouhou.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\config\\WesternLauncherOfEasternOrigins.js", updatedJson);  // Write the modified JSON string to a file



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

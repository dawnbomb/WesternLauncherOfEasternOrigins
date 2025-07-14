using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.WebRequestMethods;

namespace WesternLauncherOfEasternOrigins
{
    public partial class GameLauncher
    {


        ////////////////////////////////////////////////
        //////////////////  LOCAL FOLDERS  /////////////
        ////////////////////////////////////////////////

        private void OpenTHCrapFolder(object sender, RoutedEventArgs e)
        {
            string THCrapFolderPath = System.IO.Path.Combine(LibraryMan.TouhouLauncherPath, "Other", "Extra Programs", "Touhou Crap");

            // Ensure the directory exists before trying to open it
            if (System.IO.Directory.Exists(THCrapFolderPath))
            {
                // Use Process.Start to open the folder in File Explorer
                System.Diagnostics.Process.Start("explorer.exe", THCrapFolderPath);
            }
        }

        private void OpenModsFolder(object sender, RoutedEventArgs e)
        {
            string modsFolderPath = System.IO.Path.Combine(LibraryMan.TouhouLauncherPath, "Other","Extra Programs", "Mods");

            // Ensure the directory exists before trying to open it
            if (System.IO.Directory.Exists(modsFolderPath))
            {
                // Use Process.Start to open the folder in File Explorer
                System.Diagnostics.Process.Start("explorer.exe", modsFolderPath);
            }
        }

        private void OpenGameInfoFolder(object sender, RoutedEventArgs e)
        {
            // Ensure the directory exists before trying to open it
            if (System.IO.Directory.Exists(GameInfoPath))
            {
                // Use Process.Start to open the folder in File Explorer
                System.Diagnostics.Process.Start("explorer.exe", GameInfoPath);
            }
        }

        ////////////////////////////////////////////////
        ////////////////////  LINKS  ///////////////////
        ////////////////////////////////////////////////

        public void OpenLink(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LinkTouhouWiki(object sender, RoutedEventArgs e)
        {
            OpenLink("https://en.touhouwiki.net/wiki/Touhou_Wiki");
        }

        private void LinkTouhouWikiTools(object sender, RoutedEventArgs e)
        {
            OpenLink("https://en.touhouwiki.net/wiki/Game_Tools_and_Modifications");
        }

        private void LinkTouhouWikiMods(object sender, RoutedEventArgs e)
        {
            OpenLink("https://www.thpatch.net/wiki/Touhou_Patch_Center:Servers");
        }

        private void LinkTouhouDifficultyChart(object sender, RoutedEventArgs e)
        {
            OpenLink("https://zps-stg.github.io/dc");
        }

        private void LinkSTGDifficultyChart(object sender, RoutedEventArgs e)
        {
            OpenLink("https://shmups.system11.org/viewtopic.php?t=56114");
        }

        private void LinkTouhouUltraPatches(object sender, RoutedEventArgs e)
        {
            OpenLink("https://touhouprojectlovers.blogspot.com/p/touhou-project-ultra-patch.html");
        }

        private void LinkMyWebsite(object sender, RoutedEventArgs e)
        {
            OpenLink("https://www.crystalmods.com/");
        }

        ////////////////////////////////////////////////
        ////////////////////  TOOLS  ///////////////////
        ////////////////////////////////////////////////

        private void RunTHCrap(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "\"" + LibraryMan.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\thcrap.exe" + "\"";
            Process.Start(startInfo);
        }

        private void Joy2Key(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "\"" + LibraryMan.TouhouLauncherPath + "\\Other\\Extra Programs\\JoyToKey\\JoyToKey.exe" + "\"";
            Process.Start(startInfo);
        }

        private void PractiseModePlus(object sender, RoutedEventArgs e)
        {

            //I used to search for the exact exe name. However, updates change the name of the exe. Instead, i search for every program with thcrap prefix, and run the first one i find, avoiding version number problems.
            string exeDirectory = Path.Combine(LibraryMan.TouhouLauncherPath, "Other", "Extra Programs");
            string[] files = Directory.GetFiles(exeDirectory, "thprac*.exe");

            if (files.Length > 0)
            {
                string TouhouPracticeToolexe = files[0];
                ProcessStartInfo startInfo2 = new ProcessStartInfo { FileName = TouhouPracticeToolexe };
                Process.Start(startInfo2);
            }
        }

        private void BorderlessGaming(object sender, RoutedEventArgs e)
        {
            Process[] runningProcesses = Process.GetProcessesByName("BorderlessGaming");
            if (runningProcesses.Length > 0)
            {
                // Attempt to bring the first instance to the foreground
                IntPtr hWnd = runningProcesses[0].MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                {
                    SetForegroundWindow(hWnd);

                }
                else
                {
                    // hWnd is IntPtr.Zero, indicating the window is not visible or minimized to the tray
                    MessageBox.Show("BorderlessGaming is already running, but is minimized to the tray. It's actually rather complicated to make another program generate it's window :( so you will have to right click it and select show to make it appear.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                return; // Exit the method as the application is already running
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(LibraryMan.TouhouLauncherPath, "Other", "Extra Programs", "Borderless Gaming", "BorderlessGaming.exe"),
                UseShellExecute = true, // Necessary for the 'runas' verb
                Verb = "runas" // This verb indicates to run the process as an administrator
            };

            try
            {
                Process.Start(startInfo);
            }
            catch
            {

            }
        }







        ////////////////////////////////////////////////
        //////////////////  SETTINGS  //////////////////
        ////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool AutoMinimize
        {
            get { return Properties.Settings.Default.AutoMinimize; }
            set
            {
                if (Properties.Settings.Default.AutoMinimize != value)
                {
                    Properties.Settings.Default.AutoMinimize = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoJoy2Key
        {
            get { return Properties.Settings.Default.AutoJoy2Key; }
            set
            {
                if (Properties.Settings.Default.AutoJoy2Key != value)
                {
                    Properties.Settings.Default.AutoJoy2Key = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoPractise
        {
            get { return Properties.Settings.Default.AutoPractise; }
            set
            {
                if (Properties.Settings.Default.AutoPractise != value)
                {
                    Properties.Settings.Default.AutoPractise = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool SkipPractisePopups
        {
            get { return Properties.Settings.Default.SkipPractisePopups; }
            set
            {
                if (Properties.Settings.Default.SkipPractisePopups != value)
                {
                    Properties.Settings.Default.SkipPractisePopups = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool VolumeMixerAtLauncher
        {
            get { return Properties.Settings.Default.VolumeMixerAtLauncher; }
            set
            {
                if (Properties.Settings.Default.VolumeMixerAtLauncher != value)
                {
                    Properties.Settings.Default.VolumeMixerAtLauncher = value;
                    Properties.Settings.Default.Save();
                    if (Properties.Settings.Default.VolumeMixerAtLauncher == true) { VolumeMixerButton.Visibility = Visibility.Visible; }
                    if (Properties.Settings.Default.VolumeMixerAtLauncher == false) { VolumeMixerButton.Visibility = Visibility.Collapsed; }
                    OnPropertyChanged();
                }
            }
        }

        public bool ControllerCheck
        {
            get { return Properties.Settings.Default.ControllerCheck; }
            set
            {
                if (Properties.Settings.Default.ControllerCheck != value)
                {
                    Properties.Settings.Default.ControllerCheck = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowChallengePanel
        {
            get { return Properties.Settings.Default.ShowChallengePanel; }
            set
            {
                if (Properties.Settings.Default.ShowChallengePanel != value)
                {
                    Properties.Settings.Default.ShowChallengePanel = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        


        public bool ShowTouhouSpecialAchievements
        {
            get { return Properties.Settings.Default.ShowTouhouSpecialAchievements; }
            set
            {
                if (Properties.Settings.Default.ShowTouhouSpecialAchievements != value)
                {
                    Properties.Settings.Default.ShowTouhouSpecialAchievements = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }




        public bool ScrollToLastGame
        {
            get { return Properties.Settings.Default.ScrollToLastGame; }
            set
            {
                if (Properties.Settings.Default.ScrollToLastGame != value)
                {
                    Properties.Settings.Default.ScrollToLastGame = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowTouhouPC98
        {
            get { return Properties.Settings.Default.ShowTouhouPC98; }
            set
            {
                if (Properties.Settings.Default.ShowTouhouPC98 != value)
                {
                    Properties.Settings.Default.ShowTouhouPC98 = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowEasyMode
        {
            get { return Properties.Settings.Default.ShowEasyMode; }
            set
            {
                if (Properties.Settings.Default.ShowEasyMode != value)
                {
                    Properties.Settings.Default.ShowEasyMode = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowNormalMode
        {
            get { return Properties.Settings.Default.ShowNormalMode; }
            set
            {
                if (Properties.Settings.Default.ShowNormalMode != value)
                {
                    Properties.Settings.Default.ShowNormalMode = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }
        

        

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Windows.UI.Composition;
using static System.Net.WebRequestMethods;
using System.Windows.Media; // for swapping font family.

namespace WesternLauncherOfEasternOrigins
{
    public partial class GameLauncher
    {


        ////////////////////////////////////////////////
        //////////////////  LOCAL FOLDERS  /////////////
        ////////////////////////////////////////////////

        private void OpenMyFolder(object sender, RoutedEventArgs e)
        {
            string TheFolderToOpen = LibraryTouhou.TouhouLauncherPath;

            // Ensure the directory exists before trying to open it
            if (System.IO.Directory.Exists(TheFolderToOpen))
            {
                // Use Process.Start to open the folder in File Explorer
                System.Diagnostics.Process.Start("explorer.exe", TheFolderToOpen);
            }
        }

        private void OpenTHCrapFolder(object sender, RoutedEventArgs e)
        {
            string THCrapFolderPath = System.IO.Path.Combine(LibraryTouhou.TouhouLauncherPath, "Other", "Extra Programs", "Touhou Crap");

            // Ensure the directory exists before trying to open it
            if (System.IO.Directory.Exists(THCrapFolderPath))
            {
                // Use Process.Start to open the folder in File Explorer
                System.Diagnostics.Process.Start("explorer.exe", THCrapFolderPath);
            }
        }

        private void OpenModsFolder(object sender, RoutedEventArgs e)
        {
            string modsFolderPath = System.IO.Path.Combine(LibraryTouhou.TouhouLauncherPath, "Other","Extra Programs", "Touhou Crap", "repos");

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
            startInfo.FileName = "\"" + LibraryTouhou.TouhouLauncherPath + "\\Other\\Extra Programs\\Touhou Crap\\thcrap.exe" + "\"";
            Process.Start(startInfo);
        }

        private void Joy2Key(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "\"" + LibraryTouhou.TouhouLauncherPath + "\\Other\\Extra Programs\\JoyToKey\\JoyToKey.exe" + "\"";
            Process.Start(startInfo);
        }

        private void PractiseModePlus(object sender, RoutedEventArgs e)
        {

            //I used to search for the exact exe name. However, updates change the name of the exe. Instead, i search for every program with thcrap prefix, and run the first one i find, avoiding version number problems.
            string exeDirectory = Path.Combine(LibraryTouhou.TouhouLauncherPath, "Other", "Extra Programs");
            string[] files = Directory.GetFiles(exeDirectory, "thprac*.exe");

            if (files.Length > 0)
            {
                string TouhouPracticeToolexe = files[0];
                ProcessStartInfo startInfo2 = new ProcessStartInfo { FileName = TouhouPracticeToolexe };
                Process.Start(startInfo2);
            }
        }

        

        async private void OpenBorderless(object sender, RoutedEventArgs e)
        {

            Process[] therunningProcesses = Process.GetProcessesByName("NoMoreBorder");
            if (therunningProcesses.Length == 0) 
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(LibraryTouhou.TouhouLauncherPath, "Other", "Extra Programs", "NoMoreBorder", "NoMoreBorder.exe"),
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

            OpenIt();

            void OpenIt() 
            {
                Process[] runningProcesses = Process.GetProcessesByName("NoMoreBorder");
                foreach (var proc in runningProcesses)
                {
                    EnumWindows((hWnd, lParam) =>
                    {
                        GetWindowThreadProcessId(hWnd, out uint pid);
                        if (pid == proc.Id)
                        {
                            StringBuilder sb = new(256);
                            GetWindowText(hWnd, sb, sb.Capacity);
                            string title = sb.ToString();

                            if (title == "NoMoreBorder")
                            {
                                Console.WriteLine($"Found NoMoreBorder window (Handle: {hWnd})");

                                // Try a variety of things:
                                ShowWindow(hWnd, SW_SHOW);
                                ShowWindow(hWnd, SW_RESTORE);
                                SendMessage(hWnd, WM_SYSCOMMAND, (IntPtr)SC_RESTORE, IntPtr.Zero);
                                SetForegroundWindow(hWnd);
                                return false; // done
                            }
                        }
                        return true;
                    }, IntPtr.Zero);
                }

                string GetWindowTitle(IntPtr hWnd)
                {
                    StringBuilder sb = new StringBuilder(256);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    return sb.ToString();
                }
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

        
        public bool ShowQuestBoard
        {
            get { return Properties.Settings.Default.ShowQuestBoard; }
            set
            {
                if (Properties.Settings.Default.ShowQuestBoard != value)
                {
                    Properties.Settings.Default.ShowQuestBoard = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool OpenVolumeMixerAt100Volume
        {
            get { return Properties.Settings.Default.OpenVolumeMixerAt100Volume; }
            set
            {
                if (Properties.Settings.Default.OpenVolumeMixerAt100Volume != value)
                {
                    Properties.Settings.Default.OpenVolumeMixerAt100Volume = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ColorAchievements
        {
            get { return Properties.Settings.Default.ColorAchievements; }
            set
            {
                if (Properties.Settings.Default.ColorAchievements != value)
                {
                    Properties.Settings.Default.ColorAchievements = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowTouhou075
        {
            get { return Properties.Settings.Default.ShowTouhou075; }
            set
            {
                if (Properties.Settings.Default.ShowTouhou075 != value)
                {
                    Properties.Settings.Default.ShowTouhou075 = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowFanBulletHell
        {
            get { return Properties.Settings.Default.ShowFanBulletHell; }
            set
            {
                if (Properties.Settings.Default.ShowFanBulletHell != value)
                {
                    Properties.Settings.Default.ShowFanBulletHell = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowLenenGames
        {
            get { return Properties.Settings.Default.ShowLenenGames; }
            set
            {
                if (Properties.Settings.Default.ShowLenenGames != value)
                {
                    Properties.Settings.Default.ShowLenenGames = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowKaisendouGames
        {
            get { return Properties.Settings.Default.ShowKaisendouGames; }
            set
            {
                if (Properties.Settings.Default.ShowKaisendouGames != value)
                {
                    Properties.Settings.Default.ShowKaisendouGames = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ColorGameCards
        {
            get { return Properties.Settings.Default.ColorGameCards; }
            set
            {
                if (Properties.Settings.Default.ColorGameCards != value)
                {
                    Properties.Settings.Default.ColorGameCards = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowNotepad
        {
            get { return Properties.Settings.Default.ShowNotepad; }
            set
            {
                if (Properties.Settings.Default.ShowNotepad != value)
                {
                    Properties.Settings.Default.ShowNotepad = value;
                    Properties.Settings.Default.Save();
                    if (Properties.Settings.Default.ShowNotepad == true) { NotepadBorder.Visibility = Visibility.Visible; }
                    if (Properties.Settings.Default.ShowNotepad == false) { NotepadBorder.Visibility = Visibility.Collapsed; }
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowOtherFanTouhouBulletHell
        {
            get { return Properties.Settings.Default.ShowOtherFanTouhouBulletHell; }
            set
            {
                if (Properties.Settings.Default.ShowOtherFanTouhouBulletHell != value)
                {
                    Properties.Settings.Default.ShowOtherFanTouhouBulletHell = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowEx2
        {
            get { return Properties.Settings.Default.ShowEx2; }
            set
            {
                if (Properties.Settings.Default.ShowEx2 != value)
                {
                    Properties.Settings.Default.ShowEx2 = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }
        

        public bool ShowEx3
        {
            get { return Properties.Settings.Default.ShowEx3; }
            set
            {
                if (Properties.Settings.Default.ShowEx3 != value)
                {
                    Properties.Settings.Default.ShowEx3 = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowNewPlayerButton
        {
            get { return Properties.Settings.Default.ShowNewPlayerButton; }
            set
            {
                if (Properties.Settings.Default.ShowNewPlayerButton != value)
                {
                    Properties.Settings.Default.ShowNewPlayerButton = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();

                    if (Properties.Settings.Default.ShowNewPlayerButton == true) { NewPlayerButton.Visibility = Visibility.Visible; }
                    if (Properties.Settings.Default.ShowNewPlayerButton == false) { NewPlayerButton.Visibility = Visibility.Collapsed; }
                }
            }
        }

        public bool ShowPlayerLv
        {
            get { return Properties.Settings.Default.ShowPlayerLv; }
            set
            {
                if (Properties.Settings.Default.ShowPlayerLv != value)
                {
                    Properties.Settings.Default.ShowPlayerLv = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();

                    if (Properties.Settings.Default.ShowPlayerLv == true) { PlayerLvPanel.Visibility = Visibility.Visible; }
                    if (Properties.Settings.Default.ShowPlayerLv == false) { PlayerLvPanel.Visibility = Visibility.Collapsed; }
                }
            }
        }

        public bool UseArkPixelFont
        {
            get { return PixelWPF.Properties.Settings.Default.AppFontArkPixel; }
            set
            {
                if (PixelWPF.Properties.Settings.Default.AppFontArkPixel != value)
                {
                    PixelWPF.Properties.Settings.Default.AppFontArkPixel = value;
                    PixelWPF.Properties.Settings.Default.Save();
                    OnPropertyChanged();

                    if (PixelWPF.Properties.Settings.Default.AppFontArkPixel == true) { Application.Current.Resources["AppFont"] = new FontFamily("Dawns 10px ArkPixel");  }
                    if (PixelWPF.Properties.Settings.Default.AppFontArkPixel == false) { Application.Current.Resources["AppFont"] = new FontFamily("Segoe UI"); } //Verdana

                }
            }
        }

        public bool ShowHiddenGames
        {
            get { return Properties.Settings.Default.ShowHiddenGames; }
            set
            {
                if (Properties.Settings.Default.ShowHiddenGames != value)
                {
                    Properties.Settings.Default.ShowHiddenGames = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                    

                }
            }
        }

        

    }
}

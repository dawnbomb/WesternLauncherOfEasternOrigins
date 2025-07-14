using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;

//using System.Windows.Forms;

//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WesternLauncherOfEasternOrigins.Editors;


namespace WesternLauncherOfEasternOrigins
{
    /// <summary>
    /// Interaction logic for GameEditor.xaml
    /// </summary>
    public partial class GameEditor : UserControl
    {
        public GameEditor()
        {
            InitializeComponent();

            foreach (GameType type in Enum.GetValues(typeof(GameType))) //Load all possible game types into combo box.
            {
                GameTypeCombobox.Items.Add(new ComboBoxItem { Content = type.ToString() });
            }

            foreach (TouhouGame TouhouGame in LibraryMan.MasterGameList) //load games into treeview.
            {
                CreateTreeItem(TouhouGame);
            }
        }

        private void NewGameButton(object sender, RoutedEventArgs e)
        {
            TouhouGame TouhouGame = new();
            LibraryMan.MasterGameList.Add(TouhouGame);
            CreateTreeItem(TouhouGame);
        }

        private void CreateTreeItem(TouhouGame TouhouGame) 
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Tag = TouhouGame;
            NameBuilder(treeViewItem);
            TheTreeView.Items.Add(treeViewItem);

            ContextMenu contextMenu = new ContextMenu();
            treeViewItem.ContextMenu = contextMenu;

            MenuItem DeleteMe = new MenuItem();
            contextMenu.Items.Add(DeleteMe);
            DeleteMe.Click += new RoutedEventHandler(DeleteThisGame);
            void DeleteThisGame(object sender, RoutedEventArgs e)
            {
                TheTreeView.Items.Remove(DeleteMe);
            }
        }

        

        private void SaveAll(object sender, RoutedEventArgs e)
        {
            SaveGames saveGames = new SaveGames();

            var parentWindow = Window.GetWindow(this); //load achievements
            if (parentWindow is Editor EditorWindow)
            {                
                saveGames.SaveEverything(this, EditorWindow.TheModEditor);
            }

            
        }

        TreeViewItem TreeItem { get; set; }
        TouhouGame touhouGame { get; set; }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            TreeItem = TheTreeView.SelectedItem as TreeViewItem;
            touhouGame = TreeItem.Tag as TouhouGame;

            SeriesNameBox.Text = touhouGame.SeriesName;
            SubtitleNameBox.Text = touhouGame.SubtitleName;            
            DateBox.Text = touhouGame.Date;
            DescriptionBox.Text = touhouGame.Description;
            CodeNameBox.Text = touhouGame.CodeName;
            ArtBox.Text = touhouGame.CardArt;

            BackColorButton.Content = touhouGame.ColorBack;            
            BorderColorButton.Content = touhouGame.ColorBorder;            
            TextColorButton.Content = touhouGame.ColorText;

            ExampleLabel.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(touhouGame.ColorBack);
            ExampleBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(touhouGame.ColorBorder);
            ExampleLabel.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(touhouGame.ColorText);

            if (touhouGame.Practise == true)
            {
                PractiseCheckbox.IsChecked = true;
            }
            else 
            {
                PractiseCheckbox.IsChecked = false;
            }

            string FindGameType = touhouGame.Type.ToString();  //load game types
            foreach (ComboBoxItem item in GameTypeCombobox.Items)
            {
                if (item.Content.ToString() == FindGameType)
                {
                    GameTypeCombobox.SelectedItem = item;
                    break;
                }
            }

            var parentWindow = Window.GetWindow(this); //load achievements
            if (parentWindow is Editor EditorWindow)
            {
                EditorWindow.TheAchievementEditor.LoadAchievements();
            }

            ModsPanel.Children.Clear();
            foreach (TouhouMod Mod in touhouGame.ModList) 
            {
                LoadMod(Mod);
            }
            ModsNameLabel.Content = touhouGame.SubtitleName;
        }


        private void SeriesNameBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TreeItem == null || touhouGame == null)
            {
                return;
            }
            touhouGame.SeriesName = SeriesNameBox.Text;
            NameBuilder(TreeItem);
        }

        private void SubtitleBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TreeItem == null || touhouGame == null)
            {
                return;
            }
            touhouGame.SeriesName = SeriesNameBox.Text;
            NameBuilder(TreeItem);
        }
        
        

        void NameBuilder(TreeViewItem Item) 
        {
            //TreeItem = TheTreeView.SelectedItem as TreeViewItem;
            TouhouGame TheGame = Item.Tag as TouhouGame;

            Item.Header = TheGame.SeriesName  + "     " + TheGame.SubtitleName ;
            Item.Foreground = Brushes.White;
        }

        private void DateBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TreeItem == null || touhouGame == null)
            {
                return;
            }
            touhouGame.Date = DateBox.Text;
        }

        private void CodeNameBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TreeItem == null || touhouGame == null)
            {
                return;
            }
            touhouGame.CodeName = CodeNameBox.Text;
        }

        private void DescriptionBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TreeItem == null || touhouGame == null)
            {
                return;
            }
            touhouGame.Description = DescriptionBox.Text;   
        }

        
        private void PractiseModeChecked(object sender, RoutedEventArgs e)
        {
            if (TreeItem == null || touhouGame == null)
            {
                return;
            }
            touhouGame.Practise = true;
        }

        private void PractiseModeUnchecked(object sender, RoutedEventArgs e)
        {
            if (TreeItem == null || touhouGame == null)
            {
                return;
            }
            touhouGame.Practise = false;
        }

        private void GameTypeDropdownClosed(object sender, EventArgs e)
        {
            string SelectedType = GameTypeCombobox.Text;
            GameType NewGameType = (GameType)Enum.Parse(typeof(GameType), SelectedType);

            touhouGame.Type = NewGameType;
        }

        private void ArtBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TreeItem == null || touhouGame == null)
            {
                return;
            }
            touhouGame.CardArt = ArtBox.Text;
        }
        

        public void LoadMod(TouhouMod TouhouMod) 
        {
            DockPanel dockPanel = new DockPanel();
            DockPanel.SetDock(dockPanel, Dock.Top);
            dockPanel.Margin = new Thickness(0,5,0,0);
            ModsPanel.Children.Add(dockPanel);

            

            Button DeleteButton = new();
            DeleteButton.Content = "Delete Mod";
            DeleteButton.Margin = new Thickness(6, 2, 0, 2);
            dockPanel.Children.Add(DeleteButton);
            DeleteButton.Click += new RoutedEventHandler(RemoveMod);
            void RemoveMod(object sender, RoutedEventArgs e)
            {
                touhouGame.ModList.Remove(TouhouMod);
                ModsPanel.Children.Remove(dockPanel);
            }

            Button UPbutton = new();
            UPbutton.Content = "UP";
            UPbutton.Margin = new Thickness(6, 2, 0, 2);
            dockPanel.Children.Add(UPbutton);
            UPbutton.Click += new RoutedEventHandler(MoveUp);
            void MoveUp(object sender, RoutedEventArgs e)
            {
                int ModIndex = ModsPanel.Children.IndexOf(dockPanel);
                int ModListIndex = touhouGame.ModList.IndexOf(TouhouMod);
                if (ModIndex == 0 || ModListIndex == 0) { return; }

                touhouGame.ModList.Remove(TouhouMod);                
                ModsPanel.Children.Remove(dockPanel);

                touhouGame.ModList.Insert(ModIndex - 1, TouhouMod);
                ModsPanel.Children.Insert(ModIndex - 1, dockPanel);
            }

            Button DOWNbutton = new();
            DOWNbutton.Content = "DOWN";
            DOWNbutton.Margin = new Thickness(6, 2, 0, 2);
            dockPanel.Children.Add(DOWNbutton);
            DOWNbutton.Click += new RoutedEventHandler(MoveDown);
            void MoveDown(object sender, RoutedEventArgs e)
            {
                int ModCount = ModsPanel.Children.Count;
                int ModListCount = touhouGame.ModList.Count;

                int ModIndex = ModsPanel.Children.IndexOf(dockPanel);
                int ModListIndex = touhouGame.ModList.IndexOf(TouhouMod);
                if (ModIndex == ModCount - 1 || ModListIndex == ModListCount - 1) { return; }

                touhouGame.ModList.Remove(TouhouMod);
                ModsPanel.Children.Remove(dockPanel);

                touhouGame.ModList.Insert(ModIndex + 1, TouhouMod);
                ModsPanel.Children.Insert(ModIndex + 1, dockPanel);
            }

            Label ModLabel = new();
            ModLabel.Content = TouhouMod.Name + " " + TouhouMod.Key;
            dockPanel.Children.Add(ModLabel);



        }

        private void DeleteGame(object sender, RoutedEventArgs e)
        {
            LibraryMan.MasterGameList.Remove(touhouGame);
            TheTreeView.Items.Remove(TreeItem);
            
        }

        private void SetColors() 
        {
            BackColorButton.Content = touhouGame.ColorBack;
            BorderColorButton.Content = touhouGame.ColorBorder;
            TextColorButton.Content = touhouGame.ColorText;
            ExampleLabel.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(touhouGame.ColorBack);
            ExampleBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(touhouGame.ColorBorder);
            ExampleLabel.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(touhouGame.ColorText);
        }

        private void SetColorOfBorder(object sender, RoutedEventArgs e)
        {
            touhouGame.ColorBorder = OpenColorPicker(touhouGame.ColorBorder);
            SetColors();
        }

        private void SetColorOfBack(object sender, RoutedEventArgs e)
        {
            touhouGame.ColorBack = OpenColorPicker(touhouGame.ColorBack);
            SetColors();
        }

        private void SetColorOfText(object sender, RoutedEventArgs e)
        {
            touhouGame.ColorText = OpenColorPicker(touhouGame.ColorText);
            SetColors();
        }

        private string OpenColorPicker(string oldColorCode)
        {
            // Convert oldColorCode to System.Drawing.Color
            var oldColor = System.Drawing.ColorTranslator.FromHtml(oldColorCode);

            // Create and configure the color dialog
            using (var colorDialog = new System.Windows.Forms.ColorDialog())
            {
                colorDialog.FullOpen = true;  // Open the dialog with the custom colors section open
                colorDialog.Color = oldColor; // Set the default color to the old color

                // Show the color dialog
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // If user selects a color, convert it back to a hex string
                    System.Drawing.Color newColor = colorDialog.Color;
                    string newColorCode = "#" + newColor.R.ToString("X2") + newColor.G.ToString("X2") + newColor.B.ToString("X2");
                    return newColorCode;
                }
            }

            // If no color was selected, return the old color code
            return oldColorCode;
        }
    }
}

using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Border = System.Windows.Controls.Border;

namespace WesternLauncherOfEasternOrigins
{
    /// <summary>
    /// Interaction logic for AchievementEditor.xaml
    /// </summary>
    public partial class AchievementEditor : UserControl
    {
        public AchievementEditor()
        {
            InitializeComponent();

            
        }

        public void LoadAchievements() 
        {
            AchievementListDockPanel.Children.Clear();
            
            
            if (Window.GetWindow(this) is Editor EditorWindow)
            {
                TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                GameData touhouGame = treeViewItem.Tag as GameData;

                CreateAchievementChart(touhouGame);

                foreach (Achievement achievement in touhouGame.AchievementList)
                {
                    if (achievement.Type == Achievement.AchievementTypes.Basic) 
                    {
                        CreateAchievementPanel(achievement);
                    }
                    
                }
            }

            
        }
        

        private void NewBasicAchievementButton(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Editor EditorWindow)
            {
                TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                GameData touhouGame = treeViewItem.Tag as GameData;

                Achievement achievement = new();
                achievement.Key = LibraryTouhou.GenerateKey();
                achievement.Type = Achievement.AchievementTypes.Basic;
                touhouGame.AchievementList.Add(achievement);

                CreateAchievementPanel(achievement);


            }
        }
               

        private void NewChartRowButton(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Editor EditorWindow)
            {
                TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                GameData game = treeViewItem.Tag as GameData;

                var chartAchievements = game.AchievementList.Where(a => a.Type == Achievement.AchievementTypes.Chart).ToList();
                var difficultyGroups = chartAchievements.GroupBy(a => a.Difficulty).ToDictionary(g => g.Key, g => g.ToList());
                var shotTypeGroups = chartAchievements.GroupBy(a => a.ShotType).ToDictionary(g => g.Key, g => g.ToList());

                if (chartAchievements.Any(a => a.ShotType == "New"))
                {
                    return;
                }

                List<string> Difficultys = new();
                List<string> ShotTypes = new();

                foreach (var chartAchievement in game.AchievementList.Where(a => a.Type == Achievement.AchievementTypes.Chart))
                {
                    if (!Difficultys.Contains(chartAchievement.Difficulty)) { Difficultys.Add(chartAchievement.Difficulty); }
                    if (!ShotTypes.Contains(chartAchievement.ShotType)) { ShotTypes.Add(chartAchievement.ShotType); }
                }                


                foreach (string difficulty in Difficultys) 
                {
                    Achievement achievement = new();
                    achievement.Type = Achievement.AchievementTypes.Chart;
                    achievement.Difficulty = difficulty;
                    game.AchievementList.Add(achievement);
                }

                if (chartAchievements.Count == 0) 
                {
                    Achievement achievement = new();
                    achievement.Type = Achievement.AchievementTypes.Chart;
                    game.AchievementList.Add(achievement);
                }


                treeViewItem.IsSelected = false;
                treeViewItem.IsSelected = true;


            }
        }

        private void NewChartColumnButton(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Editor EditorWindow)
            {
                TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                GameData game = treeViewItem.Tag as GameData;

                var chartAchievements = game.AchievementList.Where(a => a.Type == Achievement.AchievementTypes.Chart).ToList();
                var difficultyGroups = chartAchievements.GroupBy(a => a.Difficulty).ToDictionary(g => g.Key, g => g.ToList());
                var shotTypeGroups = chartAchievements.GroupBy(a => a.ShotType).ToDictionary(g => g.Key, g => g.ToList());

                if (chartAchievements.Any(a => a.Difficulty == "New"))
                {
                    return;
                }

                List<string> Difficultys = new();
                List<string> ShotTypes = new();

                foreach (var chartAchievement in game.AchievementList.Where(a => a.Type == Achievement.AchievementTypes.Chart))
                {
                    if (!Difficultys.Contains(chartAchievement.Difficulty)) { Difficultys.Add(chartAchievement.Difficulty); }
                    if (!ShotTypes.Contains(chartAchievement.ShotType)) { ShotTypes.Add(chartAchievement.ShotType); }
                }


                foreach (string shottype in ShotTypes)
                {
                    Achievement achievement = new();
                    achievement.Type = Achievement.AchievementTypes.Chart;
                    achievement.ShotType = shottype;
                    game.AchievementList.Add(achievement);
                }

                if (chartAchievements.Count == 0)
                {
                    Achievement achievement = new();
                    achievement.Type = Achievement.AchievementTypes.Chart;
                    game.AchievementList.Add(achievement);
                }


                treeViewItem.IsSelected = false;
                treeViewItem.IsSelected = true;


            }

        }

        private void CreateAchievementChart(GameData game) 
        {
            DockPanel ChartPanel = new();
            DockPanel.SetDock(ChartPanel, Dock.Top);
            AchievementListDockPanel.Children.Add(ChartPanel);


            var chartAchievements = game.AchievementList.Where(a => a.Type == Achievement.AchievementTypes.Chart).ToList();
            var difficultyGroups = chartAchievements.GroupBy(a => a.Difficulty).ToDictionary(g => g.Key, g => g.ToList());
            var shotTypeGroups = chartAchievements.GroupBy(a => a.ShotType).ToDictionary(g => g.Key, g => g.ToList());

            List<string> Difficultys = new();
            List<string> ShotTypes = new();
            int LeftColumnWidth = 200;
            int ColumnWidth = 70;

            foreach (var chartAchievement in game.AchievementList.Where(a => a.Type == Achievement.AchievementTypes.Chart))
            {
                if (!Difficultys.Contains(chartAchievement.Difficulty)) { Difficultys.Add(chartAchievement.Difficulty); }
                if (!ShotTypes.Contains(chartAchievement.ShotType)) { ShotTypes.Add(chartAchievement.ShotType); }
            }

            //Difficulty Row
            DockPanel DifficultyPanel = new();
            DockPanel.SetDock(DifficultyPanel, Dock.Top);
            AchievementListDockPanel.Children.Add(DifficultyPanel);
            DifficultyPanel.LastChildFill = false;

            DockPanel BlankPanel = new();
            BlankPanel.Width = LeftColumnWidth;
            DockPanel.SetDock(BlankPanel, Dock.Left);
            DifficultyPanel.Children.Add(BlankPanel);

            foreach (var kvp in difficultyGroups)
            {
                string difficultyKey = kvp.Key;
                List<Achievement> group = kvp.Value;

                TextBox difficultyBox = new();
                difficultyBox.Width = ColumnWidth;
                difficultyBox.Text = difficultyKey;
                DockPanel.SetDock(difficultyBox, Dock.Left);
                DifficultyPanel.Children.Add(difficultyBox);

                difficultyBox.TextChanged += (sender, e) =>
                {
                    var box = (TextBox)sender;
                    string newValue = box.Text;

                    foreach (var a in group)
                    {
                        a.Difficulty = newValue;
                    }
                };

                ContextMenu contextMenu = new();
                difficultyBox.ContextMenu = contextMenu;

                MenuItem delete = new();
                delete.Header = "Delete Difficulty";
                contextMenu.Items.Add(delete);
                delete.Click += (sender, e) => 
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this difficulty?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (Achievement achievement in group) 
                        {
                            game.AchievementList.Remove(achievement);
                            LibraryTouhou.MasterAchievementsList.Remove(achievement);

                            if (Window.GetWindow(this) is Editor EditorWindow)
                            {
                                TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                                treeViewItem.IsSelected = false;
                                treeViewItem.IsSelected = true;
                            }
                                
                        }                        
                    }
                };

            }



            //Shot Type Row
            foreach (string ShotType in ShotTypes) 
            {
                DockPanel ShotTypePanel = new();
                DockPanel.SetDock(ShotTypePanel, Dock.Top);
                AchievementListDockPanel.Children.Add(ShotTypePanel);
                ShotTypePanel.LastChildFill = false;

                List<Achievement> shotGroup = shotTypeGroups[ShotType];
                TextBox ShotTypeBox = new();
                ShotTypeBox.Width = LeftColumnWidth;
                ShotTypeBox.Text = ShotType;
                DockPanel.SetDock(ShotTypeBox, Dock.Left);
                ShotTypePanel.Children.Add(ShotTypeBox);
                ShotTypeBox.TextChanged += (sender, e) =>
                {
                    var box = (TextBox)sender;
                    string newValue = box.Text;

                    foreach (var a in shotGroup)
                    {
                        a.ShotType = newValue;
                    }
                };

                foreach (string Difficulty in Difficultys)
                {
                    var match = chartAchievements.FirstOrDefault(a =>a.ShotType == ShotType && a.Difficulty == Difficulty);

                    TextBox ShotDifficultyBox = new();
                    ShotDifficultyBox.Width = ColumnWidth;
                    DockPanel.SetDock(ShotDifficultyBox, Dock.Left);
                    ShotTypePanel.Children.Add(ShotDifficultyBox);

                    if (match != null)
                    {
                        ShotDifficultyBox.Text = match.Level.ToString();

                        ShotDifficultyBox.TextChanged += (sender, e) =>
                        {
                            var box = (TextBox)sender;
                            string newText = box.Text;

                            // Try to parse — only update when valid
                            if (int.TryParse(newText, out int newLevel))
                            {
                                match.Level = newLevel;
                                box.ClearValue(TextBox.BorderBrushProperty);
                            }
                            else
                            {
                                box.BorderBrush = Brushes.Red;
                            }
                        };
                    }
                    else
                    {
                        ShotDifficultyBox.Text = "?";
                    }

                    
                }
            }
            
        }

        private void CreateAchievementPanel(Achievement achievement) 
        {
            Thickness LabelOffset = new Thickness(0, -6, 0, -8);
            //Thickness LabelOffset = new Thickness(0, 0, 0, 0);

            Border border = new Border();
            DockPanel dockPanel = new DockPanel();
            border.Height = 32;
            border.Margin = new Thickness(0,0,0,2);
            border.Child = dockPanel;
            DockPanel.SetDock(border, Dock.Top);

            AchievementListDockPanel.Children.Add(border);


            {   // VALUE
                DockPanel ValuePanel = new();
                DockPanel.SetDock(ValuePanel, Dock.Left);
                dockPanel.Children.Add(ValuePanel);
                Label Label = new();
                Label.Margin = LabelOffset;
                Label.Content = "Lv";
                ValuePanel.Children.Add(Label);
                TextBox textBox = new TextBox();
                textBox.Width = 30;
                textBox.Text = achievement.Level.ToString();
                ValuePanel.Children.Add(textBox);
                textBox.TextChanged += TheTextChanged;
                void TheTextChanged(object sender, RoutedEventArgs e)
                {
                    achievement.Level = Int32.Parse(textBox.Text); 
                }
                
            }

            if (achievement.Type == Achievement.AchievementTypes.Basic)
            {
                {   // Name
                    DockPanel NamePanel = new();
                    DockPanel.SetDock(NamePanel, Dock.Left);
                    dockPanel.Children.Add(NamePanel);
                    Label Label = new();
                    Label.Content = "Name";
                    Label.Margin = LabelOffset;
                    NamePanel.Children.Add(Label);
                    TextBox textBox = new TextBox();
                    textBox.Width = 500;
                    textBox.Text = achievement.Name;
                    NamePanel.Children.Add(textBox);
                    textBox.TextChanged += TheTextChanged;
                    void TheTextChanged(object sender, RoutedEventArgs e)
                    {
                        achievement.Name = textBox.Text;
                    }
                }
            }

            if (achievement.Type == Achievement.AchievementTypes.Chart)
            {
                {   // DIFFICULTY
                    DockPanel DifficultyPanel = new();
                    DockPanel.SetDock(DifficultyPanel, Dock.Left);
                    dockPanel.Children.Add(DifficultyPanel);
                    Label Label = new();
                    Label.Margin = LabelOffset;
                    Label.Content = "Difficulty";
                    DifficultyPanel.Children.Add(Label);
                    TextBox textBox = new TextBox();
                    textBox.Width = 80;
                    textBox.Text = achievement.Difficulty;
                    DifficultyPanel.Children.Add(textBox);
                    textBox.TextChanged += TheTextChanged;
                    void TheTextChanged(object sender, RoutedEventArgs e)
                    {
                        achievement.Difficulty = textBox.Text;                        
                    }
                }

                {   // SHOT TYPE
                    DockPanel ShotTypePanel = new();
                    DockPanel.SetDock(ShotTypePanel, Dock.Left);
                    dockPanel.Children.Add(ShotTypePanel);
                    Label Label = new();
                    Label.Margin = LabelOffset;
                    Label.Content = "Shot Type";
                    ShotTypePanel.Children.Add(Label);
                    TextBox textBox = new TextBox();
                    textBox.Width = 140;
                    textBox.Text = achievement.ShotType;
                    ShotTypePanel.Children.Add(textBox);
                    textBox.TextChanged += TheTextChanged;
                    void TheTextChanged(object sender, RoutedEventArgs e)
                    {
                        achievement.ShotType = textBox.Text;
                    }
                }
            }

            {   // Delete Button
                DockPanel DeletePanel = new();
                DeletePanel.LastChildFill = true;
                DockPanel.SetDock(DeletePanel, Dock.Left);
                dockPanel.Children.Add(DeletePanel);


                Button deleteButton = new Button();
                DockPanel.SetDock(deleteButton, Dock.Left);
                deleteButton.Content = "  Delete  ";
                DeletePanel.Children.Add(deleteButton);
                deleteButton.Click += DeleteThis;
                void DeleteThis(object sender, RoutedEventArgs e)
                {
                    if (Window.GetWindow(this) is Editor EditorWindow)
                    {
                        TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                        GameData touhouGame = treeViewItem.Tag as GameData;

                        MessageBoxResult result = MessageBox.Show("Are you sure you want to remove this achievement?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            // Only perform the following actions if the user confirms
                            touhouGame.AchievementList.Remove(achievement);
                            LibraryTouhou.MasterAchievementsList.Remove(achievement);
                            AchievementListDockPanel.Children.Remove(border);
                        }

                    }
                }

                Button UPButton = new Button();
                UPButton.Margin = new Thickness(3,0,0,0);
                DockPanel.SetDock(UPButton, Dock.Left);
                UPButton.Content = "  UP  ";
                DeletePanel.Children.Add(UPButton);
                UPButton.Click += MoveUP;
                void MoveUP(object sender, RoutedEventArgs e)
                {
                    if (Window.GetWindow(this) is Editor EditorWindow)
                    {
                        TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                        GameData touhouGame = treeViewItem.Tag as GameData;

                        //LibraryMan.MasterAchievementsList.Remove(achievement);

                        int Index1 = touhouGame.AchievementList.IndexOf(achievement);
                        if (Index1 == 0) { return; }

                        touhouGame.AchievementList.Remove(achievement);                        
                        AchievementListDockPanel.Children.Remove(border);

                        touhouGame.AchievementList.Insert(Index1 - 1,achievement);
                        AchievementListDockPanel.Children.Insert(Index1 - 1, border);
                    }
                }

                Button DOWNButton = new Button();
                DOWNButton.Margin = new Thickness(3, 0, 0, 0);
                DockPanel.SetDock(DOWNButton, Dock.Left);
                DOWNButton.Content = "  DOWN  ";
                DeletePanel.Children.Add(DOWNButton);
                DOWNButton.Click += MoveDOWN;
                void MoveDOWN(object sender, RoutedEventArgs e)
                {
                    if (Window.GetWindow(this) is Editor EditorWindow)
                    {
                        TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                        GameData touhouGame = treeViewItem.Tag as GameData;

                        int Index1 = touhouGame.AchievementList.IndexOf(achievement);
                        int counta = touhouGame.AchievementList.Count;
                        if (Index1 == counta - 1) { return; }

                        touhouGame.AchievementList.Remove(achievement);
                        AchievementListDockPanel.Children.Remove(border);

                        touhouGame.AchievementList.Insert(Index1 + 1, achievement);
                        AchievementListDockPanel.Children.Insert(Index1 + 1, border);


                    }
                }
            }

            {   // NOTES
                dockPanel.LastChildFill = false; ////////////////////////////////////////////THE LAST CHILD FILL

                //DockPanel NotesPanel = new();
                //NotesPanel.LastChildFill = true;
                //DockPanel.SetDock(NotesPanel, Dock.Left);
                //dockPanel.Children.Add(NotesPanel);
                //Label Label = new();
                //DockPanel.SetDock(Label, Dock.Left);
                //Label.Content = "Note";
                //Label.Margin = LabelOffset;
                //NotesPanel.Children.Add(Label);
                //TextBox textBox = new TextBox();
                //DockPanel.SetDock(textBox, Dock.Left);
                //textBox.Text = achievement.Note;
                //NotesPanel.Children.Add(textBox);
                //textBox.TextChanged += TheTextChanged;
                //void TheTextChanged(object sender, RoutedEventArgs e)
                //{
                //    achievement.Note = textBox.Text;
                //}
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        
    }
}

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
                TouhouGame touhouGame = treeViewItem.Tag as TouhouGame;

                foreach (Achievement achievement in touhouGame.AchievementList)
                {
                    CreateAchievementPanel(achievement);
                }
            }

            
        }
        

        private void NewBasicAchievementButton(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Editor EditorWindow)
            {
                TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                TouhouGame touhouGame = treeViewItem.Tag as TouhouGame;

                Achievement achievement = new();
                achievement.Key = LibraryMan.GenerateKey();
                achievement.Type = Achievement.AchievementTypes.Basic;
                touhouGame.AchievementList.Add(achievement);

                CreateAchievementPanel(achievement);


            }
        }

        private void NewChartAchievementButton(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Editor EditorWindow)
            {
                TreeViewItem treeViewItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                TouhouGame touhouGame = treeViewItem.Tag as TouhouGame;

                Achievement achievement = new();
                achievement.Key = LibraryMan.GenerateKey();
                achievement.Type = Achievement.AchievementTypes.Chart;
                touhouGame.AchievementList.Add(achievement);

                CreateAchievementPanel(achievement);


            }
        }

        private void CreateAchievementPanel(Achievement achievement) 
        {
            Thickness LabelOffset = new Thickness(0, -6, 0, -8);
            //Thickness LabelOffset = new Thickness(0, 0, 0, 0);

            Border border = new Border();
            DockPanel dockPanel = new DockPanel();
            border.Height = 22;
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
                    textBox.Width = 405;
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
                    textBox.Width = 120;
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
                        TouhouGame touhouGame = treeViewItem.Tag as TouhouGame;

                        MessageBoxResult result = MessageBox.Show("Are you sure you want to remove this achievement?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            // Only perform the following actions if the user confirms
                            touhouGame.AchievementList.Remove(achievement);
                            LibraryMan.MasterAchievementsList.Remove(achievement);
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
                        TouhouGame touhouGame = treeViewItem.Tag as TouhouGame;

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
                        TouhouGame touhouGame = treeViewItem.Tag as TouhouGame;

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

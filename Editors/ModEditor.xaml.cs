using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for ModEditor.xaml
    /// </summary>
    public partial class ModEditor : UserControl
    {
        TreeViewItem TreeItem { get; set; }
        TouhouMod ThisMod { get; set; }

        public ModEditor()
        {
            InitializeComponent();

            foreach (TouhouMod touhouMod in LibraryTouhou.MasterModsList)
            {
                LoadMod(touhouMod);
            }
        }

        private void NewModButton(object sender, RoutedEventArgs e)
        {
            TouhouMod touhouMod = new();
            LibraryTouhou.MasterModsList.Add(touhouMod);
            LoadMod(touhouMod);
        }

        private void DeleteModButton(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = TheTreeView.SelectedItem as TreeViewItem;
            TouhouMod touhouMod = treeViewItem.Tag as TouhouMod;

            LibraryTouhou.MasterModsList.Remove(touhouMod);
            TheTreeView.Items.Remove(treeViewItem);
        }

        public void LoadMod(TouhouMod Mod)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Tag = Mod;
            TheTreeView.Items.Add(treeViewItem);

            ModNameBuilder(Mod);



        }

        public void ModNameBuilder(TouhouMod Mod) 
        {
            foreach(TreeViewItem item in TheTreeView.Items)
            {
                if (item.Tag == Mod)
                {
                    // If the mod already exists, update its header
                    item.Header = Mod.Name;
                    if (Mod.Recommend == true) { item.Header = "👍 " + Mod.Name; }
                    item.Foreground = Brushes.White;
                    return;
                }
            }
            
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeItem = TheTreeView.SelectedItem as TreeViewItem;
            if (TreeItem == null) { return; }
            ThisMod = TreeItem.Tag as TouhouMod;


            NameBox.Text = ThisMod.Name;
            KeyBox.Text = ThisMod.Key;
            DescriptionBox.Text = ThisMod.Description;
            THCrapBox.Text = ThisMod.THCrapText;
            NoteBox.Text = ThisMod.Note;

            if (ThisMod.Recommend == true)
            {
                RecommendCheckbox.IsChecked = true;
            }
            else
            {
                RecommendCheckbox.IsChecked = false;
            }

            GamesBox.Clear();

            foreach (GameData game in LibraryTouhou.MasterGameList) 
            {
                foreach (TouhouMod mod in game.ModList) 
                {
                    if (ThisMod == mod) 
                    {
                        GamesBox.AppendText(game.CodeName + ": " + game.SubtitleName + Environment.NewLine);
                    }
                }
            }
        }

        private void NameboxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            ThisMod.Name = NameBox.Text;
            ModNameBuilder(ThisMod);
        }

        private void KeyTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            ThisMod.Key = KeyBox.Text;
            
        }

        private void THCrapTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            ThisMod.THCrapText = THCrapBox.Text;
        }

        private void DescriptionTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            ThisMod.Description = DescriptionBox.Text;
        }

        private void NoteBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            ThisMod.Note = NoteBox.Text;
        }

        private void RecommendedChecked(object sender, RoutedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            ThisMod.Recommend = true;
            ModNameBuilder(ThisMod);
        }

        private void RecommendedUnchecked(object sender, RoutedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            ThisMod.Recommend = false;
            ModNameBuilder(ThisMod);
        }

        private void AddModToGame(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this); //load achievements
            if (parentWindow is Editor EditorWindow)
            {
                TreeViewItem GameItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                GameData touhouGame = GameItem.Tag as GameData;
                touhouGame.ModList.Add(ThisMod);
                EditorWindow.TheGameEditor.LoadMod(ThisMod);
            }



        }

        

        private void MoveUP(object sender, RoutedEventArgs e)
        {
            if (TheTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                TreeViewItem parent = selectedItem.Parent as TreeViewItem;

                int currentIndex = parent != null ? parent.Items.IndexOf(selectedItem) : TheTreeView.Items.IndexOf(selectedItem);
                if (currentIndex > 0) // Ensure the item is not the first one
                {
                    // Remove the selected item from its current parent
                    if (parent != null)
                    {
                        parent.Items.Remove(selectedItem);
                    }
                    else
                    {
                        TheTreeView.Items.Remove(selectedItem);
                    }

                    // Insert the item at the new position in the same parent
                    if (parent != null)
                    {
                        parent.Items.Insert(currentIndex - 1, selectedItem);
                    }
                    else
                    {
                        TheTreeView.Items.Insert(currentIndex - 1, selectedItem);
                    }

                    // Update selection to the moved item
                    //TheTreeView.SelectedItem = selectedItem;
                    selectedItem.IsSelected = true;
                    selectedItem.Focus();
                }
            }

        }

        private void MoveDOWN(object sender, RoutedEventArgs e)
        {
            if (TheTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                TreeViewItem parent = selectedItem.Parent as TreeViewItem;

                int currentIndex = parent != null ? parent.Items.IndexOf(selectedItem) : TheTreeView.Items.IndexOf(selectedItem);
                int IC = TheTreeView.Items.Count - 1;
                if (currentIndex != IC) // Ensure the item is not the first one
                {
                    // Remove the selected item from its current parent
                    if (parent != null)
                    {
                        parent.Items.Remove(selectedItem);
                    }
                    else
                    {
                        TheTreeView.Items.Remove(selectedItem);
                    }

                    // Insert the item at the new position in the same parent
                    if (parent != null)
                    {
                        parent.Items.Insert(currentIndex + 1, selectedItem);
                    }
                    else
                    {
                        TheTreeView.Items.Insert(currentIndex + 1, selectedItem);
                    }

                    // Update selection to the moved item
                    //TheTreeView.SelectedItem = selectedItem;
                    selectedItem.IsSelected = true;
                    selectedItem.Focus();
                }
            }
        }

        
    }
}

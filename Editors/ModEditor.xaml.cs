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
        TouhouMod Mod { get; set; }

        public ModEditor()
        {
            InitializeComponent();

            foreach (TouhouMod touhouMod in LibraryMan.MasterModsList)
            {
                LoadMod(touhouMod);
            }
        }

        private void NewModButton(object sender, RoutedEventArgs e)
        {
            TouhouMod touhouMod = new();
            LibraryMan.MasterModsList.Add(touhouMod);
            LoadMod(touhouMod);
        }

        private void DeleteModButton(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = TheTreeView.SelectedItem as TreeViewItem;
            TouhouMod touhouMod = treeViewItem.Tag as TouhouMod;

            LibraryMan.MasterModsList.Remove(touhouMod);
            TheTreeView.Items.Remove(treeViewItem);
        }

        public void LoadMod(TouhouMod Mod)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = Mod.Key;
            treeViewItem.Tag = Mod;
            TheTreeView.Items.Add(treeViewItem);

            treeViewItem.Foreground = Brushes.White;



        }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeItem = TheTreeView.SelectedItem as TreeViewItem;
            if (TreeItem == null) { return; }
            Mod = TreeItem.Tag as TouhouMod;



            KeyBox.Text = Mod.Key;
            DescriptionBox.Text = Mod.Description;
            THCrapBox.Text = Mod.THCrapText;

            if (Mod.Recommend == true)
            {
                RecommendCheckbox.IsChecked = true;
            }
            else
            {
                RecommendCheckbox.IsChecked = false;
            }
        }

        private void KeyTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            Mod.Key = KeyBox.Text;
            TreeItem.Header = Mod.Key;
        }

        private void THCrapTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            Mod.THCrapText = THCrapBox.Text;
        }

        private void DescriptionTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            Mod.Description = DescriptionBox.Text;
        }

        private void RecommendedChecked(object sender, RoutedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            Mod.Recommend = true;
        }

        private void RecommendedUnchecked(object sender, RoutedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            Mod.Recommend = false;
        }

        private void AddModToGame(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this); //load achievements
            if (parentWindow is Editor EditorWindow)
            {
                TreeViewItem GameItem = EditorWindow.TheGameEditor.TheTreeView.SelectedItem as TreeViewItem;
                TouhouGame touhouGame = GameItem.Tag as TouhouGame;
                touhouGame.ModList.Add(Mod);
                EditorWindow.TheGameEditor.LoadMod(Mod);
            }



        }

        private void NameboxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TheTreeView.SelectedItem == null)
            {
                return;
            }
            Mod.Name = NameBox.Text;
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

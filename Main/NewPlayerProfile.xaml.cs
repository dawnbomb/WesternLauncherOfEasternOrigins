using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Xml;

namespace WesternLauncherOfEasternOrigins
{
    /// <summary>
    /// Interaction logic for NewPlayerProfile.xaml
    /// </summary>
    public partial class NewPlayerProfile : Window
    {
        //public string PlayerName { get; private set; }
        GameLauncher GameLauncher;

        public NewPlayerProfile(GameLauncher TheGameLauncher)
        {
            InitializeComponent();
            GameLauncher = TheGameLauncher;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate the input: non-empty and contains only allowed characters
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || !System.Text.RegularExpressions.Regex.IsMatch(nameTextBox.Text, @"^[a-zA-Z0-9 \p{L}]+$"))
            {
                MessageBox.Show("Invalid name. Only letters and numbers are allowed.");
                return;
            }

            string PlayerName = nameTextBox.Text;            

            ComboBoxItem comboBoxItem = new();
            comboBoxItem.Content = PlayerName;
            GameLauncher.PlayerComboBox.Items.Add(comboBoxItem);
            //Create a dummy achievement file for them....
            CreateNewPlayerXML(PlayerName);

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }




        private void CreateNewPlayerXML(string NewPlayerName)
        {

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(LibraryTouhou.TouhouLauncherPath + "\\Player Profiles\\" + NewPlayerName + ".xml", settings))
            {
                writer.WriteStartElement("Root");
                writer.WriteStartElement("AchievementList");

                //foreach (Achievement achievement in LibraryMan.MasterAchievementsList)
                //{
                //    writer.WriteStartElement("Achievement");
                //    writer.WriteElementString("Type", "Basic");
                //    writer.WriteElementString("Name", achievement.Name);
                //    writer.WriteElementString("Difficulty", achievement.Difficulty);
                //    writer.WriteElementString("ShotType", achievement.ShotType);
                //    writer.WriteElementString("Key", achievement.Key);
                //    writer.WriteElementString("Level", achievement.Level.ToString());
                //    writer.WriteElementString("PlayerText", achievement.PlayerText);
                //    writer.WriteElementString("Note", achievement.Note);
                //    writer.WriteEndElement(); //End Achievement 
                //}

                writer.WriteEndElement(); //End AchievementList  
                writer.WriteEndElement(); //End Root  
                writer.Flush(); //Ends the XML 
            }
        }





    }
}

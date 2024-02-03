using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WariosWoodsRando
{
    public partial class MainWindow : Window
    {
        private string LastInputFilePath
        {
            get { return Properties.Settings.Default.LastInputFilePath; }
            set
            {
                Properties.Settings.Default.LastInputFilePath = value;
                Properties.Settings.Default.Save();
            }
        }
        public MainWindow()
        {
            Properties.Settings.Default.Reload();
            InitializeComponent();


            Random r = new Random();
            seed.Text = r.Next(1000000, 9999999).ToString();


            txtInputFile.Text = LastInputFilePath;

            // Generate list of available music
            List<string> musics = new List<string>
            {
                "Normal Music",
                "VS Mode Music",
                "Time Race Music",
                "Lesson Mode Music",
            };

            foreach(String s in musics)
                cbox_musics.Items.Add(s);
            

            cbox_musics.SelectedIndex = 0;


        }

        private void BrowseInputFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = LastInputFilePath; // Set initial directory based on the last selected path

            if (openFileDialog.ShowDialog() == true)
            {
                txtInputFile.Text = openFileDialog.FileName;
                LastInputFilePath = openFileDialog.FileName; // Save the selected path
            }

        }

        private void NumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Char.IsDigit(e.Text, 0) || ((TextBox)sender).Text.Length >= 7;
        }


        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Rando.Main(LastInputFilePath, seed.Text, this);           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            seed.Text = r.Next(1000000, 9999999).ToString();

        }

        public bool NoMusic() { return (bool)Box_no_music.IsChecked; }
        public bool VanillaHeight() { return (bool)Box_vanilla_height.IsChecked; }

        private void Box_no_music_Checked(object sender, RoutedEventArgs e)
        {
            cbox_musics.IsEnabled = false;
        }

        private void Box_no_music_Unchecked(object sender, RoutedEventArgs e)
        {
            cbox_musics.IsEnabled = true;
        }
    }
}

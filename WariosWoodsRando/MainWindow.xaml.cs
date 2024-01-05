using Microsoft.Win32;
using System;
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
            Rando.Main(LastInputFilePath, seed.Text);           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            seed.Text = r.Next(1000000, 9999999).ToString();

        }
    }
}

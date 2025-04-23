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

namespace WpfCalculatorLab4_Egmilan
{
    /// <summary>
    /// Interaction logic for DateCalculationWindow.xaml
    /// </summary>
    public partial class DateCalculationWindow : Window
    {
       
        public DateCalculationWindow()
        {
            InitializeComponent();
           
            MainFrame4.Navigate(new DateCalculation());
        }

        private void OpenMenu2(object sender, RoutedEventArgs e)
        {
            if (PopupMenu != null) // Safety check
            {
                PopupMenu.IsOpen = true;
            }
            else
            {
                MessageBox.Show("PopupMenu not found!");
            }
        }

        private void OpenScientific(object sender, RoutedEventArgs e)
        {
            //var mainWindow = Application.Current.MainWindow as MainWindow; // Get the main window
            ScientificWindow scientificWindow = new ScientificWindow(); // Pass MainWindow
            scientificWindow.Show(); // Show it

            PopupMenu.IsOpen = false; // Close the menu after clicking
            this.Hide(); // Optional: Hide MainWindow if you don’t want both open
        }
        private void OpenStandard(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(); // Create MainWindow
            mainWindow.Show(); // Show MainWindow
            this.Close(); // Close the ScientificWindow
        }

        private void OpenProgrammer(object sender, RoutedEventArgs e)
        {
            ProgrammerWindow programmerWindow = new ProgrammerWindow(); // Create the Scientific window
            programmerWindow.Show(); // Show it

            PopupMenu.IsOpen = false; // Close the menu after clicking
            this.Hide(); // Optional: Hide MainWindow if you don’t want both open
        }

        /*public void UpdateDateDisplay(string text)
        {
            if (MainFrame4.Content is DateCalculation dateCalcPage)
            {
                dateCalcPage.UpdateDateDisplay(text);
            }
        }*/
    }
}

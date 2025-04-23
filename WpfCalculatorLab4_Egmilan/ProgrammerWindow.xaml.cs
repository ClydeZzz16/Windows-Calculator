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
    /// Interaction logic for ProgrammerWindow.xaml
    /// </summary>
    public partial class ProgrammerWindow : Window
    {
        public ProgrammerWindow()
        {
            InitializeComponent();
            MainFrame3.Navigate(new Programmer());
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

        private void OpenDateCalculation(object sender, RoutedEventArgs e)
        {
            DateCalculationWindow dateCalculationWindow = new DateCalculationWindow(); // Create the Scientific window
            dateCalculationWindow.Show(); // Show it

            PopupMenu.IsOpen = false; // Close the menu after clicking
            this.Hide(); // Optional: Hide MainWindow if you don’t want both open
        }

        public void UpdateProgrammerDisplay(string text)
        {
            Dispatcher.Invoke(() =>
            {
                // Handle overflow with ellipsis
                if (text.Length > 12)
                {
                    ProgrammerTextBox.FontSize = 36;
                    ProgrammerTextBox.Text = text.Substring(0, 9) + "...";
                }
                else
                {
                    ProgrammerTextBox.FontSize = 48;
                    ProgrammerTextBox.Text = text;
                }

                // Force immediate render
                ProgrammerTextBox.InvalidateVisual();
            });
        }

        public void UpdateHistoryDisplay(string historyText)
        {
            Dispatcher.Invoke(() =>
            {
                ProgrammerHistoryTextBox.Text = historyText; // Overwrite (no multi-line history)
            });
        }

        public string GetDisplayValue()
        {
            // Implement based on how your programmer calculator displays values
            // For example, if you have a main display TextBox:
            return Dispatcher.Invoke(() => ProgrammerTextBox.Text);
        }

        public void SetDisplayValue(string value)
        {
            Dispatcher.Invoke(() =>
            {
                ProgrammerTextBox.Text = value;
            });
        }
        private void ProgrammerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

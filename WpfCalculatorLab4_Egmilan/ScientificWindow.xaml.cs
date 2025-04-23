using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for ScientificWindow.xaml
    /// </summary>
    public partial class ScientificWindow : Window
    {
        public ScientificWindow() // Pass MainWindow reference
        {
            InitializeComponent();
            //LoadScientificPage();

            // Pass the reference to Scientific page in the MainFrame
            MainFrame2.Navigate(new Scientific()); // Pass the MainWindow reference

            // Optionally, pass it to SecondaryFrame if needed
            SecondaryFrame2.Navigate(new ScientificSecondFrame());

        }

        public void UpdateScientificDisplay(string text)
        {
            Dispatcher.Invoke(() =>
            {
                // Handle overflow with ellipsis
                if (text.Length > 12)
                {
                    ScientificMainTextBox.FontSize = 36;
                    ScientificMainTextBox.Text = text.Substring(0, 9) + "...";
                }
                else
                {
                    ScientificMainTextBox.FontSize = 48;
                    ScientificMainTextBox.Text = text;
                }

                // Force immediate render
                ScientificMainTextBox.InvalidateVisual();
            });
        }

        public void UpdateHistoryDisplay(string historyText)
        {
            Dispatcher.Invoke(() =>
            {
                HistoryTextBlock.Text = historyText; // Overwrite (no multi-line history)
            });
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

        private void OpenDateCalculation(object sender, RoutedEventArgs e)
        {
            DateCalculationWindow dateCalculationWindow = new DateCalculationWindow(); // Create the Scientific window
            dateCalculationWindow.Show(); // Show it

            PopupMenu.IsOpen = false; // Close the menu after clicking
            this.Hide(); // Optional: Hide MainWindow if you don’t want both open
        }

        private void ScientificMainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public void ClearHistory()
        {
            Dispatcher.Invoke(() =>
            {
                HistoryTextBlock.Text = ""; // Clear all history
            });
        }

        public string GetScientificDisplay()
        {
            return Dispatcher.Invoke(() =>
            {
                // Get current display text and remove formatting
                string displayText = ScientificMainTextBox.Text;

                // Handle special constants
                if (displayText == "π") return Math.PI.ToString();
                if (displayText == "e") return Math.E.ToString();

                // Remove any formatting commas
                return displayText.Replace(",", "");
            });
        }

        private int _currentAngleMode = 0; // 0=Deg, 1=Rad, 2=Grad

        public void SetAngleMode(int mode)
        {
            _currentAngleMode = mode;
            // You might want to update something in the UI to reflect this
        }

        public int GetAngleMode()
        {
            return _currentAngleMode;
        }

        public void SetScientificDisplay(string value)
        {
            Dispatcher.Invoke(() =>
            {
                ScientificMainTextBox.Text = value;
                ScientificMainTextBox.FontSize = 48;
                ScientificMainTextBox.InvalidateVisual();
            });
        }

        /*public void ToggleAngleMode()
        {
            (Content as Scientific)?.ToggleAngleMode();
        }*/

        public void ToggleScientificNotation()
        {
            (Content as Scientific)?.ToggleScientificNotation();
        }

       /* public void SetAngleMode2(int mode)
        {
            if (Content is Scientific scientific)
            {
                scientific.SetAngleMode((Scientific.AngleMode)mode);
            }
        }*/
    }
}

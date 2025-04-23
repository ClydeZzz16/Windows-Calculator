using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfCalculatorLab4_Egmilan;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    Standard standard;
    Scientific scientific;
    Programmer programmer;
    DateCalculation date;

    public MainWindow()
    {
        InitializeComponent();

        MainFrame.Navigate(new Standard());
        SecondaryFrame.Navigate(new StandardSecondFrame());

        standard = new Standard();
        scientific = new Scientific();
        programmer = new Programmer();
        date = new DateCalculation();

    }

    public void UpdateStandardDisplay(string text)
    {
        Dispatcher.Invoke(() =>
        {
            // Handle overflow with ellipsis
            if (text.Length > 12)
            {
                StandardTextBox.FontSize = 36;
                StandardTextBox.Text = text.Substring(0, 9) + "...";
            }
            else
            {
                StandardTextBox.FontSize = 48;
                StandardTextBox.Text = text;
            }

            // Force immediate render
            StandardTextBox.InvalidateVisual();
        });
    }

    public void UpdateHistoryDisplay(string historyText)
    {
        Dispatcher.Invoke(() =>
        {
            StandardHistoryTextBox.Text = historyText; // Overwrite (no multi-line history)
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

    private void OpenScientific(object sender, RoutedEventArgs e)
    {
        ScientificWindow scientificWindow = new ScientificWindow(); // Pass the reference of MainWindow
        scientificWindow.Show(); // Show the window

        PopupMenu.IsOpen = false; // Close the menu after clicking
        this.Hide(); // Optionally hide MainWindow if you don’t want both windows open
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

    private void StandardTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {

    }


    /*public void UpdateDisplay(string text)
    {
        MessageBox.Show($"Updating display to: {text}"); // Debug
        MainTextBox.Text = text; // Assuming your TextBox is named "Display"
    }*/

    /*public void UpdateMemoryIndicator(string indicator)
    {
        // Assuming you have a TextBlock named MemoryIndicator
        MemoryIndicatorTextBlock.Text = indicator;
    }*/

    public string GetStandardDisplay()
    {
        // Assuming you have a TextBox named StandardDisplay
        return StandardTextBox.Text;
    }

    public void SetStandardDisplay(string value)
    {
        StandardTextBox.Text = value;
    }

}
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfCalculatorLab4_Egmilan
{
    /// <summary>
    /// Interaction logic for ScientificSecondFrame.xaml
    /// </summary>
    public partial class ScientificSecondFrame : Page
    {
        // Memory fields
        private double _memoryValue = 0;
        private bool _memoryHasValue = false;
        private List<double> _memoryHistory = new List<double>();

        // Angle mode (0=Deg, 1=Rad, 2=Grad)
        private int _angleMode = 0;

        public ScientificSecondFrame()
        {
            InitializeComponent();
            UpdateMemoryButtonsState();
            UpdateAngleModeDisplay();
        }

        private ScientificWindow GetScientificWindow()
        {
            return Window.GetWindow(this) as ScientificWindow;
        }

        #region Angle Mode Functions

        private void degreeButton_Click(object sender, RoutedEventArgs e)
        {
            _angleMode = (_angleMode + 1) % 3;
            UpdateAngleModeDisplay();
            GetScientificWindow()?.SetAngleMode(_angleMode);
        }

        private void UpdateAngleModeDisplay()
        {
            degree.Visibility = _angleMode == 0 ? Visibility.Visible : Visibility.Collapsed;
            radian.Visibility = _angleMode == 1 ? Visibility.Visible : Visibility.Collapsed;
            grad.Visibility = _angleMode == 2 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Memory Functions (Identical to StandardSecondFrame but using ScientificWindow)

        // Memory Clear (MC)
        private void MemoryClearButton_Click(object sender, RoutedEventArgs e)
        {
            _memoryValue = 0;
            _memoryHasValue = false;
            _memoryHistory.Clear();
            UpdateMemoryButtonsState();
        }

        // Memory Recall (MR)
        private void MemoryRecallButton_Click(object sender, RoutedEventArgs e)
        {
            if (_memoryHasValue)
            {
                GetScientificWindow()?.SetScientificDisplay(_memoryValue.ToString());
            }
        }

        // Memory Add (M+)
        private void MemoryAddButton_Click(object sender, RoutedEventArgs e)
        {
            var sciWindow = GetScientificWindow();
            if (sciWindow != null && double.TryParse(sciWindow.GetScientificDisplay(), out double currentValue))
            {
                if (_memoryHasValue)
                {
                    _memoryValue += currentValue;
                }
                else
                {
                    _memoryValue = currentValue;
                    _memoryHasValue = true;
                }
                _memoryHistory.Add(_memoryValue);
                UpdateMemoryButtonsState();
            }
        }

        // Memory Subtract (M-)
        private void MemorySubtractButton_Click(object sender, RoutedEventArgs e)
        {
            var sciWindow = GetScientificWindow();
            if (sciWindow != null && double.TryParse(sciWindow.GetScientificDisplay(), out double currentValue))
            {
                if (_memoryHasValue)
                {
                    _memoryValue -= currentValue;
                }
                else
                {
                    _memoryValue = -currentValue;
                    _memoryHasValue = true;
                }

                if (currentValue != 0)
                {
                    _memoryHistory.Add(_memoryValue);
                }

                UpdateMemoryButtonsState();
            }
        }

        // Memory Store (MS)
        private void MemoryStoreButton_Click(object sender, RoutedEventArgs e)
        {
            var sciWindow = GetScientificWindow();
            if (sciWindow != null && double.TryParse(sciWindow.GetScientificDisplay(), out double currentValue))
            {
                _memoryValue = currentValue;
                _memoryHasValue = true;
                _memoryHistory.Add(_memoryValue);
                UpdateMemoryButtonsState();

                // Visual feedback
                sciWindow.SetScientificDisplay("Stored");
                Task.Delay(500).ContinueWith(_ =>
                {
                    sciWindow.SetScientificDisplay(currentValue.ToString());
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        // Memory Recall and Clear (M▼)
        private void MemoryRecallClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (_memoryHasValue)
            {
                MemoryHistoryList.ItemsSource = _memoryHistory
                    .Select((value, index) => $"M{index + 1}: {FormatMemoryValue(value)}")
                    .Reverse()
                    .ToList();

                MemoryHistoryPopup.IsOpen = true;
            }
        }

        private string FormatMemoryValue(double value)
        {
            return value.ToString("#,##0.##########");
        }

        private void CloseMemoryHistory_Click(object sender, RoutedEventArgs e)
        {
            MemoryHistoryPopup.IsOpen = false;
        }

        private void UpdateMemoryButtonsState()
        {
            MemoryClearButton.IsEnabled = _memoryHasValue;
            MemoryRecallButton.IsEnabled = _memoryHasValue;
            MemoryRecallClearButton.IsEnabled = _memoryHasValue;
        }

        /*private void degreeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as ScientificWindow;
            mainWindow?.ToggleAngleMode();
        }*/

        private void FENotationButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as ScientificWindow;
            mainWindow?.ToggleScientificNotation();
        }



        #endregion
    }

    /*private void degreeButton_Click(object sender, RoutedEventArgs e)
    {
        if (degree.Visibility == Visibility.Visible)
        {
            degree.Visibility = Visibility.Collapsed;
            radian.Visibility = Visibility.Visible;
        }
        else if (radian.Visibility == Visibility.Visible)
        {
            radian.Visibility = Visibility.Collapsed;
            grad.Visibility = Visibility.Visible;
        }
        else if (grad.Visibility == Visibility.Visible)
        {
            grad.Visibility = Visibility.Collapsed;
            degree.Visibility = Visibility.Visible;
        }
    }*/


}


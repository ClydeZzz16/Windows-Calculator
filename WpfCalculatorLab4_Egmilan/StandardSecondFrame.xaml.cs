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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfCalculatorLab4_Egmilan
{
    /// <summary>
    /// Interaction logic for StandardSecondFrame.xaml
    /// </summary>
    public partial class StandardSecondFrame : Page
    {
        // Memory fields
        private double _memoryValue = 0;
        private bool _memoryHasValue = false;
        private List<double> _memoryHistory = new List<double>();

        public StandardSecondFrame()
        {
            InitializeComponent();
            UpdateMemoryButtonsState();
        }

        private MainWindow MainWindow => Application.Current.MainWindow as MainWindow;

        // Memory Clear (MC)
        private void MemoryClearButton_Click(object sender, RoutedEventArgs e)
        {
            _memoryValue = 0;
            _memoryHasValue = false;
            _memoryHistory.Clear();
            //UpdateMemoryIndicator();
            UpdateMemoryButtonsState();
        }

        // Memory Recall (MR)
        private void MemoryRecallButton_Click(object sender, RoutedEventArgs e)
        {
            if (_memoryHasValue)
            {
                MainWindow?.SetStandardDisplay(_memoryValue.ToString());
                //UpdateMemoryIndicator();
            }
        }

        // Memory Add (M+)
        private void MemoryAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(MainWindow?.GetStandardDisplay(), out double currentValue))
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
                //UpdateMemoryIndicator();
                UpdateMemoryButtonsState(); // Enable buttons after first storage
            }
        }

        // Memory Subtract (M-)
        private void MemorySubtractButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(MainWindow?.GetStandardDisplay(), out double currentValue))
            {
                if (_memoryHasValue)
                {
                    _memoryValue -= currentValue;
                }
                else
                {
                    // Initialize memory with negative of current value
                    _memoryValue = -currentValue;
                    _memoryHasValue = true;
                }

                // Only add to history if the operation actually changed memory
                if (currentValue != 0)
                {
                    _memoryHistory.Add(_memoryValue);
                }

                //UpdateMemoryIndicator();
                UpdateMemoryButtonsState();
            }
        }

        // Memory Store (MS)
        private void MemoryStoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(MainWindow?.GetStandardDisplay(), out double currentValue))
            {
                _memoryValue = currentValue;
                _memoryHasValue = true;
                _memoryHistory.Add(_memoryValue);
                //UpdateMemoryIndicator();
                UpdateMemoryButtonsState(); // Enable buttons after first storage
            }
        }

        // Memory Recall and Clear (M▼)
        private void MemoryRecallClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (_memoryHasValue)
            {
                // Show memory history
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


        // Update memory indicator
        /*private void UpdateMemoryIndicator()
        {
            MainWindow?.UpdateMemoryIndicator(_memoryHasValue ? "M" : "");
        }*/
        private void UpdateMemoryButtonsState()
        {
            // Disable buttons when memory is empty
            MemoryClearButton.IsEnabled = _memoryHasValue;
            MemoryRecallButton.IsEnabled = _memoryHasValue;
            MemoryRecallClearButton.IsEnabled = _memoryHasValue;
        }
    }
}

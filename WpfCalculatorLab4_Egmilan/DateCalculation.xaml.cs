using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for DateCalculation.xaml
    /// </summary>
    public partial class DateCalculation : Page
    {
        public DateCalculation()
        {
            InitializeComponent();
            this.Loaded += DateCalculation_Loaded;

            // Add these event handlers
            BaseDatePicker.SelectedDateChanged += (s, e) => UpdateDateCalculation();
            YearsList.SelectionChanged += (s, e) => UpdateDateCalculation();
            MonthsList.SelectionChanged += (s, e) => UpdateDateCalculation();
            DaysList.SelectionChanged += (s, e) => UpdateDateCalculation();
            AddRadio.Checked += (s, e) => UpdateDateCalculation();
            SubtractRadio.Checked += (s, e) => UpdateDateCalculation();
        }

        private void DateCalculation_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set current date as default
                BaseDatePicker.SelectedDate = DateTime.Today;

                // Initialize number selectors
                InitializeNumberSelectors();

                // Set default operation to Add
                AddRadio.IsChecked = true;

                // Calculate immediately to show initial result
                UpdateDateCalculation();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initialization error: {ex.Message}");
            }
        }
        private void NumberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateAddSubtract();
        }

        private void OperationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CalculateAddSubtract();
        }

        private void BaseDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateAddSubtract();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // First verify all controls exist
            if (DifferencePanel == null || AddSubtractPanel == null || ModeComboBox == null)
            {
                Debug.WriteLine("Error: Required controls not initialized");
                return;
            }

            // Now safely update visibility
            if (ModeComboBox.SelectedIndex == 0) // Difference mode
            {
                DifferencePanel.Visibility = Visibility.Visible;
                AddSubtractPanel.Visibility = Visibility.Collapsed;
                CalculateDifference();
            }
            else // Add/Subtract mode
            {
                DifferencePanel.Visibility = Visibility.Collapsed;
                AddSubtractPanel.Visibility = Visibility.Visible;
                CalculateAddSubtract();
            }
        }


        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModeComboBox.SelectedIndex == 0)
                CalculateDifference();
            else
                CalculateAddSubtract();
        }

        private void CalculateDifference()
        {
            // First check if controls exist
            if (FromDatePicker == null || ToDatePicker == null || ResultTextBox == null)
            {
                Debug.WriteLine("Error: Required controls not initialized");
                return;
            }

            // Then check for selected dates
            if (FromDatePicker.SelectedDate == null || ToDatePicker.SelectedDate == null)
            {
                ResultTextBox.Text = "Please select both dates";
                return;
            }

            try
            {
                DateTime fromDate = FromDatePicker.SelectedDate.Value;
                DateTime toDate = ToDatePicker.SelectedDate.Value;

                if (fromDate == toDate)
                {
                    ResultTextBox.Text = "Same dates";
                    return;
                }

                TimeSpan difference = toDate > fromDate ? toDate - fromDate : fromDate - toDate;
                ResultTextBox.Text = FormatTimeSpan(difference);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Calculation error: {ex.Message}");
                ResultTextBox.Text = "Error calculating difference";
            }
        }


        private void CalculateAddSubtract()
        {
            // First verify all controls exist
            if (BaseDatePicker == null || YearsList == null || MonthsList == null || DaysList == null || ResultTextBox == null)
            {
                Debug.WriteLine("Error: Required controls not initialized");
                return;
            }

            // Verify base date is selected
            if (BaseDatePicker.SelectedDate == null)
            {
                ResultTextBox.Text = "Please select a base date";
                return;
            }

            // Get selected values from lists
            string yearsValue = YearsList.SelectedItem?.ToString() ?? "0";
            string monthsValue = MonthsList.SelectedItem?.ToString() ?? "0";
            string daysValue = DaysList.SelectedItem?.ToString() ?? "0";

            // Try to parse values
            if (!int.TryParse(yearsValue, out int years) ||
                !int.TryParse(monthsValue, out int months) ||
                !int.TryParse(daysValue, out int days))
            {
                ResultTextBox.Text = "Please select valid numbers";
                return;
            }

            try
            {
                DateTime result = BaseDatePicker.SelectedDate.Value;
                if (AddRadio?.IsChecked == true)
                    result = result.AddYears(years).AddMonths(months).AddDays(days);
                else if (SubtractRadio?.IsChecked == true)
                    result = result.AddYears(-years).AddMonths(-months).AddDays(-days);
                else
                {
                    ResultTextBox.Text = "Please select Add or Subtract";
                    return;
                }

                ResultTextBox.Text = result.ToString("dddd, d MMMM yyyy");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Date calculation error: {ex.Message}");
                ResultTextBox.Text = "Invalid date operation";
            }
        }

        // Helper method to get selected value from ListBox
        private string GetSelectedValue(ListBox listBox)
        {
            if (listBox?.SelectedItem == null) return "0";

            // Depending on how your ListBox items are defined, you might need to adjust this
            if (listBox.SelectedItem is string strValue)
                return strValue;

            if (listBox.SelectedItem is ListBoxItem item)
                return item.Content?.ToString() ?? "0";

            return listBox.SelectedItem.ToString() ?? "0";
        }

        private string FormatTimeSpan(TimeSpan span)
        {
            if (span.TotalDays >= 365)
            {
                int years = (int)(span.TotalDays / 365);
                int days = (int)(span.TotalDays % 365);
                return $"{years} year{(years != 1 ? "s" : "")}, {days} day{(days != 1 ? "s" : "")}";
            }
            if (span.TotalDays >= 1)
                return $"{(int)span.TotalDays} day{((int)span.TotalDays != 1 ? "s" : "")}";
            if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours} hour{((int)span.TotalHours != 1 ? "s" : "")}";

            return $"{(int)span.TotalMinutes} minute{((int)span.TotalMinutes != 1 ? "s" : "")}";
        }

        // Input validation for numbers only
        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void InitializeNumberSelectors()
        {
            // Years (0-100)
            YearsList.ItemsSource = Enumerable.Range(0, 101).ToList();

            // Months (0-12)
            MonthsList.ItemsSource = Enumerable.Range(0, 13).ToList();

            // Days (0-31)
            DaysList.ItemsSource = Enumerable.Range(0, 32).ToList();

            // Set default selections to 0
            YearsList.SelectedIndex = 0;
            MonthsList.SelectedIndex = 0;
            DaysList.SelectedIndex = 0;
        }

        private void NumberSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem != null)
            {
                // Smooth scroll to center the selected item
                listBox.ScrollIntoView(listBox.SelectedItem);

                // Update the calculation
                UpdateDateCalculation();
            }
        }

        private void UpdateDateCalculation()
        {
            // Verify controls exist
            if (BaseDatePicker == null || YearsList == null ||
                MonthsList == null || DaysList == null || ResultTextBox == null)
                return;

            // Get base date (default to today if null)
            DateTime baseDate = BaseDatePicker.SelectedDate ?? DateTime.Today;

            // Get selected values (default to 0 if null)
            int years = YearsList.SelectedItem as int? ?? 0;
            int months = MonthsList.SelectedItem as int? ?? 0;
            int days = DaysList.SelectedItem as int? ?? 0;

            try
            {
                DateTime result = baseDate;

                if (AddRadio.IsChecked == true)
                {
                    result = result.AddYears(years)
                                  .AddMonths(months)
                                  .AddDays(days);
                }
                else if (SubtractRadio.IsChecked == true)
                {
                    result = result.AddYears(-years)
                                  .AddMonths(-months)
                                  .AddDays(-days);
                }

                // Update the result immediately
                ResultTextBox.Text = result.ToString("dddd, d MMMM yyyy");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Calculation error: {ex.Message}");
                ResultTextBox.Text = "Invalid date operation";
            }
        }

    }
}

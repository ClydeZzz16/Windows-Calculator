using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    /// Interaction logic for Standard.xaml
    /// </summary>
    public partial class Standard : Page
    {
        private string _currentInput = "";
        private string _currentOperation = "";
        private double _firstNumber = 0;
        private bool _isNewInput = true;
        private StringBuilder _historyBuilder = new StringBuilder();

        public Standard()
        {
            InitializeComponent();
        }

        private MainWindow MainWindow => Window.GetWindow(this) as MainWindow;

        private string FormatNumberWithCommas(string numberText)
        {
            if (string.IsNullOrEmpty(numberText))
                return "0";

            // Special case: if user just pressed decimal (like "0.")
            if (numberText.EndsWith("."))
                return numberText;

            bool isNegative = numberText.StartsWith("-");
            string workingText = isNegative ? numberText.Substring(1) : numberText;

            if (double.TryParse(workingText, out double number))
            {
                // Format with commas but preserve decimals
                string formatted = number.ToString("#,##0.##########");

                // If we had decimals but formatting removed them (like 5.0 → 5)
                if (numberText.Contains('.') && !formatted.Contains('.'))
                {
                    formatted += ".";
                }

                if (formatted == "")
                    formatted = "0";

                return isNegative ? "-" + formatted : formatted;
            }

            return numberText;
        }

        private void UpdateDisplay()
        {
            string displayText = string.IsNullOrEmpty(_currentInput) ? "0" : FormatNumberWithCommas(_currentInput);
            MainWindow?.UpdateStandardDisplay(displayText);
        }

        private void UpdateHistoryDisplay()
        {
            string historyText = _historyBuilder.ToString();

            // Format all numbers in the history with commas
            var formattedHistory = System.Text.RegularExpressions.Regex.Replace(
                historyText,
                @"\d+\.?\d*",
                match => FormatNumberWithCommas(match.Value)
            );

            MainWindow?.UpdateHistoryDisplay(formattedHistory);
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (_isNewInput)
                {
                    _currentInput = "";
                    _isNewInput = false;

                    // If we're starting a new input after an operation, clear the history
                    if (!string.IsNullOrEmpty(_currentOperation))
                    {
                        _historyBuilder.Clear();
                        UpdateHistoryDisplay();
                    }
                }

                // Prevent multiple leading zeros
                if (button.Content.ToString() == "0" && _currentInput == "0")
                    return;

                if (_currentInput == "0")
                    _currentInput = button.Content.ToString();
                else
                    _currentInput += button.Content.ToString();

                UpdateDisplay();

                // Update history to show the current input as it's being typed
                if (!string.IsNullOrEmpty(_currentOperation))
                {
                    _historyBuilder.Clear();
                    _historyBuilder.Append(FormatNumberWithCommas(_firstNumber.ToString()) + " " + _currentOperation + " " + FormatNumberWithCommas(_currentInput));
                    UpdateHistoryDisplay();
                }
            }
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && !string.IsNullOrEmpty(_currentInput))
            {
                // Add current input and operation to history
                _historyBuilder.Clear();
                _historyBuilder.Append(FormatNumberWithCommas(_currentInput) + " " + button.Content + " ");

                if (!string.IsNullOrEmpty(_currentOperation))
                    CalculateResult();

                _firstNumber = double.Parse(_currentInput);
                _currentOperation = button.Content.ToString();
                _isNewInput = true;
                UpdateHistoryDisplay();
            }
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentOperation))
            {
                // Add the second number to history before calculating
                _historyBuilder.Clear();
                _historyBuilder.Append(FormatNumberWithCommas(_firstNumber.ToString()) + " " + _currentOperation + " " + FormatNumberWithCommas(_currentInput) + " = ");

                CalculateResult();
                _currentOperation = "";
                UpdateHistoryDisplay();
            }
        }

        private void CalculateResult()
        {
            if (double.TryParse(_currentInput, out double secondNumber))
            {
                double result = 0;
                bool error = false;

                switch (_currentOperation)
                {
                    case "+":
                        result = _firstNumber + secondNumber;
                        break;
                    case "-":
                        result = _firstNumber - secondNumber;
                        break;
                    case "×":
                        result = _firstNumber * secondNumber;
                        break;
                    case "÷":
                        if (secondNumber != 0)
                            result = _firstNumber / secondNumber;
                        else
                            error = true;
                        break;
                }

                if (!error)
                {
                    _currentInput = result.ToString();
                    // Add result to history
                    _historyBuilder.Append(FormatNumberWithCommas(_currentInput));
                }
                else
                {
                    MessageBox.Show("Cannot divide by zero");
                    ClearAll();
                }

                UpdateDisplay();
                _isNewInput = true;
            }
        }

        private void ClearAll()
        {
            _currentInput = "0";
            _currentOperation = "";
            _firstNumber = 0;
            _isNewInput = true;
            _historyBuilder.Clear();
            UpdateDisplay();
            UpdateHistoryDisplay();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearAll();

        private void ClearEntryButton_Click(object sender, RoutedEventArgs e)
        {
            _currentInput = "0";
            _isNewInput = true;
            UpdateDisplay();

            // Update history to reflect the cleared entry
            if (!string.IsNullOrEmpty(_currentOperation))
            {
                _historyBuilder.Clear();
                _historyBuilder.Append(FormatNumberWithCommas(_firstNumber.ToString()) + " " + _currentOperation);
                UpdateHistoryDisplay();
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentInput.Length > 0)
            {
                _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                if (_currentInput.Length == 0 || _currentInput == "-")
                    _currentInput = "0";
                UpdateDisplay();

                // Update history to reflect the backspace
                if (!string.IsNullOrEmpty(_currentOperation))
                {
                    _historyBuilder.Clear();
                    _historyBuilder.Append(FormatNumberWithCommas(_firstNumber.ToString()) + " " + _currentOperation + " " + FormatNumberWithCommas(_currentInput));
                    UpdateHistoryDisplay();
                }
            }
        }

        private void DecimalButton_Click(object sender, RoutedEventArgs e)
        {
            // If starting fresh or after an operation
            if (_isNewInput || string.IsNullOrEmpty(_currentInput))
            {
                _currentInput = "0.";
                _isNewInput = false;
            }
            // Only add decimal if one doesn't exist
            else if (!_currentInput.Contains('.'))
            {
                _currentInput += ".";
            }

            UpdateDisplay();

            // Update history if in middle of operation
            if (!string.IsNullOrEmpty(_currentOperation))
            {
                _historyBuilder.Clear();
                _historyBuilder.Append($"{FormatNumberWithCommas(_firstNumber.ToString())} {_currentOperation} {FormatNumberWithCommas(_currentInput)}");
                UpdateHistoryDisplay();
            }
        }

        private void ToggleSignButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentInput) && _currentInput != "0")
            {
                if (_currentInput.StartsWith("-"))
                    _currentInput = _currentInput.Substring(1);
                else
                    _currentInput = "-" + _currentInput;
                UpdateDisplay();

                // Update history to reflect the sign change
                if (!string.IsNullOrEmpty(_currentOperation))
                {
                    _historyBuilder.Clear();
                    _historyBuilder.Append(FormatNumberWithCommas(_firstNumber.ToString()) + " " + _currentOperation + " " + FormatNumberWithCommas(_currentInput));
                    UpdateHistoryDisplay();
                }
            }
        }

        private void SquareRootButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(_currentInput, out double number) && number >= 0)
            {
                _historyBuilder.Clear();
                _historyBuilder.AppendLine($"√({FormatNumberWithCommas(_currentInput)}) = {FormatNumberWithCommas(Math.Sqrt(number).ToString())}");
                _currentInput = Math.Sqrt(number).ToString();
                UpdateDisplay();
                UpdateHistoryDisplay();
                _isNewInput = true;
            }
            else
            {
                MessageBox.Show("Invalid input for square root");
            }
        }

        private void SquareButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(_currentInput, out double number))
            {
                _historyBuilder.Clear();
                _historyBuilder.AppendLine($"sqr({FormatNumberWithCommas(_currentInput)}) = {FormatNumberWithCommas((number * number).ToString())}");
                _currentInput = (number * number).ToString();
                UpdateDisplay();
                UpdateHistoryDisplay();
                _isNewInput = true;
            }
        }

        private void ReciprocalButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(_currentInput, out double number) && number != 0)
            {
                _historyBuilder.Clear();
                _historyBuilder.AppendLine($"1/({FormatNumberWithCommas(_currentInput)}) = {FormatNumberWithCommas((1 / number).ToString())}");
                _currentInput = (1 / number).ToString();
                UpdateDisplay();
                UpdateHistoryDisplay();
                _isNewInput = true;
            }
            else
            {
                MessageBox.Show("Cannot divide by zero");
            }
        }

        private void PercentButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(_currentInput, out double number))
            {
                _historyBuilder.Clear();
                _historyBuilder.AppendLine($"{FormatNumberWithCommas(_currentInput)}% = {FormatNumberWithCommas((number / 100).ToString())}");
                _currentInput = (number / 100).ToString();
                UpdateDisplay();
                UpdateHistoryDisplay();
                _isNewInput = true;
            }
        }
    }
}

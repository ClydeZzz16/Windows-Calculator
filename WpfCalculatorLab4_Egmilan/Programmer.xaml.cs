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
    /// Interaction logic for Programmer.xaml
    /// </summary>
    public partial class Programmer : Page
    {
        private string _currentInput = "0";
        private string _currentOperation = "";
        private long _firstNumber = 0;
        private bool _isNewInput = true;
        private StringBuilder _historyBuilder = new StringBuilder();
        private NumberBase _currentBase = NumberBase.Decimal;
        private WordSize _currentWordSize = WordSize.QWORD;
        private enum BitwiseOperation { AND, OR, XOR, NOT, NAND, NOR, XNOR }
        private enum BitShiftOperation { Left, Right, LeftCircular, RightCircular }
        private BitwiseOperation? _currentBitwiseOp = null;
        private BitShiftOperation? _currentBitShiftOp = null;
        private long _firstOperand = 0;
        private bool _waitingForSecondOperand = false;

        public Programmer()
        {
            InitializeComponent();
            UpdateBaseButtonStyles();
            UpdateDigitButtonStates();
            UpdateNumberBaseDisplays();
            UpdateMemoryButtonsState();
        }

        private ProgrammerWindow MainWindow => Window.GetWindow(this) as ProgrammerWindow;

        private enum NumberBase { Hexadecimal, Decimal, Octal, Binary }
        private enum WordSize { QWORD, DWORD, WORD, BYTE }

        private void UpdateDisplay()
        {
            string displayText = string.IsNullOrEmpty(_currentInput) ? "0" : FormatNumberWithCommas(_currentInput);
            MainWindow?.UpdateProgrammerDisplay(displayText);
            UpdateNumberBaseDisplays();
        }

        private void UpdateNumberBaseDisplays()
        {
            if (double.TryParse(_currentInput, out double number))
            {
                Dispatcher.Invoke(() =>
                {
                    // For hexadecimal display of floating-point, show integer part only
                    HexTextBox.Text = Convert.ToString((long)number, 16).ToUpper();
                    DecTextBox.Text = FormatNumberWithCommas(number.ToString());
                    OctTextBox.Text = Convert.ToString((long)number, 8);
                    BinTextBox.Text = Convert.ToString((long)number, 2);
                });
            }
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

        private string FormatNumberWithCommas(string numberText)
        {
            if (string.IsNullOrEmpty(numberText))
                return "0";

            // Handle negative numbers
            bool isNegative = numberText.StartsWith("-");
            string workingText = isNegative ? numberText.Substring(1) : numberText;

            // Check if it's a valid number
            if (double.TryParse(workingText, out double number))
            {
                // Format with commas and handle decimals
                string formatted = number.ToString("#,##0.##########");

                // Special cases:
                if (formatted.EndsWith(".0"))
                    formatted = formatted.Substring(0, formatted.Length - 2);
                if (formatted == "")
                    formatted = "0";

                return isNegative ? "-" + formatted : formatted;
            }

            return numberText; // Return original if not a number
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string digit = button.Content.ToString();

                if (_isNewInput)
                {
                    _currentInput = "";
                    _isNewInput = false;
                }

                // Validate input based on current number base
                if (_currentBase == NumberBase.Binary && digit != "0" && digit != "1")
                    return;
                if (_currentBase == NumberBase.Octal && int.Parse(digit) > 7)
                    return;
                if (_currentBase == NumberBase.Hexadecimal && !IsValidHexDigit(digit))
                    return;

                // Remove commas for internal storage
                string cleanInput = _currentInput.Replace(",", "");

                if (cleanInput == "0")
                    cleanInput = digit;
                else
                    cleanInput += digit;

                _currentInput = cleanInput;
                UpdateDisplay();
            }
        }

        private bool IsValidHexDigit(string digit)
        {
            return int.TryParse(digit, out _) || "ABCDEF".Contains(digit.ToUpper());
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && !string.IsNullOrEmpty(_currentInput))
            {
                // Convert current input to proper number first
                long currentNumber = Convert.ToInt64(_currentInput.Replace(",", ""), GetBaseValue());

                if (!string.IsNullOrEmpty(_currentOperation))
                {
                    CalculateResult();
                }

                _firstNumber = currentNumber;
                _currentOperation = button.Content.ToString();
                _isNewInput = true;

                _historyBuilder.Append($"{FormatNumberWithCommas(_currentInput)} {_currentOperation} ");
                UpdateHistoryDisplay();
            }
        }

        private int GetBaseValue()
        {
            return _currentBase switch
            {
                NumberBase.Hexadecimal => 16,
                NumberBase.Decimal => 10,
                NumberBase.Octal => 8,
                NumberBase.Binary => 2,
                _ => 10
            };
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_waitingForSecondOperand && long.TryParse(_currentInput, out long secondOperand))
            {
                long result = 0;

                if (_currentBitwiseOp.HasValue)
                {
                    result = PerformBitwiseOperation(_firstOperand, secondOperand, _currentBitwiseOp.Value);
                    _historyBuilder.AppendLine($"{_firstOperand} {_currentBitwiseOp} {secondOperand} = {result}");
                }
                else if (_currentBitShiftOp.HasValue)
                {
                    result = PerformBitShiftOperation(_firstOperand, secondOperand, _currentBitShiftOp.Value);
                    _historyBuilder.AppendLine($"{_firstOperand} {_currentBitShiftOp} {secondOperand} = {result}");
                }

                _currentInput = result.ToString();
                _currentBitwiseOp = null;
                _currentBitShiftOp = null;
                _waitingForSecondOperand = false;
                UpdateDisplay();
                UpdateHistoryDisplay();
            }

            if (!string.IsNullOrEmpty(_currentOperation))
            {
                _historyBuilder.Append($"{FormatNumberWithCommas(_currentInput)} = ");

                // Store the second number before calculation
                string secondNumber = _currentInput;

                CalculateResult();
                _currentOperation = "";

                // Update history with the correct result
                _historyBuilder.AppendLine(FormatNumberWithCommas(_currentInput));
                UpdateHistoryDisplay();
            }
        }

        private void CalculateResult()
        {
            try
            {
                long secondNumber = Convert.ToInt64(_currentInput.Replace(",", ""), GetBaseValue());
                long result = 0;

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
                        result = _firstNumber / secondNumber;
                        break;
                    default:
                        return;
                }

                // Apply word size limitation
                result = ApplyWordSize(result);

                // Convert result back to current base
                _currentInput = Convert.ToString(result, GetBaseValue());
                _historyBuilder.AppendLine(FormatNumberWithCommas(_currentInput));
                UpdateDisplay();
                _isNewInput = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Calculation error: {ex.Message}");
                ClearAll();
            }
        }

        private string EvaluateSubExpression(string subExpr)
        {
            if (string.IsNullOrWhiteSpace(subExpr))
            {
                return "0"; // Return 0 for empty parentheses
            }

            try
            {
                // First try to evaluate as a simple number
                if (long.TryParse(subExpr, out long simpleNumber))
                {
                    return simpleNumber.ToString();
                }

                // Handle basic operations
                var parts = subExpr.Split(new[] { '+', '-', '×', '÷' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    throw new InvalidOperationException("Invalid expression format");
                }

                long left = Convert.ToInt64(parts[0].Trim(), GetBaseValue());
                long right = Convert.ToInt64(parts[1].Trim(), GetBaseValue());
                char op = subExpr.First(c => "+-×÷".Contains(c));

                return op switch
                {
                    '+' => (left + right).ToString(),
                    '-' => (left - right).ToString(),
                    '×' => (left * right).ToString(),
                    '÷' when right != 0 => (left / right).ToString(),
                    '÷' => throw new DivideByZeroException(),
                    _ => throw new InvalidOperationException("Unknown operator")
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error evaluating sub-expression '{subExpr}': {ex.Message}");
                return "0"; // Fallback to 0 on errors
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
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentInput.Length > 0)
            {
                _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                if (_currentInput.Length == 0)
                    _currentInput = "0";
                UpdateDisplay();
            }
        }

        // Number base selection handlers
        private void HexButton_Click(object sender, RoutedEventArgs e)
        {
            _currentBase = NumberBase.Hexadecimal;
            UpdateBaseButtonStyles();
            UpdateDigitButtonStates();
            UpdateDisplay();
        }

        private void DecButton_Click(object sender, RoutedEventArgs e)
        {
            _currentBase = NumberBase.Decimal;
            UpdateBaseButtonStyles();
            UpdateDigitButtonStates();
            UpdateDisplay();
        }

        private void OctButton_Click(object sender, RoutedEventArgs e)
        {
            _currentBase = NumberBase.Octal;
            UpdateBaseButtonStyles();
            UpdateDigitButtonStates();
            UpdateDisplay();
        }

        private void BinButton_Click(object sender, RoutedEventArgs e)
        {
            _currentBase = NumberBase.Binary;
            UpdateBaseButtonStyles();
            UpdateDigitButtonStates();
            UpdateDisplay();
        }

        private void ReciprocalButton_Click(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(_currentInput, out long number) && number != 0)
            {
                // For programmer calculator, we'll do integer reciprocal (1/x)
                _historyBuilder.AppendLine($"1/({_currentInput})");
                _currentInput = (1.0 / number).ToString();
                UpdateDisplay();
                _isNewInput = true;
            }
            else
            {
                MessageBox.Show("Cannot divide by zero");
            }
        }

        private void PercentButton_Click(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(_currentInput, out long number))
            {
                _historyBuilder.AppendLine($"{_currentInput}%");
                _currentInput = (number / 100.0).ToString();
                UpdateDisplay();
                _isNewInput = true;
            }
        }

        private void OpenParenthesis_Click(object sender, RoutedEventArgs e)
        {
            if (_isNewInput)
            {
                _currentInput = "";
                _isNewInput = false;
            }
            _currentInput += "(";
            UpdateDisplay();
        }

        private void CloseParenthesis_Click(object sender, RoutedEventArgs e)
        {
            if (!_isNewInput && _currentInput.Contains("("))
            {
                _currentInput += ")";
                UpdateDisplay();
            }
        }

        // Bitwise operation handlers
        private void BitwiseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BitwiseComboBox.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                string operation = item.Content.ToString().ToLower();
                if (operation != "bitwise" && !string.IsNullOrEmpty(_currentInput))
                {
                    _currentOperation = operation;
                    _firstNumber = Convert.ToInt64(_currentInput, GetBaseValue());
                    _isNewInput = true;
                }
            }
        }

        private long ApplyWordSize(long value)
        {
            return _currentWordSize switch
            {
                WordSize.BYTE => (byte)value,
                WordSize.WORD => (ushort)value,
                WordSize.DWORD => (uint)value,
                _ => value // QWORD
            };
        }

        private bool IsValidInput(string input)
        {
            try
            {
                Convert.ToInt64(input.Replace(",", ""), GetBaseValue());
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Word size selection
        private void QwordButton_Click(object sender, RoutedEventArgs e)
        {
            _currentWordSize = WordSize.QWORD;
            // Apply word size limitation if needed
        }

        // Other word size buttons would follow the same pattern

        // Memory operations
        private double _memoryValue = 0;
        private bool _memoryHasValue = false;
        private List<double> _memoryHistory = new List<double>();

        private ProgrammerWindow GetProgrammerWindow()
        {
            return Window.GetWindow(this) as ProgrammerWindow;
        }

        #region Memory Functions

        // Memory Store (MS)
        private void MemoryStoreButton_Click(object sender, RoutedEventArgs e)
        {
            var progWindow = GetProgrammerWindow();
            if (progWindow != null && double.TryParse(progWindow.GetDisplayValue(), out double currentValue))
            {
                _memoryValue = currentValue;
                _memoryHasValue = true;
                _memoryHistory.Add(_memoryValue);
                UpdateMemoryButtonsState();

                // Visual feedback
                progWindow.SetDisplayValue("Stored");
                Task.Delay(500).ContinueWith(_ =>
                {
                    progWindow.SetDisplayValue(currentValue.ToString());
                }, TaskScheduler.FromCurrentSynchronizationContext());
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

        private void UpdateMemoryButtonsState()
        {
            // Enable/disable buttons based on whether we have a stored value
            var msButton = this.FindName("MSButton") as Button;
            var mvButton = this.FindName("MvButton") as Button;

            if (msButton != null) msButton.IsEnabled = true; // MS is always enabled
            if (mvButton != null) mvButton.IsEnabled = _memoryHasValue;
        }

        private void UpdateBaseButtonStyles()
        {
            HexButton.Style = _currentBase == NumberBase.Hexadecimal ?
                (Style)FindResource("ActiveBaseButtonStyle") :
                (Style)FindResource("BaseButtonStyle");

            DecButton.Style = _currentBase == NumberBase.Decimal ?
                (Style)FindResource("ActiveBaseButtonStyle") :
                (Style)FindResource("BaseButtonStyle");

            OctButton.Style = _currentBase == NumberBase.Octal ?
                (Style)FindResource("ActiveBaseButtonStyle") :
                (Style)FindResource("BaseButtonStyle");

            BinButton.Style = _currentBase == NumberBase.Binary ?
                (Style)FindResource("ActiveBaseButtonStyle") :
                (Style)FindResource("BaseButtonStyle");
        }

        private void UpdateDigitButtonStates()
        {
            // Get all digit buttons (0-9, A-F)
            var digitButtons = new[] { Btn0, Btn1, Btn2, Btn3, Btn4, Btn5, Btn6, Btn7,
                             Btn8, Btn9, BtnA, BtnB, BtnC, BtnD, BtnE, BtnF };

            foreach (var button in digitButtons)
            {
                bool shouldEnable = _currentBase switch
                {
                    NumberBase.Binary => button.Content.ToString() == "0" || button.Content.ToString() == "1",
                    NumberBase.Octal => int.TryParse(button.Content.ToString(), out int digit) && digit < 8,
                    NumberBase.Decimal => int.TryParse(button.Content.ToString(), out _),
                    NumberBase.Hexadecimal => true,
                    _ => true
                };

                button.IsEnabled = shouldEnable;
                button.Opacity = shouldEnable ? 1.0 : 0.5;
            }
        }

        private void BitwiseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string op = button.Content.ToString().ToUpper();

                if (long.TryParse(_currentInput, out long number))
                {
                    _firstOperand = number;
                    _currentInput = "0";
                    _waitingForSecondOperand = true;

                    _currentBitwiseOp = op switch
                    {
                        "AND" => BitwiseOperation.AND,
                        "OR" => BitwiseOperation.OR,
                        "XOR" => BitwiseOperation.XOR,
                        "NOT" => BitwiseOperation.NOT,
                        "NAND" => BitwiseOperation.NAND,
                        "NOR" => BitwiseOperation.NOR,
                        "XNOR" => BitwiseOperation.XNOR,
                        _ => null
                    };

                    UpdateDisplay();
                }
            }
        }

        private long PerformBitwiseOperation(long a, long b, BitwiseOperation op)
        {
            return op switch
            {
                BitwiseOperation.AND => a & b,
                BitwiseOperation.OR => a | b,
                BitwiseOperation.XOR => a ^ b,
                BitwiseOperation.NOT => ~a,
                BitwiseOperation.NAND => ~(a & b),
                BitwiseOperation.NOR => ~(a | b),
                BitwiseOperation.XNOR => ~(a ^ b),
                _ => a
            };
        }

        private void BitShiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string op = button.Content.ToString().ToUpper();

                if (long.TryParse(_currentInput, out long number))
                {
                    _firstOperand = number;
                    _currentInput = "0";
                    _waitingForSecondOperand = true;

                    _currentBitShiftOp = op switch
                    {
                        "<<" => BitShiftOperation.Left,
                        ">>" => BitShiftOperation.Right,
                        "ROL" => BitShiftOperation.LeftCircular,
                        "ROR" => BitShiftOperation.RightCircular,
                        _ => null
                    };

                    UpdateDisplay();
                }
            }
        }

        private long PerformBitShiftOperation(long value, long shift, BitShiftOperation op)
        {
            shift = shift % (int)_currentWordSize; // Prevent over-shifting

            return op switch
            {
                BitShiftOperation.Left => value << (int)shift,
                BitShiftOperation.Right => value >> (int)shift,
                BitShiftOperation.LeftCircular => (value << (int)shift) | (value >> (int)((int)_currentWordSize - shift)),
                BitShiftOperation.RightCircular => (value >> (int)shift) | (value << (int)((int)_currentWordSize - shift)),
                _ => value
            };
        }

        /*private enum WordSize { QWORD = 64, DWORD = 32, WORD = 16, BYTE = 8 }
        private WordSize _currentWordSize = WordSize.QWORD;

        private void UpdateWordSizeMask()
        {
            // Apply word size limitation to all operations
            long mask = (1L << (int)_currentWordSize) - 1;
            _firstOperand &= mask;
            _currentInput = (long.Parse(_currentInput) & mask).ToString();
        }*/
        #endregion
    }
}

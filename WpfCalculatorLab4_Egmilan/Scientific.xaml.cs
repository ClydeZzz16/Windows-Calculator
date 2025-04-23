using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for Scientific.xaml
    /// </summary>
   // Window ScientificWindow;
    public partial class Scientific : Page
    {
        private string _input = "";
        private string _currentExpression = "";
        private bool _expectingOperator = false;
        private bool _isSecondFunctionActive = false;
        private int _parenthesisCount = 0;
        public enum AngleMode { Degrees, Radians, Gradians }
        private AngleMode _angleMode = AngleMode.Degrees;
        private bool _scientificNotation = false;
        // Operator precedence and associativity
        private readonly Dictionary<string, int> _precedence = new()
        {
            ["+"] = 1,
            ["-"] = 1,
            ["×"] = 2,
            ["÷"] = 2,
            ["mod"] = 2,
            ["^"] = 3,
            ["2ˣ"] = 3,
            ["xʸ"] = 3,
            ["ʸ√x"] = 3,
            ["yroot"] = 3,
            ["!"] = 4
        };

        private readonly HashSet<string> _rightAssociative = new() { "^", "xʸ", "yroot" };
        //private readonly HashSet<string> _rightAssociative = new() { "^", "xʸ", "yroot" };
        private readonly HashSet<string> _functions = new()
        {
            "sin", "cos", "tan", "asin", "acos", "atan",
            "csc", "sec", "cot", "log", "ln", "exp",
            "sqrt", "∛x", "ʸ√x", "abs", "floor", "ceil", "rand", "dms", "deg",
            "x²", "x³", "10ˣ", "eˣ", "2ˣ","log_y", "ʸ√x"
        };

        public Scientific()
        {
            InitializeComponent();
        }

        private ScientificWindow ParentWindow => Window.GetWindow(this) as ScientificWindow;

        private void UpdateDisplay()
        {
            // Prevent multiple decimals
            if (_input.Count(c => c == '.') > 1)
            {
                _input = _input.Substring(0, _input.LastIndexOf('.'));
            }

            string displayText = string.IsNullOrEmpty(_input) ? "0" : FormatNumberWithCommas(_input);
            ParentWindow?.UpdateScientificDisplay(displayText);
        }

        private void UpdateExpressionDisplay()
        {
            string expression = _currentExpression;
            if (!string.IsNullOrEmpty(_input))
            {
                expression += " " + _input;
            }
            ParentWindow?.UpdateHistoryDisplay(expression.Trim());
        }

        #region Core Calculation Methods

        private List<string> Tokenize(string expression)
        {
            // First remove any commas from the expression for parsing
            expression = expression.Replace(",", "")
                                 .Replace("π", Math.PI.ToString())
                                 .Replace("e", Math.E.ToString())
                                 .Replace("2ˣ", "2^")
                                 .Replace("ʸ√x", "yroot");
            expression = Regex.Replace(expression, @"2\^(\d+)", "2^($1)");
            expression = expression.Replace("abs(", "abs ( ");
            expression = expression.Replace("(", " ( ").Replace(")", " ) ");


            // Handle implied multiplication (e.g., 3(4+5) → 3×(4+5))
            expression = Regex.Replace(expression, @"(\d)(\()", "$1 × $2");
            // Rest of your tokenization logic...
            var pattern = @"(\d+\.?\d*|\.\d+|[()+\-×÷^!]|mod|sin|cos|tan|asin|acos|atan|csc|sec|cot|log|ln|exp|sqrt|∛x|ʸ√x|abs|floor|ceil|x²|x³|10ˣ|eˣ|2ˣ)";
            return Regex.Matches(expression, pattern)
                      .Select(m => m.Value)
                      .ToList();
        }

        private List<string> ToPostfix(List<string> tokens)
        {
            var output = new List<string>();
            var stack = new Stack<string>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out _) || token == "π" || token == "e")
                {
                    output.Add(token);
                }
                else if (_functions.Contains(token))
                {
                    stack.Push(token);
                }
                else if (IsOperator(token))
                {
                    while (stack.Count > 0 &&
                          (IsOperator(stack.Peek()) || _functions.Contains(stack.Peek())) &&
                          (_precedence[stack.Peek()] > _precedence[token] ||
                          (_precedence[stack.Peek()] == _precedence[token] && !_rightAssociative.Contains(token))))
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
                else if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                        output.Add(stack.Pop());

                    if (stack.Count == 0 || stack.Pop() != "(")
                        throw new ArgumentException("Mismatched parentheses");

                    if (stack.Count > 0 && _functions.Contains(stack.Peek()))
                        output.Add(stack.Pop());
                }
            }

            while (stack.Count > 0)
            {
                var top = stack.Pop();
                if (top == "(" || top == ")")
                    throw new ArgumentException("Mismatched parentheses");
                output.Add(top);
            }

            return output;
        }

        private double EvaluatePostfix(List<string> postfix)
        {
            var stack = new Stack<double>();

            foreach (var token in postfix)
            {
                if (double.TryParse(token, out double num))
                {
                    stack.Push(num);
                }
                else if (IsOperator(token))
                {
                    if (token == "!")
                    {
                        stack.Push(Factorial(stack.Pop()));
                    }
                    else
                    {
                        double b = stack.Pop();
                        double a = stack.Pop();
                        stack.Push(EvaluateOperator(token, a, b));
                    }
                }
                else if (_functions.Contains(token))
                {
                    stack.Push(EvaluateFunction(token, stack.Pop()));
                }
            }

            if (stack.Count != 1)
            {
                throw new InvalidOperationException("Invalid expression");
            }

            return stack.Pop();
        }

        private double EvaluateFunction(string func, double a)
        {
            return func switch
            {
                /*"sin" => Math.Sin(a),
                "cos" => Math.Cos(a),
                "tan" => Math.Tan(a),
                "asin" => Math.Asin(a),
                "acos" => Math.Acos(a),
                "atan" => Math.Atan(a),*/
                "sin" => Math.Sin(a * Math.PI / 180),    
                "cos" => Math.Cos(a * Math.PI / 180),    
                "tan" => Math.Tan(a * Math.PI / 180),    
                "asin" => Math.Asin(a) * (180 / Math.PI), 
                "acos" => Math.Acos(a) * (180 / Math.PI), 
                "atan" => Math.Atan(a) * (180 / Math.PI),
                                                          
                "csc" => 1.0 / Math.Sin(a * Math.PI / 180),
                "sec" => 1.0 / Math.Cos(a * Math.PI / 180),
                "cot" => 1.0 / Math.Tan(a * Math.PI / 180),
                //"sin" => Math.Sin(ConvertFromRadians(a)),
                //"cos" => Math.Cos(ConvertFromRadians(a)),
                //  "tan" => Math.Tan(ConvertFromRadians(a)),
                // "asin" => ConvertFromRadians(Math.Asin(a)),
                // "acos" => ConvertFromRadians(Math.Acos(a)),
                // "atan" => ConvertFromRadians(Math.Atan(a)),

                
                "log" => Math.Log10(a),
                "ln" => Math.Log(a),
                "exp" => Math.Exp(a),
                "sqrt" => Math.Sqrt(a),
                "∛x" => Math.Pow(a, 1.0 / 3.0),
                "ʸ√x" => throw new InvalidOperationException("ʸ√x requires two operands"),
                "abs" => Math.Abs(a),
                "floor" => Math.Floor(a),
                "ceil" => Math.Ceiling(a),
                "rand" => new Random().NextDouble(),
                "x²" => a * a,
                "x³" => a * a * a,
                "10ˣ" => Math.Pow(10, a),
                "eˣ" => Math.Exp(a),
                "2ˣ" => Math.Pow(2, a),
                "log_y" => Math.Log(a), // Base y will be the next operand
                                        //"2ˣ" => Math.Pow(2, a),
                                        // "eˣ" => Math.Exp(a),
                                        //"ʸ√x" => throw new InvalidOperationException("ʸ√x requires two operands"),

                _ => throw new InvalidOperationException($"Unknown function: {func}")
            };
        }

        private double EvaluateOperator(string op, double a, double b)
        {
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "×" => a * b,
                "÷" => a / b,
                "^" or "xʸ" => Math.Pow(a, b),
                "mod" => a % b,
                "yroot" => Math.Pow(b, 1.0 / a),
                //"2ˣ" => Math.Pow(2, a), // Should be correct
                "ʸ√x" => Math.Pow(a, 1.0 / b),
                _ => throw new InvalidOperationException($"Unknown operator: {op}")
            };
        }

        private double Factorial(double n)
        {
            if (n < 0 || n != Math.Floor(n))
                throw new ArgumentException("Factorial is only defined for non-negative integers");

            double result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }

        private bool IsOperator(string token) => _precedence.ContainsKey(token);

        #endregion

        #region Button Click Handlers

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (_expectingOperator)
                {
                    _input = "";
                    _expectingOperator = false;
                }

                // Handle leading zero
                if (_input == "0" && button.Content.ToString() != ".")
                {
                    _input = button.Content.ToString();
                }
                else
                {
                    _input += button.Content.ToString();
                }

                UpdateDisplay();
            }
        }


        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content != null)
            {
                string op = button.Content.ToString();
                if (op == "xʸ") op = "^";

                if (!string.IsNullOrEmpty(_input))
                {
                    _currentExpression += _input; // No space here!
                    _input = "";
                }
                _currentExpression += " " + op + " "; // Spaces ONLY around the operator
                _expectingOperator = true;
                UpdateExpressionDisplay();
            }
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_storedBaseForYRootX != null && !string.IsNullOrEmpty(_input))
                {
                    // If y√x operation is in progress, complete it
                    YRootXButton_Click(sender, e);
                    return;
                }

                if (_expectingLogYNumber && !string.IsNullOrEmpty(_input))
                {
                    // Complete the log operation if equals is pressed
                    LogYButton_Click(sender, e);
                    return;
                }

                if (!string.IsNullOrEmpty(_input))
                {
                    _currentExpression += _input;
                }

                var tokens = Tokenize(_currentExpression);
                var postfix = ToPostfix(tokens);
                double result = EvaluatePostfix(postfix);

                string historyEntry = $"{_currentExpression} = {FormatNumberWithCommas(result.ToString())}";
                ParentWindow?.UpdateHistoryDisplay(historyEntry);

                _input = result.ToString(); // Store unformatted for calculations
                _currentExpression = "";
                _expectingOperator = true;
                UpdateDisplay(); // This will apply comma formatting
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                ClearButton_Click(sender, e);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _input = "";
            _currentExpression = "";
            _expectingOperator = false;
            _parenthesisCount = 0;
            _logYBase = null;
            _expectingLogYNumber = false;
            ParentWindow?.ClearHistory();
            UpdateDisplay();
            UpdateExpressionDisplay();
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _input = _input.Substring(0, _input.Length - 1);
                UpdateDisplay();
            }
            else if (!string.IsNullOrEmpty(_currentExpression))
            {
                _currentExpression = _currentExpression.TrimEnd();
                _currentExpression = _currentExpression.Substring(0, _currentExpression.LastIndexOf(' ') + 1);
                UpdateExpressionDisplay();
            }
        }

        private string FormatNumberWithCommas(string numberText)
        {
            if (string.IsNullOrEmpty(numberText))
                return "0";

            // Special case: preserve "0." or similar partial inputs
            if (numberText.EndsWith(".") || (numberText.StartsWith("-") && numberText.Length == 2 && numberText.EndsWith(".")))
            {
                return numberText;
            }

            if (double.TryParse(numberText, out double number))
            {
                string formatted = number.ToString("#,##0.##########");
                return formatted;
            }
            return numberText;
        }

        private void DecimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (_expectingOperator)
            {
                // If starting new input after an operator
                _input = "0.";
                _expectingOperator = false;
            }
            else if (string.IsNullOrEmpty(_input))
            {
                // If no input yet, start with "0."
                _input = "0.";
            }
            else if (!_input.Contains("."))
            {
                // Only add decimal if not already present
                _input += ".";
            }
            // If decimal already exists, do nothing

            UpdateDisplay();
        }

        private void ToggleSignButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input) && double.TryParse(_input, out double num))
            {
                _input = (-num).ToString();
                UpdateDisplay();
            }
        }

        private void PiButton_Click(object sender, RoutedEventArgs e)
        {
            if (_expectingOperator)
            {
                _currentExpression += _input + " ";
                _input = "";
                _expectingOperator = false;
            }
            _input = "π";
            UpdateDisplay();
        }

        private void EButton_Click(object sender, RoutedEventArgs e)
        {
            if (_expectingOperator)
            {
                _currentExpression += _input + " ";
                _input = "";
                _expectingOperator = false;
            }
            _input = "e";
            UpdateDisplay();
        }

        private void OpenParenthesis_Click(object sender, RoutedEventArgs e)
        {
            if (_expectingOperator || string.IsNullOrEmpty(_currentExpression))
            {
                _currentExpression += "(";
                _parenthesisCount++;
                _expectingOperator = false;
            }
            else
            {
                _currentExpression += " × (";
                _parenthesisCount++;
            }
            UpdateExpressionDisplay();
        }

        private void CloseParenthesis_Click(object sender, RoutedEventArgs e)
        {
            if (_parenthesisCount > 0 && !_expectingOperator)
            {
                if (!string.IsNullOrEmpty(_input))
                {
                    _currentExpression += _input + ")";
                    _input = "";
                }
                else
                {
                    _currentExpression += ")";
                }
                _parenthesisCount--;
                UpdateExpressionDisplay();
            }
            else
            {
                // Show error in display
                _currentExpression += " [Error: Unmatched parenthesis]";
                UpdateExpressionDisplay();
            }
        }

        /*private void SecondButton_Click(object sender, RoutedEventArgs e)
        {
            if (Second2.Visibility == Visibility.Visible)
                Second2.Visibility = Visibility.Collapsed;
            else
                Second2.Visibility = Visibility.Visible;
            // Toggle visibility of the secondary functions panel
            if (Square.Visibility == Visibility.Visible)
                Square.Visibility = Visibility.Collapsed;
            else
                Square.Visibility = Visibility.Visible;

            if (SquareRoot.Visibility == Visibility.Visible)
                SquareRoot.Visibility = Visibility.Collapsed;
            else
                SquareRoot.Visibility = Visibility.Visible;

            if (Power.Visibility == Visibility.Visible)
                Power.Visibility = Visibility.Collapsed;
            else
                Power.Visibility = Visibility.Visible;

            if (PowerTen.Visibility == Visibility.Visible)
                PowerTen.Visibility = Visibility.Collapsed;
            else
                PowerTen.Visibility = Visibility.Visible;

            if (Log.Visibility == Visibility.Visible)
                Log.Visibility = Visibility.Collapsed;
            else
                Log.Visibility = Visibility.Visible;

            if (Natural.Visibility == Visibility.Visible)
                Natural.Visibility = Visibility.Collapsed;
            else
                Natural.Visibility = Visibility.Visible;

        }*/


        private void SecondButton_Click(object sender, RoutedEventArgs e)
        {
            _isSecondFunctionActive = !_isSecondFunctionActive;
            Second2.Visibility = _isSecondFunctionActive ? Visibility.Visible : Visibility.Collapsed;
            Square.Visibility = _isSecondFunctionActive ? Visibility.Collapsed : Visibility.Visible;
            SquareRoot.Visibility = _isSecondFunctionActive ? Visibility.Collapsed : Visibility.Visible;
            Power.Visibility = _isSecondFunctionActive ? Visibility.Collapsed : Visibility.Visible;
            PowerTen.Visibility = _isSecondFunctionActive ? Visibility.Collapsed : Visibility.Visible;
            Log.Visibility = _isSecondFunctionActive ? Visibility.Collapsed : Visibility.Visible;
            Natural.Visibility = _isSecondFunctionActive ? Visibility.Collapsed : Visibility.Visible;
        }

        #region Math Functions

        private void SquarePowerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += _input + " x² ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }

        private void CubePowerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += _input + " x³ ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }

        private void SquareRootButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += _input + " sqrt ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }

        private void CubeRootButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += _input + " ∛x ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }

        /*private void PowerOfTen_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += "10ˣ " + _input + " ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }*/

        private void PowerOfTen_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += $"10^{_input}";
                _input = "";
                UpdateExpressionDisplay();
            }
            else if (!string.IsNullOrEmpty(_currentExpression))
            {
                // Handle cases like "10ˣ("
                _currentExpression += "10^";
                UpdateExpressionDisplay();
            }
        }

        private void FactorialButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += _input + " ! ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += "log " + _input + " ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }

        private void NaturalLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += "ln " + _input + " ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }

        private void ExpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += "exp " + _input + " ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }

        private void ReciprocalButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += "1 ÷ " + _input + " ";
                _input = "";
                UpdateExpressionDisplay();
            }
        }

        private void AbsoluteValueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_input))
                {
                    double value = double.Parse(_input);
                    double result = Math.Abs(value);

                    string historyEntry = $"|{_input}| = {result}";
                    ParentWindow?.UpdateHistoryDisplay(historyEntry);

                    _input = result.ToString();
                    UpdateDisplay();
                }
                else if (!string.IsNullOrEmpty(_currentExpression))
                {
                    // If there's an expression but no current input, wrap the whole expression
                    _currentExpression = $"abs({_currentExpression.Trim()})";
                    UpdateExpressionDisplay();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void ModuloButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                _currentExpression += _input + " mod ";
                _input = "";
                _expectingOperator = true;
                UpdateExpressionDisplay();
            }
        }

        private void TrigComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrigComboBox.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Content != null &&
                !string.IsNullOrEmpty(_input))
            {
                string func = selectedItem.Content.ToString();
                if (func != "Trigonometry")
                {
                    _currentExpression += func + " " + _input + " ";
                    _input = "";
                    UpdateExpressionDisplay();
                }
                TrigComboBox.SelectedIndex = 0; // Reset to default
            }
        }

        private string _storedBaseForYRootX = null;
        private void YRootXButton_Click(object sender, RoutedEventArgs e)
        {
            if (_storedBaseForYRootX == null)
            {
                // First step: Store the base (x)
                if (!string.IsNullOrEmpty(_input))
                {
                    _storedBaseForYRootX = _input;
                    _input = "";
                    _currentExpression = $"y√x({_storedBaseForYRootX},";
                    UpdateDisplay();
                    UpdateExpressionDisplay();
                }
                else
                {
                    _currentExpression = "Error: Enter base first";
                    UpdateExpressionDisplay();
                }
            }
            else
            {
                // Second step: Calculate with exponent (y)
                if (!string.IsNullOrEmpty(_input))
                {
                    try
                    {
                        double x = double.Parse(_storedBaseForYRootX);
                        double y = double.Parse(_input);
                        double result = Math.Pow(x, 1.0 / y);

                        _input = result.ToString();
                        _currentExpression = $"y√x({_storedBaseForYRootX},{y}) = ";
                        _storedBaseForYRootX = null;
                        UpdateDisplay();
                        UpdateExpressionDisplay();
                    }
                    catch (Exception ex)
                    {
                        _currentExpression = $"Error: {ex.Message}";
                        UpdateExpressionDisplay();
                    }
                }
                else
                {
                    _currentExpression = "Error: Enter exponent";
                    UpdateExpressionDisplay();
                }
            }
        }

        private void ShowErrorMessage(string message)
        {
            // Update your display to show the error
            _input = "Error";
            _currentExpression = message;
            UpdateDisplay();
            UpdateExpressionDisplay();

            // Optional: Play error sound
            System.Media.SystemSounds.Beep.Play();
        }


        private void TwoPowerXButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                // Store the current input as the exponent
                string exponent = _input;
                _input = ""; // Clear current input

                // Build the expression as 2^(exponent)
                _currentExpression = $"2^({exponent})";
                UpdateExpressionDisplay();

                // Calculate immediately
                EqualsButton_Click(sender, e);
            }
            else
            {
                // If no number entered, prepare for exponent input
                _currentExpression = "2^(";
                _parenthesisCount++;
                UpdateExpressionDisplay();
            }
        }

        private string _logYBase = null;
        private bool _expectingLogYNumber = false;

        private void LogYButton_Click(object sender, RoutedEventArgs e)
        {
            if (_logYBase == null)
            {
                // First press - store the base (y)
                if (!string.IsNullOrEmpty(_input))
                {
                    _logYBase = _input;
                    _input = "";
                    _currentExpression = $"log_{_logYBase}(";
                    _parenthesisCount++;
                    _expectingLogYNumber = true;
                    UpdateDisplay();
                    UpdateExpressionDisplay();
                }
            }
            else if (_expectingLogYNumber)
            {
                // Second press - calculate with number (x)
                if (!string.IsNullOrEmpty(_input))
                {
                    try
                    {
                        double baseValue = double.Parse(_logYBase);
                        double number = double.Parse(_input);

                        if (baseValue <= 0 || baseValue == 1)
                            throw new ArgumentException("Log base must be positive and ≠1");
                        if (number <= 0)
                            throw new ArgumentException("Log number must be positive");

                        double result = Math.Log(number, baseValue);
                        _input = result.ToString();
                        _currentExpression += $"{number}) = ";
                        _parenthesisCount--;
                        _logYBase = null;
                        _expectingLogYNumber = false;
                        UpdateDisplay();
                        UpdateExpressionDisplay();
                    }
                    catch (Exception ex)
                    {
                        _currentExpression = $"Error: {ex.Message}";
                        _logYBase = null;
                        _expectingLogYNumber = false;
                        UpdateExpressionDisplay();
                    }
                }
            }
        }


        private void EPowerXButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                try
                {
                    double exponent = double.Parse(_input);
                    double result = Math.Exp(exponent);

                    // Store the result and update display
                    _input = result.ToString();
                    _currentExpression = $"e^{exponent} = ";
                    UpdateDisplay();
                    UpdateExpressionDisplay();
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Invalid input: {ex.Message}");
                }
            }
            else
            {
                // If no input, prepare for exponent input
                _currentExpression = "e^(";
                _parenthesisCount++;
                UpdateExpressionDisplay();
            }
        }

        private void FunctionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FunctionsComboBox.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Content != null)
            {
                string func = selectedItem.Content.ToString();
                if (func != "Functions")
                {
                    try
                    {
                        switch (func)
                        {
                            case "|x|":
                                AbsoluteValueButton_Click(sender, e);
                                break;
                            case "⌊x⌋":
                                FloorFunction(sender, e);
                                break;
                            case "⌈x⌉":
                                CeilingFunction(sender, e);
                                break;
                            case "rand":
                                RandomFunction(sender, e);
                                break;
                            case "→dms":
                                ConvertToDMS(sender, e);
                                break;
                            case "→deg":
                                ConvertToDegrees(sender, e);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage(ex.Message);
                    }
                    finally
                    {
                        FunctionsComboBox.SelectedIndex = 0; // Reset to default
                    }
                }
            }
        }

        private void FloorFunction(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                double value = double.Parse(_input);
                double result = Math.Floor(value);

                string historyEntry = $"⌊{_input}⌋ = {result}";
                ParentWindow?.UpdateHistoryDisplay(historyEntry);

                _input = result.ToString();
                UpdateDisplay();
            }
        }

        private void CeilingFunction(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                double value = double.Parse(_input);
                double result = Math.Ceiling(value);

                string historyEntry = $"⌈{_input}⌉ = {result}";
                ParentWindow?.UpdateHistoryDisplay(historyEntry);

                _input = result.ToString();
                UpdateDisplay();
            }
        }

        private void RandomFunction(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            double randomValue = rand.NextDouble();

            _input = randomValue.ToString();
            ParentWindow?.UpdateHistoryDisplay($"rand = {randomValue}");
            UpdateDisplay();
        }

        private void ConvertToDMS(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input) && double.TryParse(_input, out double degrees))
            {
                int d = (int)degrees;
                double remaining = (degrees - d) * 60;
                int m = (int)remaining;
                double s = (remaining - m) * 60;

                string dms = $"{d}° {m}' {s:0.##}\"";
                ParentWindow?.UpdateHistoryDisplay($"{degrees}° → {dms}");

                _input = dms;
                UpdateDisplay();
            }
        }

        private void ConvertToDegrees(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_input))
            {
                // Try to parse DMS format (e.g., "45° 30' 15"")
                var match = Regex.Match(_input, @"^(-?\d+)°\s*(\d+)'\s*(\d+\.?\d*)\""$");
                if (match.Success)
                {
                    int degrees = int.Parse(match.Groups[1].Value);
                    int minutes = int.Parse(match.Groups[2].Value);
                    double seconds = double.Parse(match.Groups[3].Value);

                    double decimalDegrees = degrees + (minutes / 60.0) + (seconds / 3600.0);

                    ParentWindow?.UpdateHistoryDisplay($"{_input} → {decimalDegrees}°");
                    _input = decimalDegrees.ToString();
                    UpdateDisplay();
                }
                else
                {
                    throw new ArgumentException("Invalid DMS format. Use ° ' \"");
                }
            }
        }

        private double ConvertToRadians(double angle)
        {
            return _angleMode switch
            {
                AngleMode.Degrees => angle * Math.PI / 180.0,
                AngleMode.Gradians => angle * Math.PI / 200.0,
                _ => angle // already in radians
            };
        }

        private double ConvertFromRadians(double radians)
        {
            return _angleMode switch
            {
                AngleMode.Degrees => radians * 180.0 / Math.PI,
                AngleMode.Gradians => radians * 200.0 / Math.PI,
                _ => radians // return as radians
            };
        }

        public void ToggleScientificNotation()
        {
            _scientificNotation = !_scientificNotation;
            UpdateDisplay();
        }


        #endregion

        #endregion
    }

}

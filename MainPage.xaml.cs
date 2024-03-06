using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        private bool clearable = false;
        private ParseCalculator pc = new();
        public MainPage()
        {
            InitializeComponent();
        }

        private void Clear(object sender, EventArgs e)
        {
            equation.Text = " ";
            result.Text = "0";
            
            DecimalBtn.IsEnabled = true;
        }

        private void Delete(object sender, EventArgs e)
        {
            string res = result.Text;
            result.Text = res.Length == 1 || "Error: Cannot divide by 0".Contains(res) ? "0" : res.Remove(res.Length - 1, 1) ;
            if (!result.Text.Contains('.')) DecimalBtn.IsEnabled = true;
        }

        private void Input(object sender, EventArgs e)
        {
            string input = ((Button)sender).Text;

            if ("1234567890.".Contains(input))
            {
                if (clearable) // clear item so no need to backspace
                {
                    result.Text = "0";
                    clearable = false;
                    DecimalBtn.IsEnabled = true;
                }
                if (input.Equals("."))
                {
                    DecimalBtn.IsEnabled = false;
                }

                result.Text = (result.Text.Equals("0") && !input.Equals(".") || equation.Text.Contains("=") ? "" : result.Text) + input;
                if(equation.Text.Contains("=")) 
                {
                    equation.Text = "";
                }
            }
            else
            {
                string eq = equation.Text;
                string res = result.Text;

                if (res.ElementAt(res.Length - 1).Equals('.')) // remove stray decimal point
                {
                    result.Text = res.Remove(res.Length - 1, 1);
                    res = result.Text;
                }

                // Check if the input is an operation and set the result to "0"
                result.Text = "0";

                equation.Text = (clearable ?
                    eq.Remove(eq.Length - 1, 1) :
                    ValidateEq() + res + " ")
                    + input;
                clearable = true;
            }
        }

        private void Equals(object sender, EventArgs e)
        {
            equation.Text = ValidateEq() + result.Text;
            try
            {
                double evaluationResult = pc.Evaluate(equation.Text.Replace(" ", ""));
                evaluationResult = Math.Round(evaluationResult, 2);
                string fullResult = evaluationResult.ToString("0.##");

                if (double.IsInfinity(evaluationResult) || double.IsNaN(evaluationResult))
                {
                    result.Text = "Error: Cannot divide by 0";
                    equation.Text = "";
                }
                else
                {
                    equation.Text += " =";
                    result.Text = fullResult;
                }
            }
            catch (Exception ex)
            {
                result.Text = "Error: " + ex.Message;
                equation.Text = "";
            }
        }


        private string ValidateEq()
        {
            string eq = equation.Text;
            return ((eq.Contains("=") ? "0" : eq)
                 .Equals("0") ? "" : eq + " ");
        }
    }
    public class ParseCalculator
    {
        public ParseCalculator() { }

        public double Evaluate(string expression)
        {
            Stack<double> Numbers = new();
            Stack<char> Operators = new();
            StringBuilder temp = new();

            foreach (char ch in expression + "=")
            {
                if ("1234567890.".Contains(ch))
                {
                    temp.Append(ch);
                }
                else //expect an operator
                {
                    _ = double.TryParse(temp.ToString(), out double num); //suppressed a problem on testing if parsing is successful
                    temp.Clear();
                    Numbers.Push(num);

                    if (Operators.Count > 0 && Precedence(Operators.Peek()) >= Precedence(ch))
                    {
                        Calculate(Operators.Pop(), Numbers);
                    }

                    if (!ch.Equals('='))
                    {
                        Operators.Push(ch);
                    }
                }
            }

            foreach (char op in Operators)
            {
                Calculate(op, Numbers);
            }

            return Numbers.Pop();

        }

        private void Calculate(char op, Stack<double> numbers)
        {
            double num2 = numbers.Pop(), num1 = numbers.Pop(), result = 0;

            switch (op)
            {
                case '+':
                    result = num1 + num2;
                    break;
                case '-':
                    result = num1 - num2;
                    break;
                case '×':
                    result = num1 * num2;
                    break;
                case '÷':
                    result = num1 / num2;    
                    break;

            }

            numbers.Push(result);
        }

        private int Precedence(char op)
        {
            return op switch
            {
                '+' or '-' => 0,
                //assume the only operators left are '*' and '/'
                _ => 1,
            };
        }
    }

}

using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        private bool clearable = false;
        private bool isError = false;
        private bool opsEnabled = true;
        private ParseCalculator pc = new();
        public MainPage()
        {
            InitializeComponent();
        }

        private void Clear(object sender, EventArgs e)
        {
            ResetResult();
            equation.Text = " ";

            if (isError) ToggleOps();
        }

        private void Delete(object sender, EventArgs e)
        {
            string res = result.Text;
            if (equation.Text.Contains('='))
            {
                equation.Text = " ";
            }
            else if (res.Length == 1 || clearable || isError)
            {
                ResetResult();
            }
            else
            {
                result.Text = res.Remove(res.Length - 1, 1);
            }

            if (!result.Text.Contains('.')) DecimalBtn.IsEnabled = true;
        }

        private void Input(object sender, EventArgs e)
        {
            string input = ((Button)sender).Text;
            if (isError)
            {
                ResetResult();
            }

            if ("1234567890.".Contains(input))
            {
                if (clearable)
                {
                    ResetResult();
                }

                if (input.Equals(".")) 
                    DecimalBtn.IsEnabled = false;

                result.Text = 
                    (result.Text.Equals("0") && 
                    !input.Equals(".") || 
                    equation.Text.Contains('=') ? "" : result.Text) 
                    + input;
                
                if(equation.Text.Contains('=')) 
                    equation.Text = "";
            }
            else
            {
                string eq = equation.Text;
                string res = NormalizeInput();

                DecimalBtn.IsEnabled = true;
                result.Text = res;
                
                equation.Text = (clearable ?
                    eq.Remove(eq.Length - 1, 1) :
                    ValidateEq() + res + " ")
                    + input;
                clearable = true;
            }
        }

        private void Equals(object sender, EventArgs e)
        {
            equation.Text = ValidateEq() + NormalizeInput();
            try
            {
                decimal res = pc.Evaluate(equation.Text.Replace(" ", ""));
                equation.Text += " =";
                result.Text = NormalizeNum(res).ToString();
                clearable = true;
                
            }
            catch (Exception ex)
            {
                ErrorHandler(ex.Message);
            }
        }


        private void ResetResult()
        {
            result.Text = "0";
            result.FontSize = 64;
            isError = false;
            clearable = false;
            DecimalBtn.IsEnabled = true;

            if(!opsEnabled) ToggleOps();
        }

        private string ValidateEq()
        {
            string eq = equation.Text;
            return ((eq.Contains('=') ? "0" : eq)
                 .Equals("0") ? "" : eq + " ");
        }

        private string NormalizeInput()
        {
            string res = result.Text;
            if (res.ElementAt(res.Length - 1).Equals('.')) // remove stray decimal 
                result.Text = res.Remove(res.Length - 1, 1);

            result.Text = NormalizeNum(decimal.Parse(res)).ToString();

            return result.Text;
        }

        private decimal NormalizeNum(decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }


        private void ToggleOps()
        {
            Button []opBtns = [plus, minus, div, mult, eq];
            foreach(Button btn in opBtns)
            {
                btn.IsEnabled = !btn.IsEnabled;
            }

            opsEnabled = plus.IsEnabled;
        }

        private void ErrorHandler(string msg, int fontSize = 18)
        {
            isError = true;
            result.Text = "Error: " + msg;
            result.FontSize = fontSize;
            equation.Text = "";
            ToggleOps();
        }

    }
    public class ParseCalculator
    {
        public ParseCalculator() { }

        public decimal Evaluate(string expression)
        {
            Stack<decimal> Numbers = new();
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
                    _ = decimal.TryParse(temp.ToString(), out decimal num); //suppressed a problem on testing if parsing is successful
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

        private void Calculate(char op, Stack<decimal> numbers)
        {
            decimal num2 = numbers.Pop(), num1 = numbers.Pop(), result = 0;

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

using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        public static int Evaluate(String exp, Lookup variableEvaluator)
        {

            /// <summary>
            ///   "The method should evaluate the expression, using the algorithm above (or implemented below)
            ///    It should return the value of the expression if it has a value,
            ///    or throw an ArgumentException if any of the possible errors from the algorithm occurs."
            ///    Uses some helper functions and extensions for the C# stack class.
            /// </summary>
            /// <param name="exp"> Infix expression to be evaluated. </param>
            /// <param name="variableEvaluator"> 
            /// Lookup table for different variables.
            /// Also responsible for throwing exceptions whenever a variable or token is not valid.
            /// </param>
            /// <returns> Evaluation of exp parameter. </returns>
            /// <exception cref="ArgumentException"> 
            /// Default exception whenever an error is encountered within this method itself.
            /// Unrelated to errors thrown by the variableEvaluator delegate parameter.
            /// </exception>"
            string validPattern = "^[a-zA-Z]+[0-9]+$";
            if (exp.Trim() == "")
            {
                throw new ArgumentException();
            }
            exp = exp.Trim();
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            Stack<int> values = new Stack<int>();
            Stack<string> operators = new Stack<string>();
            for (int i = 0; i < substrings.Length; i++)
            {
                if (String.IsNullOrEmpty(substrings[i]))                // empty/none substring element case
                {
                    continue;
                }
                else if (int.TryParse(substrings[i], out int temp))    // blatant (or non-variable) int element case
                {
                    values.Push(temp);
                    CheckMultiplyDivide(values, operators);
                }
                else if ("+-".Contains(substrings[i]))                  // +- element case
                {
                    CheckPlusMinus(values, operators);
                    operators.Push(substrings[i]);
                }
                else if ("*/".Contains(substrings[i]))                  // */ element case
                {
                    operators.Push(substrings[i]);
                }
                else if (substrings[i] == "(")                          // open paranthesis element case
                {
                    operators.Push(substrings[i]);
                }
                else if (substrings[i] == ")")                          // close paranthesis element case CHECK OFF WITH TAs
                {
                    CheckPlusMinus(values, operators);
                    if (operators.Count == 0)
                    {
                        throw new ArgumentException();
                    }
                    if (operators.Pop() != "(")
                    {
                        throw new ArgumentException();
                    }
                    CheckMultiplyDivide(values, operators);
                    //if (Regex.IsMatch("[0-9]+", substrings[i+1])
                    //    || Regex.IsMatch(validPattern, substrings[i+1]))
                    //    {
                    //    throw new ArgumentException();
                    //}
                }
                else                                                    // variable element case
                {
                    // case also accounts for any other unknown symbols via the variableEvaluator.
                    // I trust that whoever sets up the delegate for this code will account for this wisely.
                    if (!Regex.IsMatch(substrings[i], validPattern))
                    {
                        throw new ArgumentException();
                    }
                    int temp1 = variableEvaluator(substrings[i]);
                    values.Push(temp1);
                    CheckMultiplyDivide(values, operators);
                }
            }

            /*
             * after the final token has been processed
             */

            if (operators.Count == 1)
            // addition or subtraction should be the only two options for the operation left,
            // or else something is wrong.
            {
                if (!CheckPlusMinus(values, operators))
                {
                    throw new ArgumentException();
                }
            }
            else if (operators.Count > 1)
            {
                throw new ArgumentException();
            }
            return (values.Pop());
        }

        private static void Operate(Stack<int> values, Stack<String> operators)
        {
            // the same idea essentially applies to all of these operations.
            // the error shown is also the only one relevant to the cause at hand.
            int temp1, temp2;   // there must be some better way to do this but I didn't have enough time to check
            String o;
            if (values.HasEnough() && operators.HasEnough())
            {
                temp1 = values.Pop();
                temp2 = values.Pop();
                o = operators.Pop();
            }
            else
            {
                throw new ArgumentException();
            }
            if (o == "+")
            {
                values.Push(temp2 + temp1);
            }
            else if (o == "-")
            {
                values.Push(temp2 - temp1);
            }
            else if (o == "*")
            {
                values.Push(temp2 * temp1);
            }
            else if (o == "/")
            {
                if (temp1 == 0)             // removes division by zero
                {
                    throw new ArgumentException();
                }
                values.Push(temp2 / temp1);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        // these methods are kind of silly but it was fun implementing them.
        private static bool CheckPlusMinus(Stack<int> values, Stack<String> operators)
        {
            if (operators.IsOnTop("+", "-"))
            {
                Operate(values, operators);
                return true;
            }
            return false;
        }
        private static bool CheckMultiplyDivide(Stack<int> values, Stack<String> operators)
        {
            if (operators.IsOnTop("*", "/"))
            {
                Operate(values, operators);
                return true;
            }
            return false;
        }
    }

    static class StackExtensions
    {
        public static bool IsOnTop(this Stack<String> stack, String str1, String str2)
        {
            // typically called in pairs as multiplication and addition are both different counterparts
            // and correlate to division and subtraction, as well.
            if (!stack.HasEnough())
            {
                return false;
            }
            return (stack.Peek() == str1 || stack.Peek() == str2);
        }

        public static bool HasEnough(this Stack<int> stack)
        {
            // called only whenever necessary. If something doesn't have enough, there should be an error.
            // equations require two operands.
            return stack.Count >= 2;
        }

        public static bool HasEnough(this Stack<String> stack)
        {
            // called only whenever necessary. If something doesn't have enough, there should be an error.
            // equations require one operator.
            return stack.Count >= 1;
        }
    }
}
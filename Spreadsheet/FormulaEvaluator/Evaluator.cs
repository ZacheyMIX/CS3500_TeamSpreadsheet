using System.Text.RegularExpressions;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;

/**
 * Written by Zachery Blomquist
 * Version 9/2/22
 */
namespace FormulaEvaluator
{
    /**
     * This class evaluates an formula with given values, operations, and variables
     */
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        /**
         * This static method is the main method in evaluating the formula given.
         * Returns the final calculated value from a PEMDAS algorithm
         */
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            //The value at the current token through the search
            int currentValue;
            //The stack that contains values
            Stack<int> valStack = new Stack<int>();
            //The stack that contains operations
            Stack<string> opStack = new Stack<string>();
            //Checks each token in the substring and evaluates the final result

            //A for loop that trims all the tokens of whitespace
            for (int i = 0; i < substrings.Length; i++)
            {
                substrings[i] = String.Concat(substrings[i].Where(c => !char.IsWhiteSpace(c)));

            }
            //Afterwards, deletes any empty tokens from substrings
            substrings = substrings.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            foreach (string substring in substrings)
            {
                if (int.TryParse(substring, out int n))
                {

                    //If the operator in the opStack is * or / peform an operation
                    //With that operator accordingly with the value popped from the stack and the current value
                    //If not, pushes current value onto stack
                    if (StackExtensions.IsOnTop(opStack, "*") || StackExtensions.IsOnTop(opStack, "/"))
                    {
                        //Performs the proper calculation through pushresult, and pushes the result onto the valStack
                        StackExtensions.PushResult(valStack, opStack.Pop(), n);
                    }
                    else
                        valStack.Push(n);
                }
                //Checks if the current substring is + or -
                else if (substring is "+" || substring is "-")
                {
                    //Checks if stack in operation stack is empty and checks for + or - otherwise
                    //If check fails pushes operation onto op stack
                    if (StackExtensions.IsOnTop(opStack, "+") || StackExtensions.IsOnTop(opStack, "-"))
                    {
                        //Pushes result of + or - case with PushResult
                        StackExtensions.PushResult(valStack, opStack.Pop(), valStack.Pop());
                        //Removes last operation from opStack
                        opStack.Push(substring);
                    }
                    else
                        opStack.Push(substring);
                }
                //Checks for * / and (, then pushes onto operation stack
                else if (substring is "*" || substring is "/" || substring is "(")
                {
                    opStack.Push(substring);
                }
                else if (substring is ")")
                {
                    //Pushes result from last operation and last to values from the stack
                    if (!(opStack.Peek() is "("))
                    {
                        StackExtensions.PushResult(valStack, opStack.Pop(), valStack.Pop());
                        if (opStack.Count is 0 || !(opStack.Peek() is "("))
                            throw new ArgumentException("expected a ( in the stack, but was not there");
                        //Gets rid of ( from the stack
                        opStack.Pop();
                        //If there is a * or / before the (, operate that function
                        if (!(opStack.Count is 0) && (opStack.Peek() is "*" || opStack.Peek() is "/"))
                        {
                            StackExtensions.PushResult(valStack, opStack.Pop(), valStack.Pop());
                        }
                    }
                    //If ( is the next operator, than there is no need for it, remove
                    else
                        opStack.Pop();

                }
                //else for variables
                else
                {
                    //Checks if the current substring is a valid varaible by checking the first character as a letter
                    //And the last character as a number
                    if (!(char.IsLetter(substring.First()) && char.IsDigit(substring.Last())))
                        throw new ArgumentException("Not a valid variable");
                    //Grabs the value from the variable
                    currentValue = variableEvaluator(substring);

                    //Checks if the operator stack is empty, then checks which variable is on top for * or /
                    if (StackExtensions.IsOnTop(opStack, "*") || StackExtensions.IsOnTop(opStack, "/"))
                    {
                        StackExtensions.PushResult(valStack, opStack.Pop(), currentValue);
                    }
                    //Otherwise, just push the value onto the value stack
                    else
                        valStack.Push(currentValue);
                }
            }
            //If there is an operation in opStack, runs more calculations
            if (opStack.Count > 0)
            {
                //Checks if the valStack has more than 1 value
                //Otherwise throws argument exception
                if (valStack.Count < 1)
                    throw new ArgumentException("val stack has less than 2 values");

                //Calculates and pushes the result of the last 2 values with the last operation
                StackExtensions.PushResult(valStack, opStack.Pop(), valStack.Pop());
            }
            //If there is still more than 1 value, throw argument exception
            if (!(valStack.Count is 1))
                throw new ArgumentException("val stack does not have exactly one value in it");
            return valStack.Pop();
        }
    }
    /**
     * A static class for stack extensions
     */
    public static class StackExtensions
    {
        /**
         * This extended method checks if the operation stack is empty and checks if the operation stack contains the right operation
         */
        public static bool IsOnTop(this Stack<String> stack, String c)
        {
            return stack.Count > 0 && stack.Peek() == c;
        }
        /**
         * This extended method does the proper calculations for all cases of + - / * and the closing )
         * This extended method uses the valStack class to perform such operations, then pushes the result onto the stack
         */
        public static void PushResult(this Stack<int> stack, String c, int val)
        {
            //The final value from the calculations
            int finalVal;
            if (c is "*" && stack.Count > 0)
            {
                finalVal = stack.Pop() * val;
                stack.Push(finalVal);
            }
            else if (c is "/" && stack.Count > 0)
            {
                //Checks if the value is 0, if it is throws an argument exception
                if (val is 0)
                    throw new ArgumentException("Cannot divide by zero");
                finalVal = stack.Pop() / val;
                stack.Push(finalVal);
            }
            else if (c is "+" && stack.Count > 0)
            {
                finalVal = stack.Pop() + val;
                stack.Push(finalVal);
            }
            else if (c is "-" && stack.Count > 0)
            {
                finalVal = stack.Pop() - val;
                stack.Push(finalVal);
            }
        }
    }
}
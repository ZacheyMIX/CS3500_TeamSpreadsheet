// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!

// Change log:
// Last updated: 9/8, updated for non-nullable types

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>

        //private readonly List<String> substrings;
        private readonly String expression;

        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Helper method to affirm valid variable syntax. parameters normalize and isValid used for specific use in constructor.
        /// If used anywhere else, parameterize normalize with s => s and isValid with s => true.
        /// This is because the constructor needs to confirm syntax, but other methods may need to parse variables.
        /// </summary>
        private static bool VerifyVariable
            (String token, Func<string, string> normalize, Func<string, bool> isValid)
        {
            if (Regex.IsMatch(token, "^(_|[a-zA-Z])([a-zA-Z]*|_*|[0-9]*)"))
            {
                return isValid(normalize(token));
            }
            return false;
        }
        /// <summary>
        /// Verifies operators during construction.
        /// </summary>
        private static bool VerifyOperator(String token)
        {
            return Regex.IsMatch(token, "[+*/]|-");
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            if (formula == "")
            {
                throw new FormulaFormatException("Cannot validate an empty string.");
            }
            IEnumerable<String> tokens = GetTokens(formula);
            int openParens = 0;
            int closeParens = 0;
            bool shouldBeNumber = true;
            double currentFloat;
            expression = "";
            foreach (String token in tokens)
            {
                if (shouldBeNumber && Double.TryParse(token, out currentFloat))
                {
                    expression += currentFloat.ToString();
                    shouldBeNumber = false;
                }
                else if (shouldBeNumber && VerifyVariable(token, normalize, isValid))
                {
                    expression += normalize(token);
                    shouldBeNumber = false;
                }
                else if (!shouldBeNumber && VerifyOperator(token))
                {
                    expression += token;
                    shouldBeNumber = true;
                }
                else if (shouldBeNumber && token == "(")
                {
                    // an open parentheses should only go wherever a number goes, but should be immediately followed by a number.
                    // this is why we make sure that this token "should be a number", but not toggle that bool so that
                    // the next token should also either be an open parentheses or a number.
                    openParens++;
                    expression += token;
                }
                else if (!shouldBeNumber && token == ")")
                {
                    // likewise to the open parenthesis case, a close parentheses should only go wherever an operator goes,
                    // but should be immediately followed by another operator, if not nothing. this is why we correlate this
                    // with the operator half of the shouldBeNumber toggle, but without changing it.
                    closeParens++;
                    expression += token;
                }
                else
                {
                    throw new FormulaFormatException("Invalid syntax when constructing a Formula object."); //EndingTokenRule1 fails here
                }
            }
            if (openParens != closeParens)
            {
                throw new FormulaFormatException("Inequal number of opening and closing parenthesis when creating Formula object.");
            }
            // check to see if the last character is valid
            string lastChar = "" + expression[expression.Length - 1];
            if (VerifyOperator(lastChar) || lastChar == "(")
            {
                throw new FormulaFormatException("Last character of expression is invalid.");
            }
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            IEnumerable<string> tokens = GetTokens(expression);
            Stack<double> values = new Stack<double>();
            Stack<string> operators = new Stack<string>();
            bool? operatorChecker;
            foreach (string token in tokens)
            {
                if (double.TryParse(token, out double temp))    // blatant (or non-variable) int element case
                {
                    values.Push(temp);
                    operatorChecker = CheckMultiplyDivide(values, operators);
                    if (operatorChecker is null)
                    {
                        return new FormulaError("Division by zero.");
                    }
                }
                else if ("+-".Contains(token))                  // +- element case
                {
                    operatorChecker = CheckPlusMinus(values, operators);
                    operators.Push(token);
                }
                else if ("*/".Contains(token))                  // */ element case
                {
                    operators.Push(token);
                }
                else if (token == "(")                          // open paranthesis element case
                {
                    operators.Push(token);
                }
                else if (token == ")")                          // close paranthesis element case CHECK OFF WITH TAs
                {
                    operatorChecker = CheckPlusMinus(values, operators);
                    operators.Pop();
                    operatorChecker = CheckMultiplyDivide(values, operators);
                    if (operatorChecker is null)
                    {
                        return new FormulaError("Division by zero.");
                    }
                }
                else if (VerifyVariable(token, s => s, s => true))
                {
                    // case also accounts for any other unknown symbols via the variableEvaluator.
                    // I trust that whoever sets up the delegate for this code will account for this wisely.

                    double temp1;
                    try
                    {
                        temp1 = lookup(token);
                    }
                    catch
                    {
                        return new FormulaError("Unknown variable used in evaluation. Make sure all variables are assigned.");
                    }
                    values.Push(temp1);
                    operatorChecker = CheckMultiplyDivide(values, operators);
                }
            }

            /*
             * after the final token has been processed
             */

            if (operators.Count == 1)
            // addition or subtraction should be the only two options for the operation left,
            // or else something is wrong.
            {
                operatorChecker = CheckPlusMinus(values, operators);
            }
            try
            {
                return (values.Pop());
            }
            catch
            {
                return new FormulaError("Division by zero.");
            }
        }

        /// <summary>
        /// New implementation of operate, CheckMultiply and CheckAdd that make the return value nullable
        /// as to accomodate for operation errors. 
        /// </summary>
        private static bool CheckPlusMinus(Stack<double> values, Stack<String> operators)
        {
            if (operators.IsOnTop("+", "-"))
            {
                return Operate(values, operators);
            }
            return false;
        }
        private static bool? CheckMultiplyDivide(Stack<double> values, Stack<String> operators)
        {
            if (operators.IsOnTop("*", "/"))
            {
                if (!Operate(values, operators))
                {
                    return null;
                }
                return true;
            }
            return false;
        }
        private static bool Operate(Stack<double> values, Stack<String> operators)
        // returns false whenever there should be some FormulaError object.
        {
            // the same idea essentially applies to all of these operations.
            // the error shown is also the only one relevant to the cause at hand.
            double temp1, temp2;   // there must be some better way to do this but I didn't have enough time to check
            String o;
            temp1 = values.Pop();
            temp2 = values.Pop();
            o = operators.Pop();
            bool returned = true;
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
                    return false;
                }
                values.Push(temp2 / temp1);
            }

            return returned;
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            IEnumerable<String> e = GetTokens(expression);
            HashSet<String> returned = new();
            foreach (String token in e)
            {
                if (VerifyVariable(token, s => s, s => true))
                {   // statement implies current token is a variable
                    returned.Add(token);
                }
            }
            return returned;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {   // constructor should automatically remove whitespace
            return expression;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            return expression.Equals(obj.ToString());
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return (!f1.Equals(f2));
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }

    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }


    }

    /// <summary>
    /// Helper class for extensions specifically relating to the Stack class
    /// </summary>
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

        public static bool HasEnough(this Stack<String> stack)
        {
            // called only whenever necessary. If something doesn't have enough, there should be an error.
            // equations require one operator.
            return stack.Count >= 1;
        }

    }
}
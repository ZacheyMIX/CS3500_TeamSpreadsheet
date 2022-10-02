using FormulaEvaluator;

namespace FormulaEvaluatorTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine();
            Console.WriteLine(Evaluator.Evaluate("1+1", TestLookup));
            Console.WriteLine("Answer should be 2.");
            Console.WriteLine(Evaluator.Evaluate("a1", TestLookup));
            Console.WriteLine("Answer should be 5.");
            Console.WriteLine(Evaluator.Evaluate("1+1", TestLookup));
            Console.WriteLine("Answer should be 2.");
            Console.WriteLine(Evaluator.Evaluate("a1+a1", TestLookup));
            Console.WriteLine("Answer should be 10.");
            Console.WriteLine(Evaluator.Evaluate("a1*zilch1", TestLookup));
            Console.WriteLine("Answer should be 0.");
            Console.WriteLine(Evaluator.Evaluate("(2+35)*A7", TestLookup));
            Console.WriteLine("Answer should be 37.");
            Console.WriteLine(Evaluator.Evaluate("1+((2+3)*(2-1))", TestLookup));
            Console.WriteLine("Answer should be 6.");
            Console.WriteLine(Evaluator.Evaluate("1+((2*1)+zilch1)", TestLookup));
            Console.WriteLine("Answer should be 3.");
            Console.WriteLine(Evaluator.Evaluate("2+3*(5+9)", TestLookup));
            Console.WriteLine("Answer should be 44.");

            // errors
            //Console.WriteLine(Evaluator.Evaluate("1/0", TestLookup));
            //Console.WriteLine("Program should have thrown an error.");
            //Console.WriteLine(Evaluator.Evaluate("a2", TestLookup));
            //Console.WriteLine("Program should have thrown an error.");
        }

        public static int TestLookup(String token)
        {
            if (token == null || token == "")
            {
                throw new ArgumentException();
            }
            else if (token == "a1")
            {
                return 5;
            }
            else if (token == "zilch1")
            {
                return 0;
            }
            else if (token == "same1")
            {
                return 1;
            }
            else if (token == "A7")
            {
                return 1;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
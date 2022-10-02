using SpreadsheetUtilities;

namespace FormulaTests
{
    [TestClass]
    public class FormulaTests
    {
        /// <summary>
        /// Empty formulas are syntactically invalid due to having no tokens
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void OneTokenRuleThrows()
        {
            Formula f = new("");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void RightParanthesesRuleThrows()
        {
            Formula f = new("(1+2))");
        }

        /// <summary>
        /// Following tests cover the Balanced Parantheses rule
        /// Rule states that the total number of opening parentheses must equal
        /// the total number of closing parantheses.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void BalancedParanthesesRule1()
        {
            Formula f = new("((1+2)");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void BalancedParanthesesRule2()
        {
            Formula f = new("(1+2)-((2+1)");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void BalancedParanthesesRule3()
        {
            Formula f = new("(1+2))-(2+1)");
        }

        /// <summary>
        /// The first token of an expression must be a number, variable, or opening parantheses
        /// Following tests cover this.
        /// </summary>
        [TestMethod]
        public void StartingTokenRule1()
        {
            Formula f = new("1+(3-5)");
            f = new("420+(3-5)");
            f = new("6");
            f = new("69*420");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        public void StartingTokenRule2()
        {
            // TODO: add validator
            Formula f = new("X1+(3-5)");
            f = new("X2+(3-5)");
            f = new("A3+(21-4)");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        public void StartingTokenRule3()
        {
            Formula f = new("(1-2)");
            f = new("(9+10)");
            f = new("(1*1)+1");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void StartingTokenRuleThrows()
        {
            Formula f = new(")-2");
            //TODO: figure out how to make more cases for this and how to manage throws
        }

        /// <summary>
        /// The last token of an expression must be a number, a variable, or a closing parantheses.
        /// The following tests cover that.
        /// </summary>
        [TestMethod]
        public void EndingTokenRule1()
        {
            Formula f = new("1+1");
            f = new("(21-21)+5");
            f = new("1*50");
            f = new("(6-5)*29");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        public void EndingTokenRule2()
        {
            //TODO: add validator
            Formula f = new("2+X1");
            f = new("A1+X2");
            f = new("(X2-5)*X1");
            f = new("(9+10)-C3");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        public void EndingTokenRule3()
        {
            Formula f = new("(21-31)");
            f = new("20-(10+9)");
            f = new("(B2*(A1-2))-(B2*(A1-2))"); //TODO: add validator
            f = new("2-(1*1)");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void EndingTokenRuleBreaker()
        {
            Formula f = new("(21-2(");
            //TODO: figure out how to make more cases for this and how to manage throws
        }

        /// <summary>
        /// Any token that immediately follows an opening paranthesis or an operator must be
        /// either a number, a variable, or an opening paranthesis.
        /// </summary>
        [TestMethod]
        public void OperatorFollowingRule1()
        {
            Formula f = new("(1-2)");
            f = new("1+(9+10)");
            f = new("2+(10-9)*(10-9)");
            f = new("A1+(2-1)");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        public void OperatorFollowingRule2()
        {
            Formula f = new("(A1-10)");
            f = new("1+(A1+10)");
            f = new("2+(X1-9)*(X2-9)");
            f = new("X1-(C3-A1)");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        public void OperatorFollowingRule3()
        {
            Formula f = new("((1*1))");
            f = new("((1*2)-1)");
            f = new("10-((9-7)-1)");
            f = new("(((1-1)))");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void OperatorFollowingRuleBreaker()
        {
            Formula f = new("()");
            //TODO: figure out how to make more cases for this and how to manage throws
        }

        /// <summary>
        /// Any token that immediately follows a number, variable, or a closing parenthesis
        /// must be an operator or a closing parenthesis.
        /// </summary>
        [TestMethod]
        public void ExtraFollowingRule1()
        {
            Formula f = new("1+1");
            f = new("1-1");
            f = new("1*1");
            f = new("1/1");
            f = new("(1+1)");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        public void ExtraFollowingRule2()
        {
            Formula f = new("A1+1");
            f = new("B1-1");
            f = new("C1*1");
            f = new("D1/1");
            f = new("(1-E1)");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        public void ExtraFollowingRule3()
        {
            Formula f = new("(1*1)+1");
            f = new("(1*1)-1");
            f = new("(1*1)*1");
            f = new("(1*1)/1");
            f = new("(1+(1*1))-2");
            //method shouldn't throw an error. ask if there's some Assert method for that.
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void ExtraFollowingRuleBreaker()
        {
            Formula f = new("(1+1)1");
            //TODO: figure out how to make more cases for this and how to manage throws
        }

        /// <summary>
        /// tests getvariables method
        /// </summary>
        [TestMethod]
        public void TestGetVariables()
        {
            Formula f = new("A1-A2+X1");
            IEnumerator<String> e = f.GetVariables().GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.IsTrue(e.Current == "A1");
            Assert.IsTrue(e.MoveNext());
            Assert.IsTrue(e.Current == "A2");
            Assert.IsTrue(e.MoveNext());
            Assert.IsTrue(e.Current == "X1");
            Assert.IsTrue(!e.MoveNext());
        }

        /// <summary>
        /// Some sadist may use an invalid variable. We don't want that, no sir.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]  // may change after lecture
        public void InvalidTokensBreak()
        {
            Formula f = new("1+$");
        }

            /// <summary>
            /// equality method testing
            /// </summary>
            [TestMethod]
        public void EqualsReturnsTrue()
        {
            Formula f = new("A4 + 100");
            Formula g = new("A4   +   100");
            Formula h = new("A4 + 100.000");
            Formula i = new("A4 + 1e2");
            
            Assert.IsTrue(f.Equals(f));
            Assert.IsTrue(g.Equals(f));
            Assert.IsTrue(h.Equals(f));
            Assert.IsTrue(i.Equals(f));

            Assert.IsTrue(f.Equals(g));
            Assert.IsTrue(g.Equals(g));
            Assert.IsTrue(h.Equals(g));
            Assert.IsTrue(i.Equals(g));

            Assert.IsTrue(f.Equals(h));
            Assert.IsTrue(g.Equals(h));
            Assert.IsTrue(h.Equals(h));
            Assert.IsTrue(i.Equals(h));

            Assert.IsTrue(f.Equals(i));
            Assert.IsTrue(g.Equals(i));
            Assert.IsTrue(h.Equals(i));
            Assert.IsTrue(i.Equals(i));
        }
        [TestMethod]
        public void EqualsReturnsFalse()
        {
            Formula f = new("A4 + 100");
            Formula g = new("A4 + 10");
            Formula h = new("A4 +   20");
            Formula i = new("A4     -     100");

            //Assert.IsFalse(f.Equals(f));
            Assert.IsFalse(g.Equals(f));
            Assert.IsFalse(h.Equals(f));
            Assert.IsFalse(i.Equals(f));

            Assert.IsFalse(f.Equals(g));
            //Assert.IsFalse(g.Equals(g));
            Assert.IsFalse(h.Equals(g));
            Assert.IsFalse(i.Equals(g));

            Assert.IsFalse(f.Equals(h));
            Assert.IsFalse(g.Equals(h));
            //Assert.IsFalse(h.Equals(h));
            Assert.IsFalse(i.Equals(h));

            Assert.IsFalse(f.Equals(i));
            Assert.IsFalse(g.Equals(i));
            Assert.IsFalse(h.Equals(i));
            //Assert.IsFalse(i.Equals(i));
        }

        /// <summary>
        /// == operator testing
        /// </summary>
        [TestMethod]
        public void DoubleEqualsReturnsTrue()
        {
            Formula f = new("A4 + 100");
            Formula g = new("A4   +   100");
            Formula h = new("A4 + 100.000");
            Formula i = new("A4 + 1e2");

            Assert.IsTrue(f == f);
            Assert.IsTrue(g == f);
            Assert.IsTrue(h == f);
            Assert.IsTrue(i == f);

            Assert.IsTrue(f == g);
            Assert.IsTrue(g == g);
            Assert.IsTrue(h == g);
            Assert.IsTrue(i == g);

            Assert.IsTrue(f == h);
            Assert.IsTrue(g == h);
            Assert.IsTrue(h == h);
            Assert.IsTrue(i == h);

            Assert.IsTrue(f == i);
            Assert.IsTrue(g == i);
            Assert.IsTrue(h == i);
            Assert.IsTrue(i == i);
        }
        [TestMethod]
        public void DoubleEqualsReturnsFalse()
        {
            Formula f = new("A4 + 100");
            Formula g = new("A4 + 10");
            Formula h = new("A4 +   20");
            Formula i = new("A4     -     100");

            //Assert.IsFalse(f == f);
            Assert.IsFalse(g == f);
            Assert.IsFalse(h == f);
            Assert.IsFalse(i == f);

            Assert.IsFalse(f == g);
            //Assert.IsFalse(g == g);
            Assert.IsFalse(h == g);
            Assert.IsFalse(i == g);

            Assert.IsFalse(f == h);
            Assert.IsFalse(g == h);
            //Assert.IsFalse(h == h);
            Assert.IsFalse(i == h);

            Assert.IsFalse(f == i);
            Assert.IsFalse(g == i);
            Assert.IsFalse(h == i);
            //Assert.IsFalse(i == i);
        }

        /// <summary>
        /// != operator tests
        /// </summary>
        [TestMethod]
        public void NotEqualsReturnsTrue()
        {
            Formula f = new("A4 + 100");
            Formula g = new("A4 + 10");
            Formula h = new("A4 +   20");
            Formula i = new("A4     -     100");

            //Assert.IsTrue(f != f);
            Assert.IsTrue(g != f);
            Assert.IsTrue(h != f);
            Assert.IsTrue(i != f);

            Assert.IsTrue(f != g);
            //Assert.IsTrue(g != g);
            Assert.IsTrue(h != g);
            Assert.IsTrue(i != g);

            Assert.IsTrue(f != h);
            Assert.IsTrue(g != h);
            //Assert.IsTrue(h != h);
            Assert.IsTrue(i != h);

            Assert.IsTrue(f != i);
            Assert.IsTrue(g != i);
            Assert.IsTrue(h != i);
            //Assert.IsTrue(i != i);
        }
        [TestMethod]
        public void NotEqualsReturnsFalse()
        {
            Formula f = new("A4 + 100");
            Formula g = new("A4   +   100");
            Formula h = new("A4 + 100.000");
            Formula i = new("A4 + 1e2");

            Assert.IsFalse(f != f);
            Assert.IsFalse(g != f);
            Assert.IsFalse(h != f);
            Assert.IsFalse(i != f);

            Assert.IsFalse(f != g);
            Assert.IsFalse(g != g);
            Assert.IsFalse(h != g);
            Assert.IsFalse(i != g);

            Assert.IsFalse(f != h);
            Assert.IsFalse(g != h);
            Assert.IsFalse(h != h);
            Assert.IsFalse(i != h);

            Assert.IsFalse(f != i);
            Assert.IsFalse(g != i);
            Assert.IsFalse(h != i);
            Assert.IsFalse(i != i);
        }

        /// <summary>
        /// Equals method accepts nullable parameters.
        /// </summary>
        [TestMethod]
        public void EqualsNullReturnsFalse()
        {
            Formula f = new("1+1");
            Formula? g = null;
            Assert.IsFalse(f.Equals(g));
        }

        /// <summary>
        /// GetHashCode tests comparing returned hashcodes
        /// </summary>
        [TestMethod]
        public void EqualExpressionsEqualHashCodes()
        {
            Formula f = new("A4 + 100");
            Formula g = new("A4   +   100");
            Assert.AreEqual(g.GetHashCode(), f.GetHashCode());
        }
        [TestMethod]
        public void NotEqualExpressionsInequalHashCodes()
        {
            Formula f = new("A4 + 100");
            Formula g = new("A4 + 10");
            Assert.AreNotEqual(g.GetHashCode(), f.GetHashCode());
        }



        /// <summary>
        /// Evaluate tests.
        /// Most, if not all of these are just ripped from the PS1 grade tests.
        /// </summary>
        [TestMethod(), Timeout(5000)]
        [TestCategory("3")]
        public void TestAddition()
        {
            //Assert.AreEqual(8, Evaluator.Evaluate("5+3", s => 0));
            Formula f = new("5+3");
            Assert.AreEqual(8.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("4")]
        public void TestSubtraction()
        {
            //Assert.AreEqual(8, Evaluator.Evaluate("18-10", s => 0));
            Formula f = new("18-10");
            Assert.AreEqual(8.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("5")]
        public void TestMultiplication()
        {
            //Assert.AreEqual(8, Evaluator.Evaluate("2*4", s => 0));
            Formula f = new("2*4");
            Assert.AreEqual(8.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("6")]
        public void TestDivision()
        {
            //Assert.AreEqual(8, Evaluator.Evaluate("16/2", s => 0));
            Formula f = new("16/2");
            Assert.AreEqual(8.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("7")]
        public void TestArithmeticWithVariable()
        {
            //Assert.AreEqual(6, Evaluator.Evaluate("2+X1", s => 4));
            Formula f = new("2+X1");
            Assert.AreEqual(8.0, f.Evaluate(s => 6));
        }

        
        [TestMethod(), Timeout(5000)]
        [TestCategory("8")]
        public void TestUnknownVariable()   // COME BACK TO THIS!!!
        {
            //Evaluator.Evaluate("2+X1", s => { throw new ArgumentException("Unknown variable"); });
            Formula f = new("2+X1");
            //Assert.IsTrue(f.Evaluate(s => { throw new ArgumentException("Unknown variable"); }).GetType() is FormulaError);
            Assert.IsInstanceOfType(
                f.Evaluate(s => { throw new ArgumentException("Unknown variable"); }), typeof(FormulaError));
            // Formula evaluator should RETURN an instance of FormulaError.
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("9")]
        public void TestLeftToRight()
        {
            //Assert.AreEqual(15, Evaluator.Evaluate("2*6+3", s => 0));
            Formula f = new("2*6+3");
            Assert.AreEqual(15.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("10")]
        public void TestOrderOperations()
        {
            //Assert.AreEqual(20, Evaluator.Evaluate("2+6*3", s => 0));
            Formula f = new("2+6*3");
            Assert.AreEqual(20.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("11")]
        public void TestParenthesesTimes()
        {
            //Assert.AreEqual(24, Evaluator.Evaluate("(2+6)*3", s => 0));
            Formula f = new("(2+6)*3");
            Assert.AreEqual(24.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("12")]
        public void TestTimesParentheses()
        {
            //Assert.AreEqual(16, Evaluator.Evaluate("2*(3+5)", s => 0));
            Formula f = new("2*(3+5)");
            Assert.AreEqual(16.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("13")]
        public void TestPlusParentheses()
        {
            //Assert.AreEqual(10, Evaluator.Evaluate("2+(3+5)", s => 0));
            Formula f = new("2+(3+5)");
            Assert.AreEqual(10.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("14")]
        public void TestPlusComplex()
        {
            //Assert.AreEqual(50, Evaluator.Evaluate("2+(3+5*9)", s => 0));
            Formula f = new("2+(3+5*9)");
            Assert.AreEqual(50.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("15")]
        public void TestOperatorAfterParens()
        {
            //Assert.AreEqual(0, Evaluator.Evaluate("(1*1)-2/2", s => 0));
            Formula f = new("(1*1)-2/2");
            Assert.AreEqual(0.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("16")]
        public void TestComplexTimesParentheses()
        {
            //Assert.AreEqual(26, Evaluator.Evaluate("2+3*(3+5)", s => 0));
            Formula f = new("2+3*(3+5)");
            Assert.AreEqual(26.0, f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("17")]
        public void TestComplexAndParentheses()
        {
            //Assert.AreEqual(194, Evaluator.Evaluate("2+3*5+(3+4*8)*5+2", s => 0));
            Formula f = new("2+3*5+(3+4*8)*5+2");
            Assert.AreEqual(194.0, f.Evaluate(s => 0));
        }


        /// <summary>
        ///  Tests below test for FormulaError returns or invalid syntax during construction.
        ///  These tests were also provided as part of the PS1 grade tests.
        /// </summary>
        [TestMethod(), Timeout(5000)]
        [TestCategory("18")]
        public void TestDivideByZero()
        {
            //Evaluator.Evaluate("5/0", s => 0);
            Formula f = new("5/0");
            //Assert.IsTrue(f.Evaluate(s => 0).GetType() is FormulaError);
            Assert.IsInstanceOfType(
                f.Evaluate(s => { throw new ArgumentException("Unknown variable"); }), typeof(FormulaError));

        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("19")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSingleOperator()
        {
            Formula f = new("+");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ConstructorDoesntAcceptWrongOrderOfParenthesis()
        {
            Formula f = new("1+)2-2(+1");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestEmptyParenthesis()
        {
            Formula f = new("()");
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("20")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestExtraOperator()
        {
            Formula f = new("2+5+");
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("21")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestExtraParentheses()
        {
            Formula f = new("2+5*7)");
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("24")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestParensNoOperator()
        {
            Formula f = new("5+7+(5)8");
        }


        [TestMethod(), Timeout(5000)]
        [TestCategory("25")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestEmpty()
        {
            Formula f = new("");
        }

        /// <summary>
        /// Complex evaluations.
        /// These tests also ripped from PS1.
        /// </summary>
        [TestMethod(), Timeout(5000)]
        [TestCategory("26")]
        public void TestComplexMultiVar()
        {
            //Assert.AreEqual(6, Evaluator.Evaluate("y1*3-8/2+4*(8-9*2)/14*x7", s => (s == "x7") ? 1 : 4));
            Formula f = new("y1*3-8/2+4*(8-9*2)/14*x7");
            Assert.AreEqual(5.142857142857142, f.Evaluate(s => (s == "x7") ? 1 : 4));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("27")]
        public void TestComplexNestedParensRight()
        {
            //Assert.AreEqual(6, Evaluator.Evaluate("x1+(x2+(x3+(x4+(x5+x6))))", s => 1));
            Formula f = new("x1+(x2+(x3+(x4+(x5+x6))))");
            Assert.AreEqual(6.0, f.Evaluate(s => 1));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("28")]
        public void TestComplexNestedParensLeft()
        {
            //Assert.AreEqual(12, Evaluator.Evaluate("((((x1+x2)+x3)+x4)+x5)+x6", s => 2));
            Formula f = new("((((x1+x2)+x3)+x4)+x5)+x6");
            Assert.AreEqual(12.0, f.Evaluate(s => 2));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("29")]
        public void TestRepeatedVar()
        {
            //Assert.AreEqual(0, Evaluator.Evaluate("a4-a4*a4/a4", s => 3));
            Formula f = new("a4-a4*a4/a4");
            Assert.AreEqual(0.0, f.Evaluate(s => 3));
        }

    }
}
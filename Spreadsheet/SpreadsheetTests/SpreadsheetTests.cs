// These tests are for private use only
// Redistributing this file is strictly against SoC policy.

using SS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System.Linq;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace GradingTests
{


    /// <summary>
    ///This is a test class for SpreadsheetTest and is intended
    ///to contain all SpreadsheetTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SpreadsheetTest
    {

        // EMPTY SPREADSHEETS
        [TestMethod(), Timeout(2000)]
        [TestCategory("2")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestEmptyGetContents()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.GetCellContents("1AA");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("3")]
        public void TestGetEmptyContents()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            Assert.AreEqual("", s.GetCellContents("A2"));
        }

        // SETTING CELL TO A DOUBLE
        [TestMethod(), Timeout(2000)]
        [TestCategory("5")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetInvalidNameDouble()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("1A1A", "1.5");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("6")]
        public void TestSimpleSetDouble()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("Z7", "1.5");
            Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
        }

        // SETTING CELL TO A STRING
        [TestMethod(), Timeout(2000)]
        [TestCategory("9")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleString()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("1AZ", "hello");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("10")]
        public void TestSetGetSimpleString()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("Z7", "hello");
            Assert.AreEqual("hello", s.GetCellContents("Z7"));
        }

        // SETTING CELL TO A FORMULA
        [TestMethod(), Timeout(2000)]
        [TestCategory("13")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleForm()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("1AZ", "=2");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("14")]
        public void TestSetGetForm()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("Z7", "=3");
            Formula f = (Formula)s.GetCellContents("Z7");
            Assert.AreEqual(new Formula("3"), f);
            Assert.AreNotEqual(new Formula("2"), f);
        }

        // CIRCULAR FORMULA DETECTION
        [TestMethod(), Timeout(2000)]
        [TestCategory("15")]
        [ExpectedException(typeof(CircularException))]
        public void TestSimpleCircular()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A1");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("16")]
        [ExpectedException(typeof(CircularException))]
        public void TestComplexCircular()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A3", "=A4+A5");
            s.SetContentsOfCell("A5", "=A6+A7");
            s.SetContentsOfCell("A7", "=A1+A1");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("17")]
        [ExpectedException(typeof(CircularException))]
        public void TestUndoCircular()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            try
            {
                s.SetContentsOfCell("A1", "=A2+A3");
                s.SetContentsOfCell("A2", "15");
                s.SetContentsOfCell("A3", "30");
                s.SetContentsOfCell("A2", "=A3*A1");
            }
            catch (CircularException e)
            {
                Assert.AreEqual(15, (double)s.GetCellContents("A2"), 1e-9); // fails here
                throw e;
            }
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("17b")]
        [ExpectedException(typeof(CircularException))]
        public void TestUndoCellsCircular()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            try
            {   // intentionally creates a circular dependency that throws a CircularException
                s.SetContentsOfCell("A1", "=A2");
                s.SetContentsOfCell("A2", "=A1");
            }
            catch (CircularException e)
            {   // cell "A2"'s contents should be void because of the CircularException.
                // should be easy as returning an unassigned cell should return an empty string
                Assert.AreEqual("", s.GetCellContents("A2"));   // fails here
                Assert.IsTrue(new HashSet<string> { "A1" }.SetEquals(s.GetNamesOfAllNonemptyCells()));
                throw e;
            }
        }

        // NONEMPTY CELLS
        [TestMethod(), Timeout(2000)]
        [TestCategory("18")]
        public void TestEmptyNames()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("19")]
        public void TestExplicitEmptySet()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("B1", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("20")]
        public void TestSimpleNamesString()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("21")]
        public void TestSimpleNamesDouble()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("B1", "52.25");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("22")]
        public void TestSimpleNamesFormula()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("B1", "=3.5");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("23")]
        public void TestMixedNames()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "hello");
            s.SetContentsOfCell("B1", "=3.5");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "A1", "B1", "C1" }));
        }

        // RETURN VALUE OF SET CELL CONTENTS
        [TestMethod(), Timeout(2000)]
        [TestCategory("24")]
        public void TestSetSingletonDouble()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("A1", "17.2").SequenceEqual(new List<string>() { "A1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("25")]
        public void TestSetSingletonString()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("B1", "hello").SequenceEqual(new List<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("26")]
        public void TestSetSingletonFormula()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(s.SetContentsOfCell("C1", "=5").SequenceEqual(new List<string>() { "C1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("27")]
        public void TestSetChain()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "6");
            s.SetContentsOfCell("A3", "=A2+A4");
            s.SetContentsOfCell("A4", "=A2+A5");
            Assert.IsTrue(s.SetContentsOfCell("A5", "82.5").SequenceEqual(new List<string>() { "A5", "A4", "A3", "A1" }));
        }

        // CHANGING CELLS
        [TestMethod(), Timeout(2000)]
        [TestCategory("28")]
        public void TestChangeFtoD()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "2.5");
            Assert.AreEqual(2.5, (double)s.GetCellContents("A1"), 1e-9);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("29")]
        public void TestChangeFtoS()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "Hello");
            Assert.AreEqual("Hello", (string)s.GetCellContents("A1"));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("30")]
        public void TestChangeStoF()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "Hello");
            s.SetContentsOfCell("A1", "=23");
            Assert.AreEqual(new Formula("23"), (Formula)s.GetCellContents("A1"));
            Assert.AreNotEqual(new Formula("24"), (Formula)s.GetCellContents("A1"));
        }

        // STRESS TESTS
        [TestMethod(), Timeout(2000)]
        [TestCategory("31")]
        public void TestStress1()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            s.SetContentsOfCell("A1", "=B1+B2");
            s.SetContentsOfCell("B1", "=C1-C2");
            s.SetContentsOfCell("B2", "=C3*C4");
            s.SetContentsOfCell("C1", "=D1*D2");
            s.SetContentsOfCell("C2", "=D3*D4");
            s.SetContentsOfCell("C3", "=D5*D6");
            s.SetContentsOfCell("C4", "=D7*D8");
            s.SetContentsOfCell("D1", "=E1");
            s.SetContentsOfCell("D2", "=E1");
            s.SetContentsOfCell("D3", "=E1");
            s.SetContentsOfCell("D4", "=E1");
            s.SetContentsOfCell("D5", "=E1");
            s.SetContentsOfCell("D6", "=E1");
            s.SetContentsOfCell("D7", "=E1");
            s.SetContentsOfCell("D8", "=E1");
            IList<String> cells = s.SetContentsOfCell("E1", "0");
            Assert.IsTrue(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }.SetEquals(cells));
        }

        // Repeated for extra weight
        [TestMethod(), Timeout(2000)]
        [TestCategory("32")]
        public void TestStress1a()
        {
            TestStress1();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("33")]
        public void TestStress1b()
        {
            TestStress1();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("34")]
        public void TestStress1c()
        {
            TestStress1();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("35")]
        public void TestStress2()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            ISet<String> cells = new HashSet<string>();
            for (int i = 1; i < 200; i++)
            {
                cells.Add("A" + i);
                Assert.IsTrue(cells.SetEquals(s.SetContentsOfCell("A" + i, "=A" + (i + 1))));
            }
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("36")]
        public void TestStress2a()
        {
            TestStress2();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("37")]
        public void TestStress2b()
        {
            TestStress2();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("38")]
        public void TestStress2c()
        {
            TestStress2();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("39")]
        public void TestStress3()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");
            for (int i = 1; i < 200; i++)
            {
                s.SetContentsOfCell("A" + i, "=A" + (i + 1));
            }
            try
            {
                s.SetContentsOfCell("A150", "=A50");
                Assert.Fail();
            }
            catch (CircularException)
            {
            }
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("40")]
        public void TestStress3a()
        {
            TestStress3();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("41")]
        public void TestStress3b()
        {
            TestStress3();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("42")]
        public void TestStress3c()
        {
            TestStress3();
        }

        //[TestMethod(), Timeout(2000)]
        [TestMethod()]
        [TestCategory("43")]
        public void TestStress4()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "default");

            // adding lots of cells and dependencies with those cells
            for (int i = 0; i < 500; i++)   // i < 500 in original
            {
                s.SetContentsOfCell("A1" + i, "=A1" + (i + 1));
            }

            LinkedList<string> firstCells = new LinkedList<string>();   // dependee cells
            LinkedList<string> lastCells = new LinkedList<string>();    // dependent cells

            // simulates adding without using the spreadsheet
            for (int i = 0; i < 250; i++)    // i < 250 in original
            {
                firstCells.AddFirst("A1" + i);
                lastCells.AddFirst("A1" + (i + 250)); // i + 250 in original
            }

            // ensures the spreadsheet matches the expected sequence
            // remember first cells are dependee cells,
            // and lastCells are dependent cells.
            Assert.IsTrue(s.SetContentsOfCell("A1249",  // name corresponds to some final dependent
                "25.0").SequenceEqual(firstCells));
            Assert.IsTrue(s.SetContentsOfCell("A1499",  // name corresponds to some final dependee
                "0").SequenceEqual(lastCells));
        }
        /// <summary>
        /// Stress test for Save and four parameter constructor methods
        /// </summary>
        [TestMethod, Timeout(2000)]
        [TestCategory("44")]
        public void StressTest5()
        {
            Spreadsheet s = new();

            for (int i = 0; i < 200; i++)   // i < 500 in original
            {
                s.SetContentsOfCell("A1" + i, "=A1" + (i + 1));
            }

            s.Save("valid.json");
            Spreadsheet t = new("valid.json", s => true, s => s, "default");
            Assert.IsTrue(s.SetContentsOfCell("A1199", "0").SequenceEqual(t.SetContentsOfCell("A1199", "0")));
            File.Delete("valid.json");
        }
        /// <summary>
        /// Stress test for Save and four parameter constructor methods
        /// </summary>
        [TestMethod, Timeout(2000)]
        [TestCategory("45")]
        public void StressTest5a()
        {
            StressTest5();
        }
        /// <summary>
        /// Stress test for Save and four parameter constructor methods
        /// </summary>
        [TestMethod, Timeout(2000)]
        [TestCategory("46")]
        public void StressTest5b()
        {
            StressTest5();
        }
        /// <summary>
        /// Stress test for Save and four parameter constructor methods
        /// </summary>
        [TestMethod, Timeout(2000)]
        [TestCategory("47")]
        public void StressTest5c()
        {
            StressTest5();
        }

        /// <summary>
        /// Serialized Json strings of identical objects should be the same
        /// </summary>
        [TestMethod]
        [TestCategory("48")]
        public void CompareIdenticalJsons()
        {
            Spreadsheet ss = new(s => true, s => s, "default");
            Spreadsheet tt = new(s => true, s => s, "default");
            
            ss.SetContentsOfCell("A1", "=A2+A3");
            ss.SetContentsOfCell("A2", "6");
            ss.SetContentsOfCell("A3", "=A2+A4");
            ss.SetContentsOfCell("A4", "=A2+A5");
            tt.SetContentsOfCell("A1", "=A2+A3");
            tt.SetContentsOfCell("A2", "6");
            tt.SetContentsOfCell("A3", "=A2+A4");
            tt.SetContentsOfCell("A4", "=A2+A5");

            string ssJson = JsonConvert.SerializeObject(ss);
            string ttJson = JsonConvert.SerializeObject(tt);
            Assert.IsTrue(ssJson == ttJson);
        }

        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("49")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void EmptySaveFileThrows()
        {
            Spreadsheet s = new();
            s.Save(""); // should throw here
            File.Delete("");    // used if not thrown to reduce clutter
        }
        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("50")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void InvalidSubdirectorySaveThrows()
        {
            Spreadsheet s = new();
            s.Save("does/not/exist/thing.json");         // should throw here
            File.Delete("does/not/exist/thing.json");    // used if not thrown to reduce clutter

        }
        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("51")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void WrongPositionForJsonFiletypeThrows()
        {
            Spreadsheet s = new();
            s.Save("t.jsonblahblah");         // should throw here
            File.Delete("t.jsonblahblah");    // used if not thrown to reduce clutter
        }
        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("52")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void JsonDotJsonThrows()
        {
            Spreadsheet s = new();
            s.Save(".jsonjson");         // should throw here
            File.Delete(".jsonjson");    // used if not thrown to reduce clutter
        }
        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("53")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void JsonJsonThrows()
        {
            Spreadsheet s = new();
            s.Save("jsonjson");         // should throw here
            File.Delete("jsonjson");    // used if not thrown to reduce clutter
        }
        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("54")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void InequalVersionThrows()
        {
            Spreadsheet s = new();
            s.Save("valid.json");
            try
            {
                s = new("valid.json", s => true, s => s, "not default");
            }
            catch (Exception e)
            {
                File.Delete("valid.json");
                throw e;
            }
        }
        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("55")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void InvalidJsonFileConstructionThrows()
        {
            File.WriteAllText("valid.json", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            // I am in immense pain.
            try
            {
                Spreadsheet s = new("valid.json", s => true, s => s, "default");
            }
            catch (Exception e)
            {
                File.Delete("valid.json");
                throw e;
            }
        }
        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("56")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void EmptyJsonFileConstructionThrows()
        {
            File.WriteAllText("valid.json", "");
            try
            {
                Spreadsheet s = new("valid.json", s => true, s => s, "default");
            }
            catch (Exception e)
            {
                File.Delete("valid.json");
                throw e;
            }
        }
        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("57")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void UnhappyValidatorWhenReadingSpreadsheetJson()
        {
            Spreadsheet ss = new();
            ss.SetContentsOfCell("A1", "=A2+A3");
            ss.SetContentsOfCell("A2", "6");
            ss.SetContentsOfCell("A3", "=A2+A4");
            ss.Save("valid.json");
            try
            {
                ss = new("valid.json", s => false, s => s, "default");
            }
            catch (Exception e)
            {
                File.Delete("valid.json");
                throw e;
            }
        }
        /// <summary>
        /// Tests for incorrect saves
        /// </summary>
        [TestMethod]
        [TestCategory("58")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void ArtificiallyConstructedInvalidJsonThrows()
        {
            string written = "{\r\n  \"cells\": {\r\n    \"25\": {\r\n      \"stringForm\": \"=A2+A3\"\r\n    },\r\n    \"A2\": {\r\n      \"stringForm\": \"6\"\r\n    },\r\n    \"A3\": {\r\n      \"stringForm\": \"=A2+A4\"\r\n    }\r\n  },\r\n  \"Version\": \"default\"\r\n}";
            // apologies for the long string. the full string is one I ripped from a json string written using
            // my Spreadsheet.Save() method. I just rewrote it to change a variable name to be invalid.
            File.WriteAllText("valid.json", written);
            try
            {
                Spreadsheet ss = new("valid.json", s => true, s => s, "default");
            }
            catch (Exception e)
            {
                File.Delete("valid.json");
                throw e;
            }
        }

        /// <summary>
        /// Tests for correct saves after serialization
        /// </summary>
        [TestMethod]
        [TestCategory("59")]
        public void DeserializedSavedObjectIsCorrect()
        {
            Spreadsheet ss = new(s => true, s => s, "default");
            Spreadsheet tt = new(s => true, s => s, "default");

            ss.SetContentsOfCell("A1", "=A2+A3");
            ss.SetContentsOfCell("A2", "6");
            ss.SetContentsOfCell("A3", "=A2+A4");
            tt.SetContentsOfCell("A1", "=A2+A3");
            tt.SetContentsOfCell("A2", "6");
            tt.SetContentsOfCell("A3", "=A2+A4");

            string ssJson = JsonConvert.SerializeObject(ss);
            ss.Save("valid.json");

            ss = new("valid.json", s => true, s => s, "default");
            File.Delete("valid.json");    // used if not thrown to reduce clutter
            Assert.IsTrue(ss.SetContentsOfCell("A4", "=A2+A5").SequenceEqual(tt.SetContentsOfCell("A4", "=A2+A5")));
        }
        /// <summary>
        /// Tests for correct saves after serialization
        /// </summary>
        [TestMethod]
        [TestCategory("60")]
        public void SavingToPreviouslyAssignedJsonOverrides()
        {
            Spreadsheet ss = new();
            Spreadsheet tt = new();
            
            ss.SetContentsOfCell("A2", "2");
            ss.Save("valid.json");
            tt.Save("valid.json");

            Spreadsheet fromJson = new("valid.json", s => true, s => s, "default");
            File.Delete("valid.json");
            Assert.AreEqual("", fromJson.GetCellContents("A2"));
            Assert.AreEqual("", fromJson.GetCellValue("A2"));
        }
        /// <summary>
        /// Tests for correct saves after serialization
        /// </summary>
        [TestMethod]
        [TestCategory("61")]
        public void JsonDotJsonDoesntThrow()
        {
            Spreadsheet ss = new();
            ss.Save("json.json");
            ss = new("json.json", s => true, s => s, "default");
            File.Delete("json.json");
        }


        /// <summary>
        /// Test adds a dependency, replaces a cell's contents with a new one, and then makes sure the dependency is gone
        /// </summary>
        [TestMethod]
        [TestCategory("62")]
        public void ReplacingCellWithNewContentsChangesDependencies()
        {
            Spreadsheet ss = new();
            ss.SetContentsOfCell("A4", "=A2+5");
            ss.SetContentsOfCell("A2", "=0-5");
            Assert.AreEqual(0.0, ss.GetCellValue("A4"));

            Assert.IsTrue(ss.SetContentsOfCell("A4", "1").SequenceEqual(new List<string>() { "A4" }));
            Assert.IsTrue(ss.SetContentsOfCell("A2", "5").SequenceEqual(new List<string>() { "A2" }));
        }

        /// <summary>
        /// Determines validity of string vs Formula
        /// </summary>
        [TestMethod]
        [TestCategory("63")]
        public void StringNotFormula()
        {
            Spreadsheet ss = new();
            ss.SetContentsOfCell("A4", "A2+5");
            Assert.IsTrue(ss.GetCellContents("A4") is String);

            ss.SetContentsOfCell("A4", "=A2+5");
            Assert.IsTrue(ss.GetCellContents("A4") is Formula);
        }


        /// <summary>
        /// Tests for validator and/or normalizer
        /// </summary>
        [TestMethod]
        [TestCategory("64")]
        [ExpectedException(typeof(InvalidNameException))]
        public void ValidatorThrowsOnSet()
        {
            Spreadsheet ss = new(s => false, s => s, "default");
            ss.SetContentsOfCell("A2", "101010");
        }
        /// <summary>
        /// Tests for validator and/or normalizer
        /// </summary>
        [TestMethod]
        [TestCategory("65")]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidNormalizerThrowsOnSet()
        {
            Spreadsheet ss = new(s => { return s != "A2"; }, s => "A2", "default");
            ss.SetContentsOfCell("x1", "101010");
        }
        /// <summary>
        /// Tests for validator and/or normalizer
        /// </summary>
        [TestMethod]
        [TestCategory("66")]
        public void PostNormalizedVariableBecomesNormalizedName()
        {
            Spreadsheet ss = new(s => true, s => "A5", "default");
            ss.SetContentsOfCell("x1", "0");
            Assert.AreEqual(0.0, ss.GetCellContents("A5"));
        }
        /// <summary>
        /// Tests for validator and/or normalizer
        /// </summary>
        [TestMethod]
        [TestCategory("67")]
        public void NormalizerMakesNameValid()
        {
            Spreadsheet ss = new(s => true, s => { return "A" + s; }, "default");
            ss.SetContentsOfCell("25", "0");
            Assert.AreEqual(0.0, ss.GetCellContents("25"));
        }


        /// <summary>
        /// Tests for validator and/or normalizer
        /// </summary>
        [TestMethod]
        [TestCategory("68")]
        [ExpectedException(typeof(InvalidNameException))]
        public void ValidatorThrowsOnGetContents()
        {
            Spreadsheet ss = new(s => false, s => s, "default");
            ss.GetCellContents("A2");
        }
        /// <summary>
        /// Tests for validator and/or normalizer
        /// </summary>
        [TestMethod]
        [TestCategory("69")]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidNormalizerThrowsOnGetContents()
        {
            Spreadsheet ss = new(s => { return s != "A2"; }, s => "A2", "default");
            ss.GetCellContents("x1");
        }


        /// <summary>
        /// Tests for validator and/or normalizer
        /// </summary>
        [TestMethod]
        [TestCategory("70")]
        [ExpectedException(typeof(InvalidNameException))]
        public void ValidatorThrowsOnGetValue()
        {
            Spreadsheet ss = new(s => false, s => s, "default");
            ss.GetCellValue("A2");
        }
        /// <summary>
        /// Tests for validator and/or normalizer
        /// </summary>
        [TestMethod]
        [TestCategory("71")]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidNormalizerThrowsOnGetValue()
        {
            Spreadsheet ss = new(s => { return s != "A2"; }, s => "A2", "default");
            ss.GetCellValue("x1");
        }


    }
}
using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace SS
{
    // Modified for PS5.
    /// <summary>
    /// A spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a cell name if and only if it consists of one or more letters,
    /// followed by one or more digits AND it satisfies the predicate IsValid.
    /// For example, "A15", "a15", "XY032", and "BC7" are cell names so long as they
    /// satisfy IsValid.  On the other hand, "Z", "X_", and "hello" are not cell names,
    /// regardless of IsValid.
    /// 
    /// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
    /// must be normalized with the Normalize method before it is used by or saved in 
    /// this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
    /// the Formula "x3+a5" should be converted to "X3+A5" before use.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  
    /// In addition to a name, each cell has a contents and a value.  The distinction is
    /// important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {

        /*
        // ADDED FOR PS5
        /// <summary>
        /// Method used to determine whether a string that consists of one or more letters
        /// followed by one or more digits is a valid variable name.
        /// </summary>
        =-------public Func<string, bool> IsValid { get; protected set; }---------=
        */

        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }

        /// <summary>
        /// Set of Cells in the spreadsheet. See Cell documentation for more info.
        /// </summary>
        [JsonProperty(PropertyName = "cells")]
        private Dictionary<String, Cell> cells;

        /// <summary>
        /// String form of Cells in the spreadsheet. Used when serializing and deserializing.
        /// </summary>


        /// <summary>
        /// graph maintaining dependencies between Cell objects in the Spreadsheet.
        /// See DependencyGraph documentation for more info.
        /// </summary>
        private DependencyGraph graph;
        /// <summary>
        /// Three argument constructor. 
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            cells = new();
            graph = new();
            Changed = false;
        }

        /// <summary>
        /// No argument constructor. Validates every otherwise valid variable and normalizes names to self.
        /// No reference to other files.
        /// </summary>
        public Spreadsheet()
            : this(s => true, s => s, "default")
        {
        }

        /// <summary>
        /// Four argument constructor. Same as three argument constructor, aside from a new first parameter.
        /// First parameter allows for reading Json files.
        /// </summary>

        public Spreadsheet(string path, Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            Spreadsheet? inside;
            try
            {
                string json = File.ReadAllText(path);
                inside = JsonConvert.DeserializeObject<Spreadsheet>(json);
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Error when reading or opening the file.");
            }

            if (inside is null)
                throw new SpreadsheetReadWriteException("Json string not recognized when deserializing Spreadsheet object");

            if (inside.Version != version)
                throw new SpreadsheetReadWriteException("Error when deserializing Spreadsheet Json: Version doesn't match.");

            cells = new();
            graph = new();
            Changed = false;
            foreach (string cellname in inside.cells.Keys)
            {
                string normalized = Normalize(cellname);
                // cellname is the interior Spreadsheet's cell name,
                // and normalized is the used cell name in THIS Spreadsheet being constructed.
                if (VerifyVariable(normalized))
                {
                    if (IsValid(normalized))
                        SetContentsOfCell(normalized, inside.cells[cellname].StringForm);

                    else
                        throw new SpreadsheetReadWriteException("Read cellname is no longer valid.");

                }
                else
                    throw new SpreadsheetReadWriteException("Read cellname does not pass as a valid variable.");
            }
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            HashSet<string> names = new HashSet<string>();
            foreach (string cellname in cells.Keys)
            {
                names.Add(cellname);
            }
            return names;
        }


        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            if (VerifyVariable(name))
                if (IsValid(name))
                {
                    if (cells.ContainsKey(name))
                        return cells[name].Contents;
                    return "";
                }
            throw new InvalidNameException();
        }

        // MODIFIED PROTECTION FOR PS5
        /// <summary>
        /// The contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, double number)
        {
            CheckForCircular(name, number.ToString());
            Cell newcell = new(name, number);
            cells[name] = newcell;
            Changed = true;
            // no dependencies added as doubles do not depend on other values

            IList<string> toBeUpdated = GetAllDependencies(name);

            return toBeUpdated;
        }

        // MODIFIED PROTECTION FOR PS5
        /// <summary>
        /// The contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, string text)
        {

            if (text != "")
            {
                CheckForCircular(name, text);
                Cell newcell = new(name, text);
                cells[name] = newcell;
                Changed = true;

            }
            else
            {
                cells.Remove(name);
            }
            IList<string> toBeUpdated = GetAllDependencies(name);
            return toBeUpdated;
        }

        // MODIFIED PROTECTION FOR PS5
        /// <summary>
        /// If changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula. The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            CheckForCircular(name, formula.ToString());
            Cell newcell = new(name, formula);
            newcell.Value = formula.Evaluate(s => (double)GetCellValue(s));
            cells[name] = newcell;
            Changed = true;

            // search for any dependencies and add those to the dependency graph
            return GetAllDependencies(name);
        }
        /// <summary>
        /// Checks for circular dependencies with a proposed new string value for a cell.
        /// </summary>
        /// <param name="name">variable name of cell</param>
        /// <param name="proposed">string value of cell</param>
        /// <exception cref="CircularException">thrown if the method encounters a circular dependency</exception>
        private void CheckForCircular(string name, string proposed)
        {
            try
            {
                graph.ReplaceDependents(name, proposed.GetVariables());
                GetCellsToRecalculate(name);
            }
            catch
            {
                graph.ReplaceDependents(name, GetDirectDependents(name));
                throw new CircularException();
            }
            foreach (string token in proposed.GetVariables())
            {
                if (!IsValid(token))
                    throw new FormulaFormatException("Invalid variable name used in formula.");
            }
        }

        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using a JSON format.
        /// The JSON object should have the following fields:
        /// "Version" - the version of the spreadsheet software (a string)
        /// "cells" - an object containing 0 or more cell objects
        ///           Each cell object has a field named after the cell itself 
        ///           The value of that field is another object representing the cell's contents
        ///               The contents object has a single field called "stringForm",
        ///               representing the string form of the cell's contents
        ///               - If the contents is a string, the value of stringForm is that string
        ///               - If the contents is a double d, the value of stringForm is d.ToString()
        ///               - If the contents is a Formula f, the value of stringForm is "=" + f.ToString()
        /// 
        /// For example, if this spreadsheet has a version of "default" 
        /// and contains a cell "A1" with contents being the double 5.0 
        /// and a cell "B3" with contents being the Formula("A1+2"), 
        /// a JSON string produced by this method would be:
        /// 
        /// {
        ///   "cells": {
        ///     "A1": {
        ///       "stringForm": "5"
        ///     },
        ///     "B3": {
        ///       "stringForm": "=A1+2"
        ///     }
        ///   },
        ///   "Version": "default"
        /// }
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            /*
             * FILE.WRITEALLTEXT DOCUMENTATION FROM METADATA
             * Summary:
             *     Creates a new file, writes the specified string to the file, and then closes
             *     the file. If the target file already exists, it is overwritten.
             *
             * Parameters:
             *   path:
             *     The file to write to.
             *
             *   contents:
             *     The string to write to the file.
             *
             * Exceptions:
             *   T:System.ArgumentException:
             *     .NET Framework and .NET Core versions older than 2.1: path is a zero-length string,
             *     contains only white space, or contains one or more invalid characters. You can
             *     query for invalid characters by using the System.IO.Path.GetInvalidPathChars
             *     method.
             *
             *   T:System.ArgumentNullException:
             *     path is null.
             *
             *   T:System.IO.PathTooLongException:
             *     The specified path, file name, or both exceed the system-defined maximum length.
             *
             *   T:System.IO.DirectoryNotFoundException:
             *     The specified path is invalid (for example, it is on an unmapped drive).
             *
             *   T:System.IO.IOException:
             *     An I/O error occurred while opening the file.
             *
             *   T:System.UnauthorizedAccessException:
             *     path specified a file that is read-only. -or- path specified a file that is hidden.
             *     -or- This operation is not supported on the current platform. -or- path specified
             *     a directory. -or- The caller does not have the required permission.
             *
             *   T:System.NotSupportedException:
             *     path is in an invalid format.
             *
             *   T:System.Security.SecurityException:
             *     The caller does not have the required permission.
            */

            string serialized = JsonConvert.SerializeObject(this, Formatting.Indented);
            bool beforeSave = Changed;
            try
            {
                Changed = false;
                File.WriteAllText(filename, serialized);
            }
            catch
            {
                Changed = beforeSave;
                throw new SpreadsheetReadWriteException("Error when saving Spreadsheet Json to specified path: " + filename);
            }
        }

        // ADDED FOR PS5
        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            name = Normalize(name);
            if (VerifyVariable(name))
                if (IsValid(name))
                {
                    if (cells.ContainsKey(name))
                        return cells[name].Value;
                    return "";
                }
            throw new InvalidNameException();
        }

        // ADDED FOR PS5
        /// <summary>
        /// Otherwise, if name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            name = Normalize(name);
            if (!VerifyVariable(name))
                throw new InvalidNameException();
            if (!IsValid(name))
                throw new InvalidNameException();

            IList<string> toChange;
            if (Double.TryParse(content, out double usedDouble))    // double case
            {
                toChange = SetCellContents(name, usedDouble);
                changeHelper(toChange);
                return toChange;
            }
            else if (content.Length > 0)                            // formula case
                if (content[0] == '=')
                {
                    content = content.Remove(0, 1);
                    Formula usedFormula = new(content);
                    toChange = SetCellContents(name, usedFormula);  // throws on return
                    changeHelper(toChange);
                    return toChange;
                }
            // string case
            toChange = SetCellContents(name, content);
            changeHelper(toChange);
            return toChange;
        }

        /// <summary>
        /// Helper method for adding. Updates Formula values and does other responsibilities.
        /// </summary>
        private void changeHelper(IList<string> toChange)
        {
            foreach (string updatedName in toChange)
                if (cells[updatedName].Contents is Formula)
                    cells[updatedName].Value =
                        ((Formula)cells[updatedName].Contents).Evaluate(s => (double)GetCellValue(s));
        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return graph.GetDependees(name);
        }


        //  PRIVATE HELPER METHODS

        /// <summary>
        /// Validates variable names. Helper method adopted from Formula class.
        /// </summary>
        private static bool VerifyVariable(String token)
        {
            return Regex.IsMatch(token, "^(_|[a-zA-Z])([a-zA-Z]*|_*|[0-9]*)");
        }

        /// <summary>
        /// Given a cell name "name", returns all of the direct and indirect dependencies to the cell.
        /// </summary>
        /// <param name="name">variable of the cell to be used in computation.</param>
        /// <returns>IList relating to all direct and indirect dependencies to string name.</returns>
        private IList<string> GetAllDependencies(string name)
        {
            IList<string> dependencies = new List<string>();
            if (GetCellContents(name) is String)
            {
                if ((string)GetCellContents(name) == "")
                    return dependencies;
                    
            }
            foreach (string cell in GetCellsToRecalculate(name))
            {
                dependencies.Add(cell);
            }
            return dependencies;
        }
    }

    /// <summary>
    /// An element of a Spreadsheet.
    /// A cell's values can be a string, a double, Formula objects, or FormulaError objects.
    /// The spreadsheet can be thought of as a relation between string cell names,
    /// and their contents (values) of strings, doubles, Formulas, or FormulaErrors.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Cell
    {
        /// <summary>
        /// String representation for cell. most critical when serializing Spreadsheet data.
        /// </summary>
        [JsonProperty(PropertyName = "stringForm")]
        readonly string pStringForm;
        public string StringForm { get { return pStringForm; } }
        /// <summary>
        /// String, Double, or Formula object. Not evaluated yet.
        /// </summary>
        public object Contents { get; }
        /// <summary>
        /// String, Double, or FormulaError object.
        /// Post evaluation if there is any to do.
        /// </summary>
        public object Value { set; get; }
        /// <summary>
        /// Cell constructor for string contents.
        /// Contents and Value should remain as string as long as this Cell instance is used.
        /// </summary>
        public Cell(string name, string contents)
        {
            Contents = contents;
            Value = contents;
            pStringForm = contents;
        }
        /// <summary>
        /// Cell constructor for double contents.
        /// Contents and Value should remain as double as long as this Cell instance is used.
        /// </summary>
        public Cell(string name, double contents)
        {
            Contents = contents;
            Value = contents;
            pStringForm = contents.ToString();
        }
        /// <summary>
        /// Cell constructor for Formula contents.
        /// Contents should remain as the same Formula object,
        /// but Value should be able to change whenever the Spreadsheet using this (valid) Cell instance requires.
        /// </summary>
        public Cell(string name, Formula contents)
        {
            Contents = contents;
            Value = contents;
            pStringForm = "=" + contents.ToString();
        }
        /// <summary>
        /// Default constructor used in deserialization.
        /// </summary>
        public Cell()   // ASK TAs ABOUT THIS CONSTRUCTOR
        {
            Contents = "";
            Value = "";
            pStringForm = "";
        }
    }

    /// <summary>
    /// extension class intended for ease of use with
    /// AddDependencies(string, string) helper method in Spreadsheet class.
    /// All extension methods are ripped from previous Formula methods,
    /// and adopted to be used with the string data type.
    /// </summary>
    static class StringExtensions
    {
        public static IEnumerable<string> GetVariables(this string formula)
        {
            IEnumerable<String> e = GetTokens(formula);
            HashSet<String> returned = new();
            foreach (String token in e)
            {
                if (VerifyVariable(token))
                {   // statement implies current token is a variable
                    returned.Add(token);
                }
            }
            return returned;
        }

        private static bool VerifyVariable(String token)
        {
            return Regex.IsMatch(token, "^(_|[a-zA-Z])([a-zA-Z]*|_*|[0-9]*)");
        }

        private static IEnumerable<string> GetTokens(String formula)
        // tokenizer taken from Formula skeleton code
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
}
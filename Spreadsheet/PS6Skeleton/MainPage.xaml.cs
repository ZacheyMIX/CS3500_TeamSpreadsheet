using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using SpreadsheetUtilities;
using SS;
using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{

    /// <summary>
    /// internal Spreadsheet model class. Represents the logic of the spreadsheet.
    /// </summary>
    private Spreadsheet model;

    /// <summary>
    /// used for when saving spreadsheet as to display warnings if a different spreadsheet would be overridden
    /// </summary>
    private string mostRecentSavePath;

    /// <summary>
    /// Constructor for the demo
    /// </summary>
	public MainPage()
    {
        InitializeComponent();

        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(2,3);
        SavePath.Text = "";
        model = new(s => Regex.IsMatch(s, @"^[A-Z][1-9][0-9]?$"), s => s.ToUpper(), "ps6");
        displaySelection(spreadsheetGrid);
        mostRecentSavePath = "";
    }

    /// <summary>
    /// Event for when a cell is selected.
    /// Clicking a cell has the same meaning as selecting a cell.
    /// </summary>
    private void displaySelection(SpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out int col, out int row); // gets positional data for selected column and row
        spreadsheetGrid.GetValue(col, row, out string value);   // gets value data for selected column and row
        char c = Convert.ToChar(col + 65);
        CellName.Text = c.ToString() + (row+1).ToString();
        CellValue.Text = value;

        if (CellValue.Text != "")
            CellContent.Text = model.GetCellContents(CellName.Text).ToString();
        else
            CellContent.Text = "";
    }

    /// <summary>
    /// Effectively creates a new spreadsheet by clearing all entries in the current SpreadsheetGrid.
    /// </summary>
    private async void NewClicked(Object sender, EventArgs e)
    {
        bool goAhead = true;
        if (model.Changed)
        {
            goAhead = await DisplayAlert("Current Spreadsheet Not Saved",
                "You are about to open a new spreadsheet without saving the previous one.",
                "Continue", "Abort");
        }
        if (goAhead)
        {
            spreadsheetGrid.Clear();                                                               // clears display
            model = new(s => Regex.IsMatch(s, @"^[A-Z][1-9][0-9]?$"), s => s.ToUpper(), "ps6");    // MAKE SURE TO CHANGE ON RELEASES
            SavePath.Text = "";
        }
    }

    /// <summary>
    /// Opens a .sprd file, changes model to that spreadsheet,
    /// and prints the first 100 chars of that file to the console.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                System.Diagnostics.Debug.WriteLine("Successfully chose file: " + fileResult.FileName);
                string fileContents = File.ReadAllText(fileResult.FullPath);
                try
                {
                    System.Diagnostics.Debug.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
                }
                catch
                {
                    model = new(s => Regex.IsMatch(s, "^[A-Z][1-9][0-9]?$"), s => s.ToUpper(), "ps6");
                    SpreadsheetGridChanger(model.GetNamesOfAllNonemptyCells());
                    SavePath.Text = fileResult.FullPath;
                    mostRecentSavePath = fileResult.FullPath;
                    return;
                }
                if (Regex.IsMatch(fileResult.FullPath, @"\.sprd$"))
                {
                    try
                    {
                        model = new(fileResult.FullPath, s => Regex.IsMatch(s, "^[A-Z][1-9][0-9]?$"), s => s.ToUpper(), "ps6");
                        SpreadsheetGridChanger(model.GetNamesOfAllNonemptyCells());
                        SavePath.Text = fileResult.FullPath;
                        mostRecentSavePath = fileResult.FullPath;
                    }

                    catch (Exception ex)
                    {
                        await DisplayAlert("Error opening File", "While file is a .sprd, there was trouble parsing it." + ex.ToString(), "OK");
                    }
                }

                else
                {
                    await DisplayAlert("Error opening File", "File " + fileResult.FullPath + " is not a valid .sprd file.", "OK");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No file selected.");
            }
            
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error opening file:");
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
    
    /// <summary>
    /// connects model and spreadsheetGrid to the current entry and cell input
    /// </summary>
    private async void ContentsChanged(Object sender, EventArgs e)
    {
        try
        {
            IList<string> toBeUpdated = model.SetContentsOfCell(CellName.Text, CellContent.Text);
            SpreadsheetGridChanger(toBeUpdated);
        }
        catch
        {
            await DisplayAlert("Invalid Entry", CellName.Text + " was changed to an invalid entry.", "OK");
            CellContent.Text = "";
        }
        displaySelection(spreadsheetGrid);
    }
    
    /// <summary>
    /// Standard "Save" functionality for spreadsheet programs.
    /// Should effectively save the current spreadsheet to a new file.
    /// </summary>
    private async void SaveClicked(Object sender, EventArgs e)
    {
        try
        {
            if (SavePath.Text == "")
            {
                await DisplayAlert("No File Specified", "", "OK");
            }
            else if (!Regex.IsMatch(SavePath.Text, @"\.sprd$"))
            {
                await DisplayAlert("Error Saving File", "File must be specified as a .sprd filetype", "OK");
            }
            else if (!Regex.IsMatch(SavePath.Text, @"^[A-Z]:\\") && !Regex.IsMatch(SavePath.Text, @"^/"))
            {
                if (SavePath.Text != mostRecentSavePath && mostRecentSavePath != "")
                {
                    if (!(await DisplayAlert("Possible Spreadsheet Override",
                        "The file you're about to save might override a different spreadsheet. Save anyway?",
                        "Yes", "No")))
                        return;
                }
                string execPath = AppDomain.CurrentDomain.BaseDirectory;
                SavePath.Text = execPath + SavePath.Text;
                model.Save(SavePath.Text);
                await DisplayAlert("Successfully Saved File", "File saved to path: " + SavePath.Text, "OK");
                mostRecentSavePath = SavePath.Text;
            }
            else
            {
                if (SavePath.Text != mostRecentSavePath && mostRecentSavePath != "")
                {
                    if (!(await DisplayAlert("Possible Spreadsheet Override",
                        "The file you're about to save might override a different spreadsheet. Save anyway?",
                        "Yes", "No")))
                        return;
                }
                model.Save(SavePath.Text);
                await DisplayAlert("Successfully Saved File", "File saved to path: " + SavePath.Text, "OK");
                mostRecentSavePath = SavePath.Text;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Saving File", "Error when saving file to path " + SavePath.Text + "\n" + ex.ToString(), "OK");
            System.Diagnostics.Debug.WriteLine("\nError saving to " + SavePath.Text);
        }
    }

    /// <summary>
    /// Universal changer for largescale alterations within the spreadsheet.
    /// </summary>
    /// <param name="set"> set of names of cells that need to be updated in the spreadsheet grid </param>
    private void SpreadsheetGridChanger(IEnumerable<string> set)
    {
        if(set.Count() == 0)
        {
            spreadsheetGrid.GetSelection(out int col, out int row);
            spreadsheetGrid.SetValue(col, row, "");
        }
        foreach (string cellname in set)
        {
            int letterIndex = char.ToUpper(cellname[0]) - 65;       // additional subtraction by 1 for indexing
            int numberIndex = int.Parse(cellname[1].ToString()) - 1;
            var value = model.GetCellValue(cellname);

            if (value is not FormulaError)  // otherwise this displays as a SpreadsheetUtilities.FormulaError and that's uggy
            {
                string input = model.GetCellValue(cellname).ToString();
                if (input.Length > 7)
                {
                    if (input.Contains("E"))
                    {
                        string firstHalf = input.Substring(0, 3);
                        string secondHalf = input.Split("E")[1];
                        spreadsheetGrid.SetValue(letterIndex, numberIndex, firstHalf + "E" + secondHalf);
                    }
                    else
                        spreadsheetGrid.SetValue(letterIndex, numberIndex, input.Substring(0, 7));
                }
                else
                    spreadsheetGrid.SetValue(letterIndex, numberIndex, input);
            }
            else
                spreadsheetGrid.SetValue(letterIndex, numberIndex, "FormErr");
        }
    }

    /// <summary>
    /// Changes SavePath entry text to the most recent save path in case the user forgot the current path or some other reason.
    /// SPECIAL FEATURE
    /// </summary>
    private void RefreshFilepath(Object sender, EventArgs e)
    {
        SavePath.Text = mostRecentSavePath;
    }

    // HELP MENU POPUPS

    /// <summary>
    /// Simply displays a popup for the help page for saving files.
    /// </summary>
    private async void SavingFilesPopup(Object sender, EventArgs e)
    {
        await DisplayAlert("Saving Files Help", "When saving files, your two main options are specifying a full filepath or" +
            " saving to the same directory your executable is stored. Whenever specifying the full filepath, the file is of course saved there," +
            " otherwise the filepath is left blank (aside from the name of the spreadsheet) and saved to your executable's directory." +
            " Keep in mind, too, that all spreadsheet filetypes must be .sprd when saving, or else the save will not be executed." +
            "\nIf, for some reason, you've forgotten the path to the file you're currently working on, and your filepath entry has changed," +
            " click on the Current Filepath button under the File menu.", "OK");
    }

    /// <summary>
    /// Simply displays a popup for the help page for opening files.
    /// </summary>
    private async void OpeningFilesPopup(Object sender, EventArgs e)
    {
        await DisplayAlert("Opening Files Help", "After clicking on the open files button, an operating system specific file manager will open" +
            " and you can select whichever .sprd file you would like to open. Make sure the filetype is .sprd, or else the file will not be recognized" +
            " and will not open.", "OK");
    }

    /// <summary>
    /// Simply displays a popup for the help page for creating new spreadsheets.
    /// </summary>
    private async void NewSpreadsheetPopup(Object sender, EventArgs e)
    {
        await DisplayAlert("Creating New Spreadsheets Help", "To create a new spreadsheet, simply click on the File dropdown menu and select New." +
            " If you have unsaved changes in your current working spreadsheet, there should be a popup menu asking you if you wish to continue." +
            " You can abort or continue the operation from here.", "OK");
    }

    /// <summary>
    /// Simply displays a popup for the help page for understanding cells.
    /// </summary>
    private async void UnderstandingCellsPopup(Object sender, EventArgs e)
    {
        await DisplayAlert("Understanding Cells Help", "At the top of the program, there should be a display containing the name of the currently highlighted cell," +
            " a display containing the value of the currently highlighted cell, and another display containing the contents of the currently highlighted cell." +
            " It should be noted by the user that only the contents entry can be altered, and alters the value of the selected cell. After entering in the desired contents" +
            " of your selected cell, either click off of the entry or press enter and the spreadsheet's data will reflect the change." +
            " This also changes all other cells that depend on that cell. Make sure to enter in valid entries, or the contents change will either give an error popup or display as a formula error.", "OK");
    }

    /// <summary>
    /// Simply displays a popup for the help page for understanding errors.
    /// </summary>
    private async void UnderstandingErrorsPopup(Object sender, EventArgs e)
    {
        await DisplayAlert("Understanding Errors Help", "A few different kinds of errors may show up in your use of the program. While typically descriptive of what went wrong within the title or message," +
            " these messages can still be very intimidating.\nThe first, and arguably most common error to look out for is saving your file incorrectly." +
            " Whenever this error is encountered, double check that the filepath is entered in correctly with no typos, and that the directory that you're trying to save the file to really exists." +
            "\nAnother kind of error relates to incorrectly input formulas. Ensure that all values are defined in case of formula error values, and ensure that no circular dependencies or syntax errors are present given a popup.", "OK");
    }

    private async void SpecialFeaturesPopup(Object sender, EventArgs e)
    {
        await DisplayAlert("Special Features!", 
            "While using our spreadsheet, you will find that the colors of the selected cell will change depending whether or not the cell is an odd or even cell"
            + "\nThats our first special feature!\nIf you ever somehow accidentally delete the current save path from the text box, fear not! If you use " +
            "the Current File Path in the file dropdown, it will bring the filepath back to you.\nAnd thats our second special feature!", "OK");
    }


}

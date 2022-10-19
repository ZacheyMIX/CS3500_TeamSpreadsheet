using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using SpreadsheetUtilities;
using SS;
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
        model = new(s => true, s => s, "ps6");
        displaySelection(spreadsheetGrid);
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
        System.Diagnostics.Debug.WriteLine(c);
        CellName.Text = c.ToString() + (row+1).ToString();
        CellValue.Text = value;

        if (CellValue.Text != "")
            CellContent.Text = model.GetCellContents(CellName.Text).ToString();
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
            spreadsheetGrid.Clear();                            // clears display
            model = new(s => true, s => s, "ps6");              // MAKE SURE TO CHANGE ON RELEASES
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
                System.Diagnostics.Debug.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
                if (Regex.IsMatch(fileResult.FullPath, @"\.sprd$"))
                {
                    try
                    {
                        model = new(fileResult.FullPath, s => true, s => s, "ps6");
                        SpreadsheetGridChanger(model.GetNamesOfAllNonemptyCells());
                        SavePath.Text = fileResult.FullPath;
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
        //FileResult fileResult = await FilePicker.Default.PickAsync();   // placeholder so the compiler doesn't throw a fit
        //path = 
        //model.Save(path);    // something like that goes here
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
            else if (!Regex.IsMatch(SavePath.Text, @"^[A-Z]:\\"))
            {
                string execPath = AppDomain.CurrentDomain.BaseDirectory;
                SavePath.Text = execPath + SavePath.Text;
                model.Save(SavePath.Text);
                await DisplayAlert("Successfully Saved File", "File saved to path: " + SavePath.Text, "OK");
            }
            else
            {
                model.Save(SavePath.Text);
                await DisplayAlert("Successfully Saved File", "File saved to path: " + SavePath.Text, "OK");
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
        foreach (string cellname in set)  // is this getting too in the way of the model?
        {
            int letterIndex = char.ToUpper(cellname[0]) - 65;       // additional subtraction by 1 for indexing
            int numberIndex = int.Parse(cellname[1].ToString()) - 1;
            var value = model.GetCellValue(cellname);

            if (value is not FormulaError)  // otherwise this displays as a SpreadsheetUtilities.FormulaError and that's uggy
                spreadsheetGrid.SetValue(letterIndex, numberIndex, model.GetCellValue(cellname).ToString());
            else
                spreadsheetGrid.SetValue(letterIndex, numberIndex, "FormErr");
            // possibly change string interpretation for FormulaErrors


            // put something here to accomodate for floating point shenanigans
            // e.g. "2.0000000000020E-19" as a string leaves both the Contents entry and the cell box
        }
    }
}

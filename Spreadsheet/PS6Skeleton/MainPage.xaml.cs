using SS;
using System.Data.Common;

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
        spreadsheetGrid.SetSelection(2, 3);
    }

    /// <summary>
    /// Event for when a cell is selected.
    /// Clicking a cell has the same meaning as selecting a cell.
    /// </summary>
    private void displaySelection(SpreadsheetGrid grid)
    {
        
        spreadsheetGrid.GetSelection(out int col, out int row); // gets positional data for selected column and row
        spreadsheetGrid.GetValue(col, row, out string value);   // gets value data for selected column and row
        if (value == "")
        {
            spreadsheetGrid.SetValue(col, row, DateTime.Now.ToLocalTime().ToString("T"));           // changes value of specified cell
            spreadsheetGrid.GetValue(col, row, out value);      // same as before
            DisplayAlert("Selection:", "column " + col + " row " + row + " value " + value, "OK");  // OS alerts. display box over the program
        }
    }

    /// <summary>
    /// Effectively creates a new spreadsheet by clearing all entries in the current SpreadsheetGrid.
    /// </summary>
    private void NewClicked(Object sender, EventArgs e)
    {
        spreadsheetGrid.Clear();    // clears display
        model = new();              // MAKE SURE TO CHANGE ON RELEASES
    }

    /// <summary>
    /// Opens any(!!!) file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
        // if we wish to use this as 
    {
        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                Console.WriteLine("Successfully chose file: " + fileResult.FileName);

                string fileContents = File.ReadAllText(fileResult.FullPath);
                Console.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
            }
            else
            {
                Console.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Standard "Save As" functionality for spreadsheet programs.
    /// Should effectively save the current spreadsheet to a new file.
    /// </summary>
    private async void SaveAsClicked(Object sender, EventArgs e)
    {
        FileResult fileResult = await FilePicker.Default.PickAsync();   // placeholder so the compiler doesn't throw a fit
        //model.Save(fileResult.ToString());    // something like that goes here
    }

    /// <summary>
    /// Standard "Save" functionality for spreadsheet programs.
    /// Should effectively save the current spreadsheet to a new file.
    /// </summary>
    private async void SaveClicked(Object sender, EventArgs e)
    {
        FileResult fileResult = await FilePicker.Default.PickAsync();   // placeholder so the compiler doesn't throw a fit
        //model.Save(fileResult.ToString());    // something like that goes here
    }
}

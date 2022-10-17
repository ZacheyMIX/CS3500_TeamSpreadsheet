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
    private String path;

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
        path = "";
    }

    /// <summary>
    /// Event for when a cell is selected.
    /// Clicking a cell has the same meaning as selecting a cell.
    /// </summary>
    private void displaySelection(SpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out int col, out int row); // gets positional data for selected column and row
        spreadsheetGrid.GetValue(col, row, out string value);   // gets value data for selected column and row
        
    }

    /// <summary>
    /// Effectively creates a new spreadsheet by clearing all entries in the current SpreadsheetGrid.
    /// </summary>
    private void NewClicked(Object sender, EventArgs e)
    {
        spreadsheetGrid.Clear();    // clears display
        model = new(s => true, s => s, "ps6");              // MAKE SURE TO CHANGE ON RELEASES
        path = "";
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
                if (Regex.IsMatch(fileResult.FullPath, @"\.sprd$"))
                {
                    try
                    {
                        model = new(fileResult.FullPath, s => true, s => s, "ps6");
                        foreach (string cellname in model.GetNamesOfAllNonemptyCells())  // is this getting too in the way of the model?
                        {
                            int letterIndex = char.ToUpper(cellname[0]) - 65;       // additional subtraction by 1 for indexing
                            int numberIndex = int.Parse(cellname[1].ToString()) - 1;
                            spreadsheetGrid.SetValue(letterIndex, numberIndex, model.GetCellValue(cellname).ToString());
                        }
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
        //path = 
        //model.Save(path);    // something like that goes here
    }
}

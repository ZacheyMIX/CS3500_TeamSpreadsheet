using Microsoft.Maui.Storage;
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
    private async void NewClicked(Object sender, EventArgs e)
    {
        spreadsheetGrid.Clear();    // clears display
        bool goAhead = true;
        if (model.Changed)
            goAhead = await DisplayAlert("Current Spreadsheet Not Saved",
                "You are about to open a new spreadsheet without saving the previous one.",
                "Continue", "Abort");
        if (goAhead)
        {
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
                Console.WriteLine("Successfully chose file: " + fileResult.FileName);

                string fileContents = File.ReadAllText(fileResult.FullPath);
                Console.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
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
                Console.WriteLine("No file selected.");
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex);
        }
    }
    /*
    /// <summary>
    /// connects model and spreadsheetGrid to the current entry and cell input
    /// </summary>
    private async void ContentsChanged(Object sender, EventArgs e)
    {
        try
        {
            HashSet<string> toBeUpdated = model.SetContentsOfCell(name, Contents.Text);
            SpreadsheetGridChanger(toBeUpdated);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    */
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
            string execPath = AppDomain.CurrentDomain.BaseDirectory;
            if (SavePath.Text == "")
            {
                await DisplayAlert("No File Specified", "", "OK");
            }
            try
            {
                model.Save(SavePath.Text);
                await DisplayAlert("Successfully Saved File", "File saved to path: " + SavePath.Text, "OK");
            }
            catch
            {
                SavePath.Text = execPath + SavePath.Text;
                model.Save(SavePath.Text);
                await DisplayAlert("Successfully Saved File", "File saved to path: " + SavePath.Text, "OK");
            }
            finally
            {
                await DisplayAlert("Invalid File Type", "Spreadsheet files must be the .sprd file type", "OK");
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
            spreadsheetGrid.SetValue(letterIndex, numberIndex, model.GetCellValue(cellname).ToString());
        }
    }
}

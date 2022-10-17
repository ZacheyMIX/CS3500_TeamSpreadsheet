PROJECT BY ASHTON HUNT AND ZACHERY BLOMQUIST ET AL
UNIVERSITY OF UTAH FALL 2022
--------------------------------------------------
Spreadsheet program should mimick other spreadsheet software,
e.g. Microsoft Excel, Google Sheets, etc. freeware or otherwise.


USING THE SOFTWARE:

At the top of the program window, there are three boxes:
first for cell names, second for cell values, and third for editing the cell contents.
Cell names can be things like "A1" or "B6", cell contents are expressions or numbers entered by the user,
and cell values are evaluated expressions or the same numbers entered by the user.

Under the file dropdown menu, there are multiple options:
New: Creates and displays a new, clear spreadsheet.
Save: Saves the current spreadsheet to a .sprd file.
Open: Opens a previously saved spreadsheet file.


POSSIBLE ERRORS AND WARNINGS:
*subject to change as development progresses*

- creating invalid formulas
  Will open an error pop-up.
  Caused by enterring invalid syntax. Note that invalid formulas are different from formula error contents/values;
  formula errors are caused by unevaluated values in other cells and will not cause a pop-up.

- losing spreadsheet data
  Will open a warning pop-up.
  Causes may be, but are not limited to:
  opening a new or different spreadsheet when current spreadsheet modifications are not saved,
  and overriding a different spreadsheet file by saving the current file to the other file.
  Note that fatal or likewise hairy errors may result in data loss.

THE ABOVE ENTRIES, ALTHOUGH SUBJECT TO CHANGE, ARE CONSIDERED FUNDAMENTAL TO THE PROJECT
WRITTEN FIRST ON 10/15/22.
----------------------------------------------------------------------------------------
Dev notes and TODOs:

# 10/15/22 - Ashton
- Reconsider "Save As" option in file management. Is this going to be the extra feature we're banking on?
- Make sure to keep everything in xaml.cs related to the control in MVC; any extra data members or functions should be concise.
 This is more a note for myself than for you Zach, haha.

# 10/17/22 - Ashton & Zach
- Worked extensively on adding Save and opening features. Open front end works great, but Saves still have some work to be done.
- Fix merge conflicts.
- Still need to finish the Entries and Labels for each cell. Just needs some work put in, though.
- Still deciding on whether or not paths need to be precise.
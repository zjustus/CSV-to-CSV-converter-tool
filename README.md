# CSV to CSV converter tool
This tool transforms CSV data from one format into another given a list of mapped fields.

## Features
- Define input and output CSV notation (delimiter, quotations, escapement)
- Define what columns to keep or remove
- Manipulate column data using various methods
  - RegEX clip
  - Math
  - Append
  - Prepend
- Manipulate column data using if then logical conditions
- Variable Substititution for unique values
- No Code needed. Just modify the JSON

# Application flow
## Basic usage
- on launch, if the tool does not find a JSON file containing definitions for SNF the tool will create a basic template file and close with a warning prompting you to update this file
- Drag and drop one or many csv file into the main window to begin the conversion process. 

## Advanced notes
- If multiple JSON files exist in the applications directory it will load only the first one on startup. make sure only one exists.

## Processing order
When building def.json files it may be useful to know the order of operations for CSV processing
1. load in input column value if present
2. apply transformations in order defined by def.json file
3. apply if then else logic
4. apply padding logic
5. insert processed value into the back of the existing output column or create a new one if it docent exist. 

## Json Sections
- defTitle - The title of the transformations, displayed on the main window
- delimiter - what separates the columns
- textMarks - Are there quotations around text filed, what are they?
- marks - Are there quotations around every field, what are they?
- hasHeaders - Does the CSV file have headers - if none are present, a number is assigned as its inputName
- outputDelimiter - what separates the output columns
- outputTextMarks - should there be quotations around text fields, what are they?
- outputMarks - should there be quotations around every field, what are they?
- outputHasHeaders - should the output have headers?
- columns - A list of objects describing each OUTPUT column and how to generate it.

Note: the order of columns in the final CSV will be in the order the columns defined in list of the JSON file  

# Road Map
- [ ] break processing logic into new file and modularize individual steps for better readability and abstraction
- [X] add ability to insert variables
- [ ] add ability to define variables in array in the def.json
- [ ] add built in environment variables
  - [X] File Name
  - [ ] File Path?
  - [ ] Day
  - [ ] Month
  - [ ] Year
- [ ] Include command line parameters
  - [ ] point to def.json file
  - [ ] point to csv input file
- [ ] Refresh when def.json file changes
- [ ] Create a def.json builder
- [ ] CSV previewer


# Variable parameters - In Progress
This area allows the user to define virtual columns which have the same transformable features as real columns but can be recalled as a variable in column parameters.  
variables are the same as columns, requiring at the minimum and outputName and a value (following the guidelines specified below)  

Variables in the real columns will be parsed before math and regex transformations as well as after after value logic.  

# Column parameters
inputName:  
The input name is the name of the input column the produced value is derived from.  
If columns do not contain a header, the input name is assigned as a number starting from 0.  
It is required if there are transformations.  
It is required if there is if then logic in the value field.  
It is not required if the produced value is not dependent on an input. 

outputName:  
The Output Name is the name of the output column produced.  
It is not required if the InputName is given.  

transformations:  
Transformations is an array of set rules on how to process the input column value.  
Each transformation must contain a method and a function value dictating what type of transformation it is, and how to perform it
Transformations are performed before value logic and require an inputName to be present.  
The following is a list of methods that can be used:  
  - math: applies math to the value - {"method": "math", "function": "{ * 100}"}
  - append: appends text to the end of the input - {"method": "append", "function": "End of Line"}
  - prepend: prepends text to the beginning of the input - {"method": "prepend", "function": "Start of Line "}
  - regclip: selects only particular parts of text and ignores everything else - {"method": "regclip", "function": "^[^.]*"}

value:  
The value parameter defines a fixed output or a type of "if then" lookup logic to determine the final value.  
It is applied after transformations take place.  
It is not required if an InputName is given.  
The value parameter can be a string  
The value parameter can be a list containing the if then objects that determine the final output.  

padding:  
The padding parameter defines a fixed length of the final value of that column and will fill the remaining space with a specified character.  
If the length is exceeded it will throw a warning but not an error.  
The padding parameter must be an object with the following properties:  
- side: defines where the padding will be inserted, on the left or right.
- char: defines what character will be inserted into the padding.
- length: defines the length of the value. 


# Sample JSON definitions file
```JSON
{
"defTitle": "Cool Transformation",
"delimiter": ",",
"textMarks": "",
"marks": "\"",
"hasHeaders": true,
"outputDelimiter": ",",
"outputTextMarks": "",
"outputMarks": "\"",
"outputHasHeaders": false,
"columns": [
	{
		"inputName": "Notes",
		"outputName": "Cool Notes"
	},
	{
		"inputName": "new Giver",
	},
	{
		"outputName": "OrgCode",
		"value": 1
	},
	{
		"inputName":"Donation Fund",
		"outputName": "out1",
		"value":[
			{ "if": "Tithes and Offerings", "then": "oneTwoThree"},
			{ "if": "General Fund", "then": "oneTwoThree"}
		]
	},
	{
		"inputName":"Donation Fund",
		"outputName": "out2",
		"value":[
			{ "if": "Tithes and Offerings", "then": "oneTwoThree"},
			{ "if": "General Fund}", "then": "oneTwoThree ${fileName"},
			{ "else": "OptionThree"}
		]
	},
	{
		"inputName": "Amount",
		"transformations": [
			{ "method": "math", "function": " * 100"},
			{ "method": "regClip", "function": "^[^.]*"}
		],
		"padding":{ "side": "left", "char":"0", "length":9 }
	}
]
}
```

# Sources
- Read data from JSON files - https://learn.microsoft.com/en-us/answers/questions/699941/read-and-process-json-file-with-c.html
- Read data from CSV file - https://code-maze.com/csharp-read-data-from-csv-file/
- Write data to CSV file - https://code-maze.com/csharp-writing-csv-file/
- WPF Drag n drop docs - https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/drag-and-drop-overview?view=netframeworkdesktop-4.8
- C# FileSystemWatcher docs - https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=net-7.0

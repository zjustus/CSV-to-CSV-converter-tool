# Elvanto to SNF Import Tool
This tool converts exported Elvanto Batches into an Shelby Next Financials compatible import file

# Application flow
## Basic usage
- on launch, if the tool does not find a JSON file containing definitions for SNF the tool will create a basic template file and close with a warning prompting you to update this file
- Drag and drop one or many csv file into the main window to begin the conversion process. 

## Advanced notes
- If multiple JSON files exist in the applications directory it will load only the first one on startup. make sure only one exists.

## Json Sections
- Delimiter - what separates the columns
- TextMarks - Are there quotations around text filed, what are they?
- Marks - Are there quotations around every field, what are they?
- HasHeaders - Does the CSV file have headers - if none are present, a number is assigned as its inputName
- OutputDelimiter - what separates the output columns
- OutputTextMarks - should there be quotations around text fields, what are they?
- OutputMarks - should there be quotations around every field, what are they?
- OutputHasHeaders - should the output have headers?
- Columns - A list of objects describing each OUTPUT column and how to generate it.

Note: the order of columns in the final CSV will be in the order the columns defined in list of the JSON file


# Column parameters
InputName:  
The input name is the name of the input column the produced value is derived from.  
If columns do not contain a header, the input name is assigned as a number starting from 0.  
It is required if there are transformations.  
It is required if there is if then logic in the value field.  
It is not required if the produced value is not dependent on an input. 

OutputName:  
The Output Name is the name of the output column produced.  
It is not required if the InputName is given.  

Transformations:  
Transformations is an array of set rules on how to process the input column value.  
Each transformation must contain a method and a function value dictating what type of transformation it is, and how to perform it
Transformations are performed before value logic and require an inputName to be present.  
The following is a list of methods that can be used.  
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



# Sample JSON definitions file
```JSON
{
"DefTitle": "Cool Transformation",
"Delimiter": ",",
"TextMarks": "",
"Marks": "\"",
"HasHeaders": true,
"OutputDelimiter": ",",
"OutputTextMarks": "",
"OutputMarks": "\"",
"OutputHasHeaders": false,
"Columns": [
	{
		"InputName": "Notes",
		"OutputName": "Cool Notes"
	},
	{
		"InputName": "new Giver",
	},
	{
		"OutputName": "OrgCode",
		"value": 1
	},
	{
		"InputName":"Donation Fund",
		"OutputName": "out1",
		"value":[
			{ "if": "Tithes and Offerings", "then": "oneTwoThree"},
			{ "if": "General Fund", "then": "oneTwoThree"}
		]
	},
	{
		"InputName":"Donation Fund",
		"OutputName": "out2",
		"value":[
			{ "if": "Tithes and Offerings", "then": "oneTwoThree"},
			{ "if": "General Fund", "then": "oneTwoThree"},
			{ "else": "OptionThree"}
		]
	},
	{
		"InputName": "Amount",
		"Transformations": [
			{ "method": "math", "function": " * 100"},
			{ "method": "regClip", "function": "^[^.]*"}
		]
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

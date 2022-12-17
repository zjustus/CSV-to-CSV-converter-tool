# Elvanto to SNF Import Tool
This tool converts exported Elvanto Batches into an Shelby Next Financials compatible import file


# Application flow
## Basic usage
- on launch, if the tool does not find a JSON file containing definitions for SNF the tool will create a basic template file and close with a warning prompting you to update this file
- on launch, the tool also creates an output and processed directory in the current directory if one does not exist already
- on launch, if the tool sees pre-existing .csv files, it will ask if the users wants these files imported. if not, it will ignore them till next launch
- while running, the tool watches its current directory looking for any new .csv files, when one is found it will immediately process the file placing the new file in the output directory and the processed file into the processed directory
- The tool also features a drag n drop interface that will allow the user to drop one or more .csv files into the application and it will process them placing the new file in the output directory and the processed file into the processed directory. 

## Advanced notes
- If multiple JSON files exist in the applications directory it will load all and merge in memory, it will crash if merge conflicts exist
  - This is to help organization as the definitions lists could get very long over time
- If an active JSON definitions file is modified durning execution, a warning will appear asking the user to continue with old definitions or update using new information
  - Like above, if a merge conflict exists, it will crash

# Sample JSON definitions file
```JSON
[
    {
        "name":"English Service Tithes and Offerings",
        "fund":100,
        "account":800,
        "subAccount":200
    }
]
```


# Sources
- Read data from JSON files - https://learn.microsoft.com/en-us/answers/questions/699941/read-and-process-json-file-with-c.html
- Read data from CSV file - https://code-maze.com/csharp-read-data-from-csv-file/
- Write data to CSV file - https://code-maze.com/csharp-writing-csv-file/
- WPF Drag n drop docs - https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/drag-and-drop-overview?view=netframeworkdesktop-4.8
- C# FileSystemWatcher docs - https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=net-7.0
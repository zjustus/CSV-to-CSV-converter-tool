# Church Financial Tool
This tool/project seeks to convert batch files from popular Tithing-Platforms into a bulk import Shelby Next Financial file.

# Supported Platforms
- Tith.ly
- PushPay
- Elvanto

# Need to Know
This tool uses unique column names found in each supported platform to do its job. If these column names ever change, this tool must be updated


# Notes
An Area for development notes
## Shelby Next Financial File Format
https://help.shelbyinc.com/financials/index.htm?context=201


## SNF_ImportTool.json structure
`json
{
    "inputFunds":[
        {
            "co": "0001",
            "fund": "00200",
            "department":"000",
            "account":"001041020"
        }
    ],
    "elvanto":[
        {
            "name": "Tithes & Offerings - Deductible",
            "co": "0001",
            "fund": "00000",
            "department":"000",
            "account":"001040500"
        },
    ],
    "tith.ly":[
        {...}
    ],
    "pushpay":[
        {...}
    ]
`

# Sources
- Read data from JSON files - https://learn.microsoft.com/en-us/answers/questions/699941/read-and-process-json-file-with-c.html
- Read data from CSV file - https://code-maze.com/csharp-read-data-from-csv-file/
- Write data to CSV file - https://code-maze.com/csharp-writing-csv-file/
- WPF Drag n drop docs - https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/drag-and-drop-overview?view=netframeworkdesktop-4.8
- C# FileSystemWatcher docs - https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=net-7.0

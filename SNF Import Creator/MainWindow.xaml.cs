using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SNF_Import_Creator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void fileDrop(object sender, DragEventArgs e)
        {

            // get all dropped files and return if empty
            string[] dropedFiles = Array.Empty<string>();
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                dropedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                //Trace.WriteLine(dropedFiles[0]);
            }
            if (dropedFiles.Length == 0) return;
            foreach(string file in dropedFiles)
            {   
                // if file is a CSV
                if(Regex.IsMatch(file, @"\b\.csv"))
                {
                    /* TODO: process the CSV into some kind of array of named arrays?
                     * create output file
                     * foreach row in csv
                     *      for each column in DEF file
                     *          look for matching column in CSV
                     *              perform processing as directed in CSV
                     *              create or APPEND to output column
                     * if output column headers is not checked, remove column headers from output file
                     * if quick save is not enabled prompt user where to save new file
                     */
                    Trace.WriteLine("File is CSV");
                }

                // if file is a def.JSON
                else if(Regex.IsMatch(file, @"\b\.def\.json"))
                {
                    // TODO: process json and update def with new file
                    Trace.WriteLine("File is def.json");
                }
            }

        }
    }
}

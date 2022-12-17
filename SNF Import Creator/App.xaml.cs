using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics; //Used for debugging
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SNF_Import_Creator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Trace.WriteLine("Hi Mom");
            /**
             * TODO: check if a dict file exists in current directory
             * If file or files exist, 
             *  pick first one and load it in
             *  start Main Window
             * Else launch error window
             * 
            **/
            ErrorWindow errorWin = new ErrorWindow("No Def file found!");
            errorWin.Show();

            //MainWindow main = new MainWindow();
            //main.Show();
        }
    }
}

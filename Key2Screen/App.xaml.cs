using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Key2Screen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private DockStyle dockStyle = DockStyle.Bottom;
        private double height = Screen.PrimaryScreen.WorkingArea.Height / 5.0;
        private int fadeOutDelay = 5000;
        private int historyLength = 3;

        private void OnApplicationStartup(object sender,
            StartupEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            if (ParseCommandLine())
            {
                using (var mainWindow = new MainWindow(fadeOutDelay, historyLength, dockStyle, height))
                {
                    mainWindow.ShowDialog();
                }
            }
        }

        private void OnDispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            MessageBox.Show(dispatcherUnhandledExceptionEventArgs.Exception.ToString());
        }

        private bool ParseCommandLine()
        {
            var arguments = Environment.GetCommandLineArgs();

            foreach (var argument in arguments)
            {
                string argumentValue;
                if (TryGetArgumentValue(argument, "/Height:", out argumentValue))
                {
                    double value;
                    if (double.TryParse(argumentValue, out value))
                    {
                        height = value;
                    }
                }

                if (TryGetArgumentValue(argument, "/Highlight:", out argumentValue))
                {
                    var colorConverter = new ColorConverter();
                    var convertedColor = colorConverter.ConvertFrom(argumentValue);
                    if (convertedColor != null)
                    {
                        Resources["Highlight"] = (Color)convertedColor;
                    }
                }

                if (TryGetArgumentValue(argument, "/Foreground:", out argumentValue))
                {
                    var colorConverter = new ColorConverter();
                    var convertedColor = colorConverter.ConvertFrom(argumentValue);
                    if (convertedColor != null)
                    {
                        Resources["Foreground"] = new SolidColorBrush((Color)convertedColor);
                    }
                }

                if (TryGetArgumentValue(argument, "/Shadow:", out argumentValue))
                {
                    var colorConverter = new ColorConverter();
                    var convertedColor = colorConverter.ConvertFrom(argumentValue);
                    if (convertedColor != null)
                    {
                        Resources["Shadow"] = (Color)convertedColor;
                    }
                }

                if (TryGetArgumentValue(argument, "/Dock:", out argumentValue))
                {
                    DockStyle value;
                    if (Enum.TryParse<DockStyle>(argumentValue, out value))
                    {
                        dockStyle = value;
                    }
                }

                if (TryGetArgumentValue(argument, "/HistoryLength:", out argumentValue))
                {
                    int value;
                    if (int.TryParse(argumentValue, out value))
                    {
                        historyLength = value;
                    }
                }

                if (TryGetArgumentValue(argument, "/FadeOutDelay:", out argumentValue))
                {
                    uint value;
                    if (uint.TryParse(argumentValue, out value))
                    {
                        fadeOutDelay = Convert.ToInt32(value);
                    }
                }

                if (TryGetArgumentValue(argument, "/?", out argumentValue)
                    || TryGetArgumentValue(argument, "/Help", out argumentValue))
                {
                    new HelpScreen().ShowDialog();
                    return false;
                }
            }
            return true;
        }

        private static bool TryGetArgumentValue(string argument,
            string argumentName,
            out string value)
        {
            value = string.Empty;

            if (argument.StartsWith(argumentName, StringComparison.OrdinalIgnoreCase))
            {
                value = argument.Substring(argumentName.Length);
                return true;
            }

            return false;
        }
    }
}

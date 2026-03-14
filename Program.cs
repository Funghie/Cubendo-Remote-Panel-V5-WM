// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Cubendo_Remote_Panel
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Path to language.ini in AppData\Phil Pendlebury\CN Remote\Settings
            string appDataRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Phil Pendlebury", "CN Remote", "Settings");
            string iniPath = Path.Combine(appDataRoot, "language.ini");

            string languageCode = "en"; // Default to English

            if (File.Exists(iniPath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(iniPath);
                    bool inLanguageSection = false;
                    foreach (string line in lines)
                    {
                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("[Language]", StringComparison.OrdinalIgnoreCase))
                        {
                            inLanguageSection = true;
                            continue;
                        }
                        if (inLanguageSection && trimmed.StartsWith("Language=", StringComparison.OrdinalIgnoreCase))
                        {
                            string value = trimmed.Substring("Language=".Length).Trim();
                            if (!string.IsNullOrEmpty(value))
                            {
                                languageCode = value;
                            }
                            break;
                        }
                        // Stop parsing if another section starts
                        if (inLanguageSection && trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                        {
                            break;
                        }
                    }
                }
                catch
                {
                    languageCode = "en";
                }
            }

            try
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
            }
            catch
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}

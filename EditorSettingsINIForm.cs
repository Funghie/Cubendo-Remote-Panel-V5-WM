// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Cubendo_Remote_Panel
{
    public partial class EditorSettingsForm : Form
    {
        private string settingsIniPath;
        private List<string> processNames = new List<string>();
        private List<string> processPanels = new List<string>();
        private List<string> processMidiOuts = new List<string>();
        private List<string> processMidiIns = new List<string>();
        private List<string> processComments = new List<string>();
        private int maxProcessNumber = 0;
        private bool _isUpdatingUI = false;

        private Color midiInColor;
        private Color buttonGlowColorDown;
        private Color buttonGlowColorUp;
        private Color tooltipBackColor;
        private Color tooltipTextColor;

        private class LanguageItem
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }

        private string languageIniPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Phil Pendlebury", "CN Remote", "Settings", "language.ini");

        public EditorSettingsForm()
        {
            InitializeComponent();

            PopulateMidiDeviceCombos();

            // --- Language dropdown setup ---
            LoadLanguages();
            comboLanguage.SelectedIndexChanged += comboLanguage_SelectedIndexChanged;

            // --- Color context menu ---
            ContextMenuStrip colorContextMenu = new ContextMenuStrip();
            ToolStripMenuItem copyColorMenuItem = new ToolStripMenuItem(Properties.Resources.MenuCopyColour);
            ToolStripMenuItem pasteColorMenuItem = new ToolStripMenuItem(Properties.Resources.MenuPasteColour);
            colorContextMenu.Items.AddRange(new ToolStripItem[] { copyColorMenuItem, pasteColorMenuItem });

            buttonMIDIInColour.ContextMenuStrip = colorContextMenu;
            buttonButtonGlowDown.ContextMenuStrip = colorContextMenu;
            buttonButtonGlowUp.ContextMenuStrip = colorContextMenu;
            buttonTooltipBack.ContextMenuStrip = colorContextMenu;
            buttonTooltipText.ContextMenuStrip = colorContextMenu;

            copyColorMenuItem.Click += (s, e) =>
            {
                if (colorContextMenu.SourceControl is Button btn)
                {
                    Clipboard.SetText(ColorTranslator.ToHtml(btn.BackColor));
                }
            };

            pasteColorMenuItem.Click += (s, e) =>
            {
                if (colorContextMenu.SourceControl is Button btn)
                {
                    try
                    {
                        string colorStr = Clipboard.GetText();
                        var color = ColorTranslator.FromHtml(colorStr);
                        btn.BackColor = color;
                        btn.ForeColor = GetContrastingTextColor(color);
                    }
                    catch { }
                }
            };

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AutoScroll = false;

            comboProcesses.DropDownStyle = ComboBoxStyle.DropDownList;

            settingsIniPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Phil Pendlebury", "CN Remote", "Settings", "settings.ini");

            numericAutoSwitchInterval.Minimum = 10;
            numericAutoSwitchInterval.Maximum = 5000;
            numericAutoSwitchDebounce.Minimum = 10;
            numericAutoSwitchDebounce.Maximum = 10000;

            SetupEventHandlers();
            LoadSettings();

            this.Shown += EditorSettingsForm_Shown;
        }

        private void SetupEventHandlers()
        {
            buttonMIDIInColour.Click += ButtonMIDIInColour_Click;
            buttonButtonGlowDown.Click += ButtonGlowDown_Click;
            buttonButtonGlowUp.Click += ButtonGlowUp_Click;
            buttonTooltipBack.Click += ButtonTooltipBack_Click;
            buttonTooltipText.Click += ButtonTooltipText_Click;

            comboProcesses.SelectedIndexChanged += ComboProcesses_SelectedIndexChanged;
            buttonAddProcess.Click += ButtonAddProcess_Click;
            buttonDeleteProcess.Click += ButtonDeleteProcess_Click;

            textProcessName.TextChanged += ProcessField_Changed;
            comboDefaultPanel.SelectedIndexChanged += ProcessField_Changed;
            comboProcessMidiOut.SelectedIndexChanged += ProcessField_Changed;
            comboProcessMidiIn.SelectedIndexChanged += ProcessField_Changed;

            buttonOK.Click += ButtonOK_Click;
            buttonCancel.Click += ButtonCancel_Click;
        }

        private void ProcessField_Changed(object sender, EventArgs e)
        {
            if (_isUpdatingUI) return;
            int index = comboProcesses.SelectedIndex;
            if (index >= 0 && index < processNames.Count)
            {
                processNames[index] = textProcessName.Text;
                processPanels[index] = comboDefaultPanel.SelectedItem?.ToString() ?? "";
                processMidiOuts[index] = comboProcessMidiOut.SelectedItem?.ToString() ?? "";
                processMidiIns[index] = comboProcessMidiIn.SelectedItem?.ToString() ?? "";
                comboProcesses.Items[index] = textProcessName.Text;
            }
        }

        private void LoadSettings()
        {
            string midiOutValue = ReadIni("MIDI", "MidiOut", "");
            comboMidiOut.SelectedItem = string.IsNullOrEmpty(midiOutValue) ? "None" : midiOutValue;

            string midiInValue = ReadIni("MIDI", "MidiIn", "");
            comboMidiIn.SelectedItem = string.IsNullOrEmpty(midiInValue) ? "None" : midiInValue;

            string midiInColorStr = ReadIni("MIDI", "MIDIInColour", "#00ffbf");
            midiInColor = ParseColor(midiInColorStr, Color.LimeGreen);
            buttonMIDIInColour.BackColor = midiInColor;
            buttonMIDIInColour.ForeColor = GetContrastingTextColor(midiInColor);

            string buttonGlowDownStr = ReadIni("MIDI", "ButtonGlowColourDown", "#ff0000");
            buttonGlowColorDown = ParseColor(buttonGlowDownStr, Color.Red);
            buttonButtonGlowDown.BackColor = buttonGlowColorDown;
            buttonButtonGlowDown.ForeColor = GetContrastingTextColor(buttonGlowColorDown);

            string buttonGlowUpStr = ReadIni("MIDI", "ButtonGlowColourUp", "#0099ff");
            buttonGlowColorUp = ParseColor(buttonGlowUpStr, Color.Blue);
            buttonButtonGlowUp.BackColor = buttonGlowColorUp;
            buttonButtonGlowUp.ForeColor = GetContrastingTextColor(buttonGlowColorUp);

            string tooltipBackStr = ReadIni("MIDI", "TooltipBack", "#FFFF00");
            tooltipBackColor = ParseColor(tooltipBackStr, Color.Yellow);
            buttonTooltipBack.BackColor = tooltipBackColor;
            buttonTooltipBack.ForeColor = GetContrastingTextColor(tooltipBackColor);

            string tooltipTextStr = ReadIni("MIDI", "TooltipText", "#000000");
            tooltipTextColor = ParseColor(tooltipTextStr, Color.Black);
            buttonTooltipText.BackColor = tooltipTextColor;
            buttonTooltipText.ForeColor = GetContrastingTextColor(tooltipTextColor);

            string intervalStr = ReadIni("Switching", "AutoSwitchInterval", "500");
            int interval;
            if (int.TryParse(intervalStr, out interval))
            {
                numericAutoSwitchInterval.Value = Math.Max(numericAutoSwitchInterval.Minimum,
                    Math.Min(numericAutoSwitchInterval.Maximum, interval));
            }

            string debounceStr = ReadIni("Switching", "AutoSwitchDebounce", "1000");
            int debounce;
            if (int.TryParse(debounceStr, out debounce))
            {
                numericAutoSwitchDebounce.Value = Math.Max(numericAutoSwitchDebounce.Minimum,
                    Math.Min(numericAutoSwitchDebounce.Maximum, debounce));
            }

            // --- Fallback Panel ComboBox population ---
            string currentFallback = ReadIni("Switching", "FallBack", "None");
            string buttonsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Phil Pendlebury", "CN Remote", "Buttons");

            // Get all .txt files in the directory
            var buttonFiles = Directory.Exists(buttonsDir)
                ? Directory.GetFiles(buttonsDir, "*.txt").Select(Path.GetFileName).ToList()
                : new List<string>();

            // Build ComboBox items: current value (if not "None"), all files, then "None"
            List<string> items = new List<string>();
            if (!string.IsNullOrEmpty(currentFallback) && currentFallback != "None")
                items.Add(currentFallback);

            // Add all .txt files, avoiding duplicates
            foreach (string file in buttonFiles)
                if (!items.Contains(file))
                    items.Add(file);

            // Always add "None" at the end
            if (!items.Contains("None"))
                items.Add("None");

            comboFallbackPanel.Items.Clear();
            foreach (string item in items)
                comboFallbackPanel.Items.Add(item);

            // Select the current value if present, otherwise "None"
            int idx = comboFallbackPanel.Items.IndexOf(currentFallback);
            comboFallbackPanel.SelectedIndex = idx >= 0 ? idx : comboFallbackPanel.Items.IndexOf("None");

            // --- Default Panel ComboBox population (moved here) ---
            comboDefaultPanel.Items.Clear();
            foreach (string file in buttonFiles)
                comboDefaultPanel.Items.Add(file);

            LoadProcessList();
        }

        private void LoadProcessList()
        {
            processNames.Clear();
            processPanels.Clear();
            processMidiOuts.Clear();
            processMidiIns.Clear();
            processComments.Clear();

            if (!File.Exists(settingsIniPath))
            {
                comboProcesses.Items.Clear();
                _isUpdatingUI = true;
                try
                {
                    textProcessName.Text = "";
                    comboDefaultPanel.SelectedIndex = -1;
                    comboProcessMidiOut.SelectedIndex = -1;
                    comboProcessMidiIn.SelectedIndex = -1;
                }
                finally
                {
                    _isUpdatingUI = false;
                }
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(settingsIniPath);
                bool inProcessesSection = false;

                string currentProcess = null;
                string currentPanel = "";
                string currentMidiOut = "";
                string currentMidiIn = "";

                foreach (string raw in lines)
                {
                    string rawLine = raw;
                    int sc = rawLine.IndexOf(';');
                    if (sc >= 0) rawLine = rawLine.Substring(0, sc);
                    string line = rawLine.Trim();
                    if (line.Length == 0) continue;

                    if (line.StartsWith("["))
                    {
                        if (line.Equals("[Processes]", StringComparison.OrdinalIgnoreCase))
                        {
                            inProcessesSection = true;
                            continue;
                        }
                        else if (inProcessesSection)
                        {
                            if (currentProcess != null)
                            {
                                processNames.Add(currentProcess);
                                processPanels.Add(currentPanel ?? "");
                                processMidiOuts.Add(currentMidiOut ?? "");
                                processMidiIns.Add(currentMidiIn ?? "");
                            }
                            break;
                        }
                    }

                    if (!inProcessesSection) continue;

                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    string key = line.Substring(0, eq).Trim().ToLowerInvariant();
                    string value = line.Substring(eq + 1).Trim();

                    switch (key)
                    {
                        case "process":
                            if (currentProcess != null)
                            {
                                processNames.Add(currentProcess);
                                processPanels.Add(currentPanel ?? "");
                                processMidiOuts.Add(currentMidiOut ?? "");
                                processMidiIns.Add(currentMidiIn ?? "");
                            }
                            currentProcess = value;
                            currentPanel = "";
                            currentMidiOut = "";
                            currentMidiIn = "";
                            break;
                        case "panel":
                            currentPanel = value;
                            break;
                        case "midiout":
                            currentMidiOut = value;
                            break;
                        case "midiin":
                            currentMidiIn = value;
                            break;
                    }
                }

                if (currentProcess != null)
                {
                    processNames.Add(currentProcess);
                    processPanels.Add(currentPanel ?? "");
                    processMidiOuts.Add(currentMidiOut ?? "");
                    processMidiIns.Add(currentMidiIn ?? "");
                }

                comboProcesses.Items.Clear();
                comboProcesses.Items.AddRange(processNames.ToArray());


                if (processNames.Count > 0)
                {
                    _isUpdatingUI = true;
                    comboProcesses.SelectedIndexChanged -= ComboProcesses_SelectedIndexChanged;
                    try
                    {
                        comboProcesses.SelectedIndex = 0;
                        textProcessName.Text = processNames[0];
                        comboDefaultPanel.SelectedItem = processPanels[0];
                        comboProcessMidiOut.SelectedItem = string.IsNullOrEmpty(processMidiOuts[0]) || processMidiOuts[0] == "None" ? "None" : processMidiOuts[0];
                        comboProcessMidiIn.SelectedItem = string.IsNullOrEmpty(processMidiIns[0]) || processMidiIns[0] == "None" ? "None" : processMidiIns[0];
                        this.Update();
                    }
                    finally
                    {
                        comboProcesses.SelectedIndexChanged += ComboProcesses_SelectedIndexChanged;
                        _isUpdatingUI = false;
                    }
                }
                else
                {
                    _isUpdatingUI = true;
                    try
                    {
                        textProcessName.Text = "";
                        comboDefaultPanel.SelectedIndex = -1;
                        comboProcessMidiOut.SelectedIndex = -1;
                        comboProcessMidiIn.SelectedIndex = -1;
                    }
                    finally
                    {
                        _isUpdatingUI = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.DialogSettingsLoadError + "\n{0}", ex.Message),
                    Properties.Resources.DialogError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void SaveSettings()
        {

            if (comboMidiOut.SelectedItem != null && comboMidiOut.SelectedItem.ToString() != "None")
                WriteIni("MIDI", "MidiOut", comboMidiOut.SelectedItem.ToString());
            else
                WriteIni("MIDI", "MidiOut", "");

            if (comboMidiIn.SelectedItem != null && comboMidiIn.SelectedItem.ToString() != "None")
                WriteIni("MIDI", "MidiIn", comboMidiIn.SelectedItem.ToString());
            else
                WriteIni("MIDI", "MidiIn", "");

            WriteIni("MIDI", "MIDIInColour", ColorToHtml(midiInColor));
            WriteIni("MIDI", "ButtonGlowColourDown", ColorToHtml(buttonGlowColorDown));
            WriteIni("MIDI", "ButtonGlowColourUp", ColorToHtml(buttonGlowColorUp));

            WriteIni("MIDI", "TooltipBack", ColorToHtml(tooltipBackColor));
            WriteIni("MIDI", "TooltipText", ColorToHtml(tooltipTextColor));

            WriteIni("Switching", "AutoSwitchInterval", numericAutoSwitchInterval.Value.ToString());
            WriteIni("Switching", "AutoSwitchDebounce", numericAutoSwitchDebounce.Value.ToString());

            // Save fallback panel selection
            string selectedFallback = comboFallbackPanel.SelectedItem?.ToString() ?? "None";
            WriteIni("Switching", "FallBack", selectedFallback);

            SaveProcessList();
        }

        private void SaveProcessList()
        {
            if (File.Exists(settingsIniPath))
            {
                List<string> allLines = File.ReadAllLines(settingsIniPath).ToList();
                int startIndex = -1;
                int endIndex = -1;

                for (int i = 0; i < allLines.Count; i++)
                {
                    string line = allLines[i].Trim();
                    if (line.Equals("[Processes]", StringComparison.OrdinalIgnoreCase))
                    {
                        startIndex = i;
                    }
                    else if (startIndex >= 0 && line.StartsWith("[") && line.EndsWith("]"))
                    {
                        endIndex = i - 1;
                        break;
                    }
                }

                if (startIndex >= 0)
                {
                    if (endIndex < 0) endIndex = allLines.Count - 1;
                    allLines.RemoveRange(startIndex, endIndex - startIndex + 1);
                }

                List<string> processSection = new List<string>
                {
                    "[Processes]"
                };

                for (int i = 0; i < processNames.Count; i++)
                {
                    string comment = i < processComments.Count && !string.IsNullOrEmpty(processComments[i])
                        ? processComments[i]
                        : $"; Process {++maxProcessNumber}";
                    processSection.Add(comment);
                    processSection.Add($"Process={processNames[i]}");
                    processSection.Add($"Panel={processPanels[i]}");
                    if (!string.IsNullOrEmpty(processMidiOuts[i]) && processMidiOuts[i] != "None")
                        processSection.Add($"MidiOut={processMidiOuts[i]}");
                    if (!string.IsNullOrEmpty(processMidiIns[i]) && processMidiIns[i] != "None")
                        processSection.Add($"MidiIn={processMidiIns[i]}");
                }

                int insertIndex = allLines.Count;
                allLines.InsertRange(insertIndex, processSection);
                File.WriteAllLines(settingsIniPath, allLines);
            }
            else
            {
                List<string> newFile = new List<string>
                {
                    "[MIDI]",
                    $"MidiOut={comboMidiOut.SelectedItem?.ToString() ?? ""}",
                    $"MidiIn={comboMidiIn.SelectedItem?.ToString() ?? ""}",
                    $"MIDIInColour={ColorToHtml(midiInColor)}",
                    $"ButtonGlowColourDown={ColorToHtml(buttonGlowColorDown)}",
                    $"ButtonGlowColourUp={ColorToHtml(buttonGlowColorUp)}",
                    $"TooltipBack={ColorToHtml(tooltipBackColor)}",
                    $"TooltipText={ColorToHtml(tooltipTextColor)}",
                    "",
                    "[Switching]",
                    $"AutoSwitchInterval={numericAutoSwitchInterval.Value}",
                    $"AutoSwitchDebounce={numericAutoSwitchDebounce.Value}",
                    $"FallBack={comboFallbackPanel.SelectedItem?.ToString() ?? "None"}",
                    "",
                    "[Processes]"
                };
                for (int i = 0; i < processNames.Count; i++)
                {
                    newFile.Add($"; Process {i + 1}");
                    newFile.Add($"Process={processNames[i]}");
                    newFile.Add($"Panel={processPanels[i]}");
                    if (!string.IsNullOrEmpty(processMidiOuts[i]))
                        newFile.Add($"MidiOut={processMidiOuts[i]}");
                    if (!string.IsNullOrEmpty(processMidiIns[i]))
                        newFile.Add($"MidiIn={processMidiIns[i]}");
                }

                File.WriteAllLines(settingsIniPath, newFile);
            }
        }

        #region Event Handlers

        private void ButtonMIDIInColour_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = midiInColor;
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    midiInColor = colorDialog.Color;
                    buttonMIDIInColour.BackColor = midiInColor;
                    buttonMIDIInColour.ForeColor = GetContrastingTextColor(midiInColor);
                }
            }
        }

        private void ButtonGlowDown_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = buttonGlowColorDown;
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    buttonGlowColorDown = colorDialog.Color;
                    buttonButtonGlowDown.BackColor = buttonGlowColorDown;
                    buttonButtonGlowDown.ForeColor = GetContrastingTextColor(buttonGlowColorDown);
                }
            }
        }

        private void ButtonGlowUp_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = buttonGlowColorUp;
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    buttonGlowColorUp = colorDialog.Color;
                    buttonButtonGlowUp.BackColor = buttonGlowColorUp;
                    buttonButtonGlowUp.ForeColor = GetContrastingTextColor(buttonGlowColorUp);
                }
            }
        }

        private void ButtonTooltipBack_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = tooltipBackColor;
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    tooltipBackColor = colorDialog.Color;
                    buttonTooltipBack.BackColor = tooltipBackColor;
                    buttonTooltipBack.ForeColor = GetContrastingTextColor(tooltipBackColor);
                }
            }
        }

        private void ButtonTooltipText_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = tooltipTextColor;
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    tooltipTextColor = colorDialog.Color;
                    buttonTooltipText.BackColor = tooltipTextColor;
                    buttonTooltipText.ForeColor = GetContrastingTextColor(tooltipTextColor);
                }
            }
        }

        private void ComboProcesses_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboProcesses.SelectedIndex;
            UpdateProcessFields(index);
        }

        private void ButtonAddProcess_Click(object sender, EventArgs e)
        {
            string newName = "NewProcess";
            string newPanel = "buttons.txt";
            string newMidiOut = "";
            string newMidiIn = "";
            string newComment = $"; Process {++maxProcessNumber}";

            processNames.Add(newName);
            processPanels.Add(newPanel);
            processMidiOuts.Add(newMidiOut);
            processMidiIns.Add(newMidiIn);
            processComments.Add(newComment);

            comboProcesses.Items.Add(newName);
            comboProcesses.SelectedIndex = comboProcesses.Items.Count - 1;
        }

        private void ButtonDeleteProcess_Click(object sender, EventArgs e)
        {
            int index = comboProcesses.SelectedIndex;
            if (index >= 0 && index < processNames.Count)
            {
                DialogResult result = MessageBox.Show(
                    string.Format(Properties.Resources.DialogConfirmDeleteProcess + "\n{0}", processNames[index]),
                    Properties.Resources.EditMidiActionForm_ConfirmDeleteTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    processNames.RemoveAt(index);
                    processPanels.RemoveAt(index);
                    processMidiOuts.RemoveAt(index);
                    processMidiIns.RemoveAt(index);
                    if (index < processComments.Count)
                        processComments.RemoveAt(index);

                    comboProcesses.Items.RemoveAt(index);
                    if (comboProcesses.Items.Count > 0)
                        comboProcesses.SelectedIndex = Math.Min(index, comboProcesses.Items.Count - 1);
                    else
                    {
                        textProcessName.Text = "";
                        comboDefaultPanel.SelectedIndex = -1;
                        comboProcessMidiOut.SelectedIndex = -1;
                        comboProcessMidiIn.SelectedIndex = -1;
                    }
                }
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void comboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSelectedLanguage();
            MessageBox.Show(
                Properties.Resources.GlobalEditorSettings_LanguageChangedMessage,
                Properties.Resources.GlobalEditorSettings_LanguageChangedTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void EditorSettingsForm_Shown(object sender, EventArgs e)
        {
            if (comboProcesses.SelectedIndex >= 0)
            {
                UpdateProcessFields(comboProcesses.SelectedIndex);
            }
        }

        private void labelMIDIInColour_Click(object sender, EventArgs e)
        {
            ButtonMIDIInColour_Click(buttonMIDIInColour, e);
        }

        private void labelTooltipBack_Click_1(object sender, EventArgs e)
        {
            ButtonTooltipBack_Click(buttonTooltipBack, e);
        }

        private void labelAutoSwitchDebounce_Click(object sender, EventArgs e)
        {
        }

        private void UpdateProcessFields(int index)
        {
            if (index >= 0 && index < processNames.Count)
            {
                _isUpdatingUI = true;
                try
                {
                    this.SuspendLayout();
                    textProcessName.Text = processNames[index];
                    comboDefaultPanel.SelectedItem = processPanels[index];
                    comboProcessMidiOut.SelectedItem = string.IsNullOrEmpty(processMidiOuts[index]) || processMidiOuts[index] == "None" ? "None" : processMidiOuts[index];
                    comboProcessMidiIn.SelectedItem = string.IsNullOrEmpty(processMidiIns[index]) || processMidiIns[index] == "None" ? "None" : processMidiIns[index];
                }
                finally
                {
                    this.ResumeLayout();
                    _isUpdatingUI = false;
                }
            }
        }

        private void LoadLanguages()
        {
            List<LanguageItem> languages = new List<LanguageItem>();
            string selectedCode = "en";

            if (File.Exists(languageIniPath))
            {
                foreach (string line in File.ReadAllLines(languageIniPath))
                {
                    if (line.StartsWith("Language="))
                    {
                        selectedCode = line.Substring("Language=".Length).Trim();
                    }
                    else if (line.Contains("=") && line.Contains(","))
                    {
                        string[] parts = line.Split(new[] { '=', ',' }, 3);
                        if (parts.Length == 3)
                        {
                            languages.Add(new LanguageItem { Code = parts[1].Trim(), Name = parts[2].Trim() });
                        }
                    }
                }
            }

            comboLanguage.DataSource = languages;
            comboLanguage.DisplayMember = "Name";
            comboLanguage.ValueMember = "Code";
            comboLanguage.SelectedValue = selectedCode;
        }

        private void SaveSelectedLanguage()
        {
            if (comboLanguage.SelectedItem is LanguageItem selected)
            {
                var lines = File.ReadAllLines(languageIniPath).ToList();
                int langIndex = lines.FindIndex(l => l.StartsWith("Language="));
                if (langIndex >= 0)
                    lines[langIndex] = $"Language={selected.Code}";
                else
                    lines.Insert(0, $"Language={selected.Code}");
                File.WriteAllLines(languageIniPath, lines);
            }
        }

        #endregion

        #region Helper Methods

        private string ReadIni(string section, string key, string defaultVal = "")
        {
            if (!File.Exists(settingsIniPath)) return defaultVal;

            string[] iniLines = File.ReadAllLines(settingsIniPath);
            string currentSection = "";
            foreach (string line in iniLines)
            {
                string trimmed = line.Split(';')[0].Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed.Substring(1, trimmed.Length - 2);
                    continue;
                }
                if (currentSection.Equals(section, StringComparison.OrdinalIgnoreCase))
                {
                    string[] kv = trimmed.Split(new char[] { '=' }, 2);
                    if (kv.Length == 2 && kv[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                        return kv[1].Trim();
                }
            }
            return defaultVal;
        }

        private void WriteIni(string section, string key, string value)
        {
            List<string> lines = File.Exists(settingsIniPath) ?
                File.ReadAllLines(settingsIniPath).ToList() : new List<string>();
            int secIndex = lines.FindIndex(l => l.Trim().Equals($"[{section}]", StringComparison.OrdinalIgnoreCase));

            if (secIndex == -1)
            {
                lines.Add($"[{section}]");
                lines.Add($"{key}={value}");
            }
            else
            {
                int insertIndex = secIndex + 1;
                while (insertIndex < lines.Count && !lines[insertIndex].StartsWith("[")) insertIndex++;
                int keyIndex = lines.FindIndex(secIndex + 1, insertIndex - (secIndex + 1), l =>
                {
                    string[] parts = l.Split(new char[] { '=' }, 2);
                    return parts.Length == 2 && parts[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase);
                });

                if (keyIndex >= 0)
                    lines[keyIndex] = $"{key}={value}";
                else
                    lines.Insert(insertIndex, $"{key}={value}");
            }

            try
            {
                File.WriteAllLines(settingsIniPath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.DialogSettingsSaveError + "\n{0}", ex.Message),
                    Properties.Resources.DialogError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private static Color ParseColor(string colorStr, Color defaultColor)
        {
            try
            {
                if (string.IsNullOrEmpty(colorStr)) return defaultColor;

                if (colorStr.StartsWith("#") && (colorStr.Length == 7 || colorStr.Length == 9))
                {
                    int r = Convert.ToInt32(colorStr.Substring(1, 2), 16);
                    int g = Convert.ToInt32(colorStr.Substring(3, 2), 16);
                    int b = Convert.ToInt32(colorStr.Substring(5, 2), 16);
                    return Color.FromArgb(r, g, b);
                }
                return Color.FromName(colorStr);
            }
            catch
            {
                return defaultColor;
            }
        }

        private static string ColorToHtml(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static Color GetContrastingTextColor(Color bg)
        {
            double luminance = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255;
            return luminance > 0.5 ? Color.Black : Color.White;
        }

        private List<string> GetAllMidiDeviceNames()
        {
            var names = new HashSet<string>();
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
                names.Add(MidiIn.DeviceInfo(i).ProductName);
            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
                names.Add(MidiOut.DeviceInfo(i).ProductName);
            return names.ToList();
        }

        // 1. Update PopulateMidiDeviceCombos to always add "None" as the first item
        private void PopulateMidiDeviceCombos()
        {
            var devices = GetAllMidiDeviceNames();

            comboMidiIn.Items.Clear();
            comboMidiIn.Items.Add("None");
            comboMidiOut.Items.Clear();
            comboMidiOut.Items.Add("None");
            comboProcessMidiIn.Items.Clear();
            comboProcessMidiIn.Items.Add("None");
            comboProcessMidiOut.Items.Clear();
            comboProcessMidiOut.Items.Add("None");

            foreach (string name in devices)
            {
                comboMidiIn.Items.Add(name);
                comboMidiOut.Items.Add(name);
                comboProcessMidiIn.Items.Add(name);
                comboProcessMidiOut.Items.Add(name);
            }
        }

        #endregion
    }
}

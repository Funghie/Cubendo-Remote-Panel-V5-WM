// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Cubendo_Remote_Panel
{
    public partial class EditorButtonsINIForm : Form
    {
        private readonly string iniPath;


        public EditorButtonsINIForm(string iniPath)
        {
            InitializeComponent();

            // --- Add this block for color context menu ---
            ContextMenuStrip colorContextMenu = new ContextMenuStrip();
            ToolStripMenuItem copyColorMenuItem = new ToolStripMenuItem("Copy Colour");
            ToolStripMenuItem pasteColorMenuItem = new ToolStripMenuItem("Paste Colour");
            colorContextMenu.Items.AddRange(new ToolStripItem[] { copyColorMenuItem, pasteColorMenuItem });

            // Assign context menu to color buttons
            buttonBackgroundColour.ContextMenuStrip = colorContextMenu;
            buttonButtonBorder.ContextMenuStrip = colorContextMenu;
            buttonUCButtonColour.ContextMenuStrip = colorContextMenu;
            buttonUCBackgroundColour.ContextMenuStrip = colorContextMenu;
            buttonUCButtonBorder.ContextMenuStrip = colorContextMenu;

            // Event handlers for context menu
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

                        // Update preview if a UC color button was changed
                        if (btn == buttonUCButtonColour || btn == buttonUCBackgroundColour || btn == buttonUCButtonBorder)
                        {
                            UpdateUCPreview();
                        }
                    }
                    catch
                    {
                        // Optionally show error or ignore
                    }
                }
            };
            // --- End of color context menu block ---

            // Set form properties to remove maximize/minimize buttons
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.iniPath = iniPath;
            this.Text = Properties.Resources.ButtonEditorSettings_FormTitle + ": " + Path.GetFileNameWithoutExtension(iniPath);

            // Set up event handlers - explicitly wire up the buttons
            SetupEventHandlers();

            // Load values from INI
            LoadIniValues();
        }

        private void SetupEventHandlers()
        {
            // IMPORTANT: Make sure the Save button is wired up correctly
            buttonSave.Click -= buttonSave_Click; // Remove any existing handlers first
            buttonSave.Click += buttonSave_Click;  // Add our handler

            buttonCancel.Click -= OnCancelClick;   // Remove any existing handlers
            buttonCancel.Click += OnCancelClick;   // Add our handler

            // Color picker buttons
            buttonBackgroundColour.Click += (s, e) => PickColor(buttonBackgroundColour);
            buttonButtonBorder.Click += (s, e) => PickColor(buttonButtonBorder);

            buttonUCButtonColour.Click += (s, e) => { PickColor(buttonUCButtonColour); UpdateUCPreview(); };
            buttonUCBackgroundColour.Click += (s, e) => { PickColor(buttonUCBackgroundColour); UpdateUCPreview(); };
            buttonUCButtonBorder.Click += (s, e) => { PickColor(buttonUCButtonBorder); UpdateUCPreview(); };

            // Font picker buttons
            buttonButtonFontName.Click += (s, e) => PickFont(textBoxButtonFontName, numericButtonFontSize);
            buttonUCButtonFontName.Click += (s, e) => PickFont(textBoxUCButtonFontName, numericUCButtonFontSize);
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void PickColor(Button colorButton)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.Color = colorButton.BackColor;
                dlg.FullOpen = true;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    colorButton.BackColor = dlg.Color;
                }
            }
        }

        // Add this method to your form class:
        private void UpdateUCPreview()
        {
            panelUCPreview.BackColor = buttonUCBackgroundColour.BackColor;
            panelUCPreview.Paint -= PanelUCPreview_Paint;
            panelUCPreview.Paint += PanelUCPreview_Paint;
            panelUCPreview.Invalidate();
        }

        private void PanelUCPreview_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var backgroundColor = buttonUCBackgroundColour.BackColor;
            var borderColor = buttonUCButtonBorder.BackColor;
            var buttonColor = buttonUCButtonColour.BackColor;

            // Fill the entire panel with the background color
            using (SolidBrush bgBrush = new SolidBrush(backgroundColor))
            {
                g.FillRectangle(bgBrush, 0, 0, panelUCPreview.Width, panelUCPreview.Height);
            }

            // Draw the border rectangle (middle)
            int borderThickness = 1;
            int borderOffset = 4;
            int borderSizeW = panelUCPreview.Width - 2 * borderOffset;
            int borderSizeH = panelUCPreview.Height - 2 * borderOffset;
            using (SolidBrush borderBrush = new SolidBrush(borderColor))
            {
                g.FillRectangle(borderBrush, borderOffset, borderOffset, borderSizeW, borderSizeH);
            }

            // Draw the button face rectangle (innermost)
            int faceOffset = borderOffset + borderThickness;
            int faceSizeW = panelUCPreview.Width - 2 * faceOffset;
            int faceSizeH = panelUCPreview.Height - 2 * faceOffset;
            using (SolidBrush faceBrush = new SolidBrush(buttonColor))
            {
                g.FillRectangle(faceBrush, faceOffset, faceOffset, faceSizeW, faceSizeH);
            }

            // Draw the "down" symbol (centered)
            string downSymbol = "▼";
            using (Font font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(borderColor))
            {
                SizeF textSize = g.MeasureString(downSymbol, font);
                float textX = faceOffset + (faceSizeW - textSize.Width) / 2;
                float textY = faceOffset + (faceSizeH - textSize.Height) / 2;
                g.DrawString(downSymbol, font, textBrush, textX, textY);
            }
        }

        private void PickFont(TextBox fontNameBox, NumericUpDown fontSizeControl)
        {
            using (FontDialog dlg = new FontDialog())
            {
                try
                {
                    dlg.Font = new Font(fontNameBox.Text, (float)fontSizeControl.Value);
                }
                catch
                {
                    dlg.Font = new Font("Segoe UI", 9.0f);
                }

                dlg.ShowEffects = false;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    fontNameBox.Text = dlg.Font.FontFamily.Name;

                    // Round to nearest 0.5 to match common font dialog increments
                    decimal selectedSize = (decimal)dlg.Font.SizeInPoints;
                    decimal roundedSize = Math.Round(selectedSize * 2, MidpointRounding.AwayFromZero) / 2;

                    // Clamp to NumericUpDown min/max
                    if (roundedSize < fontSizeControl.Minimum) roundedSize = fontSizeControl.Minimum;
                    if (roundedSize > fontSizeControl.Maximum) roundedSize = fontSizeControl.Maximum;

                    fontSizeControl.Value = roundedSize;
                }
            }
        }

        private void LoadIniValues()
        {
            // [Buttons] Section
            numericButtonWidth.Value = ReadIntFromIni("Buttons", "Button Width", 100);
            numericButtonHeight.Value = ReadIntFromIni("Buttons", "Button Height", 22);
            numericButtonGap.Value = ReadIntFromIni("Buttons", "Button Gap", 0);

            buttonBackgroundColour.BackColor = ParseColor(ReadIniValue("Buttons", "Background Colour"), Color.White);
            buttonButtonBorder.BackColor = ParseColor(ReadIniValue("Buttons", "Button Border"), Color.Black);

            textBoxButtonFontName.Text = ReadIniValue("Buttons", "Button Font Name", "Segoe UI");
            numericButtonFontSize.Value = ParseDecimal(ReadIniValue("Buttons", "Button Font Size"), 8.25m);

            numericTopMargin.Value = ReadIntFromIni("Buttons", "Top Margin", 2);
            numericLeftMargin.Value = ReadIntFromIni("Buttons", "Left Margin", 6);

            // [UserCollapse] Section
            numericUCButtonWidth.Value = ReadIntFromIni("UserCollapse", "Button Width", 110);
            numericUCButtonHeight.Value = ReadIntFromIni("UserCollapse", "Button Height", 30);

            buttonUCButtonColour.BackColor = ParseColor(ReadIniValue("UserCollapse", "Button Colour"), Color.Red);
            buttonUCBackgroundColour.BackColor = ParseColor(ReadIniValue("UserCollapse", "Background Colour"), Color.Black);
            buttonUCButtonBorder.BackColor = ParseColor(ReadIniValue("UserCollapse", "Button Border"), Color.Black);

            textBoxUCButtonFontName.Text = ReadIniValue("UserCollapse", "Button Font Name", "Segoe UI");
            numericUCButtonFontSize.Value = ParseDecimal(ReadIniValue("UserCollapse", "Button Font Size"), 10m);

            numericUCTopMargin.Value = ReadIntFromIni("UserCollapse", "Top Margin", 4);
            numericUCLeftMargin.Value = ReadIntFromIni("UserCollapse", "Left Margin", 4);

            UpdateUCPreview();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                // [Buttons] Section
                WriteIniValue("Buttons", "Button Width", numericButtonWidth.Value.ToString());
                WriteIniValue("Buttons", "Button Height", numericButtonHeight.Value.ToString());
                WriteIniValue("Buttons", "Button Gap", numericButtonGap.Value.ToString());
                WriteIniValue("Buttons", "Background Colour", ColorToHtml(buttonBackgroundColour.BackColor));
                WriteIniValue("Buttons", "Button Border", ColorToHtml(buttonButtonBorder.BackColor));
                WriteIniValue("Buttons", "Button Font Name", textBoxButtonFontName.Text);
                WriteIniValue("Buttons", "Button Font Size", numericButtonFontSize.Value.ToString());
                WriteIniValue("Buttons", "Top Margin", numericTopMargin.Value.ToString());
                WriteIniValue("Buttons", "Left Margin", numericLeftMargin.Value.ToString());

                // [UserCollapse] Section
                WriteIniValue("UserCollapse", "Button Width", numericUCButtonWidth.Value.ToString());
                WriteIniValue("UserCollapse", "Button Height", numericUCButtonHeight.Value.ToString());
                WriteIniValue("UserCollapse", "Button Colour", ColorToHtml(buttonUCButtonColour.BackColor));
                WriteIniValue("UserCollapse", "Background Colour", ColorToHtml(buttonUCBackgroundColour.BackColor));
                WriteIniValue("UserCollapse", "Button Border", ColorToHtml(buttonUCButtonBorder.BackColor));
                WriteIniValue("UserCollapse", "Button Font Name", textBoxUCButtonFontName.Text);
                WriteIniValue("UserCollapse", "Button Font Size", numericUCButtonFontSize.Value.ToString());
                WriteIniValue("UserCollapse", "Top Margin", numericUCTopMargin.Value.ToString());
                WriteIniValue("UserCollapse", "Left Margin", numericUCLeftMargin.Value.ToString());

                // Force file write flush
                File.SetLastWriteTime(iniPath, DateTime.Now);

                MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Helper Methods
        private string ReadIniValue(string section, string key, string defaultVal = "")
        {
            if (!File.Exists(iniPath)) return defaultVal;

            string[] lines = File.ReadAllLines(iniPath);
            string currentSection = "";

            foreach (string line in lines)
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
                    string[] parts = trimmed.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2 && parts[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                        return parts[1].Trim();
                }
            }
            return defaultVal;
        }

        private int ReadIntFromIni(string section, string key, int defaultVal)
        {
            string value = ReadIniValue(section, key, defaultVal.ToString());
            return int.TryParse(value, out int result) ? result : defaultVal;
        }

        private void WriteIniValue(string section, string key, string value)
        {
            List<string> lines = new List<string>();

            if (File.Exists(iniPath))
            {
                lines = File.ReadAllLines(iniPath).ToList();
            }

            int sectionIndex = lines.FindIndex(l =>
                l.Trim().Equals($"[{section}]", StringComparison.OrdinalIgnoreCase));

            if (sectionIndex == -1)
            {
                // Section doesn't exist, add it
                lines.Add("");  // Add blank line before new section
                lines.Add($"[{section}]");
                lines.Add($"{key}={value}");
            }
            else
            {
                // Find the end of this section
                int endIndex = lines.FindIndex(sectionIndex + 1, l =>
                    l.Trim().StartsWith("[") && l.Trim().EndsWith("]"));

                if (endIndex == -1)
                    endIndex = lines.Count; // Section goes to end of file

                // Look for the key in this section
                int keyIndex = -1;
                for (int i = sectionIndex + 1; i < endIndex; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] parts = line.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2 && parts[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        keyIndex = i;
                        break;
                    }
                }

                if (keyIndex >= 0)
                    lines[keyIndex] = $"{key}={value}";
                else
                    lines.Insert(endIndex, $"{key}={value}");
            }

            File.WriteAllLines(iniPath, lines);
        }

        private Color ParseColor(string colorString, Color defaultColor = default)
        {
            try
            {
                if (string.IsNullOrEmpty(colorString))
                    return defaultColor;

                if (colorString.StartsWith("#"))
                {
                    string hex = colorString.TrimStart('#');
                    if (hex.Length == 6)
                    {
                        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                        return Color.FromArgb(255, r, g, b);
                    }
                }

                // Try as a named color
                return Color.FromName(colorString);
            }
            catch
            {
                return defaultColor;
            }
        }

        private string ColorToHtml(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private decimal ParseDecimal(string value, decimal defaultVal)
        {
            if (decimal.TryParse(value, out decimal result))
                return result;
            return defaultVal;
        }

        private static Color GetContrastingTextColor(Color bg)
        {
            // Standard luminance formula for sRGB
            double luminance = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255;
            return luminance > 0.5 ? Color.Black : Color.White;
        }
        #endregion
    }
}

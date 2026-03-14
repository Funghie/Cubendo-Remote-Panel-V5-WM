using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
// using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Cubendo_Remote_Panel
{
    public partial class EditMidiActionForm : Form
    {
        // Action flags for Form1 to check after dialog closes
        public bool AddRequested { get; private set; }
        public bool DeleteRequested { get; private set; }
        public bool MoveLeftRequested { get; private set; }
        public bool MoveRightRequested { get; private set; }

        // Controls
        private TextBox txtName;
        private TextBox txtTooltip;
        private NumericUpDown numChannel;
        private ComboBox cmbType;
        private NumericUpDown numValue;
        private NumericUpDown numMouseDownValue;
        private NumericUpDown numMouseUpValue;
        private TextBox txtKeyString;
        private ToolTip toolTip;


        public string ActionName
        {
            get => txtName.Text;
            set => txtName.Text = value;
        }

        public string Tooltip
        {
            get => txtTooltip.Text;
            set => txtTooltip.Text = value;
        }

        public int Channel
        {
            get => (int)numChannel.Value;
            set => numChannel.Value = Math.Max(numChannel.Minimum, Math.Min(numChannel.Maximum, value));
        }

        public string Type
        {
            get => cmbType.SelectedItem?.ToString() ?? "CC";
            set
            {
                // Case-insensitive selection for ComboBox items
                for (int i = 0; i < cmbType.Items.Count; i++)
                {
                    if (string.Equals(cmbType.Items[i].ToString(), value, StringComparison.OrdinalIgnoreCase))
                    {
                        cmbType.SelectedIndex = i;
                        return;
                    }
                }
                // If not found, fallback to direct assignment (will not select if casing doesn't match)
                cmbType.SelectedItem = value;
            }
        }

        public string KeyString
        {
            get => txtKeyString.Text;
            set => txtKeyString.Text = value;
        }

        public int Value
        {
            get => (int)numValue.Value;
            set => numValue.Value = Math.Max(numValue.Minimum, Math.Min(numValue.Maximum, value));
        }

        public int MouseDownValue
        {
            get => (int)numMouseDownValue.Value;
            set => numMouseDownValue.Value = Math.Max(numMouseDownValue.Minimum, Math.Min(numMouseDownValue.Maximum, value));
        }

        public int? MouseUpValue
        {
            get => numMouseUpValue.Text == "" ? (int?)null : (int)numMouseUpValue.Value;
            set
            {
                if (value.HasValue)
                    numMouseUpValue.Value = Math.Max(numMouseUpValue.Minimum, Math.Min(numMouseUpValue.Maximum, value.Value));
                else
                    numMouseUpValue.Text = "";
            }
        }



        // Constructor with optional iniPath for background color
        public EditMidiActionForm(string iniPath = null, Color? tooltipBackColor = null, Color? tooltipForeColor = null)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;

            this.Text = Properties.Resources.EditMidiActionForm_Title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            // this.ClientSize = new System.Drawing.Size(220, 320);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Padding = new Padding(4);

            Color textColor = Color.Black; // default
            Color? borderColor = null;     // <-- Add this line

            if (!string.IsNullOrEmpty(iniPath) && File.Exists(iniPath))
            {
                var lines = File.ReadAllLines(iniPath);
                string currentSection = "";
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        currentSection = trimmed.Substring(1, trimmed.Length - 2);
                        continue;
                    }
                    if (currentSection.Equals("Buttons", StringComparison.OrdinalIgnoreCase))
                    {
                        if (trimmed.StartsWith("Background Colour=", StringComparison.OrdinalIgnoreCase))
                        {
                            var colorValue = trimmed.Substring("Background Colour=".Length).Trim();
                            this.BackColor = ColorHelper.ParseOrDefault(colorValue, Color.White);
                            int brightness = (int)(this.BackColor.R * 0.299 + this.BackColor.G * 0.587 + this.BackColor.B * 0.114);
                            textColor = brightness < 128 ? Color.White : Color.Black;
                        }
                        else if (trimmed.StartsWith("Button Border=", StringComparison.OrdinalIgnoreCase))
                        {
                            var colorValue = trimmed.Substring("Button Border=".Length).Trim();
                            borderColor = ColorHelper.ParseOrDefault(colorValue, Color.Black);
                        }
                    }
                }
            }

            // Updated widths for compact layout
            int labelWidth = 70;
            int controlWidth = 170;
            int leftLabel = 8;
            int leftControl = leftLabel + labelWidth + 4;
            int buttonWidth = 100;
            int buttonHeight = 26;
            int buttonMargin = 8;
            int buttonRow0Top = 8;
            int formWidth = 260; // fixed width for compact layout

            int topStart = buttonRow0Top + buttonHeight + buttonMargin;
            int rowHeight = 30;

            // Always use black for data entry fields
            Color entryTextColor = Color.Black;

            // Move Left and Move Right buttons
            Button btnMoveLeft = new Button
            {
                Text = Properties.Resources.EditMidiActionForm_MoveLeft,
                Left = buttonMargin,
                Top = buttonRow0Top,
                Width = buttonWidth,
                Height = buttonHeight,
                TabIndex = 10,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            if (borderColor.HasValue)
            {
                btnMoveLeft.FlatAppearance.BorderColor = borderColor.Value;
                btnMoveLeft.FlatAppearance.BorderSize = 1;
            }
            btnMoveLeft.Click += (s, e) =>
            {
                MoveLeftRequested = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            Button btnMoveRight = new Button
            {
                Text = Properties.Resources.EditMidiActionForm_MoveRight,
                Left = formWidth - buttonWidth - buttonMargin,
                Top = buttonRow0Top,
                Width = buttonWidth,
                Height = buttonHeight,
                TabIndex = 11,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            if (borderColor.HasValue)
            {
                btnMoveRight.FlatAppearance.BorderColor = borderColor.Value;
                btnMoveRight.FlatAppearance.BorderSize = 1;
            }
            btnMoveRight.Click += (s, e) =>
            {
                MoveRightRequested = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            // Labels use dynamic textColor (already set)
            Label lblName = new Label { Text = Properties.Resources.EditMidiActionForm_Name, Left = leftLabel, Top = topStart, Width = labelWidth, Height = rowHeight, TextAlign = ContentAlignment.MiddleRight, ForeColor = textColor };
            Label lblTooltip = new Label { Text = Properties.Resources.EditMidiActionForm_Tooltip, Left = leftLabel, Top = topStart + rowHeight, Width = labelWidth, Height = rowHeight, TextAlign = ContentAlignment.MiddleRight, ForeColor = textColor };
            Label lblChannel = new Label { Text = Properties.Resources.EditMidiActionForm_Channel, Left = leftLabel, Top = topStart + rowHeight * 2, Width = labelWidth, Height = rowHeight, TextAlign = ContentAlignment.MiddleRight, ForeColor = textColor };
            Label lblType = new Label { Text = Properties.Resources.EditMidiActionForm_Type, Left = leftLabel, Top = topStart + rowHeight * 3, Width = labelWidth, Height = rowHeight, TextAlign = ContentAlignment.MiddleRight, ForeColor = textColor };
            // Label lblValue = new Label { Text = "CC/Note #:", Left = leftLabel, Top = topStart + rowHeight * 4, Width = labelWidth, Height = rowHeight, TextAlign = ContentAlignment.MiddleRight, ForeColor = textColor };
            Label lblValue = new Label { Text = Properties.Resources.EditMidiActionForm_Value, Left = leftLabel, Top = topStart + rowHeight * 4, Width = labelWidth, Height = rowHeight, TextAlign = ContentAlignment.MiddleRight, ForeColor = textColor };
            Label lblMouseDown = new Label { Text = Properties.Resources.EditMidiActionForm_Press, Left = leftLabel, Top = topStart + rowHeight * 5, Width = labelWidth, Height = rowHeight, TextAlign = ContentAlignment.MiddleRight, ForeColor = textColor };
            Label lblMouseUp = new Label { Text = Properties.Resources.EditMidiActionForm_Release, Left = leftLabel, Top = topStart + rowHeight * 6, Width = labelWidth, Height = rowHeight, TextAlign = ContentAlignment.MiddleRight, ForeColor = textColor };

            // Data entry fields always use black
            txtName = new TextBox { Left = leftControl, Top = topStart, Width = controlWidth, Height = rowHeight, TabIndex = 0, ForeColor = entryTextColor };
            txtTooltip = new TextBox { Left = leftControl, Top = topStart + rowHeight, Width = controlWidth, Height = rowHeight, TabIndex = 1, ForeColor = entryTextColor };
            numChannel = new NumericUpDown { Left = leftControl, Top = topStart + rowHeight * 2, Width = 50, Height = rowHeight, Minimum = 1, Maximum = 16, TabIndex = 2, ForeColor = entryTextColor };
            cmbType = new ComboBox { Left = leftControl, Top = topStart + rowHeight * 3, Width = 60, Height = rowHeight, DropDownStyle = ComboBoxStyle.DropDownList, TabIndex = 3, ForeColor = entryTextColor };
            cmbType.Items.AddRange(new[] { "CC", "Note", "Key" });
            numValue = new NumericUpDown { Left = leftControl, Top = topStart + rowHeight * 4, Width = 50, Height = rowHeight, Minimum = 0, Maximum = 127, TabIndex = 4, ForeColor = entryTextColor };
            numMouseDownValue = new NumericUpDown { Left = leftControl, Top = topStart + rowHeight * 5, Width = 50, Height = rowHeight, Minimum = 0, Maximum = 127, TabIndex = 5, ForeColor = entryTextColor };
            numMouseUpValue = new NumericUpDown { Left = leftControl, Top = topStart + rowHeight * 6, Width = 50, Height = rowHeight, Minimum = 0, Maximum = 127, TabIndex = 6, ForeColor = entryTextColor };

            // KeyString TextBox (for KEY type only)
            txtKeyString = new TextBox
            {
                Left = leftControl,
                Top = topStart + rowHeight * 4,
                Width = controlWidth,
                Height = rowHeight,
                TabIndex = 4,
                ForeColor = entryTextColor,
                Visible = false
            };

            // Add this directly after the above block:
            txtKeyString.DoubleClick += (s, e) =>
            {
                using (var dlg = new KeyStringEntryForm(txtKeyString.Text))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        txtKeyString.Text = dlg.KeyString;
                    }
                }
            };

            // Place the "X" just to the right of the NumericUpDown
            Label lblMouseUpX = new Label
            {
                Text = Properties.Resources.EditMidiActionForm_MouseUpX,
                Left = numMouseUpValue.Right + 4, // 4px gap to the right
                Top = numMouseUpValue.Top,
                Width = 16,
                Height = numMouseUpValue.Height,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = numMouseUpValue.Text == "" ? Color.Red : Color.Gray,
                BackColor = Color.Transparent,
                Font = new Font(this.Font, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            this.Controls.Add(lblMouseUpX);



            // Update color when value changes
            void UpdateMouseUpXColor()
            {
                lblMouseUpX.ForeColor = numMouseUpValue.Text == "" ? Color.Red : Color.Gray;
            }
            numMouseUpValue.TextChanged += (s, e) => UpdateMouseUpXColor();
            numMouseUpValue.ValueChanged += (s, e) => UpdateMouseUpXColor();

            // Clicking the X clears the value
            lblMouseUpX.Click += (s, e) =>
            {
                numMouseUpValue.Text = ""; // This will trigger the TextChanged event and turn the X red
            };

            // Add/Delete/Save/Cancel buttons below data entry fields
            int buttonRow1Top = topStart + rowHeight * 7 + buttonMargin;
            int buttonRow2Top = buttonRow1Top + buttonHeight + buttonMargin;

            // Add and Delete buttons
            Button btnAdd = new Button
            {
                Text = Properties.Resources.EditMidiActionForm_Add,
                Left = buttonMargin,
                Top = buttonRow1Top,
                Width = buttonWidth,
                Height = buttonHeight,
                TabIndex = 7,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            if (borderColor.HasValue)
            {
                btnAdd.FlatAppearance.BorderColor = borderColor.Value;
                btnAdd.FlatAppearance.BorderSize = 1;
            }

            btnAdd.Click += (s, e) =>
            {
                this.Activate();
                string type = Type;
                string valueText = type.Equals("Key", StringComparison.OrdinalIgnoreCase)
                    ? $"Key: {KeyString}"
                    : $"CC/Note #: {Value}";
                var confirm = MessageBox.Show(
                    this,
                    string.Format(Properties.Resources.EditMidiActionForm_ConfirmAddText, ActionName, Tooltip, Channel, type, valueText, MouseDownValue, MouseUpValue.HasValue ? MouseUpValue.Value.ToString() : "(none)"),
                    Properties.Resources.EditMidiActionForm_ConfirmAddTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    AddRequested = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            Button btnDelete = new Button
            {
                Text = Properties.Resources.EditMidiActionForm_Delete,
                Left = formWidth - buttonWidth - buttonMargin,
                Top = buttonRow1Top,
                Width = buttonWidth,
                Height = buttonHeight,
                TabIndex = 8,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            if (borderColor.HasValue)
            {
                btnDelete.FlatAppearance.BorderColor = borderColor.Value;
                btnDelete.FlatAppearance.BorderSize = 1;
            }
            
            btnDelete.Click += (s, e) =>
            {
                this.Activate();
                var confirm = MessageBox.Show(
                    this,
                    Properties.Resources.EditMidiActionForm_ConfirmDeleteText,
                    Properties.Resources.EditMidiActionForm_ConfirmDeleteTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    DeleteRequested = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            // Save and Cancel buttons
            Button btnSave = new Button
            {
                Text = Properties.Resources.EditMidiActionForm_Save,
                Left = buttonMargin,
                Top = buttonRow2Top,
                Width = buttonWidth,
                Height = buttonHeight,
                TabIndex = 9,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            if (borderColor.HasValue)
            {
                btnSave.FlatAppearance.BorderColor = borderColor.Value;
                btnSave.FlatAppearance.BorderSize = 1;
            }
            btnSave.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                MessageBox.Show(this, Properties.Resources.EditMidiActionForm_SaveMsgText, Properties.Resources.EditMidiActionForm_SaveMsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };

            Button btnCancel = new Button
            {
                Text = Properties.Resources.EditMidiActionForm_Cancel,
                Left = formWidth - buttonWidth - buttonMargin,
                Top = buttonRow2Top,
                Width = buttonWidth,
                Height = buttonHeight,
                TabIndex = 12,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            if (borderColor.HasValue)
            {
                btnCancel.FlatAppearance.BorderColor = borderColor.Value;
                btnCancel.FlatAppearance.BorderSize = 1;
            }
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.CancelButton = btnCancel;


            // ToolTip assignments
            // ToolTip assignments
            toolTip = new ToolTip();
            toolTip.OwnerDraw = true;

            if (tooltipBackColor.HasValue)
                toolTip.BackColor = tooltipBackColor.Value;
            if (tooltipForeColor.HasValue)
                toolTip.ForeColor = tooltipForeColor.Value;

            // Use the same font as Form1, or define your own
            Font tooltipFont = new Font("Segoe UI", 9, FontStyle.Regular);

            // Draw handler: matches Form1.cs
            toolTip.Draw += (s, e) =>
            {
                string[] lines = e.ToolTipText.Replace("\\n", "\n").Split('\n');
                int topMargin = 6;
                // int bottomMargin = 8;

                using (SolidBrush b = new SolidBrush(toolTip.BackColor))
                    e.Graphics.FillRectangle(b, e.Bounds);

                using (Pen borderPen = new Pen(Color.Black))
                    e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);

                using (SolidBrush b = new SolidBrush(toolTip.ForeColor))
                {
                    int y = e.Bounds.Top + topMargin;
                    foreach (string line in lines)
                    {
                        string paddedLine = "  " + line;
                        Size lineSize = TextRenderer.MeasureText(paddedLine, tooltipFont);
                        e.Graphics.DrawString(paddedLine, tooltipFont, b, e.Bounds.Left, y);
                        y += lineSize.Height;
                    }
                    // The bottom margin is handled by the tooltip size in the Popup event.
                }
            };

            toolTip.Popup += (s, e) =>
            {
                string[] lines = toolTip.GetToolTip(e.AssociatedControl)?.Replace("\\n", "\n").Split('\n') ?? new string[0];
                int maxWidth = 0;
                int totalHeight = 0;
                int topMargin = 6;
                int bottomMargin = 8;
                foreach (string line in lines)
                {
                    string paddedLine = "  " + line;
                    Size lineSize = TextRenderer.MeasureText(paddedLine, tooltipFont);
                    if (lineSize.Width > maxWidth) maxWidth = lineSize.Width;
                    totalHeight += lineSize.Height;
                }
                totalHeight += topMargin + bottomMargin;
                e.ToolTipSize = new Size(maxWidth + 6, totalHeight + 2);
            };

            toolTip.SetToolTip(lblName, Properties.Resources.EditMidiActionForm_ToolTip_NameLabel);
            toolTip.SetToolTip(txtName, Properties.Resources.EditMidiActionForm_ToolTip_NameTextBox);
            toolTip.SetToolTip(lblTooltip, Properties.Resources.EditMidiActionForm_ToolTip_TooltipLabel);
            toolTip.SetToolTip(txtTooltip, Properties.Resources.EditMidiActionForm_ToolTip_TooltipTextBox);
            toolTip.SetToolTip(lblChannel, Properties.Resources.EditMidiActionForm_ToolTip_ChannelLabel);
            toolTip.SetToolTip(numChannel, Properties.Resources.EditMidiActionForm_ToolTip_ChannelNumeric);
            toolTip.SetToolTip(lblType, Properties.Resources.EditMidiActionForm_ToolTip_TypeLabel);
            toolTip.SetToolTip(cmbType, Properties.Resources.EditMidiActionForm_ToolTip_TypeComboBox);
            toolTip.SetToolTip(lblValue, Properties.Resources.EditMidiActionForm_ToolTip_ValueLabel);
            toolTip.SetToolTip(numValue, Properties.Resources.EditMidiActionForm_ToolTip_ValueNumeric);
            toolTip.SetToolTip(txtKeyString, Properties.Resources.EditMidiActionForm_ToolTip_KeyStringTextBox);
            toolTip.SetToolTip(lblMouseDown, Properties.Resources.EditMidiActionForm_ToolTip_PressLabel);
            toolTip.SetToolTip(numMouseDownValue, Properties.Resources.EditMidiActionForm_ToolTip_PressNumeric);
            toolTip.SetToolTip(lblMouseUp, Properties.Resources.EditMidiActionForm_ToolTip_ReleaseLabel);
            toolTip.SetToolTip(numMouseUpValue, Properties.Resources.EditMidiActionForm_ToolTip_ReleaseNumeric);
            toolTip.SetToolTip(lblMouseUpX, Properties.Resources.EditMidiActionForm_ToolTip_MouseUpXLabel);
            toolTip.SetToolTip(btnMoveLeft, Properties.Resources.EditMidiActionForm_ToolTip_MoveLeftButton);
            toolTip.SetToolTip(btnMoveRight, Properties.Resources.EditMidiActionForm_ToolTip_MoveRightButton);
            toolTip.SetToolTip(btnAdd, Properties.Resources.EditMidiActionForm_ToolTip_AddButton);
            toolTip.SetToolTip(btnDelete, Properties.Resources.EditMidiActionForm_ToolTip_DeleteButton);
            toolTip.SetToolTip(btnSave, Properties.Resources.EditMidiActionForm_ToolTip_SaveButton);
            toolTip.SetToolTip(btnCancel, Properties.Resources.EditMidiActionForm_ToolTip_CancelButton);
            //
            // For NumericUpDown text areas
            toolTip.SetToolTip(numValue.Controls[1], Properties.Resources.EditMidiActionForm_ToolTip_ValueNumericText);
            toolTip.SetToolTip(numMouseDownValue.Controls[1], Properties.Resources.EditMidiActionForm_ToolTip_PressNumericText);
            toolTip.SetToolTip(numMouseUpValue.Controls[1], Properties.Resources.EditMidiActionForm_ToolTip_ReleaseNumericText);

            // Add controls in correct order
            this.Controls.Add(btnMoveLeft);
            this.Controls.Add(btnMoveRight);
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblTooltip);
            this.Controls.Add(txtTooltip);
            this.Controls.Add(lblChannel);
            this.Controls.Add(numChannel);
            this.Controls.Add(lblType);
            this.Controls.Add(cmbType);
            this.Controls.Add(lblValue);
            this.Controls.Add(txtKeyString);
            this.Controls.Add(numValue);
            this.Controls.Add(lblMouseDown);
            this.Controls.Add(numMouseDownValue);
            this.Controls.Add(lblMouseUp);
            this.Controls.Add(numMouseUpValue);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            // Adjust form height to fit controls with a small margin
            int bottom = btnCancel.Bottom + 8; // 8px margin
            this.ClientSize = new Size(formWidth, bottom);

            // Center dialog over parent form or main window
            Form parentForm = this.Owner ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);
            if (parentForm != null)
            {
                var parentScreen = parentForm.PointToScreen(Point.Empty);
                int dlgX = parentScreen.X + (parentForm.ClientSize.Width - this.ClientSize.Width) / 2;
                int dlgY = parentScreen.Y + (parentForm.ClientSize.Height - this.ClientSize.Height) / 2 - 100;
                dlgX = Math.Max(dlgX, 0);
                dlgY = Math.Max(dlgY, 0);
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(dlgX, dlgY);
            }

            // Helper to update UI for KEY type
            void UpdateUiForType()
            {
                bool isKey = cmbType.SelectedItem?.ToString().Equals("Key", StringComparison.OrdinalIgnoreCase) == true;

                numChannel.Enabled = !isKey;
                numChannel.BackColor = isKey ? SystemColors.Control : SystemColors.Window;

                if (isKey)
                {
                    numValue.Enabled = false;
                    numValue.ReadOnly = true;
                    numValue.TabStop = false;
                    txtKeyString.Enabled = true;
                    txtKeyString.ReadOnly = false;
                    txtKeyString.TabStop = true;
                }
                else
                {
                    numValue.Enabled = true;
                    numValue.ReadOnly = false;
                    numValue.TabStop = true;
                    // Ensure numValue has a valid integer value
                    if (numValue.Value < numValue.Minimum || numValue.Value > numValue.Maximum)
                        numValue.Value = numValue.Minimum;
                    txtKeyString.Enabled = false;
                    txtKeyString.ReadOnly = true;
                    txtKeyString.TabStop = false;
                    // Optionally clear txtKeyString when not Key
                    // txtKeyString.Text = "";
                }

                numValue.Visible = !isKey;
                txtKeyString.Visible = isKey;

                numMouseDownValue.Enabled = !isKey;
                numMouseDownValue.BackColor = isKey ? SystemColors.Control : SystemColors.Window;

                numMouseUpValue.Enabled = !isKey;
                numMouseUpValue.BackColor = isKey ? SystemColors.Control : SystemColors.Window;

                // Ensure numeric fields are valid when re-enabled
                if (!isKey)
                {
                    if (numMouseDownValue.Value < numMouseDownValue.Minimum || numMouseDownValue.Value > numMouseDownValue.Maximum)
                        numMouseDownValue.Value = numMouseDownValue.Minimum;
                    if (numMouseUpValue.Value < numMouseUpValue.Minimum || numMouseUpValue.Value > numMouseUpValue.Maximum)
                        numMouseUpValue.Value = numMouseUpValue.Minimum;
                }
            }

            // Update UI when type changes
            cmbType.SelectedIndexChanged += (s, e) => UpdateUiForType();
            UpdateUiForType();
            // this.Shown += (s, e) => this.Activate();
        }
    }
}

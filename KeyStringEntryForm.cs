// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Cubendo_Remote_Panel
{
    public class KeyStringEntryForm : Form
    {
        public string KeyString { get; private set; }
        private TextBox txtEntry;

        // Modifier keys for grouping
        private static readonly HashSet<string> Modifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Ctrl", "Alt", "Shift", "Win"
        };

        // Helper: group tokens for display
        private static string FormatForDisplay(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            List<string> tokens = input.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(t => t.Trim())
                              .Where(t => !string.IsNullOrEmpty(t))
                              .ToList();

            List<string> lines = new List<string>();
            List<string> group = new List<string>();

            foreach (var token in tokens)
            {
                group.Add(token);
                // If token is not a modifier, end group
                if (!Modifiers.Contains(token))
                {
                    lines.Add(string.Join("+", group));
                    group.Clear();
                }
            }
            // If any trailing modifiers, add as a line
            if (group.Count > 0)
                lines.Add(string.Join("+", group));

            return string.Join(Environment.NewLine, lines);
        }

        public KeyStringEntryForm(string initialText)
        {
            this.Text = Properties.Resources.KeyStringEntryForm_Title;
            this.FormBorderStyle = FormBorderStyle.Sizable; // Allow resizing
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(100, 100); // Optional: set a minimum size
            this.Width = 240;
            this.Height = 320;

            // Use grouping logic for display
            string displayText = FormatForDisplay(initialText);

            // Remove minimize and maximize buttons
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            txtEntry = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Text = displayText,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtEntry);

            Button btnOk = new Button
            {
                Text = Properties.Resources.DialogOK, // Use resource string for "OK"
                Dock = DockStyle.Bottom,
                DialogResult = DialogResult.OK
            };
            btnOk.Click += (s, e) =>
            {
                // Remove empty lines, trim, remove leading/trailing '+', join with '+'
                var lines = txtEntry.Text
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Select(line => line.Trim('+', ' '))
                    .Where(line => !string.IsNullOrEmpty(line));
                KeyString = string.Join("+", lines);
            };
            this.Controls.Add(btnOk);

            // Remove AcceptButton assignment so Enter does not close the form
            // this.AcceptButton = btnOk;
        }
    }
}

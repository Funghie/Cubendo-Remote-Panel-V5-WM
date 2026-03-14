// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

namespace Cubendo_Remote_Panel
{
    partial class EditorSettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.groupBoxMidiPorts = new System.Windows.Forms.GroupBox();
            this.labelMidiOut = new System.Windows.Forms.Label();
            this.comboMidiOut = new System.Windows.Forms.ComboBox();
            this.labelMidiIn = new System.Windows.Forms.Label();
            this.comboMidiIn = new System.Windows.Forms.ComboBox();
            this.groupBoxMidiActivity = new System.Windows.Forms.GroupBox();
            this.labelMIDIInColour = new System.Windows.Forms.Label();
            this.buttonMIDIInColour = new System.Windows.Forms.Button();
            this.labelButtonGlowDown = new System.Windows.Forms.Label();
            this.buttonButtonGlowDown = new System.Windows.Forms.Button();
            this.labelButtonGlowUp = new System.Windows.Forms.Label();
            this.buttonButtonGlowUp = new System.Windows.Forms.Button();
            this.groupBoxTooltips = new System.Windows.Forms.GroupBox();
            this.labelTooltipBack = new System.Windows.Forms.Label();
            this.buttonTooltipBack = new System.Windows.Forms.Button();
            this.labelTooltipText = new System.Windows.Forms.Label();
            this.buttonTooltipText = new System.Windows.Forms.Button();
            this.groupBoxProcesses = new System.Windows.Forms.GroupBox();
            this.comboProcesses = new System.Windows.Forms.ComboBox();
            this.buttonAddProcess = new System.Windows.Forms.Button();
            this.buttonDeleteProcess = new System.Windows.Forms.Button();
            this.labelProcessName = new System.Windows.Forms.Label();
            this.textProcessName = new System.Windows.Forms.TextBox();
            this.labelPanel = new System.Windows.Forms.Label();
            this.comboDefaultPanel = new System.Windows.Forms.ComboBox();
            this.labelProcessMidiOut = new System.Windows.Forms.Label();
            this.comboProcessMidiOut = new System.Windows.Forms.ComboBox();
            this.labelProcessMidiIn = new System.Windows.Forms.Label();
            this.comboProcessMidiIn = new System.Windows.Forms.ComboBox();
            this.labelLanguageSeparator = new System.Windows.Forms.Label();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.comboLanguage = new System.Windows.Forms.ComboBox();
            this.groupBoxAutoSwitch = new System.Windows.Forms.GroupBox();
            this.labelAutoSwitchInterval = new System.Windows.Forms.Label();
            this.numericAutoSwitchInterval = new System.Windows.Forms.NumericUpDown();
            this.labelAutoSwitchDebounce = new System.Windows.Forms.Label();
            this.numericAutoSwitchDebounce = new System.Windows.Forms.NumericUpDown();
            this.comboFallbackPanel = new System.Windows.Forms.ComboBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxMidiPorts.SuspendLayout();
            this.groupBoxMidiActivity.SuspendLayout();
            this.groupBoxTooltips.SuspendLayout();
            this.groupBoxProcesses.SuspendLayout();
            this.groupBoxAutoSwitch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericAutoSwitchInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAutoSwitchDebounce)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxMidiPorts
            // 
            this.groupBoxMidiPorts.Controls.Add(this.labelMidiOut);
            this.groupBoxMidiPorts.Controls.Add(this.comboMidiOut);
            this.groupBoxMidiPorts.Controls.Add(this.labelMidiIn);
            this.groupBoxMidiPorts.Controls.Add(this.comboMidiIn);
            this.groupBoxMidiPorts.Location = new System.Drawing.Point(20, 20);
            this.groupBoxMidiPorts.Name = "groupBoxMidiPorts";
            this.groupBoxMidiPorts.Size = new System.Drawing.Size(400, 100);
            this.groupBoxMidiPorts.TabIndex = 0;
            this.groupBoxMidiPorts.TabStop = false;
            this.groupBoxMidiPorts.Text = Properties.Resources.GlobalEditorSettings_GroupBoxMidiPorts;
            // 
            // labelMidiOut
            // 
            this.labelMidiOut.Location = new System.Drawing.Point(15, 25);
            this.labelMidiOut.Name = "labelMidiOut";
            this.labelMidiOut.Size = new System.Drawing.Size(120, 23);
            this.labelMidiOut.TabIndex = 0;
            this.labelMidiOut.Text = Properties.Resources.GlobalEditorSettings_LabelPanelMidiOut;
            // 
            // MIDI Out ComboBox
            this.comboMidiOut.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMidiOut.Location = new System.Drawing.Point(161, 22);
            this.comboMidiOut.Name = "comboMidiOut";
            this.comboMidiOut.Size = new System.Drawing.Size(200, 24);
            this.comboMidiOut.TabIndex = 1;
            // 
            // labelMidiIn
            // 
            this.labelMidiIn.Location = new System.Drawing.Point(15, 55);
            this.labelMidiIn.Name = "labelMidiIn";
            this.labelMidiIn.Size = new System.Drawing.Size(120, 23);
            this.labelMidiIn.TabIndex = 2;
            this.labelMidiIn.Text = Properties.Resources.GlobalEditorSettings_LabelPanelMidiIn;
            // 
            // textMidiIn
            // 
            // MIDI In ComboBox
            this.comboMidiIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMidiIn.Location = new System.Drawing.Point(161, 52);
            this.comboMidiIn.Name = "comboMidiIn";
            this.comboMidiIn.Size = new System.Drawing.Size(200, 24);
            this.comboMidiIn.TabIndex = 3;
            // 
            // groupBoxMidiActivity
            // 
            this.groupBoxMidiActivity.Controls.Add(this.labelMIDIInColour);
            this.groupBoxMidiActivity.Controls.Add(this.buttonMIDIInColour);
            this.groupBoxMidiActivity.Controls.Add(this.labelButtonGlowDown);
            this.groupBoxMidiActivity.Controls.Add(this.buttonButtonGlowDown);
            this.groupBoxMidiActivity.Controls.Add(this.labelButtonGlowUp);
            this.groupBoxMidiActivity.Controls.Add(this.buttonButtonGlowUp);
            this.groupBoxMidiActivity.Location = new System.Drawing.Point(20, 130);
            this.groupBoxMidiActivity.Name = "groupBoxMidiActivity";
            this.groupBoxMidiActivity.Size = new System.Drawing.Size(400, 130);
            this.groupBoxMidiActivity.TabIndex = 1;
            this.groupBoxMidiActivity.TabStop = false;
            this.groupBoxMidiActivity.Text = Properties.Resources.GlobalEditorSettings_GroupBoxMidiActivity;
            // 
            // labelMIDIInColour
            // 
            this.labelMIDIInColour.Location = new System.Drawing.Point(15, 25);
            this.labelMIDIInColour.Name = "labelMIDIInColour";
            this.labelMIDIInColour.Size = new System.Drawing.Size(164, 23);
            this.labelMIDIInColour.TabIndex = 0;
            this.labelMIDIInColour.Text = Properties.Resources.GlobalEditorSettings_LabelMidiInColour;
            this.labelMIDIInColour.Click += new System.EventHandler(this.labelMIDIInColour_Click);
            // 
            // buttonMIDIInColour
            // 
            this.buttonMIDIInColour.Location = new System.Drawing.Point(242, 21);
            this.buttonMIDIInColour.Name = "buttonMIDIInColour";
            this.buttonMIDIInColour.Size = new System.Drawing.Size(119, 23);
            this.buttonMIDIInColour.TabIndex = 1;
            // 
            // labelButtonGlowDown
            // 
            this.labelButtonGlowDown.Location = new System.Drawing.Point(15, 55);
            this.labelButtonGlowDown.Name = "labelButtonGlowDown";
            this.labelButtonGlowDown.Size = new System.Drawing.Size(164, 23);
            this.labelButtonGlowDown.TabIndex = 2;
            this.labelButtonGlowDown.Text = Properties.Resources.GlobalEditorSettings_LabelButtonGlowDown;
            // 
            // buttonButtonGlowDown
            // 
            this.buttonButtonGlowDown.Location = new System.Drawing.Point(242, 50);
            this.buttonButtonGlowDown.Name = "buttonButtonGlowDown";
            this.buttonButtonGlowDown.Size = new System.Drawing.Size(119, 23);
            this.buttonButtonGlowDown.TabIndex = 3;
            // 
            // labelButtonGlowUp
            // 
            this.labelButtonGlowUp.Location = new System.Drawing.Point(15, 85);
            this.labelButtonGlowUp.Name = "labelButtonGlowUp";
            this.labelButtonGlowUp.Size = new System.Drawing.Size(181, 23);
            this.labelButtonGlowUp.TabIndex = 4;
            this.labelButtonGlowUp.Text = Properties.Resources.GlobalEditorSettings_LabelButtonGlowUp;
            // 
            // buttonButtonGlowUp
            // 
            this.buttonButtonGlowUp.Location = new System.Drawing.Point(242, 79);
            this.buttonButtonGlowUp.Name = "buttonButtonGlowUp";
            this.buttonButtonGlowUp.Size = new System.Drawing.Size(119, 23);
            this.buttonButtonGlowUp.TabIndex = 5;
            // 
            // groupBoxTooltips
            // 
            this.groupBoxTooltips.Controls.Add(this.labelTooltipBack);
            this.groupBoxTooltips.Controls.Add(this.buttonTooltipBack);
            this.groupBoxTooltips.Controls.Add(this.labelTooltipText);
            this.groupBoxTooltips.Controls.Add(this.buttonTooltipText);
            this.groupBoxTooltips.Location = new System.Drawing.Point(20, 270);
            this.groupBoxTooltips.Name = "groupBoxTooltips";
            this.groupBoxTooltips.Size = new System.Drawing.Size(400, 100);
            this.groupBoxTooltips.TabIndex = 2;
            this.groupBoxTooltips.TabStop = false;
            this.groupBoxTooltips.Text = Properties.Resources.GlobalEditorSettings_GroupBoxTooltips;
            // 
            // labelTooltipBack
            // 
            this.labelTooltipBack.Location = new System.Drawing.Point(15, 25);
            this.labelTooltipBack.Name = "labelTooltipBack";
            this.labelTooltipBack.Size = new System.Drawing.Size(140, 23);
            this.labelTooltipBack.TabIndex = 0;
            this.labelTooltipBack.Text = Properties.Resources.GlobalEditorSettings_LabelTooltipBack;
            this.labelTooltipBack.Click += new System.EventHandler(this.labelTooltipBack_Click_1);
            // 
            // buttonTooltipBack
            // 
            this.buttonTooltipBack.Location = new System.Drawing.Point(242, 20);
            this.buttonTooltipBack.Name = "buttonTooltipBack";
            this.buttonTooltipBack.Size = new System.Drawing.Size(119, 23);
            this.buttonTooltipBack.TabIndex = 1;
            // 
            // labelTooltipText
            // 
            this.labelTooltipText.Location = new System.Drawing.Point(15, 55);
            this.labelTooltipText.Name = "labelTooltipText";
            this.labelTooltipText.Size = new System.Drawing.Size(125, 23);
            this.labelTooltipText.TabIndex = 2;
            this.labelTooltipText.Text = Properties.Resources.GlobalEditorSettings_LabelTooltipText;
            // 
            // buttonTooltipText
            // 
            this.buttonTooltipText.Location = new System.Drawing.Point(242, 52);
            this.buttonTooltipText.Name = "buttonTooltipText";
            this.buttonTooltipText.Size = new System.Drawing.Size(119, 23);
            this.buttonTooltipText.TabIndex = 3;
            // 
            // groupBoxProcesses
            // 
            this.groupBoxProcesses.Controls.Add(this.comboProcesses);
            this.groupBoxProcesses.Controls.Add(this.buttonAddProcess);
            this.groupBoxProcesses.Controls.Add(this.buttonDeleteProcess);
            this.groupBoxProcesses.Controls.Add(this.labelProcessName);
            this.groupBoxProcesses.Controls.Add(this.textProcessName);
            this.groupBoxProcesses.Controls.Add(this.labelPanel);
            this.groupBoxProcesses.Controls.Add(this.comboDefaultPanel);
            this.groupBoxProcesses.Controls.Add(this.labelProcessMidiOut);
            this.groupBoxProcesses.Controls.Add(this.comboProcessMidiOut);
            this.groupBoxProcesses.Controls.Add(this.labelProcessMidiIn);
            this.groupBoxProcesses.Controls.Add(this.comboProcessMidiIn);
            this.groupBoxProcesses.Controls.Add(this.labelLanguageSeparator);
            this.groupBoxProcesses.Controls.Add(this.labelLanguage);
            this.groupBoxProcesses.Controls.Add(this.comboLanguage);
            this.groupBoxProcesses.Location = new System.Drawing.Point(440, 20);
            this.groupBoxProcesses.Name = "groupBoxProcesses";
            this.groupBoxProcesses.Size = new System.Drawing.Size(420, 240);
            this.groupBoxProcesses.TabIndex = 3;
            this.groupBoxProcesses.TabStop = false;
            this.groupBoxProcesses.Text = Properties.Resources.GlobalEditorSettings_GroupBoxProcesses;
            // 
            // comboProcesses
            // 
            this.comboProcesses.Location = new System.Drawing.Point(20, 30);
            this.comboProcesses.Name = "comboProcesses";
            this.comboProcesses.Size = new System.Drawing.Size(200, 24);
            this.comboProcesses.TabIndex = 0;
            // 
            // buttonAddProcess
            // 
            this.buttonAddProcess.Location = new System.Drawing.Point(230, 30);
            this.buttonAddProcess.Name = "buttonAddProcess";
            this.buttonAddProcess.Size = new System.Drawing.Size(73, 23);
            this.buttonAddProcess.TabIndex = 1;
            this.buttonAddProcess.Text = Properties.Resources.GlobalEditorSettings_ButtonAddProcess;
            // 
            // buttonDeleteProcess
            this.buttonDeleteProcess.Location = new System.Drawing.Point(306, 30);
            this.buttonDeleteProcess.Name = "buttonDeleteProcess";
            this.buttonDeleteProcess.Size = new System.Drawing.Size(73, 23);
            this.buttonDeleteProcess.TabIndex = 2;
            this.buttonDeleteProcess.Text = Properties.Resources.GlobalEditorSettings_ButtonDeleteProcess;
            // 
            // labelProcessName
            // 
            this.labelProcessName.Location = new System.Drawing.Point(20, 70);
            this.labelProcessName.Name = "labelProcessName";
            this.labelProcessName.Size = new System.Drawing.Size(150, 23);
            this.labelProcessName.TabIndex = 3;
            this.labelProcessName.Text = Properties.Resources.GlobalEditorSettings_LabelProcessName;
            // 
            // textProcessName
            // 
            this.textProcessName.Location = new System.Drawing.Point(179, 66);
            this.textProcessName.Name = "textProcessName";
            this.textProcessName.Size = new System.Drawing.Size(200, 22);
            this.textProcessName.TabIndex = 4;
            // 
            // labelPanel
            // 
            this.labelPanel.Location = new System.Drawing.Point(20, 100);
            this.labelPanel.Name = "labelPanel";
            this.labelPanel.Size = new System.Drawing.Size(150, 23);
            this.labelPanel.TabIndex = 5;
            this.labelPanel.Text = Properties.Resources.GlobalEditorSettings_LabelPanel;
            // 
            // comboDefaultPanel
            // 
            this.comboDefaultPanel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDefaultPanel.Location = new System.Drawing.Point(179, 95);
            this.comboDefaultPanel.Name = "comboDefaultPanel";
            this.comboDefaultPanel.Size = new System.Drawing.Size(200, 24);
            this.comboDefaultPanel.TabIndex = 6;
            // 
            // labelProcessMidiOut
            // 
            this.labelProcessMidiOut.Location = new System.Drawing.Point(20, 130);
            this.labelProcessMidiOut.Name = "labelProcessMidiOut";
            this.labelProcessMidiOut.Size = new System.Drawing.Size(150, 23);
            this.labelProcessMidiOut.TabIndex = 7;
            this.labelProcessMidiOut.Text = Properties.Resources.GlobalEditorSettings_LabelProcessMidiOut;
            // 
            // 
            // Process MIDI Out ComboBox
            this.comboProcessMidiOut.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboProcessMidiOut.Location = new System.Drawing.Point(179, 126);
            this.comboProcessMidiOut.Name = "comboProcessMidiOut";
            this.comboProcessMidiOut.Size = new System.Drawing.Size(200, 24);
            this.comboProcessMidiOut.TabIndex = 8;
            // 
            // labelProcessMidiIn
            // 
            this.labelProcessMidiIn.Location = new System.Drawing.Point(20, 160);
            this.labelProcessMidiIn.Name = "labelProcessMidiIn";
            this.labelProcessMidiIn.Size = new System.Drawing.Size(150, 23);
            this.labelProcessMidiIn.TabIndex = 9;
            this.labelProcessMidiIn.Text = Properties.Resources.GlobalEditorSettings_LabelProcessMidiIn;
            // 
            // Process MIDI In ComboBox
            this.comboProcessMidiIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboProcessMidiIn.Location = new System.Drawing.Point(179, 156);
            this.comboProcessMidiIn.Name = "comboProcessMidiIn";
            this.comboProcessMidiIn.Size = new System.Drawing.Size(200, 24);
            this.comboProcessMidiIn.TabIndex = 10;
            // 
            // labelLanguageSeparator
            // 
            this.labelLanguageSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelLanguageSeparator.Location = new System.Drawing.Point(20, 200);
            this.labelLanguageSeparator.Name = "labelLanguageSeparator";
            this.labelLanguageSeparator.Size = new System.Drawing.Size(359, 2);
            this.labelLanguageSeparator.TabIndex = 12;
            // 
            // labelLanguage
            // 
            this.labelLanguage.Location = new System.Drawing.Point(20, 210);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(123, 23);
            this.labelLanguage.TabIndex = 13;
            this.labelLanguage.Text = Properties.Resources.GlobalEditorSettings_LabelLanguage;
            // 
            // comboLanguage
            // 
            this.comboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLanguage.Location = new System.Drawing.Point(179, 210);
            this.comboLanguage.Name = "comboLanguage";
            this.comboLanguage.Size = new System.Drawing.Size(200, 24);
            this.comboLanguage.TabIndex = 11;
            // 
            // groupBoxAutoSwitch
            // 
            this.groupBoxAutoSwitch.Controls.Add(this.labelAutoSwitchInterval);
            this.groupBoxAutoSwitch.Controls.Add(this.numericAutoSwitchInterval);
            this.groupBoxAutoSwitch.Controls.Add(this.labelAutoSwitchDebounce);
            this.groupBoxAutoSwitch.Controls.Add(this.numericAutoSwitchDebounce);
            this.groupBoxAutoSwitch.Controls.Add(this.comboFallbackPanel);
            this.groupBoxAutoSwitch.Location = new System.Drawing.Point(440, 270);
            this.groupBoxAutoSwitch.Name = "groupBoxAutoSwitch";
            this.groupBoxAutoSwitch.Size = new System.Drawing.Size(420, 100);
            this.groupBoxAutoSwitch.TabIndex = 4;
            this.groupBoxAutoSwitch.TabStop = false;
            this.groupBoxAutoSwitch.Text = Properties.Resources.GlobalEditorSettings_GroupBoxAutoSwitch;
            // 
            // labelAutoSwitchInterval
            // 
            this.labelAutoSwitchInterval.Location = new System.Drawing.Point(20, 25);
            this.labelAutoSwitchInterval.Name = "labelAutoSwitchInterval";
            this.labelAutoSwitchInterval.Size = new System.Drawing.Size(128, 23);
            this.labelAutoSwitchInterval.TabIndex = 0;
            this.labelAutoSwitchInterval.Text = Properties.Resources.GlobalEditorSettings_LabelAutoSwitchInterval;
            // 
            // numericAutoSwitchInterval
            // 
            this.numericAutoSwitchInterval.Location = new System.Drawing.Point(179, 21);
            this.numericAutoSwitchInterval.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericAutoSwitchInterval.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericAutoSwitchInterval.Name = "numericAutoSwitchInterval";
            this.numericAutoSwitchInterval.Size = new System.Drawing.Size(80, 22);
            this.numericAutoSwitchInterval.TabIndex = 1;
            this.numericAutoSwitchInterval.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // labelAutoSwitchDebounce
            // 
            this.labelAutoSwitchDebounce.Location = new System.Drawing.Point(20, 55);
            this.labelAutoSwitchDebounce.Name = "labelAutoSwitchDebounce";
            this.labelAutoSwitchDebounce.Size = new System.Drawing.Size(148, 23);
            this.labelAutoSwitchDebounce.TabIndex = 2;
            this.labelAutoSwitchDebounce.Text = Properties.Resources.GlobalEditorSettings_LabelFallbackPanel;
            this.labelAutoSwitchDebounce.Click += new System.EventHandler(this.labelAutoSwitchDebounce_Click);
            // 
            // numericAutoSwitchDebounce
            // 
            this.numericAutoSwitchDebounce.Location = new System.Drawing.Point(299, 20);
            this.numericAutoSwitchDebounce.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericAutoSwitchDebounce.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericAutoSwitchDebounce.Name = "numericAutoSwitchDebounce";
            this.numericAutoSwitchDebounce.Size = new System.Drawing.Size(80, 22);
            this.numericAutoSwitchDebounce.TabIndex = 3;
            this.numericAutoSwitchDebounce.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // comboFallbackPanel
            // 
            this.comboFallbackPanel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFallbackPanel.Location = new System.Drawing.Point(179, 52);
            this.comboFallbackPanel.Name = "comboFallbackPanel";
            this.comboFallbackPanel.Size = new System.Drawing.Size(200, 24);
            this.comboFallbackPanel.TabIndex = 4;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(440, 390);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(420, 40);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = Properties.Resources.GlobalEditorSettings_ButtonOK;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(20, 390);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(400, 40);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = Properties.Resources.GlobalEditorSettings_ButtonCancel;
            // 
            // EditorSettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(882, 447);
            this.Controls.Add(this.groupBoxMidiPorts);
            this.Controls.Add(this.groupBoxMidiActivity);
            this.Controls.Add(this.groupBoxTooltips);
            this.Controls.Add(this.groupBoxProcesses);
            this.Controls.Add(this.groupBoxAutoSwitch);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "EditorSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = Properties.Resources.GlobalEditorSettings_FormTitle;
            this.groupBoxMidiPorts.ResumeLayout(false);
            this.groupBoxMidiPorts.PerformLayout();
            this.groupBoxMidiActivity.ResumeLayout(false);
            this.groupBoxTooltips.ResumeLayout(false);
            this.groupBoxProcesses.ResumeLayout(false);
            this.groupBoxProcesses.PerformLayout();
            this.groupBoxAutoSwitch.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericAutoSwitchInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAutoSwitchDebounce)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxMidiPorts;
        private System.Windows.Forms.Label labelMidiOut;
        
        private System.Windows.Forms.Label labelMidiIn;
       

        private System.Windows.Forms.GroupBox groupBoxMidiActivity;
        private System.Windows.Forms.Label labelMIDIInColour;
        private System.Windows.Forms.Button buttonMIDIInColour;
        private System.Windows.Forms.Label labelButtonGlowDown;
        private System.Windows.Forms.Button buttonButtonGlowDown;
        private System.Windows.Forms.Label labelButtonGlowUp;
        private System.Windows.Forms.Button buttonButtonGlowUp;

        private System.Windows.Forms.GroupBox groupBoxTooltips;
        private System.Windows.Forms.Label labelTooltipBack;
        private System.Windows.Forms.Button buttonTooltipBack;
        private System.Windows.Forms.Label labelTooltipText;
        private System.Windows.Forms.Button buttonTooltipText;

        private System.Windows.Forms.GroupBox groupBoxAutoSwitch;
        private System.Windows.Forms.Label labelAutoSwitchInterval;
        private System.Windows.Forms.NumericUpDown numericAutoSwitchInterval;
        private System.Windows.Forms.Label labelAutoSwitchDebounce;
        private System.Windows.Forms.NumericUpDown numericAutoSwitchDebounce;
        private System.Windows.Forms.ComboBox comboFallbackPanel;

        private System.Windows.Forms.GroupBox groupBoxProcesses;
        private System.Windows.Forms.ComboBox comboProcesses;
        private System.Windows.Forms.Button buttonAddProcess;
        private System.Windows.Forms.Button buttonDeleteProcess;
        private System.Windows.Forms.Label labelProcessName;
        private System.Windows.Forms.TextBox textProcessName;
        private System.Windows.Forms.Label labelPanel;
        private System.Windows.Forms.ComboBox comboDefaultPanel;
        private System.Windows.Forms.Label labelProcessMidiOut;
        
        private System.Windows.Forms.Label labelProcessMidiIn;

        private System.Windows.Forms.ComboBox comboMidiIn;
        private System.Windows.Forms.ComboBox comboMidiOut;
        private System.Windows.Forms.ComboBox comboProcessMidiIn;
        private System.Windows.Forms.ComboBox comboProcessMidiOut;

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ComboBox comboLanguage;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.Label labelLanguageSeparator;
    }
}

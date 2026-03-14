// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using System;

namespace Cubendo_Remote_Panel
{
    partial class EditorButtonsINIForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Controls for [Buttons] section
        private System.Windows.Forms.GroupBox groupBoxButtons;
        private System.Windows.Forms.Label labelButtonWidth;
        private System.Windows.Forms.NumericUpDown numericButtonWidth;
        private System.Windows.Forms.Label labelButtonHeight;
        private System.Windows.Forms.NumericUpDown numericButtonHeight;
        private System.Windows.Forms.Label labelButtonGap;
        private System.Windows.Forms.NumericUpDown numericButtonGap;
        private System.Windows.Forms.Label labelBackgroundColour;
        private System.Windows.Forms.Button buttonBackgroundColour;
        private System.Windows.Forms.Label labelButtonBorder;
        private System.Windows.Forms.Button buttonButtonBorder;
        private System.Windows.Forms.Label labelButtonFontName;
        private System.Windows.Forms.Button buttonButtonFontName;
        private System.Windows.Forms.TextBox textBoxButtonFontName;
        private System.Windows.Forms.Label labelButtonFontSize;
        private System.Windows.Forms.NumericUpDown numericButtonFontSize;
        private System.Windows.Forms.Label labelTopMargin;
        private System.Windows.Forms.NumericUpDown numericTopMargin;
        private System.Windows.Forms.Label labelLeftMargin;
        private System.Windows.Forms.NumericUpDown numericLeftMargin;

        // Controls for [UserCollapse] section
        private System.Windows.Forms.GroupBox groupBoxUserCollapse;
        private System.Windows.Forms.Label labelUCButtonWidth;
        private System.Windows.Forms.NumericUpDown numericUCButtonWidth;
        private System.Windows.Forms.Label labelUCButtonHeight;
        private System.Windows.Forms.NumericUpDown numericUCButtonHeight;
        private System.Windows.Forms.Label labelUCButtonColour;
        private System.Windows.Forms.Button buttonUCButtonColour;
        private System.Windows.Forms.Label labelUCBackgroundColour;
        private System.Windows.Forms.Button buttonUCBackgroundColour;
        private System.Windows.Forms.Label labelUCButtonBorder;
        private System.Windows.Forms.Button buttonUCButtonBorder;
        private System.Windows.Forms.Label labelUCButtonFontName;
        private System.Windows.Forms.Button buttonUCButtonFontName;
        private System.Windows.Forms.TextBox textBoxUCButtonFontName;
        private System.Windows.Forms.Label labelUCButtonFontSize;
        private System.Windows.Forms.NumericUpDown numericUCButtonFontSize;
        private System.Windows.Forms.Label labelUCTopMargin;
        private System.Windows.Forms.NumericUpDown numericUCTopMargin;
        private System.Windows.Forms.Label labelUCLeftMargin;
        private System.Windows.Forms.NumericUpDown numericUCLeftMargin;

        // Save/Cancel
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;

        //
        private System.Windows.Forms.Panel panelUCPreview;
        //
        // Copy Paste
        private System.Windows.Forms.ContextMenuStrip colorContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyColorMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteColorMenuItem;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxButtons = new System.Windows.Forms.GroupBox();
            this.labelButtonWidth = new System.Windows.Forms.Label();
            this.numericButtonWidth = new System.Windows.Forms.NumericUpDown();
            this.labelButtonHeight = new System.Windows.Forms.Label();
            this.numericButtonHeight = new System.Windows.Forms.NumericUpDown();
            this.labelButtonGap = new System.Windows.Forms.Label();
            this.numericButtonGap = new System.Windows.Forms.NumericUpDown();
            this.labelBackgroundColour = new System.Windows.Forms.Label();
            this.buttonBackgroundColour = new System.Windows.Forms.Button();
            this.colorContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyColorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteColorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelButtonBorder = new System.Windows.Forms.Label();
            this.buttonButtonBorder = new System.Windows.Forms.Button();
            this.labelButtonFontName = new System.Windows.Forms.Label();
            this.buttonButtonFontName = new System.Windows.Forms.Button();
            this.textBoxButtonFontName = new System.Windows.Forms.TextBox();
            this.labelButtonFontSize = new System.Windows.Forms.Label();
            this.numericButtonFontSize = new System.Windows.Forms.NumericUpDown();
            this.labelTopMargin = new System.Windows.Forms.Label();
            this.numericTopMargin = new System.Windows.Forms.NumericUpDown();
            this.labelLeftMargin = new System.Windows.Forms.Label();
            this.numericLeftMargin = new System.Windows.Forms.NumericUpDown();
            this.groupBoxUserCollapse = new System.Windows.Forms.GroupBox();
            this.labelUCButtonWidth = new System.Windows.Forms.Label();
            this.numericUCButtonWidth = new System.Windows.Forms.NumericUpDown();
            this.labelUCButtonHeight = new System.Windows.Forms.Label();
            this.numericUCButtonHeight = new System.Windows.Forms.NumericUpDown();
            this.labelUCButtonColour = new System.Windows.Forms.Label();
            this.buttonUCButtonColour = new System.Windows.Forms.Button();
            this.labelUCBackgroundColour = new System.Windows.Forms.Label();
            this.buttonUCBackgroundColour = new System.Windows.Forms.Button();
            this.labelUCButtonBorder = new System.Windows.Forms.Label();
            this.buttonUCButtonBorder = new System.Windows.Forms.Button();
            this.labelUCButtonFontName = new System.Windows.Forms.Label();
            this.buttonUCButtonFontName = new System.Windows.Forms.Button();
            this.textBoxUCButtonFontName = new System.Windows.Forms.TextBox();
            this.labelUCButtonFontSize = new System.Windows.Forms.Label();
            this.numericUCButtonFontSize = new System.Windows.Forms.NumericUpDown();
            this.labelUCTopMargin = new System.Windows.Forms.Label();
            this.numericUCTopMargin = new System.Windows.Forms.NumericUpDown();
            this.labelUCLeftMargin = new System.Windows.Forms.Label();
            this.numericUCLeftMargin = new System.Windows.Forms.NumericUpDown();
            this.panelUCPreview = new System.Windows.Forms.Panel();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericButtonWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericButtonHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericButtonGap)).BeginInit();
            this.colorContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericButtonFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTopMargin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLeftMargin)).BeginInit();
            this.groupBoxUserCollapse.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCButtonWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCButtonHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCButtonFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCTopMargin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCLeftMargin)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxButtons
            // 
            this.groupBoxButtons.Controls.Add(this.labelButtonWidth);
            this.groupBoxButtons.Controls.Add(this.numericButtonWidth);
            this.groupBoxButtons.Controls.Add(this.labelButtonHeight);
            this.groupBoxButtons.Controls.Add(this.numericButtonHeight);
            this.groupBoxButtons.Controls.Add(this.labelButtonGap);
            this.groupBoxButtons.Controls.Add(this.numericButtonGap);
            this.groupBoxButtons.Controls.Add(this.labelBackgroundColour);
            this.groupBoxButtons.Controls.Add(this.buttonBackgroundColour);
            this.groupBoxButtons.Controls.Add(this.labelButtonBorder);
            this.groupBoxButtons.Controls.Add(this.buttonButtonBorder);
            this.groupBoxButtons.Controls.Add(this.labelButtonFontName);
            this.groupBoxButtons.Controls.Add(this.buttonButtonFontName);
            this.groupBoxButtons.Controls.Add(this.textBoxButtonFontName);
            this.groupBoxButtons.Controls.Add(this.labelButtonFontSize);
            this.groupBoxButtons.Controls.Add(this.numericButtonFontSize);
            this.groupBoxButtons.Controls.Add(this.labelTopMargin);
            this.groupBoxButtons.Controls.Add(this.numericTopMargin);
            this.groupBoxButtons.Controls.Add(this.labelLeftMargin);
            this.groupBoxButtons.Controls.Add(this.numericLeftMargin);
            this.groupBoxButtons.Location = new System.Drawing.Point(12, 12);
            this.groupBoxButtons.Name = "groupBoxButtons";
            this.groupBoxButtons.Size = new System.Drawing.Size(440, 320); // Increased height
            this.groupBoxButtons.TabIndex = 0;
            this.groupBoxButtons.TabStop = false;
            this.groupBoxButtons.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_GroupBoxButtons;
            // 
            // labelButtonWidth
            // 
            this.labelButtonWidth.Location = new System.Drawing.Point(20, 28);
            this.labelButtonWidth.Name = "labelButtonWidth";
            this.labelButtonWidth.Size = new System.Drawing.Size(130, 20);
            this.labelButtonWidth.TabIndex = 0;
            this.labelButtonWidth.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelButtonWidth;
            // 
            // numericButtonWidth
            // 
            this.numericButtonWidth.Location = new System.Drawing.Point(170, 26);
            this.numericButtonWidth.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericButtonWidth.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericButtonWidth.Name = "numericButtonWidth";
            this.numericButtonWidth.Size = new System.Drawing.Size(70, 22);
            this.numericButtonWidth.TabIndex = 1;
            this.numericButtonWidth.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // labelButtonHeight
            // 
            this.labelButtonHeight.Location = new System.Drawing.Point(20, 58);
            this.labelButtonHeight.Name = "labelButtonHeight";
            this.labelButtonHeight.Size = new System.Drawing.Size(130, 20);
            this.labelButtonHeight.TabIndex = 2;
            this.labelButtonHeight.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelButtonHeight;
            // 
            // numericButtonHeight
            // 
            this.numericButtonHeight.Location = new System.Drawing.Point(170, 56);
            this.numericButtonHeight.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericButtonHeight.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericButtonHeight.Name = "numericButtonHeight";
            this.numericButtonHeight.Size = new System.Drawing.Size(70, 22);
            this.numericButtonHeight.TabIndex = 3;
            this.numericButtonHeight.Value = new decimal(new int[] {
            104,
            0,
            0,
            0});
            // 
            // labelButtonGap
            // 
            this.labelButtonGap.Location = new System.Drawing.Point(20, 88);
            this.labelButtonGap.Name = "labelButtonGap";
            this.labelButtonGap.Size = new System.Drawing.Size(130, 20);
            this.labelButtonGap.TabIndex = 4;
            this.labelButtonGap.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelButtonGap;
            // 
            // numericButtonGap
            // 
            this.numericButtonGap.Location = new System.Drawing.Point(170, 86);
            this.numericButtonGap.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericButtonGap.Name = "numericButtonGap";
            this.numericButtonGap.Size = new System.Drawing.Size(70, 22);
            this.numericButtonGap.TabIndex = 5;
            // 
            // labelBackgroundColour
            // 
            this.labelBackgroundColour.Location = new System.Drawing.Point(20, 118);
            this.labelBackgroundColour.Name = "labelBackgroundColour";
            this.labelBackgroundColour.Size = new System.Drawing.Size(130, 20);
            this.labelBackgroundColour.TabIndex = 6;
            this.labelBackgroundColour.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelBackgroundColour;
            // 
            // buttonBackgroundColour
            // 
            this.buttonBackgroundColour.ContextMenuStrip = this.colorContextMenu;
            this.buttonBackgroundColour.Location = new System.Drawing.Point(170, 116);
            this.buttonBackgroundColour.Name = "buttonBackgroundColour";
            this.buttonBackgroundColour.Size = new System.Drawing.Size(119, 23);
            this.buttonBackgroundColour.TabIndex = 7;
            // 
            // colorContextMenu
            // 
            this.colorContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.colorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyColorMenuItem,
            this.pasteColorMenuItem});
            this.colorContextMenu.Name = "colorContextMenu";
            this.colorContextMenu.Size = new System.Drawing.Size(153, 52);
            // 
            // copyColorMenuItem
            // 
            this.copyColorMenuItem.Name = "copyColorMenuItem";
            this.copyColorMenuItem.Size = new System.Drawing.Size(152, 24);
            this.copyColorMenuItem.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_CopyColorMenuItem;
            // 
            // pasteColorMenuItem
            // 
            this.pasteColorMenuItem.Name = "pasteColorMenuItem";
            this.pasteColorMenuItem.Size = new System.Drawing.Size(152, 24);
            this.pasteColorMenuItem.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_PasteColorMenuItem;
            // 
            // labelButtonBorder
            // 
            this.labelButtonBorder.Location = new System.Drawing.Point(20, 148);
            this.labelButtonBorder.Name = "labelButtonBorder";
            this.labelButtonBorder.Size = new System.Drawing.Size(130, 20);
            this.labelButtonBorder.TabIndex = 9;
            this.labelButtonBorder.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelButtonBorder;
            // 
            // buttonButtonBorder
            // 
            this.buttonButtonBorder.ContextMenuStrip = this.colorContextMenu;
            this.buttonButtonBorder.Location = new System.Drawing.Point(170, 146);
            this.buttonButtonBorder.Name = "buttonButtonBorder";
            this.buttonButtonBorder.Size = new System.Drawing.Size(119, 23);
            this.buttonButtonBorder.TabIndex = 10;
            // 
            // labelButtonFontName
            // 
            this.labelButtonFontName.Location = new System.Drawing.Point(20, 178);
            this.labelButtonFontName.Name = "labelButtonFontName";
            this.labelButtonFontName.Size = new System.Drawing.Size(130, 20);
            this.labelButtonFontName.TabIndex = 12;
            this.labelButtonFontName.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelButtonFontName;
            // 
            // buttonButtonFontName
            // 
            this.buttonButtonFontName.Location = new System.Drawing.Point(170, 176);
            this.buttonButtonFontName.Name = "buttonButtonFontName";
            this.buttonButtonFontName.Size = new System.Drawing.Size(70, 23);
            this.buttonButtonFontName.TabIndex = 13;
            this.buttonButtonFontName.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_ButtonFontNamePick;
            // 
            // textBoxButtonFontName
            // 
            this.textBoxButtonFontName.Location = new System.Drawing.Point(250, 176);
            this.textBoxButtonFontName.Name = "textBoxButtonFontName";
            this.textBoxButtonFontName.ReadOnly = true;
            this.textBoxButtonFontName.Size = new System.Drawing.Size(120, 22);
            this.textBoxButtonFontName.TabIndex = 14;
            // 
            // labelButtonFontSize
            // 
            this.labelButtonFontSize.Location = new System.Drawing.Point(20, 208);
            this.labelButtonFontSize.Name = "labelButtonFontSize";
            this.labelButtonFontSize.Size = new System.Drawing.Size(130, 20);
            this.labelButtonFontSize.TabIndex = 15;
            this.labelButtonFontSize.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelButtonFontSize;
            // 
            // numericButtonFontSize
            // 
            this.numericButtonFontSize.DecimalPlaces = 1;
            this.numericButtonFontSize.Location = new System.Drawing.Point(170, 206);
            this.numericButtonFontSize.Maximum = new decimal(new int[] {
            96,
            0,
            0,
            0});
            this.numericButtonFontSize.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericButtonFontSize.Name = "numericButtonFontSize";
            this.numericButtonFontSize.Size = new System.Drawing.Size(70, 22);
            this.numericButtonFontSize.TabIndex = 16;
            this.numericButtonFontSize.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // labelTopMargin
            // 
            this.labelTopMargin.Location = new System.Drawing.Point(20, 238);
            this.labelTopMargin.Name = "labelTopMargin";
            this.labelTopMargin.Size = new System.Drawing.Size(130, 20);
            this.labelTopMargin.TabIndex = 17;
            this.labelTopMargin.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelTopMargin;
            // 
            // numericTopMargin
            // 
            this.numericTopMargin.Location = new System.Drawing.Point(170, 236);
            this.numericTopMargin.Name = "numericTopMargin";
            this.numericTopMargin.Size = new System.Drawing.Size(70, 22);
            this.numericTopMargin.TabIndex = 18;
            // 
            // labelLeftMargin
            // 
            this.labelLeftMargin.Location = new System.Drawing.Point(20, 268); // Moved below Top Margin
            this.labelLeftMargin.Name = "labelLeftMargin";
            this.labelLeftMargin.Size = new System.Drawing.Size(130, 20);
            this.labelLeftMargin.TabIndex = 19;
            this.labelLeftMargin.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelLeftMargin;
            // 
            // numericLeftMargin
            // 
            this.numericLeftMargin.Location = new System.Drawing.Point(170, 266); // Moved below Top Margin
            this.numericLeftMargin.Name = "numericLeftMargin";
            this.numericLeftMargin.Size = new System.Drawing.Size(70, 22);
            this.numericLeftMargin.TabIndex = 20;
            // 
            // groupBoxUserCollapse
            // 
            this.groupBoxUserCollapse.Controls.Add(this.labelUCButtonWidth);
            this.groupBoxUserCollapse.Controls.Add(this.numericUCButtonWidth);
            this.groupBoxUserCollapse.Controls.Add(this.labelUCButtonHeight);
            this.groupBoxUserCollapse.Controls.Add(this.numericUCButtonHeight);
            this.groupBoxUserCollapse.Controls.Add(this.labelUCButtonColour);
            this.groupBoxUserCollapse.Controls.Add(this.buttonUCButtonColour);
            this.groupBoxUserCollapse.Controls.Add(this.labelUCBackgroundColour);
            this.groupBoxUserCollapse.Controls.Add(this.buttonUCBackgroundColour);
            this.groupBoxUserCollapse.Controls.Add(this.labelUCButtonBorder);
            this.groupBoxUserCollapse.Controls.Add(this.buttonUCButtonBorder);
            this.groupBoxUserCollapse.Controls.Add(this.labelUCButtonFontName);
            this.groupBoxUserCollapse.Controls.Add(this.buttonUCButtonFontName);
            this.groupBoxUserCollapse.Controls.Add(this.textBoxUCButtonFontName);
            this.groupBoxUserCollapse.Controls.Add(this.labelUCButtonFontSize);
            this.groupBoxUserCollapse.Controls.Add(this.numericUCButtonFontSize);
            this.groupBoxUserCollapse.Controls.Add(this.labelUCTopMargin);
            this.groupBoxUserCollapse.Controls.Add(this.numericUCTopMargin);
            this.groupBoxUserCollapse.Controls.Add(this.labelUCLeftMargin);
            this.groupBoxUserCollapse.Controls.Add(this.numericUCLeftMargin);
            this.groupBoxUserCollapse.Controls.Add(this.panelUCPreview);
            this.groupBoxUserCollapse.Location = new System.Drawing.Point(460, 12);
            this.groupBoxUserCollapse.Name = "groupBoxUserCollapse";
            this.groupBoxUserCollapse.Size = new System.Drawing.Size(440, 320); // Increased height
            this.groupBoxUserCollapse.TabIndex = 1;
            this.groupBoxUserCollapse.TabStop = false;
            this.groupBoxUserCollapse.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_GroupBoxUserCollapse;
            // 
            // labelUCButtonWidth
            // 
            this.labelUCButtonWidth.Location = new System.Drawing.Point(20, 28);
            this.labelUCButtonWidth.Name = "labelUCButtonWidth";
            this.labelUCButtonWidth.Size = new System.Drawing.Size(130, 20);
            this.labelUCButtonWidth.TabIndex = 0;
            this.labelUCButtonWidth.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelUCButtonWidth;
            // 
            // numericUCButtonWidth
            // 
            this.numericUCButtonWidth.Location = new System.Drawing.Point(168, 26);
            this.numericUCButtonWidth.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUCButtonWidth.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUCButtonWidth.Name = "numericUCButtonWidth";
            this.numericUCButtonWidth.Size = new System.Drawing.Size(70, 22);
            this.numericUCButtonWidth.TabIndex = 1;
            this.numericUCButtonWidth.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // labelUCButtonHeight
            // 
            this.labelUCButtonHeight.Location = new System.Drawing.Point(20, 58);
            this.labelUCButtonHeight.Name = "labelUCButtonHeight";
            this.labelUCButtonHeight.Size = new System.Drawing.Size(130, 20);
            this.labelUCButtonHeight.TabIndex = 2;
            this.labelUCButtonHeight.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelUCButtonHeight;
            // 
            // numericUCButtonHeight
            // 
            this.numericUCButtonHeight.Location = new System.Drawing.Point(168, 56);
            this.numericUCButtonHeight.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUCButtonHeight.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUCButtonHeight.Name = "numericUCButtonHeight";
            this.numericUCButtonHeight.Size = new System.Drawing.Size(70, 22);
            this.numericUCButtonHeight.TabIndex = 3;
            this.numericUCButtonHeight.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // labelUCButtonColour
            // 
            this.labelUCButtonColour.Location = new System.Drawing.Point(20, 88);
            this.labelUCButtonColour.Name = "labelUCButtonColour";
            this.labelUCButtonColour.Size = new System.Drawing.Size(130, 20);
            this.labelUCButtonColour.TabIndex = 4;
            this.labelUCButtonColour.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelUCButtonColour;
            // 
            // buttonUCButtonColour
            // 
            this.buttonUCButtonColour.ContextMenuStrip = this.colorContextMenu;
            this.buttonUCButtonColour.Location = new System.Drawing.Point(168, 86);
            this.buttonUCButtonColour.Name = "buttonUCButtonColour";
            this.buttonUCButtonColour.Size = new System.Drawing.Size(119, 23);
            this.buttonUCButtonColour.TabIndex = 5;
            // 
            // labelUCBackgroundColour
            // 
            this.labelUCBackgroundColour.Location = new System.Drawing.Point(20, 118);
            this.labelUCBackgroundColour.Name = "labelUCBackgroundColour";
            this.labelUCBackgroundColour.Size = new System.Drawing.Size(130, 20);
            this.labelUCBackgroundColour.TabIndex = 7;
            this.labelUCBackgroundColour.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelUCBackgroundColour;
            // 
            // buttonUCBackgroundColour
            // 
            this.buttonUCBackgroundColour.ContextMenuStrip = this.colorContextMenu;
            this.buttonUCBackgroundColour.Location = new System.Drawing.Point(168, 116);
            this.buttonUCBackgroundColour.Name = "buttonUCBackgroundColour";
            this.buttonUCBackgroundColour.Size = new System.Drawing.Size(119, 23);
            this.buttonUCBackgroundColour.TabIndex = 8;
            // 
            // labelUCButtonBorder
            // 
            this.labelUCButtonBorder.Location = new System.Drawing.Point(20, 148);
            this.labelUCButtonBorder.Name = "labelUCButtonBorder";
            this.labelUCButtonBorder.Size = new System.Drawing.Size(130, 20);
            this.labelUCButtonBorder.TabIndex = 10;
            this.labelUCButtonBorder.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelUCButtonBorder;
            // 
            // buttonUCButtonBorder
            // 
            this.buttonUCButtonBorder.ContextMenuStrip = this.colorContextMenu;
            this.buttonUCButtonBorder.Location = new System.Drawing.Point(168, 146);
            this.buttonUCButtonBorder.Name = "buttonUCButtonBorder";
            this.buttonUCButtonBorder.Size = new System.Drawing.Size(119, 23);
            this.buttonUCButtonBorder.TabIndex = 11;
            // 
            // labelUCButtonFontName
            // 
            this.labelUCButtonFontName.Location = new System.Drawing.Point(20, 178);
            this.labelUCButtonFontName.Name = "labelUCButtonFontName";
            this.labelUCButtonFontName.Size = new System.Drawing.Size(130, 20);
            this.labelUCButtonFontName.TabIndex = 13;
            this.labelUCButtonFontName.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelUCButtonFontName;
            // 
            // buttonUCButtonFontName
            // 
            this.buttonUCButtonFontName.Location = new System.Drawing.Point(168, 176);
            this.buttonUCButtonFontName.Name = "buttonUCButtonFontName";
            this.buttonUCButtonFontName.Size = new System.Drawing.Size(70, 23);
            this.buttonUCButtonFontName.TabIndex = 14;
            this.buttonUCButtonFontName.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_UCButtonFontNamePick;
            // 
            // textBoxUCButtonFontName
            // 
            this.textBoxUCButtonFontName.Location = new System.Drawing.Point(248, 176);
            this.textBoxUCButtonFontName.Name = "textBoxUCButtonFontName";
            this.textBoxUCButtonFontName.ReadOnly = true;
            this.textBoxUCButtonFontName.Size = new System.Drawing.Size(120, 22);
            this.textBoxUCButtonFontName.TabIndex = 15;
            // 
            // labelUCButtonFontSize
            // 
            this.labelUCButtonFontSize.Location = new System.Drawing.Point(20, 208);
            this.labelUCButtonFontSize.Name = "labelUCButtonFontSize";
            this.labelUCButtonFontSize.Size = new System.Drawing.Size(130, 20);
            this.labelUCButtonFontSize.TabIndex = 16;
            this.labelUCButtonFontSize.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelUCButtonFontSize;
            // 
            // numericUCButtonFontSize
            // 
            this.numericUCButtonFontSize.DecimalPlaces = 1;
            this.numericUCButtonFontSize.Location = new System.Drawing.Point(168, 206);
            this.numericUCButtonFontSize.Maximum = new decimal(new int[] {
            96,
            0,
            0,
            0});
            this.numericUCButtonFontSize.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUCButtonFontSize.Name = "numericUCButtonFontSize";
            this.numericUCButtonFontSize.Size = new System.Drawing.Size(70, 22);
            this.numericUCButtonFontSize.TabIndex = 17;
            this.numericUCButtonFontSize.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // labelUCTopMargin
            // 
            this.labelUCTopMargin.Location = new System.Drawing.Point(20, 238);
            this.labelUCTopMargin.Name = "labelUCTopMargin";
            this.labelUCTopMargin.Size = new System.Drawing.Size(130, 20);
            this.labelUCTopMargin.TabIndex = 18;
            this.labelUCTopMargin.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelUCTopMargin;
            // 
            // numericUCTopMargin
            // 
            this.numericUCTopMargin.Location = new System.Drawing.Point(168, 236);
            this.numericUCTopMargin.Name = "numericUCTopMargin";
            this.numericUCTopMargin.Size = new System.Drawing.Size(70, 22);
            this.numericUCTopMargin.TabIndex = 19;
            // 
            // labelUCLeftMargin
            // 
            this.labelUCLeftMargin.Location = new System.Drawing.Point(20, 268); // Moved below Top Margin
            this.labelUCLeftMargin.Name = "labelUCLeftMargin";
            this.labelUCLeftMargin.Size = new System.Drawing.Size(130, 20);
            this.labelUCLeftMargin.TabIndex = 20;
            this.labelUCLeftMargin.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_LabelUCLeftMargin;
            // 
            // numericUCLeftMargin
            // 
            this.numericUCLeftMargin.Location = new System.Drawing.Point(168, 266); // Moved below Top Margin
            this.numericUCLeftMargin.Name = "numericUCLeftMargin";
            this.numericUCLeftMargin.Size = new System.Drawing.Size(70, 22);
            this.numericUCLeftMargin.TabIndex = 21;
            // 
            // panelUCPreview
            // 
            this.panelUCPreview.BackColor = System.Drawing.Color.Black;
            this.panelUCPreview.Location = new System.Drawing.Point(320, 88);
            this.panelUCPreview.Name = "panelUCPreview";
            this.panelUCPreview.Size = new System.Drawing.Size(95, 80);
            this.panelUCPreview.TabIndex = 22;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(458, 354); // Moved down
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(440, 40);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_ButtonSave;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(12, 354); // Moved down
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(440, 40);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_ButtonCancel;
            // 
            // EditorButtonsINIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(915, 412); // Increased height
            this.Controls.Add(this.groupBoxButtons);
            this.Controls.Add(this.groupBoxUserCollapse);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonCancel);
            this.Name = "EditorButtonsINIForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = global::Cubendo_Remote_Panel.Properties.Resources.ButtonEditorSettings_FormTitle;
            this.groupBoxButtons.ResumeLayout(false);
            this.groupBoxButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericButtonWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericButtonHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericButtonGap)).EndInit();
            this.colorContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericButtonFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTopMargin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLeftMargin)).EndInit();
            this.groupBoxUserCollapse.ResumeLayout(false);
            this.groupBoxUserCollapse.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCButtonWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCButtonHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCButtonFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCTopMargin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUCLeftMargin)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}

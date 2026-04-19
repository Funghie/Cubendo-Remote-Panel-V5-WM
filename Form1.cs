// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using AutoHotkey.Interop;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Cubendo_Remote_Panel
{
    public partial class Form1 : Form
    {
        private MidiOut midiOut;
        private Midifromhost midiFromHost;

        private class MidiAction
        {
            public string Name { get; set; }
            public string Tooltip { get; set; }
            public int Channel { get; set; }
            public bool IsNote { get; set; }
            public bool IsKey { get; set; } // NEW: true if KEY action
            public int Value { get; set; }
            public int MouseDownValue { get; set; }
            public int? MouseUpValue { get; set; } // null = do nothing
            public string KeyString { get; set; } // <-- Add this line
        }

        // Add this at the top of Form1
        private class ProcessMidiConfig
        {
            public string ProcessName;
            public string PanelFile;
            public string MidiOutName;
            public string MidiInName;
        }

        private class SectionInfo
        {
            public string Id;
            public string Name;
            public Color Color;
        }

        // Set version number in AssemblyInfo.cs
        public static string AppVersion =>
        System.Diagnostics.FileVersionInfo
        .GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)
        .FileVersion;

        private List<MidiAction> midiActions = new List<MidiAction>();
        private Dictionary<Button, Color> buttonDefaultColors = new Dictionary<Button, Color>();
        private Color midiInColor = Color.LimeGreen; // Default if not set in INI
        private Color buttonGlowColorDown = Color.Red; // Default if not set in INI
        private Color buttonGlowColorUp = Color.Purple;  // Default if not set in INI
        private Color? buttonBorderColor = null;
        private Color? collapsibleButtonColor = null;
        private Color expandedPanelBackgroundColor = Color.White;
        private Dictionary<Button, bool> midiInActive = new Dictionary<Button, bool>();
        private string midiOutNameLabel = "";
        private string midiInNameLabel = "";
        private List<SectionInfo> sections = new List<SectionInfo>();
        private List<string> sectionIdForAction = new List<string>(); // parallel to midiActions
        private List<string> monitoredProcesses = new List<string>();
        private string buttonFontName = "Segoe UI"; // Default font
        private const int TopMargin = 2;
        private const int LeftMargin = 6;
        private int effectiveTopMargin = TopMargin;
        private int effectiveLeftMargin = LeftMargin;
        private int buttonPanelScrollOffset = 0;
        private Timer collapseCheckTimer;
        private Dictionary<string, string> autoSwitchPanel = new Dictionary<string, string>();
        private Timer autoSwitchTimer;
        // private string lastAutoSwitchedProcess = null;
        private DateTime processFocusStartTime = DateTime.MinValue;
        private string fallbackPanelFile = "buttons.txt"; // default fallback
        private Dictionary<string, ProcessMidiConfig> processMidiConfigs = new Dictionary<string, ProcessMidiConfig>();

        // AppData layout helpers
        private static readonly string AppDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Phil Pendlebury", "CN Remote");

        private static string ButtonsFolder() => Path.Combine(AppDataRoot, "Buttons");
        private static string ResourcesFolder() => Path.Combine(AppDataRoot, "Resources");
        private static string SettingsFolder() => Path.Combine(AppDataRoot, "Settings");
        private static string SettingsIniFile() => Path.Combine(SettingsFolder(), "settings.ini");
        private static string ReadmeFile() => Path.Combine(AppDataRoot, "README.txt");

        private void EnsureAppDataFoldersExist()
        {
            try
            {
                Directory.CreateDirectory(ButtonsFolder());
                Directory.CreateDirectory(ResourcesFolder());
                Directory.CreateDirectory(SettingsFolder());
            }
            catch
            {
                // ignore; IO errors will surface on file access
            }
        }

        // UI / behaviour fields
        private float buttonFontSize = 8.25f;
        private Size lastClientSize = Size.Empty;
        private int buttonWidth = 100;
        private int buttonHeight = 22;
        private int buttonGap = 0;
        private bool titleBarVisible = true;
        private bool statusVisible = true;
        private bool displayHeaders = true;
        private bool minimizeToTray = false;
        private bool statusTooltipVisible = false;
        // private int lastScrollY = 0;
        public static bool midiEnabled = true;
        // Collapse?
        private bool collapsible = false;
        private Button collapseButton;
        private Size? previousWindowSize = null;
        private Point? previousWindowLocation = null;
        private bool autoSwitchEnabled = false;
        private int autoSwitchInterval = 500; // ms, read-only from [Switching]
        private int autoSwitchDebounce = 1000; // ms, read-only from [Switching]
        private bool isSettingMidiDevices = false;


        // WinAPI for forwarding keys
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;

        private IntPtr cubaseHwnd = IntPtr.Zero;
        private string activeDawName = "None";

        // private string fileName = "buttons.txt";
        private string lastButtonFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Phil Pendlebury", "CN Remote", "Buttons", "buttons.txt");

        private ContextMenuStrip mainContextMenu;
        private ToolTip statusToolTip;
        private ToolTip buttonToolTip = new ToolTip { AutoPopDelay = 5000, InitialDelay = 500, ReshowDelay = 500, ShowAlways = true };
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusActiveDaw;
        private ToolStripStatusLabel toolStripStatusMidiOut;
        private ToolStripStatusLabel toolStripStatusMidiIn;
        private ToolStripStatusLabel toolStripStatusAOT;
        private ToolStripMenuItem trayTitleBarItem;
        private ToolStripMenuItem titleBarItem;
        private ToolStripMenuItem headersItem;
        private ToolStripMenuItem collapsibleItem;
        private Panel buttonPanel;
        private NotifyIcon trayIcon;
        private ToolStripMenuItem trayAotItem; // <-- Add this line
        private ToolStripMenuItem aotItem;     // <-- If not already present, add for main menu                                    // Place these fields at the top of your Form1 class (or adjust as needed)
        private Font tooltipFont = new Font("Segoe UI", 9, FontStyle.Regular); // Fixed font and size for tooltips
        private Color tooltipBackColor = Color.Yellow; // Set to your preferred background color
        private Color tooltipForeColor = Color.Black;  // Set to your preferred text color

        /// <summary>
        /// Constructor
        /// </summary>

        private static System.Threading.Mutex singleInstanceMutex;

        public Form1()
        {
            // Prevent multiple instances
            bool createdNew;
            singleInstanceMutex = new System.Threading.Mutex(true, "CubendoRemotePanelMutex", out createdNew);
            if (!createdNew)
            {
                Environment.Exit(0);
                return;
            }

            EnsureAppDataFoldersExist();

            // Check for configured MIDI devices
            string configuredMidiOut = ReadIni("MIDI", "MidiOut");
            string configuredMidiIn = ReadIni("MIDI", "MidiIn");

            if (!string.IsNullOrEmpty(configuredMidiOut) || !string.IsNullOrEmpty(configuredMidiIn))
            {
                bool midiOutFound = string.IsNullOrEmpty(configuredMidiOut);
                bool midiInFound = string.IsNullOrEmpty(configuredMidiIn);

                for (int i = 0; i < MidiOut.NumberOfDevices; i++)
                {
                    if (MidiOut.DeviceInfo(i).ProductName.Equals(configuredMidiOut, StringComparison.OrdinalIgnoreCase))
                    { midiOutFound = true; break; }
                }
                for (int i = 0; i < MidiIn.NumberOfDevices; i++)
                {
                    if (MidiIn.DeviceInfo(i).ProductName.Equals(configuredMidiIn, StringComparison.OrdinalIgnoreCase))
                    { midiInFound = true; break; }
                }

                if (!midiOutFound || !midiInFound)
                {
                    DialogResult result = MessageBox.Show(
                    Properties.Resources.DialogLoopMidiNotRunning.Replace("\\n", Environment.NewLine),
                    Properties.Resources.DialogMidiDeviceNotFound,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                    if (result == DialogResult.No)
                    {
                        Environment.Exit(1);
                        return;
                    }
                    midiEnabled = false;
                }
            }


            // Existing logic
            string lastOpenedFile = ReadIni("Window", "LastOpened", "buttons.txt");
            string defaultButtonFile = Path.Combine(ButtonsFolder(), lastOpenedFile);
            // Fallback if file does not exist
            if (!File.Exists(defaultButtonFile))
                defaultButtonFile = Path.Combine(ButtonsFolder(), "buttons.txt");

            // New: If even buttons.txt does not exist, show error and exit
            if (!File.Exists(defaultButtonFile))
            {
                MessageBox.Show(
                    Properties.Resources.DialogDefaultButtonsMissing,
                    Properties.Resources.DialogPanelFileMissing,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Environment.Exit(1);
                return;
            }

            lastButtonFile = defaultButtonFile;
            // WriteIni("Window", "LastOpened", Path.GetFileName(lastButtonFile));

            this.StartPosition = FormStartPosition.Manual;
            InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.Dpi; // <-- Place it here

            buttonToolTip.OwnerDraw = true;
            buttonToolTip.Draw += ToolTip_Draw;
            buttonToolTip.Popup += ToolTip_Popup;

            // FUNGHIE
            this.ResizeEnd += Form1_ResizeEnd;
            // FUNGHIE

            // Set minimum size for the form
            this.MinimumSize = new Size(1, 1); // Minimum width 40px, minimum height 40px

            // Now restore ALL settings (including [Buttons] and [Window])
            LoadPanelSettingsFromIni(lastButtonFile);
            SyncMenuItemsWithSettings(lastButtonFile);

            DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            // Load icon from AppData resources if present
            string icoPath = Path.Combine(ResourcesFolder(), "crp.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            // Tray context menu
            ContextMenuStrip trayMenu = new ContextMenuStrip();

            // Always On Top
            trayAotItem = new ToolStripMenuItem(Properties.Resources.MenuAlwaysOnTop) { Checked = TopMost, CheckOnClick = true };
            trayAotItem.Click += (s, e) =>
            {
                TopMost = trayAotItem.Checked;
                aotItem.Checked = trayAotItem.Checked; // sync main menu
                WriteIni("Window", "AOT", TopMost ? "True" : "False");
                UpdateStatusStrip();
                if (cubaseHwnd != IntPtr.Zero)
                {
                    System.Threading.Thread.Sleep(100);
                    SetForegroundWindow(cubaseHwnd);
                }
            };
            trayMenu.Items.Add(trayAotItem);

            // Show Title Bar
            trayTitleBarItem = new ToolStripMenuItem(Properties.Resources.MenuShowTitleBar) { Checked = titleBarVisible, CheckOnClick = true };
            trayTitleBarItem.Click += (s, e) =>
            {
                titleBarVisible = trayTitleBarItem.Checked;
                titleBarItem.Checked = trayTitleBarItem.Checked; // sync main menu to tray
                ToggleTitleBar();

                WriteIniToButtonFile("Window", "Borderless", (!titleBarVisible ? "True" : "False"));
                SaveButtonIniFile(lastButtonFile);
            };
            trayMenu.Items.Add(trayTitleBarItem);

            // Divider
            trayMenu.Items.Add(new ToolStripSeparator());

            // Select Button Panel
            ToolStripMenuItem traySelectButtonPanelItem = new ToolStripMenuItem(Properties.Resources.MenuSelectButtonPanel);
            traySelectButtonPanelItem.Click += SelectButtonPanel_Click;
            trayMenu.Items.Add(traySelectButtonPanelItem);

            // Move Panel to Centre
            ToolStripMenuItem trayMoveToCenterItem = new ToolStripMenuItem(Properties.Resources.MenuMovePanelToCentre);
            trayMoveToCenterItem.Click += (s, e) =>
            {
                Rectangle screen = Screen.PrimaryScreen.WorkingArea;
                this.Location = new Point(
                    screen.Left + (screen.Width - this.Width) / 2,
                    screen.Top + (screen.Height - this.Height) / 2
                );
                SaveButtonIniFile(lastButtonFile);
            };
            trayMenu.Items.Add(trayMoveToCenterItem);

            // Divider
            trayMenu.Items.Add(new ToolStripSeparator());

            // Show / Hide
            trayMenu.Items.Add(Properties.Resources.TrayShowHide, null, (s, e) =>
            {
                if (WindowState == FormWindowState.Minimized || !Visible)
                {
                    Show();
                    WindowState = FormWindowState.Normal;
                    Activate();
                }
                else
                {
                    WindowState = FormWindowState.Minimized;
                }
            });

            // Exit Application
            trayMenu.Items.Add(Properties.Resources.TrayExitApplication, null, (s, e) => Close());

            // Initialize tray icon (simplified)
            trayIcon = new NotifyIcon
            {
                Icon = this.Icon,
                Text = "", // Will be set by UpdateFormTitle()
                Visible = true,
                ContextMenuStrip = trayMenu
            };
            UpdateFormTitle();

            trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (WindowState == FormWindowState.Minimized || !Visible)
                    {
                        Show();
                        WindowState = FormWindowState.Normal;
                        Activate();
                    }
                    else
                    {
                        WindowState = FormWindowState.Minimized;
                    }
                }
            };

            // this.Text = "Cubendo Remote Panel";

            this.Resize += Form1_Resize;
            this.MouseDown += Form1_MouseDownForDrag;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.Shown += Form1_Shown;

            LoadSettingsFromIni();
            // Use lastButtonFile here, do NOT set it again in LoadMidiActionsFromFile
            autoSwitchEnabled = ReadBool("Switching", "AutoSwitch", false);
            autoSwitchInterval = int.TryParse(ReadIni("Switching", "AutoSwitchInterval", "500"), out int interval) ? interval : 250;
            autoSwitchDebounce = int.TryParse(ReadIni("Switching", "AutoSwitchDebounce", "1000"), out int debounce) ? debounce : 1000;
            fallbackPanelFile = ReadIni("Switching", "FallBack", "buttons.txt");
            LoadMidiActionsFromFile(lastButtonFile);
            // NEW MIDI DEVICES 1
            SetMidiDevicesForActiveDaw();
            UpdateStatusStrip(false);

            InitializeStatusStrip();
            statusStrip.BackColor = SystemColors.Control; // Ensure status bar uses default color
            ApplyStatusState();

            statusToolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 500,
                ShowAlways = true,
                OwnerDraw = true
            };
            statusToolTip.Draw += ToolTip_Draw;
            statusToolTip.Popup += ToolTip_Popup;
            CreateMidiButtonGrid();
            if (buttonPanel != null)
            {
                buttonPanel.PerformLayout();
                buttonPanel.Refresh();
                Application.DoEvents();
            }
            UpdateFormTitle();

            // Context menu
            mainContextMenu = new ContextMenuStrip();

            // Tray Minimize
            minimizeToTray = ReadBool("Window", "MinimizeToTray", false);

            // Always On Top for main context menu
            // aotItem = new ToolStripMenuItem("Always On Top") { Checked = TopMost, CheckOnClick = true };
            aotItem = new ToolStripMenuItem(Properties.Resources.MenuAlwaysOnTop) { Checked = TopMost, CheckOnClick = true };
            aotItem.Click += (s, e) =>
            {
                TopMost = aotItem.Checked;
                trayAotItem.Checked = aotItem.Checked; // sync tray menu
                WriteIni("Window", "AOT", TopMost ? "True" : "False");
                UpdateStatusStrip();

                // Ensure Cubase regains focus after toggling AOT
                if (cubaseHwnd != IntPtr.Zero)
                {
                    System.Threading.Thread.Sleep(100); // Optional: small delay for reliability
                    SetForegroundWindow(cubaseHwnd);
                }
            };
            mainContextMenu.Items.Add(aotItem);

            // Show Title Bar for main context menu
            // titleBarItem = new ToolStripMenuItem("Show Title Bar") { Checked = titleBarVisible, CheckOnClick = true };
            titleBarItem = new ToolStripMenuItem(Properties.Resources.MenuShowTitleBar) { Checked = titleBarVisible, CheckOnClick = true };
            titleBarItem.Click += (s, e) =>
            {
                titleBarVisible = titleBarItem.Checked;
                trayTitleBarItem.Checked = titleBarVisible; // sync tray menu to main
                ToggleTitleBar();
                // Write to the per-panel ini: Borderless = !titleBarVisible
                WriteIniToButtonFile("Window", "Borderless", (!titleBarVisible ? "True" : "False"));
                SaveButtonIniFile(lastButtonFile);
            };
            mainContextMenu.Items.Add(titleBarItem);

            // Show Status
            ToolStripMenuItem statusItem = new ToolStripMenuItem(Properties.Resources.MenuShowStatus) { Checked = statusVisible, CheckOnClick = true };
            statusItem.Click += (s, e) =>
            {
                statusVisible = statusItem.Checked;
                ApplyStatusState();
                // WriteIni("Window", "StatusVisible", statusVisible ? "True" : "False");
                SaveButtonIniFile(lastButtonFile); // Save immediately when toggled
            };
            mainContextMenu.Items.Add(statusItem);

            // Divider
            mainContextMenu.Items.Add(new ToolStripSeparator());

            // Display Section Headers
            headersItem = new ToolStripMenuItem(Properties.Resources.MenuDisplaySectionHeaders) { Checked = displayHeaders, CheckOnClick = true };
            headersItem.Click += (s, e) =>
            {
                displayHeaders = headersItem.Checked;
                CreateMidiButtonGrid();
                SaveButtonIniFile(lastButtonFile);
            };
            mainContextMenu.Items.Add(headersItem);

            // Add "Window Transparency" here
            ToolStripMenuItem transparencyItem = new ToolStripMenuItem(Properties.Resources.MenuPanelTransparency);
            transparencyItem.Click += (s, e) => ShowTransparencyDialog();
            mainContextMenu.Items.Add(transparencyItem);

            // Find the index after transparencyItem
            int insertIndex = mainContextMenu.Items.IndexOf(transparencyItem) + 1;


            // Instantiate collapsible Menu Item before using it
            collapsibleItem = new ToolStripMenuItem(
                titleBarVisible
                 ? Properties.Resources.MenuMakeCollapsibleHidesTitleBar
                 : Properties.Resources.MenuMakeCollapsible)
            {
                CheckOnClick = true,
                Enabled = true,
                Checked = collapsible && !titleBarVisible
            };


            // Replace the entire collapsible menu block with this:
            collapsibleItem.Click += (s, e) =>
            {
                if (collapsibleItem.Checked)
                {
                    // If enabling collapsible, automatically hide title bar and set borderless
                    titleBarVisible = false;
                    if (titleBarItem != null) titleBarItem.Checked = false;
                    if (trayTitleBarItem != null) trayTitleBarItem.Checked = false;
                    // Set INI for borderless
                    WriteIniToButtonFile("Window", "Borderless", "True");
                    // Hide title bar
                    ToggleTitleBar();

                    // Set collapsible in INI
                    collapsible = true;
                    WriteIniToButtonFile("Buttons", "Collapsible", "True");
                }
                else
                {
                    // If disabling collapsible, just update INI and state
                    collapsible = false;
                    WriteIniToButtonFile("Buttons", "Collapsible", "False");
                }

                // Update UI and settings
                UpdateCollapseState();
                LoadPanelSettingsFromIni(lastButtonFile);
                SyncMenuItemsWithSettings(lastButtonFile);
                UpdateFormTitle();
                UpdateStatusStrip();
            };
            // Isert Collapsible Menu Item
            mainContextMenu.Items.Insert(insertIndex, collapsibleItem);



            // Auto Switch Setup
            ToolStripMenuItem autoSwitchItem = new ToolStripMenuItem(Properties.Resources.MenuAutoSwitch)
            {
                Checked = autoSwitchEnabled,
                CheckOnClick = true
            };
            autoSwitchItem.Click += (s, e) =>
            {
                autoSwitchEnabled = autoSwitchItem.Checked;
                WriteIni("Switching", "AutoSwitch", autoSwitchEnabled ? "True" : "False");
                // Hook up logic here later
            };

            // Insert Auto Switch Menu
            mainContextMenu.Items.Insert(insertIndex + 1, autoSwitchItem);

            // Divider
            mainContextMenu.Items.Add(new ToolStripSeparator());

            // Select Button File...
            ToolStripMenuItem selectButtonPanelItem = new ToolStripMenuItem(Properties.Resources.MenuSelectButtonPanel);
            selectButtonPanelItem.Click += SelectButtonPanel_Click;
            mainContextMenu.Items.Add(selectButtonPanelItem);

            // Add "Refresh Current Panel" after "Select Button File..."
            ToolStripMenuItem refreshPanelItem = new ToolStripMenuItem(Properties.Resources.MenuRefreshCurrentPanel);
            refreshPanelItem.Click += (s, e) =>
            {
                LoadPanelSettingsFromIni(lastButtonFile);
                SyncMenuItemsWithSettings(lastButtonFile);
                LoadMidiActionsFromFile(lastButtonFile);

                if (buttonPanel != null)
                {
                    buttonPanel.Size = this.ClientSize;
                    buttonPanel.PerformLayout();
                    buttonPanel.Refresh();
                    Application.DoEvents();
                }
                CreateMidiButtonGrid();
                UpdateFormTitle();
                // UpdateStatusStrip();
                // NEW MIDI DEVICES 2
                SetMidiDevicesForActiveDaw();
                UpdateStatusStrip(false);

            };
            mainContextMenu.Items.Add(refreshPanelItem);

            // After refreshPanelItem, before the separator
            ToolStripMenuItem createNewPanelItem = new ToolStripMenuItem(Properties.Resources.MenuCreateNewButtonPanel);
            createNewPanelItem.Click += CreateNewButtonPanel_Click;
            mainContextMenu.Items.Add(createNewPanelItem);

            // Divider
            mainContextMenu.Items.Add(new ToolStripSeparator());

            // Edit Button File...
            ToolStripMenuItem editButtonFileItem = new ToolStripMenuItem(Properties.Resources.MenuEditButtonFile);
            editButtonFileItem.Click += EditButtonFile_Click;
            mainContextMenu.Items.Add(editButtonFileItem);

            // Edit Button ini File...
            ToolStripMenuItem editButtonIniFileItem = new ToolStripMenuItem(Properties.Resources.MenuEditPanelSettings);
            editButtonIniFileItem.Click += (s, e) =>
            {
                string iniPath = GetButtonIniFile(lastButtonFile);
                if (!File.Exists(iniPath))
                {
                    MessageBox.Show(
                        Properties.Resources.DialogSettingsIniFileNotFound,
                        Properties.Resources.DialogError,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                try
                {
                    using (EditorButtonsINIForm editor = new EditorButtonsINIForm(iniPath))
                    {
                        if (editor.ShowDialog(this) == DialogResult.OK)
                        {
                            // Reload settings after save
                            LoadPanelSettingsFromIni(lastButtonFile);
                            SyncMenuItemsWithSettings(lastButtonFile);
                            CreateMidiButtonGrid();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(Properties.Resources.DialogCouldNotOpenGlobalSettingsEditor, ex.Message),
                        Properties.Resources.DialogError,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            };
            mainContextMenu.Items.Add(editButtonIniFileItem);

            // Edit Settings ini File...
            ToolStripMenuItem editSettingsIniFileItem = new ToolStripMenuItem(Properties.Resources.MenuEditGlobalSettings);
            editSettingsIniFileItem.Click += (s, e) =>
            {
                string settingsIniPath = SettingsIniFile();
                if (!File.Exists(settingsIniPath))
                {
                    MessageBox.Show(
                        Properties.Resources.DialogIniFileNotFound,
                        Properties.Resources.DialogError,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                try
                {
                    using (EditorSettingsForm editor = new EditorSettingsForm())
                    {
                        if (editor.ShowDialog(this) == DialogResult.OK)
                        {
                            // Optionally reload settings here if needed
                            LoadSettingsFromIni();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Properties.Resources.DialogCouldNotOpenGlobalSettingsEditor, ex.Message), Properties.Resources.DialogError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            mainContextMenu.Items.Add(editSettingsIniFileItem);

            // Divider
            mainContextMenu.Items.Add(new ToolStripSeparator());

            // Move Panel to Centre
            ToolStripMenuItem moveToCenterItem = new ToolStripMenuItem(Properties.Resources.MenuMovePanelToCentre);
            moveToCenterItem.Click += (s, e) =>
            {
                Rectangle screen = Screen.PrimaryScreen.WorkingArea;
                this.Location = new Point(
                    screen.Left + (screen.Width - this.Width) / 2,
                    screen.Top + (screen.Height - this.Height) / 2
                );
                SaveButtonIniFile(lastButtonFile); // Save new position to ini
            };
            mainContextMenu.Items.Add(moveToCenterItem);

            // Show MIDI In From Host
            ToolStripMenuItem showMidiInFromHostItem = new ToolStripMenuItem(Properties.Resources.MenuShowMidiInFromHost);
            showMidiInFromHostItem.Click += (s, e) =>
            {
                midiFromHost.UpdateDialogLocation(this.Location);
                midiFromHost.ShowTestDialog(this); // Pass 'this' as owner
            };
            mainContextMenu.Items.Add(showMidiInFromHostItem);

            // Divider
            mainContextMenu.Items.Add(new ToolStripSeparator());

            // View README
            ToolStripMenuItem viewReadmeItem = new ToolStripMenuItem(Properties.Resources.MenuViewReadme);
            viewReadmeItem.Click += ViewReadme_Click;
            mainContextMenu.Items.Add(viewReadmeItem);

            // View PDF Manual
            ToolStripMenuItem viewPdfManualItem = new ToolStripMenuItem(Properties.Resources.MenuViewPdfManual);
            viewPdfManualItem.Click += ViewPdfManual_Click;
            mainContextMenu.Items.Add(viewPdfManualItem);

            // About
            ToolStripMenuItem aboutItem = new ToolStripMenuItem(string.Format(Properties.Resources.MenuAbout, Form1.AppVersion));
            aboutItem.Click += AboutItem_Click;
            mainContextMenu.Items.Add(aboutItem);

            // Exit
            ToolStripMenuItem exitItem = new ToolStripMenuItem(Properties.Resources.MenuExit);
            exitItem.Click += (s, e) => this.Close();
            mainContextMenu.Items.Add(exitItem);

            this.MouseUp += Form1_MouseUpForContextMenu;

            // Now sync the checked state
            trayAotItem.Checked = TopMost;
            aotItem.Checked = TopMost;
            // --- ADD THIS LINE HERE ---
            LoadPanelSettingsFromIni(lastButtonFile);
            SyncMenuItemsWithSettings(lastButtonFile);

            // Find the MIDI input device index by name
            int midiInDeviceId = 0; // fallback to 0 if not found
            if (!string.IsNullOrEmpty(midiInNameLabel))
            {
                for (int i = 0; i < MidiIn.NumberOfDevices; i++)
                {
                    MidiInCapabilities caps = MidiIn.DeviceInfo(i);
                    if (caps.ProductName.Equals(midiInNameLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        midiInDeviceId = i;
                        break;
                    }
                }
            }


            Point dialogLocation = this.Location;
            midiFromHost = new Midifromhost(midiInDeviceId, midiInNameLabel, dialogLocation);
            // Add this line right after creating midiFromHost:
            midiFromHost.MidiMessageReceived += MidiFromHost_MidiMessageReceived;

            autoSwitchTimer = new Timer { Interval = autoSwitchInterval }; // Check every 0.5s
            autoSwitchTimer.Tick += AutoSwitchTimer_Tick;
            autoSwitchTimer.Start();
        }

        // Add this helper dialog class inside Form1 (or as a nested class)
        // Replace your SectionHeaderEditDialog class with this improved version
        private class SectionHeaderEditDialog : Form
        {
            public string SectionName { get; private set; }
            public string SectionColor { get; private set; }

            public SectionHeaderEditDialog(string sectionId, string currentName, string currentColor)
            {
                Text = string.Format(Properties.Resources.DialogSectionHeaderId, sectionId);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                ClientSize = new Size(190, 94); // Slightly smaller
                MinimizeBox = false;
                MaximizeBox = false;
                ShowInTaskbar = false;

                int margin = 10;
                int labelWidth = 48;
                int textBoxWidth = ClientSize.Width - labelWidth - margin * 2 - 4;

                Label nameLabel = new Label { Text = Properties.Resources.DialogName, Left = margin, Top = 10, Width = labelWidth };
                TextBox nameBox = new TextBox { Left = margin + labelWidth, Top = 8, Width = textBoxWidth, Text = currentName };

                Label colorLabel = new Label { Text = Properties.Resources.DialogColour, Left = margin, Top = 34, Width = labelWidth };
                TextBox colorBox = new TextBox { Left = margin + labelWidth, Top = 32, Width = textBoxWidth, Text = currentColor };

                int buttonWidth = 70;
                int buttonHeight = 24;
                int gapBeforeButtons = 10;
                int buttonY = colorBox.Bottom + gapBeforeButtons;

                Button ok = new Button { Text = Properties.Resources.DialogOK, DialogResult = DialogResult.None, Left = margin, Top = buttonY, Width = buttonWidth, Height = buttonHeight };
                Button cancel = new Button { Text = Properties.Resources.DialogCancelButton, DialogResult = DialogResult.Cancel, Left = ClientSize.Width - buttonWidth - margin, Top = buttonY, Width = buttonWidth, Height = buttonHeight };

                AcceptButton = ok;
                CancelButton = cancel;

                Controls.AddRange(new Control[] { nameLabel, nameBox, colorLabel, colorBox, ok, cancel });

                ok.Click += (s, e) =>
                {
                    string name = nameBox.Text.Trim();
                    string color = colorBox.Text.Trim();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        MessageBox.Show(this, Properties.Resources.DialogSectionNameCannotBeEmpty, Properties.Resources.DialogValidation, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        nameBox.Focus();
                        return;
                    }

                    // Accept #RRGGBB or color name
                    string validatedColor = null;
                    if (color.StartsWith("#"))
                    {
                        string hex = color.Substring(1);
                        if (hex.Length > 6) hex = hex.Substring(0, 6);
                        if (hex.Length == 6 && int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out _))
                        {
                            validatedColor = "#" + hex;
                        }
                    }
                    else
                    {
                        // Try to parse as a known color name
                        try
                        {
                            Color c = ColorTranslator.FromHtml(color);
                            validatedColor = ColorTranslator.ToHtml(c);
                        }
                        catch
                        {
                            validatedColor = null;
                        }
                    }

                    if (validatedColor == null)
                    {
                        MessageBox.Show(this, Properties.Resources.DialogPleaseEnterValidColor, Properties.Resources.DialogValidation, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        colorBox.Focus();
                        return;
                    }

                    SectionName = name;
                    SectionColor = validatedColor;
                    DialogResult = DialogResult.OK;
                    Close();
                };
            }
        }

        // Event handler
        private void MidiFromHost_MidiMessageReceived(object sender, MidiMessageEventArgs e)
        {
            foreach (Button btn in buttonPanel.Controls.OfType<Button>())
            {
                MidiAction action = btn.Tag as MidiAction;
                if (action != null &&
                    action.Channel == e.Channel &&
                    action.IsNote == e.IsNote &&
                    action.Value == e.Value) // Match on CC/Note number
                {
                    if (e.Data == 127)
                    {
                        // MIDI IN active
                        midiInActive[btn] = true;
                        if (btn.InvokeRequired)
                            btn.Invoke(new Action(() =>
                            {
                                btn.BackColor = midiInColor;
                                btn.ForeColor = GetContrastingTextColor(midiInColor);
                            }));
                        else
                        {
                            btn.BackColor = midiInColor;
                            btn.ForeColor = GetContrastingTextColor(midiInColor);
                        }
                    }
                    else if (e.Data == 0)
                    {
                        // MIDI IN inactive
                        midiInActive[btn] = false;
                        Color defaultColor = buttonDefaultColors[btn];
                        if (btn.InvokeRequired)
                            btn.Invoke(new Action(() =>
                            {
                                btn.BackColor = defaultColor;
                                btn.ForeColor = GetContrastingTextColor(defaultColor);
                            }));
                        else
                        {
                            btn.BackColor = defaultColor;
                            btn.ForeColor = GetContrastingTextColor(defaultColor);
                        }
                    }
                }
            }
        }

        // Helper to write to the button ini file
        private void WriteIniToButtonFile(string section, string key, string value)
        {
            string iniPath = GetButtonIniFile(lastButtonFile);
            List<string> lines = File.Exists(iniPath) ? File.ReadAllLines(iniPath).ToList() : new List<string>();
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
                    l.Split(new char[] { '=' }, 2)[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase));
                if (keyIndex >= 0)
                    lines[keyIndex] = $"{key}={value}";
                else
                    lines.Insert(insertIndex, $"{key}={value}");
            }
            File.WriteAllLines(iniPath, lines);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveSettingsToIni();
            SaveButtonIniFile(lastButtonFile); // saves window size to current file's ini

            // Remove tray icon from system tray
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            base.OnFormClosing(e);
        }

        // --- Centralized DAW detection: always updates cubaseHwnd and activeDawName ---
        // --- Centralized DAW detection: always updates cubaseHwnd and activeDawName ---
        private void UpdateActiveDawFromForeground()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            string procName = "";

            if (hwnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(hwnd, out pid);
                try
                {
                    Process proc = Process.GetProcessById((int)pid);
                    procName = proc.ProcessName;
                }
                catch { }
            }

            // 1. If a monitored DAW is focused, set it as active
            string matchedProcess = monitoredProcesses
                .FirstOrDefault(k => !string.IsNullOrEmpty(procName) && procName.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(matchedProcess))
            {
                activeDawName = matchedProcess;
                cubaseHwnd = IntPtr.Zero;
                foreach (Process proc in Process.GetProcesses())
                {
                    if (proc.ProcessName.IndexOf(matchedProcess, StringComparison.OrdinalIgnoreCase) >= 0 && proc.MainWindowHandle != IntPtr.Zero)
                    {
                        cubaseHwnd = proc.MainWindowHandle;
                        break;
                    }
                }
                return;
            }

            // 2. If the previously active DAW is still running, keep it as active (FIXED)
            if (!string.IsNullOrEmpty(activeDawName) && activeDawName != "None")
            {
                if (IsProcessRunning(activeDawName))
                {
                    // Update cubaseHwnd if needed
                    if (cubaseHwnd == IntPtr.Zero || !IsProcessRunningByHandle(cubaseHwnd))
                    {
                        Process runningProc = Process.GetProcessesByName(activeDawName)
                            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
                        if (runningProc != null)
                        {
                            cubaseHwnd = runningProc.MainWindowHandle;
                        }
                    }
                    return; // Keep the current active DAW
                }
                else
                {
                    // The previously active DAW is no longer running, clear it
                    activeDawName = "None";
                    cubaseHwnd = IntPtr.Zero;
                }
            }

            // 3. If any monitored DAW is running, set the first running one as active
            foreach (var k in monitoredProcesses)
            {
                if (IsProcessRunning(k))
                {
                    activeDawName = k;
                    cubaseHwnd = Process.GetProcessesByName(k).FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero)?.MainWindowHandle ?? IntPtr.Zero;
                    return;
                }
            }

            // 4. No DAW found
            activeDawName = "None";
            cubaseHwnd = IntPtr.Zero;
        }

        // Helper to check if a process is running by handle
        private bool IsProcessRunningByHandle(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            if (pid == 0) return false;
            try
            {
                Process proc = Process.GetProcessById((int)pid);
                return !proc.HasExited;
            }
            catch
            {
                return false;
            }
        }

        private string GetFirstRunningDaw()
        {
            foreach (var processName in monitoredProcesses)
            {
                // Check all running processes for a partial match
                Process[] allProcs = Process.GetProcesses();
                foreach (Process proc in allProcs)
                {
                    if (proc.ProcessName.IndexOf(processName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return processName; // Return the monitored name, not the full process name
                    }
                }
            }
            return "None";
        }

        // Helper to check if a process is running
        private bool IsProcessRunning(string processName)
        {
            // Check all running processes for a partial match
            Process[] allProcs = Process.GetProcesses();
            foreach (Process proc in allProcs)
            {
                if (proc.ProcessName.IndexOf(processName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void AutoSwitchTimer_Tick(object sender, EventArgs e)
        {
            if (autoSwitchEnabled)
            {
                // When Auto Switch is ON: update DAW detection AND do panel switching
                UpdateActiveDawFromForeground();

                if (autoSwitchPanel.Count > 0)
                {
                    // Panel switching logic
                    if (activeDawName != "None" && autoSwitchPanel.ContainsKey(activeDawName))
                    {
                        string panelFile = autoSwitchPanel[activeDawName];
                        string panelPath = Path.Combine(ButtonsFolder(), panelFile);
                        if (File.Exists(panelPath) && !string.Equals(lastButtonFile, panelPath, StringComparison.OrdinalIgnoreCase))
                        {
                            SaveButtonIniFile(lastButtonFile);
                            lastButtonFile = panelPath;
                            LoadPanelSettingsFromIni(lastButtonFile);
                            SyncMenuItemsWithSettings(lastButtonFile);
                            headersItem.Checked = displayHeaders;
                            LoadMidiActionsFromFile(lastButtonFile);


                            if (buttonPanel != null)
                            {
                                buttonPanel.Size = this.ClientSize;
                                buttonPanel.PerformLayout();
                                buttonPanel.Refresh();
                                Application.DoEvents();
                            }
                            CreateMidiButtonGrid();
                            UpdateFormTitle();
                            // NEW MIDI DEVICES 3
                            SetMidiDevicesForActiveDaw();
                            UpdateStatusStrip(false);
                        }
                    }
                    else if (!string.IsNullOrEmpty(fallbackPanelFile))
                    {
                        // Fallback logic if no DAW is active
                        string fallbackPath = Path.Combine(ButtonsFolder(), fallbackPanelFile);
                        if (File.Exists(fallbackPath) && !string.Equals(lastButtonFile, fallbackPath, StringComparison.OrdinalIgnoreCase))
                        {
                            SaveButtonIniFile(lastButtonFile);
                            lastButtonFile = fallbackPath;
                            LoadPanelSettingsFromIni(lastButtonFile);
                            SyncMenuItemsWithSettings(lastButtonFile);
                            headersItem.Checked = displayHeaders;
                            LoadMidiActionsFromFile(lastButtonFile);
                            // NEW MIDI DEVICES 4
                            // TESTING
                            SetMidiDevicesForActiveDaw();
                            UpdateStatusStrip(false);
                            if (buttonPanel != null)
                            {
                                buttonPanel.Size = this.ClientSize;
                                buttonPanel.PerformLayout();
                                buttonPanel.Refresh();
                                Application.DoEvents();
                            }
                            CreateMidiButtonGrid();
                            UpdateFormTitle();
                            UpdateStatusStrip(false);
                        }
                    }
                }

                // Update status display
                UpdateStatusStrip(false);
            }
            else
            {

                string previousDaw = activeDawName;
                // When Auto Switch is OFF: Check if a different DAW is now focused
                IntPtr hwnd = GetForegroundWindow();
                uint pid;
                string procName = "";

                if (hwnd != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(hwnd, out pid);
                    try
                    {
                        Process proc = Process.GetProcessById((int)pid);
                        procName = proc.ProcessName;
                    }
                    catch { }
                }

                // Check if a different monitored DAW is now focused
                string focusedDaw = monitoredProcesses
                    .FirstOrDefault(k => !string.IsNullOrEmpty(procName) && procName.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

                if (!string.IsNullOrEmpty(focusedDaw))
                {
                    // A monitored DAW is focused - switch to it
                    activeDawName = focusedDaw;
                    cubaseHwnd = hwnd;
                }
                else if (!string.IsNullOrEmpty(activeDawName) && activeDawName != "None")
                {
                    // Current DAW is not focused - check if it's still running
                    if (!IsProcessRunning(activeDawName))
                    {
                        // The active DAW was closed, check for other running DAWs
                        string firstRunning = GetFirstRunningDaw();
                        if (firstRunning != "None")
                        {
                            activeDawName = firstRunning;
                            cubaseHwnd = Process.GetProcessesByName(firstRunning)
                                .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero)?.MainWindowHandle ?? IntPtr.Zero;
                        }
                        else
                        {
                            activeDawName = "None";
                            cubaseHwnd = IntPtr.Zero;
                        }
                    }
                    else
                    {
                        // Active DAW is still running, update handle if needed
                        if (cubaseHwnd == IntPtr.Zero || !IsProcessRunningByHandle(cubaseHwnd))
                        {
                            Process runningProc = Process.GetProcessesByName(activeDawName)
                                .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
                            if (runningProc != null)
                            {
                                cubaseHwnd = runningProc.MainWindowHandle;
                            }
                        }
                    }
                }
                else
                {
                    // No active DAW yet - do initial detection
                    string firstRunning = GetFirstRunningDaw();
                    if (firstRunning != "None")
                    {
                        activeDawName = firstRunning;
                        cubaseHwnd = Process.GetProcessesByName(firstRunning)
                            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero)?.MainWindowHandle ?? IntPtr.Zero;
                    }
                }

                // NEW: Update MIDI devices if active DAW changed
                if (previousDaw != activeDawName)
                {
                    SetMidiDevicesForActiveDaw();
                }
                // Update status display only (no panel switching)
                UpdateStatusStrip(false);
            }
        }


        private void Form1_MouseUpForContextMenu(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                mainContextMenu.Show(this, e.Location);
        }

        private void ToggleAlwaysOnTop(ToolStripMenuItem menuItem)
        {
            this.TopMost = !this.TopMost;
            menuItem.Checked = this.TopMost;
            WriteIni("Window", "AOT", this.TopMost ? "True" : "False");
            UpdateStatusStrip();
            // Save AOT state immediately
            SaveButtonIniFile(lastButtonFile);

        }

        private void Form1_MouseDownForDrag(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            // If the sender is buttonPanel, only drag if not clicking on a child control (like a button/label)
            if (sender == buttonPanel)
            {
                Control ctrl = buttonPanel.GetChildAtPoint(e.Location);
                if (ctrl != null && ctrl != buttonPanel)
                    return;
            }

            // If the sender is the form, only drag if not clicking on a child control (like the status bar)
            if (sender == this)
            {
                Control ctrl = this.GetChildAtPoint(e.Location);
                if (ctrl != null && ctrl != this && ctrl != buttonPanel)
                    return;
            }

            // Allow drag from anywhere else
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        private void ForwardKeyToCubase(Keys key, bool keyDown)
        {
            if (cubaseHwnd == IntPtr.Zero && !autoSwitchEnabled)
                UpdateActiveDawFromForeground();

            if (!(activeDawName.Equals("Cubase", StringComparison.OrdinalIgnoreCase) ||
                  activeDawName.Equals("Nuendo", StringComparison.OrdinalIgnoreCase)))
            {
                SendInputKey(key, keyDown);
            }
            else if (cubaseHwnd != IntPtr.Zero)
            {
                uint msg = keyDown ? WM_KEYDOWN : WM_KEYUP;
                PostMessage(cubaseHwnd, msg, (IntPtr)key, IntPtr.Zero);
            }
        }

        // --- INI Helpers ---
        private string ReadIni(string section, string key, string defaultVal = "")
        {
            string path = SettingsIniFile();
            if (!File.Exists(path)) return defaultVal;

            string[] iniLines = File.ReadAllLines(path);
            string currentSection = "";
            foreach (string line in iniLines)
            {
                string trimmed = line.Split(';')[0].Trim();
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
            string path = SettingsIniFile();
            List<string> lines = File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string>();
            int secIndex = lines.FindIndex(l => l.Trim().Equals($"[{section}]", StringComparison.OrdinalIgnoreCase));

            if (secIndex == -1)
            {
                lines.Add($"[{section}]");
                lines.Add($"{key}={value}");
            }
            else
            {
                int insertIndex = secIndex + 1;
                while (insertIndex < lines.Count && !lines[insertIndex].StartsWith("["))
                    insertIndex++;

                int keyIndex = lines.FindIndex(secIndex + 1, insertIndex - (secIndex + 1), l =>
                    l.Split(new char[] { '=' }, 2)[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase));

                if (keyIndex >= 0)
                    lines[keyIndex] = $"{key}={value}";
                else
                    lines.Insert(insertIndex, $"{key}={value}");
            }

            try
            {
                File.WriteAllLines(path, lines);
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.DialogSettingsSaveError, ex.Message),
                    Properties.Resources.DialogError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
);
            }
        }

        private Point ReadPoint(string section, string key, Point defaultVal)
        {
            string s = ReadIni(section, key);
            if (string.IsNullOrEmpty(s)) return defaultVal;

            string[] parts = s.Split(',');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int x) &&
                int.TryParse(parts[1], out int y))
            {
                return new Point(x, y);
            }
            return defaultVal;
        }

        private void WritePoint(string section, string key, Point value)
        {
            WriteIni(section, key, $"{value.X},{value.Y}");
        }

        private bool ReadBool(string section, string key, bool defaultVal = false)
        {
            string s = ReadIni(section, key, defaultVal ? "True" : "False");
            return s.Equals("True", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadSettingsFromIni()
        {
            midiOutNameLabel = ReadIni("MIDI", "MidiOut");
            midiInNameLabel = ReadIni("MIDI", "MidiIn");

            LoadProcessListFromIni();

            this.TopMost = ReadBool("Window", "AOT", false);

            int transparencyPercent = 0;
            int.TryParse(ReadIni("Window", "Transparency", "0"), out transparencyPercent);
            transparencyPercent = Math.Max(0, Math.Min(90, transparencyPercent));
            this.Opacity = (100 - transparencyPercent) / 100.0;

            ApplyStatusState();

            // bool maximized = ReadBool("Window", "Maximized", false);
            // this.WindowState = maximized ? FormWindowState.Maximized : FormWindowState.Normal;

            string midiInColorStr = ReadIni("MIDI", "MIDIInColour", "#00ffbf");
            midiInColor = ColorHelper.ParseOrDefault(midiInColorStr, Color.LimeGreen);

            //
            string buttonGlowColorDownStr = ReadIni("MIDI", "ButtonGlowColourDown", "#ff0000");
            buttonGlowColorDown = ColorHelper.ParseOrDefault(buttonGlowColorDownStr, Color.Red);

            string buttonGlowColorUpStr = ReadIni("MIDI", "ButtonGlowColourUp", "#0099ff");
            buttonGlowColorUp = ColorHelper.ParseOrDefault(buttonGlowColorUpStr, Color.Blue);

            // --- Tooltip colors ---
            string tooltipBackStr = ReadIni("MIDI", "TooltipBack", "");
            if (!string.IsNullOrWhiteSpace(tooltipBackStr))
            {
                tooltipBackColor = ColorHelper.ParseOrDefault(tooltipBackStr, Color.Yellow);
            }

            string tooltipTextStr = ReadIni("MIDI", "TooltipText", "");
            if (!string.IsNullOrWhiteSpace(tooltipTextStr))
            {
                tooltipForeColor = ColorHelper.ParseOrDefault(tooltipTextStr, Color.Black);
            }


        }

        private List<string> ReadSectionLines(string section)
        {
            string path = SettingsIniFile();
            List<string> result = new List<string>();
            if (!File.Exists(path)) return result;

            string[] lines = File.ReadAllLines(path);
            bool inSection = false;
            foreach (string raw in lines)
            {
                string line = raw.Split(';')[0].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    inSection = string.Equals(line.Substring(1, line.Length - 2), section, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (!inSection) continue;

                string token = line;
                int eq = line.IndexOf('=');
                if (eq >= 0)
                {
                    string rhs = line.Substring(eq + 1).Trim();
                    string lhs = line.Substring(0, eq).Trim();
                    token = string.IsNullOrEmpty(rhs) ? lhs : rhs;
                }

                if (!string.IsNullOrEmpty(token))
                    result.Add(token);
            }
            return result;
        }

        private void LoadProcessListFromIni()
        {
            monitoredProcesses.Clear();
            autoSwitchPanel.Clear();
            processMidiConfigs.Clear();

            string path = SettingsIniFile();
            if (!File.Exists(path)) return;

            string[] lines = File.ReadAllLines(path);
            bool inProcessesSection = false;

            string currentProcess = null;
            string currentPanel = null;
            string currentMidiOut = "";
            string currentMidiIn = "";

            foreach (string rawLine in lines)
            {
                // Skip comments and empty lines
                string line = rawLine.Split(';')[0].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // Check for section headers
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    // If we have a complete process definition, add it before moving to next section
                    if (inProcessesSection && !string.IsNullOrEmpty(currentProcess) && !string.IsNullOrEmpty(currentPanel))
                    {
                        AddProcessConfig(currentProcess, currentPanel, currentMidiOut, currentMidiIn);
                    }

                    inProcessesSection = line.Equals("[Processes]", StringComparison.OrdinalIgnoreCase);

                    // Reset values when entering or leaving the section
                    currentProcess = null;
                    currentPanel = null;
                    currentMidiOut = "";
                    currentMidiIn = "";
                    continue;
                }

                if (!inProcessesSection) continue;

                // Process key=value pairs in the [Processes] section
                if (line.IndexOf('=') > 0)
                {
                    string[] keyValue = line.Split(new[] { '=' }, 2);
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();

                    switch (key.ToLower())
                    {
                        case "process":
                            // If we have a complete process definition, add it before starting a new one
                            if (!string.IsNullOrEmpty(currentProcess) && !string.IsNullOrEmpty(currentPanel))
                            {
                                AddProcessConfig(currentProcess, currentPanel, currentMidiOut, currentMidiIn);
                                // Reset MIDI values for new process
                                currentMidiOut = "";
                                currentMidiIn = "";
                            }
                            currentProcess = value;
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
            }

            // Add the final process if we have one
            if (inProcessesSection && !string.IsNullOrEmpty(currentProcess) && !string.IsNullOrEmpty(currentPanel))
            {
                AddProcessConfig(currentProcess, currentPanel, currentMidiOut, currentMidiIn);
            }
        }

        // Helper method to add a process configuration
        private void AddProcessConfig(string processName, string panelFile, string midiOut, string midiIn)
        {
            monitoredProcesses.Add(processName);
            autoSwitchPanel[processName] = panelFile;
            processMidiConfigs[processName] = new ProcessMidiConfig
            {
                ProcessName = processName,
                PanelFile = panelFile,
                MidiOutName = midiOut,
                MidiInName = midiIn
            };
        }

        private ProcessMidiConfig GetActiveProcessMidiConfig()
        {
            if (!string.IsNullOrEmpty(activeDawName))
            {
                // Check for exact match first
                if (processMidiConfigs.ContainsKey(activeDawName))
                    return processMidiConfigs[activeDawName];

                // If no exact match, try case-insensitive match
                foreach (var key in processMidiConfigs.Keys)
                {
                    if (string.Equals(key, activeDawName, StringComparison.OrdinalIgnoreCase))
                        return processMidiConfigs[key];
                }

                // Third attempt - try partial match (process might contain the DAW name)
                foreach (var key in processMidiConfigs.Keys)
                {
                    if (activeDawName.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        key.IndexOf(activeDawName, StringComparison.OrdinalIgnoreCase) >= 0)
                        return processMidiConfigs[key];
                }
            }

            // Fallback: use first config if available
            return processMidiConfigs.Values.FirstOrDefault();
        }

        private void SelectButtonPanel_Click(object sender, EventArgs e)
        {
            bool wasTopMost = this.TopMost;
            this.TopMost = false;

            List<string> files = Directory.GetFiles(ButtonsFolder(), "buttons*.txt")
                .OrderBy(f => f)
                .ToList();

            if (files.Count == 0)
            {
                MessageBox.Show(
                    Properties.Resources.DialogNoButtonPanelFilesFound,
                    Properties.Resources.DialogChooseButtonPanel,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            Form dlg = new Form
            {
                Text = Properties.Resources.DialogChooseButtonPanel,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.Manual,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new Size(200, 100)
            };

            if (buttonPanel != null)
            {
                Point panelScreen = buttonPanel.PointToScreen(Point.Empty);
                int dlgX = panelScreen.X + (buttonPanel.Width - dlg.Width) / 2;
                int dlgY = panelScreen.Y + (buttonPanel.Height - dlg.Height) / 2 - 200;
                dlgY = Math.Max(dlgY, 0);
                dlg.Location = new Point(Math.Max(dlgX, 0), Math.Max(dlgY, 0));
            }
            else
            {
                Point formScreen = this.PointToScreen(Point.Empty);
                int dlgX = formScreen.X + (this.Width - dlg.Width) / 2;
                int dlgY = formScreen.Y + (this.Height - dlg.Height) / 2;
                dlg.Location = new Point(Math.Max(dlgX, 0), Math.Max(dlgY, 0));
            }

            int margin = 12;
            int formWidth = 200;

            Label label = new Label
            {
                Text = Properties.Resources.DialogSelectButtonPanel,
                AutoSize = false,
                Left = margin,
                Top = 10,
                Width = formWidth - margin * 2,
                Height = 13,
                TextAlign = ContentAlignment.MiddleLeft
            };

            ComboBox combo = new ComboBox
            {
                Left = margin,
                Top = 30,
                Width = formWidth - margin * 2,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            string activePanelName = Path.GetFileNameWithoutExtension(lastButtonFile);
            List<string> panelNames = files
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(name => !string.Equals(name, activePanelName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name)
                .ToList();
            panelNames.Insert(0, activePanelName);

            combo.Items.Clear();
            combo.Items.AddRange(panelNames.ToArray());
            combo.SelectedIndex = 0;

            Button cancel = new Button
            {
                Text = Properties.Resources.DialogCancelButton,
                DialogResult = DialogResult.Cancel,
                Left = margin,
                Top = 62,
                Width = formWidth - margin * 2,
                Height = 25
            };

            dlg.Controls.Add(label);
            dlg.Controls.Add(combo);
            dlg.Controls.Add(cancel);
            dlg.CancelButton = cancel;

            // Handle selection change
            combo.SelectedIndexChanged += (s, ev) =>
            {
                if (combo.SelectedIndex >= 0)
                {
                    // Only switch if a new panel is selected
                    string selectedPanel = combo.SelectedItem.ToString();
                    if (!string.Equals(selectedPanel, activePanelName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Store the selected panel name
                        string newPanelPath = Path.Combine(ButtonsFolder(), selectedPanel + ".txt");

                        // Close the dialog FIRST before any time-consuming operations
                        dlg.Close();

                        // Now perform all the panel loading and UI updates
                        SaveButtonIniFile(lastButtonFile);
                        lastButtonFile = newPanelPath;
                        LoadPanelSettingsFromIni(lastButtonFile);
                        SyncMenuItemsWithSettings(lastButtonFile);
                        headersItem.Checked = displayHeaders;
                        LoadMidiActionsFromFile(lastButtonFile);
                        SetMidiDevicesForActiveDaw();
                        UpdateStatusStrip(false);

                        UpdateFormTitle();
                        if (buttonPanel != null)
                        {
                            buttonPanel.Size = this.ClientSize;
                            buttonPanel.PerformLayout();
                            buttonPanel.Refresh();
                            Application.DoEvents();
                        }
                        CreateMidiButtonGrid();

                        SetMidiDevicesForActiveDaw();
                        UpdateStatusStrip(false);
                    }
                    else
                    {
                        // Close the dialog if no change
                        dlg.Close();
                    }
                }
            };

            dlg.ShowDialog(this);
            this.TopMost = wasTopMost;
        }

        private void ToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            // Add margin (2 spaces) at the start of every line
            string[] lines = e.ToolTipText.Replace("\\n", "\n").Split('\n');
            int topMargin = 6;
            int bottomMargin = 8;

            // Calculate total height for all lines
            int totalHeight = topMargin + bottomMargin;
            foreach (string line in lines)
            {
                string paddedLine = "  " + line;
                Size lineSize = TextRenderer.MeasureText(paddedLine, tooltipFont);
                totalHeight += lineSize.Height;
            }

            // Fill background
            using (SolidBrush b = new SolidBrush(tooltipBackColor))
                e.Graphics.FillRectangle(b, e.Bounds);

            // Draw border
            using (Pen borderPen = new Pen(Color.Black))
                e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);

            // Draw text with top margin
            using (SolidBrush b = new SolidBrush(tooltipForeColor))
            {
                int y = e.Bounds.Top + topMargin;
                foreach (string line in lines)
                {
                    string paddedLine = "  " + line;
                    Size lineSize = TextRenderer.MeasureText(paddedLine, tooltipFont);
                    e.Graphics.DrawString(paddedLine, tooltipFont, b, e.Bounds.Left, y);
                    y += lineSize.Height;
                }
            }
            // No need to call e.DrawBorder() since we draw our own border
        }

        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            ToolTip tip = sender as ToolTip;
            if (tip != null && tip.GetToolTip(e.AssociatedControl) != null)
            {
                // Add margin (2 spaces) at the start of every line
                string[] lines = tip.GetToolTip(e.AssociatedControl).Replace("\\n", "\n").Split('\n');
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
            }
        }

        private void UpdateCollapseState()
        {
            // Ensure the timer is always created and event attached only once
            if (collapseCheckTimer == null)
            {
                collapseCheckTimer = new Timer
                {
                    Interval = 300
                };
                collapseCheckTimer.Tick += (s2, e2) =>
                {
                    if (this.IsDisposed || collapseCheckTimer == null || buttonPanel == null || collapseButton == null)
                        return;

                    if (mainContextMenu != null && mainContextMenu.Visible)
                        return;

                    if (
                        Application.OpenForms.OfType<EditMidiActionForm>().Any(f => f.Visible) ||
                        Application.OpenForms.OfType<SectionHeaderEditDialog>().Any(f => f.Visible) ||
                        Application.OpenForms.Cast<Form>().Any(f =>
                            (f.Text == Properties.Resources.DialogWindowTransparency ||
                            f.Text == Properties.Resources.DialogChooseButtonPanel ||
                            f.Text == Properties.Resources.DialogCreateNewButtonPanel) && f.Visible)
                    )
                        return;

                    Rectangle panelRect = buttonPanel.RectangleToScreen(buttonPanel.ClientRectangle);
                    Rectangle buttonRect = collapseButton.RectangleToScreen(collapseButton.ClientRectangle);
                    Point mouse = Cursor.Position;

                    if (!panelRect.Contains(mouse) && !buttonRect.Contains(mouse))
                    {
                        collapseCheckTimer.Stop();
                        UpdateCollapseState();
                    }
                };
            }

            // Mouse leave collapse logic
            void CollapsePanel_MouseEnter(object sender, EventArgs e)
            {
                if (collapseCheckTimer != null)
                    collapseCheckTimer.Stop();
            }
            void CollapsePanel_MouseLeave(object sender, EventArgs e)
            {
                if (collapseCheckTimer != null)
                {
                    collapseCheckTimer.Interval = 300;
                    collapseCheckTimer.Start();
                }
            }
            void CollapsePanelButton_MouseEnter(object sender, EventArgs e)
            {
                if (collapseCheckTimer != null)
                    collapseCheckTimer.Stop();
            }
            void CollapsePanelButton_MouseLeave(object sender, EventArgs e)
            {
                if (collapseCheckTimer != null)
                {
                    collapseCheckTimer.Interval = 300;
                    collapseCheckTimer.Start();
                }
            }

            if (collapsible && !titleBarVisible)
            {
                if (collapseButton == null)
                {
                    collapseButton = new Button
                    {
                        Text = "▼",
                        Anchor = AnchorStyles.Top | AnchorStyles.Left,
                        FlatStyle = FlatStyle.Flat,
                    };

                    this.Controls.Add(collapseButton);
                    this.Controls.SetChildIndex(collapseButton, 0);
                    collapseButton.CreateControl();

                    collapseButton.MouseEnter += (s, e) =>
                    {
                        collapseButton.Visible = false;
                        Size expandedSize = previousWindowSize.HasValue &&
                                            previousWindowSize.Value.Width > 10 &&
                                            previousWindowSize.Value.Height > 10 ?
                                            previousWindowSize.Value : new Size(10, 10);

                        this.Size = expandedSize;
                        if (previousWindowLocation.HasValue)
                            this.Location = previousWindowLocation.Value;

                        if (buttonPanel != null)
                        {
                            buttonPanel.Visible = true;
                            buttonPanel.Size = this.ClientSize;
                        }

                        if (statusStrip != null)
                            statusStrip.Visible = statusVisible;

                        this.BackColor = expandedPanelBackgroundColor;

                        this.PerformLayout();
                        this.Invalidate(true);
                        this.Update();
                        Application.DoEvents();

                        foreach (Control control in this.Controls)
                        {
                            control.Invalidate();
                            control.Update();
                        }
                    };
                }

                // Always use [UserCollapse] INI settings for the collapse button
                string iniPath = GetButtonIniFile(lastButtonFile);
                int userWidth = int.TryParse(ReadIniFromFile(iniPath, "UserCollapse", "Button Width", "110"), out int uw) ? uw : 110;
                int userHeight = int.TryParse(ReadIniFromFile(iniPath, "UserCollapse", "Button Height", "30"), out int uh) ? uh : 30;
                string userButtonColorStr = ReadIniFromFile(iniPath, "UserCollapse", "Button Colour", "#FF0000");
                string userBackgroundColorStr = ReadIniFromFile(iniPath, "UserCollapse", "Background Colour", "#000000");
                string userBorderColorStr = ReadIniFromFile(iniPath, "UserCollapse", "Button Border", "#000000");
                string userFontName = ReadIniFromFile(iniPath, "UserCollapse", "Button Font Name", "Segoe UI");
                float userFontSize = float.TryParse(ReadIniFromFile(iniPath, "UserCollapse", "Button Font Size", "10"), out float ufs) ? ufs : 10f;
                int userTopMargin = int.TryParse(ReadIniFromFile(iniPath, "UserCollapse", "Top Margin", "4"), out int utm) ? utm : 4;
                int userLeftMargin = int.TryParse(ReadIniFromFile(iniPath, "UserCollapse", "Left Margin", "4"), out int ulm) ? ulm : 4;

                collapseButton.Width = userWidth;
                collapseButton.Height = userHeight;
                collapseButton.Font = new Font(userFontName, userFontSize, FontStyle.Regular);

                Color userButtonColor = ColorHelper.ParseOrDefault(userButtonColorStr, Color.Red);
                collapseButton.BackColor = userButtonColor;
                collapseButton.ForeColor = GetContrastingTextColor(userButtonColor);

                Color userBorderColor = ColorHelper.ParseOrDefault(userBorderColorStr, Color.Black);
                collapseButton.FlatAppearance.BorderSize = 1;
                collapseButton.FlatAppearance.BorderColor = userBorderColor;

                collapseButton.Left = userLeftMargin;
                collapseButton.Top = userTopMargin;
                this.BackColor = ColorHelper.ParseOrDefault(userBackgroundColorStr, Color.Black);

                collapseButton.Invalidate();
                collapseButton.Update();

                if (buttonPanel != null)
                {
                    buttonPanel.MouseEnter -= CollapsePanel_MouseEnter;
                    buttonPanel.MouseLeave -= CollapsePanel_MouseLeave;
                    buttonPanel.MouseEnter += CollapsePanel_MouseEnter;
                    buttonPanel.MouseLeave += CollapsePanel_MouseLeave;
                }
                if (collapseButton != null)
                {
                    collapseButton.MouseEnter -= CollapsePanelButton_MouseEnter;
                    collapseButton.MouseLeave -= CollapsePanelButton_MouseLeave;
                    collapseButton.MouseEnter += CollapsePanelButton_MouseEnter;
                    collapseButton.MouseLeave += CollapsePanelButton_MouseLeave;
                }

                if (buttonPanel != null && buttonPanel.Visible)
                {
                    previousWindowSize = this.Size;
                    previousWindowLocation = this.Location;
                }

                if (buttonPanel != null)
                    buttonPanel.Visible = false;

                this.Size = new Size(
                    collapseButton.Width + (userLeftMargin * 2),
                    collapseButton.Height + (userTopMargin * 2)
                );

                collapseButton.Visible = true;

                if (statusStrip != null)
                    statusStrip.Visible = false;

                if (collapseCheckTimer != null)
                    collapseCheckTimer.Stop();
            }
            else
            {
                if (collapseButton != null && collapseButton.Visible && previousWindowSize.HasValue)
                {
                    this.Size = previousWindowSize.Value;
                    if (previousWindowLocation.HasValue)
                        this.Location = previousWindowLocation.Value;
                }

                if (collapseButton != null)
                    collapseButton.Visible = false;
                if (buttonPanel != null)
                    buttonPanel.Visible = true;

                previousWindowSize = null;
                previousWindowLocation = null;

                if (statusStrip != null)
                    statusStrip.Visible = statusVisible;

                this.BackColor = expandedPanelBackgroundColor;
            }
        }

        private void CollapsePanel_MouseEnter(object sender, EventArgs e)
        {
            if (collapseCheckTimer != null)
                collapseCheckTimer.Stop();
        }

        private void CollapsePanel_MouseLeave(object sender, EventArgs e)
        {
            if (collapseCheckTimer != null)
            {
                collapseCheckTimer.Interval = 300; // 300ms delay
                collapseCheckTimer.Start();
            }
        }

        private void CollapsePanelButton_MouseEnter(object sender, EventArgs e)
        {
            if (collapseCheckTimer != null)
                collapseCheckTimer.Stop();
        }

        private void CollapsePanelButton_MouseLeave(object sender, EventArgs e)
        {
            if (collapseCheckTimer != null)
            {
                collapseCheckTimer.Interval = 300; // 300ms delay
                collapseCheckTimer.Start();
            }
        }

        private void ButtonPanel_Resize(object sender, EventArgs e)
        {
            // No action needed when scrollbars are disabled
        }

        private void ButtonPanel_SizeChanged(object sender, EventArgs e)
        {
            // No action needed when scrollbars are disabled
        }

        private void SaveSettingsToIni()
        {
            WriteIni("Window", "AOT", this.TopMost ? "True" : "False");
            // WriteIni("Window", "Maximized", this.WindowState == FormWindowState.Maximized ? "True" : "False");
            WriteIni("Window", "LastOpened", Path.GetFileName(lastButtonFile));
        }


        // --- Status Strip ---
        private void InitializeStatusStrip()
        {
            statusStrip = new StatusStrip();
            toolStripStatusActiveDaw = new ToolStripStatusLabel();
            toolStripStatusMidiOut = new ToolStripStatusLabel();
            toolStripStatusMidiIn = new ToolStripStatusLabel();
            toolStripStatusAOT = new ToolStripStatusLabel();

            statusStrip.Items.AddRange(new ToolStripItem[] {
            toolStripStatusActiveDaw,
            toolStripStatusMidiOut,
            toolStripStatusMidiIn,
            toolStripStatusAOT
    });

            statusStrip.Dock = DockStyle.Bottom;
            this.Controls.Add(statusStrip);
            ApplyStatusState();
            statusStrip.MouseEnter += StatusStrip_MouseHover;
            statusStrip.MouseLeave += StatusStrip_MouseLeave;
        }

        private void UpdateStatusStrip(bool updateDawHandle = true)
        {
            // Defensive: ensure status strip and labels are initialized
            if (statusStrip == null ||
                toolStripStatusActiveDaw == null ||
                toolStripStatusMidiOut == null ||
                toolStripStatusMidiIn == null ||
                toolStripStatusAOT == null)
                return;

            if (updateDawHandle && !autoSwitchEnabled)
                UpdateActiveDawFromForeground();

            string dawName = string.IsNullOrEmpty(activeDawName) ? "None" : activeDawName;
            toolStripStatusActiveDaw.Text = string.Format(Properties.Resources.StatusActiveDaw, dawName);
            toolStripStatusActiveDaw.ForeColor = dawName == "None" ? Color.Red : Color.Green;

            toolStripStatusMidiOut.Text = string.Format(Properties.Resources.StatusMidiOut, midiOutNameLabel);
            toolStripStatusMidiOut.ForeColor = string.IsNullOrEmpty(midiOutNameLabel) ? Color.Red : Color.Green;
            toolStripStatusMidiIn.Text = string.Format(Properties.Resources.StatusMidiIn, midiInNameLabel);
            toolStripStatusMidiIn.ForeColor = string.IsNullOrEmpty(midiInNameLabel) ? Color.Red : Color.Green;

            toolStripStatusAOT.Text = string.Format(Properties.Resources.StatusAOT, this.TopMost ? "On" : "Off");
            toolStripStatusAOT.ForeColor = this.TopMost ? Color.Red : Color.Green;

            statusStrip.Invalidate();
            statusStrip.Update();
        }

        private void StatusStrip_MouseHover(object sender, EventArgs e)
        {
            if (statusTooltipVisible)
                return; // Already shown, do not show again

            statusTooltipVisible = true;

            string allStatus =
                string.Format(Properties.Resources.StatusActiveDaw, string.IsNullOrEmpty(activeDawName) ? "None" : activeDawName) + "\n" +
                string.Format(Properties.Resources.StatusMidiIn, midiInNameLabel) + "\n" +
                string.Format(Properties.Resources.StatusMidiOut, midiOutNameLabel) + "\n" +
                string.Format(Properties.Resources.StatusAOT, this.TopMost ? "On" : "Off");

            string[] lines = allStatus.Split('\n');
            int totalHeight = 0;
            foreach (string line in lines)
            {
                string paddedLine = "  " + line;
                Size lineSize = TextRenderer.MeasureText(paddedLine, tooltipFont);
                totalHeight += lineSize.Height;
            }
            totalHeight += 2;

            Point mouseScreen = Cursor.Position;
            Point mouseClient = this.PointToClient(mouseScreen);

            int yAbove = mouseClient.Y - totalHeight + 30;
            if (yAbove < 0) yAbove = 0;

            statusToolTip.Show(allStatus, this, mouseClient.X, yAbove, 4000);
        }

        // In your MouseLeave handler, reset the flag and hide the tooltip:
        private void StatusStrip_MouseLeave(object sender, EventArgs e)
        {
            statusTooltipVisible = false;
            if (statusToolTip != null)
                statusToolTip.Hide(this);
        }


        private void ApplyStatusState()
        {
            if (statusStrip != null)
            {
                statusStrip.Visible = statusVisible;
                statusStrip.SizingGrip = titleBarVisible; // Show grip only if title bar is visible
            }

            if (titleBarVisible)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.ControlBox = true;
                UpdateFormTitle();
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.ControlBox = false;
            }

            // Restore Cubase window z-order if affected
            if (cubaseHwnd != IntPtr.Zero)
            {
                SetForegroundWindow(cubaseHwnd);
            }
        }

        private void ToggleTitleBar()
        {
            this.SuspendLayout();
            Size oldClientSize = this.ClientSize;
            ApplyStatusState();

            if (titleBarVisible)
            {
                // --- Place this block right here ---
                if (titleBarVisible && collapsible)
                {
                    WriteIniToButtonFile("Buttons", "Collapsible", "False");
                    collapsible = false;
                    if (collapsibleItem != null)
                        collapsibleItem.Checked = false;
                }
                // --- End of block ---
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.ControlBox = true;
                UpdateFormTitle();
                this.RecreateHandle();
                this.ClientSize = oldClientSize;
                if (this.ClientSize != oldClientSize)
                {
                    Size delta = new Size(
                        oldClientSize.Width - this.ClientSize.Width,
                        oldClientSize.Height - this.ClientSize.Height
                    );
                    this.Size = new Size(this.Width + delta.Width, this.Height + delta.Height);
                }
                // Disable and uncheck collapsible items
                if (collapsibleItem != null)
                {
                    collapsibleItem.Checked = false;
                    collapsibleItem.Enabled = true;
                    collapsibleItem.Text = Properties.Resources.MenuMakeCollapsibleHidesTitleBar;
                }

                WriteIniToButtonFile("Buttons", "Collapsible", "False");
                collapsible = false;
                UpdateCollapseState();
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.ControlBox = false;
                UpdateFormTitle();
                this.Refresh();
                this.ClientSize = oldClientSize;
                // Enable collapsible items
                if (collapsibleItem != null)
                {
                    collapsibleItem.Enabled = true;
                    collapsibleItem.Text = Properties.Resources.MenuMakeCollapsible;
                }

                // Do not change Checked here; it will be set by LoadButtonSettingsFromIni
            }

            if (buttonPanel != null)
            {
                buttonPanel.Size = this.ClientSize;
                buttonPanel.PerformLayout();
                buttonPanel.Refresh();
            }

            ForceScrollRedraw();
            this.ResumeLayout(true);
            UpdateCollapseState();
            SaveButtonIniFile(lastButtonFile);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // Perform initial DAW detection FIRST (before UpdateStatusStrip)
            if (autoSwitchEnabled)
            {
                // Use foreground detection (focus-based)
                UpdateActiveDawFromForeground();
            }
            else
            {
                // Use process-based detection (no focus required)
                string firstRunning = GetFirstRunningDaw();
                if (firstRunning != "None")
                {
                    activeDawName = firstRunning;
                    cubaseHwnd = Process.GetProcessesByName(firstRunning)
                        .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero)?.MainWindowHandle ?? IntPtr.Zero;
                }
            }

            // NOW update the status strip (which will display the detected DAW)
            UpdateStatusStrip(false); // Pass false to prevent re-detection

            // Restore DAW window z-order/focus after loading
            if (cubaseHwnd != IntPtr.Zero)
            {
                System.Threading.Thread.Sleep(100);
                SetForegroundWindow(cubaseHwnd);
            }
        }

        public class NoFocusButton : Button
        {
            public NoFocusButton()
            {
                this.TabStop = false;
                this.SetStyle(ControlStyles.Selectable, false);
            }
            protected override bool ShowFocusCues => false;
            protected override void OnGotFocus(EventArgs e)
            {
                // Prevent focus rectangle
                base.OnLostFocus(e);
            }
        }

        // Replace the existing CreateMidiButtonGrid with this version
        private void CreateMidiButtonGrid()
        {
            if (buttonPanel == null)
            {
                buttonPanel = new ResizePanel
                {
                    Left = 0,
                    Top = 0,
                    Width = this.ClientSize.Width,
                    Height = this.ClientSize.Height,
                    AutoScroll = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                };
                typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    null, buttonPanel, new object[] { true });
                buttonPanel.Resize += ButtonPanel_Resize;
                buttonPanel.SizeChanged += ButtonPanel_SizeChanged;
                buttonPanel.MouseUp += Form1_MouseUpForContextMenu;
                buttonPanel.MouseDown += Form1_MouseDownForDrag;
                this.Controls.Add(buttonPanel);
            }

            buttonPanel.SuspendLayout();
            buttonPanel.Visible = false;
            buttonPanel.Controls.Clear();
            buttonToolTip.RemoveAll();
            buttonDefaultColors.Clear();

            // Add these lines here:
            buttonPanel.MouseWheel += ButtonPanel_MouseWheel;
            buttonPanel.PreviewKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                    e.IsInputKey = true;
            };
            buttonPanel.Focus();


            this.Controls.Add(buttonPanel);


            int viewportWidth = buttonPanel.ClientSize.Width;
            int cols = Math.Max(1, (viewportWidth - effectiveLeftMargin - buttonGap) / (buttonWidth + buttonGap));

            int yOffset = effectiveTopMargin - buttonPanelScrollOffset;
            int i = 0;

            while (i < midiActions.Count)
            {
                string sectionId = sectionIdForAction[i];
                SectionInfo section = GetSectionById(sectionId);
                int sectionStart = i;
                int sectionEnd = i;
                while (sectionEnd < midiActions.Count && sectionIdForAction[sectionEnd] == sectionId)
                    sectionEnd++;

                yOffset += 4;
                Color sectionColor = section != null ? section.Color : Color.White;
                string sectionName = section != null ? section.Name : "(Unknown)";

                if (displayHeaders)
                {
                    Label sectionLabel = new Label
                    {
                        Left = 0,
                        Top = yOffset,
                        Width = buttonPanel.ClientSize.Width,
                        Height = 18,
                        Font = new Font(Font.FontFamily, 8, FontStyle.Bold),
                        Text = sectionName,
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = sectionColor,
                        ForeColor = GetContrastingTextColor(sectionColor)
                    };

                    sectionLabel.MouseUp += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            EditSectionHeader(sectionId);
                        }
                    };

                    buttonPanel.Controls.Add(sectionLabel);
                    yOffset += sectionLabel.Height + buttonGap;
                }

                for (int j = sectionStart; j < sectionEnd; j++)
                {
                    MidiAction action = midiActions[j];
                    NoFocusButton btn = new NoFocusButton
                    {
                        Width = buttonWidth,
                        Height = buttonHeight,
                        Left = effectiveLeftMargin + ((j - sectionStart) % cols) * (buttonWidth + buttonGap) + buttonGap,
                        Top = yOffset + ((j - sectionStart) / cols) * (buttonHeight + buttonGap),
                        Text = action.Name,
                        Tag = action,
                        BackColor = sectionColor,
                        Font = GetButtonFont(),
                        ForeColor = GetContrastingTextColor(sectionColor)
                    };
                    buttonDefaultColors[btn] = sectionColor;

                    btn.MouseDown += MidiButton_MouseDown;
                    btn.MouseUp += MidiButton_MouseUp;
                    btn.MouseUp += MidiButton_RightClick;
                    btn.ContextMenuStrip = CreateMidiButtonContextMenu(action);

                    if (!string.IsNullOrWhiteSpace(action.Tooltip))
                        this.buttonToolTip.SetToolTip(btn, action.Tooltip);

                    if (buttonBorderColor.HasValue)
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 1;
                        btn.FlatAppearance.BorderColor = buttonBorderColor.Value;
                    }
                    else
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                    }

                    // --- ADD THESE TWO LINES HERE ---
                    btn.MouseEnter += CollapsePanel_MouseEnter;
                    btn.MouseLeave += CollapsePanel_MouseLeave;
                    // ---------------------------------

                    buttonPanel.Controls.Add(btn);
                }

                yOffset += ((sectionEnd - sectionStart + cols - 1) / cols) * (buttonHeight + buttonGap) + buttonGap;
                i = sectionEnd;
            }

            // No scrollbar compensation needed
            RecomputeAutoScrollSize();
            buttonPanel.AutoScroll = false;

            UpdateCollapseState();
            buttonPanel.ResumeLayout(true);
            buttonPanel.Visible = true;
            lastClientSize = this.ClientSize;

            buttonPanel.PerformLayout();
            buttonPanel.Invalidate();
            buttonPanel.Update();
            buttonPanel.AutoScrollPosition = new Point(0, 0);

        }

        private SectionInfo GetSectionById(string id)
        {
            return sections.FirstOrDefault(s => s.Id == id);
        }

        private void EditSectionHeader(string sectionId)
        {
            SectionInfo section = GetSectionById(sectionId);
            if (section == null) return;
            string colorHtml = ColorHelper.ToHtml(section.Color);

            using (SectionHeaderEditDialog dlg = new SectionHeaderEditDialog(sectionId, section.Name, colorHtml))
            {
                dlg.StartPosition = FormStartPosition.Manual;
                // Center over the main form, then move up by 100px
                Point parentScreen = this.PointToScreen(Point.Empty);
                int dlgX = parentScreen.X + (this.Width - dlg.Width) / 2;
                int dlgY = parentScreen.Y + (this.Height - dlg.Height) / 2 - 100;
                dlgY = Math.Max(dlgY, 0);
                dlg.Location = new Point(Math.Max(dlgX, 0), dlgY);

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    string newName = dlg.SectionName;
                    string newColor = dlg.SectionColor;

                    if (string.IsNullOrWhiteSpace(newName) || string.IsNullOrWhiteSpace(newColor))
                        return;

                    // Update the section header line in the button file
                    if (!File.Exists(lastButtonFile)) return;
                    List<string> lines = File.ReadAllLines(lastButtonFile).ToList();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string trimmed = lines[i].Trim();
                        if (trimmed.StartsWith("#"))
                        {
                            string[] parts = trimmed.Substring(1).Split(',');
                            if (parts.Length >= 3 && parts[0].Trim() == sectionId)
                            {
                                lines[i] = $"#{sectionId},{newName},{newColor}";
                                File.WriteAllLines(lastButtonFile, lines);
                                break;
                            }
                        }
                    }

                    // Reload and refresh
                    LoadMidiActionsFromFile(lastButtonFile);
                    // NEW MIDI DEVICES 6
                    // TESTING: commented out to prevent changing devices on section edit
                    SetMidiDevicesForActiveDaw();
                    UpdateStatusStrip(false);

                    CreateMidiButtonGrid();
                }
            }
        }

        private static Color GetContrastingTextColor(Color bg)
        {
            // Standard luminance formula for sRGB
            double luminance = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255;
            return luminance > 0.5 ? Color.Black : Color.White;
        }

        // Replace the existing RepositionMidiButtons with this version
        private void RepositionMidiButtons()
        {
            if (buttonPanel == null || midiActions.Count == 0)
                return;

            int contentHeight = GetContentHeight();
            int maxOffset = Math.Max(0, contentHeight - buttonPanel.ClientSize.Height);
            buttonPanelScrollOffset = Math.Max(0, Math.Min(buttonPanelScrollOffset, maxOffset));

            buttonPanel.SuspendLayout();

            int viewportWidth = buttonPanel.ClientSize.Width;
            int cols = Math.Max(1, (viewportWidth - effectiveLeftMargin - buttonGap) / (buttonWidth + buttonGap));

            int yOffset = effectiveTopMargin - buttonPanelScrollOffset;
            int i = 0;
            int controlIndex = 0;

            while (i < midiActions.Count)
            {
                string sectionId = sectionIdForAction[i];
                int sectionStart = i;
                int sectionEnd = i;
                while (sectionEnd < midiActions.Count && sectionIdForAction[sectionEnd] == sectionId)
                    sectionEnd++;

                yOffset += 4;

                if (displayHeaders)
                {
                    if (controlIndex < buttonPanel.Controls.Count && buttonPanel.Controls[controlIndex] is Label sectionLabel)
                    {
                        sectionLabel.Top = yOffset;
                        sectionLabel.Width = buttonPanel.ClientSize.Width;
                        controlIndex++;
                    }
                    yOffset += 18 + buttonGap;
                }

                for (int j = sectionStart; j < sectionEnd; j++)
                {
                    if (controlIndex < buttonPanel.Controls.Count && buttonPanel.Controls[controlIndex] is Button btn)
                    {
                        btn.Left = effectiveLeftMargin + ((j - sectionStart) % cols) * (buttonWidth + buttonGap) + buttonGap;
                        btn.Top = yOffset + ((j - sectionStart) / cols) * (buttonHeight + buttonGap);
                        btn.Width = buttonWidth;
                        btn.Height = buttonHeight;
                        controlIndex++;
                    }
                }
                yOffset += ((sectionEnd - sectionStart + cols - 1) / cols) * (buttonHeight + buttonGap) + buttonGap;
                i = sectionEnd;
            }

            buttonPanel.ResumeLayout(true);

            buttonPanel.PerformLayout();
            buttonPanel.Invalidate();
            buttonPanel.Update();
            // buttonPanel.AutoScrollPosition = new Point(0, 0);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.ClientSize != lastClientSize)
            {
                lastClientSize = this.ClientSize;

                if (buttonPanel != null)
                {
                    RepositionMidiButtons();

                    // Add this block:
                    buttonPanel.PerformLayout();
                    buttonPanel.Invalidate();
                    buttonPanel.Update();
                    // buttonPanel.AutoScrollPosition = new Point(0, 0);
                }
                // SaveButtonIniFile(lastButtonFile); // <-- Add this line
            }
        }

        private int GetContentHeight()
        {
            int viewportWidth = buttonPanel?.ClientSize.Width ?? 0;
            int cols = Math.Max(1, (viewportWidth - effectiveLeftMargin - buttonGap) / (buttonWidth + buttonGap));
            int yOffset = effectiveTopMargin;
            int i = 0;
            while (i < midiActions.Count)
            {
                string sectionId = sectionIdForAction[i];
                int sectionStart = i;
                int sectionEnd = i;
                while (sectionEnd < midiActions.Count && sectionIdForAction[sectionEnd] == sectionId)
                    sectionEnd++;

                yOffset += 4;
                if (displayHeaders)
                    yOffset += 18 + buttonGap;

                yOffset += ((sectionEnd - sectionStart + cols - 1) / cols) * (buttonHeight + buttonGap) + buttonGap;
                i = sectionEnd;
            }
            int extra = 10;
            if (statusStrip != null && statusStrip.Visible)
                extra += statusStrip.Height;
            return yOffset + extra;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            lastClientSize = this.ClientSize;
            if (buttonPanel != null)
            {
                RepositionMidiButtons();
            }

            // Only update previousWindowSize when not collapsed
            if (!(collapsible && !titleBarVisible && collapseButton != null && collapseButton.Visible))
            {
                previousWindowSize = this.ClientSize;
            }

            SaveButtonIniFile(lastButtonFile);
        }

        private void ButtonPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // Scroll by one button per wheel notch
            int scrollAmount = buttonHeight; // Instead of SystemInformation.MouseWheelScrollLines * buttonHeight / 2
            int contentHeight = GetContentHeight();
            int maxOffset = Math.Max(0, contentHeight - buttonPanel.ClientSize.Height);

            if (e.Delta > 0)
                buttonPanelScrollOffset = Math.Max(0, buttonPanelScrollOffset - scrollAmount);
            else
                buttonPanelScrollOffset = Math.Min(maxOffset, buttonPanelScrollOffset + scrollAmount);

            RepositionMidiButtons();
        }


        // 2. Update LoadMidiActionsFromFile
        private void LoadMidiActionsFromFile(string filePath)
        {
            lastButtonFile = filePath;

            midiActions.Clear();
            sections.Clear();
            sectionIdForAction.Clear();

            if (!File.Exists(filePath))
                return;

            string[] lines = File.ReadAllLines(filePath);
            string currentSectionId = null;
            int actionIndex = 0;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                if (trimmed.StartsWith("#"))
                {
                    // ... existing section header logic ...
                    string[] parts = trimmed.Substring(1).Split(',');
                    if (parts.Length >= 3)
                    {
                        string sectionId = parts[0].Trim();
                        string sectionName = parts[1].Trim();
                        string colorStr = parts[2].Trim();
                        Color color = ColorHelper.ParseOrDefault(colorStr, Color.White);
                        sections.Add(new SectionInfo { Id = sectionId, Name = sectionName, Color = color });
                        currentSectionId = sectionId;
                    }
                    continue;
                }

                string[] actionParts = trimmed.Split(',');
                if (actionParts.Length < 6) continue;

                string type = actionParts[3].Trim();
                MidiAction action = new MidiAction
                {
                    Name = actionParts[0].Trim(),
                    Tooltip = actionParts[1].Trim(),
                    Channel = int.TryParse(actionParts[2], out int ch) ? ch : 1,
                    IsNote = type.Equals("Note", StringComparison.OrdinalIgnoreCase),
                    IsKey = type.Equals("KEY", StringComparison.OrdinalIgnoreCase), // NEW
                    Value = int.TryParse(actionParts[4], out int val) ? val : 0,
                    MouseDownValue = int.TryParse(actionParts[5], out int mdv) ? mdv : 0,
                    MouseUpValue = actionParts.Length > 6 && !string.IsNullOrWhiteSpace(actionParts[6]) ? (int.TryParse(actionParts[6], out int muv) ? (int?)muv : null) : null,
                    KeyString = type.Equals("KEY", StringComparison.OrdinalIgnoreCase) ? actionParts[4].Trim() : null // NEW
                };
                midiActions.Add(action);
                sectionIdForAction.Add(currentSectionId);
                actionIndex++;
            }

            ///////

            if (midiOut != null)
            {
                midiOut.Dispose();
                midiOut = null;
            }

            if (midiEnabled)
            {
                if (!string.IsNullOrEmpty(midiOutNameLabel))
                {
                    for (int i = 0; i < MidiOut.NumberOfDevices; i++)
                    {
                        MidiOutCapabilities caps = MidiOut.DeviceInfo(i);
                        if (caps.ProductName.Equals(midiOutNameLabel, StringComparison.OrdinalIgnoreCase))
                        {
                            midiOut = new MidiOut(i);
                            break;
                        }
                    }
                }

                if (midiOut == null && MidiOut.NumberOfDevices > 0)
                    midiOut = new MidiOut(0);
            }

        }

        private void SetMidiDevicesForActiveDaw()
        {
            if (isSettingMidiDevices) return;
            isSettingMidiDevices = true;

            try
            {
                // Get config FIRST
                ProcessMidiConfig config = GetActiveProcessMidiConfig();

                // Fix: Check for empty strings as well as null values
                string newMidiOutName = string.IsNullOrEmpty(config?.MidiOutName)
                    ? ReadIni("MIDI", "MidiOut")
                    : config.MidiOutName;

                string newMidiInName = string.IsNullOrEmpty(config?.MidiInName)
                    ? ReadIni("MIDI", "MidiIn")
                    : config.MidiInName;

                // Only change devices if they've changed
                bool midiOutChanged = midiOutNameLabel != newMidiOutName;
                bool midiInChanged = midiInNameLabel != newMidiInName;

                // Update labels first
                midiOutNameLabel = newMidiOutName;
                midiInNameLabel = newMidiInName;

                // Process MIDI OUT device if needed
                if (midiOutChanged && midiEnabled)
                {
                    // Dispose old MIDI out device first
                    if (midiOut != null)
                    {
                        try { midiOut.Dispose(); }
                        catch { /* Ignore errors */ }
                        midiOut = null;

                        // Still need a short delay for MIDI Out since we're recreating it
                        System.Threading.Thread.Sleep(200);
                    }

                    // Try to create new MIDI out device
                    try
                    {
                        if (!string.IsNullOrEmpty(midiOutNameLabel))
                        {
                            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
                            {
                                MidiOutCapabilities caps = MidiOut.DeviceInfo(i);
                                if (caps.ProductName.Equals(midiOutNameLabel, StringComparison.OrdinalIgnoreCase))
                                {
                                    midiOut = new MidiOut(i);
                                    break;
                                }
                            }
                        }

                        // If specific device not found, try to use first available
                        if (midiOut == null && MidiOut.NumberOfDevices > 0)
                        {
                            midiOut = new MidiOut(0);
                        }
                    }
                    catch (Exception)
                    {
                        // Silently ignore MIDI out errors
                    }
                }

                // Process MIDI IN device if needed - use the SwitchToDevice method
                // No additional delays needed here as they're handled within SwitchToDevice
                if (midiInChanged && midiEnabled)
                {
                    // Find device ID for the name
                    int midiInDeviceId = -1;
                    for (int i = 0; i < MidiIn.NumberOfDevices; i++)
                    {
                        MidiInCapabilities caps = MidiIn.DeviceInfo(i);
                        if (caps.ProductName.Equals(midiInNameLabel, StringComparison.OrdinalIgnoreCase))
                        {
                            midiInDeviceId = i;
                            break;
                        }
                    }

                    // If we found a valid device ID
                    if (midiInDeviceId >= 0)
                    {
                        if (midiFromHost != null)
                        {
                            // SwitchToDevice handles all cleanup and delays internally
                            midiFromHost.SwitchToDevice(midiInDeviceId, midiInNameLabel);
                        }
                        else
                        {
                            // Create new instance if none exists
                            try
                            {
                                midiFromHost = new Midifromhost(midiInDeviceId, midiInNameLabel, this.Location);
                                midiFromHost.MidiMessageReceived += MidiFromHost_MidiMessageReceived;
                            }
                            catch (Exception)
                            {
                                // Silently ignore MIDI in errors
                            }
                        }
                    }
                }
            }
            finally
            {
                isSettingMidiDevices = false;
            }
        }
        public static class InputBox
        {
            public static string Show(string prompt, string title, string defaultValue = "", Panel buttonPanel = null)
            {
                Form form = new Form();
                Label label = new Label();
                TextBox textBox = new TextBox();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = prompt;
                textBox.Text = defaultValue;

                buttonOk.Text = Properties.Resources.DialogOK;
                buttonCancel.Text = Properties.Resources.DialogCancelButton;
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                // Set dialog size to match Open Button Panel dialog
                int formWidth = 200;
                int formHeight = 100;
                form.ClientSize = new Size(formWidth, formHeight);

                // Adjust label and textbox widths for the smaller dialog
                int margin = 12;
                label.SetBounds(margin, 10, formWidth - margin * 2, 13);
                textBox.SetBounds(margin, 30, formWidth - margin * 2, 20);

                // Place OK at the left margin, Cancel at the right margin
                int buttonWidth = 80;
                int buttonHeight = 25;
                int buttonY = 62;

                buttonOk.SetBounds(margin, buttonY, buttonWidth, buttonHeight);
                buttonCancel.SetBounds(formWidth - buttonWidth - margin, buttonY, buttonWidth, buttonHeight);

                label.AutoSize = false;
                label.TextAlign = ContentAlignment.MiddleLeft;
                textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.Manual;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                // Center over buttonPanel and move up by 200 pixels
                if (buttonPanel != null)
                {
                    Point panelScreen = buttonPanel.PointToScreen(Point.Empty);
                    int dlgX = panelScreen.X + (buttonPanel.Width - formWidth) / 2;
                    int dlgY = panelScreen.Y + (buttonPanel.Height - formHeight) / 2 - 200;
                    dlgY = Math.Max(dlgY, 0);
                    form.Location = new Point(Math.Max(dlgX, 0), dlgY);
                }

                DialogResult dialogResult = form.ShowDialog();
                return dialogResult == DialogResult.OK ? textBox.Text : "";
            }
        }

        private void RecomputeAutoScrollSize(int scrollbarComp = 0)
        {
            if (buttonPanel == null) return;

            int contentWidth = 0;
            int contentHeight = 0;

            foreach (Control c in buttonPanel.Controls)
            {
                if (!c.Visible) continue;
                contentWidth = Math.Max(contentWidth, c.Right);
                contentHeight = Math.Max(contentHeight, c.Bottom);
            }

            // Clamp width to viewport to avoid horizontal scrollbars
            int viewportWidth = Math.Max(0, buttonPanel.ClientSize.Width - scrollbarComp);
            // leave a 1px slack inside the viewport
            contentWidth = Math.Min(contentWidth, Math.Max(0, viewportWidth - 1));

            buttonPanel.AutoScrollMinSize = new Size(contentWidth, contentHeight);
        }
        private void CreateNewButtonPanel_Click(object sender, EventArgs e)
        {
            // Temporarily disable TopMost so the dialog appears above the main window
            bool wasTopMost = this.TopMost;
            this.TopMost = false;

            string panelName = InputBox.Show(
                Properties.Resources.DialogEnterNewPanelName,
                Properties.Resources.DialogCreateNewButtonPanel,
                Properties.Resources.DialogMyPanel,
                buttonPanel
            );

            // Restore TopMost state
            this.TopMost = wasTopMost;

            if (string.IsNullOrWhiteSpace(panelName))
                return;

            // Sanitize name: remove invalid filename chars
            foreach (char c in Path.GetInvalidFileNameChars())
                panelName = panelName.Replace(c.ToString(), "");

            string newTxt = Path.Combine(ButtonsFolder(), $"buttons {panelName}.txt");
            string newIni = Path.Combine(ButtonsFolder(), $"buttons {panelName}.ini");

            // If file exists, prompt for overwrite
            if (File.Exists(newTxt) || File.Exists(newIni))
            {
                DialogResult result = MessageBox.Show(
                    string.Format(Properties.Resources.DialogFileExists, newTxt, newIni),
                    Properties.Resources.DialogFileExists,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                if (result != DialogResult.Yes)
                    return;
            }

            // Copy current panel files
            File.Copy(lastButtonFile, newTxt, true);
            string currentIni = GetButtonIniFile(lastButtonFile);
            if (File.Exists(currentIni))
                File.Copy(currentIni, newIni, true);

            // Switch to new panel
            lastButtonFile = newTxt;
            LoadPanelSettingsFromIni(lastButtonFile);
            SyncMenuItemsWithSettings(lastButtonFile);
            headersItem.Checked = displayHeaders;
            LoadMidiActionsFromFile(lastButtonFile);
            // NEW MIDI DEVICES 7
            // TESTING
            SetMidiDevicesForActiveDaw();
            UpdateStatusStrip(false);

            if (buttonPanel != null)
            {
                buttonPanel.Size = this.ClientSize;
                buttonPanel.PerformLayout();
                buttonPanel.Refresh();
                Application.DoEvents();
            }
            CreateMidiButtonGrid();
            UpdateFormTitle();
            // UpdateStatusStrip();
            UpdateStatusStrip(false);

            MessageBox.Show(
                string.Format(Properties.Resources.DialogPanelCreated, newTxt),
                Properties.Resources.DialogPanelCreated,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private ContextMenuStrip CreateMidiButtonContextMenu(MidiAction action)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            string valueLabel = action.IsNote ? Properties.Resources.ContextMenuNoteValue : Properties.Resources.ContextMenuCCValue;
            menu.Items.Add(string.Format(Properties.Resources.ContextMenuChannel, action.Channel));
            menu.Items.Add(string.Format(Properties.Resources.ContextMenuValueDown, valueLabel, action.MouseDownValue));
            menu.Items.Add(string.Format(Properties.Resources.ContextMenuValueUp, valueLabel, action.MouseUpValue.HasValue ? action.MouseUpValue.Value.ToString() : "(none)"));
            return menu;
        }

        private void ViewPdfManual_Click(object sender, EventArgs e)
        {
            string pdfPath = Path.Combine(AppDataRoot, "CN Remote Manual.pdf");
            if (File.Exists(pdfPath))
            {
                try { Process.Start(pdfPath); }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(Properties.Resources.DialogPdfManualNotFound + "\n{0}", ex.Message),
                        Properties.Resources.DialogError,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            else
            {
                MessageBox.Show(
                    Properties.Resources.DialogPdfManualNotFound,
                    Properties.Resources.DialogError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ViewReadme_Click(object sender, EventArgs e)
        {
            string readmePath = ReadmeFile();
            if (File.Exists(readmePath))
            {
                try { Process.Start(readmePath); }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(Properties.Resources.DialogReadmeNotFound + "\n{0}", ex.Message),
                        Properties.Resources.DialogError,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            else
            {
                MessageBox.Show(
                    Properties.Resources.DialogReadmeNotFound,
                    Properties.Resources.DialogError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void AboutItem_Click(object sender, EventArgs e)
        {
            using (SplashForm splash = new SplashForm())
            {
                splash.ShowDialog(this);
            }
        }

        private void MidiButton_RightClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Button btn = sender as Button;
                MidiAction action = btn?.Tag as MidiAction;
                if (action == null) return;

                int index = midiActions.IndexOf(action);
                if (index < 0) return;

                // Change button color to red to indicate editing
                if (btn != null)
                    btn.BackColor = Color.Red;

                using (EditMidiActionForm dlg = new EditMidiActionForm(GetButtonIniFile(lastButtonFile), tooltipBackColor, tooltipForeColor))
                {
                    // Set all fields from the action
                    dlg.ActionName = action.Name;
                    dlg.Tooltip = action.Tooltip;
                    dlg.Channel = action.Channel;
                    dlg.Type = action.IsKey ? "Key" : (action.IsNote ? "Note" : "CC");
                    dlg.Value = action.Value;
                    dlg.KeyString = action.KeyString;
                    dlg.MouseDownValue = action.MouseDownValue;
                    dlg.MouseUpValue = action.MouseUpValue;

                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        if (dlg.AddRequested)
                        {
                            MidiAction newAction = new MidiAction
                            {
                                Name = dlg.ActionName,
                                Tooltip = dlg.Tooltip,
                                Channel = dlg.Channel,
                                IsNote = dlg.Type == "Note",
                                IsKey = dlg.Type == "Key",
                                Value = dlg.Type == "Key" ? 0 : dlg.Value,
                                MouseDownValue = dlg.Type == "Key" ? 0 : dlg.MouseDownValue,
                                MouseUpValue = dlg.Type == "Key" ? (int?)null : dlg.MouseUpValue,
                                KeyString = dlg.Type == "Key" ? dlg.KeyString : null
                            };
                            midiActions.Insert(index + 1, newAction);
                            sectionIdForAction.Insert(index + 1, sectionIdForAction[index]);
                            InsertButtonFileLine(index + 1, newAction);
                            SaveButtonFile();
                            CreateMidiButtonGrid();
                        }
                        else if (dlg.DeleteRequested)
                        {
                            if (midiActions.Count > 1)
                            {
                                midiActions.RemoveAt(index);
                                sectionIdForAction.RemoveAt(index);
                                DeleteButtonFileLine(index);
                                SaveButtonFile();
                                CreateMidiButtonGrid();
                            }
                            else
                            {
                                MessageBox.Show(
                                    Properties.Resources.DialogAtLeastOneButtonMustRemain,
                                    Properties.Resources.DialogDeleteNotAllowed,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );
                            }
                        }
                        else if (dlg.MoveLeftRequested || dlg.MoveRightRequested)
                        {
                            int targetIndex = index;
                            if (dlg.MoveLeftRequested && index > 0)
                            {
                                // Only allow move if same section
                                if (sectionIdForAction[index] == sectionIdForAction[index - 1])
                                    targetIndex = index - 1;
                            }
                            else if (dlg.MoveRightRequested && index < midiActions.Count - 1)
                            {
                                // Only allow move if same section
                                if (sectionIdForAction[index] == sectionIdForAction[index + 1])
                                    targetIndex = index + 1;
                            }

                            if (targetIndex != index)
                            {
                                // Swap actions
                                MidiAction tempAction = midiActions[targetIndex];
                                midiActions[targetIndex] = midiActions[index];
                                midiActions[index] = tempAction;

                                // Section assignment stays the same (since we only allow within-section moves)
                                SaveButtonFile();
                                LoadMidiActionsFromFile(lastButtonFile);
                                // NEW MIDI DEVICES 8
                                SetMidiDevicesForActiveDaw();
                                UpdateStatusStrip(false);

                                CreateMidiButtonGrid();
                            }
                        }
                        else
                        {
                            // Update all fields from the dialog
                            action.Name = dlg.ActionName;
                            action.Tooltip = dlg.Tooltip;
                            action.Channel = dlg.Channel;
                            action.IsNote = dlg.Type == "Note";
                            action.IsKey = dlg.Type == "Key";
                            if (action.IsKey)
                            {
                                action.KeyString = dlg.KeyString;
                                action.Value = 0; // or keep previous value if you want, but 0 is safest
                                action.MouseDownValue = 0;
                                action.MouseUpValue = null;
                            }
                            else
                            {
                                action.KeyString = null;
                                action.Value = dlg.Value;
                                action.MouseDownValue = dlg.MouseDownValue;
                                action.MouseUpValue = dlg.MouseUpValue;
                            }
                            btn.Text = action.Name;
                            UpdateButtonFileLine(index, action);
                            CreateMidiButtonGrid();
                        }
                    }
                }
                // Restore button color to default or MIDI IN color after editing
                if (btn != null)
                {
                    bool isMidiInActive = midiInActive.ContainsKey(btn) && midiInActive[btn];
                    if (isMidiInActive)
                    {
                        btn.BackColor = midiInColor;
                        btn.ForeColor = GetContrastingTextColor(midiInColor);
                    }
                    else if (buttonDefaultColors.ContainsKey(btn))
                    {
                        btn.BackColor = buttonDefaultColors[btn];
                        btn.ForeColor = GetContrastingTextColor(buttonDefaultColors[btn]);
                    }
                }

                // Restore DAW focus after dialog and logic
                if (cubaseHwnd != IntPtr.Zero)
                {
                    SetForegroundWindow(cubaseHwnd);
                }
            }
        }

        // 3. Update MidiButton_MouseDown and MidiButton_MouseUp
        private void MidiButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Button btn = sender as Button;
            MidiAction action = btn?.Tag as MidiAction;

            bool isMidiInActive = btn != null && midiInActive.ContainsKey(btn) && midiInActive[btn];
            if (btn != null && !isMidiInActive && action != null && !action.IsKey)
            {
                Color glowColor = (action.MouseUpValue.HasValue) ? buttonGlowColorUp : buttonGlowColorDown;
                btn.BackColor = glowColor;
                btn.ForeColor = GetContrastingTextColor(glowColor);
            }

            if (action != null)
            {
                if (action.IsKey && !string.IsNullOrWhiteSpace(action.KeyString))
                {
                    ExecuteKeyMacro(action.KeyString, btn);
                }
                else if (midiOut != null)
                {
                    System.Threading.Thread.Sleep(10);
                    int channel = Math.Max(1, Math.Min(16, action.Channel));
                    int number = Math.Max(0, Math.Min(127, action.Value));
                    int value = Math.Max(0, Math.Min(127, action.MouseDownValue));
                    if (action.IsNote)
                        midiOut.Send(MidiMessage.StartNote(number, value, channel).RawData);
                    else
                        midiOut.Send(MidiMessage.ChangeControl(number, value, channel).RawData);
                }
            }
        }

        private void MidiButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Button btn = sender as Button;
            MidiAction action = btn?.Tag as MidiAction;

            // Prevent any action for key actions on MouseUp
            if (action != null && action.IsKey)
                return;

            // Only reset if MIDI IN is not active for this button
            if (btn != null && buttonDefaultColors.ContainsKey(btn) && action != null && !action.IsKey)
            {
                bool isMidiInActive = midiInActive.ContainsKey(btn) && midiInActive[btn];
                if (!isMidiInActive)
                {
                    if (action.MouseUpValue.HasValue)
                    {
                        // Use ButtonGlowColourUp for up, then revert to default after a short delay
                        btn.BackColor = buttonGlowColorUp;
                        btn.ForeColor = GetContrastingTextColor(buttonGlowColorUp);
                        Timer timer = new Timer { Interval = 120 };
                        timer.Tick += (s, ev) =>
                        {
                            timer.Stop();
                            btn.BackColor = buttonDefaultColors[btn];
                            btn.ForeColor = GetContrastingTextColor(buttonDefaultColors[btn]);
                            timer.Dispose();
                        };
                        timer.Start();
                    }
                    else
                    {
                        btn.BackColor = buttonDefaultColors[btn];
                        btn.ForeColor = GetContrastingTextColor(buttonDefaultColors[btn]);
                    }
                }
            }

            // Only send MouseUpValue if set and not a KEY action
            if (action != null && midiOut != null && action.MouseUpValue.HasValue && !action.IsKey)
            {
                System.Threading.Thread.Sleep(10);
                int channel = Math.Max(1, Math.Min(16, action.Channel));
                int number = Math.Max(0, Math.Min(127, action.Value)); // CC or Note number
                int value = Math.Max(0, Math.Min(127, action.MouseUpValue.Value)); // Value to send
                if (action.IsNote)
                    midiOut.Send(MidiMessage.StartNote(number, value, channel).RawData);
                else
                    midiOut.Send(MidiMessage.ChangeControl(number, value, channel).RawData);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            midiOut?.Dispose();
            midiFromHost?.Dispose(); // Dispose MIDI input and dialog

            // Dispose collapseCheckTimer if it exists
            if (collapseCheckTimer != null)
            {
                collapseCheckTimer.Stop();
                collapseCheckTimer.Dispose();
                collapseCheckTimer = null;
            }
            if (autoSwitchTimer != null)
            {
                autoSwitchTimer.Stop();
                autoSwitchTimer.Dispose();
                autoSwitchTimer = null;
            }

            base.OnFormClosed(e);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            ForwardKeyToCubase(e.KeyCode, true);
            e.Handled = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            ForwardKeyToCubase(e.KeyCode, false);
            e.Handled = true;
        }

        // SEND KEY COMMANDS
        /// <summary>
        /// Sends a key macro to AutoHotkey, handling quoted and non-quoted segments in order.
        /// - Non-quoted segments (e.g. Ctrl+Alt+K+200+X) are concatenated and sent as a single mapped AHK command.
        /// - Quoted segments (e.g. "Hello World") are sent as literal text using {Text}.
        /// - The sending order matches the original string.
        /// </summary>
        private void SendKeys(string keyString, Button btn)
        {
            // 1. Set button color to indicate action start
            if (btn != null && buttonDefaultColors.ContainsKey(btn))
            {
                btn.BackColor = buttonGlowColorDown;
                btn.ForeColor = GetContrastingTextColor(buttonGlowColorDown);
                btn.Refresh();
            }

            // 2. Split the input string into quoted and non-quoted segments, preserving order
            var segments = AhkKeyMapper.SplitKeyStringRespectingQuotes(keyString);
            List<string> nonQuotedBuffer = new List<string>();
            AutoHotkeyEngine ahk = AutoHotkey.Interop.AutoHotkeyEngine.Instance;

            // Helper: send all accumulated non-quoted segments as a single mapped AHK command
            Action sendNonQuoted = () =>
            {
                if (nonQuotedBuffer.Count > 0)
                {
                    // Concatenate with +, map to AHK, and send
                    string joined = string.Join("+", nonQuotedBuffer.Where(s => !string.IsNullOrWhiteSpace(s)));
                    string mapped = AhkKeyMapper.ToAhkKey(joined);
                    if (!string.IsNullOrWhiteSpace(mapped))
                        ahk.ExecRaw($"Send, {mapped}");
                    nonQuotedBuffer.Clear();
                }
            };

            // 3. Iterate through segments, sending in correct order
            foreach (var seg in segments)
            {
                string trimmed = seg.Trim();
                if (trimmed.StartsWith("\"") && trimmed.EndsWith("\"") && trimmed.Length >= 2)
                {
                    sendNonQuoted();
                    string text = trimmed.Substring(1, trimmed.Length - 2);

                    // Replace any single % not part of %r% with %%
                    // This will NOT touch %r% (already replaced), and will NOT touch %var% (other variables)
                    text = System.Text.RegularExpressions.Regex.Replace(
                        text,
                        @"%(?![a-zA-Z0-9_]+%)", // matches % not followed by word+%
                        "`%"
                    );

                    ahk.ExecRaw($"Send, {{Text}}{text}");
                }
                else
                {
                    nonQuotedBuffer.Add(trimmed);
                }
            }
            // 4. Send any remaining non-quoted segments
            sendNonQuoted();

            // 5. Reset button color after action
            if (btn != null && buttonDefaultColors.ContainsKey(btn))
            {
                btn.BackColor = buttonDefaultColors[btn];
                btn.ForeColor = GetContrastingTextColor(buttonDefaultColors[btn]);
                btn.Refresh();
            }
            // 6. Optionally, remove focus from the button to prevent accidental repeats
            if (btn != null && btn.Parent != null)
            {
                btn.Parent.Focus();
            }
        }

        private void ExecuteKeyMacro(string keyString, Button btn)
        {
            var lines = keyString.Replace("\r", "").Split('\n');
            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                int repeat = 1;
                var repeatMatch = System.Text.RegularExpressions.Regex.Match(line, @"^\[Repeat=(\d+)\](.*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (repeatMatch.Success)
                {
                    repeat = int.Parse(repeatMatch.Groups[1].Value);
                    line = repeatMatch.Groups[2].Value.TrimStart('+');
                }

                if (string.IsNullOrWhiteSpace(line)) continue;

                for (int i = 1; i <= repeat; i++)
                {
                    // Substitute %r% with current repeat index
                    string substitutedLine = line.Replace("%r%", i.ToString());
                    SendKeys(substitutedLine, btn);
                }
            }
        }

        private void SaveButtonFile()
        {
            if (!File.Exists(lastButtonFile)) return;

            List<string> lines = new List<string>();
            string lastSectionId = null;

            for (int i = 0; i < midiActions.Count; i++)
            {
                string sectionId = sectionIdForAction[i];
                if (sectionId != lastSectionId)
                {
                    SectionInfo section = GetSectionById(sectionId);
                    if (section != null)
                        lines.Add($"#{section.Id},{section.Name},{ColorHelper.ToHtml(section.Color)}");
                    lastSectionId = sectionId;
                }
                MidiAction action = midiActions[i];
                string newLine;
                if (action.IsKey)
                {
                    newLine = $"{action.Name},{action.Tooltip},{action.Channel},KEY,{action.KeyString},0,";
                }
                else
                {
                    newLine = $"{action.Name},{action.Tooltip},{action.Channel},{(action.IsNote ? "Note" : "CC")},{action.Value},{action.MouseDownValue},{(action.MouseUpValue.HasValue ? action.MouseUpValue.Value.ToString() : "")}";
                }
                lines.Add(newLine);
            }
            File.WriteAllLines(lastButtonFile, lines);
        }

        private void DeleteButtonFileLine(int index)
        {
            if (!File.Exists(lastButtonFile)) return;
            List<string> lines = File.ReadAllLines(lastButtonFile).ToList();
            int actionLineCount = 0;
            int lineIndex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                if (actionLineCount == index)
                {
                    lineIndex = i;
                    break;
                }
                actionLineCount++;
            }
            if (lineIndex >= 0)
            {
                lines.RemoveAt(lineIndex);
                File.WriteAllLines(lastButtonFile, lines);
            }
        }

        private void UpdateButtonFileLine(int index, MidiAction action)
        {
            if (!File.Exists(lastButtonFile)) return;

            List<string> lines = File.ReadAllLines(lastButtonFile).ToList();
            int actionLineCount = 0;
            int lineIndex = -1;

            // Find the line corresponding to the action index (skip section headers and comments)
            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                if (actionLineCount == index)
                {
                    lineIndex = i;
                    break;
                }
                actionLineCount++;
            }

            if (lineIndex >= 0)
            {
                string newLine;
                if (action.IsKey)
                {
                    newLine = $"{action.Name},{action.Tooltip},{action.Channel},KEY,{action.KeyString},0,";
                }
                else
                {
                    newLine = $"{action.Name},{action.Tooltip},{action.Channel},{(action.IsNote ? "Note" : "CC")},{action.Value},{action.MouseDownValue},{(action.MouseUpValue.HasValue ? action.MouseUpValue.Value.ToString() : "")}";
                }
                lines[lineIndex] = newLine;
                File.WriteAllLines(lastButtonFile, lines);
            }
        }
        private void InsertButtonFileLine(int index, MidiAction action)
        {
            if (!File.Exists(lastButtonFile)) return;

            List<string> lines = File.ReadAllLines(lastButtonFile).ToList();
            string targetSectionId = sectionIdForAction[index];

            // Find the section header for the target section
            int sectionHeaderIndex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.StartsWith("#"))
                {
                    string[] parts = trimmed.Substring(1).Split(',');
                    if (parts.Length >= 3 && parts[0].Trim() == targetSectionId)
                    {
                        sectionHeaderIndex = i;
                        break;
                    }
                }
            }
            if (sectionHeaderIndex == -1) return; // Section not found

            // Find the last button line in the section
            int lastButtonInSection = sectionHeaderIndex;
            for (int i = sectionHeaderIndex + 1; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.StartsWith("#")) break; // Next section
                if (!string.IsNullOrEmpty(trimmed))
                    lastButtonInSection = i;
            }

            string newLine;
            if (action.IsKey)
            {
                newLine = $"{action.Name},{action.Tooltip},{action.Channel},KEY,{action.KeyString},0,";
            }
            else
            {
                newLine = $"{action.Name},{action.Tooltip},{action.Channel},{(action.IsNote ? "Note" : "CC")},{action.Value},{action.MouseDownValue},{(action.MouseUpValue.HasValue ? action.MouseUpValue.Value.ToString() : "")}";
            }
            lines.Insert(lastButtonInSection + 1, newLine);

            File.WriteAllLines(lastButtonFile, lines);
        }
        private void EditButtonFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(lastButtonFile) && File.Exists(lastButtonFile))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = lastButtonFile,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        Properties.Resources.DialogButtonFileNotFound,
                        Properties.Resources.DialogError,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.DialogCouldNotOpenButtonFile, ex.Message),
                    Properties.Resources.DialogError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ShowTransparencyDialog()
        {
            int currentPercent = (int)Math.Round(this.Opacity * 100);
            using (Form dlg = new Form())
            {
                dlg.Text = Properties.Resources.DialogWindowTransparency;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.Manual;
                int margin = 12;
                int bottomMargin = 10;
                int buttonWidth = 80;
                int buttonHeight = 25;
                int valLblWidth = 60;
                int valLblHeight = 25;
                int trackBarHeight = 45;
                int gap = 6;

                // Calculate dialog height to fit all controls
                int dlgHeight = margin + trackBarHeight + gap + valLblHeight + gap + buttonHeight + bottomMargin;
                dlg.ClientSize = new Size(260, 110);

                // TrackBar at the top
                TrackBar bar = new TrackBar
                {
                    Minimum = 10,
                    Maximum = 100,
                    Value = 100 + 10 - currentPercent,
                    TickFrequency = 10,
                    Left = margin,
                    Top = margin,
                    Width = dlg.ClientSize.Width - margin * 2,
                    Height = trackBarHeight
                };

                // OK/Cancel buttons locked to the bottom with 6px margin
                int buttonY = dlg.ClientSize.Height - buttonHeight - bottomMargin;

                Button ok = new Button
                {
                    Text = Properties.Resources.DialogOK,
                    DialogResult = DialogResult.OK,
                    Left = margin,
                    Top = buttonY,
                    Width = buttonWidth,
                    Height = buttonHeight
                };
                Button cancel = new Button
                {
                    Text = Properties.Resources.DialogCancelButton,
                    DialogResult = DialogResult.Cancel,
                    Left = dlg.ClientSize.Width - buttonWidth - margin,
                    Top = buttonY,
                    Width = buttonWidth,
                    Height = buttonHeight
                };

                // Value label just above the buttons, centered between their centers
                int flippedValue = bar.Maximum + bar.Minimum - bar.Value;
                int okCenter = ok.Left + ok.Width / 2;
                int cancelCenter = cancel.Left + cancel.Width / 2;
                int centerBetween = (okCenter + cancelCenter) / 2;
                int valLblTop = buttonY + (buttonHeight - valLblHeight) / 2;
                Label valLbl = new Label
                {
                    Text = $"{100 - flippedValue}%",
                    Width = valLblWidth,
                    Height = valLblHeight,
                    Top = valLblTop,
                    Left = centerBetween - valLblWidth / 2,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                bar.ValueChanged += (s, e) =>
                {
                    int flipped = bar.Maximum + bar.Minimum - bar.Value;
                    this.Opacity = flipped / 100.0;
                    valLbl.Text = $"{100 - flipped}%";
                };

                dlg.Controls.Add(bar);
                dlg.Controls.Add(valLbl);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                // Center over main form and move up by 100 pixels
                Form1 parent = this;
                Point parentScreen = parent.PointToScreen(Point.Empty);
                int dlgX = parentScreen.X + (parent.Width - dlg.Width) / 2;
                int dlgY = parentScreen.Y + (parent.Height - dlg.Height) / 2 - 200;
                dlgY = Math.Max(dlgY, 0);
                dlg.Location = new Point(Math.Max(dlgX, 0), dlgY);

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    int flipped = bar.Maximum + bar.Minimum - bar.Value;
                    int transparency = 100 - flipped;
                    this.Opacity = flipped / 100.0;
                    WriteIni("Window", "Transparency", transparency.ToString());
                }
                else
                {
                    this.Opacity = currentPercent / 100.0;
                }
            }
        }

        // WinAPI SendInput structures
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private void SendInputKey(Keys key, bool keyDown)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = (ushort)key;
            inputs[0].u.ki.wScan = 0;
            inputs[0].u.ki.dwFlags = keyDown ? 0 : KEYEVENTF_KEYUP;
            inputs[0].u.ki.time = 0;
            inputs[0].u.ki.dwExtraInfo = IntPtr.Zero;

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_MINIMIZEBOX = 0x20000;
                const int WS_MAXIMIZEBOX = 0x10000;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                cp.Style &= ~WS_MINIMIZEBOX;
                cp.Style &= ~WS_MAXIMIZEBOX;
                return cp;
            }
        }

        private static string GetButtonIniFile(string buttonFilePath)
        {
            string dir = Path.GetDirectoryName(buttonFilePath);
            string baseName = Path.GetFileNameWithoutExtension(buttonFilePath);
            return Path.Combine(dir, baseName + ".ini");
        }

        private string ReadIniFromFile(string iniPath, string section, string key, string defaultVal = "")
        {
            if (!File.Exists(iniPath)) return defaultVal;
            string[] iniLines = File.ReadAllLines(iniPath);
            string currentSection = "";
            foreach (string line in iniLines)
            {
                string trimmed = line.Split(';')[0].Trim();
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

        private void LoadPanelSettingsFromIni(string filePath)
        {
            // Stop collapse timer when loading a new panel
            if (collapseCheckTimer != null)
                collapseCheckTimer.Stop();

            string iniPath = GetButtonIniFile(filePath);
            if (!File.Exists(iniPath)) return;

            // --- FIRST: Set panel background color (Background Colour=) ---
            string bgColor = ReadIniFromFile(iniPath, "Buttons", "Background Colour", "");
            expandedPanelBackgroundColor = !string.IsNullOrEmpty(bgColor)
                ? ColorHelper.ParseOrDefault(bgColor, Color.White)
                : Color.White;
            this.BackColor = expandedPanelBackgroundColor;

            // --- BUTTON SETTINGS ---
            buttonWidth = int.TryParse(ReadIniFromFile(iniPath, "Buttons", "Button Width", "100"), out int bw) ? bw : 100;
            buttonHeight = int.TryParse(ReadIniFromFile(iniPath, "Buttons", "Button Height", "22"), out int bh) ? bh : 22;
            buttonGap = int.TryParse(ReadIniFromFile(iniPath, "Buttons", "Button Gap", "0"), out int bg) ? bg : 0;
            buttonFontSize = float.TryParse(ReadIniFromFile(iniPath, "Buttons", "Button Font Size", "9"), out float bfs) ? bfs : 8.25f;
            buttonFontName = ReadIniFromFile(iniPath, "Buttons", "Button Font Name", "Segoe UI");
            effectiveTopMargin = int.TryParse(ReadIniFromFile(iniPath, "Buttons", "Top Margin", TopMargin.ToString()), out int tm) ? tm : TopMargin;
            effectiveLeftMargin = int.TryParse(ReadIniFromFile(iniPath, "Buttons", "Left Margin", LeftMargin.ToString()), out int lm) ? lm : LeftMargin;
            string borderColorStr = ReadIniFromFile(iniPath, "Buttons", "Button Border", "");
            if (!string.IsNullOrWhiteSpace(borderColorStr))
            {
                buttonBorderColor = ColorHelper.ParseOrDefault(borderColorStr, Color.Black);
            }
            else
            {
                buttonBorderColor = null;
            }

            // --- COLLAPSIBLE SETTINGS ---
            // 1. In LoadPanelSettingsFromIni, replace the entire collapse button color logic:
            string collapsibleValue = ReadIniFromFile(iniPath, "Buttons", "Collapsible", "False");
            collapsible = collapsibleValue.Equals("True", StringComparison.OrdinalIgnoreCase);

            // Set collapse button color and design from [UserCollapse] INI section
            string userButtonColorStr = ReadIniFromFile(iniPath, "UserCollapse", "Button Colour", "#FF0000");
            collapsibleButtonColor = ColorHelper.ParseOrDefault(userButtonColorStr, Color.Red);

            // --- NEW: Read DisplayHeaders from [Buttons] section ---
            displayHeaders = ReadIniFromFile(iniPath, "Window", "DisplayHeaders", "True").Equals("True", StringComparison.OrdinalIgnoreCase);

            // --- WINDOW SETTINGS ---
            bool borderless = ReadIniFromFile(iniPath, "Window", "Borderless", "False").Equals("True", StringComparison.OrdinalIgnoreCase);

            int winW = 800;
            int winH = 600;
            int.TryParse(ReadIniFromFile(iniPath, "Window", "Window Width", winW.ToString()), out winW);
            int.TryParse(ReadIniFromFile(iniPath, "Window", "Window Height", winH.ToString()), out winH);

            string winPosStr = ReadIniFromFile(iniPath, "Window", "Window Position", "100,100");
            string[] winPosParts = winPosStr.Split(',');
            Point desiredLocation = new Point(100, 100);
            if (winPosParts.Length == 2 &&
                int.TryParse(winPosParts[0], out int winX) &&
                int.TryParse(winPosParts[1], out int winY))
            {
                desiredLocation = new Point(winX, winY);
            }

            // Load status strip visibility
            statusVisible = ReadIniFromFile(iniPath, "Window", "StatusVisible", "True").Equals("True", StringComparison.OrdinalIgnoreCase);
            ApplyStatusState();

            Rectangle futureBounds = new Rectangle(desiredLocation, new Size(winW, winH));
            bool isOnScreen = Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(futureBounds));
            if (!isOnScreen)
            {
                Rectangle primary = Screen.PrimaryScreen.WorkingArea;
                desiredLocation = new Point(
                    primary.Left + (primary.Width - winW) / 2,
                    primary.Top + (primary.Height - winH) / 2
                );
            }

            // --- CRUCIAL: Hide, change style, set bounds, show ---
            this.SuspendLayout();
            bool borderlessChanged = (this.FormBorderStyle == FormBorderStyle.None) != borderless;
            if (borderlessChanged)
            {
                this.Hide();
                this.FormBorderStyle = borderless ? FormBorderStyle.None : FormBorderStyle.Sizable;
                this.ControlBox = !borderless;
                this.StartPosition = FormStartPosition.Manual;
                titleBarVisible = !borderless;
                this.WindowState = FormWindowState.Normal;
                this.Location = desiredLocation;
                this.ClientSize = new Size(winW, winH);
                this.Show();
                Application.DoEvents();
            }
            else
            {
                this.FormBorderStyle = borderless ? FormBorderStyle.None : FormBorderStyle.Sizable;
                this.ControlBox = !borderless;
                this.StartPosition = FormStartPosition.Manual;
                titleBarVisible = !borderless;
                this.WindowState = FormWindowState.Normal;
                this.Location = desiredLocation;
                this.ClientSize = new Size(winW, winH);
            }
            // Force exact size for borderless forms to avoid WinForms minimum size drift
            if (this.FormBorderStyle == FormBorderStyle.None)
            {
                this.Size = new Size(winW, winH);
            }

            if (collapsible && !titleBarVisible)
            {
                previousWindowSize = new Size(winW, winH);
                previousWindowLocation = desiredLocation;
                this.Size = previousWindowSize.Value;
                this.Location = previousWindowLocation.Value;
            }
            else
            {
                previousWindowSize = null;
                previousWindowLocation = null;
            }

            // ReApply Topmost
            this.TopMost = ReadBool("Window", "AOT", false); // Re-apply AOT after border style change
            this.ResumeLayout(true);

            if (midiFromHost != null)
                midiFromHost.UpdateDialogLocation(desiredLocation);

            // Set minimum size to allow very small panels (regression fix)
            this.MinimumSize = new Size(20, 20);

            // --- NOW call UpdateCollapseState ---
            UpdateCollapseState();
            if (buttonPanel != null)
            {
                buttonPanel.Size = this.ClientSize;
                buttonPanel.PerformLayout();
                buttonPanel.Refresh();
                Application.DoEvents();
            }
        }

        private void SyncMenuItemsWithSettings(string filePath)
        {
            // --- This method ONLY syncs menu items with the current settings ---
            string iniPath = GetButtonIniFile(filePath);
            if (!File.Exists(iniPath)) return;

            // --- COLLAPSIBLE MENU ITEMS ---
            string collapsibleValue = ReadIniFromFile(iniPath, "Buttons", "Collapsible", "False");
            bool collapsible = collapsibleValue.Equals("True", StringComparison.OrdinalIgnoreCase)
                || collapsibleValue.Equals("User", StringComparison.OrdinalIgnoreCase);
            bool useUserCollapseButtonSettings = collapsibleValue.Equals("User", StringComparison.OrdinalIgnoreCase);
            bool titleBarVisible = !ReadIniFromFile(iniPath, "Window", "Borderless", "False").Equals("True", StringComparison.OrdinalIgnoreCase);

            if (collapsibleItem != null)
            {
                collapsibleItem.Checked = collapsible && !titleBarVisible;
                collapsibleItem.Enabled = true;
                collapsibleItem.Text = titleBarVisible
                    ? Properties.Resources.MenuMakeCollapsibleHidesTitleBar
                    : Properties.Resources.MenuMakeCollapsible;
            }

            // --- SYNC MENU ITEM ---
            if (mainContextMenu != null)
            {
                ToolStripMenuItem titleBarItem = mainContextMenu.Items
                    .OfType<ToolStripMenuItem>()
                    .FirstOrDefault(item => item.Text.StartsWith("Show Title Bar"));
                if (titleBarItem != null)
                {
                    titleBarItem.Checked = titleBarVisible;
                }
                if (trayTitleBarItem != null)
                {
                    trayTitleBarItem.Checked = titleBarVisible;
                }
                ToolStripMenuItem statusItem = mainContextMenu.Items
                    .OfType<ToolStripMenuItem>()
                    .FirstOrDefault(item => item.Text.StartsWith("Show Status"));
                if (statusItem != null)
                {
                    // Read statusVisible from ini
                    bool statusVisible = ReadIniFromFile(iniPath, "Window", "StatusVisible", "True").Equals("True", StringComparison.OrdinalIgnoreCase);
                    statusItem.Checked = statusVisible;
                }
                // --- Sync Auto Switch menu item ---
                ToolStripMenuItem autoSwitchItem = mainContextMenu.Items
                    .OfType<ToolStripMenuItem>()
                    .FirstOrDefault(item => item.Text == "Auto Switch");
                if (autoSwitchItem != null)
                {
                    bool autoSwitchEnabled = ReadBool("Switching", "AutoSwitch", false);
                    autoSwitchItem.Checked = autoSwitchEnabled;
                }
                // --- End Sync Auto Switch menu item ---
            }
        }
        private void SaveButtonIniFile(string buttonFilePath)
        {
            string iniPath = GetButtonIniFile(buttonFilePath);
            List<string> lines = new List<string>();

            // Preserve all sections except [Window]
            if (File.Exists(iniPath))
            {
                string[] existingLines = File.ReadAllLines(iniPath);
                bool inWindowSection = false;
                foreach (string line in existingLines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("[Window]", StringComparison.OrdinalIgnoreCase))
                    {
                        inWindowSection = true;
                        continue; // skip old [Window] section
                    }
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]") && !trimmed.Equals("[Window]", StringComparison.OrdinalIgnoreCase))
                    {
                        inWindowSection = false;
                    }
                    if (!inWindowSection)
                    {
                        lines.Add(line);
                    }
                }
            }

            // Write new [Window] section
            lines.Add("[Window]");
            // Use previousWindowSize/Location if available (i.e., last open state), otherwise current
            // Size sizeToSave = previousWindowSize ?? this.Size;
            Size sizeToSave = previousWindowSize ?? this.ClientSize;
            Point locationToSave = previousWindowLocation ?? this.Location;
            if (this.FormBorderStyle == FormBorderStyle.None)
            {
                lines.Add($"Window Width={sizeToSave.Width}");
                lines.Add($"Window Height={sizeToSave.Height}");
                lines.Add($"Window Position={locationToSave.X},{locationToSave.Y}");
                lines.Add($"Borderless=True");
            }
            else
            {
                lines.Add($"Window Width={sizeToSave.Width}");
                lines.Add($"Window Height={sizeToSave.Height}");
                lines.Add($"Window Position={locationToSave.X},{locationToSave.Y}");
                lines.Add($"Borderless=False");
            }
            lines.Add($"StatusVisible={statusVisible}");
            lines.Add($"DisplayHeaders={(displayHeaders ? "True" : "False")}");

            File.WriteAllLines(iniPath, lines);
        }

        private Font GetButtonFont()
        {
            try
            {
                Font font = new Font(buttonFontName, buttonFontSize, FontStyle.Regular);
                return font;
            }
            catch
            {
                // Fallback if font not found
                return new Font("Segoe UI", buttonFontSize, FontStyle.Regular);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (minimizeToTray && this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }
        private void ForceScrollRedraw()
        {
            // WinForms bug: toggling border style can break scroll area until a resize.
            // This hack triggers a layout recalculation to fix scroll gaps.
            Size origSize = this.Size;
            this.Size = new Size(origSize.Width, origSize.Height + 1);
            this.Size = origSize;
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            if (buttonPanel != null)
            {
                // Force scroll to top
                // buttonPanel.AutoScrollPosition = new Point(0, 0);

                // Force layout and redraw to ensure no gap
                buttonPanel.PerformLayout();
                buttonPanel.Refresh();
            }

            // Always update previousWindowLocation to the current position
            previousWindowLocation = this.Location;
        }

        protected override void WndProc(ref Message m)
        {
            if (this.FormBorderStyle == FormBorderStyle.None && m.Msg == WM_NCHITTEST)
            {
                // Prevent resizing when collapsed
                if (collapsible && !titleBarVisible && collapseButton != null && collapseButton.Visible)
                {
                    // When collapsed, prevent resizing and dragging from anywhere (including borders and button)
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }

                // ... existing resize logic ...
                const int resizeArea = 4;
                Point cursor2 = PointToClient(Cursor.Position);

                if (cursor2.X <= resizeArea && cursor2.Y <= resizeArea)
                    m.Result = (IntPtr)HTTOPLEFT;
                else if (cursor2.X >= this.ClientSize.Width - resizeArea && cursor2.Y <= resizeArea)
                    m.Result = (IntPtr)HTTOPRIGHT;
                else if (cursor2.X <= resizeArea && cursor2.Y >= this.ClientSize.Height - resizeArea)
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                else if (cursor2.X >= this.ClientSize.Width - resizeArea && cursor2.Y >= this.ClientSize.Height - resizeArea)
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                else if (cursor2.X <= resizeArea)
                    m.Result = (IntPtr)HTLEFT;
                else if (cursor2.X >= this.ClientSize.Width - resizeArea)
                    m.Result = (IntPtr)HTRIGHT;
                else if (cursor2.Y >= this.ClientSize.Height - resizeArea)
                    m.Result = (IntPtr)HTBOTTOM;
                else
                    base.WndProc(ref m);
                return;
            }
            base.WndProc(ref m);
        }

        private class ResizePanel : Panel
        {
            private const int WM_NCHITTEST = 0x84;
            private const int HTTRANSPARENT = -1;

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    Point cursor = this.PointToClient(Cursor.Position);
                    int resizeArea = 4; // Minimum area (in pixels) always reserved for resizing

                    // Always reserve the edge/corner for resizing, even if scrollbars are present
                    bool onLeft = cursor.X <= resizeArea;
                    bool onRight = cursor.X >= this.Width - resizeArea;
                    bool onTop = cursor.Y <= resizeArea;
                    bool onBottom = cursor.Y >= this.Height - resizeArea;

                    if (onLeft || onRight || onTop || onBottom)
                    {
                        m.Result = (IntPtr)HTTRANSPARENT; // Let the parent (Form) handle resizing
                        return;
                    }
                }
                base.WndProc(ref m);
            }
        }

        private void UpdateFormTitle()
        {
            string fileNameNoExt = Path.GetFileNameWithoutExtension(lastButtonFile);
            this.Text = string.Format(Properties.Resources.FormTitle, Form1.AppVersion, fileNameNoExt);
            if (trayIcon != null)
            {
                string trayText = string.Format(Properties.Resources.FormTitle, Form1.AppVersion, fileNameNoExt);
                if (trayText.Length > 63)
                    trayText = trayText.Substring(0, 63);
                trayIcon.Text = trayText;
            }
        }
    }
}

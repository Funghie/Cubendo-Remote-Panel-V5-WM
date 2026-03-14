// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using NAudio.Midi;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cubendo_Remote_Panel
{
    public class MidiMessageEventArgs : EventArgs
    {
        public int Channel { get; }
        public bool IsNote { get; }
        public int Value { get; }
        public int Data { get; }

        public MidiMessageEventArgs(int channel, bool isNote, int value, int data)
        {
            Channel = channel;
            IsNote = isNote;
            Value = value;
            Data = data;
        }
    }

    internal class Midifromhost
    {
        public event EventHandler<MidiMessageEventArgs> MidiMessageReceived;

        private MidiTestDialog testDialog;
        private MidiIn midiIn;
        private string midiPortName;
        private Point dialogLocation;
        private bool isSwitchingDevices = false;

        public Midifromhost(int midiInDeviceId = 0, string midiPortName = "", Point dialogLocation = default(Point))
        {
            this.midiPortName = midiPortName;
            this.dialogLocation = dialogLocation;
            testDialog = new MidiTestDialog(midiPortName, dialogLocation);

            // Open the initial device
            OpenMidiDevice(midiInDeviceId);
        }

        // New method to switch MIDI devices without recreating the whole object
        public bool SwitchToDevice(int midiInDeviceId, string newPortName)
        {
            if (isSwitchingDevices) return false;

            isSwitchingDevices = true;
            try
            {
                // Update port name for dialog
                midiPortName = newPortName;
                if (testDialog != null && !testDialog.IsDisposed)
                {
                    testDialog.UpdatePortName(newPortName);
                }

                // Close current device
                CloseMidiDevice();

                // Crucial: wait for device to be released
                System.Threading.Thread.Sleep(500);

                // Force garbage collection to help release resources
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Try to open the new device
                return OpenMidiDevice(midiInDeviceId);
            }
            finally
            {
                isSwitchingDevices = false;
            }
        }

        // Private helper to open a MIDI device
        private bool OpenMidiDevice(int midiInDeviceId)
        {
            if (!Cubendo_Remote_Panel.Form1.midiEnabled)
                return false;

            // Ensure previous device is closed
            CloseMidiDevice();

            try
            {
                // Add a retry mechanism for device opening
                int maxRetries = 3;
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        // Check if the device is valid
                        if (midiInDeviceId >= 0 && midiInDeviceId < MidiIn.NumberOfDevices)
                        {
                            midiIn = new MidiIn(midiInDeviceId);
                            midiIn.MessageReceived += MidiIn_MessageReceived;
                            midiIn.Start();
                            return true;
                        }
                        else if (MidiIn.NumberOfDevices > 0)
                        {
                            // Fall back to first device if requested device is not available
                            midiIn = new MidiIn(0);
                            midiIn.MessageReceived += MidiIn_MessageReceived;
                            midiIn.Start();
                            return true;
                        }

                        // No devices available
                        return false;
                    }
                    catch (NAudio.MmException ex) when ((int)ex.Result == 4 && attempt < maxRetries)
                    {
                        // If device is allocated and we have more attempts, wait and retry
                        System.Threading.Thread.Sleep(500 * attempt);
                        continue;
                    }
                    catch (NAudio.MmException ex)
                    {
                        // Final attempt failed or other error
                        string errorDetails = $"Could not open MIDI input device (ID {midiInDeviceId}, '{midiPortName}'):\n{ex.Message}";
                        if ((int)ex.Result == 4)
                            errorDetails += "\n\nThis device is already in use by another application.";

                        // Only show message box on final attempt
                        if (attempt == maxRetries)
                        {
                            MessageBox.Show(
                                errorDetails,
                                "MIDI Device Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        if (attempt == maxRetries)
                        {
                            MessageBox.Show(
                                $"Unexpected error opening MIDI device:\n{ex.Message}",
                                "MIDI Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                        }
                        return false;
                    }
                }

                return false; // All retries failed
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Private helper to close MIDI device
        private void CloseMidiDevice()
        {
            if (midiIn != null)
            {
                try
                {
                    midiIn.Stop();
                    midiIn.MessageReceived -= MidiIn_MessageReceived;
                    midiIn.Dispose();
                }
                catch (Exception)
                {
                    // Ignore errors during cleanup
                }
                finally
                {
                    midiIn = null;
                }
            }
        }

        public void UpdateDialogLocation(Point newLocation)
        {
            dialogLocation = newLocation;
            if (testDialog != null && !testDialog.IsDisposed)
            {
                testDialog.Location = newLocation;
            }
        }

        // Show the test dialog (call this from Form1 for testing)
        public void ShowTestDialog(Form owner = null)
        {
            if (testDialog == null || testDialog.IsDisposed)
            {
                testDialog = new MidiTestDialog(midiPortName, dialogLocation);
            }
            testDialog.StartPosition = FormStartPosition.Manual;
            testDialog.Location = dialogLocation;
            if (testDialog.Visible)
            {
                testDialog.BringToFront();
                testDialog.Activate();
            }
            else
            {
                if (owner != null)
                    testDialog.Show(owner); // Show as owned dialog
                else
                    testDialog.Show();
                testDialog.Activate();
            }
        }

        private void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            // Only process Channel messages (NoteOn, NoteOff, Controller)
            int raw = e.RawMessage;
            int status = raw & 0xF0;
            int channel = Math.Max(1, Math.Min(16, (raw & 0x0F) + 1));
            int data1 = Math.Max(0, Math.Min(127, (raw >> 8) & 0x7F));
            int data2 = Math.Max(0, Math.Min(127, (raw >> 16) & 0x7F));
            bool isNote = status == 0x90 || status == 0x80;
            bool isController = status == 0xB0;

            if (isNote || isController)
            {
                int value = data1;
                int data = data2;
                OnMidiMessageReceived(channel, isNote, value, data);
            }
        }

        // Call this method when a MIDI message is received from the DAW
        protected void OnMidiMessageReceived(int channel, bool isNote, int value, int data)
        {
            try
            {
                MidiMessageReceived?.Invoke(this, new MidiMessageEventArgs(channel, isNote, value, data));
                testDialog?.LogMessage(channel, isNote, value, data);
            }
            catch (Exception)
            {
                // Silently ignore errors during event handling
            }
        }

        // Example: Simulate receiving a MIDI message
        public void SimulateReceive(int channel, bool isNote, int value, int data)
        {
            OnMidiMessageReceived(channel, isNote, value, data);
        }

        // Dispose MIDI input device when done
        public void Dispose()
        {
            CloseMidiDevice();

            if (testDialog != null)
            {
                try
                {
                    if (!testDialog.IsDisposed)
                        testDialog.Dispose();
                }
                catch
                {
                    // Ignore errors
                }
                finally
                {
                    testDialog = null;
                }
            }

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    // Simple dialog to display MIDI messages
    internal class MidiTestDialog : Form
    {
        private ListBox listBox;
        private ContextMenuStrip contextMenu;

        public MidiTestDialog(string midiPortName, Point location)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Set the dialog title to include the MIDI port name
            this.Text = $"Incoming MIDI from {midiPortName}";
            this.Width = 400;
            this.Height = 300;

            // Remove the label strip, only show the ListBox
            listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10)
            };
            this.Controls.Add(listBox);

            contextMenu = new ContextMenuStrip();
            ToolStripMenuItem clearItem = new ToolStripMenuItem("Clear Messages");
            clearItem.Click += (s, e) => listBox.Items.Clear();
            contextMenu.Items.Add(clearItem);
            listBox.ContextMenuStrip = contextMenu;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = location;
        }

        public void UpdatePortName(string midiPortName)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new Action(() => UpdatePortNameInternal(midiPortName)));
                }
                catch { /* Ignore invoke errors */ }
            }
            else
            {
                UpdatePortNameInternal(midiPortName);
            }
        }

        private void UpdatePortNameInternal(string midiPortName)
        {
            if (this.IsDisposed) return;
            this.Text = $"Incoming MIDI from {midiPortName}";
        }

        public void LogMessage(int channel, bool isNote, int value, int data)
        {
            if (this.IsDisposed) return;

            string msg = $"Ch:{channel} {(isNote ? "Note" : "CC")} Val:{value} Data:{data}  [{DateTime.Now:HH:mm:ss}]";
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new Action(() =>
                    {
                        try
                        {
                            if (!this.IsDisposed && listBox != null)
                            {
                                listBox.Items.Add(msg);
                                // Auto-scroll to the latest message
                                listBox.TopIndex = listBox.Items.Count - 1;

                                // Keep last 100 messages
                                if (listBox.Items.Count > 100)
                                    listBox.Items.RemoveAt(0);
                            }
                        }
                        catch
                        {
                            // Ignore errors if dialog is being disposed
                        }
                    }));
                }
                catch
                {
                    // Ignore invoke errors if the form is closing
                }
            }
            else
            {
                try
                {
                    if (!this.IsDisposed && listBox != null)
                    {
                        listBox.Items.Add(msg);
                        // Auto-scroll to the latest message
                        listBox.TopIndex = listBox.Items.Count - 1;

                        // Keep last 100 messages
                        if (listBox.Items.Count > 100)
                            listBox.Items.RemoveAt(0);
                    }
                }
                catch
                {
                    // Ignore errors if dialog is being disposed
                }
            }
        }
    }
}

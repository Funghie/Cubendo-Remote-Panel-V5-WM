// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Cubendo_Remote_Panel
{
    public partial class SplashForm : Form
    {
        private PictureBox pictureBox;

        public SplashForm()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };
            this.Controls.Add(pictureBox);

            // Load splash image and set form size before showing
            string splashPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Phil Pendlebury", "CN Remote", "Resources", "startup.png");

            if (File.Exists(splashPath))
            {
                try
                {
                    using (Image img = Image.FromFile(splashPath))
                    {
                        Bitmap bmp = new Bitmap(img.Width, img.Height);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(img, 0, 0, img.Width, img.Height);

                            using (Font font = new Font("Segoe UI", 8, FontStyle.Regular))
                            using (Brush brush = new SolidBrush(Color.White))
                            using (Brush shadow = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                            {
                                string versionText = "Version: " + Form1.AppVersion;
                                SizeF textSize = g.MeasureString(versionText, font);
                                float x = 530 - textSize.Width;
                                float y = 270 - textSize.Height;
                                g.DrawString(versionText, font, shadow, x + 2, y + 2);
                                g.DrawString(versionText, font, brush, x, y);
                            }
                        }
                        pictureBox.Image = bmp;
                        this.ClientSize = bmp.Size; // Set size here
                    }
                }
                catch
                {
                    // fallback: just show blank
                }
            }

            pictureBox.Click += (s, e) => this.Close();
            this.Click += (s, e) => this.Close();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // No need to set ClientSize here
        }
    }
}

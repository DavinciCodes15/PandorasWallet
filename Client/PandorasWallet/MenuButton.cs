﻿//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public class MenuButton : Button
    {
        private ContextMenuStrip FcontextButtonMenuStrip;
        private IContainer components;

        [DefaultValue(null), Browsable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ContextMenuStrip Menu { get; set; }

        [DefaultValue(20), Browsable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int SplitWidth { get; set; }

        public MenuButton()
        {
            SplitWidth = 20;
            FcontextButtonMenuStrip = new ContextMenuStrip();
            Menu = FcontextButtonMenuStrip;
        }

        public void AddMenuItem(string aItem)
        {
            Menu.Items.Add(aItem);
        }

        public void AssingOnClickEvent(EventHandler aHandler, int aItemIndex)
        {
            if (aHandler != null)
            {
                Menu.Items[aItemIndex].Click += aHandler;
            }
        }

        public void RevokeOnClickEvent(EventHandler aHandler, int aItemIndex)
        {
            if (aHandler != null)
            {
                Menu.Items[aItemIndex].Click -= aHandler;
            }
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            Rectangle splitRect = new Rectangle(Width - SplitWidth, 0, SplitWidth, Height);

            // Figure out if the button click was on the button itself or the menu split
            if (Menu != null &&
                mevent.Button == MouseButtons.Left &&
                splitRect.Contains(mevent.Location))
            {
                Menu.Show(this, 0, Height);    // Shows menu under button
                                               //Menu.Show(this, mevent.Location); // Shows menu at click location
            }
            else
            {
                base.OnMouseDown(mevent);
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            if (Menu != null && SplitWidth > 0)
            {
                // Draw the arrow glyph on the right side of the button
                int arrowX = ClientRectangle.Width - 14;
                int arrowY = ClientRectangle.Height / 2 - 1;

                Brush arrowBrush = Enabled ? SystemBrushes.ControlText : SystemBrushes.ButtonShadow;
                Point[] arrows = new[] { new Point(arrowX, arrowY), new Point(arrowX + 7, arrowY), new Point(arrowX + 3, arrowY + 4) };
                pevent.Graphics.FillPolygon(arrowBrush, arrows);

                // Draw a dashed separator on the left of the arrow
                int lineX = ClientRectangle.Width - SplitWidth;
                int lineYFrom = arrowY - 4;
                int lineYTo = arrowY + 8;
                using (Pen separatorPen = new Pen(Brushes.DarkGray) { DashStyle = DashStyle.Dot })
                {
                    pevent.Graphics.DrawLine(separatorPen, lineX, lineYFrom, lineX, lineYTo);
                }
            }
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            FcontextButtonMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            SuspendLayout();
            //
            // contextButtonMenuStrip
            //
            FcontextButtonMenuStrip.Name = "contextButtonMenuStrip";
            FcontextButtonMenuStrip.Size = new System.Drawing.Size(61, 4);
            ResumeLayout(false);
        }
    }
}
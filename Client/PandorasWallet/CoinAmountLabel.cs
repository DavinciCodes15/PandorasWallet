using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public partial class CoinAmountLabel : Label
    {
        private readonly int FControlDefaultWidth;
        private string FStringValue;
        private int FStringWidth;
        private string FCroppedStringValue;

        public CoinAmountLabel()
        {
            InitializeComponent();
            FControlDefaultWidth = this.Size.Width;
        }

        public decimal Amount { get; private set; }

        public void SetAmount(decimal aValue, ushort aPrecision)
        {
            Amount = aValue;
            FStringValue = FormatAmount(aValue, aPrecision);
            var lSize = TextRenderer.MeasureText(FStringValue, this.Font);
            if (lSize.Width > this.Width)
            {
                string lCroppedString = string.Empty;
                for (var lit = 1; lit <= FStringValue.Length; lit++)
                {
                    lCroppedString = string.Concat(FStringValue.Remove(FStringValue.Length - lit, lit), "...");
                    var lCroppedSize = TextRenderer.MeasureText(lCroppedString, this.Font);
                    if (lCroppedSize.Width <= this.Width)
                        break;
                }
                this.Text = FCroppedStringValue = lCroppedString;
                FStringWidth = lSize.Width;
            }
            else
            {
                this.Text = FStringValue;
                FCroppedStringValue = string.Empty;
            }
        }

        private string FormatAmount(decimal aAmount, ushort aPrecision)
        {
            string lSharpPattern = aPrecision > 3 ? new string('#', aPrecision - 3) : string.Empty;
            string lFormatPattern = $"{{0:0.000{lSharpPattern}}}";
            return string.Format(lFormatPattern, aAmount);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (!string.IsNullOrEmpty(FCroppedStringValue))
            {
                this.Text = FStringValue;
                this.BackColor = Color.LightYellow;
                this.BorderStyle = BorderStyle.FixedSingle;
                this.Size = new Size(FStringWidth, this.Size.Height);
                var lXLocationDiff = FStringWidth - FControlDefaultWidth;
                this.Location = new Point(this.Location.X - lXLocationDiff, this.Location.Y);
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!string.IsNullOrEmpty(FCroppedStringValue))
            {
                this.Text = FCroppedStringValue;
                this.BackColor = Color.Transparent;
                this.BorderStyle = BorderStyle.None;
                this.Size = new Size(FControlDefaultWidth, this.Size.Height);
                var lXLocationDiff = FStringWidth - FControlDefaultWidth;
                this.Location = new Point(this.Location.X + lXLocationDiff, this.Location.Y);
            }
            base.OnMouseLeave(e);
        }
    }
}
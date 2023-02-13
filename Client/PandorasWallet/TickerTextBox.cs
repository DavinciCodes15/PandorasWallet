using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public partial class TickerTextBox : UserControl
    {
        private bool FOptionsMenuEnabled;
        private int FPrecision = 8;

        public TickerTextBox()
        {
            InitializeComponent();
            AdjustTextBoxMargin();
            SetPlaceHolder();
        }

        [Browsable(true)]
        public event Action OnAmountChanged;

        [Description("Currency ticker to be displayed next to amount"), Category("Data")]
        public string CurrencyTicker
        {
            get => lblTicker.Text;
            set
            {
                lblTicker.Text = value.ToUpper();
                AdjustTextBoxMargin();
            }
        }

        [Description("Enable or disable dropdown menu"), Category("Appearance")]
        public bool UseOptionsMenu { get => FOptionsMenuEnabled; set => FOptionsMenuEnabled = pictureDownArrow.Visible = value; }

        private void SetPlaceHolder()
        {
            StringBuilder lPlaceHolder = new StringBuilder("0");
            if (FPrecision > 0)
            {
                lPlaceHolder.Append(".");
                for (var i = 0; i < FPrecision; i++)
                    lPlaceHolder.Append(0);
            }
            txtBoxAmount.Text = lPlaceHolder.ToString();
        }

        private void AdjustTextBoxMargin()
        {
            tableLayoutPanelTicker.Visible = !string.IsNullOrEmpty(CurrencyTicker);
            paneltxtbox.Padding = new Padding
            {
                Right = string.IsNullOrEmpty(CurrencyTicker) ? 3 : tableLayoutPanelTicker.Size.Width + 5,
                Bottom = 3,
                Left = 3,
                Top = 3
            };
            txtBoxAmount.Refresh();
        }

        private void pictureDownArrow_Click(object sender, EventArgs e)
        {
            contextMenuDownArrow.Show(pictureDownArrow, new Point(0, 19));
            pictureDownArrow.Enabled = false;
        }

        private void contextMenuDownArrow_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            pictureDownArrow.Enabled = true;
        }

        [Description("Enable or disable dropdown menu"), Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ToolStripItemCollection MenuCollection
        {
            get => contextMenuDownArrow.Items;
            set
            {
                if (value == null) return;
                contextMenuDownArrow.Items.Clear();
                contextMenuDownArrow.Items.AddRange(value);
            }
        }

        [Description("Amount decimal precision"), Category("Appearance")]
        public uint Precision { get => Convert.ToUInt32(FPrecision); set => FPrecision = Convert.ToInt32(value); }

        [Description("Amount shown"), Category("Data")]
        private decimal FAmount;

        public decimal Amount
        {
            get
            {
                if (string.IsNullOrEmpty(txtBoxAmount.Text))
                    FAmount = 0;
                else if (Decimal.TryParse(txtBoxAmount.Text, out decimal lValue))
                    FAmount = lValue;
                return Math.Round(FAmount, FPrecision);
            }
            set => txtBoxAmount.Text = String.Format($"{{0:F{FPrecision}}}", value);
        }

        private void txtBoxAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
                || ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1));
            if (e.Handled && string.IsNullOrWhiteSpace(txtBoxAmount.Text))
                (sender as TextBox).Text = "0";
            if (!e.Handled && txtBoxAmount.Text == "0")
            {
                e.Handled = true;
                var lTypedChar = e.KeyChar.ToString();
                (sender as TextBox).Text = e.KeyChar != '.' ? lTypedChar : "0.";
                (sender as TextBox).SelectionStart = (sender as TextBox).Text.Length;
            }
        }

        private void contextMenuDownArrow_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            decimal lPercentage = Convert.ToDecimal(e.ClickedItem.Tag);
            Amount = decimal.Round(Amount * lPercentage, FPrecision);
        }

        private void contextMenuDownArrow_Opening(object sender, CancelEventArgs e)
        {
            var lPercentages = new int[] { 1, 5, 10, 25, 50, 75 };
            foreach (var lPercentage in lPercentages)
            {
                var lMenuItem = contextMenuDownArrow.Items[$"toolStripMenuItem{lPercentage}p"];
                var lNewPrice = decimal.Round(Amount * Convert.ToDecimal(lMenuItem.Tag), FPrecision);
                lMenuItem.Text = $"{lPercentage}% ({lNewPrice})";
                lMenuItem.Enabled = lNewPrice > 0;
            }
            OnAmountChanged?.Invoke();
        }

        private void paneltxtbox_EnabledChanged(object sender, EventArgs e)
        {
            txtBoxAmount.Visible = Enabled;
            lblTicker.Visible = Enabled;
            paneltxtbox.BackColor = tableLayoutPanelTicker.BackColor = Enabled ? Color.White : Color.FromArgb(240, 240, 240);
            var lBorderStyle = Enabled ? BorderStyle.FixedSingle : BorderStyle.None;
            paneltxtbox.BorderStyle = lBorderStyle;
            pictureDownArrow.Visible = Enabled && UseOptionsMenu;
            AdjustTextBoxMargin();
        }

        private string FPreviousValue = "0";

        private void txtBoxAmount_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace((sender as TextBox).Text))
            {
                (sender as TextBox).Text = "0";
            }
            var lAmountInputRegex = $"^([0-9]*[.]{{0,{(FPrecision > 0 ? "1" : "0")}}})([0-9]{{0,{FPrecision}}})$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(lAmountInputRegex);
            if (regex.IsMatch((sender as TextBox).Text))
            {
                FPreviousValue = (sender as TextBox).Text;
                OnAmountChanged?.Invoke();
            }
            else
            {
                (sender as TextBox).Text = FPreviousValue;
                (sender as TextBox).SelectionStart = (sender as TextBox).Text.Length;
            }
        }

        //private void txtBoxAmount_TextChanged(object sender, EventArgs e)
        //{
        //    var lAmountInputRegex = $"^([0-9]*[.]{{0,{(FPrecision > 0 ? "1" : "0")}}})([0-9]{{0,{FPrecision}}})$";
        //    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(lAmountInputRegex);
        //    if (!regex.IsMatch((sender as TextBox).Text) || Amount == FPreviousValue)
        //        Amount = FPreviousValue;
        //    else
        //    {
        //        FPreviousValue = Amount;
        //        OnAmountChanged?.Invoke();
        //    }
        //}

        //private void txtBoxAmount_Leave(object sender, EventArgs e)
        //{
        //    if (FPreviousValue != (sender as TextBox).Text)
        //    {
        //        FPreviousValue = Amount;
        //        OnAmountChanged?.Invoke();
        //    }
        //}
    }
}
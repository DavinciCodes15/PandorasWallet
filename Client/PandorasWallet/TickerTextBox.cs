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
        public TickerTextBox()
        {
            InitializeComponent();            
            AdjustTextBoxMargin();
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

        private void AdjustTextBoxMargin()
        {
            tableLayoutPanelTicker.Visible = !string.IsNullOrEmpty(CurrencyTicker);
            paneltxtbox.Padding = new Padding 
            {
                Right = string.IsNullOrEmpty(CurrencyTicker)? 3 : tableLayoutPanelTicker.Size.Width + 5,
                Bottom = 3,
                Left = 3, 
                Top = 3 
            };
            txtBoxAmount.Refresh();
        }

        private void pictureDownArrow_Click(object sender, EventArgs e)
        {
            contextMenuDownArrow.Show(pictureDownArrow, new Point(0,19));
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
        private int FPrecision = 8;
        [Description("Amount decimal precision"), Category("Appearance")]
        public uint Precision { get => Convert.ToUInt32(FPrecision); set => FPrecision = Convert.ToInt32((value > 0)? value : 1); }

        [Description("Amount shown"), Category("Data")]
        public decimal Amount { get => string.IsNullOrEmpty(txtBoxAmount.Text) ? 0 : Convert.ToDecimal(txtBoxAmount.Text); set => txtBoxAmount.Text = value.ToString(); }

        private void txtBoxAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBoxAmount.Text))
            {
                (sender as TextBox).Text = "0";
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
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
        private decimal FPreviousValue;
        private void txtBoxAmount_TextChanged(object sender, EventArgs e)
        {
            if (FPreviousValue != Amount)
            {
                FPreviousValue = Amount;
                OnAmountChanged?.Invoke();
            }
        }

        private void txtBoxAmount_Leave(object sender, EventArgs e)
        {
            if (FPreviousValue != Amount)
            {
                FPreviousValue = Amount;
                OnAmountChanged?.Invoke();
            }
        }
    }
}

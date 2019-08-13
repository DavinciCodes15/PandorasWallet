using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Utils
{
    public static class ChangeFontUtil
    {
        public static void ChangeDefaultFontFamily(Control aControl)
        {
            aControl.Font = new Font(Properties.Settings.Default.FontFamily, aControl.Font.Size, aControl.Font.Style);

            foreach (Control item in aControl.Controls)
            {
                ChangeDefaultFontFamily(item);
            }
        }
    }
}

using Pandora.Client.PandorasWallet.Dialogs;
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
    public partial class FindUsersToResetWalletDialog : BaseDialog
    {
        public string SelectedUserHash { get; set; }

        public FindUsersToResetWalletDialog(IEnumerable<HashInfoUser> lListHash)
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);

            foreach (var HashUsers in lListHash)
            {               
                cbo_Users.Items.Add(HashUsers);             
            }
            cbo_Users.ValueMember = "Hash";
            cbo_Users.DisplayMember = "Username";
        }
 
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {          
            SelectedUserHash = (string)cbo_Users.SelectedItem.ToString().Substring(0, 32);         
            btnOK.Enabled = true;
        }
      
        
    }
}

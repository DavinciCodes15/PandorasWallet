using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class MessageBoxDialog : BaseDialog
    {
        private TypeMessageBox typeMessage;

        public TypeMessageBox TypeMessage
        {
            get { return typeMessage; }
            set
            {
                typeMessage = value;
                SetStateWindow(typeMessage);
            }
        }


        public string Title
        {
            get => lblTitle.Text;
            set => lblTitle.Text = value;
        }

        public string Description
        {
            get => lblDescription.Text;
            set => lblDescription.Text = value;
        }


        private MessageBoxDialog() : base()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
            Shown += MessageBoxDialog_Shown;
        }

        private void MessageBoxDialog_Shown(object sender, EventArgs e)
        {
            if (lblTitle.Height > 25)
            {
                var lNewSize = (lblTitle.Height - 25);
                this.Height += lNewSize;
                lblDescription.Location = new Point(lblDescription.Location.X, lblDescription.Location.Y + lNewSize);
            }

            if (lblDescription.Height > 17)
            {
                var lNewSize = (lblDescription.Height - 17);
                this.Height += lNewSize;
            }
                                    
        }

        private MessageBoxDialog(string aTitle, string aDescription, TypeMessageBox atypeMessage, IWin32Window aParentWindow) : this()
        {
            Title = aTitle;
            Description = aDescription;
            TypeMessage = atypeMessage;
            ParentWindow = aParentWindow;
        }

        public enum TypeMessageBox
        {
            Info,
            Warning,
            Ask,
            Error
        }

        private void SetStateWindow(TypeMessageBox atypeMessage)
        {
            base.btnCancel.Text = "Cancel";

            switch (atypeMessage)
            {
                case TypeMessageBox.Info:
                    pbIcon.Image = Properties.Resources.Information;
                    Text = "Pandora's Wallet Information Message";
                    break;
                case TypeMessageBox.Warning:
                    pbIcon.Image = Properties.Resources.Warning2;
                    Text = "Pandora's Wallet Warning Message";
                    break;
                case TypeMessageBox.Error:
                    pbIcon.Image = Properties.Resources.Error;
                    Text = "Pandora's Wallet Error Message";
                    base.btnOK.Location = base.btnCancel.Location;
                    base.btnCancel.Visible = false;
                    break;
                case TypeMessageBox.Ask:
                    pbIcon.Image = Properties.Resources.ask;
                    Text = "Pandora's Wallet Question";
                    base.btnCancel.Text = "NO";
                    break;
                default:
                    break;
            }
        }



        #region Static Methods
        public static MessageBoxDialog GetMessageBoxDialog(string aTitle, string aDescription, TypeMessageBox atypeMessage, IWin32Window aParentWindow)
        {
            return new MessageBoxDialog(aTitle, aDescription, atypeMessage, aParentWindow);
        }

        public static MessageBoxDialog GetInfoBoxDialog(string aTitle, string aDescription, IWin32Window aParentWindow = null)
        {
            return GetMessageBoxDialog(aTitle, aDescription, TypeMessageBox.Info, aParentWindow);
        }

        public static MessageBoxDialog GetWarningBoxDialog(string aTitle, string aDescription, IWin32Window aParentWindow = null)
        {
            return GetMessageBoxDialog(aTitle, aDescription, TypeMessageBox.Warning, aParentWindow);
        }

        public static MessageBoxDialog GetAskBoxDialog(string aTitle, string aDescription, IWin32Window aParentWindow = null)
        {
            return GetMessageBoxDialog(aTitle, aDescription, TypeMessageBox.Ask, aParentWindow);
        }

        public static MessageBoxDialog GetErrorBoxDialog(string aTitle, string aDescription, IWin32Window aParentWindow = null)
        {
            return GetMessageBoxDialog(aTitle, aDescription, TypeMessageBox.Error, aParentWindow);
        }

        #endregion
    }
}
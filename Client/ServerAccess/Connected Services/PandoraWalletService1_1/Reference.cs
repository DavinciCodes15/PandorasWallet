﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pandora.Client.ServerAccess.PandoraWalletService1_1 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="PandoraWalletService1_1.PandoraWalletService1_1Soap")]
    public interface PandoraWalletService1_1Soap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Logon", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult Logon(string aEmail, string aUserName, string aPassword);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Logon", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> LogonAsync(string aEmail, string aUserName, string aPassword);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Logon2", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraLogonResult Logon2(string aEmail, string aUserName, string aPassword, string aVersion);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Logon2", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraLogonResult> Logon2Async(string aEmail, string aUserName, string aPassword, string aVersion);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IsConnected", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult IsConnected(string aConnectionId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IsConnected", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> IsConnectedAsync(string aConnectionId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetServerId", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string GetServerId();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetServerId", ReplyAction="*")]
        System.Threading.Tasks.Task<string> GetServerIdAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Logoff", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult Logoff(string aConnectionId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Logoff", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> LogoffAsync(string aConnectionId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetUserStatus", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetUserStatus(string aConnectionId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetUserStatus", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetUserStatusAsync(string aConnectionId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCurrencyList", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetCurrencyList(string aConnectionId, long aStartId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCurrencyList", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetCurrencyListAsync(string aConnectionId, long aStartId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCurrency", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetCurrency(string aConnectionId, long aCurrencyId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCurrency", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetCurrencyAsync(string aConnectionId, long aCurrencyId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCurrencyIcon", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetCurrencyIcon(string aConnectionId, long aCurrencyId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCurrencyIcon", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetCurrencyIconAsync(string aConnectionId, long aCurrencyId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetLastCurrencyStatus", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetLastCurrencyStatus(string aConnectionId, long aCurrencyId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetLastCurrencyStatus", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetLastCurrencyStatusAsync(string aConnectionId, long aCurrencyId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCurrencyStatusList", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetCurrencyStatusList(string aConnectionId, long aCurrencyId, long aStartId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCurrencyStatusList", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetCurrencyStatusListAsync(string aConnectionId, long aCurrencyId, long aStartId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetBlockHeight", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetBlockHeight(string aConnectionId, long aCurrencyId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetBlockHeight", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetBlockHeightAsync(string aConnectionId, long aCurrencyId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AddMonitoredAccount", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult AddMonitoredAccount(string aConnectionId, long aCurrencyId, string aAddress);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AddMonitoredAccount", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> AddMonitoredAccountAsync(string aConnectionId, long aCurrencyId, string aAddress);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetMonitoredAcccounts", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetMonitoredAcccounts(string aConnectionId, long aCurrencyId, long aStartCurrencyAccountId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetMonitoredAcccounts", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetMonitoredAcccountsAsync(string aConnectionId, long aCurrencyId, long aStartCurrencyAccountId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/RemoveMonitoredAccount", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult RemoveMonitoredAccount(string aConnectionId, long aCurrencyAccountId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/RemoveMonitoredAccount", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> RemoveMonitoredAccountAsync(string aConnectionId, long aCurrencyAccountId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetTransactionRecords", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetTransactionRecords(string aConnectionId, long aCurrencyId, long aStartTxRecordId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetTransactionRecords", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetTransactionRecordsAsync(string aConnectionId, long aCurrencyId, long aStartTxRecordId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/CheckAddressValidity", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult CheckAddressValidity(string aConnectionId, long aCurrencyId, string aBase58CheckAddress);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/CheckAddressValidity", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> CheckAddressValidityAsync(string aConnectionId, long aCurrencyId, string aBase58CheckAddress);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/StartGetTransactionToSign", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult StartGetTransactionToSign(string aConnectionId, string aTransationData);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/StartGetTransactionToSign", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> StartGetTransactionToSignAsync(string aConnectionId, string aTransationData);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/EndGetTransactionToSign", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult EndGetTransactionToSign(string aConnectionId, long aGetTxToSignHandle);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/EndGetTransactionToSign", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> EndGetTransactionToSignAsync(string aConnectionId, long aGetTxToSignHandle);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SendTransaction", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult SendTransaction(string aConnectionId, long aCurrencyId, string aSignedTxData);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SendTransaction", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> SendTransactionAsync(string aConnectionId, long aCurrencyId, string aSignedTxData);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IsTransactionSent", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult IsTransactionSent(string aConnectionId, long aSendTxHandle);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IsTransactionSent", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> IsTransactionSentAsync(string aConnectionId, long aSendTxHandle);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetTransactionId", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetTransactionId(string aConnectionId, long aSendTxHandle);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetTransactionId", ReplyAction="*")]
        System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetTransactionIdAsync(string aConnectionId, long aSendTxHandle);
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PandoraLogonResult))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2612.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class PandoraResult : object, System.ComponentModel.INotifyPropertyChanged {
        
        private object resultField;
        
        private string errorMsgField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public object result {
            get {
                return this.resultField;
            }
            set {
                this.resultField = value;
                this.RaisePropertyChanged("result");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string ErrorMsg {
            get {
                return this.errorMsgField;
            }
            set {
                this.errorMsgField = value;
                this.RaisePropertyChanged("ErrorMsg");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2612.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class PandoraLogonResult : PandoraResult {
        
        private System.DateTime lastConnectedField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public System.DateTime LastConnected {
            get {
                return this.lastConnectedField;
            }
            set {
                this.lastConnectedField = value;
                this.RaisePropertyChanged("LastConnected");
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface PandoraWalletService1_1SoapChannel : Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraWalletService1_1Soap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class PandoraWalletService1_1SoapClient : System.ServiceModel.ClientBase<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraWalletService1_1Soap>, Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraWalletService1_1Soap {
        
        public PandoraWalletService1_1SoapClient() {
        }
        
        public PandoraWalletService1_1SoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public PandoraWalletService1_1SoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PandoraWalletService1_1SoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PandoraWalletService1_1SoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult Logon(string aEmail, string aUserName, string aPassword) {
            return base.Channel.Logon(aEmail, aUserName, aPassword);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> LogonAsync(string aEmail, string aUserName, string aPassword) {
            return base.Channel.LogonAsync(aEmail, aUserName, aPassword);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraLogonResult Logon2(string aEmail, string aUserName, string aPassword, string aVersion) {
            return base.Channel.Logon2(aEmail, aUserName, aPassword, aVersion);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraLogonResult> Logon2Async(string aEmail, string aUserName, string aPassword, string aVersion) {
            return base.Channel.Logon2Async(aEmail, aUserName, aPassword, aVersion);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult IsConnected(string aConnectionId) {
            return base.Channel.IsConnected(aConnectionId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> IsConnectedAsync(string aConnectionId) {
            return base.Channel.IsConnectedAsync(aConnectionId);
        }
        
        public string GetServerId() {
            return base.Channel.GetServerId();
        }
        
        public System.Threading.Tasks.Task<string> GetServerIdAsync() {
            return base.Channel.GetServerIdAsync();
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult Logoff(string aConnectionId) {
            return base.Channel.Logoff(aConnectionId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> LogoffAsync(string aConnectionId) {
            return base.Channel.LogoffAsync(aConnectionId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetUserStatus(string aConnectionId) {
            return base.Channel.GetUserStatus(aConnectionId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetUserStatusAsync(string aConnectionId) {
            return base.Channel.GetUserStatusAsync(aConnectionId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetCurrencyList(string aConnectionId, long aStartId) {
            return base.Channel.GetCurrencyList(aConnectionId, aStartId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetCurrencyListAsync(string aConnectionId, long aStartId) {
            return base.Channel.GetCurrencyListAsync(aConnectionId, aStartId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetCurrency(string aConnectionId, long aCurrencyId) {
            return base.Channel.GetCurrency(aConnectionId, aCurrencyId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetCurrencyAsync(string aConnectionId, long aCurrencyId) {
            return base.Channel.GetCurrencyAsync(aConnectionId, aCurrencyId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetCurrencyIcon(string aConnectionId, long aCurrencyId) {
            return base.Channel.GetCurrencyIcon(aConnectionId, aCurrencyId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetCurrencyIconAsync(string aConnectionId, long aCurrencyId) {
            return base.Channel.GetCurrencyIconAsync(aConnectionId, aCurrencyId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetLastCurrencyStatus(string aConnectionId, long aCurrencyId) {
            return base.Channel.GetLastCurrencyStatus(aConnectionId, aCurrencyId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetLastCurrencyStatusAsync(string aConnectionId, long aCurrencyId) {
            return base.Channel.GetLastCurrencyStatusAsync(aConnectionId, aCurrencyId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetCurrencyStatusList(string aConnectionId, long aCurrencyId, long aStartId) {
            return base.Channel.GetCurrencyStatusList(aConnectionId, aCurrencyId, aStartId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetCurrencyStatusListAsync(string aConnectionId, long aCurrencyId, long aStartId) {
            return base.Channel.GetCurrencyStatusListAsync(aConnectionId, aCurrencyId, aStartId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetBlockHeight(string aConnectionId, long aCurrencyId) {
            return base.Channel.GetBlockHeight(aConnectionId, aCurrencyId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetBlockHeightAsync(string aConnectionId, long aCurrencyId) {
            return base.Channel.GetBlockHeightAsync(aConnectionId, aCurrencyId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult AddMonitoredAccount(string aConnectionId, long aCurrencyId, string aAddress) {
            return base.Channel.AddMonitoredAccount(aConnectionId, aCurrencyId, aAddress);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> AddMonitoredAccountAsync(string aConnectionId, long aCurrencyId, string aAddress) {
            return base.Channel.AddMonitoredAccountAsync(aConnectionId, aCurrencyId, aAddress);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetMonitoredAcccounts(string aConnectionId, long aCurrencyId, long aStartCurrencyAccountId) {
            return base.Channel.GetMonitoredAcccounts(aConnectionId, aCurrencyId, aStartCurrencyAccountId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetMonitoredAcccountsAsync(string aConnectionId, long aCurrencyId, long aStartCurrencyAccountId) {
            return base.Channel.GetMonitoredAcccountsAsync(aConnectionId, aCurrencyId, aStartCurrencyAccountId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult RemoveMonitoredAccount(string aConnectionId, long aCurrencyAccountId) {
            return base.Channel.RemoveMonitoredAccount(aConnectionId, aCurrencyAccountId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> RemoveMonitoredAccountAsync(string aConnectionId, long aCurrencyAccountId) {
            return base.Channel.RemoveMonitoredAccountAsync(aConnectionId, aCurrencyAccountId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetTransactionRecords(string aConnectionId, long aCurrencyId, long aStartTxRecordId) {
            return base.Channel.GetTransactionRecords(aConnectionId, aCurrencyId, aStartTxRecordId);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetTransactionRecordsAsync(string aConnectionId, long aCurrencyId, long aStartTxRecordId) {
            return base.Channel.GetTransactionRecordsAsync(aConnectionId, aCurrencyId, aStartTxRecordId);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult CheckAddressValidity(string aConnectionId, long aCurrencyId, string aBase58CheckAddress) {
            return base.Channel.CheckAddressValidity(aConnectionId, aCurrencyId, aBase58CheckAddress);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> CheckAddressValidityAsync(string aConnectionId, long aCurrencyId, string aBase58CheckAddress) {
            return base.Channel.CheckAddressValidityAsync(aConnectionId, aCurrencyId, aBase58CheckAddress);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult StartGetTransactionToSign(string aConnectionId, string aTransationData) {
            return base.Channel.StartGetTransactionToSign(aConnectionId, aTransationData);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> StartGetTransactionToSignAsync(string aConnectionId, string aTransationData) {
            return base.Channel.StartGetTransactionToSignAsync(aConnectionId, aTransationData);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult EndGetTransactionToSign(string aConnectionId, long aGetTxToSignHandle) {
            return base.Channel.EndGetTransactionToSign(aConnectionId, aGetTxToSignHandle);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> EndGetTransactionToSignAsync(string aConnectionId, long aGetTxToSignHandle) {
            return base.Channel.EndGetTransactionToSignAsync(aConnectionId, aGetTxToSignHandle);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult SendTransaction(string aConnectionId, long aCurrencyId, string aSignedTxData) {
            return base.Channel.SendTransaction(aConnectionId, aCurrencyId, aSignedTxData);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> SendTransactionAsync(string aConnectionId, long aCurrencyId, string aSignedTxData) {
            return base.Channel.SendTransactionAsync(aConnectionId, aCurrencyId, aSignedTxData);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult IsTransactionSent(string aConnectionId, long aSendTxHandle) {
            return base.Channel.IsTransactionSent(aConnectionId, aSendTxHandle);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> IsTransactionSentAsync(string aConnectionId, long aSendTxHandle) {
            return base.Channel.IsTransactionSentAsync(aConnectionId, aSendTxHandle);
        }
        
        public Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult GetTransactionId(string aConnectionId, long aSendTxHandle) {
            return base.Channel.GetTransactionId(aConnectionId, aSendTxHandle);
        }
        
        public System.Threading.Tasks.Task<Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult> GetTransactionIdAsync(string aConnectionId, long aSendTxHandle) {
            return base.Channel.GetTransactionIdAsync(aConnectionId, aSendTxHandle);
        }
    }
}

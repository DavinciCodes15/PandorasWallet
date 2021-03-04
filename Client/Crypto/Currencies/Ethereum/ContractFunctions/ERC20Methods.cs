namespace Pandora.Client.Crypto.Currencies.Ethereum
{
    public enum ERC20Methods : long
    {
        Transfer = 0xa9059cbb,
        TransferFrom = 0x23b872dd,
        TotalSupply = 0x18160ddd,
        BalanceOf = 0x70a08231,
        Allowance = 0xdd62ed3e,
        Approve = 0x095ea7b3,
        Name = 0x06fdde03,
        Symbol = 0x95d89b41,
        Decimals = 0x313ce567
    }
}
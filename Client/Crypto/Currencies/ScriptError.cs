namespace Pandora.Client.Crypto.Currencies
{
    public enum ScriptError
    {
        OK = 0,
        UnknownError,
        EvalFalse,
        OpReturn,

        /* Max sizes */
        ScriptSize,
        PushSize,
        OpCount,
        StackSize,
        SigCount,
        PubkeyCount,

        /* Failed verify operations */
        Verify,
        EqualVerify,
        CheckMultiSigVerify,
        CheckSigVerify,
        NumEqualVerify,

        /* Logical/Format/Canonical errors */
        BadOpCode,
        DisabledOpCode,
        InvalidStackOperation,
        InvalidAltStackOperation,
        UnbalancedConditional,

        /* OP_CHECKLOCKTIMEVERIFY */
        NegativeLockTime,
        UnsatisfiedLockTime,

        /* BIP62 */
        SigHashType,
        SigDer,
        MinimalData,
        SigPushOnly,
        SigHighS,
        SigNullDummy,
        PubKeyType,
        CleanStack,

        /* softfork safeness */
        DiscourageUpgradableNops,
        WitnessMalleated,
        WitnessMalleatedP2SH,
        WitnessProgramEmpty,
        WitnessProgramMissmatch,
        DiscourageUpgradableWitnessProgram,
        WitnessProgramWrongLength,
        WitnessUnexpected,
        NullFail,
        MinimalIf,
        WitnessPubkeyType,
    }
}
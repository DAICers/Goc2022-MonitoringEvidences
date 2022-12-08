using System;

namespace DaicGoc.Domain.ChainObjects.CosmosSdk.SdkObjects
{
    public class CosmosBlockSignature
    {
        public uint BlockIdFlag { get; }
        public string ValidatorAddress { get; }
        public DateTime Timestamp { get; }
        public string Signature { get; }

        public CosmosBlockSignature(uint blockIdFlag, string validatorAddress, DateTime timestamp, string signature)
        {
            BlockIdFlag = blockIdFlag;
            ValidatorAddress = validatorAddress;
            Timestamp = timestamp;
            Signature = signature;
        }
    }
}
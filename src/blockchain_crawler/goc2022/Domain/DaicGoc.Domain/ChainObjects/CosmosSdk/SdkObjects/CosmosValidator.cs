namespace DaicGoc.Domain.ChainObjects.CosmosSdk.SdkObjects
{
    public class CosmosValidator
    {
        public string Address { get; }
        public string Pub_Key_type { get; }
        public string Pub_Key_value { get; }
        public long Voting_power { get; }
        public long Proposer_priority { get; }

        public CosmosValidator(string address, string pubkey_type, string pubkey_value, long voting_power, long proposer_priority)
        {
            Address = address;
            Pub_Key_type = pubkey_type;
            Pub_Key_value = pubkey_value;
            Voting_power = voting_power;
            Proposer_priority = proposer_priority;
        }
    }
}
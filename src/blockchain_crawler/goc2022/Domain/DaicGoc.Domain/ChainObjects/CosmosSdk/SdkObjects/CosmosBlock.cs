using System;
using System.Collections.Generic;

namespace DaicGoc.Domain.ChainObjects.CosmosSdk.SdkObjects
{
    public class CosmosBlock
    {
        public long Height { get; }
        public string Hash { get; }
        public int Version { get; }
        public string ChainId { get; }
        public DateTime Time { get; }

        public string ParentBlockHash { get; }

        public int PartsTotal { get; }
        public string PartsHash { get; }

        public string Last_commit_hash { get; }
        public string Data_hash { get; }
        public string Validators_hash { get; }
        public string Next_validators_hash { get; }
        public string Consensus_hash { get; }
        public string App_hash { get; }
        public string Last_results_hash { get; }
        public string Evidence_hash { get; }
        public string Proposer_address { get; }

        public long LastCommit_Height { get; }
        public int LastCommit_Round { get; }
        public string LastCommit_BlockHash { get; }

        public int LastCommit_PartsTotal { get; }
        public string LastCommit_PartsHash { get; }

        public List<CosmosBlockSignature> BlockSignatures { get; }

        public CosmosBlock(long height, string hash, int version, string chainId, DateTime time, string parentBlockHash,
                          int partsTotal, string partsHash,
                          string last_commit_hash, string data_hash, string validators_hash, string next_validators_hash, string consensus_hash, string app_hash, string last_results_hash, string evidence_hash, string proposer_address,
                          long lastCommit_Height, int lastCommit_Round, string lastCommit_Blockhash, int lastCommit_PartsTotal, string lastCommit_PartsHash, List<CosmosBlockSignature> blockSignatures)
        {
            Height = height;
            Hash = hash;
            Version = version;
            ChainId = chainId;
            Time = time;

            ParentBlockHash = parentBlockHash;

            PartsTotal = partsTotal;
            PartsHash = partsHash;

            Last_commit_hash = last_commit_hash;
            Data_hash = data_hash;
            Validators_hash = validators_hash;
            Next_validators_hash = next_validators_hash;
            Consensus_hash = consensus_hash;
            App_hash = app_hash;
            Last_results_hash = last_results_hash;
            Evidence_hash = evidence_hash;
            Proposer_address = proposer_address;

            LastCommit_Height = lastCommit_Height;
            LastCommit_Round = lastCommit_Round;
            LastCommit_BlockHash = lastCommit_Blockhash;

            LastCommit_PartsTotal = lastCommit_PartsTotal;
            LastCommit_PartsHash = lastCommit_PartsHash;

            BlockSignatures = blockSignatures;
        }
    }
}
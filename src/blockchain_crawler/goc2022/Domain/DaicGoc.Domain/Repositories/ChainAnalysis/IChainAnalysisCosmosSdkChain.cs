using DaicGoc.Domain.ChainObjects.CosmosSdk.SdkObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaicGoc.Domain.Repositories.ChainAnalysis
{
    public interface IChainAnalysisCosmosSdkChain
    {
        Task InitialiseRpcAsync(string user, string pwd, string url, int port);

        Task<CosmosBlock> GetBlock(long height);

        Task<List<CosmosValidator>> GetValidators(long height);
    }
}
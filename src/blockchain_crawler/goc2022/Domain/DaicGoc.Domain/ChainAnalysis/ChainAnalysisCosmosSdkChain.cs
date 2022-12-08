using DaicGoc.Domain.ChainObjects.CosmosSdk.SdkObjects;
using DaicGoc.Domain.Repositories.ChainAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaicGoc.Domain.ChainAnalysis
{
    public class ChainAnalysisCosmosSdkChain : IChainAnalysisCosmosSdkChain
    {
        private readonly IChainAnalysisCosmosSdkChainRepository _apiClient;

        public ChainAnalysisCosmosSdkChain(IChainAnalysisCosmosSdkChainRepository apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<CosmosBlock> GetBlock(long height)
        {
            return await _apiClient.GetBlock(height);
        }

        public async Task<List<CosmosValidator>> GetValidators(long height)
        {
            return await _apiClient.GetValidators(height);
        }

        public async Task InitialiseRpcAsync(string user, string pwd, string url, int port)
        {
            await _apiClient.InitialiseRpcAsync(user, pwd, url, port);
        }
    }
}
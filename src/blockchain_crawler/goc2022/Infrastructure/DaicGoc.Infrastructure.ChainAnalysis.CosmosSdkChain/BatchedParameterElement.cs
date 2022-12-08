using System.Collections.Generic;

namespace DaicGoc.Infrastructure.ChainAnalysis.CosmosSdkChain
{
    public class BatchedParameterElement
    {
        public List<object> Parameters { get; }
        public int Id { get; }

        public BatchedParameterElement(List<object> parameters, int id)
        {
            Parameters = parameters;
            Id = id;
        }
    }
}
using DaicGoc.Domain.ChainObjects.CosmosSdk.SdkObjects;
using DaicGoc.Domain.Repositories.ChainAnalysis;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DaicGoc.Infrastructure.ChainAnalysis.CosmosSdkChain
{
    public class ChainAnalysisCosmosSdkChainRepository : IChainAnalysisCosmosSdkChainRepository
    {
        private string _rpc_user;
        private string _rpc_pwd;
        private string _rpc_url;
        private int _rpc_port;

        //LCD Tendermitn RPC
        private string _rpc_blocks_url = "/block?height={0}";

        //Service Endpoints
        private string _rpc_validators_url = "/validators?height={0}&per_page=250";

        private async Task<string> GetResponseAsync(string baseHost, string urlSuffix)
        {
            await Task.CompletedTask;

            int loading_attempts = 0;

            string url = baseHost + urlSuffix;

            try
            {
                ServicePointManager.DefaultConnectionLimit = 100;

                loading_attempts++;
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.ContentType = "application/json";
                webRequest.Method = "GET";
                webRequest.Timeout = 200000;
                webRequest.ContinueTimeout = 200000;
                webRequest.ReadWriteTimeout = 200000;

                // serialize json for the request
                WebResponse webResponse = webRequest.GetResponse();

                string json;

                using (var sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    json = sr.ReadToEnd();
                    sr.Close();
                }

                webResponse.Close();

                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Source);
                if (ex.InnerException is not null) Console.WriteLine(ex.InnerException.Message.ToString());
                throw;
            }
        }

        public async Task InitialiseRpcAsync(string user, string pwd, string url, int port)
        {
            await Task.CompletedTask;

            _rpc_user = user;
            _rpc_pwd = pwd;
            _rpc_port = port;


            string rpcurl = "";
            if (_rpc_port == 80)
            {
                rpcurl = url;
            }
            else
            {
                rpcurl = url + ":" + _rpc_port;
            }

            _rpc_url = rpcurl;
      
        }

        public async Task<CosmosBlock> GetBlock(long height)
        {
            string url = string.Format(_rpc_blocks_url, height);


            try
            {
                string result = await GetResponseAsync(_rpc_url, url);

                JObject jsonResult = JObject.Parse(result);

                CosmosBlock block = await JsonResultToBlock(jsonResult);

                return block;
            }
            catch (Exception)
            {
                Console.WriteLine(string.Format("Error on loading '{0}' from RPC (height: {1})", nameof(this.GetBlock), height));
                throw;
            }
        }

        private async Task<CosmosBlock> JsonResultToBlock(JObject jsonResult)
        {
            await Task.CompletedTask;

            try
            {
                long height = Convert.ToInt64(jsonResult["result"]["block"]["header"]["height"].ToString());

                string hash = jsonResult["result"]["block_id"]["hash"].ToString();

                int version = Convert.ToInt32(jsonResult["result"]["block"]["header"]["version"]["block"].ToString());

                string chain_id = jsonResult["result"]["block"]["header"]["chain_id"].ToString();

                string dateString = jsonResult["result"]["block"]["header"]["time"].ToString();
                DateTime time = DateTime.Parse(dateString).ToUniversalTime();

                string parentBlockHash = jsonResult["result"]["block"]["header"]["last_block_id"]["hash"].ToString();

                int parts_total = Convert.ToInt32(jsonResult["result"]["block_id"]["parts"]["total"].ToString());

                string parts_hash = jsonResult["result"]["block_id"]["parts"]["hash"].ToString();

                string last_commit_hash = jsonResult["result"]["block"]["header"]["last_commit_hash"].ToString();

                string data_hash = jsonResult["result"]["block"]["header"]["data_hash"].ToString();

                string validators_hash = jsonResult["result"]["block"]["header"]["validators_hash"].ToString();

                string next_validators_hash = jsonResult["result"]["block"]["header"]["next_validators_hash"].ToString();

                string consensus_hash = jsonResult["result"]["block"]["header"]["consensus_hash"].ToString();

                string app_hash = jsonResult["result"]["block"]["header"]["app_hash"].ToString();

                string last_results_hash = jsonResult["result"]["block"]["header"]["last_results_hash"].ToString();

                string evidence_hash = jsonResult["result"]["block"]["header"]["evidence_hash"].ToString();

                string proposer_address = jsonResult["result"]["block"]["header"]["proposer_address"].ToString();

                string lastCommit_BlockHash = jsonResult["result"]["block"]["last_commit"]["block_id"]["hash"].ToString();

                long lastCommit_Height = Convert.ToInt64(jsonResult["result"]["block"]["last_commit"]["height"].ToString());

                int lastCommit_Round = Convert.ToInt32(jsonResult["result"]["block"]["last_commit"]["round"].ToString());

                int lastCommit_parts_total = Convert.ToInt32(jsonResult["result"]["block"]["last_commit"]["block_id"]["parts"]["total"].ToString());
                string lastCommit_parts_hash = jsonResult["result"]["block"]["last_commit"]["block_id"]["parts"]["hash"].ToString();

                //Load Block Signatures
                List<CosmosBlockSignature> blockSignatures = new List<CosmosBlockSignature>();
                foreach (JObject jsonBlockSig in jsonResult["result"]["block"]["last_commit"]["signatures"])
                {
                    uint blockIdFlag = Convert.ToUInt32(jsonBlockSig["block_id_flag"].ToString());
                    string validator_address = jsonBlockSig["validator_address"].ToString();
                    DateTime timestamp = DateTime.Parse(jsonBlockSig["timestamp"].ToString());
                    string signature = jsonBlockSig["signature"].ToString();

                    CosmosBlockSignature blockSignature = new CosmosBlockSignature(blockIdFlag, validator_address, timestamp, signature);

                    blockSignatures.Add(blockSignature);
                }

                CosmosBlock block = new CosmosBlock(height, hash, version, chain_id, time, parentBlockHash,
                                                          parts_total, parts_hash, last_commit_hash, data_hash, validators_hash,
                                                          next_validators_hash, consensus_hash, app_hash, last_results_hash, evidence_hash, proposer_address,
                                                          lastCommit_Height, lastCommit_Round, lastCommit_BlockHash, lastCommit_parts_total, lastCommit_parts_hash, blockSignatures);

                return block;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<List<CosmosValidator>> GetValidators(long height)
        {
            string url = string.Format(_rpc_validators_url, height);

            try
            {
                string result = await GetResponseAsync(_rpc_url, url);

                JObject jsonResult = JObject.Parse(result);

                List<CosmosValidator> validators = new List<CosmosValidator>();

                JArray jsonValidatorResult = (JArray)jsonResult["result"]["validators"];

                foreach (JObject jsonValidator in jsonValidatorResult)
                {
                    try
                    {
                        CosmosValidator validator;

                        validator = new CosmosValidator(jsonValidator["address"].ToString(),
                                                                  jsonValidator["pub_key"]["type"].ToString(),
                                                                  jsonValidator["pub_key"]["value"].ToString(),
                                                                  Convert.ToInt64(jsonValidator["voting_power"].ToString()),
                                                                  Convert.ToInt64(jsonValidator["proposer_priority"].ToString()));

                        validators.Add(validator);
                    }
                    catch (Exception ex)
                    {
                        string except = ex.Message;
                        throw;
                    }
                }

                return validators;
            }
            catch (Exception)
            {
                Console.WriteLine(string.Format("Error on loading '{0}' from RPC (height: {1})", nameof(this.GetValidators), height));
                throw;
            }
        }
    }
}
using DaicGoc.Domain.ChainObjects.CosmosSdk.SdkObjects;
using DaicGoc.Domain.DbConnection.DatabaseWorker;
using DaicGoc.Domain.Repositories.ChainAnalysis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaicGoc.Domain.AppServices.ChainImport
{
    public class ImporterAppService
    {
        private MySqlDatabaseWorker _mySqlDatabaseWorker;

        private IChainAnalysisCosmosSdkChain _chain;

        private string _DbHost { get; }
        private string _DbName { get; }
        private string _DbUser { get; }
        private string _DbPwd { get; }

        private string _RpcHost { get; }
        private int _RpcPort { get; }
        private string _RpcUser { get; }
        private string _RpcPwd { get; }

        private long _StartBlock;
        private long _EndBlock;

        private static string _chain_db_prefix;

        //Last Block Height Qry
        private const int INSERT_OR_UPDATE_QRY_COUNT = 100;

        private string _selectLastAnalysedBlockHeight;

        //Chains
        private string _insertBlock;

        //Validators
        private string _insertValidators;

        private string _insertBlockValidators_InsertPart;
        private string _insertBlockValidators_ValuesPart = "({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}')";

        private string _insertBlockSignatures_InsertPart;
        private string _insertBlockSignatures_ValuesPart = "({0}, {1}, '{2}', '{3}', '{4}')";

        public ImporterAppService(MySqlDatabaseWorker mySqlDatabaseWorker, IChainAnalysisCosmosSdkChain chain,
                                  string dbHost, string dbName, string dbUser, string dbPwd,
                                  string rpcHost, int rpcPort, string rpcUser, string rpcPwd,
                                  long startBlock, long endBlock, string chain_db_prefix)
        {
            _mySqlDatabaseWorker = mySqlDatabaseWorker;
            _chain = chain;

            _DbHost = dbHost;
            _DbName = dbName;
            _DbUser = dbUser;
            _DbPwd = dbPwd;

            _StartBlock = startBlock;
            _EndBlock = endBlock;

            _RpcHost = rpcHost;
            _RpcPort = rpcPort;
            _RpcUser = rpcUser;
            _RpcPwd = rpcPwd;

            _chain_db_prefix = chain_db_prefix;

            _selectLastAnalysedBlockHeight = "SELECT height FROM " + _chain_db_prefix + "_blocks ORDER BY height DESC LIMIT 1;";

            _insertBlock = "INSERT INTO " + _chain_db_prefix + "_blocks(height, hash, version, chain_id, time, time_date, time_time, parts_total, parts_hash, last_block_hash, last_commit_hash, data_hash, validators_hash, next_validators_hash, consensus_hash, app_hash, last_results_hash, evidence_hash, proposer_address, last_commit_height, last_commit_round, last_commit_block_id_hash, last_commit_parts_total, last_commit_parts_hash) VALUES ({0}, '{1}', {2}, '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}', '{22}', '{23}');";

            //Validators
            _insertValidators = $"INSERT INTO " + _chain_db_prefix + "_validators (address, pub_key_type, pub_key) SELECT * FROM (SELECT '{0}' as chain_name, '{1}' as pub_key_type, '{2}' as pub_key) AS tmp WHERE NOT EXISTS ( SELECT address FROM " + _chain_db_prefix + "_validators WHERE address = '{0}') LIMIT 1;";

            _insertBlockValidators_InsertPart = "INSERT INTO " + _chain_db_prefix + "_block_validators (blockdb_id, height, address, pub_key_type, pub_key_value, voting_power, proposer_priority) VALUES ";
            _insertBlockValidators_ValuesPart = "({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}')";

            _insertBlockSignatures_InsertPart = "INSERT INTO " + _chain_db_prefix + "_blocks_signatures(blockdb_id, height, validator_address, timestamp, signature) VALUES ";
            _insertBlockSignatures_ValuesPart = "({0}, {1}, '{2}', '{3}', '{4}')";
        }

        public async Task RunAsync()
        {
            try
            {
                DateTime startTime = DateTime.UtcNow;

                //open MySQL Connection
                _mySqlDatabaseWorker.CreateConnection(_DbHost, _DbName, _DbUser, _DbPwd, 6000);
                _mySqlDatabaseWorker.OpenConnection();

                //Initialise LCD
                await _chain.InitialiseRpcAsync(_RpcUser, _RpcPwd, _RpcHost, _RpcPort);

                //Check if started from last block in Database
                if (_StartBlock == 0)
                {
                    _StartBlock = SelectLastAnalysedBlockHeight() + 1;
                }

                //Get Blocks and do Analysis
                for (long blockHeight = _StartBlock; blockHeight <= _EndBlock; blockHeight++)
                {
                    _mySqlDatabaseWorker.CheckConnectionStateAndReopen();

                    DateTime tick = DateTime.UtcNow;

                    //Console Output
                    Console.Clear();
                    Console.WriteLine(string.Format("Analysing Job for {0}: Block {1} to {2}", _chain_db_prefix, _StartBlock, _EndBlock));
                    TimeSpan runningTime = DateTime.UtcNow - startTime;

                    Console.WriteLine("");
                    Console.WriteLine(string.Format("Analysing block {0}", blockHeight));

                    //Get Block
                    CosmosBlock block = await _chain.GetBlock(blockHeight);

                    DateTime tock = DateTime.UtcNow;

                    Console.WriteLine(string.Format("\tloading took {0} seconds...", Math.Round((tock - tick).TotalSeconds, 2)));
                    Console.WriteLine(string.Format("\tBlockTime: {0}", block.Time.ToString("yyyy-MM-dd HH:mm:ss")));

                    tick = DateTime.UtcNow;

                    //Write Block to DB and get blockDbId
                    long blockDbId = await InsertBlockIntoDb(block);

                    //Load actual Validators
                    List<CosmosValidator> validators = new List<CosmosValidator>(await _chain.GetValidators(blockHeight));
                    await InsertValidatorsAndValidatorStats(_mySqlDatabaseWorker, validators, blockHeight, blockDbId);

                    //Block Signatures
                    tick = DateTime.UtcNow;
                    List<string> blockSignatureQryValues = new List<string>();
                    foreach (CosmosBlockSignature terraBlockSignature in block.BlockSignatures)
                    {
                        blockSignatureQryValues.Add(string.Format(_insertBlockSignatures_ValuesPart, blockDbId,
                                                                                                     blockHeight,
                                                                                                     terraBlockSignature.ValidatorAddress,
                                                                                                     terraBlockSignature.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                                                     terraBlockSignature.Signature));
                    }
                    await InsertBatchwiseInDb(_mySqlDatabaseWorker, blockSignatureQryValues, _insertBlockSignatures_InsertPart);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            finally
            {
                _mySqlDatabaseWorker.CloseConnection();
            }
        }

        private long SelectLastAnalysedBlockHeight()
        {
            string qry = _selectLastAnalysedBlockHeight;

            DataTable dt = _mySqlDatabaseWorker.GetData(qry);

            long height = -1;
            if (dt.Rows.Count == 1)
            {
                height = Convert.ToInt32(dt.Rows[0]["height"].ToString());
            }

            return height;
        }

        private async Task<long> InsertBlockIntoDb(CosmosBlock block)
        {
            await Task.CompletedTask;

            //Insert Block
            string insertBlockQry = string.Format(_insertBlock, block.Height,
                                                                block.Hash,
                                                                block.Version,
                                                                block.ChainId,
                                                                block.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                block.Time.ToString("yyyy-MM-dd"),
                                                                block.Time.ToString("HH:mm:ss"),
                                                                block.PartsTotal,
                                                                block.PartsHash,
                                                                block.ParentBlockHash,
                                                                block.Last_commit_hash,
                                                                block.Data_hash,
                                                                block.Validators_hash,
                                                                block.Next_validators_hash,
                                                                block.Consensus_hash,
                                                                block.App_hash,
                                                                block.Last_results_hash,
                                                                block.Evidence_hash,
                                                                block.Proposer_address,
                                                                block.LastCommit_Height,
                                                                block.LastCommit_Round,
                                                                block.LastCommit_BlockHash,
                                                                block.LastCommit_PartsTotal,
                                                                block.LastCommit_PartsHash);

            _mySqlDatabaseWorker.ExecuteNonQuery(insertBlockQry);

            //Get Block Id
            long blockDbId = SelectBlockDbIdByHeight(block.Height);

            return blockDbId;
        }

        private long SelectBlockDbIdByHeight(long height)
        {
            try
            {
                string qry = string.Format("SELECT id FROM " + _chain_db_prefix + "_blocks WHERE height = '{0}'", height);

                DataTable dt = _mySqlDatabaseWorker.GetData(qry);

                long id = 0;

                if (dt.Rows.Count == 0)
                    throw new Exception("no block found");
                else if (dt.Rows.Count > 1)
                    throw new Exception("multiple blocks found");
                else
                    id = Convert.ToInt64(dt.Rows[0]["id"].ToString());

                return id;
            }
            catch
            {
                throw;
            }
        }

        private async Task InsertValidatorsAndValidatorStats(MySqlDatabaseWorker mySqlDatabaseWorker, List<CosmosValidator> validators, long height, long blockdbid)
        {
            try
            {
                await Task.CompletedTask;

                List<string> validatorQrys = new List<string>();
                List<string> blockValidatorsValues = new List<string>();

                //Analyse Validators
                foreach (CosmosValidator validator in validators)
                {
                    string validatorQry = string.Format(_insertValidators, validator.Address,
                                                                           validator.Pub_Key_type,
                                                                           validator.Pub_Key_value);

                    validatorQrys.Add(validatorQry);

                    string blockValidatorsQry = string.Format(_insertBlockValidators_ValuesPart, blockdbid,
                                                                                                 height,
                                                                                                 validator.Address,
                                                                                                 validator.Pub_Key_type,
                                                                                                 validator.Pub_Key_value,
                                                                                                 validator.Voting_power,
                                                                                                 validator.Proposer_priority);

                    blockValidatorsValues.Add(blockValidatorsQry);
                }

                //Write Validators
                mySqlDatabaseWorker.ExecuteNonQuery(string.Join("", validatorQrys));

                await InsertBatchwiseInDb(mySqlDatabaseWorker, blockValidatorsValues, _insertBlockValidators_InsertPart);
            }
            catch (Exception ex)
            {
                string exept = ex.Message;
                throw;
            }
        }

        private async Task InsertBatchwiseInDb(MySqlDatabaseWorker mySqlDatabaseWorker, List<string> qryValues, string qryInsertPart)
        {
            await Task.CompletedTask;

            try
            {
                //Write Logs
                for (int j = 0; j < qryValues.Count; j = j + INSERT_OR_UPDATE_QRY_COUNT)
                {
                    List<string> rowsForInsert = qryValues.Skip(j).Take(INSERT_OR_UPDATE_QRY_COUNT).ToList();

                    if (rowsForInsert.Count > 0)
                    {
                        try
                        {
                            StringBuilder insertQueries = new StringBuilder(qryInsertPart);
                            insertQueries.Append(string.Join(",", rowsForInsert));
                            insertQueries.Append(";");

                            mySqlDatabaseWorker.ExecuteNonQuery(insertQueries.ToString());
                        }
                        catch (Exception ex)
                        {
                            string exept = ex.Message;
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string exept = ex.Message;
                throw;
            }
        }
    }
}
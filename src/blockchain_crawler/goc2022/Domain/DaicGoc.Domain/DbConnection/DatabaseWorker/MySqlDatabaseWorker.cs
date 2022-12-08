using DaicGoc.Domain.DbConnection.DatabaseConnector;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DaicGoc.Domain.DbConnection.DatabaseWorker
{
    public class MySqlDatabaseWorker : IMySqlDatabaseWorker
    {
        private readonly IMySqlDatabaseConnector _DatabaseConnector;

        public void CreateConnection(string server, string database, string userId, string userPwd, int connectionTimeout = 240, bool useConnection = false, bool allowUserVariables = false)
        {
            _DatabaseConnector.CreateConnection(server, database, userId, userPwd, connectionTimeout, useConnection, allowUserVariables);
        }

        public MySqlDatabaseWorker(IMySqlDatabaseConnector databaseConnector)
        {
            _DatabaseConnector = databaseConnector;
        }

        public MySqlDatabaseWorker(IMySqlDatabaseConnector databaseConnector, string server)
        {
            _DatabaseConnector = databaseConnector;
        }

        public void OpenConnection()
        {
            _DatabaseConnector.OpenConnection();
        }

        public void CloseConnection()
        {
            _DatabaseConnector.CloseConnection();
        }

        public ConnectionState GetConnectionState()
        {
            return _DatabaseConnector.GetConnectionState();
        }

        public DataTable GetData(string query)
        {
            return _DatabaseConnector.GetData(query);
        }

        public void ExecuteNonQueryOwnReader(string nonQuery)
        {
            _DatabaseConnector.ExecuteNonQueryOwnReader(nonQuery);
        }

        public void ExecuteNonQuery(string nonQuery)
        {
            _DatabaseConnector.ExecuteNonQuery(nonQuery);
        }

        public async Task OpenConnectionAsync()
        {
            await _DatabaseConnector.OpenConnectionAsync();
        }

        public async Task CloseConnectionAsync()
        {
            await _DatabaseConnector.CloseConnectionAsync();
        }

        public async Task<DataTable> GetDataAsync(string query)
        {
            return await _DatabaseConnector.GetDataAsync(query);
        }

        public async Task ExecuteNonQueryAsync(string nonQuery)
        {
            await _DatabaseConnector.ExecuteNonQueryAsync(nonQuery);
        }

        public async Task PerfromTransactionAsync(List<string> queries)
        {
            await _DatabaseConnector.PerfromTransactionAsync(queries);
        }

        public void CheckConnectionStateAndReopen()
        {
            ConnectionState connectionState = _DatabaseConnector.GetConnectionState();
            if (connectionState != ConnectionState.Open)
            {
                _DatabaseConnector.CloseConnection();
                _DatabaseConnector.OpenConnection();
            }

            bool ping_res = _DatabaseConnector.Ping();

            if (ping_res == false)
            {
                _DatabaseConnector.CloseConnection();
                _DatabaseConnector.OpenConnection();
            }
        }

        public bool Ping()
        {
            return _DatabaseConnector.Ping();
        }
    }
}
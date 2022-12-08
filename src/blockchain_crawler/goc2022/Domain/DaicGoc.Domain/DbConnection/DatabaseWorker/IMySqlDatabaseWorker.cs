using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DaicGoc.Domain.DbConnection.DatabaseWorker
{
    public interface IMySqlDatabaseWorker : IDatabaseWorker
    {
        void CreateConnection(string server, string database, string userId, string userPwd, int connectionTimeout = 240, bool useConnection = false, bool allowUserVariables = false);

        void OpenConnection();

        Task OpenConnectionAsync();

        void CloseConnection();

        Task CloseConnectionAsync();

        ConnectionState GetConnectionState();

        DataTable GetData(string query);

        Task<DataTable> GetDataAsync(string query);

        void ExecuteNonQueryOwnReader(string nonQuery);

        void ExecuteNonQuery(string nonQuery);

        Task ExecuteNonQueryAsync(string nonQuery);

        bool Ping();

        Task PerfromTransactionAsync(List<string> queries);
    }
}
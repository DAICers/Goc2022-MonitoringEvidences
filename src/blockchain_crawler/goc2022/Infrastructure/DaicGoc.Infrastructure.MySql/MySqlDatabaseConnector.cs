using DaicGoc.Domain.DbConnection.DatabaseConnector;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DaicGoc.Infrastructure.MySql
{
    public class MySqlDatabaseConnector : IMySqlDatabaseConnector
    {
        private MySqlConnection _MySqlConnection;

        private int _connectionTimeout = 240;

        public MySqlDatabaseConnector()
        {
        }

        public void CreateConnection(string server, string database, string userId, string userPwd, int connectionTimeout = 240, bool useCompression = false, bool allowUserVariables = false)
        {
            _connectionTimeout = connectionTimeout;

            try
            {
                var connectionString = String.Format("server={0};uid={1};pwd={2};database={3};convert zero datetime=True;Connect Timeout={4}", server, userId, userPwd, database, connectionTimeout);
                if (useCompression == true)
                    connectionString += ";UseCompression=True";

                if (allowUserVariables == true)
                    connectionString += ";AllowUserVariables=True";

                _MySqlConnection = new MySqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("fehler bei der verarbeitung: " + DateTime.Now);
                Debug.WriteLine("HelpLink = {0}", ex.HelpLink);
                Debug.WriteLine("Message = {0}", ex.Message);
                Debug.WriteLine("Source = {0}", ex.Source);
                Debug.WriteLine("StackTrace = {0}", ex.StackTrace);
                Debug.WriteLine("TargetSite = {0}", ex.TargetSite);

                throw;
            }
        }

        public MySqlCommand InitSqlCommand(string query)
        {
            try
            {
                var mySqlCommand = new MySqlCommand(query, _MySqlConnection);
                mySqlCommand.CommandTimeout = _connectionTimeout;
                return mySqlCommand;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("fehler bei der verarbeitung: " + DateTime.Now);
                Debug.WriteLine("HelpLink = {0}", ex.HelpLink);
                Debug.WriteLine("Message = {0}", ex.Message);
                Debug.WriteLine("Source = {0}", ex.Source);
                Debug.WriteLine("StackTrace = {0}", ex.StackTrace);
                Debug.WriteLine("TargetSite = {0}", ex.TargetSite);

                throw;
            }
        }

        public void ExecuteNonQueryOwnReader(string nonQuery)
        {
            try
            {
                var cmd = _MySqlConnection.CreateCommand();
                cmd.CommandText = nonQuery;
                cmd.CommandTimeout = _connectionTimeout;
                MySqlDataReader reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("fehler bei der verarbeitung: " + DateTime.Now);
                Debug.WriteLine("HelpLink = {0}", ex.HelpLink);
                Debug.WriteLine("Message = {0}", ex.Message);
                Debug.WriteLine("Source = {0}", ex.Source);
                Debug.WriteLine("StackTrace = {0}", ex.StackTrace);
                Debug.WriteLine("TargetSite = {0}", ex.TargetSite);
                throw;
            }
        }

        public void ExecuteNonQuery(string nonQuery)
        {
            var mySqlCommand = new MySqlCommand(nonQuery, _MySqlConnection);
            mySqlCommand.CommandTimeout = _connectionTimeout;
            try
            {
                mySqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("fehler bei der verarbeitung: " + DateTime.Now);
                Debug.WriteLine("HelpLink = {0}", ex.HelpLink);
                Debug.WriteLine("Message = {0}", ex.Message);
                Debug.WriteLine("Source = {0}", ex.Source);
                Debug.WriteLine("StackTrace = {0}", ex.StackTrace);
                Debug.WriteLine("TargetSite = {0}", ex.TargetSite);
                throw;
            }
        }

        public async Task ExecuteNonQueryAsync(string nonQuery)
        {
            var mySqlCommand = new MySqlCommand(nonQuery, _MySqlConnection);
            mySqlCommand.CommandTimeout = _connectionTimeout;
            try
            {
                await mySqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("fehler bei der verarbeitung: " + DateTime.Now);
                Debug.WriteLine("HelpLink = {0}", ex.HelpLink);
                Debug.WriteLine("Message = {0}", ex.Message);
                Debug.WriteLine("Source = {0}", ex.Source);
                Debug.WriteLine("StackTrace = {0}", ex.StackTrace);
                Debug.WriteLine("TargetSite = {0}", ex.TargetSite);

                throw;
            }
        }

        public DataTable GetData(string query)
        {
            try
            {
                var dataTable = new DataTable();
                var dataSet = new DataSet();
                var dataAdapter = new MySqlDataAdapter { SelectCommand = InitSqlCommand(query) };

                dataAdapter.Fill(dataSet);
                dataTable = dataSet.Tables[0];
                return dataTable;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataTable> GetDataAsync(string query)
        {
            await Task.CompletedTask;

            var dataTable = new DataTable();
            var dataSet = new DataSet();
            var dataAdapter = new MySqlDataAdapter { SelectCommand = InitSqlCommand(query) };

            dataAdapter.Fill(dataSet);
            dataTable = dataSet.Tables[0];
            return dataTable;
        }

        public void OpenConnection()
        {
            if (_MySqlConnection != null && _MySqlConnection.State == ConnectionState.Closed)
            {
                try
                {
                    _MySqlConnection.Open();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("fehler bei der verarbeitung: " + DateTime.Now);
                    Debug.WriteLine("HelpLink = {0}", ex.HelpLink);
                    Debug.WriteLine("Message = {0}", ex.Message);
                    Debug.WriteLine("Source = {0}", ex.Source);
                    Debug.WriteLine("StackTrace = {0}", ex.StackTrace);
                    Debug.WriteLine("TargetSite = {0}", ex.TargetSite);

                    throw;
                }
            }
        }

        public async Task OpenConnectionAsync()
        {
            if (_MySqlConnection != null && _MySqlConnection.State == ConnectionState.Closed)
            {
                try
                {
                    await _MySqlConnection.OpenAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("fehler bei der verarbeitung: " + DateTime.Now);
                    Debug.WriteLine("HelpLink = {0}", ex.HelpLink);
                    Debug.WriteLine("Message = {0}", ex.Message);
                    Debug.WriteLine("Source = {0}", ex.Source);
                    Debug.WriteLine("StackTrace = {0}", ex.StackTrace);
                    Debug.WriteLine("TargetSite = {0}", ex.TargetSite);

                    throw;
                }
            }
        }

        public void CloseConnection()
        {
            if (_MySqlConnection != null && _MySqlConnection.State == ConnectionState.Open)
            {
                try
                {
                    _MySqlConnection.Close();
                }
                catch (MySqlException ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public async Task CloseConnectionAsync()
        {
            if (_MySqlConnection != null && _MySqlConnection.State == ConnectionState.Open)
            {
                try
                {
                    await _MySqlConnection.CloseAsync();
                }
                catch (MySqlException ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public async Task PerfromTransactionAsync(List<string> queries)
        {
            await Task.CompletedTask;

            MySqlCommand myCommand = _MySqlConnection.CreateCommand();
            MySqlTransaction myTrans;

            // Start a local transaction
            myTrans = _MySqlConnection.BeginTransaction();

            myCommand.Connection = _MySqlConnection;
            myCommand.Transaction = myTrans;

            try
            {
                foreach (string qry in queries)
                {
                    myCommand.CommandText = qry;
                    myCommand.CommandTimeout = _connectionTimeout;
                    myCommand.ExecuteNonQuery();
                }

                CheckConnectionStateAndReopen();
                myTrans.Commit();
            }
            catch (MySqlException ex)
            {
                try
                {
                    Console.WriteLine(ex.Message);
                    CheckConnectionStateAndReopen();
                    myTrans.Rollback();
                }
                catch (MySqlException mySqlEx)
                {
                    if (myTrans.Connection != null)
                    {
                        Console.WriteLine("An exception of type " + mySqlEx.GetType() +
                        " was encountered while attempting to roll back the transaction.");
                    }

                    throw;
                }
            }
        }

        public void CheckConnectionStateAndReopen()
        {
            ConnectionState connectionState = GetConnectionState();
            if (connectionState != ConnectionState.Open)
            {
                _MySqlConnection.Close();
                _MySqlConnection.Open();
            }

            bool ping_res = _MySqlConnection.Ping();

            if (ping_res == false)
            {
                _MySqlConnection.Close();
                _MySqlConnection.Open();
            }
        }

        public ConnectionState GetConnectionState()
        {
            return _MySqlConnection.State;
        }

        public bool Ping()
        {
            return _MySqlConnection.Ping();
        }

        ~MySqlDatabaseConnector()
        {
            if (_MySqlConnection != null && _MySqlConnection.State == ConnectionState.Closed)
                _MySqlConnection.Dispose();
        }
    }
}
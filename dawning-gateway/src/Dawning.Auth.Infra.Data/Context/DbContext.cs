using System;
using System.Data;
using Dawning.Auth.Dapper.Contrib;
using MySql.Data.MySqlClient;

namespace Dawning.Auth.Infra.Data.Context
{
    public class DbContext : IDisposable
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public DbContext(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        public IDbConnection Connection => _connection;

        public IDbTransaction BeginTransaction()
        {
            _transaction = _connection.BeginTransaction();
            return _transaction;
        }

        public void Commit()
        {
            _transaction?.Commit();
            _transaction?.Dispose();
        }

        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }

        // Dapper CRUD
        // public async Task<T> GetListAsync() where T : class, new() => await _connection.GetListAsync(IDataAdapter, _transaction);

        public async Task<T> GetAsync<T>(Guid id) where T : class, new() => await _connection.GetAsync<T>(id, _transaction);

        public async Task<int> InsertAsync<T>(T t) where T : class, new() => await _connection.InsertAsync(t, _transaction);

        public async Task<dynamic> UpdateAsync<T>(T t) where T : class, new() => await _connection.UpdateAsync(t, _transaction);

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class, new() => await _connection.GetAllAsync<T>();
    }
}


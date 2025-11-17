using System.Data;
using MySql.Data.MySqlClient;

namespace Dawning.Identity.Infra.Data.Context;

public class DbContext : IDisposable
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed = false;

    public DbContext(string connectionString)
    {
        _connection = new MySqlConnection(connectionString);
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }
    }

    public IDbTransaction? Transaction => _transaction;

    public IDbTransaction BeginTransaction()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started.");
        }

        if (Connection.State != ConnectionState.Open)
        {
            Connection.Open();
        }

        _transaction = Connection.BeginTransaction();
        return _transaction;
    }

    public void Commit()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to commit.");
        }

        try
        {
            _transaction.Commit();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to rollback.");
        }

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }

            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }
        }

        _disposed = true;
    }
}
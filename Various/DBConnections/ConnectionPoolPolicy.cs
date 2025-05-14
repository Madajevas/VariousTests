using Microsoft.Data.SqlClient;
using Microsoft.Extensions.ObjectPool;

using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;

namespace Various.DBConnections
{
    public class ConnectionPoolPolicy(string connectionString) : IPooledObjectPolicy<IDbConnection>
    {
        private readonly ConcurrentStack<IDbConnection> connections = new ConcurrentStack<IDbConnection>();

        public IDbConnection Create()
        {
            if (connections.TryPop(out var connection))
            {
                return connection;
            }

            connection = new SqlConnection(connectionString);
            connection.Open();

            return connection;
        }

        public bool Return(IDbConnection obj)
        {
            Debug.Assert(obj.State == ConnectionState.Open, "Connection is not open");
            connections.Push(obj);
            return true;
        }
    }
}

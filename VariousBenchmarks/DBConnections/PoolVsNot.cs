using Dapper;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.ObjectPool;

using System.Data;

using Testcontainers.MsSql;

using Various.DBConnections;

namespace VariousBenchmarks.DBConnections
{
    // WIP
    // | Method           | Mean     | Error    | StdDev   | Ratio | RatioSD |
    // |----------------- |---------:|---------:|---------:|------:|--------:|
    // | CreateNew        | 729.3 us | 27.00 us | 17.86 us |  1.00 |    0.03 |
    // | CreateNewPooling | 728.0 us | 17.90 us | 10.65 us |  1.00 |    0.03 |
    // | FromPool         | 704.1 us | 24.34 us | 16.10 us |  0.97 |    0.03 |
    [SimpleJob(iterationCount: 10)]
    public class PoolVsNot
    {
        private string connectionString = null!;
        private string connectionStringPooling = null!;
        private DefaultObjectPool<IDbConnection> pool = null!;

        [GlobalSetup]
        public async Task SetUp()
        {
            var msSqlContainer = new MsSqlBuilder().Build();
            await msSqlContainer.StartAsync();

            connectionString = msSqlContainer.GetConnectionString();
            connectionStringPooling = msSqlContainer.GetConnectionString() + ";Pooling=true";

            pool = new DefaultObjectPool<IDbConnection>(new ConnectionPoolPolicy(connectionString));
        }

        [Benchmark(Baseline = true)]
        public async Task CreateNew()
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("SELECT 1");
        }

        [Benchmark]
        public async Task CreateNewPooling()
        {
            using var connection = new SqlConnection(connectionStringPooling);
            await connection.ExecuteAsync("SELECT 1");
        }

        [Benchmark]
        public async Task FromPool()
        {
            var connection = pool.Get();
            await connection.ExecuteAsync("SELECT 1");
            pool.Return(connection);
        }
    }
}

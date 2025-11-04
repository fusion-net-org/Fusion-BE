using Fusion.Repository.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Commons.Background.Locks
{
    public sealed class SqlServerJobLock : IJobLock
    {
        private readonly IDbContextFactory<FusionDbContext> _dbFactory;
        public SqlServerJobLock(IDbContextFactory<FusionDbContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<IAsyncDisposable?> TryAcquireAsync(string resourceKey, TimeSpan ttl, CancellationToken ct)
        {
            var db = await _dbFactory.CreateDbContextAsync(ct);
            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                            DECLARE @res int;
                            EXEC @res = sp_getapplock
                            @Resource = @p0, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0;
                            SELECT @res;";
            var p = cmd.CreateParameter(); p.ParameterName = "@p0"; p.Value = resourceKey;
            cmd.Parameters.Add(p);

            var result = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            if (result < 0) { await conn.CloseAsync(); await db.DisposeAsync(); return null; }
            return new Releaser(conn, db, resourceKey);
        }

        private sealed class Releaser : IAsyncDisposable
        {
            private readonly DbConnection _conn; private readonly DbContext _db; private readonly string _key;
            public Releaser(DbConnection conn, DbContext db, string key) { _conn = conn; _db = db; _key = key; }
            public async ValueTask DisposeAsync()
            {
                try
                {
                    await using var cmd = _conn.CreateCommand();
                    cmd.CommandText = "EXEC sp_releaseapplock @Resource=@p0, @LockOwner='Session';";
                    var p = cmd.CreateParameter(); p.ParameterName = "@p0"; p.Value = _key;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }
                finally { await _conn.CloseAsync(); await _db.DisposeAsync(); }
            }
        }
    }
}
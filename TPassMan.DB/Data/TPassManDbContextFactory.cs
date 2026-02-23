using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TPassMan.DB.Data
{
    // Design-time factory used by EF tools to create the DbContext.
    public class TPassManDbContextFactory : IDesignTimeDbContextFactory<TPassManDbContext>
    {
        public TPassManDbContext CreateDbContext(string[] args)
        {
            // You can set environment variable TPASSMAN_DB to override the path used for migrations:
            // e.g. set TPASSMAN_DB=./data/tpassman.db
            var dbPathEnv = Environment.GetEnvironmentVariable("TPASSMAN_DB") ?? "data/tpassman.db";
            var dbPath = Path.GetFullPath(dbPathEnv);

            // Ensure directory exists so Sqlite can create the file.
            var dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            var optionsBuilder = new DbContextOptionsBuilder<TPassManDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new TPassManDbContext(optionsBuilder.Options);
        }
    }
}
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TPassMan.DB.Data;
using TPassMan.DB.Dto;
using TPassMan.DB.Mappings;
using TPassMan.DB.Models;

namespace TPassMan.DB.Services;

public class PasswordRepositoryEf : IDisposable
{
    private readonly string _connectionString;
    private readonly EncryptionService _encryption;

    public PasswordRepositoryEf(string dbFilePath, EncryptionService encryptionService)
    {
        if (string.IsNullOrWhiteSpace(dbFilePath)) throw new ArgumentNullException(nameof(dbFilePath));
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbFilePath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
        _encryption = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    private TPassManDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TPassManDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        return new TPassManDbContext(options);
    }

    public async Task InitializeAsync()
    {
        var builder = new SqliteConnectionStringBuilder(_connectionString);
        var dbPath = builder.DataSource!;
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        using var ctx = CreateContext();
        // Apply migrations if present
        await ctx.Database.MigrateAsync();
    }

    public async Task AddAsync(PasswordDto dto)
    {
        dto.Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
        dto.CreatedUtc = DateTime.UtcNow;
        dto.UpdatedUtc = DateTime.UtcNow;

        var entity = dto.ToEntity();
        entity.EncryptedPassword = _encryption.Encrypt(dto.Password);

        using var ctx = CreateContext();
        ctx.Passwords.Add(entity);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(PasswordDto dto)
    {
        dto.UpdatedUtc = DateTime.UtcNow;
        using var ctx = CreateContext();
        var existing = await ctx.Passwords.FindAsync(dto.Id);
        if (existing == null) throw new InvalidOperationException("Record not found");

        existing.Title = dto.Title;
        existing.Username = dto.Username;
        existing.EncryptedPassword = _encryption.Encrypt(dto.Password);
        existing.Notes = dto.Notes;
        existing.UpdatedUtc = dto.UpdatedUtc;

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        using var ctx = CreateContext();
        var entity = await ctx.Passwords.FindAsync(id);
        if (entity != null)
        {
            ctx.Passwords.Remove(entity);
            await ctx.SaveChangesAsync();
        }
    }

    public async Task<PasswordDto?> GetAsync(Guid id)
    {
        using var ctx = CreateContext();
        var entity = await ctx.Passwords.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return null;
        var decrypted = _encryption.Decrypt(entity.EncryptedPassword);
        return entity.ToDto(decrypted);
    }

    public async Task<IEnumerable<PasswordDto>> GetAllAsync()
    {
        using var ctx = CreateContext();
        var list = await ctx.Passwords.AsNoTracking().OrderBy(p => p.Title).ToListAsync();
        var result = new List<PasswordDto>(list.Count);
        foreach (var e in list)
        {
            var decrypted = _encryption.Decrypt(e.EncryptedPassword);
            result.Add(e.ToDto(decrypted));
        }
        return result;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
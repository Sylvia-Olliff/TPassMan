using System;
using System.Diagnostics;

namespace TPassMan.UI.Models;

/// <summary>
/// Type of credential this entry represents.
/// </summary>
public enum CredentialType
{
    Application,
    WebDomain
}

/// <summary>
/// Represents a securely-held password entry associated with a user.
/// The password is stored in a char[] buffer that is zeroed when cleared or disposed.
/// This type is disposable to ensure the in-memory password is cleared as soon as feasible.
/// </summary>
[DebuggerDisplay("{Name} ({Type}) - UserId = {UserId}")]
public sealed class PasswordEntry : IDisposable
{
    private char[]? _password;

    /// <summary>
    /// Unique identifier for this password entry.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// User id this password belongs to.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Whether the entry is for an application or a web domain.
    /// </summary>
    public CredentialType Type { get; init; }

    /// <summary>
    /// Application name or web domain name depending on <see cref="Type"/>.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// True when a password is present in memory.
    /// </summary>
    public bool HasPassword => _password != null && _password.Length > 0;

    /// <summary>
    /// Construct a new PasswordEntry and set its password from the provided span.
    /// </summary>
    public PasswordEntry(Guid userId, CredentialType type, string name, ReadOnlySpan<char> password)
    {
        UserId = userId;
        Type = type;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        SetPassword(password);
    }

    /// <summary>
    /// Sets or replaces the in-memory password. Clears any previously stored password.
    /// The incoming password is copied, callers remain responsible for clearing their source if needed.
    /// </summary>
    public void SetPassword(ReadOnlySpan<char> password)
    {
        ClearPassword();

        if (password.Length == 0)
        {
            _password = Array.Empty<char>();
            return;
        }

        _password = new char[password.Length];
        password.CopyTo(_password);
    }

    /// <summary>
    /// Returns a copy of the password buffer. Caller is responsible for clearing the returned array promptly.
    /// Returns null if there is no password set.
    /// </summary>
    public char[]? GetPasswordCopy()
    {
        if (_password == null) return null;
        var copy = new char[_password.Length];
        Array.Copy(_password, copy, _password.Length);
        return copy;
    }

    /// <summary>
    /// Clears and removes the in-memory password.
    /// </summary>
    public void ClearPassword()
    {
        if (_password == null) return;

        for (var i = 0; i < _password.Length; i++)
            _password[i] = '\0';

        _password = null;
    }

    /// <summary>
    /// Dispose pattern to ensure memory is cleared.
    /// </summary>
    public void Dispose()
    {
        ClearPassword();
        GC.SuppressFinalize(this);
    }
}
using System;

namespace TPassMan.DB.Dto;

// Decoupled DTO for use by UI code. Map to/from TPassMan.UI.PasswordEntry in UI project.
public class PasswordDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // plaintext in memory
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
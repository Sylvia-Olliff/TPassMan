using System;
using TPassMan.DB.Dto;
using TPassMan.DB.Models;

namespace TPassMan.DB.Mappings;

internal static class PasswordMappings
{
    public static PasswordEntity ToEntity(this PasswordDto dto)
    {
        return new PasswordEntity
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Title = dto.Title,
            Username = dto.Username,
            // EncryptedPassword will be set by repository
            Notes = dto.Notes,
            CreatedUtc = dto.CreatedUtc == default ? DateTime.UtcNow : dto.CreatedUtc,
            UpdatedUtc = dto.UpdatedUtc == default ? DateTime.UtcNow : dto.UpdatedUtc
        };
    }

    public static PasswordDto ToDto(this PasswordEntity entity, string decryptedPassword)
    {
        return new PasswordDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Username = entity.Username,
            Password = decryptedPassword,
            Notes = entity.Notes,
            CreatedUtc = entity.CreatedUtc,
            UpdatedUtc = entity.UpdatedUtc
        };
    }
}
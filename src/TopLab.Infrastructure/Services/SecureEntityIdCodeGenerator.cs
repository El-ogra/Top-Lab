using System.Security.Cryptography;
using TopLab.Application.Features.ExternalEntities.Common.Interfaces;

namespace TopLab.Infrastructure.Services;

public sealed class SecureEntityIdCodeGenerator : IEntityIdCodeGenerator
{
    public const int CodeLength = 12;

    internal const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public string Generate()
    {
        return string.Create(CodeLength, Alphabet, static (span, alphabet) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
        });
    }
}

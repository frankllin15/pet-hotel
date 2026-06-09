using System.Buffers.Text;
using System.Text;

namespace PetHotel.SharedKernel;

/// <summary>
/// Página de uma lista paginada por keyset/cursor (docs/04). <see cref="NextCursor"/>
/// é nulo quando não há mais itens.
/// </summary>
public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor)
{
    public static CursorPage<T> Empty { get; } = new([], null);
}

/// <summary>
/// Cursor de keyset opaco: posição estável (CreatedAt, Id) numa ordenação por
/// data de criação decrescente com Id como desempate. Serializado em base64url
/// para ser tratado como string opaca pelo cliente.
/// </summary>
public readonly record struct Cursor(DateTimeOffset Timestamp, Guid Id)
{
    public string Encode()
    {
        var raw = $"{Timestamp.UtcTicks}:{Id:N}";
        return Base64Url.EncodeToString(Encoding.UTF8.GetBytes(raw));
    }

    public static bool TryDecode(string? value, out Cursor cursor)
    {
        cursor = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            var raw = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(value));
            var separator = raw.IndexOf(':');
            if (separator <= 0)
            {
                return false;
            }

            if (!long.TryParse(raw[..separator], out var ticks) ||
                !Guid.TryParse(raw[(separator + 1)..], out var id))
            {
                return false;
            }

            cursor = new Cursor(new DateTimeOffset(ticks, TimeSpan.Zero), id);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

namespace PetHotel.Notifications.Domain.Reports;

/// <summary>Identificador tipado de uma mensagem ao tutor (relatório).</summary>
public readonly record struct OutboundMessageId(Guid Value)
{
    public static OutboundMessageId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

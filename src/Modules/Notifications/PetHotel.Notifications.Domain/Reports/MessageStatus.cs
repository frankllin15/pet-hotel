namespace PetHotel.Notifications.Domain.Reports;

/// <summary>Estado da mensagem/relatório ao tutor.</summary>
public enum MessageStatus
{
    /// <summary>Rascunho montado, ainda não enviado/compartilhado.</summary>
    Draft = 1,

    /// <summary>Marcado como enviado/compartilhado (sem canal real por ora).</summary>
    Sent = 2,
}

namespace PetHotel.Registry.Domain.Tutors;

/// <summary>
/// Finalidades de tratamento que dependem de consentimento do titular (LGPD). O serviço
/// essencial de hospedagem se apoia em execução de contrato; estas finalidades, não.
/// </summary>
public enum ConsentType
{
    /// <summary>Uso de imagem do pet (fotos em redes sociais / divulgação).</summary>
    ImageUse = 1,

    /// <summary>Comunicações de marketing e promoções.</summary>
    Marketing = 2,

    /// <summary>Compartilhamento de dados com parceiros (ex.: pet shop, seguradora).</summary>
    DataSharing = 3,
}

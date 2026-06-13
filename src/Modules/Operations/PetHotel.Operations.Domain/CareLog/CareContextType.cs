namespace PetHotel.Operations.Domain.CareLog;

/// <summary>
/// Contexto de presença ao qual a ocorrência do diário pertence. Hoje só estadia de hotel;
/// a creche (Fase 3) entra como um novo valor, sem mudar o agregado.
/// </summary>
public enum CareContextType
{
    /// <summary>Estadia de hotel (reserva do Booking).</summary>
    HotelStay = 1,

    /// <summary>Frequência em creche (módulo Daycare, Fase 3).</summary>
    DaycareAttendance = 2,
}

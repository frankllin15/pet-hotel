using PetHotel.Registry.Domain.Tutors;

namespace PetHotel.Registry.Application.Tutors.SetTutorConsents;

/// <summary>Registra as decisões de consentimento LGPD de um tutor (uma por finalidade).</summary>
public sealed record SetTutorConsents(
    Guid TutorId,
    IReadOnlyList<ConsentDecisionInput> Consents);

/// <summary>Decisão de consentimento para uma finalidade.</summary>
public sealed record ConsentDecisionInput(ConsentType Type, bool Granted);

using PetHotel.Registry.Domain.Pets;

namespace PetHotel.Registry.Domain.Packs;

/// <summary>Sinal de atenção de compatibilidade de um pet numa matilha.</summary>
public enum PackCompatibilityFlag
{
    /// <summary>Reatividade alta — pode reagir mal ao convívio.</summary>
    Reactive = 1,

    /// <summary>Baixa sociabilidade — tende a não se dar bem em grupo.</summary>
    LowSociability = 2,
}

/// <summary>
/// Critério objetivo de compatibilidade de matilha, baseado na avaliação comportamental do
/// pet (docs/03). É um ALERTA, não um bloqueio — a equipe decide. Centralizado aqui para
/// ser testável e reutilizado pelo lado de leitura (lista e ficha da matilha).
/// </summary>
public static class PackCompatibility
{
    public static IReadOnlyList<PackCompatibilityFlag> Evaluate(BehaviorLevel? sociability, BehaviorLevel? reactivity)
    {
        var flags = new List<PackCompatibilityFlag>();

        if (reactivity == BehaviorLevel.High)
        {
            flags.Add(PackCompatibilityFlag.Reactive);
        }

        if (sociability == BehaviorLevel.Low)
        {
            flags.Add(PackCompatibilityFlag.LowSociability);
        }

        return flags;
    }
}

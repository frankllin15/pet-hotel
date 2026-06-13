using PetHotel.Notifications.Domain.Reports;
using PetHotel.SharedKernel;

namespace PetHotel.Notifications.UnitTests;

public class OutboundMessageTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly TutorReference Tutor = new(Guid.NewGuid());
    private static readonly PetReference Pet = new(Guid.NewGuid());
    private static readonly Guid Stay = Guid.NewGuid();
    private static readonly DateOnly Day = new(2026, 6, 13);
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-06-13T18:00:00Z");

    private static OutboundMessage NewDraft() =>
        OutboundMessage.CreateReport(Tenant, Tutor, Pet, Stay, Day, "Relatório do Rex", "Comeu bem; brincou.").Value;

    [Fact]
    public void Criar_relatorio_valido_fica_em_rascunho()
    {
        var result = OutboundMessage.CreateReport(Tenant, Tutor, Pet, Stay, Day, " Relatório ", " conteúdo ");

        Assert.True(result.IsSuccess);
        Assert.Equal(MessageStatus.Draft, result.Value.Status);
        Assert.Equal("Relatório", result.Value.Title); // trim
        Assert.Equal("conteúdo", result.Value.Content);
        Assert.Null(result.Value.SentAt);
    }

    [Fact]
    public void Criar_sem_conteudo_falha()
    {
        var result = OutboundMessage.CreateReport(Tenant, Tutor, Pet, Stay, Day, "Título", "  ");

        Assert.True(result.IsFailure);
        Assert.Equal("report.content_required", result.Error.Code);
    }

    [Fact]
    public void Criar_sem_tutor_falha()
    {
        var result = OutboundMessage.CreateReport(Tenant, new TutorReference(Guid.Empty), Pet, Stay, Day, "T", "C");

        Assert.True(result.IsFailure);
        Assert.Equal("report.tutor_required", result.Error.Code);
    }

    [Fact]
    public void Marcar_enviado_carimba_e_bloqueia_reenvio()
    {
        var report = NewDraft();

        var first = report.MarkSent(Now);
        Assert.True(first.IsSuccess);
        Assert.Equal(MessageStatus.Sent, report.Status);
        Assert.Equal(Now, report.SentAt);

        var second = report.MarkSent(Now.AddMinutes(1));
        Assert.True(second.IsFailure);
        Assert.Equal("report.already_sent", second.Error.Code);
    }
}

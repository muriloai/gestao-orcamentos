using GestaoOrcamentos.Web.Models;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Web.Data;

public class GestaoOrcamentosDbContext(DbContextOptions<GestaoOrcamentosDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ConfiguracaoNegocio> ConfiguracoesNegocio => Set<ConfiguracaoNegocio>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
    public DbSet<ItemOrcamento> ItensOrcamento => Set<ItemOrcamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(cliente => cliente.Nome).HasMaxLength(150).IsRequired();
            entity.Property(cliente => cliente.PessoaContato).HasMaxLength(150);
            entity.Property(cliente => cliente.Telefone).HasMaxLength(30);
            entity.Property(cliente => cliente.Email).HasMaxLength(254);
            entity.Property(cliente => cliente.Cep).HasMaxLength(9);
            entity.Property(cliente => cliente.Logradouro).HasMaxLength(200);
            entity.Property(cliente => cliente.Numero).HasMaxLength(20);
            entity.Property(cliente => cliente.Complemento).HasMaxLength(100);
            entity.Property(cliente => cliente.Bairro).HasMaxLength(100);
            entity.Property(cliente => cliente.Cidade).HasMaxLength(100);
            entity.Property(cliente => cliente.Uf).HasMaxLength(2);
        });

        modelBuilder.Entity<ConfiguracaoNegocio>(entity =>
        {
            entity.ToTable(table => table.HasCheckConstraint("CK_ConfiguracoesNegocio_Id", "[Id] = 1"));
            entity.Property(negocio => negocio.Id).ValueGeneratedNever();
            entity.Property(negocio => negocio.Nome).HasMaxLength(150).IsRequired();
            entity.Property(negocio => negocio.Telefone).HasMaxLength(30);
            entity.Property(negocio => negocio.Email).HasMaxLength(254);
            entity.Property(negocio => negocio.Cep).HasMaxLength(9);
            entity.Property(negocio => negocio.Logradouro).HasMaxLength(200);
            entity.Property(negocio => negocio.Numero).HasMaxLength(20);
            entity.Property(negocio => negocio.Complemento).HasMaxLength(100);
            entity.Property(negocio => negocio.Bairro).HasMaxLength(100);
            entity.Property(negocio => negocio.Cidade).HasMaxLength(100);
            entity.Property(negocio => negocio.Uf).HasMaxLength(2);
        });

        modelBuilder.Entity<Orcamento>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Orcamentos_Situacao", "[Situacao] IN (0, 1, 2, 3)");
                table.HasCheckConstraint("CK_Orcamentos_DadosRegistrados",
                    "([Situacao] = 0 AND [ClienteRegistrado_Nome] IS NULL AND [NegocioRegistrado_Nome] IS NULL) " +
                    "OR ([Situacao] IN (1, 2, 3) AND [ClienteRegistrado_Nome] IS NOT NULL AND [NegocioRegistrado_Nome] IS NOT NULL)");
            });
            entity.Property(orcamento => orcamento.Titulo).HasMaxLength(150).IsRequired();
            entity.Property(orcamento => orcamento.DataEmissao).HasColumnType("date");
            entity.Property(orcamento => orcamento.Validade).HasColumnType("date");
            entity.HasOne(orcamento => orcamento.Cliente)
                .WithMany()
                .HasForeignKey(orcamento => orcamento.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(orcamento => orcamento.Itens)
                .WithOne()
                .HasForeignKey(item => item.OrcamentoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.OwnsOne(orcamento => orcamento.ClienteRegistrado, ConfigurarDadosRegistrados);
            entity.OwnsOne(orcamento => orcamento.NegocioRegistrado, ConfigurarDadosRegistrados);
        });

        modelBuilder.Entity<ItemOrcamento>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_ItensOrcamento_Quantidade", "[Quantidade] > 0");
                table.HasCheckConstraint("CK_ItensOrcamento_PrecoUnitario", "[PrecoUnitario] >= 0");
            });
            entity.Property(item => item.Descricao).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Unidade).HasMaxLength(30).IsRequired();
            entity.Property(item => item.Quantidade).HasPrecision(18, 3);
            entity.Property(item => item.PrecoUnitario).HasPrecision(18, 2);
        });
    }

    private static void ConfigurarDadosRegistrados<TOwner>(
        Microsoft.EntityFrameworkCore.Metadata.Builders.OwnedNavigationBuilder<TOwner, DadosParteOrcamento> entity)
        where TOwner : class
    {
        entity.Property(dados => dados.Nome).HasMaxLength(150).IsRequired();
        entity.Property(dados => dados.PessoaContato).HasMaxLength(150);
        entity.Property(dados => dados.Telefone).HasMaxLength(30);
        entity.Property(dados => dados.Email).HasMaxLength(254);
        entity.Property(dados => dados.Cep).HasMaxLength(9);
        entity.Property(dados => dados.Logradouro).HasMaxLength(200);
        entity.Property(dados => dados.Numero).HasMaxLength(20);
        entity.Property(dados => dados.Complemento).HasMaxLength(100);
        entity.Property(dados => dados.Bairro).HasMaxLength(100);
        entity.Property(dados => dados.Cidade).HasMaxLength(100);
        entity.Property(dados => dados.Uf).HasMaxLength(2);
    }
}
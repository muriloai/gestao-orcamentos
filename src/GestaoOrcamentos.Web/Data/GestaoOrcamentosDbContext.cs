using GestaoOrcamentos.Web.Models;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Web.Data;

public class GestaoOrcamentosDbContext(DbContextOptions<GestaoOrcamentosDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ConfiguracaoNegocio> ConfiguracoesNegocio => Set<ConfiguracaoNegocio>();

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
    }
}
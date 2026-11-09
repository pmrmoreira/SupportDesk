using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Data;

public class SupportDeskDbContext : IdentityDbContext<ApplicationUser>
{
    public SupportDeskDbContext(DbContextOptions<SupportDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();
     
    public DbSet<TicketAiSuggestion> AiSuggestions => Set<TicketAiSuggestion>();
    public DbSet<TicketAiSummary> AiSummaries => Set<TicketAiSummary>();
    public DbSet<KnowledgeArticle> KnowledgeArticles => Set<KnowledgeArticle>();
    public DbSet<KnowledgeChunk> KnowledgeChunks => Set<KnowledgeChunk>();
    public DbSet<TicketAiDraft> AiDrafts => Set<TicketAiDraft>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aula 18: a extensão pgvector (tipo vector, distâncias e índices HNSW).
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<ApplicationUser>().Property(u => u.DisplayName).HasMaxLength(80);



        modelBuilder.Entity<Ticket>(ticket =>
        {
            ticket.Property(t => t.Title).HasMaxLength(120);
            ticket.Property(t => t.Description).HasMaxLength(2000);
            ticket.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            ticket.Property(t => t.Category).HasConversion<string>().HasMaxLength(20);
            ticket.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Tickets_Status", "\"Status\" IN ('Open', 'InProgress', 'Resolved')");
                t.HasCheckConstraint("CK_Tickets_Priority", "\"Priority\" BETWEEN 1 AND 3");
            });
            ticket.HasOne(t => t.CreatedBy).WithMany().HasForeignKey(t => t.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(comment =>
        {
            comment.Property(c => c.Author).HasMaxLength(80);
            comment.Property(c => c.Text).HasMaxLength(2000);
            comment.HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Restrict);
            comment.HasOne<ApplicationUser>().WithMany().HasForeignKey(c => c.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<TicketAiSuggestion>(suggestion =>
        {
            suggestion.Property(s => s.Category).HasConversion<string>().HasMaxLength(20);
            suggestion.Property(s => s.Decision).HasConversion<string>().HasMaxLength(20);
            suggestion.Property(s => s.Summary).HasMaxLength(200);
            suggestion.Property(s => s.Model).HasMaxLength(100);
            suggestion.HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(s => s.TicketId)
                .OnDelete(DeleteBehavior.Restrict);
            suggestion.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.DecidedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<TicketAiSummary>(summary =>
        {
            summary.Property(s => s.Problem).HasMaxLength(500);
            summary.Property(s => s.Model).HasMaxLength(100);
            // Um resumo por pedido: gerar outro substitui o anterior.
            summary.HasIndex(s => s.TicketId).IsUnique();
            summary.HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(s => s.TicketId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<KnowledgeArticle>(article =>
        {
            article.Property(a => a.Slug).HasMaxLength(100);
            article.HasIndex(a => a.Slug).IsUnique();
            article.Property(a => a.Title).HasMaxLength(200);
            article.Property(a => a.ContentHash).HasMaxLength(64);
            // Os trechos só existem por causa do artigo: apagar o artigo apaga-os (Cascade).
            article.HasMany(a => a.Chunks)
                .WithOne(c => c.Article)
                .HasForeignKey(c => c.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KnowledgeChunk>(chunk =>
        {
            chunk.Property(c => c.Heading).HasMaxLength(300);
            // Pesquisa textual: o PostgreSQL calcula o tsvector (configuração portuguese) e mantém um índice GIN.
            chunk.HasGeneratedTsVectorColumn(c => c.SearchVector, "portuguese", c => new { c.Heading, c.Text })
                .HasIndex(c => c.SearchVector)
                .HasMethod("GIN");
            // Pesquisa semântica: 1024 dimensões (as do bge-m3) e um índice HNSW para a distância do cosseno.
            chunk.Property(c => c.Embedding).HasColumnType("vector(1024)");
            chunk.HasIndex(c => c.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");
            // Exercício C da aula 18: o modelo que gerou o vetor.
            chunk.Property(c => c.EmbeddingModel).HasMaxLength(100);
        });

        // Aula 19: os rascunhos de resposta da IA, com as fontes que o modelo recebeu.
        modelBuilder.Entity<TicketAiDraft>(draft =>
        {
            draft.Property(d => d.ClarifyingQuestion).HasMaxLength(500);
            draft.Property(d => d.Model).HasMaxLength(100);
            draft.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
            // As fontes só existem com o rascunho (Cascade); o rascunho não apaga o pedido nem o técnico (Restrict).
            draft.HasMany(d => d.Sources)
                .WithOne()
                .HasForeignKey(s => s.DraftId)
                .OnDelete(DeleteBehavior.Cascade);
            draft.HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.Restrict);
            draft.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(d => d.DecidedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketAiDraftSource>(source =>
        {
            // Sem DbSet, a tabela teria o nome da classe; assim fica ao lado de AiDrafts.
            source.ToTable("AiDraftSources");
            source.Property(s => s.ArticleSlug).HasMaxLength(100);
            source.Property(s => s.Heading).HasMaxLength(300);
        });
    }
}

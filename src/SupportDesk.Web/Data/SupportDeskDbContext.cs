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
    // Aula 15, ciclo 2: com o DbSet, a tabela chama-se "AiSuggestions" (sem ele, o EF usa o nome da classe).
    public DbSet<TicketAiSuggestion> AiSuggestions => Set<TicketAiSuggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
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
    }
}

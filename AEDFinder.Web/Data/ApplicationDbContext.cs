using AEDFinder.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AEDFinder.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
	public DbSet<Aed> Aeds => Set<Aed>();

	public DbSet<AedAvailabilityWindow> AedAvailabilityWindows => Set<AedAvailabilityWindow>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		  builder.Entity<Aed>(entity =>
		  {
			entity.HasIndex(a => new { a.PostalCode, a.City });
			entity.Property(a => a.OwnerId).IsRequired();
			entity.HasOne(a => a.Owner)
				.WithMany()
				.HasForeignKey(a => a.OwnerId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
			entity.HasMany(a => a.AvailabilityWindows)
				  .WithOne(w => w.Aed)
				  .HasForeignKey(w => w.AedId)
				  .OnDelete(DeleteBehavior.Cascade);
		});

		builder.Entity<AedAvailabilityWindow>(entity =>
		{
			entity.HasIndex(w => new { w.AedId, w.DayOfWeek });
		});
	}
}

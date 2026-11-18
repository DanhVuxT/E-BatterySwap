using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EBatterySwap.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Battery> Batteries => Set<Battery>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<BatteryAssignment> BatteryAssignments => Set<BatteryAssignment>();
    public DbSet<BatteryTelemetry> BatteryTelemetries => Set<BatteryTelemetry>();
    public DbSet<ChargingSession> ChargingSessions => Set<ChargingSession>();
    public DbSet<SwapSession> SwapSessions => Set<SwapSession>();
    public DbSet<SwapItem> SwapItems => Set<SwapItem>();
    public DbSet<StationCommand> StationCommands => Set<StationCommand>();
    public DbSet<StationEvent> StationEvents => Set<StationEvent>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}

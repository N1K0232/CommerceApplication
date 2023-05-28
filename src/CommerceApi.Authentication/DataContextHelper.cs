using CommerceApi.Authentication.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CommerceApi.Authentication;

public partial class AuthenticationDataContext
{
    protected virtual void OnSavedChanges(object sender, SaveChangesEventArgs e)
    {
    }

    protected virtual void OnSavingChanges(object sender, SavingChangesEventArgs e)
    {
    }

    protected virtual void OnSaveChangesFailed(object sender, SaveChangesFailedEventArgs e)
    {
    }

    partial void OnConfiguringCore(DbContextOptionsBuilder optionsBuilder)
    {
        SavedChanges += new EventHandler<SavedChangesEventArgs>(OnSavedChanges);
        SavingChanges += new EventHandler<SavingChangesEventArgs>(OnSavingChanges);
        SaveChangesFailed += new EventHandler<SaveChangesFailedEventArgs>(OnSaveChangesFailed);
    }

    partial void OnModelCreatingCore(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    modelBuilder.Entity(entity.Name)
                    .Property(property.Name)
                    .HasConversion(trimStringConverter);
                }
            }
        }
    }
}
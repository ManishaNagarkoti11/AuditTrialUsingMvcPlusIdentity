using AuditTrialUsingMvcPlusIdentity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditTrialUsingMvcPlusIdentity.Data
{
    public abstract class AuditableIdentityContext : IdentityDbContext
    {
        public AuditableIdentityContext(DbContextOptions options) : base(options)
        {

        }
        //add migration and update
        public DbSet<Audit> AuditLogs { get; set; }

        //Create a SaveChangeAsync() similar to the base class that Take userId as parameter. We will be updating the product controller class to adapt this method and provide the current logged in userID.
        public virtual async Task<int> SaveChangesAsync(string userId = null)
        {
            OnBeforeSaveChanges(userId);
            var result = await base.SaveChangesAsync();
            return result;
        }
        private void OnBeforeSaveChanges(string userId)
        {
            //Scans the entities for changes
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            //Loop through the collection of all changed entity
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;
                var auditEntry = new AuditEntry(entry);
                //Get the Table Name from the entity object.
                auditEntry.TableName = entry.Entity.GetType().Name;
                auditEntry.UserId = userId;
                auditEntries.Add(auditEntry);
                //Loops through all the properties of the Entity. In our demonstration, it is going to be the Product Entity.
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    //We use switch case to detect the state of entity (Added, Deleted or Modified). If the entity is created , we assign the Create enum to the AuditType property and add the property to the NewValues dictionary.
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = Enums.AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = Enums.AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = Enums.AuditType.Edit;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            //we convert all the AuditEntries to Audits and save the changes at Line 24.
            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(auditEntry.ToAudit());
            }
        }
    }
}

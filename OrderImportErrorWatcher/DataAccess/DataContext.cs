/** 
 * This file is part of the OrderImportErrorWatcher project.
 * Copyright (c) 2014 Dai Nguyen
 * Author: Dai Nguyen
**/

using OrderImportErrorWatcher.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace OrderImportErrorWatcher.DataAccess
{
    public class DataContext : DbContext
    {
        public DataContext()
        {
            Database.SetInitializer<DataContext>(null);
        }

        public DbSet<FileImport> FileImports { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}

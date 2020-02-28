﻿using FIRSTShares.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIRSTShares.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options = null) : base(options) { }

        public DbSet<Time> Times { get; set; }
        public DbSet<Permissao> Permissoes { get; set; }
        public DbSet<Cargo> Cargos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cargo>()
                .HasMany(p => p.Permissoes);

            modelBuilder.Entity<Usuario>()
                .HasOne( t => t.Time);

            modelBuilder.Entity<Usuario>()
                .HasOne(c => c.Cargo);
        }
    }
}
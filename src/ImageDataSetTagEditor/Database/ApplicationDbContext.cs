using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace ImageDataSetTagEditor.Database;

public class ApplicationDbContext : DbContext
{
    private readonly string _dbPath;
    
    public ApplicationDbContext(string dbPath) => _dbPath = dbPath;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");

    public DbSet<Image> Images { get; set; }
    public DbSet<Tag> Tags { get; set; }
}
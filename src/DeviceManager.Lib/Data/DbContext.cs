using System;
using System.Collections.Generic;
using DeviceManager.Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Lib.Data;

public partial class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbContext()
    {
    }

    public DbContext(DbContextOptions<DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceEmployee> DeviceEmployees { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC07D193917C");

            entity.ToTable("Account");

            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("UQ__Account__536C85E4");

            entity.HasOne(d => d.Employee).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Account__Employe__1446FBA6");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Account__RoleId__153B1FDF");
            
            base.OnModelCreating(modelBuilder);
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Device__3214EC0762A60B0E");

            entity.ToTable("Device");

            entity.Property(e => e.AdditionalProperties)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.DeviceType).WithMany(p => p.Devices)
                .HasForeignKey(d => d.DeviceTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Device__DeviceTy__18178C8A");
        });

        modelBuilder.Entity<DeviceEmployee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeviceEm__3214EC078ECAF137");

            entity.ToTable("DeviceEmployee");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceEmployees)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK__DeviceEmp__Devic__1AF3F935");

            entity.HasOne(d => d.Employee).WithMany(p => p.DeviceEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__DeviceEmp__Emplo__1BE81D6E");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeviceTy__3214EC07F901624F");

            entity.ToTable("DeviceType");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC079D7FC86A");

            entity.ToTable("Employee");

            entity.Property(e => e.Salary).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Person).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PersonId)
                .HasConstraintName("FK__Employee__Person__116A8EFB");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .HasConstraintName("FK__Employee__Positi__10766AC2");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Person__3214EC072D5645F2");

            entity.ToTable("Person");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MiddleName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PassportNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Position__3214EC07CBBC4A23");

            entity.ToTable("Position");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC075731E6D4");

            entity.ToTable("Role");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("UQ__Role__737584F6");
        });

      
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" }
        );

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

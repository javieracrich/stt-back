﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Services;

namespace Services.Migrations
{
    [DbContext(typeof(SttContext))]
    [Migration("20190528124647_1")]
    partial class _1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Domain.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CardCode")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<DateTime?>("Created");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<bool?>("IsDisabled");

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<DateTime?>("Updated");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("CardCode")
                        .IsUnique()
                        .HasFilter("[CardCode] IS NOT NULL");

                    b.HasIndex("UserId");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("Domain.Device", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("Created");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<string>("DeviceCode")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<bool?>("IsDisabled");

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<int?>("RelatedDeviceId");

                    b.Property<int?>("SchoolId");

                    b.Property<int>("Type");

                    b.Property<DateTime?>("Updated");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.HasIndex("DeviceCode")
                        .IsUnique()
                        .HasFilter("[DeviceCode] IS NOT NULL");

                    b.HasIndex("RelatedDeviceId");

                    b.HasIndex("SchoolId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Domain.PushLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CardCode")
                        .IsRequired()
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<DateTime?>("Created");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<DateTime>("Date");

                    b.Property<string>("DeviceCode")
                        .IsRequired()
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<bool?>("IsDisabled");

                    b.Property<DateTime?>("Updated");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.HasIndex("DeviceCode", "CardCode")
                        .IsUnique();

                    b.ToTable("PushLogs");
                });

            modelBuilder.Entity("Domain.School", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<DateTime?>("Created");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<string>("Email")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<bool?>("IsDisabled");

                    b.Property<double?>("Lat");

                    b.Property<double?>("Lng");

                    b.Property<string>("LogoUrl")
                        .HasMaxLength(2000)
                        .IsUnicode(false);

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<string>("Phone")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<Guid>("SchoolCode");

                    b.Property<DateTime?>("Updated");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.ToTable("Schools");
                });

            modelBuilder.Entity("Domain.SchoolBus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("Created");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<int>("DeviceId");

                    b.Property<bool?>("IsDisabled");

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<int?>("SchoolId");

                    b.Property<DateTime?>("Updated");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.HasIndex("DeviceId")
                        .IsUnique();

                    b.HasIndex("SchoolId");

                    b.ToTable("SchoolBuses");
                });

            modelBuilder.Entity("Domain.StudentCardHistoryItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CardId");

                    b.Property<DateTime?>("Created");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<DateTime>("FromDate");

                    b.Property<bool?>("IsDisabled");

                    b.Property<DateTime?>("UntilDate");

                    b.Property<DateTime?>("Updated");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.ToTable("StudentCardHistoryItem");
                });

            modelBuilder.Entity("Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccessFailedCount");

                    b.Property<int>("Category");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasMaxLength(8000);

                    b.Property<DateTime?>("Created");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<int?>("Grade");

                    b.Property<bool?>("IsDisabled");

                    b.Property<string>("LastName")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<int?>("ParentId");

                    b.Property<string>("PasswordHash")
                        .HasMaxLength(8000);

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("PhotoUrl")
                        .HasMaxLength(2000)
                        .IsUnicode(false);

                    b.Property<int?>("SchoolBusId");

                    b.Property<int?>("SchoolId");

                    b.Property<string>("SecurityStamp")
                        .HasMaxLength(8000);

                    b.Property<string>("StudentId")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<DateTime?>("Updated");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<Guid>("UserCode");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.HasIndex("ParentId");

                    b.HasIndex("SchoolBusId");

                    b.HasIndex("SchoolId");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Domain.UserRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<int>("RoleId");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<int>("UserId");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<int>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Domain.Card", b =>
                {
                    b.HasOne("Domain.User", "User")
                        .WithMany("Cards")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Domain.Device", b =>
                {
                    b.HasOne("Domain.Device", "RelatedDevice")
                        .WithMany()
                        .HasForeignKey("RelatedDeviceId");

                    b.HasOne("Domain.School", "School")
                        .WithMany("Devices")
                        .HasForeignKey("SchoolId");
                });

            modelBuilder.Entity("Domain.SchoolBus", b =>
                {
                    b.HasOne("Domain.Device", "Device")
                        .WithOne("SchoolBus")
                        .HasForeignKey("Domain.SchoolBus", "DeviceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Domain.School", "School")
                        .WithMany()
                        .HasForeignKey("SchoolId");
                });

            modelBuilder.Entity("Domain.User", b =>
                {
                    b.HasOne("Domain.User", "Parent")
                        .WithMany("Students")
                        .HasForeignKey("ParentId");

                    b.HasOne("Domain.SchoolBus")
                        .WithMany("DriversAndSupervisors")
                        .HasForeignKey("SchoolBusId");

                    b.HasOne("Domain.School", "School")
                        .WithMany("Users")
                        .HasForeignKey("SchoolId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("Domain.UserRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("Domain.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("Domain.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.HasOne("Domain.UserRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Domain.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("Domain.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}

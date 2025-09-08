
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PeopleConnect.Infrastructure.Persistence;

#nullable disable

namespace PeopleConnect.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20250905155157_InitialCreate")]
    partial class InitialCreate
    {
        
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("PeopleConnect.Domain.Entities.ContactInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsPrimary")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("PersonId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("ContactInfos");
                });

            modelBuilder.Entity("PeopleConnect.Domain.Entities.Person", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CPF")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DataNascimento")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Nacionalidade")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Naturalidade")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Sexo")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CPF")
                        .IsUnique();

                    b.ToTable("Persons");
                });

            modelBuilder.Entity("PeopleConnect.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = new Guid("f0c747d1-664e-4700-b6ab-2def665f8936"),
                            CreatedAt = new DateTime(2025, 9, 5, 15, 51, 56, 154, DateTimeKind.Utc).AddTicks(1163),
                            PasswordHash = "$2a$11$px9uBqM3wOOghGGX/O5kY.ygZtcQuHSE6oJTPwvgMN555pkefvpSK",
                            UpdatedAt = new DateTime(2025, 9, 5, 15, 51, 56, 154, DateTimeKind.Utc).AddTicks(1172),
                            Username = "admin"
                        });
                });

            modelBuilder.Entity("PeopleConnect.Domain.Entities.ContactInfo", b =>
                {
                    b.HasOne("PeopleConnect.Domain.Entities.Person", null)
                        .WithMany("Contacts")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PeopleConnect.Domain.Entities.Person", b =>
                {
                    b.Navigation("Contacts");
                });
#pragma warning restore 612, 618
        }
    }
}

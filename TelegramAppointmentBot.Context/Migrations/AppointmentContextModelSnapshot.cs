﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TelegramAppointmentBot.Context;

#nullable disable

namespace TelegramAppointmentBot.Context.Migrations
{
    [DbContext(typeof(AppointmentContext))]
    partial class AppointmentContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("TelegramAppointmentBot.Context.Models.AppointmentHunter", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("DesiredDay")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DesiredTime")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DoctorId")
                        .HasColumnType("int");

                    b.Property<int?>("LpuId")
                        .HasColumnType("int");

                    b.Property<Guid>("PatientId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SpecialityName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Statement")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PatientId");

                    b.ToTable("Hunters");
                });

            modelBuilder.Entity("TelegramAppointmentBot.Context.Models.Profile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("Birthdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsFilled")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OMS")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Patronomyc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Surname")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("TelegramAppointmentBot.Context.Models.User", b =>
                {
                    b.Property<Guid>("SystemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CurrentHunter")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CurrentProfile")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<int>("Statement")
                        .HasColumnType("int");

                    b.HasKey("SystemId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TelegramAppointmentBot.Context.Models.Visit", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("appointmentId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("lpuId")
                        .HasColumnType("int");

                    b.Property<string>("patientId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Visits");
                });

            modelBuilder.Entity("TelegramAppointmentBot.Context.Models.AppointmentHunter", b =>
                {
                    b.HasOne("TelegramAppointmentBot.Context.Models.Profile", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("TelegramAppointmentBot.Context.Models.Profile", b =>
                {
                    b.HasOne("TelegramAppointmentBot.Context.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });
#pragma warning restore 612, 618
        }
    }
}

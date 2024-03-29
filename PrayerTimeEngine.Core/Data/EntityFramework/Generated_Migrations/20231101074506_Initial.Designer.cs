﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PrayerTimeEngine.Core.Data.EntityFramework;

#nullable disable

namespace PrayerTimeEngine.Core.Data.EntityFramework.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20231101074506_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.13");

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.FaziletCity", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CountryID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("CountryID");

                    b.ToTable("FaziletCities");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.FaziletCountry", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("FaziletCountries");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.FaziletPrayerTimes", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Asr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("CityID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Dhuhr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Fajr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Imsak")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Isha")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Maghrib")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("NextFajr")
                        .HasColumnType("TEXT");

                    b.Property<string>("Shuruq")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("FaziletPrayerTimes");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.MuwaqqitPrayerTimes", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Asr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AsrKaraha")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("AsrKarahaDegree")
                        .HasColumnType("REAL");

                    b.Property<string>("AsrMithlayn")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Dhuhr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Duha")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Fajr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("FajrDegree")
                        .HasColumnType("REAL");

                    b.Property<string>("InsertInstant")
                        .HasColumnType("TEXT");

                    b.Property<string>("Isha")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("IshaDegree")
                        .HasColumnType("REAL");

                    b.Property<string>("Ishtibaq")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("IshtibaqDegree")
                        .HasColumnType("REAL");

                    b.Property<decimal>("Latitude")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Longitude")
                        .HasColumnType("TEXT");

                    b.Property<string>("Maghrib")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("NextFajr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Shuruq")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("MuwaqqitPrayerTimes");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.SemerkandCity", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CountryID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("CountryID");

                    b.ToTable("SemerkandCities");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.SemerkandCountry", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("SemerkandCountries");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.SemerkandPrayerTimes", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Asr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("CityID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DayOfYear")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Dhuhr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Fajr")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Isha")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Maghrib")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("NextFajr")
                        .HasColumnType("TEXT");

                    b.Property<string>("Shuruq")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("SemerkandPrayerTimes");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Profile", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("LocationName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("SequenceNo")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.ProfileManagement.Models.ProfileLocationConfig", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CalculationSource")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LocationData")
                        .HasColumnType("TEXT");

                    b.Property<int>("ProfileID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("ProfileID");

                    b.ToTable("ProfileLocations");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.ProfileManagement.Models.ProfileTimeConfig", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CalculationConfiguration")
                        .HasColumnType("TEXT");

                    b.Property<int>("ProfileID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TimeType")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("ProfileID");

                    b.ToTable("ProfileConfigs");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.FaziletCity", b =>
                {
                    b.HasOne("PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.FaziletCountry", "Country")
                        .WithMany("Cities")
                        .HasForeignKey("CountryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.SemerkandCity", b =>
                {
                    b.HasOne("PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.SemerkandCountry", "Country")
                        .WithMany("Cities")
                        .HasForeignKey("CountryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.ProfileManagement.Models.ProfileLocationConfig", b =>
                {
                    b.HasOne("PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Profile", "Profile")
                        .WithMany("LocationConfigs")
                        .HasForeignKey("ProfileID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Profile");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.ProfileManagement.Models.ProfileTimeConfig", b =>
                {
                    b.HasOne("PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Profile", "Profile")
                        .WithMany("TimeConfigs")
                        .HasForeignKey("ProfileID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Profile");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.FaziletCountry", b =>
                {
                    b.Navigation("Cities");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.SemerkandCountry", b =>
                {
                    b.Navigation("Cities");
                });

            modelBuilder.Entity("PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Profile", b =>
                {
                    b.Navigation("LocationConfigs");

                    b.Navigation("TimeConfigs");
                });
#pragma warning restore 612, 618
        }
    }
}

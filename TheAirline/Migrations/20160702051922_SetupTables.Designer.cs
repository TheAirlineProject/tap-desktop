using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TheAirline.Db;

namespace TheAirline.Migrations
{
    [DbContext(typeof(AirlineContext))]
    [Migration("20160702051922_SetupTables")]
    partial class SetupTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("TheAirline.Models.General.Countries.Continent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Uid");

                    b.HasKey("Id");

                    b.ToTable("Continents");
                });

            modelBuilder.Entity("TheAirline.Models.General.Countries.Currency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Exchange");

                    b.Property<DateTime>("From");

                    b.Property<string>("Name");

                    b.Property<string>("Symbol");

                    b.Property<bool>("SymbolOnRight");

                    b.Property<DateTime>("To");

                    b.HasKey("Id");

                    b.ToTable("Currencies");
                });

            modelBuilder.Entity("TheAirline.Models.General.Countries.Region", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ContinentId");

                    b.Property<double>("FuelIndex");

                    b.Property<string>("Name");

                    b.Property<string>("Uid");

                    b.HasKey("Id");

                    b.HasIndex("ContinentId");

                    b.ToTable("Regions");
                });

            modelBuilder.Entity("TheAirline.Models.General.Difficulty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("AiLevel");

                    b.Property<double>("LoanLevel");

                    b.Property<double>("MoneyLevel");

                    b.Property<string>("Name");

                    b.Property<double>("PassengersLevel");

                    b.Property<double>("PriceLevel");

                    b.Property<double>("StartDataLevel");

                    b.HasKey("Id");

                    b.ToTable("Difficulties");
                });

            modelBuilder.Entity("TheAirline.Models.General.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ContinentId");

                    b.Property<int?>("DifficultyId");

                    b.Property<int>("Focus");

                    b.Property<bool>("MajorAirports");

                    b.Property<int>("NumOfOpponents");

                    b.Property<bool>("Paused");

                    b.Property<bool>("RandomOpponents");

                    b.Property<bool>("RealData");

                    b.Property<int?>("RegionId");

                    b.Property<bool>("SameRegion");

                    b.Property<int>("StartYear");

                    b.Property<bool>("UseDays");

                    b.HasKey("Id");

                    b.HasIndex("ContinentId");

                    b.HasIndex("DifficultyId");

                    b.HasIndex("RegionId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("TheAirline.Models.General.Settings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AirportCodeDisplay");

                    b.Property<int>("AutoSave");

                    b.Property<int>("ClearStats");

                    b.Property<bool>("CurrencyShorten");

                    b.Property<int?>("DifficultyDisplayId");

                    b.Property<int>("GameSpeed");

                    b.Property<string>("Language");

                    b.Property<bool>("MailsOnAirlineRoutes");

                    b.Property<bool>("MailsOnBadWeather");

                    b.Property<bool>("MailsOnLandings");

                    b.Property<int>("Mode");

                    b.HasKey("Id");

                    b.HasIndex("DifficultyDisplayId");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("TheAirline.Models.General.Countries.Region", b =>
                {
                    b.HasOne("TheAirline.Models.General.Countries.Continent", "Continent")
                        .WithMany("Regions")
                        .HasForeignKey("ContinentId");
                });

            modelBuilder.Entity("TheAirline.Models.General.Player", b =>
                {
                    b.HasOne("TheAirline.Models.General.Countries.Continent", "Continent")
                        .WithMany()
                        .HasForeignKey("ContinentId");

                    b.HasOne("TheAirline.Models.General.Difficulty", "Difficulty")
                        .WithMany()
                        .HasForeignKey("DifficultyId");

                    b.HasOne("TheAirline.Models.General.Countries.Region", "Region")
                        .WithMany()
                        .HasForeignKey("RegionId");
                });

            modelBuilder.Entity("TheAirline.Models.General.Settings", b =>
                {
                    b.HasOne("TheAirline.Models.General.Difficulty", "DifficultyDisplay")
                        .WithMany()
                        .HasForeignKey("DifficultyDisplayId");
                });
        }
    }
}

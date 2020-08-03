﻿// <auto-generated />
using System;
using Algoserver.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Algoserver.API.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20200729122420_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.11-servicing-32099")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Algoserver.API.Models.Statistic", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("AccountSize");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("Ip");

                    b.Property<string>("LastName");

                    b.Property<string>("Market");

                    b.Property<decimal>("RiskOverride");

                    b.Property<decimal>("SplitPositions");

                    b.Property<decimal>("StopLossRatio");

                    b.Property<int>("TimeFrameInterval");

                    b.Property<string>("TimeFramePeriodicity");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Statistics");
                });
#pragma warning restore 612, 618
        }
    }
}
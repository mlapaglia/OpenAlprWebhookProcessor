﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenAlprWebhookProcessor.Data;

namespace OpenAlprWebhookProcessor.Migrations
{
    [DbContext(typeof(ProcessorContext))]
    [Migration("20210617142110_vehicleDescription")]
    partial class vehicleDescription
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0-preview.4.21253.1");

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.Agent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("EndpointUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Hostname")
                        .HasColumnType("TEXT");

                    b.Property<long>("LastSuccessfulScrapeEpoch")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<double>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<string>("OpenAlprWebServerApiKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("OpenAlprWebServerUrl")
                        .HasColumnType("TEXT");

                    b.Property<int>("SunriseOffset")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SunsetOffset")
                        .HasColumnType("INTEGER");

                    b.Property<double>("TimeZoneOffset")
                        .HasColumnType("REAL");

                    b.Property<string>("Uid")
                        .HasColumnType("TEXT");

                    b.Property<string>("Version")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Agents");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.Alert", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsStrictMatch")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlateNumber")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Alerts");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.Camera", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CameraPassword")
                        .HasColumnType("TEXT");

                    b.Property<string>("CameraUsername")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("DayFocus")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("DayZoom")
                        .HasColumnType("TEXT");

                    b.Property<string>("IpAddress")
                        .HasColumnType("TEXT");

                    b.Property<string>("LatestProcessedPlateUuid")
                        .HasColumnType("TEXT");

                    b.Property<double?>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<double?>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<int>("Manufacturer")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModelNumber")
                        .HasColumnType("TEXT");

                    b.Property<string>("NextClearOverlayScheduleId")
                        .HasColumnType("TEXT");

                    b.Property<string>("NextDayNightScheduleId")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("NightFocus")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("NightZoom")
                        .HasColumnType("TEXT");

                    b.Property<long>("OpenAlprCameraId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("OpenAlprEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OpenAlprName")
                        .HasColumnType("TEXT");

                    b.Property<int>("PlatesSeen")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SunriseOffset")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SunsetOffset")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("TimezoneOffset")
                        .HasColumnType("REAL");

                    b.Property<bool>("UpdateDayNightModeEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdateDayNightModeUrl")
                        .HasColumnType("TEXT");

                    b.Property<bool>("UpdateOverlayEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdateOverlayTextUrl")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Cameras");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.Ignore", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsStrictMatch")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlateNumber")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Ignores");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.PlateGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AlertDescription")
                        .HasColumnType("TEXT");

                    b.Property<string>("BestNumber")
                        .HasColumnType("TEXT");

                    b.Property<double>("Confidence")
                        .HasColumnType("REAL");

                    b.Property<double>("Direction")
                        .HasColumnType("REAL");

                    b.Property<bool>("IsAlert")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Jpeg")
                        .HasColumnType("TEXT");

                    b.Property<int>("OpenAlprCameraId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("OpenAlprProcessingTimeMs")
                        .HasColumnType("REAL");

                    b.Property<string>("OpenAlprUuid")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlateCoordinates")
                        .HasColumnType("TEXT");

                    b.Property<string>("PossibleNumbers")
                        .HasColumnType("TEXT");

                    b.Property<long>("ReceivedOnEpoch")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Region")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleColor")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleMake")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleMakeModel")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleType")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleYear")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("PlateGroups");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.WebhookForward", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("ForwardGroupPreviews")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ForwardGroups")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ForwardSinglePlates")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FowardingDestination")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IgnoreSslErrors")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("WebhookForwards");
                });
#pragma warning restore 612, 618
        }
    }
}

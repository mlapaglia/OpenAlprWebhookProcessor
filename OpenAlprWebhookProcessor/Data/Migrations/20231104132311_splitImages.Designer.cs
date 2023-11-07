﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenAlprWebhookProcessor.Data;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    [DbContext(typeof(ProcessorContext))]
    [Migration("20231104132311_splitImages")]
    partial class splitImages
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.13");

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.Agent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("EndpointUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Hostname")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDebugEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsImageCompressionEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LastSuccessfulScrapeEpoch")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<double?>("Longitude")
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

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.Enricher", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ApiKey")
                        .HasColumnType("TEXT");

                    b.Property<int>("EnricherType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EnrichmentType")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Enrichers");
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

                    b.Property<double?>("AgentImageScrapeOccurredOn")
                        .HasColumnType("REAL");

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

                    b.Property<bool>("IsEnriched")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<int>("OpenAlprCameraId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("OpenAlprProcessingTimeMs")
                        .HasColumnType("REAL");

                    b.Property<string>("OpenAlprUuid")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlateCoordinates")
                        .HasColumnType("TEXT");

                    b.Property<long>("ReceivedOnEpoch")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VehicleColor")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleMake")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleMakeModel")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleRegion")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleType")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleYear")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("OpenAlprUuid");

                    b.HasIndex("ReceivedOnEpoch");

                    b.HasIndex("VehicleColor");

                    b.HasIndex("VehicleMake");

                    b.HasIndex("VehicleMakeModel");

                    b.HasIndex("VehicleRegion");

                    b.HasIndex("VehicleType");

                    b.HasIndex("VehicleYear");

                    b.HasIndex("BestNumber", "ReceivedOnEpoch");

                    b.ToTable("PlateGroups");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.PlateGroupPossibleNumbers", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Number")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PlateGroupId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Number");

                    b.HasIndex("PlateGroupId", "Number");

                    b.ToTable("PlateGroupPossibleNumbers");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.PlateGroupRaw", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("PlateGroupId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RawPlateGroup")
                        .HasColumnType("TEXT");

                    b.Property<long>("ReceivedOnEpoch")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("WasProcessedCorrectly")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("RawPlateGroups");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.PlateImage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsCompressed")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Jpeg")
                        .HasColumnType("BLOB");

                    b.Property<Guid>("PlateGroupId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("PlateGroupId")
                        .IsUnique();

                    b.ToTable("PlateImage");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.Pushover", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ApiToken")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SendPlatePreview")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserKey")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("PushoverAlertClients");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.VehicleImage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsCompressed")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Jpeg")
                        .HasColumnType("BLOB");

                    b.Property<Guid>("PlateGroupId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PlateGroupId")
                        .IsUnique();

                    b.ToTable("VehicleImage");
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

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.PlateGroupPossibleNumbers", b =>
                {
                    b.HasOne("OpenAlprWebhookProcessor.Data.PlateGroup", "PlateGroup")
                        .WithMany("PossibleNumbers")
                        .HasForeignKey("PlateGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlateGroup");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.PlateImage", b =>
                {
                    b.HasOne("OpenAlprWebhookProcessor.Data.PlateGroup", "PlateGroup")
                        .WithOne("PlateImage")
                        .HasForeignKey("OpenAlprWebhookProcessor.Data.PlateImage", "PlateGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlateGroup");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.VehicleImage", b =>
                {
                    b.HasOne("OpenAlprWebhookProcessor.Data.PlateGroup", "PlateGroup")
                        .WithOne("VehicleImage")
                        .HasForeignKey("OpenAlprWebhookProcessor.Data.VehicleImage", "PlateGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlateGroup");
                });

            modelBuilder.Entity("OpenAlprWebhookProcessor.Data.PlateGroup", b =>
                {
                    b.Navigation("PlateImage");

                    b.Navigation("PossibleNumbers");

                    b.Navigation("VehicleImage");
                });
#pragma warning restore 612, 618
        }
    }
}

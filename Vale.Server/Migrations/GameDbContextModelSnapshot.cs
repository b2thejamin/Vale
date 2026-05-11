using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vale.Server.Data;

#nullable disable

namespace Vale.Server.Migrations
{
    [DbContext(typeof(GameDbContext))]
    partial class GameDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.14");

            modelBuilder.Entity("Vale.Server.Data.Entities.StructureEntity", b =>
                {
                    b.Property<Guid>("StructureId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("ChunkX")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChunkY")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OwnerPlayerId")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("PlacedAtUtc")
                        .HasColumnType("TEXT");

                    b.Property<int>("StructureType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TileX")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TileY")
                        .HasColumnType("INTEGER");

                    b.HasKey("StructureId");

                    b.HasIndex("ChunkX", "ChunkY", "TileX", "TileY")
                        .IsUnique();

                    b.ToTable("Structures");
                });
#pragma warning restore 612, 618
        }
    }
}

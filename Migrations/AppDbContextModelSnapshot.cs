using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oracle.EntityFrameworkCore.Metadata;
using SpaceAgro.DotNetApi.Data;

#nullable disable

namespace SpaceAgro.DotNetApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            OracleModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SpaceAgro.DotNetApi.Models.LeituraSensor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasColumnName("ID_LEITURA");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DataHora")
                        .HasColumnType("TIMESTAMP(7)")
                        .HasColumnName("DATA_HORA");

                    b.Property<int>("IdDispositivo")
                        .HasColumnType("NUMBER(10)")
                        .HasColumnName("ID_DISPOSITIVO");

                    b.Property<double>("Temperatura")
                        .HasColumnType("BINARY_DOUBLE")
                        .HasColumnName("TEMPERATURA");

                    b.Property<double>("UmidadeAr")
                        .HasColumnType("BINARY_DOUBLE")
                        .HasColumnName("UMIDADE_AR");

                    b.Property<double>("UmidadeSolo")
                        .HasColumnType("BINARY_DOUBLE")
                        .HasColumnName("UMIDADE_SOLO");

                    b.HasKey("Id");

                    b.ToTable("TB_LEITURA_SENSOR");
                });

            modelBuilder.Entity("SpaceAgro.DotNetApi.Models.Talhao", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasColumnName("ID_TALHAO");

                    OraclePropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<double>("AreaHectares")
                        .HasColumnType("BINARY_DOUBLE")
                        .HasColumnName("AREA_HECTARES");

                    b.Property<string>("Cultura")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)")
                        .HasColumnName("CULTURA");

                    b.Property<int>("IdProdutor")
                        .HasColumnType("NUMBER(10)")
                        .HasColumnName("ID_PRODUTOR");

                    b.Property<double>("Latitude")
                        .HasColumnType("BINARY_DOUBLE")
                        .HasColumnName("LATITUDE");

                    b.Property<double>("Longitude")
                        .HasColumnType("BINARY_DOUBLE")
                        .HasColumnName("LONGITUDE");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)")
                        .HasColumnName("NOME_TALHAO");

                    b.HasKey("Id");

                    b.ToTable("TB_TALHAO");
                });
#pragma warning restore 612, 618
        }
    }
}

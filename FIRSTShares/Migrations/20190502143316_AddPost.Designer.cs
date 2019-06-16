﻿// <auto-generated />
using System;
using FIRSTShares.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FIRSTShares.Migrations
{
    [DbContext(typeof(LazyContext))]
    [Migration("20190502143316_AddPost")]
    partial class AddPost
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FIRSTShares.Entities.Cargo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Excluido");

                    b.Property<int>("Tipo");

                    b.HasKey("ID");

                    b.ToTable("Cargos");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Categoria", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Excluido");

                    b.Property<string>("Nome");

                    b.HasKey("ID");

                    b.ToTable("Categorias");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Curtida", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Curtiu");

                    b.Property<bool>("Excluido");

                    b.Property<int?>("PostagemID");

                    b.Property<int?>("PostagemID1");

                    b.Property<int?>("UsuarioID");

                    b.HasKey("ID");

                    b.HasIndex("PostagemID");

                    b.HasIndex("PostagemID1");

                    b.HasIndex("UsuarioID");

                    b.ToTable("Curtidas");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Discussao", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Assunto");

                    b.Property<DateTime>("DataCriacao");

                    b.Property<bool>("Excluido");

                    b.HasKey("ID");

                    b.ToTable("Discussoes");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Permissao", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("CargoID");

                    b.Property<bool>("Excluido");

                    b.Property<string>("Nome");

                    b.HasKey("ID");

                    b.HasIndex("CargoID");

                    b.ToTable("Permissoes");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Postagem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Conteudo");

                    b.Property<DateTime>("DataCriacao");

                    b.Property<int?>("DiscussaoID");

                    b.Property<bool>("Excluido");

                    b.Property<int?>("IDPostagemPaiID");

                    b.Property<bool>("PostagemOficial");

                    b.Property<int?>("UsuarioID");

                    b.HasKey("ID");

                    b.HasIndex("DiscussaoID");

                    b.HasIndex("IDPostagemPaiID");

                    b.HasIndex("UsuarioID");

                    b.ToTable("Postagens");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Time", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CodPais");

                    b.Property<bool>("Excluido");

                    b.Property<string>("Nome");

                    b.Property<string>("Numero");

                    b.Property<string>("Pais");

                    b.HasKey("ID");

                    b.ToTable("Times");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Usuario", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("CargoID");

                    b.Property<int>("CargoTime");

                    b.Property<DateTime>("DataCriacao");

                    b.Property<string>("Email");

                    b.Property<bool>("Excluido");

                    b.Property<string>("Nome");

                    b.Property<string>("Senha");

                    b.Property<int?>("TimeID");

                    b.HasKey("ID");

                    b.HasIndex("CargoID");

                    b.HasIndex("TimeID");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Curtida", b =>
                {
                    b.HasOne("FIRSTShares.Entities.Postagem", "Postagem")
                        .WithMany()
                        .HasForeignKey("PostagemID");

                    b.HasOne("FIRSTShares.Entities.Postagem")
                        .WithMany("Curtidas")
                        .HasForeignKey("PostagemID1");

                    b.HasOne("FIRSTShares.Entities.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioID");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Permissao", b =>
                {
                    b.HasOne("FIRSTShares.Entities.Cargo")
                        .WithMany("Permissoes")
                        .HasForeignKey("CargoID");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Postagem", b =>
                {
                    b.HasOne("FIRSTShares.Entities.Discussao", "Discussao")
                        .WithMany()
                        .HasForeignKey("DiscussaoID");

                    b.HasOne("FIRSTShares.Entities.Postagem", "IDPostagemPai")
                        .WithMany("Postagens")
                        .HasForeignKey("IDPostagemPaiID");

                    b.HasOne("FIRSTShares.Entities.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioID");
                });

            modelBuilder.Entity("FIRSTShares.Entities.Usuario", b =>
                {
                    b.HasOne("FIRSTShares.Entities.Cargo", "Cargo")
                        .WithMany()
                        .HasForeignKey("CargoID");

                    b.HasOne("FIRSTShares.Entities.Time", "Time")
                        .WithMany()
                        .HasForeignKey("TimeID");
                });
#pragma warning restore 612, 618
        }
    }
}
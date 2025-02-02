﻿// <auto-generated />
using System;
using DormBuddy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DormBuddy.Migrations
{
    [DbContext(typeof(DBContext))]
    [Migration("20241203153117_AddIsAdminToApplicationUser")]
    partial class AddIsAdminToApplicationUser
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("DormBuddy.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<int>("Credits")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("FirstName")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("LastLoginDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("LastName")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("PasswordHash")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("RememberMe")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<int>("TotalLogins")
                        .HasColumnType("int");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("DormBuddy.Models.DashboardChatModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("UserId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("message")
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("sent_at")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("DashboardChatModel");
                });

            modelBuilder.Entity("DormBuddy.Models.ExpenseModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65,30)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ExpenseName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("SharedWith")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<bool>("isSplit")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.ToTable("Expenses");
                });

            modelBuilder.Entity("DormBuddy.Models.FriendsModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("UserId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<bool>("blocked")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("pending")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.ToTable("FriendsModel");
                });

            modelBuilder.Entity("DormBuddy.Models.GroupMemberModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupMembers", (string)null);
                });

            modelBuilder.Entity("DormBuddy.Models.GroupModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CreatedByUserId")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("InvitationCode")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("varchar(8)");

                    b.Property<int>("MaxMembers")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("TotalMembers")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Groups", (string)null);
                });

            modelBuilder.Entity("DormBuddy.Models.LogModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Description")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("Details")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<string>("LogType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Logs", (string)null);
                });

            modelBuilder.Entity("DormBuddy.Models.Notifications", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Message")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<int>("MessageType")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("DormBuddy.Models.PeerLendingModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65,30)");

                    b.Property<string>("BorrowerId")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsRepaid")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.ToTable("PeerLendings");
                });

            modelBuilder.Entity("DormBuddy.Models.Profile_PostsModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Message")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<int>("Reply_Id")
                        .HasColumnType("int");

                    b.Property<string>("TargetId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("UserId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.HasIndex("TargetId");

                    b.HasIndex("UserId");

                    b.ToTable("Profile_Posts");
                });

            modelBuilder.Entity("DormBuddy.Models.TaskModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AssignedTo")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("TaskName")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("DormBuddy.Models.UserLastUpdate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UserId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserLastUpdate");
                });

            modelBuilder.Entity("DormBuddy.Models.UserProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BannerImageUrl")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("Bio")
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<string>("CompanyName")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("FacebookUrl")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("InstagramUrl")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("JobTitle")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<DateTime>("LastLogin")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("LinkedInUrl")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("Preferred_Language")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("ProfileImageUrl")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<bool>("ProfileVisibleToPublic")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("ReceiveEmailNotifications")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SchoolName")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("TwitterUrl")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("UserId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<bool>("Verified")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserProfiles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("Name")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("ClaimValue")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("ClaimValue")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("ProviderDisplayName")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("RoleId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("Name")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("Value")
                        .HasMaxLength(160)
                        .HasColumnType("varchar(160)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("DormBuddy.Models.DashboardChatModel", b =>
                {
                    b.HasOne("DormBuddy.Models.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DormBuddy.Models.GroupMemberModel", b =>
                {
                    b.HasOne("DormBuddy.Models.GroupModel", "Group")
                        .WithMany("Members")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("DormBuddy.Models.Notifications", b =>
                {
                    b.HasOne("DormBuddy.Models.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DormBuddy.Models.Profile_PostsModel", b =>
                {
                    b.HasOne("DormBuddy.Models.ApplicationUser", "TargetUser")
                        .WithMany()
                        .HasForeignKey("TargetId");

                    b.HasOne("DormBuddy.Models.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("TargetUser");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DormBuddy.Models.UserLastUpdate", b =>
                {
                    b.HasOne("DormBuddy.Models.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DormBuddy.Models.UserProfile", b =>
                {
                    b.HasOne("DormBuddy.Models.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("DormBuddy.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("DormBuddy.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DormBuddy.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("DormBuddy.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DormBuddy.Models.GroupModel", b =>
                {
                    b.Navigation("Members");
                });
#pragma warning restore 612, 618
        }
    }
}

        using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NewDotnet.Models;

namespace NewDotnet.Context;

public partial class OODBModelContext : DbContext
{
 

    public OODBModelContext(DbContextOptions<OODBModelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountQuiz> AccountQuizzes { get; set; }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<AuthGroup> AuthGroups { get; set; }

    public virtual DbSet<AuthGroupPermission> AuthGroupPermissions { get; set; }

    public virtual DbSet<AuthPermission> AuthPermissions { get; set; }

    public virtual DbSet<AuthUser> AuthUsers { get; set; }

    public virtual DbSet<AuthUserGroup> AuthUserGroups { get; set; }

    public virtual DbSet<AuthUserUserPermission> AuthUserUserPermissions { get; set; }

    public virtual DbSet<CohortPrompt> CohortPrompts { get; set; }

    public virtual DbSet<Configuration> Configurations { get; set; }

    public virtual DbSet<Content> Contents { get; set; }

    public virtual DbSet<DjangoAdminLog> DjangoAdminLogs { get; set; }

    public virtual DbSet<DjangoContentType> DjangoContentTypes { get; set; }

    public virtual DbSet<DjangoMigration> DjangoMigrations { get; set; }

    public virtual DbSet<DjangoSession> DjangoSessions { get; set; }

    public virtual DbSet<Flag> Flags { get; set; }

    public virtual DbSet<Guest> Guests { get; set; }

    public virtual DbSet<Playlist> Playlists { get; set; }

    public virtual DbSet<Playlist1> Playlists1 { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<SchemaUpdate> SchemaUpdates { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=campus-quest.com,20011;Database=Orientation;User Id=sa;Password=academic2024CS680!;multipleactiveresultsets=False;App=OnlineOrientationAPI;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.StarId).HasName("PK__account__A8C63019B694D2A2");

            entity.ToTable("account", "orientation");

            entity.Property(e => e.StarId)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("starId");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("lastName");

            entity.HasMany(d => d.Flags).WithMany(p => p.Accounts)
                .UsingEntity<Dictionary<string, object>>(
                    "AccountFlag",
                    r => r.HasOne<Flag>().WithMany()
                        .HasForeignKey("FlagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_accountFlag_flag"),
                    l => l.HasOne<Account>().WithMany()
                        .HasForeignKey("StarId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_accountFlag_account"),
                    j =>
                    {
                        j.HasKey("StarId", "FlagId");
                        j.ToTable("accountFlag", "orientation");
                        j.IndexerProperty<string>("StarId")
                            .HasMaxLength(32)
                            .IsUnicode(false)
                            .HasColumnName("starId");
                        j.IndexerProperty<int>("FlagId").HasColumnName("flagId");
                    });
        });

        modelBuilder.Entity<AccountQuiz>(entity =>
        {
            entity.HasKey(e => new { e.StarId, e.SectionId, e.TimeCompleted }).HasName("PK__accountQ__5AA9E57EA872FB50");

            entity.ToTable("accountQuiz", "orientation");

            entity.Property(e => e.StarId)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("starId");
            entity.Property(e => e.SectionId).HasColumnName("sectionId");
            entity.Property(e => e.TimeCompleted)
                .HasColumnType("datetime")
                .HasColumnName("timeCompleted");
            entity.Property(e => e.AccountPassed).HasColumnName("accountPassed");

            entity.HasOne(d => d.Section).WithMany(p => p.AccountQuizzes)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accountQuiz_sectionId");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountQuizzes)
                .HasForeignKey(d => d.StarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accountQuiz_starId");
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.AssetId).HasName("PK_assets");

            entity.ToTable("asset", "orientation");

            entity.Property(e => e.AssetId).HasColumnName("assetId");
            entity.Property(e => e.AssetData).HasColumnName("assetData");
            entity.Property(e => e.AssetName)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("assetName");
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => new { e.StarId, e.AssignedPlaylist }).HasName("PK_orientation.assignment");

            entity.ToTable("assignment", "orientation");

            entity.Property(e => e.StarId)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("starId");
            entity.Property(e => e.AssignedPlaylist).HasColumnName("assignedPlaylist");
            entity.Property(e => e.CurrentPosition).HasColumnName("currentPosition");
            entity.Property(e => e.CurrentProgress).HasColumnName("currentProgress");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("endTime");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("startTime");

            entity.HasOne(d => d.Account).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.StarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_assignment_account");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.EntryId);

            entity.ToTable("auditLog", "orientation");

            entity.Property(e => e.EntryId).HasColumnName("entry_id");
            entity.Property(e => e.ContentAfter)
                .IsUnicode(false)
                .HasColumnName("content_after");
            entity.Property(e => e.ContentBefore)
                .IsUnicode(false)
                .HasColumnName("content_before");
            entity.Property(e => e.EntrySource)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasColumnName("entry_source");
            entity.Property(e => e.EntryStarid)
                .HasMaxLength(70)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("entry_starid");
            entity.Property(e => e.EntryText)
                .IsUnicode(false)
                .HasColumnName("entry_text");
            entity.Property(e => e.EntryTime)
                .HasColumnType("datetime")
                .HasColumnName("entry_time");
        });

        modelBuilder.Entity<AuthGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__auth_gro__3213E83FDB77B094");

            entity.ToTable("auth_group");

            entity.HasIndex(e => e.Name, "auth_group_name_a6ea08ec_uniq").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
        });

        modelBuilder.Entity<AuthGroupPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__auth_gro__3213E83FB2A49E16");

            entity.ToTable("auth_group_permissions");

            entity.HasIndex(e => e.GroupId, "auth_group_permissions_group_id_b120cbf9");

            entity.HasIndex(e => new { e.GroupId, e.PermissionId }, "auth_group_permissions_group_id_permission_id_0cd325b0_uniq")
                .IsUnique()
                .HasFilter("([group_id] IS NOT NULL AND [permission_id] IS NOT NULL)");

            entity.HasIndex(e => e.PermissionId, "auth_group_permissions_permission_id_84c5c92e");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");

            entity.HasOne(d => d.Group).WithMany(p => p.AuthGroupPermissions)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auth_group_permissions_group_id_b120cbf9_fk_auth_group_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.AuthGroupPermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auth_group_permissions_permission_id_84c5c92e_fk_auth_permission_id");
        });

        modelBuilder.Entity<AuthPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__auth_per__3213E83F0C60AFA8");

            entity.ToTable("auth_permission");

            entity.HasIndex(e => e.ContentTypeId, "auth_permission_content_type_id_2f476e4b");

            entity.HasIndex(e => new { e.ContentTypeId, e.Codename }, "auth_permission_content_type_id_codename_01ab375a_uniq")
                .IsUnique()
                .HasFilter("([content_type_id] IS NOT NULL AND [codename] IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codename)
                .HasMaxLength(100)
                .HasColumnName("codename");
            entity.Property(e => e.ContentTypeId).HasColumnName("content_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.ContentType).WithMany(p => p.AuthPermissions)
                .HasForeignKey(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auth_permission_content_type_id_2f476e4b_fk_django_content_type_id");
        });

        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__auth_use__3213E83F6B10F136");

            entity.ToTable("auth_user");

            entity.HasIndex(e => e.Username, "auth_user_username_6821ab7c_uniq").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateJoined).HasColumnName("date_joined");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(150)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsStaff).HasColumnName("is_staff");
            entity.Property(e => e.IsSuperuser).HasColumnName("is_superuser");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.Property(e => e.LastName)
                .HasMaxLength(150)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(128)
                .HasColumnName("password");
            entity.Property(e => e.Username)
                .HasMaxLength(150)
                .HasColumnName("username");
        });

        modelBuilder.Entity<AuthUserGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__auth_use__3213E83FF8069FC7");

            entity.ToTable("auth_user_groups");

            entity.HasIndex(e => e.GroupId, "auth_user_groups_group_id_97559544");

            entity.HasIndex(e => e.UserId, "auth_user_groups_user_id_6a12ed8b");

            entity.HasIndex(e => new { e.UserId, e.GroupId }, "auth_user_groups_user_id_group_id_94350c0c_uniq")
                .IsUnique()
                .HasFilter("([user_id] IS NOT NULL AND [group_id] IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Group).WithMany(p => p.AuthUserGroups)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auth_user_groups_group_id_97559544_fk_auth_group_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuthUserGroups)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auth_user_groups_user_id_6a12ed8b_fk_auth_user_id");
        });

        modelBuilder.Entity<AuthUserUserPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__auth_use__3213E83F7C278564");

            entity.ToTable("auth_user_user_permissions");

            entity.HasIndex(e => e.PermissionId, "auth_user_user_permissions_permission_id_1fbb5f2c");

            entity.HasIndex(e => e.UserId, "auth_user_user_permissions_user_id_a95ead1b");

            entity.HasIndex(e => new { e.UserId, e.PermissionId }, "auth_user_user_permissions_user_id_permission_id_14a6b632_uniq")
                .IsUnique()
                .HasFilter("([user_id] IS NOT NULL AND [permission_id] IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.AuthUserUserPermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auth_user_user_permissions_permission_id_1fbb5f2c_fk_auth_permission_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuthUserUserPermissions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auth_user_user_permissions_user_id_a95ead1b_fk_auth_user_id");
        });

        modelBuilder.Entity<CohortPrompt>(entity =>
        {
            entity.HasKey(e => new { e.PromptOrder, e.PlaylistId }).HasName("PK_orientation.cohortPrompt");

            entity.ToTable("cohortPrompt", "orientation");

            entity.HasIndex(e => new { e.PlaylistId, e.PromptOrder }, "IX_playlistId_promptOrder");

            entity.Property(e => e.PromptOrder).HasColumnName("promptOrder");
            entity.Property(e => e.PlaylistId).HasColumnName("playlistId");
            entity.Property(e => e.CohortMap)
                .HasDefaultValue("")
                .HasColumnName("cohortMap");
            entity.Property(e => e.PromptOptions).HasColumnName("promptOptions");
            entity.Property(e => e.PromptText).HasColumnName("promptText");

            entity.HasOne(d => d.Playlist).WithMany(p => p.CohortPrompts)
                .HasForeignKey(d => d.PlaylistId)
                .HasConstraintName("FK_orientation.cohortPrompt_orientation.playlists_playlistId");
        });

        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.OptionId);

            entity.ToTable("configuration", "orientation");

            entity.Property(e => e.OptionId).HasColumnName("optionId");
            entity.Property(e => e.OptionAcceptedValues)
                .IsUnicode(false)
                .HasColumnName("optionAcceptedValues");
            entity.Property(e => e.OptionAppliesTo).HasColumnName("optionAppliesTo");
            entity.Property(e => e.OptionDescription)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("optionDescription");
            entity.Property(e => e.OptionKind)
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasDefaultValue("text")
                .HasColumnName("optionKind");
            entity.Property(e => e.OptionShortName)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasColumnName("optionShortName");
            entity.Property(e => e.OptionValue)
                .IsUnicode(false)
                .HasColumnName("optionValue");

            entity.HasOne(d => d.OptionAppliesToNavigation).WithMany(p => p.Configurations)
                .HasForeignKey(d => d.OptionAppliesTo)
                .HasConstraintName("FK_configuration_playlists");
        });

        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.ContentId).HasName("PK__oo_conte__0BDC871975A40182");

            entity.ToTable("content", "orientation");

            entity.Property(e => e.ContentId).HasColumnName("contentId");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("comment");
            entity.Property(e => e.ContentData)
                .IsUnicode(false)
                .HasColumnName("contentData");
            entity.Property(e => e.ContentTitle)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contentTitle");
            entity.Property(e => e.HeaderImage)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValue("https://www.campus-quest.com/assets/msu-flame.jpg")
                .HasColumnName("headerImage");
            entity.Property(e => e.SectionId).HasColumnName("sectionId");

            entity.HasOne(d => d.Section).WithMany(p => p.Contents)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__oo_conten__secti__37A5467C");

            entity.HasMany(d => d.Flags).WithMany(p => p.Contents)
                .UsingEntity<Dictionary<string, object>>(
                    "ContentFlag",
                    r => r.HasOne<Flag>().WithMany()
                        .HasForeignKey("FlagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_contentFlag_flag"),
                    l => l.HasOne<Content>().WithMany()
                        .HasForeignKey("ContentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_contentFlag_content"),
                    j =>
                    {
                        j.HasKey("ContentId", "FlagId");
                        j.ToTable("contentFlag", "orientation");
                        j.IndexerProperty<int>("ContentId").HasColumnName("contentId");
                        j.IndexerProperty<int>("FlagId").HasColumnName("flagId");
                    });
        });

        modelBuilder.Entity<DjangoAdminLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__django_a__3213E83FDF00C745");

            entity.ToTable("django_admin_log");

            entity.HasIndex(e => e.ContentTypeId, "django_admin_log_content_type_id_c4bce8eb");

            entity.HasIndex(e => e.UserId, "django_admin_log_user_id_c564eba6");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionFlag).HasColumnName("action_flag");
            entity.Property(e => e.ActionTime).HasColumnName("action_time");
            entity.Property(e => e.ChangeMessage).HasColumnName("change_message");
            entity.Property(e => e.ContentTypeId).HasColumnName("content_type_id");
            entity.Property(e => e.ObjectId).HasColumnName("object_id");
            entity.Property(e => e.ObjectRepr)
                .HasMaxLength(200)
                .HasColumnName("object_repr");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ContentType).WithMany(p => p.DjangoAdminLogs)
                .HasForeignKey(d => d.ContentTypeId)
                .HasConstraintName("django_admin_log_content_type_id_c4bce8eb_fk_django_content_type_id");

            entity.HasOne(d => d.User).WithMany(p => p.DjangoAdminLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("django_admin_log_user_id_c564eba6_fk_auth_user_id");
        });

        modelBuilder.Entity<DjangoContentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__django_c__3213E83F184BF113");

            entity.ToTable("django_content_type");

            entity.HasIndex(e => new { e.AppLabel, e.Model }, "django_content_type_app_label_model_76bd3d3b_uniq")
                .IsUnique()
                .HasFilter("([app_label] IS NOT NULL AND [model] IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppLabel)
                .HasMaxLength(100)
                .HasColumnName("app_label");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .HasColumnName("model");
        });

        modelBuilder.Entity<DjangoMigration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__django_m__3213E83F0E54CB8D");

            entity.ToTable("django_migrations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.App)
                .HasMaxLength(255)
                .HasColumnName("app");
            entity.Property(e => e.Applied).HasColumnName("applied");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<DjangoSession>(entity =>
        {
            entity.HasKey(e => e.SessionKey).HasName("PK__django_s__B3BA0F1F6B1CB077");

            entity.ToTable("django_session");

            entity.HasIndex(e => e.ExpireDate, "django_session_expire_date_a5c62663");

            entity.Property(e => e.SessionKey)
                .HasMaxLength(40)
                .HasColumnName("session_key");
            entity.Property(e => e.ExpireDate).HasColumnName("expire_date");
            entity.Property(e => e.SessionData).HasColumnName("session_data");
        });

        modelBuilder.Entity<Flag>(entity =>
        {
            entity.ToTable("flag", "orientation");

            entity.Property(e => e.FlagId).HasColumnName("flagId");
            entity.Property(e => e.FlagAppliesTo).HasColumnName("flagAppliesTo");
            entity.Property(e => e.FlagDescription)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("flagDescription");
            entity.Property(e => e.FlagHelpText)
                .IsUnicode(false)
                .HasColumnName("flagHelpText");
            entity.Property(e => e.FlagName)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("flagName");
            entity.Property(e => e.FlagType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("flagType");
        });

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.ToTable("guest", "orientation");

            entity.Property(e => e.GuestId)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasColumnName("guestId");
            entity.Property(e => e.AssignedPlaylist)
                .HasDefaultValue(0)
                .HasColumnName("assignedPlaylist");
            entity.Property(e => e.CurrentPosition).HasColumnName("currentPosition");
            entity.Property(e => e.LastAction)
                .HasColumnType("datetime")
                .HasColumnName("lastAction");

            entity.HasOne(d => d.AssignedPlaylistNavigation).WithMany(p => p.Guests)
                .HasForeignKey(d => d.AssignedPlaylist)
                .HasConstraintName("FK_guest_playlists");
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => new { e.PlaylistOrder, e.PlaylistId }).HasName("PK__playlist__71F9B03086E0A4B5");

            entity.ToTable("playlist", "orientation");

            entity.Property(e => e.PlaylistOrder).HasColumnName("playlistOrder");
            entity.Property(e => e.PlaylistId).HasColumnName("playlistId");
            entity.Property(e => e.ItemId).HasColumnName("itemId");
            entity.Property(e => e.ItemType)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("itemType");

            entity.HasOne(d => d.PlaylistNavigation).WithMany(p => p.Playlists)
                .HasForeignKey(d => d.PlaylistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_playlist_playlists");
        });

        modelBuilder.Entity<Playlist1>(entity =>
        {
            entity.HasKey(e => e.PlaylistId);

            entity.ToTable("playlists", "orientation");

            entity.Property(e => e.PlaylistId)
                .ValueGeneratedNever()
                .HasColumnName("playlistId");
            entity.Property(e => e.PlaylistTitle)
                .HasMaxLength(64)
                .HasColumnName("playlistTitle");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__oo_quest__6238D4B26729B558");

            entity.ToTable("question", "orientation");

            entity.Property(e => e.QuestionId)
                .ValueGeneratedNever()
                .HasColumnName("questionId");
            entity.Property(e => e.QuestionAnswers)
                .IsUnicode(false)
                .HasColumnName("questionAnswers");
            entity.Property(e => e.QuestionText)
                .HasMaxLength(512)
                .IsUnicode(false)
                .HasColumnName("questionText");
            entity.Property(e => e.SectionId).HasColumnName("sectionId");

            entity.HasOne(d => d.Section).WithMany(p => p.Questions)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_question_section");
        });

        modelBuilder.Entity<SchemaUpdate>(entity =>
        {
            entity.HasKey(e => e.SchemaId);

            entity.ToTable("_schema_updates", "orientation");

            entity.Property(e => e.SchemaId)
                .ValueGeneratedNever()
                .HasColumnName("schema_id");
            entity.Property(e => e.AppledAt)
                .HasColumnType("datetime")
                .HasColumnName("appled_at");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("PK__oo_secti__3F58FD52CB3CDFAB");

            entity.ToTable("section", "orientation");

            entity.Property(e => e.SectionId).HasColumnName("sectionId");
            entity.Property(e => e.Comment)
                .IsUnicode(false)
                .HasColumnName("comment");
            entity.Property(e => e.SectionTitle)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("sectionTitle");

            entity.HasMany(d => d.Flags).WithMany(p => p.Sections)
                .UsingEntity<Dictionary<string, object>>(
                    "SectionFlag",
                    r => r.HasOne<Flag>().WithMany()
                        .HasForeignKey("FlagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_sectionFlag_flag"),
                    l => l.HasOne<Section>().WithMany()
                        .HasForeignKey("SectionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_sectionFlag_section"),
                    j =>
                    {
                        j.HasKey("SectionId", "FlagId");
                        j.ToTable("sectionFlag", "orientation");
                        j.IndexerProperty<int>("SectionId").HasColumnName("sectionId");
                        j.IndexerProperty<int>("FlagId").HasColumnName("flagId");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user", "orientation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("firstName");
            entity.Property(e => e.IsAdmin).HasColumnName("isAdmin");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("lastName");
            entity.Property(e => e.Password)
                .HasMaxLength(60)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("password");
            entity.Property(e => e.UserId)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("userId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

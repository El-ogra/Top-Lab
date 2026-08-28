using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BaselineDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Antibiotics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsPregnancyFlagged = table.Column<bool>(type: "bit", nullable: false),
                    IsChildrenFlagged = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Antibiotics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CultureAntibioticAttachments",
                columns: table => new
                {
                    TestId = table.Column<int>(type: "int", nullable: false),
                    AntibioticId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultureAntibioticAttachments", x => new { x.TestId, x.AntibioticId });
                });

            migrationBuilder.CreateTable(
                name: "CustomGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnvelopePrintItemPositions",
                columns: table => new
                {
                    ItemName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LeftOffsetCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TopOffsetCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvelopePrintItemPositions", x => x.ItemName);
                });

            migrationBuilder.CreateTable(
                name: "EnvelopeSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TopMarginCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    HeaderFooterMode = table.Column<byte>(type: "tinyint", nullable: false),
                    SuppressCaptions = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvelopeSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalConditionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalConditionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sex = table.Column<byte>(type: "tinyint", nullable: false),
                    AgeValue = table.Column<int>(type: "int", nullable: false),
                    AgeUnit = table.Column<byte>(type: "tinyint", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TreatingDoctorId = table.Column<int>(type: "int", nullable: true),
                    ReferralEntityId = table.Column<int>(type: "int", nullable: true),
                    AccountType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsVip = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PickupDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFastingIndicated = table.Column<bool>(type: "bit", nullable: false),
                    FastingHours = table.Column<int>(type: "int", nullable: true),
                    RecentContrastImaging = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientTitles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientTitles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrinterAssignments",
                columns: table => new
                {
                    OutputType = table.Column<byte>(type: "tinyint", nullable: false),
                    PrinterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterAssignments", x => x.OutputType);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TopMarginCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PickupTimeDefault = table.Column<TimeOnly>(type: "time", nullable: true),
                    PrintOnce = table.Column<bool>(type: "bit", nullable: false),
                    TestDetailDisplayMode = table.Column<byte>(type: "tinyint", nullable: false),
                    CashierPrinterEnabled = table.Column<bool>(type: "bit", nullable: false),
                    HeaderFooterMode = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PageMarginLeftCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PageMarginBottomCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ReportTopSpaceCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PaperSize = table.Column<byte>(type: "tinyint", nullable: false),
                    HeaderFooterMode = table.Column<byte>(type: "tinyint", nullable: false),
                    DoctorSignatureEnabled = table.Column<bool>(type: "bit", nullable: false),
                    HeaderColor = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    FooterColor = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    HistorySortMode = table.Column<byte>(type: "tinyint", nullable: false),
                    HistoryAutoDisplayEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSettings", x => x.Id);
                    table.CheckConstraint("CK_ReportSettings_TopSpace", "[ReportTopSpaceCm] <= 8");
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DefaultAccountType = table.Column<byte>(type: "tinyint", nullable: false),
                    PrintLabIdInsteadOfPatientId = table.Column<bool>(type: "bit", nullable: false),
                    AutoReviewAndComplete = table.Column<bool>(type: "bit", nullable: false),
                    ResultScreenAccountDisplayMode = table.Column<byte>(type: "tinyint", nullable: false),
                    SaveTreatingDoctorOnlyFromEntityWindow = table.Column<bool>(type: "bit", nullable: false),
                    EnablePatientNameSearchAssist = table.Column<bool>(type: "bit", nullable: false),
                    DisableAutoTitleInsertion = table.Column<bool>(type: "bit", nullable: false),
                    PrintFileExternalBarcode = table.Column<bool>(type: "bit", nullable: false),
                    PrintDateTimeOnTubeBarcode = table.Column<bool>(type: "bit", nullable: false),
                    PrintAccountInsteadOfDateOnReport = table.Column<bool>(type: "bit", nullable: false),
                    DailyBackupEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DailyBackupPath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    InternalWindowsPasswordHash = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsAbsolutePermission = table.Column<bool>(type: "bit", nullable: false),
                    DiscountLimitPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BlockPrintOnRemainingBalance = table.Column<bool>(type: "bit", nullable: false),
                    WorkStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    WorkEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    HasBreakPeriod = table.Column<bool>(type: "bit", nullable: false),
                    BreakDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkGroupLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkGroupLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomGroupItems",
                columns: table => new
                {
                    CustomGroupId = table.Column<int>(type: "int", nullable: false),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomGroupItems", x => new { x.CustomGroupId, x.TestId });
                    table.ForeignKey(
                        name: "FK_CustomGroupItems_CustomGroups_CustomGroupId",
                        column: x => x.CustomGroupId,
                        principalTable: "CustomGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientMedicalConditions",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MedicalConditionTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedicalConditions", x => new { x.PatientId, x.MedicalConditionTypeId });
                    table.ForeignKey(
                        name: "FK_PatientMedicalConditions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientPhoneNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SortOrder = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPhoneNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientPhoneNumbers_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsExtraCharge = table.Column<bool>(type: "bit", nullable: false),
                    OperationType = table.Column<byte>(type: "tinyint", nullable: false),
                    ReceivedByUserId = table.Column<int>(type: "int", nullable: false),
                    OperationAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVoided = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentOperations_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ResponsiblePersonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ResponsiblePersonPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PriceListId = table.Column<int>(type: "int", nullable: true),
                    DiscountOrCommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    GeneratedIdCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalEntities_PriceLists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "PriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PriceListItems",
                columns: table => new
                {
                    PriceListId = table.Column<int>(type: "int", nullable: false),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceListItems", x => new { x.PriceListId, x.TestId });
                    table.ForeignKey(
                        name: "FK_PriceListItems_PriceLists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "PriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReceiptName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TestGroupId = table.Column<int>(type: "int", nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompletionDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsSentOut = table.Column<bool>(type: "bit", nullable: false),
                    SentOutCostPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PatientPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LabToLabPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ResultKind = table.Column<byte>(type: "tinyint", nullable: false),
                    IsCultureType = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tests_TestGroups_TestGroupId",
                        column: x => x.TestGroupId,
                        principalTable: "TestGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CheckInAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BreakStartAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BreakEndAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OvertimeMinutes = table.Column<int>(type: "int", nullable: true),
                    LatenessMinutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissionGrants",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissionGrants", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserPermissionGrants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkGroupLogItems",
                columns: table => new
                {
                    WorkGroupLogId = table.Column<int>(type: "int", nullable: false),
                    TestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkGroupLogItems", x => new { x.WorkGroupLogId, x.TestId });
                    table.ForeignKey(
                        name: "FK_WorkGroupLogItems_WorkGroupLogs_WorkGroupLogId",
                        column: x => x.WorkGroupLogId,
                        principalTable: "WorkGroupLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementType = table.Column<byte>(type: "tinyint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RelatedExternalEntityId = table.Column<int>(type: "int", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashMovements_ExternalEntities_RelatedExternalEntityId",
                        column: x => x.RelatedExternalEntityId,
                        principalTable: "ExternalEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PatientTests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    PriceAtOrderTime = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsUrine = table.Column<bool>(type: "bit", nullable: false),
                    IsStool = table.Column<bool>(type: "bit", nullable: false),
                    IsBlood = table.Column<bool>(type: "bit", nullable: false),
                    IsSemen = table.Column<bool>(type: "bit", nullable: false),
                    IsCsf = table.Column<bool>(type: "bit", nullable: false),
                    IsTakenOutsideLab = table.Column<bool>(type: "bit", nullable: false),
                    IsSampleDrawn = table.Column<bool>(type: "bit", nullable: false),
                    SampleDrawnAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResultFlag = table.Column<byte>(type: "tinyint", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: true),
                    EnteredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPrinted = table.Column<bool>(type: "bit", nullable: false),
                    PrintCount = table.Column<int>(type: "int", nullable: false),
                    LastPrintedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastPrintedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false),
                    DeliveredByUserId = table.Column<int>(type: "int", nullable: true),
                    DeliveredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsExported = table.Column<bool>(type: "bit", nullable: false),
                    ExportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientTests_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientTests_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceRanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    Sex = table.Column<byte>(type: "tinyint", nullable: true),
                    AgeUnit = table.Column<byte>(type: "tinyint", nullable: false),
                    AgeMin = table.Column<int>(type: "int", nullable: false),
                    AgeMax = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LowComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HighComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferenceRanges_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestComments_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CultureResults",
                columns: table => new
                {
                    PatientTestId = table.Column<int>(type: "int", nullable: false),
                    Sample = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrganismA = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    OrganismB = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    OrganismC = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CultureCondition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ColonyCount = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultureResults", x => x.PatientTestId);
                    table.ForeignKey(
                        name: "FK_CultureResults_PatientTests_PatientTestId",
                        column: x => x.PatientTestId,
                        principalTable: "PatientTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileResultItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientTestId = table.Column<int>(type: "int", nullable: false),
                    AnalyteName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ResultValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Flag = table.Column<byte>(type: "tinyint", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsPrinted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileResultItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileResultItems_PatientTests_PatientTestId",
                        column: x => x.PatientTestId,
                        principalTable: "PatientTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SentOutSamples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientTestId = table.Column<int>(type: "int", nullable: false),
                    ExternalLabEntityId = table.Column<int>(type: "int", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PatientPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentOutSamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentOutSamples_ExternalEntities_ExternalLabEntityId",
                        column: x => x.ExternalLabEntityId,
                        principalTable: "ExternalEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SentOutSamples_PatientTests_PatientTestId",
                        column: x => x.PatientTestId,
                        principalTable: "PatientTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CultureAntibioticResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientTestId = table.Column<int>(type: "int", nullable: false),
                    AntibioticId = table.Column<int>(type: "int", nullable: false),
                    SensitivityCategory = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultureAntibioticResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CultureAntibioticResults_Antibiotics_AntibioticId",
                        column: x => x.AntibioticId,
                        principalTable: "Antibiotics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CultureAntibioticResults_CultureResults_PatientTestId",
                        column: x => x.PatientTestId,
                        principalTable: "CultureResults",
                        principalColumn: "PatientTestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SentOutSamplePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SentOutSampleId = table.Column<int>(type: "int", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentOutSamplePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentOutSamplePayments_SentOutSamples_SentOutSampleId",
                        column: x => x.SentOutSampleId,
                        principalTable: "SentOutSamples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EnvelopePrintItemPositions",
                columns: new[] { "ItemName", "IsEnabled", "LeftOffsetCm", "TopOffsetCm" },
                values: new object[,]
                {
                    { "Code", true, 1.0m, 2.0m },
                    { "Date", true, 1.0m, 4.0m },
                    { "Name", true, 1.0m, 1.0m },
                    { "ReferralEntity", true, 1.0m, 3.0m }
                });

            migrationBuilder.InsertData(
                table: "EnvelopeSettings",
                columns: new[] { "Id", "HeaderFooterMode", "SuppressCaptions", "TopMarginCm" },
                values: new object[] { 1, (byte)0, false, 3.0m });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description" },
                values: new object[,]
                {
                    { 1, "ADD_EDIT_PATIENT", "Add and edit patient data" },
                    { 2, "EDIT_RESULTS", "Enter and edit patient test results" },
                    { 3, "REVIEW_RESULTS", "Review and edit patient test results" },
                    { 4, "PRINT_RESULTS", "Print patient results" },
                    { 5, "BLOCK_PRINT_ON_BALANCE", "Block printing when balance remains" },
                    { 6, "DELIVER_RESULTS", "Deliver results" },
                    { 7, "DISCOUNT_LIMIT", "Discount limit" },
                    { 8, "PRINT_WORKSHEET", "Print worksheet and Log" },
                    { 9, "DELETE_PATIENT", "Delete patients" },
                    { 10, "EDIT_SYSTEM_SETTINGS", "Edit system and test settings" },
                    { 11, "CASH_DISBURSE_DEPOSIT", "Cash disbursement and deposit" },
                    { 12, "STATISTICS", "Statistics" },
                    { 13, "PT_AUDIT_ACCESS", "P/T audit access" }
                });

            migrationBuilder.InsertData(
                table: "PrinterAssignments",
                columns: new[] { "OutputType", "PrinterName" },
                values: new object[,]
                {
                    { (byte)0, "Reports" },
                    { (byte)1, "Barcode" },
                    { (byte)2, "Envelope" },
                    { (byte)3, "Receipt" }
                });

            migrationBuilder.InsertData(
                table: "ReceiptSettings",
                columns: new[] { "Id", "CashierPrinterEnabled", "Currency", "HeaderFooterMode", "PickupTimeDefault", "PrintOnce", "TestDetailDisplayMode", "TopMarginCm" },
                values: new object[] { 1, false, "L.E.", (byte)0, null, false, (byte)1, 1.0m });

            migrationBuilder.InsertData(
                table: "ReportSettings",
                columns: new[] { "Id", "DoctorSignatureEnabled", "FooterColor", "HeaderColor", "HeaderFooterMode", "HistoryAutoDisplayEnabled", "HistorySortMode", "PageMarginBottomCm", "PageMarginLeftCm", "PaperSize", "ReportTopSpaceCm" },
                values: new object[] { 1, false, null, null, (byte)0, true, (byte)0, 1.0m, 1.0m, (byte)0, 2.0m });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "AutoReviewAndComplete", "DailyBackupEnabled", "DailyBackupPath", "DefaultAccountType", "DisableAutoTitleInsertion", "EnablePatientNameSearchAssist", "PrintAccountInsteadOfDateOnReport", "PrintDateTimeOnTubeBarcode", "PrintFileExternalBarcode", "PrintLabIdInsteadOfPatientId", "ResultScreenAccountDisplayMode", "SaveTreatingDoctorOnlyFromEntityWindow" },
                values: new object[] { 1, false, false, null, (byte)0, false, false, false, false, false, false, (byte)0, false });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_UserId",
                table: "AttendanceRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_RelatedExternalEntityId",
                table: "CashMovements",
                column: "RelatedExternalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_CultureAntibioticResults_AntibioticId",
                table: "CultureAntibioticResults",
                column: "AntibioticId");

            migrationBuilder.CreateIndex(
                name: "IX_CultureAntibioticResults_PatientTestId",
                table: "CultureAntibioticResults",
                column: "PatientTestId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEntities_EntityType",
                table: "ExternalEntities",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEntities_PriceListId",
                table: "ExternalEntities",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPhoneNumbers_PatientId",
                table: "PatientPhoneNumbers",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPhoneNumbers_PhoneNumber",
                table: "PatientPhoneNumbers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_FullName",
                table: "Patients",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_LabId",
                table: "Patients",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NationalId",
                table: "Patients",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_RegistrationDateUtc",
                table: "Patients",
                column: "RegistrationDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PatientTests_IsReviewed_IsPrinted_IsDelivered",
                table: "PatientTests",
                columns: new[] { "IsReviewed", "IsPrinted", "IsDelivered" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientTests_PatientId",
                table: "PatientTests",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientTests_TestId",
                table: "PatientTests",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOperations_PatientId",
                table: "PaymentOperations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileResultItems_PatientTestId",
                table: "ProfileResultItems",
                column: "PatientTestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceRanges_TestId",
                table: "ReferenceRanges",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_SentOutSamplePayments_SentOutSampleId",
                table: "SentOutSamplePayments",
                column: "SentOutSampleId");

            migrationBuilder.CreateIndex(
                name: "IX_SentOutSamples_ExternalLabEntityId",
                table: "SentOutSamples",
                column: "ExternalLabEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_SentOutSamples_PatientTestId",
                table: "SentOutSamples",
                column: "PatientTestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestComments_TestId",
                table: "TestComments",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_Name",
                table: "Tests",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_TestGroupId",
                table: "Tests",
                column: "TestGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "CashMovements");

            migrationBuilder.DropTable(
                name: "CultureAntibioticAttachments");

            migrationBuilder.DropTable(
                name: "CultureAntibioticResults");

            migrationBuilder.DropTable(
                name: "CustomGroupItems");

            migrationBuilder.DropTable(
                name: "EnvelopePrintItemPositions");

            migrationBuilder.DropTable(
                name: "EnvelopeSettings");

            migrationBuilder.DropTable(
                name: "MedicalConditionTypes");

            migrationBuilder.DropTable(
                name: "PatientMedicalConditions");

            migrationBuilder.DropTable(
                name: "PatientPhoneNumbers");

            migrationBuilder.DropTable(
                name: "PatientTitles");

            migrationBuilder.DropTable(
                name: "PaymentOperations");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "PriceListItems");

            migrationBuilder.DropTable(
                name: "PrinterAssignments");

            migrationBuilder.DropTable(
                name: "ProfileResultItems");

            migrationBuilder.DropTable(
                name: "ReceiptSettings");

            migrationBuilder.DropTable(
                name: "ReferenceRanges");

            migrationBuilder.DropTable(
                name: "ReportSettings");

            migrationBuilder.DropTable(
                name: "SentOutSamplePayments");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TestComments");

            migrationBuilder.DropTable(
                name: "UserPermissionGrants");

            migrationBuilder.DropTable(
                name: "WorkGroupLogItems");

            migrationBuilder.DropTable(
                name: "Antibiotics");

            migrationBuilder.DropTable(
                name: "CultureResults");

            migrationBuilder.DropTable(
                name: "CustomGroups");

            migrationBuilder.DropTable(
                name: "SentOutSamples");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WorkGroupLogs");

            migrationBuilder.DropTable(
                name: "ExternalEntities");

            migrationBuilder.DropTable(
                name: "PatientTests");

            migrationBuilder.DropTable(
                name: "PriceLists");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "TestGroups");
        }
    }
}

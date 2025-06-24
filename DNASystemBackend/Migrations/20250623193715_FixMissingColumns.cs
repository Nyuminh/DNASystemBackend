using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DNASystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    roleID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    rolename = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__CD98460AF905C65E", x => x.roleID);
                });

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    serviceID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Service__4550733FEB577886", x => x.serviceID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    username = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    password = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fullname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gender = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    roleID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    email = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    birthdate = table.Column<DateOnly>(type: "date", nullable: true),
                    image = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__CB9A1CDFE783B793", x => x.userID);
                    table.ForeignKey(
                        name: "FK__Users__roleID__5629CD9C",
                        column: x => x.roleID,
                        principalTable: "Role",
                        principalColumn: "roleID");
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    bookingID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    customerID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    date = table.Column<DateTime>(type: "datetime", nullable: true),
                    staffID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    serviceID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    address = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Booking__C6D03BED7DEC36F2", x => x.bookingID);
                    table.ForeignKey(
                        name: "FK__Booking__custome__48CFD27E",
                        column: x => x.customerID,
                        principalTable: "Users",
                        principalColumn: "userID");
                    table.ForeignKey(
                        name: "FK__Booking__service__49C3F6B7",
                        column: x => x.serviceID,
                        principalTable: "Service",
                        principalColumn: "serviceID");
                    table.ForeignKey(
                        name: "FK__Booking__staffID__4AB81AF0",
                        column: x => x.staffID,
                        principalTable: "Users",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    courseID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    managerID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    date = table.Column<DateTime>(type: "datetime", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Course__2AA84FF1DBECDE9A", x => x.courseID);
                    table.ForeignKey(
                        name: "FK__Course__managerI__4BAC3F29",
                        column: x => x.managerID,
                        principalTable: "Users",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    feedbackID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    customerID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    serviceID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rating = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Feedback__2613FDC4B749BF2C", x => x.feedbackID);
                    table.ForeignKey(
                        name: "FK__Feedback__custom__4CA06362",
                        column: x => x.customerID,
                        principalTable: "Users",
                        principalColumn: "userID");
                    table.ForeignKey(
                        name: "FK__Feedback__servic__4D94879B",
                        column: x => x.serviceID,
                        principalTable: "Service",
                        principalColumn: "serviceID");
                });

            migrationBuilder.CreateTable(
                name: "Kit",
                columns: table => new
                {
                    kitID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    customerID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    staffID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    receivedate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Kit__98C65C8066A586FD", x => x.kitID);
                    table.ForeignKey(
                        name: "FK__Kit__customerID__5165187F",
                        column: x => x.customerID,
                        principalTable: "Users",
                        principalColumn: "userID");
                    table.ForeignKey(
                        name: "FK__Kit__staffID__52593CB8",
                        column: x => x.staffID,
                        principalTable: "Users",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "TestResult",
                columns: table => new
                {
                    resultID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    customerID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    staffID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    serviceID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    date = table.Column<DateTime>(type: "datetime", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TestResu__C6EADC7BB3693170", x => x.resultID);
                    table.ForeignKey(
                        name: "FK__TestResul__custo__534D60F1",
                        column: x => x.customerID,
                        principalTable: "Users",
                        principalColumn: "userID");
                    table.ForeignKey(
                        name: "FK__TestResul__servi__5441852A",
                        column: x => x.serviceID,
                        principalTable: "Service",
                        principalColumn: "serviceID");
                    table.ForeignKey(
                        name: "FK__TestResul__staff__5535A963",
                        column: x => x.staffID,
                        principalTable: "Users",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    invoiceID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    bookingID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    date = table.Column<DateTime>(type: "datetime", nullable: true),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Invoice__1252410C4834CE8B", x => x.invoiceID);
                    table.ForeignKey(
                        name: "FK__Invoice__booking__4E88ABD4",
                        column: x => x.bookingID,
                        principalTable: "Booking",
                        principalColumn: "bookingID");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDetail",
                columns: table => new
                {
                    invoicedetailID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    invoiceID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    serviceID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__InvoiceD__FDA5DC32D84EADAB", x => x.invoicedetailID);
                    table.ForeignKey(
                        name: "FK__InvoiceDe__invoi__4F7CD00D",
                        column: x => x.invoiceID,
                        principalTable: "Invoice",
                        principalColumn: "invoiceID");
                    table.ForeignKey(
                        name: "FK__InvoiceDe__servi__5070F446",
                        column: x => x.serviceID,
                        principalTable: "Service",
                        principalColumn: "serviceID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_customerID",
                table: "Booking",
                column: "customerID");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_serviceID",
                table: "Booking",
                column: "serviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_staffID",
                table: "Booking",
                column: "staffID");

            migrationBuilder.CreateIndex(
                name: "IX_Course_managerID",
                table: "Course",
                column: "managerID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_customerID",
                table: "Feedback",
                column: "customerID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_serviceID",
                table: "Feedback",
                column: "serviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_bookingID",
                table: "Invoice",
                column: "bookingID");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_invoiceID",
                table: "InvoiceDetail",
                column: "invoiceID");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_serviceID",
                table: "InvoiceDetail",
                column: "serviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Kit_customerID",
                table: "Kit",
                column: "customerID");

            migrationBuilder.CreateIndex(
                name: "IX_Kit_staffID",
                table: "Kit",
                column: "staffID");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_customerID",
                table: "TestResult",
                column: "customerID");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_serviceID",
                table: "TestResult",
                column: "serviceID");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_staffID",
                table: "TestResult",
                column: "staffID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_roleID",
                table: "Users",
                column: "roleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "InvoiceDetail");

            migrationBuilder.DropTable(
                name: "Kit");

            migrationBuilder.DropTable(
                name: "TestResult");

            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}

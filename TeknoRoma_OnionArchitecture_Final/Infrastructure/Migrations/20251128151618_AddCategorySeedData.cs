using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategorySeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "Description", "IsActive", "IsDeleted", "ModifiedDate", "Name" },
                values: new object[,]
                {
                    { 11, new DateTime(2024, 11, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2385), null, "Akıllı telefonlar ve cep telefonları. iPhone, Samsung, Xiaomi, Oppo ve diğer markalar.", true, false, null, "Cep Telefonları" },
                    { 12, new DateTime(2024, 11, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2401), null, "Dizüstü ve masaüstü bilgisayarlar. Gaming, iş ve öğrenci laptopları.", true, false, null, "Bilgisayar & Laptop" },
                    { 13, new DateTime(2024, 11, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2403), null, "Tablet bilgisayarlar ve iPad modelleri. Çizim ve eğitim tabletleri.", true, false, null, "Tablet & iPad" },
                    { 14, new DateTime(2024, 11, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2405), null, "LED, OLED, QLED televizyonlar. Projeksiyon ve monitörler.", true, false, null, "TV & Görüntü Sistemleri" },
                    { 15, new DateTime(2024, 11, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2407), null, "Buzdolabı, çamaşır makinesi, bulaşık makinesi, fırın ve ankastre ürünler.", true, false, null, "Beyaz Eşya" },
                    { 16, new DateTime(2024, 11, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2410), null, "Mikser, blender, kahve makinesi, ütü, elektrikli süpürge.", true, false, null, "Küçük Ev Aletleri" },
                    { 17, new DateTime(2024, 11, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2412), null, "Hoparlör, kulaklık, soundbar, home theater sistemleri.", true, false, null, "Ses Sistemleri" },
                    { 18, new DateTime(2024, 11, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2413), null, "DSLR, aynasız kameralar, aksiyon kameraları, drone'lar.", true, false, null, "Fotoğraf & Kamera" },
                    { 19, new DateTime(2024, 11, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2415), null, "PlayStation, Xbox, Nintendo Switch ve oyun aksesuarları.", true, false, null, "Oyun Konsolları" },
                    { 20, new DateTime(2024, 12, 28, 18, 16, 18, 209, DateTimeKind.Local).AddTicks(2418), null, "Smartwatch, fitness tracker, akıllı bileklikler.", true, false, null, "Akıllı Saat & Bileklik" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 20);
        }
    }
}

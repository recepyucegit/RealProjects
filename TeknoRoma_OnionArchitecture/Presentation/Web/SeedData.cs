using Domain.Enums;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Web
{
    /// <summary>
    /// Seed Data - İlk Kullanıcılar ve Roller
    ///
    /// AMAÇ:
    /// - İlk çalıştırmada varsayılan rolleri oluşturur
    /// - Her rol için demo kullanıcı oluşturur
    /// - Haluk Bey ve ekibi için kullanıcılar
    ///
    /// ROLLER:
    /// 1. SubeYoneticisi - Haluk Bey (Şube Müdürü)
    /// 2. KasaSatis - Gül Satar (Kasa Satış)
    /// 3. MobilSatis - Fahri Cepçi (Mobil Satış)
    /// 4. Depo - Kerim Zulacı (Depo)
    /// 5. Muhasebe - Feyza Paragöz (Muhasebe)
    /// 6. TeknikServis - Özgün Kablocu (Teknik Servis)
    ///
    /// KULLANICI BİLGİLERİ:
    /// Email: rol@teknoroma.com (örn: subemuduru@teknoroma.com)
    /// Password: TeknoRoma123!
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Rolleri ve kullanıcıları oluşturur
        /// </summary>
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<TeknoromaDbContext>();

            // Database'in oluşturulduğundan emin ol
            await context.Database.MigrateAsync();

            // ====== ROLLERI OLUŞTUR ======
            await CreateRolesAsync(roleManager);

            // ====== KULLANICILARI OLUŞTUR ======
            await CreateUsersAsync(userManager, context);
        }

        /// <summary>
        /// Rolleri oluşturur
        /// </summary>
        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // UserRole enum'ındaki tüm rolleri oluştur
            var roles = new[]
            {
                UserRole.SubeYoneticisi.ToString(),
                UserRole.KasaSatis.ToString(),
                UserRole.Depo.ToString(),
                UserRole.Muhasebe.ToString(),
                UserRole.TeknikServis.ToString()
            };

            // MobilSatis ayrı eklendi (enum'da yok ama kullanılabilir)
            var additionalRoles = new[] { "MobilSatis" };

            foreach (var roleName in roles.Concat(additionalRoles))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"✓ Rol oluşturuldu: {roleName}");
                }
            }
        }

        /// <summary>
        /// Demo kullanıcılarını oluşturur
        /// </summary>
        private static async Task CreateUsersAsync(
            UserManager<IdentityUser> userManager,
            TeknoromaDbContext context)
        {
            // Varsayılan şifre
            var defaultPassword = "TeknoRoma123!";

            // ====== 1. ŞUBE MÜDÜRÜtesi (Haluk Bey) ======
            await CreateUserAsync(
                userManager,
                context,
                email: "halukbey@teknoroma.com",
                username: "halukbey",
                password: defaultPassword,
                role: UserRole.SubeYoneticisi.ToString(),
                firstName: "Haluk",
                lastName: "Yönetici",
                identityNumber: "12345678901",
                storeId: 1, // İstanbul merkez
                departmentId: 1 // Yönetim
            );

            // ====== 2. KASA SATIŞ (Gül Satar) ======
            await CreateUserAsync(
                userManager,
                context,
                email: "gulsatar@teknoroma.com",
                username: "gulsatar",
                password: defaultPassword,
                role: UserRole.KasaSatis.ToString(),
                firstName: "Gül",
                lastName: "Satar",
                identityNumber: "12345678902",
                storeId: 1,
                departmentId: 2, // Satış departmanı
                salesQuota: 10000m // 10.000 TL satış kotası
            );

            // ====== 3. MOBİL SATIŞ (Fahri Cepçi) ======
            await CreateUserAsync(
                userManager,
                context,
                email: "fahricepci@teknoroma.com",
                username: "fahricepci",
                password: defaultPassword,
                role: "MobilSatis",
                firstName: "Fahri",
                lastName: "Cepçi",
                identityNumber: "12345678903",
                storeId: 1,
                departmentId: 2, // Satış departmanı
                salesQuota: 10000m
            );

            // ====== 4. DEPO (Kerim Zulacı) ======
            await CreateUserAsync(
                userManager,
                context,
                email: "kerimzulaci@teknoroma.com",
                username: "kerimzulaci",
                password: defaultPassword,
                role: UserRole.Depo.ToString(),
                firstName: "Kerim",
                lastName: "Zulacı",
                identityNumber: "12345678904",
                storeId: 1,
                departmentId: 3 // Depo departmanı
            );

            // ====== 5. MUHASEBE (Feyza Paragöz) ======
            await CreateUserAsync(
                userManager,
                context,
                email: "feyzaparagoz@teknoroma.com",
                username: "feyzaparagoz",
                password: defaultPassword,
                role: UserRole.Muhasebe.ToString(),
                firstName: "Feyza",
                lastName: "Paragöz",
                identityNumber: "12345678905",
                storeId: 1,
                departmentId: 4 // Muhasebe departmanı
            );

            // ====== 6. TEKNİK SERVİS (Özgün Kablocu) ======
            await CreateUserAsync(
                userManager,
                context,
                email: "ozgunkablocu@teknoroma.com",
                username: "ozgunkablocu",
                password: defaultPassword,
                role: UserRole.TeknikServis.ToString(),
                firstName: "Özgün",
                lastName: "Kablocu",
                identityNumber: "12345678906",
                storeId: 1,
                departmentId: 5 // Teknik Servis departmanı
            );

            Console.WriteLine("\n✓ Tüm demo kullanıcılar oluşturuldu!");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("DEMO KULLANICI BİLGİLERİ:");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("1. Şube Müdürü (Haluk Bey)");
            Console.WriteLine("   Email: halukbey@teknoroma.com");
            Console.WriteLine("   Şifre: TeknoRoma123!");
            Console.WriteLine();
            Console.WriteLine("2. Kasa Satış (Gül Satar)");
            Console.WriteLine("   Email: gulsatar@teknoroma.com");
            Console.WriteLine("   Şifre: TeknoRoma123!");
            Console.WriteLine();
            Console.WriteLine("3. Mobil Satış (Fahri Cepçi)");
            Console.WriteLine("   Email: fahricepci@teknoroma.com");
            Console.WriteLine("   Şifre: TeknoRoma123!");
            Console.WriteLine();
            Console.WriteLine("4. Depo (Kerim Zulacı)");
            Console.WriteLine("   Email: kerimzulaci@teknoroma.com");
            Console.WriteLine("   Şifre: TeknoRoma123!");
            Console.WriteLine();
            Console.WriteLine("5. Muhasebe (Feyza Paragöz)");
            Console.WriteLine("   Email: feyzaparagoz@teknoroma.com");
            Console.WriteLine("   Şifre: TeknoRoma123!");
            Console.WriteLine();
            Console.WriteLine("6. Teknik Servis (Özgün Kablocu)");
            Console.WriteLine("   Email: ozgunkablocu@teknoroma.com");
            Console.WriteLine("   Şifre: TeknoRoma123!");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        /// <summary>
        /// Tek bir kullanıcı oluşturur
        /// </summary>
        private static async Task CreateUserAsync(
            UserManager<IdentityUser> userManager,
            TeknoromaDbContext context,
            string email,
            string username,
            string password,
            string role,
            string firstName,
            string lastName,
            string identityNumber,
            int storeId,
            int departmentId,
            decimal? salesQuota = null)
        {
            // Kullanıcı zaten var mı?
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                Console.WriteLine($"  Kullanıcı zaten mevcut: {email}");
                return;
            }

            // Identity User oluştur
            var identityUser = new IdentityUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(identityUser, password);

            if (result.Succeeded)
            {
                // Role ata
                await userManager.AddToRoleAsync(identityUser, role);

                // Employee kaydı oluştur
                var employee = new Domain.Entities.Employee
                {
                    IdentityUserId = identityUser.Id,
                    IdentityNumber = identityNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = "0555 000 00 00",
                    BirthDate = DateTime.Now.AddYears(-30),
                    HireDate = DateTime.Now.AddYears(-2),
                    Salary = 15000m, // Varsayılan maaş
                    Role = (UserRole)Enum.Parse(typeof(UserRole), role == "MobilSatis" ? "KasaSatis" : role),
                    StoreId = storeId,
                    DepartmentId = departmentId,
                    SalesQuota = salesQuota,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                context.Employees.Add(employee);
                await context.SaveChangesAsync();

                Console.WriteLine($"✓ Kullanıcı oluşturuldu: {email} ({role})");
            }
            else
            {
                Console.WriteLine($"✗ Kullanıcı oluşturulamadı: {email}");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"  - {error.Description}");
                }
            }
        }
    }
}

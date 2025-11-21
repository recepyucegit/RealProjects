using AutoMapper;
using TeknoRoma.Business.DTOs;
using TeknoRoma.Business.Services.Abstract;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.Entities;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Services.Concrete;

/// <summary>
/// Sale Service Implementation - Satış işlemleri business logic
/// EN KOMPLEKS SERVİS!
///
/// NEDEN KOMPLEKs?
/// 1. Master-Detail ilişkisi (Sale + SaleDetails)
/// 2. Transaction yönetimi (atomik işlemler)
/// 3. Stok güncellemesi (Product.Stock -= Quantity)
/// 4. Çoklu validasyon (stok kontrolü, fiyat hesaplama)
///
/// TRANSACTİON KULLANIMI:
/// - CreateSale: Begin → Sale + SaleDetails + Stock Update → Commit/Rollback
/// - CancelSale: Begin → Status = Iptal + Stock Restore → Commit/Rollback
/// </summary>
public class SaleService : ISaleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SaleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    // ====== CRUD OPERATIONS ======

    public async Task<IEnumerable<SaleSummaryDto>> GetAllSalesAsync(bool includeDeleted = false)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var result = new List<SaleSummaryDto>();

        foreach (var sale in sales)
        {
            var dto = await MapToSaleSummaryDto(sale);
            result.Add(dto);
        }

        return result;
    }

    public async Task<SaleDto?> GetSaleByIdAsync(int id)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(id);
        if (sale == null) return null;

        return await MapToSaleDto(sale);
    }

    public async Task<SaleDto?> GetSaleByNumberAsync(string saleNumber)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var sale = sales.FirstOrDefault(s => s.SaleNumber == saleNumber);

        if (sale == null) return null;

        return await MapToSaleDto(sale);
    }

    /// <summary>
    /// Yeni satış oluşturur - TRANSACTION KRİTİK!
    ///
    /// İŞ AKIŞI:
    /// 1. Validasyonlar (müşteri, mağaza, çalışan, ürünler)
    /// 2. Stok kontrolü (her ürün için yeterli stok var mı?)
    /// 3. Transaction başlat
    /// 4. Sale kaydı oluştur
    /// 5. SaleDetail kayıtları oluştur
    /// 6. Stokları azalt
    /// 7. TotalAmount hesapla
    /// 8. Transaction commit
    /// 9. Hata varsa rollback
    /// </summary>
    public async Task<SaleDto?> CreateSaleAsync(CreateSaleDto createDto)
    {
        // ====== VALIDASYONLAR ======

        // Müşteri mevcut mu?
        var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
        if (customer == null)
            throw new InvalidOperationException("Müşteri bulunamadı.");

        // Mağaza mevcut mu?
        var store = await _unitOfWork.Stores.GetByIdAsync(createDto.StoreId);
        if (store == null)
            throw new InvalidOperationException("Mağaza bulunamadı.");

        // Çalışan mevcut mu?
        var employee = await _unitOfWork.Employees.GetByIdAsync(createDto.EmployeeId);
        if (employee == null)
            throw new InvalidOperationException("Çalışan bulunamadı.");

        // En az 1 ürün olmalı
        if (createDto.SaleDetails == null || createDto.SaleDetails.Count == 0)
            throw new InvalidOperationException("En az 1 ürün eklemelisiniz.");

        // ====== STOK KONTROLÜ ======

        var products = new List<Product>();
        foreach (var detail in createDto.SaleDetails)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Ürün bulunamadı (ID: {detail.ProductId}).");

            if (product.Stock < detail.Quantity)
                throw new InvalidOperationException(
                    $"'{product.Name}' ürünü için stok yetersiz. " +
                    $"Mevcut: {product.Stock}, İstenen: {detail.Quantity}");

            products.Add(product);
        }

        // ====== TRANSACTION BAŞLAT ======

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. SaleNumber oluştur
            var saleNumber = await GenerateSaleNumberAsync();

            // 2. TotalAmount hesapla
            decimal subTotal = createDto.SaleDetails.Sum(d => (d.UnitPrice * d.Quantity) - d.Discount);
            decimal totalAmount = subTotal + createDto.ShippingCost - createDto.DiscountAmount;

            // 3. Sale kaydı oluştur
            var sale = new Sale
            {
                SaleNumber = saleNumber,
                SaleDate = createDto.SaleDate,
                Status = SaleStatus.Beklemede,
                PaymentMethod = createDto.PaymentMethod,
                IsPaid = createDto.IsPaid,
                PaymentDate = createDto.IsPaid ? DateTime.Now : null,
                ShippingAddress = createDto.ShippingAddress,
                ShippingCost = createDto.ShippingCost,
                TotalAmount = totalAmount,
                DiscountAmount = createDto.DiscountAmount,
                Notes = createDto.Notes,
                CustomerId = createDto.CustomerId,
                StoreId = createDto.StoreId,
                EmployeeId = createDto.EmployeeId
            };

            await _unitOfWork.Sales.AddAsync(sale);
            await _unitOfWork.CommitAsync(); // Sale.Id oluşsun

            // 4. SaleDetail kayıtları oluştur ve stokları azalt
            for (int i = 0; i < createDto.SaleDetails.Count; i++)
            {
                var detailDto = createDto.SaleDetails[i];
                var product = products[i];

                // SaleDetail oluştur
                var saleDetail = new SaleDetail
                {
                    SaleId = sale.Id,
                    ProductId = detailDto.ProductId,
                    UnitPrice = detailDto.UnitPrice,
                    Quantity = detailDto.Quantity,
                    Discount = detailDto.Discount
                };

                await _unitOfWork.SaleDetails.AddAsync(saleDetail);

                // Stok azalt
                product.Stock -= detailDto.Quantity;

                // StockStatus güncelle
                product.StockStatus = CalculateStockStatus(product.Stock, product.CriticalStockLevel);

                _unitOfWork.Products.Update(product);
            }

            // 5. Transaction commit
            await _unitOfWork.CommitTransactionAsync();

            // 6. Oluşturulan satışı dön
            return await GetSaleByIdAsync(sale.Id);
        }
        catch
        {
            // Hata olursa rollback
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<SaleDto?> UpdateSaleAsync(UpdateSaleDto updateDto)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(updateDto.Id);
        if (sale == null) return null;

        // BUSINESS LOGIC: İptal için ayrı metod kullan
        if (updateDto.Status == SaleStatus.Iptal)
            throw new InvalidOperationException(
                "İptal işlemi için CancelSaleAsync metodunu kullanın (stok iadesi gerekir).");

        // BUSINESS LOGIC: Tamamlandı ise ödeme zorunlu
        if (updateDto.Status == SaleStatus.Tamamlandi && !updateDto.IsPaid)
            throw new InvalidOperationException(
                "Satış tamamlandı olarak işaretlenemez, önce ödeme alınmalı.");

        // Güncellenebilir alanlar
        sale.Status = updateDto.Status;
        sale.IsPaid = updateDto.IsPaid;
        sale.PaymentDate = updateDto.PaymentDate;
        sale.Notes = updateDto.Notes;

        _unitOfWork.Sales.Update(sale);
        var saved = await _unitOfWork.SaveChangesAsync();

        if (!saved) return null;

        return await GetSaleByIdAsync(sale.Id);
    }

    public async Task<bool> DeleteSaleAsync(int id)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(id);
        if (sale == null) return false;

        _unitOfWork.Sales.Delete(sale);
        return await _unitOfWork.SaveChangesAsync();
    }


    // ====== BUSINESS LOGIC METHODS ======

    public async Task<IEnumerable<SaleSummaryDto>> GetSalesByStoreAsync(int storeId, DateTime? startDate = null, DateTime? endDate = null, SaleStatus? status = null)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var storeSales = sales.Where(s => s.StoreId == storeId);

        if (startDate.HasValue)
            storeSales = storeSales.Where(s => s.SaleDate >= startDate.Value);

        if (endDate.HasValue)
            storeSales = storeSales.Where(s => s.SaleDate <= endDate.Value);

        if (status.HasValue)
            storeSales = storeSales.Where(s => s.Status == status.Value);

        var result = new List<SaleSummaryDto>();
        foreach (var sale in storeSales)
        {
            result.Add(await MapToSaleSummaryDto(sale));
        }

        return result;
    }

    public async Task<IEnumerable<SaleDto>> GetSalesByCustomerAsync(int customerId)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var customerSales = sales.Where(s => s.CustomerId == customerId);

        var result = new List<SaleDto>();
        foreach (var sale in customerSales)
        {
            result.Add(await MapToSaleDto(sale));
        }

        return result;
    }

    public async Task<IEnumerable<SaleSummaryDto>> GetSalesByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var employeeSales = sales.Where(s => s.EmployeeId == employeeId);

        if (startDate.HasValue)
            employeeSales = employeeSales.Where(s => s.SaleDate >= startDate.Value);

        if (endDate.HasValue)
            employeeSales = employeeSales.Where(s => s.SaleDate <= endDate.Value);

        var result = new List<SaleSummaryDto>();
        foreach (var sale in employeeSales)
        {
            result.Add(await MapToSaleSummaryDto(sale));
        }

        return result;
    }

    public async Task<IEnumerable<SaleSummaryDto>> GetPendingSalesAsync(int? storeId = null)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var pending = sales.Where(s =>
            s.Status == SaleStatus.Beklemede ||
            s.Status == SaleStatus.Hazirlaniyor);

        if (storeId.HasValue)
            pending = pending.Where(s => s.StoreId == storeId.Value);

        var result = new List<SaleSummaryDto>();
        foreach (var sale in pending)
        {
            result.Add(await MapToSaleSummaryDto(sale));
        }

        return result;
    }

    public async Task<IEnumerable<SaleSummaryDto>> GetUnpaidSalesAsync(int? storeId = null)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var unpaid = sales.Where(s => !s.IsPaid && s.Status != SaleStatus.Iptal);

        if (storeId.HasValue)
            unpaid = unpaid.Where(s => s.StoreId == storeId.Value);

        var result = new List<SaleSummaryDto>();
        foreach (var sale in unpaid)
        {
            result.Add(await MapToSaleSummaryDto(sale));
        }

        return result;
    }

    /// <summary>
    /// Satışı iptal eder ve stokları geri ekler - TRANSACTION KRİTİK!
    /// </summary>
    public async Task<bool> CancelSaleAsync(int saleId, string cancelReason)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(saleId);
        if (sale == null) return false;

        // BUSINESS LOGIC: Sadece Beklemede veya Hazirlaniyor iptal edilebilir
        if (sale.Status == SaleStatus.Tamamlandi)
            throw new InvalidOperationException(
                "Tamamlanmış satış iptal edilemez. İade işlemi başlatın.");

        if (sale.Status == SaleStatus.Iptal)
            throw new InvalidOperationException("Bu satış zaten iptal edilmiş.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // SaleDetails al ve stokları geri ekle
            var saleDetails = await _unitOfWork.SaleDetails.GetAllAsync();
            var saleDet = saleDetails.Where(sd => sd.SaleId == saleId).ToList();

            foreach (var detail in saleDet)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
                if (product != null)
                {
                    // Stok iadesi
                    product.Stock += detail.Quantity;
                    product.StockStatus = CalculateStockStatus(product.Stock, product.CriticalStockLevel);
                    _unitOfWork.Products.Update(product);
                }
            }

            // Sale durumunu güncelle
            sale.Status = SaleStatus.Iptal;
            sale.Notes = $"{sale.Notes}\n[İPTAL: {DateTime.Now:dd.MM.yyyy HH:mm}] {cancelReason}";
            _unitOfWork.Sales.Update(sale);

            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> MarkSaleAsPaidAsync(int saleId)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(saleId);
        if (sale == null) return false;

        sale.IsPaid = true;
        sale.PaymentDate = DateTime.Now;

        // Beklemedeyse hazırlanıyor yap
        if (sale.Status == SaleStatus.Beklemede)
            sale.Status = SaleStatus.Hazirlaniyor;

        _unitOfWork.Sales.Update(sale);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> CompleteSaleAsync(int saleId)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(saleId);
        if (sale == null) return false;

        if (!sale.IsPaid)
            throw new InvalidOperationException("Ödeme alınmadan satış tamamlanamaz.");

        if (sale.Status != SaleStatus.Hazirlaniyor)
            throw new InvalidOperationException("Sadece 'Hazırlanıyor' durumundaki satışlar tamamlanabilir.");

        sale.Status = SaleStatus.Tamamlandi;
        _unitOfWork.Sales.Update(sale);

        return await _unitOfWork.SaveChangesAsync();
    }

    public decimal CalculateTotalAmount(List<CreateSaleDetailDto> saleDetails, decimal shippingCost, decimal discountAmount)
    {
        decimal subTotal = saleDetails.Sum(d => (d.UnitPrice * d.Quantity) - d.Discount);
        return subTotal + shippingCost - discountAmount;
    }

    public async Task<object> GetDailySalesReportAsync(DateTime date, int? storeId = null)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var dailySales = sales.Where(s =>
            s.SaleDate.Date == date.Date &&
            s.Status != SaleStatus.Iptal);

        if (storeId.HasValue)
            dailySales = dailySales.Where(s => s.StoreId == storeId.Value);

        var salesList = dailySales.ToList();

        return new
        {
            Date = date.Date,
            TotalSalesCount = salesList.Count,
            TotalSalesAmount = salesList.Sum(s => s.TotalAmount),
            AverageOrderValue = salesList.Count > 0 ? Math.Round(salesList.Average(s => s.TotalAmount), 2) : 0,
            CompletedCount = salesList.Count(s => s.Status == SaleStatus.Tamamlandi),
            PendingCount = salesList.Count(s => s.Status == SaleStatus.Beklemede || s.Status == SaleStatus.Hazirlaniyor),
            PaidAmount = salesList.Where(s => s.IsPaid).Sum(s => s.TotalAmount),
            UnpaidAmount = salesList.Where(s => !s.IsPaid).Sum(s => s.TotalAmount)
        };
    }

    public async Task<object> GetMonthlySalesReportAsync(int year, int month, int? storeId = null)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var monthlySales = sales.Where(s =>
            s.SaleDate.Year == year &&
            s.SaleDate.Month == month &&
            s.Status != SaleStatus.Iptal);

        if (storeId.HasValue)
            monthlySales = monthlySales.Where(s => s.StoreId == storeId.Value);

        var salesList = monthlySales.ToList();

        return new
        {
            Year = year,
            Month = month,
            TotalSalesCount = salesList.Count,
            TotalSalesAmount = salesList.Sum(s => s.TotalAmount),
            DailyBreakdown = salesList
                .GroupBy(s => s.SaleDate.Day)
                .Select(g => new { Day = g.Key, Total = g.Sum(s => s.TotalAmount), Count = g.Count() })
                .OrderBy(x => x.Day)
        };
    }

    public async Task<object> GetTopSellingProductsAsync(int topCount, DateTime startDate, DateTime endDate, int? storeId = null)
    {
        var saleDetails = await _unitOfWork.SaleDetails.GetAllAsync();
        var sales = await _unitOfWork.Sales.GetAllAsync();

        var validSaleIds = sales
            .Where(s => s.SaleDate >= startDate &&
                       s.SaleDate <= endDate &&
                       s.Status == SaleStatus.Tamamlandi &&
                       (!storeId.HasValue || s.StoreId == storeId.Value))
            .Select(s => s.Id)
            .ToList();

        var topProducts = saleDetails
            .Where(sd => validSaleIds.Contains(sd.SaleId))
            .GroupBy(sd => sd.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQuantity = g.Sum(sd => sd.Quantity),
                TotalAmount = g.Sum(sd => sd.LineTotal)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(topCount)
            .ToList();

        var result = new List<object>();
        foreach (var item in topProducts)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            result.Add(new
            {
                ProductId = item.ProductId,
                ProductName = product?.Name ?? "Bilinmiyor",
                TotalQuantity = item.TotalQuantity,
                TotalAmount = item.TotalAmount
            });
        }

        return result;
    }

    public async Task<object> GetEmployeeSalesPerformanceAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var employeeSales = sales
            .Where(s => s.EmployeeId == employeeId &&
                       s.SaleDate >= startDate &&
                       s.SaleDate <= endDate &&
                       s.Status == SaleStatus.Tamamlandi)
            .ToList();

        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);

        return new
        {
            EmployeeId = employeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Bilinmiyor",
            Period = new { StartDate = startDate, EndDate = endDate },
            TotalSalesCount = employeeSales.Count,
            TotalSalesAmount = employeeSales.Sum(s => s.TotalAmount),
            AverageOrderValue = employeeSales.Count > 0 ? Math.Round(employeeSales.Average(s => s.TotalAmount), 2) : 0,
            SalesQuota = employee?.SalesQuota,
            QuotaAchievement = employee?.SalesQuota > 0
                ? Math.Round(employeeSales.Sum(s => s.TotalAmount) / employee.SalesQuota.Value * 100, 2)
                : (decimal?)null
        };
    }

    public async Task<decimal> GetTotalSalesAmountAsync(int storeId, DateTime startDate, DateTime endDate)
    {
        var sales = await _unitOfWork.Sales.GetAllAsync();
        return sales
            .Where(s => s.StoreId == storeId &&
                       s.SaleDate >= startDate &&
                       s.SaleDate <= endDate &&
                       s.Status == SaleStatus.Tamamlandi)
            .Sum(s => s.TotalAmount);
    }


    // ====== PRIVATE HELPER METHODS ======

    private async Task<string> GenerateSaleNumberAsync()
    {
        var year = DateTime.Now.Year;
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var yearSales = sales.Where(s => s.SaleNumber.Contains($"S-{year}-"));

        var maxNumber = 0;
        foreach (var sale in yearSales)
        {
            var parts = sale.SaleNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int num))
            {
                if (num > maxNumber) maxNumber = num;
            }
        }

        return $"S-{year}-{(maxNumber + 1):D5}";
    }

    private StockStatus CalculateStockStatus(int stock, int? criticalLevel)
    {
        if (stock <= 0) return StockStatus.Tukendi;
        if (criticalLevel.HasValue && stock <= criticalLevel.Value) return StockStatus.Azaliyor;
        if (stock > 1000) return StockStatus.CokFazla;
        return StockStatus.Yeterli;
    }

    private async Task<SaleDto> MapToSaleDto(Sale sale)
    {
        var dto = _mapper.Map<SaleDto>(sale);

        // İlişkili verileri getir
        var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId);
        var store = await _unitOfWork.Stores.GetByIdAsync(sale.StoreId);
        var employee = await _unitOfWork.Employees.GetByIdAsync(sale.EmployeeId);

        dto.CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Bilinmiyor";
        dto.StoreName = store?.StoreName ?? "Bilinmiyor";
        dto.EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Bilinmiyor";

        // SaleDetails getir
        var allDetails = await _unitOfWork.SaleDetails.GetAllAsync();
        var saleDetails = allDetails.Where(sd => sd.SaleId == sale.Id).ToList();

        dto.SaleDetails = new List<SaleDetailDto>();
        foreach (var detail in saleDetails)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
            dto.SaleDetails.Add(new SaleDetailDto
            {
                Id = detail.Id,
                ProductId = detail.ProductId,
                ProductName = product?.Name ?? "Bilinmiyor",
                UnitPrice = detail.UnitPrice,
                Quantity = detail.Quantity,
                Discount = detail.Discount
            });
        }

        return dto;
    }

    private async Task<SaleSummaryDto> MapToSaleSummaryDto(Sale sale)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId);
        var store = await _unitOfWork.Stores.GetByIdAsync(sale.StoreId);
        var employee = await _unitOfWork.Employees.GetByIdAsync(sale.EmployeeId);

        var allDetails = await _unitOfWork.SaleDetails.GetAllAsync();
        var saleDetails = allDetails.Where(sd => sd.SaleId == sale.Id).ToList();

        return new SaleSummaryDto
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            SaleDate = sale.SaleDate,
            Status = sale.Status,
            StatusText = sale.Status.ToString(),
            TotalAmount = sale.TotalAmount,
            IsPaid = sale.IsPaid,
            CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Bilinmiyor",
            StoreName = store?.StoreName ?? "Bilinmiyor",
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Bilinmiyor",
            TotalItemCount = saleDetails.Sum(sd => sd.Quantity)
        };
    }
}

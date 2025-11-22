using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeknoRoma.Business.DTOs;
using TeknoRoma.Business.Services.Abstract;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.Entities;

namespace TeknoRoma.Business.Services.Concrete;

/// <summary>
/// Store Service Implementation - Mağaza işlemleri business logic
///
/// DEPENDENCY INJECTION:
/// - IUnitOfWork: Repository'lere erişim ve transaction yönetimi
/// - IMapper: Entity ↔ DTO dönüşümü
///
/// NEDEN Constructor Injection?
/// - Loose coupling: Service runtime'da enjekte edilir
/// - Testability: Mock ile test kolaylaşır
/// - Single Responsibility: Her dependency'nin tek görevi var
/// </summary>
public class StoreService : IStoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Constructor - Dependency Injection ile bağımlılıkları al
    /// </summary>
    public StoreService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    // ====== CRUD OPERATIONS ======

    public async Task<IEnumerable<StoreDto>> GetAllStoresAsync(bool includeInactive = false)
    {
        var stores = await _unitOfWork.Stores.GetAllAsync();

        if (!includeInactive)
        {
            stores = stores.Where(s => s.IsActive);
        }

        var storeDtos = _mapper.Map<IEnumerable<StoreDto>>(stores);

        // Her mağaza için çalışan sayısını hesapla
        foreach (var storeDto in storeDtos)
        {
            storeDto.CurrentEmployeeCount = await GetEmployeeCountAsync(storeDto.Id);
        }

        return storeDtos;
    }

    public async Task<StoreDto?> GetStoreByIdAsync(int id)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null) return null;

        var storeDto = _mapper.Map<StoreDto>(store);
        storeDto.CurrentEmployeeCount = await GetEmployeeCountAsync(id);

        return storeDto;
    }

    public async Task<StoreDto?> GetStoreByCodeAsync(string storeCode)
    {
        var stores = await _unitOfWork.Stores.GetAllAsync();
        var store = stores.FirstOrDefault(s => s.StoreCode == storeCode);

        if (store == null) return null;

        var storeDto = _mapper.Map<StoreDto>(store);
        storeDto.CurrentEmployeeCount = await GetEmployeeCountAsync(store.Id);

        return storeDto;
    }

    public async Task<StoreDto?> CreateStoreAsync(CreateStoreDto createStoreDto)
    {
        // VALIDASYON: StoreCode benzersiz olmalı
        var existingStores = await _unitOfWork.Stores.GetAllAsync();
        if (existingStores.Any(s => s.StoreCode == createStoreDto.StoreCode))
        {
            throw new InvalidOperationException($"'{createStoreDto.StoreCode}' kodlu mağaza zaten mevcut.");
        }

        // DTO → Entity dönüşümü
        var store = _mapper.Map<Store>(createStoreDto);

        // Repository'ye ekle
        await _unitOfWork.Stores.AddAsync(store);

        // Kaydet
        var saved = await _unitOfWork.SaveChangesAsync();
        if (!saved) return null;

        // Oluşturulan entity'yi DTO olarak dön
        return _mapper.Map<StoreDto>(store);
    }

    public async Task<StoreDto?> UpdateStoreAsync(UpdateStoreDto updateStoreDto)
    {
        // Mevcut kaydı bul
        var store = await _unitOfWork.Stores.GetByIdAsync(updateStoreDto.Id);
        if (store == null) return null;

        // Güncellenebilir alanları güncelle
        // NOT: StoreCode ve OpeningDate değiştirilemez
        store.StoreName = updateStoreDto.StoreName;
        store.City = updateStoreDto.City;
        store.District = updateStoreDto.District;
        store.Address = updateStoreDto.Address;
        store.Phone = updateStoreDto.Phone;
        store.SquareMeters = updateStoreDto.SquareMeters;
        store.EmployeeCapacity = updateStoreDto.EmployeeCapacity;
        store.IsActive = updateStoreDto.IsActive;

        // Repository'de güncelle
        _unitOfWork.Stores.Update(store);

        // Kaydet
        var saved = await _unitOfWork.SaveChangesAsync();
        if (!saved) return null;

        var storeDto = _mapper.Map<StoreDto>(store);
        storeDto.CurrentEmployeeCount = await GetEmployeeCountAsync(store.Id);

        return storeDto;
    }

    public async Task<bool> DeleteStoreAsync(int id)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null) return false;

        // BUSINESS LOGIC: Mağazada çalışan varsa silinemez
        var employeeCount = await GetEmployeeCountAsync(id);
        if (employeeCount > 0)
        {
            throw new InvalidOperationException(
                $"Bu mağazada {employeeCount} çalışan bulunuyor. Önce çalışanları başka mağazaya transfer edin.");
        }

        // Soft Delete
        _unitOfWork.Stores.Delete(store);
        return await _unitOfWork.SaveChangesAsync();
    }


    // ====== BUSINESS LOGIC METHODS ======

    public async Task<IEnumerable<StoreDto>> GetStoresByCityAsync(string city)
    {
        var stores = await _unitOfWork.Stores.GetAllAsync();
        var cityStores = stores.Where(s => s.City.ToLower() == city.ToLower() && s.IsActive);

        var storeDtos = _mapper.Map<IEnumerable<StoreDto>>(cityStores);

        foreach (var storeDto in storeDtos)
        {
            storeDto.CurrentEmployeeCount = await GetEmployeeCountAsync(storeDto.Id);
        }

        return storeDtos;
    }

    public async Task<bool> HasEmployeeCapacityAsync(int storeId)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(storeId);
        if (store == null) return false;

        var currentCount = await GetEmployeeCountAsync(storeId);
        return currentCount < store.EmployeeCapacity;
    }

    public async Task<int> GetEmployeeCountAsync(int storeId)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        return employees.Count(e => e.StoreId == storeId && e.IsActive);
    }

    public async Task<object> GetStorePerformanceReportAsync(int storeId, DateTime startDate, DateTime endDate)
    {
        // Mağaza bilgisi
        var store = await _unitOfWork.Stores.GetByIdAsync(storeId);
        if (store == null)
            throw new InvalidOperationException("Mağaza bulunamadı.");

        // Satışlar
        var sales = await _unitOfWork.Sales.GetAllAsync();
        var storeSales = sales
            .Where(s => s.StoreId == storeId &&
                       s.SaleDate >= startDate &&
                       s.SaleDate <= endDate &&
                       s.Status == Entities.Enums.SaleStatus.Tamamlandi);

        var totalSalesAmount = storeSales.Sum(s => s.TotalAmount);
        var salesCount = storeSales.Count();

        // Giderler
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var storeExpenses = expenses
            .Where(e => e.StoreId == storeId &&
                       e.ExpenseDate >= startDate &&
                       e.ExpenseDate <= endDate);

        var totalExpenseAmount = storeExpenses.Sum(e => e.AmountInTRY);

        // Çalışan sayısı
        var employeeCount = await GetEmployeeCountAsync(storeId);

        // Hesaplamalar
        var netProfit = totalSalesAmount - totalExpenseAmount;
        var salesPerEmployee = employeeCount > 0 ? totalSalesAmount / employeeCount : 0;
        var salesPerSquareMeter = store.SquareMeters > 0 ? totalSalesAmount / store.SquareMeters : 0;

        return new
        {
            StoreId = storeId,
            StoreName = store.StoreName,
            StoreCode = store.StoreCode,
            Period = new { StartDate = startDate, EndDate = endDate },
            TotalSalesAmount = totalSalesAmount,
            SalesCount = salesCount,
            TotalExpenseAmount = totalExpenseAmount,
            NetProfit = netProfit,
            EmployeeCount = employeeCount,
            SalesPerEmployee = Math.Round(salesPerEmployee, 2),
            SalesPerSquareMeter = Math.Round(salesPerSquareMeter, 2),
            AverageSaleAmount = salesCount > 0 ? Math.Round(totalSalesAmount / salesCount, 2) : 0
        };
    }
}

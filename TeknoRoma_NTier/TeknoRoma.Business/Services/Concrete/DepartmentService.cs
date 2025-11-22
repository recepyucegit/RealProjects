using AutoMapper;
using TeknoRoma.Business.DTOs;
using TeknoRoma.Business.Services.Abstract;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.Entities;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Services.Concrete;

/// <summary>
/// Department Service Implementation - Departman işlemleri business logic
///
/// İŞ KURALLARI:
/// - Her mağazada 5 temel departman otomatik oluşturulabilir
/// - DepartmentCode mağaza içinde benzersiz olmalı
/// - Departman müdürü atandığında rol kontrolü yapılır
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // ====== CRUD OPERATIONS ======

    public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(bool includeInactive = false)
    {
        var departments = await _unitOfWork.Departments.GetAllAsync();

        if (!includeInactive)
        {
            departments = departments.Where(d => d.IsActive);
        }

        var departmentDtos = new List<DepartmentDto>();

        foreach (var dept in departments)
        {
            var dto = _mapper.Map<DepartmentDto>(dept);

            // Store adını getir
            var store = await _unitOfWork.Stores.GetByIdAsync(dept.StoreId);
            dto.StoreName = store?.StoreName ?? "Bilinmiyor";

            // Manager adını getir
            if (dept.ManagerEmployeeId.HasValue)
            {
                var manager = await _unitOfWork.Employees.GetByIdAsync(dept.ManagerEmployeeId.Value);
                dto.ManagerFullName = manager != null ? $"{manager.FirstName} {manager.LastName}" : null;
            }

            // Çalışan sayısı
            dto.EmployeeCount = await GetEmployeeCountAsync(dept.Id);

            departmentDtos.Add(dto);
        }

        return departmentDtos;
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null) return null;

        var dto = _mapper.Map<DepartmentDto>(department);

        var store = await _unitOfWork.Stores.GetByIdAsync(department.StoreId);
        dto.StoreName = store?.StoreName ?? "Bilinmiyor";

        if (department.ManagerEmployeeId.HasValue)
        {
            var manager = await _unitOfWork.Employees.GetByIdAsync(department.ManagerEmployeeId.Value);
            dto.ManagerFullName = manager != null ? $"{manager.FirstName} {manager.LastName}" : null;
        }

        dto.EmployeeCount = await GetEmployeeCountAsync(id);

        return dto;
    }

    public async Task<DepartmentDto?> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto)
    {
        // VALIDASYON: Mağaza mevcut mu?
        var store = await _unitOfWork.Stores.GetByIdAsync(createDepartmentDto.StoreId);
        if (store == null)
            throw new InvalidOperationException("Mağaza bulunamadı.");

        // VALIDASYON: DepartmentCode mağaza içinde benzersiz olmalı
        var departments = await _unitOfWork.Departments.GetAllAsync();
        var existsInStore = departments.Any(d =>
            d.StoreId == createDepartmentDto.StoreId &&
            d.DepartmentCode == createDepartmentDto.DepartmentCode);

        if (existsInStore)
            throw new InvalidOperationException(
                $"Bu mağazada '{createDepartmentDto.DepartmentCode}' kodlu departman zaten mevcut.");

        var department = _mapper.Map<Department>(createDepartmentDto);
        await _unitOfWork.Departments.AddAsync(department);

        var saved = await _unitOfWork.SaveChangesAsync();
        if (!saved) return null;

        return await GetDepartmentByIdAsync(department.Id);
    }

    public async Task<DepartmentDto?> UpdateDepartmentAsync(UpdateDepartmentDto updateDepartmentDto)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(updateDepartmentDto.Id);
        if (department == null) return null;

        // Güncellenebilir alanları güncelle
        department.DepartmentName = updateDepartmentDto.DepartmentName;
        department.Description = updateDepartmentDto.Description;
        department.ManagerEmployeeId = updateDepartmentDto.ManagerEmployeeId;
        department.IsActive = updateDepartmentDto.IsActive;

        _unitOfWork.Departments.Update(department);
        var saved = await _unitOfWork.SaveChangesAsync();

        if (!saved) return null;

        return await GetDepartmentByIdAsync(department.Id);
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null) return false;

        // BUSINESS LOGIC: Departmanda çalışan varsa silinemez
        var employeeCount = await GetEmployeeCountAsync(id);
        if (employeeCount > 0)
            throw new InvalidOperationException(
                $"Bu departmanda {employeeCount} çalışan bulunuyor. Önce çalışanları transfer edin.");

        _unitOfWork.Departments.Delete(department);
        return await _unitOfWork.SaveChangesAsync();
    }


    // ====== BUSINESS LOGIC METHODS ======

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByStoreAsync(int storeId)
    {
        var departments = await _unitOfWork.Departments.GetAllAsync();
        var storeDepartments = departments.Where(d => d.StoreId == storeId && d.IsActive);

        var result = new List<DepartmentDto>();
        foreach (var dept in storeDepartments)
        {
            var dto = _mapper.Map<DepartmentDto>(dept);
            dto.EmployeeCount = await GetEmployeeCountAsync(dept.Id);
            result.Add(dto);
        }

        return result;
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByTypeAsync(UserRole departmentType)
    {
        var departments = await _unitOfWork.Departments.GetAllAsync();
        var typeDepartments = departments.Where(d => d.DepartmentType == departmentType && d.IsActive);

        return _mapper.Map<IEnumerable<DepartmentDto>>(typeDepartments);
    }

    public async Task<bool> AssignManagerAsync(int departmentId, int employeeId)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
        if (department == null)
            throw new InvalidOperationException("Departman bulunamadı.");

        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            throw new InvalidOperationException("Çalışan bulunamadı.");

        // BUSINESS LOGIC: Çalışan bu departmanda mı?
        if (employee.DepartmentId != departmentId)
            throw new InvalidOperationException("Çalışan bu departmanda çalışmıyor.");

        department.ManagerEmployeeId = employeeId;
        _unitOfWork.Departments.Update(department);

        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetEmployeeCountAsync(int departmentId)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        return employees.Count(e => e.DepartmentId == departmentId && e.IsActive);
    }

    public async Task<int> CreateDefaultDepartmentsForStoreAsync(int storeId)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(storeId);
        if (store == null)
            throw new InvalidOperationException("Mağaza bulunamadı.");

        // 5 Temel departman tanımları
        var defaultDepartments = new List<(string Name, string Code, UserRole Type, string Desc)>
        {
            ("Mağaza Yönetimi", "YON", UserRole.SubeYoneticisi, "Mağaza yönetim ve koordinasyon"),
            ("Kasa/Satış", "SAT", UserRole.KasaSatis, "Satış işlemleri ve müşteri hizmetleri"),
            ("Depo", "DEP", UserRole.Depo, "Stok yönetimi ve lojistik"),
            ("Muhasebe", "MUH", UserRole.Muhasebe, "Mali işler ve gider yönetimi"),
            ("Teknik Servis", "TEK", UserRole.TeknikServis, "Teknik destek ve servis hizmetleri")
        };

        int createdCount = 0;

        foreach (var (name, code, type, desc) in defaultDepartments)
        {
            // Zaten varsa atla
            var existing = (await _unitOfWork.Departments.GetAllAsync())
                .FirstOrDefault(d => d.StoreId == storeId && d.DepartmentCode == code);

            if (existing != null) continue;

            var department = new Department
            {
                DepartmentName = name,
                DepartmentCode = code,
                DepartmentType = type,
                Description = desc,
                StoreId = storeId,
                IsActive = true
            };

            await _unitOfWork.Departments.AddAsync(department);
            createdCount++;
        }

        await _unitOfWork.SaveChangesAsync();
        return createdCount;
    }
}

using AutoMapper;
using TeknoRoma.Business.DTOs;
using TeknoRoma.Business.Services.Abstract;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.Entities;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Services.Concrete;

/// <summary>
/// Expense Service Implementation - Gider yönetimi business logic
/// Feyza Paragöz'ün (Muhasebe) kullandığı servis
///
/// ÖNEMLİ ÖZELLİKLER:
/// - Otomatik ExpenseNumber oluşturma (G-{Yıl}-{5 hane})
/// - Çoklu para birimi desteği ve döviz kuru hesaplama
/// - AmountInTRY otomatik hesaplama
/// - Vade takibi ve gecikme hesaplama
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    // ====== CRUD OPERATIONS ======

    public async Task<IEnumerable<ExpenseSummaryDto>> GetAllExpensesAsync(bool includeDeleted = false)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var result = new List<ExpenseSummaryDto>();

        foreach (var expense in expenses)
        {
            var dto = _mapper.Map<ExpenseSummaryDto>(expense);

            var store = await _unitOfWork.Stores.GetByIdAsync(expense.StoreId);
            dto.StoreName = store?.StoreName ?? "Bilinmiyor";

            if (expense.EmployeeId.HasValue)
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(expense.EmployeeId.Value);
                dto.EmployeeFullName = employee != null ? $"{employee.FirstName} {employee.LastName}" : null;
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<ExpenseDto?> GetExpenseByIdAsync(int id)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null) return null;

        var dto = _mapper.Map<ExpenseDto>(expense);

        var store = await _unitOfWork.Stores.GetByIdAsync(expense.StoreId);
        dto.StoreName = store?.StoreName ?? "Bilinmiyor";

        if (expense.EmployeeId.HasValue)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(expense.EmployeeId.Value);
            dto.EmployeeFullName = employee != null ? $"{employee.FirstName} {employee.LastName}" : null;
        }

        return dto;
    }

    public async Task<ExpenseDto?> GetExpenseByNumberAsync(string expenseNumber)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var expense = expenses.FirstOrDefault(e => e.ExpenseNumber == expenseNumber);

        if (expense == null) return null;

        return await GetExpenseByIdAsync(expense.Id);
    }

    public async Task<ExpenseDto?> CreateExpenseAsync(CreateExpenseDto createDto)
    {
        // VALIDASYON: Mağaza mevcut mu?
        var store = await _unitOfWork.Stores.GetByIdAsync(createDto.StoreId);
        if (store == null)
            throw new InvalidOperationException("Mağaza bulunamadı.");

        // VALIDASYON: CalisanOdemesi ise EmployeeId zorunlu
        if (createDto.ExpenseType == ExpenseType.CalisanOdemesi && !createDto.EmployeeId.HasValue)
            throw new InvalidOperationException("Çalışan ödemesi için çalışan seçimi zorunludur.");

        // VALIDASYON: EmployeeId verilmişse çalışan mevcut mu?
        if (createDto.EmployeeId.HasValue)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(createDto.EmployeeId.Value);
            if (employee == null)
                throw new InvalidOperationException("Çalışan bulunamadı.");
        }

        // ExpenseNumber oluştur: G-{Yıl}-{5 haneli sıra}
        var expenseNumber = await GenerateExpenseNumberAsync();

        // Döviz kuru hesaplama
        decimal exchangeRate = 1m;
        if (createDto.Currency != Currency.TRY)
        {
            exchangeRate = createDto.ExchangeRate ?? await GetCurrentExchangeRateAsync(createDto.Currency);
        }

        // AmountInTRY hesapla
        decimal amountInTRY = createDto.Amount * exchangeRate;

        var expense = new Expense
        {
            ExpenseNumber = expenseNumber,
            ExpenseDate = createDto.ExpenseDate,
            ExpenseType = createDto.ExpenseType,
            Amount = createDto.Amount,
            Currency = createDto.Currency,
            ExchangeRate = createDto.Currency == Currency.TRY ? null : exchangeRate,
            AmountInTRY = amountInTRY,
            Description = createDto.Description,
            DocumentNumber = createDto.DocumentNumber,
            IsPaid = createDto.IsPaid,
            PaymentDate = createDto.IsPaid ? createDto.PaymentDate ?? DateTime.Now : createDto.PaymentDate,
            PaymentMethod = createDto.PaymentMethod,
            StoreId = createDto.StoreId,
            EmployeeId = createDto.EmployeeId
        };

        await _unitOfWork.Expenses.AddAsync(expense);
        var saved = await _unitOfWork.SaveChangesAsync();

        if (!saved) return null;

        return await GetExpenseByIdAsync(expense.Id);
    }

    public async Task<ExpenseDto?> UpdateExpenseAsync(UpdateExpenseDto updateDto)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(updateDto.Id);
        if (expense == null) return null;

        // Döviz kuru yeniden hesapla
        decimal exchangeRate = 1m;
        if (updateDto.Currency != Currency.TRY)
        {
            exchangeRate = updateDto.ExchangeRate ?? expense.ExchangeRate ?? await GetCurrentExchangeRateAsync(updateDto.Currency);
        }

        // Güncellenebilir alanlar
        expense.ExpenseDate = updateDto.ExpenseDate;
        expense.ExpenseType = updateDto.ExpenseType;
        expense.Amount = updateDto.Amount;
        expense.Currency = updateDto.Currency;
        expense.ExchangeRate = updateDto.Currency == Currency.TRY ? null : exchangeRate;
        expense.AmountInTRY = updateDto.Amount * exchangeRate;
        expense.Description = updateDto.Description;
        expense.DocumentNumber = updateDto.DocumentNumber;
        expense.IsPaid = updateDto.IsPaid;
        expense.PaymentDate = updateDto.PaymentDate;
        expense.PaymentMethod = updateDto.PaymentMethod;

        _unitOfWork.Expenses.Update(expense);
        var saved = await _unitOfWork.SaveChangesAsync();

        if (!saved) return null;

        return await GetExpenseByIdAsync(expense.Id);
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null) return false;

        _unitOfWork.Expenses.Delete(expense);
        return await _unitOfWork.SaveChangesAsync();
    }


    // ====== BUSINESS LOGIC METHODS ======

    public async Task<IEnumerable<ExpenseSummaryDto>> GetExpensesByStoreAsync(int storeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var storeExpenses = expenses.Where(e => e.StoreId == storeId);

        if (startDate.HasValue)
            storeExpenses = storeExpenses.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            storeExpenses = storeExpenses.Where(e => e.ExpenseDate <= endDate.Value);

        return _mapper.Map<IEnumerable<ExpenseSummaryDto>>(storeExpenses);
    }

    public async Task<IEnumerable<ExpenseSummaryDto>> GetExpensesByTypeAsync(ExpenseType expenseType, DateTime? startDate = null, DateTime? endDate = null)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var typeExpenses = expenses.Where(e => e.ExpenseType == expenseType);

        if (startDate.HasValue)
            typeExpenses = typeExpenses.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            typeExpenses = typeExpenses.Where(e => e.ExpenseDate <= endDate.Value);

        return _mapper.Map<IEnumerable<ExpenseSummaryDto>>(typeExpenses);
    }

    public async Task<IEnumerable<ExpenseDto>> GetEmployeeExpensesAsync(int employeeId)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var employeeExpenses = expenses.Where(e => e.EmployeeId == employeeId);

        return _mapper.Map<IEnumerable<ExpenseDto>>(employeeExpenses);
    }

    public async Task<IEnumerable<ExpenseSummaryDto>> GetPendingExpensesAsync(int? storeId = null)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var pending = expenses.Where(e => !e.IsPaid);

        if (storeId.HasValue)
            pending = pending.Where(e => e.StoreId == storeId.Value);

        return _mapper.Map<IEnumerable<ExpenseSummaryDto>>(pending);
    }

    public async Task<IEnumerable<ExpenseSummaryDto>> GetOverdueExpensesAsync(int? storeId = null)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var overdue = expenses.Where(e =>
            !e.IsPaid &&
            e.PaymentDate.HasValue &&
            e.PaymentDate.Value < DateTime.Now);

        if (storeId.HasValue)
            overdue = overdue.Where(e => e.StoreId == storeId.Value);

        return _mapper.Map<IEnumerable<ExpenseSummaryDto>>(overdue);
    }

    public async Task<bool> MarkAsPaidAsync(int expenseId, PaymentType paymentMethod)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
        if (expense == null) return false;

        expense.IsPaid = true;
        expense.PaymentDate = DateTime.Now;
        expense.PaymentMethod = paymentMethod;

        _unitOfWork.Expenses.Update(expense);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<decimal> GetCurrentExchangeRateAsync(Currency currency)
    {
        // TODO: Gerçek API entegrasyonu yapılabilir (TCMB, Fixer.io vb.)
        // Şimdilik sabit değerler kullanıyoruz
        return currency switch
        {
            Currency.USD => 32.50m, // Örnek kur
            Currency.EUR => 35.20m, // Örnek kur
            _ => 1m
        };
    }

    public async Task<object> GetMonthlyExpenseReportAsync(int year, int month, int? storeId = null)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var monthlyExpenses = expenses.Where(e =>
            e.ExpenseDate.Year == year &&
            e.ExpenseDate.Month == month);

        if (storeId.HasValue)
            monthlyExpenses = monthlyExpenses.Where(e => e.StoreId == storeId.Value);

        var expensesList = monthlyExpenses.ToList();

        return new
        {
            Year = year,
            Month = month,
            TotalAmountTRY = expensesList.Sum(e => e.AmountInTRY),
            ExpenseCount = expensesList.Count,
            ByType = expensesList
                .GroupBy(e => e.ExpenseType)
                .Select(g => new { Type = g.Key.ToString(), Total = g.Sum(e => e.AmountInTRY) }),
            PaidCount = expensesList.Count(e => e.IsPaid),
            PendingCount = expensesList.Count(e => !e.IsPaid)
        };
    }

    public async Task<object> GetYearlyExpenseReportAsync(int year, int? storeId = null)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var yearlyExpenses = expenses.Where(e => e.ExpenseDate.Year == year);

        if (storeId.HasValue)
            yearlyExpenses = yearlyExpenses.Where(e => e.StoreId == storeId.Value);

        var expensesList = yearlyExpenses.ToList();

        return new
        {
            Year = year,
            TotalAmountTRY = expensesList.Sum(e => e.AmountInTRY),
            ExpenseCount = expensesList.Count,
            MonthlyBreakdown = expensesList
                .GroupBy(e => e.ExpenseDate.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(e => e.AmountInTRY) })
                .OrderBy(x => x.Month),
            ByType = expensesList
                .GroupBy(e => e.ExpenseType)
                .Select(g => new { Type = g.Key.ToString(), Total = g.Sum(e => e.AmountInTRY) })
        };
    }

    public async Task<decimal> GetTotalExpenseAsync(int storeId, DateTime startDate, DateTime endDate)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        return expenses
            .Where(e => e.StoreId == storeId &&
                       e.ExpenseDate >= startDate &&
                       e.ExpenseDate <= endDate)
            .Sum(e => e.AmountInTRY);
    }


    // ====== PRIVATE HELPER METHODS ======

    private async Task<string> GenerateExpenseNumberAsync()
    {
        var year = DateTime.Now.Year;
        var expenses = await _unitOfWork.Expenses.GetAllAsync();
        var yearExpenses = expenses.Where(e => e.ExpenseNumber.Contains($"G-{year}-"));

        var maxNumber = 0;
        foreach (var expense in yearExpenses)
        {
            var parts = expense.ExpenseNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int num))
            {
                if (num > maxNumber) maxNumber = num;
            }
        }

        return $"G-{year}-{(maxNumber + 1):D5}";
    }
}

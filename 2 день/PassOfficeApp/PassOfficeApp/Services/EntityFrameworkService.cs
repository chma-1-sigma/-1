// Services/EntityFrameworkService.cs
using Microsoft.EntityFrameworkCore;
using PassOfficeApp.Data;
using PassOfficeApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PassOfficeApp.Services
{
    public class EntityFrameworkService
    {
        private readonly AppDbContext _context;

        public EntityFrameworkService(AppDbContext context)
        {
            _context = context;
        }

        private string GetMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // Регистрация через ORM
        public async Task<bool> RegisterUserORM(string email, string password)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
            if (existingUser != null)
                return false;

            var user = new User
            {
                email = email,
                password_hash = GetMD5Hash(password),
                created_at = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Авторизация через ORM
        public async Task<User> LoginUserORM(string email, string password)
        {
            string hash = GetMD5Hash(password);
            return await _context.Users
                .FirstOrDefaultAsync(u => u.email == email && u.password_hash == hash);
        }

        // Получение заявок через ORM
        public async Task<List<RequestViewModel>> GetUserRequestsORM(int userId)
        {
            var personalRequests = await _context.PersonalRequests
                .Where(r => r.user_id == userId)
                .Include(r => r.Department)
                .Include(r => r.Status)
                .Select(r => new RequestViewModel
                {
                    Id = r.id,
                    RequestType = "Личная",
                    DepartmentName = r.Department.name,
                    StartDate = r.start_date,
                    EndDate = r.end_date,
                    Status = r.Status.status_name,
                    RejectionReason = r.rejection_reason,
                    CreatedAt = r.created_at
                })
                .ToListAsync();

            return personalRequests.OrderByDescending(r => r.CreatedAt).ToList();
        }

        // Создание личной заявки через ORM
        public async Task<int> CreatePersonalRequestORM(PersonalRequest request)
        {
            _context.PersonalRequests.Add(request);
            await _context.SaveChangesAsync();
            return request.id;
        }

        // Получение подразделений
        public async Task<List<Department>> GetDepartments()
        {
            return await _context.Departments.OrderBy(d => d.name).ToListAsync();
        }

        // Получение сотрудников по подразделению
        public async Task<List<Employee>> GetEmployeesByDepartment(int departmentId)
        {
            return await _context.Employees
                .Where(e => e.department_id == departmentId)
                .OrderBy(e => e.full_name)
                .ToListAsync();
        }

        // Получение целей посещения
        public async Task<List<VisitPurpose>> GetVisitPurposes()
        {
            return await _context.VisitPurposes.ToListAsync();
        }

        // Получение статуса "проверка"
        public async Task<int> GetPendingStatusId()
        {
            var status = await _context.RequestStatuses
                .FirstOrDefaultAsync(s => s.status_name == "проверка");
            return status?.id ?? 1;
        }
    }
}
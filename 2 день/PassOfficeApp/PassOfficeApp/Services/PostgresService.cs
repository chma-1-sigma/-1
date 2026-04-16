using Npgsql;
using PassOfficeApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PassOfficeApp.Services
{
    public class PostgresService
    {
        private readonly string _connectionString;

        public PostgresService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Проверка подключения
        public async Task<bool> TestConnection()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection error: {ex.Message}");
                return false;
            }
        }

        // MD5 хеширование
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

        // Регистрация через прямой SQL
        public async Task<bool> RegisterUserSQL(string email, string password)
        {
            string passwordHash = GetMD5Hash(password);
            string query = "INSERT INTO kp.users (email, password_hash) VALUES (@email, @hash)";

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@hash", passwordHash);
                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
                return false;
            }
        }

        // Регистрация через хранимую процедуру
        public async Task<bool> RegisterUserSP(string email, string password)
        {
            string passwordHash = GetMD5Hash(password);
            // Используем функцию PostgreSQL
            string query = "SELECT kp.sp_register_user(@email, @hash)";

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@hash", passwordHash);
                        var result = await cmd.ExecuteScalarAsync();
                        int userId = result != null ? Convert.ToInt32(result) : -1;
                        return userId > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register SP error: {ex.Message}");
                // Если хранимая процедура не создана, используем обычный SQL
                return await RegisterUserSQL(email, password);
            }
        }

        // Авторизация через прямой SQL
        public async Task<User> LoginUserSQL(string email, string password)
        {
            string passwordHash = GetMD5Hash(password);
            string query = "SELECT id, email, created_at FROM kp.users WHERE email = @email AND password_hash = @hash";

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@hash", passwordHash);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    id = reader.GetInt32(0),
                                    email = reader.GetString(1),
                                    created_at = reader.GetDateTime(2)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
            return null;
        }

        // Авторизация через хранимую процедуру
        public async Task<User> LoginUserSP(string email, string password)
        {
            string passwordHash = GetMD5Hash(password);
            // Используем функцию PostgreSQL
            string query = "SELECT * FROM kp.sp_login_user(@email, @hash)";

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@hash", passwordHash);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    id = reader.GetInt32(0),
                                    email = reader.GetString(1),
                                    created_at = reader.GetDateTime(2)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login SP error: {ex.Message}");
                // Если хранимая процедура не создана, используем обычный SQL
                return await LoginUserSQL(email, password);
            }
            return null;
        }

        // Получение заявок пользователя
        public async Task<List<RequestViewModel>> GetUserRequestsSQL(int userId)
        {
            var requests = new List<RequestViewModel>();
            string query = @"
                SELECT id, 'Личная' as RequestType, 
                       (SELECT name FROM kp.departments WHERE id = department_id) as DepartmentName,
                       start_date, end_date,
                       (SELECT status_name FROM kp.request_statuses WHERE id = status_id) as Status,
                       rejection_reason, created_at
                FROM kp.personal_requests
                WHERE user_id = @userId
                ORDER BY created_at DESC";

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                requests.Add(new RequestViewModel
                                {
                                    Id = reader.GetInt32(0),
                                    RequestType = reader.GetString(1),
                                    DepartmentName = reader.GetString(2),
                                    StartDate = reader.GetDateTime(3),
                                    EndDate = reader.GetDateTime(4),
                                    Status = reader.GetString(5),
                                    RejectionReason = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    CreatedAt = reader.GetDateTime(7)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get requests error: {ex.Message}");
            }
            return requests;
        }

        // Создание личной заявки
        public async Task<int> CreatePersonalRequestSQL(PersonalRequest request)
        {
            string query = @"
                INSERT INTO kp.personal_requests 
                (user_id, status_id, purpose_id, department_id, employee_id,
                 start_date, end_date, visit_comment, last_name, first_name, middle_name,
                 phone, email_visitor, organization, birth_date, passport_series,
                 passport_number, photo_path, passport_scan_path, created_at)
                VALUES 
                (@userId, @statusId, @purposeId, @deptId, @empId,
                 @startDate, @endDate, @comment, @lastName, @firstName, @middleName,
                 @phone, @email, @org, @birthDate, @passSeries,
                 @passNumber, @photoPath, @scanPath, @createdAt)
                RETURNING id;";

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", request.user_id);
                        cmd.Parameters.AddWithValue("@statusId", request.status_id);
                        cmd.Parameters.AddWithValue("@purposeId", request.purpose_id);
                        cmd.Parameters.AddWithValue("@deptId", request.department_id);
                        cmd.Parameters.AddWithValue("@empId", request.employee_id);
                        cmd.Parameters.AddWithValue("@startDate", request.start_date);
                        cmd.Parameters.AddWithValue("@endDate", request.end_date);
                        cmd.Parameters.AddWithValue("@comment", request.visit_comment ?? "");
                        cmd.Parameters.AddWithValue("@lastName", request.last_name);
                        cmd.Parameters.AddWithValue("@firstName", request.first_name);
                        cmd.Parameters.AddWithValue("@middleName", request.middle_name ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@phone", request.phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", request.email_visitor);
                        cmd.Parameters.AddWithValue("@org", request.organization ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@birthDate", request.birth_date);
                        cmd.Parameters.AddWithValue("@passSeries", request.passport_series);
                        cmd.Parameters.AddWithValue("@passNumber", request.passport_number);
                        cmd.Parameters.AddWithValue("@photoPath", request.photo_path ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@scanPath", request.passport_scan_path);
                        cmd.Parameters.AddWithValue("@createdAt", request.created_at);

                        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create request error: {ex.Message}");
                throw;
            }
        }

        // Получение списка подразделений
        public async Task<List<Department>> GetDepartments()
        {
            var departments = new List<Department>();
            string query = "SELECT id, name FROM kp.departments ORDER BY name";

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                departments.Add(new Department
                                {
                                    id = reader.GetInt32(0),
                                    name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get departments error: {ex.Message}");
            }
            return departments;
        }

        // Получение сотрудников по подразделению
        public async Task<List<Employee>> GetEmployeesByDepartment(int departmentId)
        {
            var employees = new List<Employee>();
            string query = "SELECT id, full_name FROM kp.employees WHERE department_id = @deptId ORDER BY full_name";

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@deptId", departmentId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                employees.Add(new Employee
                                {
                                    id = reader.GetInt32(0),
                                    full_name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get employees error: {ex.Message}");
            }
            return employees;
        }

        // Получение целей посещения
        public async Task<List<VisitPurpose>> GetVisitPurposes()
        {
            var purposes = new List<VisitPurpose>();
            string query = "SELECT id, purpose_name FROM kp.visit_purposes";

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                purposes.Add(new VisitPurpose
                                {
                                    id = reader.GetInt32(0),
                                    purpose_name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get purposes error: {ex.Message}");
            }
            return purposes;
        }

        // Получение статуса "проверка"
        public async Task<int> GetPendingStatusId()
        {
            string query = "SELECT id FROM kp.request_statuses WHERE status_name = 'проверка'";
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : 1;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get status error: {ex.Message}");
                return 1;
            }
        }
    }
}
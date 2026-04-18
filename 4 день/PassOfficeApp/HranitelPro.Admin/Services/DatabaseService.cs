using Npgsql;
using HranitelPro.Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HranitelPro.Admin.Services
{
    public class DatabaseService
    {
        public string ConnectionString { get; private set; }

        public DatabaseService(string connectionString)
        {
            ConnectionString = connectionString;
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

        // Регистрация пользователя с логином
        public async Task<(bool success, string message, string login)> RegisterUserWithLogin(string email, string password)
        {
            string passwordHash = GetMD5Hash(password);
            string query = "INSERT INTO kp.users (email, password_hash) VALUES (@email, @hash) RETURNING id";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@hash", passwordHash);
                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        int userId = Convert.ToInt32(result);

                        // Получаем сгенерированный логин
                        var loginCmd = new NpgsqlCommand("SELECT login FROM kp.users WHERE id = @id", conn);
                        loginCmd.Parameters.AddWithValue("@id", userId);
                        string login = (await loginCmd.ExecuteScalarAsync())?.ToString();

                        return (true, "Регистрация успешна", login);
                    }
                }
            }
            return (false, "Ошибка регистрации", null);
        }

        // Авторизация сотрудника
        public async Task<(int id, string fullName, string role, string department)> AuthEmployee(string authCode)
        {
            try
            {
                // Простой запрос без функций
                string query = @"
            SELECT e.id, e.full_name, ea.role, d.name 
            FROM kp.employee_auth ea
            INNER JOIN kp.employees e ON e.id = ea.employee_id
            INNER JOIN kp.departments d ON d.id = e.department_id
            WHERE ea.auth_code = @code";

                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", authCode);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int id = reader.GetInt32(0);
                                string fullName = reader.GetString(1);
                                string role = reader.GetString(2);
                                string department = reader.GetString(3);

                                System.Diagnostics.Debug.WriteLine($"Auth success: {id}, {fullName}, {role}, {department}");

                                return (id, fullName, role, department);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Auth failed: code {authCode} not found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuthEmployee error: {ex.Message}");
                throw;
            }
            return (0, null, null, null);
        }

        // Получение всех заявок
        public async Task<List<RequestViewModel>> GetAllRequests(string requestType = null, int? departmentId = null, string status = null, string searchText = null)
        {
            var requests = new List<RequestViewModel>();

            string query = @"
                SELECT request_id, request_type, user_id, user_email, department_id, department_name,
                       employee_name, start_date, end_date, visit_comment,
                       last_name, first_name, middle_name, phone, visitor_email, organization,
                       birth_date, passport_series, passport_number, status, rejection_reason,
                       created_at, full_name, group_leader_name, members_count, purpose,
                       visit_start_time, visit_end_time
                FROM kp.view_list_requests
                WHERE (@requestType IS NULL OR request_type = @requestType)
                  AND (@departmentId IS NULL OR department_id = @departmentId)
                  AND (@status IS NULL OR status = @status)
                  AND (@searchText IS NULL OR 
                       last_name ILIKE @searchPattern OR 
                       first_name ILIKE @searchPattern OR 
                       passport_number ILIKE @searchPattern)
                ORDER BY created_at DESC";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@requestType", requestType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@departmentId", departmentId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", status ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@searchText", searchText ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@searchPattern", searchText != null ? $"%{searchText}%" : (object)DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var req = new RequestViewModel
                            {
                                request_id = reader.GetInt32(0),
                                request_type = reader.GetString(1),
                                user_id = reader.GetInt32(2),
                                user_email = reader.GetString(3),
                                department_id = reader.GetInt32(4),
                                department_name = reader.GetString(5),
                                employee_name = reader.GetString(6),
                                start_date = reader.GetDateTime(7),
                                end_date = reader.GetDateTime(8),
                                visit_comment = reader.IsDBNull(9) ? null : reader.GetString(9),
                                last_name = reader.GetString(10),
                                first_name = reader.GetString(11),
                                middle_name = reader.IsDBNull(12) ? null : reader.GetString(12),
                                phone = reader.IsDBNull(13) ? null : reader.GetString(13),
                                visitor_email = reader.GetString(14),
                                organization = reader.IsDBNull(15) ? null : reader.GetString(15),
                                birth_date = reader.IsDBNull(16) ? (DateTime?)null : reader.GetDateTime(16),
                                passport_series = reader.GetString(17),
                                passport_number = reader.GetString(18),
                                status = reader.GetString(19),
                                rejection_reason = reader.IsDBNull(20) ? null : reader.GetString(20),
                                created_at = reader.GetDateTime(21),
                                full_name = reader.GetString(22),
                                group_leader_name = reader.IsDBNull(23) ? null : reader.GetString(23),
                                members_count = reader.IsDBNull(24) ? (int?)null : (int?)reader.GetInt64(24),
                                purpose = reader.IsDBNull(25) ? null : reader.GetString(25),
                                visit_start_time = reader.IsDBNull(26) ? (DateTime?)null : reader.GetDateTime(26),
                                visit_end_time = reader.IsDBNull(27) ? (DateTime?)null : reader.GetDateTime(27)
                            };

                            // Для групповых заявок загружаем членов группы
                            if (req.request_type == "Групповая")
                            {
                                await LoadGroupMembers(req.request_id, req);
                            }

                            requests.Add(req);
                        }
                    }
                }
            }
            return requests;
        }

        // Загрузка членов группы
        private async Task LoadGroupMembers(int requestId, RequestViewModel request)
        {
            string query = @"
                SELECT gm.id, gm.row_number, gm.full_name_initials, gm.contact_info,
                       SUBSTRING(gm.full_name_initials, 1, CHARINDEX(' ', gm.full_name_initials) - 1) as last_name,
                       SUBSTRING(gm.full_name_initials, CHARINDEX(' ', gm.full_name_initials) + 1, 
                                 CHARINDEX(' ', gm.full_name_initials + ' ', CHARINDEX(' ', gm.full_name_initials) + 1) - CHARINDEX(' ', gm.full_name_initials) - 1) as first_name
                FROM kp.group_members gm
                WHERE gm.group_request_id = @requestId
                ORDER BY gm.row_number";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var member = new GroupMember
                            {
                                id = reader.GetInt32(0),
                                row_number = reader.GetInt32(1),
                                full_name_initials = reader.GetString(2),
                                contact_info = reader.GetString(3),
                                last_name = reader.IsDBNull(4) ? null : reader.GetString(4),
                                first_name = reader.IsDBNull(5) ? null : reader.GetString(5)
                            };
                            request.GroupMembers.Add(member);
                        }
                    }
                }
            }
        }

        // Получение одобренных заявок для подразделения
        public async Task<List<RequestViewModel>> GetApprovedRequestsForDepartment(int departmentId, DateTime? date = null)
        {
            var requests = new List<RequestViewModel>();

            string query = @"
                SELECT request_id, request_type, user_id, user_email, department_id, department_name,
                       employee_name, start_date, end_date, visit_comment,
                       last_name, first_name, middle_name, phone, visitor_email, organization,
                       birth_date, passport_series, passport_number, status, rejection_reason,
                       created_at, full_name, group_leader_name, members_count, purpose,
                       visit_start_time, visit_end_time
                FROM kp.view_list_requests
                WHERE department_id = @departmentId
                  AND status = 'одобрена'
                  AND (@date IS NULL OR start_date = @date)
                ORDER BY start_date";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@departmentId", departmentId);
                    cmd.Parameters.AddWithValue("@date", date ?? (object)DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var req = new RequestViewModel
                            {
                                request_id = reader.GetInt32(0),
                                request_type = reader.GetString(1),
                                user_id = reader.GetInt32(2),
                                user_email = reader.GetString(3),
                                department_id = reader.GetInt32(4),
                                department_name = reader.GetString(5),
                                employee_name = reader.GetString(6),
                                start_date = reader.GetDateTime(7),
                                end_date = reader.GetDateTime(8),
                                visit_comment = reader.IsDBNull(9) ? null : reader.GetString(9),
                                last_name = reader.GetString(10),
                                first_name = reader.GetString(11),
                                middle_name = reader.IsDBNull(12) ? null : reader.GetString(12),
                                phone = reader.IsDBNull(13) ? null : reader.GetString(13),
                                visitor_email = reader.GetString(14),
                                organization = reader.IsDBNull(15) ? null : reader.GetString(15),
                                birth_date = reader.IsDBNull(16) ? (DateTime?)null : reader.GetDateTime(16),
                                passport_series = reader.GetString(17),
                                passport_number = reader.GetString(18),
                                status = reader.GetString(19),
                                rejection_reason = reader.IsDBNull(20) ? null : reader.GetString(20),
                                created_at = reader.GetDateTime(21),
                                full_name = reader.GetString(22),
                                group_leader_name = reader.IsDBNull(23) ? null : reader.GetString(23),
                                members_count = reader.IsDBNull(24) ? (int?)null : (int?)reader.GetInt64(24),
                                purpose = reader.IsDBNull(25) ? null : reader.GetString(25),
                                visit_start_time = reader.IsDBNull(26) ? (DateTime?)null : reader.GetDateTime(26),
                                visit_end_time = reader.IsDBNull(27) ? (DateTime?)null : reader.GetDateTime(27)
                            };

                            if (req.request_type == "Групповая")
                            {
                                await LoadGroupMembers(req.request_id, req);
                            }

                            requests.Add(req);
                        }
                    }
                }
            }
            return requests;
        }

        // Обновление статуса заявки
        public async Task<bool> UpdateRequestStatus(int requestId, string requestType, int statusId, string rejectionReason = null, DateTime? visitTime = null)
        {
            string query = "SELECT kp.update_request_status(@id, @type, @statusId, @reason, @visitTime)";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", requestId);
                    cmd.Parameters.AddWithValue("@type", requestType);
                    cmd.Parameters.AddWithValue("@statusId", statusId);
                    cmd.Parameters.AddWithValue("@reason", rejectionReason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@visitTime", visitTime ?? (object)DBNull.Value);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null && (bool)result;
                }
            }
        }

        // Установка времени прибытия (сотрудник подразделения)
        public async Task<bool> SetArrivalTime(int requestId, string requestType, int memberId = 0)
        {
            string table = requestType == "Личная" ? "kp.personal_requests" : "kp.group_requests";
            string query = $@"
                UPDATE {table}
                SET visit_arrival_time = @arrivalTime
                WHERE id = @id";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", requestId);
                    cmd.Parameters.AddWithValue("@arrivalTime", DateTime.Now);
                    int result = await cmd.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        // Установка времени убытия (сотрудник охраны)
        public async Task<bool> SetDepartureTime(int requestId, string requestType)
        {
            string table = requestType == "Личная" ? "kp.personal_requests" : "kp.group_requests";
            string query = $@"
                UPDATE {table}
                SET visit_end_time = @endTime
                WHERE id = @id";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", requestId);
                    cmd.Parameters.AddWithValue("@endTime", DateTime.Now);
                    int result = await cmd.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        // Проверка черного списка
        public async Task<(bool isInBlacklist, string reason)> CheckBlacklist(string passportSeries, string passportNumber)
        {
            string query = "SELECT is_in_blacklist, reason FROM kp.check_blacklist(@series, @number)";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@series", passportSeries);
                    cmd.Parameters.AddWithValue("@number", passportNumber);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return (reader.GetBoolean(0), reader.IsDBNull(1) ? null : reader.GetString(1));
                        }
                    }
                }
            }
            return (false, null);
        }

        // Добавление в черный список
        public async Task<int> AddToBlacklist(string passportSeries, string passportNumber, string lastName, string firstName, string middleName, string reason, int addedBy)
        {
            string query = "SELECT kp.add_to_blacklist(@series, @number, @lastName, @firstName, @middleName, @reason, @addedBy)";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@series", passportSeries);
                    cmd.Parameters.AddWithValue("@number", passportNumber);
                    cmd.Parameters.AddWithValue("@lastName", lastName);
                    cmd.Parameters.AddWithValue("@firstName", firstName);
                    cmd.Parameters.AddWithValue("@middleName", middleName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@reason", reason);
                    cmd.Parameters.AddWithValue("@addedBy", addedBy);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        // Получение списка подразделений
        public async Task<List<Department>> GetDepartments()
        {
            var departments = new List<Department>();
            string query = "SELECT id, name FROM kp.departments ORDER BY name";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        departments.Add(new Department { id = reader.GetInt32(0), name = reader.GetString(1) });
                    }
                }
            }
            return departments;
        }

        // Получение статусов
        public async Task<List<RequestStatus>> GetStatuses()
        {
            var statuses = new List<RequestStatus>();
            string query = "SELECT id, status_name FROM kp.request_statuses ORDER BY id";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        statuses.Add(new RequestStatus { id = reader.GetInt32(0), status_name = reader.GetString(1) });
                    }
                }
            }
            return statuses;
        }

        // Получение ID статуса по названию
        public async Task<int> GetStatusIdByName(string statusName)
        {
            string query = "SELECT id FROM kp.request_statuses WHERE status_name = @name";
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", statusName);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 1;
                }
            }
        }

        // Получение статистики посещений для отчета
        public async Task<List<ReportData>> GetVisitStatistics(DateTime startDate, DateTime endDate, int? departmentId = null)
        {
            var data = new List<ReportData>();

            string query = @"
                SELECT 
                    DATE(v.created_at) as Date,
                    v.department_name,
                    COUNT(*) as VisitCount
                FROM kp.view_list_requests v
                WHERE v.status = 'одобрена'
                  AND v.created_at BETWEEN @startDate AND @endDate
                  AND (@departmentId IS NULL OR v.department_id = @departmentId)
                GROUP BY DATE(v.created_at), v.department_name
                ORDER BY Date DESC, v.department_name";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@departmentId", departmentId ?? (object)DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new ReportData
                            {
                                Date = reader.GetDateTime(0),
                                DepartmentName = reader.GetString(1),
                                VisitCount = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }
            return data;
        }

        // Получение текущих посетителей на территории
        public async Task<List<RequestViewModel>> GetCurrentVisitors()
        {
            var requests = new List<RequestViewModel>();

            string query = @"
                SELECT request_id, request_type, user_id, user_email, department_id, department_name,
                       employee_name, start_date, end_date, visit_comment,
                       last_name, first_name, middle_name, phone, visitor_email, organization,
                       birth_date, passport_series, passport_number, status, rejection_reason,
                       created_at, full_name, group_leader_name, members_count, purpose,
                       visit_start_time, visit_end_time
                FROM kp.view_list_requests
                WHERE status = 'одобрена'
                  AND visit_start_time IS NOT NULL
                  AND visit_end_time IS NULL
                ORDER BY department_name, last_name";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var req = new RequestViewModel
                            {
                                request_id = reader.GetInt32(0),
                                request_type = reader.GetString(1),
                                user_id = reader.GetInt32(2),
                                user_email = reader.GetString(3),
                                department_id = reader.GetInt32(4),
                                department_name = reader.GetString(5),
                                employee_name = reader.GetString(6),
                                start_date = reader.GetDateTime(7),
                                end_date = reader.GetDateTime(8),
                                visit_comment = reader.IsDBNull(9) ? null : reader.GetString(9),
                                last_name = reader.GetString(10),
                                first_name = reader.GetString(11),
                                middle_name = reader.IsDBNull(12) ? null : reader.GetString(12),
                                phone = reader.IsDBNull(13) ? null : reader.GetString(13),
                                visitor_email = reader.GetString(14),
                                organization = reader.IsDBNull(15) ? null : reader.GetString(15),
                                birth_date = reader.IsDBNull(16) ? (DateTime?)null : reader.GetDateTime(16),
                                passport_series = reader.GetString(17),
                                passport_number = reader.GetString(18),
                                status = reader.GetString(19),
                                rejection_reason = reader.IsDBNull(20) ? null : reader.GetString(20),
                                created_at = reader.GetDateTime(21),
                                full_name = reader.GetString(22),
                                group_leader_name = reader.IsDBNull(23) ? null : reader.GetString(23),
                                members_count = reader.IsDBNull(24) ? (int?)null : (int?)reader.GetInt64(24),
                                purpose = reader.IsDBNull(25) ? null : reader.GetString(25),
                                visit_start_time = reader.IsDBNull(26) ? (DateTime?)null : reader.GetDateTime(26),
                                visit_end_time = reader.IsDBNull(27) ? (DateTime?)null : reader.GetDateTime(27)
                            };
                            requests.Add(req);
                        }
                    }
                }
            }
            return requests;
        }
    }
}
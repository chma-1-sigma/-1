using Npgsql;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HranitelPRO
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

        public async Task<bool> RegisterUser(string email, string password)
        {
            string passwordHash = GetMD5Hash(password);
            string query = "INSERT INTO kp.users (email, password_hash) VALUES (@email, @hash)";

            using (var conn = new NpgsqlConnection(ConnectionString))
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

        public async Task<User> LoginUser(string email, string password)
        {
            string passwordHash = GetMD5Hash(password);
            string query = "SELECT id, email, login, created_at FROM kp.users WHERE email = @email AND password_hash = @hash";

            using (var conn = new NpgsqlConnection(ConnectionString))
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
                                login = reader.IsDBNull(2) ? null : reader.GetString(2),
                                created_at = reader.GetDateTime(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        // ИСПРАВЛЕННЫЙ МЕТОД AuthEmployee
        public async Task<(int id, string fullName, string role, string department)> AuthEmployee(string authCode)
        {
            string query = "SELECT employee_id, full_name, role, department_name FROM kp.auth_employee(@code)";

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
                            return (id, fullName, role, department);
                        }
                    }
                }
            }
            return (0, null, null, null);
        }

        public async Task<int> GetEmployeeDepartmentId(int employeeId)
        {
            string query = "SELECT department_id FROM kp.employees WHERE id = @id";
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", employeeId);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<int> GetPendingStatusId()
        {
            string query = "SELECT id FROM kp.request_statuses WHERE status_name = 'проверка'";
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 1;
                }
            }
        }

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

        public async Task<List<Employee>> GetEmployeesByDepartment(int departmentId)
        {
            var employees = new List<Employee>();
            string query = "SELECT id, full_name FROM kp.employees WHERE department_id = @deptId ORDER BY full_name";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@deptId", departmentId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            employees.Add(new Employee { id = reader.GetInt32(0), full_name = reader.GetString(1) });
                        }
                    }
                }
            }
            return employees;
        }

        public async Task<List<VisitPurpose>> GetVisitPurposes()
        {
            var purposes = new List<VisitPurpose>();
            string query = "SELECT id, purpose_name FROM kp.visit_purposes";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        purposes.Add(new VisitPurpose { id = reader.GetInt32(0), purpose_name = reader.GetString(1) });
                    }
                }
            }
            return purposes;
        }

        public async Task<List<RequestViewModel>> GetAllRequests(string requestType = null, int? departmentId = null, string status = null, string searchText = null)
        {
            var requests = new List<RequestViewModel>();

            string query = @"
                SELECT request_id, request_type, user_email, department_name, employee_name,
                       start_date, end_date, visit_comment, last_name, first_name, middle_name,
                       phone, visitor_email, organization, birth_date, passport_series, passport_number,
                       status, rejection_reason, created_at, group_leader_name, members_count,
                       visit_start_time, visit_end_time
                FROM kp.filtering_requests(
                    @p_request_type,
                    @p_department_id,
                    @p_status_id,
                    @p_date_from,
                    @p_date_to,
                    @p_visitor_name
                )";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    int? statusId = null;
                    if (!string.IsNullOrEmpty(status))
                    {
                        var statusCmd = new NpgsqlCommand("SELECT id FROM kp.request_statuses WHERE status_name = @name", conn);
                        statusCmd.Parameters.AddWithValue("@name", status);
                        var result = await statusCmd.ExecuteScalarAsync();
                        statusId = result != null ? Convert.ToInt32(result) : (int?)null;
                    }

                    cmd.Parameters.AddWithValue("@p_request_type", requestType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p_department_id", departmentId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p_status_id", statusId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@p_date_from", DBNull.Value);
                    cmd.Parameters.AddWithValue("@p_date_to", DBNull.Value);
                    cmd.Parameters.AddWithValue("@p_visitor_name", searchText ?? (object)DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var req = new RequestViewModel
                            {
                                request_id = reader.GetInt32(0),
                                request_type = reader.GetString(1),
                                user_email = reader.GetString(2),
                                department_name = reader.GetString(3),
                                employee_name = reader.GetString(4),
                                start_date = reader.GetDateTime(5),
                                end_date = reader.GetDateTime(6),
                                visit_comment = reader.IsDBNull(7) ? null : reader.GetString(7),
                                last_name = reader.GetString(8),
                                first_name = reader.GetString(9),
                                middle_name = reader.IsDBNull(10) ? null : reader.GetString(10),
                                phone = reader.IsDBNull(11) ? null : reader.GetString(11),
                                visitor_email = reader.GetString(12),
                                organization = reader.IsDBNull(13) ? null : reader.GetString(13),
                                birth_date = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14),
                                passport_series = reader.GetString(15),
                                passport_number = reader.GetString(16),
                                status = reader.GetString(17),
                                rejection_reason = reader.IsDBNull(18) ? null : reader.GetString(18),
                                created_at = reader.GetDateTime(19),
                                group_leader_name = reader.IsDBNull(20) ? null : reader.GetString(20),
                                members_count = reader.IsDBNull(21) ? (int?)null : (int?)reader.GetInt64(21),
                                visit_start_time = reader.IsDBNull(22) ? (DateTime?)null : reader.GetDateTime(22),
                                visit_end_time = reader.IsDBNull(23) ? (DateTime?)null : reader.GetDateTime(23),
                                full_name = $"{reader.GetString(8)} {reader.GetString(9)}"
                            };
                            requests.Add(req);
                        }
                    }
                }
            }
            return requests;
        }

        public async Task<List<RequestViewModel>> GetUserRequests(int userId)
        {
            var requests = new List<RequestViewModel>();
            string query = @"
                SELECT id, 'Личная', 
                       (SELECT name FROM kp.departments WHERE id = department_id),
                       start_date, end_date,
                       (SELECT status_name FROM kp.request_statuses WHERE id = status_id),
                       rejection_reason, created_at, last_name, first_name, middle_name,
                       passport_series, passport_number, email_visitor, phone
                FROM kp.personal_requests
                WHERE user_id = @userId
                UNION ALL
                SELECT id, 'Групповая',
                       (SELECT name FROM kp.departments WHERE id = department_id),
                       start_date, end_date,
                       (SELECT status_name FROM kp.request_statuses WHERE id = status_id),
                       rejection_reason, created_at, group_leader_last_name, group_leader_first_name, group_leader_middle_name,
                       group_leader_passport_series, group_leader_passport_number, group_leader_email, group_leader_phone
                FROM kp.group_requests
                WHERE user_id = @userId
                ORDER BY created_at DESC";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var req = new RequestViewModel
                            {
                                request_id = reader.GetInt32(0),
                                request_type = reader.GetString(1),
                                department_name = reader.GetString(2),
                                start_date = reader.GetDateTime(3),
                                end_date = reader.GetDateTime(4),
                                status = reader.GetString(5),
                                rejection_reason = reader.IsDBNull(6) ? null : reader.GetString(6),
                                created_at = reader.GetDateTime(7),
                                last_name = reader.GetString(8),
                                first_name = reader.GetString(9),
                                middle_name = reader.IsDBNull(10) ? null : reader.GetString(10),
                                passport_series = reader.GetString(11),
                                passport_number = reader.GetString(12),
                                visitor_email = reader.GetString(13),
                                phone = reader.IsDBNull(14) ? null : reader.GetString(14),
                                full_name = $"{reader.GetString(8)} {reader.GetString(9)}"
                            };
                            requests.Add(req);
                        }
                    }
                }
            }
            return requests;
        }

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

        public async Task<bool> SetDepartureTime(int requestId, string requestType)
        {
            string query = "SELECT kp.set_departure_time(@id, @type)";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", requestId);
                    cmd.Parameters.AddWithValue("@type", requestType);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null && (bool)result;
                }
            }
        }

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

        public async Task<int> CreatePersonalRequest(PersonalRequest request)
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

            using (var conn = new NpgsqlConnection(ConnectionString))
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

        public async Task<int> CreateGroupRequest(GroupRequest request, List<VisitorItem> members)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var tran = await conn.BeginTransactionAsync())
                {
                    string query = @"
                        INSERT INTO kp.group_requests 
                        (user_id, status_id, purpose_id, department_id, employee_id,
                         start_date, end_date, visit_comment,
                         group_leader_last_name, group_leader_first_name, group_leader_middle_name,
                         group_leader_phone, group_leader_email, group_leader_organization,
                         group_leader_birth_date, group_leader_passport_series, group_leader_passport_number,
                         passport_scan_path, created_at)
                        VALUES 
                        (@userId, @statusId, @purposeId, @deptId, @empId,
                         @startDate, @endDate, @comment,
                         @lastName, @firstName, @middleName,
                         @phone, @email, @org,
                         @birthDate, @passSeries, @passNumber,
                         @scanPath, @createdAt)
                        RETURNING id;";

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
                        cmd.Parameters.AddWithValue("@lastName", request.group_leader_last_name);
                        cmd.Parameters.AddWithValue("@firstName", request.group_leader_first_name);
                        cmd.Parameters.AddWithValue("@middleName", request.group_leader_middle_name ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@phone", request.group_leader_phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", request.group_leader_email);
                        cmd.Parameters.AddWithValue("@org", request.group_leader_organization ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@birthDate", request.group_leader_birth_date);
                        cmd.Parameters.AddWithValue("@passSeries", request.group_leader_passport_series);
                        cmd.Parameters.AddWithValue("@passNumber", request.group_leader_passport_number);
                        cmd.Parameters.AddWithValue("@scanPath", request.passport_scan_path);
                        cmd.Parameters.AddWithValue("@createdAt", request.created_at);

                        int groupId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                        int rowNum = 1;
                        foreach (var member in members)
                        {
                            var memberCmd = new NpgsqlCommand(@"
                                INSERT INTO kp.group_members 
                                (group_request_id, row_number, full_name_initials, contact_info)
                                VALUES (@groupId, @rowNum, @fullName, @contactInfo)", conn);
                            memberCmd.Parameters.AddWithValue("@groupId", groupId);
                            memberCmd.Parameters.AddWithValue("@rowNum", rowNum++);
                            memberCmd.Parameters.AddWithValue("@fullName", member.FullName);
                            memberCmd.Parameters.AddWithValue("@contactInfo", $"тел. {member.Phone}, email: {member.Email}");
                            await memberCmd.ExecuteNonQueryAsync();
                        }

                        await tran.CommitAsync();
                        return groupId;
                    }
                }
            }
        }

        public async Task<List<VisitorDetail>> GetGroupMembers(int groupRequestId)
        {
            var members = new List<VisitorDetail>();
            string query = "SELECT id, row_number, full_name_initials, contact_info FROM kp.group_members WHERE group_request_id = @groupId ORDER BY row_number";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@groupId", groupRequestId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int idx = 1;
                        while (await reader.ReadAsync())
                        {
                            members.Add(new VisitorDetail
                            {
                                Id = idx++,
                                FullName = reader.GetString(2),
                                PassportSeries = "",
                                PassportNumber = ""
                            });
                        }
                    }
                }
            }
            return members;
        }

    }
}
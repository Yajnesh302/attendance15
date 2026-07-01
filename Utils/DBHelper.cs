using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace AttendanceApp.Utils
{
    public static class DBHelper
    {
        public static string GetCompanyDBConnection()
        {
            return ConfigurationManager.ConnectionStrings["CompanyDB"].ConnectionString;
        }

        public static string GetAttendanceDBConnection()
        {
            return ConfigurationManager.ConnectionStrings["AttendanceDB"].ConnectionString;
        }

        [ThreadStatic]
        private static bool _inAutoClose;
        private static DateTime _lastAutoCloseCheck = DateTime.MinValue;
        private static readonly object _syncLock = new object();

        public static void AutoCloseExpiredContracts()
        {
            if (_inAutoClose) return;

            // Throttle checks to run at most once every 5 seconds per app domain
            if ((DateTime.UtcNow - _lastAutoCloseCheck).TotalSeconds < 5) return;

            lock (_syncLock)
            {
                if ((DateTime.UtcNow - _lastAutoCloseCheck).TotalSeconds < 5) return;
                _lastAutoCloseCheck = DateTime.UtcNow;
            }

            _inAutoClose = true;
            try
            {
                string connStr = GetAttendanceDBConnection();
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // Find all active contract periods whose EndDate has passed
                    List<Tuple<int, DateTime>> expiredPeriods = new List<Tuple<int, DateTime>>();
                    string selectExpiredSql = "SELECT Id, EndDate FROM ContractPeriods WHERE Status = 'Active' AND EndDate < TRUNC(SYSDATE)";
                    using (OracleCommand cmd = new OracleCommand(selectExpiredSql, conn))
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = Convert.ToInt32(reader["Id"]);
                                DateTime endDate = Convert.ToDateTime(reader["EndDate"]);
                                expiredPeriods.Add(Tuple.Create(id, endDate));
                            }
                        }
                    }

                    if (expiredPeriods.Count > 0)
                    {
                        foreach (var period in expiredPeriods)
                        {
                            int periodId = period.Item1;
                            DateTime endDate = period.Item2;

                            using (OracleTransaction trans = conn.BeginTransaction())
                            {
                                try
                                {
                                    // a. Close ContractPeriod
                                    string closeCPSql = "UPDATE ContractPeriods SET Status = 'Closed' WHERE Id = :Id";
                                    using (OracleCommand cmd = new OracleCommand(closeCPSql, conn))
                                    {
                                        cmd.Transaction = trans;
                                        cmd.Parameters.Add(new OracleParameter("Id", periodId));
                                        cmd.ExecuteNonQuery();
                                    }

                                    // b. Find active employee engagements under this period
                                    List<Tuple<int, string>> activeEngs = new List<Tuple<int, string>>();
                                    string activeEngsSql = "SELECT Id, EmpID FROM EmployeeEngagements WHERE ContractPeriodId = :PeriodId AND EndDate IS NULL";
                                    using (OracleCommand cmd = new OracleCommand(activeEngsSql, conn))
                                    {
                                        cmd.Transaction = trans;
                                        cmd.Parameters.Add(new OracleParameter("PeriodId", periodId));
                                        using (OracleDataReader reader = cmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                int engId = Convert.ToInt32(reader["Id"]);
                                                string empId = reader["EmpID"].ToString();
                                                activeEngs.Add(Tuple.Create(engId, empId));
                                            }
                                        }
                                    }

                                    foreach (var eng in activeEngs)
                                    {
                                        int engId = eng.Item1;
                                        string empId = eng.Item2;

                                        // Close engagement
                                        string closeEngSql = "UPDATE EmployeeEngagements SET EndDate = :EndDate, EndReason = 'ContractEnd' WHERE Id = :Id";
                                        using (OracleCommand cmd = new OracleCommand(closeEngSql, conn))
                                        {
                                            cmd.Transaction = trans;
                                            cmd.Parameters.Add(new OracleParameter("EndDate", endDate));
                                            cmd.Parameters.Add(new OracleParameter("Id", engId));
                                            cmd.ExecuteNonQuery();
                                        }

                                        // Update Employee Master
                                        string updateEmpSql = "UPDATE Employees SET CurrentEngagementId = NULL, ContractEndDate = :ContractEndDate, Status = 'ContractEnded' WHERE MasterId = :MasterId AND CurrentEngagementId = :Id";
                                        using (OracleCommand cmd = new OracleCommand(updateEmpSql, conn))
                                        {
                                            cmd.Transaction = trans;
                                            cmd.Parameters.Add(new OracleParameter("ContractEndDate", endDate));
                                            cmd.Parameters.Add(new OracleParameter("MasterId", empId));
                                            cmd.Parameters.Add(new OracleParameter("Id", engId));
                                            cmd.ExecuteNonQuery();
                                        }
                                    }

                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    trans.Rollback();
                                    System.Diagnostics.Debug.WriteLine("Error auto-closing contract period: " + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in AutoCloseExpiredContracts: " + ex.Message);
            }
            finally
            {
                _inAutoClose = false;
            }
        }

        private static T RunWithRetry<T>(Func<T> operation, int maxRetries = 3, int delayMs = 500)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    attempts++;
                    return operation();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Database operation failed. Attempt {0} of {1}. Error: {2}", attempts, maxRetries, ex.Message));
                    if (attempts >= maxRetries)
                    {
                        throw;
                    }
                    // Wait before retrying (exponential backoff)
                    System.Threading.Thread.Sleep(delayMs * attempts);
                }
            }
        }

        private static OracleParameter[] CloneParameters(OracleParameter[] parameters)
        {
            if (parameters == null) return null;
            OracleParameter[] cloned = new OracleParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                cloned[i] = new OracleParameter(parameters[i].ParameterName, parameters[i].Value)
                {
                    DbType = parameters[i].DbType,
                    Direction = parameters[i].Direction,
                    IsNullable = parameters[i].IsNullable,
                    Size = parameters[i].Size,
                    SourceColumn = parameters[i].SourceColumn,
                    SourceVersion = parameters[i].SourceVersion
                };
            }
            return cloned;
        }

        public static DataTable ExecuteQuery(string connectionString, string query, params OracleParameter[] parameters)
        {
            if (connectionString == GetAttendanceDBConnection())
            {
                AutoCloseExpiredContracts();
            }
            return RunWithRetry(() =>
            {
                DataTable dt = new DataTable();
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.BindByName = true;
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(CloneParameters(parameters));
                        }
                        using (OracleDataAdapter sda = new OracleDataAdapter(cmd))
                        {
                            sda.Fill(dt);
                        }
                    }
                }
                return dt;
            });
        }

        public static int ExecuteNonQuery(string connectionString, string query, params OracleParameter[] parameters)
        {
            if (connectionString == GetAttendanceDBConnection())
            {
                AutoCloseExpiredContracts();
            }
            return RunWithRetry(() =>
            {
                int rowsAffected = 0;
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.BindByName = true;
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(CloneParameters(parameters));
                        }
                        conn.Open();
                        rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                return rowsAffected;
            });
        }

        public static object ExecuteScalar(string connectionString, string query, params OracleParameter[] parameters)
        {
            if (connectionString == GetAttendanceDBConnection())
            {
                AutoCloseExpiredContracts();
            }
            return RunWithRetry(() =>
            {
                object result = null;
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.BindByName = true;
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(CloneParameters(parameters));
                        }
                        conn.Open();
                        result = cmd.ExecuteScalar();
                    }
                }
                return result;
            });
        }
    }
}

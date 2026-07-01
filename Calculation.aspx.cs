using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.Services;
using AttendanceApp.Utils;
using Oracle.ManagedDataAccess.Client;

namespace AttendanceApp
{
    public partial class Calculation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated || Session["PCNO"] == null)
            {
                System.Web.Security.FormsAuthentication.SignOut();
                Response.Redirect("Login.aspx");
                return;
            }
            
            int role = Convert.ToInt32(Session["Role"] ?? 0);
            if (role != 1)
            {
                Response.Write("<!DOCTYPE html><html><head><title>Access Denied</title><link href='Static/fontawesome-free/css/all.min.css' rel='stylesheet' type='text/css' /><link href='Static/css/sb-admin-2.min.css' rel='stylesheet' /><style>body { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); height: 100vh; display: flex; align-items: center; justify-content: center; font-family: 'Nunito', sans-serif; color: #f1f5f9; margin: 0; } .error-card { background: rgba(30, 41, 59, 0.7); backdrop-filter: blur(10px); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 16px; padding: 40px; text-align: center; max-width: 450px; width: 90%; box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.3), 0 8px 10px -6px rgba(0, 0, 0, 0.3); animation: fadeIn 0.5s ease-out; } @keyframes fadeIn { from { opacity: 0; transform: translateY(-20px); } to { opacity: 1; transform: translateY(0); } } .error-icon { font-size: 64px; color: #f43f5e; margin-bottom: 20px; animation: pulse 2s infinite; } @keyframes pulse { 0% { transform: scale(1); } 50% { transform: scale(1.05); } 100% { transform: scale(1); } } h2 { font-size: 24px; margin-bottom: 10px; font-weight: 700; } p { color: #94a3b8; font-size: 16px; margin-bottom: 30px; line-height: 1.5; } .btn-action { display: inline-block; background: linear-gradient(135deg, #4f46e5 0%, #3b82f6 100%); color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: 600; transition: all 0.3s ease; box-shadow: 0 4px 6px -1px rgba(79, 70, 229, 0.2); margin: 5px; } .btn-action:hover { transform: translateY(-2px); box-shadow: 0 10px 15px -3px rgba(79, 70, 229, 0.4); color: white; } .btn-secondary-action { display: inline-block; background: rgba(255, 255, 255, 0.1); color: #e2e8f0; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: 600; transition: all 0.3s ease; margin: 5px; } .btn-secondary-action:hover { background: rgba(255, 255, 255, 0.2); color: white; }</style></head><body><div class='error-card'><div class='error-icon'><i class='fas fa-exclamation-triangle'></i></div><h2>Access Denied</h2><p>This page is restricted. Only administrators are allowed to access this resource.</p><div><a href='Login.aspx' class='btn-action'>Login as Admin</a><a href='Dashboard.aspx' class='btn-secondary-action'>Go to Dashboard</a></div></div></body></html>");
                Response.End();
                return;
            }
        }

        [WebMethod]
        public static string GetCalculationData(int year, int month, string category, string division, float wage, int? contractPeriodId = null, string search = "")
        {
            DateTime firstDay = new DateTime(year, month + 1, 1);
            DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);

            // If category is not "All" and contractPeriodId is not specified, try to resolve to the first available contract
            if ((!contractPeriodId.HasValue || contractPeriodId.Value <= 0) && category != "All")
            {
                string cpQuery = @"
                    SELECT Id FROM ContractPeriods 
                    WHERE Category = :Category 
                      AND StartDate <= :LastDay 
                      AND (EndDate IS NULL OR EndDate >= :FirstDay)
                    ORDER BY Status ASC, StartDate DESC";
                DataTable dtCp = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), cpQuery,
                    new OracleParameter("Category", category),
                    new OracleParameter("LastDay", lastDay),
                    new OracleParameter("FirstDay", firstDay));
                if (dtCp.Rows.Count > 0)
                {
                    contractPeriodId = Convert.ToInt32(dtCp.Rows[0]["Id"]);
                }
            }

            // 1. Get all category wages for the given year & month
            Dictionary<string, float> wageDict = new Dictionary<string, float>();
            string wQ = "SELECT Category, WageRate FROM CalculationWages WHERE Year=:Y AND Month=:M";
            DataTable dtWages = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), wQ,
                new OracleParameter("Y", year), new OracleParameter("M", month));
            foreach (DataRow dr in dtWages.Rows)
            {
                wageDict[dr["Category"].ToString()] = Convert.ToSingle(dr["WageRate"]);
            }

            // Fallback for single category if wage is 0
            if (wage == 0 && category != "All" && wageDict.ContainsKey(category))
            {
                wage = wageDict[category];
            }

            // 2. Get Employees from EmployeeEngagements joined with Employees active in the month
            string eQ = @"
                SELECT ee.Id AS EngagementId, ee.EmpID AS MasterId, NVL(ee.EmployeeId, e.ID) AS ID, e.Name, 
                       ee.Department, ee.Category, ee.StartDate, ee.EndDate, cp.EndDate AS CpEndDate
                FROM EmployeeEngagements ee
                JOIN Employees e ON ee.EmpID = e.MasterId
                JOIN ContractPeriods cp ON ee.ContractPeriodId = cp.Id
                WHERE (ee.StartDate <= :LastDay AND (ee.EndDate IS NULL OR ee.EndDate >= :FirstDay))";
            
            List<OracleParameter> pList = new List<OracleParameter> {
                new OracleParameter("LastDay", lastDay),
                new OracleParameter("FirstDay", firstDay)
            };

            if (category != "All")
            {
                eQ += " AND ee.Category=:C";
                pList.Add(new OracleParameter("C", category));
            }
            if (division != "All")
            {
                eQ += " AND ee.Department=:Div";
                pList.Add(new OracleParameter("Div", division));
            }
            if (contractPeriodId.HasValue && contractPeriodId.Value > 0)
            {
                eQ += " AND ee.ContractPeriodId = :SelectedCpId";
                pList.Add(new OracleParameter("SelectedCpId", contractPeriodId.Value));
            }
            if (!string.IsNullOrEmpty(search))
            {
                eQ += " AND (UPPER(ee.EmpID) LIKE UPPER(:Search) OR UPPER(e.ID) LIKE UPPER(:Search) OR UPPER(ee.EmployeeId) LIKE UPPER(:Search) OR UPPER(e.Name) LIKE UPPER(:Search))";
                pList.Add(new OracleParameter("Search", "%" + search + "%"));
            }
            eQ += " ORDER BY ee.Department ASC, e.Name ASC, ee.StartDate ASC";
            DataTable dtEmp = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), eQ, pList.ToArray());

            // 3. Get Attendance Records for the Year/Month
            string aQ = "SELECT EmpID, Day, StatusValue, IsHoliday, LeaveType FROM Attendance WHERE Year=:Y AND Month=:M";
            DataTable dtAtt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), aQ,
                new OracleParameter("Y", year), new OracleParameter("M", month));
            
            Dictionary<string, Dictionary<int, AttCell>> attDict = new Dictionary<string, Dictionary<int, AttCell>>();
            foreach (DataRow dr in dtAtt.Rows)
            {
                string empId = dr["EmpID"].ToString();
                int day = Convert.ToInt32(dr["Day"]);
                float? val = dr["StatusValue"] == DBNull.Value ? (float?)null : Convert.ToSingle(dr["StatusValue"]);
                bool isHoliday = Convert.ToInt32(dr["IsHoliday"]) == 1;
                string leaveType = dr["LeaveType"] == DBNull.Value ? "" : dr["LeaveType"].ToString();

                if (!attDict.ContainsKey(empId))
                    attDict[empId] = new Dictionary<int, AttCell>();

                attDict[empId][day] = new AttCell { Val = val, IsHoliday = isHoliday, LeaveType = leaveType };
            }

            // 4. Get Overrides
            string oQ = "SELECT EmpID, Category, FinalDays, Remarks FROM CalculationOverrides WHERE Year=:Y AND Month=:M";
            List<OracleParameter> oParams = new List<OracleParameter> {
                new OracleParameter("Y", year),
                new OracleParameter("M", month)
            };
            if (category != "All")
            {
                oQ += " AND Category=:C";
                oParams.Add(new OracleParameter("C", category));
            }
            DataTable dtOver = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), oQ, oParams.ToArray());
            Dictionary<string, OverrideInfo> overDict = new Dictionary<string, OverrideInfo>(StringComparer.OrdinalIgnoreCase);
            foreach(DataRow dr in dtOver.Rows)
            {
                string overKey = dr["EmpID"].ToString() + "_" + dr["Category"].ToString();
                overDict[overKey] = new OverrideInfo {
                    FinalDays = Convert.ToSingle(dr["FinalDays"]),
                    Remarks = dr["Remarks"] == DBNull.Value ? "" : dr["Remarks"].ToString()
                };
            }

            // Get Global Adjustment
            float globalAdj = 0;
            string gq = "SELECT StatusValue FROM Attendance WHERE Year=:Y AND Month=:M AND EmpID='GLOBAL' AND Day=0 AND ROWNUM <= 1";
            object gRes = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), gq,
                new OracleParameter("Y", year), new OracleParameter("M", month));
            if (gRes != null && gRes != DBNull.Value)
            {
                globalAdj = Convert.ToSingle(gRes);
            }

            // Get Category-specific Global Adjustments
            Dictionary<string, float> catGlobalAdjDict = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string catGq = "SELECT EmpID, StatusValue FROM Attendance WHERE Year=:Y AND Month=:M AND EmpID LIKE 'GLOBAL_%' AND Day=0";
                DataTable dtCatGlob = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), catGq,
                    new OracleParameter("Y", year), new OracleParameter("M", month));
                foreach (DataRow row in dtCatGlob.Rows)
                {
                    string empId = row["EmpID"].ToString();
                    string catName = empId.Substring(7); // Extract category from 'GLOBAL_<Category>'
                    float val = row["StatusValue"] != DBNull.Value ? Convert.ToSingle(row["StatusValue"]) : 0f;
                    catGlobalAdjDict[catName] = val;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error fetching category global adjustments in Calculation: " + ex.Message);
            }

            // 5. Pre-scan dtEmp to find the latest engagement row per employee (for global adjustment mapping)
            Dictionary<string, int> latestEngMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow dr in dtEmp.Rows)
            {
                string masterId = dr["MasterId"].ToString();
                int engId = Convert.ToInt32(dr["EngagementId"]);
                // Because of ORDER BY ee.StartDate ASC, the last row processed for a MasterId is naturally the latest
                latestEngMap[masterId] = engId;
            }

            int daysInMonth = DateTime.DaysInMonth(year, month + 1);
            List<object> list = new List<object>();
            foreach(DataRow dr in dtEmp.Rows)
            {
                string masterId = dr["MasterId"].ToString();
                string id = dr["ID"].ToString();
                string empCat = dr["Category"].ToString();
                int engagementId = Convert.ToInt32(dr["EngagementId"]);
                
                DateTime eeStartDate = Convert.ToDateTime(dr["StartDate"]);
                DateTime? eeEndDate = dr["EndDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["EndDate"]) : null;
                DateTime? cpEndDate = dr["CpEndDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["CpEndDate"]) : null;
                
                DateTime? effectiveEndDate = null;
                if (eeEndDate.HasValue && cpEndDate.HasValue)
                {
                    effectiveEndDate = eeEndDate.Value < cpEndDate.Value ? eeEndDate.Value : cpEndDate.Value;
                }
                else if (eeEndDate.HasValue)
                {
                    effectiveEndDate = eeEndDate;
                }
                else if (cpEndDate.HasValue)
                {
                    effectiveEndDate = cpEndDate;
                }

                Dictionary<int, AttCell> empAtt = attDict.ContainsKey(masterId) ? attDict[masterId] : new Dictionary<int, AttCell>();

                float presentCount = 0f;
                for (int d = 1; d <= daysInMonth; d++)
                {
                    DateTime currDate = new DateTime(year, month + 1, d);

                    // Boundary check for this specific stint
                    if (currDate < eeStartDate.Date || (effectiveEndDate.HasValue && currDate > effectiveEndDate.Value.Date))
                    {
                        continue;
                    }

                    AttCell cell = empAtt.ContainsKey(d) ? empAtt[d] : new AttCell { Val = null, IsHoliday = false, LeaveType = "" };

                    // Skip non-holiday Sundays
                    if (currDate.DayOfWeek == DayOfWeek.Sunday && !cell.IsHoliday) continue;

                    if (cell.IsHoliday)
                    {
                        presentCount += 1.0f;
                    }
                    else if (cell.LeaveType == "Carried")
                    {
                        presentCount += 1.0f;
                    }
                    else if (cell.LeaveType == "Paired Paid")
                    {
                        presentCount += 1.0f;
                    }
                    else if (cell.LeaveType == "Paired Unpaid")
                    {
                        // +0.0
                    }
                    else if (cell.Val == 1.0f)
                    {
                        presentCount += 1.0f;
                    }
                    else if (cell.Val == 0.5f)
                    {
                        presentCount += 0.5f;
                    }
                    else if (cell.Val == 0.0f)
                    {
                        if (cell.LeaveType == "Paid")
                        {
                            presentCount += 1.0f;
                        }
                    }
                }

                float present = presentCount;
                if (latestEngMap.ContainsKey(masterId) && latestEngMap[masterId] == engagementId)
                {
                    present += globalAdj;
                    float catGlobalAdj = 0;
                    if (catGlobalAdjDict.ContainsKey(empCat))
                    {
                        catGlobalAdj = catGlobalAdjDict[empCat];
                    }
                    present += catGlobalAdj;
                }

                string overKey = masterId + "_" + empCat;
                bool isOverridden = overDict.ContainsKey(overKey);
                float final = isOverridden ? overDict[overKey].FinalDays : present;
                string overRemarks = isOverridden ? overDict[overKey].Remarks : "";
                
                // Determine employee's specific wage rate
                float empWage = wage > 0 ? wage : (wageDict.ContainsKey(empCat) ? wageDict[empCat] : 0f);
                
                list.Add(new {
                    MasterId = masterId,
                    ID = id,
                    Name = dr["Name"].ToString(),
                    Department = dr["Department"].ToString(),
                    Category = empCat,
                    Present = present,
                    Final = final,
                    Amount = final * empWage,
                    WageRate = empWage,
                    IsOverridden = isOverridden,
                    Remarks = overRemarks
                });
            }

            return new JavaScriptSerializer().Serialize(list);
        }

        [WebMethod]
        public static string SaveWage(int year, int month, string category, float wage)
        {
            string q = @"MERGE INTO CalculationWages t
                         USING (SELECT :Y as Year, :M as Month, :C as Category, :W as WageRate FROM DUAL) s
                         ON (t.Year = s.Year AND t.Month = s.Month AND t.Category = s.Category)
                         WHEN MATCHED THEN
                           UPDATE SET t.WageRate = s.WageRate
                         WHEN NOT MATCHED THEN
                           INSERT (Year, Month, Category, WageRate) VALUES (s.Year, s.Month, s.Category, s.WageRate)";
            DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), q,
                new OracleParameter("Y", year), new OracleParameter("M", month),
                new OracleParameter("C", category), new OracleParameter("W", wage));
            return "{\"status\":\"success\"}";
        }

        [WebMethod]
        public static string SaveOverride(int year, int month, string category, string empId, float finalDays, string remarks)
        {
            if (category == "All")
            {
                string qCat = "SELECT Category FROM Employees WHERE MasterId=:MasterId";
                object res = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), qCat, new OracleParameter("MasterId", empId));
                if (res != null && res != DBNull.Value)
                {
                    category = res.ToString();
                }
            }
            string q = @"MERGE INTO CalculationOverrides t
                         USING (SELECT :Y as Year, :M as Month, :C as Category, :ID as EmpID, :F as FinalDays, :R as Remarks FROM DUAL) s
                         ON (t.Year = s.Year AND t.Month = s.Month AND t.Category = s.Category AND t.EmpID = s.EmpID)
                         WHEN MATCHED THEN
                           UPDATE SET t.FinalDays = s.FinalDays, t.Remarks = s.Remarks
                         WHEN NOT MATCHED THEN
                           INSERT (Year, Month, Category, EmpID, FinalDays, Remarks) VALUES (s.Year, s.Month, s.Category, s.EmpID, s.FinalDays, s.Remarks)";
            DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), q,
                new OracleParameter("Y", year), new OracleParameter("M", month),
                new OracleParameter("C", category), new OracleParameter("ID", empId),
                new OracleParameter("F", finalDays), new OracleParameter("R", remarks));
            return "{\"status\":\"success\"}";
        }

        [WebMethod]
        public static string DeleteOverride(int year, int month, string category, string empId)
        {
            if (category == "All")
            {
                string qCat = "SELECT Category FROM Employees WHERE MasterId=:MasterId";
                object res = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), qCat, new OracleParameter("MasterId", empId));
                if (res != null && res != DBNull.Value)
                {
                    category = res.ToString();
                }
            }
            string q = "DELETE FROM CalculationOverrides WHERE Year=:Y AND Month=:M AND Category=:C AND EmpID=:ID";
            DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), q,
                new OracleParameter("Y", year), new OracleParameter("M", month),
                new OracleParameter("C", category), new OracleParameter("ID", empId));
            return "{\"status\":\"success\"}";
        }

        [WebMethod]
        public static string GetCategories()
        {
            string query = "SELECT Name FROM Categories ORDER BY Name ASC";
            DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
            List<string> list = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(dr["Name"].ToString());
            }
            return new JavaScriptSerializer().Serialize(list);
        }

        [WebMethod]
        public static string GetDivisions()
        {
            string query = "SELECT Name FROM Divisions ORDER BY Name ASC";
            DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
            List<string> list = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(dr["Name"].ToString());
            }
            return new JavaScriptSerializer().Serialize(list);
        }

        [WebMethod]
        public static string GetContractsForMonth(int year, int month, string category)
        {
            if (category == "All")
            {
                return "[]";
            }
            try
            {
                DateTime firstDay = new DateTime(year, month + 1, 1);
                DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);

                string query = @"
                    SELECT cp.Id, cp.StartDate, cp.EndDate, cp.Status, v.Name AS VendorName 
                    FROM ContractPeriods cp 
                    JOIN Vendors v ON cp.VendorId = v.Id 
                    WHERE cp.Category = :Category 
                      AND cp.StartDate <= :LastDay 
                      AND (cp.EndDate IS NULL OR cp.EndDate >= :FirstDay)
                    ORDER BY cp.Status ASC, cp.StartDate DESC";
                
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query, 
                    new OracleParameter("Category", category),
                    new OracleParameter("LastDay", lastDay),
                    new OracleParameter("FirstDay", firstDay));

                var list = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    int id = Convert.ToInt32(row["Id"]);
                    string status = row["Status"].ToString();
                    DateTime start = Convert.ToDateTime(row["StartDate"]);
                    string dateStr = start.ToString("yyyy-MM-dd");
                    if (row["EndDate"] != DBNull.Value)
                    {
                        dateStr += " to " + Convert.ToDateTime(row["EndDate"]).ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        dateStr += " onwards";
                    }
                    string vendor = row["VendorName"].ToString();
                    string text = string.Format("{0} ({1}) - {2}", status, dateStr, vendor);

                    list.Add(new { Value = id, Text = text });
                }
                return new JavaScriptSerializer().Serialize(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetContractsForMonth: " + ex.Message);
                return "[]";
            }
        }

        private struct AttCell
        {
            public float? Val;
            public bool IsHoliday;
            public string LeaveType;
        }

        private class EngagementRange
        {
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }

        private class OverrideInfo
        {
            public float FinalDays { get; set; }
            public string Remarks { get; set; }
        }
    }
}

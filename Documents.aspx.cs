using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using AttendanceApp.Utils;
using Oracle.ManagedDataAccess.Client;

namespace AttendanceApp
{
    public partial class Documents : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            int role = Convert.ToInt32(Session["Role"] ?? 0);
            if (role != 1)
            {
                Response.Redirect("Dashboard.aspx");
            }
        }

        [WebMethod]
        public static string GetCategories()
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "[]";

            try
            {
                string sql = "SELECT Name FROM Categories ORDER BY Name ASC";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), sql);
                var list = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(row["Name"].ToString());
                }
                return new JavaScriptSerializer().Serialize(list);
            }
            catch (Exception ex)
            {
                return "{\"error\":\"" + ex.Message + "\"}";
            }
        }

        [WebMethod]
        public static string GetDivisions()
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "[]";

            try
            {
                string sql = "SELECT Name FROM Divisions ORDER BY Name ASC";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), sql);
                var list = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(row["Name"].ToString());
                }
                return new JavaScriptSerializer().Serialize(list);
            }
            catch (Exception ex)
            {
                return "{\"error\":\"" + ex.Message + "\"}";
            }
        }

        [WebMethod]
        public static string GetContractsForMonth(int year, int month, string category)
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "[]";

            // month is 0-indexed (0 = Jan, 11 = Dec)
            DateTime firstDay = new DateTime(year, month + 1, 1);
            DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);

            try
            {
                string query = @"
                    SELECT cp.Id, cp.GemId, cp.StartDate, cp.EndDate, cp.Status, 
                           v.Name AS VendorName, v.Address AS VendorAddress
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
                    DateTime start = Convert.ToDateTime(row["StartDate"]);
                    string dateStr = start.ToString("dd-MMM-yyyy");
                    if (row["EndDate"] != DBNull.Value)
                    {
                        dateStr += " to " + Convert.ToDateTime(row["EndDate"]).ToString("dd-MMM-yyyy");
                    }
                    else
                    {
                        dateStr += " onwards";
                    }

                    list.Add(new {
                        Id = Convert.ToInt32(row["Id"]),
                        GemId = row["GemId"] != DBNull.Value ? row["GemId"].ToString() : "",
                        StartDate = start.ToString("dd-MMM-yyyy"),
                        EndDate = row["EndDate"] != DBNull.Value ? Convert.ToDateTime(row["EndDate"]).ToString("dd-MMM-yyyy") : "",
                        Status = row["Status"].ToString(),
                        VendorName = row["VendorName"].ToString(),
                        VendorAddress = row["VendorAddress"] != DBNull.Value ? row["VendorAddress"].ToString() : "",
                        DisplayName = string.Format("{0} ({1}) - {2}", row["Status"], dateStr, row["VendorName"])
                    });
                }

                return new JavaScriptSerializer().Serialize(list);
            }
            catch (Exception ex)
            {
                return "{\"error\":\"" + ex.Message + "\"}";
            }
        }

        [WebMethod]
        public static string GetCertificateData(int year, int month, string category, int? contractPeriodId)
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "[]";

            // month is 0-indexed (0 = Jan, 11 = Dec)
            DateTime firstDay = new DateTime(year, month + 1, 1);
            DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);

            try
            {
                int? selectedCpId = contractPeriodId;
                DateTime? cpStartDate = null;
                DateTime? cpEndDate = null;

                if (category != "All" && !selectedCpId.HasValue)
                {
                    // Fallback to active contract if not passed explicitly
                    string query = @"
                        SELECT Id, StartDate, EndDate 
                        FROM ContractPeriods 
                        WHERE Category = :Category 
                          AND StartDate <= :LastDay 
                          AND (EndDate IS NULL OR EndDate >= :FirstDay)
                        ORDER BY Status ASC, StartDate DESC";
                    DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query, 
                        new OracleParameter("Category", category),
                        new OracleParameter("LastDay", lastDay),
                        new OracleParameter("FirstDay", firstDay));
                    if (dt.Rows.Count > 0)
                    {
                        selectedCpId = Convert.ToInt32(dt.Rows[0]["Id"]);
                    }
                }
                
                if (selectedCpId.HasValue)
                {
                    string cpQuery = "SELECT StartDate, EndDate FROM ContractPeriods WHERE Id = :Id";
                    DataTable dtCp = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), cpQuery, 
                        new OracleParameter("Id", selectedCpId.Value));
                    if (dtCp.Rows.Count > 0)
                    {
                        cpStartDate = Convert.ToDateTime(dtCp.Rows[0]["StartDate"]);
                        if (dtCp.Rows[0]["EndDate"] != DBNull.Value)
                        {
                            cpEndDate = Convert.ToDateTime(dtCp.Rows[0]["EndDate"]);
                        }
                    }
                }

                // 1. Fetch Employees
                string empQuery;
                List<OracleParameter> empParams = new List<OracleParameter>();
                
                if (selectedCpId.HasValue)
                {
                    empQuery = @"
                        SELECT DISTINCT e.MasterId, NVL(ee.EmployeeId, e.ID) AS ID, e.Name, 
                               e.Department, e.Category, e.JoinDate, e.ResignDate, e.ContractEndDate, 
                               CASE WHEN (SELECT ee_curr.ContractPeriodId FROM EmployeeEngagements ee_curr WHERE ee_curr.Id = e.CurrentEngagementId) = :SelectedCpId 
                                    THEN e.LeaveBalance 
                                    ELSE e.PrevLeaveBalance 
                               END AS LeaveBalance
                        FROM Employees e
                        JOIN EmployeeEngagements ee ON e.MasterId = ee.EmpID
                        WHERE e.Status IN ('Active', 'Upgraded', 'Downgraded', 'ContractEnded', 'Resigned', 'Transferred')
                          AND ee.ContractPeriodId = :SelectedCpId";
                    empParams.Add(new OracleParameter("SelectedCpId", selectedCpId.Value));
                }
                else
                {
                    empQuery = "SELECT MasterId, ID, Name, Department, Category, JoinDate, ResignDate, ContractEndDate, LeaveBalance FROM Employees WHERE Status IN ('Active', 'Upgraded', 'Downgraded', 'ContractEnded', 'Resigned', 'Transferred')";
                }
                
                if (category != "All" && !selectedCpId.HasValue)
                {
                    empQuery += " AND Category = :Cat";
                    empParams.Add(new OracleParameter("Cat", category));
                }

                string orderDept = selectedCpId.HasValue ? "e.Department" : "Department";
                string orderName = selectedCpId.HasValue ? "e.Name" : "Name";
                empQuery += string.Format(" ORDER BY {0} ASC, {1} ASC", orderDept, orderName);

                DataTable dtEmp = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), empQuery, empParams.ToArray());

                // Fetch Global Adjustment
                float globalAdj = 0;
                string gq = "SELECT StatusValue FROM Attendance WHERE Year=:Y AND Month=:M AND EmpID='GLOBAL' AND Day=0 AND ROWNUM <= 1";
                object gRes = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), gq,
                    new OracleParameter("Y", year), new OracleParameter("M", month));
                if (gRes != null && gRes != DBNull.Value)
                {
                    globalAdj = Convert.ToSingle(gRes);
                }

                // Fetch Category-specific Global Adjustments
                Dictionary<string, float> catGlobalAdjDict = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    string catGq = "SELECT EmpID, StatusValue FROM Attendance WHERE Year=:Y AND Month=:M AND EmpID LIKE 'GLOBAL_%' AND Day=0";
                    DataTable dtCatGlob = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), catGq,
                        new OracleParameter("Y", year), new OracleParameter("M", month));
                    foreach (DataRow r in dtCatGlob.Rows)
                    {
                        string empId = r["EmpID"].ToString();
                        string catName = empId.Substring(7);
                        float val = r["StatusValue"] != DBNull.Value ? Convert.ToSingle(r["StatusValue"]) : 0f;
                        catGlobalAdjDict[catName] = val;
                    }
                }
                catch { }

                // Fetch Overlapping Engagements
                string engQuery = @"
                    SELECT ee.EmpID, ee.StartDate, ee.EndDate, cp.EndDate AS CpEndDate
                    FROM EmployeeEngagements ee
                    JOIN ContractPeriods cp ON ee.ContractPeriodId = cp.Id
                    WHERE (ee.StartDate <= :LastDay AND (ee.EndDate IS NULL OR ee.EndDate >= :FirstDay))";
                List<OracleParameter> engParams = new List<OracleParameter> {
                    new OracleParameter("LastDay", lastDay),
                    new OracleParameter("FirstDay", firstDay)
                };
                if (selectedCpId.HasValue)
                {
                    engQuery += " AND ee.ContractPeriodId = :SelectedCpId";
                    engParams.Add(new OracleParameter("SelectedCpId", selectedCpId.Value));
                }
                DataTable dtEng = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), engQuery, engParams.ToArray());

                Dictionary<string, List<EngagementRange>> engDict = new Dictionary<string, List<EngagementRange>>();
                foreach (DataRow dr in dtEng.Rows)
                {
                    string empId = dr["EmpID"].ToString();
                    DateTime startDate = Convert.ToDateTime(dr["StartDate"]);
                    DateTime? eeEndDate = dr["EndDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["EndDate"]) : null;
                    DateTime? rowCpEndDate = dr["CpEndDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["CpEndDate"]) : null;
                    
                    DateTime? effectiveEndDate = null;
                    if (eeEndDate.HasValue && rowCpEndDate.HasValue)
                    {
                        effectiveEndDate = eeEndDate.Value < rowCpEndDate.Value ? eeEndDate.Value : rowCpEndDate.Value;
                    }
                    else if (eeEndDate.HasValue)
                    {
                        effectiveEndDate = eeEndDate;
                    }
                    else if (rowCpEndDate.HasValue)
                    {
                        effectiveEndDate = rowCpEndDate;
                    }

                    if (!engDict.ContainsKey(empId))
                        engDict[empId] = new List<EngagementRange>();

                    engDict[empId].Add(new EngagementRange { StartDate = startDate, EndDate = effectiveEndDate });
                }

                // Fetch Calculation Overrides
                string oQ = "SELECT EmpID, FinalDays, Remarks FROM CalculationOverrides WHERE Year=:Y AND Month=:M";
                List<OracleParameter> oParams = new List<OracleParameter> {
                    new OracleParameter("Y", year),
                    new OracleParameter("M", month)
                };
                if (selectedCpId.HasValue)
                {
                    oQ += " AND (ContractPeriodId = :SelectedCpId OR (ContractPeriodId IS NULL AND Category = :C))";
                    oParams.Add(new OracleParameter("SelectedCpId", selectedCpId.Value));
                    oParams.Add(new OracleParameter("C", category));
                }
                else if (category != "All")
                {
                    oQ += " AND Category=:C";
                    oParams.Add(new OracleParameter("C", category));
                }
                DataTable dtOver = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), oQ, oParams.ToArray());
                Dictionary<string, OverrideInfo> overDict = new Dictionary<string, OverrideInfo>();
                foreach (DataRow dr in dtOver.Rows)
                {
                    overDict[dr["EmpID"].ToString()] = new OverrideInfo {
                        FinalDays = Convert.ToSingle(dr["FinalDays"]),
                        Remarks = dr["Remarks"] == DBNull.Value ? "" : dr["Remarks"].ToString()
                    };
                }

                var employeesList = new List<object>();

                foreach (DataRow row in dtEmp.Rows)
                {
                    string masterId = row["MasterId"].ToString();
                    string empId = row["ID"].ToString();
                    DateTime? joinDate = row["JoinDate"] != DBNull.Value ? Convert.ToDateTime(row["JoinDate"]) : (DateTime?)null;
                    DateTime? resignDate = row["ResignDate"] != DBNull.Value ? Convert.ToDateTime(row["ResignDate"]) : (DateTime?)null;
                    DateTime? contractEndDate = row["ContractEndDate"] != DBNull.Value ? Convert.ToDateTime(row["ContractEndDate"]) : (DateTime?)null;
                    double initialBalance = row["LeaveBalance"] != DBNull.Value ? Convert.ToDouble(row["LeaveBalance"]) : 0;

                    // Bounds Check
                    if (joinDate.HasValue)
                    {
                        if (year < joinDate.Value.Year || (year == joinDate.Value.Year && month < (joinDate.Value.Month - 1)))
                            continue;
                    }
                    if (resignDate.HasValue)
                    {
                        if (year > resignDate.Value.Year || (year == resignDate.Value.Year && month > (resignDate.Value.Month - 1)))
                            continue;
                    }
                    DateTime? effectiveContractEndDate = cpEndDate ?? contractEndDate;
                    if (effectiveContractEndDate.HasValue)
                    {
                        if (year > effectiveContractEndDate.Value.Year || (year == effectiveContractEndDate.Value.Year && month > (effectiveContractEndDate.Value.Month - 1)))
                            continue;
                    }

                    // Query raw attendance rows for the current month
                    string currQuery = @"
                        SELECT Day, StatusValue, IsHoliday, LeaveType, AutoSat, Remarks
                        FROM Attendance
                        WHERE EmpID = :EmpID AND Year = :TY AND Month = :TM AND EmpID != 'GLOBAL'";
                    
                    List<OracleParameter> currParams = new List<OracleParameter> {
                        new OracleParameter("EmpID", masterId),
                        new OracleParameter("TY", year),
                        new OracleParameter("TM", month)
                    };

                    if (cpStartDate.HasValue)
                    {
                        currQuery += " AND TO_DATE(Year || '-' || (Month + 1) || '-' || Day, 'YYYY-MM-DD') >= :CpStartDate";
                        currParams.Add(new OracleParameter("CpStartDate", cpStartDate.Value));
                    }
                    if (cpEndDate.HasValue)
                    {
                        currQuery += " AND TO_DATE(Year || '-' || (Month + 1) || '-' || Day, 'YYYY-MM-DD') <= :CpEndDate";
                        currParams.Add(new OracleParameter("CpEndDate", cpEndDate.Value));
                    }

                    DataTable dtCurr = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), currQuery, currParams.ToArray());

                    int currFullPaid = 0;
                    int currFullUnpaid = 0;
                    int currSatCut = 0;
                    int currHalfCount = 0;
                    double currLegacyHalf = 0;
                    
                    List<string> remarksList = new List<string>();
                    List<string> cellSpecificRemarksList = new List<string>();
                    List<string> satRemarksList = new List<string>();

                    foreach (DataRow r in dtCurr.Rows)
                    {
                        int day = Convert.ToInt32(r["Day"]);
                        float? val = r["StatusValue"] == DBNull.Value ? (float?)null : Convert.ToSingle(r["StatusValue"]);
                        string leaveType = r["LeaveType"].ToString();
                        bool isHoliday = Convert.ToInt32(r["IsHoliday"]) == 1;
                        int autoSat = r.Table.Columns.Contains("AutoSat") && r["AutoSat"] != DBNull.Value ? Convert.ToInt32(r["AutoSat"]) : 1;
                        string cellRemarks = r.Table.Columns.Contains("Remarks") && r["Remarks"] != DBNull.Value ? r["Remarks"].ToString() : "";
                        DateTime cellDate = new DateTime(year, month + 1, day);

                        if (isHoliday) continue;
                        if (cellDate.DayOfWeek == DayOfWeek.Sunday) continue;

                        // Check Saturday Edits & General Remarks
                        if (!string.IsNullOrEmpty(cellRemarks))
                        {
                            if (cellDate.DayOfWeek == DayOfWeek.Saturday && autoSat == 0)
                            {
                                satRemarksList.Add("Saturday edited: " + cellDate.ToString("yyyy-MM-dd") + " (Reason: " + cellRemarks + ")");
                            }
                            else
                            {
                                cellSpecificRemarksList.Add(cellDate.ToString("dd-MMM") + ": " + cellRemarks);
                            }
                        }

                        if (val == 0.5f)
                        {
                            currLegacyHalf += 1.0;
                        }
                        else if (val == 0f)
                        {
                            if (leaveType == "Paid")
                            {
                                currFullPaid++;
                            }
                            else if (cellDate.DayOfWeek == DayOfWeek.Saturday)
                            {
                                currSatCut++;
                            }
                            else if (leaveType == "Unpaid")
                            {
                                currFullUnpaid++;
                            }
                        }
                    }

                    // Chronological half-day pairing
                    DateTime stintStartDate = new DateTime(1900, 1, 1);
                    string stintQuery = @"
                        SELECT MIN(ee.StartDate) AS StintStartDate
                        FROM EmployeeEngagements ee
                        WHERE ee.EmpID = :EmpID AND ee.StartDate > COALESCE(
                            (SELECT MAX(ex.EndDate) FROM EmployeeEngagements ex WHERE ex.EmpID = ee.EmpID AND UPPER(ex.EndReason) = 'RESIGNED'),
                            TO_DATE('1900-01-01', 'YYYY-MM-DD')
                        )";
                    DataTable dtStint = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), stintQuery, new OracleParameter("EmpID", masterId));
                    if (dtStint.Rows.Count > 0 && dtStint.Rows[0]["StintStartDate"] != DBNull.Value)
                    {
                        stintStartDate = Convert.ToDateTime(dtStint.Rows[0]["StintStartDate"]);
                    }

                    DateTime targetMonthLastDay = new DateTime(year, month + 1, 1).AddMonths(1).AddDays(-1);
                    string halfDaysQuery;
                    List<OracleParameter> halfParams = new List<OracleParameter> {
                        new OracleParameter("EmpID", masterId),
                        new OracleParameter("StintStartDate", stintStartDate),
                        new OracleParameter("LastDay", targetMonthLastDay)
                    };

                    if (selectedCpId.HasValue)
                    {
                        halfDaysQuery = @"
                            SELECT a.Year, a.Month, a.Day, a.LeaveType
                            FROM Attendance a
                            JOIN EmployeeEngagements ee_past ON a.EmpID = ee_past.EmpID
                              AND TO_DATE(a.Year || '-' || (a.Month + 1) || '-' || a.Day, 'YYYY-MM-DD') >= ee_past.StartDate
                              AND (ee_past.EndDate IS NULL OR TO_DATE(a.Year || '-' || (a.Month + 1) || '-' || a.Day, 'YYYY-MM-DD') <= ee_past.EndDate)
                            WHERE a.EmpID = :EmpID
                              AND ee_past.ContractPeriodId = :SelectedCpId
                              AND a.LeaveType IN ('Carried', 'Paired Paid', 'Paired Unpaid')
                              AND TO_DATE(a.Year || '-' || (a.Month + 1) || '-' || a.Day, 'YYYY-MM-DD') >= :StintStartDate
                              AND TO_DATE(a.Year || '-' || (a.Month + 1) || '-' || a.Day, 'YYYY-MM-DD') <= :LastDay
                            ORDER BY a.Year ASC, a.Month ASC, a.Day ASC";
                        halfParams.Add(new OracleParameter("SelectedCpId", selectedCpId.Value));
                    }
                    else
                    {
                        halfDaysQuery = @"
                            SELECT a.Year, a.Month, a.Day, a.LeaveType
                            FROM Attendance a
                            JOIN EmployeeEngagements ee_past ON a.EmpID = ee_past.EmpID
                              AND TO_DATE(a.Year || '-' || (a.Month + 1) || '-' || a.Day, 'YYYY-MM-DD') >= ee_past.StartDate
                              AND (ee_past.EndDate IS NULL OR TO_DATE(a.Year || '-' || (a.Month + 1) || '-' || a.Day, 'YYYY-MM-DD') <= ee_past.EndDate)
                            JOIN EmployeeEngagements ee_curr ON a.EmpID = ee_curr.EmpID
                              AND ee_curr.StartDate <= :LastDay
                              AND (ee_curr.EndDate IS NULL OR ee_curr.EndDate >= :FirstDay)
                            WHERE a.EmpID = :EmpID
                              AND (ee_past.ContractPeriodId = ee_curr.ContractPeriodId OR (ee_past.ContractPeriodId IS NULL AND ee_curr.ContractPeriodId IS NULL AND ee_past.Category = ee_curr.Category))
                              AND a.LeaveType IN ('Carried', 'Paired Paid', 'Paired Unpaid')
                              AND TO_DATE(a.Year || '-' || (a.Month + 1) || '-' || a.Day, 'YYYY-MM-DD') >= :StintStartDate
                              AND TO_DATE(a.Year || '-' || (a.Month + 1) || '-' || a.Day, 'YYYY-MM-DD') <= :LastDay
                            ORDER BY a.Year ASC, a.Month ASC, a.Day ASC";
                        halfParams.Add(new OracleParameter("FirstDay", firstDay));
                    }
                    
                    DataTable dtHalfDays = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), halfDaysQuery, halfParams.ToArray());

                    List<Tuple<DateTime, DateTime, string>> completedPairs = new List<Tuple<DateTime, DateTime, string>>();
                    DateTime? unpairedCarriedDate = null;
                    
                    for (int idx = 0; idx < dtHalfDays.Rows.Count; idx++)
                    {
                        DataRow r = dtHalfDays.Rows[idx];
                        int yr = Convert.ToInt32(r["Year"]);
                        int mn = Convert.ToInt32(r["Month"]);
                        int dy = Convert.ToInt32(r["Day"]);
                        string leaveType = r["LeaveType"].ToString();
                        DateTime date = new DateTime(yr, mn + 1, dy);
                        
                        if (yr == year && mn == month)
                        {
                            currHalfCount++;
                        }

                        int overallIdx = idx + 1;
                        if (overallIdx % 2 == 1)
                        {
                            unpairedCarriedDate = date;
                        }
                        else
                        {
                            if (unpairedCarriedDate.HasValue)
                            {
                                completedPairs.Add(Tuple.Create(unpairedCarriedDate.Value, date, leaveType));
                                unpairedCarriedDate = null;
                            }
                        }
                    }

                    int pairedPaidThisMonth = 0;
                    int pairedUnpaidThisMonth = 0;

                    foreach (var pair in completedPairs)
                    {
                        DateTime firstDate = pair.Item1;
                        DateTime secondDate = pair.Item2;
                        string leaveType = pair.Item3;

                        if (secondDate.Year == year && secondDate.Month == (month + 1))
                        {
                            remarksList.Add("2 half days: " + firstDate.ToString("dd-MMM") + " & " + secondDate.ToString("dd-MMM"));
                            if (leaveType == "Paired Paid")
                            {
                                pairedPaidThisMonth++;
                            }
                            else if (leaveType == "Paired Unpaid")
                            {
                                pairedUnpaidThisMonth++;
                            }
                        }
                    }

                    int totalPaid = currFullPaid + pairedPaidThisMonth;
                    int totalUnpaid = currFullUnpaid + pairedUnpaidThisMonth;

                    // Present Days calculation
                    Dictionary<int, AttCell> empAtt = new Dictionary<int, AttCell>();
                    foreach (DataRow r in dtCurr.Rows)
                    {
                        int day = Convert.ToInt32(r["Day"]);
                        float? val = r["StatusValue"] == DBNull.Value ? (float?)null : Convert.ToSingle(r["StatusValue"]);
                        bool isHoliday = Convert.ToInt32(r["IsHoliday"]) == 1;
                        string leaveType = r["LeaveType"] == DBNull.Value ? "" : r["LeaveType"].ToString();
                        empAtt[day] = new AttCell { Val = val, IsHoliday = isHoliday, LeaveType = leaveType };
                    }

                    int daysInMonth = DateTime.DaysInMonth(year, month + 1);
                    float presentCount = 0f;
                    List<EngagementRange> empEngs = engDict.ContainsKey(masterId) ? engDict[masterId] : new List<EngagementRange>();

                    for (int d = 1; d <= daysInMonth; d++)
                    {
                        DateTime currDate = new DateTime(year, month + 1, d);
                        // boundary check matching Attendance.aspx
                        bool isOutOfBounds = true;
                        foreach (var eng in empEngs)
                        {
                            if (currDate >= eng.StartDate.Date && (!eng.EndDate.HasValue || currDate <= eng.EndDate.Value.Date))
                            {
                                isOutOfBounds = false;
                                break;
                            }
                        }

                        // Special Friday resignation exception for Saturday:
                        // If the day is a Saturday, and the employee resigned on the preceding Friday, Saturday is treated as in-bounds.
                        if (isOutOfBounds && currDate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            DateTime prevFriday = currDate.AddDays(-1);
                            if (resignDate.HasValue && resignDate.Value.Date == prevFriday.Date && resignDate.Value.DayOfWeek == DayOfWeek.Friday)
                            {
                                foreach (var eng in empEngs)
                                {
                                    if (prevFriday >= eng.StartDate.Date && (!eng.EndDate.HasValue || prevFriday <= eng.EndDate.Value.Date))
                                    {
                                        isOutOfBounds = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (isOutOfBounds) continue;

                        AttCell cell = empAtt.ContainsKey(d) ? empAtt[d] : new AttCell { Val = null, IsHoliday = false, LeaveType = "" };
                        if (currDate.DayOfWeek == DayOfWeek.Sunday && !cell.IsHoliday) continue;

                        if (cell.IsHoliday)
                        {
                            presentCount += 1.0f;
                        }
                        else if (cell.LeaveType == "Carried" || cell.LeaveType == "Paired Paid")
                        {
                            presentCount += 1.0f;
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

                    float catGlobalAdj = 0;
                    string empCat = row["Category"].ToString();
                    if (catGlobalAdjDict.ContainsKey(empCat))
                    {
                        catGlobalAdj = catGlobalAdjDict[empCat];
                    }
                    float presentDays = presentCount + globalAdj + catGlobalAdj;

                    // Final days
                    bool isOverridden = overDict.ContainsKey(masterId);
                    float finalDays = isOverridden ? overDict[masterId].FinalDays : presentDays;
                    string overrideRemarks = isOverridden ? overDict[masterId].Remarks : "";

                    // Custom Join/Resign comments
                    string joinResignRemark = "";
                    if (joinDate.HasValue && joinDate.Value.Year == year && joinDate.Value.Month == (month + 1))
                    {
                        joinResignRemark = "Joined on " + joinDate.Value.ToString("dd-MMM-yyyy");
                    }
                    else if (resignDate.HasValue && resignDate.Value.Year == year && resignDate.Value.Month == (month + 1))
                    {
                        joinResignRemark = "Resigned on " + resignDate.Value.ToString("dd-MMM-yyyy");
                    }

                    employeesList.Add(new {
                        MasterId = masterId,
                        EmployeeId = empId,
                        Name = row["Name"].ToString(),
                        Department = row["Department"].ToString(),
                        Category = empCat,
                        PresentDays = presentDays,
                        FinalDays = finalDays,
                        Paid = totalPaid,
                        Unpaid = totalUnpaid,
                        SatCut = currSatCut,
                        OverrideRemark = overrideRemarks,
                        JoinResignRemark = joinResignRemark,
                        LeavePairRemark = string.Join("; ", remarksList),
                        CellRemarks = string.Join("; ", cellSpecificRemarksList),
                        SaturdayRemark = string.Join("; ", satRemarksList),
                        IsOverridden = isOverridden
                    });
                }

                return new JavaScriptSerializer().Serialize(employeesList);
            }
            catch (Exception ex)
            {
                return "{\"error\":\"" + ex.Message + "\"}";
            }
        }

        [WebMethod]
        public static string GetTemplates()
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "{}";

            try
            {
                string query = "SELECT TemplateKey, TemplateValue FROM CertificateTemplates";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (DataRow row in dt.Rows)
                {
                    dict[row["TemplateKey"].ToString()] = row["TemplateValue"].ToString();
                }
                
                // Fallbacks if not seeded
                if (!dict.ContainsKey("AttDesc1"))
                {
                    dict["AttDesc1"] = "This is certify that DEO ({Category}) under GeM Contract No: {ContractNo}, dated: {ContractDate}";
                }
                if (!dict.ContainsKey("AttDesc2"))
                {
                    dict["AttDesc2"] = "M/s. {VendorName}, {VendorAddress} worked as following, for the period from {StartDate} to {EndDate}";
                }
                if (!dict.ContainsKey("SatDesc1"))
                {
                    dict["SatDesc1"] = "This is to certify that M/s. <b>{VendorName}</b>, {VendorAddress} is engaged as an Industry Partner in our Establishment to provide Services towards <b>{Services}</b> for a period of {Duration} w.e.f. {WefDate} against GeM Contract No. <b>{ContractNo}</b> dated {ContractDate}.";
                }
                if (!dict.ContainsKey("SatDesc2"))
                {
                    dict["SatDesc2"] = "The Industry Partner provided <b>{EmpCount}</b> Contract Employees and found working satisfactorily.";
                }
                if (!dict.ContainsKey("SatDesc3"))
                {
                    dict["SatDesc3"] = "The service provided by the Industry Partner from {StartDate} to {EndDate} is found satisfactory.";
                }
                if (!dict.ContainsKey("SatSignatory"))
                {
                    dict["SatSignatory"] = "(Raajita B Reddy)";
                }
                if (!dict.ContainsKey("SatDesignation"))
                {
                    dict["SatDesignation"] = "Scientist 'F'";
                }
                if (!dict.ContainsKey("SatServices"))
                {
                    dict["SatServices"] = "Human Resource Outsourcing Services";
                }
                if (!dict.ContainsKey("CovPhone"))
                {
                    dict["CovPhone"] = "2312";
                }
                if (!dict.ContainsKey("CovRefNo"))
                {
                    dict["CovRefNo"] = "49805/HRD/HM/{Year}";
                }
                if (!dict.ContainsKey("CovSubject"))
                {
                    dict["CovSubject"] = "HIRING OF MANPOWER SERVICES";
                }
                if (!dict.ContainsKey("CovBody"))
                {
                    dict["CovBody"] = "The copies of the Attendance report along with the wage calculation for {Category} category Contract Employees from M/s. {VendorName}, {VendorAddress} for the period of {StartDate} to {EndDate} is enclosed. This is for purpose of their payment processing please.";
                }
                if (!dict.ContainsKey("CovSignatory"))
                {
                    dict["CovSignatory"] = "Usha Nandini AA";
                }
                if (!dict.ContainsKey("CovDesignation"))
                {
                    dict["CovDesignation"] = "TO C";
                }
                if (!dict.ContainsKey("CovAuthority"))
                {
                    dict["CovAuthority"] = "For GD, {Division}";
                }
                if (!dict.ContainsKey("CovRecipient"))
                {
                    dict["CovRecipient"] = "To,\r\nD-FMM/Purchase";
                }
                if (!dict.ContainsKey("WagesHdrContract"))
                {
                    dict["WagesHdrContract"] = "Contract No. <b>{ContractNo}</b> Dt. <b>{ContractDate}</b> {ExtraCode}";
                }
                if (!dict.ContainsKey("WagesHdrCategory"))
                {
                    dict["WagesHdrCategory"] = "Manpower Outstanding Services - Data Entry Operators({CategoryDesc}) - {PeopleCount} No.s";
                }
                if (!dict.ContainsKey("WagesHdrPeriod"))
                {
                    dict["WagesHdrPeriod"] = "Contract Period <b>{Period}</b>";
                }
                if (!dict.ContainsKey("WagesHdrVendor"))
                {
                    dict["WagesHdrVendor"] = "M/s {VendorName}, {VendorAddress}";
                }
                if (!dict.ContainsKey("WagesHdrPayment"))
                {
                    dict["WagesHdrPayment"] = "Payment for the period <b>{PaymentStart}</b> to <b>{PaymentEnd}</b> - <b>{WorkingDays} days</b>";
                }

                return new JavaScriptSerializer().Serialize(dict);
            }
            catch (Exception)
            {
                // Return default fallback dictionary if table missing or database error
                var fallback = new Dictionary<string, string>
                {
                    { "AttDesc1", "This is certify that DEO ({Category}) under GeM Contract No: {ContractNo}, dated: {ContractDate}" },
                    { "AttDesc2", "M/s. {VendorName}, {VendorAddress} worked as following, for the period from {StartDate} to {EndDate}" },
                    { "SatDesc1", "This is to certify that M/s. <b>{VendorName}</b>, {VendorAddress} is engaged as an Industry Partner in our Establishment to provide Services towards <b>{Services}</b> for a period of {Duration} w.e.f. {WefDate} against GeM Contract No. <b>{ContractNo}</b> dated {ContractDate}." },
                    { "SatDesc2", "The Industry Partner provided <b>{EmpCount}</b> Contract Employees and found working satisfactorily." },
                    { "SatDesc3", "The service provided by the Industry Partner from {StartDate} to {EndDate} is found satisfactory." },
                    { "SatSignatory", "(Raajita B Reddy)" },
                    { "SatDesignation", "Scientist 'F'" },
                    { "SatServices", "Human Resource Outsourcing Services" },
                    { "CovPhone", "2312" },
                    { "CovRefNo", "49805/HRD/HM/{Year}" },
                    { "CovSubject", "HIRING OF MANPOWER SERVICES" },
                    { "CovBody", "The copies of the Attendance report along with the wage calculation for {Category} category Contract Employees from M/s. {VendorName}, {VendorAddress} for the period of {StartDate} to {EndDate} is enclosed. This is for purpose of their payment processing please." },
                    { "CovSignatory", "Usha Nandini AA" },
                    { "CovDesignation", "TO C" },
                    { "CovAuthority", "For GD, {Division}" },
                    { "CovRecipient", "To,\r\nD-FMM/Purchase" },
                    { "WagesHdrContract", "Contract No. <b>{ContractNo}</b> Dt. <b>{ContractDate}</b> {ExtraCode}" },
                    { "WagesHdrCategory", "Manpower Outstanding Services - Data Entry Operators({CategoryDesc}) - {PeopleCount} No.s" },
                    { "WagesHdrPeriod", "Contract Period <b>{Period}</b>" },
                    { "WagesHdrVendor", "M/s {VendorName}, {VendorAddress}" },
                    { "WagesHdrPayment", "Payment for the period <b>{PaymentStart}</b> to <b>{PaymentEnd}</b> - <b>{WorkingDays} days</b>" },
                    { "WagesDesc_Skilled", "Data Entry Operators(Skilled)" },
                    { "WagesDesc_Semi_Skilled", "Staff" },
                    { "WagesDesc_Unskilled", "attender" }
                };
                return new JavaScriptSerializer().Serialize(fallback);
            }
        }

        [WebMethod]
        public static string SaveTemplates(string desc1, string desc2)
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "{\"status\":\"error\",\"message\":\"Unauthorized access.\"}";

            if (string.IsNullOrEmpty(desc1) || string.IsNullOrEmpty(desc2))
            {
                return "{\"status\":\"error\",\"message\":\"Templates cannot be empty.\"}";
            }

            try
            {
                string mergeSql = @"
                    MERGE INTO CertificateTemplates t
                    USING (
                        SELECT 'AttDesc1' AS TemplateKey, :Desc1 AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'AttDesc2' AS TemplateKey, :Desc2 AS TemplateValue FROM DUAL
                    ) s
                    ON (t.TemplateKey = s.TemplateKey)
                    WHEN MATCHED THEN
                        UPDATE SET t.TemplateValue = s.TemplateValue
                    WHEN NOT MATCHED THEN
                        INSERT (TemplateKey, TemplateValue) VALUES (s.TemplateKey, s.TemplateValue)";

                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), mergeSql,
                    new OracleParameter("Desc1", desc1),
                    new OracleParameter("Desc2", desc2));

                ActionLogger.LogAction("EDIT_TEMPLATE", "SYSTEM", "Updated Certificate Template Descriptions", null, null);

                return "{\"status\":\"success\",\"message\":\"Templates saved successfully.\"}";
            }
            catch (Exception ex)
            {
                return "{\"status\":\"error\",\"message\":\"Failed to save templates: " + ex.Message.Replace("\"", "\\\"") + "\"}";
            }
        }

        [WebMethod]
        public static string SaveSatTemplates(string desc1, string desc2, string desc3, string signatory, string designation, string services)
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "{\"status\":\"error\",\"message\":\"Unauthorized access.\"}";

            if (string.IsNullOrEmpty(desc1) || string.IsNullOrEmpty(desc2) || string.IsNullOrEmpty(desc3))
            {
                return "{\"status\":\"error\",\"message\":\"Templates cannot be empty.\"}";
            }

            try
            {
                string mergeSql = @"
                    MERGE INTO CertificateTemplates t
                    USING (
                        SELECT 'SatDesc1' AS TemplateKey, :Desc1 AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'SatDesc2' AS TemplateKey, :Desc2 AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'SatDesc3' AS TemplateKey, :Desc3 AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'SatSignatory' AS TemplateKey, :Signatory AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'SatDesignation' AS TemplateKey, :Designation AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'SatServices' AS TemplateKey, :Services AS TemplateValue FROM DUAL
                    ) s
                    ON (t.TemplateKey = s.TemplateKey)
                    WHEN MATCHED THEN
                        UPDATE SET t.TemplateValue = s.TemplateValue
                    WHEN NOT MATCHED THEN
                        INSERT (TemplateKey, TemplateValue) VALUES (s.TemplateKey, s.TemplateValue)";

                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), mergeSql,
                    new OracleParameter("Desc1", desc1),
                    new OracleParameter("Desc2", desc2),
                    new OracleParameter("Desc3", desc3),
                    new OracleParameter("Signatory", signatory ?? "(Raajita B Reddy)"),
                    new OracleParameter("Designation", designation ?? "Scientist 'F'"),
                    new OracleParameter("Services", services ?? "Human Resource Outsourcing Services"));

                ActionLogger.LogAction("EDIT_TEMPLATE", "SYSTEM", "Updated Satisfactory Certificate Templates & Settings", null, null);

                return "{\"status\":\"success\",\"message\":\"Templates saved successfully.\"}";
            }
            catch (Exception ex)
            {
                return "{\"status\":\"error\",\"message\":\"Failed to save templates: " + ex.Message.Replace("\"", "\\\"") + "\"}";
            }
        }

        [WebMethod]
        public static string SaveCovTemplates(string phone, string refNo, string subject, string body, string signatory, string designation, string authority, string recipient)
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "{\"status\":\"error\",\"message\":\"Unauthorized access.\"}";

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(refNo) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body) || string.IsNullOrEmpty(signatory) || string.IsNullOrEmpty(designation) || string.IsNullOrEmpty(authority) || string.IsNullOrEmpty(recipient))
            {
                return "{\"status\":\"error\",\"message\":\"All fields are required and cannot be empty.\"}";
            }

            try
            {
                string mergeSql = @"
                    MERGE INTO CertificateTemplates t
                    USING (
                        SELECT 'CovPhone' AS TemplateKey, :Phone AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'CovRefNo' AS TemplateKey, :RefNo AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'CovSubject' AS TemplateKey, :Subject AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'CovBody' AS TemplateKey, :Body AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'CovSignatory' AS TemplateKey, :Signatory AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'CovDesignation' AS TemplateKey, :Designation AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'CovAuthority' AS TemplateKey, :Authority AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'CovRecipient' AS TemplateKey, :Recipient AS TemplateValue FROM DUAL
                    ) s
                    ON (t.TemplateKey = s.TemplateKey)
                    WHEN MATCHED THEN
                        UPDATE SET t.TemplateValue = s.TemplateValue
                    WHEN NOT MATCHED THEN
                        INSERT (TemplateKey, TemplateValue) VALUES (s.TemplateKey, s.TemplateValue)";

                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), mergeSql,
                    new OracleParameter("Phone", phone),
                    new OracleParameter("RefNo", refNo),
                    new OracleParameter("Subject", subject),
                    new OracleParameter("Body", body),
                    new OracleParameter("Signatory", signatory),
                    new OracleParameter("Designation", designation),
                    new OracleParameter("Authority", authority),
                    new OracleParameter("Recipient", recipient));

                ActionLogger.LogAction("EDIT_TEMPLATE", "SYSTEM", "Updated Covering Letter Templates & Settings", null, null);

                return "{\"status\":\"success\",\"message\":\"Covering Letter templates saved successfully.\"}";
            }
            catch (Exception ex)
            {
                return "{\"status\":\"error\",\"message\":\"Failed to save covering letter templates: " + ex.Message.Replace("\"", "\\\"") + "\"}";
            }
        }

        [WebMethod]
        public static string SaveWagesTemplates(string hdrContract, string hdrCategory, string hdrPeriod, string hdrVendor, string hdrPayment, string category, string categoryDesc, string epfLimit, string epfRate, string epfCappedAmount)
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "{\"status\":\"error\",\"message\":\"Unauthorized access.\"}";

            if (string.IsNullOrEmpty(hdrContract) || string.IsNullOrEmpty(hdrCategory) || string.IsNullOrEmpty(hdrPeriod) || string.IsNullOrEmpty(hdrVendor) || string.IsNullOrEmpty(hdrPayment) || string.IsNullOrEmpty(category) || string.IsNullOrEmpty(categoryDesc) || string.IsNullOrEmpty(epfLimit) || string.IsNullOrEmpty(epfRate) || string.IsNullOrEmpty(epfCappedAmount))
            {
                return "{\"status\":\"error\",\"message\":\"Templates and EPF settings cannot be empty.\"}";
            }

            try
            {
                string safeCategory = category.Replace("-", "_");
                string mergeSql = @"
                    MERGE INTO CertificateTemplates t
                    USING (
                        SELECT 'WagesHdrContract' AS TemplateKey, :HdrContract AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'WagesHdrCategory' AS TemplateKey, :HdrCategory AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'WagesHdrPeriod' AS TemplateKey, :HdrPeriod AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'WagesHdrVendor' AS TemplateKey, :HdrVendor AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'WagesHdrPayment' AS TemplateKey, :HdrPayment AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'WagesDesc_' || :SafeCategory AS TemplateKey, :CategoryDesc AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'WagesEpfLimit' AS TemplateKey, :EpfLimit AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'WagesEpfRate' AS TemplateKey, :EpfRate AS TemplateValue FROM DUAL
                        UNION ALL
                        SELECT 'WagesEpfCappedAmount' AS TemplateKey, :EpfCappedAmount AS TemplateValue FROM DUAL
                    ) s
                    ON (t.TemplateKey = s.TemplateKey)
                    WHEN MATCHED THEN
                        UPDATE SET t.TemplateValue = s.TemplateValue
                    WHEN NOT MATCHED THEN
                        INSERT (TemplateKey, TemplateValue) VALUES (s.TemplateKey, s.TemplateValue)";

                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), mergeSql,
                    new OracleParameter("HdrContract", hdrContract),
                    new OracleParameter("HdrCategory", hdrCategory),
                    new OracleParameter("HdrPeriod", hdrPeriod),
                    new OracleParameter("HdrVendor", hdrVendor),
                    new OracleParameter("HdrPayment", hdrPayment),
                    new OracleParameter("SafeCategory", safeCategory),
                    new OracleParameter("CategoryDesc", categoryDesc),
                    new OracleParameter("EpfLimit", epfLimit),
                    new OracleParameter("EpfRate", epfRate),
                    new OracleParameter("EpfCappedAmount", epfCappedAmount));

                ActionLogger.LogAction("EDIT_TEMPLATE", "SYSTEM", "Updated Wages Certificate Template Headings, Category Description, EPF Limit (" + epfLimit + "), Rate (" + epfRate + "%), Capped Amount (" + epfCappedAmount + ") for " + category, null, null);

                return "{\"status\":\"success\",\"message\":\"Templates saved successfully.\"}";
            }
            catch (Exception ex)
            {
                return "{\"status\":\"error\",\"message\":\"Failed to save templates: " + ex.Message.Replace("\"", "\\\"") + "\"}";
            }
        }

        [WebMethod]
        public static string GetWagesMetadata(int year, int month, string category, int? contractPeriodId)
        {
            int role = Convert.ToInt32(HttpContext.Current.Session["Role"] ?? 0);
            if (role != 1) return "{}";

            try
            {
                float dailyWage = 0f;
                using (OracleConnection conn = new OracleConnection(DBHelper.GetAttendanceDBConnection()))
                {
                    conn.Open();
                    string wageSql = "SELECT WageRate FROM CalculationWages WHERE Year = :Year AND Month = :Month AND Category = :Category";
                    using (OracleCommand cmd = new OracleCommand(wageSql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("Year", year));
                        cmd.Parameters.Add(new OracleParameter("Month", month));
                        cmd.Parameters.Add(new OracleParameter("Category", category));
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                        {
                            dailyWage = Convert.ToSingle(res);
                        }
                    }
                }

                object contractDetails = null;
                if (contractPeriodId.HasValue)
                {
                    string contractSql = @"
                        SELECT cp.Id, cp.GemId, cp.StartDate, cp.EndDate, v.Name AS VendorName, v.Address AS VendorAddress
                        FROM ContractPeriods cp
                        JOIN Vendors v ON cp.VendorId = v.Id
                        WHERE cp.Id = :Id";
                    using (OracleConnection conn = new OracleConnection(DBHelper.GetAttendanceDBConnection()))
                    {
                        conn.Open();
                        using (OracleCommand cmd = new OracleCommand(contractSql, conn))
                        {
                            cmd.Parameters.Add(new OracleParameter("Id", contractPeriodId.Value));
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    contractDetails = new {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        GemId = reader["GemId"] != DBNull.Value ? reader["GemId"].ToString() : "",
                                        StartDate = reader["StartDate"] != DBNull.Value ? Convert.ToDateTime(reader["StartDate"]).ToString("dd-MMM-yyyy") : "",
                                        EndDate = reader["EndDate"] != DBNull.Value ? Convert.ToDateTime(reader["EndDate"]).ToString("dd-MMM-yyyy") : "",
                                        VendorName = reader["VendorName"] != DBNull.Value ? reader["VendorName"].ToString() : "",
                                        VendorAddress = reader["VendorAddress"] != DBNull.Value ? reader["VendorAddress"].ToString() : ""
                                    };
                                }
                            }
                        }
                    }
                }

                var result = new {
                    DailyWage = dailyWage,
                    Contract = contractDetails
                };

                return new JavaScriptSerializer().Serialize(result);
            }
            catch (Exception ex)
            {
                return "{\"error\":\"" + ex.Message + "\"}";
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

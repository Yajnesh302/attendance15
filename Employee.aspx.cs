using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI.WebControls;
using AttendanceApp.Utils;
using Oracle.ManagedDataAccess.Client;

namespace AttendanceApp
{
    public partial class Employee : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated || Session["PCNO"] == null)
            {
                System.Web.Security.FormsAuthentication.SignOut();
                Response.Redirect("Login.aspx");
                return;
            }

            // Only admin can access employee master in the original logic
            int role = Convert.ToInt32(Session["Role"] ?? 0);
            if (role != 1)
            {
                Response.Write("<!DOCTYPE html><html><head><title>Access Denied</title><link href='Static/fontawesome-free/css/all.min.css' rel='stylesheet' type='text/css' /><link href='Static/css/sb-admin-2.min.css' rel='stylesheet' /><style>body { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); height: 100vh; display: flex; align-items: center; justify-content: center; font-family: 'Nunito', sans-serif; color: #f1f5f9; margin: 0; } .error-card { background: rgba(30, 41, 59, 0.7); backdrop-filter: blur(10px); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 16px; padding: 40px; text-align: center; max-width: 450px; width: 90%; box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.3), 0 8px 10px -6px rgba(0, 0, 0, 0.3); animation: fadeIn 0.5s ease-out; } @keyframes fadeIn { from { opacity: 0; transform: translateY(-20px); } to { opacity: 1; transform: translateY(0); } } .error-icon { font-size: 64px; color: #f43f5e; margin-bottom: 20px; animation: pulse 2s infinite; } @keyframes pulse { 0% { transform: scale(1); } 50% { transform: scale(1.05); } 100% { transform: scale(1); } } h2 { font-size: 24px; margin-bottom: 10px; font-weight: 700; } p { color: #94a3b8; font-size: 16px; margin-bottom: 30px; line-height: 1.5; } .btn-action { display: inline-block; background: linear-gradient(135deg, #4f46e5 0%, #3b82f6 100%); color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: 600; transition: all 0.3s ease; box-shadow: 0 4px 6px -1px rgba(79, 70, 229, 0.2); margin: 5px; } .btn-action:hover { transform: translateY(-2px); box-shadow: 0 10px 15px -3px rgba(79, 70, 229, 0.4); color: white; } .btn-secondary-action { display: inline-block; background: rgba(255, 255, 255, 0.1); color: #e2e8f0; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: 600; transition: all 0.3s ease; margin: 5px; } .btn-secondary-action:hover { background: rgba(255, 255, 255, 0.2); color: white; }</style></head><body><div class='error-card'><div class='error-icon'><i class='fas fa-exclamation-triangle'></i></div><h2>Access Denied</h2><p>This page is restricted. Only administrators are allowed to access this resource.</p><div><a href='Login.aspx' class='btn-action'>Login as Admin</a><a href='Dashboard.aspx' class='btn-secondary-action'>Go to Dashboard</a></div></div></body></html>");
                Response.End();
                return;
            }

            if (!IsPostBack)
            {
                PopulateDropdowns();
                BindResignedEmployees();
                PopulateDeleteEmployeeDropdown();
                BindGrid();
            }
        }

        private void PopulateDropdowns()
        {
            try
            {
                // Populate Divisions
                string divQuery = "SELECT Name FROM Divisions ORDER BY Name ASC";
                DataTable dtDiv = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), divQuery);
                ddlDept.DataSource = dtDiv;
                ddlDept.DataTextField = "Name";
                ddlDept.DataValueField = "Name";
                ddlDept.DataBind();

                // Populate Categories
                string catQuery = "SELECT Name FROM Categories ORDER BY Name ASC";
                DataTable dtCat = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), catQuery);
                
                // For manual entry category
                ddlCat.DataSource = dtCat;
                ddlCat.DataTextField = "Name";
                ddlCat.DataValueField = "Name";
                ddlCat.DataBind();

                // For import category
                ddlImportCat.DataSource = dtCat;
                ddlImportCat.DataTextField = "Name";
                ddlImportCat.DataValueField = "Name";
                ddlImportCat.DataBind();

                // For search filter (preserve "All" at index 0 and clear existing dynamic items)
                string selectedFilter = ddlFilter.SelectedValue;
                ddlFilter.Items.Clear();
                ddlFilter.Items.Add(new ListItem("All", "All"));
                foreach (DataRow row in dtCat.Rows)
                {
                    ddlFilter.Items.Add(new ListItem(row["Name"].ToString(), row["Name"].ToString()));
                }
                
                // Try restoring selection if it still exists
                if (ddlFilter.Items.FindByValue(selectedFilter) != null)
                {
                    ddlFilter.SelectedValue = selectedFilter;
                }

                // For search division filter (preserve "All" at index 0 and clear existing dynamic items)
                string selectedDivFilter = ddlFilterDiv.SelectedValue;
                ddlFilterDiv.Items.Clear();
                ddlFilterDiv.Items.Add(new ListItem("All", "All"));
                foreach (DataRow row in dtDiv.Rows)
                {
                    ddlFilterDiv.Items.Add(new ListItem(row["Name"].ToString(), row["Name"].ToString()));
                }
                
                // Try restoring selection if it still exists
                if (ddlFilterDiv.Items.FindByValue(selectedDivFilter) != null)
                {
                    ddlFilterDiv.SelectedValue = selectedDivFilter;
                }

                // Populate Bulk Leave Category Dropdown
                ddlBulkLeaveCategory.Items.Clear();
                ddlBulkLeaveCategory.Items.Add(new ListItem("All Categories", "All"));
                foreach (DataRow row in dtCat.Rows)
                {
                    ddlBulkLeaveCategory.Items.Add(new ListItem(row["Name"].ToString(), row["Name"].ToString()));
                }

                // Populate Bulk Leave Division Dropdown
                ddlBulkLeaveDivision.Items.Clear();
                ddlBulkLeaveDivision.Items.Add(new ListItem("All Divisions", "All"));
                foreach (DataRow row in dtDiv.Rows)
                {
                    ddlBulkLeaveDivision.Items.Add(new ListItem(row["Name"].ToString(), row["Name"].ToString()));
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error populating dropdowns: " + ex.Message, false);
            }
        }

        private void BindGrid()
        {
            string filter = ddlFilter.SelectedValue;
            string divFilter = ddlFilterDiv.SelectedValue;
            string search = txtSearch.Text.Trim();
            string tabStatus = string.IsNullOrEmpty(hfActiveTab.Value) ? "Active" : hfActiveTab.Value;

            string query = "";
            List<OracleParameter> pList = new List<OracleParameter>();

            if (tabStatus == "Active")
            {
                query = "SELECT e.MasterId, e.ID, e.Name, e.Department, e.Category, NVL(e.OriginalJoinDate, e.JoinDate) AS JoinDate, e.LeaveBalance, e.PrevLeaveBalance, e.Status, e.Experience, e.ExperienceIn, e.Qualification, ee.StartDate AS CurrentEngStartDate FROM Employees e LEFT JOIN EmployeeEngagements ee ON e.CurrentEngagementId = ee.Id WHERE e.Status IN ('Active', 'Upgraded', 'Downgraded', 'ContractEnded', 'Transferred')";
            }
            else
            {
                query = "SELECT e.MasterId, e.ID, e.Name, e.Department, e.Category, NVL(e.OriginalJoinDate, e.JoinDate) AS JoinDate, e.LeaveBalance, e.PrevLeaveBalance, e.Status, e.Experience, e.ExperienceIn, e.Qualification, ee.StartDate AS CurrentEngStartDate FROM Employees e LEFT JOIN EmployeeEngagements ee ON e.CurrentEngagementId = ee.Id WHERE e.Status = 'Resigned'";
            }
            
            if (filter != "All")
            {
                query += " AND e.Category = :Category";
                pList.Add(new OracleParameter("Category", filter));
            }
            if (divFilter != "All")
            {
                query += " AND e.Department = :Department";
                pList.Add(new OracleParameter("Department", divFilter));
            }
            if (!string.IsNullOrEmpty(search))
            {
                query += " AND (UPPER(e.ID) LIKE UPPER(:Search) OR UPPER(e.Name) LIKE UPPER(:Search))";
                pList.Add(new OracleParameter("Search", "%" + search + "%"));
            }

            string statusFilter = ddlFilterStatus.SelectedValue;
            if (statusFilter != "All")
            {
                if (statusFilter == "Active")
                {
                    query += " AND e.Status IN ('Active', 'Upgraded', 'Downgraded', 'Transferred')";
                }
                else
                {
                    query += " AND e.Status = :StatusFilter";
                    pList.Add(new OracleParameter("StatusFilter", statusFilter));
                }
            }
            
            query += " ORDER BY e.Department ASC, e.Name ASC";
            
            DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query, pList.ToArray());
            
            gvEmployees.DataSource = dt;
            gvEmployees.DataBind();

            // Set active states on the tab LinkButtons
            if (tabStatus == "Resigned")
            {
                btnTabActive.CssClass = "nav-link";
                btnTabResigned.CssClass = "nav-link active";
            }
            else
            {
                btnTabActive.CssClass = "nav-link active";
                btnTabResigned.CssClass = "nav-link";
            }
        }

        protected void btnSubmitBulkLeave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate amount
                float amount;
                if (!float.TryParse(txtBulkLeaveAmount.Text, out amount))
                {
                    ShowMessage("Please enter a valid numeric leave amount.", false);
                    return;
                }

                // Validate date
                DateTime effectiveDate;
                if (!DateTime.TryParse(txtBulkLeaveDate.Text, out effectiveDate))
                {
                    ShowMessage("Please enter a valid effective date.", false);
                    return;
                }

                // Validate remarks
                string remarks = txtBulkLeaveRemarks.Text.Trim();
                if (string.IsNullOrEmpty(remarks))
                {
                    ShowMessage("Please enter remarks for this bulk leave adjustment.", false);
                    return;
                }

                string selectedCat = ddlBulkLeaveCategory.SelectedValue;
                string selectedDiv = ddlBulkLeaveDivision.SelectedValue;

                // Build query to select target employees who have active engagements
                string selectSql = @"
                    SELECT e.MasterId, ee.ContractPeriodId 
                    FROM Employees e
                    JOIN EmployeeEngagements ee ON e.CurrentEngagementId = ee.Id
                    WHERE e.Status <> 'Resigned'";
                
                var parameters = new List<OracleParameter>();
                if (selectedCat != "All")
                {
                    selectSql += " AND e.Category = :Category";
                    parameters.Add(new OracleParameter("Category", selectedCat));
                }
                if (selectedDiv != "All")
                {
                    selectSql += " AND e.Department = :Division";
                    parameters.Add(new OracleParameter("Division", selectedDiv));
                }

                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), selectSql, parameters.ToArray());

                int rowsAffected = 0;
                string connStr = DBHelper.GetAttendanceDBConnection();
                foreach (DataRow row in dt.Rows)
                {
                    string empMasterId = row["MasterId"].ToString();
                    int cpId = Convert.ToInt32(row["ContractPeriodId"]);

                    // Insert record in EmployeeLeaveCredits
                    string insCredit = @"
                        INSERT INTO EmployeeLeaveCredits (EmpID, ContractPeriodId, Amount, EffectiveDate, Remarks)
                        VALUES (:EmpID, :CpId, :Amount, :EffectiveDate, :Remarks)";
                    DBHelper.ExecuteNonQuery(connStr, insCredit,
                        new OracleParameter("EmpID", empMasterId),
                        new OracleParameter("CpId", cpId),
                        new OracleParameter("Amount", amount),
                        new OracleParameter("EffectiveDate", effectiveDate),
                        new OracleParameter("Remarks", remarks));

                    // Update Employee Master balance
                    string updEmp = "UPDATE Employees SET LeaveBalance = LeaveBalance + :Amount WHERE MasterId = :EmpID";
                    DBHelper.ExecuteNonQuery(connStr, updEmp,
                        new OracleParameter("Amount", amount),
                        new OracleParameter("EmpID", empMasterId));

                    rowsAffected++;
                }

                // Log the action
                string desc = $"Bulk leave adjustment: {(amount >= 0 ? "+" : "")}{amount} days applied with Effective Date {effectiveDate:yyyy-MM-dd} to Category: '{selectedCat}', Division: '{selectedDiv}'. Rows updated: {rowsAffected}. Remarks: {remarks}";
                ActionLogger.LogAction("BULK_LEAVE", "ALL", desc, null, null);

                // Clear controls
                txtBulkLeaveAmount.Text = "";
                txtBulkLeaveDate.Text = "";
                txtBulkLeaveRemarks.Text = "";

                // Bind grid & show success
                BindGrid();
                ShowMessage($"Successfully applied leave adjustment of {amount} days (Effective: {effectiveDate:yyyy-MM-dd}) to {rowsAffected} active employees.", true);
            }
            catch (Exception ex)
            {
                ShowMessage("Error applying bulk leave adjustment: " + ex.Message, false);
            }
        }

        protected void btnResetBulkLeave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate date
                DateTime effectiveDate;
                if (!DateTime.TryParse(txtBulkLeaveDate.Text, out effectiveDate))
                {
                    ShowMessage("Please enter a valid effective date.", false);
                    return;
                }

                // Validate remarks
                string remarks = txtBulkLeaveRemarks.Text.Trim();
                if (string.IsNullOrEmpty(remarks))
                {
                    ShowMessage("Please enter remarks for this bulk leave reset.", false);
                    return;
                }

                string selectedCat = ddlBulkLeaveCategory.SelectedValue;
                string selectedDiv = ddlBulkLeaveDivision.SelectedValue;

                // Build query to select target employees who have active engagements and get their current LeaveBalance
                string selectSql = @"
                    SELECT e.MasterId, e.LeaveBalance, ee.ContractPeriodId 
                    FROM Employees e
                    JOIN EmployeeEngagements ee ON e.CurrentEngagementId = ee.Id
                    WHERE e.Status <> 'Resigned'";
                
                var parameters = new List<OracleParameter>();
                if (selectedCat != "All")
                {
                    selectSql += " AND e.Category = :Category";
                    parameters.Add(new OracleParameter("Category", selectedCat));
                }
                if (selectedDiv != "All")
                {
                    selectSql += " AND e.Department = :Division";
                    parameters.Add(new OracleParameter("Division", selectedDiv));
                }

                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), selectSql, parameters.ToArray());

                int rowsAffected = 0;
                string connStr = DBHelper.GetAttendanceDBConnection();
                foreach (DataRow row in dt.Rows)
                {
                    string empMasterId = row["MasterId"].ToString();
                    float currentBalance = Convert.ToSingle(row["LeaveBalance"]);
                    int cpId = Convert.ToInt32(row["ContractPeriodId"]);

                    // We only apply correction if current balance is not zero
                    if (Math.Abs(currentBalance) > 0.001f)
                    {
                        float amountToAdjust = -currentBalance;

                        // Insert record in EmployeeLeaveCredits
                        string insCredit = @"
                            INSERT INTO EmployeeLeaveCredits (EmpID, ContractPeriodId, Amount, EffectiveDate, Remarks)
                            VALUES (:EmpID, :CpId, :Amount, :EffectiveDate, :Remarks)";
                        DBHelper.ExecuteNonQuery(connStr, insCredit,
                            new OracleParameter("EmpID", empMasterId),
                            new OracleParameter("CpId", cpId),
                            new OracleParameter("Amount", amountToAdjust),
                            new OracleParameter("EffectiveDate", effectiveDate),
                            new OracleParameter("Remarks", remarks));

                        // Update Employee Master balance to 0
                        string updEmp = "UPDATE Employees SET LeaveBalance = 0 WHERE MasterId = :EmpID";
                        DBHelper.ExecuteNonQuery(connStr, updEmp,
                            new OracleParameter("EmpID", empMasterId));

                        rowsAffected++;
                    }
                }

                // Log the action
                string desc = $"Bulk leave reset to 0 applied with Effective Date {effectiveDate:yyyy-MM-dd} to Category: '{selectedCat}', Division: '{selectedDiv}'. Active employees adjusted: {rowsAffected}. Remarks: {remarks}";
                ActionLogger.LogAction("BULK_LEAVE_RESET", "ALL", desc, null, null);

                // Clear controls
                txtBulkLeaveAmount.Text = "";
                txtBulkLeaveDate.Text = "";
                txtBulkLeaveRemarks.Text = "";

                // Bind grid & show success
                BindGrid();
                ShowMessage($"Successfully reset leave balance to 0 for {rowsAffected} active employees (Effective: {effectiveDate:yyyy-MM-dd}).", true);
            }
            catch (Exception ex)
            {
                ShowMessage("Error resetting bulk leave: " + ex.Message, false);
            }
        }


        protected void btnAddEmployee_Click(object sender, EventArgs e)
        {
            try
            {
                string id = txtEmpID.Text.Trim();
                string name = txtEmpName.Text.Trim();
                string dept = ddlDept.SelectedValue;
                string cat = ddlCat.SelectedValue;
                string joinDate = txtJoinDate.Text;
                float l, pl;
                float leave = float.TryParse(txtLeaveBalance.Text, out l) ? l : 0;
                float prevLeave = float.TryParse(txtPrevLeaveBalance.Text, out pl) ? pl : 0;
                string phone = txtPhone.Text.Trim();
                string email = txtEmail.Text.Trim();
                string aadhar = txtAadhar.Text.Trim();
                string address = txtAddress.Text.Trim();
                string qualification = txtQualification.Text.Trim();
                float exp;
                float? experience = float.TryParse(txtExperience.Text, out exp) ? (float?)exp : null;
                string experienceIn = txtExperienceIn.Text.Trim();
                string oldId = hfEditOldID.Value;

                // Server-side fallback for rejoining employee name detail
                if (string.IsNullOrEmpty(oldId) && chkIsRejoining.Checked && !string.IsNullOrEmpty(ddlRejoiningEmployee.SelectedValue))
                {
                    ListItem selectedItem = ddlRejoiningEmployee.SelectedItem;
                    if (selectedItem != null)
                    {
                        string fallbackName = selectedItem.Attributes["data-name"];
                        if (string.IsNullOrEmpty(fallbackName))
                        {
                            string itemText = selectedItem.Text;
                            int dashIdx = itemText.IndexOf(" - ");
                            int parenIdx = itemText.LastIndexOf(" (Ex-");
                            if (dashIdx >= 0 && parenIdx > dashIdx)
                            {
                                fallbackName = itemText.Substring(dashIdx + 3, parenIdx - (dashIdx + 3)).Trim();
                            }
                        }
                        if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(fallbackName))
                        {
                            name = fallbackName;
                            txtEmpName.Text = name;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(phone))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{10}$"))
                    {
                        ShowMessage("Phone number must be exactly 10 digits.", false, "employeeModal");
                        return;
                    }
                }

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                {
                    ShowMessage("ID and Name are required", false, "employeeModal");
                    return;
                }

                string preState = null;
                string actionType = "ADD";
                string description = "";
                string targetMasterId = "";

                if (string.IsNullOrEmpty(oldId))
                {
                    string masterId = "";
                    if (chkIsRejoining.Checked && !string.IsNullOrEmpty(ddlRejoiningEmployee.SelectedValue))
                    {
                        masterId = ddlRejoiningEmployee.SelectedValue;
                        preState = ActionLogger.CaptureEmployeeState(masterId);
                        description = "Rejoined employee " + name + " (ID: " + id + ")";
                    }
                    else
                    {
                        masterId = GenerateNextMasterId();
                        description = "Registered new employee " + name + " (ID: " + id + ")";
                    }
                    targetMasterId = masterId;

                    string qCheck = "SELECT COUNT(*) FROM Employees WHERE ID = :ID AND Category = :Category AND Status IN ('Active', 'Upgraded', 'Downgraded', 'ContractEnded', 'Transferred') AND MasterId != :MasterId";
                    int count = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), qCheck, 
                        new OracleParameter("ID", id),
                        new OracleParameter("Category", cat),
                        new OracleParameter("MasterId", masterId)));
                    if (count > 0)
                    {
                        ShowMessage("Employee ID already exists in this category.", false, "employeeModal");
                        return;
                    }

                    object dbJoinDate = string.IsNullOrEmpty(joinDate) ? (object)DBNull.Value : DateTime.Parse(joinDate);

                    if (chkIsRejoining.Checked && !string.IsNullOrEmpty(ddlRejoiningEmployee.SelectedValue))
                    {
                        // Update existing resigned employee to rejoin
                        string query = @"
                            UPDATE Employees 
                            SET ID = :ID, 
                                Name = :Name, 
                                Department = :Dept, 
                                Category = :Cat, 
                                OriginalJoinDate = NVL(OriginalJoinDate, JoinDate),
                                JoinDate = :JoinDate, 
                                LeaveBalance = :Leave, 
                                PrevLeaveBalance = :PrevLeave, 
                                Status = 'ContractEnded', 
                                ResignDate = NULL, 
                                ContractEndDate = NULL, 
                                CurrentEngagementId = NULL,
                                Phone = :Phone,
                                Email = :Email,
                                Aadhar = :Aadhar,
                                Address = :Address,
                                Qualification = :Qualification,
                                Experience = :Experience,
                                ExperienceIn = :ExperienceIn
                            WHERE MasterId = :MasterId";
                        OracleParameter[] p = new OracleParameter[] {
                            new OracleParameter("ID", id),
                            new OracleParameter("Name", name),
                            new OracleParameter("Dept", dept),
                            new OracleParameter("Cat", cat),
                            new OracleParameter("JoinDate", dbJoinDate),
                            new OracleParameter("Leave", leave),
                            new OracleParameter("PrevLeave", prevLeave),
                            new OracleParameter("Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone),
                            new OracleParameter("Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                            new OracleParameter("Aadhar", string.IsNullOrEmpty(aadhar) ? (object)DBNull.Value : aadhar),
                            new OracleParameter("Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address),
                            new OracleParameter("Qualification", string.IsNullOrEmpty(qualification) ? (object)DBNull.Value : qualification),
                            new OracleParameter("Experience", experience.HasValue ? (object)experience.Value : DBNull.Value),
                            new OracleParameter("ExperienceIn", string.IsNullOrEmpty(experienceIn) ? (object)DBNull.Value : experienceIn),
                            new OracleParameter("MasterId", masterId)
                        };
                        DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), query, p);
                    }
                    else
                    {
                        // Insert new employee
                        string query = "INSERT INTO Employees (MasterId, ID, Name, Department, Category, JoinDate, OriginalJoinDate, LeaveBalance, PrevLeaveBalance, Status, Phone, Email, Aadhar, Address, Qualification, Experience, ExperienceIn) VALUES (:MasterId, :ID, :Name, :Dept, :Cat, :JoinDate, :OriginalJoinDate, :Leave, :PrevLeave, 'ContractEnded', :Phone, :Email, :Aadhar, :Address, :Qualification, :Experience, :ExperienceIn)";
                        OracleParameter[] p = new OracleParameter[] {
                            new OracleParameter("MasterId", masterId),
                            new OracleParameter("ID", id),
                            new OracleParameter("Name", name),
                            new OracleParameter("Dept", dept),
                            new OracleParameter("Cat", cat),
                            new OracleParameter("JoinDate", dbJoinDate),
                            new OracleParameter("OriginalJoinDate", dbJoinDate),
                            new OracleParameter("Leave", leave),
                            new OracleParameter("PrevLeave", prevLeave),
                            new OracleParameter("Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone),
                            new OracleParameter("Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                            new OracleParameter("Aadhar", string.IsNullOrEmpty(aadhar) ? (object)DBNull.Value : aadhar),
                            new OracleParameter("Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address),
                            new OracleParameter("Qualification", string.IsNullOrEmpty(qualification) ? (object)DBNull.Value : qualification),
                            new OracleParameter("Experience", experience.HasValue ? (object)experience.Value : DBNull.Value),
                            new OracleParameter("ExperienceIn", string.IsNullOrEmpty(experienceIn) ? (object)DBNull.Value : experienceIn)
                        };
                        DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), query, p);
                    }

                    AutoEnrollActiveContract(masterId, id, cat, dept, string.IsNullOrEmpty(joinDate) ? (DateTime?)null : DateTime.Parse(joinDate));
                    SyncIndividualLeaveCredits(DBHelper.GetAttendanceDBConnection(), masterId, leave, prevLeave);

                    // Log addition/rejoining
                    string postState = ActionLogger.CaptureEmployeeState(targetMasterId);
                    ActionLogger.LogAction(actionType, targetMasterId, description, preState, postState);

                    ShowMessage("Employee added successfully with Master ID " + masterId + ".", true);
                }
                else
                {
                    // UPDATE MODE
                    string oldMasterId = oldId; // oldId contains MasterId when editing
                    targetMasterId = oldMasterId;
                    preState = ActionLogger.CaptureEmployeeState(targetMasterId);
                    description = "Updated employee " + name + " (ID: " + id + ")";
                    actionType = "EDIT";
                    
                    string currentDeptQuery = "SELECT Department, CurrentEngagementId FROM Employees WHERE MasterId = :MasterId";
                    DataTable dtCurrent = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), currentDeptQuery, new OracleParameter("MasterId", oldMasterId));
                    if (dtCurrent.Rows.Count > 0)
                    {
                        DataRow drCurrent = dtCurrent.Rows[0];
                        bool hasActiveEng = drCurrent["CurrentEngagementId"] != DBNull.Value;
                        if (hasActiveEng)
                        {
                            dept = drCurrent["Department"].ToString();
                        }
                    }

                    string qCheck = "SELECT COUNT(*) FROM Employees WHERE ID = :ID AND Category = :Category AND Status IN ('Active', 'Upgraded', 'Downgraded', 'ContractEnded', 'Transferred') AND MasterId != :MasterId";
                    int count = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), qCheck, 
                        new OracleParameter("ID", id),
                        new OracleParameter("Category", cat),
                        new OracleParameter("MasterId", oldMasterId)));
                    if (count > 0)
                    {
                        ShowMessage("New Employee ID is already present in this category.", false, "employeeModal");
                        return;
                    }

                    string updateQuery = "UPDATE Employees SET ID = :ID, Name = :Name, Department = :Dept, Category = :Cat, OriginalJoinDate = :OriginalJoinDate, JoinDate = CASE WHEN CurrentEngagementId IS NULL THEN :JoinDate ELSE JoinDate END, LeaveBalance = :Leave, PrevLeaveBalance = :PrevLeave, Phone = :Phone, Email = :Email, Aadhar = :Aadhar, Address = :Address, Qualification = :Qualification, Experience = :Experience, ExperienceIn = :ExperienceIn WHERE MasterId = :MasterId";
                    object dbJoinDate = string.IsNullOrEmpty(joinDate) ? (object)DBNull.Value : DateTime.Parse(joinDate);
                    OracleParameter[] pUpdate = new OracleParameter[] {
                        new OracleParameter("ID", id),
                        new OracleParameter("Name", name),
                        new OracleParameter("Dept", dept),
                        new OracleParameter("Cat", cat),
                        new OracleParameter("OriginalJoinDate", dbJoinDate),
                        new OracleParameter("JoinDate", dbJoinDate),
                        new OracleParameter("Leave", leave),
                        new OracleParameter("PrevLeave", prevLeave),
                        new OracleParameter("Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone),
                        new OracleParameter("Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                        new OracleParameter("Aadhar", string.IsNullOrEmpty(aadhar) ? (object)DBNull.Value : aadhar),
                        new OracleParameter("Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address),
                        new OracleParameter("Qualification", string.IsNullOrEmpty(qualification) ? (object)DBNull.Value : qualification),
                        new OracleParameter("Experience", experience.HasValue ? (object)experience.Value : DBNull.Value),
                        new OracleParameter("ExperienceIn", string.IsNullOrEmpty(experienceIn) ? (object)DBNull.Value : experienceIn),
                        new OracleParameter("MasterId", oldMasterId)
                    };
                    DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), updateQuery, pUpdate);
                    SyncIndividualLeaveCredits(DBHelper.GetAttendanceDBConnection(), oldMasterId, leave, prevLeave);

                    // Log edit
                    string postState = ActionLogger.CaptureEmployeeState(targetMasterId);
                    ActionLogger.LogAction(actionType, targetMasterId, description, preState, postState);

                    ShowMessage("Employee updated successfully.", true);
                }

                ResetForm();
                BindResignedEmployees();
                PopulateDeleteEmployeeDropdown();
                BindGrid();
            }
            catch(Exception ex)
            {
                ShowMessage("Error: " + ex.Message, false, "employeeModal");
            }
        }

        protected void btnImport_Click(object sender, EventArgs e)
        {
            if (fileCSV.HasFile || !string.IsNullOrEmpty(hfImportData.Value))
            {
                try
                {
                    string cat = ddlImportCat.SelectedValue;
                    int currentNextMaster = 10001;
                    try
                    {
                        string maxQuery = "SELECT MasterId FROM Employees WHERE MasterId IS NOT NULL ORDER BY MasterId DESC";
                        DataTable dtMax = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), maxQuery);
                        foreach (DataRow row in dtMax.Rows)
                        {
                            string mId = row["MasterId"].ToString();
                            int val;
                            if (int.TryParse(mId, out val))
                            {
                                if (val >= currentNextMaster)
                                {
                                    currentNextMaster = val + 1;
                                }
                            }
                        }
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(hfImportData.Value))
                    {
                        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        serializer.MaxJsonLength = int.MaxValue;
                        var rows = serializer.Deserialize<List<List<object>>>(hfImportData.Value);
                        if (rows != null && rows.Count > 1)
                        {
                            for (int i = 1; i < rows.Count; i++)
                            {
                                var v = rows[i];
                                if (v.Count >= 2 && v[0] != null && v[1] != null && !string.IsNullOrWhiteSpace(v[0].ToString()) && !string.IsNullOrWhiteSpace(v[1].ToString()))
                                {
                                    string id = v[0].ToString().Trim();
                                    string name = v[1].ToString().Trim();
                                    string dept = v.Count > 2 && v[2] != null ? v[2].ToString().Trim() : "GENERAL";
                                    if (string.IsNullOrWhiteSpace(dept)) dept = "GENERAL";
                                    string joinDate = v.Count > 3 && v[3] != null ? v[3].ToString().Trim() : "";
                                    float l;
                                    float leave = v.Count > 4 && v[4] != null && float.TryParse(v[4].ToString(), out l) ? l : 0;
                                    string qualification = v.Count > 5 && v[5] != null ? v[5].ToString().Trim() : "";
                                    float? experience = null;
                                    float expVal;
                                    if (v.Count > 6 && v[6] != null && float.TryParse(v[6].ToString(), out expVal))
                                    {
                                        experience = expVal;
                                    }
                                    string experienceIn = v.Count > 7 && v[7] != null ? v[7].ToString().Trim() : "";
                                    string phone = v.Count > 8 && v[8] != null ? v[8].ToString().Trim() : "";
                                    string email = v.Count > 9 && v[9] != null ? v[9].ToString().Trim() : "";
                                    string aadhar = v.Count > 10 && v[10] != null ? v[10].ToString().Trim() : "";
                                    string address = v.Count > 11 && v[11] != null ? v[11].ToString().Trim() : "";

                                    ProcessImportedRow(id, name, dept, joinDate, leave, qualification, experience, experienceIn, phone, email, aadhar, address, cat, ref currentNextMaster);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Fallback to original CSV reader
                        using (StreamReader sr = new StreamReader(fileCSV.PostedFile.InputStream))
                        {
                            string line = sr.ReadLine(); // header
                            while ((line = sr.ReadLine()) != null)
                            {
                                string[] v = line.Split(',');
                                if (v.Length >= 2 && !string.IsNullOrWhiteSpace(v[0]) && !string.IsNullOrWhiteSpace(v[1]))
                                {
                                    string id = v[0].Trim();
                                    string name = v[1].Trim();
                                    string dept = v.Length > 2 ? v[2].Trim() : "GENERAL";
                                    if (string.IsNullOrWhiteSpace(dept)) dept = "GENERAL";
                                    string joinDate = v.Length > 3 ? v[3].Trim() : "";
                                    float l;
                                    float leave = v.Length > 4 && float.TryParse(v[4], out l) ? l : 0;
                                    string qualification = v.Length > 5 ? v[5].Trim() : "";
                                    float? experience = null;
                                    float expVal;
                                    if (v.Length > 6 && float.TryParse(v[6], out expVal))
                                    {
                                        experience = expVal;
                                    }
                                    string experienceIn = v.Length > 7 ? v[7].Trim() : "";
                                    string phone = v.Length > 8 ? v[8].Trim() : "";
                                    string email = v.Length > 9 ? v[9].Trim() : "";
                                    string aadhar = v.Length > 10 ? v[10].Trim() : "";
                                    string address = v.Length > 11 ? v[11].Trim() : "";

                                    ProcessImportedRow(id, name, dept, joinDate, leave, qualification, experience, experienceIn, phone, email, aadhar, address, cat, ref currentNextMaster);
                                }
                            }
                        }
                    }

                    // Reset hidden field value after successful processing
                    hfImportData.Value = "";

                    PopulateDropdowns(); // Refresh dropdown lists with any new divisions from import
                    BindResignedEmployees();
                    PopulateDeleteEmployeeDropdown();
                    BindGrid();
                    ShowMessage("Import successful.", true);
                }
                catch(Exception ex)
                {
                    ShowMessage("Import Error: " + ex.Message, false, "importModal");
                }
            }
        }

        private void ProcessImportedRow(string id, string name, string dept, string joinDate, float leave, string qualification, float? experience, string experienceIn, string phone, string email, string aadhar, string address, string cat, ref int currentNextMaster)
        {
            // Auto-register new division if not exists in db
            if (!string.IsNullOrEmpty(dept))
            {
                string checkDiv = "SELECT Name FROM Divisions WHERE UPPER(Name) = UPPER(:Name) AND ROWNUM <= 1";
                object existingDiv = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), checkDiv, new OracleParameter("Name", dept));
                if (existingDiv != null && existingDiv != DBNull.Value)
                {
                    dept = existingDiv.ToString(); // Take exact case representation from database
                }
                else
                {
                    string insertDiv = "INSERT INTO Divisions (Name) VALUES (:Name)";
                    DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), insertDiv, new OracleParameter("Name", dept));
                }
            }

            // Resolve Category case-insensitively to matching database record if exists
            if (!string.IsNullOrEmpty(cat))
            {
                string checkCat = "SELECT Name FROM Categories WHERE UPPER(Name) = UPPER(:Name) AND ROWNUM <= 1";
                object existingCat = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), checkCat, new OracleParameter("Name", cat));
                if (existingCat != null && existingCat != DBNull.Value)
                {
                    cat = existingCat.ToString(); // Take exact case representation from database
                }
            }

            // check if exists in this category
            string qCheck = "SELECT COUNT(*) FROM Employees WHERE ID = :ID AND Category = :Category AND Status IN ('Active', 'Upgraded', 'Downgraded', 'ContractEnded', 'Transferred')";
            int count = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), qCheck, 
                new OracleParameter("ID", id),
                new OracleParameter("Category", cat)));
            if (count == 0)
            {
                string masterId = currentNextMaster.ToString();
                currentNextMaster++;

                string query = "INSERT INTO Employees (MasterId, ID, Name, Department, Category, JoinDate, OriginalJoinDate, LeaveBalance, Status, Qualification, Experience, ExperienceIn, Phone, Email, Aadhar, Address) VALUES (:MasterId, :ID, :Name, :Dept, :Cat, :JoinDate, :OriginalJoinDate, :Leave, 'ContractEnded', :Qualification, :Experience, :ExperienceIn, :Phone, :Email, :Aadhar, :Address)";
                object dbJoinDate = DBNull.Value;
                if (!string.IsNullOrEmpty(joinDate))
                {
                    DateTime parsedDate;
                    if (DateTime.TryParse(joinDate, out parsedDate))
                    {
                        dbJoinDate = parsedDate;
                    }
                }
                
                string formattedExpIn = FormatExperienceIn(experienceIn);
                
                OracleParameter[] p = new OracleParameter[] {
                    new OracleParameter("MasterId", masterId),
                    new OracleParameter("ID", id),
                    new OracleParameter("Name", name),
                    new OracleParameter("Dept", dept),
                    new OracleParameter("Cat", cat),
                    new OracleParameter("JoinDate", dbJoinDate),
                    new OracleParameter("OriginalJoinDate", dbJoinDate),
                    new OracleParameter("Leave", leave),
                    new OracleParameter("Qualification", string.IsNullOrEmpty(qualification) ? (object)DBNull.Value : qualification),
                    new OracleParameter("Experience", experience.HasValue ? (object)experience.Value : DBNull.Value),
                    new OracleParameter("ExperienceIn", string.IsNullOrEmpty(formattedExpIn) ? (object)DBNull.Value : formattedExpIn),
                    new OracleParameter("Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone),
                    new OracleParameter("Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                    new OracleParameter("Aadhar", string.IsNullOrEmpty(aadhar) ? (object)DBNull.Value : aadhar),
                    new OracleParameter("Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address)
                };
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), query, p);
                
                DateTime? enrollDate = null;
                if (dbJoinDate != DBNull.Value)
                {
                    enrollDate = (DateTime)dbJoinDate;
                }
                AutoEnrollActiveContract(masterId, id, cat, dept, enrollDate);
                SyncIndividualLeaveCredits(DBHelper.GetAttendanceDBConnection(), masterId, leave, 0);

                // Log imported employee
                string postState = ActionLogger.CaptureEmployeeState(masterId);
                ActionLogger.LogAction("ADD", masterId, "Imported employee " + name + " (ID: " + id + ")", null, postState);
            }
        }

        private string FormatExperienceIn(string expIn)
        {
            if (string.IsNullOrWhiteSpace(expIn)) return "";

            string[] lines;
            if (expIn.Contains("\n"))
            {
                lines = expIn.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (expIn.Contains(";"))
            {
                lines = expIn.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (expIn.Contains("|"))
            {
                lines = expIn.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                lines = new[] { expIn };
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    if (!line.StartsWith("-"))
                    {
                        line = "- " + line;
                    }
                    lines[i] = line;
                }
            }

            return string.Join("\r\n", lines);
        }

        protected void btnTabActive_Click(object sender, EventArgs e)
        {
            hfActiveTab.Value = "Active";
            if (ddlFilterStatus.SelectedValue == "Resigned")
            {
                ddlFilterStatus.SelectedValue = "All";
            }
            BindGrid();
        }

        protected void btnTabResigned_Click(object sender, EventArgs e)
        {
            hfActiveTab.Value = "Resigned";
            if (ddlFilterStatus.SelectedValue != "Resigned" && ddlFilterStatus.SelectedValue != "All")
            {
                ddlFilterStatus.SelectedValue = "All";
            }
            BindGrid();
        }

        protected void ddlFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected string GetActiveCount()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Employees WHERE Status IN ('Active', 'Upgraded', 'Downgraded', 'ContractEnded', 'Transferred')";
                object result = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), query);
                return result != null ? result.ToString() : "0";
            }
            catch
            {
                return "0";
            }
        }

        protected string GetResignedCount()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Employees WHERE Status = 'Resigned'";
                object result = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), query);
                return result != null ? result.ToString() : "0";
            }
            catch
            {
                return "0";
            }
        }

        protected void ddlFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void ddlFilterDiv_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ResetForm();
            ShowMessage("Edit cancelled.", true);
        }

        private void ResetForm()
        {
            txtEmpID.Text = "";
            txtEmpName.Text = "";
            txtLeaveBalance.Text = "";
            txtPrevLeaveBalance.Text = "";
            txtJoinDate.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtAadhar.Text = "";
            txtAddress.Text = "";
            txtQualification.Text = "";
            txtExperience.Text = "";
            txtExperienceIn.Text = "";
            hfEditOldID.Value = "";
            txtMasterID.Text = "(Auto-Generated)";
            chkIsRejoining.Checked = false;
            chkIsRejoining.Enabled = true;
            ddlRejoiningEmployee.Enabled = true;
            if (ddlRejoiningEmployee.Items.Count > 0)
            {
                ddlRejoiningEmployee.SelectedIndex = 0;
            }
            txtEmpID.Enabled = true;
            txtJoinDate.Enabled = true;
            ddlDept.Enabled = true;
            lblDeptHelp.Style["display"] = "none";
            ddlCat.Enabled = true;
            lblCatHelp.Style["display"] = "none";
            btnAddEmployee.Text = "Add Employee";
            btnCancelEdit.Visible = false;
        }

        private void PopulateDeleteEmployeeDropdown()
        {
            try
            {
                string query = "SELECT MasterId, ID, Name FROM Employees ORDER BY Name ASC";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
                ddlDeleteEmployee.Items.Clear();
                ddlDeleteEmployee.Items.Add(new ListItem("-- Select Employee to Delete --", ""));
                foreach (DataRow row in dt.Rows)
                {
                    string mId = row["MasterId"].ToString();
                    string id = row["ID"].ToString();
                    string name = row["Name"].ToString();
                    ddlDeleteEmployee.Items.Add(new ListItem(name + " (" + id + " - Master: " + mId + ")", mId));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error populating delete dropdown: " + ex.Message);
            }
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            string masterId = ddlDeleteEmployee.SelectedValue;
            if (string.IsNullOrEmpty(masterId))
            {
                ShowMessage("Please select an employee to delete.", false, "deleteModal");
                return;
            }

            try
            {
                // Retrieve employee name for logging
                string nameQuery = "SELECT Name, ID FROM Employees WHERE MasterId = :MasterId";
                DataTable dtName = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), nameQuery, new OracleParameter("MasterId", masterId));
                string empName = "Unknown";
                string empId = "Unknown";
                if (dtName.Rows.Count > 0)
                {
                    empName = dtName.Rows[0]["Name"].ToString();
                    empId = dtName.Rows[0]["ID"].ToString();
                }

                // Log pre-state if logging is supported
                string preState = ActionLogger.CaptureEmployeeState(masterId);

                string delOver = "DELETE FROM CalculationOverrides WHERE EmpID = :MasterId";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), delOver, new OracleParameter("MasterId", masterId));

                string delAtt = "DELETE FROM Attendance WHERE EmpID = :MasterId";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), delAtt, new OracleParameter("MasterId", masterId));
                
                string nullEng = "UPDATE Employees SET CurrentEngagementId = NULL WHERE MasterId = :MasterId";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), nullEng, new OracleParameter("MasterId", masterId));

                string delEng = "DELETE FROM EmployeeEngagements WHERE EmpID = :MasterId";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), delEng, new OracleParameter("MasterId", masterId));

                string delEmp = "DELETE FROM Employees WHERE MasterId = :MasterId";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), delEmp, new OracleParameter("MasterId", masterId));
                
                // Log action
                ActionLogger.LogAction("DELETE", masterId, "Deleted employee " + empName + " (ID: " + empId + ")", preState, null);

                PopulateDeleteEmployeeDropdown();
                BindResignedEmployees();
                BindGrid();
                
                // Clear selected value
                ddlDeleteEmployee.SelectedIndex = 0;

                ShowMessage("Employee " + empName + " and their attendance history completely deleted.", true);
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting employee: " + ex.Message, false, "deleteModal");
            }
        }

        private void BindResignedEmployees()
        {
            try
            {
                string query = @"
                    SELECT MasterId, Name, Department
                    FROM (
                        SELECT MasterId, Name, Department,
                               ROW_NUMBER() OVER (PARTITION BY MasterId ORDER BY JoinDate DESC, ID DESC) as rn
                        FROM Employees
                        WHERE Status = 'Resigned' AND MasterId IS NOT NULL
                    )
                    WHERE rn = 1
                    ORDER BY Name ASC";
                
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
                
                ddlRejoiningEmployee.Items.Clear();
                ddlRejoiningEmployee.Items.Add(new ListItem("-- Select Resigned Employee --", ""));
                
                foreach (DataRow row in dt.Rows)
                {
                    string mId = row["MasterId"].ToString();
                    string name = row["Name"].ToString();
                    string dept = row["Department"] != DBNull.Value ? row["Department"].ToString() : "N/A";
                    
                    ListItem item = new ListItem(mId + " - " + name + " (Ex-" + dept + ")", mId);
                    item.Attributes.Add("data-name", name);
                    ddlRejoiningEmployee.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading resigned employees: " + ex.Message);
            }
        }

        private string GenerateNextMasterId()
        {
            int currentNextMaster = 10001;
            try
            {
                string query = "SELECT MasterId FROM Employees WHERE MasterId IS NOT NULL ORDER BY MasterId DESC";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
                foreach (DataRow row in dt.Rows)
                {
                    string mId = row["MasterId"].ToString();
                    int val;
                    if (int.TryParse(mId, out val))
                    {
                        if (val >= currentNextMaster)
                        {
                            currentNextMaster = val + 1;
                        }
                    }
                }
            }
            catch { }
            return currentNextMaster.ToString();
        }

        private void AutoEnrollActiveContract(string masterId, string empId, string category, string department, DateTime? joinDate)
        {
            try
            {
                // Check if there is an active contract for this category
                string activeContractSql = "SELECT Id, VendorId, StartDate FROM ContractPeriods WHERE Category = :Category AND Status = 'Active'";
                DataTable dtContract = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), activeContractSql, new OracleParameter("Category", category));
                if (dtContract.Rows.Count > 0)
                {
                    int contractPeriodId = Convert.ToInt32(dtContract.Rows[0]["Id"]);
                    int vendorId = Convert.ToInt32(dtContract.Rows[0]["VendorId"]);
                    DateTime cpStartDate = Convert.ToDateTime(dtContract.Rows[0]["StartDate"]);

                    DateTime parsedJoinDate = joinDate ?? DateTime.Today;
                    if (parsedJoinDate < cpStartDate)
                    {
                        parsedJoinDate = cpStartDate;
                    }

                    using (OracleConnection conn = new OracleConnection(DBHelper.GetAttendanceDBConnection()))
                    {
                        conn.Open();
                        using (OracleTransaction trans = conn.BeginTransaction())
                        {
                            try
                            {
                                // 1. Insert new engagement
                                string insertEngSql = @"
                                    INSERT INTO EmployeeEngagements (EmpID, ContractPeriodId, Category, VendorId, Department, StartDate, EmployeeId) 
                                    VALUES (:EmpID, :ContractPeriodId, :Category, :VendorId, :Department, :StartDate, :EmployeeId)
                                    RETURNING Id INTO :NewEngagementId";

                                int newEngagementId = 0;
                                using (OracleCommand cmd = new OracleCommand(insertEngSql, conn))
                                {
                                    cmd.Transaction = trans;
                                    cmd.BindByName = true;
                                    cmd.Parameters.Add(new OracleParameter("EmpID", masterId));
                                    cmd.Parameters.Add(new OracleParameter("ContractPeriodId", contractPeriodId));
                                    cmd.Parameters.Add(new OracleParameter("Category", category));
                                    cmd.Parameters.Add(new OracleParameter("VendorId", vendorId));
                                    cmd.Parameters.Add(new OracleParameter("Department", department));
                                    cmd.Parameters.Add(new OracleParameter("StartDate", parsedJoinDate));
                                    cmd.Parameters.Add(new OracleParameter("EmployeeId", empId));

                                    OracleParameter outParam = new OracleParameter("NewEngagementId", OracleDbType.Int32);
                                    outParam.Direction = ParameterDirection.Output;
                                    cmd.Parameters.Add(outParam);

                                    cmd.ExecuteNonQuery();
                                    newEngagementId = Convert.ToInt32(outParam.Value.ToString());
                                }

                                // 2. Update Employee Master to Active and link the engagement
                                string updateEmpSql = @"
                                    UPDATE Employees 
                                    SET CurrentEngagementId = :CurrentEngagementId, 
                                        JoinDate = NVL(JoinDate, :JoinDate),
                                        OriginalJoinDate = NVL(OriginalJoinDate, :JoinDate),
                                        Status = 'Active' 
                                    WHERE MasterId = :MasterId";
                                using (OracleCommand cmd = new OracleCommand(updateEmpSql, conn))
                                {
                                    cmd.Transaction = trans;
                                    cmd.BindByName = true;
                                    cmd.Parameters.Add(new OracleParameter("CurrentEngagementId", newEngagementId));
                                    cmd.Parameters.Add(new OracleParameter("JoinDate", parsedJoinDate));
                                    cmd.Parameters.Add(new OracleParameter("MasterId", masterId));
                                    cmd.ExecuteNonQuery();
                                }

                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw ex;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error auto-enrolling employee: " + ex.Message);
            }
        }

        protected void gvEmployees_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            // Empty handler to allow GridView to fire RowCommand without error for CommandName="Delete" if used.
            // Actual delete logic will be in RowCommand.
        }

        protected void gvEmployees_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HiddenField hfStatus = (HiddenField)e.Row.FindControl("hfStatus");
                DropDownList ddlStatus = (DropDownList)e.Row.FindControl("ddlStatus");
                
                if (hfStatus != null && ddlStatus != null)
                {
                    ddlStatus.SelectedValue = hfStatus.Value;

                    foreach (ListItem item in ddlStatus.Items)
                    {
                        if (item.Value == "Active" && (hfStatus.Value == "Resigned" || hfStatus.Value == "ContractEnded"))
                        {
                            item.Enabled = false;
                        }
                        else if (item.Value == "ContractEnded" && hfStatus.Value != "ContractEnded")
                        {
                            item.Enabled = false;
                        }
                        else if ((item.Value == "Upgraded" || item.Value == "Downgraded" || item.Value == "Transferred") && hfStatus.Value == "Resigned")
                        {
                            item.Enabled = false;
                        }
                    }

                    if (hfStatus.Value == "Resigned")
                    {
                        e.Row.CssClass = "resigned-row strike";
                        ddlStatus.CssClass = "form-select form-select-sm status-badge badge-resigned";
                    }
                    else if (hfStatus.Value == "Upgraded")
                    {
                        ddlStatus.CssClass = "form-select form-select-sm status-badge badge-upgraded";
                    }
                    else if (hfStatus.Value == "Downgraded")
                    {
                        ddlStatus.CssClass = "form-select form-select-sm status-badge badge-downgraded";
                    }
                    else if (hfStatus.Value == "ContractEnded")
                    {
                        ddlStatus.CssClass = "form-select form-select-sm status-badge badge-contractended";
                    }
                    else if (hfStatus.Value == "Transferred")
                    {
                        ddlStatus.CssClass = "form-select form-select-sm status-badge badge-transferred";
                    }
                    else
                    {
                        ddlStatus.CssClass = "form-select form-select-sm status-badge badge-active";
                    }
                }

                // ── Data attributes for client-side advanced filtering ────────────
                DataRowView drv = e.Row.DataItem as DataRowView;
                if (drv != null)
                {
                    // Join date in sortable ISO format (yyyy-MM-dd)
                    string jd = "";
                    if (drv.Row.Table.Columns.Contains("JoinDate") && drv["JoinDate"] != DBNull.Value)
                        jd = Convert.ToDateTime(drv["JoinDate"]).ToString("yyyy-MM-dd");
                    e.Row.Attributes["data-joindate"] = jd;

                    // Experience in years (numeric string, empty if null)
                    string exp = "";
                    if (drv.Row.Table.Columns.Contains("Experience") && drv["Experience"] != DBNull.Value)
                        exp = drv["Experience"].ToString();
                    e.Row.Attributes["data-experience"] = exp;

                    // Experience In — lowercased for easy contains-match in JS
                    string expIn = "";
                    if (drv.Row.Table.Columns.Contains("ExperienceIn") && drv["ExperienceIn"] != DBNull.Value)
                        expIn = drv["ExperienceIn"].ToString();
                    e.Row.Attributes["data-experiencein"] = expIn;

                    // Qualification — lowercased for easy contains-match in JS
                    string qual = "";
                    if (drv.Row.Table.Columns.Contains("Qualification") && drv["Qualification"] != DBNull.Value)
                        qual = drv["Qualification"].ToString();
                    e.Row.Attributes["data-qualification"] = qual;
                }
                // ─────────────────────────────────────────────────────────────────
            }
        }

        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddl.NamingContainer;
            HiddenField hfEmpID = (HiddenField)row.FindControl("hfEmpID");
            HiddenField hfResignDate = (HiddenField)row.FindControl("hfResignDate");
 
            string id = hfEmpID.Value;
            string status = ddl.SelectedValue;

            // Capture pre-state
            string preState = ActionLogger.CaptureEmployeeState(id);

            // Check if the employee is currently resigned in the database
            string currentStatus = "";
            string queryCurrentStatus = "SELECT Status FROM Employees WHERE MasterId = :MasterId";
            DataTable dtStatus = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), queryCurrentStatus, new OracleParameter("MasterId", id));
            if (dtStatus.Rows.Count > 0)
            {
                currentStatus = dtStatus.Rows[0]["Status"].ToString();
            }

            if (currentStatus == "Resigned" && (status == "Upgraded" || status == "Downgraded" || status == "Transferred"))
            {
                ShowMessage("A resigned employee cannot be upgraded, downgraded, or transferred. They must rejoin first.", false);
                BindGrid();
                return;
            }
 
            if (status == "Active" || status == "ContractEnded")
            {
                bool isTransitioned = false;
                if (preState != null)
                {
                    isTransitioned = preState.IndexOf("\"STATUS\":\"Upgraded\"", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     preState.IndexOf("\"STATUS\":\"Downgraded\"", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     preState.IndexOf("\"STATUS\":\"Transferred\"", StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (status == "Active" && isTransitioned)
                {
                    // Allow manual reset to Active for transitioned employees
                }
                else
                {
                    ShowMessage("Status cannot be manually set to Active or Contract Ended.", false);
                    BindGrid();
                    return;
                }
            }
 
            if (status == "Upgraded" || status == "Downgraded")
            {
                string newCategory = hfChangeCategory.Value;
                string changeDateStr = hfChangeDate.Value;
                string newEmpId = hfChangeEmpId.Value.Trim();
 
                if (string.IsNullOrEmpty(newCategory) || string.IsNullOrEmpty(changeDateStr) || string.IsNullOrEmpty(newEmpId))
                {
                    ShowMessage("Category change parameters or New Employee ID are missing. Transition cancelled.", false);
                    BindGrid();
                    return;
                }
 
                DateTime changeDate;
                if (!DateTime.TryParse(changeDateStr, out changeDate))
                {
                    ShowMessage("Invalid change date: " + changeDateStr, false);
                    BindGrid();
                    return;
                }
 
                DateTime endDateForOldEngagement = changeDate.AddDays(-1);
 
                using (OracleConnection conn = new OracleConnection(DBHelper.GetAttendanceDBConnection()))
                {
                    conn.Open();
                    using (OracleTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // Validate that the new Employee ID is unique within the target category
                            string qCheck = "SELECT COUNT(*) FROM Employees WHERE ID = :ID AND Category = :Category AND Status IN ('Active', 'Upgraded', 'Downgraded', 'ContractEnded', 'Transferred') AND MasterId != :MasterId";
                            using (OracleCommand cmd = new OracleCommand(qCheck, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.BindByName = true;
                                cmd.Parameters.Add(new OracleParameter("ID", newEmpId));
                                cmd.Parameters.Add(new OracleParameter("Category", newCategory));
                                cmd.Parameters.Add(new OracleParameter("MasterId", id));
                                int count = Convert.ToInt32(cmd.ExecuteScalar());
                                if (count > 0)
                                {
                                    throw new Exception("New Employee ID '" + newEmpId + "' is already present in category '" + newCategory + "'.");
                                }
                            }

                            // 1. Fetch employee details
                            string empDetailsSql = @"
                                SELECT e.Department, e.Category, e.CurrentEngagementId, ee.StartDate AS CurrentEngStartDate, ee.VendorId 
                                FROM Employees e 
                                LEFT JOIN EmployeeEngagements ee ON e.CurrentEngagementId = ee.Id 
                                WHERE e.MasterId = :MasterId";
                            string empDept = "GENERAL";
                            string oldCategory = "";
                            int? oldEngagementId = null;
                            DateTime? currentEngStartDate = null;
                            int? oldVendorId = null;
 
                            using (OracleCommand cmd = new OracleCommand(empDetailsSql, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.Parameters.Add(new OracleParameter("MasterId", id));
                                using (OracleDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        empDept = reader["Department"] != DBNull.Value ? reader["Department"].ToString() : "GENERAL";
                                        oldCategory = reader["Category"]?.ToString();
                                        if (reader["CurrentEngagementId"] != DBNull.Value)
                                        {
                                            oldEngagementId = Convert.ToInt32(reader["CurrentEngagementId"]);
                                        }
                                        if (reader["CurrentEngStartDate"] != DBNull.Value)
                                        {
                                            currentEngStartDate = Convert.ToDateTime(reader["CurrentEngStartDate"]);
                                        }
                                        if (reader["VendorId"] != DBNull.Value)
                                        {
                                            oldVendorId = Convert.ToInt32(reader["VendorId"]);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Employee record not found.");
                                    }
                                }
                            }

                             // Fallback: If no current active engagement (e.g. contract ended), query the last closed engagement
                             if (!oldEngagementId.HasValue)
                             {
                                 string lastEngSql = @"
                                     SELECT Id, StartDate, VendorId FROM (
                                         SELECT Id, StartDate, VendorId FROM EmployeeEngagements 
                                         WHERE EmpID = :MasterId 
                                         ORDER BY StartDate DESC, Id DESC
                                     ) WHERE ROWNUM = 1";
                                 using (OracleCommand cmd = new OracleCommand(lastEngSql, conn))
                                 {
                                     cmd.Transaction = trans;
                                     cmd.BindByName = true;
                                     cmd.Parameters.Add(new OracleParameter("MasterId", id));
                                     using (OracleDataReader reader = cmd.ExecuteReader())
                                     {
                                         if (reader.Read())
                                         {
                                             oldEngagementId = Convert.ToInt32(reader["Id"]);
                                             currentEngStartDate = Convert.ToDateTime(reader["StartDate"]);
                                             if (reader["VendorId"] != DBNull.Value)
                                             {
                                                 oldVendorId = Convert.ToInt32(reader["VendorId"]);
                                             }
                                         }
                                     }
                                 }
                             }
 
                            // Validate transition date
                            if (currentEngStartDate.HasValue && changeDate <= currentEngStartDate.Value)
                            {
                                throw new Exception("Transition date (" + changeDate.ToString("yyyy-MM-dd") + ") must be after the current engagement start date (" + currentEngStartDate.Value.ToString("yyyy-MM-dd") + ").");
                            }
 
                            // 2. Fetch the active contract period for the new category
                            string contractSql = "SELECT Id, VendorId, StartDate FROM ContractPeriods WHERE Category = :Category AND Status = 'Active'";
                            int? newPeriodId = null;
                            int? newVendorId = null;
                            DateTime? cpStartDate = null;
                            string finalStatus = "Active";

                            using (OracleCommand cmd = new OracleCommand(contractSql, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.Parameters.Add(new OracleParameter("Category", newCategory));
                                using (OracleDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        newPeriodId = Convert.ToInt32(reader["Id"]);
                                        newVendorId = Convert.ToInt32(reader["VendorId"]);
                                        if (reader["StartDate"] != DBNull.Value)
                                        {
                                            cpStartDate = Convert.ToDateTime(reader["StartDate"]);
                                        }
                                    }
                                    else
                                    {
                                        // Allow transition even when no active contract exists.
                                        // The new engagement will have ContractPeriodId = null (meaning no contract).
                                        // Vendor is carried over from the employee's previous engagement.
                                        if (oldVendorId.HasValue)
                                        {
                                            newPeriodId = null;
                                            newVendorId = oldVendorId.Value;
                                            finalStatus = "ContractEnded";
                                        }
                                        else
                                        {
                                            throw new Exception("No active contract period configured for category '" + newCategory + "' and no previous vendor ID could be resolved.");
                                        }
                                    }
                                }
                            }
 
                            DateTime targetEngStartDate = changeDate;
                            if (cpStartDate.HasValue && changeDate < cpStartDate.Value)
                            {
                                targetEngStartDate = cpStartDate.Value;
                            }
 
                            // 3. Close the previous active engagement
                            if (oldEngagementId.HasValue)
                            {
                                string closeOldEngSql = @"
                                    UPDATE EmployeeEngagements 
                                    SET EndDate = :EndDate, EndReason = :EndReason 
                                    WHERE Id = :OldEngagementId AND EndDate IS NULL";
                                
                                using (OracleCommand cmd = new OracleCommand(closeOldEngSql, conn))
                                {
                                    cmd.Transaction = trans;
                                    cmd.BindByName = true;
                                    cmd.Parameters.Add(new OracleParameter("EndDate", endDateForOldEngagement));
                                    cmd.Parameters.Add(new OracleParameter("EndReason", status));
                                    cmd.Parameters.Add(new OracleParameter("OldEngagementId", oldEngagementId.Value));
                                    cmd.ExecuteNonQuery();
                                }
                            }
 
                            // 4. Insert the new engagement
                            string insertEngSql = @"
                                INSERT INTO EmployeeEngagements (EmpID, ContractPeriodId, Category, VendorId, Department, StartDate, IsCarriedOver, PrevEngagementId, EmployeeId) 
                                VALUES (:EmpID, :ContractPeriodId, :Category, :VendorId, :Department, :StartDate, 1, :PrevEngagementId, :EmployeeId)
                                RETURNING Id INTO :NewEngagementId";
                            
                            int newEngagementId = 0;
                            using (OracleCommand cmd = new OracleCommand(insertEngSql, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.BindByName = true;
                                cmd.Parameters.Add(new OracleParameter("EmpID", id));
                                cmd.Parameters.Add(new OracleParameter("ContractPeriodId", (object)newPeriodId ?? DBNull.Value));
                                cmd.Parameters.Add(new OracleParameter("Category", newCategory));
                                cmd.Parameters.Add(new OracleParameter("VendorId", (object)newVendorId ?? DBNull.Value));
                                cmd.Parameters.Add(new OracleParameter("Department", empDept));
                                cmd.Parameters.Add(new OracleParameter("StartDate", targetEngStartDate));
                                cmd.Parameters.Add(new OracleParameter("PrevEngagementId", oldEngagementId.HasValue ? (object)oldEngagementId.Value : DBNull.Value));
                                cmd.Parameters.Add(new OracleParameter("EmployeeId", newEmpId));
 
                                OracleParameter outParam = new OracleParameter("NewEngagementId", OracleDbType.Int32);
                                outParam.Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(outParam);
 
                                cmd.ExecuteNonQuery();
                                newEngagementId = Convert.ToInt32(outParam.Value.ToString());
                            }
 
                            // 5. Update the master record in Employees
                            string updateEmpSql = @"
                                UPDATE Employees 
                                SET ID = :NewEmpID,
                                    CurrentEngagementId = :CurrentEngagementId, 
                                    Category = :Category, 
                                    Status = :Status,
                                    ResignDate = NULL,
                                    ContractEndDate = NULL 
                                WHERE MasterId = :MasterId";
                            
                            using (OracleCommand cmd = new OracleCommand(updateEmpSql, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.BindByName = true;
                                cmd.Parameters.Add(new OracleParameter("NewEmpID", newEmpId));
                                cmd.Parameters.Add(new OracleParameter("CurrentEngagementId", newEngagementId));
                                cmd.Parameters.Add(new OracleParameter("Category", newCategory));
                                cmd.Parameters.Add(new OracleParameter("Status", finalStatus));
                                cmd.Parameters.Add(new OracleParameter("MasterId", id));
                                cmd.ExecuteNonQuery();
                            }
 
                            trans.Commit();

                             // Capture post-state and log the status change
                             try
                             {
                                 string postState = ActionLogger.CaptureEmployeeState(id);
                                 string actType = status.ToUpper();
                                 string desc = status + " employee to category " + newCategory + " (New ID: " + newEmpId + ")";
                                 ActionLogger.LogAction(actType, id, desc, preState, postState);
                             }
                             catch (Exception exLog)
                             {
                                 System.Diagnostics.Debug.WriteLine("Error logging status change: " + exLog.Message);
                             }

                            ShowMessage("Employee status successfully changed to " + status + ". Category transitioned from " + oldCategory + " to " + newCategory + " and vendor reassigned.", true);
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            if (ex.Message.Contains("already present") || ex.Message.Contains("Transition date") || ex.Message.Contains("configured"))
                            {
                                ShowMessage(ex.Message, false);
                            }
                            else
                            {
                                ShowMessage("Database error during status change: " + ex.Message, false);
                            }
                        }
                    }
                }
            }
            else if (status == "Transferred")
            {
                string newDivision = hfChangeDivision.Value;
                string changeDateStr = hfChangeDate.Value;

                if (string.IsNullOrEmpty(newDivision) || string.IsNullOrEmpty(changeDateStr))
                {
                    ShowMessage("Division change parameters are missing. Transfer cancelled.", false);
                    BindResignedEmployees();
                    BindGrid();
                    return;
                }

                DateTime changeDate;
                if (!DateTime.TryParse(changeDateStr, out changeDate))
                {
                    ShowMessage("Invalid change date: " + changeDateStr, false);
                    BindResignedEmployees();
                    BindGrid();
                    return;
                }

                DateTime endDateForOldEngagement = changeDate.AddDays(-1);

                using (OracleConnection conn = new OracleConnection(DBHelper.GetAttendanceDBConnection()))
                {
                    conn.Open();
                    using (OracleTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Fetch employee details
                            string empDetailsSql = @"
                                SELECT e.ID, e.Name, e.Department, e.Category, e.CurrentEngagementId, ee.StartDate AS CurrentEngStartDate,
                                       ee.ContractPeriodId, ee.VendorId 
                                FROM Employees e 
                                LEFT JOIN EmployeeEngagements ee ON e.CurrentEngagementId = ee.Id 
                                WHERE e.MasterId = :MasterId";
                            
                            string empId = "";
                            string empName = "";
                            string oldDivision = "";
                            string empCategory = "";
                            int? oldEngagementId = null;
                            DateTime? currentEngStartDate = null;
                            int? contractPeriodId = null;
                            int? vendorId = null;

                            using (OracleCommand cmd = new OracleCommand(empDetailsSql, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.Parameters.Add(new OracleParameter("MasterId", id));
                                using (OracleDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        empId = reader["ID"].ToString();
                                        empName = reader["Name"].ToString();
                                        oldDivision = reader["Department"] != DBNull.Value ? reader["Department"].ToString() : "";
                                        empCategory = reader["Category"].ToString();
                                        if (reader["CurrentEngagementId"] != DBNull.Value)
                                        {
                                            oldEngagementId = Convert.ToInt32(reader["CurrentEngagementId"]);
                                        }
                                        if (reader["CurrentEngStartDate"] != DBNull.Value)
                                        {
                                            currentEngStartDate = Convert.ToDateTime(reader["CurrentEngStartDate"]);
                                        }
                                        if (reader["ContractPeriodId"] != DBNull.Value)
                                        {
                                            contractPeriodId = Convert.ToInt32(reader["ContractPeriodId"]);
                                        }
                                        if (reader["VendorId"] != DBNull.Value)
                                        {
                                            vendorId = Convert.ToInt32(reader["VendorId"]);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Employee record not found.");
                                    }
                                }
                            }

                            // Validate transition date
                            if (currentEngStartDate.HasValue && changeDate <= currentEngStartDate.Value)
                            {
                                throw new Exception("Transfer date (" + changeDate.ToString("yyyy-MM-dd") + ") must be after the current engagement start date (" + currentEngStartDate.Value.ToString("yyyy-MM-dd") + ").");
                            }

                            if (!oldEngagementId.HasValue)
                            {
                                throw new Exception("Employee does not have an active contract engagement to transfer.");
                            }
 
                            DateTime? cpStartDate = null;
                            if (contractPeriodId.HasValue)
                            {
                                string cpSql = "SELECT StartDate FROM ContractPeriods WHERE Id = :Id";
                                using (OracleCommand cmd = new OracleCommand(cpSql, conn))
                                {
                                    cmd.Transaction = trans;
                                    cmd.Parameters.Add(new OracleParameter("Id", contractPeriodId.Value));
                                    object res = cmd.ExecuteScalar();
                                    if (res != null && res != DBNull.Value)
                                    {
                                        cpStartDate = Convert.ToDateTime(res);
                                    }
                                }
                            }
                            DateTime targetEngStartDate = changeDate;
                            if (cpStartDate.HasValue && changeDate < cpStartDate.Value)
                            {
                                targetEngStartDate = cpStartDate.Value;
                            }
 
                            // 2. Close the previous active engagement
                            string closeOldEngSql = @"
                                UPDATE EmployeeEngagements 
                                SET EndDate = :EndDate, EndReason = 'Transferred' 
                                WHERE Id = :OldEngagementId AND EndDate IS NULL";
                            
                            using (OracleCommand cmd = new OracleCommand(closeOldEngSql, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.BindByName = true;
                                cmd.Parameters.Add(new OracleParameter("EndDate", endDateForOldEngagement));
                                cmd.Parameters.Add(new OracleParameter("OldEngagementId", oldEngagementId.Value));
                                cmd.ExecuteNonQuery();
                            }
 
                            // 3. Insert the new engagement in the new department/division
                            string insertEngSql = @"
                                INSERT INTO EmployeeEngagements (EmpID, ContractPeriodId, Category, VendorId, Department, StartDate, IsCarriedOver, PrevEngagementId, EmployeeId) 
                                VALUES (:EmpID, :ContractPeriodId, :Category, :VendorId, :Department, :StartDate, 1, :PrevEngagementId, :EmployeeId)
                                RETURNING Id INTO :NewEngagementId";
                            
                            int newEngagementId = 0;
                            using (OracleCommand cmd = new OracleCommand(insertEngSql, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.BindByName = true;
                                cmd.Parameters.Add(new OracleParameter("EmpID", id));
                                cmd.Parameters.Add(new OracleParameter("ContractPeriodId", contractPeriodId ?? 0));
                                cmd.Parameters.Add(new OracleParameter("Category", empCategory));
                                cmd.Parameters.Add(new OracleParameter("VendorId", vendorId ?? 0));
                                cmd.Parameters.Add(new OracleParameter("Department", newDivision));
                                cmd.Parameters.Add(new OracleParameter("StartDate", targetEngStartDate));
                                cmd.Parameters.Add(new OracleParameter("PrevEngagementId", oldEngagementId.Value));
                                cmd.Parameters.Add(new OracleParameter("EmployeeId", empId));

                                OracleParameter outParam = new OracleParameter("NewEngagementId", OracleDbType.Int32);
                                outParam.Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(outParam);

                                cmd.ExecuteNonQuery();
                                newEngagementId = Convert.ToInt32(outParam.Value.ToString());
                            }

                            // 4. Update the master record in Employees
                            string updateEmpSql = @"
                                UPDATE Employees 
                                SET Department = :Dept,
                                    CurrentEngagementId = :CurrentEngagementId,
                                    Status = 'Active',
                                    ResignDate = NULL,
                                    ContractEndDate = NULL 
                                WHERE MasterId = :MasterId";
                            
                            using (OracleCommand cmd = new OracleCommand(updateEmpSql, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.BindByName = true;
                                cmd.Parameters.Add(new OracleParameter("Dept", newDivision));
                                cmd.Parameters.Add(new OracleParameter("CurrentEngagementId", newEngagementId));
                                cmd.Parameters.Add(new OracleParameter("MasterId", id));
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();

                             // Capture post-state and log the division transfer
                             try
                             {
                                 string postState = ActionLogger.CaptureEmployeeState(id);
                                 string desc = "Transferred employee division to " + newDivision;
                                 ActionLogger.LogAction("TRANSFER", id, desc, preState, postState);
                             }
                             catch (Exception exLog)
                             {
                                 System.Diagnostics.Debug.WriteLine("Error logging division transfer: " + exLog.Message);
                             }

                            ShowMessage("Employee division successfully changed to " + newDivision + " and recorded in history.", true);
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            ShowMessage("Error performing transfer: " + ex.Message, false);
                        }
                    }
                }
            }
            else
            {
                // Active, Resigned or ContractEnded
                string resignDateQuery = status == "Resigned" ? ", ResignDate = :ResignDate, ContractEndDate = NULL" : ", ResignDate = NULL, ContractEndDate = NULL";
                object dbDate = DBNull.Value;
                DateTime changeDate = DateTime.Now;
                if (status == "Resigned")
                {
                    DateTime dt;
                    if (DateTime.TryParse(hfResignDate.Value, out dt))
                    {
                        dbDate = dt;
                        changeDate = dt;
                    }
                    else
                    {
                        dbDate = DateTime.Now;
                        changeDate = DateTime.Now;
                    }
                }
                else if (status == "ContractEnded")
                {
                    changeDate = DateTime.Today;
                }

                using (OracleConnection conn = new OracleConnection(DBHelper.GetAttendanceDBConnection()))
                {
                    conn.Open();
                    using (OracleTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            if (status == "Resigned" || status == "ContractEnded")
                            {
                                string getEngSql = "SELECT CurrentEngagementId FROM Employees WHERE MasterId = :MasterId";
                                int? currentEngId = null;
                                using (OracleCommand cmd = new OracleCommand(getEngSql, conn))
                                {
                                    cmd.Transaction = trans;
                                    cmd.Parameters.Add(new OracleParameter("MasterId", id));
                                    object res = cmd.ExecuteScalar();
                                    if (res != null && res != DBNull.Value)
                                    {
                                        currentEngId = Convert.ToInt32(res);
                                    }
                                }

                                if (currentEngId.HasValue)
                                {
                                    string closeSql = @"
                                        UPDATE EmployeeEngagements 
                                        SET EndDate = :EndDate, EndReason = :EndReason 
                                        WHERE Id = :EngagementId AND EndDate IS NULL";
                                    using (OracleCommand cmd = new OracleCommand(closeSql, conn))
                                    {
                                        cmd.Transaction = trans;
                                        cmd.BindByName = true;
                                        cmd.Parameters.Add(new OracleParameter("EndDate", changeDate));
                                        cmd.Parameters.Add(new OracleParameter("EndReason", status));
                                        cmd.Parameters.Add(new OracleParameter("EngagementId", currentEngId.Value));
                                        cmd.ExecuteNonQuery();
                                    }

                                    string clearEngSql = "UPDATE Employees SET CurrentEngagementId = NULL WHERE MasterId = :MasterId";
                                    using (OracleCommand cmd = new OracleCommand(clearEngSql, conn))
                                    {
                                        cmd.Transaction = trans;
                                        cmd.Parameters.Add(new OracleParameter("MasterId", id));
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            string updateSql = "UPDATE Employees SET Status = :Status " + resignDateQuery + " WHERE MasterId = :MasterId";
                            using (OracleCommand cmd = new OracleCommand(updateSql, conn))
                            {
                                cmd.Transaction = trans;
                                cmd.BindByName = true;
                                cmd.Parameters.Add(new OracleParameter("Status", status));
                                cmd.Parameters.Add(new OracleParameter("ResignDate", dbDate));
                                cmd.Parameters.Add(new OracleParameter("MasterId", id));
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();

                             // Capture post-state and log status update
                             try
                             {
                                 string postState = ActionLogger.CaptureEmployeeState(id);
                                 string actType = status.ToUpper();
                                 string desc = "Changed employee status to " + status;
                                 ActionLogger.LogAction(actType, id, desc, preState, postState);
                             }
                             catch (Exception exLog)
                             {
                                 System.Diagnostics.Debug.WriteLine("Error logging status change: " + exLog.Message);
                             }

                            ShowMessage("Employee status successfully updated to " + status + ".", true);
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            ShowMessage("Error updating employee status: " + ex.Message, false);
                        }
                    }
                }
            }
 
            BindResignedEmployees();
            BindGrid();
        }

        protected void gvEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditEmp")
            {
                string masterId = e.CommandArgument.ToString();
                string query = "SELECT * FROM Employees WHERE MasterId = :MasterId";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query, new OracleParameter("MasterId", masterId));
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    hfEditOldID.Value = dr["MasterId"].ToString();
                    txtEmpID.Text = dr["ID"].ToString();
                    txtEmpID.Enabled = false;
                    txtJoinDate.Enabled = false;
                    txtEmpName.Text = dr["Name"].ToString();
                    txtMasterID.Text = dr["MasterId"]?.ToString() ?? "";
                    
                    chkIsRejoining.Checked = false;
                    chkIsRejoining.Enabled = false;
                    ddlRejoiningEmployee.Enabled = false;

                    string deptValue = dr["Department"].ToString();
                    if (ddlDept.Items.FindByValue(deptValue) != null)
                    {
                        ddlDept.SelectedValue = deptValue;
                    }
                    else
                    {
                        ddlDept.Items.Add(new ListItem(deptValue, deptValue));
                        ddlDept.SelectedValue = deptValue;
                    }

                    bool hasActiveEng = dr["CurrentEngagementId"] != DBNull.Value;
                    ddlDept.Enabled = !hasActiveEng;
                    lblDeptHelp.Style["display"] = hasActiveEng ? "block" : "none";
                    ddlCat.Enabled = !hasActiveEng;
                    lblCatHelp.Style["display"] = hasActiveEng ? "block" : "none";

                    string catValue = dr["Category"].ToString();
                    if (ddlCat.Items.FindByValue(catValue) != null)
                    {
                        ddlCat.SelectedValue = catValue;
                    }
                    else
                    {
                        ddlCat.Items.Add(new ListItem(catValue, catValue));
                        ddlCat.SelectedValue = catValue;
                    }

                    object oJoinDate = dr["OriginalJoinDate"] != DBNull.Value ? dr["OriginalJoinDate"] : dr["JoinDate"];
                    txtJoinDate.Text = oJoinDate != DBNull.Value ? Convert.ToDateTime(oJoinDate).ToString("yyyy-MM-dd") : "";
                    txtLeaveBalance.Text = dr["LeaveBalance"].ToString();
                    txtPrevLeaveBalance.Text = dr["PrevLeaveBalance"] != DBNull.Value ? dr["PrevLeaveBalance"].ToString() : "0";
                    txtPhone.Text = dr["Phone"] != DBNull.Value ? dr["Phone"].ToString() : "";
                    txtEmail.Text = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : "";
                    txtAadhar.Text = dr["Aadhar"] != DBNull.Value ? dr["Aadhar"].ToString() : "";
                    txtAddress.Text = dr["Address"] != DBNull.Value ? dr["Address"].ToString() : "";
                    txtQualification.Text = dr["Qualification"] != DBNull.Value ? dr["Qualification"].ToString() : "";
                    txtExperience.Text = dr["Experience"] != DBNull.Value ? dr["Experience"].ToString() : "";
                    txtExperienceIn.Text = dr["ExperienceIn"] != DBNull.Value ? dr["ExperienceIn"].ToString() : "";
                    
                    btnAddEmployee.Text = "Update Employee";
                    btnCancelEdit.Visible = true;
                    ShowMessage("Editing employee " + txtEmpName.Text, true, "employeeModal");
                }
            }
            else if (e.CommandName == "DeleteEmp")
            {
                string masterId = e.CommandArgument.ToString();
                try
                {
                    string delOver = "DELETE FROM CalculationOverrides WHERE EmpID = :MasterId";
                    DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), delOver, new OracleParameter("MasterId", masterId));

                    string delAtt = "DELETE FROM Attendance WHERE EmpID = :MasterId";
                    DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), delAtt, new OracleParameter("MasterId", masterId));
                    
                    string nullEng = "UPDATE Employees SET CurrentEngagementId = NULL WHERE MasterId = :MasterId";
                    DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), nullEng, new OracleParameter("MasterId", masterId));

                    string delEng = "DELETE FROM EmployeeEngagements WHERE EmpID = :MasterId";
                    DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), delEng, new OracleParameter("MasterId", masterId));

                    string delEmp = "DELETE FROM Employees WHERE MasterId = :MasterId";
                    DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), delEmp, new OracleParameter("MasterId", masterId));
                    
                    BindResignedEmployees();
                    BindGrid();
                    ShowMessage("Employee and their attendance history completely deleted.", true);
                }
                catch (Exception ex)
                {
                    ShowMessage("Error deleting employee: " + ex.Message, false);
                }
            }
            else if (e.CommandName == "ViewHistory")
            {
                string masterId = e.CommandArgument.ToString();
                OpenHistoryModal(masterId);
            }
        }

        protected void btnHiddenEditTrigger_Click(object sender, EventArgs e)
        {
            string masterId = hfHiddenEditMasterId.Value;
            if (!string.IsNullOrEmpty(masterId))
            {
                string query = "SELECT * FROM Employees WHERE MasterId = :MasterId";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query, new OracleParameter("MasterId", masterId));
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    hfEditOldID.Value = dr["MasterId"].ToString();
                    txtEmpID.Text = dr["ID"].ToString();
                    txtEmpID.Enabled = false;
                    txtJoinDate.Enabled = false;
                    txtEmpName.Text = dr["Name"].ToString();
                    txtMasterID.Text = dr["MasterId"]?.ToString() ?? "";
                    
                    chkIsRejoining.Checked = false;
                    chkIsRejoining.Enabled = false;
                    ddlRejoiningEmployee.Enabled = false;

                    string deptValue = dr["Department"].ToString();
                    if (ddlDept.Items.FindByValue(deptValue) != null)
                    {
                        ddlDept.SelectedValue = deptValue;
                    }
                    else
                    {
                        ddlDept.Items.Add(new ListItem(deptValue, deptValue));
                        ddlDept.SelectedValue = deptValue;
                    }

                    bool hasActiveEng = dr["CurrentEngagementId"] != DBNull.Value;
                    ddlDept.Enabled = !hasActiveEng;
                    lblDeptHelp.Style["display"] = hasActiveEng ? "block" : "none";
                    ddlCat.Enabled = !hasActiveEng;
                    lblCatHelp.Style["display"] = hasActiveEng ? "block" : "none";

                    string catValue = dr["Category"].ToString();
                    if (ddlCat.Items.FindByValue(catValue) != null)
                    {
                        ddlCat.SelectedValue = catValue;
                    }
                    else
                    {
                        ddlCat.Items.Add(new ListItem(catValue, catValue));
                        ddlCat.SelectedValue = catValue;
                    }

                    object oJoinDate = dr["OriginalJoinDate"] != DBNull.Value ? dr["OriginalJoinDate"] : dr["JoinDate"];
                    txtJoinDate.Text = oJoinDate != DBNull.Value ? Convert.ToDateTime(oJoinDate).ToString("yyyy-MM-dd") : "";
                    txtLeaveBalance.Text = dr["LeaveBalance"].ToString();
                    txtPrevLeaveBalance.Text = dr["PrevLeaveBalance"] != DBNull.Value ? dr["PrevLeaveBalance"].ToString() : "0";
                    txtPhone.Text = dr["Phone"] != DBNull.Value ? dr["Phone"].ToString() : "";
                    txtEmail.Text = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : "";
                    txtAadhar.Text = dr["Aadhar"] != DBNull.Value ? dr["Aadhar"].ToString() : "";
                    txtAddress.Text = dr["Address"] != DBNull.Value ? dr["Address"].ToString() : "";
                    txtQualification.Text = dr["Qualification"] != DBNull.Value ? dr["Qualification"].ToString() : "";
                    txtExperience.Text = dr["Experience"] != DBNull.Value ? dr["Experience"].ToString() : "";
                    txtExperienceIn.Text = dr["ExperienceIn"] != DBNull.Value ? dr["ExperienceIn"].ToString() : "";
                    
                    btnAddEmployee.Text = "Update Employee";
                    btnCancelEdit.Visible = true;
                    ShowMessage("Editing employee " + txtEmpName.Text, true, "employeeModal");
                }
            }
        }


        // ── Employee History ─────────────────────────────────────────────────────

        protected void gvEmployees_ViewHistory(object sender, GridViewCommandEventArgs e)
        {
            // handled via inline RowCommand – delegate here
        }

        /// <summary>
        /// Called from RowCommand when CommandName == "ViewHistory".
        /// Builds a JSON-like HTML block and pushes it to the client via JS.
        /// </summary>
        private void OpenHistoryModal(string masterId)
        {
            try
            {
                // 1. Fetch employee master info
                string empSql = @"
                    SELECT e.MasterId, e.ID, e.Name, e.Department, e.Category, e.Status,
                           e.JoinDate, e.OriginalJoinDate, e.ResignDate, e.ContractEndDate,
                           e.Phone, e.Email, e.Aadhar, e.Address, e.Qualification, e.Experience, e.ExperienceIn
                    FROM   Employees e
                    WHERE  e.MasterId = :MasterId";
                DataTable dtEmp = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), empSql,
                    new OracleParameter("MasterId", masterId));

                if (dtEmp == null || dtEmp.Rows.Count == 0)
                {
                    ShowMessage("Employee not found.", false);
                    return;
                }

                DataRow emp = dtEmp.Rows[0];
                string empName     = emp["Name"].ToString();
                string empDept     = emp["Department"] != DBNull.Value ? emp["Department"].ToString() : "—";
                string empStatus   = emp["Status"].ToString();
                string empMasterId = emp["MasterId"].ToString();
                string empId       = emp["ID"] != DBNull.Value ? emp["ID"].ToString() : "—";

                DateTime? joinDateVal = emp["JoinDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(emp["JoinDate"]) : null;
                DateTime? origJoinDateVal = emp["OriginalJoinDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(emp["OriginalJoinDate"]) : null;

                string origJoinDateStr = origJoinDateVal.HasValue ? origJoinDateVal.Value.ToString("dd-MMM-yyyy") : (joinDateVal.HasValue ? joinDateVal.Value.ToString("dd-MMM-yyyy") : "—");
                string rejoinDateStr = "";
                if (joinDateVal.HasValue && origJoinDateVal.HasValue && joinDateVal.Value.Date != origJoinDateVal.Value.Date)
                {
                    rejoinDateStr = joinDateVal.Value.ToString("dd-MMM-yyyy");
                }

                string resignDateStr = emp["ResignDate"] != DBNull.Value
                    ? Convert.ToDateTime(emp["ResignDate"]).ToString("dd-MMM-yyyy") : "";
                string contractEndDateStr = emp["ContractEndDate"] != DBNull.Value
                    ? Convert.ToDateTime(emp["ContractEndDate"]).ToString("dd-MMM-yyyy") : "";
                
                string empPhone = emp["Phone"] != DBNull.Value ? emp["Phone"].ToString() : "";
                string empEmail = emp["Email"] != DBNull.Value ? emp["Email"].ToString() : "";
                string empAadhar = emp["Aadhar"] != DBNull.Value ? emp["Aadhar"].ToString() : "";
                string empAddress = emp["Address"] != DBNull.Value ? emp["Address"].ToString() : "";
                string empQualification = emp["Qualification"] != DBNull.Value ? emp["Qualification"].ToString() : "";
                string empExperience = emp["Experience"] != DBNull.Value ? emp["Experience"].ToString() : "";
                string empExperienceIn = emp["ExperienceIn"] != DBNull.Value ? emp["ExperienceIn"].ToString() : "";
 
                // 2. Fetch full engagement history (oldest → newest)
                string histSql = @"
                    SELECT ee.Id, ee.Category, ee.StartDate, ee.EndDate, ee.EndReason,
                           ee.IsCarriedOver, ee.PrevEngagementId, ee.EmployeeId, ee.Department,
                           v.Name  AS VendorName,  v.MasterId AS VendorMasterId,
                           cp.StartDate AS ContractStart, cp.EndDate AS ContractEnd
                    FROM   EmployeeEngagements ee
                    JOIN   Vendors            v  ON v.Id  = ee.VendorId
                    LEFT JOIN ContractPeriods cp ON cp.Id = ee.ContractPeriodId
                    WHERE  ee.EmpID = :MasterId
                    ORDER  BY ee.StartDate ASC, ee.Id ASC";
                DataTable dtHist = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), histSql,
                    new OracleParameter("MasterId", masterId));
 
                // 3. Build timeline JSON for client-side rendering
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("[");
 
                if (dtHist != null && dtHist.Rows.Count > 0)
                {
                    for (int i = 0; i < dtHist.Rows.Count; i++)
                    {
                        DataRow r = dtHist.Rows[i];
                        string cat        = r["Category"].ToString();
                        string dept       = r["Department"] != DBNull.Value ? r["Department"].ToString() : "";
                        string vName      = r["VendorName"].ToString();
                        string vId        = r["VendorMasterId"].ToString();
                        string startDate  = Convert.ToDateTime(r["StartDate"]).ToString("dd-MMM-yyyy");
                        string endDate    = r["EndDate"] != DBNull.Value
                            ? Convert.ToDateTime(r["EndDate"]).ToString("dd-MMM-yyyy") : "";
                        string endReason  = r["EndReason"] != DBNull.Value ? r["EndReason"].ToString() : "";
                        
                        string cpRange = "No Contract";
                        if (r["ContractStart"] != DBNull.Value)
                        {
                            string cpStart = Convert.ToDateTime(r["ContractStart"]).ToString("dd-MMM-yyyy");
                            string cpEnd   = r["ContractEnd"] != DBNull.Value ? Convert.ToDateTime(r["ContractEnd"]).ToString("dd-MMM-yyyy") : "Present";
                            cpRange = cpStart + " to " + cpEnd;
                        }
                        
                        bool carriedOver  = Convert.ToInt32(r["IsCarriedOver"]) == 1;
                        bool hasPrev      = r["PrevEngagementId"] != DBNull.Value;
                        string historicalEmpId = r["EmployeeId"] != DBNull.Value ? r["EmployeeId"].ToString() : "—";
 
                        if (i > 0) sb.Append(",");
                        sb.Append("{");
                        sb.AppendFormat("\"cat\":\"{0}\",", EscapeJs(cat));
                        sb.AppendFormat("\"dept\":\"{0}\",", EscapeJs(dept));
                        sb.AppendFormat("\"vendor\":\"{0} ({1})\",", EscapeJs(vName), EscapeJs(vId));
                        sb.AppendFormat("\"start\":\"{0}\",", startDate);
                        sb.AppendFormat("\"end\":\"{0}\",", endDate);
                        sb.AppendFormat("\"endReason\":\"{0}\",", EscapeJs(endReason));
                        sb.AppendFormat("\"cpRange\":\"{0}\",", EscapeJs(cpRange));
                        sb.AppendFormat("\"carriedOver\":{0},", carriedOver ? "true" : "false");
                        sb.AppendFormat("\"hasPrev\":{0},", hasPrev ? "true" : "false");
                        sb.AppendFormat("\"historicalEmpId\":\"{0}\"", EscapeJs(historicalEmpId));
                        sb.Append("}");
                    }
                }
                sb.Append("]");
 
                // Escape for JS string literal
                string empJson = string.Format("{{\"name\":\"{0}\",\"masterId\":\"{1}\",\"dept\":\"{2}\",\"status\":\"{3}\",\"joinDate\":\"{4}\",\"rejoinDate\":\"{5}\",\"resignDate\":\"{6}\",\"contractEndDate\":\"{7}\",\"empId\":\"{8}\",\"phone\":\"{9}\",\"email\":\"{10}\",\"aadhar\":\"{11}\",\"address\":\"{12}\",\"qualification\":\"{13}\",\"experience\":\"{14}\",\"experienceIn\":\"{15}\"}}",
                    EscapeJs(empName), EscapeJs(empMasterId), EscapeJs(empDept), EscapeJs(empStatus),
                    origJoinDateStr, rejoinDateStr, resignDateStr, contractEndDateStr, EscapeJs(empId),
                    EscapeJs(empPhone), EscapeJs(empEmail), EscapeJs(empAadhar), EscapeJs(empAddress),
                    EscapeJs(empQualification), EscapeJs(empExperience), EscapeJs(empExperienceIn));

                string script = string.Format("openHistoryModal({0}, {1});", empJson, sb.ToString());
                ClientScript.RegisterStartupScript(this.GetType(), "hist_" + Guid.NewGuid().ToString("N"), script, true);
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading employee history: " + ex.Message, false);
            }
        }

        private static string EscapeJs(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'")
                    .Replace("\r\n", "\\n").Replace("\r", "").Replace("\n", "\\n");
        }
        // ── End Employee History ─────────────────────────────────────────────────

        private void ShowMessage(string msg, bool success)
        {
            ShowMessage(msg, success, null);
        }

        private void ShowMessage(string msg, bool success, string showModalId)
        {
            lblMessage.Text = msg;
            lblMessage.Visible = false;

            string cleanMessage = msg.Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
            string toastType = success ? "success" : "error";
            string script = string.Format("showToast('{0}', '{1}');", cleanMessage, toastType);

            if (!string.IsNullOrEmpty(showModalId))
            {
                script += string.Format(" var modalEl = document.getElementById('{0}'); if (modalEl) {{ var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl); modal.show(); }}", showModalId);
                if (showModalId == "employeeModal" && !string.IsNullOrEmpty(hfEditOldID.Value))
                {
                    script += " var label = document.getElementById('employeeModalLabel'); if (label) label.textContent = 'Edit Employee Details';";
                }
            }

            ClientScript.RegisterStartupScript(this.GetType(), "toast_" + Guid.NewGuid().ToString("N"), script, true);
        }

        private void SyncIndividualLeaveCredits(string connStr, string masterId, double currentBalance, double prevBalance)
        {
            try
            {
                // Find current engagement details
                string currSql = @"
                    SELECT ee.ContractPeriodId, ee.StartDate 
                    FROM Employees e
                    JOIN EmployeeEngagements ee ON e.CurrentEngagementId = ee.Id
                    WHERE e.MasterId = :MasterId";
                DataTable dtCurr = DBHelper.ExecuteQuery(connStr, currSql, new OracleParameter("MasterId", masterId));
                if (dtCurr.Rows.Count > 0)
                {
                    int currCpId = Convert.ToInt32(dtCurr.Rows[0]["ContractPeriodId"]);
                    DateTime currStartDate = Convert.ToDateTime(dtCurr.Rows[0]["StartDate"]);

                    // Check if initial balance record exists
                    string checkCurr = @"
                        SELECT Id FROM EmployeeLeaveCredits 
                        WHERE EmpID = :EmpID AND ContractPeriodId = :CpId AND Remarks = 'Contract Initial Balance'";
                    object resCurr = DBHelper.ExecuteScalar(connStr, checkCurr, 
                        new OracleParameter("EmpID", masterId),
                        new OracleParameter("CpId", currCpId));
                    if (resCurr != null && resCurr != DBNull.Value)
                    {
                        // Update
                        string updateCurr = "UPDATE EmployeeLeaveCredits SET Amount = :Amount WHERE Id = :Id";
                        DBHelper.ExecuteNonQuery(connStr, updateCurr, 
                            new OracleParameter("Amount", currentBalance),
                            new OracleParameter("Id", Convert.ToInt32(resCurr)));
                    }
                    else
                    {
                        // Insert
                        string insertCurr = "INSERT INTO EmployeeLeaveCredits (EmpID, ContractPeriodId, Amount, EffectiveDate, Remarks) VALUES (:EmpID, :CpId, :Amount, :EffectiveDate, 'Contract Initial Balance')";
                        DBHelper.ExecuteNonQuery(connStr, insertCurr, 
                            new OracleParameter("EmpID", masterId),
                            new OracleParameter("CpId", currCpId),
                            new OracleParameter("Amount", currentBalance),
                            new OracleParameter("EffectiveDate", currStartDate));
                    }
                }

                // Find latest past engagement details
                string pastSql = @"
                    SELECT ee.ContractPeriodId, ee.StartDate 
                    FROM Employees e
                    JOIN EmployeeEngagements ee ON e.MasterId = ee.EmpID
                    WHERE e.MasterId = :MasterId 
                      AND (e.CurrentEngagementId IS NULL OR ee.Id != e.CurrentEngagementId)
                    ORDER BY ee.StartDate DESC";
                DataTable dtPast = DBHelper.ExecuteQuery(connStr, pastSql, new OracleParameter("MasterId", masterId));
                if (dtPast.Rows.Count > 0)
                {
                    int pastCpId = Convert.ToInt32(dtPast.Rows[0]["ContractPeriodId"]);
                    DateTime pastStartDate = Convert.ToDateTime(dtPast.Rows[0]["StartDate"]);

                    // Check if initial balance record exists
                    string checkPast = @"
                        SELECT Id FROM EmployeeLeaveCredits 
                        WHERE EmpID = :EmpID AND ContractPeriodId = :CpId AND Remarks = 'Contract Initial Balance'";
                    object resPast = DBHelper.ExecuteScalar(connStr, checkPast, 
                        new OracleParameter("EmpID", masterId),
                        new OracleParameter("CpId", pastCpId));
                    if (resPast != null && resPast != DBNull.Value)
                    {
                        // Update
                        string updatePast = "UPDATE EmployeeLeaveCredits SET Amount = :Amount WHERE Id = :Id";
                        DBHelper.ExecuteNonQuery(connStr, updatePast, 
                            new OracleParameter("Amount", prevBalance),
                            new OracleParameter("Id", Convert.ToInt32(resPast)));
                    }
                    else
                    {
                        // Insert
                        string insertPast = "INSERT INTO EmployeeLeaveCredits (EmpID, ContractPeriodId, Amount, EffectiveDate, Remarks) VALUES (:EmpID, :CpId, :Amount, :EffectiveDate, 'Contract Initial Balance')";
                        DBHelper.ExecuteNonQuery(connStr, insertPast, 
                            new OracleParameter("EmpID", masterId),
                            new OracleParameter("CpId", pastCpId),
                            new OracleParameter("Amount", prevBalance),
                            new OracleParameter("EffectiveDate", pastStartDate));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error syncing leave credits: " + ex.Message);
            }
        }
    }
}

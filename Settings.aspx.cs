using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using AttendanceApp.Utils;
using Oracle.ManagedDataAccess.Client;

namespace AttendanceApp
{
    public partial class Settings : System.Web.UI.Page
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
                return;
            }

            if (!IsPostBack)
            {
                BindDivisions();
                BindCategories();
                BindActionLogs();
            }
        }

        private void ShowToast(string message, string type)
        {
            string cleanMessage = message.Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
            string script = string.Format("showToast('{0}', '{1}');", cleanMessage, type);
            ClientScript.RegisterStartupScript(this.GetType(), "toast_" + Guid.NewGuid().ToString("N"), script, true);
        }

        #region Division Management

        private void BindDivisions()
        {
            try
            {
                string query = "SELECT Id, Name FROM Divisions ORDER BY Name ASC";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
                gvDivisions.DataSource = dt;
                gvDivisions.DataBind();
            }
            catch (Exception ex)
            {
                ShowToast("Error loading divisions: " + ex.Message, "error");
            }
        }

        protected void btnAddDiv_Click(object sender, EventArgs e)
        {
            string divName = txtNewDivName.Text.Trim();
            if (string.IsNullOrEmpty(divName))
            {
                ShowToast("Division name cannot be empty.", "warning");
                return;
            }

            try
            {
                string checkQuery = "SELECT COUNT(*) FROM Divisions WHERE UPPER(Name) = UPPER(:Name)";
                int count = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), checkQuery, new OracleParameter("Name", divName)));
                if (count > 0)
                {
                    ShowToast("Division '" + divName + "' already exists.", "warning");
                    return;
                }

                string insertQuery = "INSERT INTO Divisions (Name) VALUES (:Name)";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), insertQuery, new OracleParameter("Name", divName));
                txtNewDivName.Text = "";
                BindDivisions();
                ShowToast("Division '" + divName + "' added successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowToast("Error adding division: " + ex.Message, "error");
            }
        }

        protected void gvDivisions_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvDivisions.EditIndex = e.NewEditIndex;
            BindDivisions();
        }

        protected void gvDivisions_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvDivisions.EditIndex = -1;
            BindDivisions();
        }

        protected void gvDivisions_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvDivisions.DataKeys[e.RowIndex].Value);
            TextBox txtName = (TextBox)gvDivisions.Rows[e.RowIndex].FindControl("txtDivName");
            string newName = txtName != null ? txtName.Text.Trim() : "";

            if (string.IsNullOrEmpty(newName))
            {
                ShowToast("Division name cannot be empty.", "warning");
                return;
            }

            try
            {
                // Get old name first
                string getOldNameQuery = "SELECT Name FROM Divisions WHERE Id = :Id";
                string oldName = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), getOldNameQuery, new OracleParameter("Id", id))?.ToString();

                if (string.IsNullOrEmpty(oldName))
                {
                    ShowToast("Error: Division not found.", "error");
                    return;
                }

                if (oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                {
                    gvDivisions.EditIndex = -1;
                    BindDivisions();
                    return;
                }

                // Check for uniqueness
                string checkQuery = "SELECT COUNT(*) FROM Divisions WHERE UPPER(Name) = UPPER(:Name) AND Id != :Id";
                int count = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), checkQuery, 
                    new OracleParameter("Name", newName),
                    new OracleParameter("Id", id)));
                if (count > 0)
                {
                    ShowToast("Division name '" + newName + "' is already in use.", "warning");
                    return;
                }

                // Update Divisions table
                string updateQuery = "UPDATE Divisions SET Name = :Name WHERE Id = :Id";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), updateQuery, 
                    new OracleParameter("Name", newName),
                    new OracleParameter("Id", id));

                // Cascade update Employees table
                string cascadeQuery = "UPDATE Employees SET Department = :NewName WHERE Department = :OldName";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), cascadeQuery, 
                    new OracleParameter("NewName", newName),
                    new OracleParameter("OldName", oldName));

                gvDivisions.EditIndex = -1;
                BindDivisions();
                ShowToast("Division updated to '" + newName + "' successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowToast("Error updating division: " + ex.Message, "error");
            }
        }

        protected void gvDivisions_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = Convert.ToInt32(gvDivisions.DataKeys[e.RowIndex].Value);

            try
            {
                // Get name first
                string getNameQuery = "SELECT Name FROM Divisions WHERE Id = :Id";
                string name = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), getNameQuery, new OracleParameter("Id", id))?.ToString();

                if (string.IsNullOrEmpty(name))
                {
                    ShowToast("Error: Division not found.", "error");
                    return;
                }

                // Verify if division is assigned to any employees
                string countEmpQuery = "SELECT COUNT(*) FROM Employees WHERE Department = :Dept";
                int count = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), countEmpQuery, new OracleParameter("Dept", name)));
                
                if (count > 0)
                {
                    ShowToast("Cannot delete: assigned to " + count + " employee(s).", "warning");
                    return;
                }

                // Delete
                string deleteQuery = "DELETE FROM Divisions WHERE Id = :Id";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), deleteQuery, new OracleParameter("Id", id));

                BindDivisions();
                ShowToast("Division '" + name + "' deleted successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowToast("Error deleting division: " + ex.Message, "error");
            }
        }

        #endregion

        #region Category Management

        private void BindCategories()
        {
            try
            {
                string query = "SELECT Id, Name FROM Categories ORDER BY Name ASC";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
                gvCategories.DataSource = dt;
                gvCategories.DataBind();
            }
            catch (Exception ex)
            {
                ShowToast("Error loading categories: " + ex.Message, "error");
            }
        }

        protected void btnAddCat_Click(object sender, EventArgs e)
        {
            string catName = txtNewCatName.Text.Trim();
            if (string.IsNullOrEmpty(catName))
            {
                ShowToast("Category name cannot be empty.", "warning");
                return;
            }

            try
            {
                string checkQuery = "SELECT COUNT(*) FROM Categories WHERE UPPER(Name) = UPPER(:Name)";
                int count = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), checkQuery, new OracleParameter("Name", catName)));
                if (count > 0)
                {
                    ShowToast("Category '" + catName + "' already exists.", "warning");
                    return;
                }

                string insertQuery = "INSERT INTO Categories (Name) VALUES (:Name)";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), insertQuery, new OracleParameter("Name", catName));
                txtNewCatName.Text = "";
                BindCategories();
                ShowToast("Category '" + catName + "' added successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowToast("Error adding category: " + ex.Message, "error");
            }
        }

        protected void gvCategories_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvCategories.EditIndex = e.NewEditIndex;
            BindCategories();
        }

        protected void gvCategories_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvCategories.EditIndex = -1;
            BindCategories();
        }

        protected void gvCategories_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvCategories.DataKeys[e.RowIndex].Value);
            TextBox txtName = (TextBox)gvCategories.Rows[e.RowIndex].FindControl("txtCatName");
            string newName = txtName != null ? txtName.Text.Trim() : "";

            if (string.IsNullOrEmpty(newName))
            {
                ShowToast("Category name cannot be empty.", "warning");
                return;
            }

            try
            {
                // Get old name first
                string getOldNameQuery = "SELECT Name FROM Categories WHERE Id = :Id";
                string oldName = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), getOldNameQuery, new OracleParameter("Id", id))?.ToString();

                if (string.IsNullOrEmpty(oldName))
                {
                    ShowToast("Error: Category not found.", "error");
                    return;
                }

                if (oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                {
                    gvCategories.EditIndex = -1;
                    BindCategories();
                    return;
                }

                // Check for uniqueness
                string checkQuery = "SELECT COUNT(*) FROM Categories WHERE UPPER(Name) = UPPER(:Name) AND Id != :Id";
                int count = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), checkQuery, 
                    new OracleParameter("Name", newName),
                    new OracleParameter("Id", id)));
                if (count > 0)
                {
                    ShowToast("Category name '" + newName + "' is already in use.", "warning");
                    return;
                }

                // Update Categories table
                string updateQuery = "UPDATE Categories SET Name = :Name WHERE Id = :Id";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), updateQuery, 
                    new OracleParameter("Name", newName),
                    new OracleParameter("Id", id));

                // Cascade update Employees table
                string cascadeQuery = "UPDATE Employees SET Category = :NewName WHERE Category = :OldName";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), cascadeQuery, 
                    new OracleParameter("NewName", newName),
                    new OracleParameter("OldName", oldName));

                // Cascade update CalculationWages table
                string cascadeWages = "UPDATE CalculationWages SET Category = :NewName WHERE Category = :OldName";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), cascadeWages, 
                    new OracleParameter("NewName", newName),
                    new OracleParameter("OldName", oldName));

                // Cascade update CalculationOverrides table
                string cascadeOverrides = "UPDATE CalculationOverrides SET Category = :NewName WHERE Category = :OldName";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), cascadeOverrides, 
                    new OracleParameter("NewName", newName),
                    new OracleParameter("OldName", oldName));

                gvCategories.EditIndex = -1;
                BindCategories();
                ShowToast("Category updated to '" + newName + "' successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowToast("Error updating category: " + ex.Message, "error");
            }
        }

        protected void gvCategories_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = Convert.ToInt32(gvCategories.DataKeys[e.RowIndex].Value);

            try
            {
                // Get name first
                string getNameQuery = "SELECT Name FROM Categories WHERE Id = :Id";
                string name = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), getNameQuery, new OracleParameter("Id", id))?.ToString();

                if (string.IsNullOrEmpty(name))
                {
                    ShowToast("Error: Category not found.", "error");
                    return;
                }

                // Verify if category is assigned to any employees
                string countEmpQuery = "SELECT COUNT(*) FROM Employees WHERE Category = :Cat";
                int count = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), countEmpQuery, new OracleParameter("Cat", name)));
                
                if (count > 0)
                {
                    ShowToast("Cannot delete: assigned to " + count + " employee(s).", "warning");
                    return;
                }

                // Verify if category has wages configured
                string countWagesQuery = "SELECT COUNT(*) FROM CalculationWages WHERE Category = :Cat";
                int countWages = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), countWagesQuery, new OracleParameter("Cat", name)));

                if (countWages > 0)
                {
                    ShowToast("Cannot delete: category has wage records.", "warning");
                    return;
                }

                // Delete
                string deleteQuery = "DELETE FROM Categories WHERE Id = :Id";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), deleteQuery, new OracleParameter("Id", id));

                BindCategories();
                ShowToast("Category '" + name + "' deleted successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowToast("Error deleting category: " + ex.Message, "error");
            }
        }

        #endregion

        #region Undo Manager

        private void BindActionLogs()
        {
            try
            {
                // Retrieve the last 5 logs overall to show full context, latest first
                string query = "SELECT * FROM (SELECT Id, ActionTime, ActionType, Description, IsUndone FROM EmployeeActionLogs ORDER BY ActionTime DESC, Id DESC) WHERE ROWNUM <= 5";
                DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
                gvActionLogs.DataSource = dt;
                gvActionLogs.DataBind();
            }
            catch (Exception ex)
            {
                ShowToast("Error loading action logs: " + ex.Message, "error");
            }
        }

        protected void gvActionLogs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "UndoCommand")
            {
                int targetLogId = Convert.ToInt32(e.CommandArgument);
                try
                {
                    // Find all active logs for this employee that are newer than or equal to the target log
                    string selectChain = @"
                        SELECT Id, Description FROM EmployeeActionLogs 
                        WHERE EmpMasterId = (SELECT EmpMasterId FROM EmployeeActionLogs WHERE Id = :TargetLogId)
                          AND Id >= :TargetLogId 
                          AND IsUndone = 0 
                        ORDER BY Id DESC";
                    
                    DataTable dtChain = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), selectChain, new OracleParameter("TargetLogId", targetLogId));
                    if (dtChain.Rows.Count == 0)
                    {
                        ShowToast("No active action log found to undo.", "warning");
                        return;
                    }

                    int successfulUndos = 0;
                    string lastError = "";

                    // Run the undo in reverse chronological order
                    foreach (DataRow row in dtChain.Rows)
                    {
                        int logId = Convert.ToInt32(row["Id"]);
                        string desc = row["Description"].ToString();
                        
                        string errMsg;
                        bool success = ActionLogger.UndoAction(logId, out errMsg);
                        if (!success)
                        {
                            lastError = errMsg;
                            break; // Stop immediately to preserve logical ordering and consistency
                        }
                        successfulUndos++;
                    }

                    BindActionLogs();

                    if (successfulUndos == dtChain.Rows.Count)
                    {
                        if (successfulUndos == 1)
                        {
                            ShowToast("Action successfully undone.", "success");
                        }
                        else
                        {
                            ShowToast(string.Format("Successfully rolled back {0} linked changes in sequence.", successfulUndos), "success");
                        }
                    }
                    else
                    {
                        if (successfulUndos > 0)
                        {
                            ShowToast(string.Format("Partially rolled back {0} change(s). Stopped due to error: {1}", successfulUndos, lastError), "error");
                        }
                        else
                        {
                            ShowToast("Undo failed: " + lastError, "error");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowToast("Error during undo operation: " + ex.Message, "error");
                }
            }
        }

        #endregion

        #region Database Backup & Restore

        private static readonly string[] BackupTables = new string[] {
            "AppUsers",
            "Divisions",
            "Categories",
            "UserDivisions",
            "Vendors",
            "ContractPeriods",
            "ContractPeriodVendors",
            "ContractExtensions",
            "Employees",
            "EmployeeEngagements",
            "EmployeeLeaveCredits",
            "Attendance",
            "CalculationWages",
            "CalculationOverrides",
            "EmployeeActionLogs",
            "ActionLog",
            "AdminActionLog",
            "Attendance_Audit_Log",
            "AttendanceRemarks",
            "Notices"
        };

        private static readonly string[] DeleteSequence = new string[] {
            "AttendanceRemarks",
            "Attendance_Audit_Log",
            "AdminActionLog",
            "ActionLog",
            "EmployeeActionLogs",
            "CalculationOverrides",
            "CalculationWages",
            "Attendance",
            "EmployeeLeaveCredits",
            "Employees_NullEngagements",
            "EmployeeEngagements",
            "Employees",
            "ContractExtensions",
            "ContractPeriodVendors",
            "ContractPeriods",
            "Vendors",
            "UserDivisions",
            "Categories",
            "Divisions",
            "AppUsers",
            "Notices"
        };

        private static readonly string[] InsertSequence = new string[] {
            "AppUsers",
            "Divisions",
            "Categories",
            "UserDivisions",
            "Vendors",
            "ContractPeriods",
            "ContractPeriodVendors",
            "ContractExtensions",
            "Employees",
            "EmployeeEngagements",
            "Employees_UpdateEngagements",
            "EmployeeLeaveCredits",
            "Attendance",
            "CalculationWages",
            "CalculationOverrides",
            "EmployeeActionLogs",
            "ActionLog",
            "AdminActionLog",
            "Attendance_Audit_Log",
            "AttendanceRemarks",
            "Notices"
        };

        protected void btnExportBackup_Click(object sender, EventArgs e)
        {
            try
            {
                var backupData = new Dictionary<string, object>();

                // Export other tables
                foreach (string table in BackupTables)
                {
                    DataTable dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), "SELECT * FROM " + table);
                    var list = new List<Dictionary<string, object>>();
                    foreach (DataRow row in dt.Rows)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in dt.Columns)
                        {
                            dict[col.ColumnName] = ConvertValue(row[col]);
                        }
                        list.Add(dict);
                    }
                    backupData[table] = list;
                }

                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;
                string json = serializer.Serialize(backupData);

                Response.Clear();
                Response.ContentType = "application/json";
                Response.AddHeader("content-disposition", "attachment; filename=attendance_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
                Response.Write(json);
                Response.End();
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Normal and expected on Response.End()
            }
            catch (Exception ex)
            {
                ShowToast("Error exporting database: " + ex.Message, "error");
            }
        }

        private object ConvertValue(object val)
        {
            if (val == null || val == DBNull.Value)
                return null;
            if (val is DateTime)
                return ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss.fff");
            if (val is string || val is ValueType)
                return val;
            return val.ToString();
        }

        protected void btnRestoreBackup_Click(object sender, EventArgs e)
        {
            if (!fuBackupFile.HasFile)
            {
                ShowToast("Please select a JSON backup file to restore.", "warning");
                return;
            }

            try
            {
                string jsonContent = "";
                using (var reader = new StreamReader(fuBackupFile.FileContent))
                {
                    jsonContent = reader.ReadToEnd();
                }

                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;
                var rawBackupData = serializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(jsonContent);

                if (rawBackupData == null)
                {
                    ShowToast("Invalid backup file format.", "error");
                    return;
                }

                // Convert all table names and row dictionary keys to be case-insensitive
                var backupData = new Dictionary<string, List<Dictionary<string, object>>>(StringComparer.OrdinalIgnoreCase);
                foreach (var kp in rawBackupData)
                {
                    var list = new List<Dictionary<string, object>>();
                    foreach (var dict in kp.Value)
                    {
                        list.Add(new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase));
                    }
                    backupData[kp.Key] = list;
                }

                using (OracleConnection conn = new OracleConnection(DBHelper.GetAttendanceDBConnection()))
                {
                    conn.Open();
                    using (OracleTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Delete all tables in sequence
                            foreach (string table in DeleteSequence)
                            {
                                if (table == "Employees_NullEngagements")
                                {
                                    using (OracleCommand cmd = new OracleCommand("UPDATE Employees SET CurrentEngagementId = NULL", conn))
                                    {
                                        cmd.Transaction = trans;
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    using (OracleCommand cmd = new OracleCommand("DELETE FROM " + table, conn))
                                    {
                                        cmd.Transaction = trans;
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            // 2. Insert all tables in sequence
                            foreach (string table in InsertSequence)
                            {
                                if (table == "Employees_UpdateEngagements")
                                {
                                    if (backupData.ContainsKey("Employees"))
                                    {
                                        var employeeRows = backupData["Employees"];
                                        using (OracleCommand updateCmd = new OracleCommand("UPDATE Employees SET CurrentEngagementId = :CurrentEngagementId WHERE MasterId = :MasterId", conn))
                                        {
                                            updateCmd.Transaction = trans;
                                            updateCmd.BindByName = true;

                                            foreach (var row in employeeRows)
                                            {
                                                if (row.ContainsKey("CurrentEngagementId") && row["CurrentEngagementId"] != null)
                                                {
                                                    updateCmd.Parameters.Clear();
                                                    updateCmd.Parameters.Add(new OracleParameter("CurrentEngagementId", Convert.ToInt32(row["CurrentEngagementId"])));
                                                    updateCmd.Parameters.Add(new OracleParameter("MasterId", row["MasterId"].ToString()));
                                                    updateCmd.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (table == "Employees")
                                {
                                    if (backupData.ContainsKey("Employees"))
                                    {
                                        var employeeRows = backupData["Employees"];
                                        var modifiedRows = new List<Dictionary<string, object>>();
                                        foreach (var row in employeeRows)
                                        {
                                            var copy = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                                            if (copy.ContainsKey("CurrentEngagementId"))
                                            {
                                                copy["CurrentEngagementId"] = null;
                                            }
                                            modifiedRows.Add(copy);
                                        }
                                        InsertRows(conn, trans, "Employees", modifiedRows);
                                    }
                                }
                                else
                                {
                                    if (backupData.ContainsKey(table))
                                    {
                                        InsertRows(conn, trans, table, backupData[table]);
                                    }
                                }
                            }

                            // 3. Reset all sequences and recompile triggers
                            var sequenceMapping = new[] {
                                new { Table = "Divisions", Sequence = "SEQ_Divisions", Trigger = "TRG_Divisions" },
                                new { Table = "Categories", Sequence = "SEQ_Categories", Trigger = "TRG_Categories" },
                                new { Table = "Vendors", Sequence = "SEQ_Vendors", Trigger = "TRG_Vendors" },
                                new { Table = "ContractPeriods", Sequence = "SEQ_ContractPeriods", Trigger = "TRG_ContractPeriods" },
                                new { Table = "ContractPeriodVendors", Sequence = "SEQ_ContractPeriodVendors", Trigger = "TRG_ContractPeriodVendors" },
                                new { Table = "ContractExtensions", Sequence = "SEQ_ContractExtensions", Trigger = "TRG_ContractExtensions" },
                                new { Table = "EmployeeEngagements", Sequence = "SEQ_EmployeeEngagements", Trigger = "TRG_EmployeeEngagements" },
                                new { Table = "EmployeeLeaveCredits", Sequence = "SEQ_EmployeeLeaveCredits", Trigger = "TRG_EmployeeLeaveCredits" },
                                new { Table = "Attendance", Sequence = "SEQ_Attendance", Trigger = "TRG_Attendance" },
                                new { Table = "EmployeeActionLogs", Sequence = "SEQ_EmployeeActionLogs", Trigger = "TRG_EmployeeActionLogs" },
                                new { Table = "ActionLog", Sequence = "SEQ_ActionLog", Trigger = "TRG_ActionLog" },
                                new { Table = "AdminActionLog", Sequence = "SEQ_AdminActionLog", Trigger = "TRG_AdminActionLog" },
                                new { Table = "Attendance_Audit_Log", Sequence = "SEQ_AttendanceAuditLog", Trigger = "TRG_AttendanceAuditLog" },
                                new { Table = "AttendanceRemarks", Sequence = "SEQ_AttendanceRemarks", Trigger = "TRG_AttendanceRemarks" },
                                new { Table = "Notices", Sequence = "SEQ_Notices", Trigger = "TRG_Notices" }
                            };

                            foreach (var map in sequenceMapping)
                            {
                                ResetSequenceAndTrigger(conn, trans, map.Table, map.Sequence, map.Trigger);
                            }

                            // 4. Log database restoration
                            string userPcno = Session["PCNO"]?.ToString() ?? "SYSTEM";
                            string userDisplayName = Session["Name"]?.ToString() ?? "Administrator";
                            string logSql = @"
                                INSERT INTO ActionLog (ActionType, PerformedBy, TargetId, Description, PreState, PostState)
                                VALUES ('DATABASE_RESTORE', :PerformedBy, 'SYSTEM', :Description, NULL, NULL)";
                            
                            using (OracleCommand logCmd = new OracleCommand(logSql, conn))
                            {
                                logCmd.Transaction = trans;
                                logCmd.Parameters.Add(new OracleParameter("PerformedBy", userDisplayName + " (" + userPcno + ")"));
                                logCmd.Parameters.Add(new OracleParameter("Description", "Database successfully restored from JSON backup file."));
                                logCmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                            
                            BindDivisions();
                            BindCategories();
                            BindActionLogs();

                            ShowToast("Database successfully restored from backup file.", "success");
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            ShowToast("Error restoring database: " + ex.Message, "error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowToast("Failed to process backup file: " + ex.Message, "error");
            }
        }

        private void InsertRows(OracleConnection conn, OracleTransaction trans, string tableName, List<Dictionary<string, object>> rows)
        {
            if (rows == null || rows.Count == 0)
                return;

            var typeDict = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            using (OracleCommand schemaCmd = new OracleCommand("SELECT * FROM " + tableName + " WHERE 1=0", conn))
            {
                schemaCmd.Transaction = trans;
                using (OracleDataReader reader = schemaCmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    DataTable schemaTable = reader.GetSchemaTable();
                    if (schemaTable != null)
                    {
                        foreach (DataRow colRow in schemaTable.Rows)
                        {
                            string colName = colRow["ColumnName"].ToString();
                            Type dataType = (Type)colRow["DataType"];
                            typeDict[colName] = dataType;
                        }
                    }
                }
            }

            var firstRow = rows[0];
            var columnNames = new List<string>();
            var parameterNames = new List<string>();

            foreach (var key in firstRow.Keys)
            {
                if (typeDict.ContainsKey(key))
                {
                    columnNames.Add(key);
                    parameterNames.Add(":" + key);
                }
            }

            if (columnNames.Count == 0)
                return;

            string insertSql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", 
                tableName, 
                string.Join(", ", columnNames), 
                string.Join(", ", parameterNames));

            using (OracleCommand cmd = new OracleCommand(insertSql, conn))
            {
                cmd.Transaction = trans;
                cmd.BindByName = true;

                foreach (var row in rows)
                {
                    cmd.Parameters.Clear();
                    foreach (string colName in columnNames)
                    {
                        object value = row[colName];
                        Type targetType;
                        Type tType;
                        if (value == null)
                        {
                            value = DBNull.Value;
                        }
                        else if (typeDict.TryGetValue(colName, out targetType) && targetType == typeof(DateTime))
                        {
                            if (value is string)
                            {
                                DateTime parsedDate;
                                if (DateTime.TryParse((string)value, out parsedDate))
                                {
                                    value = parsedDate;
                                }
                                else
                                {
                                    value = DBNull.Value;
                                }
                            }
                        }
                        else if (typeDict.TryGetValue(colName, out tType) && (tType == typeof(int) || tType == typeof(long) || tType == typeof(short) || tType == typeof(decimal) || tType == typeof(double) || tType == typeof(float)))
                        {
                            if (value is string)
                            {
                                decimal parsedNum;
                                if (decimal.TryParse((string)value, out parsedNum))
                                {
                                    value = parsedNum;
                                }
                                else
                                {
                                    value = DBNull.Value;
                                }
                            }
                        }

                        cmd.Parameters.Add(new OracleParameter(colName, value));
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ResetSequenceAndTrigger(OracleConnection conn, OracleTransaction trans, string tableName, string seqName, string triggerName)
        {
            long maxId = 0;
            string queryMax = string.Format("SELECT MAX(Id) FROM {0}", tableName);
            using (OracleCommand cmd = new OracleCommand(queryMax, conn))
            {
                cmd.Transaction = trans;
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value)
                {
                    maxId = Convert.ToInt64(res);
                }
            }

            long startWith = maxId + 1;
            if (startWith < 1) startWith = 1;

            try
            {
                string dropSql = string.Format("DROP SEQUENCE {0}", seqName);
                using (OracleCommand cmd = new OracleCommand(dropSql, conn))
                {
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }

            string createSql = string.Format("CREATE SEQUENCE {0} START WITH {1} INCREMENT BY 1 NOCACHE NOCYCLE", seqName, startWith);
            using (OracleCommand cmd = new OracleCommand(createSql, conn))
            {
                cmd.Transaction = trans;
                cmd.ExecuteNonQuery();
            }

            try
            {
                string compileSql = string.Format("ALTER TRIGGER {0} COMPILE", triggerName);
                using (OracleCommand cmd = new OracleCommand(compileSql, conn))
                {
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        #endregion
    }
}

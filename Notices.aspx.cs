using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AttendanceApp.Utils;
using Oracle.ManagedDataAccess.Client;

namespace AttendanceApp
{
    public partial class Notices : System.Web.UI.Page
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
                phAdminNoticeUpload.Visible = false; // Hide upload container for regular users
            }
            else
            {
                phAdminNoticeUpload.Visible = true;
            }

            if (!IsPostBack)
            {
                LoadNotices();
            }
        }

        private void ShowToast(string message, string type)
        {
            string cleanMessage = message.Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
            string script = string.Format("showToast('{0}', '{1}');", cleanMessage, type);
            ClientScript.RegisterStartupScript(this.GetType(), "toast_" + Guid.NewGuid().ToString("N"), script, true);
        }

        private void LoadNotices()
        {
            try
            {
                int role = Convert.ToInt32(Session["Role"] ?? 0);
                string query;
                DataTable dt;

                if (role == 1)
                {
                    // Admin can see all notices (including hidden)
                    query = "SELECT Id, Name, FilePath, IsHidden, UploadDate FROM Notices ORDER BY UploadDate DESC";
                    dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
                }
                else
                {
                    // Regular users can only see visible notices
                    query = "SELECT Id, Name, FilePath, IsHidden, UploadDate FROM Notices WHERE IsHidden = 0 ORDER BY UploadDate DESC";
                    dt = DBHelper.ExecuteQuery(DBHelper.GetAttendanceDBConnection(), query);
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    rptNotices.DataSource = dt;
                    rptNotices.DataBind();
                    rptNotices.Visible = true;
                    phNoNotices.Visible = false;
                }
                else
                {
                    rptNotices.Visible = false;
                    phNoNotices.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowToast("Error loading notices: " + ex.Message, "error");
            }
        }

        protected string GetNoticeBorderColor(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return "#64748b";
            string ext = Path.GetExtension(filePath).ToLower();
            switch (ext)
            {
                case ".pdf":
                    return "#ef4444";
                case ".docx":
                    return "#2563eb";
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                    return "#10b981";
                default:
                    return "#64748b";
            }
        }

        protected string GetNoticeBadgeBg(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return "#f1f5f9";
            string ext = Path.GetExtension(filePath).ToLower();
            switch (ext)
            {
                case ".pdf":
                    return "#fee2e2";
                case ".docx":
                    return "#dbeafe";
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                    return "#d1fae5";
                default:
                    return "#f1f5f9";
            }
        }

        protected string GetNoticeIconClass(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return "fas fa-file-alt";
            string ext = Path.GetExtension(filePath).ToLower();
            switch (ext)
            {
                case ".pdf":
                    return "fas fa-file-pdf";
                case ".docx":
                    return "fas fa-file-word";
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                    return "fas fa-file-image";
                default:
                    return "fas fa-file-alt";
            }
        }

        protected string GetNoticeCardStyle(object filePathObj, object isHiddenObj)
        {
            string filePath = filePathObj != null ? filePathObj.ToString() : "";
            int isHidden = isHiddenObj != null ? Convert.ToInt32(isHiddenObj) : 0;
            string borderColor = GetNoticeBorderColor(filePath);
            string style = "border-radius: 14px; border: 1px solid #e2e8f0; border-left: 5px solid " + borderColor + " !important; background: white; transition: all 0.2s; position: relative;";
            if (isHidden == 1)
            {
                style += " opacity: 0.72; background-color: #f8fafc;";
            }
            return style;
        }

        protected string GetNoticeBadgeStyle(object filePathObj)
        {
            string filePath = filePathObj != null ? filePathObj.ToString() : "";
            return "font-size: 0.72rem; padding: 4px 8px; border-radius: 6px; font-weight: 700; background: " + GetNoticeBadgeBg(filePath) + "; color: " + GetNoticeBorderColor(filePath) + ";";
        }

        protected void btnUploadNotice_Click(object sender, EventArgs e)
        {
            int role = Convert.ToInt32(Session["Role"] ?? 0);
            if (role != 1)
            {
                ShowToast("Unauthorized access.", "error");
                return;
            }

            if (!fuNotice.HasFile)
            {
                ShowToast("Please select a file to upload.", "warning");
                return;
            }

            try
            {
                string ext = Path.GetExtension(fuNotice.FileName).ToLower();
                if (ext != ".pdf" && ext != ".docx" && ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif")
                {
                    ShowToast("Unsupported file type. Only PDF, DOCX, and Images are allowed.", "error");
                    return;
                }

                // Verify file size limit (10MB)
                if (fuNotice.PostedFile.ContentLength > 10 * 1024 * 1024)
                {
                    ShowToast("File size exceeds 10MB limit.", "error");
                    return;
                }

                string noticeName = Path.GetFileNameWithoutExtension(fuNotice.FileName);
                if (string.IsNullOrEmpty(noticeName))
                {
                    noticeName = "Untitled Notice";
                }

                string uploadDir = Server.MapPath("~/Static/Uploads/Notices/");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                string fileGuid = Guid.NewGuid().ToString("N");
                string safeFileName = fileGuid + "_" + Path.GetFileName(fuNotice.FileName);
                string serverFilePath = Path.Combine(uploadDir, safeFileName);
                
                fuNotice.SaveAs(serverFilePath);

                string dbRelativePath = "~/Static/Uploads/Notices/" + safeFileName;

                string insertQuery = "INSERT INTO Notices (Name, FilePath, IsHidden) VALUES (:Name, :FilePath, 0)";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), insertQuery,
                    new OracleParameter("Name", noticeName),
                    new OracleParameter("FilePath", dbRelativePath));

                LoadNotices();
                ShowToast("Notice uploaded successfully!", "success");
            }
            catch (Exception ex)
            {
                ShowToast("Error uploading notice: " + ex.Message, "error");
            }
        }

        protected void rptNotices_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int role = Convert.ToInt32(Session["Role"] ?? 0);
            if (role != 1)
            {
                ShowToast("Unauthorized access.", "error");
                return;
            }

            int noticeId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "DeleteNotice")
            {
                try
                {
                    // Get filepath to delete physical file
                    string selectSql = "SELECT FilePath FROM Notices WHERE Id = :Id";
                    string relativePath = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), selectSql, new OracleParameter("Id", noticeId))?.ToString();

                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        try
                        {
                            string physicalPath = Server.MapPath(relativePath);
                            if (File.Exists(physicalPath))
                            {
                                File.Delete(physicalPath);
                            }
                        }
                        catch (Exception fileEx)
                        {
                            System.Diagnostics.Debug.WriteLine("Error deleting physical file: " + fileEx.Message);
                        }
                    }

                    string deleteSql = "DELETE FROM Notices WHERE Id = :Id";
                    DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), deleteSql, new OracleParameter("Id", noticeId));

                    LoadNotices();
                    ShowToast("Notice deleted successfully.", "success");
                }
                catch (Exception ex)
                {
                    ShowToast("Error deleting notice: " + ex.Message, "error");
                }
            }
            else if (e.CommandName == "ToggleHide")
            {
                try
                {
                    string selectSql = "SELECT IsHidden FROM Notices WHERE Id = :Id";
                    int currentHidden = Convert.ToInt32(DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), selectSql, new OracleParameter("Id", noticeId)));

                    int newHidden = currentHidden == 1 ? 0 : 1;

                    string updateSql = "UPDATE Notices SET IsHidden = :IsHidden WHERE Id = :Id";
                    DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), updateSql,
                        new OracleParameter("IsHidden", newHidden),
                        new OracleParameter("Id", noticeId));

                    LoadNotices();
                    string statusMsg = newHidden == 1 ? "Notice is now hidden." : "Notice is now visible.";
                    ShowToast(statusMsg, "success");
                }
                catch (Exception ex)
                {
                    ShowToast("Error toggling visibility: " + ex.Message, "error");
                }
            }
        }

        private void HandleRenameNotice(string eventArgument)
        {
            int role = Convert.ToInt32(Session["Role"] ?? 0);
            if (role != 1)
            {
                ShowToast("Unauthorized access.", "error");
                return;
            }

            if (string.IsNullOrEmpty(eventArgument) || !eventArgument.Contains("|"))
            {
                ShowToast("Invalid rename arguments.", "error");
                return;
            }

            try
            {
                string[] parts = eventArgument.Split('|');
                int noticeId = Convert.ToInt32(parts[0]);
                string newName = parts[1].Trim();

                if (string.IsNullOrEmpty(newName))
                {
                    ShowToast("Notice name cannot be empty.", "warning");
                    return;
                }

                string updateSql = "UPDATE Notices SET Name = :Name WHERE Id = :Id";
                DBHelper.ExecuteNonQuery(DBHelper.GetAttendanceDBConnection(), updateSql,
                    new OracleParameter("Name", newName),
                    new OracleParameter("Id", noticeId));

                LoadNotices();
                ShowToast("Notice renamed successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowToast("Error renaming notice: " + ex.Message, "error");
            }
        }

        protected void btnRenameSubmit_Click(object sender, EventArgs e)
        {
            string data = hfRenameData.Value;
            HandleRenameNotice(data);
        }
    }
}

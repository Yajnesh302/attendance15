using System;
using System.Web.Security;
using AttendanceApp.Utils;

namespace AttendanceApp
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected int UnreadCount { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Auto close expired contracts on page load
            try
            {
                AttendanceApp.Utils.DBHelper.AutoCloseExpiredContracts();
            }
            catch { }

            if (Session["PCNO"] == null && Page.User.Identity.IsAuthenticated)
            {
                // Session was lost (e.g., app rebuild) but auth cookie remains. Force logout.
                FormsAuthentication.SignOut();
                Response.Redirect("Login.aspx");
                return;
            }

            int role = Convert.ToInt32(Session["Role"] ?? 0);

            // Query unread remarks count for admin (server-side, no extra round-trip)
            if (role == 1)
            {
                try
                {
                    string sql = "SELECT COUNT(DISTINCT ar.SubmittedBy || '_' || ar.EmpID || '_' || ar.Message || '_' || TO_CHAR(ar.CreatedAt, 'YYYYMMDDHH24MISS')) FROM AttendanceRemarks ar WHERE ar.IsRead = 0";
                    object result = DBHelper.ExecuteScalar(DBHelper.GetAttendanceDBConnection(), sql);
                    UnreadCount = result != null && result != System.DBNull.Value ? Convert.ToInt32(result) : 0;
                }
                catch { UnreadCount = 0; }
            }

            if (role == 1)
            {
                lblUserName.InnerText = "Administrator";
                myfw.Attributes["class"] = "fas fa-user-shield text-success";
                phEmployeeMaster.Visible = true;
                phCalculation.Visible = true;
                phVendors.Visible = true;
                phContracts.Visible = true;
            }
            else
            {
                lblUserName.InnerText = "User";
                myfw.Attributes["class"] = "fas fa-user";
                phEmployeeMaster.Visible = false;
                phCalculation.Visible = false;
                phVendors.Visible = false;
                phContracts.Visible = false;
            }

            // Toggle sidebar and top nav visibility based on the page
            string currentPage = System.IO.Path.GetFileName(Request.Url.AbsolutePath);
            if (currentPage.Equals("Dashboard.aspx", StringComparison.OrdinalIgnoreCase) ||
                currentPage.Equals("Dashboard", StringComparison.OrdinalIgnoreCase))
            {
                phSidebar.Visible = true;
                phTopNav.Visible = false;
            }
            else
            {
                phSidebar.Visible = false;
                phTopNav.Visible = true;
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            Response.Redirect("Login.aspx");
        }
    }
}


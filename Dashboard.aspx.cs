using System;
using System.Web;

namespace AttendanceApp
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                int role = Convert.ToInt32(Session["Role"] ?? 0);
                if (role != 1)
                {
                    phAdmin_Emp.Visible       = false;
                    phAdmin_Calc.Visible      = false;
                    phAdmin_AdminMgmt.Visible = false;
                    phAdmin_Settings.Visible  = false;
                    phAdmin_Vendors.Visible   = false;
                    phAdmin_Contracts.Visible = false;
                    phAdmin_Remarks.Visible   = false;
                }
            }
        }
    }
}

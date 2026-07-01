<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="AttendanceApp.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Dashboard
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .dashboard-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 20px;
            margin-top: 20px;
            max-width: 1200px;
        }
        @media (max-width: 992px) { .dashboard-grid { grid-template-columns: repeat(3, 1fr); } }
        @media (max-width: 768px) { .dashboard-grid { grid-template-columns: repeat(2, 1fr); } }
        @media (max-width: 480px) { .dashboard-grid { grid-template-columns: 1fr; } }

        .card-custom {
            background: white; padding: 25px; border-radius: 14px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.08); text-align: center;
            cursor: pointer; transition: transform 0.2s, box-shadow 0.2s;
            text-decoration: none; color: #333; display: block;
            border: 1px solid #f1f5f9;
        }
        .card-custom:hover {
            transform: translateY(-6px); box-shadow: 0 12px 28px rgba(0,0,0,0.12);
            color: #4f46e5; text-decoration: none;
        }
        .card-custom h3 { margin: 10px 0; font-size: 1.4rem; font-weight: 700; }
        .card-custom p { color: #64748b; margin-bottom: 0; font-size: 0.88rem; }

        /* Send Remark card special style */
        .card-remark {
            background: linear-gradient(135deg, #4f46e5 0%, #3730a3 100%);
            color: white !important;
            border: none;
        }
        .card-remark:hover { color: white !important; }
        .card-remark p { color: rgba(255,255,255,0.8) !important; }
        .card-remark h3 { color: white; }

        /* My Remarks section */
        .my-remarks-section {
            margin-top: 36px;
            max-width: 1200px;
        }
        .section-title {
            font-size: 1.15rem; font-weight: 700; color: #0f172a;
            margin-bottom: 16px; display: flex; align-items: center; gap: 10px;
        }
        .section-title i { color: #4f46e5; }

        .my-remark-card {
            background: white; border-radius: 12px; padding: 16px 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05); margin-bottom: 12px;
            border: 1px solid #e2e8f0; border-left: 4px solid #4f46e5;
            transition: all 0.2s;
        }
        .my-remark-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.1); }
        .my-remark-card.seen { border-left-color: #10b981; }
        .my-remark-header {
            display: flex; align-items: center; justify-content: space-between;
            margin-bottom: 6px; flex-wrap: wrap; gap: 6px;
        }
        .my-remark-emp {
            font-weight: 700; color: #0f172a; font-size: 0.93rem;
            display: flex; align-items: center; gap: 6px;
        }
        .my-remark-date { font-size: 0.8rem; color: #64748b; }
        .my-remark-msg { color: #475569; font-size: 0.88rem; line-height: 1.55; margin-top: 4px; }
        .my-remark-meta { display: flex; align-items: center; gap: 10px; margin-top: 8px; }
        .badge-seen { background: #dcfce7; color: #15803d; font-size: 0.75rem; padding: 2px 10px; border-radius: 20px; font-weight: 600; }
        .badge-pending { background: #eff6ff; color: #1d4ed8; font-size: 0.75rem; padding: 2px 10px; border-radius: 20px; font-weight: 600; }

        #myRemarksEmpty { text-align: center; color: #94a3b8; padding: 30px; font-size: 0.93rem; }

        /* Modal overlay */
        #remarkModal {
            display: none; position: fixed; inset: 0;
            background: rgba(15,23,42,0.45); backdrop-filter: blur(6px);
            z-index: 100000; align-items: center; justify-content: center;
            opacity: 0; transition: opacity 0.25s;
        }
        #remarkModal.open { display: flex; opacity: 1; }
        .remark-modal-box {
            background: white; border-radius: 18px; width: 520px; max-width: 95%;
            box-shadow: 0 25px 50px -12px rgba(0,0,0,0.3);
            transform: scale(0.95); transition: transform 0.28s cubic-bezier(0.34,1.56,0.64,1);
            overflow: hidden;
        }
        #remarkModal.open .remark-modal-box { transform: scale(1); }
        .remark-modal-header {
            background: linear-gradient(135deg, #4f46e5, #3730a3);
            padding: 20px 24px; color: white;
            display: flex; align-items: center; justify-content: space-between;
        }
        .remark-modal-title { font-size: 1.1rem; font-weight: 700; display: flex; align-items: center; gap: 10px; }
        .remark-modal-close {
            background: rgba(255,255,255,0.15); border: none; color: white;
            width: 32px; height: 32px; border-radius: 50%; cursor: pointer;
            font-size: 1.1rem; display: flex; align-items: center; justify-content: center;
            transition: background 0.2s;
        }
        .remark-modal-close:hover { background: rgba(255,255,255,0.3); }
        .remark-modal-body { padding: 24px; }
        .remark-field { margin-bottom: 18px; }
        .remark-label { font-size: 0.85rem; font-weight: 700; color: #374151; margin-bottom: 6px; display: block; }
        .remark-input {
            width: 100%; padding: 10px 14px; border-radius: 8px;
            border: 1.5px solid #e2e8f0; font-size: 0.9rem; color: #1e293b;
            outline: none; transition: border-color 0.2s, box-shadow 0.2s;
            background: #f8fafc; box-sizing: border-box;
        }
        .remark-input:focus { border-color: #4f46e5; box-shadow: 0 0 0 3px rgba(79,70,229,0.12); background: white; }
        .char-counter { font-size: 0.76rem; color: #94a3b8; text-align: right; margin-top: 4px; }
        .remark-modal-footer {
            padding: 16px 24px; border-top: 1px solid #f1f5f9;
            display: flex; gap: 10px; justify-content: flex-end;
        }
        .btn-remark-cancel {
            padding: 9px 20px; border-radius: 8px; border: 1.5px solid #e2e8f0;
            background: white; color: #64748b; font-weight: 600; cursor: pointer;
            font-size: 0.88rem; transition: all 0.2s;
        }
        .btn-remark-cancel:hover { background: #f8fafc; color: #334155; }
        .btn-remark-submit {
            padding: 9px 24px; border-radius: 8px; border: none;
            background: linear-gradient(135deg, #4f46e5, #3730a3);
            color: white; font-weight: 700; cursor: pointer; font-size: 0.88rem;
            transition: all 0.2s; box-shadow: 0 4px 12px rgba(79,70,229,0.25);
            display: inline-flex; align-items: center; gap: 7px;
        }
        .btn-remark-submit:hover { transform: translateY(-1px); box-shadow: 0 6px 16px rgba(79,70,229,0.35); }
        .btn-remark-submit:disabled { opacity: 0.6; cursor: not-allowed; transform: none; }

        #remarkEmpInfo {
            background: #eff6ff; border: 1px solid #bfdbfe; border-radius: 8px;
            padding: 8px 12px; font-size: 0.82rem; color: #1d4ed8;
            margin-top: 6px; display: none;
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h2 style="font-weight:800; color:#0f172a;">HR Dashboard</h2>
    <hr style="border-color:#e2e8f0;" />

    <div class="dashboard-grid">
        <asp:PlaceHolder ID="phAdmin_Emp" runat="server">
            <a href="Employee.aspx" class="card-custom" style="border-left: 4px solid #4f46e5; position:relative;">
                <i class="fas fa-users fa-3x mb-3" style="color:#4f46e5;"></i>
                <h3 style="color:#4f46e5;">Employee</h3>
                <p>Manage employees</p>
            </a>
        </asp:PlaceHolder>

        <a href="Attendance.aspx" class="card-custom" style="border-left: 4px solid #10b981; position:relative;">
            <i class="fas fa-calendar-check fa-3x mb-3" style="color:#10b981;"></i>
            <h3 style="color:#10b981;">Attendance</h3>
            <p>Mark attendance</p>
        </a>

        <a href="Ledger.aspx" class="card-custom" style="border-left: 4px solid #0ea5e9; position:relative;">
            <i class="fas fa-book fa-3x mb-3" style="color:#0ea5e9;"></i>
            <h3 style="color:#0ea5e9;">Ledger</h3>
            <p>Leave tracking</p>
        </a>

        <asp:PlaceHolder ID="phAdmin_Calc" runat="server">
            <a href="Calculation.aspx" class="card-custom" style="border-left: 4px solid #f59e0b; position:relative;">
                <i class="fas fa-calculator fa-3x mb-3" style="color:#f59e0b;"></i>
                <h3 style="color:#f59e0b;">Calculation</h3>
                <p>Salary processing</p>
            </a>

            <a href="Documents.aspx" class="card-custom" style="border-left: 4px solid #64748b; position:relative;">
                <i class="fas fa-file-alt fa-3x mb-3" style="color:#64748b;"></i>
                <h3 style="color:#64748b;">Documents</h3>
                <p>Manage employee documents</p>
            </a>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phAdmin_AdminMgmt" runat="server">
            <a href="AdminManagement.aspx" class="card-custom" style="border-left: 4px solid #ef4444; position:relative;">
                <i class="fas fa-user-shield fa-3x mb-3" style="color:#ef4444;"></i>
                <h3 style="color:#ef4444;">Admin Management</h3>
                <p>Configure administrators</p>
            </a>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phAdmin_Settings" runat="server">
            <a href="Settings.aspx" class="card-custom" style="border-left: 4px solid #64748b; position:relative;">
                <i class="fas fa-cog fa-3x mb-3" style="color:#64748b;"></i>
                <h3 style="color:#64748b;">Settings</h3>
                <p>Manage divisions &amp; categories</p>
            </a>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phAdmin_Vendors" runat="server">
            <a href="Vendors.aspx" class="card-custom" style="border-left: 4px solid #0ea5e9; position:relative;">
                <i class="fas fa-handshake fa-3x mb-3" style="color:#0ea5e9;"></i>
                <h3 style="color:#0ea5e9;">Vendors</h3>
                <p>Manage vendors &amp; agencies</p>
            </a>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phAdmin_Contracts" runat="server">
            <a href="Contracts.aspx" class="card-custom" style="border-left: 4px solid #f59e0b; position:relative;">
                <i class="fas fa-file-signature fa-3x mb-3" style="color:#f59e0b;"></i>
                <h3 style="color:#f59e0b;">Contracts</h3>
                <p>Configure contract periods</p>
            </a>
        </asp:PlaceHolder>

        <%-- Notices card (visible to everyone) --%>
        <a href="Notices.aspx" class="card-custom" style="border-left: 4px solid #10b981; position:relative;">
            <i class="fas fa-bullhorn fa-3x mb-3" style="color:#10b981;"></i>
            <h3 style="color:#10b981;">Notices</h3>
            <p>View announcements &amp; notices</p>
        </a>

        <%-- Admin: Remarks inbox card --%>
        <asp:PlaceHolder ID="phAdmin_Remarks" runat="server">
            <a href="Remarks.aspx" class="card-custom" style="border-left: 4px solid #4f46e5; position:relative;">
                <i class="fas fa-inbox fa-3x mb-3" style="color:#4f46e5;"></i>
                <h3 style="color:#4f46e5;">Remarks Inbox</h3>
                <p>View attendance remarks from users</p>
            </a>
        </asp:PlaceHolder>

        <%-- Remarks card (regular users only) --%>
        <% if (Convert.ToInt32(Session["Role"] ?? 0) != 1) { %>
        <a href="UserRemarks.aspx" class="card-custom" style="border-left: 4px solid #4f46e5; position:relative;">
            <i class="fas fa-comment-alt fa-3x mb-3" style="color:#4f46e5;"></i>
            <h3 style="color:#4f46e5;">Remarks</h3>
            <p>View sent &amp; report attendance corrections</p>
        </a>
        <% } %>
    </div>
</asp:Content>


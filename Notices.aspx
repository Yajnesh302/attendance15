<%@ Page Title="Notices & Announcements" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Notices.aspx.cs" Inherits="AttendanceApp.Notices" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Notices &amp; Announcements
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .page-header-block {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 24px;
            flex-wrap: wrap;
            gap: 12px;
        }
        .page-title-container {
            display: flex;
            align-items: center;
            gap: 14px;
        }
        .page-title-icon {
            width: 48px;
            height: 48px;
            border-radius: 14px;
            background: linear-gradient(135deg, #10b981, #059669);
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 1.3rem;
            box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
        }
        .page-title-main {
            font-size: 1.6rem;
            font-weight: 800;
            color: #0f172a;
            margin: 0;
        }
        .page-title-sub {
            font-size: 0.85rem;
            color: #64748b;
            margin: 0;
        }

        .btn-back-dashboard {
            padding: 10px 20px;
            border-radius: 10px;
            border: 1px solid #cbd5e1;
            background: white;
            color: #475569;
            font-weight: 700;
            font-size: 0.9rem;
            cursor: pointer;
            transition: all 0.2s;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }
        .btn-back-dashboard:hover {
            background: #f8fafc;
            color: #1e293b;
            border-color: #94a3b8;
            transform: translateY(-1px);
        }

        .notice-card {
            transition: transform 0.2s, box-shadow 0.2s;
        }
        .notice-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 10px 20px rgba(0,0,0,0.06) !important;
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Page Header -->
    <div class="page-header-block">
        <div class="page-title-container">
            <div class="page-title-icon">
                <i class="fas fa-bullhorn"></i>
            </div>
            <div>
                <p class="page-title-main">Notices &amp; Announcements</p>
                <p class="page-title-sub">View and download announcements uploaded by administrators</p>
            </div>
        </div>
        <a href="Dashboard.aspx" class="btn-back-dashboard">
            <i class="fas fa-arrow-left"></i> Back to Dashboard
        </a>
    </div>

    <!-- Admin Notice Upload Section -->
    <asp:PlaceHolder ID="phAdminNoticeUpload" runat="server">
        <div class="card mb-4 shadow-sm border-0" style="border-radius: 14px; background: white; border: 1px solid #e2e8f0;">
            <div class="card-body p-4">
                <h5 class="card-title font-weight-bold text-dark mb-3" style="font-size: 1.05rem; display: flex; align-items: center; gap: 8px;">
                    <i class="fas fa-upload text-success"></i> Upload New Announcement File
                </h5>
                <div class="row align-items-center">
                    <div class="col-md-9 mb-3 mb-md-0">
                        <label class="small font-weight-bold text-muted d-block mb-2">Select File (PDF, DOCX, Images)</label>
                        <div class="d-flex align-items-center flex-wrap" style="gap: 10px;">
                            <label for="<%= fuNotice.ClientID %>" class="btn btn-outline-secondary mb-0 animate-hover" style="border-radius: 8px; padding: 10px 20px; font-weight: 700; font-size: 0.88rem; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; border: 1.5px solid #cbd5e1; background: #f8fafc;">
                                <i class="fas fa-folder-open text-primary" style="font-size: 1.05rem;"></i> Choose File...
                            </label>
                            <asp:FileUpload ID="fuNotice" runat="server" accept=".pdf,.docx,.png,.jpg,.jpeg,.gif" style="display: none;" onchange="updateFileNameLabel(this);" />
                            <span id="lblSelectedFileName" class="text-muted small" style="font-weight: 600; font-size: 0.9rem; margin-left: 12px;">No file chosen</span>
                        </div>
                    </div>
                    <div class="col-md-3 text-right mt-3 mt-md-0">
                        <asp:Button ID="btnUploadNotice" runat="server" Text="Upload Notice" CssClass="btn btn-success font-weight-bold w-100" style="border-radius: 8px; padding: 10px 20px; font-size: 0.9rem; background: linear-gradient(135deg, #10b981, #059669); border: none; box-shadow: 0 4px 12px rgba(16,185,129,0.25); height: 42px;" OnClick="btnUploadNotice_Click" />
                    </div>
                </div>
            </div>
        </div>
    </asp:PlaceHolder>

    <!-- Notices Cards List -->
    <div class="row">
        <asp:Repeater ID="rptNotices" runat="server" OnItemCommand="rptNotices_ItemCommand">
            <ItemTemplate>
                <div class="col-12 col-md-6 col-lg-4 mb-4">
                    <div class="card shadow-sm border-0 notice-card" style='<%# GetNoticeCardStyle(Eval("FilePath"), Eval("IsHidden")) %>'>
                        <div class="card-body p-4 d-flex flex-column justify-content-between" style="min-height: 160px;">
                            <div>
                                <div class="d-flex align-items-center justify-content-between mb-2">
                                    <span class='badge' style='<%# GetNoticeBadgeStyle(Eval("FilePath")) %>'>
                                        <i class='<%# GetNoticeIconClass(Eval("FilePath").ToString()) %>'></i>
                                        <%# System.IO.Path.GetExtension(Eval("FilePath").ToString()).ToUpper().Replace(".", "") %>
                                    </span>
                                    <span class="small text-muted" style="font-size: 0.75rem;"><%# Convert.ToDateTime(Eval("UploadDate")).ToString("dd-MMM-yyyy") %></span>
                                </div>
                                <h6 class="font-weight-bold text-dark notice-title mb-2" style="font-size: 0.98rem; line-height: 1.4;"><%# Eval("Name") %></h6>
                                
                                <div class="notice-meta mb-3">
                                    <%# Convert.ToInt32(Eval("IsHidden")) == 1 ? "<span class='badge badge-warning' style='font-size:0.7rem; font-weight:600;'><i class='fas fa-eye-slash mr-1'></i>Hidden</span>" : "" %>
                                </div>
                            </div>
                            
                            <div class="d-flex align-items-center justify-content-between pt-3 border-top" style="border-top-color: #f1f5f9 !important;">
                                <%-- View Link --%>
                                <a href='<%# ResolveUrl(Eval("FilePath").ToString()) %>' target="_blank" class="btn btn-sm btn-outline-primary font-weight-bold" style="border-radius: 6px; padding: 5px 12px; font-size: 0.8rem; border-color: #4f46e5; color: #4f46e5;">
                                    <i class="fas fa-external-link-alt mr-1"></i>View File
                                </a>

                                <%-- Admin Action Controls --%>
                                <asp:PlaceHolder ID="phAdminActions" runat="server" Visible='<%# Convert.ToInt32(Session["Role"] ?? 0) == 1 %>'>
                                    <div class="btn-group shadow-sm" style="border-radius: 6px; overflow: hidden;">
                                        <button type="button" class="btn btn-sm btn-light text-primary" style="background:#f8fafc; border: 1px solid #e2e8f0; font-size:0.8rem;" title="Rename" data-id='<%# Eval("Id") %>' data-name='<%# Eval("Name") != null ? HttpUtility.HtmlEncode(Eval("Name").ToString()) : "" %>' onclick="renameNotice(this); return false;">
                                            <i class="fas fa-edit"></i>
                                        </button>
                                        <asp:LinkButton ID="btnToggleHide" runat="server" CommandName="ToggleHide" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-sm btn-light text-warning" style="background:#f8fafc; border: 1px solid #e2e8f0; font-size:0.8rem;" ToolTip='<%# Convert.ToInt32(Eval("IsHidden")) == 1 ? "Unhide Notice" : "Hide Notice" %>'>
                                            <i class='<%# Convert.ToInt32(Eval("IsHidden")) == 1 ? "fas fa-eye" : "fas fa-eye-slash" %>'></i>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnDeleteNotice" runat="server" CommandName="DeleteNotice" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-sm btn-light text-danger" style="background:#f8fafc; border: 1px solid #e2e8f0; font-size:0.8rem;" OnClientClick="return confirm('Are you sure you want to delete this notice?');" ToolTip="Delete Notice">
                                            <i class="fas fa-trash"></i>
                                        </asp:LinkButton>
                                    </div>
                                </asp:PlaceHolder>
                            </div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
            
            <asp:PlaceHolder ID="phNoNotices" runat="server" Visible="false">
                <div class="col-12 text-center py-5">
                    <i class="fas fa-bullhorn fa-3x text-muted mb-3" style="opacity: 0.4;"></i>
                    <h6 class="text-muted font-weight-bold">No announcements or notices posted yet.</h6>
                </div>
            </asp:PlaceHolder>
        </div>
    </div>

    <!-- Hidden controls for Rename Postback -->
    <asp:HiddenField ID="hfRenameData" runat="server" />
    <asp:Button ID="btnRenameSubmit" runat="server" OnClick="btnRenameSubmit_Click" style="display:none;" />

    <%-- SweetAlert2 Rename script --%>
    <script>
        function updateFileNameLabel(input) {
            var lbl = document.getElementById("lblSelectedFileName");
            if (input.files && input.files.length > 0) {
                lbl.textContent = input.files[0].name;
                lbl.className = "text-success small font-weight-bold";
                lbl.style.marginLeft = "12px";
            } else {
                lbl.textContent = "No file chosen";
                lbl.className = "text-muted small";
                lbl.style.marginLeft = "12px";
            }
        }

        function renameNotice(btn) {
            var id = btn.getAttribute("data-id");
            var currentName = btn.getAttribute("data-name") || "";
            if (typeof Swal === 'undefined') {
                var newName = prompt("Rename Notice:", currentName);
                if (newName && newName.trim() !== "") {
                    triggerRename(id, newName.trim());
                }
                return;
            }

            Swal.fire({
                title: 'Rename Notice',
                input: 'text',
                inputValue: currentName,
                inputPlaceholder: 'Enter new display name...',
                showCancelButton: true,
                confirmButtonText: 'Save',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#10b981',
                cancelButtonColor: '#64748b',
                inputValidator: (value) => {
                    if (!value || value.trim() === '') {
                        return 'You need to write a name!';
                    }
                }
            }).then((result) => {
                if (result.isConfirmed && result.value) {
                    triggerRename(id, result.value.trim());
                }
            });
        }

        function triggerRename(id, newName) {
            document.getElementById("<%= hfRenameData.ClientID %>").value = id + '|' + newName;
            document.getElementById("<%= btnRenameSubmit.ClientID %>").click();
        }
    </script>
</asp:Content>

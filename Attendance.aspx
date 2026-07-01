<%@ Page Title="Attendance" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Attendance.aspx.cs" Inherits="AttendanceApp.Attendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Attendance Management
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .emp-name-click {
            color: #4f46e5;
            font-weight: 600;
            cursor: pointer;
            transition: color 0.15s ease;
        }
        .emp-name-click:hover {
            color: #3730a3;
            text-decoration: underline;
        }
        /* Custom spacing and layout for controls */
        .calc-controls-container {
            display: flex;
            flex-wrap: wrap;
            align-items: flex-end;
            justify-content: space-between;
            width: 100%;
        }
        .calc-left-group {
            display: flex;
            flex-wrap: wrap;
            align-items: flex-end;
            flex-grow: 1;
        }
        .calc-control-item {
            margin-right: 12px;
            margin-bottom: 6px;
            min-width: 110px;
            flex: 1 1 auto;
        }
        .calc-control-item-category {
            margin-right: 12px;
            margin-bottom: 6px;
            min-width: 160px;
            flex: 1.5 1 auto;
        }
        .calc-control-item-wage {
            margin-right: 12px;
            margin-bottom: 6px;
            min-width: 220px;
            flex: 2 1 auto;
        }
        .calc-right-group {
            display: flex;
            flex-wrap: wrap;
            align-items: flex-end;
            margin-bottom: 6px;
        }
        
        /* Select and Input styling to ensure matching heights */
        .calc-left-group select.form-control, 
        .calc-left-group input.form-control {
            height: 38px !important;
            font-size: 0.9rem;
            border-radius: 4px;
            border: 1px solid #d1d3e2;
            color: #111827;
            font-weight: 500;
            box-shadow: inset 0 1px 2px rgba(0,0,0,0.05);
        }
        .calc-left-group select.form-control:focus, 
        .calc-left-group input.form-control:focus {
            border-color: #4f46e5;
            box-shadow: 0 0 0 0.2rem rgba(79,70,229,0.25);
        }

        /* Holiday Buttons Styling */
        .calc-left-group .btn {
            height: 38px !important;
            font-size: 0.85rem;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            padding: 0 12px;
            transition: all 0.2s ease;
            cursor: pointer;
            font-weight: bold;
        }
        
        /* Standalone Actions Button Styling */
        .btn-custom {
            height: 38px !important;
            padding: 0 18px;
            font-size: 0.9rem;
            font-weight: 600;
            border-radius: 4px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            border: none;
            transition: all 0.2s ease;
            box-shadow: 0 2px 4px rgba(0,0,0,0.08);
            cursor: pointer;
            color: white !important;
        }
        .btn-custom i {
            margin-right: 6px;
        }
        .btn-custom-calc {
            background-color: #10b981;
            margin-right: 8px;
        }
        .btn-custom-calc:hover {
            background-color: #059669;
            box-shadow: 0 4px 8px rgba(16,185,129,0.2);
            transform: translateY(-1px);
        }
        .btn-custom-export {
            background-color: #17a2b8;
        }
        .btn-custom-export:hover {
            background-color: #117a8b;
            box-shadow: 0 4px 8px rgba(23,162,184,0.2);
            transform: translateY(-1px);
        }
        .wrapper {
            height: calc(100vh - 195px);
            min-height: 450px;
            overflow: auto;
            border: 1px solid #e3e6f0;
            background: white;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.05);
        }
        table.att-table {
            border-collapse: collapse;
            width: max-content;
            min-width: 100%;
        }
        .att-table th, .att-table td {
            border: 1px solid #ddd;
            padding: 4px;
            text-align: center;
            font-size: 14px;
            min-width: 40px;
            height: 50px;
            position: relative;
            vertical-align: top;
        }
        .att-table th {
            position: sticky;
            top: 0;
            background: #f0f2f5;
            z-index: 10;
        }
        .att-table th.sortable-header {
            cursor: pointer;
            user-select: none;
            transition: background-color 0.2s ease;
        }
        .att-table th.sortable-header:hover {
            background-color: #e2e8f0 !important;
        }
        #tbody tr {
            transition: background-color 0.15s ease;
        }
        #tbody tr:hover {
            background-color: #eef2ff !important;
        }
        #tbody tr:hover td {
            box-shadow: inset 0 0 0 9999px rgba(79, 70, 229, 0.06);
        }
        .green { background: #d9f7be !important; }
        .red { background: #ffa39e !important; }
        .royal-blue { background: #4169E1 !important; color: white !important; }
        .light-yellow { background: #fff9c4 !important; }
        .gray { background: #e5e7eb !important; color: #666; }
        input.att {
            width: 35px;
            text-align: center;
            border: 1px solid #999;
            font-weight: bold;
            background: transparent;
            outline: none;
            font-size: 14px;
            margin-top: 2px;
        }
        .label-text {
            display: block;
            font-size: 11px;
            font-weight: bold;
            color: #333;
            margin-top: 1px;
        }
        select.leave-opt {
            position: absolute;
            bottom: 1px;
            left: 1px;
            width: calc(100% - 2px);
            font-size: 10px;
            padding: 0;
            height: 16px;
            box-sizing: border-box;
        }
        /* Elegant Toast Container at Top Right */
        #toast-container {
            position: fixed;
            top: 24px;
            right: 24px;
            display: flex;
            flex-direction: column;
            gap: 12px;
            z-index: 200000;
            pointer-events: none;
        }
        
        .modern-toast {
            display: flex;
            align-items: center;
            gap: 14px;
            background: rgba(255, 255, 255, 0.9);
            backdrop-filter: blur(12px) saturate(180%);
            -webkit-backdrop-filter: blur(12px) saturate(180%);
            border-radius: 12px;
            padding: 14px 20px;
            min-width: 320px;
            max-width: 420px;
            color: #1e293b;
            font-size: 0.92rem;
            font-weight: 600;
            font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04), inset 0 0 0 1px rgba(255, 255, 255, 0.5);
            transform: translateX(120%);
            transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1), opacity 0.3s ease;
            opacity: 0;
            pointer-events: auto;
            border-left: 6px solid #64748b;
        }
        
        .modern-toast.toast-show {
            transform: translateX(0);
            opacity: 1;
        }
        
        .modern-toast.toast-hide {
            transform: translateY(-20px) scale(0.9);
            opacity: 0;
        }
        
        .toast-icon {
            font-size: 1.35rem;
            flex-shrink: 0;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        /* Toast Alert States with Curated Color Accents */
        .toast-success {
            border-left-color: #10b981;
            background: rgba(240, 253, 250, 0.95);
        }
        .toast-success .toast-icon {
            color: #10b981;
        }
        
        .toast-error {
            border-left-color: #ef4444;
            background: rgba(254, 242, 242, 0.95);
        }
        .toast-error .toast-icon {
            color: #ef4444;
        }
        
        .toast-warning {
            border-left-color: #f59e0b;
            background: rgba(255, 251, 235, 0.95);
        }
        .toast-warning .toast-icon {
            color: #f59e0b;
        }
        
        .toast-info {
            border-left-color: #3b82f6;
            background: rgba(239, 246, 255, 0.95);
        }
        .toast-info .toast-icon {
            color: #3b82f6;
        }
        
        .toast-close-btn {
            background: transparent;
            border: none;
            color: #94a3b8;
            cursor: pointer;
            font-size: 1.2rem;
            padding: 2px;
            line-height: 1;
            transition: color 0.15s ease;
            margin-left: auto;
        }
        .toast-close-btn:hover {
            color: #475569;
        }

        /* Custom styled Confirm Dialog Modal */
        #confirmModal, #globalAdjustModal {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100vw;
            height: 100vh;
            background: rgba(15, 23, 42, 0.4);
            backdrop-filter: blur(8px);
            -webkit-backdrop-filter: blur(8px);
            z-index: 100000;
            align-items: center;
            justify-content: center;
            opacity: 0;
            transition: opacity 0.3s cubic-bezier(0.16, 1, 0.3, 1);
        }
        
        .confirm-modal-box {
            background: rgba(255, 255, 255, 0.95);
            border-radius: 16px;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25), inset 0 0 0 1px rgba(255, 255, 255, 0.6);
            width: 480px;
            max-width: 90%;
            transform: scale(0.92);
            transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
            overflow: hidden;
            font-family: 'Segoe UI', system-ui, sans-serif;
            border: 1px solid rgba(226, 232, 240, 0.8);
        }
        
        .confirm-modal-header {
            background: #f8fafc;
            padding: 20px 24px;
            border-bottom: 1px solid #e2e8f0;
            display: flex;
            align-items: center;
            gap: 14px;
        }
        
        .confirm-modal-icon-container {
            background: #fef3c7;
            color: #d97706;
            width: 42px;
            height: 42px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 4px 6px -1px rgba(217, 119, 6, 0.1);
        }
        
        .confirm-modal-title {
            font-size: 1.25rem;
            font-weight: 700;
            color: #0f172a;
            letter-spacing: -0.01em;
        }
        
        .confirm-modal-body {
            padding: 24px;
            font-size: 1rem;
            line-height: 1.6;
            color: #334155;
        }
        
        .confirm-modal-footer {
            background: #f8fafc;
            padding: 16px 24px;
            border-top: 1px solid #e2e8f0;
            display: flex;
            justify-content: flex-end;
            gap: 10px;
        }
        
        .btn-modal-action {
            padding: 10px 18px;
            font-size: 0.88rem;
            font-weight: 600;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
            border: none;
            display: inline-flex;
            align-items: center;
            justify-content: center;
        }
        
        .btn-modal-cancel {
            border: 1px solid #cbd5e1;
            background: white;
            color: #475569;
        }
        .btn-modal-cancel:hover {
            background: #f1f5f9;
            color: #1e293b;
            border-color: #94a3b8;
        }
        
        .btn-modal-discard {
            background: #fee2e2;
            color: #dc2626;
            border: 1px solid #fecaca;
        }
        .btn-modal-discard:hover {
            background: #fecaca;
            color: #b91c1c;
            box-shadow: 0 4px 12px rgba(220, 38, 38, 0.15);
        }
        
        .btn-modal-save {
            background: linear-gradient(135deg, #4f46e5 0%, #4338ca 100%);
            color: white;
            box-shadow: 0 4px 12px rgba(79, 70, 229, 0.25);
        }
        .btn-modal-save:hover {
            background: linear-gradient(135deg, #4338ca 0%, #3730a3 100%);
            box-shadow: 0 6px 16px rgba(79, 70, 229, 0.35);
            transform: translateY(-1px);
        }
        
        .btn-purple {
            background-color: #9333ea;
            color: white;
            border: none;
            padding: 8px 15px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            font-weight: bold;
        }
        .btn-purple:hover {
            background-color: #7e22ce;
        }

        #loadingOverlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100vw;
            height: 100vh;
            background: rgba(255, 255, 255, 0.7);
            backdrop-filter: blur(4px);
            -webkit-backdrop-filter: blur(4px);
            z-index: 99999;
            align-items: center;
            justify-content: center;
            flex-direction: column;
            font-family: 'Segoe UI', system-ui, sans-serif;
        }
        .spinner-border-custom {
            width: 3.5rem;
            height: 3.5rem;
            border: 5px solid #e2e8f0;
            border-top: 5px solid #4f46e5;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }
        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        /* Mini Leave Popup on click/focus */
        .mini-leave-popup {
            position: absolute;
            width: 250px;
            background: rgba(255, 255, 255, 0.98);
            backdrop-filter: blur(8px);
            -webkit-backdrop-filter: blur(8px);
            border-radius: 8px;
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
            border: 1px solid rgba(226, 232, 240, 0.8);
            border-left: 4px solid #10b981;
            padding: 10px 12px;
            z-index: 99999;
            font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
            font-size: 0.82rem;
            color: #1f2937;
            display: none;
            opacity: 0;
            transition: opacity 0.15s ease-out;
            pointer-events: auto;
        }
        .mini-leave-popup.show {
            display: block;
            opacity: 1;
        }
        .mini-leave-item {
            display: flex;
            justify-content: space-between;
            margin-bottom: 4px;
            line-height: 1.4;
        }
        .mini-leave-item:last-child {
            margin-bottom: 0;
        }
        .mini-leave-label {
            font-weight: 600;
            color: #4b5563;
        }
        .mini-leave-value {
            font-weight: 500;
            color: #111827;
            text-align: right;
            padding-left: 10px;
        }

        /* Floating Leave Balance Popup at Bottom Left */
        .leave-info-popup {
            position: fixed;
            bottom: 24px;
            left: 24px;
            width: 310px;
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(8px);
            -webkit-backdrop-filter: blur(8px);
            border-radius: 12px;
            box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1);
            border: 1px solid rgba(226, 232, 240, 0.8);
            border-left: 5px solid #4f46e5;
            padding: 15px;
            z-index: 9999;
            font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
            display: none;
            opacity: 0;
            transform: translateX(-30px);
            transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
            pointer-events: auto;
        }
        .leave-info-popup.show {
            display: block;
            opacity: 1;
            transform: translateX(0);
        }
        .leave-popup-close {
            position: absolute;
            top: 8px;
            right: 12px;
            background: transparent;
            border: none;
            font-size: 1.25rem;
            color: #94a3b8;
            cursor: pointer;
            line-height: 1;
            transition: color 0.15s ease;
        }
        .leave-popup-close:hover {
            color: #475569;
        }
        .leave-popup-header {
            display: flex;
            align-items: center;
            border-bottom: 1px solid #f1f5f9;
            padding-bottom: 6px;
            margin-bottom: 8px;
        }
        .leave-popup-title {
            font-weight: 700;
            color: #0f172a;
            font-size: 0.92rem;
            letter-spacing: -0.01em;
        }
        .leave-popup-item {
            display: flex;
            justify-content: space-between;
            margin-bottom: 4px;
            font-size: 0.86rem;
        }
        .leave-popup-label {
            color: #64748b;
            font-weight: 500;
        }
        .leave-popup-value {
            color: #1e293b;
            font-weight: 600;
        }
        .leave-popup-divider {
            height: 1px;
            background: #f1f5f9;
            margin: 6px 0;
        }
        /* Cell Remarks Indicator */
        .has-remarks {
            position: relative;
        }
        .has-remarks::after {
            content: '';
            position: absolute;
            top: 2px;
            right: 2px;
            width: 0;
            height: 0;
            border-style: solid;
            border-width: 0 6px 6px 0;
            border-color: transparent #3b82f6 transparent transparent;
            pointer-events: none;
        }
        /* Custom Saturday Context Menu Styles */
        .sat-context-menu {
            display: none;
            position: absolute;
            z-index: 10000;
            background: rgba(255, 255, 255, 0.96);
            backdrop-filter: blur(8px);
            border: 1px solid rgba(226, 232, 240, 0.8);
            border-radius: 8px;
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.1);
            padding: 6px 0;
            min-width: 220px;
            transform: scale(0.95);
            transform-origin: top left;
            transition: transform 0.15s cubic-bezier(0.16, 1, 0.3, 1), opacity 0.15s cubic-bezier(0.16, 1, 0.3, 1);
            opacity: 0;
            pointer-events: none;
        }
        .sat-context-menu.show {
            display: block;
            transform: scale(1);
            opacity: 1;
            pointer-events: auto;
        }
        .sat-context-item {
            padding: 8px 16px;
            cursor: pointer;
            font-size: 13px;
            color: #334155;
            display: flex;
            align-items: center;
            gap: 10px;
            transition: background-color 0.15s ease, color 0.15s ease;
        }
        .sat-context-item:hover {
            background-color: #f1f5f9;
            color: #4f46e5;
        }
        .sat-context-item i {
            font-size: 14px;
            width: 16px;
            text-align: center;
        }
        
        /* Floating Attendance Correction Request Banner */
        #correctionRemarkBanner {
            position: fixed;
            top: 90px;
            right: 24px;
            width: 380px;
            max-width: 90%;
            z-index: 100000;
            box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.15), 0 8px 10px -6px rgba(0, 0, 0, 0.15);
            animation: slideInRight 0.4s cubic-bezier(0.16, 1, 0.3, 1);
        }
        @keyframes slideInRight {
            from { transform: translateX(120%); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    
    <div id="toast-container"></div>

    <div id="loadingOverlay">
        <div class="spinner-border-custom" role="status"></div>
        <div id="loadingText" style="margin-top: 16px; font-size: 1.1rem; font-weight: 700; color: #0f172a;">Loading Attendance Data...</div>
    </div>
    
    <!-- Custom Confirm Dialog Modal -->
    <div id="confirmModal">
        <div class="confirm-modal-box">
            <div class="confirm-modal-header">
                <div class="confirm-modal-icon-container">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <span class="confirm-modal-title">Unsaved Changes</span>
            </div>
            <div class="confirm-modal-body">
                You have unsaved changes in the attendance grid. What would you like to do?
            </div>
            <div class="confirm-modal-footer">
                <button id="btnModalCancel" type="button" class="btn-modal-action btn-modal-cancel">Cancel</button>
                <button id="btnModalDiscard" type="button" class="btn-modal-action btn-modal-discard">Discard Changes</button>
                <button id="btnModalSave" type="button" class="btn-modal-action btn-modal-save">Save & Continue</button>
            </div>
        </div>
    </div>

    <!-- Custom Pairing Confirm Modal -->
    <div id="pairingModal" style="display: none; position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(15, 23, 42, 0.4); backdrop-filter: blur(8px); -webkit-backdrop-filter: blur(8px); z-index: 100000; align-items: center; justify-content: center; opacity: 0; transition: opacity 0.3s cubic-bezier(0.16, 1, 0.3, 1);">
        <div class="confirm-modal-box">
            <div class="confirm-modal-header" style="background: #f0fdf4; border-bottom: 1px solid #bbf7d0;">
                <div class="confirm-modal-icon-container" style="background: #dcfce7; color: #15803d;">
                    <i class="fas fa-users-cog"></i>
                </div>
                <span class="confirm-modal-title">Half Day Pairing Options</span>
            </div>
            <div class="confirm-modal-body">
                Please select the type of pairing to apply for this employee's half day:
                <div style="margin-top: 12px; font-size: 0.88rem; color: #64748b; line-height: 1.5;">
                    &bull; <strong>Paid Pairing:</strong> Value becomes 1 (Present) and deducts 1 Paid Leave from their balance.<br/>
                    &bull; <strong>Unpaid Pairing:</strong> Value becomes 0 (Absent) without affecting their paid leave balance.
                </div>
            </div>
            <div class="confirm-modal-footer">
                <button id="btnPairingCancel" type="button" class="btn-modal-action btn-modal-cancel">Cancel</button>
                <button id="btnPairingUnpaid" type="button" class="btn-modal-action btn-modal-discard" style="background: #fee2e2; color: #dc2626; border: 1px solid #fecaca;">Unpaid (Value: 0)</button>
                <button id="btnPairingPaid" type="button" class="btn-modal-action btn-modal-save" style="background: linear-gradient(135deg, #10b981 0%, #059669 100%); box-shadow: 0 4px 12px rgba(16, 185, 129, 0.25);">Paid (Value: 1)</button>
            </div>
        </div>
    </div>

    <!-- Custom Global Adjust Modal -->
    <div id="globalAdjustModal">
        <div class="confirm-modal-box">
            <div class="confirm-modal-header" style="background: #faf5ff; border-bottom: 1px solid #e9d5ff;">
                <div class="confirm-modal-icon-container" style="background: #f3e8ff; color: #9333ea;">
                    <i class="fas fa-sliders-h"></i>
                </div>
                <span class="confirm-modal-title">Global Adjustment</span>
            </div>
            <div class="confirm-modal-body">
                <div style="font-size: 0.95rem; color: #475569; margin-bottom: 16px;">
                    Apply a global offset value (in days) to employee totals for this month.
                </div>
                <div class="form-group mb-3">
                    <label for="globalAdjustCatSel" class="font-weight-bold" style="font-size: 0.88rem; color: #334155; margin-bottom: 6px;">Target Category *</label>
                    <select id="globalAdjustCatSel" class="form-control" style="height: 42px; font-size: 1rem; border-radius: 8px;" onchange="updateGlobalAdjustModalCurrentVal()"></select>
                </div>
                <div style="background: #f8fafc; border-radius: 8px; padding: 12px; border: 1px solid #e2e8f0; margin-bottom: 16px;">
                    <span style="font-weight: 600; color: #1e293b;">Current Adjustment:</span>
                    <span id="globalAdjustCurrentVal" style="font-weight: 700; color: #9333ea; margin-left: 6px;">0</span>
                </div>
                <div class="form-group mb-0">
                    <label for="globalAdjustInput" class="font-weight-bold" style="font-size: 0.88rem; color: #334155; margin-bottom: 6px;">New Adjustment Value</label>
                    <input type="number" id="globalAdjustInput" class="form-control" step="0.5" placeholder="e.g. 1, -1, 0.5, -0.5" style="height: 42px; font-size: 1rem; border-radius: 8px;" />
                </div>
            </div>
            <div class="confirm-modal-footer">
                <button id="btnGlobalAdjustCancel" type="button" class="btn-modal-action btn-modal-cancel">Cancel</button>
                <button id="btnGlobalAdjustApply" type="button" class="btn-modal-action btn-modal-save" style="background: linear-gradient(135deg, #9333ea 0%, #7e22ce 100%); box-shadow: 0 4px 12px rgba(147, 51, 234, 0.25);">Apply</button>
            </div>
        </div>
    </div>

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h2 class="h3 mb-0 text-gray-800">Attendance Management</h2>
    </div>

    <!-- Pinned Attendance Correction Request Banner -->
    <div id="correctionRemarkBanner" style="display:none;">
        <div class="card border-0 shadow-sm" style="border-radius: 12px; border-left: 5px solid #4f46e5 !important; background: #faf5ff;">
            <div class="card-body p-3">
                <div class="d-flex align-items-start justify-content-between">
                    <div class="d-flex align-items-start" style="gap: 12px;">
                        <div style="background: #e0e7ff; color: #4f46e5; width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1.1rem; flex-shrink: 0;">
                            <i class="fas fa-magic"></i>
                        </div>
                        <div>
                            <h6 class="font-weight-bold mb-1" style="color: #1e1b4b; font-size: 0.95rem;">Attendance Correction Request</h6>
                            <p class="small mb-2" style="color: #4338ca; font-weight: 600;" id="bannerMeta"></p>
                            <div class="p-2 rounded" style="background: white; border: 1px dashed #cbd5e1; font-size: 0.88rem; color: #334155; line-height: 1.5; font-style: italic;" id="bannerText"></div>
                        </div>
                    </div>
                    <button type="button" class="close" onclick="dismissCorrectionBanner()" style="outline: none; padding: 4px 8px; font-size: 1.25rem; border: none; background: transparent; cursor: pointer; color: #94a3b8;">&times;</button>
                </div>
            </div>
        </div>
    </div>
    
    <div class="card shadow-sm border-0 rounded-lg mb-2">
        <div class="card-body py-2 px-3 bg-white text-dark">
            <div class="calc-controls-container">
                <!-- Left Side: Dropdowns, Search and Holiday inputs -->
                <div class="calc-left-group">
                    <!-- Year selector -->
                    <div class="calc-control-item">
                        <label class="form-label font-weight-bold mb-1 text-gray-800">
                            <i class="fas fa-calendar mr-1 text-primary"></i> Year
                        </label>
                        <select id="yearSel" class="form-control"></select>
                    </div>
                    
                    <!-- Month selector -->
                    <div class="calc-control-item">
                        <label class="form-label font-weight-bold mb-1 text-gray-800">
                            <i class="fas fa-calendar-alt mr-1 text-primary"></i> Month
                        </label>
                        <select id="monthSel" class="form-control"></select>
                    </div>
                    
                    <!-- Category selector -->
                    <div class="calc-control-item-category">
                        <label class="form-label font-weight-bold mb-1 text-gray-800">
                            <i class="fas fa-th-list mr-1 text-primary"></i> Category
                        </label>
                        <select id="catSel" class="form-control">
                        </select>
                    </div>

                    <!-- Division selector -->
                    <div class="calc-control-item-category">
                        <label class="form-label font-weight-bold mb-1 text-gray-800">
                            <i class="fas fa-building mr-1 text-primary"></i> Division
                        </label>
                        <select id="divSel" class="form-control">
                        </select>
                    </div>
                    
                    <!-- Search input -->
                    <div class="calc-control-item-category">
                        <label class="form-label font-weight-bold mb-1 text-gray-800">
                            <i class="fas fa-search mr-1 text-primary"></i> Search
                        </label>
                        <input id="search" class="form-control" placeholder="Search ID/Name" />
                    </div>
                    
                    <!-- Holidays input group -->
                    <div id="holidayDiv" class="calc-control-item-wage">
                        <label class="form-label font-weight-bold mb-1 text-gray-800">
                            <i class="fas fa-umbrella-beach mr-1 text-danger"></i> Holiday
                        </label>
                        <div class="input-group">
                            <input id="holidayInput" class="form-control" placeholder="14,26" />
                            <div class="input-group-append">
                                <button type="button" class="btn btn-primary" onclick="applyHoliday()" title="Apply Holidays">
                                    Apply
                                </button>
                                <button type="button" class="btn btn-danger" onclick="removeHoliday()" title="Remove Holidays">
                                    Remove
                                </button>
                            </div>
                        </div>
                    </div>
                    
                    <!-- Global Adjust Button -->
                    <div id="globalAdjustDiv" class="calc-control-item" style="min-width: 140px;">
                        <label class="form-label d-block mb-1">&nbsp;</label>
                        <button type="button" onclick="globalAdjust()" class="btn btn-custom w-100" style="background-color: #9333ea; font-size: 0.85rem; font-weight: bold; height: 38px !important;">
                             <i class="fas fa-adjust mr-1"></i> Global Adjust
                        </button>
                    </div>

                    <!-- Unlock Past Attendance Toggle (Admin Only) -->
                    <div id="unlockClosedDiv" class="calc-control-item" style="min-width: 210px; display: none;">
                        <label class="form-label d-block mb-1">&nbsp;</label>
                        <div class="custom-control custom-switch" style="height: 38px; display: flex; align-items: center;">
                            <input type="checkbox" class="custom-control-input" id="chkUnlockClosed" onchange="toggleUnlockClosed()" style="cursor: pointer; width: 36px; height: 18px;" />
                            <label class="custom-control-label font-weight-bold text-gray-800" for="chkUnlockClosed" style="cursor: pointer; user-select: none; margin-left: 8px;">
                                <i class="fas fa-lock-open mr-1 text-warning"></i> Unlock Past Attendance
                            </label>
                        </div>
                    </div>
                </div>
                
                <!-- Right Side: Save and Refresh Actions -->
                <div class="calc-right-group">
                    <button type="button" class="btn btn-custom btn-custom-calc" onclick="saveData()">
                        <i class="fas fa-save"></i> Save
                    </button>
                    <button type="button" class="btn btn-custom btn-custom-export" onclick="fetchData()" style="background-color: #17a2b8;">
                        <i class="fas fa-sync-alt"></i> Refresh
                    </button>
                </div>
            </div>
        </div>
    </div>

    <div class="wrapper">
        <table class="att-table" id="attTable">
            <thead id="thead"></thead>
            <tbody id="tbody"></tbody>
        </table>
    </div>

    <!-- Floating Leave Balance Popup at Bottom Left -->
    <div id="leaveInfoPopup" class="leave-info-popup">
        <button type="button" class="leave-popup-close" onclick="closeLeavePopup()">&times;</button>
        <div class="leave-popup-header">
            <i class="fas fa-id-card text-primary mr-2"></i>
            <span class="leave-popup-title">Employee Leave Summary</span>
        </div>
        <div class="leave-popup-body" id="leavePopupBody">
            <!-- Filled dynamically -->
        </div>
    </div>

    <!-- Mini Leave Popup on Cell Click/Focus -->
    <div id="miniLeavePopup" class="mini-leave-popup"></div>

    <!-- Custom Cell Context Menu -->
    <div id="satContextMenu" class="sat-context-menu">
        <div id="ctxSatPresent" class="sat-context-item" onclick="overrideSaturdayPresent()">
            <i class="fas fa-check-circle text-success"></i> Mark as Present (1) with Remarks
        </div>
        <div id="ctxReset" class="sat-context-item" onclick="resetSaturdayAuto()">
            <i class="fas fa-undo-alt text-warning"></i> Reset to Auto-Calculate
        </div>
        <div id="ctxAddRemarks" class="sat-context-item" onclick="addEditCellRemarks()">
            <i class="fas fa-comment-dots text-primary"></i> Add/Edit Remarks
        </div>
        <div id="ctxClearRemarks" class="sat-context-item" onclick="clearCellRemarks()">
            <i class="fas fa-comment-slash text-danger"></i> Clear Remarks
        </div>
    </div>

    <script>
        const role = '<%= Session["Role"] != null ? Session["Role"].ToString() : "0" %>';
        let attendanceData = {};
        let prevAttendanceData = {};
        let futureCarriedData = {};
        let futureUpdates = {};
        let employees = [];
        let engagements = [];
        let _isDirty = false;
        let hasPushedDirtyState = false;
        Object.defineProperty(window, 'isDirty', {
            get: function() { return _isDirty; },
            set: function(val) {
                _isDirty = val;
                if (val && !hasPushedDirtyState) {
                    history.pushState({ page: 'attendance_dirty' }, null, window.location.href);
                    hasPushedDirtyState = true;
                } else if (!val) {
                    hasPushedDirtyState = false;
                }
            }
        });
        let currentSortCol = '';
        let currentSortDir = 'none';

        function toggleUnlockClosed() {
            render();
        }

        let activePopupEmpId = null;
        let popupTimeout = null;

        // Reset timeout on hover interaction
        window.addEventListener('load', function() {
            const popup = document.getElementById("leaveInfoPopup");
            if (popup) {
                popup.addEventListener("mouseenter", function() {
                    if (popupTimeout) {
                        clearTimeout(popupTimeout);
                        popupTimeout = null;
                    }
                });
                popup.addEventListener("mouseleave", function() {
                    closeLeavePopup();
                });
            }

            // Saturday Context Menu Click Handler (click anywhere to close)
            const menu = document.getElementById("satContextMenu");
            if (menu) {
                document.addEventListener('click', function() {
                    menu.classList.remove("show");
                });
            }
        });

        let activeContextEmpId = null;
        let activeContextDay = null;

        function getRecentRemarks() {
            const remarksSet = new Set();
            for (const empId in attendanceData) {
                const days = attendanceData[empId];
                for (const d in days) {
                    const cell = days[d];
                    if (cell && cell.Remarks && cell.Remarks.trim() !== "") {
                        remarksSet.add(cell.Remarks.trim());
                    }
                }
            }
            const defaults = ["Half Day Leave", "Late Entry", "Special Duty", "Permission", "On Tour", "Official Duty", "Training"];
            defaults.forEach(d => {
                if (remarksSet.size < 15) {
                    remarksSet.add(d);
                }
            });
            return Array.from(remarksSet);
        }

        function buildRecentRemarksHtml(inputId) {
            const list = getRecentRemarks();
            if (list.length === 0) return "";
            let html = `<div style="margin-top: 12px;"><label class="font-weight-bold mb-1" style="font-size: 0.8rem; color: #64748b; margin-bottom: 6px; display: block;">Recent / Common Remarks (Click to select):</label>`;
            html += `<div style="display: flex; flex-wrap: wrap; gap: 6px; max-height: 120px; overflow-y: auto; padding: 2px;">`;
            list.forEach(rem => {
                const escaped = rem.replace(/'/g, "\\'").replace(/"/g, "&quot;");
                html += `<span class="badge" onclick="document.getElementById('${inputId}').value = '${escaped}';" style="cursor: pointer; background-color: #f1f5f9; color: #334155; border: 1px solid #cbd5e1; padding: 5px 10px; border-radius: 15px; font-size: 0.78rem; font-weight: 600; display: inline-block; user-select: none; transition: all 0.15s;" onmouseover="this.style.backgroundColor='#e2e8f0'; this.style.borderColor='#94a3b8';" onmouseout="this.style.backgroundColor='#f1f5f9'; this.style.borderColor='#cbd5e1';">${rem}</span>`;
            });
            html += `</div></div>`;
            return html;
        }

        function overrideSaturdayPresent() {
            if (parseInt(role) !== 1) {
                showToast("Permission denied: Admin only.", "error");
                return;
            }
            const empId = activeContextEmpId;
            const day = activeContextDay;
            if (!empId || !day) return;

            const emp = employees.find(e => e.MasterId === empId) || {};
            const empEngagements = engagements.filter(ee => ee.EmpID === empId);
            const state = getCellState(emp, parseInt(yS.value), parseInt(mS.value), day, empEngagements);
            if (state.isClosed && !document.getElementById('chkUnlockClosed').checked) {
                showToast("Cannot edit: This contract is closed/locked.", "error");
                return;
            }
            
            Swal.fire({
                title: 'Override Saturday Cut',
                html: `
                    <div style="text-align: left;">
                        <p style="font-size: 0.95rem; color: #64748b; margin-bottom: 15px;">Manually mark this Saturday as Present. Please provide a reason/remark.</p>
                        <div class="form-group mb-3">
                            <label class="font-weight-bold mb-1" style="font-size: 0.9rem; color: #475569;">Remarks / Reason *</label>
                            <input type="text" id="swalOverrideRemarks" class="form-control" placeholder="e.g. Overtime or Special Work Day" style="font-weight: 600;" />
                        </div>
                        ${buildRecentRemarksHtml('swalOverrideRemarks')}
                    </div>
                `,
                showCancelButton: true,
                confirmButtonText: 'Confirm Override',
                confirmButtonColor: '#3b82f6',
                cancelButtonText: 'Cancel',
                preConfirm: () => {
                    const remarks = Swal.getPopup().querySelector('#swalOverrideRemarks').value.trim();
                    if (!remarks) {
                        Swal.showValidationMessage('Remarks are required to override Saturday Cut.');
                        return false;
                    }
                    return remarks;
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    const trimmedRemarks = result.value;
                    
                    attendanceData[empId] = attendanceData[empId] || {};
                    attendanceData[empId][day] = attendanceData[empId][day] || {};
                    attendanceData[empId][day].Val = 1;
                    attendanceData[empId][day].ManualOverride = true;
                    attendanceData[empId][day].Remarks = trimmedRemarks;
                    attendanceData[empId][day].AutoSat = false;
                    
                    isDirty = true;
                    calcSat(empId);
                    
                    const tr = document.querySelector(`tr[data-empid="${empId}"]`);
                    if (tr) {
                        updateRowUI(tr, empId);
                    }
                    
                    showPop("Saturday manually marked as Present");
                }
            });
        }

        function resetSaturdayAuto() {
            if (parseInt(role) !== 1) {
                showToast("Permission denied: Admin only.", "error");
                return;
            }
            const empId = activeContextEmpId;
            const day = activeContextDay;
            if (!empId || !day) return;

            const emp = employees.find(e => e.MasterId === empId) || {};
            const empEngagements = engagements.filter(ee => ee.EmpID === empId);
            const state = getCellState(emp, parseInt(yS.value), parseInt(mS.value), day, empEngagements);
            if (state.isClosed && !document.getElementById('chkUnlockClosed').checked) {
                showToast("Cannot edit: This contract is closed/locked.", "error");
                return;
            }
            
            if (attendanceData[empId]?.[day]) {
                delete attendanceData[empId][day].ManualOverride;
                attendanceData[empId][day].Remarks = "";
                attendanceData[empId][day].Val = null; // Let calcSat recompute it
                attendanceData[empId][day].AutoSat = true;
            }
            
            isDirty = true;
            calcSat(empId);
            
            const tr = document.querySelector(`tr[data-empid="${empId}"]`);
            if (tr) {
                updateRowUI(tr, empId);
            }
            
            showPop("Saturday reset to Auto-Calculate");
        }

        function addEditCellRemarks() {
            if (parseInt(role) !== 1) {
                showToast("Permission denied: Admin only.", "error");
                return;
            }
            const empId = activeContextEmpId;
            const day = activeContextDay;
            if (!empId || !day) return;

            const emp = employees.find(e => e.MasterId === empId) || {};
            const empEngagements = engagements.filter(ee => ee.EmpID === empId);
            const state = getCellState(emp, parseInt(yS.value), parseInt(mS.value), day, empEngagements);
            if (state.isClosed && !document.getElementById('chkUnlockClosed').checked) {
                showToast("Cannot edit: This contract is closed/locked.", "error");
                return;
            }

            const cell = attendanceData[empId]?.[day] || {};
            const existingRemarks = cell.Remarks || "";
            
            Swal.fire({
                title: 'Cell Remarks',
                html: `
                    <div style="text-align: left;">
                        <p style="font-size: 0.95rem; color: #64748b; margin-bottom: 15px;">Set custom remarks/reasons for this day's attendance.</p>
                        <div class="form-group mb-3">
                            <label class="font-weight-bold mb-1" style="font-size: 0.9rem; color: #475569;">Remarks / Reason</label>
                            <input type="text" id="swalCellRemarks" class="form-control" value="${existingRemarks.replace(/"/g, '&quot;')}" placeholder="e.g. Half Day Leave, Training, etc." style="font-weight: 600;" />
                        </div>
                        ${buildRecentRemarksHtml('swalCellRemarks')}
                    </div>
                `,
                showCancelButton: true,
                confirmButtonText: 'Save Remarks',
                confirmButtonColor: '#3b82f6',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    const trimmedRemarks = Swal.getPopup().querySelector('#swalCellRemarks').value.trim();
                    
                    attendanceData[empId] = attendanceData[empId] || {};
                    attendanceData[empId][day] = attendanceData[empId][day] || {};
                    
                    // If Val is not set, set default to 1 (Present)
                    if (attendanceData[empId][day].Val === undefined || attendanceData[empId][day].Val === null) {
                        attendanceData[empId][day].Val = 1;
                    }
                    
                    attendanceData[empId][day].Remarks = trimmedRemarks;
                    
                    if (trimmedRemarks === "") {
                        delete attendanceData[empId][day].Remarks;
                    }
                    
                    isDirty = true;
                    render();
                    showToast("Remarks updated.", "success");
                }
            });
        }

        function clearCellRemarks() {
            if (parseInt(role) !== 1) {
                showToast("Permission denied: Admin only.", "error");
                return;
            }
            const empId = activeContextEmpId;
            const day = activeContextDay;
            if (!empId || !day) return;

            const emp = employees.find(e => e.MasterId === empId) || {};
            const empEngagements = engagements.filter(ee => ee.EmpID === empId);
            const state = getCellState(emp, parseInt(yS.value), parseInt(mS.value), day, empEngagements);
            if (state.isClosed && !document.getElementById('chkUnlockClosed').checked) {
                showToast("Cannot edit: This contract is closed/locked.", "error");
                return;
            }

            if (attendanceData[empId] && attendanceData[empId][day]) {
                delete attendanceData[empId][day].Remarks;
                isDirty = true;
                render();
                showToast("Remarks cleared.", "success");
            }
        }

        function getContractPeriodIdForDate(empId, y, m, day) {
            let dStr = `${y}-${String(m + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            let empEngagements = engagements.filter(ee => ee.EmpID === empId);
            let activeEng = empEngagements.find(ee => ee.StartDate <= dStr && (!ee.EndDate || dStr <= ee.EndDate));
            return activeEng ? activeEng.ContractPeriodId : null;
        }

        function hasEnoughLeaveBalance(empId, day, y, m, targetVal, targetLeave) {
            if (empId.startsWith("GLOBAL")) return true;
            const emp = employees.find(e => e.MasterId === empId);
            if (!emp) return true;

            const checkY = (y !== undefined && y !== null) ? y : parseInt(yS.value);
            const checkM = (m !== undefined && m !== null) ? m : parseInt(mS.value);

            const cpId = getContractPeriodIdForDate(empId, checkY, checkM, day);
            if (cpId === null) return true;

            const cpIdKey = cpId.toString();
            const prevUsed = (emp.PrevLeaves && emp.PrevLeaves[cpIdKey]) ? emp.PrevLeaves[cpIdKey] : 0.0;

            const targetDateStr = `${checkY}-${String(checkM + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            
            const activeDates = new Set();
            activeDates.add(targetDateStr);

            const data = attendanceData[empId] || {};
            Object.keys(data).forEach(d => {
                const dNum = parseInt(d);
                const cellCpId = getContractPeriodId(empId, dNum);
                if (cellCpId === cpId) {
                    const dStr = `${parseInt(yS.value)}-${String(parseInt(mS.value) + 1).padStart(2, '0')}-${String(dNum).padStart(2, '0')}`;
                    activeDates.add(dStr);
                }
            });

            const futDays = futureCarriedData[empId] || [];
            futDays.forEach(fut => {
                if (fut.ContractPeriodId === cpId) {
                    const dStr = `${fut.Year}-${String(fut.Month + 1).padStart(2, '0')}-${String(fut.Day).padStart(2, '0')}`;
                    activeDates.add(dStr);
                }
            });

            const sortedDates = Array.from(activeDates).sort();

            function getLeaveWeight(cellVal, cellLeave, isHoliday) {
                if (isHoliday) return 0.0;
                let weight = 0.0;
                if ((cellVal === 0 && cellLeave === "Paid") || cellLeave === "Paired Paid") {
                    weight = 1.0;
                } else if (cellVal === 0.5) {
                    weight = 0.5;
                }
                return weight;
            }

            for (let i = 0; i < sortedDates.length; i++) {
                const checkDateStr = sortedDates[i];
                if (checkDateStr < targetDateStr) continue;

                let leavesUsed = prevUsed;

                Object.keys(data).forEach(d => {
                    const dNum = parseInt(d);
                    const cellCpId = getContractPeriodId(empId, dNum);
                    if (cellCpId !== cpId) return;

                    const dStr = `${parseInt(yS.value)}-${String(parseInt(mS.value) + 1).padStart(2, '0')}-${String(dNum).padStart(2, '0')}`;
                    if (dStr > checkDateStr) return;

                    if (parseInt(yS.value) === checkY && parseInt(mS.value) === checkM && dNum === day) {
                        leavesUsed += getLeaveWeight(targetVal, targetLeave, false);
                    } else {
                        const cell = data[d];
                        if (cell) {
                            leavesUsed += getLeaveWeight(cell.Val, cell.Leave, cell.Holiday);
                        }
                    }
                });

                futDays.forEach(fut => {
                    if (fut.ContractPeriodId !== cpId) return;
                    const dStr = `${fut.Year}-${String(fut.Month + 1).padStart(2, '0')}-${String(fut.Day).padStart(2, '0')}`;
                    if (dStr > checkDateStr) return;

                    if (fut.Year === checkY && fut.Month === checkM && fut.Day === day) {
                        leavesUsed += getLeaveWeight(targetVal, targetLeave, false);
                    } else {
                        const cell = getFutureDayState(empId, fut);
                        leavesUsed += getLeaveWeight(cell.Val, cell.Leave, false);
                    }
                });

                let allowedCredits = 0.0;
                if (emp.Credits && emp.Credits.length > 0) {
                    let creditSum = 0.0;
                    emp.Credits.forEach(cr => {
                        if (cr.ContractPeriodId === cpId && cr.EffectiveDate <= checkDateStr) {
                            creditSum += cr.Amount;
                        }
                    });
                    allowedCredits = creditSum;
                } else {
                    allowedCredits = cpId === emp.CurrentContractPeriodId ? (emp.LeaveBalance || 0) : (emp.PrevLeaveBalance || 0);
                }

                if (leavesUsed > allowedCredits) {
                    return false;
                }
            }

            return true;
        }

        function showLeavePopup(empId) {
            const emp = employees.find(e => e.MasterId === empId);
            if (!emp) return;

            activePopupEmpId = empId;

            const y = parseInt(yS.value);
            const m = parseInt(mS.value);
            const days = new Date(y, m + 1, 0).getDate();
            let activeCpId = null;
            for (let d = days; d >= 1; d--) {
                activeCpId = getContractPeriodId(empId, d);
                if (activeCpId !== null) break;
            }

            const data = attendanceData[empId] || {};
            let currPaid = 0;

            Object.keys(data).forEach(d => {
                const dNum = parseInt(d);
                if (getContractPeriodId(empId, dNum) !== activeCpId) return;

                const cell = data[d];
                if (cell) {
                    if ((cell.Val === 0 && cell.Leave === "Paid") || cell.Leave === "Paired Paid") {
                        currPaid += 1;
                    } else if (cell.Val === 0.5) {
                        currPaid += 0.5;
                    }
                }
            });

            let usedPaid = currPaid;
            let totalLeft = emp.OpeningBalance - usedPaid;

            let unpaidCount = 0;
            Object.keys(data).forEach(d => {
                const cell = data[d];
                if (cell && (cell.Leave === "Unpaid" || cell.Leave === "Paired Unpaid")) {
                    unpaidCount++;
                }
            });

            const popup = document.getElementById("leaveInfoPopup");
            const body = document.getElementById("leavePopupBody");
            if (!popup || !body) return;

            body.innerHTML = `
                <div class="leave-popup-item">
                    <span class="leave-popup-label">Employee:</span>
                    <span class="leave-popup-value">${emp.Name}</span>
                </div>
                <div class="leave-popup-item">
                    <span class="leave-popup-label">Employee ID:</span>
                    <span class="leave-popup-value">${emp.ID}</span>
                </div>
                <div class="leave-popup-item">
                    <span class="leave-popup-label">Master ID:</span>
                    <span class="leave-popup-value" style="font-size: 0.78rem; color: #475569;">${emp.MasterId}</span>
                </div>
                <div class="leave-popup-item">
                    <span class="leave-popup-label">Division:</span>
                    <span class="leave-popup-value" style="font-size: 0.78rem; color: #475569;">${emp.Dept || 'N/A'}</span>
                </div>
                <div class="leave-popup-item">
                    <span class="leave-popup-label">Category:</span>
                    <span class="leave-popup-value" style="font-size: 0.78rem; color: #475569;">${emp.Category || 'N/A'}</span>
                </div>
                <div class="leave-popup-divider"></div>
                <div class="leave-popup-item">
                    <span class="leave-popup-label">Initial Balance:</span>
                    <span class="leave-popup-value">${emp.LeaveBalance} Days</span>
                </div>
                <div class="leave-popup-item">
                    <span class="leave-popup-label">Opening Balance:</span>
                    <span class="leave-popup-value">${emp.OpeningBalance} Days</span>
                </div>
                <div class="leave-popup-item">
                    <span class="leave-popup-label">Used This Month:</span>
                    <span class="leave-popup-value">${usedPaid} Days</span>
                </div>
                <div class="leave-popup-divider"></div>
                <div class="leave-popup-item" style="font-weight: bold;">
                    <span class="leave-popup-label" style="color: #4f46e5;">Paid Leaves Left:</span>
                    <span class="leave-popup-value" style="color: #4f46e5; font-size: 1.02rem;">${totalLeft.toFixed(1)} Days</span>
                </div>
                <div class="leave-popup-item">
                    <span class="leave-popup-label">Unpaid (This Month):</span>
                    <span class="leave-popup-value" style="color: #dc2626;">${unpaidCount} Days</span>
                </div>
            `;

            if (!popup.classList.contains("show")) {
                popup.style.display = "block";
                // trigger reflow
                popup.offsetHeight;
                popup.classList.add("show");
            }

            // Reset and start timeout for auto disappearance
            if (popupTimeout) {
                clearTimeout(popupTimeout);
            }
            popupTimeout = setTimeout(closeLeavePopup, 3000);
        }

        function closeLeavePopup() {
            if (popupTimeout) {
                clearTimeout(popupTimeout);
                popupTimeout = null;
            }
            const popup = document.getElementById("leaveInfoPopup");
            if (popup) {
                popup.classList.remove("show");
                setTimeout(() => {
                    if (!popup.classList.contains("show")) {
                        popup.style.display = "none";
                    }
                }, 300);
            }
            activePopupEmpId = null;
        }

        // Global timeout variable for mini popup
        let miniPopupTimeout = null;

        function shouldShowMiniPopupForCell(empId, day) {
            const cell = attendanceData[empId]?.[day];
            if (!cell) return false;

            const leave = cell.Leave ? cell.Leave.trim() : "";
            if (leave === "Paid" || leave === "Unpaid" || leave === "Carried" || leave === "Paired Paid" || leave === "Paired Unpaid") {
                return true;
            }

            if (cell.Val === 0 || cell.Val === 0.5) {
                return true;
            }

            if (cell.Val === 1) return false;

            return false;
        }

        function showMiniLeavePopup(element, empId) {
            const emp = employees.find(e => e.MasterId === empId);
            if (!emp) return;

            const y = parseInt(yS.value);
            const m = parseInt(mS.value);
            const days = new Date(y, m + 1, 0).getDate();
            let activeCpId = null;
            for (let d = days; d >= 1; d--) {
                activeCpId = getContractPeriodId(empId, d);
                if (activeCpId !== null) break;
            }

            const data = attendanceData[empId] || {};
            let currPaid = 0;
            let monthHalfDayCount = 0;

            Object.keys(data).forEach(d => {
                const dNum = parseInt(d);
                if (getContractPeriodId(empId, dNum) !== activeCpId) return;

                const cell = data[d];
                if (cell) {
                    if ((cell.Val === 0 && cell.Leave === "Paid") || cell.Leave === "Paired Paid") {
                        currPaid += 1;
                    } else if (cell.Val === 0.5) {
                        currPaid += 0.5;
                    }

                    if (cell.Leave === "Carried" || cell.Leave === "Paired Paid" || cell.Leave === "Paired Unpaid" || cell.Val === 0.5) {
                        monthHalfDayCount++;
                    }
                }
            });

            // Also check future carried count for this CP
            const futDays = (futureCarriedData[empId] || []).filter(fut => fut.ContractPeriodId === activeCpId);
            const futDaysCount = futDays.length;

            const prevHalfCount = (emp.PrevHalfCounts && emp.PrevHalfCounts[activeCpId]) ? emp.PrevHalfCounts[activeCpId] : 0;
            const totalHalfDays = prevHalfCount + monthHalfDayCount + futDaysCount;
            const hasHalfDayCarried = (totalHalfDays % 2 === 1);

            let usedPaid = currPaid;
            let totalLeft = emp.OpeningBalance - usedPaid;

            const miniPopup = document.getElementById("miniLeavePopup");
            if (!miniPopup) return;

            let halfDayRow = "";
            if (hasHalfDayCarried) {
                halfDayRow = `
                    <div class="mini-leave-item" style="color: #047857; margin-top: 4px; border-top: 1px solid #e2e8f0; padding-top: 4px;">
                        <span class="mini-leave-label" style="color: #047857;">Half Day Carried:</span>
                        <span class="mini-leave-value" style="font-weight: bold;">Yes</span>
                    </div>
                `;
            }

            miniPopup.innerHTML = `
                <div class="mini-leave-item">
                    <span class="mini-leave-label">Name:</span>
                    <span class="mini-leave-value" style="font-weight: 600;">${emp.Name}</span>
                </div>
                <div class="mini-leave-item">
                    <span class="mini-leave-label">ID:</span>
                    <span class="mini-leave-value">${emp.ID}</span>
                </div>
                <div class="mini-leave-item">
                    <span class="mini-leave-label">Leave Balance:</span>
                    <span class="mini-leave-value" style="font-weight: 600; color: #4f46e5;">${totalLeft.toFixed(1)} Days</span>
                </div>
                ${halfDayRow}
            `;

            miniPopup.style.display = "block";
            
            // Get bounding rect of the parent td element to keep popup positioning completely stable
            const cellElement = element.closest("td");
            const rect = cellElement ? cellElement.getBoundingClientRect() : element.getBoundingClientRect();
            const popupWidth = miniPopup.offsetWidth || 250;
            const popupHeight = miniPopup.offsetHeight || 90;

            let topPos = rect.top + window.scrollY - popupHeight - 8;
            if (topPos - window.scrollY < 10) {
                topPos = rect.bottom + window.scrollY + 8;
            }

            let leftPos = rect.left + window.scrollX + (rect.width - popupWidth) / 2;
            if (leftPos - window.scrollX < 10) leftPos = window.scrollX + 10;
            if (leftPos - window.scrollX + popupWidth > window.innerWidth - 10) {
                leftPos = window.scrollX + window.innerWidth - popupWidth - 10;
            }

            miniPopup.style.top = `${topPos}px`;
            miniPopup.style.left = `${leftPos}px`;
            
            // Trigger reflow
            miniPopup.offsetHeight;
            miniPopup.classList.add("show");

            resetMiniPopupTimeout();
        }

        function resetMiniPopupTimeout() {
            if (miniPopupTimeout) {
                clearTimeout(miniPopupTimeout);
            }
            miniPopupTimeout = setTimeout(hideMiniLeavePopup, 3000);
        }

        function hideMiniLeavePopup() {
            if (miniPopupTimeout) {
                clearTimeout(miniPopupTimeout);
                miniPopupTimeout = null;
            }
            const miniPopup = document.getElementById("miniLeavePopup");
            if (miniPopup) {
                miniPopup.classList.remove("show");
                setTimeout(() => {
                    if (!miniPopup.classList.contains("show")) {
                        miniPopup.style.display = "none";
                    }
                }, 200);
            }
        }

        function initMiniPopupEvents() {
            const miniPopup = document.getElementById("miniLeavePopup");
            if (miniPopup) {
                miniPopup.addEventListener("mouseenter", function() {
                    if (miniPopupTimeout) {
                        clearTimeout(miniPopupTimeout);
                        miniPopupTimeout = null;
                    }
                });
                miniPopup.addEventListener("mouseleave", function() {
                    resetMiniPopupTimeout();
                });
            }

            const table = document.getElementById("attTable");
            if (table) {
                const handlePopupTrigger = (e) => {
                    const target = e.target;
                    if (target && (target.classList.contains("att") || target.classList.contains("leave-opt"))) {
                        const tr = target.closest("tr");
                        const td = target.closest("td");
                        if (tr && td && tr.dataset.empid && td.dataset.day) {
                            const empId = tr.dataset.empid;
                            const day = parseInt(td.dataset.day);
                            if (shouldShowMiniPopupForCell(empId, day)) {
                                showMiniLeavePopup(target, empId);
                            } else {
                                hideMiniLeavePopup();
                            }
                        }
                    }
                };
                table.addEventListener("focusin", handlePopupTrigger);
                table.addEventListener("click", handlePopupTrigger);
            }
        }

        function getFutureDayState(id, futDay) {
            const dateKey = `${futDay.Year}-${futDay.Month}-${futDay.Day}`;
            if (futureUpdates[id] && futureUpdates[id][dateKey]) {
                return futureUpdates[id][dateKey];
            }
            return futDay;
        }

        function setFutureDayState(id, futDay, val, leave) {
            const dateKey = `${futDay.Year}-${futDay.Month}-${futDay.Day}`;
            if (futDay.Val === val && futDay.Leave === leave) {
                if (futureUpdates[id]) {
                    delete futureUpdates[id][dateKey];
                }
                return;
            }
            futureUpdates[id] = futureUpdates[id] || {};
            futureUpdates[id][dateKey] = {
                Year: futDay.Year,
                Month: futDay.Month,
                Day: futDay.Day,
                Val: val,
                Leave: leave
            };
        }

        function getContractPeriodId(empId, day) {
            return getContractPeriodIdForDate(empId, parseInt(yS.value), parseInt(mS.value), day);
        }

        function reprocessHalfDays(id, editedDay = null, prevCellState = null, event = null) {
            const emp = employees.find(e => e.MasterId === id) || {};
            const data = attendanceData[id] || {};
            
            // Get all contract period IDs active for this employee
            const empEngagements = engagements.filter(ee => ee.EmpID === id);
            const contractPeriodIds = [...new Set(empEngagements.map(ee => ee.ContractPeriodId))];
            
            // Get edited CP ID
            const editedCpId = editedDay !== null ? getContractPeriodId(id, editedDay) : null;
            
            // We will run the analysis for each CP ID.
            let needsChoiceInfo = null;
            
            function runForCp(cpId, choice = null, forceApply = false) {
                // 1. Gather current month half-days in this CP
                let halfDays = [];
                Object.keys(data).forEach(d => {
                    const dNum = parseInt(d);
                    const cell = data[d];
                    if (cell) {
                        if (cell.Leave === "Carried" || cell.Leave === "Paired Paid" || cell.Leave === "Paired Unpaid" || cell.Val === 0.5) {
                            if (getContractPeriodId(id, dNum) === cpId) {
                                if (!halfDays.includes(dNum)) halfDays.push(dNum);
                            }
                        }
                    }
                });
                halfDays.sort((a, b) => a - b);
                
                // 2. Get prevHalfCount for this CP
                const prevHalfCount = (emp.PrevHalfCounts && emp.PrevHalfCounts[cpId]) ? emp.PrevHalfCounts[cpId] : 0;
                
                // 3. Get future half-days for this CP
                const futDays = (futureCarriedData[id] || []).filter(fut => fut.ContractPeriodId === cpId);
                
                // Find even day needing choice
                let evenDayNeedingChoice = null;
                for (let i = 0; i < halfDays.length; i++) {
                    const dNum = halfDays[i];
                    const overallIdx = prevHalfCount + 1 + i;
                    if (overallIdx % 2 === 0) {
                        const cell = data[dNum] || {};
                        if (dNum === editedDay || (cell.Leave !== "Paired Paid" && cell.Leave !== "Paired Unpaid")) {
                            evenDayNeedingChoice = dNum;
                            break;
                        }
                    }
                }
                
                let futureDayNeedingChoice = null;
                let futureOverallIdxNeedingChoice = null;
                if (evenDayNeedingChoice === null) {
                    for (let j = 0; j < futDays.length; j++) {
                        const fut = futDays[j];
                        const overallIdx = prevHalfCount + halfDays.length + 1 + j;
                        if (overallIdx % 2 === 0) {
                            const currentState = getFutureDayState(id, fut);
                            if (currentState.Leave === "Carried") {
                                futureDayNeedingChoice = fut;
                                futureOverallIdxNeedingChoice = overallIdx;
                                break;
                            }
                        }
                    }
                }
                
                // If this is the edited CP, and we are not forcing application, and we need a choice,
                // store the info and do NOT apply states yet.
                if (!forceApply && cpId === editedCpId && (evenDayNeedingChoice !== null || futureDayNeedingChoice !== null)) {
                    needsChoiceInfo = {
                        evenDayNeedingChoice,
                        futureDayNeedingChoice,
                        futureOverallIdxNeedingChoice,
                        halfDays,
                        prevHalfCount,
                        futDays
                    };
                    return;
                }
                
                // Apply states
                for (let i = 0; i < halfDays.length; i++) {
                    const dNum = halfDays[i];
                    const overallIdx = prevHalfCount + 1 + i;
                    data[dNum] = data[dNum] || {};
                    
                    if (overallIdx % 2 === 1) {
                        data[dNum].Val = 1;
                        data[dNum].Leave = "Carried";
                    } else {
                        if (dNum === evenDayNeedingChoice && cpId === editedCpId && choice) {
                            if (choice === "Paid") {
                                data[dNum].Val = 1;
                                data[dNum].Leave = "Paired Paid";
                            } else if (choice === "Unpaid") {
                                data[dNum].Val = 0;
                                data[dNum].Leave = "Paired Unpaid";
                            }
                        } else {
                            const cell = data[dNum] || {};
                            if (cell.Leave === "Paired Paid") {
                                data[dNum].Val = 1;
                                data[dNum].Leave = "Paired Paid";
                            } else if (cell.Leave === "Paired Unpaid") {
                                data[dNum].Val = 0;
                                data[dNum].Leave = "Paired Unpaid";
                            } else {
                                data[dNum].Val = 1;
                                data[dNum].Leave = "Paired Paid";
                            }
                        }
                    }
                }
                
                // Process future half-days
                for (let j = 0; j < futDays.length; j++) {
                    const fut = futDays[j];
                    const overallIdx = prevHalfCount + halfDays.length + 1 + j;
                    const currentState = getFutureDayState(id, fut);
                    
                    if (overallIdx % 2 === 1) {
                        setFutureDayState(id, fut, 1, "Carried");
                    } else {
                        if (overallIdx === futureOverallIdxNeedingChoice && cpId === editedCpId && choice) {
                            if (choice === "Paid") {
                                setFutureDayState(id, fut, 1, "Paired Paid");
                            } else if (choice === "Unpaid") {
                                setFutureDayState(id, fut, 0, "Paired Unpaid");
                            }
                        } else {
                            if (currentState.Leave === "Paired Paid" || currentState.Leave === "Paired Unpaid") {
                                setFutureDayState(id, fut, currentState.Val, currentState.Leave);
                            } else {
                                setFutureDayState(id, fut, 1, "Paired Paid");
                            }
                        }
                    }
                }
            }

            // Blur target if it is the active input
            if (event && event.target) {
                event.target.blur();
            }
            
            function applyStates(choice) {
                contractPeriodIds.forEach(cpId => {
                    runForCp(cpId, choice, true);
                });
                
                isDirty = true;
                calcSat(id);
                const tr = event && event.target ? event.target.closest("tr") : document.querySelector(`tr[data-empid="${id}"]`);
                if (tr) updateRowUI(tr, id);
                if (activePopupEmpId === id) showLeavePopup(id);
            }

            function revertEdit() {
                if (editedDay !== null && prevCellState) {
                    data[editedDay] = data[editedDay] || {};
                    data[editedDay].Val = prevCellState.Val;
                    data[editedDay].Leave = prevCellState.Leave;
                    
                    if (futureUpdates[id]) {
                        delete futureUpdates[id];
                    }
                    
                    contractPeriodIds.forEach(cpId => {
                        runForCp(cpId, null, true);
                    });
                }
                
                isDirty = true;
                calcSat(id);
                const tr = event && event.target ? event.target.closest("tr") : document.querySelector(`tr[data-empid="${id}"]`);
                if (tr) {
                    updateRowUI(tr, id);
                    if (editedDay !== null) {
                        const inp = tr.querySelector(`td[data-day="${editedDay}"] .att`);
                        if (inp) {
                            let valText = "";
                            if (prevCellState && prevCellState.Val !== null) {
                                valText = prevCellState.Val;
                            }
                            inp.value = valText;
                        }
                    }
                }
                if (activePopupEmpId === id) showLeavePopup(id);
            }

            // Run analysis first for all CP IDs (this compiles/identifies if any need choice)
            contractPeriodIds.forEach(cpId => {
                runForCp(cpId, null, false);
            });
            
            if (needsChoiceInfo !== null) {
                const { evenDayNeedingChoice, futureDayNeedingChoice, futureOverallIdxNeedingChoice } = needsChoiceInfo;
                
                if (evenDayNeedingChoice !== null) {
                    showPairingConfirmModal(id, evenDayNeedingChoice, parseInt(yS.value), parseInt(mS.value), () => {
                        applyStates("Paid");
                        showPop("Paired Paid applied (-1 Paid Leave)");
                    }, () => {
                        applyStates("Unpaid");
                        showPop("Paired Unpaid applied");
                    }, () => {
                        revertEdit();
                    });
                } else if (futureDayNeedingChoice !== null) {
                    const fut = futureDayNeedingChoice;
                    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                    const dateText = `${monthNames[fut.Month]} ${fut.Day}, ${fut.Year}`;
                    showPairingConfirmModal(id, fut.Day, fut.Year, fut.Month, () => {
                        applyStates("Paid");
                        showPop(`Future day ${dateText} paired as Paid (-1 Paid Leave)`);
                        reprocessHalfDays(id, null, null, event);
                    }, () => {
                        applyStates("Unpaid");
                        showPop(`Future day ${dateText} paired as Unpaid`);
                        reprocessHalfDays(id, null, null, event);
                    }, () => {
                        revertEdit();
                    });
                }
            } else {
                applyStates(null);
                
                if (editedDay !== null && prevCellState && (prevCellState.Val !== 0.5 && prevCellState.Leave !== "Carried" && prevCellState.Leave !== "Paired Paid" && prevCellState.Leave !== "Paired Unpaid")) {
                    showPop("First half day automatically Carried");
                }
                
                if (event && event.target) {
                    setTimeout(() => {
                        let currentTd = event.target.closest("td");
                        let nextTd = currentTd ? currentTd.nextElementSibling : null;
                        while (nextTd) {
                            let nextInp = nextTd.querySelector(".att");
                            if (nextInp && !nextInp.readOnly) { nextInp.focus(); nextInp.select(); break; }
                            nextTd = nextTd.nextElementSibling;
                        }
                    }, 0);
                }
            }
        }

        function processHalfDay(id, day, event) {
            const data = attendanceData[id] || {};
            const cell = data[day];
            let prevCellState = cell ? { Val: cell.Val, Leave: cell.Leave } : { Val: null, Leave: "" };
            reprocessHalfDays(id, day, prevCellState, event);
        }


        function getCellState(emp, y, m, day, filteredEngs) {
            let dStr = `${y}-${String(m + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            let targetEngs = filteredEngs || engagements;
            let activeEng = targetEngs.find(ee => (filteredEngs ? true : ee.EmpID === emp.MasterId) && ee.StartDate <= dStr && (!ee.EndDate || dStr <= ee.EndDate));
            
            let isOutOfBounds = !activeEng;

            // Saturday adjustment for resigned employees:
            // If the day is Saturday, and the employee resigned on the preceding Friday, we treat Saturday as in-bounds.
            let d = new Date(y, m, day);
            if (isOutOfBounds && d.getDay() === 6) {
                let prevFriday = new Date(d);
                prevFriday.setDate(d.getDate() - 1);
                let pfStr = `${prevFriday.getFullYear()}-${String(prevFriday.getMonth() + 1).padStart(2, '0')}-${String(prevFriday.getDate()).padStart(2, '0')}`;
                if (emp.ResignDate === pfStr) {
                    activeEng = targetEngs.find(ee => (filteredEngs ? true : ee.EmpID === emp.MasterId) && ee.StartDate <= pfStr && (!ee.EndDate || pfStr <= ee.EndDate));
                    if (activeEng) {
                        isOutOfBounds = false;
                    }
                }
            }
            
            let isClosed = activeEng && activeEng.Status === 'Closed';
            
            let isReadonlyCell = false;
            let readonlyAttr = "";
            
            let isToday = false;
            const todayDate = new Date();
            if (d.getDate() === todayDate.getDate() && d.getMonth() === todayDate.getMonth() && d.getFullYear() === todayDate.getFullYear()) {
                isToday = true;
            }
            
            let isLockedClosed = isClosed && !document.getElementById('chkUnlockClosed').checked;
            let isAdmin = parseInt(role) === 1;
            
            if (isOutOfBounds) {
                isReadonlyCell = true;
                readonlyAttr = 'readonly tabindex="-1" style="background:#d1d5db; border:1px solid #9ca3af; cursor:not-allowed;"';
            } else if (d.getDay() === 6) {
                isReadonlyCell = true;
                if (!isAdmin || isLockedClosed) {
                    readonlyAttr = 'readonly tabindex="-1" style="background:#e5e7eb; color:#6b7280; border:1px solid #d1d5db; cursor:not-allowed;"';
                } else {
                    const cell = attendanceData[emp.MasterId]?.[day];
                    if (cell && cell.ManualOverride) {
                        readonlyAttr = 'readonly tabindex="-1" style="background:transparent; font-weight:bold; border:none; cursor:pointer;"';
                    } else {
                        readonlyAttr = 'readonly tabindex="-1" style="background:#e5e7eb; color:#6b7280; border:1px solid #d1d5db; cursor:pointer;"';
                    }
                }
            } else {
                if (isLockedClosed) {
                    isReadonlyCell = true;
                    readonlyAttr = 'readonly tabindex="-1" style="background:#f3f4f6; color:#4b5563; border:1px solid #e5e7eb; cursor:not-allowed;"';
                } else if (!isAdmin && !isToday) {
                    isReadonlyCell = true;
                    readonlyAttr = 'readonly tabindex="-1" style="background:#f3f4f6; color:#4b5563; border:1px solid #e5e7eb; cursor:not-allowed;"';
                }
            }
            
            return {
                isOutOfBounds: isOutOfBounds,
                isClosed: isClosed,
                isReadonlyCell: isReadonlyCell,
                readonlyAttr: readonlyAttr
            };
        }

        const yS = document.getElementById('yearSel');
        const mS = document.getElementById('monthSel');
        const cS = document.getElementById('catSel');
        const divS = document.getElementById('divSel');
        const searchBox = document.getElementById('search');
        const tb = document.getElementById('tbody');
        const th = document.getElementById('thead');

        // Add keyboard arrow key navigation for attendance inputs
        tb.addEventListener('keydown', function(event) {
            if (event.target.classList.contains('att')) {
                if (event.key === 'ArrowLeft' || event.keyCode === 37) {
                    let currentTd = event.target.closest('td');
                    let prevTd = currentTd.previousElementSibling;
                    while (prevTd) {
                        let prevInp = prevTd.querySelector('.att');
                        if (prevInp && !prevInp.readOnly) {
                            prevInp.focus();
                            prevInp.select();
                            event.preventDefault();
                            break;
                        }
                        prevTd = prevTd.previousElementSibling;
                    }
                } else if (event.key === 'ArrowRight' || event.keyCode === 39) {
                    let currentTd = event.target.closest('td');
                    let nextTd = currentTd.nextElementSibling;
                    while (nextTd) {
                        let nextInp = nextTd.querySelector('.att');
                        if (nextInp && !nextInp.readOnly) {
                            nextInp.focus();
                            nextInp.select();
                            event.preventDefault();
                            break;
                        }
                        nextTd = nextTd.nextElementSibling;
                    }
                }
            }
        });

        // Show leave balance popup when clicking/focusing any cell
        tb.addEventListener('focusin', function(event) {
            if (event.target.classList.contains('att')) {
                const target = event.target;
                setTimeout(() => {
                    target.select();
                    let tr = target.closest('tr');
                    if (tr) {
                        let empId = tr.dataset.empid;
                        if (empId) {
                            const cellDay = target.closest('td')?.dataset.day;
                            const cell = attendanceData[empId]?.[cellDay];
                            const isLeaveCell = cell && (cell.Leave === "Paid" || cell.Leave === "Unpaid" || cell.Leave === "Paired Paid" || cell.Leave === "Paired Unpaid" || cell.Leave === "Carried");
                            
                            if (isLeaveCell || activePopupEmpId !== null) {
                                showLeavePopup(empId);
                            }
                        }
                    }
                }, 0);
            }
        });

        // Click handler to auto-select text
        tb.addEventListener('click', function(event) {
            if (event.target.classList.contains('att')) {
                const target = event.target;
                setTimeout(() => {
                    target.select();
                }, 0);
            }
        });

        // Right-click context menu listener for all attendance cells
        tb.addEventListener('contextmenu', function(event) {
            const input = event.target;
            if (input.classList.contains('att')) {
                const td = input.closest('td');
                const tr = input.closest('tr');
                if (td && tr) {
                    const day = parseInt(td.dataset.day);
                    const empId = tr.dataset.empid;
                    if (empId.startsWith("GLOBAL")) return; // Skip global adjustments

                    const y = parseInt(yS.value);
                    const m = parseInt(mS.value);
                    const d = new Date(y, m, day);
                    
                    const isAdmin = parseInt(role) === 1;
                    if (!isAdmin) {
                        return; // Regular users cannot edit or add remarks
                    }
                    
                    // Check if day is out of bounds
                    const emp = employees.find(e => e.MasterId === empId) || {};
                    const empEngagements = engagements.filter(ee => ee.EmpID === empId);
                    const state = getCellState(emp, y, m, day, empEngagements);
                    if (state.isOutOfBounds) {
                        return;
                    }
                    
                    // Check if closed and locked
                    let isLockedClosed = state.isClosed && !document.getElementById('chkUnlockClosed').checked;
                    if (isLockedClosed) {
                        return;
                    }
                    
                    event.preventDefault();
                    
                    activeContextEmpId = empId;
                    activeContextDay = day;
                    
                    const menu = document.getElementById("satContextMenu");
                    if (menu) {
                        menu.style.left = `${event.pageX}px`;
                        menu.style.top = `${event.pageY}px`;
                        menu.classList.add("show");
                        
                        const isSaturday = d.getDay() === 6;
                        const cell = attendanceData[empId]?.[day] || {};
                        
                        document.getElementById("ctxSatPresent").style.display = isSaturday ? "flex" : "none";
                        document.getElementById("ctxReset").style.display = (isSaturday && cell.ManualOverride) ? "flex" : "none";
                        document.getElementById("ctxAddRemarks").style.display = "flex";
                        document.getElementById("ctxClearRemarks").style.display = (cell.Remarks && cell.Remarks.trim() !== "" && !cell.ManualOverride) ? "flex" : "none";
                        
                        if (isSaturday) {
                            document.getElementById("ctxReset").style.borderTop = "1px solid #f1f5f9";
                            document.getElementById("ctxAddRemarks").style.borderTop = "1px solid #f1f5f9";
                        } else {
                            document.getElementById("ctxAddRemarks").style.borderTop = "none";
                        }
                    }
                }
            }
        });

        const currentYear = new Date().getFullYear();
        for (let y = currentYear - 2; y <= currentYear + 5; y++) {
            yS.innerHTML += `<option value="${y}">${y}</option>`;
        }

        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"].forEach((m, i) => {
            mS.innerHTML += `<option value="${i}">${m}</option>`;
        });

        yS.value = currentYear;
        mS.value = new Date().getMonth();

        let currentYearVal;
        let currentMonthVal;
        let currentCatVal;
        let currentDivVal;
        let currentSearchVal;

        const userDivision = '<%= Session["Division"] != null ? Session["Division"].ToString() : "" %>';

        function initSelectors() {
            if (role != 1) {
                const hd = document.getElementById("holidayDiv");
                if (hd) hd.style.display = "none";
                const ga = document.getElementById("globalAdjustDiv");
                if (ga) ga.style.display = "none";
                const uc = document.getElementById("unlockClosedDiv");
                if (uc) uc.style.display = "none";
            } else {
                const uc = document.getElementById("unlockClosedDiv");
                if (uc) uc.style.display = "block";
            }

            // Fetch categories
            const p1 = fetch('Attendance.aspx/GetCategories', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            }).then(r => r.json()).then(res => {
                const categories = JSON.parse(res.d);
                cS.innerHTML = '<option value="All">All</option>';
                categories.forEach(cat => {
                    cS.innerHTML += `<option value="${cat}">${cat}</option>`;
                });
                cS.value = "All";
            });

            // Fetch divisions
            const p2 = fetch('Attendance.aspx/GetDivisions', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            }).then(r => r.json()).then(res => {
                const divisions = JSON.parse(res.d);
                divS.innerHTML = '';
                if (role == 1 || divisions.length > 1) {
                    divS.innerHTML += '<option value="All">All</option>';
                }
                divisions.forEach(d => {
                    divS.innerHTML += `<option value="${d}">${d}</option>`;
                });
                if (divisions.length > 0) {
                    divS.value = (role == 1 || divisions.length > 1) ? "All" : divisions[0];
                }
                if (role != 1 && divisions.length <= 1) {
                    divS.disabled = true;
                } else {
                    divS.disabled = false;
                }
            });

            Promise.all([p1, p2]).then(() => {
                // Check if query parameters exist to pre-select correct Year and Month
                const urlParams = new URLSearchParams(window.location.search);
                if (urlParams.has('empId') && urlParams.has('date')) {
                    const dateStr = urlParams.get('date');
                    const parts = dateStr.split('-');
                    if (parts.length === 3) {
                        const monthAbbr = parts[1];
                        const year = parseInt(parts[2]);
                        const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                        const monthIndex = monthNames.indexOf(monthAbbr);
                        if (monthIndex >= 0 && !isNaN(year)) {
                            yS.value = year;
                            mS.value = monthIndex;
                        }
                    }
                    cS.value = "All";
                    if (divS.querySelector('option[value="All"]')) {
                        divS.value = "All";
                    }
                }

                // Initialize tracking values
                currentYearVal = yS.value;
                currentMonthVal = mS.value;
                currentCatVal = cS.value;
                currentDivVal = divS.value;
                currentSearchVal = searchBox.value;
                
                // Fetch data
                fetchData();
            }).catch(e => {
                console.error("Error loading dropdown data: ", e);
                currentYearVal = yS.value;
                currentMonthVal = mS.value;
                currentCatVal = cS.value;
                currentDivVal = divS.value;
                currentSearchVal = searchBox.value;
                fetchData();
            });
        }

        window.onbeforeunload = function() {
            if (isDirty) return "You have unsaved changes! Are you sure you want to leave?";
        };

        window.addEventListener('popstate', function(event) {
            if (_isDirty) {
                // Re-push the state to keep the user on the page
                history.pushState({ page: 'attendance_dirty' }, null, window.location.href);
                hasPushedDirtyState = true;
                
                // Show our custom confirmation modal
                showConfirmSaveModal(
                    () => {
                        saveData().then(success => {
                            if (success) {
                                window.isDirty = false;
                                history.back();
                            }
                        });
                    },
                    () => {
                        window.isDirty = false;
                        history.back();
                    },
                    () => {
                        // Cancel - do nothing and stay on page
                    }
                );
            }
        });

        // Prevent enter key from triggering default button (Logout) and intercept keyboard refresh keys (F5, Ctrl+R, Cmd+R)
        document.addEventListener('keydown', function(event) {
            if (event.keyCode === 13 && event.target.tagName === 'INPUT') {
                event.preventDefault();
                return false;
            }

            const isF5 = event.keyCode === 116;
            const isCtrlR = (event.ctrlKey || event.metaKey) && event.keyCode === 82;
            if ((isF5 || isCtrlR) && _isDirty) {
                event.preventDefault();
                
                showConfirmSaveModal(
                    () => {
                        saveData().then(success => {
                            if (success) {
                                window.isDirty = false;
                                window.location.reload();
                            }
                        });
                    },
                    () => {
                        window.isDirty = false;
                        window.location.reload();
                    },
                    () => {
                        // Cancel - do nothing and stay on page
                    }
                );
            }
        });

        // Custom Confirm Dialog Modal controller
        function showConfirmSaveModal(onSave, onDiscard, onCancel) {
            const modal = document.getElementById("confirmModal");
            if (!modal) return;
            
            const btnSave = document.getElementById("btnModalSave");
            const btnDiscard = document.getElementById("btnModalDiscard");
            const btnCancel = document.getElementById("btnModalCancel");
            
            modal.style.display = "flex";
            // trigger reflow
            modal.offsetHeight;
            modal.style.opacity = "1";
            modal.querySelector(".confirm-modal-box").style.transform = "scale(1)";
            
            function closeModal() {
                modal.style.opacity = "0";
                modal.querySelector(".confirm-modal-box").style.transform = "scale(0.92)";
                setTimeout(() => {
                    modal.style.display = "none";
                }, 250);
            }
            
            btnSave.onclick = function() {
                closeModal();
                if (onSave) onSave();
            };
            
            btnDiscard.onclick = function() {
                closeModal();
                if (onDiscard) onDiscard();
            };
            
            btnCancel.onclick = function() {
                closeModal();
                if (onCancel) onCancel();
            };
        }

        // Custom Pairing Confirm Modal controller
        function showPairingConfirmModal(id, day, y, m, onPaid, onUnpaid, onCancel) {
            const modal = document.getElementById("pairingModal");
            if (!modal) return;
            
            const btnPaid = document.getElementById("btnPairingPaid");
            const btnUnpaid = document.getElementById("btnPairingUnpaid");
            const btnCancel = document.getElementById("btnPairingCancel");
            
            modal.style.display = "flex";
            // trigger reflow
            modal.offsetHeight;
            modal.style.opacity = "1";
            modal.querySelector(".confirm-modal-box").style.transform = "scale(1)";
            
            function closeModal() {
                modal.style.opacity = "0";
                modal.querySelector(".confirm-modal-box").style.transform = "scale(0.92)";
                setTimeout(() => {
                    modal.style.display = "none";
                }, 250);
            }
            
            btnPaid.onclick = function() {
                if (!hasEnoughLeaveBalance(id, day, y, m, null, "Paired Paid")) {
                    showToast("Cannot select Paid pairing: Insufficient leave balance.", "error");
                    return;
                }
                closeModal();
                if (onPaid) onPaid();
            };
            
            btnUnpaid.onclick = function() {
                closeModal();
                if (onUnpaid) onUnpaid();
            };
            
            btnCancel.onclick = function() {
                closeModal();
                if (onCancel) onCancel();
            };
        }

        // Upgraded Toast Notification System
        function showToast(msg, type = 'info') {
            let container = document.getElementById("toast-container");
            if (!container) {
                container = document.createElement("div");
                container.id = "toast-container";
                document.body.appendChild(container);
            }

            const toast = document.createElement("div");
            toast.className = `modern-toast toast-${type}`;
            
            let iconClass = "fas fa-info-circle";
            if (type === "success") iconClass = "fas fa-check-circle";
            else if (type === "warning") iconClass = "fas fa-exclamation-triangle";
            else if (type === "error") iconClass = "fas fa-times-circle";

            toast.innerHTML = `
                <div class="toast-icon"><i class="${iconClass}"></i></div>
                <div style="flex-grow: 1; padding-right: 8px;">${msg}</div>
                <button type="button" class="toast-close-btn" onclick="this.parentElement.classList.remove('toast-show'); setTimeout(() => this.parentElement.remove(), 400);">&times;</button>
            `;

            container.appendChild(toast);
            
            // Trigger reflow to run transition
            toast.offsetHeight;
            toast.classList.add("toast-show");

            // Auto dismiss
            setTimeout(() => {
                if (toast.parentElement) {
                    toast.classList.remove("toast-show");
                    toast.classList.add("toast-hide");
                    setTimeout(() => {
                        toast.remove();
                    }, 400);
                }
            }, 4000);
        }

        // Maintain compatibility with existing code calling showPop
        function showPop(msg) {
            let type = "info";
            let lowerMsg = msg.toLowerCase();
            if (lowerMsg.includes("success") || lowerMsg.includes("reset")) {
                type = "success";
            } else if (lowerMsg.includes("error") || lowerMsg.includes("fail") || lowerMsg.includes("invalid")) {
                type = "error";
            } else if (lowerMsg.includes("saturday cut") || lowerMsg.includes("unsaved")) {
                type = "warning";
            }
            showToast(msg, type);
        }

        function handleDropdownChange(dropdown, prevValName) {
            const newVal = dropdown.value;
            const prevVal = window[prevValName];
            if (newVal === prevVal) return;
            
            if (isDirty) {
                // Revert UI immediately to preserve user view until choice is made
                dropdown.value = prevVal;
                
                showConfirmSaveModal(() => {
                    // Save and Continue
                    saveData().then(success => {
                        if (success) {
                            dropdown.value = newVal;
                            window[prevValName] = newVal;
                            fetchData();
                        }
                    });
                }, () => {
                    // Discard and Continue
                    isDirty = false;
                    dropdown.value = newVal;
                    window[prevValName] = newVal;
                    fetchData();
                }, () => {
                    // Cancel - stay on page, do not change dropdown value
                });
            } else {
                window[prevValName] = newVal;
                fetchData();
            }
        }

        yS.onchange = () => handleDropdownChange(yS, 'currentYearVal');
        mS.onchange = () => handleDropdownChange(mS, 'currentMonthVal');
        cS.onchange = () => handleDropdownChange(cS, 'currentCatVal');
        divS.onchange = () => handleDropdownChange(divS, 'currentDivVal');

        let searchTimeout = null;
        searchBox.oninput = function() {
            const newVal = searchBox.value;
            if (newVal === currentSearchVal) return;
            
            if (isDirty) {
                searchBox.value = currentSearchVal;
                showConfirmSaveModal(() => {
                    saveData().then(success => {
                        if (success) {
                            searchBox.value = newVal;
                            currentSearchVal = newVal;
                            fetchData();
                        }
                    });
                }, () => {
                    isDirty = false;
                    searchBox.value = newVal;
                    currentSearchVal = newVal;
                    fetchData();
                }, () => {
                    // Cancel - keep previous value
                });
            } else {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    currentSearchVal = newVal;
                    fetchData();
                }, 300);
            }
        };

        // Intercept navigation links
        document.addEventListener("DOMContentLoaded", () => {
            const interceptLinks = () => {
                const links = document.querySelectorAll('a[href]');
                links.forEach(link => {
                    if (link.dataset.intercepted) return;
                    link.dataset.intercepted = "true";
                    
                    link.addEventListener('click', function(e) {
                        const href = this.getAttribute('href');
                        if (!href || href.startsWith('#') || href.startsWith('javascript:') || this.getAttribute('target') === '_blank') return;
                        
                        if (isDirty) {
                            e.preventDefault();
                            showConfirmSaveModal(() => {
                                saveData().then(success => {
                                    if (success) {
                                        isDirty = false;
                                        window.location.href = href;
                                    }
                                });
                            }, () => {
                                isDirty = false;
                                window.location.href = href;
                            }, () => {
                                // Cancel
                            });
                        }
                    });
                });
            };
            
            interceptLinks();
            // Periodically check for dynamically added links
            setInterval(interceptLinks, 1500);

            // Intercept standard postback triggers like Logout button
            const form = document.getElementById('form1');
            if (form) {
                form.addEventListener('submit', function(e) {
                    const activeElement = document.activeElement;
                    if (activeElement && activeElement.id && activeElement.id.includes('btnLogout')) {
                        if (isDirty) {
                            e.preventDefault();
                            showConfirmSaveModal(() => {
                                saveData().then(success => {
                                    if (success) {
                                        isDirty = false;
                                        __doPostBack(activeElement.name || activeElement.id, '');
                                    }
                                });
                            }, () => {
                                isDirty = false;
                                __doPostBack(activeElement.name || activeElement.id, '');
                            }, () => {
                                // Cancel
                            });
                        }
                    }
                });
            }
            initMiniPopupEvents();
        });

        function showLoading(text) {
            const overlay = document.getElementById("loadingOverlay");
            const txt = document.getElementById("loadingText");
            if (txt) {
                txt.textContent = text || "Loading Attendance Data...";
            }
            if (overlay) overlay.style.display = "flex";
        }
        
        function hideLoading() {
            const overlay = document.getElementById("loadingOverlay");
            if (overlay) overlay.style.display = "none";
        }

        function fetchData() {
            showLoading();
            const req = {
                year: parseInt(yS.value),
                month: parseInt(mS.value),
                category: cS.value,
                division: divS.value,
                search: searchBox.value
            };

            fetch('Attendance.aspx/GetData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(req)
            }).then(r => r.json()).then(res => {
                const data = JSON.parse(res.d);
                employees = data.Employees;
                attendanceData = data.Attendance || {};
                prevAttendanceData = data.PrevAttendance || {};
                futureCarriedData = data.FutureCarried || {};
                futureUpdates = {};
                engagements = data.Engagements || [];
                
                // Restore ManualOverride flag for manually set Saturdays
                const y = parseInt(yS.value);
                const m = parseInt(mS.value);
                for (let empId in attendanceData) {
                    if (empId.startsWith("GLOBAL")) continue;
                    const empCells = attendanceData[empId];
                    for (let dKey in empCells) {
                        const cell = empCells[dKey];
                        const dayNum = parseInt(dKey);
                        const d = new Date(y, m, dayNum);
                        if (d.getDay() === 6 && cell && cell.Val !== null && cell.AutoSat === false) {
                            cell.ManualOverride = true;
                        }
                    }
                }
                
                // Automatically run Saturday Cut calculations for all loaded employees on load
                employees.forEach(emp => {
                    attendanceData[emp.MasterId] = attendanceData[emp.MasterId] || {};
                    calcSat(emp.MasterId, true);
                });
                
                isDirty = false;
                if (currentSortCol) {
                    employees.sort((a, b) => {
                        let valA = (currentSortCol === 'ID') ? a.ID : a.Name;
                        let valB = (currentSortCol === 'ID') ? b.ID : b.Name;
                        valA = (valA || '').toString().toLowerCase().trim();
                        valB = (valB || '').toString().toLowerCase().trim();
                        if (currentSortCol === 'ID') {
                            const numA = parseFloat(valA);
                            const numB = parseFloat(valB);
                            if (!isNaN(numA) && !isNaN(numB)) {
                                return currentSortDir === 'asc' ? numA - numB : numB - numA;
                            }
                        }
                        return currentSortDir === 'asc' ? valA.localeCompare(valB) : valB.localeCompare(valA);
                    });
                }
                render();
                hideLoading();
                
                // Show loaded notification
                const year = yS.value;
                const monthText = mS.options[mS.selectedIndex].text;
                const category = cS.value;
                const division = divS.value;
                
                // Highlight and show correction banner if query parameter exists
                const urlParams = new URLSearchParams(window.location.search);
                if (urlParams.has('empId') && urlParams.has('date') && urlParams.has('remark')) {
                    const empId = urlParams.get('empId');
                    const dateStr = urlParams.get('date');
                    const remarkMsg = urlParams.get('remark');
                    
                    const emp = employees.find(e => e.MasterId === empId);
                    if (emp) {
                        document.getElementById('bannerMeta').innerHTML = 
                            `Employee: <span class="font-weight-bold" style="color:#1e293b;">${emp.Name} (${emp.ID})</span> &bull; Dates of Concern: <span class="font-weight-bold" style="color:#1e293b;">${dateStr}</span>`;
                        document.getElementById('bannerText').innerText = remarkMsg;
                        
                        const banner = document.getElementById('correctionRemarkBanner');
                        banner.style.display = 'block';
                        
                        // Automatically dismiss the banner smoothly after 8 seconds
                        setTimeout(() => {
                            dismissCorrectionBanner();
                        }, 8000);

                        // Parse all concern days
                        const dates = dateStr.split(',').map(d => d.trim());
                        const dayNums = dates.map(dStr => {
                            const parts = dStr.split('-');
                            return parts.length > 0 ? parseInt(parts[0]) : NaN;
                        }).filter(num => !isNaN(num));

                        setTimeout(() => {
                            const row = document.querySelector(`tr[data-empid="${empId}"]`);
                            if (row) {
                                row.scrollIntoView({ behavior: 'smooth', block: 'center' });
                                
                                // Set prominent yellow background
                                row.style.transition = "background-color 0.8s ease";
                                row.style.backgroundColor = "#fef08a"; // Tailwind yellow-200
                                
                                let firstFocused = false;
                                dayNums.forEach(dayNum => {
                                    const cell = row.querySelector(`td[data-day="${dayNum}"]`);
                                    if (cell) {
                                        cell.style.outline = "3px solid #ef4444"; // Highlight outline
                                        cell.style.outlineOffset = "-3px";
                                        if (!firstFocused) {
                                            const cellInput = cell.querySelector(".att");
                                            if (cellInput) {
                                                cellInput.focus();
                                                cellInput.select();
                                                firstFocused = true;
                                            }
                                        }
                                    }
                                });
                                
                                // Smoothly fade back the row highlight color and cell outlines after 5 seconds
                                setTimeout(() => {
                                    row.style.backgroundColor = "";
                                    dayNums.forEach(dayNum => {
                                        const cell = row.querySelector(`td[data-day="${dayNum}"]`);
                                        if (cell) {
                                            cell.style.outline = "";
                                            cell.style.outlineOffset = "";
                                        }
                                    });
                                }, 5000);
                            }
                        }, 600);
                    }
                } else {
                    showPop(`Loaded attendance for ${monthText} ${year} (${category} - ${division}) successfully!`);
                }
            }).catch(e => {
                console.error(e);
                hideLoading();
                showPop("Error loading attendance data");
            });
        }

        function saveData(saveYear, saveMonth) {
            // Validate: If 0 is entered/selected, make sure Paid or Unpaid is selected
            const y = (typeof saveYear === 'number') ? saveYear : parseInt(yS.value);
            const m = (typeof saveMonth === 'number') ? saveMonth : parseInt(mS.value);
            const days = new Date(y, m + 1, 0).getDate();

            for (let empId in attendanceData) {
                if (empId.startsWith("GLOBAL")) continue;
                const emp = employees.find(e => e.MasterId === empId);
                if (!emp) continue;

                const data = attendanceData[empId];
                const empEngagements = engagements.filter(ee => ee.EmpID === emp.MasterId);
                for (let i = 1; i <= days; i++) {
                    const d = new Date(y, m, i);
                    if (d.getDay() === 0) continue; // Skip Sundays

                    const cell = data[i] || {};
                    let state = getCellState(emp, y, m, i, empEngagements);
                    if (state.isReadonlyCell || state.isOutOfBounds) continue;

                    // If cell value is 0 (and not Holiday)
                    if (cell.Val === 0 && !cell.Holiday) {
                        if (cell.Leave !== "Paid" && cell.Leave !== "Unpaid" && cell.Leave !== "Paired Unpaid") {
                            showToast(`Please select Paid or Unpaid leave for ${emp.Name} (ID: ${emp.ID}) on day ${i}`, "error");
                            
                            // Highlight the select dropdown
                            const row = Array.from(tb.querySelectorAll("tr")).find(tr => tr.dataset.empid === empId);
                            if (row) {
                                const td = row.querySelector(`td[data-day="${i}"]`);
                                if (td) {
                                    const select = td.querySelector(".leave-opt");
                                    if (select) {
                                        select.focus();
                                        select.style.border = "2px solid #dc2626";
                                    }
                                }
                            }
                            return Promise.resolve(false);
                        }
                    }
                }
            }

            const req = {
                year: y,
                month: m,
                category: cS.value,
                data: JSON.stringify(attendanceData),
                futureUpdates: JSON.stringify(futureUpdates)
            };

            showLoading("Saving Attendance Data...");

            return fetch('Attendance.aspx/SaveData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(req)
            }).then(r => {
                hideLoading();
                if (!r.ok) throw new Error("Server error: " + r.statusText);
                return r.json();
            }).then(res => {
                const responseObj = JSON.parse(res.d || "{}");
                if (responseObj.status === "error") {
                    showToast(responseObj.message || "Error saving", "error");
                    showPop(responseObj.message || "Error saving");
                    return false;
                }
                isDirty = false;
                futureUpdates = {};
                showPop("Saved Successfully");
                return true;
            }).catch(e => {
                hideLoading();
                console.error(e);
                showPop("Error saving");
                return false;
            });
        }

        function getRefDays(y, m) {
            let d = new Date(y, m, 0), arr = [];
            while (d.getDay() != 6) {
                if (d.getDay() != 0) {
                    arr.unshift(new Date(d));
                }
                d.setDate(d.getDate() - 1);
            }
            return arr;
        }

        function render() {
            const y = parseInt(yS.value);
            const m = parseInt(mS.value);
            const days = new Date(y, m + 1, 0).getDate();
            const refs = getRefDays(y, m);
            
            let idArrow = '<span class="sort-icon ml-1" style="color: #94a3b8; font-size: 0.75rem;"><i class="fas fa-sort"></i></span>';
            let nameArrow = '<span class="sort-icon ml-1" style="color: #94a3b8; font-size: 0.75rem;"><i class="fas fa-sort"></i></span>';
            
            if (currentSortCol === 'ID') {
                idArrow = `<span class="sort-icon ml-1" style="color: #4f46e5; font-size: 0.75rem;"><i class="fas ${currentSortDir === 'asc' ? 'fa-sort-up' : 'fa-sort-down'}"></i></span>`;
            } else if (currentSortCol === 'Name') {
                nameArrow = `<span class="sort-icon ml-1" style="color: #4f46e5; font-size: 0.75rem;"><i class="fas ${currentSortDir === 'asc' ? 'fa-sort-up' : 'fa-sort-down'}"></i></span>`;
            }
            
            let head = `<tr><th class="sortable-header" onclick="toggleSort('ID')">ID ${idArrow}</th><th class="sortable-header" style="text-align:left;" onclick="toggleSort('Name')">Name ${nameArrow}</th>`;
            
            refs.forEach(d => {
                head += `<th class="gray">${String(d.getDate()).padStart(2, '0')}<br>${d.toLocaleDateString('en', { weekday: 'short' })}</th>`;
            });

            for (let i = 1; i <= days; i++) {
                let d = new Date(y, m, i);
                if (d.getDay() !== 0) { 
                    head += `<th>${String(i).padStart(2, '0')}<br>${d.toLocaleDateString('en', { weekday: 'short' })}</th>`;
                }
            }
            head += `<th>Present</th><th>Adj</th><th>Total</th></tr>`;
            th.innerHTML = head;

            let rows = "";
            employees.forEach((emp, empIdx) => {
                const empEngagements = engagements.filter(ee => ee.EmpID === emp.MasterId);
                attendanceData[emp.MasterId] = attendanceData[emp.MasterId] || {};
                let count = 0;
                let maxTotal = 0;
                let r = `<tr data-empid="${emp.MasterId}"><td>${emp.ID}</td><td class="emp-name-click" style="text-align:left;" onclick="showLeavePopup('${emp.MasterId}')">${emp.Name}</td>`;

                refs.forEach(d => {
                    let prevDay = d.getDate();
                    let pCell = (prevAttendanceData[emp.MasterId] && prevAttendanceData[emp.MasterId][prevDay]) ? prevAttendanceData[emp.MasterId][prevDay] : { Val: null, Leave: "" };
                    let pVal = (pCell.Val === null || pCell.Val === undefined) ? "" : pCell.Val;
                    const pLeaveClean = pCell.Leave ? pCell.Leave.trim() : "";
                    if (pLeaveClean === "Paid" || pLeaveClean === "Paired Paid" || pLeaveClean === "Carried") {
                        pVal = "1";
                    } else if (pLeaveClean === "Unpaid" || pLeaveClean === "Paired Unpaid") {
                        pVal = "0";
                    }
                    let pLabel = pCell.Leave ? `<span class="label-text" style="color:gray;">${pCell.Leave}</span>` : "";
                    
                    r += `<td class="gray"><input class="att" value="${pVal}" readonly tabindex="-1" style="color:gray;border-color:#ccc;">${pLabel}</td>`;
                });

                for (let i = 1; i <= days; i++) {
                    let d = new Date(y, m, i);
                    let isToday = false;
                    const todayDate = new Date();
                    if (d.getDate() === todayDate.getDate() && d.getMonth() === todayDate.getMonth() && d.getFullYear() === todayDate.getFullYear()) {
                        isToday = true;
                    }
                    const cell = attendanceData[emp.MasterId][i] || { Val: null, Holiday: false, Leave: "" };
                    
                    if (d.getDay() === 0 && !cell.Holiday) continue;

                    let cls = "", drop = "", label = "";
                    let valToDisplay = (cell.Val === null || cell.Val === undefined) ? '' : cell.Val;

                    let state = getCellState(emp, y, m, i, empEngagements);
                    let isOutOfBounds = state.isOutOfBounds;
                    let readonlyAttr = state.readonlyAttr;

                    if (!isOutOfBounds) maxTotal++;

                    if (isOutOfBounds) {
                        cls = "";
                        valToDisplay = "";
                    } else {
                        if (cell.Holiday) { 
                            cls = "royal-blue"; 
                            valToDisplay = "H";
                            count += 1;
                            readonlyAttr = 'readonly tabindex="-1" style="background:transparent; border:none;"';
                        }
                        else if (cell.Leave === "Carried") {
                            cls = "green";
                            valToDisplay = "1";
                            count += 1;
                        }
                        else if (cell.Leave === "Paired Paid") {
                            cls = "light-yellow";
                            valToDisplay = "1";
                            count += 1;
                        }
                        else if (cell.Leave === "Paired Unpaid") {
                            cls = "light-yellow";
                            valToDisplay = "0";
                        }
                        else if (cell.Val === 1) { 
                            cls = "green"; 
                            count += 1; 
                        }
                        else if (cell.Val === 0.5) { 
                            cls = "light-yellow"; 
                            count += 0.5;
                        }
                        else if (cell.Val === 0) { 
                            if (cell.Leave === "Paid") {
                                cls = "green";
                                valToDisplay = "1";
                                count += 1;
                            } else {
                                cls = "red";
                            }
                        }

                        if (cell.Leave) {
                            label = `<span class="label-text">${cell.Leave}</span>`;
                        }

                        if (!state.isReadonlyCell) {
                            if (cell.Leave === "Carried" || cell.Leave === "Paired Paid" || cell.Leave === "Paired Unpaid") {
                                drop = `<select class="leave-opt" onchange="setLeave('${emp.MasterId}', ${i}, this.value, event)">
                                    <option value="${cell.Leave}" selected>${cell.Leave}</option>
                                    <option value="Reset">Reset</option>
                                </select>`;
                            }
                            else if (cell.Val == 0.5) {
                                drop = `<select class="leave-opt" onchange="setLeave('${emp.MasterId}', ${i}, this.value, event)">
                                    <option value=""></option>
                                    <option value="Carried">Carried</option>
                                    <option value="Pairing">Pairing</option>
                                </select>`;
                            }
                            else if (cell.Val == 0 && cell.Val !== "") {
                                drop = `<select class="leave-opt" onchange="setLeave('${emp.MasterId}', ${i}, this.value, event)">
                                    <option value=""></option>
                                    <option value="Paid" ${cell.Leave=="Paid"?"selected":""}>Paid</option>
                                    <option value="Unpaid" ${cell.Leave=="Unpaid"?"selected":""}>Unpaid</option>
                                </select>`;
                            }
                        }
                    }

                    let titleAttr = (cell.Holiday || cell.Remarks) ? `title="${(cell.Remarks || (cell.Holiday ? 'Holiday' : '')).replace(/"/g, '&quot;')}"` : '';
                    let tdClass = cls;
                    if (cell.Remarks && cell.Remarks.trim() !== "") {
                        tdClass += " has-remarks";
                    }

                    r += `<td class="${tdClass}" data-day="${i}" ${titleAttr}>
                            <input class="att" value="${valToDisplay}" oninput="setVal('${emp.MasterId}', ${i}, this.value, event)" ${readonlyAttr} ${titleAttr}>
                            ${label}
                            ${drop}
                          </td>`;
                }
                
                let adj = 0;
                if (attendanceData["GLOBAL"] && attendanceData["GLOBAL"][0]) {
                    adj += attendanceData["GLOBAL"][0].Val || 0;
                }
                if (emp.Category) {
                    const catKey = "GLOBAL_" + emp.Category;
                    if (attendanceData[catKey] && attendanceData[catKey][0]) {
                        adj += attendanceData[catKey][0].Val || 0;
                    }
                }
                
                r += `<td class="total-col fw-bold">${count}</td>
                      <td class="total-col">${(adj > 0 ? '+' : '') + (adj !== 0 ? adj : '-')}</td>
                      <td class="total-col text-primary fw-bold">${count + adj}</td></tr>`;
                rows += r;
            });
            tb.innerHTML = rows;
        }

        function toggleSort(col) {
            if (currentSortCol === col) {
                currentSortDir = (currentSortDir === 'asc') ? 'desc' : 'asc';
            } else {
                currentSortCol = col;
                currentSortDir = 'asc';
            }
            
            // Perform sort on employees array
            employees.sort((a, b) => {
                let valA = (col === 'ID') ? a.ID : a.Name;
                let valB = (col === 'ID') ? b.ID : b.Name;
                
                valA = (valA || '').toString().toLowerCase().trim();
                valB = (valB || '').toString().toLowerCase().trim();
                
                // Try numeric sort for ID if both are numbers
                if (col === 'ID') {
                    const numA = parseFloat(valA);
                    const numB = parseFloat(valB);
                    if (!isNaN(numA) && !isNaN(numB)) {
                        return currentSortDir === 'asc' ? numA - numB : numB - numA;
                    }
                }
                
                return currentSortDir === 'asc' ? valA.localeCompare(valB) : valB.localeCompare(valA);
            });
            
            render();
        }

        function setLeave(id, day, val, event) {
            const y = parseInt(yS.value);
            const m = parseInt(mS.value);
            const emp = employees.find(e => e.MasterId === id) || {};
            const empEngagements = engagements.filter(ee => ee.EmpID === id);
            let state = getCellState(emp, y, m, day, empEngagements);
            if (state.isReadonlyCell) {
                if (event && event.target) {
                    event.target.value = attendanceData[id]?.[day]?.Leave || "";
                }
                return;
            }

            attendanceData[id] = attendanceData[id] || {};
            attendanceData[id][day] = attendanceData[id][day] || {};
            let cell = attendanceData[id][day];
            let prevCellState = { Val: cell.Val, Leave: cell.Leave };
            
            if (val === "Reset") {
                cell.Val = null;
                cell.Leave = "";
                showPop("Reset Completed");
                reprocessHalfDays(id, day, prevCellState, event);
                return;
            }
            else if (val === "Carried") {
                cell.Val = 1;
                cell.Leave = "Carried";
                showPop("Half Day Carried to Ledger Pending");
                reprocessHalfDays(id, day, prevCellState, event);
                return;
            }
            else if (val === "Pairing") {
                cell.Val = 0.5;
                cell.Leave = "";
                reprocessHalfDays(id, day, prevCellState, event);
                return;
            }
            else if (val === "Paid") {
                if (!hasEnoughLeaveBalance(id, day, null, null, 0, "Paid")) {
                    showToast("Cannot apply for Paid Leave: Leave balance is 0 or insufficient.", "error");
                    if (event && event.target) {
                        event.target.value = prevCellState.Leave || "";
                    }
                    cell.Val = prevCellState.Val;
                    cell.Leave = prevCellState.Leave;
                    return;
                }
                cell.Val = 0;
                cell.Leave = "Paid";
                showPop("Paid Leave Added");
            }
            else if (val === "Unpaid") {
                cell.Val = 0;
                cell.Leave = "Unpaid";
                showPop("Unpaid Leave Added");
            }
            else {
                cell.Leave = "";
            }
            
            isDirty = true;
            calcSat(id);
            updateRowUI(event.target.closest("tr"), id);
            if (activePopupEmpId === id || val === "Paid" || val === "Unpaid" || val === "Reset" || val === "Carried") {
                showLeavePopup(id);
            }
            
            const wasHalfDay = prevCellState.Val === 0.5 || prevCellState.Leave === "Carried" || prevCellState.Leave === "Paired Paid" || prevCellState.Leave === "Paired Unpaid";
            if (wasHalfDay) {
                reprocessHalfDays(id, day, prevCellState, event);
            }
        }

        function calcSat(id, isInitialLoad) {
            const y = parseInt(yS.value);
            const m = parseInt(mS.value);
            const days = new Date(y, m + 1, 0).getDate();
            const data = attendanceData[id];
            const emp = employees.find(e => e.MasterId === id) || {};
            const empEngagements = engagements.filter(ee => ee.EmpID === id);

            let halfEntries = 0;
            Object.keys(data).forEach(d => {
                if (data[d]?.Val === 0.5) halfEntries++;
            });
            const halfPairExists = halfEntries >= 2 && halfEntries % 2 === 0;

            let satChanged = false;

            for (let i = 1; i <= days; i++) {
                let d = new Date(y, m, i);
                if (d.getDay() == 6) {
                    let state = getCellState(emp, y, m, i, empEngagements);
                    if (state.isOutOfBounds) {
                        if (data[i]) {
                            delete data[i].Val;
                            delete data[i].AutoSat;
                        }
                        continue;
                    }
                    if (data[i] && data[i].ManualOverride) {
                        continue;
                    }
                    let ok = true;
                    
                    // If employee joined in the middle of this week (Monday < JoinDate <= Friday), Saturday is 0
                    let monday = new Date(d);
                    monday.setDate(d.getDate() - 5);
                    let friday = new Date(d);
                    friday.setDate(d.getDate() - 1);
                    
                    let monStr = `${monday.getFullYear()}-${String(monday.getMonth() + 1).padStart(2, '0')}-${String(monday.getDate()).padStart(2, '0')}`;
                    let friStr = `${friday.getFullYear()}-${String(friday.getMonth() + 1).padStart(2, '0')}-${String(friday.getDate()).padStart(2, '0')}`;
                    
                    if (emp.JoinDate && emp.JoinDate > monStr && emp.JoinDate <= friStr) {
                        ok = false;
                    } else {
                        let checkedDaysCount = 0;
                        for (let k = 1; k <= 5; k++) {
                            let c = new Date(d);
                            c.setDate(d.getDate() - k);

                            // Check employee engagement bounds
                            let cStr = `${c.getFullYear()}-${String(c.getMonth() + 1).padStart(2, '0')}-${String(c.getDate()).padStart(2, '0')}`;
                             let isOutOfBoundsWeek = !empEngagements.find(ee => ee.StartDate <= cStr && (!ee.EndDate || cStr <= ee.EndDate));
                            if (isOutOfBoundsWeek) {
                                continue; // Out of bounds, skip checking (ignored, does not penalize Saturday)
                            }

                            checkedDaysCount++;

                            let v = null;
                            let l = "";
                            let isHol = false;
                            if (c.getMonth() == m) {
                                v = data[c.getDate()]?.Val;
                                l = data[c.getDate()]?.Leave;
                                isHol = data[c.getDate()]?.Holiday || false;
                            } else {
                                v = prevAttendanceData[id]?.[c.getDate()]?.Val;
                                l = prevAttendanceData[id]?.[c.getDate()]?.Leave;
                                isHol = prevAttendanceData[id]?.[c.getDate()]?.Holiday || false;
                            }
                            
                            let came = (v === 1) || (v === 0.5) || (l === "Paid") || (l === "Carried") || (l === "Paired Paid") || (l === "Paired Unpaid") || (isHol === true);
                            if (!came) {
                                ok = false;
                                break;
                            }
                        }
                        if (checkedDaysCount === 0) {
                            ok = false;
                        }
                    }

                    let oldVal = data[i]?.Val;
                    if (!ok && !halfPairExists) {
                        if (!data[i]?.Holiday) {
                            data[i] = data[i] || {};
                            data[i].Val = 0;
                            data[i].AutoSat = true;
                            if (oldVal !== 0) { 
                                satChanged = true; 
                                if (!isInitialLoad) {
                                    showPop("Saturday Cut Applied");
                                    isDirty = true;
                                }
                            }
                        }
                    } else {
                        data[i] = data[i] || {};
                        data[i].Val = 1;
                        data[i].AutoSat = true;
                        if (oldVal !== 1) { 
                            satChanged = true; 
                            if (!isInitialLoad) {
                                isDirty = true;
                            }
                        }
                    }
                }
            }
            return satChanged;
        }

        function updateRowUI(tr, empID) {
            const emp = employees.find(e => e.MasterId === empID) || {};
            const empEngagements = engagements.filter(ee => ee.EmpID === emp.MasterId);
            let count = 0;
            let maxTotal = 0;
            const y = parseInt(yS.value);
            const m = parseInt(mS.value);
            const days = new Date(y, m + 1, 0).getDate();
            const data = attendanceData[empID];
            
            // First loop: calculate total count
            for (let i = 1; i <= days; i++) {
                const cell = data[i] || { Val: null, Holiday: false, Leave: "" };
                let d = new Date(y, m, i);
                
                let state = getCellState(emp, y, m, i, empEngagements);
                let isOutOfBounds = state.isOutOfBounds;
                if (isOutOfBounds) {
                    continue;
                }
                
                if (d.getDay() === 0 && !cell.Holiday) continue;
                maxTotal++;

                if (cell.Holiday) count += 1;
                else if (cell.Leave === "Carried") count += 1;
                else if (cell.Leave === "Paired Paid") count += 1;
                else if (cell.Leave === "Paired Unpaid") { /* count += 0 */ }
                else if (cell.Val === 1) count += 1;
                else if (cell.Val === 0.5) count += 0.5;
                else if (cell.Val === 0) {
                    if (cell.Leave === "Paid") count += 1;
                }
            }
            
            let adj = 0;
            if (attendanceData["GLOBAL"] && attendanceData["GLOBAL"][0]) {
                adj += attendanceData["GLOBAL"][0].Val || 0;
            }
            if (emp.Category) {
                const catKey = "GLOBAL_" + emp.Category;
                if (attendanceData[catKey] && attendanceData[catKey][0]) {
                    adj += attendanceData[catKey][0].Val || 0;
                }
            }
            let cols = tr.querySelectorAll(".total-col");
            if (cols[0].innerText !== String(count)) cols[0].innerText = count;
            let adjText = (adj > 0 ? '+' : '') + (adj !== 0 ? adj : '-');
            if (cols[1].innerText !== adjText) cols[1].innerText = adjText;
            let totalVal = count + adj;
            if (cols[2].innerText !== String(totalVal)) cols[2].innerText = totalVal;

            for (let i = 1; i <= days; i++) {
                let d = new Date(y, m, i);
                const cell = data[i] || { Val: null, Holiday: false, Leave: "" };
                if (d.getDay() === 0 && !cell.Holiday) continue;
                
                const td = tr.querySelector(`td[data-day="${i}"]`);
                if (!td) continue;

                let state = getCellState(emp, y, m, i, empEngagements);
                let isOutOfBounds = state.isOutOfBounds;
                let readonlyAttr = state.readonlyAttr;

                let cls = "", valToDisplay = cell.Val;
                
                if (isOutOfBounds) {
                    cls = "";
                    valToDisplay = "";
                } else {
                    if (cell.Holiday) { 
                        cls = "royal-blue"; 
                        valToDisplay = "H"; 
                        readonlyAttr = 'readonly tabindex="-1" style="background:transparent; border:none;"';
                    }
                    else if (cell.Leave === "Carried") {
                        cls = "green";
                        valToDisplay = "1";
                    }
                    else if (cell.Leave === "Paired Paid") {
                        cls = "light-yellow";
                        valToDisplay = "1";
                    }
                    else if (cell.Leave === "Paired Unpaid") {
                        cls = "light-yellow";
                        valToDisplay = "0";
                    }
                    else if (cell.Val === 1) {
                        cls = "green";
                        valToDisplay = "1";
                    }
                    else if (cell.Val === 0.5) {
                        cls = "light-yellow";
                        valToDisplay = "0.5";
                    }
                    else if (cell.Val === 0) {
                        if (cell.Leave === "Paid") {
                            cls = "green";
                            valToDisplay = "1";
                        } else {
                            cls = "red";
                        }
                    }
                }
                
                if (td.className !== cls) {
                    td.className = cls;
                }
                
                const inp = td.querySelector(".att");
                if (inp && document.activeElement !== inp) { 
                    let currentVal = (valToDisplay === null || valToDisplay === undefined) ? "" : valToDisplay;
                    if (inp.value !== currentVal) {
                        inp.value = currentVal;
                    }
                    if (inp.dataset.readonlyAttr !== readonlyAttr) {
                        if (readonlyAttr.includes("readonly")) {
                            if (!inp.hasAttribute("readonly")) {
                                inp.setAttribute("readonly", "readonly");
                                inp.setAttribute("tabindex", "-1");
                            }
                        } else {
                            if (inp.hasAttribute("readonly")) {
                                inp.removeAttribute("readonly");
                                inp.removeAttribute("tabindex");
                            }
                        }
                        let styleVal = readonlyAttr.split('style="')[1]?.split('"')[0] || "";
                        inp.setAttribute("style", styleVal);
                        inp.dataset.readonlyAttr = readonlyAttr;
                    }
                }

                if (cell.Holiday || cell.Remarks) {
                    let titleVal = cell.Remarks || 'Holiday';
                    if (td.getAttribute("title") !== titleVal) {
                        td.setAttribute("title", titleVal);
                    }
                    if (inp && inp.getAttribute("title") !== titleVal) {
                        inp.setAttribute("title", titleVal);
                    }
                } else {
                    if (td.hasAttribute("title")) {
                        td.removeAttribute("title");
                    }
                    if (inp && inp.hasAttribute("title")) {
                        inp.removeAttribute("title");
                    }
                }

                let drop = td.querySelector(".leave-opt");
                let shouldShowDrop = false;
                let dropHtml = "";

                if (!isOutOfBounds && !cell.Holiday && !state.isReadonlyCell) {
                    if (cell.Leave === "Carried" || cell.Leave === "Paired Paid" || cell.Leave === "Paired Unpaid") {
                        shouldShowDrop = true;
                        dropHtml = `<option value="${cell.Leave}" selected>${cell.Leave}</option><option value="Reset">Reset</option>`;
                    }
                    else if (cell.Val == 0.5) {
                        shouldShowDrop = true;
                        dropHtml = `<option value=""></option><option value="Carried">Carried</option><option value="Pairing">Pairing</option>`;
                    }
                    else if (cell.Val == 0 && cell.Val !== "") {
                        shouldShowDrop = true;
                        dropHtml = `<option value=""></option><option value="Paid" ${cell.Leave=="Paid"?"selected":""}>Paid</option><option value="Unpaid" ${cell.Leave=="Unpaid"?"selected":""}>Unpaid</option>`;
                    }
                }

                if (shouldShowDrop) {
                    let mode = "", leaveVal = "";
                    if (cell.Leave === "Carried" || cell.Leave === "Paired Paid" || cell.Leave === "Paired Unpaid") {
                        mode = "carried-paired";
                        leaveVal = cell.Leave;
                    } else if (cell.Val == 0.5) {
                        mode = "half-day";
                    } else if (cell.Val == 0 && cell.Val !== "") {
                        mode = "zero-absence";
                        leaveVal = cell.Leave || "";
                    }

                    if (!drop) {
                        drop = document.createElement("select");
                        drop.className = "leave-opt";
                        drop.onchange = (e) => {
                            setLeave(empID, i, e.target.value, e);
                        };
                        td.appendChild(drop);
                    }
                    if (drop.dataset.mode !== mode || drop.dataset.leave !== leaveVal) {
                        drop.innerHTML = dropHtml;
                        drop.dataset.mode = mode;
                        drop.dataset.leave = leaveVal;
                    }
                } else {
                    if (drop) drop.remove();
                }

                let labelSpan = td.querySelector(".label-text");
                if (cell.Leave) {
                    if (!labelSpan) {
                        labelSpan = document.createElement("span");
                        labelSpan.className = "label-text";
                        td.appendChild(labelSpan);
                    }
                    if (labelSpan.innerText !== cell.Leave) {
                        labelSpan.innerText = cell.Leave;
                    }
                } else {
                    if (labelSpan) labelSpan.remove();
                }
            }

            // Refresh the mini popup only if the active element belongs to this row and its cell state warrants it
            const activeEl = document.activeElement;
            if (activeEl && (activeEl.classList.contains("att") || activeEl.classList.contains("leave-opt"))) {
                const activeTr = activeEl.closest("tr");
                if (activeTr === tr) {
                    const activeTd = activeEl.closest("td");
                    const day = activeTd ? parseInt(activeTd.dataset.day) : null;
                    if (day && shouldShowMiniPopupForCell(empID, day)) {
                        showMiniLeavePopup(activeEl, empID);
                    } else {
                        hideMiniLeavePopup();
                    }
                }
            }
        }

        function setVal(id, day, v, event) {
            const y = parseInt(yS.value);
            const m = parseInt(mS.value);
            const emp = employees.find(e => e.MasterId === id) || {};
            const empEngagements = engagements.filter(ee => ee.EmpID === id);
            let state = getCellState(emp, y, m, day, empEngagements);
            if (state.isReadonlyCell) {
                if (event && event.target) {
                    event.target.value = attendanceData[id]?.[day]?.Val !== null ? attendanceData[id][day].Val : "";
                }
                return;
            }

            if (v === "5") {
                v = "0.5";
                event.target.value = "0.5";
            }

            if (v !== "" && v !== "0" && v !== "1" && v !== "0.5" && v !== ".5") {
                if (v !== ".") event.target.value = "";
                return;
            }

            if (v === ".") return;

            let num = (v === "") ? null : Number(v);
            
            attendanceData[id] = attendanceData[id] || {};
            let prevCell = attendanceData[id][day];
            let prevCellState = prevCell ? { Val: prevCell.Val, Leave: prevCell.Leave } : { Val: null, Leave: "" };

            attendanceData[id][day] = attendanceData[id][day] || {};
            attendanceData[id][day].Val = num;
            
            isDirty = true;
            if (num !== 0 && attendanceData[id][day].Leave) attendanceData[id][day].Leave = "";

            if (num === 0.5) {
                if (!hasEnoughLeaveBalance(id, day, null, null, 0.5, "")) {
                    showToast("Cannot assign half-day: Leave balance is 0 or insufficient.", "error");
                    if (event && event.target) {
                        event.target.value = prevCellState.Val !== null ? prevCellState.Val : "";
                    }
                    attendanceData[id][day].Val = prevCellState.Val;
                    return;
                }
                reprocessHalfDays(id, day, prevCellState, event);
                return;
            }

            const wasHalfDay = prevCellState.Val === 0.5 || prevCellState.Leave === "Carried" || prevCellState.Leave === "Paired Paid" || prevCellState.Leave === "Paired Unpaid";
            if (wasHalfDay) {
                reprocessHalfDays(id, day, prevCellState, event);
                return;
            }

            calcSat(id);
            updateRowUI(event.target.closest("tr"), id);

            setTimeout(() => {
                if (activePopupEmpId === id) {
                    showLeavePopup(id);
                }

                if (v !== "") {
                    let currentTd = event && event.target ? event.target.closest("td") : null;
                    if (currentTd) {
                        let nextTd = currentTd.nextElementSibling;
                        while (nextTd) {
                            let nextInp = nextTd.querySelector(".att");
                            if (nextInp && !nextInp.readOnly) { nextInp.focus(); nextInp.select(); break; }
                            nextTd = nextTd.nextElementSibling;
                        }
                    }
                }
            }, 0);
        }

        function setValDropdown(id, day, v, event) {
            const y = parseInt(yS.value);
            const m = parseInt(mS.value);
            const emp = employees.find(e => e.MasterId === id) || {};
            const empEngagements = engagements.filter(ee => ee.EmpID === id);
            let state = getCellState(emp, y, m, day, empEngagements);
            if (state.isReadonlyCell) return;

            let num = (v === "") ? null : parseFloat(v);
            
            attendanceData[id] = attendanceData[id] || {};
            let prevCell = attendanceData[id][day];
            let prevCellState = prevCell ? { Val: prevCell.Val, Leave: prevCell.Leave } : { Val: null, Leave: "" };

            attendanceData[id][day] = attendanceData[id][day] || {};
            attendanceData[id][day].Val = num;
            if (num !== 0 && attendanceData[id][day].Leave) attendanceData[id][day].Leave = "";
            
            isDirty = true;
            if (num === 0.5) {
                if (!hasEnoughLeaveBalance(id, day, null, null, 0.5, "")) {
                    showToast("Cannot assign half-day: Leave balance is 0 or insufficient.", "error");
                    if (event && event.target) {
                        event.target.value = prevCellState.Val !== null ? prevCellState.Val : "";
                    }
                    attendanceData[id][day].Val = prevCellState.Val;
                    return;
                }
                reprocessHalfDays(id, day, prevCellState, event);
                return;
            }
            
            const wasHalfDay = prevCellState.Val === 0.5 || prevCellState.Leave === "Carried" || prevCellState.Leave === "Paired Paid" || prevCellState.Leave === "Paired Unpaid";
            if (wasHalfDay) {
                reprocessHalfDays(id, day, prevCellState, event);
                return;
            }

            calcSat(id);
            updateRowUI(event.target.closest("tr"), id);
            setTimeout(() => {
                if (activePopupEmpId === id) {
                    showLeavePopup(id);
                }
            }, 0);
        }

        async function applyHoliday() {
            const daysInput = document.getElementById('holidayInput').value.trim();
            if (!daysInput) return;

            const days = daysInput.split(',').map(Number);
            const y = parseInt(yS.value);
            const m = parseInt(mS.value);

            let dayRemarks = {};
            for (let idx = 0; idx < days.length; idx++) {
                let d = days[idx];
                if (d > 0 && d <= 31) {
                    const result = await Swal.fire({
                        title: `Holiday on Day ${d}`,
                        html: `
                            <div style="text-align: left;">
                                <p style="font-size: 0.95rem; color: #64748b; margin-bottom: 15px;">Please provide the remarks or occasion for the holiday on day ${d}.</p>
                                <div class="form-group mb-3">
                                    <label class="font-weight-bold mb-1" style="font-size: 0.9rem; color: #475569;">Remarks / Occasion *</label>
                                    <input type="text" id="swalHolidayRemark" class="form-control" placeholder="e.g. Independence Day, Christmas" style="font-weight: 600;" />
                                </div>
                            </div>
                        `,
                        showCancelButton: true,
                        confirmButtonText: 'Continue',
                        confirmButtonColor: '#3b82f6',
                        cancelButtonText: 'Cancel',
                        preConfirm: () => {
                            const remark = Swal.getPopup().querySelector('#swalHolidayRemark').value.trim();
                            if (!remark) {
                                Swal.showValidationMessage('Please enter a remark or reason');
                                return false;
                            }
                            return remark;
                        }
                    });

                    if (!result.isConfirmed) {
                        return; // Cancel applies to whole operation
                    }
                    dayRemarks[d] = result.value;
                }
            }

            employees.forEach(emp => {
                const empEngagements = engagements.filter(ee => ee.EmpID === emp.MasterId);
                days.forEach(d => {
                    if(d > 0 && d <= 31) {
                        let state = getCellState(emp, y, m, d, empEngagements);
                        if (!state.isReadonlyCell) {
                            attendanceData[emp.MasterId][d] = { Holiday: true, Val: null, Leave: "", Remarks: dayRemarks[d] || "" };
                            isDirty = true;
                        }
                    }
                });
            });
            render();
        }

        function removeHoliday() {
            const days = document.getElementById('holidayInput').value.split(',').map(Number);
            const y = parseInt(yS.value);
            const m = parseInt(mS.value);
            employees.forEach(emp => {
                const empEngagements = engagements.filter(ee => ee.EmpID === emp.MasterId);
                days.forEach(d => {
                    if (d > 0 && d <= 31 && attendanceData[emp.MasterId]?.[d]?.Holiday) {
                        let state = getCellState(emp, y, m, d, empEngagements);
                        if (!state.isReadonlyCell) {
                            attendanceData[emp.MasterId][d] = { Holiday: false, Val: null, Leave: "", Remarks: "" };
                            isDirty = true;
                        }
                    }
                });
            });
            render();
        }

        function updateGlobalAdjustModalCurrentVal() {
            const adjustCatS = document.getElementById("globalAdjustCatSel");
            const currentLabel = document.getElementById("globalAdjustCurrentVal");
            const inputField = document.getElementById("globalAdjustInput");
            if (!adjustCatS || !currentLabel || !inputField) return;

            const selectedCat = adjustCatS.value;
            const empId = selectedCat === "All" ? "GLOBAL" : "GLOBAL_" + selectedCat;

            let current = 0;
            if (attendanceData[empId] && attendanceData[empId][0]) {
                current = attendanceData[empId][0].Val || 0;
            }

            currentLabel.textContent = (current > 0 ? '+' : '') + current;
            inputField.value = current !== 0 ? current : "";
        }

        function globalAdjust() {
            const modal = document.getElementById("globalAdjustModal");
            const adjustCatS = document.getElementById("globalAdjustCatSel");
            const inputField = document.getElementById("globalAdjustInput");
            const btnApply = document.getElementById("btnGlobalAdjustApply");
            const btnCancel = document.getElementById("btnGlobalAdjustCancel");
            
            if (!modal || !adjustCatS || !inputField) return;

            // Populate categories
            adjustCatS.innerHTML = '<option value="All">All Categories</option>';
            Array.from(cS.options).forEach(opt => {
                if (opt.value !== "All") {
                    adjustCatS.innerHTML += `<option value="${opt.value}">${opt.value}</option>`;
                }
            });
            adjustCatS.value = cS.value;

            updateGlobalAdjustModalCurrentVal();
            
            modal.style.display = "flex";
            modal.offsetHeight; // trigger reflow
            modal.style.opacity = "1";
            modal.querySelector(".confirm-modal-box").style.transform = "scale(1)";
            
            function closeModal() {
                modal.style.opacity = "0";
                modal.querySelector(".confirm-modal-box").style.transform = "scale(0.92)";
                setTimeout(() => {
                    modal.style.display = "none";
                }, 250);
            }
            
            btnCancel.onclick = function() {
                closeModal();
            };
            
            btnApply.onclick = function() {
                const selectedCat = adjustCatS.value;
                const empId = selectedCat === "All" ? "GLOBAL" : "GLOBAL_" + selectedCat;
                const val = inputField.value.trim();
                
                if (val === "") {
                    // Reset adjustment
                    attendanceData[empId] = attendanceData[empId] || {};
                    attendanceData[empId][0] = { Val: 0 };
                    isDirty = true;
                    showPop(`Global Adjustment (${selectedCat}) Reset`);
                    render();
                    closeModal();
                    return;
                }
                
                const num = Number(val);
                if (isNaN(num)) {
                    showPop("Please enter a valid number");
                    return;
                }
                
                attendanceData[empId] = attendanceData[empId] || {};
                attendanceData[empId][0] = { Val: num };
                isDirty = true;
                showPop(`Global Adjustment (${selectedCat}) ${num > 0 ? '+' : ''}${num} Applied`);
                render();
                closeModal();
            };
        }

        function dismissCorrectionBanner() {
            const banner = document.getElementById('correctionRemarkBanner');
            if (!banner || banner.style.display === 'none') return;
            
            banner.style.transition = "transform 0.4s cubic-bezier(0.16, 1, 0.3, 1), opacity 0.3s ease";
            banner.style.transform = "translateX(120%)";
            banner.style.opacity = "0";
            
            setTimeout(() => {
                banner.style.display = 'none';
                banner.style.transform = "";
                banner.style.opacity = "";
            }, 400);

            // Clean up URL parameters without refreshing page
            const url = new URL(window.location);
            url.search = '';
            window.history.replaceState({}, document.title, url.toString());
        }

        setTimeout(initSelectors, 100);
    </script>
</asp:Content>

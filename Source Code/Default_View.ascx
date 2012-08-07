<%@ Control Language="C#" 
AutoEventWireup="true" 
CodeBehind="Default_View.ascx.cs" 
Inherits="AdvisingNotes.Default_View" %>
<%@ Register TagPrefix="common" 
Assembly="Jenzabar.Common" 
Namespace="Jenzabar.Common.Web.UI.Controls" %>
<div class="Jenzabar">
<asp:Label runat="server" ID="lblMsg" />
<asp:Label runat="server" ID="lblAdvisorDropdown" Visible="false"><br />Show only advisees of:</asp:Label>
<asp:DropDownList ID="ddlAdvisors" AutoPostBack="true" runat="server" Visible="false" />

<asp:Label runat="server" ID="lblAdviseeDropdown" Visible="true"><br/>Select advisee:</asp:Label>
<asp:DropDownList ID="ddlAdvisees" runat="server" AutoPostBack="true" Visible="true" />
<br />
<br />
<common:CollapsiblePanel id="pnlInfo" Text="Advisee Information" runat="server" Collapsed="false">
<asp:Panel ID="pnlStaticInfo" runat="server" Visible="false">
<table>
    <tr>
    <td style="padding-right: 30px;" valign="top">
        <table>
            <tr><td valign="top" style="padding-right: 6px;">Name:</td><td valign="top"><asp:Label runat="server" ID="lblIName" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Division:</td><td valign="top"><asp:Label  runat="server" ID="lblIDivision" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Classification:</td><td valign="top"><asp:Label runat="server" ID="lblIClass" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">PT FT Status:</td><td valign="top"><asp:Label runat="server" ID="lblIPTFT" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Last Graded:</td><td valign="top"><asp:Label runat="server" ID="lblILastGrade" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Last Enrolled:</td><td valign="top"><asp:Label runat="server" ID="lblIEnrolled" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Last Trm Hrs:</td><td valign="top"><asp:Label runat="server" ID="lblITrmHrs" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Last Trm GPA:</td><td valign="top"><asp:Label runat="server" ID="lblITrmGPA" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Career Hrs:</td><td valign="top"><asp:Label runat="server" ID="lblIHrs" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Career GPA:</td><td valign="top"><asp:Label runat="server" ID="lblIGPA" /></td></tr>       
        </table>
    </td>
    <td valign="top">
        <table>
            <tr><td valign="top" style="padding-right: 6px;">Degree Code:</td><td valign="top"><asp:Label runat="server" ID="lblIDegree" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Major(s):</td><td valign="top"><asp:Label runat="server" ID="lblIMajors" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Minor(s):</td><td valign="top"><asp:Label runat="server" ID="lblIMinors" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Concentration(s):</td><td valign="top"><asp:Label runat="server" ID="lblIConcs" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Academic Standing:</td><td valign="top"><asp:Label runat="server" ID="lblIProbation" /></td></tr>
            <tr><td valign="top" style="padding-right: 6px;">Academic Honors:</td><td valign="top"><asp:Label runat="server" ID="lblIHonors" /></td></tr>
        </table>
    </td>
    </tr>
</table>
</asp:Panel>
<asp:Label id="lblCurrentCourses" runat="server" Visible="false" Text="Courses Currently Enrolled" Font-Bold="true"/>
    <common:GroupedGrid RenderGroupHeaders="true" ID="grdCourses" runat="server">
        <EmptyTableTemplate>This account is not currently enrolled in any courses.</EmptyTableTemplate>
        <Columns>
            <asp:TemplateColumn HeaderText = "Term"> 
                <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "YR_CDE")%> : <%# DataBinder.Eval(Container.DataItem, "TRM_CDE")%>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText = "Course"> 
                <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "CRS_CDE")%> : <%# DataBinder.Eval(Container.DataItem, "CRS_TITLE")%>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn DataField="credit_hrs" HeaderText="Credits" />
            <asp:BoundColumn DataField="original_reg_dte" HeaderText="Dte Reg" />
            <asp:BoundColumn DataField="grade" HeaderText="Grade" />
        </Columns>
    </common:GroupedGrid>
</common:CollapsiblePanel>

<common:CollapsiblePanel ID="pnlNotes" Text="Advisee Notes" runat="server" Collapsed="false">
    <common:GroupedGrid 
    OnItemCommand="grdNotes_ItemCommand" 
    OnItemDataBound="grdNotes_ItemDataBound" 
    AutoGenerateColumns="false"
    RenderGroupHeaders="true" 
     ID="grdNotes" runat="server">
        <EmptyTableTemplate>There are no active advising notes entered for this account.</EmptyTableTemplate>
        <Columns>
            <asp:BoundColumn DataField="ID_NUMBER" Visible="false" />
            <asp:BoundColumn DataField="GROUP_NUMBER" Visible="false" />
            <asp:BoundColumn DataField="SUBGROUP_NUMBER" Visible="false" />
            <asp:BoundColumn DataField="GROUP_SEQUENCE" Visible="false" />
            <asp:BoundColumn DataField="TO_DO_ID_NUMBER" Visible="false" />
            <asp:TemplateColumn ItemStyle-Width="80px" ItemStyle-VerticalAlign="Top" HeaderText="Date">
                <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "Item_Date", "{0:d}")%><br />
                    <%# DataBinder.Eval(Container.DataItem, "Item_Time", "{0:t}")%>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn ItemStyle-VerticalAlign="Top" DataField="ITEM_DESCRIPTION" HeaderText="Text" />
            <asp:TemplateColumn ItemStyle-Width="80px" ItemStyle-VerticalAlign="Top" HeaderText="Start">
                <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "Start_Date", "{0:d}")%><br />
                    <%# DataBinder.Eval(Container.DataItem, "Start_Time", "{0:t}")%>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn ItemStyle-Width="80px" ItemStyle-VerticalAlign="Top" HeaderText="End">
                <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "End_Date", "{0:d}")%><br />
                    <%# DataBinder.Eval(Container.DataItem, "End_Time", "{0:t}")%>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn ItemStyle-VerticalAlign="Top" HeaderText = "Assigned">
                <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "TO_DO_USER_ID")%>
                    <%# strCompletion(
                        ((string)(((System.Data.DataRowView)Container.DataItem)["COMPLETION_CODE"])),
                        ((DateTime)(((System.Data.DataRowView)Container.DataItem)["COMPLETION_DTE"]))
                            )%>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn ItemStyle-Width="86px" ItemStyle-VerticalAlign="Top" HeaderText = "Last Modified">
                <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "JOB_TIME", "{0:d}")%><br />
                    by <%# DataBinder.Eval(Container.DataItem, "USER_NAME")%>
                </ItemTemplate>
            </asp:TemplateColumn>
        </Columns>
    </common:GroupedGrid>
    
    <asp:Panel ID="pnlAddNote" Visible="false" runat="server">
        <table style="border:solid 1px black; padding: 3px;" cellspacing="0">
        <tr>
            <td valign="top" style="padding-left: 3px; padding-top: 3px;" colspan="2"><asp:label ID="lblAddNewNote" Font-Bold="true" Runat="server" Visible="false" Text="Add new note:" /></td>
        </tr>
        <tr>
            <td valign="top" style="padding-left: 3px;"colspan="2"><asp:TextBox ID="txtNoteDesc" Width="400px" Height="80px" TextMode="MultiLine" Runat="server" Visible="false" /></td>
        </tr>
        <tr>
            <td style="padding-left: 3px;" colspan="2" valign="bottom">Item Date</td>
        </tr>
        <tr>
            <td valign="top"><asp:TextBox ID="txtItemDate" Width="80px" runat="server" Visible="false" /></td>
        </tr>
        <tr>
            <td style="padding-left: 3px;" valign="bottom">Start Date</td>
            <td valign="bottom">Due Date</td>
        </tr>
        <tr>
            <td valign="top"><asp:TextBox ID="txtStartDate" Width="80px" Runat="server" Visible="false" /></td>
            <td valign="top"><asp:TextBox ID="txtEndDate" Width="80px" Runat="server" Visible="false" /></td>
        </tr>
        <tr>
            <td style="padding-left: 3px;" valign="bottom">Assigned to:</td>
            <td valign="bottom">Show on Web?</td>
        </tr>
        <tr>
            <td valign="top" style="padding-left: 3px;"><asp:DropDownList ID="ddlNotepadUsers" Runat="server" Visible="false" /></td>
            <td valign="top"><asp:CheckBox ID="cbShowOnWeb" Runat="server" Visible="false" /></td>
        </tr>
        <tr>
            <td style="padding-left: 3px;" valign="bottom">Is Completed?</td>
            <td valign="bottom">Date Completed:</td>
        </tr>
        <tr>
            <td style="padding-left: 3px;" valign="top"><asp:CheckBox ID="cbCompleted" Runat="server" Visible="false" /></td>
            <td valign="top"><asp:TextBox ID="txtCompletedDate" Width="80px" Runat="server" Visible="false" /></td>
        </tr>
        <tr><td align="center" valign="bottom" colspan="2"><asp:Button ID="btnAddNote" Text="Add Note" runat="server" Visible="false"/></td></tr>
        </table>
        <asp:Label runat="server" id="lblGroupNum" Visible="false" />
        <asp:Label runat="server"  id="lblSubgroupNum" Visible="false" />
        <asp:Label runat="server"  id="lblSequenceNum" Visible="false" />
        <asp:Label runat="server"  id="lblIDNumber" Visible="false" />
    </asp:Panel>
    
</common:CollapsiblePanel>
</div>



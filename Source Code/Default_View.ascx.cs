#region Using statements
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Data.Sql;
using System.Data.SqlClient;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Web;
using Jenzabar.Portal.Framework.Web.UI;
using Jenzabar.Portal.Framework.Web.Common;
using Jenzabar.Portal.Framework.Configuration;
using Jenzabar.Framework.Authentication;
using Jenzabar.Common.Web.UI.Controls;
using DataUtils;
#endregion

namespace AdvisingNotes
{
    public partial class Default_View : PortletViewBase
    {
        //To move this to production, uncomment the line containing TmsEPrd and comment out the Play DB.
        //protected static string strDatabase = "TmsEPly";
        //protected static string strDatabase = "TmsEPrd";
        
        protected static string strRegex = @"(?!<(?:b|/b|i|/i|u|/u|a href=.*?|/a)>)<.*?>";
        
        //You'll want to customize this data below.
        //protected static string strServer = @"YOURSERVERNAME";
        //protected static string strUser = "JICSAdvisingNotes";
        //protected static string strPassword = @"ACCOUNTPASSWORD";
        protected static string strAdvisingModuleCode = "AV";

        protected string strERPConn = System.Configuration.ConfigurationManager.ConnectionStrings["JenzabarConnectionString"].ConnectionString;

        #region Define controls
        protected System.Web.UI.WebControls.Label lblMsg;
        protected System.Web.UI.WebControls.Label lblAdvisorDropdown;
        protected System.Web.UI.WebControls.Label lblAddNewNote;
        protected Jenzabar.Common.Web.UI.Controls.GroupedGrid grdAdviseesforAdvisor;
        protected Jenzabar.Common.Web.UI.Controls.GroupedGrid grdCourses;
        protected Jenzabar.Common.Web.UI.Controls.GroupedGrid grdNotes;
        protected Jenzabar.Common.Web.UI.Controls.CollapsiblePanel pnlNotes;
        protected Jenzabar.Common.Web.UI.Controls.CollapsiblePanel pnlInfo;
        protected System.Web.UI.WebControls.DropDownList ddlNotepadUsers;
        protected System.Web.UI.WebControls.DropDownList ddlAdvisors;
        protected System.Web.UI.WebControls.DropDownList ddlAdvisees;
        protected System.Web.UI.WebControls.Button btnAddNote;
        protected System.Web.UI.WebControls.TextBox txtNoteDesc;
        protected System.Web.UI.WebControls.TextBox txtStartDate;
        protected System.Web.UI.WebControls.TextBox txtEndDate;
        protected System.Web.UI.WebControls.TextBox txtItemDate;
        protected System.Web.UI.WebControls.TextBox txtCompletedDate;
        protected System.Web.UI.WebControls.CheckBox cbShowOnWeb;
        protected System.Web.UI.WebControls.CheckBox cbCompleted;
        protected System.Web.UI.WebControls.Label lblGroupNum;
        protected System.Web.UI.WebControls.Label lblSubgroupNum;
        protected System.Web.UI.WebControls.Label lblSequenceNum;
        protected System.Web.UI.WebControls.Label lblIDNumber;
        protected System.Web.UI.WebControls.Panel pnlAddNote;
        protected System.Web.UI.WebControls.Label lblCurrentCourses;
        protected System.Web.UI.WebControls.Label lblIName;
        protected System.Web.UI.WebControls.Label lblIDivision;
        protected System.Web.UI.WebControls.Label lblIClass; 
        protected System.Web.UI.WebControls.Label lblIPTFT;
        protected System.Web.UI.WebControls.Label lblIEnrolled;
        protected System.Web.UI.WebControls.Label lblIHrs;
        protected System.Web.UI.WebControls.Label lblIGPA;
        protected System.Web.UI.WebControls.Label lblITrmHrs;
        protected System.Web.UI.WebControls.Label lblILastGrade;
        protected System.Web.UI.WebControls.Label lblITrmGPA;
        protected System.Web.UI.WebControls.Label lblIHonors;
        protected System.Web.UI.WebControls.Label lblIProbation;
        protected System.Web.UI.WebControls.Label lblIDegree;
        protected System.Web.UI.WebControls.Label lblIMajors;
        protected System.Web.UI.WebControls.Label lblIMinors;
        protected System.Web.UI.WebControls.Label lblIConcs;
        protected System.Web.UI.WebControls.Panel pnlStaticInfo;
        protected System.Web.UI.WebControls.Label lblAdviseeDropdown;
        #endregion

        #region Page Load methods
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session.Remove("Notes");
                Session.Remove("Advisee");
                OnFirstLoad();
            }
            else
            {
                DataTable dtNotes = new DataTable();
                try
                {
                    dtNotes = (DataTable)Session["Notes"];
                    if(dtNotes.Rows.Count > 0)
                    {
                        grdNotes.DataSource = dtNotes;
                        grdNotes.DataBind();
                    }
                }
                catch
                {
                }
            }

            OnInit();

        }
        protected void OnFirstLoad()
        {
            lblMsg.Text = "";
            SecurityCheck();
            pnlInfo.Visible = false;
            pnlNotes.Visible = false;
            FillAllDropdowns();
            if ((Session["AdminLvl"].ToString() == "ViewAll") || (Session["AdminLvl"].ToString() == "EditAll")
                || (Session["AdminLvl"].ToString() == "Admin"))
            {
                ddlAdvisors.SelectedIndexChanged += new EventHandler(ddlAdvisors_SelectedIndexChanged);
                ddlAdvisees.SelectedIndexChanged += new EventHandler(ddlAdvisees_SelectedIndexChanged);
            }
            else if (!IsAdvisor())
            {
                ddlAdvisees.Visible = false;
                PopulateGridsForStudent(Int32.Parse(PortalUser.Current.HostID));
            }
        }
        protected void OnInit()
        {
            if (ddlAdvisors.Visible == true)
            {
                ddlAdvisors.SelectedIndexChanged += new EventHandler(ddlAdvisors_SelectedIndexChanged);
            }
            if (ddlAdvisees.Visible == true)
            {
                ddlAdvisees.SelectedIndexChanged += new EventHandler(ddlAdvisees_SelectedIndexChanged);
            }
            if (ddlAdvisees.Visible == true)
            {
                btnAddNote.Click += new EventHandler(btnAddNote_Click);
            }
        }
        #endregion

        #region Security methods
        protected void SecurityCheck()
        {
            this.Session["AdminLvl"] = "None";
            pnlAddNote.Visible = false;
            lblAdvisorDropdown.Visible = false;
            lblAdviseeDropdown.Visible = false;
            lblCurrentCourses.Visible = false;
            ddlAdvisees.Visible = false;
            ddlAdvisors.Visible = false;

            if (IsAdvisor())
            {
                this.Session["AdminLvl"] = "Self";
                ddlAdvisees.Visible = true;
                lblAdviseeDropdown.Visible = true;
            }
            if(this.ParentPortlet.AccessCheck("ViewAll"))
            {
                this.Session["AdminLvl"] = "ViewAll";
                ddlAdvisors.Visible = true;
                lblAdvisorDropdown.Visible = true;
                lblAdviseeDropdown.Visible = true;
                ddlAdvisees.Visible = true;
            }
            if (this.ParentPortlet.AccessCheck("EditAll"))
            {
                this.Session["AdminLvl"] = "EditAll";
                ddlAdvisors.Visible = true;
                lblAdvisorDropdown.Visible = true;
                lblAdviseeDropdown.Visible = true;
                ddlAdvisees.Visible = true;
            }
            if (this.ParentPortlet.AccessCheck("CanAdminPortlet"))
            {
                this.Session["AdminLvl"] = "Admin";
                ddlAdvisors.Visible = true;
                lblAdvisorDropdown.Visible = true;
                lblAdviseeDropdown.Visible = true;
                ddlAdvisees.Visible = true;
            }
        }
        protected void ShowAddNoteFunctionality()
        {
            pnlAddNote.Visible = true;
            
            txtNoteDesc.Visible = true;
            txtStartDate.Visible = true;
            txtEndDate.Visible = true;
            txtItemDate.Visible = true;
            txtCompletedDate.Visible = true;
            cbShowOnWeb.Visible = true;
            cbCompleted.Visible = true;
            ddlNotepadUsers.Visible = true;
            lblAddNewNote.Visible = true;
            if (ddlNotepadUsers.Items.Count > 0)
            {
            }
            else
            {
                bool IsSuccess = true;
                SqlConnection sqlConn = new SqlConnection(strERPConn);
                try
                {
                    sqlConn.Open();
                    FillNotepadUserDropdown(sqlConn);
                }
                catch (Exception ex)
                {
                    IsSuccess = false;
                    ParentPortlet.ShowFeedback("An error occurred while attempting to retrieve a list of Notepad users from the database. Error: " + ex.ToString());
                }
                finally
                {
                    try
                    {
                        sqlConn.Close();
                    }
                    catch
                    {
                    }
                }
                if (IsSuccess)
                {
                    btnAddNote.Visible = true;
                }
            }
        }
        #endregion

        #region Methods that fill controls
        protected void FillAdviseeInfo(DataTable dt)
        {
            #region Define strings
            string strName = "";
            string strDiv = "";
            string strClass = "";
            string strPTFT = "";
            string strEnrolled = "";
            string strHrs = "";
            string strGPA = "";
            string strTrmHrs = "";
            string strLastGraded = "";
            string strTrmGPA = "";
            string strHonors = "";
            string strStanding = "";
            string strDegree = "";
            string strMajors = "";
            string strMinors = "";
            string strConcentrations = "";
            #endregion

            pnlInfo.Visible = true;
            pnlInfo.Collapsed = true;
            pnlStaticInfo.Visible = true;
            lblCurrentCourses.Visible = true;

            foreach (DataRow dr in dt.Rows)
            {
                #region Assign values to strings
                strName = dr[1].ToString().Trim();
                strDiv = dr[2].ToString().Trim();
                strClass = dr[3].ToString().Trim();
                strPTFT = dr[4].ToString().Trim();
                strEnrolled = dr[7].ToString().Trim();
                strHrs = dr[15].ToString().Trim();
                strGPA = dr[16].ToString().Trim();
                if (dr[18].ToString().Trim() == "0")
                {
                    strTrmHrs = "None";
                }
                else
                {
                    strTrmHrs = dr[17].ToString().Trim() + " (" + dr[18].ToString().Trim() + " course";
                    if (dr[18].ToString().Trim() != "1")
                    {
                        strTrmHrs = strTrmHrs + "s";
                    }
                    strTrmHrs = strTrmHrs + ".)";
                }

                strLastGraded = dr[20].ToString().Trim();
                strTrmGPA = dr[19].ToString().Trim();
                strHonors = dr[5].ToString().Trim();
                strStanding = dr[6].ToString().Trim();
                strDegree = dr[8].ToString().Trim();
                if (dr[10].ToString().Trim().Length > 0)
                {
                    if (dr[9].ToString().Trim().Length > 0)
                    {
                        strMajors = dr[9].ToString().Trim() + @"<br/>" + dr[10].ToString().Trim();
                    }
                    else
                    {
                        strMajors = dr[10].ToString().Trim();
                    }
                }
                else
                {
                    strMajors = dr[9].ToString().Trim();
                }
                if (dr[12].ToString().Trim().Length > 0)
                {
                    if (dr[11].ToString().Trim().Length > 0)
                    {
                        strMinors = dr[11].ToString().Trim() + @"<br/>" + dr[12].ToString().Trim();
                    }
                    else
                    {
                        strMinors = dr[12].ToString().Trim();
                    }
                }
                else
                {
                    strMinors = dr[11].ToString().Trim();
                }
                if (dr[14].ToString().Trim().Length > 0)
                {
                    if (dr[13].ToString().Trim().Length > 0)
                    {
                        strConcentrations = dr[13].ToString().Trim() + @"<br/>" + dr[14].ToString().Trim();
                    }
                    else
                    {
                        strConcentrations = dr[14].ToString().Trim();
                    }
                }
                else
                {
                    strConcentrations = dr[13].ToString().Trim();
                }
                #endregion
            }
            dt.Dispose();

            #region Assign strings to labels
            lblIName.Text = strName;
            lblIDivision.Text = strDiv;
            lblIClass.Text = strClass;
            lblIPTFT.Text = strPTFT;
            lblIEnrolled.Text = strEnrolled;
            lblIHrs.Text = strHrs;
            lblIGPA.Text = strGPA;
            lblITrmHrs.Text = strTrmHrs;
            lblILastGrade.Text = strLastGraded;
            lblITrmGPA.Text = strTrmGPA;
            lblIHonors.Text = strHonors;
            lblIProbation.Text = strStanding;
            lblIDegree.Text = strDegree;
            lblIMajors.Text = strMajors;
            lblIMinors.Text = strMinors;
            lblIConcs.Text = strConcentrations;
            #endregion

            string strAdminLvl = this.Session["AdminLvl"].ToString();

            if ((strAdminLvl == "ViewAll") || (strAdminLvl == "EditAll") || (strAdminLvl == "Admin"))
            {
                ShowAddNoteFunctionality();
            }
        }
        protected void FillAdviseeNotes(int ID_NUM)
        {
            Session.Remove("Notes");
            bool IsSuccess = true;
            SqlConnection sqlConn = new SqlConnection(strERPConn);
            DataTable _dtNotes = new DataTable();

            try
            {
                sqlConn.Open();
                _dtNotes = dtAdviseeNotes(ID_NUM, sqlConn);

            }
            catch (Exception ex)
            {
                IsSuccess = false;
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }
            if (IsSuccess)
            {
                grdNotes.DataSource = _dtNotes;
                grdNotes.DataBind();
                pnlNotes.Visible = true;
                Session["Notes"] = _dtNotes;
            }
        }
        protected void ResetAddNote()
        {
            lblAddNewNote.Text = "Add new note:";
            btnAddNote.Text = "Add Note";
            lblGroupNum.Text = "";
            lblSubgroupNum.Text = "";
            lblSequenceNum.Text = "";
            txtNoteDesc.Text = "";
            txtStartDate.Text = "";
            txtEndDate.Text = "";
            txtItemDate.Text = "";
            txtCompletedDate.Text = "";
            cbCompleted.Checked = false;
            cbShowOnWeb.Checked = false;
        }
        protected void AddEditColumns()
        {
            Jenzabar.Common.Web.UI.Controls.ImageCommandColumn colEdit = new ImageCommandColumn();
            Jenzabar.Common.Web.UI.Controls.ImageCommandColumn colDel = new ImageCommandColumn();
            colEdit.ImageUrl = @"~/ui/common/images/PortletImages/Icons/edit.gif";
            colDel.ImageUrl = @"~/ui/common/images/PortletImages/Icons/delete.gif";
            colEdit.HeaderText = "Edit";
            colDel.HeaderText = "Del";
            colEdit.ToolTip = "Edit this note.";
            colDel.ToolTip = "Set this note inactive.";
            colEdit.ItemStyle.Width = Unit.Pixel(35);
            colDel.ItemStyle.Width = Unit.Pixel(35);
            colDel.ClickConfirmMessage = "Are you sure you want to set this note inactive? It will no longer appear in this list once deactivated, but will not be deleted.";
            colEdit.CommandName = "Edit";
            colDel.CommandName = "Del";
            grdNotes.Columns.Add(colEdit);
            grdNotes.Columns.Add(colDel);
            grdNotes.ItemCommand += new DataGridCommandEventHandler(grdNotes_ItemCommand);
        }
        protected void PopulateGridsForStudent(int ID_NUM)
        {
            pnlAddNote.Visible = false;
            bool IsSuccess = true;
            SqlConnection sqlConn = new SqlConnection(strERPConn);
            DataTable _dtCourses = new DataTable();
            DataTable _dtNotes = new DataTable();
            DataTable _dtAdviseeInfo = new DataTable();

            try
            {
                sqlConn.Open();
                _dtCourses = dtAdviseeCourses(ID_NUM, sqlConn);
                _dtNotes = dtAdviseeNotes(ID_NUM, sqlConn);
                _dtAdviseeInfo = dtAdviseeInfo(ID_NUM, sqlConn);

            }
            catch (Exception ex)
            {
                IsSuccess = false;
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }
            if (IsSuccess)
            {
                FillAdviseeInfo(_dtAdviseeInfo);
                grdNotes.DataSource = _dtNotes;
                grdCourses.DataSource = _dtCourses;
                grdNotes.DataBind();
                grdCourses.DataBind();
                pnlNotes.Visible = true;
                pnlInfo.Visible = true;
                pnlStaticInfo.Visible = true;
                grdCourses.Visible = true;
                grdNotes.Visible = true;
                pnlInfo.Collapsed = false;
                lblAdviseeDropdown.Visible = false;
            }
        }
        #region Methods that fill dropdownlists
        protected void FillAllDropdowns()
        {
            bool IsSuccess = true;
            SqlConnection sqlConn = new SqlConnection(strERPConn);
            try
            {
                sqlConn.Open();
            }
            catch (Exception ex2)
            {
                IsSuccess = false;
                lblMsg.Text = "An error occurred while trying to access the database. Please contact your system administrator. "
                        + "Error Text: " + ex2.Message;
            }
            if (IsSuccess)
            {
                try
                {
                    //FillNotepadUserDropdown(sqlConn);
                    FillAdvisorDropdown(sqlConn);
                }
                catch (Exception ex)
                {
                    lblMsg.Text = "An error occurred while trying to populate the user dropdowns. Please contact your system administrator. "
                        + "Error Text: " + ex.ToString();
                    IsSuccess = false;
                }
                finally
                {
                    try
                    {
                        sqlConn.Close();
                    }
                    catch
                    {
                    }
                }
            }

            if (IsSuccess)
            {
                FillAdviseeDropdown();
            }
        }
        protected void FillNotepadUserDropdown(SqlConnection sqlConn)
        {
            try
            {
                ddlNotepadUsers.Items.Clear();
            }
            catch
            {
            }
            DataTable _dtNotepadUsers = new DataTable();
            _dtNotepadUsers = dtNotepadUsers(sqlConn);
            if (_dtNotepadUsers.Rows.Count > 1)
            {
                foreach (DataRow dr in _dtNotepadUsers.Rows)
                {
                    ListItem li = new ListItem(dr[0].ToString().Trim().ToUpper(), dr[1].ToString().Trim());
                    ddlNotepadUsers.Items.Add(li);
                }
            }
            ListItem liNA = new ListItem(@"N/A", @"N/A");
            ddlNotepadUsers.Items.Insert(0, liNA);
            ddlNotepadUsers.SelectedIndex = 0;
        }
        protected void FillAdvisorDropdown(SqlConnection sqlConn)
        {
            Session.Remove("Notes");
            try
            {
                ddlAdvisors.Items.Clear();
            }
            catch
            {
            }
            DataTable _dtAdvisors = new DataTable();
            _dtAdvisors = dtAdvisors(sqlConn);
            if (_dtAdvisors.Rows.Count > 1)
            {
                foreach (DataRow dr in _dtAdvisors.Rows)
                {
                    ListItem li = new ListItem(dr[1].ToString().Trim(), dr[0].ToString().Trim());
                    ddlAdvisors.Items.Add(li);
                }
            }
            ListItem liNA = new ListItem("Show all advisors", "None selected");
            ddlAdvisors.Items.Insert(0, liNA);
            ddlAdvisors.SelectedIndex = 0;
        }
        protected void FillAdviseeDropdown()
        {
            Session.Remove("Notes");
            string strAdminLvl = Session["AdminLvl"].ToString();
            ddlAdvisees.Items.Clear();
            DataTable _dtAdvisees = new DataTable();
            if (
                (ddlAdvisors.SelectedIndex == 0) &&
                ((strAdminLvl == "EditAll") ||
                 (strAdminLvl == "ViewAll") ||
                 (strAdminLvl == "Admin"))
                )
            {
                _dtAdvisees = dtAllAdvisees();
                foreach (DataRow dr in _dtAdvisees.Rows)
                {
                    ddlAdvisees.Items.Add(new ListItem(dr[1].ToString().Trim() + "  (Advisor: " + dr[3].ToString().Trim() + ")",
                        dr[0].ToString().Trim()));
                }
                ddlAdvisees.Items.Insert(0, new ListItem("None selected", "None selected"));
                ddlAdvisees.SelectedIndex = 0;
            }
            else
            {
                int iUserID = 0;
                if (strAdminLvl == "Self")
                {
                    iUserID = Int32.Parse(PortalUser.Current.HostID);
                }
                else
                {
                    if ((Session["AdminLvl"].ToString() == "EditAll") ||
                        (Session["AdminLvl"].ToString() == "ViewAll") ||
                        (Session["AdminLvl"].ToString() == "Admin"))
                    {
                        iUserID = Int32.Parse(ddlAdvisors.SelectedValue);
                    }
                }
                _dtAdvisees = dtAdviseesByAdvisor(iUserID);


                foreach (DataRow dr in _dtAdvisees.Rows)
                {
                    ddlAdvisees.Items.Add(new ListItem(dr[1].ToString().Trim() + ", " + dr[2].ToString().Trim(), dr[0].ToString().Trim()));
                }
                ddlAdvisees.Items.Insert(0, new ListItem("None selected", "None selected"));
                ddlAdvisees.SelectedIndex = 0;
            }
        }
        #endregion
        #endregion



        #region Data access bools (actually do data editing)
        protected bool AddNote(int ID_NUM, string strDesc, DateTime dtStart, DateTime dtEnd, DateTime dtCompleted, 
            int TODO_IDNUM, string strToDoUser, DateTime dtItem, string strModuleCode, 
            bool ShowOnWeb, bool IsCompleted, bool HasStartDate, bool HasEndDate)
        {
            bool IsSuccess = true;
            string strQuery = "insert into items "
                + "(ID_NUMBER, GROUP_NUMBER, SUBGROUP_NUMBER, GROUP_SEQUENCE, "
                + "ITEM_TYPE, ITEM_DESCRIPTION, START_DATE, START_TIME, END_DATE, END_TIME, "
                + "TO_DO_ID_NUMBER, TO_DO_USER_ID, ACTIVE_INACTIVE, COMPLETION_CODE, COMPLETION_DTE, "
                + "ITEM_DATE, ITEM_TIME, "
                + "MODULE_CODE, DISPLAY_ON_WEB, "
                + "USER_NAME, JOB_NAME, JOB_TIME) "
                + "VALUES(@ID_NUM, [dbo].[CUS_fn_GetNextNotepadGroupNumber] (@ID_NUM), 1, 1, "
                + "'NOTE', @ITEM_DESC, @START_DATE, @START_TIME, @END_DATE, @END_TIME, "
                + "@TODO_IDNUM, @TODO_USER, 'A', @COMPLETION_CDE, @COMPLETION_DATE, "
                + "@ITEM_DATE, @ITEM_TIME, "
                + "@MODULE_CODE, @SHOW_ON_WEB, "
                + "@USERNAME, 'J:NewAdvisingNote', GETDATE())";

            string strPortalUserID = "J:" + PortalUser.Current.HostID.ToString();
            string strShowWeb = "N";
            string strCompletionCode = "P";

            if (ShowOnWeb)
            {
                strShowWeb = "Y";
            }
            if (IsCompleted)
            {
                strCompletionCode = "C";
            }

            SqlConnection sqlConn = new SqlConnection(strERPConn);
            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);

            #region Define parameters
            sqlCmd.Parameters.Add("@ID_NUM", SqlDbType.Int);
            sqlCmd.Parameters.Add("@ITEM_DESC", SqlDbType.Text);
            sqlCmd.Parameters.Add("@MODULE_CODE", SqlDbType.Char, 2);
            sqlCmd.Parameters.Add("@START_DATE", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@END_DATE", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@ITEM_DATE", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@START_TIME", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@END_TIME", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@ITEM_TIME", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@TODO_USER", SqlDbType.Char, 15);
            sqlCmd.Parameters.Add("@TODO_IDNUM", SqlDbType.Int);
            sqlCmd.Parameters.Add("@COMPLETION_CDE", SqlDbType.Char, 1);
            sqlCmd.Parameters.Add("@COMPLETION_DATE", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@SHOW_ON_WEB", SqlDbType.Char, 1);
            sqlCmd.Parameters.Add("@USERNAME", SqlDbType.Char, 15);
            sqlCmd.Parameters["@ID_NUM"].Value = ID_NUM;
            sqlCmd.Parameters["@ITEM_DATE"].Value = DateTime.Parse(dtItem.ToShortDateString());
            sqlCmd.Parameters["@ITEM_TIME"].Value = DateTime.Parse(dtItem.ToShortTimeString());
            sqlCmd.Parameters["@ITEM_DESC"].Value = Server.HtmlEncode(strDesc);
            sqlCmd.Parameters["@MODULE_CODE"].Value = strModuleCode;
            sqlCmd.Parameters["@SHOW_ON_WEB"].Value = strShowWeb;
            sqlCmd.Parameters["@USERNAME"].Value = strPortalUserID;
            sqlCmd.Parameters["@COMPLETION_CDE"].Value = strCompletionCode;
            
            #endregion

            
            #region Set possible null values
            if (HasStartDate)
            {
                sqlCmd.Parameters["@START_DATE"].Value = DateTime.Parse(dtStart.ToShortDateString());
                sqlCmd.Parameters["@START_TIME"].Value = DateTime.Parse(dtStart.ToShortTimeString());
            }
            else
            {
                sqlCmd.Parameters["@START_DATE"].Value = DBNull.Value;
                sqlCmd.Parameters["@START_TIME"].Value = DBNull.Value;
            }
            if (HasEndDate)
            {
                sqlCmd.Parameters["@END_DATE"].Value = DateTime.Parse(dtEnd.ToShortDateString());
                sqlCmd.Parameters["@END_TIME"].Value = DateTime.Parse(dtEnd.ToShortTimeString());
            }
            else
            {
                sqlCmd.Parameters["@END_DATE"].Value = DBNull.Value;
                sqlCmd.Parameters["@END_TIME"].Value = DBNull.Value;
            }
            if (strCompletionCode == "C")
            {
                sqlCmd.Parameters["@COMPLETION_CDE"].Value = strCompletionCode;
                sqlCmd.Parameters["@COMPLETION_DTE"].Value = dtCompleted;
            }
            else
            {
                sqlCmd.Parameters["@COMPLETION_CDE"].Value = "P";
                sqlCmd.Parameters["@COMPLETION_DATE"].Value = DBNull.Value;
            }
            if (strToDoUser == @"N/A")
            {
                sqlCmd.Parameters["@TODO_IDNUM"].Value = DBNull.Value;
                sqlCmd.Parameters["@TODO_USER"].Value = DBNull.Value;
            }
            else
            {
                sqlCmd.Parameters["@TODO_IDNUM"].Value = TODO_IDNUM;
                sqlCmd.Parameters["@TODO_USER"].Value = strToDoUser;
            }
            #endregion

            int iCount = 0;
            try
            {
                sqlConn.Open();
                iCount = sqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                ParentPortlet.ShowFeedback(FeedbackType.Error, "An error occurred while trying to add a note. Error: " + ex.ToString());   
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }

            if (iCount != 1)
            {
                IsSuccess = false;
            }

            return IsSuccess;
        }

        protected bool EditNote(int ID_NUM, int GROUP_NUM, int SUBGROUP_NUM, int SEQ_NUM, string strDesc, DateTime dtStart, 
            DateTime dtEnd, DateTime dtItem, DateTime dtCompleted, int TODO_IDNUM, string strToDoUser, 
            string strModuleCode, bool ShowOnWeb, string strCompletionCode, bool HasStartDate, bool HasEndDate, bool HasItemDate)
        {
            bool IsSuccess = true;

            string strQuery = "UPDATE ITEMS "
                + "SET ITEM_DESCRIPTION = @DESC, START_DATE = @START_DATE, END_DATE = @END_DATE, START_TIME = @START_TIME, END_TIME = @END_TIME, "
                + "ITEM_DATE = @ITEM_DATE, ITEM_TIME = @ITEM_TIME, TO_DO_ID_NUMBER = @TODO_IDNUM, TO_DO_USER_ID = @TODOUSER, "
                + "DISPLAY_ON_WEB = @SHOW_WEB, MODULE_CODE = @MODULE_CODE, COMPLETION_CODE = @COMPLETION_CDE, COMPLETION_DTE = @COMPLETION_DTE, "
                + "USER_NAME = @USERNAME, JOB_NAME = 'J:EditAVNote', JOB_TIME = GETDATE() "
                + "WHERE ID_NUMBER = @ID_NUM AND "
                + "GROUP_NUMBER = @GROUP_NUMBER AND SUBGROUP_NUMBER = @SUBGROUP_NUMBER AND GROUP_SEQUENCE = @GROUP_SEQUENCE";

            string strPortalUserID = "J:" + PortalUser.Current.HostID.ToString();
            string strShowWeb = "N";
            if (ShowOnWeb)
            {
                strShowWeb = "Y";
            }
            
            SqlConnection sqlConn = new SqlConnection(strERPConn);
            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);

            sqlCmd.Parameters.Add("@ID_NUM", SqlDbType.Int);
            sqlCmd.Parameters.Add("@DESC", SqlDbType.Text);
            sqlCmd.Parameters.Add("@MODULE_CODE", SqlDbType.Char,2);
            sqlCmd.Parameters.Add("@START_DATE", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@END_DATE", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@ITEM_DATE", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@START_TIME", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@END_TIME", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@ITEM_TIME", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@USERNAME", SqlDbType.Char, 15);
            sqlCmd.Parameters.Add("@TODOUSER", SqlDbType.Char, 15);
            sqlCmd.Parameters.Add("@TODO_IDNUM", SqlDbType.Int);
            sqlCmd.Parameters.Add("@COMPLETION_CDE", SqlDbType.Char,1);
            sqlCmd.Parameters.Add("@COMPLETION_DTE", SqlDbType.DateTime);
            sqlCmd.Parameters.Add("@GROUP_NUMBER", SqlDbType.Int);
            sqlCmd.Parameters.Add("@SUBGROUP_NUMBER", SqlDbType.Int);
            sqlCmd.Parameters.Add("@GROUP_SEQUENCE", SqlDbType.Int);
            sqlCmd.Parameters.Add("@SHOW_WEB", SqlDbType.Char,1);

            sqlCmd.Parameters["@ID_NUM"].Value = ID_NUM;
            sqlCmd.Parameters["@DESC"].Value = strDesc;
            sqlCmd.Parameters["@MODULE_CODE"].Value = strModuleCode;
            sqlCmd.Parameters["@USERNAME"].Value = strPortalUserID;
            sqlCmd.Parameters["@GROUP_NUMBER"].Value = GROUP_NUM;
            sqlCmd.Parameters["@SUBGROUP_NUMBER"].Value = SUBGROUP_NUM;
            sqlCmd.Parameters["@GROUP_SEQUENCE"].Value = SEQ_NUM;
            sqlCmd.Parameters["@SHOW_WEB"].Value = strShowWeb;

            #region Set possible null values
            if (HasStartDate)
            {
                sqlCmd.Parameters["@START_DATE"].Value = DateTime.Parse(dtStart.ToShortDateString());
                sqlCmd.Parameters["@START_TIME"].Value = DateTime.Parse(dtStart.ToShortTimeString());
            }
            else
            {
                sqlCmd.Parameters["@START_DATE"].Value = DBNull.Value;
                sqlCmd.Parameters["@START_TIME"].Value = DBNull.Value;
            }
            if (HasEndDate)
            {
                sqlCmd.Parameters["@END_DATE"].Value = DateTime.Parse(dtEnd.ToShortDateString());
                sqlCmd.Parameters["@END_TIME"].Value = DateTime.Parse(dtEnd.ToShortTimeString());
            }
            else
            {
                sqlCmd.Parameters["@END_DATE"].Value = DBNull.Value;
                sqlCmd.Parameters["@END_TIME"].Value = DBNull.Value;
            }
            if (HasItemDate)
            {
                sqlCmd.Parameters["@ITEM_DATE"].Value = DateTime.Parse(dtItem.ToShortDateString());
                sqlCmd.Parameters["@ITEM_TIME"].Value = DateTime.Parse(dtItem.ToShortTimeString());
            }
            else
            {
                sqlCmd.Parameters["@ITEM_DATE"].Value = DateTime.Parse(DateTime.Now.ToShortDateString());
                sqlCmd.Parameters["@ITEM_TIME"].Value = DateTime.Parse(DateTime.Now.ToShortTimeString());
            }
            if (strCompletionCode == "C")
            {
                sqlCmd.Parameters["@COMPLETION_CDE"].Value = strCompletionCode;
                sqlCmd.Parameters["@COMPLETION_DTE"].Value = dtCompleted;
            }
            else
            {
                sqlCmd.Parameters["@COMPLETION_CDE"].Value = "P";
                sqlCmd.Parameters["@COMPLETION_DTE"].Value = DBNull.Value;
            }
            if (strToDoUser == @"N/A")
            {
                sqlCmd.Parameters["@TODO_IDNUM"].Value = DBNull.Value;
                sqlCmd.Parameters["@TODOUSER"].Value = DBNull.Value;
            }
            else
            {
                sqlCmd.Parameters["@TODO_IDNUM"].Value = TODO_IDNUM;
                sqlCmd.Parameters["@TODOUSER"].Value = strToDoUser;
            }
            #endregion

            int iCount = 0;
            try
            {
                sqlConn.Open();
                iCount = sqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ParentPortlet.ShowFeedback(FeedbackType.Error, "An error occurred while trying "
                    + "to update this note in the database. Error: " + ex.ToString());
                IsSuccess = false;
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }

            if (iCount != 1)
            {
                IsSuccess = false;
            }

            return IsSuccess;
        }

        protected bool DeleteNote(int ID_NUM, int GROUP_NUM, int SUBGROUP_NUM, int SEQ_NUM)
        {
            bool IsSuccess = true;
            string strQuery = "UPDATE ITEMS "
                + "SET ACTIVE_INACTIVE = 'I', USER_NAME = @USERNAME, JOB_NAME = 'J:DelAdvisingNote', JOB_TIME = GETDATE() "
                + "WHERE ID_NUMBER = @ID_NUM AND GROUP_NUMBER = @GROUP_NUMBER AND "
                + "SUBGROUP_NUMBER = @SUBGROUP_NUMBER AND GROUP_SEQUENCE = @SEQ_NUM";
            
            string strPortalUserID = "J:" + PortalUser.Current.HostID.ToString();

            SqlConnection sqlConn = new SqlConnection(strERPConn);
            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
            sqlCmd.Parameters.Add("@USERNAME", SqlDbType.Char, 15);
            sqlCmd.Parameters.Add("@ID_NUM", SqlDbType.Int);
            sqlCmd.Parameters.Add("@GROUP_NUMBER", SqlDbType.Int);
            sqlCmd.Parameters.Add("@SUBGROUP_NUMBER", SqlDbType.Int);
            sqlCmd.Parameters.Add("@SEQ_NUM", SqlDbType.Int);
            sqlCmd.Parameters["@USERNAME"].Value = strPortalUserID;
            sqlCmd.Parameters["@ID_NUM"].Value = ID_NUM;
            sqlCmd.Parameters["@GROUP_NUMBER"].Value = GROUP_NUM;
            sqlCmd.Parameters["@SUBGROUP_NUMBER"].Value = SUBGROUP_NUM;
            sqlCmd.Parameters["@SEQ_NUM"].Value = SEQ_NUM;

            int iCount = 0;
            try
            {
                sqlConn.Open();
                iCount = sqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ParentPortlet.ShowFeedback(FeedbackType.Error,"An error occurred while trying to set this note inactive. Error: " + ex.ToString());
                IsSuccess = false;
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }

            if (iCount != 1)
            {
                IsSuccess = false;
            }
            return IsSuccess;
        }

        #endregion
       
        #region OnEvent methods
        #region Methods for selection dropboxes OnSelectedIndexChanged
        protected void ddlAdvisors_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlNotes.Visible = false;
            pnlInfo.Visible = false;
            ResetAddNote();
            FillAdviseeDropdown();
        }
        protected void ddlAdvisees_SelectedIndexChanged(object sender, EventArgs e)
        {
            int ID_NUM = 0;
            Session.Remove("Status");
            Session.Remove("Advisee");
            Session.Remove("IsConcerned");
            ResetAddNote();

            bool IsSuccess = true;

            try
            {
                ID_NUM = Int32.Parse(ddlAdvisees.SelectedValue);
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                lblMsg.Text = "Error retrieving course information: " + ex.ToString();
            }

            if (IsSuccess)
            {
                string strAdminLvl = Session["AdminLvl"].ToString();

                DataTable _dtCourses = new DataTable();
                DataTable _dtAdviseeInfo = new DataTable();
                SqlConnection sqlConn = new SqlConnection(strERPConn);
                sqlConn.Open();
                _dtCourses = dtAdviseeCourses(ID_NUM, sqlConn);
                _dtAdviseeInfo = dtAdviseeInfo(ID_NUM, sqlConn);
                sqlConn.Close();

                grdCourses.DataSource = _dtCourses;
                grdCourses.DataBind();
                if (_dtAdviseeInfo.Rows.Count == 1)
                {
                    Session["Status"] = "New";
                    Session["Advisee"] = ID_NUM.ToString();
                    if (strAdminLvl == "ViewAll")
                    {
                        btnAddNote.Enabled = false;
                        btnAddNote.Visible = false;
                        pnlAddNote.Visible = false;
                        if (IsConcerned(ID_NUM))
                        {
                            Session["IsConcerned"] = Session["Advisee"].ToString();
                            pnlAddNote.Visible = true;
                            btnAddNote.Visible = true;
                            btnAddNote.Enabled = true;
                        }
                    }
                    FillAdviseeNotes(ID_NUM);
                    FillAdviseeInfo(_dtAdviseeInfo);
                }
            }
        }
        #endregion    
        protected void grdNotes_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            string strSec = Session["AdminLvl"].ToString();
            if ((strSec == "EditAll") || (strSec == "Admin"))
            {
                if (grdNotes.Columns.Count < 12)
                {
                    AddEditColumns();
                }
                else
                {
                    if ((strSec == "Self"))
                    {
                        AddEditColumns();
                    }
                    else
                    {
                        if ((strSec == "ViewAll") && 
                            (Session["IsConcerned"].ToString() == Session["Advisee"].ToString()))
                        {
                            AddEditColumns();
                        }
                    }
                }
            }
        }
        protected void grdNotes_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                #region Get data for text fields
                int GROUP_NUMBER = 0;
                int SUBGROUP_NUMBER = 0;
                int SEQ_NUMBER = 0;
                int ID_NUMBER = 0;
                bool IsSuccess = true;
                string strSQN = "";
                string strGRP = "";
                string strSGN = "";

                strSQN = ((GroupedGridTableCell)e.Item.Controls[3]).Text.Trim();
                strGRP = ((GroupedGridTableCell)e.Item.Controls[1]).Text.Trim();
                strSGN = ((GroupedGridTableCell)e.Item.Controls[2]).Text.Trim();

                try
                {
                    ID_NUMBER = Int32.Parse(Session["Advisee"].ToString().Trim());
                    GROUP_NUMBER = Int32.Parse(strGRP);
                    SUBGROUP_NUMBER = Int32.Parse(strSGN);
                    SEQ_NUMBER = Int32.Parse(strSQN);
                }
                catch
                {
                    IsSuccess = false;
                }
                #endregion

                if (IsSuccess)
                {
                    DataTable dt = dtAdviseeNote(ID_NUMBER, GROUP_NUMBER, SUBGROUP_NUMBER, SEQ_NUMBER);
                    if (dt.Rows.Count > 0)
                    {
                        #region Populate text fields
                        lblAddNewNote.Text = "Edit this note:";
                        btnAddNote.Text = "Edit Note";
                        foreach (DataRow dr in dt.Rows)
                        {
                            lblGroupNum.Text = dr[1].ToString().Trim();
                            lblSubgroupNum.Text = dr[2].ToString().Trim();
                            lblSequenceNum.Text = dr[3].ToString().Trim();
                            txtNoteDesc.Text = dr[4].ToString().Trim();
                            txtStartDate.Text = dr[5].ToString().Trim();
                            txtEndDate.Text = dr[6].ToString().Trim();
                            txtItemDate.Text = dr[7].ToString().Trim();
                            if (dr[8].ToString().Trim() == "C")
                            {
                                cbCompleted.Checked = true;
                            }
                            else
                            {
                                cbCompleted.Checked = false;
                            }
                            txtCompletedDate.Text = dr[9].ToString().Trim();
                            if(dr[10].ToString().Trim() != "")
                            {
                            try
                            {
                                ddlNotepadUsers.SelectedValue = dr[10].ToString().Trim();
                            }
                            catch
                            {
                                ddlNotepadUsers.SelectedIndex = 0;
                                ParentPortlet.ShowFeedback(FeedbackType.Error,"Warning: the user to whom this task was assigned, ID #" + dr[11].ToString().Trim() 
                                    + ", cannot be identified. The assigned user has been stripped. This change will be made permanent if you "
                                    + "save the edited note.");
                            }
                            }
                            if (dr[12].ToString().Trim() == "Y")
                            {
                                cbShowOnWeb.Checked = true;
                            }
                            else
                            {
                                cbShowOnWeb.Checked = false;
                            }
                        }
                        #endregion
                        txtNoteDesc.Focus();
                    }
                    else
                    {
                        ParentPortlet.ShowFeedback(FeedbackType.Error, "Error: This item could not be found to edit.");
                    }
                }

            }
            if (e.CommandName == "Del")
            {
                int GROUP_NUMBER = 0;
                int SUBGROUP_NUMBER = 0;
                int SEQ_NUMBER = 0;
                int ID_NUMBER = 0;
                bool IsSuccess = true;
                string strSQN = "";
                string strGRP = "";
                string strSGN = "";

                strSQN = ((GroupedGridTableCell)e.Item.Controls[3]).Text.Trim();
                strGRP = ((GroupedGridTableCell)e.Item.Controls[1]).Text.Trim();
                strSGN = ((GroupedGridTableCell)e.Item.Controls[2]).Text.Trim();

                //lblMsg.Text = "ID: " + Session["Advisee"].ToString() + " -- Group:" +
                //    strGRP + ":" + strSGN + " -- SEQ Num:" + strSQN;
                try
                {
                    ID_NUMBER = Int32.Parse(Session["Advisee"].ToString().Trim());
                    GROUP_NUMBER = Int32.Parse(strGRP);
                    SUBGROUP_NUMBER = Int32.Parse(strSGN);
                    SEQ_NUMBER = Int32.Parse(strSQN);
                }
                catch
                {
                    IsSuccess = false;
                }

                if (IsSuccess)
                {
                    if ((strGRP == "") || (strSQN == "") || (strSGN == ""))
                    {
                        ParentPortlet.ShowFeedback(FeedbackType.Error, "The item you are trying to delete is missing a vital piece of information. "
                            + "Please delete it using desktop client software.");
                    }
                    else
                    {
                        if (DeleteNote(ID_NUMBER, GROUP_NUMBER, SUBGROUP_NUMBER, SEQ_NUMBER))
                        {
                            FillAdviseeNotes(ID_NUMBER);
                            ParentPortlet.ShowFeedback(FeedbackType.Message, "Note deleted successfully.");
                        }
                    }
                }
                else
                {
                    ParentPortlet.ShowFeedback(FeedbackType.Error, "The item you are trying to delete is damaged in some way. "
                            + "Please delete it using desktop client software.");
                }
            }
        }
        protected void btnAddNote_Click(object sender, EventArgs e)
        {
            int ID_NUM = 0;
            bool ParseSuccess = true;
            try
            {
                ID_NUM = Int32.Parse(Session["Advisee"].ToString());
            }
            catch
            {
                ParseSuccess = false;
            }
            if (ParseSuccess)
            {
                #region Validate security
                string strAdminLvl = Session["AdminLvl"].ToString();
                bool CanAdd = false;
                bool CanEdit = false;

                if ((strAdminLvl == "Admin") || (strAdminLvl == "EditAll"))
                {
                    CanAdd = true;
                    CanEdit = true;
                }
                if (strAdminLvl == "ViewAll")
                {
                    if (Session["IsConcerned"].ToString() == Session["Advisee"].ToString())
                    {
                        CanAdd = true;
                        CanEdit = true;
                    }
                }
                #endregion
                switch (btnAddNote.Text.Trim())
                {
                    case "Add Note":
                        #region Add Note
                        if (CanAdd)
                        {
                            #region Validate user fields
                            #region Define variables
                            bool IsValidated = true;
                            DateTime dtStart = DateTime.Now;
                            DateTime dtEnd = DateTime.Now;
                            DateTime dtCompleted = DateTime.Now;
                            DateTime dtItem = DateTime.Now;
                            string strStartDate = txtStartDate.Text.Trim();
                            string strEndDate = txtEndDate.Text.Trim();
                            string strCompletedDate = txtCompletedDate.Text.Trim();
                            bool HasStartDate = false;
                            bool HasEndDate = false;
                            bool IsCompleted = false;
                            string strToDoIDNUM = @"N/A";
                            string strToDoUser = @"N/A";
                            int TO_DO_IDNUM = 0;
                            #endregion

                            strToDoIDNUM = ddlNotepadUsers.SelectedValue;
                            if (strToDoIDNUM != @"N/A")
                            {
                                strToDoUser = ddlNotepadUsers.SelectedItem.Text;
                                TO_DO_IDNUM = Int32.Parse(ddlNotepadUsers.SelectedValue);
                            }
                            #region Set DateTime values
                            if (strStartDate.Length > 0)
                            {
                                try
                                {
                                    dtStart = DateTime.Parse(strStartDate);
                                    HasStartDate = true;
                                }
                                catch
                                {
                                    IsValidated = false;
                                    ParentPortlet.ShowFeedback(FeedbackType.Error, "Please verify that all dates entered are valid.");
                                }
                            }
                            if (strEndDate.Length > 0)
                            {
                                try
                                {
                                    dtEnd = DateTime.Parse(strEndDate);
                                    HasEndDate = true;
                                }
                                catch
                                {
                                    IsValidated = false;
                                    ParentPortlet.ShowFeedback("Please verify that all dates entered are valid.");
                                }
                            }
                            if (strCompletedDate.Trim().Length > 0)
                            {
                                try
                                {
                                    dtCompleted = DateTime.Parse(strCompletedDate);
                                    cbCompleted.Checked = true;
                                    IsCompleted = true;
                                }
                                catch
                                {
                                    IsValidated = false;
                                    ParentPortlet.ShowFeedback("Please verify that all dates entered are valid.");
                                }
                            }
                            if ((cbCompleted.Checked == true) && (strCompletedDate.Trim().Length > 0))
                            {
                                dtCompleted = DateTime.Now;
                                txtCompletedDate.Text = dtCompleted.ToShortDateString();
                                IsCompleted = true;
                            }
                            #endregion
                            #endregion

                            if (IsValidated)
                            {
                                #region Get remaining values
                                bool ShowOnWeb = cbShowOnWeb.Checked;
                                string strDesc = "";
                                strDesc = txtNoteDesc.Text.Trim();
                                if (AddNote(ID_NUM, strDesc, dtStart, dtEnd, dtCompleted, TO_DO_IDNUM, strToDoUser, dtItem, strAdvisingModuleCode,
                                            ShowOnWeb, IsCompleted, HasStartDate, HasEndDate))
                                {
                                    FillAdviseeNotes(ID_NUM);
                                    ResetAddNote();
                                    ddlNotepadUsers.SelectedIndex = 0;
                                }

                                #endregion
                            }
                        }
                        else
                        {
                            ParentPortlet.ShowFeedback(FeedbackType.Error, "Error: You do not have permission to add notes to this advisee.");
                        }
                        #endregion
                        break;
                    case "Edit Note":
                        #region Edit Note
                        if (CanEdit)
                        {
                        #region Validate user fields
                        #region Define variables
                        bool IsValidated = true;
                        DateTime dtStart = DateTime.Now;
                        DateTime dtEnd = DateTime.Now;
                        DateTime dtCompleted = DateTime.Now;
                        DateTime dtItem = DateTime.Now;
                        string strItemDate = txtItemDate.Text.Trim();
                        string strStartDate = txtStartDate.Text.Trim();
                        string strEndDate = txtEndDate.Text.Trim();
                        string strCompletedDate = txtCompletedDate.Text.Trim();
                        string strGroupNum = lblGroupNum.Text.Trim();
                        string strSubGroupNum = lblSubgroupNum.Text.Trim();
                        string strSeqNum = lblSequenceNum.Text.Trim();
                        bool HasStartDate = false;
                        bool HasEndDate = false;
                        bool IsCompleted = false;
                        bool HasItemDate = false;
                        string strToDoIDNUM = @"N/A";
                        string strToDoUser = @"N/A";
                        int TO_DO_IDNUM = 0;
                        int GROUP_NUMBER = 0;
                        int SUBGROUP_NUMBER = 0;
                        int SEQ_NUM = 0;
                        #endregion

                        strToDoIDNUM = ddlNotepadUsers.SelectedValue;
                        if (strToDoIDNUM != @"N/A")
                        {
                            strToDoUser = ddlNotepadUsers.SelectedItem.Text;
                            TO_DO_IDNUM = Int32.Parse(ddlNotepadUsers.SelectedValue);
                        }
                        #region Get group and subgroup numbers
                        try
                        {
                            GROUP_NUMBER = Int32.Parse(strGroupNum);
                            SUBGROUP_NUMBER = Int32.Parse(strSubGroupNum);
                            SEQ_NUM = Int32.Parse(strSeqNum);
                        }
                        catch
                        {
                            IsValidated = false;
                            ParentPortlet.ShowFeedback(FeedbackType.Error, "Error: could not retrieve group or sequence number for note to be edited.");
                        }
                        #endregion
                        #region Set DateTime values
                        if (strItemDate.Length > 0)
                        {
                            try
                            {
                                dtItem = DateTime.Parse(strItemDate);
                                HasItemDate = true;
                            }
                            catch
                            {
                                IsValidated = false;
                                ParentPortlet.ShowFeedback(FeedbackType.Error,
                                    "Please verify that the Item Date entered is valid.");
                            }
                        }
                        if (strStartDate.Length > 0)
                        {
                            try
                            {
                                dtStart = DateTime.Parse(strStartDate);
                                HasStartDate = true;
                            }
                            catch
                            {
                                IsValidated = false;
                                ParentPortlet.ShowFeedback(FeedbackType.Error, "Please verify that the Start Date entered is valid.");
                            }
                        }
                        if (strEndDate.Length > 0)
                        {
                            try
                            {
                                dtEnd = DateTime.Parse(strEndDate);
                                HasEndDate = true;
                            }
                            catch
                            {
                                IsValidated = false;
                                ParentPortlet.ShowFeedback("Please verify that the End Date entered is valid.");
                            }
                        }
                        if (strCompletedDate.Trim().Length > 0)
                        {
                            try
                            {
                                dtCompleted = DateTime.Parse(strCompletedDate);
                                cbCompleted.Checked = true;
                                IsCompleted = true;
                            }
                            catch
                            {
                                IsValidated = false;
                                ParentPortlet.ShowFeedback("Please verify that the Date Completed entered is valid.");
                            }
                        }
                        if ((cbCompleted.Checked == true) && (strCompletedDate.Trim().Length == 0))
                        {
                            dtCompleted = DateTime.Now;
                            txtCompletedDate.Text = dtCompleted.ToShortDateString();
                            IsCompleted = true;
                        }
                        #endregion
                        #endregion

                            if (IsValidated)
                            {
                                #region Get remaining values
                                string strCompleted = "P";
                                if (cbCompleted.Checked)
                                {
                                    strCompleted = "C";
                                }
                                bool ShowOnWeb = cbShowOnWeb.Checked;

                                string strDesc = "";
                                strDesc = txtNoteDesc.Text.Trim();
                                #endregion
                                #region Edit
                                lblMsg.Text = "Tried to edit note.";
                                if (EditNote(ID_NUM, GROUP_NUMBER, SUBGROUP_NUMBER, SEQ_NUM,
                                        strDesc, dtStart, dtEnd, dtItem, dtCompleted,
                                        TO_DO_IDNUM, strToDoUser, strAdvisingModuleCode,
                                        ShowOnWeb, strCompleted, HasStartDate, HasEndDate, HasItemDate))
                                {
                                    ResetAddNote();
                                    ddlNotepadUsers.SelectedIndex = 0;
                                    FillAdviseeNotes(ID_NUM);
                                }
                                #endregion
                            }                      
                        }

                        else
                        {
                    ParentPortlet.ShowFeedback(FeedbackType.Error, "Error: You do not have permission to edit this note.");
                        }
                        break;
                        #endregion
                }
            }
            else
            {
                ParentPortlet.ShowFeedback(FeedbackType.Error, "Error: unable to determine advisee ID.");
            }
        }
        #endregion

        #region Returned values
        public string strCompletion(string strCode, DateTime dtDate)
        {
            string strFinal = "";
            if (strCode == "C")
            {
                strFinal = "Completed: date unknown.";
                if (dtDate > DateTime.Parse("1/2/1900"))
                {
                    strFinal = "Completed: " + dtDate.ToShortDateString() + " " + dtDate.ToShortTimeString();
                }
            }
            return strFinal;
        }
        protected bool IsConcerned(int ID_NUM)
        {
            bool _isconcerned = false;
            int IsConcerned = 0;

            string strQuery = @"select dbo.[CUS_fn_AdvisingNotesIsConcerned] (@ID_NUM, @ADVISOR_ID)";

            SqlConnection sqlConn = new SqlConnection(strERPConn);
            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
            sqlCmd.Parameters.Add("@ID_NUM", SqlDbType.Int);
            sqlCmd.Parameters["@ID_NUM"].Value = ID_NUM;
            sqlCmd.Parameters.Add("@ADVISOR_ID",SqlDbType.Int);
            sqlCmd.Parameters["@ADVISOR_ID"].Value = Int32.Parse(PortalUser.Current.HostID);

            try
            {
                sqlConn.Open();
                IsConcerned = (Int32)sqlCmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                IsConcerned = 0;
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }
            if (IsConcerned > 0)
            {
                _isconcerned = true;
            }

            return _isconcerned;

        }
        protected bool IsAdvisor()
        {
            bool _isadvisor = false;
            int AdviseeCount = 0;

            string strQuery = "select count(id_num) from advisor_stud_table where id_num = @ID_NUM";

            SqlConnection sqlConn = new SqlConnection(strERPConn);
            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
            sqlCmd.Parameters.Add("@ID_NUM", SqlDbType.Int);
            sqlCmd.Parameters["@ID_NUM"].Value = Int32.Parse(PortalUser.Current.HostID);

            try
            {
                sqlConn.Open();
                AdviseeCount = (Int32)sqlCmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                AdviseeCount = 0;
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }
            if (AdviseeCount > 0)
            {
                _isadvisor = true;
            }

            return _isadvisor;

        }
        #endregion
        #region Datatables
        protected DataTable dtAdviseesByAdvisor(int ID_NUM)
        {
            DataTable dt = new DataTable();
            string strQuery = "SELECT distinct ID_NUM, LAST_NAME, FIRST_NAME "
                + "FROM CUS_vw_AdviseesByAdvisorForNotesPortlet "
                + "WHERE ADVISOR_ID = @ADVISOR_ID AND "
                + "(MODULE_CODE = '*AV' OR MODULE_CODE = @MODULE_CODE) "
                + "ORDER BY LAST_NAME, FIRST_NAME";

            SqlConnection sqlConn = new SqlConnection(strERPConn);
            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
            sqlCmd.Parameters.Add("@ADVISOR_ID", SqlDbType.Int);
            sqlCmd.Parameters["@ADVISOR_ID"].Value = ID_NUM;
            sqlCmd.Parameters.Add("@MODULE_CODE", SqlDbType.Char, 2);
            sqlCmd.Parameters["@MODULE_CODE"].Value = strAdvisingModuleCode;

            try
            {
                sqlConn.Open();
                SqlDataReader dr = sqlCmd.ExecuteReader();
                DataUtils.DataReaderAdapter dra = new DataUtils.DataReaderAdapter();
                dra.FillFromReader(dt, dr);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }

            return dt;
        }
        protected DataTable dtAllAdvisees()
        {
            DataTable dt = new DataTable();
            string strQuery = "SELECT distinct * FROM CUS_vw_AllAdviseesForNotesPortlet ORDER BY ADVISEE_NAME, ADVISOR_NAME";
            SqlConnection sqlConn = new SqlConnection(strERPConn);
            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);

            try
            {
                sqlConn.Open();
                SqlDataReader dr = sqlCmd.ExecuteReader();
                DataUtils.DataReaderAdapter dra = new DataUtils.DataReaderAdapter();
                dra.FillFromReader(dt, dr);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }

            return dt;
        }
        protected DataTable dtAdvisors(SqlConnection sqlConn)
        {
            DataTable dt = new DataTable();
            string strQuery = "SELECT distinct * FROM "
                + "CUS_vw_AdvisorListForAdviseeNotesPortlet ORDER BY NAME";

            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);

            try
            {
                SqlDataReader dr = sqlCmd.ExecuteReader();
                DataUtils.DataReaderAdapter dra = new DataUtils.DataReaderAdapter();
                dra.FillFromReader(dt, dr);
                dr.Dispose();
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        protected DataTable dtNotepadUsers(SqlConnection sqlConn)
        {
            DataTable dt = new DataTable();
            string strQuery = "select USER_ID, ID_NUMBER from CUS_vw_NotepadUsers ORDER BY USER_ID";

            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);

            try
            {
                SqlDataReader dr = sqlCmd.ExecuteReader();
                DataUtils.DataReaderAdapter dra = new DataUtils.DataReaderAdapter();
                dra.FillFromReader(dt, dr);
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        protected DataTable dtAdviseeInfo(int ID_NUM, SqlConnection sqlConn)
        {
            DataTable dt = new DataTable();
            string strQuery = "SELECT distinct * FROM CUS_vw_AdviseeInfoForNotesPortlet WHERE ID_NUM = @ID_NUM";

            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
            sqlCmd.Parameters.Add("@ID_NUM", SqlDbType.Int);
            sqlCmd.Parameters["@ID_NUM"].Value = ID_NUM;

            SqlDataReader dr = sqlCmd.ExecuteReader();
            DataUtils.DataReaderAdapter dra = new DataUtils.DataReaderAdapter();
            dra.FillFromReader(dt, dr);
            dr.Dispose();
            return dt;
        }
        protected DataTable dtAdviseeCourses(int ID_NUM, SqlConnection sqlConn)
        {
            DataTable dt = new DataTable();

            string strQuery = "SELECT DISTINCT * FROM CUS_vw_AdviseeCoursesForNotesPortlet WHERE id_num = @ID_NUM";

            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
            sqlCmd.Parameters.Add("@ID_NUM", SqlDbType.Int);
            sqlCmd.Parameters["@ID_NUM"].Value = ID_NUM;

            SqlDataReader dr = sqlCmd.ExecuteReader();
            DataUtils.DataReaderAdapter dra = new DataUtils.DataReaderAdapter();
            dra.FillFromReader(dt, dr);
            dr.Dispose();
            return dt;
        }
        protected DataTable dtAdviseeNotes(int ID_NUM, SqlConnection sqlConn)
        {
            DataTable dt = new DataTable();

            string strQuery = "select GROUP_NUMBER, SUBGROUP_NUMBER, GROUP_SEQUENCE, ID_NUMBER, " +
                "ITEM_DESCRIPTION, START_DATE, START_TIME, END_DATE, END_TIME, ITEM_DATE, ITEM_TIME, MODULE_CODE, " +
                "TO_DO_ID_NUMBER, TO_DO_USER_ID, DISPLAY_ON_WEB, " +
                "COMPLETION_CODE, CASE WHEN COMPLETION_DTE IS NULL THEN CAST('1/1/1900' AS DATETIME) ELSE COMPLETION_DTE END AS COMPLETION_DTE, " +
                "USER_NAME, JOB_NAME, JOB_TIME " +
                "FROM ITEMS WHERE MODULE_CODE = @MODULE_CODE " +
                "AND ITEM_TYPE = 'NOTE' AND ACTIVE_INACTIVE = 'A' " +
                "AND ID_NUMBER = @ID_NUM " +
                "ORDER BY ID_NUMBER,ITEM_DATE ASC";

            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
            sqlCmd.Parameters.Add("@ID_NUM", SqlDbType.Int);
            sqlCmd.Parameters["@ID_NUM"].Value = ID_NUM;
            sqlCmd.Parameters.Add("@MODULE_CODE", SqlDbType.Char, 2);
            sqlCmd.Parameters["@MODULE_CODE"].Value = strAdvisingModuleCode;

            SqlDataReader dr = sqlCmd.ExecuteReader();
            DataUtils.DataReaderAdapter dra = new DataUtils.DataReaderAdapter();
            dra.FillFromReader(dt, dr);
            dr.Dispose();
            foreach (DataRow dRow in dt.Rows)
            {
                if (dRow["ITEM_DESCRIPTION"].ToString().Trim().Length > 0)
                {
                    dRow["ITEM_DESCRIPTION"] = @"<pre>" + dRow["ITEM_DESCRIPTION"].ToString().Trim() + @"</pre>";
                }
            }
            return dt;
        }
        protected DataTable dtAdviseeNote(int ID_NUM, int GROUP_NUM, int SUBGROUP_NUM, int SEQ_NUM)
        {
            DataTable dt = new DataTable();
            string strQuery = "SELECT TOP 1 [ID_NUMBER], [GROUP_NUMBER], [SUBGROUP_NUMBER], [GROUP_SEQUENCE], "
                + "[ITEM_DESCRIPTION], "
                + "CASE WHEN [START_DATE] IS NULL THEN '' ELSE CONVERT(VARCHAR,[START_DATE],101) END + "
                + "CASE WHEN [START_TIME] IS NULL THEN '' ELSE ' ' + CONVERT(VARCHAR,[START_TIME],8) END AS START_DATE, "
                + "CASE WHEN [END_DATE] IS NULL THEN '' ELSE CONVERT(VARCHAR,[END_DATE],101) END + "
                + "CASE WHEN [END_TIME] IS NULL THEN '' ELSE ' ' + CONVERT(VARCHAR,[END_TIME],8) END AS END_DATE, "
                + "CASE WHEN [ITEM_DATE] IS NULL THEN '' ELSE CONVERT(VARCHAR,[ITEM_DATE],101) END + "
                + "CASE WHEN [ITEM_TIME] IS NULL THEN '' ELSE ' ' + CONVERT(VARCHAR,[ITEM_TIME],8) END AS ITEM_DATE, "
                + "[COMPLETION_CODE], [COMPLETION_DTE], "
                + "[TO_DO_ID_NUMBER], [TO_DO_USER_ID], [DISPLAY_ON_WEB] "
                + "FROM ITEMS "
                + "WHERE ID_NUMBER = @ID_NUM AND MODULE_CODE = @MODULE_CODE "
                + "AND GROUP_NUMBER = @GROUP_NUM AND SUBGROUP_NUMBER = @SUBGROUP_NUM "
                + "AND GROUP_SEQUENCE = @SEQ_NUM ";
            SqlConnection sqlConn = new SqlConnection(strERPConn);
            SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
            sqlCmd.Parameters.Add("@ID_NUM", SqlDbType.Int);
            sqlCmd.Parameters.Add("@GROUP_NUM", SqlDbType.Int);
            sqlCmd.Parameters.Add("@SUBGROUP_NUM", SqlDbType.Int);
            sqlCmd.Parameters.Add("@SEQ_NUM", SqlDbType.Int);
            sqlCmd.Parameters.Add("@MODULE_CODE", SqlDbType.Char, 2);
            sqlCmd.Parameters["@ID_NUM"].Value = ID_NUM;
            sqlCmd.Parameters["@GROUP_NUM"].Value = GROUP_NUM;
            sqlCmd.Parameters["@SUBGROUP_NUM"].Value = SUBGROUP_NUM;
            sqlCmd.Parameters["@SEQ_NUM"].Value = SEQ_NUM;
            sqlCmd.Parameters["@MODULE_CODE"].Value = strAdvisingModuleCode;

            try
            {
                sqlConn.Open();
                SqlDataReader dr = sqlCmd.ExecuteReader();
                DataUtils.DataReaderAdapter dra = new DataUtils.DataReaderAdapter();
                dra.FillFromReader(dt, dr);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                try
                {
                    sqlConn.Close();
                }
                catch
                {
                }
            }

            return dt;
        }
        #endregion
    }
}
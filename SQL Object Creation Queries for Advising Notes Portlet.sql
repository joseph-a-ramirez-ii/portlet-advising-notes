-- Before executing this script, read and obey the comment below!
--
--
--
--
-- Enter a password for the login in the line below, then enter the same password 
-- into the Default_View.ascx.cs file and recompile (after also setting your server name, etc.)
-- You do not need to use the login name JICSAdvisingNotes, but it will need to be changed everywhere
-- it appears in this script and in the strUsername string in the .CS file if you choose to use a 
-- different login. This login functions as the database user for the portlet; you will not have
-- database connectivity unless this login is created and granted access to the appropriate objects,
-- and the portlet code is configured and recompiled with the correct connection information.

--CREATE LOGIN [JICSAdvisingNotes] WITH PASSWORD = 'ENTER_A_STRONG_PASSWORD_HERE'
GO

CREATE FUNCTION [dbo].[CUS_fn_GetNextNotepadGroupNumber]
(
	@ID_NUM int
)
RETURNS int
AS
BEGIN
	DECLARE @GROUP_NUMBER int;

	IF EXISTS (SELECT ID_NUMBER FROM ITEMS where ID_NUMBER = @ID_NUM) 
	BEGIN
	SET @GROUP_NUMBER = (SELECT top 1 rownum from 
	(select id_number, group_number, row_number() over (order by id_number, group_number) as RowNum
	from (select distinct i.id_number, i.group_number from ITEMS I where i.id_number = @ID_NUM) temp) list
	where rownum <> group_number)
	IF @GROUP_NUMBER IS NULL
		BEGIN
			SET @GROUP_NUMBER = (SELECT GROUP_NUM  FROM 
				(SELECT id_number, max(group_number) + 1 AS GROUP_NUM from items where id_number = @id_num group by id_number ) temp)
		END
	END
	ELSE
	BEGIN
	SET @GROUP_NUMBER = 1;
	END

	RETURN @GROUP_NUMBER

END

GO

/****** Object:  View [dbo].[CUS_vw_AdviseeInfoForNotesPortlet]    Script Date: 04/09/2010 10:06:24 ******/
CREATE VIEW [dbo].[CUS_vw_AdviseeCoursesForNotesPortlet]
AS
SELECT     ID_NUM, YR_CDE, TRM_CDE, CRS_CDE, CRS_TITLE, CREDIT_HRS, CASE WHEN grade_cde IS NULL THEN CASE WHEN midterm_gra_cde IS NULL 
                      THEN 'Not Graded' ELSE midterm_gra_cde END ELSE grade_cde END AS grade, ORIGINAL_REG_DTE
FROM         dbo.STUDENT_CRS_HIST
WHERE     (TRANSACTION_STS = 'C')
ORDER BY YR_CDE DESC, TRM_CDE DESC, CRS_CDE
GO

CREATE VIEW [dbo].[CUS_vw_NotepadUsers]
AS
SELECT     TOP (100) PERCENT USER_ID, ID_NUMBER, USER_NAME__LONG_, USER_TITLE, MODULE_CODE
FROM         dbo.NOTE_PAD_USER
WHERE     (ID_NUMBER <> 1) AND (USER_ID IS NOT NULL) AND (RTRIM(USER_ID) <> '')
ORDER BY USER_ID
GO

CREATE VIEW [dbo].[CUS_vw_AdviseeInfoForNotesPortlet]
AS
SELECT DISTINCT 
                      NAM.ID_NUM, CASE WHEN NAM.LAST_NAME IS NULL THEN 'Unknown' ELSE CASE WHEN NAM.PREFERRED_NAME IS NULL OR
                      RTRIM(NAM.PREFERRED_NAME) = '' THEN RTRIM(NAM.FIRST_NAME) + ' ' + RTRIM(NAM.LAST_NAME) ELSE RTRIM(NAM.PREFERRED_NAME) 
                      + ' ' + RTRIM(NAM.LAST_NAME) END END AS NAME, CASE WHEN DV.DIV_DESC IS NULL THEN 'Unknown' ELSE DV.DIV_DESC END AS DIVISION, 
                      CASE WHEN CD.CLASS_DESC IS NULL THEN 'Unknown' ELSE CD.CLASS_DESC END AS CLASS, CASE WHEN SM.TRM_PT_FT_STS IS NULL 
                      THEN '' WHEN SM.TRM_PT_FT_STS = 'P' THEN 'Part-Time' WHEN SM.TRM_PT_FT_STS = 'F' THEN 'Full-Time' END AS PT_FT_STS, 
                      CASE WHEN HA.ACAD_STAND_DESC IS NULL THEN 'N/A' ELSE HA.ACAD_STAND_DESC END AS HONORS, CASE WHEN PA.ACAD_STAND_DESC IS NULL 
                      THEN 'N/A' ELSE PA.ACAD_STAND_DESC END AS ACADEMIC_STANDING, CASE WHEN sm.most_recnt_yr_enr IS NULL 
                      THEN 'Unknown' ELSE CASE WHEN SM.MOST_RECNT_TRM_ENR IS NULL THEN RTRIM(SM.MOST_RECNT_YR_ENR) ELSE RTRIM(SM.MOST_RECNT_YR_ENR) 
                      + ':' + RTRIM(sm.most_recnt_trm_enr) END END AS LAST_TRM, CASE WHEN DG.TABLE_DESC IS NULL THEN 'N/A' ELSE DG.TABLE_DESC END AS DEGREE, 
                      CASE WHEN MJ1.MAJOR_MINOR_DESC IS NULL THEN '' ELSE MJ1.MAJOR_MINOR_DESC END AS MAJOR_1, CASE WHEN MJ2.MAJOR_MINOR_DESC IS NULL 
                      THEN '' ELSE MJ2.MAJOR_MINOR_DESC END AS MAJOR_2, CASE WHEN MN1.MAJOR_MINOR_DESC IS NULL 
                      THEN '' ELSE MN1.MAJOR_MINOR_DESC END AS MINOR_1, CASE WHEN MN2.MAJOR_MINOR_DESC IS NULL 
                      THEN '' ELSE MN2.MAJOR_MINOR_DESC END AS MINOR_2, CASE WHEN CN1.CONC_DESC IS NULL THEN '' ELSE CN1.CONC_DESC END AS CONC_1, 
                      CASE WHEN CN2.CONC_DESC IS NULL THEN '' ELSE CN2.CONC_DESC END AS CONC_2, CASE WHEN sd.career_hrs_earned IS NULL 
                      THEN 0 ELSE SD.CAREER_HRS_EARNED END AS CAREER_HRS_EARNED, CASE WHEN sd.career_gpa IS NULL 
                      THEN 0 ELSE SD.CAREER_GPA END AS CAREER_GPA, CASE WHEN sm.trm_hrs_enrolled IS NULL 
                      THEN 0 ELSE SM.TRM_HRS_ENROLLED END AS TRM_HRS_ENROLLED, CASE WHEN sm.trm_num_of_crs IS NULL 
                      THEN 0 ELSE sm.TRM_NUM_OF_CRS END AS TRM_NUM_OF_CRS, CASE WHEN sd.trm_gpa IS NULL THEN 0 ELSE SD.TRM_GPA END AS TRM_GPA, 
                      SD.LAST_GRADE_RPT
FROM         dbo.NAME_MASTER AS NAM INNER JOIN
                      dbo.STUDENT_MASTER AS SM ON SM.ID_NUM = NAM.ID_NUM LEFT OUTER JOIN
                      dbo.DEGREE_HISTORY AS DH ON DH.ID_NUM = NAM.ID_NUM AND SM.DEGR_HIST_SEQ_NUM = DH.SEQ_NUM_2 LEFT OUTER JOIN
                      dbo.STUDENT_DIV_MAST AS SD ON SD.ID_NUM = NAM.ID_NUM AND SM.CUR_STUD_DIV = SD.DIV_CDE LEFT OUTER JOIN
                      dbo.DIVISION_DEF AS DV ON DV.DIV_CDE = SM.CUR_STUD_DIV LEFT OUTER JOIN
                      dbo.ACAD_STANDING_DEF AS HA ON HA.ACAD_STAND_CODE = SM.CUR_ACAD_HONORS LEFT OUTER JOIN
                      dbo.ACAD_STANDING_DEF AS PA ON PA.ACAD_STAND_CODE = SM.CUR_ACAD_PROBATION LEFT OUTER JOIN
                      dbo.CLASS_DEFINITION AS CD ON CD.CLASS_CDE = SM.CURRENT_CLASS_CDE LEFT OUTER JOIN
                      dbo.TABLE_DETAIL AS DG ON DG.TABLE_VALUE = DH.DEGR_CDE AND DG.COLUMN_NAME = 'degree' LEFT OUTER JOIN
                      dbo.MAJOR_MINOR_DEF AS MJ1 ON MJ1.MAJOR_CDE = DH.MAJOR_1 LEFT OUTER JOIN
                      dbo.MAJOR_MINOR_DEF AS MJ2 ON MJ2.MAJOR_CDE = DH.MAJOR_2 LEFT OUTER JOIN
                      dbo.MAJOR_MINOR_DEF AS MN1 ON MN1.MAJOR_CDE = DH.MINOR_1 LEFT OUTER JOIN
                      dbo.MAJOR_MINOR_DEF AS MN2 ON MN2.MAJOR_CDE = DH.MINOR_2 LEFT OUTER JOIN
                      dbo.CONCENTRATION_DEF AS CN1 ON CN1.CONC_CDE = DH.CONCENTRATION_1 LEFT OUTER JOIN
                      dbo.CONCENTRATION_DEF AS CN2 ON CN2.CONC_CDE = DH.CONCENTRATION_2

GO

CREATE VIEW [dbo].[CUS_vw_AdvisorListforAdviseeNotesPortlet]
AS
SELECT DISTINCT 
                      TOP (100) PERCENT AT.ADVISOR_ID, RTRIM(ANAM.LAST_NAME) + CASE WHEN ANAM.PREFERRED_NAME IS NULL THEN CASE WHEN ANAM.FIRST_NAME IS NULL 
                      THEN '' ELSE ', ' + ANAM.FIRST_NAME END ELSE ', ' + ANAM.PREFERRED_NAME END AS NAME
FROM         dbo.ADVISOR_STUD_TABLE AS AT INNER JOIN
                      dbo.NAME_MASTER AS ANAM ON ANAM.ID_NUM = AT.ADVISOR_ID INNER JOIN
                      dbo.STUDENT_MASTER AS SM ON SM.ID_NUM = AT.ID_NUM INNER JOIN
                      dbo.DEGREE_HISTORY AS DH ON DH.ID_NUM = SM.ID_NUM AND SM.DEGR_HIST_SEQ_NUM = DH.SEQ_NUM_2
WHERE     (AT.ADVISOR_TABLE_STS = 'A') AND (AT.BEGIN_DTE IS NULL OR
                      AT.BEGIN_DTE <= GETDATE()) AND (AT.END_DTE IS NULL OR
                      AT.END_DTE > GETDATE()) AND (DH.DTE_DEGR_CONFERRED IS NULL)
ORDER BY NAME

GO

CREATE VIEW [dbo].[CUS_vw_AllAdviseesForNotesPortlet]
AS
SELECT DISTINCT 
                      TOP (100) PERCENT AT.ID_NUM, NAM.LAST_NAME + ', ' + CASE WHEN NAM.PREFERRED_NAME IS NULL OR
                      RTRIM(NAM.PREFERRED_NAME) = '' THEN NAM.FIRST_NAME ELSE NAM.PREFERRED_NAME END AS ADVISEE_NAME, AT.ADVISOR_ID, 
                      ANAM.LAST_NAME + ', ' + ANAM.FIRST_NAME AS ADVISOR_NAME
FROM         dbo.ADVISOR_STUD_TABLE AS AT INNER JOIN
                      dbo.NAME_MASTER AS NAM ON NAM.ID_NUM = AT.ID_NUM INNER JOIN
                      dbo.NAME_MASTER AS ANAM ON ANAM.ID_NUM = AT.ADVISOR_ID INNER JOIN
                      dbo.STUDENT_MASTER AS SM ON SM.ID_NUM = AT.ID_NUM INNER JOIN
                      dbo.DEGREE_HISTORY AS DH ON DH.ID_NUM = SM.ID_NUM AND SM.DEGR_HIST_SEQ_NUM = DH.SEQ_NUM_2
WHERE     (AT.ADVISOR_TABLE_STS = 'A') AND (AT.BEGIN_DTE IS NULL OR
                      AT.BEGIN_DTE <= GETDATE()) AND (AT.END_DTE IS NULL OR
                      AT.END_DTE > GETDATE()) AND (DH.DTE_DEGR_CONFERRED IS NULL)
ORDER BY ADVISEE_NAME, ADVISOR_NAME

GO

CREATE VIEW [dbo].[CUS_vw_AdviseesByAdvisorForNotesPortlet]
AS
SELECT DISTINCT TOP (100) PERCENT ADVISOR_ID, ID_NUM, LAST_NAME, FIRST_NAME, MODULE_CODE
FROM         (SELECT     TOP (100) PERCENT AT.ADVISOR_ID, AT.ID_NUM, NAM.LAST_NAME, CASE WHEN NAM.PREFERRED_NAME IS NULL OR
                                              RTRIM(NAM.PREFERRED_NAME) = '' THEN NAM.FIRST_NAME ELSE NAM.PREFERRED_NAME END AS FIRST_NAME, '*AV' AS MODULE_CODE
                       FROM          dbo.ADVISOR_STUD_TABLE AS AT INNER JOIN
                                              dbo.NAME_MASTER AS NAM ON NAM.ID_NUM = AT.ID_NUM INNER JOIN
                                              dbo.STUDENT_MASTER AS SM ON SM.ID_NUM = AT.ID_NUM INNER JOIN
                                              dbo.DEGREE_HISTORY AS DH ON DH.ID_NUM = SM.ID_NUM AND SM.DEGR_HIST_SEQ_NUM = DH.SEQ_NUM_2
                       WHERE      (AT.ADVISOR_TABLE_STS = 'A') AND (AT.BEGIN_DTE IS NULL OR
                                              AT.BEGIN_DTE <= GETDATE()) AND (AT.END_DTE IS NULL OR
                                              AT.END_DTE > GETDATE()) AND (DH.DTE_DEGR_CONFERRED IS NULL)
                       UNION
                       SELECT     TOP (100) PERCENT I.TO_DO_ID_NUMBER, I.ID_NUMBER, NAM1.LAST_NAME, CASE WHEN NAM1.PREFERRED_NAME IS NULL OR
                                             RTRIM(NAM1.PREFERRED_NAME) = '' THEN NAM1.FIRST_NAME ELSE NAM1.PREFERRED_NAME END AS FIRST_NAME, I.MODULE_CODE
                       FROM         dbo.ITEMS AS I INNER JOIN
                                             dbo.NAME_MASTER AS NAM1 ON NAM1.ID_NUM = I.ID_NUMBER
                       WHERE     (I.COMPLETION_CODE <> 'C') AND (I.TO_DO_ID_NUMBER IN
                                                 (SELECT     ADVISOR_ID
                                                   FROM          dbo.ADVISOR_MASTER))) AS TEMP
ORDER BY LAST_NAME, FIRST_NAME

GO


CREATE FUNCTION [dbo].[CUS_fn_AdvisingNotesIsConcerned]
(
	@ID_NUM int, @ADVISOR_ID int
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @ISCONCERNED int
	
	SELECT @ISCONCERNED = COUNT(ID_NUM)
	from ADVISOR_STUD_TABLE
	WHERE ID_NUM IN 
	(
	SELECT ID_NUM FROM ADVISOR_STUD_TABLE
	WHERE ADVISOR_ID = @ADVISOR_ID AND ID_NUM = @ID_NUM
	AND ADVISOR_TABLE_STS = 'A'
	)
	OR ID_NUM IN
	(
	SELECT ID_NUMBER FROM ITEMS
	WHERE MODULE_CODE = 'AV' 
	AND TO_DO_ID_NUMBER = @ADVISOR_ID AND ID_NUMBER = @ID_NUM
	AND COMPLETION_CODE <> 'C'
	)
	-- Return the result of the function
	RETURN @ISCONCERNED

END

GO



CREATE USER [JICSAdvisingNotes] FOR LOGIN [JICSAdvisingNotes] WITH DEFAULT_SCHEMA=[dbo]
GO

GRANT SELECT, UPDATE, INSERT ON [dbo].[ITEMS] TO JICSAdvisingNotes
GRANT SELECT ON [dbo].[ADVISOR_STUD_TABLE] TO JICSAdvisingNotes
GRANT SELECT ON [dbo].[ADVISOR_MASTER] TO JICSAdvisingNotes
GRANT SELECT ON [dbo].[CUS_vw_NotepadUsers] TO JICSAdvisingNotes
GRANT SELECT ON [dbo].[CUS_vw_AdviseeInfoForNotesPortlet] TO JICSAdvisingNotes
GRANT SELECT ON [dbo].[CUS_vw_NotepadUsers] TO JICSAdvisingNotes
GRANT SELECT ON [dbo].[CUS_vw_AdvisorListforAdviseeNotesPortlet] TO JICSAdvisingNotes
GRANT SELECT ON [dbo].[CUS_vw_AllAdviseesForNotesPortlet] TO JICSAdvisingNotes
GRANT SELECT ON [dbo].[CUS_vw_AdviseesByAdvisorForNotesPortlet] TO JICSAdvisingNotes
GRANT EXECUTE ON [dbo].[CUS_fn_GetNextNotepadGroupNumber] TO JICSAdvisingNotes
GRANT EXECUTE ON [dbo].[CUS_fn_AdvisingNotesIsConcerned] TO JICSAdvisingNotes

GO
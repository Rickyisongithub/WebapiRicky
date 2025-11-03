// ReSharper disable InconsistentNaming
namespace KairosWebAPI.Models.ResponseResults.Login
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ReturnObj
    {
        public List<UserFile>? UserFile { get; set; }
        public List<UserComp>? UserComp { get; set; }
        public List<object>? UserCompExt { get; set; }
        public List<object>? ExtensionTables { get; set; }
    }

    public class KLoginResponseDto
    {
        public ReturnObj? returnObj { get; set; }
    }

    public class UserComp
    {
        public string? UserID { get; set; }
        public string? Company { get; set; }
        public string? Name { get; set; }
        public string? CurPlant { get; set; }
        public string? PlantList { get; set; }
        public object? SolutionID { get; set; }
        public decimal SysRevID { get; set; }
        public string? SysRowID { get; set; }
        public bool CanUpdateExpense { get; set; }
        public bool CanUpdateTime { get; set; }
        public string? EmpID { get; set; }
        public string? WorkstationID { get; set; }
        public int BitFlag { get; set; }
        public string? CompanyName { get; set; }
        public string? UserIDName { get; set; }
        public string? RowMod { get; set; }
    }

    public class UserFile
    {
        public string? UserID { get; set; }
        public string? Name { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZIP { get; set; }
        public string? Country { get; set; }
        public string? OfficePhone { get; set; }
        public string? Phone { get; set; }
        public string? EMailAddress { get; set; }
        public bool UserDisabled { get; set; }
        public bool SecurityMgr { get; set; }
        public bool CanChangeSaveSettings { get; set; }
        public bool SaveSettings { get; set; }
        public string? CurComp { get; set; }
        public DateTime PwdLastChanged { get; set; }
        public int PwdExpiresDays { get; set; }
        public object? PwdExpires { get; set; }
        public int PwdGrace { get; set; }
        public string? GroupList { get; set; }
        public string? CompList { get; set; }
        public bool ViewFavoriteBar { get; set; }
        public bool ViewStatusBar { get; set; }
        public int WinX { get; set; }
        public int WinY { get; set; }
        public int CurFolderSeq { get; set; }
        public string? CurMenuID { get; set; }
        public int WinWidth { get; set; }
        public int WinHeight { get; set; }
        public bool DispTips { get; set; }
        public bool CanChangePassword { get; set; }
        public string? LangNameID { get; set; }
        public string? LastGraphType { get; set; }
        public bool UseInternalWebBrowser { get; set; }
        public bool AllowMultipleSessions { get; set; }
        public bool WebUser { get; set; }
        public int ListViewMode { get; set; }
        public bool CanMaintainFavQueries { get; set; }
        public bool CanMaintainFavURLs { get; set; }
        public bool CanMaintainFavPrograms { get; set; }
        public bool CanCustomize { get; set; }
        public int ViewTreeOnly { get; set; }
        public int Timeout { get; set; }
        public bool CanPersonalize { get; set; }
        public bool CanTranslate { get; set; }
        public bool CanEditCompAnnotations { get; set; }
        public bool CanEditUserAnnotations { get; set; }
        public string? CanEditHelpLinks { get; set; }
        public bool AutoStartMonitor { get; set; }
        public int MonitorPollingInterval { get; set; }
        public string? FormOpenMode { get; set; }
        public string? EntConType { get; set; }
        public bool DashboardDeveloper { get; set; }
        public bool CanDesignQSearch { get; set; }
        public bool RequireSso { get; set; }
        public bool ViewStatusPanelUserID { get; set; }
        public bool ViewStatusPanelLanguage { get; set; }
        public bool ViewStatusPanelCompany { get; set; }
        public bool ViewStatusPanelPlant { get; set; }
        public bool ViewStatusPanelServer { get; set; }
        public bool ViewStatusPanelWorkstationID { get; set; }
        public string? DomainName { get; set; }
        public bool BPMAdvancedUser { get; set; }
        public int MaxGroupsFavorites { get; set; }
        public int MaxGroupsSystemMenu { get; set; }
        public string? OSUserID { get; set; }
        public bool CanTheme { get; set; }
        public string? FormatCulture { get; set; }
        public bool CanUseEntSearch { get; set; }
        public bool CanAccEpiEverywhere { get; set; }
        public bool CanAcessMobile { get; set; }
        public string? StartMenuClient { get; set; }
        public string? StartMenuEWA { get; set; }
        public string? StartMenuMobile { get; set; }
        public bool AdvBAQRights { get; set; }
        public bool SecurityAccessOnly { get; set; }
        public bool SolutionMgrCreate { get; set; }
        public bool SolutionMgrInstall { get; set; }
        public string? EntSearchURL { get; set; }
        public bool CanMaintQuickSearch { get; set; }
        public bool UseDefaultEntSearch { get; set; }
        public bool UserAccessOnly { get; set; }
        public bool GlbCompSM { get; set; }
        public bool BAQXCompany { get; set; }
        public DateTime LastLogOnAttempt { get; set; }
        public bool CanImpersonate { get; set; }
        public bool ViewStatusPanelSolutionID { get; set; }
        public bool CanMaintPredictiveSearch { get; set; }
        public object? ThemeID { get; set; }
        public bool DisableTheming { get; set; }
        public decimal SysRevID { get; set; }
        public string? SysRowID { get; set; }
        public bool CanPublishLayout { get; set; }
        public object? DefaultLayoutID { get; set; }
        public bool CanViewDocStar { get; set; }
        public object? PwdChangeRequestOn { get; set; }
        public object? ExternalIdentity { get; set; }
        public bool SSRSReportDesigner { get; set; }
        public string? DefaultHomepageLayoutID { get; set; }
        public object? AccessScopeID { get; set; }
        public bool IoTUser { get; set; }
        public bool IoTAdministrator { get; set; }
        public bool DMTUser { get; set; }
        public bool EVAUser { get; set; }
        public string? DefaultFormType { get; set; }
        public bool HideKineticToast { get; set; }
        public bool RestrictIP { get; set; }
        public bool AdvancedConfiguratorUser { get; set; }
        public bool CanOverrideAllocPart { get; set; }
        public bool ClearPassword { get; set; }
        public bool ExpirePassword { get; set; }
        public int FailedAttempts { get; set; }
        public bool IntegrationAccount { get; set; }
        public bool LockedOut { get; set; }
        public object? LockedOutUntil { get; set; }
        public string? MobilePassword { get; set; }
        public bool PasswordBlank { get; set; }
        public string? PasswordEmail { get; set; }
        public int ShpTrackerIntMinute { get; set; }
        public string? UpdateWarning { get; set; }
        public bool UseCompression { get; set; }
        public bool AllowReq { get; set; }
        public string? LangNameIDDescription { get; set; }
        public int BitFlag { get; set; }
        public string? RowMod { get; set; }
    }
}


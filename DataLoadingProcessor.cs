using ClosedXML.Excel;
using ConsoleApp1.Data;
using ConsoleApp1.Extension;
using JsonLib;
using mcpdipData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class DataLoadingProcessor
    {
        private static string maxReceiveDate = DateTime.Today.ToString("yyyyMM") + "01";
        private static string reportingPeriod = DateTime.Parse(maxReceiveDate.Substring(4, 2) + "/01/" + maxReceiveDate.Substring(0, 4)).AddDays(-1).ToString("yyyyMMdd");

        public static void ProcessExcel(string IPAName, string OperationMode, string OutboundFolder, FileInfo fi, CancellationTokenSource cts)
        {
            var workBook = new XLWorkbook(fi.FullName);
            if (IPAName == "IEHP")
            {
            }
            else if (IPAName == "Kaiser")
            {
                if (fi.Name.Contains("PCPA"))
                {
                    int sheetCounts = workBook.Worksheets.Count;
                    if (sheetCounts < 2) return;
                    var sheet = workBook.Worksheet(2);
                    string PlanCode, Cin, Npi;
                    int i = 2;
                    List<ExcelPcpa> excelPcpas = new List<ExcelPcpa>();
                    List<ExcelPcpa> validExcelPcpas = new List<ExcelPcpa>();
                    List<ExcelPcpa> errorExcelPcpas = new List<ExcelPcpa>();
                    var row = sheet.Row(2);
                    while (!string.IsNullOrEmpty(row.Cell(2).Value.ToString()))
                    {
                        PlanCode = row.Cell(1).Value.ToString();
                        Cin = row.Cell(2).Value.ToString();
                        Npi = row.Cell(3).Value.ToString();
                        ExcelPcpa excelPcpa = new ExcelPcpa
                        {
                            PlanCode = PlanCode,
                            Cin = Cin,
                            Npi = Npi,
                            ErrorMessage = ""
                        };
                        if (!string.IsNullOrEmpty(PlanCode) && !string.IsNullOrEmpty(Cin) && !string.IsNullOrEmpty(Npi))
                        {
                            excelPcpas.Add(excelPcpa);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(PlanCode))
                            {
                                excelPcpa.PlanCode = "NA";
                                excelPcpa.ErrorMessage += "Missing PlanCode~";
                            }
                            if (string.IsNullOrEmpty(Cin))
                            {
                                excelPcpa.Cin = "NA";
                                excelPcpa.ErrorMessage += "Missing CIN~";
                            }
                            if (string.IsNullOrEmpty(Npi))
                            {
                                excelPcpa.Npi = "NA";
                                excelPcpa.ErrorMessage += "Missing NPI~";
                            }
                            excelPcpa.ErrorMessage = excelPcpa.ErrorMessage.Remove(excelPcpa.ErrorMessage.Length - 1);
                            errorExcelPcpas.Add(excelPcpa);
                        }
                        i++;
                        row = sheet.Row(i);
                    }
                    //validate
                    Validate_PcpAssignment(ref cts, ref excelPcpas, ref errorExcelPcpas, ref validExcelPcpas, "Kaiser");
                    //generate response file
                    string errorFileName = Path.Combine(OutboundFolder, OperationMode, IPAName, Path.GetFileNameWithoutExtension(fi.FullName) + "_RESP." + fi.Extension);
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(errorExcelPcpas.ToDataTable());
                        wb.SaveAs(errorFileName);
                    }
                }
                else if (fi.Name.Contains("MCPD"))
                {
                    int sheetCounts = workBook.Worksheets.Count;
                    if (sheetCounts < 5) return;
                    var sheet = workBook.Worksheet(2); //grievance
                    int i = 2;
                    string PlanCode, Cin, GrievanceId, RecordType, ParentGrievanceId, GrievanceReceivedDate, GrievanceType, BenefitType, ExemptIndicator;
                    var row = sheet.Row(2);
                    McpdGrievance excelGrievance;
                    List<McpdGrievance> grievances = new List<McpdGrievance>();
                    List<McpdGrievance> errorGrievances = new List<McpdGrievance>();
                    List<McpdGrievance> validGrievances = new List<McpdGrievance>();
                    while (!string.IsNullOrEmpty(row.Cell(3).Value.ToString()))
                    {
                        PlanCode = row.Cell(1).Value.ToString();
                        Cin = row.Cell(2).Value.ToString();
                        GrievanceId = row.Cell(3).Value.ToString();
                        RecordType = row.Cell(4).Value.ToString();
                        ParentGrievanceId = row.Cell(5).Value.ToString();
                        GrievanceReceivedDate = row.Cell(6).Value.ToString();
                        GrievanceType = row.Cell(7).Value.ToString();
                        BenefitType = row.Cell(8).Value.ToString();
                        ExemptIndicator = row.Cell(9).Value.ToString();
                        excelGrievance = new McpdGrievance
                        {
                            PlanCode = PlanCode,
                            Cin = Cin,
                            GrievanceId = GrievanceId,
                            RecordType = RecordType,
                            ParentGrievanceId = ParentGrievanceId==""?null:ParentGrievanceId,
                            GrievanceReceivedDate = GrievanceReceivedDate,
                            GrievanceType = GrievanceType,
                            BenefitType = BenefitType,
                            ExemptIndicator = ExemptIndicator,
                            TradingPartnerCode = "Kaiser",
                            ErrorMessage = "",
                            DataSource = "Kaiser"
                        };
                        if (!string.IsNullOrEmpty(PlanCode) && !string.IsNullOrEmpty(Cin) && !string.IsNullOrEmpty(GrievanceId) && !string.IsNullOrEmpty(RecordType) && !string.IsNullOrEmpty(GrievanceReceivedDate) && !string.IsNullOrEmpty(GrievanceType) && !string.IsNullOrEmpty(BenefitType) && !string.IsNullOrEmpty(ExemptIndicator))
                        {
                            grievances.Add(excelGrievance);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(PlanCode))
                            {
                                excelGrievance.PlanCode = "NA";
                                excelGrievance.ErrorMessage += "Missing PlanCode~";
                            }
                            if (string.IsNullOrEmpty(Cin))
                            {
                                excelGrievance.Cin = "NA";
                                excelGrievance.ErrorMessage += "Missing CIN~";
                            }
                            if (string.IsNullOrEmpty(GrievanceId))
                            {
                                excelGrievance.GrievanceId = "NA";
                                excelGrievance.ErrorMessage += "Missing GrievanceId~";
                            }
                            if (string.IsNullOrEmpty(RecordType))
                            {
                                excelGrievance.RecordType = "NA";
                                excelGrievance.ErrorMessage += "Missing RecordType~";
                            }
                            if (string.IsNullOrEmpty(GrievanceReceivedDate))
                            {
                                excelGrievance.GrievanceReceivedDate = "NA";
                                excelGrievance.ErrorMessage += "Missing GrievanceReceivedDate~";
                            }
                            if (string.IsNullOrEmpty(GrievanceType))
                            {
                                excelGrievance.GrievanceType = "NA";
                                excelGrievance.ErrorMessage += "Missing GrievanceType~";
                            }
                            if (string.IsNullOrEmpty(BenefitType))
                            {
                                excelGrievance.BenefitType = "NA";
                                excelGrievance.ErrorMessage += "Missing BenefitType~";
                            }
                            excelGrievance.ErrorMessage.Remove(excelGrievance.ErrorMessage.Length - 1);
                            errorGrievances.Add(excelGrievance);
                        }
                        i++;
                        row = sheet.Row(i);
                    }
                    sheet = workBook.Worksheet(3); //appeal
                    i = 2;
                    string AppealId, ParentAppealId, AppealReceivedDate, NoticeOfActionDate, AppealType, AppealResolutionStatusIndicator, AppealResolutionDate, PartiallyOverturnIndicator, ExpeditedIndicator;
                    List<McpdAppeal> appeals = new List<McpdAppeal>();
                    List<McpdAppeal> errorAppeals = new List<McpdAppeal>();
                    List<McpdAppeal> validAppeals = new List<McpdAppeal>();
                    McpdAppeal excelAppeal;
                    row = sheet.Row(2);
                    while (!string.IsNullOrEmpty(row.Cell(3).Value.ToString().Trim()))
                    {
                        PlanCode = row.Cell(1).Value.ToString();
                        Cin = row.Cell(2).Value.ToString();
                        AppealId = row.Cell(3).Value.ToString();
                        RecordType = row.Cell(4).Value.ToString();
                        ParentGrievanceId = row.Cell(5).Value.ToString();
                        ParentAppealId = row.Cell(6).Value.ToString();
                        AppealReceivedDate = row.Cell(7).Value.ToString();
                        NoticeOfActionDate = row.Cell(8).Value.ToString();
                        AppealType = row.Cell(9).Value.ToString();
                        BenefitType = row.Cell(10).Value.ToString();
                        AppealResolutionStatusIndicator = row.Cell(11).Value.ToString();
                        AppealResolutionDate = row.Cell(12).Value.ToString();
                        PartiallyOverturnIndicator = row.Cell(13).Value.ToString();
                        ExpeditedIndicator = row.Cell(14).Value.ToString();
                        excelAppeal = new McpdAppeal
                        {
                            PlanCode = PlanCode,
                            Cin = Cin,
                            AppealId = AppealId,
                            RecordType = RecordType,
                            ParentGrievanceId = ParentGrievanceId==""?null:ParentGrievanceId ,
                            ParentAppealId = ParentAppealId==""?null:ParentAppealId,
                            AppealReceivedDate = AppealReceivedDate,
                            NoticeOfActionDate = NoticeOfActionDate==""?null:NoticeOfActionDate,
                            AppealType = AppealType,
                            BenefitType = BenefitType,
                            AppealResolutionStatusIndicator = AppealResolutionStatusIndicator,
                            AppealResolutionDate = AppealResolutionDate==""?null:AppealResolutionDate,
                            PartiallyOverturnIndicator = PartiallyOverturnIndicator,
                            ExpeditedIndicator = ExpeditedIndicator,
                            TradingPartnerCode = "Kaiser",
                            ErrorMessage = "",
                            DataSource = "Kaiser"
                        };
                        if (!string.IsNullOrEmpty(PlanCode) && !string.IsNullOrEmpty(Cin) && !string.IsNullOrEmpty(AppealId) && !string.IsNullOrEmpty(RecordType) && !string.IsNullOrEmpty(AppealReceivedDate) && !string.IsNullOrEmpty(AppealType) && !string.IsNullOrEmpty(BenefitType) && !string.IsNullOrEmpty(AppealResolutionStatusIndicator) && !string.IsNullOrEmpty(PartiallyOverturnIndicator) && !string.IsNullOrEmpty(ExpeditedIndicator))
                        {
                            appeals.Add(excelAppeal);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(PlanCode))
                            {
                                excelAppeal.PlanCode = "NA";
                                excelAppeal.ErrorMessage += "Missing PlanCode~";
                            }
                            if (string.IsNullOrEmpty(Cin))
                            {
                                excelAppeal.Cin = "NA";
                                excelAppeal.ErrorMessage += "Missing CIN~";
                            }
                            if (string.IsNullOrEmpty(AppealId))
                            {
                                excelAppeal.AppealId = "NA";
                                excelAppeal.ErrorMessage += "Missing AppealId~";
                            }
                            if (string.IsNullOrEmpty(RecordType))
                            {
                                excelAppeal.RecordType = "NA";
                                excelAppeal.ErrorMessage += "Missing RecordType~";
                            }
                            if (string.IsNullOrEmpty(AppealReceivedDate))
                            {
                                excelAppeal.AppealReceivedDate = "NA";
                                excelAppeal.ErrorMessage += "Missing AppealReceivedDate~";
                            }
                            if (string.IsNullOrEmpty(AppealType))
                            {
                                excelAppeal.AppealType = "NA";
                                excelAppeal.ErrorMessage += "Missing AppealType~";
                            }
                            if (string.IsNullOrEmpty(BenefitType))
                            {
                                excelAppeal.BenefitType = "NA";
                                excelAppeal.ErrorMessage += "Missing BenefitType~";
                            }
                            if (string.IsNullOrEmpty(AppealResolutionStatusIndicator))
                            {
                                excelAppeal.AppealResolutionStatusIndicator = "NA";
                                excelAppeal.ErrorMessage += "Missing AppealResolutionStatusIndicator~";
                            }
                            if (string.IsNullOrEmpty(PartiallyOverturnIndicator))
                            {
                                excelAppeal.PartiallyOverturnIndicator = "NA";
                                excelAppeal.ErrorMessage += "Missing PArtiallyOverturnIndicator~";
                            }
                            if (string.IsNullOrEmpty(ExpeditedIndicator))
                            {
                                excelAppeal.ExpeditedIndicator = "NA";
                                excelAppeal.ErrorMessage += "Missing ExpeditedIndicator~";
                            }
                            excelAppeal.ErrorMessage = excelAppeal.ErrorMessage.Remove(excelAppeal.ErrorMessage.Length - 1);
                            errorAppeals.Add(excelAppeal);
                        }
                        i++;
                        row = sheet.Row(i);
                    }
                    sheet = workBook.Worksheet(4); //coc
                    i = 2;
                    string CocId, ParentCocId, CocReceivedDate, CocType, CocDispositionIndicator, CocExpirationDate, CocDenialReasonIndicator, SubmittingProviderNpi, CocProviderNpi, ProviderTaxonomy, MerExemptionId, ExemptionToEnrollmentDenialCode, ExemptionToEnrollmentDenialDate, MerCocDispositionIndicator, MerCocDispositionDate, ReasonMerCocNotMetIndicator;

                    List<McpdContinuityOfCare> cocs = new List<McpdContinuityOfCare>();
                    List<McpdContinuityOfCare> errorCocs = new List<McpdContinuityOfCare>();
                    List<McpdContinuityOfCare> validCocs = new List<McpdContinuityOfCare>();
                    McpdContinuityOfCare excelCoc;
                    row = sheet.Row(2);
                    while (!string.IsNullOrEmpty(row.Cell(3).Value.ToString()))
                    {
                        PlanCode = row.Cell(1).Value.ToString();
                        Cin = row.Cell(2).Value.ToString();
                        CocId = row.Cell(3).Value.ToString();
                        RecordType = row.Cell(4).Value.ToString();
                        ParentCocId = row.Cell(5).Value.ToString();
                        CocReceivedDate = row.Cell(6).Value.ToString();
                        CocType = row.Cell(7).Value.ToString();
                        BenefitType = row.Cell(8).Value.ToString();
                        CocDispositionIndicator = row.Cell(9).Value.ToString();
                        CocExpirationDate = row.Cell(10).Value.ToString();
                        CocDenialReasonIndicator = row.Cell(11).Value.ToString();
                        SubmittingProviderNpi = row.Cell(12).Value.ToString();
                        CocProviderNpi = row.Cell(13).Value.ToString();
                        ProviderTaxonomy = row.Cell(14).Value.ToString();
                        MerExemptionId = row.Cell(15).Value.ToString();
                        ExemptionToEnrollmentDenialCode = row.Cell(16).Value.ToString();
                        ExemptionToEnrollmentDenialDate = row.Cell(17).Value.ToString();
                        MerCocDispositionIndicator = row.Cell(18).Value.ToString();
                        MerCocDispositionDate = row.Cell(19).Value.ToString();
                        ReasonMerCocNotMetIndicator = row.Cell(20).Value.ToString();

                        excelCoc = new McpdContinuityOfCare
                        {
                            PlanCode = PlanCode,
                            Cin = Cin,
                            CocId = CocId,
                            RecordType = RecordType,
                            ParentCocId = ParentCocId == "" ? null : ParentCocId,
                            CocReceivedDate = CocReceivedDate,
                            CocType = CocType,
                            BenefitType = BenefitType,
                            CocDispositionIndicator = CocDispositionIndicator,
                            CocExpirationDate = CocExpirationDate == "" ? null : CocExpirationDate,
                            CocDenialReasonIndicator = CocDenialReasonIndicator == "" ? null : CocDenialReasonIndicator,
                            SubmittingProviderNpi = SubmittingProviderNpi == "" ? null : SubmittingProviderNpi,
                            CocProviderNpi = CocProviderNpi == "" ? null : CocProviderNpi,
                            ProviderTaxonomy = ProviderTaxonomy == "" ? null : ProviderTaxonomy,
                            MerExemptionId = MerExemptionId==""?null:MerExemptionId,
                            ExemptionToEnrollmentDenialCode=ExemptionToEnrollmentDenialCode==""?null:ExemptionToEnrollmentDenialCode,
                            ExemptionToEnrollmentDenialDate=ExemptionToEnrollmentDenialDate==""?null:ExemptionToEnrollmentDenialDate,
                            MerCocDispositionIndicator=MerCocDispositionIndicator==""?null:MerCocDispositionIndicator,
                            MerCocDispositionDate=MerCocDispositionDate==""?null:MerCocDispositionDate,
                            ReasonMerCocNotMetIndicator=ReasonMerCocNotMetIndicator==""?null:ReasonMerCocNotMetIndicator,
                            ErrorMessage = ""
                        };
                        if (!string.IsNullOrEmpty(PlanCode) && !string.IsNullOrEmpty(Cin) && !string.IsNullOrEmpty(CocId) && !string.IsNullOrEmpty(RecordType) && !string.IsNullOrEmpty(CocReceivedDate) && !string.IsNullOrEmpty(BenefitType) && !string.IsNullOrEmpty(CocDispositionIndicator))
                        {
                            cocs.Add(excelCoc);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(PlanCode))
                            {
                                excelCoc.PlanCode = "NA";
                                excelCoc.ErrorMessage += "Missing PlanCode~";
                            }
                            if (string.IsNullOrEmpty(Cin))
                            {
                                excelCoc.Cin = "NA";
                                excelCoc.ErrorMessage += "Missing Cin~";
                            }
                            if (string.IsNullOrEmpty(CocId))
                            {
                                excelCoc.CocId = "NA";
                                excelCoc.ErrorMessage += "Missing CocId~";
                            }
                            if (string.IsNullOrEmpty(RecordType))
                            {
                                excelCoc.RecordType = "NA";
                                excelCoc.ErrorMessage += "Missing RecordType~";
                            }
                            if (string.IsNullOrEmpty(CocReceivedDate))
                            {
                                excelCoc.CocReceivedDate = "NA";
                                excelCoc.ErrorMessage += "Missing CocReceivedDate~";
                            }
                            if (string.IsNullOrEmpty(BenefitType))
                            {
                                excelCoc.BenefitType = "NA";
                                excelCoc.ErrorMessage += "Missing BenefitType~";
                            }
                            if (string.IsNullOrEmpty(CocDispositionIndicator))
                            {
                                excelCoc.CocDispositionIndicator = "NA";
                                excelCoc.ErrorMessage += "Missing CocDispositionIndicator~";
                            }
                            excelCoc.ErrorMessage = excelCoc.ErrorMessage.Remove(excelCoc.ErrorMessage.Length - 1);
                            errorCocs.Add(excelCoc);
                        }
                        i++;
                        row = sheet.Row(i);
                    }

                    sheet = workBook.Worksheet(5); //oon
                    i = 2;
                    string OonId, ParentOonId, OonRequestReceivedDate, ReferralRequestReasonIndicator
                            , OonResolutionStatusIndicator, OonRequestResolvedDate, PartialApprovalExplanation
                            , SpecialistProviderNpi, ServiceLocationAddressLine1
                            , ServiceLocationAddressLine2, ServiceLocationCity, ServiceLocationState
                            , ServiceLocationZip, ServiceLocationCountry;
                    List<McpdOutOfNetwork> oons = new List<McpdOutOfNetwork>();
                    List<McpdOutOfNetwork> errorOons = new List<McpdOutOfNetwork>();
                    List<McpdOutOfNetwork> validOons = new List<McpdOutOfNetwork>();
                    McpdOutOfNetwork excelOon;
                    row = sheet.Row(2);
                    while (!string.IsNullOrEmpty(row.Cell(3).Value.ToString()))
                    {
                        PlanCode = row.Cell(1).Value.ToString();
                        Cin = row.Cell(2).Value.ToString();
                        OonId = row.Cell(3).Value.ToString();
                        RecordType = row.Cell(4).Value.ToString();
                        ParentOonId = row.Cell(5).Value.ToString();
                        OonRequestReceivedDate = row.Cell(6).Value.ToString();
                        ReferralRequestReasonIndicator = row.Cell(7).Value.ToString();
                        OonResolutionStatusIndicator = row.Cell(8).Value.ToString();
                        OonRequestResolvedDate = row.Cell(9).Value.ToString();
                        PartialApprovalExplanation = row.Cell(10).Value.ToString();
                        SpecialistProviderNpi = row.Cell(11).Value.ToString();
                        ProviderTaxonomy = row.Cell(12).Value.ToString();
                        ServiceLocationAddressLine1 = row.Cell(13).Value.ToString();
                        ServiceLocationAddressLine2 = row.Cell(14).Value.ToString();
                        ServiceLocationCity = row.Cell(15).Value.ToString();
                        ServiceLocationState = row.Cell(16).Value.ToString();
                        ServiceLocationZip = row.Cell(17).Value.ToString();
                        ServiceLocationCountry = row.Cell(18).Value.ToString();
                        excelOon = new McpdOutOfNetwork
                        {
                            PlanCode = PlanCode,
                            Cin = Cin,
                            OonId = OonId,
                            RecordType = RecordType,
                            ParentOonId = ParentOonId == "" ? null : ParentOonId,
                            OonRequestReceivedDate = OonRequestReceivedDate,
                            ReferralRequestReasonIndicator = ReferralRequestReasonIndicator.MemberPreference(),
                            OonResolutionStatusIndicator = OonResolutionStatusIndicator == "" ? null : OonResolutionStatusIndicator.AllFirstChacUpper(),
                            OonRequestResolvedDate = OonRequestResolvedDate == "" ? null : OonRequestResolvedDate,
                            PartialApprovalExplanation = PartialApprovalExplanation == "" ? null : PartialApprovalExplanation,
                            SpecialistProviderNpi = SpecialistProviderNpi,
                            ProviderTaxonomy = ProviderTaxonomy,
                            ServiceLocationAddressLine1 = ServiceLocationAddressLine1,
                            ServiceLocationAddressLine2 = ServiceLocationAddressLine2 == "" ? null : ServiceLocationAddressLine2,
                            ServiceLocationCity = ServiceLocationCity,
                            ServiceLocationState = ServiceLocationState == "" ? null : ServiceLocationState,
                            ServiceLocationZip = ServiceLocationZip == "" ? null : ServiceLocationZip,
                            ServiceLocationCountry = ServiceLocationCountry,
                            ErrorMessage=""
                        };
                        if (!string.IsNullOrEmpty(PlanCode) && !string.IsNullOrEmpty(Cin) && !string.IsNullOrEmpty(OonId) && !string.IsNullOrEmpty(RecordType) && !string.IsNullOrEmpty(OonRequestReceivedDate) && !string.IsNullOrEmpty(ReferralRequestReasonIndicator) && !string.IsNullOrEmpty(OonResolutionStatusIndicator) && !string.IsNullOrEmpty(SpecialistProviderNpi) && !string.IsNullOrEmpty(ProviderTaxonomy) && !string.IsNullOrEmpty(ServiceLocationAddressLine1) && !string.IsNullOrEmpty(ServiceLocationCity))
                        {
                            oons.Add(excelOon);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(PlanCode))
                            {
                                excelOon.PlanCode = "NA";
                                excelOon.ErrorMessage += "Missing PlanCode~";
                            }
                            if (string.IsNullOrEmpty(Cin))
                            {
                                excelOon.Cin = "NA";
                                excelOon.ErrorMessage += "Missing Cin~";
                            }
                            if (string.IsNullOrEmpty(OonId))
                            {
                                excelOon.OonId = "NA";
                                excelOon.ErrorMessage += "Missing OonId~";
                            }
                            if (string.IsNullOrEmpty(RecordType))
                            {
                                excelOon.RecordType = "NA";
                                excelOon.ErrorMessage += "Missing RecordType~";
                            }
                            if (string.IsNullOrEmpty(OonRequestReceivedDate))
                            {
                                excelOon.OonRequestReceivedDate = "NA";
                                excelOon.ErrorMessage += "Missing OonRequestReceivedDate~";
                            }
                            if (string.IsNullOrEmpty(ReferralRequestReasonIndicator))
                            {
                                excelOon.ReferralRequestReasonIndicator = "NA";
                                excelOon.ErrorMessage += "Missing ReferralRequestReasonIndicator~";
                            }
                            if (string.IsNullOrEmpty(OonResolutionStatusIndicator))
                            {
                                excelOon.OonResolutionStatusIndicator = "NA";
                                excelOon.ErrorMessage += "Missing OonResolutionStatusIndicator~";
                            }
                            if (string.IsNullOrEmpty(SpecialistProviderNpi))
                            {
                                excelOon.SpecialistProviderNpi = "NA";
                                excelOon.ErrorMessage += "Missing SpecialistProviderNpi~";
                            }
                            if (string.IsNullOrEmpty(ProviderTaxonomy))
                            {
                                excelOon.ProviderTaxonomy = "NA";
                                excelOon.ErrorMessage += "Missing ProviderTaxonomy~";
                            }
                            if (string.IsNullOrEmpty(ServiceLocationAddressLine1))
                            {
                                excelOon.ServiceLocationAddressLine1 = "NA";
                                excelOon.ErrorMessage += "Missing ServiceLocationAddressLine1~";
                            }
                            if (string.IsNullOrEmpty(ServiceLocationCity))
                            {
                                excelOon.ServiceLocationCity = "NA";
                                excelOon.ErrorMessage += "Missing ServiceLocationCity~";
                            }
                            excelOon.ErrorMessage = excelOon.ErrorMessage.Remove(excelOon.ErrorMessage.Length - 1);
                            errorOons.Add(excelOon);
                        }
                        i++;
                        row = sheet.Row(i);
                    }
                    //validate
                    Validate_Grievance(ref cts, ref grievances, ref validGrievances, ref errorGrievances, "Kaiser");
                    Validate_Appeal(ref cts, ref appeals, ref validAppeals, ref errorAppeals, "Kaiser");
                    Validate_Coc(ref cts, ref cocs, ref validCocs, ref errorCocs, "Kaiser");
                    Validate_Oon(ref cts, ref oons, ref validOons, ref errorOons, "Kaiser");
                    
                    //generate response file
                    string errorFileName = Path.Combine(OutboundFolder, OperationMode, IPAName, Path.GetFileNameWithoutExtension(fi.FullName) + "_RESP." + fi.Extension);
                    List<ExcelGrievance> errorGrievanceKaiser = errorGrievances.Select(x => new ExcelGrievance
                    {
                        PlanCode=x.PlanCode,
                        Cin=x.Cin,
                        GrievanceId=x.GrievanceId,
                        RecordType=x.RecordType,
                        ParentGrievanceId=x.ParentGrievanceId,
                        GrievanceReceivedDate=x.GrievanceReceivedDate,
                        GrievanceType=x.GrievanceType,
                        BenefitType=x.BenefitType,
                        ExemptIndicator=x.ExemptIndicator,
                        ErrorMessage=x.ErrorMessage 
                    }).ToList();
                    List<ExcelAppeal> errorAppealKaiser = errorAppeals.Select(x => new ExcelAppeal
                    {
                        PlanCode=x.PlanCode,
                        Cin=x.Cin,
                        AppealId=x.AppealId,
                        RecordType=x.RecordType,
                        ParentGrievanceId=x.ParentGrievanceId,
                        ParentAppealId=x.ParentAppealId,
                        AppealReceivedDate=x.AppealReceivedDate,
                        NoticeOfActionDate=x.NoticeOfActionDate,
                        AppealType=x.AppealType,
                        BenefitType=x.BenefitType,
                        AppealResolutionStatusIndicator=x.AppealResolutionStatusIndicator,
                        AppealResolutionDate=x.AppealResolutionDate,
                        PartiallyOverturnIndicator=x.PartiallyOverturnIndicator,
                        ExpeditedIndicator=x.ExpeditedIndicator,
                        ErrorMessage=x.ErrorMessage 
                    }).ToList();
                    List<ExcelCoc> errorCocKaiser = errorCocs.Select(x => new ExcelCoc
                    {
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        CocId = x.CocId,
                        RecordType = x.RecordType,
                        ParentCocId = x.ParentCocId,
                        CocReceivedDate = x.CocReceivedDate,
                        CocType = x.CocType,
                        BenefitType = x.BenefitType,
                        CocDispositionIndicator = x.CocDispositionIndicator,
                        CocExpirationDate = x.CocExpirationDate,
                        CocDenialReasonIndicator = x.CocDenialReasonIndicator,
                        SubmittingProviderNpi = x.SubmittingProviderNpi,
                        CocProviderNpi = x.CocProviderNpi,
                        ProviderTaxonomy = x.ProviderTaxonomy,
                        MerExemptionId = x.MerExemptionId,
                        ExemptionToEnrollmentDenialCode=x.ExemptionToEnrollmentDenialCode,
                        ExemptionToEnrollmentDenialDate=x.ExemptionToEnrollmentDenialDate,
                        MerCocDispositionIndicator=x.MerCocDispositionIndicator,
                        MerCocDispositionDate=x.MerCocDispositionDate,
                        ReasonMerCocNotMetIndicator=x.ReasonMerCocNotMetIndicator,
                        ErrorMessage = x.ErrorMessage
                    }).ToList();
                    List<ExcelOon> errorOonKaiser = errorOons.Select(x => new ExcelOon
                    {
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        OonId = x.OonId,
                        RecordType = x.RecordType,
                        ParentOonId = x.ParentOonId,
                        OonRequestReceivedDate = x.OonRequestReceivedDate,
                        ReferralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                        OonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                        OonRequestResolvedDate = x.OonRequestResolvedDate,
                        PartialApprovalExplanation = x.PartialApprovalExplanation,
                        SpecialistProviderNpi = x.SpecialistProviderNpi,
                        ProviderTaxonomy = x.ProviderTaxonomy,
                        ServiceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                        ServiceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                        ServiceLocationCity = x.ServiceLocationCity,
                        ServiceLocationState = x.ServiceLocationState,
                        ServiceLocationZip = x.ServiceLocationZip,
                        ServiceLocationCountry = x.ServiceLocationCountry,
                        ErrorMessage = x.ErrorMessage
                    }).ToList();
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(errorGrievanceKaiser.ToDataTable());
                        wb.Worksheets.Add(errorAppealKaiser.ToDataTable());
                        wb.Worksheets.Add(errorCocKaiser.ToDataTable());
                        wb.Worksheets.Add(errorOonKaiser.ToDataTable());
                        wb.SaveAs(errorFileName);
                    }
                }
            }
            else
            {
                int sheetCounts = workBook.Worksheets.Count;
                if (sheetCounts < 4) return;
                var sheet = workBook.Worksheet(2);
                var row = sheet.Row(2);
                string IPACode = row.Cell(2).Value.ToString();
                int i = 6;
                string Cin, CocId, RecordType, ParentCocId, CocReceivedDate, CocType, BenefitType, CocDispositionIndicator, CocExpirationDate, CocDenialReasonIndicator, SubmittingProviderNpi, CocProviderNpi, ProviderTaxonomy;
                List<McpdContinuityOfCare> cocs = new List<McpdContinuityOfCare>();
                List<McpdContinuityOfCare> validCocs = new List<McpdContinuityOfCare>();
                List<McpdContinuityOfCare> errorCocs = new List<McpdContinuityOfCare>();
                McpdContinuityOfCare excelCoc;
                row = sheet.Row(6);
                while (!string.IsNullOrEmpty(row.Cell(3).Value.ToString()) && row.Cell(3).Value.ToString() != "NTR")
                {
                    Cin = row.Cell(2).Value.ToString();
                    CocId = row.Cell(3).Value.ToString();
                    RecordType = row.Cell(4).Value.ToString();
                    ParentCocId = row.Cell(5).Value.ToString();
                    CocReceivedDate = row.Cell(6).Value.ToString();
                    CocType = row.Cell(7).Value.ToString();
                    BenefitType = row.Cell(8).Value.ToString();
                    CocDispositionIndicator = row.Cell(9).Value.ToString();
                    CocExpirationDate = row.Cell(10).Value.ToString();
                    CocDenialReasonIndicator = row.Cell(11).Value.ToString();
                    SubmittingProviderNpi = row.Cell(12).Value.ToString();
                    CocProviderNpi = row.Cell(13).Value.ToString();
                    ProviderTaxonomy = row.Cell(14).Value.ToString();
                    excelCoc = new McpdContinuityOfCare
                    {
                        PlanCode = "305",
                        Cin = Cin,
                        CocId = CocId,
                        RecordType = RecordType,
                        ParentCocId = ParentCocId == "" ? null : ParentCocId,
                        CocReceivedDate = CocReceivedDate,
                        CocType = CocType,
                        BenefitType = BenefitType,
                        CocDispositionIndicator = CocDispositionIndicator,
                        CocExpirationDate = CocExpirationDate == "" ? null : CocExpirationDate,
                        CocDenialReasonIndicator = CocDenialReasonIndicator == "" ? null : CocDenialReasonIndicator,
                        SubmittingProviderNpi = SubmittingProviderNpi == "" ? null : SubmittingProviderNpi,
                        CocProviderNpi = CocProviderNpi == "" ? null : CocProviderNpi,
                        ProviderTaxonomy = ProviderTaxonomy == "" ? null : ProviderTaxonomy,
                        TradingPartnerCode = row.Cell(15).Value.ToString()
                    };
                    if (!string.IsNullOrEmpty(Cin) && !string.IsNullOrEmpty(CocId) && !string.IsNullOrEmpty(RecordType) && !string.IsNullOrEmpty(CocReceivedDate) && !string.IsNullOrEmpty(BenefitType) && !string.IsNullOrEmpty(CocDispositionIndicator))
                    {
                        cocs.Add(excelCoc);
                    }
                    else
                    {
                        excelCoc.ErrorMessage = "";
                        if (string.IsNullOrEmpty(Cin))
                        {
                            excelCoc.Cin = "NA";
                            excelCoc.ErrorMessage += "Missing Cin~";
                        }
                        if (string.IsNullOrEmpty(CocId))
                        {
                            excelCoc.CocId = "NA";
                            excelCoc.ErrorMessage += "Missing CocId~";
                        }
                        if (string.IsNullOrEmpty(RecordType))
                        {
                            excelCoc.RecordType = "NA";
                            excelCoc.ErrorMessage += "Missing RecordType~";
                        }
                        if (string.IsNullOrEmpty(CocReceivedDate))
                        {
                            excelCoc.CocReceivedDate = "NA";
                            excelCoc.ErrorMessage += "Missing CocReceivedDate~";
                        }
                        if (string.IsNullOrEmpty(BenefitType))
                        {
                            excelCoc.BenefitType = "NA";
                            excelCoc.ErrorMessage += "Missing BenefitType~";
                        }
                        if (string.IsNullOrEmpty(CocDispositionIndicator))
                        {
                            excelCoc.CocDispositionIndicator = "NA";
                            excelCoc.ErrorMessage += "Missing CocDispositionIndicator~";
                        }
                        excelCoc.ErrorMessage = excelCoc.ErrorMessage.Remove(excelCoc.ErrorMessage.Length - 1);
                        errorCocs.Add(excelCoc);
                    }
                    i++;
                    row = sheet.Row(i);
                }

                sheet = workBook.Worksheet(4);
                i = 6;
                string OonId, ParentOonId, OonRequestReceivedDate, ReferralRequestReasonIndicator
                        , OonResolutionStatusIndicator, OonRequestResolvedDate, PartialApprovalExplanation
                        , SpecialistProviderNpi, ServiceLocationAddressLine1
                        , ServiceLocationAddressLine2, ServiceLocationCity, ServiceLocationState
                        , ServiceLocationZip, ServiceLocationCountry;
                List<McpdOutOfNetwork> oons = new List<McpdOutOfNetwork>();
                List<McpdOutOfNetwork> validOons = new List<McpdOutOfNetwork>();
                List<McpdOutOfNetwork> errorOons = new List<McpdOutOfNetwork>();
                McpdOutOfNetwork excelOon;
                row = sheet.Row(6);
                while (!string.IsNullOrEmpty(row.Cell(3).Value.ToString()) && row.Cell(3).Value.ToString() != "NTR")
                {
                    Cin = row.Cell(2).Value.ToString();
                    OonId = row.Cell(3).Value.ToString();
                    RecordType = row.Cell(4).Value.ToString();
                    ParentOonId = row.Cell(5).Value.ToString();
                    OonRequestReceivedDate = row.Cell(6).Value.ToString();
                    ReferralRequestReasonIndicator = row.Cell(7).Value.ToString();
                    OonResolutionStatusIndicator = row.Cell(8).Value.ToString();
                    OonRequestResolvedDate = row.Cell(9).Value.ToString();
                    PartialApprovalExplanation = row.Cell(10).Value.ToString();
                    SpecialistProviderNpi = row.Cell(11).Value.ToString();
                    ProviderTaxonomy = row.Cell(12).Value.ToString();
                    ServiceLocationAddressLine1 = row.Cell(13).Value.ToString();
                    ServiceLocationAddressLine2 = row.Cell(14).Value.ToString();
                    ServiceLocationCity = row.Cell(15).Value.ToString();
                    ServiceLocationState = row.Cell(16).Value.ToString();
                    ServiceLocationZip = row.Cell(17).Value.ToString();
                    ServiceLocationCountry = row.Cell(18).Value.ToString();
                    excelOon = new McpdOutOfNetwork
                    {
                        PlanCode = "305",
                        Cin = Cin,
                        OonId = OonId,
                        RecordType = RecordType,
                        ParentOonId = ParentOonId == "" ? null : ParentOonId,
                        OonRequestReceivedDate = OonRequestReceivedDate,
                        ReferralRequestReasonIndicator = ReferralRequestReasonIndicator.MemberPreference(),
                        OonResolutionStatusIndicator = OonResolutionStatusIndicator == "" ? null : OonResolutionStatusIndicator.AllFirstChacUpper(),
                        OonRequestResolvedDate = OonRequestResolvedDate == "" ? null : OonRequestResolvedDate,
                        PartialApprovalExplanation = PartialApprovalExplanation == "" ? null : PartialApprovalExplanation,
                        SpecialistProviderNpi = SpecialistProviderNpi,
                        ProviderTaxonomy = ProviderTaxonomy,
                        ServiceLocationAddressLine1 = ServiceLocationAddressLine1,
                        ServiceLocationAddressLine2 = ServiceLocationAddressLine2 == "" ? null : ServiceLocationAddressLine2,
                        ServiceLocationCity = ServiceLocationCity,
                        ServiceLocationState = ServiceLocationState == "" ? null : ServiceLocationState,
                        ServiceLocationZip = ServiceLocationZip == "" ? null : ServiceLocationZip,
                        ServiceLocationCountry = "US",
                        TradingPartnerCode = row.Cell(19).Value.ToString()
                    };
                    if (!string.IsNullOrEmpty(Cin) && !string.IsNullOrEmpty(OonId) && !string.IsNullOrEmpty(RecordType) && !string.IsNullOrEmpty(OonRequestReceivedDate) && !string.IsNullOrEmpty(ReferralRequestReasonIndicator) && !string.IsNullOrEmpty(OonResolutionStatusIndicator) && !string.IsNullOrEmpty(SpecialistProviderNpi) && !string.IsNullOrEmpty(ProviderTaxonomy) && !string.IsNullOrEmpty(ServiceLocationAddressLine1) && !string.IsNullOrEmpty(ServiceLocationCity))
                    {
                        oons.Add(excelOon);
                    }
                    else
                    {
                        excelOon.ErrorMessage = "";
                        if (string.IsNullOrEmpty(Cin))
                        {
                            excelOon.Cin = "NA";
                            excelOon.ErrorMessage += "Missing Cin~";
                        }
                        if (string.IsNullOrEmpty(OonId))
                        {
                            excelOon.OonId = "NA";
                            excelOon.ErrorMessage += "Missing OonId~";
                        }
                        if (string.IsNullOrEmpty(RecordType))
                        {
                            excelOon.RecordType = "NA";
                            excelOon.ErrorMessage += "Missing RecordType~";
                        }
                        if (string.IsNullOrEmpty(OonRequestReceivedDate))
                        {
                            excelOon.OonRequestReceivedDate = "NA";
                            excelOon.ErrorMessage += "Missing OonRequestReceivedDate~";
                        }
                        if (string.IsNullOrEmpty(ReferralRequestReasonIndicator))
                        {
                            excelOon.ReferralRequestReasonIndicator = "NA";
                            excelOon.ErrorMessage += "Missing ReferralRequestReasonIndicator~";
                        }
                        if (string.IsNullOrEmpty(OonResolutionStatusIndicator))
                        {
                            excelOon.OonResolutionStatusIndicator = "NA";
                            excelOon.ErrorMessage += "Missing OonResolutionStatusIndicator~";
                        }
                        if (string.IsNullOrEmpty(SpecialistProviderNpi))
                        {
                            excelOon.SpecialistProviderNpi = "NA";
                            excelOon.ErrorMessage += "Missing SpecialistProviderNpi~";
                        }
                        if (string.IsNullOrEmpty(ProviderTaxonomy))
                        {
                            excelOon.ProviderTaxonomy = "NA";
                            excelOon.ErrorMessage += "Missing ProviderTaxonomy~";
                        }
                        if (string.IsNullOrEmpty(ServiceLocationAddressLine1))
                        {
                            excelOon.ServiceLocationAddressLine1 = "NA";
                            excelOon.ErrorMessage += "Missing ServiceLocationAddressLine1~";
                        }
                        if (string.IsNullOrEmpty(ServiceLocationCity))
                        {
                            excelOon.ServiceLocationCity = "NA";
                            excelOon.ErrorMessage += "Missing ServiceLocationCity~";
                        }
                        excelOon.ErrorMessage = excelOon.ErrorMessage.Remove(excelOon.ErrorMessage.Length - 1);
                        errorOons.Add(excelOon);
                    }
                    i++;
                    row = sheet.Row(i);
                }
                //validate
                Validate_Coc(ref cts, ref cocs, ref validCocs, ref errorCocs, IPAName);
                Validate_Oon(ref cts, ref oons, ref validOons, ref errorOons, IPAName);
                //generate response file
                string errorFileName = Path.Combine(OutboundFolder, OperationMode, IPAName, Path.GetFileNameWithoutExtension(fi.FullName) + "_RESP." + fi.Extension);
                List<ExcelCocIpa> errorCocIpas = errorCocs.Select(x => new ExcelCocIpa
                {
                    PlanCode = x.PlanCode,
                    Cin = x.Cin,
                    CocId = x.CocId,
                    RecordType = x.RecordType,
                    ParentCocId = x.ParentCocId,
                    CocReceivedDate = x.CocReceivedDate,
                    CocType = x.CocType,
                    BenefitType = x.BenefitType,
                    CocDispositionIndicator = x.CocDispositionIndicator,
                    CocExpirationDate = x.CocExpirationDate,
                    CocDenialReasonIndicator = x.CocDenialReasonIndicator,
                    SubmittingProviderNpi = x.SubmittingProviderNpi,
                    CocProviderNpi = x.CocProviderNpi,
                    ProviderTaxonomy = x.ProviderTaxonomy,
                    ExtractionDate = x.TradingPartnerCode,
                    ErrorMessage = x.ErrorMessage
                }).ToList();
                List<ExcelOon> errorOonIpas = errorOons.Select(x => new ExcelOon
                {
                    PlanCode = x.PlanCode,
                    Cin = x.Cin,
                    OonId = x.OonId,
                    RecordType = x.RecordType,
                    ParentOonId = x.ParentOonId,
                    OonRequestReceivedDate = x.OonRequestReceivedDate,
                    ReferralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                    OonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                    OonRequestResolvedDate = x.OonRequestResolvedDate,
                    PartialApprovalExplanation = x.PartialApprovalExplanation,
                    SpecialistProviderNpi = x.SpecialistProviderNpi,
                    ProviderTaxonomy = x.ProviderTaxonomy,
                    ServiceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                    ServiceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                    ServiceLocationCity = x.ServiceLocationCity,
                    ServiceLocationState = x.ServiceLocationState,
                    ServiceLocationZip = x.ServiceLocationZip,
                    ServiceLocationCountry = x.ServiceLocationCountry,
                    ExtractionDate = x.TradingPartnerCode,
                    ErrorMessage = x.ErrorMessage
                }).ToList();
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(errorCocIpas.ToDataTable());
                    wb.Worksheets.Add(errorOonIpas.ToDataTable());
                    wb.SaveAs(errorFileName);
                }
            }
            using (LogContext context = new LogContext())
            {
                if (cts.IsCancellationRequested) return;
                IpaFileLog fileLog = context.IpaFileLogs.FirstOrDefault(x => x.ReportingPeriod == reportingPeriod.Substring(0, 6));
                if (fileLog == null)
                {
                    context.IpaFileLogs.Add(new IpaFileLog
                    {
                        ReportingPeriod=reportingPeriod.Substring(0,6),
                    });
                }
                context.OperationLogs.Add(new mcpdipData.OperationLog
                {
                    UserId = Environment.UserName,
                    OperationTime = DateTime.Now,
                    ModuleName = "Process Excel",
                    Message = "Processed " + fi.Name
                });
                context.SaveChanges();
                
            }
        }
        private static void Validate_PcpAssignment(ref CancellationTokenSource cts, ref List<ExcelPcpa> excelPcpas, ref List<ExcelPcpa> errorExcelPcpas, ref List<ExcelPcpa> validExcelPcpas, string IPAName)
        {
            //validate
            string cinPattern = "^[0-9]{8}[A-Z]$";
            string npiPattern = "^[0-9]{10}$";
            bool hasError = false;

            using (StagingContext _context = new StagingContext())
            using (HistoryContext _contextHistory = new HistoryContext())
            using (ErrorContext _contextError = new ErrorContext())
            {
                if (cts.IsCancellationRequested) return;
                PcpHeader pcpHeader = _context.PcpHeaders.FirstOrDefault();
                if (pcpHeader.ReportingPeriod != reportingPeriod)
                {
                    pcpHeader.ReportingPeriod = reportingPeriod;
                    _context.Database.ExecuteSqlCommand("delete from staging.pcpassignment");
                }
                List<string> dupCins = _context.PcpAssignments.Select(x => x.Cin).ToList();
                dupCins.AddRange(excelPcpas.Select(x => x.Cin));
                dupCins = dupCins.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                foreach (ExcelPcpa excelPcpa in excelPcpas)
                {
                    hasError = false;
                    if (dupCins.Contains(excelPcpa.Cin))
                    {
                        excelPcpa.ErrorMessage += "Business Error: Duplicated Cin";
                        hasError = true;
                    }
                    if (excelPcpa.PlanCode != "305" && excelPcpa.PlanCode != "306")
                    {
                        excelPcpa.ErrorMessage += "Business Error: Invalid PlanCode";
                        hasError = true;
                    }
                    if (!System.Text.RegularExpressions.Regex.Match(excelPcpa.Cin, cinPattern).Success)
                    {
                        excelPcpa.ErrorMessage += "Schema Error: Invalid CIN";
                        hasError = true;
                    }
                    if (!System.Text.RegularExpressions.Regex.Match(excelPcpa.Npi, npiPattern).Success)
                    {
                        excelPcpa.ErrorMessage += "Schema Error: Invalid NPI";
                        hasError = true;
                    }
                    if (hasError)
                    {
                        errorExcelPcpas.Add(excelPcpa);
                    }
                    else
                    {
                        validExcelPcpas.Add(excelPcpa);
                    }
                }
                _context.PcpAssignments.AddRange(validExcelPcpas.Select(x => new PcpAssignment
                {
                    PcpHeaderId = pcpHeader.PcpHeaderId,
                    PlanCode = x.PlanCode,
                    Cin = x.Cin,
                    Npi = x.Npi,
                    TradingPartnerCode = IPAName,
                    DataSource = IPAName
                }));
                _context.SaveChanges();
                if (errorExcelPcpas.Count > 0)
                {
                    PcpHeader headerError = _contextError.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod == reportingPeriod);
                    if (headerError == null)
                    {
                        headerError = new PcpHeader
                        {
                            PlanParent = pcpHeader.PlanParent,
                            SubmissionDate = pcpHeader.SubmissionDate,
                            ReportingPeriod = reportingPeriod,
                            SubmissionType = pcpHeader.SubmissionType,
                            SubmissionVersion = pcpHeader.SubmissionVersion,
                            SchemaVersion = pcpHeader.SchemaVersion
                        };
                        _contextError.PcpHeaders.Add(headerError);
                        _contextError.SaveChanges();
                    }
                    _contextError.PcpAssignments.AddRange(errorExcelPcpas.Select(x => new PcpAssignment
                    {
                        PcpHeaderId = headerError.PcpHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        Npi = x.Npi,
                        TradingPartnerCode = IPAName,
                        ErrorMessage = x.ErrorMessage,
                        DataSource = IPAName
                    }));
                    _contextError.SaveChanges();
                }
            }
        }
        private static void Validate_Grievance(ref CancellationTokenSource cts, ref List<McpdGrievance> allGrievances, ref List<McpdGrievance> validGrievances, ref List<McpdGrievance> errorGrievances, string IPAName)
        {
            if (cts.IsCancellationRequested) return;
            using (StagingContext _context = new StagingContext())
            using (HistoryContext _contextHistory = new HistoryContext())
            using (ErrorContext _contextError = new ErrorContext())
            {
                if (cts.IsCancellationRequested) return;
                McpdHeader header = _context.McpdHeaders.FirstOrDefault();
                if (header.ReportingPeriod != reportingPeriod)
                {
                    header.SubmissionDate = DateTime.Now;
                    header.ReportingPeriod = reportingPeriod;
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdgrievance");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdappeal");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdoutofnetwork");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdcontinuityofcare");
                }
                if (allGrievances.Count > 0)
                {
                    List<Tuple<string, bool, string>> grievanceSchemas = JsonOperationsNew.ValidateGrievance(allGrievances);
                    List<string> dupGrievanceIds = _context.Grievances.Select(x => x.GrievanceId).ToList();
                    dupGrievanceIds.AddRange(allGrievances.Select(x => x.GrievanceId));
                    dupGrievanceIds = dupGrievanceIds.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                    foreach (McpdGrievance grievance in allGrievances)
                    {
                        bool hasError = false;
                        grievance.ErrorMessage = "";
                        if (!grievanceSchemas.First(x => x.Item1 == grievance.GrievanceId).Item2)
                        {
                            grievance.ErrorMessage = "Schema Error:" + grievanceSchemas.First(x => x.Item1 == grievance.GrievanceId).Item3;
                            hasError = true;
                        }
                        else
                        {
                            //BL_Grievance001
                            if (dupGrievanceIds.Contains(grievance.GrievanceId))
                            {
                                grievance.ErrorMessage += "Business Error: Duplicated Grievance Id~";
                                hasError = true;
                            }
                            //BL_Grievance002
                            if (grievance.GrievanceId.Substring(0, 3) != grievance.PlanCode)
                            {
                                grievance.ErrorMessage += "Business Error: grievance id should start with plan code~";
                                hasError = true;
                            }
                            //BL_Grievance003
                            if (string.Compare(grievance.GrievanceReceivedDate, maxReceiveDate) >= 0)
                            {
                                grievance.ErrorMessage += "Business Error: Receive date should be prior to current month~";
                                hasError = true;
                            }
                            //BL_Grievance004
                            if (grievance.RecordType == "Original" && !string.IsNullOrEmpty(grievance.ParentGrievanceId))
                            {
                                grievance.ErrorMessage += "Business Error: Parent grievance id not allowed for Original~";
                                hasError = true;
                            }
                            if (grievance.RecordType != "Original")
                            {
                                if (string.IsNullOrEmpty(grievance.ParentGrievanceId))
                                {
                                    grievance.ErrorMessage += "Business Error: Parent grievance id is missing for non Original~";
                                    hasError = true;
                                }
                                else
                                {
                                    var parentGrievance = _contextHistory.Grievances.FirstOrDefault(x => x.GrievanceId == grievance.ParentGrievanceId);
                                    if (parentGrievance == null)
                                    {
                                        grievance.ErrorMessage += "Business Error: Parent grievance id couldnot be found~";
                                        hasError = true;
                                    }
                                    else
                                    {
                                        var processedGrievance = _contextHistory.Grievances.FirstOrDefault(x => x.ParentGrievanceId == grievance.ParentGrievanceId);
                                        if (processedGrievance != null)
                                        {
                                            grievance.ErrorMessage += "Business Error: Already processed before, no more actions~";
                                            hasError = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (hasError)
                        {
                            if (grievance.ErrorMessage.Length > 2000) grievance.ErrorMessage = grievance.ErrorMessage.Substring(0, 2000);
                            errorGrievances.Add(grievance);
                        }
                        else
                        {
                            validGrievances.Add(grievance);
                        }
                    }
                    //save to staging table
                    _context.Grievances.AddRange(validGrievances.Select(x => new McpdGrievance
                    {
                        McpdHeaderId = header.McpdHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        GrievanceId = x.GrievanceId,
                        RecordType = x.RecordType,
                        ParentGrievanceId = x.ParentGrievanceId,
                        GrievanceReceivedDate = x.GrievanceReceivedDate,
                        GrievanceType = x.GrievanceType,
                        BenefitType = x.BenefitType,
                        ExemptIndicator = x.ExemptIndicator,
                        TradingPartnerCode = x.TradingPartnerCode,
                        DataSource = x.DataSource,
                        CaseNumber = x.CaseNumber,
                        CaseStatus = x.CaseStatus
                    }));
                    _context.SaveChanges();
                }
                if (errorGrievances.Count > 0)
                {
                    //save to error table
                    McpdHeader headerError = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod == reportingPeriod);
                    if (headerError == null)
                    {
                        headerError = new McpdHeader
                        {
                            PlanParent = header.PlanParent,
                            SubmissionDate = header.SubmissionDate,
                            SchemaVersion = header.SchemaVersion,
                            ReportingPeriod = header.ReportingPeriod
                        };
                        _contextError.McpdHeaders.Add(headerError);
                        _contextError.SaveChanges();
                    }
                    _contextError.Grievances.AddRange(errorGrievances.Select(x => new McpdGrievance
                    {
                        McpdHeaderId = headerError.McpdHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        GrievanceId = x.GrievanceId,
                        RecordType = x.RecordType,
                        ParentGrievanceId = x.ParentGrievanceId,
                        GrievanceReceivedDate = x.GrievanceReceivedDate,
                        GrievanceType = x.GrievanceType,
                        BenefitType = x.BenefitType,
                        ExemptIndicator = x.ExemptIndicator,
                        TradingPartnerCode = x.TradingPartnerCode,
                        ErrorMessage = x.ErrorMessage,
                        DataSource = x.DataSource,
                        CaseNumber = x.CaseNumber,
                        CaseStatus = x.CaseStatus
                    }));
                    _contextError.SaveChanges();
                }
            }
        }
        private static void Validate_Appeal(ref CancellationTokenSource cts, ref List<McpdAppeal> allAppeals, ref List<McpdAppeal> validAppeals, ref List<McpdAppeal> errorAppeals, string IPAName)
        {
            using (StagingContext _context = new StagingContext())
            using (HistoryContext _contextHistory = new HistoryContext())
            using (ErrorContext _contextError = new ErrorContext())
            {
                if (cts.IsCancellationRequested) return;
                McpdHeader header = _context.McpdHeaders.FirstOrDefault();
                if (header.ReportingPeriod != reportingPeriod)
                {
                    header.SubmissionDate = DateTime.Now;
                    header.ReportingPeriod = reportingPeriod;
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdgrievance");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdappeal");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdoutofnetwork");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdcontinuityofcare");
                }
                if (allAppeals.Count > 0)
                {
                    List<Tuple<string, bool, string>> appealSchemas = JsonOperationsNew.ValidateAppeal(allAppeals);
                    List<string> dupAppealIds = _context.Appeals.Select(x => x.AppealId).ToList();
                    dupAppealIds.AddRange(allAppeals.Select(x => x.AppealId));
                    dupAppealIds = dupAppealIds.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                    foreach (McpdAppeal appeal in allAppeals)
                    {
                        bool hasError = false;
                        appeal.ErrorMessage = "";
                        if (!appealSchemas.First(x => x.Item1 == appeal.AppealId).Item2)
                        {
                            appeal.ErrorMessage = "Schema Error: " + appealSchemas.First(x => x.Item1 == appeal.AppealId).Item3;
                            hasError = true;
                        }
                        else
                        {
                            //BL_Appeal001
                            if (dupAppealIds.Contains(appeal.AppealId))
                            {
                                appeal.ErrorMessage += "Business Error: Duplicated appeal id~";
                                hasError = true;
                            }
                            //BL_Appeal002
                            if (appeal.AppealId.Substring(0, 3) != appeal.PlanCode)
                            {
                                appeal.ErrorMessage += "Business Error: Appeal id should start with plan code~";
                                hasError = true;
                            }
                            //BL_Appeal003
                            if (string.Compare(appeal.AppealReceivedDate, maxReceiveDate) >= 0)
                            {
                                appeal.ErrorMessage += "Business Error: Receive date should be prior to current month~";
                                hasError = true;
                            }
                            //BL_Appeal004
                            if (string.Compare(appeal.NoticeOfActionDate, maxReceiveDate) >= 0)
                            {
                                appeal.ErrorMessage += "Business Error: Action date should be prior to current month~";
                                hasError = true;
                            }
                            //BL_Appeal005
                            if (appeal.RecordType == "Original" && !string.IsNullOrEmpty(appeal.ParentAppealId))
                            {
                                appeal.ErrorMessage += "Business Error: Parent appeal id not allowed for Original~";
                                hasError = true;
                            }
                            if (appeal.RecordType != "Original")
                            {
                                if (string.IsNullOrEmpty(appeal.ParentAppealId))
                                {
                                    appeal.ErrorMessage += "Business Error: Parent appeal id is missing for non Original~";
                                    hasError = true;
                                }
                                else
                                {
                                    var parentAppeal = _contextHistory.Appeals.FirstOrDefault(x => x.AppealId == appeal.ParentAppealId);
                                    if (parentAppeal == null)
                                    {
                                        appeal.ErrorMessage += "Business Error: Parent appeal id couldnot be found~";
                                        hasError = true;
                                    }
                                    else
                                    {
                                        var processedAppeal = _contextHistory.Appeals.FirstOrDefault(x => x.ParentAppealId == appeal.ParentAppealId);
                                        if (processedAppeal != null)
                                        {
                                            appeal.ErrorMessage += "Business Error: Already processed before, no more actions~";
                                            hasError = true;
                                        }
                                    }
                                }
                            }
                            //BL_Appeal006
                            if (appeal.AppealResolutionStatusIndicator == "Unresolved")
                            {
                                if (!string.IsNullOrEmpty(appeal.AppealResolutionDate))
                                {
                                    appeal.ErrorMessage += "Business Error: Resolution date not allowed for unresolved appeal~";
                                    hasError = true;
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(appeal.AppealResolutionDate))
                                {
                                    appeal.ErrorMessage += "Business Error: Resolution date must be populated for resolved appeal~";
                                    hasError = true;
                                }
                                else
                                {
                                    if (string.Compare(appeal.AppealResolutionDate, maxReceiveDate) >= 0)
                                    {
                                        appeal.ErrorMessage += "Business Error: Resolution date should be prior to current month~";
                                        hasError = true;
                                    }
                                    if (string.Compare(appeal.AppealResolutionDate, appeal.AppealReceivedDate) < 0)
                                    {
                                        appeal.ErrorMessage += "Business Error: Resolution date cannot be prior to receive date~";
                                        hasError = true;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(appeal.ParentGrievanceId))
                            {
                                var appealParentGrievance = _contextHistory.Grievances.FirstOrDefault(x => x.GrievanceId == appeal.ParentGrievanceId);
                                if (appealParentGrievance == null)
                                {
                                    appeal.ParentGrievanceId = null;
                                }
                            }
                        }
                        if (hasError)
                        {
                            if (appeal.ErrorMessage.Length > 2000) appeal.ErrorMessage = appeal.ErrorMessage.Substring(0, 2000);
                            errorAppeals.Add(appeal);
                        }
                        else
                        {
                            validAppeals.Add(appeal);
                        }
                    }
                    //save to staging table
                    _context.Appeals.AddRange(validAppeals.Select(x => new McpdAppeal
                    {
                        McpdHeaderId = header.McpdHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        AppealId = x.AppealId,
                        RecordType = x.RecordType,
                        ParentGrievanceId = x.ParentGrievanceId,
                        ParentAppealId = x.ParentAppealId,
                        AppealReceivedDate = x.AppealReceivedDate,
                        NoticeOfActionDate = x.NoticeOfActionDate,
                        AppealType = x.AppealType,
                        BenefitType = x.BenefitType,
                        AppealResolutionStatusIndicator = x.AppealResolutionStatusIndicator,
                        AppealResolutionDate = x.AppealResolutionDate,
                        PartiallyOverturnIndicator = x.PartiallyOverturnIndicator,
                        ExpeditedIndicator = x.ExpeditedIndicator,
                        TradingPartnerCode = x.TradingPartnerCode,
                        DataSource = x.DataSource,
                        CaseNumber = x.CaseNumber,
                        CaseStatus = x.CaseStatus

                    }));
                    _context.SaveChanges();
                }
                if (errorAppeals.Count > 0)
                {
                    //save to error table
                    McpdHeader headerError = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod == reportingPeriod);
                    if (headerError == null)
                    {
                        headerError = new McpdHeader
                        {
                            PlanParent = header.PlanParent,
                            SubmissionDate = header.SubmissionDate,
                            SchemaVersion = header.SchemaVersion,
                            ReportingPeriod = header.ReportingPeriod
                        };
                        _contextError.McpdHeaders.Add(headerError);
                        _contextError.SaveChanges();
                    }
                    _contextError.Appeals.AddRange(errorAppeals.Select(x => new McpdAppeal
                    { 
                        McpdHeaderId = headerError.McpdHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        AppealId = x.AppealId,
                        RecordType = x.RecordType,
                        ParentGrievanceId = x.ParentGrievanceId,
                        ParentAppealId = x.ParentAppealId,
                        AppealReceivedDate = x.AppealReceivedDate,
                        NoticeOfActionDate = x.NoticeOfActionDate,
                        AppealType = x.AppealType,
                        BenefitType = x.BenefitType,
                        AppealResolutionStatusIndicator = x.AppealResolutionStatusIndicator,
                        AppealResolutionDate = x.AppealResolutionDate,
                        PartiallyOverturnIndicator = x.PartiallyOverturnIndicator,
                        ExpeditedIndicator = x.ExpeditedIndicator,
                        TradingPartnerCode = x.TradingPartnerCode,
                        ErrorMessage = x.ErrorMessage,
                        DataSource = x.DataSource,
                        CaseNumber = x.CaseNumber,
                        CaseStatus = x.CaseStatus
                    }));
                    _contextError.SaveChanges();
                }
            }
        }
        private static void Validate_Coc(ref CancellationTokenSource cts, ref List<McpdContinuityOfCare> cocs, ref List<McpdContinuityOfCare> validCocs, ref List<McpdContinuityOfCare> errorCocs, string IPAName)
        {
            //schema check
            using (StagingContext _context = new StagingContext())
            using (HistoryContext _contextHistory = new HistoryContext())
            using (ErrorContext _contextError = new ErrorContext())
            {
                if (cts.IsCancellationRequested) return;
                McpdHeader header = _context.McpdHeaders.FirstOrDefault();
                if (header.ReportingPeriod != reportingPeriod)
                {
                    header.SubmissionDate = DateTime.Now;
                    header.ReportingPeriod = reportingPeriod;
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdgrievance");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdappeal");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdoutofnetwork");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdcontinuityofcare");
                }
                if (cocs.Count > 0)
                {
                    List<Tuple<string, bool, string>> cocSchemas = JsonOperationsNew.ValidateCOC(cocs);
                    List<string> dupCocIds = _context.McpdContinuityOfCare.Select(x => x.CocId).ToList();
                    dupCocIds.AddRange(cocs.Select(x => x.CocId));
                    dupCocIds = dupCocIds.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                    foreach (McpdContinuityOfCare coc in cocs)
                    {
                        bool hasError = false;
                        coc.ErrorMessage = "";
                        if (!cocSchemas.First(x => x.Item1 == coc.CocId).Item2)
                        {
                            coc.ErrorMessage = "Schem Error: " + cocSchemas.First(x => x.Item1 == coc.CocId).Item3;
                            hasError = true;
                        }
                        else
                        {
                            //BL_Coc001
                            if (dupCocIds.Contains(coc.CocId))
                            {
                                coc.ErrorMessage += "Business Error: Duplicated COC id~";
                                hasError = true;
                            }
                            //BL_Coc002
                            if (coc.CocId.Substring(0, 3) != coc.PlanCode)
                            {
                                coc.ErrorMessage += "Business Error: COC id should start with plan code~";
                                hasError = true;
                            }
                            //BL_Coc003
                            if (string.Compare(coc.CocReceivedDate, maxReceiveDate) >= 0)
                            {
                                coc.ErrorMessage += "Business Error: Receive date should be prior to current month~";
                                hasError = true;
                            }
                            //BL_Coc004
                            if (coc.RecordType == "Original" && !string.IsNullOrEmpty(coc.ParentCocId))
                            {
                                coc.ErrorMessage += "Business Error: Parent COC id not allowed for Original~";
                                hasError = true;
                            }
                            if (coc.RecordType != "Original")
                            {
                                if (string.IsNullOrEmpty(coc.ParentCocId))
                                {
                                    coc.ErrorMessage += "Business Error: Parent COC id is missing for non Original~";
                                    hasError = true;
                                }
                                else
                                {
                                    var parentCoc = _contextHistory.McpdContinuityOfCare.FirstOrDefault(x => x.CocId == coc.ParentCocId);
                                    if (parentCoc == null)
                                    {
                                        coc.ErrorMessage += "Business Error: Parent COC id couldnot be found~";
                                        hasError = true;
                                    }
                                    else
                                    {
                                        var processedCoc = _contextHistory.McpdContinuityOfCare.FirstOrDefault(x => x.ParentCocId == coc.ParentCocId);
                                        if (processedCoc != null)
                                        {
                                            coc.ErrorMessage += "Business Error: Already processed before, no more actions~";
                                            hasError = true;
                                        }
                                    }
                                }
                            }
                            //BL_Coc005
                            if (coc.CocType != "MER Denial" && coc.CocDispositionIndicator == "Provider is in MCP Network")
                            {
                                coc.ErrorMessage += "Business Error: COC disposition indicator must <> Provider is in MCP Network, if COC type <> MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc006
                            if (coc.CocDispositionIndicator == "Denied" && !string.IsNullOrEmpty(coc.CocExpirationDate))
                            {
                                coc.ErrorMessage += "Business Error:  Expiration date must be blank if COC disposition indicator = Denied~";
                                hasError = true;
                            }
                            //BL_Coc007
                            if (coc.CocDispositionIndicator == "Approved" && string.IsNullOrEmpty(coc.CocExpirationDate))
                            {
                                coc.ErrorMessage += "Business Error: Expiration date must be populated if COC disposition indicator = Denied~";
                                hasError = true;
                            }
                            //BL_Coc008
                            if (coc.CocDispositionIndicator == "Denied" && string.IsNullOrEmpty(coc.CocDenialReasonIndicator))
                            {
                                coc.ErrorMessage += "Business Error: COC denial reason indicator must be populated if COC disposition indicator = Denied~";
                                hasError = true;
                            }
                            //BL_Coc009
                            if (coc.CocDispositionIndicator != "Denied" && !string.IsNullOrEmpty(coc.CocDenialReasonIndicator))
                            {
                                coc.ErrorMessage += "Business Error: COC denial reason indicator must be blank if COC type <> Denied~";
                                hasError = true;
                            }
                            //BL_Coc010
                            if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.MerExemptionId))
                            {
                                coc.ErrorMessage += "Business Error: MER exemption id must be populated if COC type = MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc011
                            if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.MerExemptionId))
                            {
                                coc.ErrorMessage += "Business Error: MER excemption id must be blank if COC type <> MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc012
                            if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.ExemptionToEnrollmentDenialCode))
                            {
                                coc.ErrorMessage += "Business Error: Exemption to enrollment denial code must be populated if COC type = MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc013
                            if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.ExemptionToEnrollmentDenialCode))
                            {
                                coc.ErrorMessage += "Business Error: Exemption to enrollment denial code must be blank if COC type <> MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc014
                            if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.ExemptionToEnrollmentDenialDate))
                            {
                                coc.ErrorMessage += "Business Error: Excemption to enrollment denial date must be populated if COC type = MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc015
                            if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.ExemptionToEnrollmentDenialDate))
                            {
                                coc.ErrorMessage += "Business Error: Exemption to enrollment denial date must be blank if COC type <> MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc016
                            if (coc.CocType == "MER Denial" && string.Compare(coc.ExemptionToEnrollmentDenialDate, maxReceiveDate) >= 0)
                            {
                                coc.ErrorMessage += "Business Error: Exemption to enrollment denial date must be prior to current month~";
                                hasError = true;
                            }
                            //BL_Coc017
                            if (coc.MerCocDispositionIndicator != "MER COC Not Met" && coc.CocProviderNpi != coc.SubmittingProviderNpi)
                            {
                                coc.ErrorMessage += "Business Error: COC provider NPI must = submitting provider NPI, if MER COC disposition indicator <> MER COC Not Met~";
                                hasError = true;
                            }
                            //BL_Coc018
                            if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.MerCocDispositionIndicator))
                            {
                                coc.ErrorMessage += "Business Error: MER COC disposition indicator must be populated, if COC type = MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc019
                            if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.MerCocDispositionIndicator))
                            {
                                coc.ErrorMessage += "Business Error: MER COC disposition indicator must be blank if COC type <> MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc020
                            if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.MerCocDispositionDate))
                            {
                                coc.ErrorMessage += "Business Error: MER COC disposition date must be populated if COC type = MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc021
                            if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.MerCocDispositionDate))
                            {
                                coc.ErrorMessage += "Business Error: MER COC disposition date must be blank if COC type <> MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc022
                            if (coc.CocType == "MER Denial" && string.Compare(coc.MerCocDispositionDate, maxReceiveDate) >= 0)
                            {
                                coc.ErrorMessage += "Business Error: MER COC disposition date must be prior to current month~";
                                hasError = true;
                            }
                            //BL_Coc023
                            if (coc.CocType == "MER Denial" && coc.MerCocDispositionIndicator == "MER COC Not Met" && string.IsNullOrEmpty(coc.ReasonMerCocNotMetIndicator))
                            {
                                coc.ErrorMessage += "Business Error: Reason MER COC not met must be populated if COC type = MER Denial and MER COC disposition indicator = MER COC Not Met~";
                                hasError = true;
                            }
                            //BL_Coc024
                            if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.ReasonMerCocNotMetIndicator))
                            {
                                coc.ErrorMessage += "Business Error: Reason MER COC not met must be blank if COC type <> MER Denial~";
                                hasError = true;
                            }
                            //BL_Coc025
                            if (coc.MerCocDispositionIndicator != "MER COC Not Met" && !string.IsNullOrEmpty(coc.ReasonMerCocNotMetIndicator))
                            {
                                coc.ErrorMessage += "Business Error: Reason MER COC not met must be blank if MER COC disposition indicator <> MER COC Not Met~";
                                hasError = true;
                            }
                        }
                        if (hasError)
                        {
                            if (coc.ErrorMessage.Length > 2000) coc.ErrorMessage = coc.ErrorMessage.Substring(0, 2000);
                            errorCocs.Add(coc);
                        }
                        else
                        {
                            validCocs.Add(coc);
                        }
                    }
                    _context.McpdContinuityOfCare.AddRange(validCocs.Select(x => new McpdContinuityOfCare
                    {
                        McpdHeaderId = header.McpdHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        CocId = x.CocId,
                        RecordType = x.RecordType,
                        ParentCocId = x.ParentCocId,
                        CocReceivedDate = x.CocReceivedDate,
                        CocType = x.CocType,
                        BenefitType = x.BenefitType,
                        CocDispositionIndicator = x.CocDispositionIndicator,
                        CocExpirationDate = x.CocExpirationDate,
                        CocDenialReasonIndicator = x.CocDenialReasonIndicator,
                        SubmittingProviderNpi = x.SubmittingProviderNpi,
                        CocProviderNpi = x.CocProviderNpi,
                        ProviderTaxonomy = x.ProviderTaxonomy,
                        MerExemptionId = x.MerExemptionId,
                        ExemptionToEnrollmentDenialCode = x.ExemptionToEnrollmentDenialCode,
                        ExemptionToEnrollmentDenialDate = x.ExemptionToEnrollmentDenialDate,
                        MerCocDispositionIndicator = x.MerCocDispositionIndicator,
                        MerCocDispositionDate = x.MerCocDispositionDate,
                        ReasonMerCocNotMetIndicator = x.ReasonMerCocNotMetIndicator,
                        TradingPartnerCode = IPAName=="Kaiser"?"Kaiser":"IEHP",
                        DataSource = IPAName,
                        CaseNumber=x.CaseNumber,
                        CaseStatus=x.CaseStatus
                    }));
                    _context.SaveChanges();
                }
                if (errorCocs.Count() > 0)
                {
                    McpdHeader headerError = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod == reportingPeriod);
                    if (headerError == null)
                    {
                        headerError = new McpdHeader
                        {
                            PlanParent = header.PlanParent,
                            SubmissionDate = header.SubmissionDate,
                            SchemaVersion = header.SchemaVersion,
                            ReportingPeriod = header.ReportingPeriod
                        };
                        _contextError.McpdHeaders.Add(headerError);
                        _contextError.SaveChanges();
                    }
                    _contextError.McpdContinuityOfCare.AddRange(errorCocs.Select(x => new McpdContinuityOfCare
                    {
                        McpdHeaderId = headerError.McpdHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        CocId = x.CocId,
                        RecordType = x.RecordType,
                        ParentCocId = x.ParentCocId,
                        CocReceivedDate = x.CocReceivedDate,
                        CocType = x.CocType,
                        BenefitType = x.BenefitType,
                        CocDispositionIndicator = x.CocDispositionIndicator,
                        CocExpirationDate = x.CocExpirationDate,
                        CocDenialReasonIndicator = x.CocDenialReasonIndicator,
                        SubmittingProviderNpi = x.SubmittingProviderNpi,
                        CocProviderNpi = x.CocProviderNpi,
                        ProviderTaxonomy = x.ProviderTaxonomy?.Substring(0, 10),
                        MerExemptionId = x.MerExemptionId,
                        ExemptionToEnrollmentDenialCode = x.ExemptionToEnrollmentDenialCode,
                        ExemptionToEnrollmentDenialDate = x.ExemptionToEnrollmentDenialDate,
                        MerCocDispositionIndicator = x.MerCocDispositionIndicator,
                        MerCocDispositionDate = x.MerCocDispositionDate,
                        ReasonMerCocNotMetIndicator = x.ReasonMerCocNotMetIndicator,
                        TradingPartnerCode = IPAName == "Kaiser" ? "Kaiser" : "IEHP",
                        ErrorMessage = x.ErrorMessage,
                        DataSource = IPAName,
                        CaseNumber = x.CaseNumber,
                        CaseStatus = x.CaseStatus
                    }));
                    _contextError.SaveChanges();
                }
            }
        }
        private static void Validate_Oon(ref CancellationTokenSource cts, ref List<McpdOutOfNetwork> oons, ref List<McpdOutOfNetwork> validOons, ref List<McpdOutOfNetwork> errorOons, string IPAName)
        {
            using (StagingContext _context = new StagingContext())
            using (HistoryContext _contextHistory = new HistoryContext())
            using (ErrorContext _contextError = new ErrorContext())
            {
                if (cts.IsCancellationRequested) return;
                McpdHeader header = _context.McpdHeaders.FirstOrDefault();
                if (header.ReportingPeriod != reportingPeriod)
                {
                    header.SubmissionDate = DateTime.Now;
                    header.ReportingPeriod = reportingPeriod;
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdgrievance");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdappeal");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdoutofnetwork");
                    _context.Database.ExecuteSqlCommand("delete from staging.mcpdcontinuityofcare");
                }
                if (oons.Count > 0)
                {
                    List<Tuple<string, bool, string>> oonSchemas = JsonOperationsNew.ValidateOON(oons);
                    List<string> dupOonIds = _context.McpdOutOfNetwork.Select(x => x.OonId).ToList();
                    dupOonIds.AddRange(oons.Select(x => x.OonId));
                    dupOonIds = dupOonIds.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                    foreach (McpdOutOfNetwork oon in oons)
                    {
                        bool hasError = false;
                        oon.ErrorMessage = "";
                        if (!oonSchemas.First(x => x.Item1 == oon.OonId).Item2)
                        {
                            oon.ErrorMessage = "Schem Error: " + oonSchemas.First(x => x.Item1 == oon.OonId).Item3;
                            hasError = true;
                        }
                        else
                        {
                            //BL_Oon001
                            if (dupOonIds.Contains(oon.OonId))
                            {
                                oon.ErrorMessage += "Business Error: Duplicated Oon id~";
                                hasError = true;
                            }
                            //BL_Oon002
                            if (oon.OonId.Substring(0, 3) != oon.PlanCode)
                            {
                                oon.ErrorMessage += "Business Error: Oon id should start with plan code~";
                                hasError = true;
                            }
                            //BL_Oon003
                            if (string.Compare(oon.OonRequestReceivedDate, maxReceiveDate) >= 0)
                            {
                                oon.ErrorMessage += "Business Error: Receive date should be prior to current month~";
                                hasError = true;
                            }
                            //BL_Oon004
                            if (oon.RecordType == "Original" && !string.IsNullOrEmpty(oon.ParentOonId))
                            {
                                oon.ErrorMessage += "Business Error: Parent Oon id not allowed for Original~";
                                hasError = true;
                            }
                            if (oon.RecordType != "Original")
                            {
                                if (string.IsNullOrEmpty(oon.ParentOonId))
                                {
                                    oon.ErrorMessage += "Business Error: Parent Oon id is missing for non Original~";
                                    hasError = true;
                                }
                                else
                                {
                                    var parentOon = _contextHistory.McpdOutOfNetwork.FirstOrDefault(x => x.OonId == oon.ParentOonId);
                                    if (parentOon == null)
                                    {
                                        oon.ErrorMessage += "Business Error: Parent Oon id couldnot be found~";
                                        hasError = true;
                                    }
                                    else
                                    {
                                        var processedOon = _contextHistory.McpdOutOfNetwork.FirstOrDefault(x => x.ParentOonId == oon.ParentOonId);
                                        if (processedOon != null)
                                        {
                                            oon.ErrorMessage += "Business Error: Already processed before, no more actions~";
                                            hasError = true;
                                        }
                                    }
                                }
                            }
                            //BL_Oon005
                            if (oon.OonResolutionStatusIndicator == "Partial Approval" && string.IsNullOrEmpty(oon.PartialApprovalExplanation))
                            {
                                oon.ErrorMessage += "Business Error: Partial Approval Explanation must be populated when OON Resolution Status Indicator = Partial Approval~";
                                hasError = true;
                            }
                            //BL_Oon006
                            if (oon.OonResolutionStatusIndicator == "Pending" && !string.IsNullOrEmpty(oon.OonRequestResolvedDate))
                            {
                                oon.ErrorMessage += "Business Error: OON Request Resolved Date must be blank if OON Resolution Status Indicator = Pending~";
                                hasError = true;
                            }
                            //BL_Oon007
                            if (oon.OonResolutionStatusIndicator != "Pending" && string.IsNullOrEmpty(oon.OonRequestResolvedDate))
                            {
                                oon.ErrorMessage += "Business Error: OON Request Resolved Date must be populated if OON Resolution Status Indicator <> Pending~";
                                hasError = true;
                            }
                            //BL_Oon008
                            if (oon.OonResolutionStatusIndicator != "Pending" && string.Compare(oon.OonRequestResolvedDate, maxReceiveDate) >= 0)
                            {
                                oon.ErrorMessage += "Business Error: OON Request Resolved Date is not a past date~";
                                hasError = true;
                            }
                            //BL_Oon009
                            if (oon.OonResolutionStatusIndicator != "Pending" && string.Compare(oon.OonRequestResolvedDate, oon.OonRequestReceivedDate) < 0)
                            {
                                oon.ErrorMessage += "Business Error: OON Request Resolved Date must be >= OON Request Received Date~";
                                hasError = true;
                            }
                            //BL_OON010
                            if (oon.ServiceLocationCountry != "US" && !string.IsNullOrEmpty(oon.ServiceLocationState))
                            {
                                oon.ErrorMessage += "Business ERror: Foreign country, state should be blank~";
                                hasError = true;
                            }
                            //BL_OON011
                            if (oon.ServiceLocationCountry != "US" && !string.IsNullOrEmpty(oon.ServiceLocationZip))
                            {
                                oon.ErrorMessage += "Business Error: Foreigh country, zip should be blank~";
                                hasError = true;
                            }
                        }
                        if (hasError)
                        {
                            if (oon.ErrorMessage.Length > 255) oon.ErrorMessage = oon.ErrorMessage.Substring(0, 255);
                            errorOons.Add(oon);
                        }
                        else
                        {
                            validOons.Add(oon);
                        }
                    }

                    _context.McpdOutOfNetwork.AddRange(validOons.Select(x => new McpdOutOfNetwork
                    {
                        McpdHeaderId = header.McpdHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        OonId = x.OonId,
                        RecordType = x.RecordType,
                        ParentOonId = x.ParentOonId,
                        OonRequestReceivedDate = x.OonRequestReceivedDate,
                        ReferralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                        OonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                        OonRequestResolvedDate = x.OonRequestResolvedDate,
                        PartialApprovalExplanation = x.PartialApprovalExplanation,
                        SpecialistProviderNpi = x.SpecialistProviderNpi,
                        ProviderTaxonomy = x.ProviderTaxonomy,
                        ServiceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                        ServiceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                        ServiceLocationCity = x.ServiceLocationCity,
                        ServiceLocationState = x.ServiceLocationState,
                        ServiceLocationZip = x.ServiceLocationZip,
                        ServiceLocationCountry = x.ServiceLocationCountry,
                        TradingPartnerCode = IPAName=="Kaiser"?"Kaiser":"IEHP",
                        DataSource = IPAName,
                        CaseNumber=x.CaseNumber,
                        CaseStatus=x.CaseStatus
                    }));
                    _context.SaveChanges();
                }
                if (errorOons.Count() > 0)
                {
                    McpdHeader headerError = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod == reportingPeriod);
                    if (headerError == null)
                    {
                        headerError = new McpdHeader
                        {
                            PlanParent = header.PlanParent,
                            SubmissionDate = header.SubmissionDate,
                            SchemaVersion = header.SchemaVersion,
                            ReportingPeriod = header.ReportingPeriod
                        };
                        _contextError.McpdHeaders.Add(headerError);
                        _contextError.SaveChanges();
                    }
                    _contextError.McpdOutOfNetwork.AddRange(errorOons.Select(x => new McpdOutOfNetwork
                    {
                        McpdHeaderId = headerError.McpdHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        OonId = x.OonId,
                        RecordType = x.RecordType,
                        ParentOonId = x.ParentOonId,
                        OonRequestReceivedDate = x.OonRequestReceivedDate,
                        ReferralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                        OonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                        OonRequestResolvedDate = x.OonRequestResolvedDate,
                        PartialApprovalExplanation = x.PartialApprovalExplanation,
                        SpecialistProviderNpi = x.SpecialistProviderNpi,
                        ProviderTaxonomy = x.ProviderTaxonomy,
                        ServiceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                        ServiceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                        ServiceLocationCity = x.ServiceLocationCity,
                        ServiceLocationState = x.ServiceLocationState,
                        ServiceLocationZip = x.ServiceLocationZip?.Trim().Substring(0, 5),
                        ServiceLocationCountry = x.ServiceLocationCountry,
                        TradingPartnerCode = IPAName == "Kaiser" ? "Kaiser" : "IEHP",
                        ErrorMessage = x.ErrorMessage,
                        DataSource = IPAName,
                        CaseNumber = x.CaseNumber,
                        CaseStatus = x.CaseStatus
                    }));
                    _contextError.SaveChanges();
                }
            }
        }
    }
}

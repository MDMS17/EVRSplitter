using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mcpdipData;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Validation;
using Newtonsoft.Json;

namespace JsonLib
{
    public class JsonOperationsNew
    {
        public static List<Tuple<string, bool, string>> ValidateGrievance(List<McpdGrievance> grievances)
        {
            List<JsonGrievance> jsonGrievances = grievances.Select(x => new JsonGrievance
            {
                planCode = x.PlanCode,
                cin = x.Cin,
                grievanceId = x.GrievanceId,
                recordType = x.RecordType,
                parentGrievanceId = string.IsNullOrEmpty(x.ParentGrievanceId) ? null : x.ParentGrievanceId,
                grievanceReceivedDate = x.GrievanceReceivedDate,
                grievanceType = x.GrievanceType.Split( new char[] { '|',';'}).ToList(),
                benefitType = x.BenefitType,
                exemptIndicator = x.ExemptIndicator
            }).ToList();

            string GrievanceSchemaFile = File.ReadAllText("JsonSchema\\grievance.json");
            var schema = SchemaReader.ReadSchema(GrievanceSchemaFile, "JsonSchema\\grievance.json");
            Validator validator = new Validator(schema);
            List<Tuple<string, bool, string>> result = new List<Tuple<string, bool, string>>();
            foreach (JsonGrievance grievance in jsonGrievances)
            {
                string GrievanceJsonString = JsonConvert.SerializeObject(grievance, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Result[] errors = validator.Validate(GrievanceJsonString, "grievance.json");
                List<string> errorList = errors.Select(x => "Invalid " + x.Message.Arguments[0].ToString()).ToList();
                result.Add(Tuple.Create(grievance.grievanceId, errors.Any()?false:true, String.Join("~", errorList)));
            }
            return result;
        }
        public static List<Tuple<string, bool, string>> ValidateAppeal(List<McpdAppeal> appeals)
        {
            List<JsonAppeal> jsonAppeals = appeals.Select(x => new JsonAppeal
            {
                planCode = x.PlanCode,
                cin = x.Cin,
                appealId = x.AppealId,
                recordType = x.RecordType,
                parentGrievanceId = string.IsNullOrEmpty(x.ParentGrievanceId) ? null : x.ParentGrievanceId,
                parentAppealId = string.IsNullOrEmpty(x.ParentAppealId) ? null : x.ParentAppealId,
                appealReceivedDate = x.AppealReceivedDate,
                noticeOfActionDate = x.NoticeOfActionDate,
                appealType = x.AppealType,
                benefitType = x.BenefitType,
                appealResolutionStatusIndicator = x.AppealResolutionStatusIndicator,
                appealResolutionDate = x.AppealResolutionDate,
                partiallyOverturnIndicator = x.PartiallyOverturnIndicator,
                expeditedIndicator = x.ExpeditedIndicator

            }).ToList();
            string AppealSchemaFile = File.ReadAllText("JsonSchema\\appeal.json");
            var schema = SchemaReader.ReadSchema(AppealSchemaFile, "JsonSchema\\appeal.json");
            Validator validator = new Validator(schema);
            List<Tuple<string, bool, string>> result = new List<Tuple<string, bool, string>>();
            foreach (JsonAppeal appeal in jsonAppeals)
            {
                string AppealJsonString = JsonConvert.SerializeObject(appeal, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Result[] errors = validator.Validate(AppealJsonString, "appeal.json");
                List<string> errorList = errors.Select(x => "Invalid " + x.Message.Arguments[0].ToString()).ToList();
                result.Add(Tuple.Create(appeal.appealId, errors.Any() ? false : true, String.Join("~", errorList)));
            }
            return result;
        }
        public static List<Tuple<string, bool, string>> ValidateCOC(List<McpdContinuityOfCare> cocs)
        {
            List<JsonCOC> jsonCOCs = cocs.Select(x => new JsonCOC
            {
                planCode = x.PlanCode,
                cin = x.Cin,
                cocId = x.CocId,
                recordType = x.RecordType,
                parentCocId = x.ParentCocId,
                cocReceivedDate = x.CocReceivedDate,
                cocType = x.CocType,
                benefitType = x.BenefitType,
                cocDispositionIndicator = x.CocDispositionIndicator,
                cocExpirationDate = x.CocExpirationDate,
                cocDenialReasonIndicator = x.CocDenialReasonIndicator,
                submittingProviderNpi = x.SubmittingProviderNpi,
                cocProviderNpi = x.CocProviderNpi,
                providerTaxonomy = x.ProviderTaxonomy,
                merExemptionId = x.MerExemptionId,
                exemptionToEnrollmentDenialCode = x.ExemptionToEnrollmentDenialCode,
                exemptionToEnrollmentDenialDate = x.ExemptionToEnrollmentDenialDate,
                merCocDispositionIndicator = x.MerCocDispositionIndicator,
                merCocDispositionDate = x.MerCocDispositionDate,
                reasonMerCocNotMetIndicator = x.ReasonMerCocNotMetIndicator
            }).ToList();
            string CocSchemaFile = File.ReadAllText("JsonSchema\\coc.json");
            var schema = SchemaReader.ReadSchema(CocSchemaFile, "JsonSchema\\coc.json");
            Validator validator = new Validator(schema);
            List<Tuple<string, bool, string>> result = new List<Tuple<string, bool, string>>();
            foreach (JsonCOC coc in jsonCOCs)
            {
                string CocJsonString = JsonConvert.SerializeObject(coc, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Result[] errors = validator.Validate(CocJsonString, "coc.json");
                List<string> errorList = errors.Select(x => "Invalid " + x.Message.Arguments[0].ToString()).ToList();
                result.Add(Tuple.Create(coc.cocId, errors.Any() ? false : true, String.Join("~", errorList)));
            }
            return result;
        }
        public static List<Tuple<string, bool, string>> ValidateOON(List<McpdOutOfNetwork> oons)
        {
            List<JsonOON> jsonOONs = oons.Select(x => new JsonOON
            {
                planCode = x.PlanCode,
                cin = x.Cin,
                oonId = x.OonId,
                recordType = x.RecordType,
                parentOonId = x.ParentOonId,
                oonRequestReceivedDate = x.OonRequestReceivedDate,
                referralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                oonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                oonRequestResolvedDate = x.OonRequestResolvedDate,
                partialApprovalExplanation = x.PartialApprovalExplanation,
                specialistProviderNpi = x.SpecialistProviderNpi,
                providerTaxonomy = x.ProviderTaxonomy,
                serviceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                serviceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                serviceLocationCity = x.ServiceLocationCity,
                serviceLocationState = x.ServiceLocationState,
                serviceLocationZip = x.ServiceLocationZip,
                serviceLocationCountry = x.ServiceLocationCountry
            }).ToList();
            string OonSchemaFile = File.ReadAllText("JsonSchema\\oon.json");
            var schema = SchemaReader.ReadSchema(OonSchemaFile, "JsonSchema\\oon.json");
            Validator validator = new Validator(schema);
            List<Tuple<string, bool, string>> result = new List<Tuple<string, bool, string>>();
            foreach (JsonOON oon in jsonOONs)
            {
                string OonJsonString = JsonConvert.SerializeObject(oon, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Result[] errors = validator.Validate(OonJsonString, "oon.json");
                List<string> errorList = errors.Select(x =>"Invalid " + x.Message.Arguments[0].ToString()).ToList();
                result.Add(Tuple.Create(oon.oonId, errors.Any() ? false : true, String.Join("~", errorList)));
            }
            return result;
        }
        public static Tuple<bool, string> ValidateMcpdFile(ref string jsonString)
        {
            string McpdSchemaFile = System.IO.File.ReadAllText("JsonSchema\\mcpd.json");
            var schema = SchemaReader.ReadSchema(McpdSchemaFile, "JsonSchema\\mcpd.json");
            Validator validator = new Validator(schema);
            Result[] errors = validator.Validate(jsonString, "mcpd.json");
            List<string> errorList = errors.Select(x => "Invalid " + x.Message.Arguments[0].ToString()).ToList();
            return Tuple.Create(errors.Any()?false:true, String.Join("~", errorList));
        }
        public static Tuple<bool, string> ValidatePcpaFile(ref string jsonString)
        {
            string PcpaSchemaFile = System.IO.File.ReadAllText("JsonSchema\\pcpa.json");
            var schema = SchemaReader.ReadSchema(PcpaSchemaFile, "JsonSchema\\pcpa.json");
            Validator validator = new Validator(schema);
            Result[] errors = validator.Validate(jsonString, "pcpa.json");
            List<string> errorList = errors.Select(x => "Invalid " + x.Message.Arguments[0].ToString()).ToList();
            return Tuple.Create(errors.Any() ? false : true, String.Join("~", errorList));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;
using System.Configuration;
using mcpdipData;
using JsonLib;
using MCPDIP.JsonSchema.Validation;
using MCPDIP.JsonSchema;
using Microsoft.CodeAnalysis.Sarif;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            ////DirectoryInfo di = new DirectoryInfo(ConfigurationManager.AppSettings["IPA_Inbound"]);
            ////foreach (FileInfo fi in di.GetFiles())
            ////{
            ////    if (fi.Extension == ".json")
            ////    {
            ////        processJson(fi);
            ////    }
            ////    else if (fi.Extension == ".xlsx")
            ////    {
            ////        //processExcel(fi);
            ////    }
            ////    else
            ////    {
            ////        string destinationFileName = Path.Combine(ConfigurationManager.AppSettings["IPA_Inbound_Archive"], fi.Name);
            ////        if (File.Exists(destinationFileName)) File.Delete(destinationFileName);
            ////        fi.MoveTo(destinationFileName);
            ////    }
            ////}
            CancellationTokenSource cts = new CancellationTokenSource();
            string OperationMode = ConfigurationManager.AppSettings["OperationMode"];
            string OutboundFolder = ConfigurationManager.AppSettings["IPA_Outbound"];
            DirectoryInfo di = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["IPA_Inbound"], OperationMode));
            foreach (var d in di.GetDirectories())
            {
                foreach (FileInfo fi in d.GetFiles())
                {
                    if (fi.Extension == ".json")
                    {
                        //DataLoadingProcessor.ProcessJson(fi, cts);
                    }
                    else if (fi.Extension == ".xlsx")
                    {
                        DataLoadingProcessor.ProcessExcel(d.Name, OperationMode, OutboundFolder, fi, cts);
                    }

                    string destinationFileName = Path.Combine(ConfigurationManager.AppSettings["IPA_Inbound_Archive"], OperationMode, d.Name, fi.Name);
                    if (File.Exists(destinationFileName)) File.Delete(destinationFileName);
                    fi.MoveTo(destinationFileName);
                }
            }
            cts = null;
        }
        static bool IsValidMcpdFile(FileInfo fi)
        {
            string McpdSchemaFile = File.ReadAllText("JsonSchema\\mcpd.json");
            var schema = SchemaReader.ReadSchema(McpdSchemaFile, "JsonSchema\\mcpd.json");
            string McpdJsonFile = File.ReadAllText(fi.FullName);
            Validator validator = new Validator(schema);
            Result[] errors = validator.Validate(McpdJsonFile, fi.FullName);
            bool result = true;
            if (errors.Any()) result = false;
            return result;
        }
        static bool IsValidPcpaFile(FileInfo fi)
        {
            string PcpaSchemaFile = System.IO.File.ReadAllText("JsonSchema\\pcpa.json");
            var schema = SchemaReader.ReadSchema(PcpaSchemaFile, "JsonSchema\\pcpa.json");
            string PcpaJsonFile = File.ReadAllText(fi.FullName);
            Validator validator = new Validator(schema);
            Result[] errors = validator.Validate(PcpaJsonFile, fi.FullName);
            bool result = true;
            if (errors.Any()) result = false;
            foreach (var error in errors)
            {
                var message = error.Message;
                string message2 = null;
            }
            return result;
        }
        static void processJson(FileInfo fi)
        {
            string ss2 = System.IO.File.ReadAllText(fi.FullName);
            if (fi.Name.ToUpper().Contains("MCPD"))
            {
                if (IsValidMcpdFile(fi))
                {
                    JsonMcpd jsonMcpd = JsonDeserialize.DeserializeJsonMcpd(ref ss2);
                }
            }
            else if (fi.Name.ToUpper().Contains("PCPA"))
            {
                if (IsValidPcpaFile(fi))
                {
                    JsonPcpa jsonPcpa = JsonDeserialize.DeserializeJsonPcpa(ref ss2);
                }
            }
        }
        static void processExcel(FileInfo fi)
        {
            var workBook = new XLWorkbook(fi.FullName);
            var sheetCounts = workBook.Worksheets.Count;
            var sheet = workBook.Worksheet(2); //COC
            var row = sheet.Row(2);
            string IPACode = row.Cell(2).Value.ToString();
            row = sheet.Row(3);
            string IPAName = row.Cell(2).Value.ToString();
            row = sheet.Row(5);
            int cellCount = row.CellCount();
            int rowcount = sheet.RowCount();
            int i = 6;
            string Cin, CocId, RecordType, ParentCocId, CocReceivedDate, CocType, BenefitType, CocDispositionIndicator, CocExpirationDate, CocDenialReasonIndicator, SubmittingProviderNpi, CocProviderNpi, ProviderTaxonomy;
            List<McpdContinuityOfCare> cocs = new List<McpdContinuityOfCare>();
            List<McpdContinuityOfCare> errorCocs = new List<McpdContinuityOfCare>();
            McpdContinuityOfCare coc;
            do
            {
                row = sheet.Row(i);
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
                coc = new McpdContinuityOfCare
                {
                    PlanCode = "305",
                    Cin = Cin,
                    CocId = CocId,
                    ParentCocId = ParentCocId,
                    CocReceivedDate = CocReceivedDate,
                    CocType = CocType,
                    BenefitType = BenefitType,
                    CocDispositionIndicator = CocDispositionIndicator,
                    CocExpirationDate = CocExpirationDate,
                    CocDenialReasonIndicator = CocDenialReasonIndicator,
                    SubmittingProviderNpi = SubmittingProviderNpi,
                    CocProviderNpi = CocProviderNpi,
                    ProviderTaxonomy = ProviderTaxonomy
                };
                if (!string.IsNullOrEmpty(Cin) && !string.IsNullOrEmpty(CocId) && !string.IsNullOrEmpty(RecordType) && !string.IsNullOrEmpty(CocReceivedDate) && !string.IsNullOrEmpty(BenefitType) && !string.IsNullOrEmpty(CocDispositionIndicator))
                {
                    cocs.Add(coc);
                }
                else
                {
                    coc.ErrorMessage = "Missing required elements";
                    errorCocs.Add(coc);
                }
                i++;
            }
            while (!string.IsNullOrEmpty(row.Cell(3).Value.ToString()));
        }
    }
}

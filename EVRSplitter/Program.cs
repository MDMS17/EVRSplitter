using System;
using static System.Console;
using Newtonsoft.Json;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace EVRSplitter
{
    public class Program
    {
        static void Main(string[] args)
        {
            //this is to read start date and end date from hosting application, if not started from hosting application, set it to three years
            string hostStart, hostEnd;
            if (args.Length > 0)
            {
                hostStart = args[0].Split(':')[0];
                hostEnd = args[0].Split(':')[1];
            }
            else
            {
                hostStart = DateTime.Today.AddYears(-3).ToShortDateString();
                hostEnd = DateTime.Today.ToShortDateString();
            }
            //retrieve settings, connection string and destination folder
            string path1 = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string configpath = Path.Combine(Path.GetDirectoryName(path1), "AppConfig.json");
            string configText = File.ReadAllText(configpath);
            ConfigModel config = JsonConvert.DeserializeObject<ConfigModel>(configText);
            string connectionString = config.ConnectionString;

            //prepare log path
            string destinationPath = config.DestinationPath;
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
            string logpath = Path.Combine(destinationPath, "Logs");
            if (!Directory.Exists(logpath)) Directory.CreateDirectory(logpath);

            System.Text.StringBuilder ProcessMessage = new System.Text.StringBuilder();
            ProcessMessage.Append("Start Encounter Response File Splitting " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + Environment.NewLine);

            try
            {
                EVRContext context = new EVRContext(connectionString);

                //prepare staging table EVRSplitterTable
                context.Database.ExecuteSqlCommand(@"insert into JsonDoc_Splitted select a.id,0 from JsonDoc a left join JsonDoc_Splitted b on a.id=b.id where b.id is null");
                context.Database.ExecuteSqlCommand("truncate table EVRSplitterTable");
                System.Text.StringBuilder sb1 = new System.Text.StringBuilder();
                sb1.Append("insert into EVRSplitterTable select a.encounterreferencenumber,substring(a.encounterreferencenumber,4,17) as iehpencounterid,a.encounterstatus,e.id as JsonDocId from dhcsresponse_encounter a inner join dhcsresponse_transaction b on a.Transaction_ID=b.ID inner join DHCSResponse c on c.id=b.[File_ID] inner join jsondoc d on d.FileName=c.EncounterFileName inner join jsondoc_splitted e on e.id=d.ID where e.splitted=0 and d.DateCreated between '" + hostStart + "' and '" + hostEnd + "'");
                context.Database.ExecuteSqlCommand(sb1.ToString());
                context.Database.ExecuteSqlCommand(";with cte as(select *,row_number() over (partition by iehpencounterid order by encounterstatus) as rn from EVRSplitterTable) delete from cte where rn>1");

                //prepare EVR by trading partner
                context.Database.ExecuteSqlCommand(@"if exists (select * from sys.views where name='vEVRByTradingPartner') drop view dbo.vEVRByTradingPartner");
                context.Database.ExecuteSqlCommand(@"create view dbo.vEVRByTradingPartner as select c.TradingPartnerId,count(a.IEHPEncounterId) as Counts from EVRSplitterTable a inner join vEncounterIdentifications b on a.IEHPEncounterId=b.IehpEncounterId cross apply (select top 1 case when b.SubmitterCode in ('46','unkn','Unkwn') then 0 else TradingPartnerId end as TradingPartnerId,case when b.SubmitterCode in ('46','unkn','Unkwn') then 'C2E' else TradingPartnerName end as TradingPartnerName,TradingPartnerCode from vTradingPartners where tradingpartnercode=case when b.SubmitterCode in ('46','unkn') then 'Unkwn' else b.SubmitterCode end order by TradingPartnerId) c group by c.TradingPartnerId");
                List<EVRByTradingPartner> evrByTradingPartners = context.EVRByTradingPartners.ToList();

                //run through each trading partner
                int totalrecords, startNumber, endNumber = 0;
                foreach (EVRByTradingPartner evrByTradingPartner in evrByTradingPartners)
                {
                    ProcessMessage.Append("Processing Trading Partner ID: " + evrByTradingPartner.TradingPartnerId.ToString() + " Total Encounters: " + evrByTradingPartner.Counts.ToString() + Environment.NewLine);
                    Console.WriteLine("Processing Trading Partner ID: " + evrByTradingPartner.TradingPartnerId.ToString() + " Total Encounters: " + evrByTradingPartner.Counts.ToString());
                    //if a single trading partner total records more than 5000, need to split to multiple outputs
                    totalrecords = evrByTradingPartner.Counts;
                    for (int iloop = 0; iloop <= totalrecords / 5000; iloop++)
                    {
                        startNumber = 1 + iloop * 5000;
                        endNumber = (iloop + 1) * 5000;

                        //prepare collect of evr detail records
                        context.Database.ExecuteSqlCommand(@"if exists(select * from sys.views where name='vEVRDetail') drop view dbo.vEVRDetail");
                        context.Database.SetCommandTimeout(900);
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        sb.Append("create view dbo.vEVRDetail as ");
                        sb.Append("select t.TradingPartnerId, case when t.TradingPartnerId=0 then 'C2E' else t.TradingPartnerName end as TradingPartnerName, t.SubmitterClaimIdentification, t.IEHPEncounterID, t.EncounterStatus,t.JsonDocId,b.Severity,b.IssueId,b.IsSNIP,b.Description from (");
                        sb.Append("select e.TradingPartnerId, e.TradingPartnerName,d.SubmitterClaimIdentification,c.EncounterReferenceNumber,c.IEHPEncounterID,c.EncounterStatus,c.JsondocId,row_number() over (order by (c.ID)) as RowNumber ");
                        sb.Append("from EVRSplitterTable c ");
                        sb.Append("inner join vEncounterIdentifications d on d.IehpEncounterId = c.IEHPEncounterID ");
                        sb.Append("cross apply (select top 1 case when d.SubmitterCode in ('46','unkn','Unkwn') then 0 else TradingPartnerId end as TradingPartnerId,case when d.SubmitterCode in ('46','unkn','Unkwn') then 'C2E' else TradingPartnerName end as TradingPartnerName,TradingPartnerCode from vTradingPartners where tradingpartnercode=case when d.SubmitterCode in ('46','unkn') then 'Unkwn' else d.SubmitterCode end order by TradingPartnerId) e ");
                        sb.Append("where e.TradingPartnerId=" + evrByTradingPartner.TradingPartnerId.ToString() + ") t ");
                        sb.Append("left join DHCSResponse_Encounter a on a.EncounterReferenceNumber=t.EncounterReferenceNumber ");
                        sb.Append("left join DHCSResponse_EncounterResponse b on a.ID=b.Encounter_ID ");
                        sb.Append("where RowNumber between " + startNumber.ToString() + " and " + endNumber.ToString());
                        context.Database.ExecuteSqlCommand(sb.ToString());
                        List<EVRDetail> evrDetails = context.EVRDetails.ToList();
                        if (evrDetails.Count == 0) continue;

                        //export to physical file
                        string dir = (Path.Combine(destinationPath, evrDetails[0].TradingPartnerName));
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        string fileName = "RESP_" + DateTime.Now.ToString("yyMMddHHmmssfff") + "_" + evrDetails[0].TradingPartnerId.ToString().PadLeft(3, '0') + ".xml";
                        StreamWriter sw = new StreamWriter(Path.Combine(dir, fileName), true);
                        CreateXMLHeader(ref sw, evrDetails[0]);
                        string SubmitterEID = "";
                        bool hasResponse = false;
                        for (int i = 0; i < evrDetails.Count; i++)
                        {
                            if (i == 0)
                            {
                                CreateXMLEncounter(ref sw, evrDetails[i]);
                                if (!string.IsNullOrEmpty(evrDetails[i].Severity))
                                {
                                    CreateXMLResponseHeader(ref sw);
                                    CreateXMLResponse(ref sw, evrDetails[i]);
                                    hasResponse = true;
                                }
                            }
                            if (i > 0 && SubmitterEID != evrDetails[i].SubmitterClaimIdentification)
                            {
                                if (hasResponse) CreateXMLResponseTrailer(ref sw);
                                CreateXMLEncounterTrailer(ref sw);
                                hasResponse = false;
                                CreateXMLEncounter(ref sw, evrDetails[i]);
                                if (!string.IsNullOrEmpty(evrDetails[i].Severity))
                                {
                                    CreateXMLResponseHeader(ref sw);
                                    CreateXMLResponse(ref sw, evrDetails[i]);
                                    hasResponse = true;
                                }
                            }
                            if (i > 0 && SubmitterEID == evrDetails[i].SubmitterClaimIdentification && !string.IsNullOrEmpty(evrDetails[i].Severity))
                            {
                                CreateXMLResponse(ref sw, evrDetails[i]);
                            }

                            SubmitterEID = evrDetails[i].SubmitterClaimIdentification;
                        }
                        if (hasResponse) CreateXMLResponseTrailer(ref sw);
                        CreateXMLEncounterTrailer(ref sw);
                        CreateXMLTrailer(ref sw);
                        var v1 = evrDetails.Select(x => new { x.JsonDocId }).Distinct().ToList();
                        foreach (var jid in v1)
                        {
                            EncounterTrackModel etm = new EncounterTrackModel();
                            etm.JsonDocId = jid.JsonDocId;
                            etm.TradingPartnerId = evrByTradingPartner.TradingPartnerId;
                            etm.FileName = fileName;
                            etm.CreateDate = DateTime.Now;
                            context.EncounterTrack.Add(etm);
                        }
                    }
                }
                context.SaveChanges();
                sb1.Clear();
                sb1.Append("update JsonDoc_Splitted set splitted=1 from JsonDoc_Splitted a inner join JSONDoc b on a.id=b.ID where splitted=0 and b.DateCreated between '" + hostStart + "' and '" + hostEnd + "'");
                context.Database.ExecuteSqlCommand(sb1.ToString());
                Emails EVREmail = new Emails();
                EVREmail.isHTML = true;
                EVREmail.sendFrom = config.SendEmailFrom;
                EVREmail.sendMessage = ProcessMessage.ToString();
                EVREmail.sendSubject = "EVR Splitter Process Summary";
                EVREmail.sendTo = config.SendEmailTo.Split(';');
                EVREmail.attachments = new List<string>();
                EVREmail.SMTPServer = config.SMTPServer;
                EVREmail.sendEmail();
            }
            catch (Exception ex)
            {
                ProcessMessage.Append(ex.Message + Environment.NewLine);
            }
            ProcessMessage.Append("End Encounter Response File Splitting " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + Environment.NewLine);
            File.AppendAllText(Path.Combine(logpath, "EVRSplitterLog.txt"), ProcessMessage.ToString());
        }

        private static void CreateXMLHeader(ref StreamWriter sw, EVRDetail evrDetail)
        {
            sw.WriteLine(@"<EncounterResponse>");
            sw.WriteLine("\t<SubmitterName>" + evrDetail.TradingPartnerName + @"</SubmitterName>");
            sw.WriteLine("\t<ResponseDateTime>" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff") + @"</ResponseDateTime>");
            sw.WriteLine("\t<Encounters>");
        }
        private static void CreateXMLEncounter(ref StreamWriter sw, EVRDetail evrDetail)
        {
            sw.WriteLine("\t\t<Encounter>");
            sw.WriteLine("\t\t\t<SubmitterClaimIdentification>" + evrDetail.SubmitterClaimIdentification + @"</SubmitterClaimIdentification>");
            sw.WriteLine("\t\t\t<IehpClaimIdentification>" + evrDetail.IEHPEncounterID + @"</IehpClaimIdentification>");
            sw.WriteLine("\t\t\t<EncounterStatus>" + evrDetail.EncounterStatus + @"</EncounterStatus>");
        }
        private static void CreateXMLEncounterTrailer(ref StreamWriter sw)
        {
            sw.WriteLine("\t\t" + @"</Encounter>");
        }
        private static void CreateXMLResponseHeader(ref StreamWriter sw)
        {
            sw.WriteLine("\t\t\t<Responses>");
        }
        private static void CreateXMLResponse(ref StreamWriter sw, EVRDetail evrResponse)
        {
            sw.WriteLine("\t\t\t\t<Response>");
            sw.WriteLine("\t\t\t\t\t<Severity>" + evrResponse.Severity + @"</Severity>");
            sw.WriteLine("\t\t\t\t\t<IssueId>" + evrResponse.IssueId + @"</IssueId>");
            sw.WriteLine("\t\t\t\t\t<IsSNIP>" + evrResponse.IsSNIP.ToString() + @"</IsSNIP>");
            sw.WriteLine("\t\t\t\t\t<Description>" + evrResponse.Description + @"</Description>");
            sw.WriteLine("\t\t\t\t</Response>");
        }
        private static void CreateXMLResponseTrailer(ref StreamWriter sw)
        {
            sw.WriteLine("\t\t\t" + @"</Responses>");
        }
        private static void CreateXMLTrailer(ref StreamWriter sw)
        {
            sw.WriteLine("\t" + @"</Encounters>");
            sw.WriteLine(@"</EncounterResponse>");
            sw.Flush();
            sw.Close();
        }
    }
}

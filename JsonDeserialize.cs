using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mcpdipData;
using Newtonsoft.Json;

namespace JsonLib
{
    public static class JsonDeserialize
    {
        public static TestJsonGrievance DeserializeGrievances(string jsonString)
        {
            return JsonConvert.DeserializeObject<TestJsonGrievance>(jsonString);
        }
        public static List<TestJsonGrievance> DeserializeListOfGrievances(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<TestJsonGrievance>>(jsonString);
        }
        public static TestJsonAppeal DeserializeAppeals(string jsonString)
        {
            return JsonConvert.DeserializeObject<TestJsonAppeal>(jsonString);
        }
        public static List<TestJsonAppeal> DeserializeListOfAppeals(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<TestJsonAppeal>>(jsonString);
        }
        public static TestJsonPcpa DeserializePcpa(string jsonString)
        {
            return JsonConvert.DeserializeObject<TestJsonPcpa>(jsonString);
        }
        public static List<TestJsonPcpa> DeserializeListOfPcpa(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<TestJsonPcpa>>(jsonString);
        }
        public static ResponseFile DeserializeReponseFile(ref string responseFileString)
        {
            return JsonConvert.DeserializeObject<ResponseFile>(responseFileString);
        }
        public static TestJsonCoc DeserializeCoc(string jsonString)
        {
            return JsonConvert.DeserializeObject<TestJsonCoc>(jsonString);
        }
        public static List<TestJsonCoc> DeserializeListOfCoc(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<TestJsonCoc>>(jsonString);
        }
        public static TestJsonOon DeserializeOon(string jsonString)
        {
            return JsonConvert.DeserializeObject<TestJsonOon>(jsonString);
        }
        public static List<TestJsonOon> DeserializeListOfOon(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<TestJsonOon>>(jsonString);
        }
        public static JsonMcpd DeserializeJsonMcpd(ref string jsonString)
        {
            return JsonConvert.DeserializeObject<JsonMcpd>(jsonString);
        }
        public static JsonPcpa DeserializeJsonPcpa(ref string jsonString)
        {
            return JsonConvert.DeserializeObject<JsonPcpa>(jsonString);
        }
    }
}

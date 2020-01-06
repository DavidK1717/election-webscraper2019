#undef DATABASE 
#define CSV
#define JSON

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using CsvHelper;
using Newtonsoft.Json;

namespace WebScraper
{
    
    public class Constituency
    {
        public int conID { get; set; } = 0;
        public string conName { get; set; } = "";
        public string ons { get; set; } = "";
    }

    public class ConstituencyResult
    {
        public int conID { get; set; } =0;
        public int electionID { get; set; } = 0;
        public string winningParty { get; set; } = "";
        public string headline { get; set; } = "";
        public int majority { get; set; } = 0;
        public int electorate { get; set; } = 0;
        public decimal turnout { get; set; } = 0.00M;
        public decimal turnoutChange { get; set; } = 0.00M;
    }

    public class CandidateResult
    {
        public int conID { get; set; } = 0;
        public int electionID { get; set; } = 0;
        public string partyCode { get; set; } = "";
        public string partyName { get; set; } = "";
        public string candidateName { get; set; } = "";
        public int votes { get; set; } = 0;
        public decimal voteShare { get; set; } = 0.00M;
        public decimal voteShareChange { get; set; } = 0.00M;
    }

    public class ConstituencyHead
    {
        public string ons { get; set; } = "";
        public string wp { get; set; } = "";
        public string wpp { get; set; } = "";
        public string flash { get; set; } = "";
    }

    public class ConstituencyHeadData
    {
        public string wp { get; set; } = "";
        public string wpp { get; set; } = "";
        public string flash { get; set; } = "";
    }

    class Program
    {
        static void Main(string[] args)
        {
            var conID_Sim = 0;

            var constituencies = new List<Constituency>();
            var constituencyResults = new List<ConstituencyResult>();
            var candidateResults = new List<CandidateResult>();
            var constituencyHeads = new List<ConstituencyHead>();

            // set up sql server connection

            DataAccess.connectionString = "server=" + "DESKTOP-7UJF7DE" +
                                          ";Trusted_Connection=yes; database=" + "GER";

            // write csv files to this directory

            const string csvDir = "d:\\temp\\";

            // BBC election data root node

            var web = new HtmlWeb();
            var doc = web.Load("https://www.bbc.co.uk/news/politics/constituencies");

            // get list of what gets added to root node to get each constituency result web page

            var conNodes = doc.DocumentNode
                .SelectNodes("//tr[@class='az-table__row']/th/a").ToList();

            // for each constituency page

            var i = 0;

            foreach (var item in conNodes)
            {
                i++;

                // get the web page for the constituency

                var webInner1 = new HtmlWeb();

                var docInner1 = web.Load("https://www.bbc.co.uk/" + item.Attributes["href"].Value);

                // first extract and save json data of winner, previous winner and headline for each constituency
                // this is on every page so only save it once

                if (i == 1)
                {
                    var json = GetHeadlineData(constituencyHeads, docInner1);

#if JSON
                    // save json file - we also save as csv later
                    File.WriteAllText(csvDir + "election_results2019.json", json);
#endif
                }

                Constituency c = GetConstituency(ref conID_Sim, constituencies, item, docInner1);

                // get constituency level vote data

                var conR = GetConstituencyResult(constituencyResults, docInner1, c);

                // get a collection of the nodes for each candidate

                const string xpathCandidates = 
                    "//ol[@class=\'ge2019-constituency-result__list\']" +
                    "//li[starts-with(@class, 'ge2019-constituency-result__item ge2019__party--border ge2019__party--border')]";

                HtmlNodeCollection candidateListItems =
                    docInner1.DocumentNode.SelectNodes(xpathCandidates);

                // loop through candidate list items

                foreach (var candidateListItem in candidateListItems)
                    GetCandidateResult(candidateResults, c, conR, candidateListItem);
            }

#if CSV
            // create csv files. If they already exist they will be overwritten.
            // using UTF8 to cope with accented characters etc

            const string csvPathC = csvDir + "constituencies.csv";
            using (var writer = new StreamWriter(
                new FileStream(csvPathC, FileMode.Create, FileAccess.Write),
                Encoding.UTF8))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(constituencies);
            }

            const string csvPathConH = csvDir + "constituency_headline.csv";
            using (var writer = new StreamWriter(
                new FileStream(csvPathConH, FileMode.Create, FileAccess.Write),
                Encoding.UTF8))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(constituencyHeads);
            }

            const string csvPathConR = csvDir + "constituency_results.csv";
            using (var writer = new StreamWriter(
                new FileStream(csvPathConR, FileMode.Create, FileAccess.Write),
                Encoding.UTF8))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(constituencyResults);
            }

            const string csvPathCanR = csvDir + "candidate_results.csv";
            using (var writer = new StreamWriter(
                new FileStream(csvPathCanR, FileMode.Create, FileAccess.Write), 
                Encoding.UTF8))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(candidateResults);
            }
#endif

        }

        private static void GetCandidateResult(List<CandidateResult> CandidateResults, Constituency c, ConstituencyResult conR, HtmlNode candidateListItem)
        {
            CandidateResult canR = new CandidateResult();

            // get each item of data for candidate

            const string xpathPartyCode = 
                ".//div[@class='ge2019-constituency-result__row']" +
                "/div[@class='ge2019-constituency-result__party']" +
                "/span[@class='ge2019-constituency-result__party-code']";

            canR.partyCode =
                candidateListItem.SelectNodes(xpathPartyCode)
                    .FirstOrDefault()
                    .InnerText;

            const string xpathPartyName =
                ".//div[@class='ge2019-constituency-result__row']" +
                "/div[@class='ge2019-constituency-result__party']" +
                "/span[@class='ge2019-constituency-result__party-name']";

            canR.partyName =
                candidateListItem.SelectNodes(xpathPartyName)
                    .FirstOrDefault()
                    .InnerText;

            const string xpathCandidateName = 
                ".//div[@class='ge2019-constituency-result__row']" +
                "/div[@class='ge2019-constituency-result__candidate']" +
                "/span[@class='ge2019-constituency-result__candidate-name']";

            canR.candidateName =
                candidateListItem.SelectNodes(xpathCandidateName)
                    .FirstOrDefault()
                    .InnerText;

            // 3 list items not individually named so loop through them

            const string xpathDetails = 
                ".//div[@class='ge2019-constituency-result__details']" +
                "/ul[@class='ge2019-constituency-result__details-list']" +
                "/li[@class='ge2019-constituency-result__details-item']" +
                "/span[@class='ge2019-constituency-result__text-wrapper']" +
                "/span[@class='ge2019-constituency-result__details-value']";

            var voteNodes =
                candidateListItem.SelectNodes(xpathDetails);

            var a = 0;

            foreach (var v in voteNodes)
            {
                a++;
                if (a == 1)
                    canR.votes = Convert.ToInt32(v.InnerText.Replace(",", ""));
                else if (a == 2)
                    canR.voteShare = System.Convert.ToDecimal(v.InnerText);
                else if (a == 3)
                    canR.voteShareChange = System.Convert.ToDecimal(v.InnerText);
            }


            var space = "              ";
            Console.WriteLine(space + canR.partyCode);
            Console.WriteLine(space + canR.partyName);
            Console.WriteLine(space + canR.candidateName);
            Console.WriteLine(space + canR.votes);
            Console.WriteLine(space + canR.voteShare);
            Console.WriteLine(space + canR.voteShareChange);

            canR.conID = c.conID;
            canR.electionID = conR.electionID;

            CandidateResults.Add(canR);


            // write candidate record

#if DATABASE
                    try
                    {
                        using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                        {
                            cn.Open();
                            SqlCommand cmd = new SqlCommand("add_candidate_result", cn);

                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 600;
                            cmd.Parameters.Add(new SqlParameter("@con_id",
                                SqlDbType.Int));
                            cmd.Parameters.Add(new SqlParameter("@election_id",
                                SqlDbType.Int));
                            cmd.Parameters.Add(new SqlParameter("@party_code",
                                SqlDbType.NVarChar));
                            cmd.Parameters.Add(new SqlParameter("@party",
                                SqlDbType.NVarChar));
                            cmd.Parameters.Add(new SqlParameter("@candidate",
                                SqlDbType.NVarChar));
                            cmd.Parameters.Add(new SqlParameter("@votes",
                                SqlDbType.Int));
                            cmd.Parameters.Add(new SqlParameter("@vote_share",
                                SqlDbType.Decimal));
                            cmd.Parameters.Add(new SqlParameter("@vote_share_change",
                                SqlDbType.Decimal));

                            cmd.Parameters[0].Value = c.conID;
                            cmd.Parameters[1].Value = canR.electionID;
                            cmd.Parameters[2].Value = canR.partyCode;
                            cmd.Parameters[3].Value = canR.partyName;
                            cmd.Parameters[4].Value = canR.candidateName;
                            cmd.Parameters[5].Value = canR.votes;
                            cmd.Parameters[6].Value = canR.voteShare;
                            cmd.Parameters[7].Value = canR.voteShareChange;

                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        DataAccess.SqlExceptionHandler(ex);
                    }
#endif
#if !DATABASE && !CSV
                    Console.ReadLine();
#endif

        }

        private static ConstituencyResult GetConstituencyResult(List<ConstituencyResult> ConstituencyResults, HtmlDocument docInner1, Constituency c)
        {
            ConstituencyResult conR = new ConstituencyResult();

            const string xpathWinningParty = 
                "//span[@class='ge2019-constituency-result-turnout__block ge2019-constituency-result-turnout__majority']"
                + "/span[@class='ge2019-constituency-result-turnout__label']";

            var winningParty =
                docInner1.DocumentNode.SelectNodes(xpathWinningParty)
                    .FirstOrDefault()
                    .InnerText;

            conR.winningParty = winningParty.Substring(0, winningParty.IndexOf(" "));

            const string xpathHeadline = "//p[@class='ge2019-constituency-result-headline__text']";

            var headline =
                docInner1.DocumentNode.SelectNodes(xpathHeadline)
                    .FirstOrDefault()
                    .InnerText;

            conR.headline = headline.Substring(conR.winningParty.Length + 1);

            const string xpathMajority = 
                "//span[@class='ge2019-constituency-result-turnout__block ge2019-constituency-result-turnout__majority']"
                + "/span[@class='ge2019-constituency-result-turnout__value']";

            var majority =
                docInner1.DocumentNode.SelectNodes(xpathMajority)
                    .FirstOrDefault()
                    .InnerText;

            conR.majority = Convert.ToInt32(majority.Replace(",", ""));

            const string xpathElectorate = 
                "//span[@class='ge2019-constituency-result-turnout__block ge2019-constituency-result-turnout__electorate']"
                + "/span[@class='ge2019-constituency-result-turnout__value']";

            var electorate =
                docInner1.DocumentNode.SelectNodes(xpathElectorate)
                    .FirstOrDefault()
                    .InnerText;

            conR.electorate = Convert.ToInt32(electorate.Replace(",", ""));


            const string xpathTurnout = 
                "//span[@class='ge2019-constituency-result-turnout__block ge2019-constituency-result-turnout__percentage']"
                + "/span[@class='ge2019-constituency-result-turnout__value']";

            var turnout =
                docInner1.DocumentNode.SelectNodes(xpathTurnout)
                    .FirstOrDefault()
                    .InnerText;

            turnout = turnout.Substring(0, turnout.Length - 1);
            conR.turnout = System.Convert.ToDecimal(turnout);


            const string xpathTurnoutChange = 
                "//span[@class='ge2019-constituency-result-turnout__block ge2019-constituency-result-turnout__change']"
                + "/span[@class='ge2019-constituency-result-turnout__value']";

            var turnoutChange =
                docInner1.DocumentNode.SelectNodes(xpathTurnoutChange)
                    .FirstOrDefault()
                    .InnerText;

            conR.turnoutChange = System.Convert.ToDecimal(turnoutChange);


            conR.conID = c.conID;
            conR.electionID = 3;

            ConstituencyResults.Add(conR);

            Console.WriteLine("CONSTITUENCY");
            Console.WriteLine("============");
            Console.WriteLine(c.ons);
            Console.WriteLine(c.conName);
            Console.WriteLine(c.conID);
            Console.WriteLine("Winner: " + conR.winningParty + " " + conR.headline);
            Console.WriteLine("Majority: " + conR.majority);
            Console.WriteLine("Electorate: " + conR.electorate);
            Console.WriteLine("Turnout: " + conR.turnout);
            Console.WriteLine("Turnout change: " + conR.turnoutChange);
            Console.WriteLine("              CANDIDATES");
            Console.WriteLine("              **********");


#if DATABASE
                // write constituency result record    
                try
                {
                    using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                    {
                        cn.Open();
                        SqlCommand cmd = new SqlCommand("add_constituency_result", cn);

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.Add(new SqlParameter("@con_id",
                            SqlDbType.Int));
                        cmd.Parameters.Add(new SqlParameter("@election_id",
                            SqlDbType.Int));
                        cmd.Parameters.Add(new SqlParameter("@winning_party",
                                    SqlDbType.NVarChar));
                        cmd.Parameters.Add(new SqlParameter("@headline",
                                    SqlDbType.NVarChar));                
                        cmd.Parameters.Add(new SqlParameter("@majority",
                            SqlDbType.Int));
                        cmd.Parameters.Add(new SqlParameter("@electorate",
                            SqlDbType.Int));
                        cmd.Parameters.Add(new SqlParameter("@turnout",
                            SqlDbType.Decimal));
                        cmd.Parameters.Add(new SqlParameter("@turnout_change",
                            SqlDbType.Decimal));

                        cmd.Parameters[0].Value = c.conID;
                        cmd.Parameters[1].Value = conR.electionID;
                        cmd.Parameters[2].Value = conR.winningParty;
                        cmd.Parameters[3].Value = conR.headline;
                        cmd.Parameters[4].Value = conR.majority;
                        cmd.Parameters[5].Value = conR.electorate;
                        cmd.Parameters[6].Value = conR.turnout;
                        cmd.Parameters[7].Value = conR.turnoutChange;

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    DataAccess.SqlExceptionHandler(ex);

                }
#endif
            return conR;
        }

        private static Constituency GetConstituency(ref int conID_Sim, List<Constituency> Constituencies, HtmlNode item, HtmlDocument docInner1)
        {
            var fullTitle = docInner1.DocumentNode.SelectSingleNode("//head/title").InnerText;

            var c = new Constituency
            {
                // extract ONS code and constituency name

                ons = item.Attributes["href"].Value.Substring(30, 9),

                conName = fullTitle.Substring(0, fullTitle.IndexOf(" parliamentary"))
            };
            
#if !DATABASE
            conID_Sim += 1;  // simulate database autogenerated PK
            c.conID = conID_Sim;
#endif

            Constituencies.Add(c);

#if DATABASE
                // write constituency record

                try
                {
                    using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                    {
                        cn.Open();
                        SqlCommand cmd = new SqlCommand("add_constituency", cn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.Add(new SqlParameter("@ons",
                            SqlDbType.NChar));
                        cmd.Parameters.Add(new SqlParameter("@con_name",
                            SqlDbType.NVarChar));
                        cmd.Parameters[0].Value = c.ons;
                        cmd.Parameters[1].Value = c.conName;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    DataAccess.SqlExceptionHandler(ex);
                }

                // get new con id from database

                try
                {
                    string sql = "select ConstituencyID from Constituency where ONS_code = '" + c.ons + "'";

                    object retval = DataAccess.GetSingleValueSql(sql);

                    if (retval == null)
                    {
                        Console.WriteLine("Database access failed: constituency ID");
                    }
                    else
                    {
                        c.conID = (int)retval;
                    }
                }
                catch (SqlException ex)
                {
                    DataAccess.SqlExceptionHandler(ex);
                }
#endif
            return c;
        }

        private static string GetHeadlineData(List<ConstituencyHead> ConstituencyHeads, HtmlDocument docInner1)
        {
            const string xpathJson = "//div[@id='map_data2019_en']/script";

            var json =
                docInner1.DocumentNode.SelectNodes(xpathJson)
                    .LastOrDefault()
                    .InnerText;

            var startAt = json.IndexOf("var map_data = ") + 15;

            var stringLength = json.Length -
                (json.IndexOf("var map_data =") + 15 +
                (json.Length - json.IndexOf("}, };")));

            json = json.Substring(startAt, stringLength) + "}}";

            // Because of the way the json is structured with the root key itself containing the 
            // ons code, we cannot directly deserialize to a list of objects. Instead we deserialize to
            // a dictionary and then populate the list via the dictionary.

            var dic = JsonConvert.DeserializeObject<Dictionary<string, ConstituencyHeadData>>(json);

            foreach (var pair in dic)
            {
                var ch = new ConstituencyHead
                {
                    ons = pair.Key,
                    wp = pair.Value.wp,
                    wpp = pair.Value.wpp,
                    flash = pair.Value.flash
                };
                ConstituencyHeads.Add(ch);
            }

            return json;
        }
    }

    // SQL Server helper methods
    public class DataAccess
    {
        public static string connectionString;
        
        public static void SaveException(string ex)
        {
            DataAccess.ExecuteStoredProc_stringParam("save_exception", ex, "@ex", 4000);
        }

        public static void ExceptionHandler(Exception ex, string title)
        {
            Console.WriteLine("[" + ex.ToString() + "]");

        }

        public static void SqlExceptionHandler(SqlException ex)
        {
            string message = ex.ToString();

            if (ex.Errors.Count == 1)
            {
                if (!ex.Errors[0].Procedure.Equals(string.Empty))
                {
                    message += "\n\nStored procedure: " + ex.Errors[0].Procedure
                        + "\nLine number: " + ex.Errors[0].LineNumber;
                }
            }
            else
            {
                for (int x = 0; x < ex.Errors.Count; x++)
                {
                    message += "\n\nSQL Server Error " + x.ToString() +
                        "\n---------------\n";
                    if (!ex.Errors[x].Procedure.Equals(string.Empty))
                    {
                        message += "Stored procedure: " + ex.Errors[x].Procedure +
                            "\nLine number: " + ex.Errors[x].LineNumber + "\n";
                    }
                    message += "Error message: " + ex.Errors[x];
                }
            }
            Console.WriteLine(message);

        }

       public static object GetSingleValueSql(string sqlString)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(sqlString, cn)
                    {
                        CommandType = CommandType.Text
                    };

                    return cmd.ExecuteScalar();

                }
            }
            catch (SqlException Sqlex)
            {
                Console.WriteLine("Database problem: ["
                    + Sqlex.ToString() + "]");
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[" + ex.ToString() + "]");
                return -1;
            }
        }

        public static bool ExecuteStoredProc_stringParam(string spName, string param, string paramName, int size)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(DataAccess.connectionString))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(spName, cn);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.Add(new SqlParameter(paramName,
                        SqlDbType.NVarChar, size));
                    cmd.Parameters[0].Value = param;

                    cmd.ExecuteNonQuery();

                    return true;
                }
            }

            catch (SqlException Sqlex)
            {
                Console.WriteLine("Database problem: ["
                                  + Sqlex.ToString() + "]");
                return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine("[" + ex.ToString() + "]");
                return false;
            }
        }
    }
}

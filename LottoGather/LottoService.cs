using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SODA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LottoGather
{
    public class LottoService
    {
        private static LottyDBEntities lottyContext;
        private static int status;

        private WebClient client;
        private Stream stream;
        private StreamReader reader;
        private const String POWERBALL_URI = "http://www.powerball.com/powerball/winnums-text.txt";
        private const String MEGAMILLIONS_API = "data.ny.gov";
        private const String SODA_RESOURCE = "5xaw-6ayf";
        private const String SODA_APP_TOKEN = "seF3hIO1X4xS0jOC1eexLedeE";
        
        public static void Main(string[] args)
        {
            new LottoService().Start(args);
        }

        public int Start(string[] args)
        {
            LottoService lottoSeriveIns = new LottoService();
            lottyContext = new LottyDBEntities();

            //Initialize Powerball table if no records exist
            if (lottyContext.Powerball_Numbers.Count() < 1)
            {
                status = lottoSeriveIns.InitializePowerBallTable(lottyContext.Powerball_Numbers);
            }

            //Initialize Powerball table if no records exist
            if (lottyContext.MegaMillions_Numbers.Count() < 1)
            {
                status = lottoSeriveIns.InitializeMegaMillionTable(lottyContext.MegaMillions_Numbers);
            }

            //insert new drawing to table
            status = lottoSeriveIns.InsertPowerBallWinning(lottyContext.Powerball_Numbers);

            //insert new drawing to table
            status = lottoSeriveIns.InsertMegaMillionWinning(lottyContext.MegaMillions_Numbers);

            return status;
        }

        private int InitializePowerBallTable(DbSet<Powerball_Numbers> contextPB)
        {
            client = new WebClient();

            try
            {
                stream = client.OpenRead(POWERBALL_URI);
                reader = new StreamReader(stream);

                //ignore first(header) line
                String line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
#if DEBUG
                    Debug.WriteLine(line);
#endif
                    /* 6/19/2015 data currently gathered for powerball from
                     * http://www.powerball.com/powerball/winnums-text.txt where
                     * the format is as follows:
                     *   lines[0]    [1] [2] [3] [4] [5] [6] [7]
                     *   Draw Date   WB1 WB2 WB3 WB4 WB5 PB  PP
                     *   06/13/2015  41  29  52  54  48  29  2
                    */
                    int powerPlay = 0;
                    String[] lineS = Regex.Split(line, @"\s+");
                    Powerball_Numbers powerBallDO = new Powerball_Numbers();
                    powerBallDO.Date = DateTime.Parse(lineS[0]);
                    powerBallDO.N1 = Int32.Parse(lineS[1]);
                    powerBallDO.N2 = Int32.Parse(lineS[2]);
                    powerBallDO.N3 = Int32.Parse(lineS[3]);
                    powerBallDO.N4 = Int32.Parse(lineS[4]);
                    powerBallDO.N5 = Int32.Parse(lineS[5]);
                    powerBallDO.PB = Int32.Parse(lineS[6]);
                    if (Int32.TryParse(lineS[7], out powerPlay))
                    {
                        powerBallDO.PP = powerPlay;
                    }
                    contextPB.Add(powerBallDO);

                }//while
                lottyContext.SaveChanges();
            }
            catch(Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                return -1;
            }

            return 0;
        }

        private int InsertPowerBallWinning(DbSet<Powerball_Numbers> contextPB)
        {
            client = new WebClient();

            try
            {
                stream = client.OpenRead(POWERBALL_URI);
                reader = new StreamReader(stream);

                //ignore first(header) line
                String line = reader.ReadLine();
                line = reader.ReadLine();

#if DEBUG
                Debug.WriteLine(line);
#endif
                /* 6/19/2015 data currently gathered for powerball from
                     * http://www.powerball.com/powerball/winnums-text.txt where
                     * the format is as follows:
                     *   lines[0]    [1] [2] [3] [4] [5] [6] [7]
                     *   Draw Date   WB1 WB2 WB3 WB4 WB5 PB  PP
                     *   06/13/2015  41  29  52  54  48  29  2
                    */
                String[] lineS = Regex.Split(line, @"\s+");
                DateTime drawDate = DateTime.Parse(lineS[0]);

                //check if drawing is already in table by checking draw date, if not (default) insert new record
                if (contextPB.Where(w => w.Date == drawDate).Select(s => s.Date).FirstOrDefault() == DateTime.MinValue)
                {
                    int powerPlay = 0;
                    Powerball_Numbers powerBallDO = new Powerball_Numbers();
                    powerBallDO.Date = drawDate;
                    powerBallDO.N1 = Int32.Parse(lineS[1]);
                    powerBallDO.N2 = Int32.Parse(lineS[2]);
                    powerBallDO.N3 = Int32.Parse(lineS[3]);
                    powerBallDO.N4 = Int32.Parse(lineS[4]);
                    powerBallDO.N5 = Int32.Parse(lineS[5]);
                    powerBallDO.PB = Int32.Parse(lineS[6]);
                    if (Int32.TryParse(lineS[7], out powerPlay))
                    {
                        powerBallDO.PP = powerPlay;
                    }
                    contextPB.Add(powerBallDO);
                    lottyContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                return -1;
            }

            return 0;
        }

        private int InitializeMegaMillionTable(DbSet<MegaMillions_Numbers> contextMM)
        {
            try
            {
                //use of SodaDotNet for implementing API for mega million data
                //initialize a new client
                var client = new SodaClient(MEGAMILLIONS_API, SODA_APP_TOKEN);

                //read metadata of a dataset using the SODA_RESOURCE identifier (Socrata 4x4)
                //var metadata = client.GetMetadata(SODA_RESOURCE);

                //get a reference to the SODA_RESOURCE itself
                //the result (a Resouce object) is a generic type
                //the type parameter represents the underlying rows of the SODA_RESOURCE
                //of course, a custom type can be used as long as it is JSON serializable
                var dataset = client.GetResource<MegaMillionsModel>(SODA_RESOURCE);

                //Resource objects read their own data
                //A limit has to be included or defaults to 1000 which isn't enough
                //when initializing the table
                var allRows = dataset.GetRows(5000);

                foreach (var row in allRows)
                {
#if DEBUG
                    Debug.WriteLine(JObject.FromObject(row).ToString(Formatting.None));
#endif
                   /* 6/19/2015 data currently gathered for powerball from
                     * https://data.ny.gov/SODA_RESOURCE/5xaw-6ayf.json where
                     * the format is as follows:
                     *   Draw Date   Winning Numbers MB  MN
                     *   06/13/2015  41 29 52 54 48  29  2
                    */
                    String[] winningNumbers = Regex.Split(row.Numbers, @"\s+");
                    MegaMillions_Numbers megaMillionsDO = new MegaMillions_Numbers();
                    megaMillionsDO.Date = row.Date;
                    megaMillionsDO.N1 = Int32.Parse(winningNumbers[0]);
                    megaMillionsDO.N2 = Int32.Parse(winningNumbers[1]);
                    megaMillionsDO.N3 = Int32.Parse(winningNumbers[2]);
                    megaMillionsDO.N4 = Int32.Parse(winningNumbers[3]);
                    megaMillionsDO.N5 = Int32.Parse(winningNumbers[4]);
                    megaMillionsDO.MB = row.MB;
                    megaMillionsDO.MN = row.MN;

                    contextMM.Add(megaMillionsDO);

                }//foreach
                lottyContext.SaveChanges();

            }
            catch(Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                return -1;
            }

            return 0;
        }

        private int InsertMegaMillionWinning(DbSet<MegaMillions_Numbers> contextMM)
        {
            try
            {
                //use of SodaDotNet for implementing API for mega million data
                //initialize a new client
                var client = new SodaClient(MEGAMILLIONS_API, SODA_APP_TOKEN);

                //read metadata of a dataset using the SODA_RESOURCE identifier (Socrata 4x4)
                //var metadata = client.GetMetadata(SODA_RESOURCE);

                //get a reference to the SODA_RESOURCE itself
                //the result (a Resouce object) is a generic type
                //the type parameter represents the underlying rows of the SODA_RESOURCE
                //of course, a custom type can be used as long as it is JSON serializable
                var dataset = client.GetResource<MegaMillionsModel>(SODA_RESOURCE);

                //query to get latest drawing from table
                var soql = new SoqlQuery().Select("*").Order(SoqlOrderDirection.DESC).Limit(1);

                //Resource objects read their own data
                var row = dataset.Query(soql).First();

#if DEBUG
                Debug.WriteLine(JObject.FromObject(row).ToString(Formatting.None));
#endif
                /* 6/19/2015 data currently gathered for powerball from
                    * https://data.ny.gov/SODA_RESOURCE/5xaw-6ayf.json where
                    * the format is as follows:
                    *   Draw Date   Winning Numbers MB  MN
                    *   06/13/2015  41 29 52 54 48  29  2
                */
                String[] winningNumbers = Regex.Split(row.Numbers, @"\s+");
                DateTime drawDate = row.Date;

                //check if drawing is already in table by checking draw date, if not (default) insert new record
                if (contextMM.Where(w => w.Date == drawDate).Select(s => s.Date).FirstOrDefault() == DateTime.MinValue)
                {
                    MegaMillions_Numbers megaMillionsDO = new MegaMillions_Numbers();
                    megaMillionsDO.Date = row.Date;
                    megaMillionsDO.N1 = Int32.Parse(winningNumbers[0]);
                    megaMillionsDO.N2 = Int32.Parse(winningNumbers[1]);
                    megaMillionsDO.N3 = Int32.Parse(winningNumbers[2]);
                    megaMillionsDO.N4 = Int32.Parse(winningNumbers[3]);
                    megaMillionsDO.N5 = Int32.Parse(winningNumbers[4]);
                    megaMillionsDO.MB = row.MB;
                    megaMillionsDO.MN = row.MN;
                    contextMM.Add(megaMillionsDO);
                    lottyContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                return -1;
            }

            return 0;
        }

    }
}

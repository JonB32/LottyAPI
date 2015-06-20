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

            //insert new drawwing to table
            status = lottoSeriveIns.InsertPowerBallWinning(lottyContext.Powerball_Numbers);

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
                    String[] lineS = Regex.Split(line, @"\s+");
                    Powerball_Numbers powerBallDO = new Powerball_Numbers();
                    powerBallDO.Date = DateTime.Parse(lineS[0]);
                    powerBallDO.N1 = Int32.Parse(lineS[1]);
                    powerBallDO.N2 = Int32.Parse(lineS[2]);
                    powerBallDO.N3 = Int32.Parse(lineS[3]);
                    powerBallDO.N4 = Int32.Parse(lineS[4]);
                    powerBallDO.N5 = Int32.Parse(lineS[5]);
                    powerBallDO.PB = Int32.Parse(lineS[6]);
                    powerBallDO.PP = Int32.Parse(lineS[7]);
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

                //check if drawing is already in table by checking draw date, if not (null) insert new record
                if (contextPB.Where(w => w.Date == drawDate).Select(s => s.Date).First() == null)
                {
                    Powerball_Numbers powerBallDO = new Powerball_Numbers();
                    powerBallDO.Date = drawDate;
                    powerBallDO.N1 = Int32.Parse(lineS[1]);
                    powerBallDO.N2 = Int32.Parse(lineS[2]);
                    powerBallDO.N3 = Int32.Parse(lineS[3]);
                    powerBallDO.N4 = Int32.Parse(lineS[4]);
                    powerBallDO.N5 = Int32.Parse(lineS[5]);
                    powerBallDO.PB = Int32.Parse(lineS[6]);
                    powerBallDO.PP = Int32.Parse(lineS[7]);
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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Xml;

namespace KyLotto
{
    class Win
    {
        public int ID { get; set; }

        public DateTime Date { get; set; }
    }

    class Numbers
    {
        public int ID { get; set; }

        public int Win { get; set; }

        public int Number { get; set; }

        public bool MegaBall { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var connection = new SqlConnection("Data Source=win7-pc;User ID=sa;Password=peppers1234;Initial Catalog=KyLotto;"))
                {
                    connection.Open();

                    XmlDocument doc = new XmlDocument();
                    doc.Load(@"C:\Users\User\Desktop\MegaMillions.xml");

                    connection.Execute("DELETE FROM [Wins]");
                    connection.Execute("DELETE FROM [Number]");

                    int winCount = 0, numberCount = 0;
                    foreach (XmlNode element in doc.FirstChild.ChildNodes)
                    {
                        connection.Execute("INSERT INTO [Wins] (ID, Date) VALUES (@ID, @Date)", new Win
                        {
                            ID = ++winCount,
                            Date = DateTime.Parse(element.FirstChild.InnerText),
                        });

                        foreach (var number in element.ChildNodes[1].InnerText.Split('-'))
                        {
                            connection.Execute("INSERT INTO [Number] (ID, Win, Number, MegaBall) VALUES (@ID, @Win, @Number, @MegaBall)", new Numbers
                            {
                                ID = ++numberCount,
                                Win = winCount,
                                Number = int.Parse(number),
                                MegaBall = false,
                            });
                        }

                        connection.Execute("INSERT INTO [Number] (ID, Win, Number, MegaBall) VALUES (@ID, @Win, @Number, @MegaBall)", new Numbers
                        {
                            ID = ++numberCount,
                            Win = winCount,
                            Number = int.Parse(element.ChildNodes[2].InnerText.Replace("Mega Ball ", string.Empty)),
                            MegaBall = true,
                        });
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }

            Console.WriteLine("DONE!");
            Console.ReadLine();
        }
    }
}

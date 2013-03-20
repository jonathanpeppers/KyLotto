using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;
using System.Xml;
using System.IO;
using System.Reflection;

namespace KyLotto
{
    class Win
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public DateTime Date { get; set; }
    }

    class Numbers
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int Win { get; set; }

        public int Number { get; set; }

        public bool MegaBall { get; set; }
    }

    class Result
    {
        public int Total { get; set; }

        public int Number { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "kylotto.db3");

                if (File.Exists(path))
                    File.Delete(path);

                using (var connection = new SQLiteConnection(path))
                {
                    connection.CreateTable<Win>();
                    connection.CreateTable<Numbers>();

                    KyPowerball(connection);
                    //MegaMillions(connection);

                    //Print best numbers
                    Console.WriteLine("Wins\tNumber");
                    foreach (var result in connection.Query<Result>("select count(ID) as Total, Number from Numbers group by Number order by Total desc"))
                    {
                        Console.WriteLine(result.Total + "\t" + result.Number);
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

        private static void KyPowerball(SQLiteConnection connection)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "Powerball.txt");

            using (var stream = File.OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        var split = line.Split('\t');

                        var win = new Win
                        {
                            Date = DateTime.Parse(split[0]),
                        };
                        connection.Insert(win);

                        var numbers = split[1].Split('–');
                        for (int i = 0; i < numbers.Length; i++)
                        {
                            connection.Insert(new Numbers
                            {
                                Number = int.Parse(numbers[i].Trim()),
                                Win = win.ID,
                                MegaBall = i == 5,
                            });
                        }
                    }
                }
            }
        }

        private static void MegaMillions(SQLiteConnection connection)
        {
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
}

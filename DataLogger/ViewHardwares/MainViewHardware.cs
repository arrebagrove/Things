using FileDbNs;
using IoT.Hardwares.Base;
using IoT.Services;
using IoT.ViewHardwares.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.IO;
using Windows.Storage.Streams;
using Windows.Storage;
using SQLite.Net.Attributes;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;

namespace DataLogger.ViewHardwares
{
    public class MainViewHardware : ViewHardware
    {
        public string Temperature = "24";

        public async override void Setup()
        {
            //Case one: Write a file per data input
            //CaseOne();

            //Case two: Add content to a file
            //CaseTwo();

            //Case three: FileDB
            //CaseThree();

            //Case four: SQlite
            CaseFour();
            await Copy();

        }

        public void CaseOne()
        {
            DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
            dt.Tick += async (s, e) =>
            {
                var file = await FileService.FindFile($"data_{FileService.GetDateTime()}.dat", true);

                if (file != null)
                {
                    await FileService.WriteFile(file, $"{Temperature}_{FileService.GetDateTime()}");
                }
            };
            dt.Start();
        }

        public async void CaseTwo()
        {
            DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

            var file = await FileService.FindFile($"data_{FileService.GetDate()}.dat", true);
            dt.Tick += async (s, e) =>
            {
                if (file != null)
                {
                    await FileService.AddContentToFile(file, $"{Temperature}_{FileService.GetDateTime()}");
                }
            };
            dt.Start();
        }

        #region Case Three
        GreenSensorModel sensor = new GreenSensorModel()
        {
            Id = 0,
            Humidity = 10,
            Lightness = 10,
            Pressure = 1024,
            Temperature = 24
        };

        GreenSensorModel Sensor
        {
            get { return sensor; }
            set
            {
                sensor = value;
                NotifyPropertyChanged();
            }
        }

        StorageFile file;
        FileDb GreenHouseDb;
        public async void CaseThree()
        {
            await CheckDatabase();

            for (int i = 0; i < 10; i++)
            {
                AddRecord(Sensor);
                await Task.Delay(1000);
                Sensor.TimeStamp = DateTime.Now;
                Sensor.Temperature += 4;
            }

            GreenHouseDb.Close();
        }

        private async Task CheckDatabase()
        {
            string name = "greenhouse.dat";
            file = await FileService.FindFile(name, false);

            if (file == null)
            {
                CreateDatabase(name);
            }
            else
            {
                GreenHouseDb = new FileDb();
                var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                GreenHouseDb.Open(stream.AsStream());
            }
        }

        private async void CreateDatabase(string name)
        {
            var file = await FileService.CreateFile(name);
            var stream = (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStream();

            Field field;
            var fields = new List<Field>(20);

            field = new Field("Id", DataTypeEnum.Int32)
            {
                AutoIncStart = 0,
                IsPrimaryKey = true
            };
            fields.Add(field);

            fields.Add(new Field("Temperature", DataTypeEnum.Double));
            fields.Add(new Field("Pressure", DataTypeEnum.Double));
            fields.Add(new Field("Lightness", DataTypeEnum.Double));
            fields.Add(new Field("Humidity", DataTypeEnum.Double));
            fields.Add(new Field("TimeStamp", DataTypeEnum.DateTime));

            GreenHouseDb = new FileDb();
            GreenHouseDb.Create(stream,fields.ToArray());
        }
        public void AddRecord(GreenSensorModel model)
        {
            var record = new FieldValues();
            record.Add("Id", model.Id);
            record.Add("Temperature", model.Temperature);
            record.Add("Pressure", model.Pressure);
            record.Add("Lightness", model.Lightness);
            record.Add("Humidity", model.Humidity);
            record.Add("TimeStamp", model.TimeStamp);

            if(GreenHouseDb!=null)
            {
                GreenHouseDb.AddRecord(record);
            }
        }
        #endregion

        #region Case Four
        

        public bool TableExists<T>(SQLiteConnection connection)
        {
            var tableName = typeof(T).Name;
            var info = connection.GetTableInfo(tableName);
            return info.Any();
        }

        SQLiteConnection connection;
        public void CaseFour()
        {
            string dbPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            string dbName = "greenhouse.sqlite";
            connection = new SQLiteConnection(new SQLitePlatformWinRT(), Path.Combine(dbPath, dbName));

            if(!TableExists<GreenSensorLiteModel>(connection))
            {
                connection.CreateTable<GreenSensorLiteModel>();
            }

            for (int i = 0; i < 10; i++)
            {
                connection.Insert(new GreenSensorLiteModel()
                {
                    Humidity = 10, Pressure = 10, Temperature = 24 + i, Lightness = 10, TimeStamp = DateTime.Now
                });
            }

            connection.Commit();
            connection.Close();
        }

        public async Task Copy()
        {
            string dbPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            string dbName = "greenhouse.sqlite";

            var storagefile = await StorageFile.GetFileFromPathAsync(Path.Combine(dbPath, dbName));

            var destinationfile = await FileService.FindFile(dbName.Replace("sqlite","dat"),true);

            if (storagefile!=null)
            {
                await storagefile.CopyAndReplaceAsync(destinationfile);
            }
        }
        #endregion
    }

    public class GreenSensorModel
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Lightness { get; set; }
        public double Humidity { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class GreenSensorLiteModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Lightness { get; set; }
        public double Humidity { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}

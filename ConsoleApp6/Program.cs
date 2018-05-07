using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.IO;


namespace ConsoleApp5
{
    class Program
    {
        static void Main(string[] args)
        {
            //reads and store JSON file into a string
            string json = new StreamReader(@"C:\Users\Vince\Desktop\jsontest\GPS1.json").ReadToEnd();

            //Deseralizes json string to seperate and store data
            RootObject test = JsonConvert.DeserializeObject<RootObject>(json);

            int Id = 0;
            int Pos_Id = test.Updates[0].ID;
            int VP_Id = 1;
            double Latitude = test.Updates[0].GeometryX.Y;
            double Longitude = test.Updates[0].GeometryX.X;

            //optional values that may be null
            double? odometer = null;
            double? speed = null;
            double? bearing = null;

            odometer = test.Updates[0].Odometer;
            speed = test.Updates[0].Speed;
            bearing = test.Updates[0].Bearing;

            //Query database if Pos_Id already exists
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Position WHERE Pos_Id=@Pos_Id", con))
                {
                    command.Parameters.Add("@Pos_Id", SqlDbType.Int).Value = Pos_Id;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        //If Pos_Id is in database store value in Id
                        Id = reader.GetInt32(0);
                    }
                }
            }
            Console.WriteLine("{0} and {1}", Pos_Id, Id);

            //If Pos_Id and Id are the same update table if Pos_Id does not exist in table then insert
            if (Pos_Id == Id)
                UpdatePosition(Pos_Id, VP_Id, Latitude, Longitude, bearing, odometer, speed);
            else
                InsertPosition(Pos_Id, VP_Id, Latitude, Longitude, bearing, odometer, speed);

            //testing to check that data is being read from the json file
            Console.WriteLine(test);
            Console.WriteLine(test.Updates[0]);
            Console.WriteLine(test.Updates[0].ID);
            Console.WriteLine(test.Updates[0].RadioName);
            Console.WriteLine(test.Updates[0].GeometryX.Y);
            Console.WriteLine(test.Updates[0].GeometryX.X);
            Console.ReadLine();

        }

        public class SpatialReference
        {
            public int wkid { get; set; }
        }

        public class GeometryX
        {
            public double X { get; set; }
            public double Y { get; set; }
            public SpatialReference spatialReference { get; set; }
        }

        public class Update
        {
            public string Type { get; set; }
            public int ID { get; set; }
            public DateTime Received { get; set; }
            public string RadioAddress { get; set; }
            public string RadioName { get; set; }
            public DateTime FixTime { get; set; }
            public string LatLon { get; set; }
            public int Bearing { get; set; }
            public double Speed { get; set; }
            public int LogID { get; set; }
            public string Event { get; set; }
            public DateTime EventTime { get; set; }
            public int Odometer { get; set; }
            public int RunningTime { get; set; }
            public int IdleTime { get; set; }
            public GeometryX GeometryX { get; set; }
        }

        public class RootObject
        {
            public List<Update> Updates { get; set; }
        }

        //If Id is not currently in the database, then creates a new row and inserts data
        static void InsertPosition(int Pos_Id, int VP_Id, double Latitude, double Longitude,
            double? bearing, double? odometer, double? speed)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO Position VALUES(@Pos_Id, @VP_Id, @Latitude, @Longitude, " +
                        "@bearing, @odometer, @speed)", con))
                    {
                        //Required data inserted into database
                        command.Parameters.AddWithValue("Pos_Id", Pos_Id);
                        command.Parameters.AddWithValue("VP_Id", VP_Id);
                        command.Parameters.AddWithValue("Latitude", Latitude);
                        command.Parameters.AddWithValue("Longitude", Longitude);

                        //Opeional data checked, if null the database column is left as null,
                        //otherwise data is inserted
                        if (bearing == null)
                            command.Parameters.AddWithValue("bearing", DBNull.Value);
                        else
                            command.Parameters.AddWithValue("bearing", bearing);
                        if (odometer == null)
                            command.Parameters.AddWithValue("odometer", DBNull.Value);
                        else
                            command.Parameters.AddWithValue("odometer", odometer);
                        if (speed == null)
                            command.Parameters.AddWithValue("speed", DBNull.Value);
                        else
                            command.Parameters.AddWithValue("speed", speed);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("insert error");
                }
            }
        }

        static void UpdatePosition(int Pos_Id, int VP_Id, double Latitude, double Longitude,
            double? bearing, double? odometer, double? speed)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("UPDATE Position SET VP_Id = @VP_Id, " +
                        "Latitude = @Latitude, Longitude = @Longitude, bearing = @bearing, " +
                        "odometer = @odometer, speed = @speed where Pos_Id = @Pos_Id", con))
                    {
                        //Required data inserted into database
                        command.Parameters.AddWithValue("Pos_Id", Pos_Id);
                        command.Parameters.AddWithValue("VP_Id", VP_Id);
                        command.Parameters.AddWithValue("Latitude", Latitude);
                        command.Parameters.AddWithValue("Longitude", Longitude);

                        //Opeional data checked, if null the database column is left as null,
                        //otherwise data is inserted
                        if (bearing == null)
                            command.Parameters.AddWithValue("bearing", DBNull.Value);
                        else
                            command.Parameters.AddWithValue("bearing", bearing);
                        if (odometer == null)
                            command.Parameters.AddWithValue("odometer", DBNull.Value);
                        else
                            command.Parameters.AddWithValue("odometer", odometer);
                        if (speed == null)
                            command.Parameters.AddWithValue("speed", DBNull.Value);
                        else
                            command.Parameters.AddWithValue("speed", speed);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("update error");
                }
            }
        }
    }
}

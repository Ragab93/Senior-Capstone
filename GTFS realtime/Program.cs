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
using System.Timers;
using System.Net;

/*This program is to request and read a JSON file for GPS data from the radios, 
 match each radio address to the radio SQL table to insert or update each radio
 address with information from the JSON file and SQL static database*/
namespace GTFSrealtimeloader
{
    class Program
    {
        static void Main(string[] args)
        {
                int x = 1;
                int check = 1;

                //JSON request to server and read
                /*
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string html = reader.ReadToEnd();
                }
                */


                //reads and store JSON file into a string for testing
                string json = new StreamReader(@"C:\Users\Vince\Desktop\jsontest\GPS1.json").ReadToEnd();

                //Deseralizes the JSON string to seperate and store data
                var test = JsonConvert.DeserializeObject<RootObject>(json);

                //get the size of the GPS report array
                try
                {
                    while (x != 10000)
                    {
                        check = test.Updates[x].ID;
                        x++;
                    }
                }
                catch { }

                int[] size = new int[x];

                //loops for each radio update provided
                for (int arr = 0; arr < x; arr++)
                {
                    int Id = 0;
                    int Id2 = 0;
                    int FE_Id = 0;
                    string BusId = "Null";
                    string route_Id = "Null";

                    double Latitude = test.Updates[arr].GeometryX.Y;
                    double Longitude = test.Updates[arr].GeometryX.X;
                    string RadioAddress = test.Updates[arr].RadioAddress;

                    //optional values that may be null
                    double? odometer = null;
                    double? speed = null;
                    double? bearing = null;

                    odometer = test.Updates[arr].Odometer;
                    speed = test.Updates[arr].Speed;
                    bearing = test.Updates[arr].Bearing;

                    //calculate time in unix Epoch format from the radio message
                    TimeSpan timeDifference = test.Updates[arr].EventTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    long unixEpochTimeMessage = Convert.ToInt64(timeDifference.TotalSeconds);

                    var time23 = test.Updates[arr].EventTime;
                    long CT = long.Parse(time23.ToString("yyyyMMddHHmmss"));

                    //calculate time this program is running on the server in unix Epoch format
                    timeDifference = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    long unixEpochTimeServer = Convert.ToInt64(timeDifference.TotalSeconds);

                    var TimeMessage = Convert.ToDateTime(test.Updates[arr].EventTime);

                    string TIME = Convert.ToString(test.Updates[arr].EventTime);
                    var result = Convert.ToDateTime(TIME);
                    string TEST = result.ToString("hh:mm:ss tt", System.Globalization.CultureInfo.CurrentCulture);

                    /*For all Sqlconnections, connection string from server explorer is just 
                     copy and pasted*/
                    //query Radio database with radio address from JSON
                    using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
                    {
                        con.Open();
                        using (SqlCommand command = new SqlCommand("SELECT * FROM Radio", con))
                        {
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                Id = reader.GetInt32(0);
                                string RadioAddress2 = reader.GetString(1);
                                string Bus = reader.GetString(2);
                                string route = reader.GetString(3);

                                //If Json radioaddress matches radioaddress in database then store data in Id2 and BusId
                                if (RadioAddress == RadioAddress2)
                                {
                                    Id2 = Id;
                                    BusId = Bus;
                                    route_Id = route;
                                }
                            }
                        }
                    }

                    FE_Id = Id2;
                    Id = 0;

                    //Query database if Id from radio is already in use
                    using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
                    {
                        con.Open();

                        using (SqlCommand command = new SqlCommand("SELECT * FROM FeedEntity WHERE FE_Id = @FE_Id", con))
                        {
                            command.Parameters.Add("@FE_Id", SqlDbType.Int).Value = FE_Id;
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                //If FE_Id is in database store value in Id
                                Id = reader.GetInt32(0);
                            }
                        }
                    }


                    //If Id from radio database exist the update Id entries in database, otherwise create Id entries
                    if (Id == Id2)
                    {
                        UpdateFeedEntity(Id2);
                        UpdateVehiclePosition(Id2, unixEpochTimeMessage);
                        UpdateTripUpdate(Id2, unixEpochTimeMessage);
                        UpdatePosition(Id2, Latitude, Longitude, bearing, odometer, speed);
                        string ptrip = QueryTripDescriptor(Id2);
                        string ctrip = UpdateTripDescriptor(Id2, route_Id, TIME, CT);
                        /*if the current trip is different from the previous trips then replace the old stops 
                        with new the new stops*/
                        if (ptrip != ctrip)
                        {
                            DeleteStopTimeUpdate(Id2, ptrip);
                            InsertStopTimeUpdate(Id2, ctrip);
                        }
                    }
                    else
                    {
                        InsertFeedEntity(Id2);
                        InsertVehiclePosition(Id2, unixEpochTimeMessage);
                        InsertTripUpdate(Id2, unixEpochTimeMessage);
                        InsertVehicleDescriptor(Id2, BusId);
                        InsertPosition(Id2, Latitude, Longitude, bearing, odometer, speed);
                        string ctrip = InsertTripDescriptor(Id2, route_Id, TIME);
                        InsertStopTimeUpdate(Id2, ctrip);
                    }

                    //waits 30 seconds then begins again, propose SQL agent tasks to
                    //instead for actual use
                    System.Threading.Thread.Sleep(30000);
                }
                //testing to check that data is being read from the json file
                Console.ReadLine();
        }


        //Public classes for reading JSON file
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
        static void InsertFeedEntity(int Id)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO FeedEntity VALUES(@FE_Id, @FM_Id, @Id)", con))
                    {
                        //Required data inserted into database
                        command.Parameters.AddWithValue("FE_Id", Id);
                        command.Parameters.AddWithValue("FM_Id", 1);
                        command.Parameters.AddWithValue("Id", "TARTA");
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("FeedEntity insert error");
                }
            }
        }

        static void InsertVehiclePosition(int Id, long time)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO VehiclePosition VALUES(@VP_Id, @FE_Id, @current_stop_sequence," +
                        "@stop_id, @timestamp)", con))
                    {
                        //Required data inserted into database
                        command.Parameters.AddWithValue("VP_Id", Id);
                        command.Parameters.AddWithValue("FE_Id", Id);
                        command.Parameters.AddWithValue("current_stop_sequence", 0);
                        command.Parameters.AddWithValue("stop_Id", "Null");
                        command.Parameters.AddWithValue("timestamp", time);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("VehiclePosition insert error");
                }
            }
        }

        static void InsertTripUpdate(int Id, long messagetime)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO TripUpdate VALUES(@TP_Id, @FE_Id, @timestamp," +
                        "@delay)", con))
                    {
                        //Required data inserted into database
                        command.Parameters.AddWithValue("TP_Id", Id);
                        command.Parameters.AddWithValue("FE_Id", Id);
                        command.Parameters.AddWithValue("timestamp", messagetime);
                        command.Parameters.AddWithValue("delay", DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("TripUpdate insert error");
                }
            }
        }

        static void InsertVehicleDescriptor(int Id, string Bus)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO VehicleDescriptor VALUES(@V_Id, @Id, @Label," +
                        "@License_Plate, @VP_Id, @TP_Id)", con))
                    {
                        //Required data inserted into database
                        command.Parameters.AddWithValue("V_Id", Id);
                        command.Parameters.AddWithValue("Id", "Null");
                        command.Parameters.AddWithValue("Label", Bus);
                        command.Parameters.AddWithValue("License_Plate", "Null");
                        command.Parameters.AddWithValue("VP_Id", Id);
                        command.Parameters.AddWithValue("TP_Id", Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("VehicleDescriptor insert error");
                }
            }
        }

        static void InsertPosition(int Pos_Id, double Latitude, double Longitude,
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
                        command.Parameters.AddWithValue("VP_Id", Pos_Id);
                        command.Parameters.AddWithValue("Latitude", Latitude);
                        command.Parameters.AddWithValue("Longitude", Longitude);

                        //Optional data checked, if null the database column is left as null,
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
                    Console.WriteLine("Position insert error");
                }
            }
        }

        static string InsertTripDescriptor(int Id, string route_Id, string timemessage)
        {
            string routeId = "R" + route_Id;
            int[] trip = new int[1000];
            string[] time = new string[1000];
            string[] starttime = new string[100];
            int i = 0;

            var result = Convert.ToDateTime(timemessage);
            string Date = result.ToString("dd/M/yyyy", System.Globalization.CultureInfo.CurrentCulture);

            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM [" + routeId + "]", con))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        trip[i] = reader.GetInt32(1);
                        time[i] = reader.GetString(3);
                    }
                }
            }

            string tripId = Convert.ToString(trip[0]);

            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO TripDescriptor VALUES(@TD_Id, @TP_Id, @VP_Id, @trip_Id," +
                        "@route_Id, @direction_Id, @start_time, @start_date)", con))
                    {
                        //Required data inserted into database
                        command.Parameters.AddWithValue("TD_Id", Id);
                        command.Parameters.AddWithValue("TP_Id", Id);
                        command.Parameters.AddWithValue("VP_Id", Id);
                        command.Parameters.AddWithValue("trip_Id", tripId);
                        command.Parameters.AddWithValue("route_Id", route_Id);
                        command.Parameters.AddWithValue("direction_Id", DBNull.Value);
                        command.Parameters.AddWithValue("start_time", time[0]);
                        command.Parameters.AddWithValue("start_date", Date);
                        command.ExecuteNonQuery();

                    }
                }
                catch
                {
                    Console.WriteLine("TripDescriptor insert error");
                }
            }

            return tripId;

        }

        static void InsertStopTimeUpdate(int Id, string trip)
        {
            int i = 0;
            int[] stop_seq = new int[100000];
            string[] time = new string[100000];
            string[] stop_Id = new string[100000];


            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM StopTimes WHERE trip_Id = @trip_Id", con))
                {
                    command.Parameters.Add("@trip_Id", SqlDbType.Int).Value = trip;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        time[i] = reader.GetString(1);
                        stop_Id[i] = reader.GetString(3);
                        stop_seq[i] = reader.GetInt32(4);
                        i++;
                    }
                }
            }

            int Id2 = Id * 100 - 99;

            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    for (int x = 0; x < i; x++)
                    {
                        string stop1 = stop_Id[x];
                        int stop_seq1 = stop_seq[x];

                        using (SqlCommand command = new SqlCommand("INSERT INTO StopTimeUpdate VALUES(@ST_Id, @stop_sequence, @stop_Id," +
                            "@TP_Id, @ScheduleRelationship)", con))
                        {
                            //Required data inserted into database
                            command.Parameters.AddWithValue("ST_Id", Id2);
                            command.Parameters.AddWithValue("stop_sequence", stop_seq1);
                            command.Parameters.AddWithValue("stop_Id", stop1);
                            command.Parameters.AddWithValue("TP_Id", Id);
                            command.Parameters.AddWithValue("ScheduleRelationship", "Scheduled");
                            command.ExecuteNonQuery();
                        }

                        Id2++;

                    }
                }
                catch
                {
                    Console.WriteLine("StopTimeUpdate insert error");
                }
            }

            Id2 = Id * 100 - 99;

            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    for (int x = 0; x < i; x++)
                    {
                        var result = Convert.ToDateTime(time[x]);
                        TimeSpan timeDifference = result - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        long stoptime = Convert.ToInt64(timeDifference.TotalSeconds);

                        using (SqlCommand command = new SqlCommand("INSERT INTO StopTimeEvent VALUES(@SE_Id, @delay, @time," +
                            "@uncertainty, @ST_Id)", con))
                        {
                            //Required data inserted into database
                            command.Parameters.AddWithValue("SE_Id", Id2);
                            command.Parameters.AddWithValue("delay", DBNull.Value);
                            command.Parameters.AddWithValue("time", stoptime);
                            command.Parameters.AddWithValue("uncertainty", DBNull.Value);
                            command.Parameters.AddWithValue("ST_Id", Id2);
                            command.ExecuteNonQuery();
                        }

                        Id2++;

                    }
                }
                catch
                {
                    Console.WriteLine("StopTimeEvent insert error");
                }
            }

        }



        //If Id already exists then update the information
        static void UpdateFeedEntity(int Id)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("UPDATE FeedEntity SET FE_Id = @FE_Id, " +
                        "FM_Id = @FM_Id, Id = @Id WHERE FE_Id =" + Id, con))
                    {
                        //Required data updated in database
                        command.Parameters.AddWithValue("FE_Id", Id);
                        command.Parameters.AddWithValue("FM_Id", 1);
                        command.Parameters.AddWithValue("Id", "TARTA");
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("FeedEntity update error");
                }
            }
        }

        static void UpdateVehiclePosition(int Id, long time)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    //SET and which columns may be updated, WHERE is for which columns are used to determine which rows are to be updated
                    using (SqlCommand command = new SqlCommand("UPDATE VehiclePosition SET current_stop_sequence = @current_stop_sequence, " +
                        "stop_Id = @stop_Id, timestamp = @timestamp WHERE VP_Id = @VP_Id", con))
                    {
                        //value used for determining which data rows need updated
                        command.Parameters.Add("@VP_Id", SqlDbType.Int).Value = Id;
                        //columns that are updated
                        command.Parameters.AddWithValue("current_stop_sequence", 0);
                        command.Parameters.AddWithValue("stop_Id", "Null");
                        command.Parameters.AddWithValue("timestamp", time);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("VehiclePosition update error");
                }
            }
        }

        static void UpdateTripUpdate(int Id, long messagetime)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("Update TripUpdate SET " +
                        "timestamp = @timestamp WHERE TP_Id = @TP_Id", con))
                    {
                        //Required data inserted into database
                        command.Parameters.Add("@TP_Id", SqlDbType.Int).Value = Id;
                        command.Parameters.AddWithValue("timestamp", messagetime);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("TripUpdate update error");
                }
            }
        }

        static void UpdatePosition(int Pos_Id, double Latitude, double Longitude,
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
                        command.Parameters.AddWithValue("VP_Id", Pos_Id);
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
                    Console.WriteLine("Position update error");
                }
            }
        }

        static string UpdateTripDescriptor(int Id, string route_Id, string timemessage, long CT)
        {
            string routeId = "R" + route_Id;
            int[] trip = new int[10000];
            string[] time = new string[10000];
            string[] starttime = new string[1000];
            int i = 0;
            int x = 0;
            int set = 0;
            long t1 = 0;

            var result = Convert.ToDateTime(timemessage);
            string Date = result.ToString("dd/M/yyyy", System.Globalization.CultureInfo.CurrentCulture);


            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM [" + routeId + "]", con))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        trip[i] = reader.GetInt32(1);
                        time[i] = reader.GetString(3);
                        i++;
                    }
                }
            }

            for (x = 0; x < i; x++)
            {
                var result2 = Convert.ToDateTime(time[x]);
                t1 = long.Parse(result2.ToString("yyyyMMddHHmmss"));
                if (t1 > CT)
                {
                    set = x;
                    x = 100000;
                }

            }

            string tripname = Convert.ToString(trip[set]);
            string tripId = Convert.ToString(trip[set]);


            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("UPDATE TripDescriptor SET trip_Id = @trip_Id," +
                        "direction_Id = @direction_Id, start_time = @start_time, start_date = @start_date WHERE TD_Id = @TD_Id", con))
                    {
                        //Required data inserted into database
                        command.Parameters.Add("@TD_Id", SqlDbType.Int).Value = Id;
                        command.Parameters.AddWithValue("trip_Id", tripId);
                        command.Parameters.AddWithValue("direction_Id", DBNull.Value);
                        command.Parameters.AddWithValue("start_time", time[set]);
                        command.Parameters.AddWithValue("start_date", Date);
                        command.ExecuteNonQuery();

                    }
                }
                catch
                {
                    Console.WriteLine("TripDescriptor update error");
                }
            }
            return tripId;
        }




        //used for removing bus_Ids once route is completed or an unexpected problem takes down the bus or route
        static void DeleteStopTimeUpdate(int Id, string trip)
        {
            int[] ST_Id = new int[1000];
            int i = 0;
            int x = 0;

            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM StopTimeUpdate WHERE TP_Id = @TP_Id", con))
                {
                    command.Parameters.Add("@TP_Id", SqlDbType.Int).Value = Id;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        //If FE_Id is in database store value in Id
                        ST_Id[i] = reader.GetInt32(0);
                        i++;
                    }

                }

            }

            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    for (x = 0; x < i; x++)
                    {
                        using (SqlCommand command = new SqlCommand("DELETE FROM StopTimeEvent WHERE ST_Id = @ST_Id", con))
                        {
                            command.Parameters.AddWithValue("@ST_Id", ST_Id[x]);
                            command.ExecuteNonQuery();

                        }
                    }
                }
                catch
                {
                    Console.WriteLine("StopeTimeEvent" + ST_Id[x] + "delete failed");
                }
            }

            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DELETE FROM StopTimeUpdate WHERE TP_Id = @TP_Id", con))
                    {
                        command.Parameters.AddWithValue("@TP_Id", Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("StopTimeUpdate" + Id + "delete failed");
                }
            }

        }

        static void DeleteTripDescriptor(int Id)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DELETE FROM TripDescriptor WHERE TD_Id = @TD_Id", con))
                    {
                        command.Parameters.AddWithValue("@TD_Id", Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("TripDescriptor" + Id + "delete failed");
                }
            }
        }

        static void DeletePosition(int Id)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DELETE FROM Position WHERE Pos_Id = @Pos_Id", con))
                    {
                        command.Parameters.AddWithValue("@Pos_Id", Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Position" + Id + "delete failed");
                }
            }
        }

        static void DeleteVehicleDescriptor(int Id)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DELETE FROM VehicleDescriptor WHERE V_Id = @V_Id", con))
                    {
                        command.Parameters.AddWithValue("@V_Id", Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("VehicleDescriptor" + Id + "delete failed");
                }
            }
        }

        static void DeleteTripUpdate(int Id)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DELETE FROM TripUpdate WHERE TP_Id = @TP_Id", con))
                    {
                        command.Parameters.AddWithValue("@TP_Id", Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("TripUpdate" + Id + "delete faile");
                }
            }
        }

        static void DeleteVehiclePosition(int Id)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DELETE FROM VehiclePosition WHERE VP_Id = @VP_Id", con))
                    {
                        command.Parameters.AddWithValue("@VP_Id", Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("VehiclePoistion" + Id + "delete failed");
                }
            }
        }

        static void DeleteFeedEntity(int Id)
        {
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DELETE FROM FeedEntity WHERE FE_Id = @FE_Id", con))
                    {
                        command.Parameters.AddWithValue("@FE_Id", Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("FeedEntity" + Id + "delete failed");
                }
            }
        }



        static string QueryTripDescriptor(int Id)
        {
            string Trip = "Null";

            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM TripDescriptor WHERE TD_Id = @TD_Id", con))
                {
                    command.Parameters.Add("@TD_Id", SqlDbType.Int).Value = Id;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Trip = reader.GetString(3);
                    }
                }
            }

            return Trip;
        }

    }

}


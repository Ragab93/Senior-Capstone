using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using System.Data;

//Responsible for turning text documents into SQL tables.
namespace Staticdatabasecreator
{
    class Program
    {
        static void Main(string[] args)
        {
            //Each document is read and split apart
            string readText = File.ReadAllText(@"C:\Users\Vince\Desktop\jsontest\calendar.txt");
            List<string> listStrLineElements = readText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            List<string> rowList = listStrLineElements.SelectMany(s => s.Split(',')).ToList();

            /*If a table already exists the table is quickly dropped and recreated. 
              Then the information for the text document is inserted to keep up to date*/
            DropCalendarTable();
            CreateCalendarTable();
            Calendar(rowList);

            readText = File.ReadAllText(@"C:\Users\Vince\Desktop\jsontest\trips.txt");
            listStrLineElements = readText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            rowList = listStrLineElements.SelectMany(s => s.Split(',')).ToList();

            DropTripsTable();
            CreateTripsTable();
            Trips(rowList);

            readText = File.ReadAllText(@"C:\Users\Vince\Desktop\jsontest\routes.txt");
            listStrLineElements = readText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            rowList = listStrLineElements.SelectMany(s => s.Split(',')).ToList();

            DropRoutesTable();
            CreateRoutesTable();
            Routes(rowList);

            readText = File.ReadAllText(@"C:\Users\Vince\Desktop\jsontest\agency.txt");
            listStrLineElements = readText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            rowList = listStrLineElements.SelectMany(s => s.Split(',')).ToList();

            DropAgencyTable();
            CreateAgencyTable();
            Agency(rowList);

            readText = File.ReadAllText(@"C:\Users\Vince\Desktop\jsontest\stop_times.txt");
            listStrLineElements = readText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            rowList = listStrLineElements.SelectMany(s => s.Split(',')).ToList();

            DropStopTimesTable();
            CreateStopTimesTable();
            StopTimes(rowList);

            readText = File.ReadAllText(@"C:\Users\Vince\Desktop\jsontest\stops.txt");
            listStrLineElements = readText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            rowList = listStrLineElements.SelectMany(s => s.Split(',')).ToList();

            DropStopsTable();
            CreateStopsTable();
            Stops(rowList);

            Console.ReadLine();
        }

        static void DropCalendarTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DROP TABLE Calendar", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Calendar does not exsist or failed to drop.");
                }
            }
        }

        static void DropTripsTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DROP TABLE Trips", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Trips does not exsist or failed to drop.");
                }
            }
        }

        static void DropRoutesTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DROP TABLE Routes", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Routes does not exsist or failed to drop.");
                }
            }
        }

        static void DropAgencyTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DROP TABLE Agency", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Agency does not exsist or failed to drop.");
                }
            }
        }

        static void DropStopTimesTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DROP TABLE StopTimes", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table StopTimes does not exsist or failed to drop.");
                }
            }
        }

        static void DropStopsTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DROP TABLE Stops", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Stops does not exsist or failed to drop.");
                }
            }
        }


        static void CreateCalendarTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("CREATE TABLE Calendar (service_Id INT, monday INT, tuesday INT, wednesday INT," +
                        "thursday INT, friday INT, saturday INT, sunday INT, start_date NUMERIC(20,0), end_date NUMERIC(20,0))", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Calendar not created.");
                }
            }
        }

        static void CreateTripsTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("CREATE TABLE Trips (route_Id INT, service_Id INT, trip_Id INT, trip_headsign TEXT," +
                        "trip_short_name TEXT, direction_Id INT, block_Id INT, shape_Id INT, wheelchair_accessible INT, bikes_allowed INT)", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Trips not created.");
                }
            }
        }

        static void CreateRoutesTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("CREATE TABLE Routes (route_Id INT, agency_Id TEXT, route_short_name TEXT, route_long_name TEXT," +
                        "route_desc TEXT, route_type INT, route_url TEXT, route_color TEXT, route_text_color TEXT)", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Routes not created.");
                }
            }
        }

        static void CreateAgencyTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("CREATE TABLE Agency (agency_Id TEXT, agency_name TEXT, agency_url TEXT, agency_timezone TEXT," +
                        "agency_lang TEXT, agency_phone TEXT, agency_fare_url TEXT)", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Agency not created.");
                }
            }
        }

        static void CreateStopTimesTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("CREATE TABLE StopTimes (Trip_Id INT, arrival_time TEXT, departure_time TEXT, stop_Id INT," +
                        "stop_sequence INT, stop_headsign INT, pickup_type INT, drop_off_type INT, shape_dist_traveled FLOAT)", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table StopTimes not created.");
                }
            }
        }

        static void CreateStopsTable()
        {
            using (SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("CREATE TABLE Stops (stop_Id INT, stop_name TEXT, stop_desc TEXT, stop_lat FLOAT," +
                        "stop_lon FLOAT, zone_Id TEXT, stop_url TEXT, location_type TEXT, parent_station TEXT, stop_timezone TEXT, wheelchair_boarding INT)", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table Stops not created.");
                }
            }
        }


        static void Calendar(List<string> rowList)
        {
            //Replace with your server credentials/info
            SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False");
            try
            {
                con.Open();
                for (int i = 10; i <= rowList.Count - 10; i += 10)//Implement by 10...
                {
                    //Replace table_name with your table name, and Column1 with your column names (replace for all).
                    using (SqlCommand command = new SqlCommand("INSERT INTO Calendar VALUES(@service_Id, @monday, @tuesday, @wednesday," +
                        "@thursday, @friday, @saturday, @sunday, @start_date, @end_date)", con))
                    {
                        command.Parameters.AddWithValue("service_Id", rowList[i]);
                        command.Parameters.AddWithValue("monday", rowList[i + 1]);
                        command.Parameters.AddWithValue("tuesday", rowList[i + 2]);
                        command.Parameters.AddWithValue("wednesday", rowList[i + 3]);
                        command.Parameters.AddWithValue("thursday", rowList[i + 4]);
                        command.Parameters.AddWithValue("friday", rowList[i + 5]);
                        command.Parameters.AddWithValue("saturday", rowList[i + 6]);
                        command.Parameters.AddWithValue("sunday", rowList[i + 7]);
                        command.Parameters.AddWithValue("start_date", rowList[i + 8]);
                        command.Parameters.AddWithValue("end_date", rowList[i + 9]);
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch
            {
                Console.WriteLine("Calendar insert error");
            }

        }

        static void Trips(List<string> rowList)
        {
            //Replace with your server credentials/info
            SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False");
            try
            {
                con.Open();
                for (int i = 10; i <= rowList.Count - 10; i += 10)//Implement by 10...
                {
                    //Replace table_name with your table name, and Column1 with your column names (replace for all).
                    using (SqlCommand command = new SqlCommand("INSERT INTO Trips VALUES(@route_Id, @service_Id, @trip_Id, @trip_headsign," +
                        "@trip_short_name, @direction_Id, @block_Id, @shape_Id, @wheelchair_accessible, @bikes_allowed)", con))
                    {
                        command.Parameters.AddWithValue("route_Id", rowList[i]);
                        command.Parameters.AddWithValue("service_Id", rowList[i + 1]);
                        command.Parameters.AddWithValue("trip_Id", rowList[i + 2]);
                        command.Parameters.AddWithValue("trip_headsign", rowList[i + 3]);
                        command.Parameters.AddWithValue("trip_short_name", rowList[i + 4]);
                        command.Parameters.AddWithValue("direction_Id", rowList[i + 5]);
                        command.Parameters.AddWithValue("block_Id", rowList[i + 6]);
                        command.Parameters.AddWithValue("shape_Id", rowList[i + 7]);
                        command.Parameters.AddWithValue("wheelchair_accessible", rowList[i + 8]);
                        command.Parameters.AddWithValue("bikes_allowed", rowList[i + 9]);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                Console.WriteLine("Trips insert error");
            }
        }

        static void Routes(List<string> rowList)
        {
            //Replace with your server credentials/info
            SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False");
            try
            {
                con.Open();
                for (int i = 9; i <= rowList.Count - 9; i += 9)//Implement by 9...
                {
                    //Replace table_name with your table name, and Column1 with your column names (replace for all).
                    using (SqlCommand command = new SqlCommand("INSERT INTO Routes VALUES(@route_Id, @agency_Id, @route_short_name, @route_long_name," +
                        "@route_desc, @route_type, @route_url, @route_color, @route_text_color)", con))
                    {
                        command.Parameters.AddWithValue("route_Id", rowList[i]);
                        command.Parameters.AddWithValue("agency_Id", rowList[i + 1]);
                        command.Parameters.AddWithValue("route_short_name", rowList[i + 2]);
                        command.Parameters.AddWithValue("route_long_name", rowList[i + 3]);
                        command.Parameters.AddWithValue("route_desc", rowList[i + 4]);
                        command.Parameters.AddWithValue("route_type", rowList[i + 5]);
                        command.Parameters.AddWithValue("route_url", rowList[i + 6]);
                        command.Parameters.AddWithValue("route_color", rowList[i + 7]);
                        command.Parameters.AddWithValue("route_text_color", rowList[i + 8]);
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch
            {
                Console.WriteLine("Routes insert error");
            }

        }

        static void Agency(List<string> rowList)
        {
            //Replace with your server credentials/info
            SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False");
            try
            {
                con.Open();
                for (int i = 7; i <= rowList.Count - 7; i += 7)//Implement by 7...
                {
                    //Replace table_name with your table name, and Column1 with your column names (replace for all).
                    using (SqlCommand command = new SqlCommand("INSERT INTO Agency VALUES(@agency_Id, @agency_name, @agency_url, @agency_timezone," +
                        "@agency_lang, @agency_phone, @agency_fare_url)", con))
                    {
                        command.Parameters.AddWithValue("agency_Id", rowList[i]);
                        command.Parameters.AddWithValue("agency_name", rowList[i + 1]);
                        command.Parameters.AddWithValue("agency_url", rowList[i + 2]);
                        command.Parameters.AddWithValue("agency_timezone", rowList[i + 3]);
                        command.Parameters.AddWithValue("agency_lang", rowList[i + 4]);
                        command.Parameters.AddWithValue("agency_phone", rowList[i + 5]);
                        command.Parameters.AddWithValue("agency_fare_url", rowList[i + 6]);
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch
            {
                Console.WriteLine("Agency insert error");
            }

        }

        static void StopTimes(List<string> rowList)
        {
            //Replace with your server credentials/info
            SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False");
            try
            {
                con.Open();
                for (int i = 9; i <= rowList.Count - 9; i += 9)//Implement by 9...
                {
                    //Replace table_name with your table name, and Column1 with your column names (replace for all).
                    using (SqlCommand command = new SqlCommand("INSERT INTO StopTimes VALUES(@Trip_Id, @arrival_time, @departure_time, @stop_Id," +
                        "@stop_sequence, @stop_headsign, @pickup_type, @drop_off_type, @shape_dist_traveled)", con))
                    {
                        command.Parameters.AddWithValue("Trip_Id", rowList[i]);
                        command.Parameters.AddWithValue("arrival_time", rowList[i + 1]);
                        command.Parameters.AddWithValue("departure_time", rowList[i + 2]);
                        command.Parameters.AddWithValue("stop_Id", rowList[i + 3]);
                        command.Parameters.AddWithValue("stop_sequence", rowList[i + 4]);
                        command.Parameters.AddWithValue("stop_headsign", rowList[i + 5]);
                        command.Parameters.AddWithValue("pickup_type", rowList[i + 6]);
                        command.Parameters.AddWithValue("drop_off_type", rowList[i + 7]);
                        command.Parameters.AddWithValue("shape_dist_traveled", rowList[i + 8]);
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch
            {
                Console.WriteLine("StopTimes insert error");
            }

        }

        static void Stops(List<string> rowList)
        {
            //Replace with your server credentials/info
            SqlConnection con = new SqlConnection("Data Source=Vince-Desktop;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False");
            try
            {
                con.Open();
                for (int i = 11; i <= rowList.Count - 11; i += 11)//Implement by 11...
                {
                    //Replace table_name with your table name, and Column1 with your column names (replace for all).
                    using (SqlCommand command = new SqlCommand("INSERT INTO Stops VALUES(@stop_Id, @stop_name, @stop_desc, @stop_lat," +
                        "@stop_lon, @zone_Id, @stop_url, @location_type, @parent_station, @stop_timezone, @wheelchair_boarding)", con))
                    {
                        command.Parameters.AddWithValue("stop_Id", rowList[i]);
                        command.Parameters.AddWithValue("stop_name", rowList[i + 1]);
                        command.Parameters.AddWithValue("stop_desc", rowList[i + 2]);
                        command.Parameters.AddWithValue("stop_lat", rowList[i + 3]);
                        command.Parameters.AddWithValue("stop_lon", rowList[i + 4]);
                        command.Parameters.AddWithValue("zone_Id", rowList[i + 5]);
                        command.Parameters.AddWithValue("stop_url", rowList[i + 6]);
                        command.Parameters.AddWithValue("location_type", rowList[i + 7]);
                        command.Parameters.AddWithValue("parent_station", rowList[i + 8]);
                        command.Parameters.AddWithValue("stop_timezone", rowList[i + 9]);
                        command.Parameters.AddWithValue("wheelchair_boarding", rowList[i + 10]);
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch
            {
                Console.WriteLine("Stops insert error");
            }

        }
    }
}

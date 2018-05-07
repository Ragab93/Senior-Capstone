using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;

namespace RadioLogin
{
    class Login
    {
        static void Main(string[] args)
        {
            string log = "0";
            int Id = 1;
            int Id3 = 0;
            int route2 = 0;

            while (log != "-1")
            {
                Console.WriteLine("Enter 1 to login or 2 to logout");
                log = Console.ReadLine();

                //loging in the radio
                if (log == "1")
                {
                    Console.WriteLine("Enter radio address.");
                    string RadioAddress = Console.ReadLine();

                    //optional
                    Console.WriteLine("Enter Bus Id");
                    string Bus = Console.ReadLine();

                    Console.WriteLine("Enter route Id");
                    string routeId = Console.ReadLine();

                    Console.WriteLine("Enter serviceID");
                    int ServiceId = Convert.ToInt32(Console.ReadLine());

                    InsertRadio(Id, RadioAddress, Bus, routeId, ServiceId);

                    Id++;
                }


                //logging out the radio
                else if (log == "2")
                {
                    Console.WriteLine("Enter radio address.");
                    string RadioAddress = Console.ReadLine();


                    using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
                    {
                        con.Open();
                        //checks that the radio to logout is currently in use in the database
                        using (SqlCommand command = new SqlCommand("SELECT * FROM Radio", con))
                        {
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                int Id2 = reader.GetInt32(0);
                                string RadioAddress2 = reader.GetString(1);
                                string Bus = reader.GetString(2);
                                int route = reader.GetInt32(3);

                                if (RadioAddress == RadioAddress2)
                                {
                                    Id3 = Id2;
                                    route2 = route;
                                }
                            }
                        }
                    }

                    string trip = QueryTripDescriptor(Id3);

                    /*if the radio is in use it is removed from the table
                     and all uses of it are removed from the real-time database*/
                    DeleteRadio(Id3, route2);
                    DeletePosition(Id3);
                    DeleteStopTimeUpdate(Id3, trip);
                    DeleteVehicleDescriptor(Id3);
                    DeleteTripDescriptor(Id3);
                    DeleteVehiclePosition(Id3);
                    DeleteTripUpdate(Id3);
                    DeleteFeedEntity(Id3);
                }
                else
                {
                    Console.WriteLine("Invalid selection");
                }
            }
        }




        static void InsertRadio(int Id, string RadioAddress, string Bus, string routeId, int Service_Id)
        {
            int a = 0;
            int i = 0;
            int x = 0;
            int[] y = new int[1000];
            int z = 0;
            int[] trip = new int[1000];
            string[,] arrival = new string[1000, 1000];
            int route_Id;
            Int32.TryParse(routeId, out route_Id);
            string Route = "R" + routeId;

            //Insert given Info into radio table for future use
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO Radio VALUES(@Id, @RadioAddress, @Bus, @RouteId)", con))
                    {
                        //Required data inserted into database
                        command.Parameters.AddWithValue("Id", Id);
                        command.Parameters.AddWithValue("RadioAddress", RadioAddress);
                        command.Parameters.AddWithValue("Bus", Bus);
                        command.Parameters.AddWithValue("RouteId", route_Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Radio Name of Bus is already in use");
                }
            }


            //Create a table named after entered route Id
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("CREATE TABLE [" + Route + "] (route_Id INT, trip_Id INT, Service_Id INT, arrival TEXT)", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Route table not created");
                }
            }

            route_Id = Convert.ToInt32(routeId);

            //Search Trips for all trip_Ids corresponding to the route+Id
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM Trips WHERE route_Id = @route_Id AND Service_Id = @Service_Id", con))
                {
                    command.Parameters.Add("@route_Id", SqlDbType.Int).Value = route_Id;
                    command.Parameters.Add("@Service_Id", SqlDbType.Int).Value = Service_Id;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        //If route_Id in table matches route then the trip_Id is stored in an array and the 
                        //array incremented by 1
                        int route = reader.GetInt32(0);
                        trip[i] = reader.GetInt32(2);

                        if (route == route_Id)
                        {
                            i++;
                        }
                    }
                }
            }

            for (x = 0; x < i; x++)
            {
                using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("SELECT * FROM StopTimes WHERE trip_Id = @trip_Id", con))
                    {
                        command.Parameters.Add("@trip_Id", SqlDbType.Int).Value = trip[x];
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            //If route_Id in table matches route then the trip_Id is stored in an array and the 
                            //array incremented by 1
                            int trip_Id = reader.GetInt32(0);
                            arrival[x, y[a]] = reader.GetString(1);

                            if (trip[x] == trip_Id)
                            {
                                y[a]++;
                            }
                        }
                    }
                }
                a++;
            }

            //Take array of trips and store into RId
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    a = 0;
                    for (x = 0; x < i; x++)
                    {
                        for (z = 0; z < y[a]; z++)
                        {
                            using (SqlCommand command = new SqlCommand("INSERT INTO [" + Route + "] VALUES(@route_Id, @trip_Id, @Service_Id, @arrival)", con))
                            {
                                //Required data inserted into database
                                command.Parameters.AddWithValue("route_Id", route_Id);
                                command.Parameters.AddWithValue("trip_Id", trip[x]);
                                command.Parameters.AddWithValue("Service_Id", Service_Id);
                                command.Parameters.AddWithValue("arrival", arrival[x, z]);
                                command.ExecuteNonQuery();
                            }
                        }
                        a++;
                    }
                }
                catch
                {
                    Console.WriteLine("Route insert failed");
                }
            }
        }



        //for possible use of trips changes must be made
        static void InsertTrips(int Id, string RadioAddress, string Bus, string routeId, int Service_Id, int tripId)
        {
            int a = 0;
            int i = 0;
            int x = 0;
            int[] y = new int[1000];
            int z = 0;
            int[] trip = new int[1000];
            string[,] arrival = new string[1000, 1000];
            int route_Id;
            Int32.TryParse(routeId, out route_Id);
            string Route = "R" + routeId;

            //Search Trips for all trip_Ids corresponding to the route+Id
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM Trips WHERE trip_Id = @trip_Id AND Service_Id = @Service_Id", con))
                {
                    command.Parameters.Add("@trip_Id", SqlDbType.Int).Value = tripId;
                    command.Parameters.Add("@Service_Id", SqlDbType.Int).Value = Service_Id;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        //If route_Id in table matches route then the trip_Id is stored in an array and the 
                        //array incremented by 1
                        int route = reader.GetInt32(0);
                        trip[i] = reader.GetInt32(2);

                        if (route == route_Id)
                        {
                            i++;
                        }
                    }
                }
            }

            for (x = 0; x < i; x++)
            {
                using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=Static-Data;Integrated Security=True;Pooling=False"))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("SELECT * FROM StopTimes WHERE trip_Id = @trip_Id", con))
                    {
                        command.Parameters.Add("@trip_Id", SqlDbType.Int).Value = trip[x];
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            //If route_Id in table matches route then the trip_Id is stored in an array and the 
                            //array incremented by 1
                            int trip_Id = reader.GetInt32(0);
                            arrival[x, y[a]] = reader.GetString(1);

                            if (trip[x] == trip_Id)
                            {
                                y[a]++;
                            }
                        }
                    }
                }
                a++;
            }

            //Take array of trips and store into RId
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    a = 0;
                    for (x = 0; x < i; x++)
                    {
                        for (z = 0; z < y[a]; z++)
                        {
                            using (SqlCommand command = new SqlCommand("INSERT INTO [" + Route + "] VALUES(@route_Id, @trip_Id, @Service_Id, @arrival)", con))
                            {
                                //Required data inserted into database
                                command.Parameters.AddWithValue("route_Id", route_Id);
                                command.Parameters.AddWithValue("trip_Id", trip[x]);
                                command.Parameters.AddWithValue("Service_Id", Service_Id);
                                command.Parameters.AddWithValue("arrival", arrival[x, z]);
                                command.ExecuteNonQuery();
                            }
                        }
                        a++;
                    }
                }
                catch
                {
                    Console.WriteLine("Route insert failed");
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




        static void DeleteRadio(int Id, int route_Id)
        {
            string route = "R" + route_Id;
            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DELETE FROM Radio WHERE Id = @Id", con))
                    {
                        command.Parameters.AddWithValue("@Id", Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Radio is already not in use.");
                }
            }

            using (SqlConnection con = new SqlConnection("Data Source=VINCE-DESKTOP;Initial Catalog=storeDB;Integrated Security=True;Pooling=False"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("DROP TABLE [" + route_Id + "]", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    Console.WriteLine("Table already deleted.");
                }
            }
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


    }

}

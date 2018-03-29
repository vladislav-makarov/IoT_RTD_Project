using Gtk;
using System;
using System.Net;
using System.IO;
using System.Text;
using ProtoBuf;
using transit_realtime;
using System.Collections.Generic;

/** Authors: Vlad Makarov, Mark Nilov **/
namespace RTD_UI_Application
{

    class Program
	{
        public static List<Stop.stop_t> allstops = new List<Stop.stop_t>();
        public static string tripStatus = "N/A";
        
		static void Main(string[] args)
        {
            //============ Code to initialize and open UI Window ============//

            Application.Init();
            MainWindow win = new MainWindow();
            win.Title = "RTD Bus Trip Planner";
            win.ModifyBg(Gtk.StateType.Normal, new Gdk.Color(255, 255, 255));
            //win.Resizable = false;
            win.Show();
            Application.Run();

            // Test your code here
            //double result = getTimeOfArrivalEstimate("Union Station", "Knox Station");
            //Console.WriteLine("Bus Arrival ETA is: " + result);
        }


        // Returns all bus stops; gets called from MainWindow.cs
        public static List<Stop.stop_t> returnAllBusStops()
        {
			Uri myUri = new Uri("http://www.rtd-denver.com/google_sync/VehiclePosition.pb");
			WebRequest myWebRequest = HttpWebRequest.Create(myUri);

			HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;

			// This username and password is issued for the IWKS 4120 class. Please DO NOT redistribute.
			NetworkCredential myNetworkCredential = new NetworkCredential("RTDgtfsRT", "realT!m3Feed");    // insert credentials here

			CredentialCache myCredentialCache = new CredentialCache();
			myCredentialCache.Add(myUri, "Basic", myNetworkCredential);

			myHttpWebRequest.PreAuthenticate = true;
			myHttpWebRequest.Credentials = myCredentialCache;

            Stop stop_inst = new Stop();
			FeedMessage feed = Serializer.Deserialize<FeedMessage>(myWebRequest.GetResponse().GetResponseStream());

			foreach (FeedEntity entity in feed.entity)
            {
                if (entity.vehicle != null)
                {
                    if (entity.vehicle.trip != null)
                    {
                        if (entity.vehicle.trip.route_id != null)
                        {
                            Console.WriteLine("Vehicle ID = " + entity.vehicle.vehicle.id);
                            Console.WriteLine("Current Position Information:");
                            Console.WriteLine("Current Latitude = " + entity.vehicle.position.latitude);
                            Console.WriteLine("Current Longitude = " + entity.vehicle.position.longitude);
                            Console.WriteLine("Current Bearing = " + entity.vehicle.position.bearing);
                            Console.WriteLine("Current Status = " + entity.vehicle.current_status + " StopID: " + entity.vehicle.stop_id);
                            if (Stop.stops.ContainsKey(entity.vehicle.stop_id))
                            {
                                Console.WriteLine("The name of this StopID is \"" + Stop.stops[entity.vehicle.stop_id].stop_name + "\"");

                                var cd = new Stop.stop_t();                     //populates list with stops and their info
                                cd.stop_id = entity.vehicle.stop_id;
                                cd.stop_name = Stop.stops[entity.vehicle.stop_id].stop_name;
                                cd.stop_lat = Stop.stops[entity.vehicle.stop_id].stop_lat;
                                cd.stop_long = Stop.stops[entity.vehicle.stop_id].stop_long;
                                allstops.Add(cd);

                                Console.WriteLine("The Latitude of this StopID is \"" + Stop.stops[entity.vehicle.stop_id].stop_lat + "\"");
                                Console.WriteLine("The Longitude of this StopID is \"" + Stop.stops[entity.vehicle.stop_id].stop_long + "\"");
                                string wheelChairOK = "IS NOT";
                                if (Stop.stops[entity.vehicle.stop_id].wheelchair_access)
                                {
                                    wheelChairOK = "IS";
                                }
                                Console.WriteLine("This stop is " + wheelChairOK + " wheelchair accessible");
                            }

                            Console.WriteLine("Trip ID = " + entity.vehicle.trip.trip_id);
                            if (Trip.trips.ContainsKey(entity.vehicle.trip.trip_id))
                            {
                                if (entity.vehicle.current_status.ToString() == "IN_TRANSIT_TO")
                                {
                                    if (Stop.stops.ContainsKey(entity.vehicle.stop_id))
                                    {
                                        Console.WriteLine("Vehicle in transit to: " + Stop.stops[entity.vehicle.stop_id].stop_name);
                                        Trip.trip_t trip = Trip.trips[entity.vehicle.trip.trip_id];
                                        foreach (Trip.trip_stops_t stop in trip.tripStops)
                                        {
                                            // Console.WriteLine(stop.stop_id);      //should print whole trip with names of stops, only prints ID's
                                            foreach (Stop.stop_t s in allstops)
                                            {

                                                if (stop.stop_id == s.stop_id)      //dosent work correctly
                                                {
                                                    //      Console.WriteLine(s.stop_name);
                                                }

                                            }


                                            if (stop.stop_id == entity.vehicle.stop_id)
                                            {
                                                Console.WriteLine(".. and is scheduled to arrive there at " + stop.arrive_time);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return allstops;
        }



		// Takes bus stop name and calculates when next bus leaves
		public static double getNextDepartureTimeForStopName(string stopname)
		{
			Uri myUri = new Uri("http://www.rtd-denver.com/google_sync/TripUpdate.pb");
			//Uri myUri = new Uri("http://www.rtd-denver.com/google_sync/VehiclePosition.pb");
			WebRequest myWebRequest = HttpWebRequest.Create(myUri);

			HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;

			// This username and password is issued for the IWKS 4120 class. Please DO NOT redistribute.
			NetworkCredential myNetworkCredential = new NetworkCredential("RTDgtfsRT", "realT!m3Feed");    // insert credentials here

			CredentialCache myCredentialCache = new CredentialCache();
			myCredentialCache.Add(myUri, "Basic", myNetworkCredential);

			myHttpWebRequest.PreAuthenticate = true;
			myHttpWebRequest.Credentials = myCredentialCache;

			FeedMessage feed = Serializer.Deserialize<FeedMessage>(myWebRequest.GetResponse().GetResponseStream());

			//  Stop stop_inst = new Stop();
			Trip trip_inst = new Trip();
			foreach (FeedEntity entity in feed.entity)
			{
				if (entity.trip_update != null)
				{
					if (entity.trip_update.vehicle != null)
					{
						if (entity.trip_update.vehicle.id != null)


							for (int i = 0; i < entity.trip_update.stop_time_update.Count; i++)
							{


								if (Stop.stops.ContainsKey(entity.trip_update.stop_time_update[i].stop_id))
								{
									if (Stop.stops[entity.trip_update.stop_time_update[i].stop_id].stop_name == stopname)
									{
										if (entity.trip_update.stop_time_update[i].departure != null)
										{
                                            tripStatus = entity.trip_update.stop_time_update[i].schedule_relationship.ToString();
											return entity.trip_update.stop_time_update[i].departure.time;
										}
									}
								}

							}
					}
				}
			}
            tripStatus = "N/A";
			return 0;
		}


		// Takes starting and destination stop and gives time of arrival estimate
		public static double getTimeOfArrivalEstimate(string start, string finish)
		{
			Uri myUri = new Uri("http://www.rtd-denver.com/google_sync/TripUpdate.pb");
			//Uri myUri = new Uri("http://www.rtd-denver.com/google_sync/VehiclePosition.pb");
			WebRequest myWebRequest = HttpWebRequest.Create(myUri);

			HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;

			// This username and password is issued for the IWKS 4120 class. Please DO NOT redistribute.
			NetworkCredential myNetworkCredential = new NetworkCredential("RTDgtfsRT", "realT!m3Feed");    // insert credentials here

			CredentialCache myCredentialCache = new CredentialCache();
			myCredentialCache.Add(myUri, "Basic", myNetworkCredential);

			myHttpWebRequest.PreAuthenticate = true;
			myHttpWebRequest.Credentials = myCredentialCache;

			FeedMessage feed = Serializer.Deserialize<FeedMessage>(myWebRequest.GetResponse().GetResponseStream());

			//  Stop stop_inst = new Stop();
			Trip trip_inst = new Trip();

			foreach (FeedEntity entity in feed.entity)
			{
				if (entity.trip_update != null)
				{
					if (entity.trip_update.trip != null)
					{
						if (entity.trip_update.stop_time_update != null)
						{

							foreach (TripUpdate.StopTimeUpdate update in entity.trip_update.stop_time_update)
							{
								if (Stop.stops.ContainsKey(update.stop_id))
								{
									if (Stop.stops[update.stop_id].stop_name == start)
									{

										for (int i = 0; i < entity.trip_update.stop_time_update.Count; i++)
										{
											//if(entity.trip_update.vehicle.id == )
											if (Stop.stops[entity.trip_update.stop_time_update[i].stop_id].stop_name == finish)
											{
												//Console.WriteLine(entity.trip_update.stop_time_update[i].arrival.time);
												return entity.trip_update.stop_time_update[i].arrival.time;

											}
										}
										return 0;

									}
								}
							}
						}
					}
				}
			}
			return 0;
		}


		// Takes starting and destination stop and gives number of stops inbetween
		public static int getNumberOfStopsForTrip(string first, string last)
		{
			Uri myUri = new Uri("http://www.rtd-denver.com/google_sync/TripUpdate.pb");
			//Uri myUri = new Uri("http://www.rtd-denver.com/google_sync/VehiclePosition.pb");
			WebRequest myWebRequest = HttpWebRequest.Create(myUri);

			HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;

			// This username and password is issued for the IWKS 4120 class. Please DO NOT redistribute.
			NetworkCredential myNetworkCredential = new NetworkCredential("RTDgtfsRT", "realT!m3Feed");    // insert credentials here

			CredentialCache myCredentialCache = new CredentialCache();
			myCredentialCache.Add(myUri, "Basic", myNetworkCredential);

			myHttpWebRequest.PreAuthenticate = true;
			myHttpWebRequest.Credentials = myCredentialCache;

			FeedMessage feed = Serializer.Deserialize<FeedMessage>(myWebRequest.GetResponse().GetResponseStream());

			//  Stop stop_inst = new Stop();
			Trip trip_inst = new Trip();
			int stopcount = 0;

			foreach (FeedEntity entity in feed.entity)
			{
                stopcount = 0;
				if (entity.trip_update != null)
				{
					if (entity.trip_update.trip != null)
					{
						if (entity.trip_update.stop_time_update != null)
						{


							foreach (TripUpdate.StopTimeUpdate update in entity.trip_update.stop_time_update)
							{
								if (Stop.stops.ContainsKey(update.stop_id))
								{
									if (Stop.stops[update.stop_id].stop_name == first)
									{

										for (int i = 0; i < entity.trip_update.stop_time_update.Count; i++)
										{
											if (Stop.stops[entity.trip_update.stop_time_update[i].stop_id].stop_name == first)
											{

												for (int n = i; n < entity.trip_update.stop_time_update.Count; n++)
												{
													stopcount++;
													if (Stop.stops[entity.trip_update.stop_time_update[n].stop_id].stop_name == last)
													{
														//stopcount--;

														return stopcount;

													}
												}
											}

										}
										return 0;

									}
								}

							}



						}
					}
				}
			}
			return 0;
		}


	}   // End of Program.cs
} // End of Namespace

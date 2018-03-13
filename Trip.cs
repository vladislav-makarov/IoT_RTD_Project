using System.Collections.Generic;
using System.IO;

namespace RTD_UI_Application
{
	class Trip
	{
		public struct trip_stops_t
		{
			public string stop_seq;
			public string stop_id;
			public string arrive_time;
			public string dept_time;
		}

		public struct trip_t
		{
			public string trip_id;
			public List<trip_stops_t> tripStops;
		}

		const int TRIP_TRIP_ID = 0;
		const int TRIP_STOP_ARRIVE_TIME = 1;
		const int TRIP_STOP_DEPT_TIME = 2;
		const int TRIP_STOP_STOP_ID = 3;
		const int TRIP_STOP_STOP_SEQ = 4;

		const string stopTimesFileName = "stop_times.txt";

		public static Dictionary<string, trip_t> trips = new Dictionary<string, trip_t>() { };

		public Trip()
		{
			// constructor
			initializeTrips();
		}

		static void initializeTrips()
		{
			StreamReader file = new StreamReader(stopTimesFileName);
			string line;
			string[] row = new string[9];

			trip_t thisTrip;
			List<trip_stops_t> stops;

			while ((line = file.ReadLine()) != null)
			{
				row = line.Split(',');

				// There are 2 cases to consider
				//      1) this is the first time we have seen this trip ID
				//      2) we are build a list of stops for this trip

				// If this is the first time we have seen this trip ID, start a new dictionary entry
				// otherwise keep going with the current trip

				string tripID = row[TRIP_TRIP_ID];

				if (!trips.ContainsKey(tripID)) // we have never seen this trip ID; start a new trip
				{
					thisTrip = new trip_t { };
					thisTrip.trip_id = tripID;
					stops = new List<trip_stops_t>();
					stops.Add(new trip_stops_t
					{
						stop_id = row[TRIP_STOP_STOP_ID],
						stop_seq = row[TRIP_STOP_STOP_SEQ],
						arrive_time = row[TRIP_STOP_ARRIVE_TIME],
						dept_time = row[TRIP_STOP_DEPT_TIME]
					});
					thisTrip.tripStops = stops;
					trips.Add(tripID, thisTrip); // add this trip to the trips dictionary
				}
				else // we have seen this trip ID; append this stop to the stops list
				{
					trips[tripID].tripStops.Add(new trip_stops_t
					{
						stop_id = row[TRIP_STOP_STOP_ID],
						stop_seq = row[TRIP_STOP_STOP_SEQ],
						arrive_time = row[TRIP_STOP_ARRIVE_TIME],
						dept_time = row[TRIP_STOP_DEPT_TIME]
					});

				}

			}
			file.Close();
		}
	}
}

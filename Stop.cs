using System.Collections.Generic;
using System.IO;

namespace RTD_UI_Application
{
	public class Stop
	{
		public struct stop_t
		{
			public string stop_id;
			public string stop_name;
			public string stop_lat;
			public string stop_long;
			public bool wheelchair_access;
		}

		// CSV column Index
		const int STOP_STOP_ID = 0;
		const int STOP_NAME = 2;
		const int STOP_LAT = 4;
		const int STOP_LONG = 5;
		const int STOP_WHEELCHAIR = 11;

		const string stopsFileName = "stops.txt";

		public static Dictionary<string, stop_t> stops = new Dictionary<string, stop_t>() { };

		public Stop()
		{
			// constructor
			initializeStops();
		}

		static void initializeStops()
		{
			StreamReader file = new StreamReader(stopsFileName);
			string line;
			string[] row = new string[12];

			while ((line = file.ReadLine()) != null)
			{
				row = line.Split(',');
				stop_t thisStop = new stop_t { };
				thisStop.stop_id = row[STOP_STOP_ID];
				thisStop.stop_name = row[STOP_NAME];
				thisStop.stop_lat = row[STOP_LAT];
				thisStop.stop_long = row[STOP_LONG];
				if (row[STOP_WHEELCHAIR] == "1")
				{
					thisStop.wheelchair_access = true;
				}
				else
				{
					thisStop.wheelchair_access = false;
				}
				stops.Add(thisStop.stop_id, thisStop); // add this stop to the stops dictionary
			}
			file.Close();
		}
	}
}

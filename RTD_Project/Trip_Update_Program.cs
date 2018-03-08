using System;
using System.Net;
using ProtoBuf;
using transit_realtime;

namespace RTD_Project
{
    class Program
    {
        // This function converts the Unix time provided by RTD to a real date and time
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        static void Main(string[] args)
        {
            // This example parses the trip update data stream only, and does not use any of the static feeds
            Uri myUri = new Uri("http://www.rtd-denver.com/google_sync/TripUpdate.pb");
            WebRequest myWebRequest = HttpWebRequest.Create(myUri);

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;

            // This username nad password is issued for theIWKS 4120 class. Please DO NOT redistribute.
            NetworkCredential myNetworkCredential = new NetworkCredential("RTDgtfsRT", "realT!m3Feed"); // insert credentials

            CredentialCache myCredentialCache = new CredentialCache();
            myCredentialCache.Add(myUri, "Basic", myNetworkCredential);

            myHttpWebRequest.PreAuthenticate = true;
            myHttpWebRequest.Credentials = myCredentialCache;

            FeedMessage feed = Serializer.Deserialize<FeedMessage>(myWebRequest.GetResponse().GetResponseStream());

            foreach (FeedEntity entity in feed.entity)
            {
                if (entity.trip_update != null)
                {
                    if (entity.trip_update.trip != null)
                    {
                        if (entity.trip_update.stop_time_update != null)
                        {
                            Console.WriteLine("Trip ID  = " + entity.trip_update.trip.trip_id);
                            Console.WriteLine("Schedule Relationship  = " + entity.trip_update.trip.schedule_relationship.ToString());
                            Console.WriteLine("Route ID  = " + entity.trip_update.trip.route_id);
                            Console.WriteLine("Direction ID  = " + entity.trip_update.trip.direction_id);
                            if (entity.trip_update.vehicle != null)
                            {
                                Console.WriteLine("Vehicle ID = " + entity.trip_update.vehicle.id);
                            }
                            foreach (TripUpdate.StopTimeUpdate update in entity.trip_update.stop_time_update)
                            {
                                // StopTimeUpdates *may* have the following data:
                                //  stop_sequence:  uint
                                //  arrival:        StopTimeEvent - see below
                                //  departure:      StopTimeEvent - see below
                                //  stop_id:        string
                                //  schedule_relationsip:  SCHEDULED, SKIPPED, or NO_DATA

                                Console.WriteLine();
                                Console.WriteLine("Stop Sequence = " + update.stop_sequence.ToString());

                                //  Arrival and Departure are StopTimeEvents, which have three components
                                //  delay:          int
                                //  time:           long
                                //  uncertainty:    int 

                                if (update.arrival != null)
                                {
                                    Console.WriteLine("Arrival Time = " + UnixTimeStampToDateTime(update.arrival.time).ToString());
                                    //Console.WriteLine("Delay = " + update.arrival.delay.ToString()); // RTD appears to always sends 0
                                }
                                if (update.departure != null)
                                {
                                    Console.WriteLine("Departure Time = " + UnixTimeStampToDateTime(update.departure.time).ToString());
                                    //Console.WriteLine("Delay = " + update.departure.delay.ToString()); // RTD appears always sends 0
                                }
                                Console.WriteLine("Stop ID = " + update.stop_id);
                                Console.WriteLine("Schedule Relationship  = " + update.schedule_relationship.ToString());
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }

                Console.WriteLine("Press any key to continue");
                Console.ReadLine();
        }
    }
}




/* Denver RTD Trip Updates Feed Format
-------------------------------
header {
    gtfs_realtime_version: "1.0"
    incrementality: FULL_DATASET
    timestamp: 1449176392
}
entity {
    id: "1449176392_109470943"
    trip_update {
        trip {
            trip_id: "109470943"
            schedule_relationship: SCHEDULED
            route_id: "0"
            direction_id: 0
        }
        stop_time_update {
            stop_sequence: 6
            arrival {
                time: 1449176381
            }
            departure {
               time: 1449176381
            }
            stop_id: "25676"
            schedule_relationship: SCHEDULED
        }
        stop_time_update {
            stop_sequence: 7
            arrival {
                time: 1449176479
            }
            departure {
                time: 1449176479
            }
            stop_id: "22454"
            schedule_relationship: SCHEDULED
        }
        stop_time_update {
            stop_sequence: 8
            arrival {
                time: 1449176585
            }
            departure {
                time: 1449176585
            }
            stop_id: "20378"
            schedule_relationship: SCHEDULED
        }
        vehicle {
            id: "6010"
            label: "6010"
        }
        timestamp: 1449042054
    }
}
*/
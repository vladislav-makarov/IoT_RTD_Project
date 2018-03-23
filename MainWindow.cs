using System;
using Gdk;
using Pango;
using ProtoBuf;
using transit_realtime;
using System.Collections.Generic;
using GoogleMaps.LocationServices;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

using RTD_UI_Application;

/** Author: Vlad Makarov **/
public partial class MainWindow : Gtk.Window
{
    public static List<Stop.stop_t> allStops, allDestinations;
    public static String userSelectedStartLocation;           // holds user selected Start location
    public static String userSelectedDestinationLocation;     // holds user selected Destination location
    public FontDescription smallFontStyle, mediumFontStyle, mediumBoldFontStyle, bigFontStyle, bigBoldFontStyle, extraBigBoldFontStyle;
    public String userFullName = "";
    public String userLocation;
    public String userLocationText = "Your current location is:";
    public String helloText;
    public char setUnits = 'M';
    public Boolean goButtonVisible;
    public Boolean startSelected, destinationSelected;
    public double startLatitude, startLongitude;
    public double destinationLatitude, destinationLongitude;
    public double calcDistance;
    private GoogleLocationService service = new GoogleLocationService();
    private double userLatitude = 39.746025;
    private double userLongitude = -104.999083;
    private BackgroundWorker backgroundWorker1 = new BackgroundWorker();


    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        // Set up username and fonts
        setUpUserName();
        setUpFonts();
        setDistanceUnits();

        // Set labels
        setLabelTextWithStyle(helloLabel, helloText, extraBigBoldFontStyle);
        helloLabel.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(75, 0, 130));
        setLabelTextWithStyle(currentLocationText, userLocationText, mediumBoldFontStyle);

        // Get user location and update current location text
        setLabelTextWithStyle(currentLocationAddress, getUserLocationFromLatLong(), mediumFontStyle);

        // Set up and show current date and time
        showDateAndTime();

        // Set up start and destination texts
        setUpStartAndDestinationText();

        // Set up start and destination location combo boxes
        setUpStartLocationBox();
        setUpDestinationLocationBox();

        // Set up Go button
        setUpGoButton();
        setGoButtonVisible(goButtonVisible);

        // Set up Background worker and progress bar
        setUpBackgroundWorker();
    }

    public void setUpUserName()
    {
        userFullName = Bash("dscl . -read /Users/" + Environment.UserName + " RealName");
        helloText = "Hello," + userFullName + "!";
    }

    public void setUpFonts()
    {
        smallFontStyle = new FontDescription();
        smallFontStyle.Family = "Arial";
        smallFontStyle.Weight = Pango.Weight.Ultralight;
        smallFontStyle.Size = Convert.ToInt32(9 * Pango.Scale.PangoScale);

        mediumFontStyle = new FontDescription();
        mediumFontStyle.Family = "Arial";
        mediumFontStyle.Weight = Pango.Weight.Light;
        mediumFontStyle.Size = Convert.ToInt32(12 * Pango.Scale.PangoScale);

        mediumBoldFontStyle = new FontDescription();
        mediumBoldFontStyle.Family = "Arial";
        mediumBoldFontStyle.Weight = Pango.Weight.Semibold;
        mediumBoldFontStyle.Size = Convert.ToInt32(12 * Pango.Scale.PangoScale);

        bigFontStyle = new FontDescription();
        bigFontStyle.Family = "Arial";
        bigFontStyle.Weight = Pango.Weight.Light;
        bigFontStyle.Size = Convert.ToInt32(16 * Pango.Scale.PangoScale);

        bigBoldFontStyle = new FontDescription();
        bigBoldFontStyle.Family = "Arial";
        bigBoldFontStyle.Weight = Pango.Weight.Bold;
        bigBoldFontStyle.Size = Convert.ToInt32(16 * Pango.Scale.PangoScale);

        extraBigBoldFontStyle = new FontDescription();
        extraBigBoldFontStyle.Family = "Arial";
        extraBigBoldFontStyle.Weight = Pango.Weight.Semibold;
        extraBigBoldFontStyle.Size = Convert.ToInt32(20 * Pango.Scale.PangoScale);
    }

    public void setLabelTextWithStyle(Gtk.Label label, String text, FontDescription style)
    {
        if (label != null && text != null)
        {
            label.Text = text;
            label.ModifyFont(style);
        }
    }

    public void showDateAndTime()
    {
        setLabelTextWithStyle(dateText, DateTime.Now.ToString("dddd, MMMM dd"), bigFontStyle);
        setLabelTextWithStyle(currentTimeText, DateTime.Now.ToString("hh:mm tt"), extraBigBoldFontStyle);
        dateText.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(128, 128, 128));
        currentTimeText.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(128, 128, 128));

        // Update date and time every 30 seconds
        System.Threading.Timer timer = new System.Threading.Timer((e) =>
        {
            updateDateAndTime();
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(0.5));
    }

    public void updateDateAndTime()
    {
        setLabelTextWithStyle(dateText, DateTime.Now.ToString("dddd, MMMM dd"), bigFontStyle);
        setLabelTextWithStyle(currentTimeText, DateTime.Now.ToString("hh:mm tt"), extraBigBoldFontStyle);
    }

    public void setUpStartLocationBox()
    {
        startBox.Name = "startAtBox";
        startBox.TooltipText = "Please select your Starting Location (RTD Stop)";
        startBox.SetSizeRequest(560, 30);
        allStops = Program.returnAllBusStops();

		Console.WriteLine("\nSort Based on Name");
        Comparison<Stop.stop_t> comparisonModel = new Comparison<Stop.stop_t>(Stop.CompareTwoStops);
		allStops.Sort(comparisonModel);

        int i = 0;
        foreach (Stop.stop_t s in allStops)
        {
            //Console.WriteLine(s.stop_name);
            startBox.InsertText(i, s.stop_name);
            i++;
        }

        startBox.Changed += new EventHandler(onComboBoxChanged);
    }

    public void setUpDestinationLocationBox()
    {
        destinationBox.Name = "destinationBox";
        destinationBox.TooltipText = "Please select your Destination (RTD Stop)";
        destinationBox.SetSizeRequest(560, 30);
        allDestinations = allStops;
        //allDestinations = Program.returnAllBusStops();

        int i = 0;
        foreach (Stop.stop_t s in allDestinations)
        {
            //Console.WriteLine(s.stop_name);
            destinationBox.InsertText(i, s.stop_name);
            i++;
        }


        destinationBox.Changed += new EventHandler(onComboBoxChanged);
    }

    public void setUpStartAndDestinationText()
    {
        setLabelTextWithStyle(startAtText, "Start at", bigBoldFontStyle);
        setLabelTextWithStyle(destinationText, "Destination", bigBoldFontStyle);
        setLabelTextWithStyle(startAtTextNote, "  displaying\nclosest to you", smallFontStyle);

        // Set color to text 
        startAtText.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(0, 204, 102));
        destinationText.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(204, 0, 0));
        startAtTextNote.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(128, 128, 128));

        tripDistance.Visible = false;
        nextBusDeparture.Visible = false;
        estimatedArrival.Visible = false;
        numberOfStops.Visible = false;
    }

    public String getUserLocationFromLatLong()
    {
        // convert lat, long to physical address and return it
        String result = service.GetAddressFromLatLang(userLatitude, userLongitude).ToString();
        if (System.String.IsNullOrEmpty(result))
        {
            result = "Lawrence Street Center, Denver, CO, 80204";
        }
        //String result = "lat: " + service.GetLatLongFromAddress(userLocation).Latitude.ToString() + ", long: " + service.GetLatLongFromAddress(userLocation).Longitude.ToString();
        //Console.WriteLine("result:  ****   " + result)

        return result;
    }

    /* Takes: String address, returns: double latitude */
    public double getUserLatitudeFromLocation(String userLocation)
    {
        return service.GetLatLongFromAddress(userLocation).Latitude;
    }

    /* Takes: String address, returns: double longitude */
    public double getUserLongitudeFromLocation(String userLocation)
    {
        return service.GetLatLongFromAddress(userLocation).Longitude;
    }

	public void setUpBackgroundWorker()
	{
		backgroundWorker1.DoWork += backgroundWorker1_DoWork;
		backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
		backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted; 
		backgroundWorker1.WorkerReportsProgress = true;
		backgroundWorker1.WorkerSupportsCancellation = true;
		progressbar1.Visible = false;
	}

    public void setUpGoButton()
    {
        //goButton.ModifyFont(bigFontStyle);
        goButton.TooltipText = "Show trip details for Selected Trip";
        goButton.Clicked += new EventHandler(goButtonClicked);
    }

    public void setGoButtonVisible(Boolean visible)
    {
        goButton.Sensitive = visible;
    }

    public void goButtonClicked(object obj, EventArgs args)
    {
        backgroundWorker1.RunWorkerAsync();
    }

    public void onComboBoxChanged(object o, EventArgs args)
    {
        Gtk.ComboBox combo = o as Gtk.ComboBox;
        if (o == null)
            return;

        Gtk.TreeIter iter;

        if (combo.GetActiveIter(out iter))
        {
            if (combo.Name == "startAtBox")
            {
                userSelectedStartLocation = (string)combo.Model.GetValue(iter, 0);
                startSelected = true;
                Console.WriteLine("*** Start Location selected: " + userSelectedStartLocation);
            }
            else if (combo.Name == "destinationBox")
            {
                userSelectedDestinationLocation = (string)combo.Model.GetValue(iter, 0);
                destinationSelected = true;
                Console.WriteLine("*** Destination Location selected: " + userSelectedDestinationLocation);
            }

            // update Go button visibility
            setGoButtonVisible(startSelected && destinationSelected);
        }
    }

    /* Performs search on combo box contents for desired stop name (value) */
    public void searchComboBoxFor(List<Stop.stop_t> comboBoxContents, string searchValue, Boolean startBox)
    {
        int i = 0;
        foreach (Stop.stop_t s in comboBoxContents)
        {
            if (s.stop_name == searchValue)
            {
                if (startBox)
                {
                    startLatitude = Convert.ToDouble(s.stop_lat);
                    startLongitude = Convert.ToDouble(s.stop_long);
                    Console.WriteLine("Start latitude: " + (s.stop_long).ToString());
                    Console.WriteLine("Start longitude: " + (s.stop_long).ToString());
                }
                else
                {
                    destinationLatitude = Convert.ToDouble(s.stop_lat);
                    destinationLongitude = Convert.ToDouble(s.stop_long);
                    Console.WriteLine("Destination latitude: " + s.stop_long);
                    Console.WriteLine("Destination longitude: " + s.stop_long);
                }

            }
            //Console.WriteLine(s.stop_name);
            //destinationBox.InsertText(i, s.stop_name);
            i++;
        }
    }

    public string Bash(string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        string result = null;
        while (!process.StandardOutput.EndOfStream)
        {
            result = process.StandardOutput.ReadLine();
        }
        process.WaitForExit();
        return result;
    }

    /* 
	 * Calculates distance between two (latitude,longitude) points
	 * Units: Pass 'K' for kilometers, 'M' for miles (char)
     * -> Usage examples:
     *  distance(32.9697, -96.80322, 29.46786, -98.53506, 'M');
     *  Console.WriteLine(distance(32.9697, -96.80322, 29.46786, -98.53506, 'K'));
     *  Console.WriteLine(distance(32.9697, -96.80322, 29.46786, -98.53506, 'M'));
     */
    public static double distance(double lat1, double lon1, double lat2, double lon2, char unit)
    {
        double theta = lon1 - lon2;
        double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
        dist = Math.Acos(dist);
        dist = rad2deg(dist);
        dist = dist * 60 * 1.1515;
        if (unit == 'K')
        {
            dist = dist * 1.609344;
        }
        else if (unit == 'M')
        {
            dist = dist * 0.8684;
        }
        else
        {
            return 0;       // units not recognized, return 0
        }
        return dist;
    }

    public static double deg2rad(double deg)
    {
        return (deg * Math.PI / 180.0);
    }

    public static double rad2deg(double rad)
    {
        return (rad / Math.PI * 180.0);
    }

	public static double miles2km(double miles)
	{
		return miles * 1.609344;
	}

	public static double km2miles(double kilometers)
	{
		//
		// Multiply by this constant.
		//
		return kilometers * 0.621371192;
	}

    public void resetStartDestinationCoordinates()
    {
        startLatitude = 0;
        startLongitude = 0;
        destinationLatitude = 0;
        destinationLongitude = 0;
    }

    public void updateTextForTripDistance(char units)
    {
        if (units == 'M')
        {
            if (setUnits != units) {
                calcDistance = km2miles(calcDistance);
                setUnits = units;
            }
            setLabelTextWithStyle(tripDistance, "Your Trip's Estimated Distance is:  " + calcDistance.ToString("0.0") + " miles", mediumBoldFontStyle);
        }
        else if (units == 'K')
        {
			if (setUnits != units)
			{
                calcDistance = miles2km(calcDistance);
                setUnits = units;
			}
            setLabelTextWithStyle(tripDistance, "Your Trip's Estimated Distance is:  " + calcDistance.ToString("0.0") + " kilometers", mediumBoldFontStyle);
        }
    }

    public void updateNextBusDepartureTime()
    {
        double time = Program.getNextDepartureTimeForStopName(userSelectedStartLocation);
        if (time > 0)
        {
            DateTime estimatedTime = convertUnixTimeStampToDateTime(time);
            nextBusDeparture.Text = "Next Bus Departure is:  @" + estimatedTime.ToString("hh:mm tt on dddd, MMMM dd");
            setLabelTextWithStyle(nextBusDeparture, nextBusDeparture.Text, mediumBoldFontStyle);
        }
        else
        {
            nextBusDeparture.Text = "Next Bus Departure is:  no active busses for that stop";
            setLabelTextWithStyle(nextBusDeparture, nextBusDeparture.Text, mediumBoldFontStyle);
        }
    }

    public void updateEstimatedArrivalTime() {
        double eta = Program.getTimeOfArrivalEstimate(userSelectedStartLocation, userSelectedDestinationLocation);

        if (eta > 0) {
			DateTime estimatedTime = convertUnixTimeStampToDateTime(eta);
			estimatedArrival.Text = "Estimated Time of Arrival is:  @" + estimatedTime.ToString("hh:mm tt on dddd, MMMM dd");
			setLabelTextWithStyle(estimatedArrival, estimatedArrival.Text, mediumBoldFontStyle);
        }
        else {
            estimatedArrival.Text = "Estimated Time of Arrival is:  no bus route between these stops";
            setLabelTextWithStyle(estimatedArrival, estimatedArrival.Text, mediumBoldFontStyle);
        }
    }

    public void updateNumberOfStopsForTrip() {
        int result = Program.getNumberOfStopsForTrip(userSelectedStartLocation, userSelectedDestinationLocation);

		numberOfStops.Text = "Number of Stops for Selected Trip:  " + result;
        setLabelTextWithStyle(numberOfStops, numberOfStops.Text, mediumBoldFontStyle);
    }

	//Start Process
	private void button1_Click(object sender, EventArgs e)
	{
		backgroundWorker1.RunWorkerAsync();
	}

	//Cancel Process
	private void button2_Click(object sender, EventArgs e)
	{
		//Check if background worker is doing anything and send a cancellation if it is
		if (backgroundWorker1.IsBusy)
		{
			backgroundWorker1.CancelAsync();
		}

	}

	private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
	{
		// Hide all result UI elements and disable go button
        tripDistance.Visible = false;
        nextBusDeparture.Visible = false;
        estimatedArrival.Visible = false;
        numberOfStops.Visible = false;

        goButton.Visible = false;


        // Update latitude and longitude for user selected start location
		searchComboBoxFor(allStops, userSelectedStartLocation, true);

		// Update latitude and longitude for user selected destination location
		searchComboBoxFor(allDestinations, userSelectedDestinationLocation, false);

		if (startLatitude != 0 && destinationLatitude != 0)
		{
			calcDistance = distance(startLatitude, startLongitude,
			destinationLatitude, destinationLongitude, setUnits);
			//Console.WriteLine("INSIDE goButtonClicked");
			resetStartDestinationCoordinates();

			// get Trip scan results
			updateTextForTripDistance(setUnits);
            backgroundWorker1.ReportProgress(25);
			updateNextBusDepartureTime();
            backgroundWorker1.ReportProgress(50);
			updateEstimatedArrivalTime();
            backgroundWorker1.ReportProgress(75);
			updateNumberOfStopsForTrip();
            backgroundWorker1.ReportProgress(100);

			// display Trip scan results
            tripDistance.Visible = true;
			nextBusDeparture.Visible = true;
			estimatedArrival.Visible = true;
			numberOfStops.Visible = true;
		}

		//Check if there is a request to cancel the process
		if (backgroundWorker1.CancellationPending)
		{
			e.Cancel = true;
			backgroundWorker1.ReportProgress(0);
			return;
		}

        goButton.Visible = true;
        return;
	}

	private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
	{
        progressbar1.Visible = true;
        progressbar1.Text = "Computing results (" + e.ProgressPercentage + "%)";
        progressbar1.Adjustment.Value = e.ProgressPercentage;
	}

	private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
	{
		if (e.Cancelled)
		{
            progressbar1.Text = "Request cancelled";
		}
		else
		{
			progressbar1.Text = "Computed Successfully";
		}

		// Hide progress bar after 5 seconds
		Thread.Sleep(5000);
		progressbar1.Visible = false;
	}
    public static DateTime convertUnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    public void setDistanceUnits()
    {
        kilometersRadioButton.Pressed += new EventHandler(onKilometersToggled);
        milesRadioButton.Pressed += new EventHandler(onMilesToggled);
    }

    void onMilesToggled(object obj, EventArgs args)
    {
        updateTextForTripDistance('M');
    }

    void onKilometersToggled(object obj, EventArgs args) {
        updateTextForTripDistance('K');
	}

}

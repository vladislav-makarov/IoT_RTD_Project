using System;
using Gdk;
using Pango;
using ProtoBuf;
using transit_realtime;
using System.Collections.Generic;

using RTD_UI_Application;

/** Author: Vlad Makarov **/
public partial class MainWindow : Gtk.Window
{
    public static List<Stop.stop_t> allStops, allDestinations;
    public FontDescription smallFontStyle, mediumFontStyle, mediumBoldFontStyle, bigFontStyle, bigBoldFontStyle, extraBigBoldFontStyle;
    public String userName = "John Smith";
    public String userLocation = "Lawrence Street Center, 1380 Lawrence St, Denver, CO 80204";
    public String userLocationText = "Your current location is:";
    public String helloText;

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        userName = Environment.UserName;
        helloText = "Hello, " + userName + "!";

        // Set up fonts
        setUpFonts();

        // Set labels
        setLabelTextWithStyle(this.helloLabel, helloText, extraBigBoldFontStyle);
        helloLabel.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(75, 0, 130));
        setLabelTextWithStyle(this.currentLocationText, userLocationText, mediumBoldFontStyle);
        setLabelTextWithStyle(this.currentLocationAddress, userLocation, mediumFontStyle);

        // Set up and show current date and time
        showDateAndTime();

        // Set up start and destination texts
        setUpStartAndDestinationText();

        // Set up start and destination location combo boxes
        setUpStartLocationBox();
        setUpDestinationLocationBox();
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
        setLabelTextWithStyle(this.dateText, DateTime.Now.ToString("dddd, MMMM dd"), bigFontStyle);
        setLabelTextWithStyle(this.currentTimeText, DateTime.Now.ToString("hh:mm tt"), extraBigBoldFontStyle);
        this.dateText.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(128, 128, 128));
        this.currentTimeText.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(128, 128, 128));
    }

    public void setUpStartLocationBox()
    {
        this.startBox.SetSizeRequest(600, 30);
        allStops = Program.returnAllBusStops();
       
        int i = 0;
        foreach (Stop.stop_t s in allStops)
        {
            //Console.WriteLine(s.stop_name);
            this.startBox.InsertText(i, s.stop_name);
            i++;      
        }
    }

    public void setUpDestinationLocationBox()
    {
        this.destinationBox.SetSizeRequest(600, 30);
        allDestinations = allStops;
		//allDestinations = Program.returnAllBusStops();

		int i = 0;
		foreach (Stop.stop_t s in allDestinations)
		{
            //Console.WriteLine(s.stop_name);
            this.destinationBox.InsertText(i, s.stop_name);
			i++;
		}
    }

    public void setUpStartAndDestinationText()
    {
        setLabelTextWithStyle(this.startAtText, "Start at", bigBoldFontStyle);
        setLabelTextWithStyle(this.destinationText, "Destination", bigBoldFontStyle);
        setLabelTextWithStyle(this.startAtTextNote, "  displaying\nclosest to you", smallFontStyle);

        // Set color to text 
        this.startAtText.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(0, 204, 102));
        this.destinationText.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(204, 0, 0));
        this.startAtTextNote.ModifyFg(Gtk.StateType.Normal, new Gdk.Color(128, 128, 128));
    }
}

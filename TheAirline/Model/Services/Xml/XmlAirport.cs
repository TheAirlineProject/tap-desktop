using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using System.Xml;
using System.Globalization;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.Services.Xml
{
    public class XmlAirport
    {
        private readonly AirportProfile profile;
        private readonly List<Runway> runways;
        private readonly List<Terminal> terminals;
        private Xml xml;

        public XmlAirport(AirportProfile profile, List<Runway> runways, List<Terminal> terminals)
        {
            this.profile = profile;
            this.runways = runways;
            this.terminals = terminals;
        }

        public void SetXmlDocument(Xml xml)
        {
            this.xml = xml;
        }

        public XmlElement GetElement(Xml doc)
        {
            XmlElement airportNode = GetAirportNode();

            if (PeriodIsValid())
            {
                airportNode.AppendChild(GetPeriodNode());
            }

            airportNode.AppendChild(GetTownNode());
            airportNode.AppendChild(GetCoordinateNode());
            airportNode.AppendChild(GetSizeNode());
            airportNode.AppendChild(GetTerminalsNode());
            airportNode.AppendChild(GetRunwaysNode());

            return airportNode;
        }

        private XmlElement GetAirportNode()
        {
            XmlElement airportNode = xml.CreateElement("airport");

            airportNode.SetAttribute("name", profile.Name);
            airportNode.SetAttribute("icao", profile.ICAOCode);
            airportNode.SetAttribute("iata", profile.IATACode);
            airportNode.SetAttribute("type", profile.Type.ToString());
            airportNode.SetAttribute("season", profile.Season.ToString());

            return airportNode;
        }

        private XmlElement GetCoordinateNode()
        {
            XmlElement coordinatesNode = xml.CreateElement("coordinates");

            XmlElement latitudeNode = xml.CreateElement("latitude");
            latitudeNode.SetAttribute("value", profile.Coordinates.Latitude.toLongString(true));
            coordinatesNode.AppendChild(latitudeNode);

            XmlElement longitudeNode = xml.CreateElement("longitude");
            longitudeNode.SetAttribute("value", profile.Coordinates.Longitude.toLongString(false));
            coordinatesNode.AppendChild(longitudeNode);

            return coordinatesNode;
        }

        private XmlElement GetPeriodNode()
        {
            XmlElement periodNode = xml.CreateElement("period");
            periodNode.SetAttribute("from", profile.Period.From.ToString(new CultureInfo("en-US", false)));
            periodNode.SetAttribute("to", profile.Period.To.ToString(new CultureInfo("en-US", false)));

            return periodNode;
        }

        private bool PeriodIsValid()
        {
            return profile.Period.From.Year >= 1960 && profile.Period.To.Year <= 2199;
        }

        private XmlElement GetTownNode()
        {
            XmlElement townNode = xml.CreateElement("town");
            townNode.SetAttribute("town", GetTownName());
            townNode.SetAttribute("country", profile.Town.Country.Uid);
            townNode.SetAttribute("GMT", profile.OffsetGMT.ToString());
            townNode.SetAttribute("DST", profile.OffsetDST.ToString());

            return townNode;
        }

        private string GetTownName()
        {
            string town = profile.Town.Name;

            if (profile.Town.State != null)
            {
                town += ", " + profile.Town.State.ShortName;
            }

            return town;
        }

        private XmlElement GetSizeNode()
        {
            XmlElement sizeNode = xml.CreateElement("size");
            sizeNode.SetAttribute("value", profile.Size.ToString());
            sizeNode.SetAttribute("pax", profile.Pax.ToString());
            sizeNode.SetAttribute("cargo", profile.Cargo.ToString());
            sizeNode.SetAttribute("cargovolume", profile.CargoVolume.ToString());

            return sizeNode;
        }

        private XmlElement GetRunwaysNode()
        {
            XmlElement airportRunwaysNode = xml.CreateElement("runways");

            foreach (Runway runway in runways)
            {
                airportRunwaysNode.AppendChild(GetRunwayNode(runway));
            }

            return airportRunwaysNode;
        }

        private XmlElement GetRunwayNode(Runway runway)
        {
            XmlElement node = xml.CreateElement("runway");
            node.SetAttribute("name", runway.Name);
            node.SetAttribute("length", runway.Length.ToString());
            node.SetAttribute("surface", runway.Surface.ToString());

            return node;
        }

        private XmlElement GetTerminalsNode()
        {
            XmlElement terminalsNode = xml.CreateElement("terminals");

            foreach (Terminal terminal in terminals)
            {
                terminalsNode.AppendChild(GetTerminalNode(terminal));
            }

            return terminalsNode;
        }

        private XmlElement GetTerminalNode(Terminal terminal)
        {
            XmlElement node = xml.CreateElement("terminal");
            node.SetAttribute("name", terminal.Name);
            node.SetAttribute("gates", terminal.Gates.getGates().Count.ToString());

            return node;
        }
    }
}

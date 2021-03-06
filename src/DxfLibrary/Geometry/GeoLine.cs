// GeoLine.cs
// Created By: Adam Renaud
// Created on: 2019-01-09

using System;
using System.Collections.Generic;
using System.Linq;

using DxfLibrary.GeoMath;

namespace DxfLibrary.Geometry
{
    /// <summary>
    /// Geometric class that defines a segment.
    /// This segment can either be a line or an arc
    /// </summary>
    public class GeoLine : GeoBase, IGeoLength, IGeoArea, IVectorable
    {
        #region Constructors

        /// <summary>
        /// Default Constructor for the Point
        /// </summary>
        public GeoLine() 
            : this(new GeoPoint(0,0), new GeoPoint(0,0), new Bulge(0))
        {
        }

        /// <summary>
        /// Constructor from two points
        /// </summary>
        /// <param name="p0">First Point</param>
        /// <param name="p1">Second Point</param>
        public GeoLine(GeoPoint p0, GeoPoint p1) 
            : this(p0, p1, new Bulge(0))
        {
        }

        /// <summary>
        /// Constructor for the geoline that takes two points
        /// and a bulge
        /// </summary>
        /// <param name="p0">Starting point</param>
        /// <param name="p1">Ending point</param>
        /// <param name="bulge">The Bulge</param>
        public GeoLine(GeoPoint p0, GeoPoint p1, Bulge bulge)
        {
            this.Bulge = bulge;
            Point0 = p0;
            Point1 = p1;
        }

        /// <summary>
        /// Constructor from a center point, starting angle,
        /// ending angle, and radius. Note that all angles are in radians.
        /// </summary>
        /// <param name="geoCenter">The Center point</param>
        /// <param name="startAngle">The Starting Angle (Radians)</param>
        /// <param name="endAngle">The Ending Angle (Radians)</param>
        /// <param name="radius">The Radius of the segment</param>
        public GeoLine(GeoPoint geoCenter, double startAngle, double endAngle, double radius) :
            this(new GeoPoint(geoCenter.X + radius * Math.Cos(startAngle), geoCenter.Y + radius * Math.Sin(startAngle), geoCenter.Z),
                 new GeoPoint(geoCenter.X + radius * Math.Cos(endAngle), geoCenter.Y + radius * Math.Sin(endAngle), geoCenter.Z),
                 Geometry.Bulge.FromAngle(endAngle - startAngle))
        {
            
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Starting point of the line
        /// </summary>
        public GeoPoint Point0 {get; set;}

        /// <summary>
        /// Ending Point of the line
        /// </summary>
        public GeoPoint Point1 {get; set;}

        /// <summary>
        /// The Bulge of the segment
        /// </summary>
        public Bulge Bulge {get;}

        /// <summary>
        /// Returns True if the line has a bulge, and false
        /// if the file does not have a bulge
        /// </summary>
        public bool HasBulge => !(BasicGeometry.DoubleCompare(Bulge.Value, 0));

        /// <summary>
        /// The Length of the Line.
        /// </summary>
        /// <remarks>
        /// If the Bulge is 0: The length will return the distance
        /// from point 0 to point 1
        /// 
        /// If the Bulge is not 0: The length will return the arc
        /// length from by an arc from point 0 to point 1 with a 
        /// Bulge of the Bulge.Value
        /// </remarks>
        public double Length => CalcLength();

        /// <summary>
        /// Area of the Segment. The Area of the segment
        /// Is the area of a trapizoid that is bounded by the two points of the
        /// line. Note that if the segment is an arc then the area is the area
        /// bounded by that arc.
        /// </summary>
        public double Area => CalcArea();

        /// <summary>
        /// Radius of the Arc, if the Bulge is not 0.
        /// If the Bulge is 0 this should return positive infinity
        /// </summary>
        public double Radius 
            => Bulge.Value != 0 ? Bulge.Radius(Point0, Point1) : double.PositiveInfinity;

        /// <summary>
        /// The angle that the arc makes.
        /// If the bulge is zero then the angle will return 
        /// positive infinity
        /// </summary>
        /// <remarks>
        /// If there is no arc then the segment is a line and
        /// will then have an angle of PI (180 degrees). This angle will be returned 
        /// in radians.
        /// </remarks>
        public double Angle
            => Bulge.Value != 0 ? Bulge.Angle : Math.PI;

        #endregion

        #region Public Methods

        /// <summary>
        /// Override of the ToString Method
        /// </summary>
        /// <returns>Returns: The Lines points and coordinates</returns>
        public override string ToString()
        {
            return $"P0({Point0.X}, {Point0.Y}, {Point0.Z}), P1({Point1.X}, {Point1.Y}, {Point1.Z})";
        }

        /// <summary>
        /// Public Override of the Equals Function. This function
        /// will compare the following properties of a GeoLine:
        /// - Point0
        /// - Point1
        /// - Bulge Value
        /// </summary>
        /// <param name="obj">Object we are comparing to</param>
        /// <returns>Returns: True if the lines are the same and false if they are not</returns>
        public override bool Equals(object obj)
        {
            // Try and convert the obj to a line
            // if it returns null then this is not a line
            // and return false
            var line = obj as GeoLine;
            if (line == null)
                return false;

            // Compare the major properties of the two lines
            return Point0.Equals(line.Point0) && Point1.Equals(line.Point1) 
                && BasicGeometry.DoubleCompare(Bulge.Value, line.Bulge.Value);
        }

        /// <summary>
        /// Public Override of the GetHashCode Function. This function will hash
        /// the following properties of this type:
        /// - Point0
        /// - Point1
        /// - Bulge Value
        /// </summary>
        /// <returns>Returns: A hash of the object</returns>
        public override int GetHashCode()
        {
            int hash = 238728232;
            int otherValue = 928392928;
            hash = (hash * otherValue) + Point0.GetHashCode();
            hash = (hash * otherValue) + Point1.GetHashCode();
            hash = (hash * otherValue) + Bulge.Value.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Convert this entity to a vector
        /// </summary>
        /// <returns>
        /// Returns: A new vector that has the origin and destination
        /// the same as this lines point 0 and point 1
        /// </returns>
        public Vector ToVector() => new Vector(Point0, Point1);

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculate the length of the Segment.
        /// </summary>
        /// <remarks>
        /// If the bulge value is 0: Returns the length of the line segment
        /// 
        /// If the bulge value is not 0: Returns the arc length of the arc segment
        /// </remarks>
        /// <returns>The length of the segment</returns>
        private double CalcLength()
        {
            // If the line has no bulge then calculate the straight length
            // between the two points
            if (Bulge.Value == 0) return BasicGeometry.Distance(Point0, Point1);

            // Return the arc length of the arc if the Bulge is not equal to 0
            return BasicGeometry.ArcLength(Bulge.Radius(Point0, Point1), Bulge.Angle);
        }

        /// <summary>
        /// Calculate the area of the shape that
        /// bounds the line and the x axis.
        /// Note that the shape can include the area of the chord
        /// if the line is an arc
        /// </summary>
        /// <returns>The area of the shape that is defined above</returns>
        private double CalcArea()
        {
            // If there is no bulge then treat as a regular line
            if (Bulge.Value == 0) return BasicGeometry.TrapzArea(this);

            // If the bulge value is not equal to 0 then return the 
            // Area of the circle segment
            return Math.Abs(BasicGeometry.CircleSegmentArea(Bulge.Radius(Point0, Point1), Bulge.Angle));
        }

        #endregion
    }
}
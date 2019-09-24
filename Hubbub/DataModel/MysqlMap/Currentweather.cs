using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class Currentweather
    {
        /// <summary>
        /// There are no comments for ID in the schema.
        /// </summary>
        public virtual int ID
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Lat in the schema.
        /// </summary>
        public virtual double Lat
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Lon in the schema.
        /// </summary>
        public virtual double Lon
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Main in the schema.
        /// </summary>
        public virtual string Main
        {
            get;
            set;
        }

        public virtual string Cityname { get; set; }


        /// <summary>
        /// There are no comments for Icon in the schema.
        /// </summary>
        public virtual string Icon
        {
            get;
            set;
        }

        /// <summary>
        /// There are no comments for Description in the schema.
        /// </summary>
        public virtual string Description
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Temp in the schema.
        /// </summary>
        public virtual float Temp
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Pressure in the schema.
        /// </summary>
        public virtual float? Pressure
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Humidity in the schema.
        /// </summary>
        public virtual float? Humidity
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for TempMin in the schema.
        /// </summary>
        public virtual float? TempMin
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for TempMax in the schema.
        /// </summary>
        public virtual float? TempMax
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Clouds in the schema.
        /// </summary>
        public virtual float? Clouds
        {
            get;
            set;
        }



        /// <summary>
        /// There are no comments for Timestamp in the schema.
        /// </summary>
        public virtual System.DateTime Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// There are no comments for Timestamp in the schema.
        /// </summary>
        public virtual System.DateTime? Sunrise
        {
            get;
            set;
        }

        /// <summary>
        /// There are no comments for Timestamp in the schema.
        /// </summary>
        public virtual System.DateTime? Sunset
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Siteid in the schema.
        /// </summary>
        public virtual int Siteid
        {
            get;
            set;
        }

        /// <summary>
        /// There are no comments for Siteid in the schema.
        /// </summary>
        public virtual int Code
        {
            get;
            set;
        }

        /// <summary>
        /// There are no comments for Rain1h in the schema.
        /// </summary>
        public virtual float? Rain1h
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Rain3h in the schema.
        /// </summary>
        public virtual float? Rain3h
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Snow1h in the schema.
        /// </summary>
        public virtual float? Snow1h
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Snow3h in the schema.
        /// </summary>
        public virtual float? Snow3h
        {
            get;
            set;
        }

    }
}

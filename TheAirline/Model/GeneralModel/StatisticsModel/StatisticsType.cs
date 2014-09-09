﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    //the class for a type of statistics
    [Serializable]
    public class StatisticsType : ISerializable
    {
        #region Constructors and Destructors

        public StatisticsType(string name, string shortname)
        {
            Name = name;
            Shortname = shortname;
        }

        private StatisticsType(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            IEnumerable<FieldInfo> fields =
                GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop =
                    propsAndFields.FirstOrDefault(
                        p => ((Versioning) p.GetCustomAttribute(typeof (Versioning))).Name == entry.Name);

                if (prop != null)
                {
                    if (prop is FieldInfo)
                    {
                        ((FieldInfo) prop).SetValue(this, entry.Value);
                    }
                    else
                    {
                        ((PropertyInfo) prop).SetValue(this, entry.Value);
                    }
                }
            }

            IEnumerable<MemberInfo> notSetProps =
                propsAndFields.Where(p => ((Versioning) p.GetCustomAttribute(typeof (Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                var ver = (Versioning) notSet.GetCustomAttribute(typeof (Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                    {
                        ((FieldInfo) notSet).SetValue(this, ver.DefaultValue);
                    }
                    else
                    {
                        ((PropertyInfo) notSet).SetValue(this, ver.DefaultValue);
                    }
                }
            }
        }

        #endregion

        #region Public Properties

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("shortname")]
        public string Shortname { get; set; }

        #endregion

        #region Public Methods and Operators

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = GetType();

            IEnumerable<FieldInfo> fields =
                myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                      .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                          .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                {
                    propValue = ((FieldInfo) member).GetValue(this);
                }
                else
                {
                    propValue = ((PropertyInfo) member).GetValue(this, null);
                }

                var att = (Versioning) member.GetCustomAttribute(typeof (Versioning));

                info.AddValue(att.Name, propValue);
            }
        }

        #endregion
    }

    //the collection of statistics types
    public class StatisticsTypes
    {
        #region Static Fields

        private static Dictionary<string, StatisticsType> _types = new Dictionary<string, StatisticsType>();

        #endregion

        #region Public Methods and Operators

        public static void AddStatisticsType(StatisticsType type)
        {
            _types.Add(type.Shortname, type);
        }

        public static void Clear()
        {
            _types = new Dictionary<string, StatisticsType>();
        }

        //return a statistics type
        public static StatisticsType GetStatisticsType(string name)
        {
            return _types[name];
        }

        //returns the list of types
        public static List<StatisticsType> GetStatisticsTypes()
        {
            return _types.Values.ToList();
        }

        #endregion

        //clears the list

        //adds a type to the collection
    }
}
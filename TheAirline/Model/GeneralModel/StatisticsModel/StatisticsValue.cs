﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    [Serializable]
    public class StatisticsValue : ISerializable
    {
        #region Constructors and Destructors

        public StatisticsValue(int year, StatisticsType stat, double value)
        {
            Value = value;
            Stat = stat;
            Year = year;
        }

        protected StatisticsValue(SerializationInfo info, StreamingContext ctxt)
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

        [Versioning("stat")]
        public StatisticsType Stat { get; set; }

        [Versioning("value")]
        public double Value { get; set; }

        [Versioning("year")]
        public int Year { get; set; }

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
}
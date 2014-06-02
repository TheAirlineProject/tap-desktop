using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TheAirline.Model.AirlinerModel;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.Serialization;


namespace TheAirline.Model.GeneralModel
{
    //the class for handling the tail numbers for the country
    [Serializable]
    public class CountryTailNumber : ISerializable
    {
        [Versioning("lasttailnumber")]
        public string LastTailNumber { get; set; }
        [Versioning("country")]
        public Country Country { get; set; }
        public CountryTailNumber(Country country)
        {
            this.Country = country;

        }
        //returns if a tail number matches the country
        public Boolean isMatch(string tailNumber)
        {

            string countryID = this.Country.TailNumberFormat.Split('-')[0];
            string numberFormat = this.Country.TailNumberFormat.Split('-')[1];

            int length = 0;//Convert.ToInt16(numberFormat.Substring(numberFormat.Length - 1));

            int sIndex = numberFormat.IndexOf('s');
            int dIndex = numberFormat.IndexOf('d');


            if (sIndex != -1)
                length += Convert.ToInt16(numberFormat.Substring(sIndex + 1, 1));
            if (dIndex != -1)
                length += Convert.ToInt16(numberFormat.Substring(dIndex + 1, 1));


            // if (tailNumber.Split('-').Length < 2) 
            //   return true;

            string tailID = tailNumber.Split('-')[0];
            string tailFormat = tailNumber.Split('-')[1];

            return tailID == countryID && tailFormat.Length == length;

        }
        //returns the tail number for digits
        private string getTailNumber(int number, int digits)
        {
            string format = "{0:";
            for (int i = 0; i < digits; i++)
                format += "0";
            format += "}";
            return String.Format(format, number);
        }
        //returns the tail number for strings
        private string getTailNumber(string lastCode, int chars)
        {
            if (lastCode == null)
            {
                string code = "";
                for (int i = 0; i < chars; i++)
                    code += "A";
                return code;
            }
            else
            {

                int i = 0;
                Boolean found = false;
                while (!found && i < chars)
                {
                    if (lastCode[lastCode.Length - 1 - i] < 'Z')
                        found = true;
                    else
                        i++;
                }

                char replaceChar = lastCode[lastCode.Length - 1 - i];
                replaceChar++;

                if (i == 0)
                    return lastCode.Substring(0, chars - i - 1) + replaceChar + lastCode.Substring(chars - i);
                else
                {
                    string postfix = "";
                    for (int j = 0; j < i; j++)
                        postfix += "A";

                    return lastCode.Substring(0, chars - i - 1) + replaceChar + postfix;
                }

            }
        }
        //returns the next tail number
        public string getNextTailNumber()
        {
            if (!this.Country.TailNumberFormat.Contains("-"))
                return "";

            try
            {
                string countryID = this.Country.TailNumberFormat.Split('-')[0];
                string numberFormat = this.Country.TailNumberFormat.Split('-')[1];

                int length = Convert.ToInt16(numberFormat.Substring(numberFormat.Length - 1));

                if (numberFormat.Contains("\\s") && numberFormat.Contains("\\d"))
                {

                    int dLenght = Convert.ToInt16(numberFormat.Substring(numberFormat.IndexOf('d') + 1, 1));
                    int sLenght = Convert.ToInt16(numberFormat.Substring(numberFormat.IndexOf('s') + 1, 1));
                    if (LastTailNumber == null)
                    {
                        if (dLenght < sLenght)
                            this.LastTailNumber = countryID + "-" + getTailNumber(0, dLenght) + getTailNumber(null, sLenght);
                        else
                            this.LastTailNumber = countryID + "-" + getTailNumber(null, sLenght) + getTailNumber(0, dLenght);
                    }
                    else
                    {
                        string t = this.LastTailNumber.Split('-')[1].Substring(0, dLenght);
                        string s = this.LastTailNumber.Split('-')[1].Substring(sLenght, dLenght);
                        string postfix = this.LastTailNumber.Split('-')[1];
                        string lastCode = dLenght < sLenght ? this.LastTailNumber.Split('-')[1].Substring(dLenght, sLenght) : this.LastTailNumber.Split('-')[1].Substring(0, sLenght);

                        int lastNumber = dLenght < sLenght ? Convert.ToInt16(t) : Convert.ToInt16(s);
                        int number = lastNumber + 1;

                        int nLenght = number.ToString().Length;

                        string sNumber = nLenght > dLenght ? getTailNumber(lastCode, sLenght) : lastCode;
                        string dNumber = nLenght > dLenght ? getTailNumber(0, dLenght) : getTailNumber(number, dLenght);
                        string code = getTailNumber(lastCode, sLenght);

                        if (dLenght < sLenght)
                            this.LastTailNumber = countryID + "-" + dNumber + sNumber;
                        else
                            this.LastTailNumber = countryID + "-" + sNumber + dNumber;
                    }
                }
                if (numberFormat.Contains("\\d") && !numberFormat.Contains("\\s"))
                {
                    int number;
                    if (LastTailNumber == null)
                        number = 0;
                    else
                        number = Convert.ToInt32(this.LastTailNumber.Split('-')[1]) + 1;
                    /*
                    string format = countryID + "-{0:";
                    for (int i = 0; i < length; i++)
                        format += "0";
                    format += "}";
                     * */
                    this.LastTailNumber = countryID + "-" + getTailNumber(number, length);///String.Format(format, number);
                }
                if (numberFormat.Contains("\\s") && !numberFormat.Contains("\\d"))
                {
                    this.LastTailNumber = countryID + "-" + getTailNumber(this.LastTailNumber == null ? this.LastTailNumber : this.LastTailNumber.Split('-')[1], length);
                    /*
                    if (LastTailNumber == null)
                    {
                        string code = countryID + "-";
                        for (int i = 0; i < length; i++)
                            code += "A";
                        this.LastTailNumber = code;
                    }
                    else
                    {
                        string lastCode = this.LastTailNumber.Split('-')[1];


                        int i = 0;
                        Boolean found = false;
                        while (!found && i < length)
                        {
                            if (lastCode[lastCode.Length - 1 - i] < 'Z')
                                found = true;
                            else
                                i++;
                        }
                   
              
                        char replaceChar = lastCode[lastCode.Length - 1 - i];
                        replaceChar++;

                        string newCode = lastCode.Substring(0, length - i - 1) + replaceChar + lastCode.Substring(length - i);

                        this.LastTailNumber = countryID + "-" + newCode;
                    }
                     * */
                }
            }
            catch (Exception)
            {
                return "";
            }
        
            return this.LastTailNumber;
        }
             private CountryTailNumber(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }


        }

    }

}

using System;
using System.Globalization;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Countries
{
    //the class for handling the tail numbers for the country
    [Serializable]
    public class CountryTailNumber : BaseModel
    {
        public CountryTailNumber(Country country)
        {
            Country = country;
        }

        private CountryTailNumber(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        [Versioning("lasttailnumber")]
        public string LastTailNumber { get; set; }

        [Versioning("country")]
        public Country Country { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //returns if a tail number matches the country
        public bool IsMatch(string tailNumber)
        {
            string countryID = Country.TailNumberFormat.Split('-')[0];
            string numberFormat = Country.TailNumberFormat.Split('-')[1];

            int length = 0; //Convert.ToInt16(numberFormat.Substring(numberFormat.Length - 1));

            int sIndex = numberFormat.IndexOf('s');
            int dIndex = numberFormat.IndexOf('d');


            if (sIndex != -1)
                length += Convert.ToInt16(numberFormat.Substring(sIndex + 1, 1));
            if (dIndex != -1)
                length += Convert.ToInt16(numberFormat.Substring(dIndex + 1, 1));

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
            return string.Format(format, number);
        }

        //returns the tail number for strings
        private string getTailNumber(string lastCode, int chars)
        {
            int i;
            if (lastCode == null)
            {
                string code = "";
                for (i = 0; i < chars; i++)
                    code += "A";
                return code;
            }
            i = 0;
            var found = false;
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
            string postfix = "";
            for (int j = 0; j < i; j++)
                postfix += "A";

            return lastCode.Substring(0, chars - i - 1) + replaceChar + postfix;
        }

        //returns the next tail number
        public string GetNextTailNumber()
        {
            if (!Country.TailNumberFormat.Contains("-"))
                return "";

            try
            {
                string countryID = Country.TailNumberFormat.Split('-')[0];
                string numberFormat = Country.TailNumberFormat.Split('-')[1];

                int length = Convert.ToInt16(numberFormat.Substring(numberFormat.Length - 1));

                if (numberFormat.Contains("\\s") && numberFormat.Contains("\\d"))
                {
                    int dLenght = Convert.ToInt16(numberFormat.Substring(numberFormat.IndexOf('d') + 1, 1));
                    int sLenght = Convert.ToInt16(numberFormat.Substring(numberFormat.IndexOf('s') + 1, 1));
                    if (LastTailNumber == null)
                    {
                        if (dLenght < sLenght)
                            LastTailNumber = countryID + "-" + getTailNumber(0, dLenght) + getTailNumber(null, sLenght);
                        else
                            LastTailNumber = countryID + "-" + getTailNumber(null, sLenght) + getTailNumber(0, dLenght);
                    }
                    else
                    {
                        string t = LastTailNumber.Split('-')[1].Substring(0, dLenght);
                        string s = LastTailNumber.Split('-')[1].Substring(sLenght, dLenght);
                        string postfix = LastTailNumber.Split('-')[1];
                        string lastCode = dLenght < sLenght ? LastTailNumber.Split('-')[1].Substring(dLenght, sLenght) : LastTailNumber.Split('-')[1].Substring(0, sLenght);

                        int lastNumber = dLenght < sLenght ? Convert.ToInt16(t) : Convert.ToInt16(s);
                        int number = lastNumber + 1;

                        int nLenght = number.ToString(CultureInfo.InvariantCulture).Length;

                        string sNumber = nLenght > dLenght ? getTailNumber(lastCode, sLenght) : lastCode;
                        string dNumber = nLenght > dLenght ? getTailNumber(0, dLenght) : getTailNumber(number, dLenght);
                        string code = getTailNumber(lastCode, sLenght);

                        if (dLenght < sLenght)
                            LastTailNumber = countryID + "-" + dNumber + sNumber;
                        else
                            LastTailNumber = countryID + "-" + sNumber + dNumber;
                    }
                }
                if (numberFormat.Contains("\\d") && !numberFormat.Contains("\\s"))
                {
                    int number;
                    if (LastTailNumber == null)
                        number = 0;
                    else
                        number = Convert.ToInt32(LastTailNumber.Split('-')[1]) + 1;
                    /*
                    string format = countryID + "-{0:";
                    for (int i = 0; i < length; i++)
                        format += "0";
                    format += "}";
                     * */
                    LastTailNumber = countryID + "-" + getTailNumber(number, length);
                }
                if (numberFormat.Contains("\\s") && !numberFormat.Contains("\\d"))
                {
                    LastTailNumber = countryID + "-" + getTailNumber(LastTailNumber == null ? LastTailNumber : LastTailNumber.Split('-')[1], length);
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

            return LastTailNumber;
        }
    }
}
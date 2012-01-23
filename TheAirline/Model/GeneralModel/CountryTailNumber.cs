using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.Model.GeneralModel
{
    //the class for handling the tail numbers for the country
    public class CountryTailNumber
    {
        private string LastTailNumber;
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

            int length = Convert.ToInt16(numberFormat.Substring(numberFormat.Length - 1));

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
        private string getTailNumber(string lastNumber, int chars)
        {
            if (lastNumber == null)
            {
                string code = "";
                for (int i = 0; i < chars; i++)
                    code += "A";
                return code;
            }
            else
            {
                string lastCode = this.LastTailNumber.Split('-')[1];


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

                return lastCode.Substring(0, chars - i - 1) + replaceChar + lastCode.Substring(chars - i);

            }
        }
        //returns the next tail number
        public string getNextTailNumber()
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
                    this.LastTailNumber = countryID + "-" + getTailNumber(0, dLenght) + getTailNumber(null, sLenght);
                }
                else
                {
                    string lastCode = this.LastTailNumber.Split('-')[1].Substring(dLenght, sLenght);
                    int lastNumber = Convert.ToInt16(this.LastTailNumber.Split('-')[1].Substring(0, dLenght));
                    this.LastTailNumber = countryID + "-" + getTailNumber(lastNumber, dLenght) + getTailNumber(lastCode, sLenght);
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



            return this.LastTailNumber;
        }
    }

}

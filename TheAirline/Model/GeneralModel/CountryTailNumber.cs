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
            //returns the next tail number
            public string getNextTailNumber()
            {
                string countryID = this.Country.TailNumberFormat.Split('-')[0];
                string numberFormat = this.Country.TailNumberFormat.Split('-')[1];

                int length = Convert.ToInt16(numberFormat.Substring(numberFormat.Length - 1));

                if (numberFormat.Contains("\\d"))
                {
                    int number;
                    if (LastTailNumber == null)
                        number = 0;
                    else
                        number = Convert.ToInt32(this.LastTailNumber.Split('-')[1]) + 1;
                    string format = countryID + "-{0:";
                    for (int i = 0; i < length; i++)
                        format += "0";
                    format += "}";
                    this.LastTailNumber = String.Format(format, number);
                }
                if (numberFormat.Contains("\\s"))
                {
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
                }
                 // chs, 2011-27-10 if tailnumber already exits create log file with the tail number
                if (Airliners.TailNumberExits(this.LastTailNumber))
                {
                    using (TextWriter tw = new StreamWriter("tailnumber.log"))
                    {
                        tw.WriteLine(string.Format("{0}: Tailnumber: {1}, Country: {2}",DateTime.Now,this.LastTailNumber,this.Country.Name));
                    }
                   
                }
                

                return this.LastTailNumber;
            }
        }
    
}

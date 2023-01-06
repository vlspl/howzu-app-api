using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Globalization;

namespace Validation
{
    public class InputValidation
    {
        # region Check Data is Numeric or not Retun Bool

        public bool IsNumeric(object value)
        {

            try
            {
                double d = System.Double.Parse(value.ToString(), System.Globalization.NumberStyles.Any);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        #endregion
        # region Check Number is Integer or not

        public bool IsInteger(String strNumber)
        {
            Regex objNotIntPattern = new Regex("[^0-9-]");
            Regex objIntPattern = new Regex("^-[0-9]+$|^[0-9]+$");
            return !objNotIntPattern.IsMatch(strNumber) && objIntPattern.IsMatch(strNumber);
        }

        public bool IsWholeNumber(String strNumber)
        {
            Regex objNotWholePattern = new Regex("[^0-9]");

            return !objNotWholePattern.IsMatch(strNumber);
        }
        public bool IsMatch(String strCompair, String strToCompair)
        {
            bool RtutnState = false;
            if (strCompair == strToCompair)
            {
                RtutnState = true;
            }
            return RtutnState;
        }
        public bool IsValidDate(String StrDate)
        {
            try
            {
                Regex regex = new Regex(@"(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d))$");

                //Verify whether date entered in dd/MM/yyyy format.
                bool isValid = regex.IsMatch(StrDate.Trim());

                //Verify whether entered date is Valid date.
                DateTime dt;
                isValid = DateTime.TryParseExact(StrDate, "dd/MM/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out dt);
                if (!isValid)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch(Exception ex)
            {
                return false;
            }

            //return false;
        }
        public string StringInQuote(String strInput)
        {
            strInput = strInput.Replace("'", "''");
            strInput = "'" + strInput + "'";
            return strInput;
        }


        #endregion

        public bool AadharValidation(String ObjTextBox)
        {
            bool RtutnState = false;

            if (!String.IsNullOrEmpty(ObjTextBox.Trim()))
            {
                if (ObjTextBox.Length == 12)
                {
                    RtutnState = true;
                }
            }
            return RtutnState;
        }
        public bool PincodeValidation(String ObjTextBox)
        {
            bool RtutnState = false;

            if (!String.IsNullOrEmpty(ObjTextBox.Trim()))
            {
                if (ObjTextBox.Length == 6)
                {
                    RtutnState = true;
                }
            }
            return RtutnState;
        }
        public bool IsDecimal(String strNumber)
        {
            Regex objPatternnumber = new Regex(@"^\d+\.?\d*$");

            if (objPatternnumber.IsMatch(strNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsValidDateForDateFilteration(String StrDate)
        {
            try
            {
                Regex regex = new Regex(@"(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d))$");

                //Verify whether date entered in dd/MM/yyyy format.
                bool isValid = regex.IsMatch(StrDate.Trim());

                //Verify whether entered date is Valid date.
                DateTime dt;
                isValid = DateTime.TryParseExact(StrDate, "yyyy-MM-dd", new CultureInfo("en-GB"), DateTimeStyles.None, out dt);
                if (!isValid)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            //return false;
        }
        #region Phone no

        public bool IsPhoneNoValidation(String ObjTextBox)
        {
            bool RtutnState = false;
            Regex regexPhoneNumber = new Regex(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$");
            if (regexPhoneNumber.IsMatch(ObjTextBox))
            {
                string formattedPhoneNumber = regexPhoneNumber.Replace(ObjTextBox, "($1)$2-$3");
                RtutnState = true;
            }
            else
            {
                RtutnState = false;

            }

            return RtutnState;
        }

        #endregion
        #region charonly
        //written bY vinayak
        public bool IsCharOnly(string StrInput)
        {


            string pattern = @"^[a-z A-Z\\s]* *$";

            System.Text.RegularExpressions.Match match = Regex.Match(StrInput.Trim(), pattern, RegexOptions.IgnoreCase);

            return match.Success;
        }

        //written by Abhay
        public bool IsCharWithBasicPuncWithNoSpace(string StrInput)
        {
            string pattern = @"^[a-z A-Z\-\.\,\(\)\'\""]* *$";
            System.Text.RegularExpressions.Match match = Regex.Match(StrInput.Trim(), pattern, RegexOptions.IgnoreCase);
            return match.Success;
        }

        #endregion
        # region Check for Valid EmailID When Email is Mandatory

        public bool IsValidEmailAddress(String sEmail)
        {

            string pattern = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";

            System.Text.RegularExpressions.Match match = Regex.Match(sEmail.Trim(), pattern, RegexOptions.IgnoreCase);

            return match.Success;


        }


        #endregion
        # region Check for Valid Website When website not Mandatory

        public bool IsValidWebAddress(String sURL)
        {
            if (string.IsNullOrEmpty(sURL))
            {
                return true;
            }
            else
            {

                return Regex.IsMatch(sURL, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            }


        }

        #endregion
        #region Time Format
        //written by Sachin
        public bool IsValidTime(string StrInput)
        {///Time Format HH:MM:AM or PM


            string pattern = @"^((0?[1-9]|1[012])(:[0-5]\d){0,2}(:[AP]M))$";

            System.Text.RegularExpressions.Match match = Regex.Match(StrInput.Trim(), pattern, RegexOptions.IgnoreCase);

            return match.Success;


        }

        #endregion
        #region TextBox Validation for Empty or Null

        public bool IsTextBoxEmpty(String ObjTextBox)
        {
            bool RtutnState = false;
            if (String.IsNullOrEmpty(ObjTextBox.Trim()))
            {


                RtutnState = true;
            }
            return RtutnState;
        }

        #endregion
        #region TextBox Validation for Empty or Null WithTextbox Array

        public bool IsTextBoxEmptyArray(String[] ObjTextBox)
        {
            bool RtutnState = false;
            foreach (String TempCtrl in ObjTextBox)
            {
                if (String.IsNullOrEmpty(TempCtrl.Trim()))
                {

                    RtutnState = true;

                }
            }

            return RtutnState;
        }

        #endregion
        #region

        public bool IsInRangeINT(String StringValue, int MinRange, int MaxRange)
        {
            bool rtn = false;

            if (string.IsNullOrEmpty(StringValue))
            {
                rtn = true;
            }

            else
            {

                if ((int.Parse(StringValue) >= MinRange) && (int.Parse(StringValue) <= MaxRange))
                {
                    rtn = true;
                }
            }
            return rtn;


        }


        #endregion
        #region Phone or Mobile no must be 10 or > 10 digit

        public bool MobileValidation(String ObjTextBox)
        {
            bool RtutnState = false;

            if (!String.IsNullOrEmpty(ObjTextBox.Trim()))
            {
                if (ObjTextBox.Length == 10)
                {
                    RtutnState = true;
                }
            }
            return RtutnState;
        }

        #endregion
        #region minlength
        public bool IsMinLength(String strval, int length)
        {
            bool returnval = false;
            if (string.IsNullOrEmpty(strval))
            {
                return true;
            }
            else if (strval.Length >= length)
            {
                returnval = true;
            }
            else
            {
                returnval = false;
            }
            return returnval;
        }
        #endregion
        #region maxlength
        public bool IsMaxLength(String strval, int length)
        {
            bool returnval = false;
            if (string.IsNullOrEmpty(strval))
            {
                return true;
            }
            else if (strval.Length <= length)
            {
                returnval = true;
            }
            else
            {
                returnval = false;
            }
            return returnval;
        }
        #endregion
        #region alphanumeric validation
        public bool IsAlphanumeric(String stralpchar)
        {
            //string pattern = @"^^[a-z]+$|^[A-Z]+$|^[0-9|]";

            //System.Text.RegularExpressions.Match match = Regex.Match(stralpchar.Trim(), pattern, RegexOptions.IgnoreCase);

            //return match.Success;
            char[] str = stralpchar.ToCharArray();
            bool Rtunstate = true;
            foreach (char c in stralpchar)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    Rtunstate = false;
                    break;
                }
            }
            return Rtunstate;

        }
        #endregion
        #region nowhitespace validation
        public bool IsWhiteSpace(String stralpchar)
        {
            char[] str = stralpchar.ToCharArray();
            bool Rtunstate = true;
            foreach (char c in stralpchar)
            {
                if (char.IsWhiteSpace(c))
                {
                    Rtunstate = true;

                    break;
                }
                else
                {
                    Rtunstate = false;

                }
            }
            return Rtunstate;

        }
        #endregion
        #region Minimum Words and min length of each word  validation
        public bool IsMinWordsLength(String stralpchar, int words, int length)
        {
            int CntWhitespace = 0;
            int cntChar = 0;
            char[] str = stralpchar.ToCharArray();
            bool Rtunstate = true;
            foreach (char c in stralpchar)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (cntChar >= length)
                    {
                        CntWhitespace++;
                        cntChar = 0;
                    }
                    else
                    {
                        break;
                        return Rtunstate = false;
                    }
                }
                else
                {
                    cntChar++;
                }

            }
            if (cntChar >= length && CntWhitespace >= words)
            {

                return Rtunstate = true;
            }
            else
            {
                return Rtunstate = false;
            }

        }
        #endregion
        #region Minimum Words validation
        public bool IsMinWords(String stralpchar, int words)
        {

            string[] str = stralpchar.Split(' ');


            if (str.Length >= words)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region Alternate Phone or Mobile no must be 10 or > 10 digit or Empty

        public bool AltenatePhoneMobileValidation(String ObjTextBox)
        {
            bool RtutnState = false;

            if (!String.IsNullOrEmpty(ObjTextBox.Trim()))
            {

                if (ObjTextBox.Length < 10 || ObjTextBox.Length > 15)
                {
                    RtutnState = true;
                }

                if (ObjTextBox.Length == 0)
                {
                    RtutnState = false;
                }
            }

            return RtutnState;
        }

        #endregion


        public bool ValidatePasswordNew(string password)
        {
            var input = password;
            //  ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var hasMiniMaxChars = new Regex(@".{6,15}");
          
           
             if (!hasMiniMaxChars.IsMatch(input))
            {
                // ErrorMessage = "Password should not be less than or greater than 12 characters";
                return false;
            }
            
            else
            {
                return true;
            }
        }
        public bool ValidatePassword(string password)
        {
            var input = password;
            //  ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{6,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasLowerChar.IsMatch(input))
            {
                //  ErrorMessage = "Password should contain At least one lower case letter";
                return false;
            }
            else if (!hasUpperChar.IsMatch(input))
            {
                // ErrorMessage = "Password should contain At least one upper case letter";
                return false;
            }
            else if (!hasMiniMaxChars.IsMatch(input))
            {
                // ErrorMessage = "Password should not be less than or greater than 12 characters";
                return false;
            }
            else if (!hasNumber.IsMatch(input))
            {
                //  ErrorMessage = "Password should contain At least one numeric value";
                return false;
            }

            else if (!hasSymbols.IsMatch(input))
            {
                // ErrorMessage = "Password should contain At least one special case characters";
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool GenderValidation(String ObjTextBox)
        {
            bool RtutnState = false;

            if (!String.IsNullOrEmpty(ObjTextBox.Trim()))
            {
                if (ObjTextBox.ToString() == "Male")
                {
                    RtutnState = true;
                }
                else if (ObjTextBox.ToString() == "Female")
                    RtutnState = true;
            }
            return RtutnState;
        }

        public bool AgeValidation(String strNumber)
            {
                Regex objNotIntPattern = new Regex("[^0-9-]");
                Regex objIntPattern = new Regex("^-[0-9]+$|^[0-9]+$");
                return !objNotIntPattern.IsMatch(strNumber) && objIntPattern.IsMatch(strNumber);
            }

        

        //internal bool AgeValidation(string age)
        //{


        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Configuration;
using System.Data;
using System.Web.UI.WebControls;

namespace RIMBA
{
    public class Tools
    {
        /*******************************************************************************************************/
        #region CONFIG FILE METHODS

        public static string getAppSettingsValue(string key)
        {
            return WebConfigurationManager.AppSettings[key];
        }

        public static string getConnectionString(string key)
        {
            return WebConfigurationManager.ConnectionStrings[key].ConnectionString;
        }

        #endregion
        /*******************************************************************************************************/
        #region NULL MANIPULATORS

        public static T wrapValue<T>(object value)
        {
            return wrapDBNullValue<T>(value);
        }

        public static T wrapDBNullValue<T>(object value)
        {
            object val = wrapNullable(value);
            if (val == null || val == DBNull.Value)
            {
                if (typeof(T) == typeof(Guid?))
                {
                    object obj = null;
                    return (T)obj;
                }
                else
                    return default(T);
            }
            else if (typeof(T) == typeof(Guid?))
                return (T)val;
            else if (Nullable.GetUnderlyingType(typeof(T)) != null)
                return (T)Convert.ChangeType(val, Nullable.GetUnderlyingType(typeof(T)));
            else
                return (T)Convert.ChangeType(val, typeof(T));
        }

        public static object wrapNullable(object value)
        {
            if (value != null && value.GetType() == typeof(string) && string.IsNullOrEmpty((string)value))
                return DBNull.Value;
            else
                return value ?? DBNull.Value;
        }

        #endregion
        /*******************************************************************************************************/
        #region TYPE MANIPULATORS

        public static T parseData<T>(DataRow row, string columnName)
        {
            object obj = row[columnName];
            return wrapDBNullValue<T>(obj);
        }
        
        public static bool toBool(string value)
        {
            return value == "1" || value.ToUpper() == "TRUE";
        }

        #endregion 
        /*******************************************************************************************************/
        #region STRING MANIPULATORS

        public static string append(string Text, string NewText, string Delimiter)
        {
            if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(NewText))
                return NewText;
            else if (string.IsNullOrEmpty(NewText))
                return Text;
            else
            {
                if (!string.IsNullOrEmpty(Text)) Text += Delimiter;
                return Text += NewText;
            }
        }

        #endregion
        /*******************************************************************************************************/
        #region COOKIES

        public static T getCookieData<T>(string key)
        {
            object value = (HttpContext.Current.Items[key] ?? string.Empty).ToString();

            if (typeof(T) == typeof(bool))
                return (T)Convert.ChangeType(toBool(value.ToString()), typeof(T));

            return (T)value;
        }

        public static void setCookieData(string key)
        {
            HttpContext.Current.Items[key] = key;
        }

        public static string buildCookieData(params string[] values)
        {
            string data = "";
            string delimiter = GlobalVariables.COOKIEDATE_DELIMITER.ToString();
            foreach(string value in values)
            {
                data = Tools.append(data, value, delimiter);
            }
            return data;
        }
        
        #endregion
        /*******************************************************************************************************/

        public static void retainTextboxData(HttpRequest request, params TextBox[] textboxes)
        {
            foreach(TextBox textbox in textboxes)
                textbox.Text = request[textbox.UniqueID];
        }
    }
}
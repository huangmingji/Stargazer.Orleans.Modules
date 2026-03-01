using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Stargazer.Orleans.Utility.Extend
{
    public static partial class Ext
    {
        /// <summary>
        /// 字符串转换成字节数组
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string strValue)
        {
            return System.Text.Encoding.UTF8.GetBytes(strValue);
        }

        /// <summary>
        /// 字节数组转换成字符串
        /// </summary>
        /// <param name="byteValue"></param>
        /// <returns></returns>
        public static string ToString(this byte[] byteValue)
        {
            return System.Text.Encoding.UTF8.GetString(byteValue);
        }

        /// <summary>
        /// 日期字符串转换成DateTime，如转换失败则返回当前时间
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="formatsValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string datetime, List<string> formatsValue = null)
        {
            return DateTimeParse(datetime, DateTime.Now, formatsValue);
        }

        /// <summary>
        /// 日期字符串转换成DateTime，如转换失败则返回默认时间
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="dafaultValue"></param>
        /// <param name="formatsValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string datetime, DateTime dafaultValue, List<string> formatsValue = null)
        {
            return DateTimeParse(datetime, dafaultValue, formatsValue);
        }

        /// <summary>
        /// 指定字符串表示形式转换为其等效的 System.DateTime，如返回false，则为无效的字符串。
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <param name="dafaultValue"></param>
        /// <param name="formatsValue"></param>
        /// <returns></returns>
        private static DateTime DateTimeParse(string dateTimeString, DateTime dafaultValue, List<string> formatsValue = null)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                return dafaultValue;
            }

            List<string> formats = new List<string> {"M/d/yyyy h:mm:ss tt",
                                "M/d/yyyy h:mm tt",
                                "MM/dd/yyyy hh:mm:ss",
                                "M/d/yyyy h:mm:ss",
                                "M/d/yyyy hh:mm tt",
                                "M/d/yyyy hh tt",
                                "M/d/yyyy h:mm",
                                "M/d/yyyy h:mm",
                                "MM/dd/yyyy hh:mm",
                                "M/dd/yyyy hh:mm",
                                "yyyyMMdd",
                                "yyyy-MM-dd",
                                "yyyy/MM/dd",
                                "yyyyMMddHHmmss",
                                "yyyy-MM-dd HH:mm:ss",
                                "yyyy-MM-dd HH:mm",
                                "yyyyMMddhhmmss",
                                "yyyyMMddHHmmssfff",
                                "yyyyMMddHHmmssfffffff",
                                "yyyy-MM-dd HH:mm:ss.fff",
                                "yyyy-MM-dd HH:mm:ss.fffffff",
                                "yyyyMMddhhmmssfff",
                                "yyyyMMddhhmmssfffffff"};

            if (formatsValue != null && formatsValue.Any())
            {
                formats.AddRange(formatsValue);
            }

            DateTime dateTime;
            if (!DateTime.TryParse(dateTimeString, out dateTime))
            {
                //优先匹配英文格式，其次中文
                if (!DateTime.TryParseExact(dateTimeString,
                                            formats.ToArray(),
                                            new System.Globalization.CultureInfo("en-US"),
                                            System.Globalization.DateTimeStyles.None,
                                            out dateTime) &&
                    !DateTime.TryParseExact(dateTimeString,
                                            formats.ToArray(),
                                            new System.Globalization.CultureInfo("zh-CN"),
                                            System.Globalization.DateTimeStyles.None,
                                            out dateTime))
                {
                    dateTime = dafaultValue;
                }
            }

            return dateTime;
        }

        /// <summary>
        ///   Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        /// <summary>
        ///  Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatWith(this string text, params object[] args)
        {
            return string.Format(text, args);
        }

        /// <summary>
        /// 将JSON转换实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="JsonStr"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(this string JsonStr) where T : class, new()
        {
            try
            {
                var rep1 = "\n";
                var rep2 = "\r";
                var rep3 = "\t";
                var rep4 = "\ufeff";
                JsonStr = JsonStr.Replace(rep1, "").Replace(rep2, "").Replace(rep3, "").Replace(rep4, "").Replace(" ", "").Trim();
                return JsonConvert.DeserializeObject<T>(JsonStr);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// 将实体装换成json字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SerializeObject(this object value)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.Converters.Add(new DateTimeJsonConverter());
            settings.Converters.Add(new LongJsonConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);
        }

        /// <summary>
        /// 将实体转换成xml字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ConverToXmlString<T>(this T t) where T : class, new()
        {
            string xmlString = string.Empty;

            XmlDocument xd = new XmlDocument();
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                ser.Serialize(sw, t);
                xd.LoadXml(sw.ToString());
                XmlElement element = xd.DocumentElement;
                element.RemoveAllAttributes();
                xmlString = element.OuterXml.ToString();
            }

            return xmlString;
        }

        /// <summary>
        /// 将xml字符串转换成实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T ConvertToObject<T>(this string xmlString) where T : class, new()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                object obj = ser.Deserialize(new StringReader(xmlString));
                return (T)obj;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// 输入Float格式数字，将其转换为货币表达方式
        /// </summary>
        /// <param name="money">传入的int数字</param>
        /// <param name="type">货币表达类型：0=带￥的货币表达方式；1=不带￥的货币表达方式；其它=带￥的货币表达方式</param>
        /// <returns>返回转换的货币表达形式</returns>
        public static string ToMoneyString(this double money, int type)
        {
            string _rmoney;
            try
            {
                switch (type)
                {
                    case 0:
                        _rmoney = string.Format("{0:C2}", money);
                        break;

                    case 1:
                        _rmoney = string.Format("{0:N2}", money);
                        break;

                    default:
                        _rmoney = string.Format("{0:C2}", money);
                        break;
                }
            }
            catch
            {
                _rmoney = "";
            }

            return _rmoney;
        }

        #region url参数处理

        /// <summary>
        /// 组合参数
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string AsQueryString(this object source)
        {
            var keyValuePairs = source
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetValue(source, null) != null)
                .Select(x => string.Concat(x.Name.ToLower(), "=", WebUtility.UrlEncode(x.GetValue(source, null).ToString())));

            return string.Concat("?", string.Join("&", keyValuePairs));
        }

        /// <summary>
        /// 组合参数
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string AsQueryString(this NameValueCollection source)
        {
            if (source == null)
            {
                return string.Empty;
            }

            string paramz = "";
            foreach (String key in source.AllKeys)
            {
                paramz += string.Format("&{0}={1}", key, WebUtility.UrlEncode(source[key]));
            }
            return string.Concat("?", paramz.TrimStart('&'));
        }

        /// <summary>
        /// 分析 url 字符串中的参数信息
        /// </summary>
        /// <param name="url">输入的 URL</param>
        /// <param name="baseUrl">输出 URL 的基础部分</param>
        /// <returns>输出分析后得到的 (参数名,参数值) 的集合</returns>
        public static NameValueCollection ParseUrl(this string url, out string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("url");
            }

            NameValueCollection nvc = new NameValueCollection();
            baseUrl = "";

            int questionMarkIndex = url.IndexOf('?');
            if (questionMarkIndex == -1)
            {
                baseUrl = url;
                return new NameValueCollection();
            }
            baseUrl = url.Substring(0, questionMarkIndex);
            if (questionMarkIndex == url.Length - 1)
            {
                return new NameValueCollection();
            }

            string ps = url.Substring(questionMarkIndex + 1);
            // 开始分析参数对  
            Regex re = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
            MatchCollection mc = re.Matches(ps);
            foreach (Match m in mc)
            {
                nvc.Add(m.Result("$2"), m.Result("$3"));
            }
            return nvc;
        }

        #endregion

        #region 创建随机字符串

        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string CreateNonceStr(int length)
        {
            return CreateNonceStr(length, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
        }

        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="length"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string CreateNonceStr(int length, string key)
        {
            char[] charArray = key.ToCharArray();
            Random rd = new Random(GetRandomSeed());
            StringBuilder result = new StringBuilder(length);

            for (int i = 0; i < result.Capacity; i++)
            {
                result.Append(charArray[rd.Next(charArray.Length)]);
            }

            return result.ToString();
        }

        private static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        #endregion 创建随机字符串

        #region 加密解密

        /// <summary>
        /// 256位AES加密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AESEncrypt(this string value, string key)
        {
            return Stargazer.Orleans.Utility.Cryptography.Cryptography.AESEncrypt(value, key);
        }

        /// <summary>
        /// 256位AES解密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AESDecrypt(this string value, string key)
        {
            return Stargazer.Orleans.Utility.Cryptography.Cryptography.AESDecrypt(value, key);
        }

        /// <summary>
        /// base64编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base64Encode(this string str)
        {
            byte[] input = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(input);
        }

        /// <summary>
        /// base64解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base64Decode(this string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }

        /// <summary>
        /// 大写MD5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertToUpperMd5(this string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create(); //实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
                string p = s[i].ToString("X");
                if (p.Length < 2)
                {
                    p = "0" + p;
                }
                pwd = pwd + p;
            }
            return pwd;
        }

        /// <summary>
        /// 小写MD5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertToLowerMd5(this string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create(); //实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
                string p = s[i].ToString("x");
                if (p.Length < 2)
                {
                    p = "0" + p;
                }
                pwd = pwd + p;
            }
            return pwd;
        }

        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SHA1(this string str)
        {
            byte[] data = System.Text.Encoding.Default.GetBytes(str);//以字节方式存储
            System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] result = sha1.ComputeHash(data);//得到哈希值
            return System.BitConverter.ToString(result).Replace("-", "").ToLower(); //转换成为字符串的显示
        }

        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static string SHA1(this string str, string charset)
        {
            byte[] data = Encoding.GetEncoding(charset).GetBytes(str);//以字节方式存储
            System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] result = sha1.ComputeHash(data);//得到哈希值
            return System.BitConverter.ToString(result).Replace("-", "").ToLower(); //转换成为字符串的显示
        }

        #endregion 加密解密

        #region JsonConverter

        public class LongJsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(long).IsAssignableFrom(objectType);
            }

            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                return long.Parse(reader.Value.ToString());
            }
        }

        public class DateTimeJsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(DateTime).IsAssignableFrom(objectType);
            }

            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                DateTime dt = (DateTime)value;
                writer.WriteValue(dt.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                return DateTime.Parse(reader.Value.ToString());
            }
        }

        #endregion JsonConverter
    }
}